/*
 * img_utils.c
 *
 *  Created on: Jan 23, 2026
 *      Author: jorda
 */


#include "img_utils.h"

#include <string.h> // For memset

//void img_utils_to_gray_scale(uint8_t * buffer, uint16_t width, uint16_t height, img_utils_format_e format)
//{
//	uint16_t * rbg565_data = (uint16_t*)buffer;
//
//	if (format != TYPE_RGB565) return;
//
//	for(int y = 0; y < height; ++y)
//	{
//		for(int x = 0; x < width; ++x)
//		{
//			// source format is RGB565
//			uint16_t pixel = rbg565_data[y * width + x];
//			int r = ((pixel >> 11) & 0x1f) << 3; // 8 bits
//			int g = ((pixel >> 5) & 0x3f) << 2;  // 8 bits
//			int b = ((pixel >> 0) & 0x1f) << 3;  // 8 bits
//
//			float fr = 0.3f * (float)r;
//			float fg = 0.59f * (float)g;
//			float fb = 0.11f * (float)b;
//
//			// This one is on 8 bits maximum
//			int gs = (int) (fr + fg + fb);
//
//			uint8_t gs_5bits = (uint8_t) (gs >> 3);
//			uint8_t gs_6bits = (uint8_t) (gs >> 2);
//
//			// Convert it back
//			rbg565_data[y * width + x] = (((uint16_t)gs_5bits) << 11)
//					| (((uint16_t)gs_6bits) << 5)
//					| (uint16_t)gs_5bits;
//		}
//	}
//}

/**
 * @brief Convert (/!\ in place modification) the RGB image inside buffer into a grayscale image
 */
void img_utils_to_gray_scale(uint8_t * buffer, uint16_t width, uint16_t height, img_utils_format_e format)
{
	uint16_t * rbg565_data = (uint16_t*)buffer;

	if (format != TYPE_RGB565) return;

	for(int y = 0; y < height; ++y)
	{
		for(int x = 0; x < width; ++x)
		{
			// source format is RGB565
			uint16_t pixel = rbg565_data[y * width + x];
			int r = ((pixel >> 11) & 0x1f) << 3; // 8 bits
			int g = ((pixel >> 5) & 0x3f) << 2;  // 8 bits
			int b = ((pixel >> 0) & 0x1f) << 3;  // 8 bits

			float fr = 0.3f * (float)r;
			float fg = 0.59f * (float)g;
			float fb = 0.11f * (float)b;

			// This one is on 8 bits maximum
			int gs = (int) (fr + fg + fb);

//			uint8_t gs_5bits = (uint8_t) (gs >> 3);
//			uint8_t gs_6bits = (uint8_t) (gs >> 2);

			// Convert it back
			// Remark: store the "gray" value as a 16 bits value! (do not convert is back to 5 or 6 bits)
			rbg565_data[y * width + x] = (uint16_t) gs;
		}
	}
}

static int max(int a, int b)
{
	if (a > b) return a;
	return b;
}

static int min(int a, int b)
{
	if (a < b) return a;
	return b;
}

//void img_utils_adaptive_threshold(uint8_t * buffer, uint16_t width, uint16_t height, img_utils_format_e format, int block_size, int cst, float* matrix)
//{
//	uint16_t * rbg565_data = (uint16_t*)buffer;
//
//	// Clear the matrix
//	memset(matrix, 0, sizeof(float)*((width+1)*(height+1)));
//
//	// Compute integral image
//	for (int y = 0; y < height; ++y)
//	{
//		float row_sum = 0;
//		for (int x = 0; x < width; ++x)
//		{
//			uint16_t pixel = rbg565_data[y * width + x];
//			int r = ((pixel >> 11) & 0x1f) << 3; // 8 bits
//			row_sum += r;
//			matrix[(x + 1) + ((y + 1) * (width+1))] = row_sum + matrix[(x + 1) + (y * (width+1))];
//		}
//	}
//
//	int radius = block_size / 2;
//
//	// Calculate Thresholds using Integral Image
//	for (int y = 0; y < height; y++)
//	{
//	    for (int x = 0; x < width; x++)
//	    {
//	        // Define the bounding box of the neighborhood
//	        int x1 = max(1, x - radius + 1);
//	        int y1 = max(1, y - radius + 1);
//	        int x2 = min(width, x + radius + 1);
//	        int y2 = min(height, y + radius + 1);
//
//	        // Quick area sum calculation: Area = BottomRight + TopLeft - BottomLeft - TopRight
//	        float count = (x2 - x1 + 1) * (y2 - y1 + 1);
//	        float sum = matrix[(x2) + (y2 * (width+1))]
//									  	  	  	  	  - matrix[(x1 - 1) + (y2 * (width+1))]
//													  - matrix[(x2) + ((y1 - 1) * (width+1))]
//													  + matrix[(x1 - 1) + ((y1 - 1) * (width+1))];
//
//
//	        float threshold = ((float)sum / (float)count) - cst;
//
//	        uint16_t pixel = rbg565_data[y * width + x];
//	        int r = ((pixel >> 11) & 0x1f) << 3; // 8 bits
//
//	        if (r > threshold)
//	        {
//	        	rbg565_data[y * width + x] = 0xFFFF;
//	            // retval.SetPixel(x, y, Color.White);
//	        }
//	        else
//	        {
//	        	rbg565_data[y * width + x] = 0x0;
//	            //retval.SetPixel(x, y, Color.Black);
//	        }
//	    }
//	}
//}

void img_utils_adaptive_threshold(uint8_t * buffer, uint16_t width, uint16_t height, img_utils_format_e format, int block_size, int cst, float* matrix)
{
	uint16_t * rbg565_data = (uint16_t*)buffer;

	// Clear the matrix
	memset(matrix, 0, sizeof(float)*((width+1)*(height+1)));

	// Compute integral image
	for (int y = 0; y < height; ++y)
	{
		float row_sum = 0;
		for (int x = 0; x < width; ++x)
		{
			uint16_t pixel = rbg565_data[y * width + x];
			int r = pixel;
			row_sum += r;
			matrix[(x + 1) + ((y + 1) * (width+1))] = row_sum + matrix[(x + 1) + (y * (width+1))];
		}
	}


	int radius = block_size / 2;

	// Calculate Thresholds using Integral Image
	for (int y = 0; y < height; y++)
	{
	    for (int x = 0; x < width; x++)
	    {
	        // Define the bounding box of the neighborhood
	        int x1 = max(1, x - radius + 1);
	        int y1 = max(1, y - radius + 1);
	        int x2 = min(width, x + radius + 1);
	        int y2 = min(height, y + radius + 1);

	        // Quick area sum calculation: Area = BottomRight + TopLeft - BottomLeft - TopRight
	        float count = (x2 - x1 + 1) * (y2 - y1 + 1);
	        float sum = matrix[(x2) + (y2 * (width+1))]
									  	  	  	  	  - matrix[(x1 - 1) + (y2 * (width+1))]
													  - matrix[(x2) + ((y1 - 1) * (width+1))]
													  + matrix[(x1 - 1) + ((y1 - 1) * (width+1))];


	        float threshold = ((float)sum / (float)count) - cst;

	        uint16_t pixel = rbg565_data[y * width + x];
	        int r = pixel;

	        if (r > threshold)
	        {
	        	rbg565_data[y * width + x] = 0xFFFF;
	            // retval.SetPixel(x, y, Color.White);
	        }
	        else
	        {
	        	rbg565_data[y * width + x] = 0x0;
	            //retval.SetPixel(x, y, Color.Black);
	        }
	    }
	}
}

void img_utils_crop(uint8_t * buffer, uint16_t width, uint16_t height, img_utils_format_e format, uint16_t cx, uint16_t cy, uint16_t cw, uint16_t ch)
{
	uint16_t * rbg565_data = (uint16_t*)buffer;

	uint16_t lx = cx;
	uint16_t ly = cy;
	uint16_t rx = cx + cw;
	uint16_t ry = cy + ch;

	for (uint16_t x = 0; x < width; ++x)
	{
		for (uint16_t y = 0; y < height; ++y)
		{
			uint8_t inside = 1;

			// within crop zone or not?
			if ((x < lx) || (x > rx)) inside = 0;
			if ((y < ly) || (y > ry)) inside = 0;

			if (inside == 0)
			{
				// set it to white if outside
				rbg565_data[y * width + x] = 0xFFFF;
			}
		}
	}
}
