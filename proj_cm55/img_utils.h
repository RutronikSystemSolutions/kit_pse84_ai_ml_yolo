/*
 * img_utils.h
 *
 *  Created on: Jan 23, 2026
 *      Author: jorda
 */

#ifndef IMG_UTILS_H_
#define IMG_UTILS_H_

#include <stdint.h>

typedef enum
{
	TYPE_RGB565
} img_utils_format_e;

void img_utils_to_gray_scale(uint8_t * buffer, uint16_t width, uint16_t height, img_utils_format_e format);

void img_utils_adaptive_threshold(uint8_t * buffer, uint16_t width, uint16_t height, img_utils_format_e format, int block_size, int cst, float* matrix);

void img_utils_crop(uint8_t * buffer, uint16_t width, uint16_t height, img_utils_format_e format, uint16_t x, uint16_t y, uint16_t w, uint16_t h);

#endif /* IMG_UTILS_H_ */
