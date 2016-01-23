/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <math.h>
#include <iostream>

// for I2C
#include <linux/i2c-dev.h>
#include <fcntl.h>
#include <sys/ioctl.h>

#include "../../Lib/drivers/I2cOledDrv/CI2cOledDrv.h"

#define I2C_PORT	("/dev/i2c-1")

int main(int argc, char* argv[])
{
	int iRet = 0;

	int fd = -1;
	CI2cOledDrv* pOled = NULL;

	try
	{
		// I2CポートをRead/Write属性でオープン。
		fd = open(I2C_PORT, O_RDWR);
		if ( fd < 0 ){
			printf("failed to open i2c port\n");
			throw 0;
		}
		
		// ready Oled
		pOled = CI2cOledDrv::createInstance(fd);
		if(!pOled){
			printf("failed to CI2cOledDrv::createInstance(fd=%d)¥n",fd);
			throw 0;
		}
		
		if( pOled->useDevice() != 0 ){
			throw 0;
		}
		pOled->writeString("Stanby OK!!");
		sleep(1);

		pOled->clearDisplay();
		pOled->writeString("Input '#' to Exit.");
		printf("Input '#' to Exit.\n");
		
		while(1){
			printf("loop_cnt=%d\n",loop_cnt);
			std::string sendBuf = "";
			std::cin >> sendBuf;
			
			pOled->clearDisplay();
			if(sendBuf == "#"){
				pOled->clearDisplay();
				break;
			}
			pOled->writeString(sendBuf.c_str());
			sleep(0);
		}
		
		pOled->writeString("Bye-bye!!");
		sleep(1);
		pOled->displayOFF();
		
		if(pOled){
			delete pOled;
			pOled = NULL;
		}
		
		if(fd >= 0){
			close(fd);
		}
	}
	catch(...)
	{
		printf("catch!! \n");
		iRet = -1;
		
		if(pOled){
			delete pOled;
			pOled = NULL;
		}
		
		if(fd >= 0){
			close(fd);
		}
	}

	return iRet;
}
