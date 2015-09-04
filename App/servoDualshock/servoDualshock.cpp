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

int main(int argc, char* argv[])
{
	printf("Press Maru-Button to Exit Process.\n");
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

		// DUALSHOCK LeftJoystick(m_pAxis[0])
		const int axis_min = -32767;
		const int axis_max = 32767;
		const int axis_mid = 0;

		// servoMotor GWS park hpx min25 mid74 max123
		const int servo_mid = 73;
		const int servo_min = servo_mid - 15;
		const int servo_max = servo_mid + 15;

		pwmWrite(GPIO_NO, servo_mid);
		printf("begin loop \n");
		while(1){
			// Joystickの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()\n");
				throw 0;
			}

			int roll = pJoystick->getAxisState(0);
			int val = servo_mid;
			if(roll==axis_mid){ // 中間値
				val = servo_mid;
			}else if(roll > axis_mid){ // 右
				double rate = fabs( (double)roll / (double)(axis_max) );
				int delta = (int)( (double)( servo_mid - servo_min) * rate );
				val = servo_mid - delta;
				if(val < servo_min){
					val = servo_min;
				}
			}else if(roll < axis_mid){ // 左
				double rate = fabs( (double)roll / (double)(axis_min) );
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

		pwmWrite(GPIO_NO, servo_mid);
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
