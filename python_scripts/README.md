An application note will be available soon, to explain how to train and deploy a YOLO model using:
- YOLOv5 legacy
- Newer version of YOLO

Contact us if you are interested solutions@rutronik.com


## dataset_split.py:
Enables to split a dataset into 2 parts: one for training and one for validation.

To use it, modify lines 9, 10 and 11 according to your needs:

```
data_path = "data\\train"
yolo_output = "data_test"
train_percent = 0.9
```

## adaptive_threshold_single_file.py
Read a RBG BMP file, and apply an adaptive threshold to it to convert it to a black and white picture.

## adaptive_threshold_conversion.py
Convert all pictures inside a directory (RBG) to black and white pictures.

## analyse_output.py
Enable to analyse the output of the training by Deepcraft(TM) Studio to see what has been discovered.

## deepcraft_utils.py
Enable to do data augmentation based on a ".improj" project. Enable to plot statistics. Enable to convert it to YOLO format.