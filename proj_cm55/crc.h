/*
 * crc.h
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


#ifndef CRC_H_
#define CRC_H_

#include <stdint.h>

/**
 * @brief Compute CRC of the buffer
 *
 * @param [in] buffer Address of the buffer
 * @param [in] length Length of the buffer
 *
 * @retval CRC value
 */
uint8_t crc_compute(uint8_t* buffer, uint32_t length);

#endif /* CRC_H_ */
