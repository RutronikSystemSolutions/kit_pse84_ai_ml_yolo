################################################################################
# \file Makefile
# \version 1.0
#
# \brief
# Makefile for configuring the Cortex-M55 core project
#
################################################################################
# \copyright
# Copyright 2023-2025, Cypress Semiconductor Corporation (an Infineon company)
# SPDX-License-Identifier: Apache-2.0
# 
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# 
#     http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
################################################################################

include ../common.mk

################################################################################
# Basic Configuration
################################################################################


# Name of application (used to derive name of final linked file).
#
# If APPNAME is edited, ensure to update or regenerate launch
# configurations for your IDE.
APPNAME=proj_cm55

# If set to "true" or "1", display full command-lines when building.
VERBOSE=

# Neural Network Configuration. Options include
#
# float    -- floating point for the input data and weights
# int8x8   -- 8-bit fixed-point for the input data and weights
NN_TYPE=int8x8

# Model Name to be loaded to the firmware
NN_MODEL_NAME=DIGIT_DETECT

# Folder name containing the model and regression data
NN_MODEL_FOLDER=mtb_ml_gen

# Choose the inference engine
# tflm      -- TensorFlow Lite for Microcontrollers inference engine with interpreter
# Note tflm_less option is not supported on CM55+U55
NN_INFERENCE_ENGINE=tflm

################################################################################
# Advanced Configuration
################################################################################

# Enable optional code that is ordinarily disabled by default.
#
# Available components depend on the specific targeted hardware and firmware
# in use. In general, if you have
#
#    COMPONENTS=foo bar
#
# ... then code in directories named COMPONENT_foo and COMPONENT_bar will be
# added to the build
#
COMPONENTS+=USBD_BASE SHARED_MEM CMSIS_DSP CMSIS_NN U55

# Like COMPONENTS, but disable optional code that was enabled by default.
DISABLE_COMPONENTS=

CORE=CM55
CORE_NAME=CM55_0
NPU=U55

# By default the build system automatically looks in the Makefile's directory
# tree for source code and builds it. The SOURCES variable can be used to
# manually add source code to the build process from a location not searched
# by default, or otherwise not found by the build system.
SOURCES+=

# Select only the regression and model files that belong to the desired
# settings.
MODEL_PREFIX=$(subst $\",,$(NN_MODEL_NAME))
CY_IGNORE+=$(NN_MODEL_FOLDER)

# Check which kit is being used and apply the corresponding configurations
ifeq (APP_KIT_PSE84_AI, $(TARGET))
DEFINES+=USE_KIT_PSE84_AI
endif

# Add additional defines to the build process (without a leading -D).
DEFINES+=CY_RETARGET_IO_CONVERT_LF_TO_CRLF MTB_SHARED_MEM ARM_MATH_HELIUM ARM_MATH_DSP ARM_MATH_AUTOVECTORIZE

# Like SOURCES, but for include directories. Value should be paths to
# directories (without a leading -I).
INCLUDES+=$(NN_MODEL_FOLDER)/mtb_ml_models

# Add additional defines to the build process (without a leading -D).
ifeq (tflm, $(NN_INFERENCE_ENGINE))
COMPONENTS+=ML_TFLM
DEFINES+=TF_LITE_STATIC_MEMORY
endif

YOLO_VERSION=legacy
#YOLO_VERSION=new

YOLO_EXPORT_WAY=deepcraft_studio
#YOLO_EXPORT_WAY=model_converter

ifeq (deepcraft_studio, $(YOLO_EXPORT_WAY))
# When using Deepcraft Studio IMAI_Compute expects int8 format
DEFINES+=YOLO_INPUT_TYPE_INT8
DEFINES+=YOLO_DEEPCRAFT_STUDIO
else
# When using model converter (5.8.4...) input type is float
DEFINES+=YOLO_INPUT_TYPE_FLOAT
endif

ifeq (legacy, $(YOLO_VERSION))
# For legacy version
DEFINES+=ML_IMAGIMOB_CM55 MODEL_NAME=$(NN_MODEL_NAME)
DEFINES+=CY_ML_MODEL_MEM=.cy_socmem_data
DEFINES+=CY_ML_ARENA_MEM=.cy_socmem_data
DEFINES+=CY_ML_WORKING_MEM=.cy_xip
DEFINES+=USE_YOLO_LEGACY

CY_IGNORE+=model/model_int8x8.c
CY_IGNORE+=model/model_int8x8.h

ifeq (deepcraft_studio, $(YOLO_EXPORT_WAY))#
# Generated using DEEPCRAFT Model Converter (v. 5.8.4)
# Trained using Yolo v5 legacy (and Python script) then exported to TFLITE
CY_IGNORE+=model/model_int8x8_legacy.c
CY_IGNORE+=model/model_int8x8_legacy.h
else
# _DCS: generated using Deepcraft Studio
CY_IGNORE+=model/model_int8x8_legacy_dcs.c
CY_IGNORE+=model/model_int8x8_legacy_dcs.h
endif

else
# For YOLO newest version
DEFINES+=ML_IMAGIMOB_CM55 MODEL_NAME=$(NN_MODEL_NAME) CY_ML_MODEL_MEM=.cy_socmem_data
DEFINES+=CY_ML_ARENA_MEM=.cy_xip
DEFINES+=CY_ML_WORKING_MEM=.cy_xip

CY_IGNORE+=model/model_int8x8_legacy.c
CY_IGNORE+=model/model_int8x8_legacy.h

CY_IGNORE+=model/model_int8x8_legacy_dcs.c
CY_IGNORE+=model/model_int8x8_legacy_dcs.h

endif






# Depending which Neural Network Type, add a specific DEFINE and COMPONENT
ifeq (float, $(NN_TYPE))
COMPONENTS+=ML_FLOAT32
endif
ifeq (int8x8, $(NN_TYPE))
COMPONENTS+=ML_INT8x8
endif

CY_IGNORE+=$(SEARCH_cmsis)/COMPONENT_CMSIS_NN/Include/arm_nnfunctions.h
CY_IGNORE+=$(SEARCH_cmsis)/COMPONENT_CMSIS_NN/Source/ConvolutionFunctions/arm_convolve_1_x_n_s8.c
CY_IGNORE+=$(SEARCH_cmsis)/COMPONENT_CMSIS_NN/Source/ConvolutionFunctions/arm_convolve_1x1_s8_fast.c
CY_IGNORE+=$(SEARCH_cmsis)/COMPONENT_CMSIS_NN/Source/ConvolutionFunctions/arm_convolve_s8.c
CY_IGNORE+=$(SEARCH_cmsis)/COMPONENT_CMSIS_NN/Source/ConvolutionFunctions/arm_convolve_wrapper_s8.c
CY_IGNORE+=$(SEARCH_cmsis)/COMPONENT_CMSIS_NN/Source/ConvolutionFunctions/arm_depthwise_conv_s8_opt.c
CY_IGNORE+=$(SEARCH_cmsis)/COMPONENT_CMSIS_NN/Source/ConvolutionFunctions/arm_depthwise_conv_wrapper_s8.c

# Select softfp or hardfp floating point. Default is softfp.
VFP_SELECT+=hardfp

# Additional / custom C compiler flags.
#
# NOTE: Includes and defines should use the INCLUDES and DEFINES variable
# above.
CFLAGS+=

# Additional / custom C++ compiler flags.
#
# NOTE: Includes and defines should use the INCLUDES and DEFINES variable
# above.
CXXFLAGS+=

# Additional / custom assembler flags.
#
# NOTE: Includes and defines should use the INCLUDES and DEFINES variable
# above.
ASFLAGS+=

# Additional / custom linker flags.
LDFLAGS+=

# Additional / custom libraries to link in to the application.
LDLIBS+=

# Path to the linker script to use (if empty, use the default linker script).
LINKER_SCRIPT=

# Custom pre-build commands to run.
PREBUILD=

# Custom post-build commands to run.
POSTBUILD=


################################################################################
# Paths
################################################################################

# Relative path to the project directory (default is the Makefile's directory).
#
# This controls where automatic source code discovery looks for code.
CY_APP_PATH=

# Relative path to the shared repo location.
#
# All .mtb files have the format, <URI>#<COMMIT>#<LOCATION>. If the <LOCATION> 
# field begins with $$ASSET_REPO$$, then the repo is deposited in the path 
# specified by the CY_GETLIBS_SHARED_PATH variable. The default location is one 
# directory level above the current app directory. This is used with 
# CY_GETLIBS_SHARED_NAME variable, which specifies the directory name.
CY_GETLIBS_SHARED_PATH=../..

# Directory name of the shared repo location.
#
CY_GETLIBS_SHARED_NAME=mtb_shared

include $(CY_TOOLS_DIR)/make/start.mk
