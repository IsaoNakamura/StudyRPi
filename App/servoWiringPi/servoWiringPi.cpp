/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>

#include <wiringPi.h>

#define EXEC_CNT	(10)
#define DELAY_SEC	(1)
#define GPIO_NO	(13)

#define GWS_PARK_MIN	(36)
#define GWS_PARK_MID	(76)
#define GWS_PARK_MAX	(122)

int main(int argc, char* argv[])
{
	
	if( wiringPiSetupGpio() == -1 ){
		printf("failed to wiringPiSetupGpio()\n");
		return 1;
	}
	
	pinMode(GPIO_NO, PWM_OUTPUT);
	pwmSetMode(PWM_MODE_MS);
	pwmSetClock(400);
	pwmSetRange(1024);

	printf("Input '-1' to Next-Stage.\n");
	while(1){
		int num = 0;
		std::cin >> num;
		
		if(num==-1){
			break;
		}
		pwmWrite(GPIO_NO, num);
	}
/*
	// Change to the Middle-Pos.
	pwmWrite(GPIO_NO, GWS_PARK_MID);

	// EXEC_CNT回ループしてONとOFFを繰り返す
	int i=0;
	for(i=0; i<EXEC_CNT; i++){
		if( (i%2) == 0 ){
			// 偶数の場合
			// GPIO18をMAXにする。
			pwmWrite(GPIO_NO, GWS_PARK_MAX);
			printf("pwm is Max\n");
		}else{ 
			// 奇数の場合
			// GPIO18をMINにする。
			pwmWrite(GPIO_NO, GWS_PARK_MIN);
			printf("pwm is Min\n");
		}

		// DELAY_SEC秒待つ
		sleep(DELAY_SEC);
	}
*/
	// Return to the Middle-Pos.
	pwmWrite(GPIO_NO, GWS_PARK_MID);

	return 0;
}
