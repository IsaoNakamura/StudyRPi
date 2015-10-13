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

// PWM-Channel0 is on gpios 12 or 18.
// PWM-Channel1 is on gpios 13 or 19.
#define GPIO_NO	(12)

// for TowerPro SG90
#define SERVO_MIN	(36)
#define SERVO_MID	(76)
#define SERVO_MAX	(122)

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
		if( num < SERVO_MIN ){
			printf("%d is under min. \n",num);
			num = SERVO_MIN;
		}else if( num > SERVO_MAX ){
			printf("%d is over max. \n",num);
			num = SERVO_MAX;
		}
		pwmWrite(GPIO_NO, num);
	}

	// Change to the Middle-Pos.
	pwmWrite(GPIO_NO, SERVO_MID);

	// EXEC_CNT回ループしてONとOFFを繰り返す
	int i=0;
	for(i=0; i<EXEC_CNT; i++){
		if( (i%2) == 0 ){
			// 偶数の場合
			// GPIO_NOをMAXにする。
			pwmWrite(GPIO_NO, SERVO_MAX);
			printf("pwm is Max\n");
		}else{ 
			// 奇数の場合
			// GPIO_NOをMINにする。
			pwmWrite(GPIO_NO, SERVO_MIN);
			printf("pwm is Min\n");
		}

		// DELAY_SEC秒待つ
		sleep(DELAY_SEC);
	}

	// Return to the Middle-Pos.
	pwmWrite(GPIO_NO, SERVO_MID);

	return 0;
}
