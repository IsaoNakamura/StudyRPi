/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <math.h>

#include <wiringPi.h>
#include "../../Lib/drivers/JoystickDrv/CJoystickDrv.h"

// PWM-Channel0 is on gpios 12 or 18.
// PWM-Channel1 is on gpios 13 or 19.
#define GPIO_NO		(12)
#define DELAY_USEC	(0)	// 100000usec = 100msec = 0.1sec

#define DEF_PWM_CLOCK	(400)
#define DEF_PWM_RANGE	(1024)

#define DUALSHOCK_ANALOG_VAL_MAX	( 32767)
#define DUALSHOCK_ANALOG_VAL_MID	(     0)
#define DUALSHOCK_ANALOG_VAL_MIN	(-32767)

#define DUALSHOCK_ANALOG_LEFT	(0)

// for TowerPro SG90
#define SERVO_MIN	(36)
#define SERVO_MID	(76)
#define SERVO_MAX	(122)

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
		pwmSetClock(DEF_PWM_CLOCK);
		pwmSetRange(DEF_PWM_RANGE);
	
		// ready Joystick
		pJoystick = CJoystickDrv::createInstance();
		if(!pJoystick){
			throw 0;
		}
		if(pJoystick->connectJoystick()!=0){
			printf("failed to connectJoystick()\n");
			throw 0;
		}

		const int axis_min = DUALSHOCK_ANALOG_VAL_MIN;
		const int axis_max = DUALSHOCK_ANALOG_VAL_MAX;
		const int axis_mid = DUALSHOCK_ANALOG_VAL_MID;

		const int servo_mid = SERVO_MID;
		const int servo_max = SERVO_MID + 15;
		const int servo_min = SERVO_MID - 15;

		pwmWrite(GPIO_NO, servo_mid);
		printf("begin loop \n");
		int pre_val = servo_mid;
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
			if( pre_val != val ){
				printf("axis=%d, servo_val=%d\n",roll,val);
				pwmWrite(GPIO_NO, val);
				usleep(0);//usleep(DELAY_USEC);
				pre_val = val;
			}
			
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
