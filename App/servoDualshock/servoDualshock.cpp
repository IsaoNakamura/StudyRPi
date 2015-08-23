/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include <wiringPi.h>
#include "../../Lib/drivers/JoystickDrv/CJoystickDrv.h"

#define GPIO_NO		(18)

#define GWS_PARK_MIN	(30)
#define GWS_PARK_MAX	(110)

int main(int argc, char* argv[])
{
	int iRet = 0;

	CJoystickDrv* pJoystick = NULL;

	try
	{
		// ready GPIO
		if( wiringPiSetupGpio() == -1 ){
			printf("failed to wiringPiSetupGpio()¥n");
			return 1;
		}

		// ready PWM
		pinMode(GPIO_NO, PWM_OUTPUT);
		pwmSetMode(PWM_MODE_MS);
		pwmSetClock(400);
		pwmSetRange(1024);
	
		// ready Joystick
		pJoystick = CJoystickDrv::createInstance();
		if(!pJoystick){
			throw 0;
		}
		if(pJoystick->connectJoystick()!=0){
			printf("failed to connectJoystick()\n");
			throw 0;
		}
		printf("begin loop \n");

		int i=0;
		while(1){
			// Joystickの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()\n");
				throw 0;
			}
			
			// DUALSHOCK m_pAxis[23] roll値 中０度0 左９０度4000　右９０度 -4600
			// servoMotor GWS park hpx min25 mid74 max123
			int roll = pJoystick->getAxisState(23);
			int val = 74;
			if(roll==0){ // 中間値
				val = 74;
			}else if(roll < 0){ // 右
				double rate = (double)roll / (double)(-4600);
				int delta = (int)( (double)( 74 - 25) * rate );
				val = 74 - delta;
			}else if(roll > 0){ // 左
				double rate = (double)roll / (double)(4000);
				int delta = (int)( (double)( 123 - 74 ) * rate );
				val = 74 + delta;
			}
			pwmWrite(GPIO_NO, val);
			
			//Maru
			if(pJoystick->getButtonState(JOY_MARU) == BUTTON_ON){
				printf("pushed Maru-Button\n");
				break;
			}

			i++;
			sleep(0);
		}
		printf("end loop\n");

		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}
	catch(...)
	{
		printf("catch!! \n");
		iRet = -1;
		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}

	return iRet;
}
