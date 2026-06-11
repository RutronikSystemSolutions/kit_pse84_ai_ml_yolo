/*
 * rectangle.h
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

#ifndef RECTANGLE_H_
#define RECTANGLE_H_

typedef struct
{
	float origin_x;
	float origin_y;
	float width;
	float height;
} rectangle_t;

/**
 * @brief Check if 2 rectangles overlap or not
 *
 * @retval 0 not overlapping
 * @retval 1 overlapping
 */
int rectangle_overlap(rectangle_t rect1, rectangle_t rect2);

#endif /* RECTANGLE_H_ */
