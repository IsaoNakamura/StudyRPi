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
#define GPIO_VUP	(25)
#define GPIO_VDWN	(18)

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
	pinMode(GPIO_VUP, INPUT);
	pinMode(GPIO_VDWN, INPUT);

	while(1){
		//  default status is HIGH.
		int valExit = digitalRead(GPIO_EXIT);
		int valNext = digitalRead(GPIO_NEXT);
		int valPrev = digitalRead(GPIO_PREV);
		int valStop = digitalRead(GPIO_STOP);
		int valPlay = digitalRead(GPIO_PLAY);
		int valVolUp = digitalRead(GPIO_VUP);
		int valVolDwn = digitalRead(GPIO_VDWN);
				
		if(valExit == LOW){
			printf("shutdown system.\n");
			system("sudo halt");
		}
		if(valNext == LOW)){
			printf("next.\n");
			system("mpc next");
		}
		if(valPrev == LOW)){
			printf("prev\n");
			system("mpc prev");
		}
		if(valStop == LOW)){
			printf("stop.\n");
			system("mpc stop");
		}
		if(valPlay == LOW)){
			printf("play.\n");
			system("mpc play");
		}
		if(valVolUp == LOW)){
			printf("volume up.\n");
			system("mpc volume +2");
		}
		if(valVolDwn == LOW)){
			printf("volume up.\n");
			system("mpc volume -2");
		}
		sleep(0);
	}

	return 0;
}
