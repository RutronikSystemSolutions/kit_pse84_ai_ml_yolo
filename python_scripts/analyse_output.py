import os
import glob
import pathlib
import xml.etree.ElementTree as ET
import cv2
import numpy as np

class Point:
    def __init__(self, x, y):
        self.x = x
        self.y = y

class Rectangle:
    def __init__(self, top_left: Point, bottom_right: Point):
        self.top_left = top_left
        self.bottom_right = bottom_right

def do_overlap(l1, r1, l2, r2):
    # If one rectangle is to the left of the other
    if l1.x > r2.x or l2.x > r1.x:
        return False

    # If one rectangle is above the other
    if r1.y < l2.y or r2.y < l1.y:
        return False

    return True

# Extract the session directories
# Remark: path relative to project file
def extract_sessions_and_images_paths(project_path):
    tree = ET.parse(project_path)
    root = tree.getroot()

    session_directories = []
    img_files = []
    
    database_dir = os.path.dirname(os.path.abspath(project_path))
    print(f"Project file directory: {database_dir}")

    database = root.find('.//Database')
    if database is None:
        raise ValueError("No Database element found in project file")

    for dataset in database.findall('DataSet'):
        sessions = dataset.findall('Session') + dataset.findall('Segment')
        for session in sessions:
            try:
                session_file = session.find('SessionFile').text
                img_file = session.find('DataFile').text

                session_file_obj = pathlib.Path(session_file)
                session_directories.append(database_dir + str(session_file_obj.parent))

                img_file_obj = pathlib.Path(img_file)
                img_files.append(database_dir + str(img_file_obj))

            except Exception as e:
                print(f"Warning: Could not parse session: {e}")
                continue

    return session_directories, img_files

def get_complete_path(img_files, img_name):
    for img_file in img_files:
        if os.path.basename(img_file) == img_name:
            return img_file
    return None

def draw_text(img, text,
          font=cv2.FONT_HERSHEY_PLAIN,
          pos=(0, 0),
          font_scale=3,
          font_thickness=1,
          text_color=(0, 255, 0),
          text_color_bg=(255, 255, 255)
          ):

    x, y = pos
    text_size, _ = cv2.getTextSize(text, font, font_scale, font_thickness)
    text_w, text_h = text_size
    cv2.rectangle(img, pos, (x + text_w, y - text_h), text_color_bg, -1)
    cv2.putText(img, text, pos, font, font_scale, text_color, font_thickness)

    return text_size

def analyse_output(output_dir, improj_path):
    print(f'Analyse output in {output_dir} with improj {improj_path}')

    legend_font = cv2.FONT_HERSHEY_PLAIN
    legend_font_scale = 1.5

    # From project file, extract session directories and image files
    session_directories, img_files = extract_sessions_and_images_paths(improj_path)

    # List directories of output_dir/Predictions
    predictions_dir = os.path.join(output_dir, "Predictions")
    directories = [ name for name in os.listdir(predictions_dir) if os.path.isdir(os.path.join(predictions_dir, name)) ]
    
    for directory in directories:
        print(f"Processing directory: {os.path.join(predictions_dir, directory)}")
        # Get labelxml file
        labelxmlfile_list = list(pathlib.Path(os.path.join(predictions_dir, directory)).glob("*.labelxml"))
        if (not labelxmlfile_list):
            continue

        labelxmlfile = labelxmlfile_list[0]

        # Load xml file
        tree = ET.parse(labelxmlfile)
        root = tree.getroot()

        for annotation in root.findall('Annotation'):
            img_name = annotation.find('Path').text
            img_path = get_complete_path(img_files, os.path.basename(img_name))

            # Load image
            image = cv2.imread(img_path)
            if image is None:
                print(f"Could not read image: {img_path}")
                return
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

             # Get image dimensions
            img_height, img_width = image.shape[:2]

            objects = annotation.findall('Object')
            print(f"Number of objects in annotation: {len(objects)}")

            blank_image = np.zeros((img_height + 50,img_width + 50,3), np.uint8)
            blank_image[:,:] = (255,255,255)
            blank_image[0:img_height, 0:img_width] = image_rgb.copy()

            taken_places = []

            first_y = 0

            for object in objects:
                class_name = object.find('Name').text
                print(f"Object name: {class_name}")

                confidence = object.find('Confidence').text
                confidence_value = float(confidence)

                bndbox = object.find('Bndbox')
                center_x = float(bndbox.find('X').text)
                center_y = float(bndbox.find('Y').text)
                width = float(bndbox.find('Width').text)
                height = float(bndbox.find('Height').text)

                # Draw bounding box on the label
                top_left_x = int(center_x * img_width - (width * img_width) / 2)
                top_left_y = int(center_y * img_height - (height * img_height) / 2)

                bottom_right_x = top_left_x + int(width * img_width)
                bottom_right_y = top_left_y + int(height * img_height)

                first_y = bottom_right_y + 20

                new_text = "[" +class_name + "]" + "{:.1f}".format(confidence_value)
                new_text_size, _ = cv2.getTextSize(new_text, legend_font, legend_font_scale, 1)

                # check if it will overlap
                while True:
                    overlapped = False
                    rect_new = Rectangle(Point(top_left_x, first_y), Point(top_left_x + new_text_size[0], first_y + new_text_size[1]))
                    for rect in taken_places:
                        if do_overlap(rect.top_left, rect.bottom_right, rect_new.top_left, rect_new.bottom_right):
                            overlapped = True
                            first_y += 25
                            break
                    if not overlapped:
                        break

                cv2.rectangle(blank_image, (top_left_x, top_left_y), (bottom_right_x, bottom_right_y), (0, 0, 255), 1)
                text_sz = draw_text(blank_image, 
                                    new_text,
                                    font=legend_font, 
                                    pos=(top_left_x, first_y), 
                                    font_scale=legend_font_scale, text_color=(0, 0, 255))
                
                text_w, text_h = text_sz
                rect = Rectangle(Point(top_left_x, first_y), Point(top_left_x + text_w, first_y + text_h))
                taken_places.append(rect)

            # end for object in objects:    

            '''
            cv2.imshow(f"Image: {os.path.basename(img_path)}", blank_image)
            cv2.waitKey(0)
            cv2.destroyAllWindows()
            '''  

            output_img_path = os.path.join(predictions_dir, directory,"annotated.png")
            cv2.imwrite(output_img_path, cv2.cvtColor(blank_image, cv2.COLOR_RGB2BGR))

            all_img_path = os.path.join(predictions_dir, "annotated",directory + ".png")
            cv2.imwrite(all_img_path, cv2.cvtColor(blank_image, cv2.COLOR_RGB2BGR))

        # break

if __name__ == "__main__":
    print('Analyse Deepcraft Studio output')

    output_dir = "C:\\tmp\\20260603_v5legacy_nodsaugm\\yolov5n-legacy-size320-batch16-epoch300"
    project_path = "C:\\rutronik\\deepcraft_workspace\\WaterCounterAnalysis\\WaterCounterAnalysis.improj"

    analyse_output(output_dir, project_path)

    # From project file, extract session directories and image files
    # session_directories, img_files = extract_sessions_and_images_paths(project_path)
    # print(f"Session directories: {img_files}")