/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include <wiringPi.h>

#define EXEC_CNT	(100)
#define DELAY_USEC	(0)	// 100000usec = 100msec = 0.1sec
#define GPIO_A		(17)
#define GPIO_B		(18)

int main(int argc, char* argv[])
{
	// WiringPiを初期化
	if( wiringPiSetupGpio() == -1 ){
		printf("failed to wiringPiSetupGpio()\n");
		return 1;
	}
	
	// GPIO_A を出力に設定
	pinMode(GPIO_A, OUTPUT);

	// GPIO_Bを入力に設定
	pinMode(GPIO_B, INPUT);

	// EXEC_CNT分処理を繰り返す
	int i=0;
	while(i<=EXEC_CNT){
		// GPIO_Bの状態を読み込み
		int val = digitalRead(GPIO_B);
		printf("%03d: GPIO_B is %d\n",i,val);
		if(val>0){
			// GPIO_AをON(Highレベル)にする
			digitalWrite(GPIO_A, HIGH);
		}else{
			// GPIO_AをOFF(Lowレベル)にする。
			digitalWrite(GPIO_A, LOW);
		}

		i++;
		usleep(DELAY_USEC);
	}

	return 0;
}
