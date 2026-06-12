import cv2
import os
from pathlib import Path

print('Apply adaptive threshold')

cwd = os.getcwd()

data_path = "rgb_picture_directory"
output_path = "bw_directory/images"

input_image_path = os.path.join(data_path,'images')

img_file_list = [path for path in Path(input_image_path).rglob('*')]

for img_path in img_file_list:
    print(f'Processing image file: {img_path}')
    image_src = cv2.imread(img_path)
    # Convert to grayscale
    gray = cv2.cvtColor(image_src, cv2.COLOR_BGR2GRAY)

    # Apply adaptive thresholding
    binary_picture = cv2.adaptiveThreshold(gray,255,cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY,11,3)

    output_file_path = os.path.join(output_path, os.path.basename(img_path))
    cv2.imwrite(output_file_path, binary_picture)

print("Done")