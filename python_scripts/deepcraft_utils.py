from logging import root
import xml.etree.ElementTree as ET
import os
import matplotlib.pyplot as plt
import numpy as np
import cv2
import pathlib
from copy import copy, deepcopy
import shutil

# Plot the class distribution
def plot_distribution(project_path):
    tree = ET.parse(project_path)
    root = tree.getroot()

    classes_count = []
    for i in range(10):
        classes_count.append(0)

    session_map = {}
    database_dir = os.path.dirname(os.path.abspath(project_path))
    print(f"Project file directory: {database_dir}")

    database = root.find('.//Database')
    if database is None:
        raise ValueError("No Database element found in project file")
    print(f"Count of database.findall('DataSet') = {len(database.findall('DataSet'))}")

    for dataset in database.findall('DataSet'):
        dataset_id = dataset.attrib.get('id', 'unknown')
        print(f"Processing dataset: {dataset_id}")
        sessions = dataset.findall('Session') + dataset.findall('Segment')
        print(f"Found {len(sessions)} sessions in dataset: {dataset_id}")

        for session in sessions:
            print(f"Processing session: {session.attrib.get('name', 'unknown')} (ID: {session.attrib.get('sessionId', 'unknown')}) in dataset: {dataset_id}")
            try:
                session_name = session.attrib.get('name', '').strip().strip('"').strip("'")
                session_id = session.attrib.get('sessionId', '')

                data_file_elem = session.find('DataFile')
                data_file_relative = data_file_elem.text.strip().strip('"').strip("'").lstrip('/') if data_file_elem is not None else ''
                data_file_name = os.path.basename(data_file_relative)

                labels = session.find('Labels')
                print(f"Found {len(labels)} labels in session: {session_id}")

                for label in labels:
                    classes_count[int(label.text)] += int(label.attrib.get('count', '0'))

                data_file_path = os.path.abspath(os.path.join(
                    database_dir, data_file_relative
                ))

                if session_id:
                    session_map[session_id] = {
                        "path": data_file_path,
                        "name": session_name,
                        "dataset": dataset_id,
                        "filename": data_file_name
                    }
            except Exception as e:
                print(f"Warning: Could not parse session: {e}")
                continue

    plt.figure()
    bar_xlabels = [f'Class {i}' for i in range(len(classes_count))]
    #plt.bar(x=np.arange(len(classes_count)), height=classes_count, label=bar_labels, color='skyblue')
    plt.bar(bar_xlabels, classes_count, color='skyblue')
    plt.grid(True, axis='y', linestyle='--', alpha=0.7)
    plt.ylabel('Count')
    
    plt.title('Distribution of Classes')
    plt.show()

# Extract the session directories
# Remark: path relative to project file
def extract_session_directories(project_path):
    tree = ET.parse(project_path)
    root = tree.getroot()

    session_directories = []
    
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

                session_file_obj = pathlib.Path(session_file)
                session_directories.append(database_dir + str(session_file_obj.parent))

            except Exception as e:
                print(f"Warning: Could not parse session: {e}")
                continue

    return session_directories

# Visualize an image from a session
def visualize_image(session_directory, display_info=True):
    directory_obj = pathlib.Path(session_directory)
    
    # Name of the image is assumed to be the same as the session directory name with .png extension
    image_path = session_directory + "\\" + directory_obj.name + ".png"
    # Name of the label file is assumed to be the same as the session directory name with .labelxml extension
    label_path = session_directory + "\\" + directory_obj.name + ".labelxml"

    # Load image
    image = cv2.imread(image_path)
    if image is None:
        print(f"Could not read image: {image_path}")
        return
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # Get image dimensions
    img_height, img_width = image.shape[:2]
    
    # Load xml file
    tree = ET.parse(label_path)
    root = tree.getroot()

    for annotation in root.findall('Annotation'):
        objects = annotation.findall('Object')
        if (len(objects) != 1):
            print(f"Warning: Found {len(objects)} objects in annotation, expected 1. Skipping annotation.")
            continue

        class_name = objects[0].find('Name').text
        print(f"Object name: {class_name}")

        bndbox = objects[0].find('Bndbox')
        center_x = float(bndbox.find('X').text)
        center_y = float(bndbox.find('Y').text)
        width = float(bndbox.find('Width').text)
        height = float(bndbox.find('Height').text)

        # Draw bounding box on the label
        top_left_x = int(center_x * img_width - (width * img_width) / 2)
        top_left_y = int(center_y * img_height - (height * img_height) / 2)

        bottom_right_x = top_left_x + int(width * img_width)
        bottom_right_y = top_left_y + int(height * img_height)

        if (display_info == True):
            cv2.rectangle(image_rgb, (top_left_x, top_left_y), (bottom_right_x, bottom_right_y), (0, 0, 255), 2)
            cv2.putText(image_rgb, class_name, (top_left_x, top_left_y - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 0, 255), 2)

    cv2.imshow(f"Image: {os.path.basename(image_path)}", image_rgb)
    cv2.waitKey(0)
    cv2.destroyAllWindows()

def modify_session(session_directory, output_directory, output_base_directory=None, display_image=True):

    generated_classes = []
    for _ in range(10):
        generated_classes.append(0)

    directory_obj = pathlib.Path(session_directory)
    output_directory_obj = pathlib.Path(output_directory)
    
    # Name of the image is assumed to be the same as the session directory name with .png extension
    image_path = session_directory + "\\" + directory_obj.name + ".png"
    # Name of the label file is assumed to be the same as the session directory name with .labelxml extension
    label_path = session_directory + "\\" + directory_obj.name + ".labelxml"

    image_debug_path = None
    if (output_base_directory is not None):
        image_debug_path = output_base_directory + "\\" + directory_obj.name + "_debug.png"

    output_image_path = output_directory + "\\" + output_directory_obj.name + ".png"
    output_label_path = output_directory + "\\" + output_directory_obj.name + ".labelxml"

    # Check if output directory exists, if not create it
    if not os.path.exists(output_directory):
        os.makedirs(output_directory)

    # Load image
    image = cv2.imread(image_path)
    if image is None:
        print(f"Could not read image: {image_path}")
        return
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # Get image dimensions
    img_height, img_width = image.shape[:2]

    # Remove first 100 rows of the image by filling them with white color
    image_rgb[0:100, 0:img_width] = (255, 255, 255)

    # Load xml file
    tree = ET.parse(label_path)
    # Root is Annotations
    root = tree.getroot()

    areas = []
    areas_classes = []

    for annotation in root.findall('Annotation'):
        objects = annotation.findall('Object')
        if (len(objects) != 1):
            print(f"Warning: Found {len(objects)} objects in annotation, expected 1. Skipping annotation.")
            continue

        class_name = objects[0].find('Name').text
        print(f"Object name: {class_name}")

        bndbox = objects[0].find('Bndbox')
        center_x = float(bndbox.find('X').text)
        center_y = float(bndbox.find('Y').text)
        width = float(bndbox.find('Width').text)
        height = float(bndbox.find('Height').text)

        # Draw bounding box on the label
        top_left_x = int(center_x * img_width - (width * img_width) / 2)
        top_left_y = int(center_y * img_height - (height * img_height) / 2)

        bottom_right_x = top_left_x + int(width * img_width)
        bottom_right_y = top_left_y + int(height * img_height)

        class_idx = int(class_name)
        if (class_idx != 0):
            # Copy the bounding box area to the top-left corner of the image
            box_width = bottom_right_x - top_left_x
            box_height = bottom_right_y - top_left_y
            if (box_width > 0 and box_height > 0):
                cropped_box = image_rgb[top_left_y:bottom_right_y, top_left_x:bottom_right_x]
                areas.append(deepcopy(cropped_box))
                areas_classes.append(class_idx)

        # Remove it from XML
        root.remove(annotation)

    # Clear image
    image_rgb[0:img_height, 0:img_width] = (255, 255, 255)

    # Add the areas
    x_index = 0
    y_index = 0
    for idx, area in enumerate(areas):
        
        count = 20
        if (areas_classes[idx] == 1):
            count = 4
        if (areas_classes[idx] == 2):
            count = 12
        if (areas_classes[idx] == 4):
            count = 25
        if (areas_classes[idx] == 6):
            count = 15
        if (areas_classes[idx] == 8):
            count = 23

        print(f"Area len {len(area)}, width {area.shape[1]}, height {area.shape[0]}, class {areas_classes[idx]}")

        for _ in range(count):
            if ((x_index + area.shape[1]) >= img_width):
                x_index = 0
                y_index += 60

            if ((y_index + area.shape[0]) >= img_height):
                print("No more space to add areas, stopping.")
                break

            generated_classes[areas_classes[idx]] += 1

            image_rgb[y_index:y_index+area.shape[0], x_index:x_index+area.shape[1]] = area

            # Add some random points
            nb_points = np.random.randint(5,10)
            for _ in range(nb_points):
                random_x = np.random.randint(0, area.shape[1])
                random_y = np.random.randint(0, area.shape[0])
                image_rgb[y_index + random_y, x_index + random_x] = (0, 0, 0)  # Mark the random point in black

            # Add to xml file
            new_annotation = ET.Element('Annotation')
            new_object = ET.Element('Object')

            new_name = ET.Element('Name')
            new_name.text = str(areas_classes[idx])
            new_object.append(new_name)

            new_bndbox = ET.Element('Bndbox')
            new_object.append(new_bndbox)

            x_value = (x_index + area.shape[1] / 2) / img_width
            y_value = (y_index + area.shape[0] / 2) / img_height
            width_value = area.shape[1] / img_width
            height_value = area.shape[0] / img_height

            new_x = ET.Element('X')
            new_x.text = str(x_value)
            new_bndbox.append(new_x)
            new_y = ET.Element('Y')
            new_y.text = str(y_value)
            new_bndbox.append(new_y)
            new_width = ET.Element('Width')
            new_width.text = str(width_value)
            new_bndbox.append(new_width)
            new_height = ET.Element('Height')
            new_height.text = str(height_value)
            new_bndbox.append(new_height)

            new_annotation.append(new_object)
            root.append(new_annotation)

            x_index += area.shape[1] + 5

    if (display_image == True):
        cv2.imshow(f"Image: {os.path.basename(image_path)}", image_rgb)
        cv2.waitKey(0)
        cv2.destroyAllWindows()

    tree.write(output_label_path, xml_declaration=True, encoding="UTF-8")
    cv2.imwrite(output_image_path, image_rgb)

    if (image_debug_path is not None):
        cv2.imwrite(image_debug_path, image_rgb)

    return generated_classes

# Create new files for all sessions in the project with the modified images and xml files
def modify_all_project_sessions(project_path, output_base_directory):
    session_directories = extract_session_directories(project_path)

    generated_classes_total = []
    for _ in range(10):
        generated_classes_total.append(0)

    for session_directory in session_directories:
        print(f"Modifying session: {session_directory}")
        directory_obj = pathlib.Path(session_directory)
        output_directory = os.path.join(output_base_directory, directory_obj.name) + "_new"
        generated_classes = modify_session(session_directory, output_directory, output_base_directory, display_image=False)
        for i in range(len(generated_classes)):
            generated_classes_total[i] += generated_classes[i]

    plt.figure()
    bar_xlabels = [f'Class {i}' for i in range(len(generated_classes_total))]
    plt.bar(bar_xlabels, generated_classes_total, color='skyblue')
    plt.grid(True, axis='y', linestyle='--', alpha=0.7)
    plt.ylabel('Count')
    
    plt.title('Created Classes')
    plt.show()

def convert_to_yolo_format(session_directory, session_name, output_directory):
    print(f"Converting session to YOLO format: {session_directory} - name {session_name} -> {output_directory}")

    images_directory = output_directory + "\\images"
    labels_directory = output_directory + "\\labels"

    # Check if output directory exists, if not create it
    if not os.path.exists(images_directory):
        os.makedirs(images_directory)
    if not os.path.exists(labels_directory):
        os.makedirs(labels_directory)

    # Copy image to image directory
    shutil.copy(session_directory + "\\" + session_name + ".png", images_directory + "\\" + session_name + ".png")

    label_path = session_directory + "\\" + session_name + ".labelxml"

    # Generate label file
    file = open(labels_directory + "\\" + session_name + ".txt", "w")
    
    # Load xml file
    tree = ET.parse(label_path)
    root = tree.getroot()

    for annotation in root.findall('Annotation'):
        objects = annotation.findall('Object')
        if (len(objects) != 1):
            print(f"Warning: Found {len(objects)} objects in annotation, expected 1. Skipping annotation.")
            continue

        class_name = objects[0].find('Name').text
        print(f"Object name: {class_name}")

        bndbox = objects[0].find('Bndbox')
        center_x = float(bndbox.find('X').text)
        center_y = float(bndbox.find('Y').text)
        width = float(bndbox.find('Width').text)
        height = float(bndbox.find('Height').text)

        file.write(f"{class_name} {center_x} {center_y} {width} {height}\n")

    file.close()

# Convert all the sessions of the project into Yolo format
def improj_to_yolo(improj_path, output_directory):
    session_directories = extract_session_directories(improj_path)

    print(f"Converting {len(session_directories)} sessions to YOLO format...")

    for session_directory in session_directories:
        directory_obj = pathlib.Path(session_directory)
        convert_to_yolo_format(session_directory, directory_obj.name, output_directory)
        

if __name__ == "__main__":
    print('Augmentation and statistics')
    project_path = "C:\\rutronik\\deepcraft_workspace\\WaterCounterAnalysis\\WaterCounterAnalysis.improj"
    plot_distribution(project_path)
    
    #session_directories = extract_session_directories(project_path)
    #visualize_image(session_directories[1], display_info=True)

    # visualize_image("C:\\rutronik\\deepcraft_workspace\\WaterCounterAnalysis\\Data\\yolo_data_bw\\train\\images\\07e4060a-2_61")

    '''
    modify_session(session_directory="C:\\rutronik\\deepcraft_workspace\\WaterCounterAnalysis\\Data\\yolo_data_bw\\train\\images\\01b27fc6-116", 
                   output_directory=".\\test\\1",
                   display_image=False)
    visualize_image(".\\test\\1", display_info=False)
    '''

    #modify_all_project_sessions(project_path, output_base_directory=".\\modified_sessions")

    #improj_to_yolo(improj_path=project_path, output_directory=".\\yolo_data")
    
    '''
    convert_to_yolo_format("C:\\rutronik\\deepcraft_workspace\\WaterCounterAnalysis\\Data\\yolo_data_bw\\validation\\images\\41e42490-189", 
                           "41e42490-189",
                           ".\\yolo_data")
    '''