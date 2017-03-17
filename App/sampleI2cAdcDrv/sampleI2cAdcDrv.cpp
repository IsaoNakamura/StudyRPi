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

#include "../../Lib/drivers/I2cAdcDrv/CI2cAdcDrv.h"

#define I2C_PORT	("/dev/i2c-1")

int main(int argc, char* argv[])
{
	int iRet = 0;

	int fd = -1;
	CI2cAdcDrv* pAdc = NULL;

	try
	{
		// I2CポートをRead/Write属性でオープン。
		fd = open(I2C_PORT, O_RDWR);
		if ( fd < 0 ){
			printf("failed to open i2c port\n");
			throw 0;
		}
		
		// ready Adc
		pAdc = CI2cAdcDrv::createInstance(fd);
		if(!pAdc){
			printf("failed to CI2cAdcDrv::createInstance(fd=%d)¥n",fd);
			throw 0;
		}
		
		if( pAdc->useDevice() != 0 ){
			printf("failed to CI2cAdcDrv::useDevice()¥n");
			throw 0;
		}
		
		int loop_cnt = 0;
		while(loop_cnt<1000){
			// 全チャンネルの値を取得
			for(unsigned int i=0; i<CH_NUM; i++){
				unsigned short		ret_value=0;
				int					ret_state=0;
				if( pAdc->getChannelValue(ret_value, ret_state, i)==0 ){
					// 取得成功
					printf("ch[%02d]:val=%05d,stat=%02d ",i,ret_value, ret_state);
				}else{
					// 取得失敗
					printf("ch[%02d]:failed      ",i);
				}
			}
			printf("\n");
			loop_cnt++;

			sleep(0);
		}
		
		if(pAdc){
			delete pAdc;
			pAdc = NULL;
		}
		
		if(fd >= 0){
			close(fd);
		}

		// ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		printf("catch!! \n");
		iRet = -1;
		
		if(pAdc){
			delete pAdc;
			pAdc = NULL;
		}
		
		if(fd >= 0){
			close(fd);
		}
	}

	return iRet;
}
