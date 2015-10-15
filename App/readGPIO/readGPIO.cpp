/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include<stdio.h>
#include<stdlib.h>
#include<unistd.h>
#include "../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.h"

#define EXEC_CNT	(300)
#define DELAY_USEC	(100000)	// 100000usec = 100msec = 0.1sec
#define GPIO_A		(17)
#define GPIO_B		(23)

int main(int argc, char* argv[])
{
	// GPIOを初期化
	if( RPiGpioDrv::init(RPI_VER_TWO) != 0 ){
		printf("failed to RPiGpioDrv::init().\n");
		return -1;
	}
	
	// GPIO_A を出力に設定
	if( RPiGpioDrv::setPinMode(GPIO_A, GPIO_OUTPUT) != 0 ){
		printf("failed to RPiGpioDrv::setPinMode(GPIO_OUTPUT).\n");
		return -1;
	}

	// GPIO_Bを入力に設定
	if( RPiGpioDrv::setPinMode(GPIO_B, GPIO_INPUT) != 0 ){
		printf("failed to RPiGpioDrv::setPinMode(GPIO_INPUT).\n");
		return -1;
	}

	// EXEC_CNT分処理を繰り返す
	int i=0;
	while(i<=EXEC_CNT){
		// GPIO_Bの状態を読み込み
		int val = 0;
		if( RPiGpioDrv::getLevel(GPIO_B, val)!=0 ){
			printf("failed to RPiGpioDrv::getLevel().\n");
			return -1;
		}
		printf("%03d: GPIO_B is %d\n",i,val);
		if(val>0){
			// GPIO_AをON(Highレベル)にする
			if( RPiGpioDrv::setOutLevel(GPIO_A, GPIO_LV_HIGH) != 0){
				printf("failed to RPiGpioDrv::setOutLevel(GPIO_LV_HIGH).\n");
				return -1;
			}
		}else{
			// GPIO_AをOFF(Lowレベル)にする。
			if( RPiGpioDrv::setOutLevel(GPIO_A, GPIO_LV_LOW) != 0){
				printf("failed to RPiGpioDrv::setOutLevel(GPIO_LV_LOW).\n");
				return -1;
			}
		}

		i++;
		usleep(DELAY_USEC);
	}

	return 0;
}

