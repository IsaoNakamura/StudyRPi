#include <Wire.h>
#include <SeeedOLED.h>
#include <avr/pgmspace.h>
#include <Servo.h>

static const unsigned char TamiyaLogo_pause[] PROGMEM ={
0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0x7f, 0x3f, 0x3f, 0x3f,
 0x3f, 0x3f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x7f, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xf9,
 0xf1, 0xe1, 0xc1, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
 0x01, 0x01, 0x01, 0xff, 0x7f, 0x3f, 0x3f, 0x3f, 0x1f, 0x1f, 0x1f, 0x0f, 0xcf, 0xcf, 0x07, 0xff,
 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x3f,
 0x3f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x3f, 0x3f, 0x3f, 0x1f, 0x1f, 0x0f, 0x0f, 0x0f, 0x07,
 0x07, 0x03, 0x03, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x03,
 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x07, 0x07, 0x07, 0x07,
 0x07, 0x07, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x80, 0xc0, 0xc0, 0xc0, 0xe0, 0xe0, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfc, 0xf8, 0xf8, 0xfc, 0xfc, 0xfe, 0xfe, 0xfe, 0x3e, 0x1e, 0x07,
 0x03, 0x01, 0x00, 0xff, 0x8c, 0x9c, 0x98, 0x39, 0x31, 0x73, 0xf0, 0xf0, 0xe7, 0xe7, 0x43, 0xff,
 0xff, 0xe0, 0xc0, 0x04, 0x0e, 0x04, 0x00, 0x00, 0x81, 0x81, 0x81, 0x80, 0x00, 0x00, 0x1e, 0x1f,
 0x1f, 0x1f, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc0, 0xc0, 0xe0, 0xe0, 0xe0, 0xe0,
 0xe0, 0xe0, 0xff, 0x00, 0x00, 0x02, 0x07, 0x07, 0x0f, 0x0f, 0x0f, 0x1f, 0x1f, 0x3f, 0x3f, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0xff, 0xff, 0xff, 0xfc, 0xf0, 0xe0, 0xc0,
 0x00, 0x00, 0x00, 0xff, 0x47, 0xe7, 0xe3, 0xf0, 0xf8, 0xf8, 0xfc, 0xfc, 0xfc, 0xfc, 0x78, 0xff,
 0xff, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x06, 0x04, 0x04, 0x07, 0x20, 0x38, 0x3c, 0x7e,
 0xfe, 0x01, 0x00, 0x02, 0x0e, 0xfc, 0xf0, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7f,
 0x3f, 0x1f, 0x0f, 0x07, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01,
 0x01, 0x03, 0x00, 0xff, 0x30, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x30, 0xff,
 0xff, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x40, 0x60, 0x60, 0xc0, 0x04, 0x18, 0x78, 0x7c,
 0xff, 0x00, 0x00, 0xc0, 0x70, 0x3f, 0x1f, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xfd,
 0xf9, 0xf1, 0xe1, 0x81, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
 0x01, 0x81, 0x01, 0xff, 0x38, 0x38, 0x38, 0x1a, 0x1a, 0x92, 0x96, 0xc6, 0xc6, 0xc6, 0x64, 0xff,
 0xff, 0x07, 0x02, 0x20, 0x70, 0x20, 0x00, 0x00, 0x81, 0x83, 0x03, 0x01, 0x00, 0x00, 0xf0, 0xf8,
 0xf8, 0xf8, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x07, 0x07, 0x07, 0x07, 0x07,
 0x07, 0x0f, 0xff, 0x00, 0x80, 0xc0, 0xc0, 0xe0, 0xe0, 0xe0, 0xe0, 0xf0, 0xf0, 0xf0, 0xf8, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xfc, 0xfc, 0xfc, 0xfc, 0xfe, 0xfe, 0x7e, 0x7f, 0x1f, 0x0f, 0x03,
 0x01, 0x00, 0x00, 0xff, 0x7c, 0x7c, 0x7c, 0x7c, 0x7c, 0x3c, 0x1c, 0x1c, 0x9c, 0x9c, 0x08, 0xff,
 0xff, 0xff, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfc, 0xfd,
 0xfd, 0xfd, 0xfc, 0xfe, 0xfe, 0xfe, 0xfc, 0xfc, 0xfc, 0xf8, 0xf8, 0xf0, 0xf0, 0xe0, 0xe0, 0xc0,
 0xc0, 0xc0, 0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc0, 0xc0,
 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0,
 0xe0, 0xe0, 0xff, 0x00, 0x00, 0x01, 0x01, 0x03, 0x03, 0x03, 0x03, 0x07, 0x07, 0x07, 0x0f, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0xfe, 0xf8, 0xf0, 0xc0,
 0x80, 0x00, 0x00, 0xff, 0x18, 0x98, 0x98, 0xd2, 0xf2, 0xf3, 0xf0, 0xe0, 0xef, 0xcf, 0x82, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe,
 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0xff,
 0xbf, 0x9f, 0x8f, 0x83, 0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x81, 0x81, 0x83,
 0x83, 0x86, 0x80, 0xff, 0x80, 0x99, 0xb9, 0xb9, 0xf9, 0xf9, 0xf9, 0xf9, 0xf9, 0xf9, 0xe0, 0xff
};

static const unsigned char TamiyaLogo_ff[] PROGMEM ={
0xff, 0x1f, 0x1f, 0x0f, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x0f, 0x0f, 0x1f, 0x1f, 0x1f, 0x1f,
 0x3f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xf9,
 0xf1, 0xe1, 0xc1, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
 0x01, 0x01, 0x01, 0xff, 0x7f, 0x3f, 0x3f, 0x3f, 0x1f, 0x1f, 0x1f, 0x0f, 0xcf, 0xcf, 0x07, 0xff,
 0xff, 0xf0, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x01, 0x01, 0x01, 0x03, 0x07, 0x0f, 0x0f, 0x1f, 0x3f, 0x3f, 0x7f, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f,
 0x7f, 0x7f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f,
 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f,
 0x7f, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x80, 0xc0, 0xc0, 0xc0, 0xe0, 0xe0, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfc, 0xf8, 0xf8, 0xfc, 0xfc, 0xfe, 0xfe, 0xfe, 0x3e, 0x1e, 0x07,
 0x03, 0x01, 0x00, 0xff, 0x8c, 0x9c, 0x98, 0x39, 0x31, 0x73, 0xf0, 0xf0, 0xe7, 0xe7, 0x43, 0xff,
 0xff, 0xff, 0x1f, 0xcf, 0xcf, 0x4c, 0x08, 0x08, 0x18, 0x38, 0x10, 0x10, 0x00, 0xc0, 0xc0, 0xc0,
 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x03,
 0x03, 0x03, 0x03, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0xff, 0xff, 0x00, 0x00, 0x02, 0x07, 0x07, 0x0f, 0x0f, 0x0f, 0x1f, 0x1f, 0x3f, 0x3f, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0xff, 0xff, 0xff, 0xfc, 0xf0, 0xe0, 0xc0,
 0x00, 0x00, 0x00, 0xff, 0x47, 0xe7, 0xe3, 0xf0, 0xf8, 0xf8, 0xfc, 0xfc, 0xfc, 0xfc, 0x78, 0xff,
 0xff, 0xff, 0x7e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0xd0, 0x98, 0xf0, 0x60, 0x00, 0x81, 0xc1,
 0xe1, 0x01, 0x20, 0xc0, 0xc0, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfc, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe,
 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7f,
 0x3f, 0x1f, 0x0f, 0x07, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01,
 0x01, 0x03, 0x00, 0xff, 0x30, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x30, 0xff,
 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x83,
 0xff, 0x00, 0x00, 0x01, 0x01, 0xff, 0xff, 0xfe, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xfd,
 0xf9, 0xf1, 0xe1, 0x81, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
 0x01, 0x81, 0x01, 0xff, 0x38, 0x38, 0x38, 0x1a, 0x1a, 0x92, 0x96, 0xc6, 0xc6, 0xc6, 0x64, 0xff,
 0xff, 0xff, 0x7c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x14, 0x32, 0x1e, 0x08, 0x00, 0x83, 0xc7,
 0xc7, 0x00, 0x00, 0x06, 0x06, 0x03, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3f, 0x3f, 0x3f, 0x3f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f,
 0xff, 0xff, 0xff, 0x00, 0x80, 0xc0, 0xc0, 0xe0, 0xe0, 0xe0, 0xe0, 0xf0, 0xf0, 0xf0, 0xf8, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xfc, 0xfc, 0xfc, 0xfc, 0xfe, 0xfe, 0x7e, 0x7f, 0x1f, 0x0f, 0x03,
 0x01, 0x00, 0x00, 0xff, 0x7c, 0x7c, 0x7c, 0x7c, 0x7c, 0x3c, 0x1c, 0x1c, 0x9c, 0x9c, 0x08, 0xff,
 0xff, 0xff, 0xf0, 0xe4, 0xee, 0xe4, 0xe0, 0xe0, 0xf0, 0xf8, 0xf0, 0xf0, 0xe0, 0xee, 0xdf, 0xdf,
 0xdf, 0xee, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xc0, 0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x01, 0xff, 0xff, 0x00, 0x00, 0x01, 0x01, 0x03, 0x03, 0x03, 0x03, 0x07, 0x07, 0x07, 0x0f, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0xfe, 0xf8, 0xf0, 0xc0,
 0x80, 0x00, 0x00, 0xff, 0x18, 0x98, 0x98, 0xd2, 0xf2, 0xf3, 0xf0, 0xe0, 0xef, 0xcf, 0x82, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfe, 0xf8,
 0xf8, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xe0, 0xe0, 0xe0, 0xe0, 0xc0, 0xc0, 0xc0, 0xc0,
 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0xc0, 0xf8, 0xf8, 0xf8, 0xf8, 0xf8,
 0xf8, 0xf8, 0xf8, 0xf8, 0xf8, 0xf8, 0xf8, 0xf8, 0xf8, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe,
 0xfe, 0xff, 0xff, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0xff,
 0xbf, 0x9f, 0x8f, 0x83, 0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x81, 0x81, 0x83,
 0x83, 0x86, 0x80, 0xff, 0x80, 0x99, 0xb9, 0xb9, 0xf9, 0xf9, 0xf9, 0xf9, 0xf9, 0xf9, 0xe0, 0xff
};

static const unsigned char TamiyaLogo_rewind[] PROGMEM ={
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x1f,
 0x1f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x07, 0x07, 0x07, 0x07, 0x03, 0x03, 0x03, 0x03,
 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x03, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f,
 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f,
 0x7f, 0xff, 0xff, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xf9,
 0xf1, 0xe1, 0xc1, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
 0x01, 0x01, 0x01, 0xff, 0x7f, 0x3f, 0x3f, 0x3f, 0x1f, 0x1f, 0x1f, 0x0f, 0xcf, 0xcf, 0x07, 0xff,
 0xff, 0xff, 0x0f, 0x27, 0x77, 0x27, 0x07, 0x07, 0x0f, 0x1f, 0x0f, 0x0f, 0x07, 0x77, 0xfb, 0xfb,
 0xfb, 0x77, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x03, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x80, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x80, 0xc0, 0xc0, 0xc0, 0xe0, 0xe0, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfc, 0xf8, 0xf8, 0xfc, 0xfc, 0xfe, 0xfe, 0xfe, 0x3e, 0x1e, 0x07,
 0x03, 0x01, 0x00, 0xff, 0x8c, 0x9c, 0x98, 0x39, 0x31, 0x73, 0xf0, 0xf0, 0xe7, 0xe7, 0x43, 0xff,
 0xff, 0xff, 0x3e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x28, 0x4c, 0x78, 0x10, 0x00, 0xc1, 0xe3,
 0xe3, 0x00, 0x00, 0x60, 0x60, 0xc0, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfc, 0xfc, 0xfc, 0xfc, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe,
 0xff, 0xff, 0xff, 0x00, 0x00, 0x02, 0x07, 0x07, 0x0f, 0x0f, 0x0f, 0x1f, 0x1f, 0x3f, 0x3f, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0xff, 0xff, 0xff, 0xfc, 0xf0, 0xe0, 0xc0,
 0x00, 0x00, 0x00, 0xff, 0x47, 0xe7, 0xe3, 0xf0, 0xf8, 0xf8, 0xfc, 0xfc, 0xfc, 0xfc, 0x78, 0xff,
 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x80, 0xc1,
 0xff, 0x00, 0x00, 0x80, 0x80, 0xff, 0xff, 0x7f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7f,
 0x3f, 0x1f, 0x0f, 0x07, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01,
 0x01, 0x03, 0x00, 0xff, 0x30, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x30, 0xff,
 0xff, 0xff, 0x7e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x0b, 0x19, 0x0f, 0x06, 0x00, 0x81, 0x83,
 0x87, 0x80, 0x04, 0x03, 0x03, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f,
 0xff, 0xff, 0xff, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xfd,
 0xf9, 0xf1, 0xe1, 0x81, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
 0x01, 0x81, 0x01, 0xff, 0x38, 0x38, 0x38, 0x1a, 0x1a, 0x92, 0x96, 0xc6, 0xc6, 0xc6, 0x64, 0xff,
 0xff, 0xff, 0xf8, 0xf3, 0xf3, 0x32, 0x10, 0x10, 0x18, 0x1c, 0x08, 0x08, 0x00, 0x03, 0x03, 0x03,
 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc0, 0xc0,
 0xc0, 0xc0, 0xc0, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0xff, 0xff, 0x00, 0x80, 0xc0, 0xc0, 0xe0, 0xe0, 0xe0, 0xe0, 0xf0, 0xf0, 0xf0, 0xf8, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xfc, 0xfc, 0xfc, 0xfc, 0xfe, 0xfe, 0x7e, 0x7f, 0x1f, 0x0f, 0x03,
 0x01, 0x00, 0x00, 0xff, 0x7c, 0x7c, 0x7c, 0x7c, 0x7c, 0x3c, 0x1c, 0x1c, 0x9c, 0x9c, 0x08, 0xff,
 0xff, 0x0f, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x80, 0x80, 0x80, 0xc0, 0xe0, 0xf0, 0xf0, 0xf8, 0xfc, 0xfc, 0xfe, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe,
 0xfe, 0xfe, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc,
 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe,
 0xfe, 0xff, 0xff, 0x00, 0x00, 0x01, 0x01, 0x03, 0x03, 0x03, 0x03, 0x07, 0x07, 0x07, 0x0f, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0xfe, 0xf8, 0xf0, 0xc0,
 0x80, 0x00, 0x00, 0xff, 0x18, 0x98, 0x98, 0xd2, 0xf2, 0xf3, 0xf0, 0xe0, 0xef, 0xcf, 0x82, 0xff,
 0xff, 0xf8, 0xf8, 0xf0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xf0, 0xf0, 0xf8, 0xf8, 0xf8, 0xf8,
 0xfc, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0xff,
 0xbf, 0x9f, 0x8f, 0x83, 0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x81, 0x81, 0x83,
 0x83, 0x86, 0x80, 0xff, 0x80, 0x99, 0xb9, 0xb9, 0xf9, 0xf9, 0xf9, 0xf9, 0xf9, 0xf9, 0xe0, 0xff
};

static const unsigned char TamiyaLogo_stop[] PROGMEM ={
 0xff, 0xff, 0xff, 0xff, 0x03, 0x03, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x03,
 0x03, 0x03, 0x03, 0x07, 0x1f, 0x3f, 0x3f, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xf9,
 0xf1, 0xe1, 0xc1, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
 0x01, 0x01, 0x01, 0xff, 0x7f, 0x3f, 0x3f, 0x3f, 0x1f, 0x1f, 0x1f, 0x0f, 0xcf, 0xcf, 0x07, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xfe, 0xf8, 0xe0, 0xe0, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x03, 0x07, 0x07, 0x0f, 0x1f,
 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f,
 0x0f, 0x0f, 0x0f, 0x0f, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f,
 0x0f, 0x0f, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x80, 0xc0, 0xc0, 0xc0, 0xe0, 0xe0, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfc, 0xf8, 0xf8, 0xfc, 0xfc, 0xfe, 0xfe, 0xfe, 0x3e, 0x1e, 0x07,
 0x03, 0x01, 0x00, 0xff, 0x8c, 0x9c, 0x98, 0x39, 0x31, 0x73, 0xf0, 0xf0, 0xe7, 0xe7, 0x43, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xe1, 0x0d, 0x0d, 0x05, 0x01, 0x01, 0x01, 0x03, 0x80, 0x00, 0x00,
 0x0c, 0x1c, 0x1c, 0x18, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0,
 0xe0, 0xf0, 0xff, 0x00, 0x00, 0x02, 0x07, 0x07, 0x0f, 0x0f, 0x0f, 0x1f, 0x1f, 0x3f, 0x3f, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0xff, 0xff, 0xff, 0xfc, 0xf0, 0xe0, 0xc0,
 0x00, 0x00, 0x00, 0xff, 0x47, 0xe7, 0xe3, 0xf0, 0xf8, 0xf8, 0xfc, 0xfc, 0xfc, 0xfc, 0x78, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x0d, 0x09, 0x0f, 0x06,
 0x10, 0x18, 0x3c, 0xfe, 0x00, 0x02, 0x1c, 0xf8, 0xf0, 0xe0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7f,
 0x3f, 0x1f, 0x0f, 0x07, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01,
 0x01, 0x03, 0x00, 0xff, 0x30, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x30, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xe0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0xb0, 0x90, 0xf0, 0x60,
 0x08, 0x18, 0x3c, 0x7f, 0x00, 0x40, 0x38, 0x1f, 0x0f, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xfd,
 0xf9, 0xf1, 0xe1, 0x81, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
 0x01, 0x81, 0x01, 0xff, 0x38, 0x38, 0x38, 0x1a, 0x1a, 0x92, 0x96, 0xc6, 0xc6, 0xc6, 0x64, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0x87, 0x30, 0x30, 0x20, 0x00, 0x00, 0x80, 0xc0, 0x81, 0x80, 0x00,
 0x30, 0x38, 0x38, 0x18, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
 0x07, 0x0f, 0xff, 0x00, 0x80, 0xc0, 0xc0, 0xe0, 0xe0, 0xe0, 0xe0, 0xf0, 0xf0, 0xf0, 0xf8, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xfc, 0xfc, 0xfc, 0xfc, 0xfe, 0xfe, 0x7e, 0x7f, 0x1f, 0x0f, 0x03,
 0x01, 0x00, 0x00, 0xff, 0x7c, 0x7c, 0x7c, 0x7c, 0x7c, 0x3c, 0x1c, 0x1c, 0x9c, 0x9c, 0x08, 0xff,
 0xff, 0xff, 0xff, 0xff, 0x7f, 0x1f, 0x0f, 0x0f, 0x03, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00,
 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x80, 0xc0, 0xe0, 0xe0, 0xf0, 0xf0,
 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xfc, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0,
 0xf0, 0xf0, 0xf0, 0xf0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0,
 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xe0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0, 0xf0,
 0xf0, 0xf0, 0xff, 0x00, 0x00, 0x01, 0x01, 0x03, 0x03, 0x03, 0x03, 0x07, 0x07, 0x07, 0x0f, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0xfe, 0xf8, 0xf0, 0xc0,
 0x80, 0x00, 0x00, 0xff, 0x18, 0x98, 0x98, 0xd2, 0xf2, 0xf3, 0xf0, 0xe0, 0xef, 0xcf, 0x82, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xc0, 0xc0, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x80, 0xc0,
 0xc0, 0xc0, 0xc0, 0xe0, 0xf8, 0xfc, 0xfc, 0xfe, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
 0xff, 0xff, 0xff, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0xff,
 0xbf, 0x9f, 0x8f, 0x83, 0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x81, 0x81, 0x83,
 0x83, 0x86, 0x80, 0xff, 0x80, 0x99, 0xb9, 0xb9, 0xf9, 0xf9, 0xf9, 0xf9, 0xf9, 0xf9, 0xe0, 0xff
};

#define PIN_FORWARD     8
#define PIN_BACKWARD    9

#define LOOP_MAX    5

#define DRIVE_MSEC    30000
#define PAUSE_MSEC    30000
#define HIDEN_MSEC     1000

Servo myservo;
#define PIN_SERVO   10
#define MAX_SERVO  100
#define MIN_SERVO   65
#define MOVE_DELTA   1
#define QUICK_SERVO  6
#define SLOW_SERVO  15

unsigned long g_splitTime = 0;
int g_motor_state = -1; // -1:stop 0:forward 1:pause_forward 2:backward 3:pause_backward
int g_loop_num = 0;

void setup() {
  Wire.begin();
  SeeedOled.init();  //initialze SEEED OLED display

  SeeedOled.clearDisplay();          //clear the screen and set start position to top left corner
  SeeedOled.setPageMode();           //Set addressing mode to Page Mode
  SeeedOled.setTextXY(0,0);          //Set the cursor to Xth Page, Yth Column  
  SeeedOled.drawBitmap((unsigned char*) TamiyaLogo_pause,1024);   // 1024 = 128 Pixels * 64 Pixels / 8

  pinMode(PIN_FORWARD, OUTPUT);
  pinMode(PIN_BACKWARD, OUTPUT);

  // STOP
  digitalWrite(PIN_FORWARD, LOW);
  digitalWrite(PIN_BACKWARD, LOW);

  myservo.attach( PIN_SERVO );
  // HATCH-CLOSE
  moveServoSmoothly(myservo, MAX_SERVO, MIN_SERVO, MOVE_DELTA, SLOW_SERVO);

  g_splitTime = millis();
}

void loop() {
  // get current time.
  unsigned long curTime = millis();
  unsigned long timeInterval = 0;
  if(curTime >= g_splitTime){
    timeInterval = curTime - g_splitTime;
  }

  // calc current motor_state
  int motor_state = g_motor_state;
  bool isChanged = calcCurrentMotorState(motor_state, g_loop_num, g_motor_state, curTime, timeInterval);
  if( isChanged ){
    g_splitTime = curTime;
  }

  // action by motor_state.
  actionMotor(motor_state, isChanged, timeInterval, g_loop_num);

  // update g_motor_state.
  if( isChanged ){
    g_motor_state = motor_state;
  }

  delay(0);
}

void actionMotor
(
  const int motor_state,
  const bool isChanged,
  const unsigned long timeInterval,
  const int loop_num
)
{
  if(motor_state == 0 ){
    // FORWARD
    if(isChanged){
      // DRAW LEFT-HAND
      SeeedOled.setTextXY(0,0);
      SeeedOled.drawBitmap((unsigned char*) TamiyaLogo_ff,1024);
            

      // MOTOR-FORWARD
      digitalWrite(PIN_FORWARD, HIGH);
      digitalWrite(PIN_BACKWARD, LOW);

      // HATCH-CLOSE
      moveServoSmoothly(myservo, MAX_SERVO, MIN_SERVO, MOVE_DELTA, SLOW_SERVO);


    }
    
    SeeedOled.setTextXY(0,0);
    SeeedOled.putString("FORWARD(");
    SeeedOled.putNumber(loop_num);
    SeeedOled.putString("):");
    SeeedOled.putNumber(timeInterval/1000);
  }else if(motor_state == 2){
    // BACKWARD
    if(isChanged){
      // DRAW RIGHT-HAND
      SeeedOled.setTextXY(0,0);
      SeeedOled.drawBitmap((unsigned char*) TamiyaLogo_rewind,1024);

      // MOTOR-BACKWARD
      digitalWrite(PIN_FORWARD,  LOW);
      digitalWrite(PIN_BACKWARD, HIGH);

      // HATCH-CLOSE
      moveServoSmoothly(myservo, MAX_SERVO, MIN_SERVO, MOVE_DELTA, SLOW_SERVO);
    }
    
    SeeedOled.setTextXY(0,0);
    SeeedOled.putString("BACKWARD(");
    SeeedOled.putNumber(loop_num);
    SeeedOled.putString("):");
    SeeedOled.putNumber(timeInterval/1000);
  }else if(motor_state == 1 || motor_state == 3){
    // PAUSE
    if(isChanged){
      // DRAW NO-HAND
      SeeedOled.setTextXY(0,0);
      SeeedOled.drawBitmap((unsigned char*) TamiyaLogo_pause,1024);

      // MOTOR-STOP
      digitalWrite(PIN_FORWARD, LOW);
      digitalWrite(PIN_BACKWARD, LOW);

      // HATCH-OPEN
      moveServoSmoothly(myservo, MIN_SERVO, MAX_SERVO, MOVE_DELTA, QUICK_SERVO);
    }
    SeeedOled.setTextXY(0,0);
    SeeedOled.putString("PAUSE(");
    SeeedOled.putNumber(loop_num);
    SeeedOled.putString("):");
    SeeedOled.putNumber(timeInterval/1000);
  }else{
    // STOP
    if(isChanged){
      // DRAW ALL-HAND
      SeeedOled.setTextXY(0,0);
      SeeedOled.drawBitmap((unsigned char*) TamiyaLogo_stop,1024);

      // MOTOR-STOP
      digitalWrite(PIN_FORWARD, LOW);
      digitalWrite(PIN_BACKWARD, LOW);

      // HATCH-OPEN
      moveServoSmoothly(myservo, MIN_SERVO, MAX_SERVO, MOVE_DELTA, QUICK_SERVO);
    }
    SeeedOled.setTextXY(0,0);
    SeeedOled.putString("STOP:");
    SeeedOled.putNumber(timeInterval/1000);
  }
  return;
}

bool calcCurrentMotorState
(
  int& motor_state,
  int& loop_num,
  const int prev_motor_state,
  const unsigned long curTime,
  const unsigned long timeInterval
) 
{
  bool bRet = false;

  // this frame motor_stae
  motor_state = prev_motor_state;
  
  // calc current motor_state
  if(prev_motor_state==-1){ // -1:stop
    if( (loop_num < LOOP_MAX) && (timeInterval > HIDEN_MSEC) ){
      bRet = true;

      // stop to forward
      motor_state = 0;
    }
  }else 
  if(prev_motor_state==0 || prev_motor_state==2){ // 0:forward 2:backward
    if( timeInterval > DRIVE_MSEC){
      bRet = true;

      if(prev_motor_state == 0 ){
        // forward to pause
        motor_state = 1;
      }else{
        // backward to pause
        motor_state = 3;
        loop_num++;
      }
    }
  }else if(prev_motor_state==1 || prev_motor_state==3){ // 1:pause_forward or 3:pause_backward
    if( timeInterval > PAUSE_MSEC){
      bRet = true;

      if(prev_motor_state == 1 ){
        // pause to backward
        motor_state = 2;
      }else{
        if( loop_num >= LOOP_MAX ){
          // pause to stop
          motor_state = -1;
          // loop_num = 0;
        }else{
          // pause to forward
          motor_state = 0;
        }
      }
    }
  }
  return bRet;
}

void moveServoSmoothly
(
  Servo& servo,
  const int beg_pos,
  const int end_pos,
  const int move_delta,
  const int delay_time
)
{
  int pos = beg_pos;
  servo.write(pos);
  delay(delay_time);

  if(beg_pos <= end_pos){
    while(1){
      pos+=move_delta;
      if(pos>end_pos){
        pos=end_pos;
      }
      servo.write(pos);
      delay(delay_time);
      if(pos>=end_pos){
        break;
      }
    }
  }else{
    while(1){
      pos-=move_delta;
      if(pos<end_pos){
        pos=end_pos;
      }
      servo.write(pos);
      delay(delay_time);
      if(pos<=end_pos){
        break;
      }
    }
  }
  return;
}
