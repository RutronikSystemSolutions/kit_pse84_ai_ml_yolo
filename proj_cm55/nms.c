/*
 * nms.c
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

#include "nms.h"

/**
 * @brief block_count: #define IMAI_DATA_OUT_SHAPE (((int[]){2100, 14}) -> 2100
 */
#ifdef YOLO_INPUT_TYPE_INT8
int nms(int8_t * output_buffer, int* detected_indices, int block_count, int class_count, float confidence_threshold)
#else
int nms(float * output_buffer, int* detected_indices, int block_count, int class_count, float confidence_threshold)
#endif
{
	int detected_count = 0;

	// Do a first pass, to check which detection block is above threshold
	for(int blk = 0; blk < block_count; ++blk)
	{
		float max_conf = -1;
		int max_class_idx = -1;

		get_confidence_for_block(output_buffer, block_count, class_count, blk, &max_conf, &max_class_idx);

		if (max_conf > confidence_threshold)
		{
			detected_indices[detected_count] = blk;
			detected_count++;
		}
	}

	// Check intersection
	for (int i = 0; i < detected_count; ++i)
	{
		for (int j = (i + 1); j < detected_count; ++j)
		{
			// Still valid?
			if (detected_indices[i] < 0) break;
			if (detected_indices[j] < 0) continue;

			rectangle_t rect1;
			rectangle_t rect2;

			get_rect_for_block(output_buffer, &rect1, block_count, class_count, detected_indices[i]);
			get_rect_for_block(output_buffer, &rect2, block_count, class_count, detected_indices[j]);


			if (rectangle_overlap(rect1, rect2))
			{
				// TODO keep the one with the biggest confidence
				detected_indices[j] = -1;
			}
		}
	}

	return detected_count;
}
