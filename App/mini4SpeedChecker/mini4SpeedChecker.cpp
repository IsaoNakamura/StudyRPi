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


/*
3.速度の検出原理を推測する。
・ローラーの直径とギヤ比から、ローラー回転パルス信号の1間隔の距離を計算する。
ローラーの直径＝29.4[mm]
ローラーの外周＝29.4[mm]×3.1415926＝92.36282244[mm]
ローラーと回転検出部のギヤ比＝6:1
ローラー回転パルス信号の1間隔の距離＝92.36282244[mm]×6＝554.17693464[mm]＝0.55417693464[m]

・車を走行させて、計測した速度と回転検出信号の周波数から、ローラー回転パルスの1間隔の距離を計算する。
ミニ四駆カーを走行させたら、14km/hを表示。
ローラーの回転信号の周波数はデジタルマルチメータの測定で約7Hz。
計測速度14[km/h]＝14,000[m/h]＝14,000[m] / 3600[s]＝3.8888889[m/s]
ローラー回転パルス信号の1間隔の距離＝3.8888889[m/s] / 7[pulse/s]＝0.5555556[m]

以上の結果から、
ローラー回転パルス信号の1間隔の距離は、0.55417693464[m]と推測する。

length[m] = speed[m/s] / pulse[Hz]

speed[m/s] = length[m] * pulse[Hz]
speed[km/h] = (length[m] * pulse[Hz] ) / 1000[m]  / 3600[s]
length[m] = 0.55417693464

# pulseの検出方法
while()が1秒間立ったら、閾値以上のret_valueが何回きたかカウントする

*/