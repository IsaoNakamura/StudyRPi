/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include<math.h>

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
			printf("failed to wiringPiSetupGpio()\n");
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

		// DUALSHOCK m_pAxis[23] roll値 中０度0 左９０度4000　右９０度 -4600
		const int axis_min = -4600;
		const int axis_max = 4000;
		const int axis_mid = 0;

		// servoMotor GWS park hpx min25 mid74 max123
		const int servo_min = 25;
		const int servo_max = 123;
		const int servo_mid = servo_min + (int)( (servo_max - servo_min) / 2 );

		printf("begin loop \n");
		while(1){
			// Joystickの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()\n");
				throw 0;
			}

			int roll = pJoystick->getAxisState(23);
			int val = servo_mid;
			if(roll==axis_mid){ // 中間値
				val = servo_mid;
			}else if(roll < axis_mid){ // 右
				double rate = fabs( (double)roll / (double)(axis_min) );
				int delta = (int)( (double)( servo_mid - servo_min) * rate );
				val = servo_mid - delta;
				if(val < servo_min){
					val = servo_min;
				}
			}else if(roll > axis_mid){ // 左
				double rate = fabs( (double)roll / (double)(axis_max) );
				int delta = (int)( (double)( servo_max - servo_mid ) * rate );
				val = servo_mid + delta;
				if(val > servo_max){
					val = servo_max;
				}
			}
			pwmWrite(GPIO_NO, val);
			
			//Maru
			if(pJoystick->getButtonState(JOY_MARU) == BUTTON_ON){
				printf("pushed Maru-Button\n");
				break;
			}

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
