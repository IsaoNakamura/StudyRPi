/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <math.h>

#include <wiringPi.h>
#include "../../Lib/drivers/JoystickDrv/CJoystickDrv.h"
#include "../../Lib/drivers/ServoDrv/CServoDrv.h"

#define GPIO_YAW	(12)
#define GPIO_PITCH	(13)
#define SERVO_MIN	(36)
#define SERVO_MID	(76)
#define SERVO_MAX	(122)
#define SERVO_RANGE	(180)

#define STICK_LEFT_X	(0)
#define STICK_LEFT_Y	(1)
#define STICK_RIGHT_X	(2)
#define STICK_RIGHT_Y	(3)

int main(int argc, char* argv[])
{
	printf("Press Maru-Button to Exit Process.\n");
	int iRet = 0;

	CJoystickDrv* pJoystick = NULL;
	CServoDrv* pServoYaw	= NULL;
	CServoDrv* pServoPitch	= NULL;

	try
	{
		// DUALSHOCK LeftJoystick(m_pAxis[0])
		const int axis_min = -32767;
		const int axis_max = 32767;
		const int axis_mid = 0;

		const int servo_mid = SERVO_MID;
		const int servo_min = SERVO_MIN;
		const int servo_max = SERVO_MAX;
		const int servo_range = SERVO_RANGE;
		
		// ready Joystick
		pJoystick = CJoystickDrv::createInstance();
		if(!pJoystick){
			throw 0;
		}
		if(pJoystick->connectJoystick()!=0){
			printf("failed to connectJoystick()\n");
			throw 0;
		}
		
		// ready GPIO
		if( wiringPiSetupGpio() == -1 ){
			printf("failed to wiringPiSetupGpio()\n");
			return 1;
		}
	
		// ready Servo-Obj
		pServoYaw = CServoDrv::createInstance(	GPIO_YAW,
												servo_min,
												servo_max,
												servo_range	);
		if(!pServoYaw){
			printf("failed to create pServoYaw\n");
			throw 0;
		}
		if(!pServoYaw->setLimitAngleValue(servo_mid-15,servo_mid+15)){
			printf("failed to setLimitAngleValue pServoYaw\n");
			throw 0;
		}
		if(!pServoYaw->setMidAngleValue(servo_mid)){
			printf("failed to setLimitAngleValue() pServoYaw\n");
			throw 0;
		}

		pServoPitch = CServoDrv::createInstance(	GPIO_PITCH,
													servo_min,
													servo_max,
													servo_range	);
		if(!pServoPitch){
			printf("failed to create pServoPitch\n");
			throw 0;
		}
		if(!pServoPitch->setLimitAngleValue(servo_mid-15,servo_mid+15)){
			printf("failed to setLimitAngleValue pServoPitch\n");
			throw 0;
		}
		if(!pServoPitch->setMidAngleValue(servo_mid)){
			printf("failed to setLimitAngleValue() pServoPitch\n");
			throw 0;
		}

		// reflesh Servo-Angle.
		pServoYaw->refleshServo();
		pServoPitch->refleshServo();
	
		printf("begin loop \n");
		while(1){
			// Joystickの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()\n");
				throw 0;
			}

			int joy_yaw		= pJoystick->getAxisState(STICK_LEFT_X);
			int joy_pitch	= pJoystick->getAxisState(STICK_RIGHT_Y);
			
			int val_yaw		= servo_mid;
			int val_pitch	= servo_mid;
			
			if(joy_yaw==axis_mid){ // 中間値
				val_yaw = servo_mid;
			}else if(joy_yaw > axis_mid){ // 右
				double ratio = fabs( (double)joy_yaw / (double)(axis_max) );
				int delta = (int)( (double)( servo_mid - servo_min) * ratio );
				val_yaw = servo_mid - delta;

			}else if(joy_yaw < axis_mid){ // 左
				double ratio = fabs( (double)joy_yaw / (double)(axis_min) );
				int delta = (int)( (double)( servo_max - servo_mid ) * ratio );
				val_yaw = servo_mid + delta;
			}
			
			if(joy_pitch==axis_mid){ // 中間値
				val_pitch = servo_mid;
			}else if(joy_pitch > axis_mid){ // 右
				double ratio = fabs( (double)joy_pitch / (double)(axis_max) );
				int delta = (int)( (double)( servo_mid - servo_min) * ratio );
				val_pitch = servo_mid - delta;

			}else if(joy_pitch < axis_mid){ // 左
				double ratio = fabs( (double)joy_pitch / (double)(axis_min) );
				int delta = (int)( (double)( servo_max - servo_mid ) * ratio );
				val_pitch = servo_mid + delta;
			}
			
			// Write PWM.
			//printf("val_yaw=%d,val_pitch=%d\n",val_yaw,val_pitch);
			pServoYaw->writeAngleValue(val_yaw);
			pServoPitch->writeAngleValue(val_pitch);
			
			//Maru
			if(pJoystick->getButtonState(JOY_MARU) == BUTTON_ON){
				printf("pushed Maru-Button\n");
				break;
			}

			sleep(0);
		}
		printf("end loop\n");

		// reflesh Servo-Angle.
		pServoYaw->refleshServo();
		pServoPitch->refleshServo();
		
		if(pServoYaw){
			delete pServoYaw;
			pServoYaw = NULL;
		}
		if(pServoPitch){
			delete pServoPitch;
			pServoPitch = NULL;
		}
		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}
	catch(...)
	{
		printf("catch!! \n");
		iRet = -1;
		if(pServoYaw){
			delete pServoYaw;
			pServoYaw = NULL;
		}
		if(pServoPitch){
			delete pServoPitch;
			pServoPitch = NULL;
		}
		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}

	return iRet;
}
