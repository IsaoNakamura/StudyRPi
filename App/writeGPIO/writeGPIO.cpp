/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include<stdio.h>
#include<stdlib.h>
#include<unistd.h>
#include "../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.h"

#define EXEC_CNT	(10)
#define DELAY_SEC	(1)
#define GPIO_NO		(17)

int main(int argc, char* argv[])
{
	// GPIOを初期化
	if( RPiGpioDrv::init(RPI_VER_TWO) != 0 ){
		printf("failed to RPiGpioDrv::init().\n");
		return -1;
	}
	
	// GPIO_17 を出力に設定
	if( RPiGpioDrv::setPinMode(GPIO_NO, GPIO_OUTPUT) != 0 ){
		printf("failed to RPiGpioDrv::setPinMode(GPIO_OUTPUT).\n");
		return -1;
	}
	
	// EXEC_CNT回ループしてONとOFFを繰り返す
	int i=0;
	for(i=0; i<EXEC_CNT; i++){
		if( (i%2) == 0 ){
			// 偶数の場合
			// GPIO17をON(Highレベル)にする。
			if( RPiGpioDrv::setOutLevel(GPIO_NO, GPIO_LV_HIGH) != 0 ){
				printf("failed to setOutLevel(GPIO_LV_HIGH).\n");
				return -1;
			}
			printf("gpio is HighLevel\n");
		}else{ 
			// 奇数の場合
			// GPIO17をOFF(Lowレベル)にする。
			if( RPiGpioDrv::setOutLevel(GPIO_NO, GPIO_LV_LOW) != 0){
				printf("failed to setOutLevel(GPIO_LV_LOW).\n");
				return -1;
			}
			printf("gpio is LowLevel\n");
		}

		// DELAY_SEC秒待つ
		sleep(DELAY_SEC);
	}
	return 0;
}
