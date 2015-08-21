/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>

#include <wiringPi/wiringPi.h>

#define EXEC_CNT	(10)
#define DELAY_SEC	(1)
#define GPIO_NO		(18)

int main(int argc, char* argv[])
{
	
	if( wiringPiSetupGpio() == -1 ){
		printf("failed to wiringPiSetupGpio()¥n");
		return 1;
	}
	
	pinMode(GPIO_NO, PWM_OUTPUT);
	pwmSetMode(PWM_MODE_MS);
	pwmSetClock(400);
	pwmSetRange(1024);
	
	while(1){
		int num = 0;
		std::cin >> num;
		
		if(num==-1){
			break;
		}
		pwmWrite(GPIO_NO, num);
	}

	return 0;
}
