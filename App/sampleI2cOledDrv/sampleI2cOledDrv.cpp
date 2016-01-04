/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <math.h>

// for I2C
#include <linux/i2c-dev.h>
#include <fcntl.h>
#include <sys/ioctl.h>

#include "../../Lib/drivers/I2cOledDrv/CI2cOledDrv.h"

#define I2C_PORT	("/dev/i2c-1")

int main(int argc, char* argv[])
{
	int iRet = 0;
	
	CI2cOledDrv* pOled = NULL;

	try
	{
		// I2CポートをRead/Write属性でオープン。
		int fd = open(I2C_PORT, O_RDWR);
		if ( fd < 0 ){
			printf("failed to open i2c port\n");
			throw 0;
		}
		
		// ready Oled
		pOled = CI2cOledDrv::createInstance(fd);
		if(!pOled){
			printf("failed to CI2cOledDrv::createInstance(fd=%d,address=%d)¥n");
			throw 0;
		}
		
		if( pOled->useDevice() != 0 ){
			throw 0;
		}
		
		pOled->writeString("TEST");
		
		sleep(3);
		
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
