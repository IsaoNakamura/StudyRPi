/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>

#include <wiringPi.h>

#define GPIO_EXIT	(17)
#define GPIO_NEXT	(27)
#define GPIO_PREV	(22)
#define GPIO_STOP	(23)
#define GPIO_PLAY	(24)

int main(int argc, char* argv[])
{
	
	if( wiringPiSetupGpio() == -1 ){
		printf("failed to wiringPiSetupGpio()\n");
		return 1;
	}
	
	pinMode(GPIO_EXIT, INPUT);
	pinMode(GPIO_NEXT, INPUT);
	pinMode(GPIO_PREV, INPUT);
	pinMode(GPIO_STOP, INPUT);
	pinMode(GPIO_PLAY, INPUT);

	while(1){
		int valExit = digitalRead(GPIO_EXIT);
		int valNext = digitalRead(GPIO_NEXT);
		int valPrev = digitalRead(GPIO_PREV);
		int valStop = digitalRead(GPIO_STOP);
		int valPlay = digitalRead(GPIO_PLAY);
				
		if(valExit<0){
			printf("shutdown system.\n");
			system("sudo halt");
		}
		if(valNext<0){
			printf("next.\n");
			system("mpc next");
		}
		if(valPrev<0){
			printf("prev\n");
			system("mpc prev");
		}
		if(valStop<0){
			printf("stop.\n");
			system("mpc stop");
		}
		if(valPlay<0){
			printf("play.\n");
			system("mpc play");
		}
		sleep(0);
	}

	return 0;
}
