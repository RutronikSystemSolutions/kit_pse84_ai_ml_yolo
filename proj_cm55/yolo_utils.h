/*
 * yolo_utils.h
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

#ifndef YOLO_UTILS_H_
#define YOLO_UTILS_H_

#include <stdint.h>
#include "rectangle.h"

#ifdef YOLO_INPUT_TYPE_INT8
void get_confidence_for_block(int8_t * output_buffer, int block_count, int class_count, int blk, float* max_confidence, int* class_idx);
#else
void get_confidence_for_block(float * output_buffer, int block_count, int class_count, int blk, float* max_confidence, int* class_idx);
#endif

#ifdef YOLO_INPUT_TYPE_INT8
void get_rect_for_block(int8_t * output_buffer, rectangle_t* rect, int block_count, int class_count, int blk_index);
#else
void get_rect_for_block(float * output_buffer, rectangle_t* rect, int block_count, int class_count, int blk_index);
#endif

#endif /* YOLO_UTILS_H_ */
