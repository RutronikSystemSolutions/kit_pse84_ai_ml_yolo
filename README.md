# KIT PSE84 AI: YOLO usage on PSOC EDGE using OV7675 camera

This project collects the data of the OV7675 and perform an inference on it using a YOLO model.

The goal is to extract digits (to read a water counter), but you can change the YOLO model as you want.

A software is available inside the /gui directory to visualize the output.

<img src="pictures/gui_screenshot.png" style="zoom:50%;" />

## Requirements

- [ModusToolbox&trade;](https://www.infineon.com/modustoolbox) v3.6 or later (tested with v3.6)
- Board support package (BSP) minimum required version: 1.0.0
- Programming language: C
- [KIT_PSE84_AI](https://www.rutronik24.com/produit/infineon/kitpse84aitobo1/27510830.html)

<img src="pictures/kit_pse84_ai.png" style="zoom:50%;" />

## Supported toolchains (make variable 'TOOLCHAIN')

- GNU Arm&reg; Embedded Compiler v14.2.1 (`GCC_ARM`) – Default value of `TOOLCHAIN`
- LLVM Embedded Toolchain for Arm&reg; v19.1.5 (`LLVM_ARM`)


## Supported kits (make variable 'TARGET')

- [PSOC&trade; Edge E84 AI Kit](https://www.infineon.com/KIT_PSE84_AI) (`KIT_PSE84_AI`)


## Hardware setup

This example uses the board's default configuration.

## Software setup

See the [ModusToolbox&trade; tools package installation guide](https://www.infineon.com/ModusToolboxInstallguide) for information about installing and configuring the tools package.

Install a terminal emulator if you do not have one. Instructions in this document use [Tera Term](https://teratermproject.github.io/index-en.html).

This example requires no additional software or tools.


## Operation

See [Using the code example](doc/using_the_code_example.md) for instructions on creating a project, opening it in various supported IDEs, and performing tasks, such as building, programming, and debugging the application within the respective IDEs.

1. Connect the board to your PC using the provided USB cable through the KitProg3 USB connector.

2. Open a terminal program and select the KitProg3 COM port. Set the serial port parameters to 8N1 and 115200 baud

3. After programming, the application starts automatically

4. Confirm that a blue LED is flashing.

5. Connect the board to your PC through the PSOC E84 USB connector and start the GUI to stream the data.

## Configuration

The file cm55/Makefile enables you to choose the model type.

<img src="./pictures/makefile_choice.png" style="zoom:100%;" />

Possible combinations:
- Yolov5 legacy model generated using Deepcraft Studio
```
YOLO_VERSION=legacy
YOLO_EXPORT_WAY=deepcraft_studio
```
In that case, model/model_int8x8_legacy_dcs will be used
- Yolov5 legacy model, trained using Python scripts and generated using Deepcraft Model Converter v5.8.4
```
YOLO_VERSION=legacy
YOLO_EXPORT_WAY=model_converter
```
In that case, model/model_int8x8_legacy will be used
- Yolo model, trained using Python scripts and latest version of Yolo, generated using Deepcraft Model Converter v5.8.4
```
YOLO_VERSION=new
YOLO_EXPORT_WAY=model_converter
```
In that case, model/model_int8x8 will be used

## Legal Disclaimer

The evaluation board including the software is for testing purposes only and, because it has limited functions and limited resilience, is not suitable for permanent use under real conditions. If the evaluation board is nevertheless used under real conditions, this is done at one’s responsibility; any liability of Rutronik is insofar excluded. 

<img src="pictures/rutronik.png" style="zoom:50%;" />