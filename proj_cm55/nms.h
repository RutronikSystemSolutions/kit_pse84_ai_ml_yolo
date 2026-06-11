/*
 * nms.h
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

#ifndef NMS_H_
#define NMS_H_

#include <stdint.h>

#include "yolo_utils.h"

#ifdef YOLO_INPUT_TYPE_INT8
int nms(int8_t * output_buffer, int* detected_indices, int block_count, int class_count, float confidence_threshold);
#else
int nms(float * output_buffer, int* detected_indices, int block_count, int class_count, float confidence_threshold);
#endif

#endif /* NMS_H_ */
