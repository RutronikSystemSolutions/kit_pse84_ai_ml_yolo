/*
 * rectangle.c
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

#include "rectangle.h"

/**
 * @brief Check if 2 rectangles overlap or not
 *
 * @retval 0 not overlapping
 * @retval 1 overlapping
 */
int rectangle_overlap(rectangle_t rect1, rectangle_t rect2)
{
	float l1x = rect1.origin_x;
	float r1x = rect1.origin_x + rect1.width;

	float l2x = rect2.origin_x;
	float r2x = rect2.origin_x + rect2.width;

	float l1y = rect1.origin_y;
	float r1y = rect1.origin_y + rect1.height;
	float l2y = rect2.origin_y;
	float r2y = rect2.origin_y + rect2.height;

//	// Check if x coordinates overlap
//	if (l1x > r2x || l2x > r1x)
//	{
//		printf("if (l1x > r2x || l2x > r1x)\r\n");
//		return 0;
//	}
//
//	// Check if y coordinates overlap
//	if (r1y > l2y || r2y > l1y)
//	{
//		printf("if (r1y > l2y || r2y > l1y)\r\n");
//		return 0;
//	}

	// Check x first
	if ( (r1x < l2x) || (l1x > r2x)) return 0;

	// Check y then
	if ( (r1y < l2y) || (l1y > r2y)) return 0;

	return 1;
}
