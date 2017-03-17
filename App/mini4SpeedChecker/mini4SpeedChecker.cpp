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

// for gettimeofday
#include <sys/time.h>

#include "../../Lib/drivers/I2cAdcDrv/CI2cAdcDrv.h"

#define I2C_PORT	("/dev/i2c-1")

#define USE_CH 0

#define PULSE_INTERVAL_DIST (0.55417693464)	//[m]
#define SAMPLING_RATE (1000000) // [usec]

#define LOOP_MAX 1000

int calcSpeedPerOn( CI2cAdcDrv* pAdc );
int calcSpeedPerHz( CI2cAdcDrv* pAdc , suseconds_t samplingRate=SAMPLING_RATE);

int main(int argc, char* argv[])
{
	int iRet = 0;

	int fd = -1;
	CI2cAdcDrv* pAdc = NULL;

	int calcSpeedMode = 0;
	if( argc > 1 ){
		calcSpeedMode = 1;
	}

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
		
		if(calcSpeedMode==0){
			if( calcSpeedPerHz(pAdc) != 0){
				printf("failed to calcSpeedPerHz()¥n");
				throw 0;
			}
		}else if(calcSpeedMode==1){
			if( calcSpeedPerOn(pAdc) != 0){
				printf("failed to calcSpeedPerOn()¥n");
				throw 0;
			}
		}else{
			printf("error!! calcSpeedMode(%d) is out of range. ¥n",calcSpeedMode);
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

int calcSpeedPerHz( CI2cAdcDrv* pAdc, suseconds_t samplingRate/*=SAMPLING_RATE*/ )
{
	int iRet = -1;
	try
	{
		if(!pAdc){
			printf("error! pAdc is NULL\n");
			throw 0;
		}

		struct timeval stNow;	// 現時刻取得用
		struct timeval stLen;	// 任意時間
		struct timeval stEnd;	// 現時刻から任意時間経過した時刻
		timerclear(&stNow);
		timerclear(&stLen);
		timerclear(&stEnd);

		int loop_cnt = 0;

		gettimeofday(&stNow, NULL);
		stLen.tv_sec = 0;
		stLen.tv_usec = samplingRate;
		timeradd(&stNow, &stLen, &stEnd);

		int pulse_cnt = 0;

		while(loop_cnt<LOOP_MAX){
			// チャンネル0の値を取得
			unsigned short		ret_value=0;
			int					ret_state=0;
			if( pAdc->getChannelValue(ret_value, ret_state, USE_CH)==0 ){
				// 取得成功
				if( ret_state > 0 ){
					//printf("ch[%02d]:val=%05d,stat=%02d \n",USE_CH,ret_value, ret_state);
					pulse_cnt++;
				}
			}else{
				// 取得失敗
				printf("ch[%02d]:failed      \n",0);
			}

			// 現在時刻を取得
			gettimeofday(&stNow, NULL);
			if( timercmp(&stNow, &stEnd, >) ){
				// 任意時間経過

				// スピードを計算
				double speed_m_s = PULSE_INTERVAL_DIST * pulse_cnt / (samplingRate*1e-6);
				double speed_km_h = speed_m_s * 3600.0 / 1000.0;
				printf("Speed:%03.02f[km/s]:%05.02f[m/s]", speed_km_h, speed_m_s);

				// 経過時刻を更新
				timerclear(&stEnd);
            	gettimeofday(&stNow, NULL);
            	timeradd(&stNow, &stLen, &stEnd);

				// パルスをリセット
				pulse_cnt = 0;
			}


			loop_cnt++;

			sleep(0);
		}
	}
	catch(...)
	{
		printf("calcSpeedPerOn() catch!! \n");
		iRet = -1;
	}
	return iRet;
}

int calcSpeedPerOn( CI2cAdcDrv* pAdc )
{
	int iRet = -1;
	try
	{
		if(!pAdc){
			printf("error! pAdc is NULL\n");
			throw 0;
		}

		struct timeval stTrue;	// パルスONを検出時の時刻取得用
		struct timeval stLast;	// 前回パルスON時の時刻
		timerclear(&stTrue);
		timerclear(&stLast);

		int loop_cnt = 0;
		bool isFirstOn = true;
		while(loop_cnt<LOOP_MAX){
			// チャンネル0の値を取得
			unsigned short		ret_value=0;
			int					ret_state=0;
			if( pAdc->getChannelValue(ret_value, ret_state, USE_CH)==0 ){
				// 取得成功
				if( ret_state > 0 ){
					printf("ch[%02d]:val=%05d,stat=%02d \n",USE_CH,ret_value, ret_state);

					// 現在時刻を取得
					gettimeofday(&stTrue, NULL);
					if(isFirstOn){
						// 初回
						isFirstOn = false;
					}else{
						// 初回以降

						// スピードを計算
						time_t diffsec = difftime(stTrue.tv_sec, stLast.tv_sec);
						suseconds_t diffsub = stTrue.tv_usec - stLast.tv_usec;
						double realsec = diffsec+diffsub*1e-6; //[s]
						double speed_m_s = PULSE_INTERVAL_DIST / realsec ; //[m/s]
						double speed_km_h = speed_m_s * 3600.0 / 1000.0;
						printf("Speed:%03.02f[km/s]:%05.02f[m/s]", speed_km_h, speed_m_s);
					}

					// 前回時刻を更新
					stLast = stTrue;
				}
			}else{
				// 取得失敗
				printf("ch[%02d]:failed      \n",0);
			}
			loop_cnt++;

			sleep(0);
		}
	}
	catch(...)
	{
		printf("calcSpeedPerOn() catch!! \n");
		iRet = -1;
	}
	return iRet;
}



/*
ローラー回転パルス信号の1間隔の距離は、0.55417693464[m]と推測。

distance[m] = 0.55417693464
distance[m] = speed[m/s] / pulse[Hz]

speed[m/s] = distance[m] * pulse[Hz]
speed[km/h] = (distance[m] * pulse[Hz] ) / 1000[m]  * 3600[s]


# pulseの検出方法
while()が1秒間立ったら、閾値以上のret_valueが何回きたかカウントする

*/