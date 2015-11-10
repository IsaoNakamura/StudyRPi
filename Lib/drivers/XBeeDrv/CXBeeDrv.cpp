/*
 * CXBeeDrv.cpp
 *
 *  Created on: 2014/05/21
 *      Author: isao
 */

#include "CXBeeDrv.h"
#include "../SerialDrv/CSerialDrv.h"

#include <stdio.h>
#include <unistd.h>

#include <linux/i2c-dev.h>
#include <fcntl.h>
#include <sys/ioctl.h>

#include <stdlib.h>

#include <termios.h>
#include <strings.h>

CXBeeDrv::CXBeeDrv() {
	// TODO 自動生成されたコンストラクター・スタブ
	init();

}

CXBeeDrv::~CXBeeDrv() {
	// TODO Auto-generated destructor stub
	destroy();
}

CXBeeDrv* CXBeeDrv::createInstance(	const char*	serialPort,
									const int&	baudrate)
{
	CXBeeDrv* pObj = NULL;
	try
	{
		pObj = new CXBeeDrv();
		if(!pObj)
		{
			throw 0;
		}

		if(pObj->startInstance(serialPort, baudrate)!=0)
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
	m_pSerial = NULL;
	return;
}

void CXBeeDrv::destroy()
{
	if(m_pSerial){
		delete m_pSerial;
		m_pSerial = NULL;
	}
	return;
}


int CXBeeDrv::startInstance(	const char*	serialPort,
								const int&	baudrate	)
{
	int iRet = -1;
	try
	{
		m_pSerial = CSerialDrv::createInstance(serialPort, baudrate);
		if(!m_pSerial){
			throw 0;
		}

		iRet = 0;
	}
	catch(...)
	{
		if(m_pSerial)
		{
			delete m_pSerial;
			m_pSerial = NULL;
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
		if(!m_pSerial){
			throw 0;
		}

		if( m_pSerial->receiveData(receiveBuf, bufNum) != 0 ){
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
		if(!m_pSerial){
			throw 0;
		}

		if( m_pSerial->sendData(sendBuf, bufNum) != 0 ){
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
		sendBuf[0]	= 0x7E;	// 開始デリミタ
		sendBuf[1]	= 0x00;	// MSB1
		sendBuf[2]	= 0x22;	// LSB2
		sendBuf[3]	= 0x10;	// FrameType :TX要求(ZigBee送信要求)
		sendBuf[4]	= 0x01;	// FrameID
		sendBuf[5]	= 0x00;	// 64bit Address start
		sendBuf[6]	= 0x13;	//  top
		sendBuf[7]	= 0xA2;
		sendBuf[8]	= 0x00;
		sendBuf[9]	= 0x40;	//  bottom
		sendBuf[10] = 0xB7;
		sendBuf[11] = 0x70;
		sendBuf[12] = 0x80;	// 64bit Address end
		sendBuf[13] = 0xFF; // 16bit Address start
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
		if(m_pSerial->sendDat(sendBuf, 26) != 0){
			printf("@CXBeeDrv::mainLoop() Error write to serial\n");
		}

		unsigned int loopCnt = 0;
		while(1)
		{
			printf("@CXBeeDrv::mainLoop() Start read from serial\n");
			unsigned char readBuf[1] = {0};
			if(m_pSerial->receiveData(readBuf, 1) != 0){
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
