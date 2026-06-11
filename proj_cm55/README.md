# KIT PSE84 AI: OV7675 and BGT60TR13C data streaming over USB

The CM55 project initializes the OV7675 and the radar sensor BGT60TR13C. It then collects the data and send them through USB using the USBD class.

Protocol looks like:


| Header | Data |
| :---:|:---:|

The header is an array of 8 bytes:
| 0 | 1 | 2 | 3 | 4 | 5 | 6| 7 |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| 0x55 | 0x55 | counter | data size |||| crc |

For the documentation related to the example, click  [here](../README.md).