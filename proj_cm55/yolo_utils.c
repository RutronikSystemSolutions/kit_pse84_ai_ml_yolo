/*
 * yolo_utils.c
 *
 *  Created on: Jun 11, 2026
 *      Author: jorda
 *
 * Rutronik Elektronische Bauelemente GmbH Disclaimer: The evaluation board
 * including the software is for testing purposes only and,
 * because it has limited functions and limited resilience, is not suitable
 * for permanent use under real conditions. If the evaluation board is
 * nevertheless used under real conditions, this is done at one’s responsibility;
 * any liability of Rutronik is insofar excluded
 */

#include "yolo_utils.h"

#ifdef YOLO_INPUT_TYPE_INT8
/**
 * Helper function to convert int8 to float
 */
static inline float i8_to_float(int8_t raw_value) {
    return ((float)raw_value + 128.0f) / 255.0f;
}
#endif

/**
 * @brief Get the maximum confidence for one block
 */
#ifdef YOLO_INPUT_TYPE_INT8
void get_confidence_for_block(int8_t * output_buffer, int block_count, int class_count, int blk, float* max_confidence, int* class_idx)
#else
void get_confidence_for_block(float * output_buffer, int block_count, int class_count, int blk, float* max_confidence, int* class_idx)
#endif
{
	float max_conf = -1;
	int max_class_idx = -1;

#ifdef USE_YOLO_LEGACY
	// 5: x, y, width, height, block confidence
	int block_size = class_count + 5;
#ifdef YOLO_INPUT_TYPE_INT8
	float blk_confidence = i8_to_float(output_buffer[4 + (block_size * blk)]);
#else
	//float blk_confidence = ((float)output_buffer[4 + (block_size * blk)])/255.f;
	float blk_confidence = output_buffer[4 + (block_size * blk)];
#endif

	if (output_buffer[4 + (block_size * blk)] >= 0)
	{
		for (int class_idx = 0; class_idx < class_count; ++class_idx)
		{
			if (output_buffer[(5 + class_idx) + (block_size * blk)] >= 0)
			{
#ifdef YOLO_INPUT_TYPE_INT8
				float confidence = (float)i8_to_float(output_buffer[(5 + class_idx) + (block_size * blk)]);
#else
				//float confidence = ((float)output_buffer[(5 + class_idx) + (block_size * blk)])/255.f;
				float confidence = output_buffer[(5 + class_idx) + (block_size * blk)];
#endif

				confidence *= blk_confidence;
				if (confidence > max_conf)
				{
					max_conf = confidence;
					max_class_idx = class_idx;
				}
			}
		}
	}
#else
	for (int class_idx = 0; class_idx < class_count; ++class_idx)
	{
		float confidence = output_buffer[(block_count * (4 + class_idx)) + blk];
		if (confidence > max_conf)
		{
			max_conf = confidence;
			max_class_idx = class_idx;
		}
	}
#endif

	// Copy to output
	*max_confidence = max_conf;
	*class_idx = max_class_idx;
}

#ifdef YOLO_INPUT_TYPE_INT8
void get_rect_for_block(int8_t * output_buffer, rectangle_t* rect, int block_count, int class_count, int blk_index)
#else
void get_rect_for_block(float * output_buffer, rectangle_t* rect, int block_count, int class_count, int blk_index)
#endif
{
#ifdef USE_YOLO_LEGACY
	// 5: x, y, width, height, block confidence
	int block_size = class_count + 5;

#ifdef YOLO_INPUT_TYPE_INT8
	rect->origin_x =	i8_to_float(output_buffer[0 + (block_size * blk_index)]);
	rect->origin_y =	i8_to_float(output_buffer[1 + (block_size * blk_index)]);
	rect->width =		i8_to_float(output_buffer[2 + (block_size * blk_index)]);
	rect->height =		i8_to_float(output_buffer[3 + (block_size * blk_index)]);
#else
	rect->origin_x =	output_buffer[0 + (block_size * blk_index)];
	rect->origin_y =	output_buffer[1 + (block_size * blk_index)];
	rect->width =		output_buffer[2 + (block_size * blk_index)];
	rect->height =		output_buffer[3 + (block_size * blk_index)];
#endif // YOLO_INPUT_TYPE_INT8
#else
	rect->origin_x =	output_buffer[(block_count * 0) + blk_index];
	rect->origin_y =	output_buffer[(block_count * 1) + blk_index];
	rect->width =		output_buffer[(block_count * 2) + blk_index];
	rect->height =		output_buffer[(block_count * 3) + blk_index];
#endif
	rect->origin_x -= rect->width / 2.f;
	rect->origin_y -= rect->height / 2.f;
}
