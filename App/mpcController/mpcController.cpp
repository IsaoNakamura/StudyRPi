/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>

#include <wiringPi.h>

#define GPIO_NO	(12)

#define GPIO_A		(23)
#define GPIO_B		(22)

int main(int argc, char* argv[])
{
	
	if( wiringPiSetupGpio() == -1 ){
		printf("failed to wiringPiSetupGpio()\n");
		return 1;
	}
	
	pinMode(GPIO_A, INPUT);
	pinMode(GPIO_B, INPUT);


	while(1){
	}

	return 0;
}
