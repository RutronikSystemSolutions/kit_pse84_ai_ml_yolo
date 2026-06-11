/******************************************************************************
* File Name:   main.c
*
* Description:	Perform YOLO inference on the stream coming from the OV7675 camera
* 				Goal is to extract digits (numbers) out of it
*
* Related Document: See README.md
*
*
*  Created on: 2025-12-08
*  Company: Rutronik Elektronische Bauelemente GmbH
*  Address: Industriestraße 2, 75228 Ispringen, Germany
*  Author: RJ030
*
*******************************************************************************
* Copyright 2023-2025, Cypress Semiconductor Corporation (an Infineon company) or
* an affiliate of Cypress Semiconductor Corporation.  All rights reserved.
*
* This software, including source code, documentation and related
* materials ("Software") is owned by Cypress Semiconductor Corporation
* or one of its affiliates ("Cypress") and is protected by and subject to
* worldwide patent protection (United States and foreign),
* United States copyright laws and international treaty provisions.
* Therefore, you may use this Software only as provided in the license
* agreement accompanying the software package from which you
* obtained this Software ("EULA").
* If no EULA applies, Cypress hereby grants you a personal, non-exclusive,
* non-transferable license to copy, modify, and compile the Software
* source code solely for use in connection with Cypress's
* integrated circuit products.  Any reproduction, modification, translation,
* compilation, or representation of this Software except as specified
* above is prohibited without the express written permission of Cypress.
*
* Disclaimer: THIS SOFTWARE IS PROVIDED AS-IS, WITH NO WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, NONINFRINGEMENT, IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. Cypress
* reserves the right to make changes to the Software without notice. Cypress
* does not assume any liability arising out of the application or use of the
* Software or any product or circuit described in the Software. Cypress does
* not authorize its products for use in any products where a malfunction or
* failure of the Cypress product may reasonably be expected to result in
* significant property damage, injury or death ("High Risk Product"). By
* including Cypress's product in a High Risk Product, the manufacturer
* of such system or application assumes all risk of such use and in doing
* so agrees to indemnify Cypress against all liability.
*
* Rutronik Elektronische Bauelemente GmbH Disclaimer: The evaluation board
* including the software is for testing purposes only and,
* because it has limited functions and limited resilience, is not suitable
* for permanent use under real conditions. If the evaluation board is
* nevertheless used under real conditions, this is done at one’s responsibility;
* any liability of Rutronik is insofar excluded
*******************************************************************************/

#include "cybsp.h"
#include "cy_pdl.h"

#include <stdlib.h>
#include <string.h>

// Access to retarget-io initialization
// used to have printf over KitProg3
#include "driver/retarget_io/retarget_io_init.h"

// Driver of the OV7675 camera (over DVP for stream and I2C for configuration)
#include "driver/ov7675/mtb_dvp_camera_ov7675.h"

// Driver for USBD support (using emUSB from Seeger)
#include "driver/usbd/usbd.h"

#include "crc.h"
#include "rectangle.h"
#include "nms.h"

// Import model (multiple versions are available)
#ifdef USE_YOLO_LEGACY
#ifdef YOLO_DEEPCRAFT_STUDIO
// Trained and exported using Deepcraft Studio
#include "model/model_int8x8_legacy_dcs.h"
#else
// Trained using Yolo v5 legacy repository and exported using Deepcraft Model Converter version 5.8.4
#include "model/model_int8x8_legacy.h"
#endif
#else
// Trained using new Yolo
#include "model/model_int8x8.h"
#endif

#include "cycfg_qspi_memslot.h"
#include "mtb_serial_memory.h"
#define SMIF_INIT_TIMEOUT_USEC              (1000u)  // 1ms SMIF init timeout

#include "ifx_time_utils.h"
#include "img_utils.h"

/**
 * @def COM_OVERHEAD
 * Size of the header packet (USB communication)
 * Used for synchronization and validation
 */
#define COM_OVERHEAD	8

/**
 * @def COM_EXTRACTED_SIZE
 * Size of the buffer to signal NN output
 */
#define COM_NN_OUT_SIZE	8

/**
 * @def COM_CMD_SIZE
 * Size of a command from computer to PSoC Edge
 */
#define COM_CMD_SIZE	1

/**
 * @def COM_CMD_START_STREAM
 * Command enabling to start the streaming of data
 */
#define COM_CMD_START_STREAM	49

/**
 * @brief Access to external PSRAM
 */
static mtb_serial_memory_t serial_memory_obj;
static cy_stc_smif_mem_context_t smif_mem_context;
static cy_stc_smif_mem_info_t smif_mem_info;

/**
 * @brief Matrix needed when computing adaptive threshold on a picture
 * Big place needed, so place it in the external RAM
 */
CY_SECTION(".cy_xip") __attribute__((used))
float adapt_threshold_matrix[(OV7675_FRAME_WIDTH+1) * (OV7675_FRAME_HEIGHT+1)];

/**
 * @brief Output buffer that will be used to extract the digits
 * NMS algorithm will be used in place
 *
 * /!\ This needs to match your model output topology
 * See IMAI_DATA_OUT_SHAPE
 */
#ifdef USE_YOLO_LEGACY
CY_SECTION(".cy_xip") __attribute__((used))
int detected_indices[6300];
#else
CY_SECTION(".cy_xip") __attribute__((used))
int detected_indices[2100];
#endif

/**
 * @brief Report an error
 */
void report_error(char *message, uint32_t status)
{
   printf("\n\r====================================================\n\r");
   printf("\n\rFAIL: %s\n\r", message);
   printf("Error Code: 0x%x\n\r", (int) status);
   printf("\n\r====================================================\n\r");

   // On failure, turn the LED ON and stop
   Cy_GPIO_Set(CYBSP_USER_LED_PORT, CYBSP_USER_LED_PIN);
   while(true);
}

/**
 * @brief Initialize access to the external PSRAM
 */
static void smif_ospi_psram_init(void)
{
	cy_rslt_t result;

	// Disable SMIF Block for reconfiguration.
	Cy_SMIF_Disable(CYBSP_SMIF_CORE_1_PSRAM_HW);

	// Initialize SMIF-1 Peripheral.
	result = Cy_SMIF_Init((CYBSP_SMIF_CORE_1_PSRAM_hal_config.base),
						   (CYBSP_SMIF_CORE_1_PSRAM_hal_config.config),
						   SMIF_INIT_TIMEOUT_USEC, &smif_mem_context.smif_context);

	if (result) report_error("Cy_SMIF_Init failed", result);

	// Configure Data Select Option for SMIF-1
	Cy_SMIF_SetDataSelect(CYBSP_SMIF_CORE_1_PSRAM_hal_config.base,
						  smif1BlockConfig.memConfig[0]->slaveSelect,
						  smif1BlockConfig.memConfig[0]->dataSelect);

	// Enable the SMIF_CORE_1 block.
	Cy_SMIF_Enable(CYBSP_SMIF_CORE_1_PSRAM_hal_config.base, &smif_mem_context.smif_context);

	// Set-up serial memory.
	result = mtb_serial_memory_setup(&serial_memory_obj,
								MTB_SERIAL_MEMORY_CHIP_SELECT_2,
								CYBSP_SMIF_CORE_1_PSRAM_hal_config.base,
								CYBSP_SMIF_CORE_1_PSRAM_hal_config.clock,
								&smif_mem_context,
								&smif_mem_info,
								&smif1BlockConfig);

	if (result) report_error("serial memory setup failed", result);
}



/**
 * Convert a OV7675 frame (in RGB565 format) into a float Tensor
 * that will be used for the inference
 *
 * @param [in] data Buffer representing a frame from the OV7675 in RGB565 format
 * @param [out] tensor Tensor to be used as input for the inference
 *
 * Remark: perform a "vertical" flip of the picture
 */
#ifdef YOLO_INPUT_TYPE_INT8
static void ov7675_to_tensor(uint8_t* data, uint8_t* tensor)
#else
static void ov7675_to_tensor(uint8_t* data, float* tensor)
#endif
{
	const int WIDTH = OV7675_FRAME_WIDTH;
	const int HEIGHT = OV7675_FRAME_HEIGHT;

	uint16_t * rbg565_data = (uint16_t*)data;

	for(int y = 0; y < HEIGHT; ++y)
	{
		for(int x = 0; x < WIDTH; ++x)
		{
			// RGB565 to 24bits RGB
			uint16_t pixel = rbg565_data[y * WIDTH + x];
			int r = ((pixel >> 11) & 0x1f) << 3;
			int g = ((pixel >> 5) & 0x3f) << 2;
			int b = ((pixel >> 0) & 0x1f) << 3;

			// ****************
			// Flip vertically !
			// Width -  1 - x
			int start_index = ((y * WIDTH) + (WIDTH - 1 - x)) * 3;

#ifdef YOLO_INPUT_TYPE_INT8
			tensor[start_index + 0] = (uint8_t)r;
			tensor[start_index + 1] = (uint8_t)g;
			tensor[start_index + 2] = (uint8_t)b;
#else
			tensor[start_index + 0] = (float)r / 255.f;
			tensor[start_index + 1] = (float)g / 255.f;
			tensor[start_index + 2] = (float)b / 255.f;
#endif
		}
	}
}

int main(void)
{
	// Used to store video stream
	// Double buffering used
	uint8_t* image_buffer_0 = NULL;
	uint8_t* image_buffer_1 = NULL;

	uint8_t* comm_buffer = NULL;
	uint8_t* camera_out_buffer = NULL;

	cy_stc_scb_i2c_context_t i2c_master_context;
	usbd_t* usb_handle;

	bool frame_ready = false;
	bool active_frame = false;

	int send_data = 0;

	uint8_t counterint = 0;

    cy_rslt_t result;

    // Initialize the device and board peripherals
    result = cybsp_init();
    if (CY_RSLT_SUCCESS != result)
    {
        CY_ASSERT(0);
    }

    // Init retarget-io -> printf redirected to KitProg3
	init_retarget_io();

    // Enable global interrupts
    __enable_irq();

	printf("PSOC EDGE - Yolo Inference v1.0\r\n");

	// Initialize PSRAM and set-up serial memory
	smif_ospi_psram_init();

	// Enable XIP mode for the SMIF memory slot associated with the PSRAM.
	result = mtb_serial_memory_enable_xip(&serial_memory_obj, true);
	if (result) report_error("mtb_serial_memory_enable_xip: failed", result);

	// Enable write for the SMIF memory slot associated with the PSRAM.
	result = mtb_serial_memory_set_write_enable(&serial_memory_obj, true);
	if (result) report_error("mtb_serial_memory_set_write_enable: failed", result);

	// Start timer stuff to measure inference time
	ifx_time_start();

	// Initialize model
	if (IMAI_init() != IPWIN_RET_SUCCESS)
	{
		printf("Error calling IMAI_init. Exit...\r\n");
		return -1;
	}
	printf("Model initialized successfully\r\n");

	// Allocate buffer in RAM for input and output of the model
#ifdef YOLO_INPUT_TYPE_INT8
	uint8_t * input_buffer = malloc(IMAI_DATA_IN_COUNT * sizeof(uint8_t));
	int8_t * output_buffer = malloc(IMAI_DATA_OUT_COUNT * sizeof(int8_t));
#else
	float * input_buffer = malloc(IMAI_DATA_IN_COUNT * sizeof(float));
	float * output_buffer = malloc(IMAI_DATA_OUT_COUNT * sizeof(float));
#endif

	// Error handling
	if (input_buffer == NULL)
	{
		printf("Cannot allocate input buffer. Exit...\r\n");
		return -1;
	}
	if (output_buffer == NULL)
	{
		printf("Cannot allocate output buffer. Exit...\r\n");
		return -1;
	}

    // Enable I2C Controller -> used to configure the OV7675
    result = Cy_SCB_I2C_Init(CYBSP_I2C_CAM_CONTROLLER_HW, &CYBSP_I2C_CAM_CONTROLLER_config,
                             &i2c_master_context);
    if (CY_SCB_I2C_SUCCESS != result)
    {
        CY_ASSERT(0);
    }
    Cy_SCB_I2C_Enable(CYBSP_I2C_CAM_CONTROLLER_HW);

    // Memory allocation
    image_buffer_0 = malloc(OV7675_MEMORY_BUFFER_SIZE);
	if (image_buffer_0 == NULL)
	{
		printf("Cannot allocate image_buffer_0 \r\n");
		return 0;
	}
	memset(image_buffer_0, 0, OV7675_MEMORY_BUFFER_SIZE);

	image_buffer_1 = malloc(OV7675_MEMORY_BUFFER_SIZE);
	if (image_buffer_1 == NULL)
	{
		printf("Cannot allocate image_buffer_1 \r\n");
		return 0;
	}

	camera_out_buffer = malloc(OV7675_MEMORY_BUFFER_SIZE);
	if (camera_out_buffer == NULL)
	{
		printf("Cannot allocate camera_out_buffer \r\n");
		return 0;
	}

	// Allocate communication buffer
	comm_buffer = malloc(OV7675_MEMORY_BUFFER_SIZE + COM_OVERHEAD);
	if (comm_buffer == NULL)
	{
		printf("Cannot allocate comm_buffer \r\n");
		return 0;
	}

	// Init USB CDC
	usb_handle = usbd_create();

    // Initialize the camera DVP OV7675
    result = mtb_dvp_cam_ov7675_init(image_buffer_0, image_buffer_1,
    		&i2c_master_context,
			&frame_ready, &active_frame);
    if (CY_RSLT_SUCCESS != result)
    {
		printf("Cannot initialize OV7675 \r\n");
		return 0;
    }

	printf("Sensors have been initialized - Start collecting data and inference \r\n");

    for (;;)
    {
    	uint8_t cmd = 0;

    	// Something in USB read buffer?
    	if ( usbd_read(usb_handle, &cmd, COM_CMD_SIZE) == COM_CMD_SIZE)
    	{
    		printf("Received command: %d \r\n", cmd);
    		if (cmd == COM_CMD_START_STREAM)
    		{
    			send_data = 1;
    			counterint = 0;
    		}
    		else send_data = 0;
    	}

    	// Frame ready from the OV7675?
		if (frame_ready)
		{
			frame_ready = false;

			//  Copy to the communication buffer
			if (active_frame == 0)
			{
				memcpy(&comm_buffer[COM_OVERHEAD], image_buffer_0, OV7675_MEMORY_BUFFER_SIZE);
				memcpy(camera_out_buffer, image_buffer_0, OV7675_MEMORY_BUFFER_SIZE);
			}
			else
			{
				memcpy(&comm_buffer[COM_OVERHEAD], image_buffer_1, OV7675_MEMORY_BUFFER_SIZE);
				memcpy(camera_out_buffer, image_buffer_1, OV7675_MEMORY_BUFFER_SIZE);
			}

			Cy_GPIO_Write(CYBSP_USER_LED2_PORT, CYBSP_USER_LED2_PIN, 1);
			{
				ifx_time_reset();
				{
					// Convert to gray scale and apply adaptive threshold
					img_utils_to_gray_scale(camera_out_buffer, 320, 240, TYPE_RGB565);
					img_utils_adaptive_threshold(camera_out_buffer, 320, 240, TYPE_RGB565, 11, 3, adapt_threshold_matrix);
					img_utils_crop(camera_out_buffer, 320, 240, TYPE_RGB565, 10, 130, 280, 100);
				}
				float pre_processing_stop = ifx_time_get_ms_f();

				// Convert picture to tensor (and flip it)
				ov7675_to_tensor(camera_out_buffer, input_buffer);

				ifx_time_reset();
				{
					// Inference on it
					IMAI_compute(input_buffer, output_buffer);
				}
				float time_stop = ifx_time_get_ms_f();

				printf("Pre-processing takes %.1f ms. Inference takes %.1f ms\r\n", pre_processing_stop, time_stop);
			}
			Cy_GPIO_Write(CYBSP_USER_LED2_PORT, CYBSP_USER_LED2_PIN, 0);

			// Send picture over USB
			// Add overhead
			comm_buffer[0] = 0x55;
			comm_buffer[1] = 0x55;
			comm_buffer[2] = counterint;
			counterint++;
			*((uint32_t*)&comm_buffer[3]) = OV7675_MEMORY_BUFFER_SIZE;
			comm_buffer[7] = crc_compute(&comm_buffer[COM_OVERHEAD], OV7675_MEMORY_BUFFER_SIZE);

			// Send per USB
			if (send_data == 1)
			{
				Cy_GPIO_Write(CYBSP_LED_RGB_GREEN_PORT, CYBSP_LED_RGB_GREEN_PIN, 1);
				if (usbd_write(usb_handle, comm_buffer, OV7675_MEMORY_BUFFER_SIZE + COM_OVERHEAD) != 0)
				{
					printf("Failed to write OV7675 values over USB\r\n");
					send_data = 0;
				}
				Cy_GPIO_Write(CYBSP_LED_RGB_GREEN_PORT, CYBSP_LED_RGB_GREEN_PIN, 0);
			}

#ifdef USE_YOLO_LEGACY
			int block_count = IMAI_DATA_OUT_SHAPE[1];
			// -5 because:
			// 0: x, 1: y, 2: width, 3: height, 4: block confidence, 5: class 0 confidence, 6: class 1 confidence...
			int class_count = IMAI_DATA_OUT_SHAPE[0] - 5;
#else
			int block_count = IMAI_DATA_OUT_SHAPE[0];
			// -4 because:
			// 0: x, 1: y, 2: width, 3: height, 4: class 0 confidence, 5: class 1 confidence...
			int class_count = IMAI_DATA_OUT_SHAPE[1] - 4;
#endif

			// Apply NMS algorithm to extract boxes
			int nms_count = nms(output_buffer, detected_indices, block_count, class_count, 0.5f);

			int digit_counter = 0;

			for(int i = 0;  i < COM_NN_OUT_SIZE; ++i)
				comm_buffer[COM_OVERHEAD + i] = 0xFF;

			for(;;)
			{
				// Check for overflow...
				if (digit_counter >= COM_NN_OUT_SIZE)
					break;

				// Select the digit with the smallest X position and continue (we want the digits to be sorted by X position)
				float min_x = 0.f;
				int min_idx = -1;
				for (int i = 0; i < nms_count; ++i)
				{
					if (detected_indices[i] < 0) continue;

					rectangle_t rect;
					get_rect_for_block(output_buffer, &rect, block_count, class_count, detected_indices[i]);

					if (min_idx < 0)
					{
						min_idx = i;
						min_x = rect.origin_x;
					}
					else if (rect.origin_x < min_x)
					{
						min_idx = i;
						min_x = rect.origin_x;
					}
				}

				if (min_idx < 0)
				{
					printf("\r\n");
					break;
				}

				float max_conf = -1;
				int max_class_idx = -1;
				get_confidence_for_block(output_buffer, block_count, class_count, detected_indices[min_idx], &max_conf, &max_class_idx);

				printf("%d [%.2f];", max_class_idx, max_conf);

				detected_indices[min_idx] = -1;

				comm_buffer[COM_OVERHEAD + digit_counter] = (uint8_t) max_class_idx;
				digit_counter++;
			}


			// Send detected digits
			// Add overhead
			comm_buffer[0] = 0x55;
			comm_buffer[1] = 0x55;
			comm_buffer[2] = counterint;
			counterint++;
			*((uint32_t*)&comm_buffer[3]) = COM_NN_OUT_SIZE;
			comm_buffer[7] = crc_compute(&comm_buffer[COM_OVERHEAD], COM_NN_OUT_SIZE);

			// Send per USB
			if (send_data == 1)
			{
				Cy_GPIO_Write(CYBSP_LED_RGB_GREEN_PORT, CYBSP_LED_RGB_GREEN_PIN, 1);
				if (usbd_write(usb_handle, comm_buffer, COM_NN_OUT_SIZE + COM_OVERHEAD) != 0)
				{
					printf("Failed to write NN output values over USB\r\n");
					send_data = 0;
				}
				Cy_GPIO_Write(CYBSP_LED_RGB_GREEN_PORT, CYBSP_LED_RGB_GREEN_PIN, 0);
			}
		}
    }
}
