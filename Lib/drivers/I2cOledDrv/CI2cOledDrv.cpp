/*
 * CI2cOledDrv.cpp
 *
 *  Created on: 2013/06/23
 *      Author: isao
 */

#include "CI2cOledDrv.h"
#include <stdio.h>
#include <unistd.h>

#include <linux/i2c-dev.h>
#include <fcntl.h>
#include <sys/ioctl.h>

#include "font_8x8.h"

CI2cOledDrv::CI2cOledDrv() {
	// TODO 自動生成されたコンストラクター・スタブ
	initInstance();

}

CI2cOledDrv::~CI2cOledDrv() {
	// TODO Auto-generated destructor stub
	this->destroyInstance();
}

CI2cOledDrv* CI2cOledDrv::createInstance(const int& fd, const int& address/*=OLDE_ADDRESS*/)
{
	CI2cOledDrv* pObj = NULL;

	try
	{
		if( fd==0 || address==0x00)
		{
			printf("Error: fd or Address is NULL \n");
			throw 0;
		}

		pObj = new CI2cOledDrv;
		if(pObj==NULL)
		{
			printf("Error: failed to create instance of CI2cOledDrv \n");
			throw 0;
		}

		pObj->m_fd = fd;
		pObj->m_address = address;

		if( pObj->startInstance()!=0 )
		{
			throw 0;
		}

	}
	catch(...)
	{
		printf("Catch: in CI2cOledDrv::createInstance() \n");
		if(pObj)
		{
			delete pObj;
			pObj = NULL;
		}
	}

	return pObj;

}


void CI2cOledDrv::initInstance()
{
	this->destroyInstance();
	m_fd = 0;
	m_address = 0;
}

int CI2cOledDrv::startInstance()
{
	int iRet = -1;

	try
	{
		//デバイスの使用準備
		if(this->useDevice()!=0)
		{
			throw 0;
		}

		// ディスプレイの使用準備
		this->initDisplay();
		this->clearDisplay();
		this->startDisplay();

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		printf("Catch: in CI2cOledDrv::startInstance() \n");
		iRet = -1;
	}

	return iRet;
}

void CI2cOledDrv::destroyInstance()
{
}

void CI2cOledDrv::initDisplay()
{
	writeData(OLED_RS_CMD, CMD_DISPLAY_OFF);
	usleep(5*1000);

	writeData(OLED_RS_CMD,CMD_SET_SEGMENT_REMAP);
	writeDataArg2(OLED_RS_CMD,CMD_SET_COM_PINS_CONFIG,MODE_COM_PINS_RESET);

	writeData(OLED_RS_CMD,CMD_REMAPPED_MODE);
	writeDataArg2(OLED_RS_CMD,CMD_SET_MULTIPLEX_RATIO,MODE_MULTIPLEX_RATIO_RESET);

	writeDataArg2(OLED_RS_CMD,CMD_SET_DISPLAY_CLK_DIV,MODE_DISPLAY_CLK_DIV_RESET);

	writeDataArg2(OLED_RS_CMD,CMD_SET_CONTRAST_CONTROL,0x50);

	writeDataArg2(OLED_RS_CMD,CMD_SET_PRE_CHARGE_PERIOD,0x21);

	writeDataArg2(OLED_RS_CMD,CMD_SET_MEMORY_ADDR_MODE,ADDR_MODE_PAGE);

	writeDataArg2(OLED_RS_CMD,CMD_SET_V_DESELECT_LV,0x30);

	writeDataArg2(OLED_RS_CMD,CMD_EXT_OR_INT_SELECTION,0x00);

	writeData(OLED_RS_CMD,CMD_ENTIRE_DISPLAY_OFF);
	writeData(OLED_RS_CMD,CMD_SET_NORMAL_DISPLAY);

	writeData(OLED_RS_CMD, CMD_DISPLAY_ON);
	usleep(5*1000);
}

void CI2cOledDrv::startDisplay()
{
	setMemoryAdressingMode(ADDR_MODE_HORIZONTAL);
	setPageAddress(0x00,0x07);
	setColumnAddress(0x00,0x7F);
}

void CI2cOledDrv::clearDisplay()
{
	int i=0;

	//this->ctlIO();

	setPageAddress(0x00,0x07);
	setColumnAddress(0x00,0x7F);

	//for(i=0;i<OLDE_COLS*OLDE_PAGES;i++)
	//{
	//	writeDataChar(' ',m_fd);
	//}
	char buf[OLDE_COLS*8*OLDE_PAGES+1];
	buf[0]=OLED_RS_DATA;
	for(i=1;i<OLDE_COLS*8*OLDE_PAGES+1;i++)
	{
		buf[i]=0x0;
	}
	if( write(m_fd,buf,OLDE_COLS*8*OLDE_PAGES+1) != (OLDE_COLS*8*OLDE_PAGES+1) )
	{
		printf("Error writeing to i2c slave1\n");
	}
}

void CI2cOledDrv::writeChar(const char& chr)
{
	int i=0;
	char buf[9];
	const int char_index = static_cast<int>(chr - 0x20);

	buf[0]=OLED_RS_DATA;

	for(i=0; i<FONT8x8_WIDTH; i++)
	{
		buf[i+1]=font_8x8[char_index][i];
	}

	if(write(m_fd,buf,9) != 9)
	{
		printf("Error writeing to i2c slave1\n");
	}
	return;
}

void CI2cOledDrv::writeString(const char* str)
{
	int i;
    for(i = 0; i < OLDE_COLS*OLDE_PAGES; i++)
	{
        if (str[i] == 0x00)
		{
			break;
		}
		else if(str[i] == 0x0c)
		{
			printf("FF\n");
		}
		else
		{
			writeChar(str[i]);
		}
	}
}

void CI2cOledDrv::writeDataArg2(const char& rs, const char& cmd, const char& arg)
{
	char buf[3];
	//this->ctlIO();
	buf[0]=OLED_RS_CMD;
	buf[1]=cmd;
	buf[2]=arg;
	if(write(m_fd,buf,3) != 3)
	{
		printf("Error writeing to i2c slave1\n");
	}
	return;
}

void CI2cOledDrv::setCOMPinsHardConfig( const char& mode)
{
	char buf[3];
	//this->ctlIO();
	buf[0]=OLED_RS_CMD;
	buf[1]=0xda;
	buf[2]=mode;
	if(write(m_fd,buf,3) != 3)
	{
		printf("Error writeing to i2c slave1\n");
	}
	return;
}

void CI2cOledDrv::setPageAddress(const char& startPage, const char& endPage)
{
	char buf[4];
	//this->ctlIO();
	buf[0]=OLED_RS_CMD;
	buf[1]=CMD_SET_PAGE_ADDRESS;
	buf[2]=startPage;
	buf[3]=endPage;
	if(write(m_fd,buf,4) != 4)
	{
		printf("Error writeing to i2c slave1\n");
	}
	return;
}

void CI2cOledDrv::setColumnAddress(const char& startCol, const char& endCol)
{
	char buf[4];

	//this->ctlIO();

	buf[0]=OLED_RS_CMD;
	buf[1]=CMD_SET_COLUMN_ADDRESS;
	buf[2]=startCol;
	buf[3]=endCol;
	if(write(m_fd,buf,4) != 4)
	{
		printf("Error writeing to i2c slave1\n");
	}
	return;
}

void CI2cOledDrv::setMemoryAdressingMode(const char& mode)
{
	char buf[3];

	//this->ctlIO();

	buf[0]=OLED_RS_CMD;
	buf[1]=CMD_SET_MEMORY_ADDR_MODE;//SetMemoryAddressingMode
	buf[2]=mode;
	if(write(m_fd,buf,3) != 3)
	{
		printf("Error setMemoryAdressingMode\n");
	}
	return;
}

void CI2cOledDrv::writeData(const char& rs, const char& data)
{
    char buf[2];

    //this->ctlIO();

    if (rs == OLED_RS_CMD || rs == OLED_RS_DATA)
	{
        // OLED_RS_CMD ならコマンドモード。OLED_RS_DATA ならデータモード。

        buf[0] = rs;
        buf[1] = data;
        if (write(m_fd, buf, 2) != 2){
            printf("Error writeing to i2c slave1\n");
        }
    }
    else{
        // rsの指定がOLED_RS_CMD,OLED_RS_DATA以外ならなにもしない。
    }
}

void CI2cOledDrv::setCursor(const char& col, const char& row)
{
	writeData(OLED_RS_CMD, 0xB0 + row);
	writeData(OLED_RS_CMD, 0x00 + (8*col & 0x0F));
	writeData(OLED_RS_CMD, 0x10 + ((8*col>>4)&0x0F));
}

void CI2cOledDrv::displayON()
{
	writeData(OLED_RS_CMD, CMD_DISPLAY_ON);
}

void CI2cOledDrv::displayOFF()
{
	writeData(OLED_RS_CMD, CMD_DISPLAY_OFF);
}

int CI2cOledDrv::useDevice()
{
	int iRet = -1;
	try
	{
		// 通信先アドレスの設定。
		if (ioctl(m_fd, I2C_SLAVE, m_address) < 0)
		{
			printf("Error: Unable to get bus access to talk to slave\n");
			throw 0;
		}

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}
void CI2cOledDrv::writeTetrisBlock()
{
	char buf[9];

	//1block
	buf[0]=OLED_RS_DATA;
	buf[1]=0xFF;
	buf[2]=0x83;
	buf[3]=0x85;
	buf[4]=0x89;
	buf[5]=0x91;
	buf[6]=0xA1;
	buf[7]=0xC1;
	buf[8]=0xFF;

	if(write(m_fd,buf,9) != 9)
	{
		printf("Error writeing to i2c slave1\n");
	}
	return;
}

void CI2cOledDrv::writeTetrisWall()
{
	char buf[9];

	//1block
	buf[0]=OLED_RS_DATA;
	buf[1]=0x00;
	buf[2]=0x7E;
	buf[3]=0x7E;
	buf[4]=0x7E;
	buf[5]=0x7E;
	buf[6]=0x7E;
	buf[7]=0x7E;
	buf[8]=0x00;

	if(write(m_fd,buf,9) != 9)
	{
		printf("Error writeing to i2c slave1\n");
	}
	return;
}
