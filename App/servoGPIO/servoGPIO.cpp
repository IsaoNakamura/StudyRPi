/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include<stdio.h>
#include<stdlib.h>
#include<unistd.h>
#include "../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.h"

#define EXEC_CNT	(10)
#define DELAY_SEC	(1)
#define GPIO_NO		(18)

int main(int argc, char* argv[])
{
	// GPIOを初期化
	if( RPiGpioDrv::init(RPI_VER_TWO) != 0 ){
		printf("failed to RPiGpioDrv::init().\n");
		return -1;
	}
	
	// GPIO_17 を出力に設定
	if( RPiGpioDrv::setPinMode(GPIO_NO, GPIO_PWM) != 0 ){
		printf("failed to RPiGpioDrv::setPinMode(GPIO_PWM).\n");
		return -1;
	}
	
	if( RPiGpioDrv::setPwmRange(1024) != 0 ){
		printf("failed to RPiGpioDrv::setPwmRange().\n");
		return -1;
	}
	if( RPiGpioDrv::setPwmClock(400) ){
		printf("failed to RPiGpioDrv::setPwmClock().\n");
		return -1;
	}
	
	// EXEC_CNT回ループ
	int i=0;
	for(i=0; i<EXEC_CNT; i++){
		if( (i%2) == 0 ){
			// 偶数の場合
			if( RPiGpioDrv::writePwmGpio(GPIO_NO, 50) != 0 ){
				printf("failed to writePwmGpio() 0.\n");
				return -1;
			}
			printf("pwm is Min\n");
		}else{ 
			// 奇数の場合
			if( RPiGpioDrv::writePwmGpio(GPIO_NO, 100) != 0){
				printf("failed to writePwmGpio() 1.\n");
				return -1;
			}
			printf("pwm is Max\n");
		}

		// DELAY_SEC秒待つ
		sleep(DELAY_SEC);
	}
	return 0;
}
