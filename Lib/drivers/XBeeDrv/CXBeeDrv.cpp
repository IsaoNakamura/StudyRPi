/*
 * CXBeeDrv.cpp
 *
 *  Created on: 2014/05/21
 *      Author: isao
 */

#include "CXBeeDrv.h"
#include <stdio.h>
#include <unistd.h>

#include <linux/i2c-dev.h>
#include <fcntl.h>
#include <sys/ioctl.h>

#include <stdlib.h>

#include <termios.h>
#include <strings.h>

#define _USE_USB (1)
#if _USE_USB
#define SERIAL_PORT "/dev/ttyUSB0"
#else
#define SERIAL_PORT "/dev/ttyAMA0"
#endif //_USE_USB

#define BAUDRATE  B115200

CXBeeDrv::CXBeeDrv() {
	// TODO 自動生成されたコンストラクター・スタブ
	init();

}

CXBeeDrv::~CXBeeDrv() {
	// TODO Auto-generated destructor stub
	destroy();
}

CXBeeDrv* CXBeeDrv::createInstance()
{
	CXBeeDrv* pObj = NULL;
	try
	{
		pObj = new CXBeeDrv();
		if(!pObj)
		{
			throw 0;
		}

		if(pObj->startInstance()!=0)
		{
			throw 0;
		}
	}
	catch(...)
	{
		if(pObj)
		{
			delete pObj;
			pObj = NULL;
		}
	}

	return pObj;
}


void CXBeeDrv::init()
{
	m_fd = 0;
	m_address = 0;

	return;
}

void CXBeeDrv::destroy()
{
	if( m_fd > 0 )
	{
		close(m_fd);
	}
	return;
}

int CXBeeDrv::startInstance()
{
	printf("@CXBeeDrv::startInstance() start\n");
	int iRet = -1;
	try
	{
		m_fd = open(SERIAL_PORT, O_RDWR );
		if( m_fd < 0 )
		{
			printf("@CXBeeDrv::startInstance() failed to open SERIAL_PORT \n");
			throw 0;
		}

		struct termios oldtio;
		struct termios newtio;
		if( ioctl(m_fd, TCGETS, &oldtio) < 0 )
		{
			printf("@CXBeeDrv::startInstance() failed to ioctl(TCGETS) \n");
			throw 0;
		}
		bzero(&newtio, sizeof(newtio) );
		newtio = oldtio;
		newtio.c_cflag = ( BAUDRATE | CS8 | CLOCAL | CREAD );
		newtio.c_iflag = ( IGNPAR );
		newtio.c_oflag = 0;
		newtio.c_lflag = ICANON;
		if( ioctl(m_fd, TCSETS, &newtio) < 0 )
		{
			printf("@CXBeeDrv::startInstance() failed to ioctl(TCSETS) \n");
			throw 0;
		}

		iRet = 0;
	}
	catch(...)
	{
		if( m_fd > 0 )
		{
			close(m_fd);
		}
		iRet = -1;
	}
	return iRet;
}

int CXBeeDrv::receiveData(unsigned char* receiveBuf, int& bufNum)
{
	int iRet = -1;
	try
	{
		if(!receiveBuf){
			throw 0;
		}
		if( bufNum <= 0){
			throw 0;
		}
		
		if (read(m_fd, receiveBuf, bufNum) != bufNum){
			printf("@CXBeeDrv::receiveData() Error read from serial\n");
			throw 0;
		}
			
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CXBeeDrv::sendData(const unsigned char* sendBuf, const int& bufNum)
{
	int iRet = -1;
	try
	{
		if(!sendBuf){
			throw 0;
		}
		if( bufNum <= 0){
			throw 0;
		}
		
		if (write(m_fd, sendBuf, bufNum) != bufNum){
			printf("@CXBeeDrv::sendData() Error write to serial\n");
			throw 0;
		}
		
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CXBeeDrv::mainLoop()
{
	int iRet = -1;
	try
	{
		unsigned char sendBuf[26] = {0};
		sendBuf[0] = 0x7E;	// 開始デリミタ
		sendBuf[1] = 0x00;	// MSB1
		sendBuf[2] = 0x22;	// LSB2
		sendBuf[3] = 0x10;	// FrameType :TX要求(ZigBee送信要求)
		sendBuf[4] = 0x01;	// FrameID
		sendBuf[5] = 0x00;	// 64bit Address start
		sendBuf[6] = 0x13;	//  top
		sendBuf[7] = 0xA2;
		sendBuf[8] = 0x00;
		sendBuf[9] = 0x40;	//  bottom
		sendBuf[10] = 0xB7;
		sendBuf[11] = 0x70;
		sendBuf[12] = 0x80;	// 64bit Address end
		sendBuf[13] = 0xFF; 	// 16bit Address start
		sendBuf[14] = 0xFE;	// 16bit Address end
		sendBuf[15] = 0x00;	// Broadcast Range
		sendBuf[16] = 0x00;	// Option
		sendBuf[17] = 0x01;	// RF-DATA start
		sendBuf[18] = 0x02;
		sendBuf[19] = 0x03;
		sendBuf[20] = 0x04;
		sendBuf[21] = 0x05;
		sendBuf[22] = 0x06;
		sendBuf[23] = 0x07;
		sendBuf[24] = 0x08;	// RF-DATA end
		long sum = 0;
		for(unsigned int i=3; i<25; i++)
		{
			sum = sum + sendBuf[i];
		}
		sendBuf[25] = 0xFF - (sum & 0xFF);	// Check-Sum
		if (write(m_fd, sendBuf, 26) != 26){
			printf("@CXBeeDrv::mainLoop() Error write to serial\n");
		}

		unsigned int loopCnt = 0;
		while(1)
		{
			printf("@CXBeeDrv::mainLoop() Start read from serial\n");
			unsigned char readBuf[1] = {0};
			if (read(m_fd, readBuf, 1) != 1){
				printf("@CXBeeDrv::mainLoop() Error read from serial\n");
				continue;
			}
			printf("@CXBeeDrv::mainLoop() Success read from serial %d\n",readBuf[0]);

			if(readBuf[0] == 0x7E )
			{
				printf("@CXBeeDrv::mainLoop() Success read start-byte \n");
			}

			loopCnt++;
			usleep(10*1000);//10ms待つ

			if(loopCnt>100)
			{
				printf("@CXBeeDrv::mainLoop() send-loop breakl\n");
				break;
			}
		}

		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}

	return iRet;
}
