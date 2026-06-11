/*
 * crc.c
 *
 *  Created on: Dec 8, 2025
 *      Author: ROJ030
 *
 * Rutronik Elektronische Bauelemente GmbH Disclaimer: The evaluation board
 * including the software is for testing purposes only and,
 * because it has limited functions and limited resilience, is not suitable
 * for permanent use under real conditions. If the evaluation board is
 * nevertheless used under real conditions, this is done at one’s responsibility;
 * any liability of Rutronik is insofar excluded
 */

#include "crc.h"

uint8_t crc_compute(uint8_t* buffer, uint32_t length)
{
	uint8_t crc = 0x15;
	uint32_t i = 0;

	for (i = 0; i < length; ++i)
	{
		uint8_t j = 0;
		uint8_t tmp = (uint8_t)(buffer[i] ^ crc);
		for (j = 0; j < 8; ++j)
		{
			uint8_t lsb = tmp & 0x01;
			tmp >>= 1;
			if (lsb != 0) tmp ^= 0x8C; // Polynome 0x8C
		}
		crc = tmp;
	}

	return crc;
}
