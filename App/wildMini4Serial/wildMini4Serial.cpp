/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <math.h>

#include "../../Lib/drivers/JoystickDrv/CJoystickDrv.h"
#include "../../Lib/drivers/SerialDrv/CSerialDrv.h"
#include "../../Lib/utilities/LinearTransformation/LinearTransformation.h"

#define SERVO_DEG_MIN	(0)
#define SERVO_DEG_MID	(81)
#define SERVO_DEG_MAX	(180)
#define SERVO_DEG_CLEARANCE (20)

#define DUALSHOCK_ANALOG_VAL_MAX	( 32767)
#define DUALSHOCK_ANALOG_VAL_MID	(     0)
#define DUALSHOCK_ANALOG_VAL_MIN	(-32767)

#define DUALSHOCK_ANALOG_LEFT_X		(0)
#define DUALSHOCK_ANALOG_LEFT_Y		(1)
#define DUALSHOCK_ANALOG_RIGHT_X	(2)
#define DUALSHOCK_ANALOG_RIGHT_Y	(3)

bool sendMotorParam(	CSerialDrv* pSerial,
						const int& val_yaw,
						const int& val_accel	)
{
	
	if(!pSerial){
		printf("pSerial is NULL @sendMotorParam\n");
		return false;
	}
	if(val_yaw < 0){
		printf("val_yaw is under Zero @sendMotorParam\n");
		return false;
	}
	if(val_accel < 0){
		printf("val_accel is under Zero @sendMotorParam\n");
		return false;
	}

	unsigned char sendBuf[4] = {0};
	sendBuf[0]	= 0x7E;	// 開始デリミタ
	sendBuf[1]	= 0x00;	// FrameType
	sendBuf[2]	= static_cast<unsigned char>(val_yaw);	// ヨー角度
	sendBuf[3]	= static_cast<unsigned char>(val_accel);	// 速度
	
	if( pSerial->sendData(sendBuf, 4) != 0 ){
		printf("failed to sendData() @sendMotorParam\n");
		return false;
	}
	
	return true;
}

int main(int argc, char* argv[])
{
	printf("Press Maru-Button to Exit Process.\n");
	int iRet = 0;

	CJoystickDrv*	pJoystick	= NULL;
	CSerialDrv*		pSerial		= NULL;

	try
	{
		// DUALSHOCK LeftJoystick(m_pAxis[0])
		const int axis_min = DUALSHOCK_ANALOG_VAL_MIN;
		const int axis_max = DUALSHOCK_ANALOG_VAL_MAX;
		const int axis_mid = DUALSHOCK_ANALOG_VAL_MID;

		const int servo_mid = SERVO_DEG_MID;
		const int servo_min = SERVO_DEG_MIN;
		const int servo_max = SERVO_DEG_MAX;
		
		// ready Joystick-Obj
		pJoystick = CJoystickDrv::createInstance();
		if(!pJoystick){
			throw 0;
		}
		if(pJoystick->connectJoystick()!=0){
			printf("failed to connectJoystick()\n");
			throw 0;
		}

	
		// ready Serial-Obj
		pSerial = CSerialDrv::createInstance();
		if(!pSerial){
			printf("failed to create pSerial\n");
			throw 0;
		}
		
		// Init Motor-Param.
		if(! sendMotorParam( pSerial, servo_mid, servo_mid)){
			printf("failed to sendMotorParam()\n");
			throw 0;
		}

		int val_yaw_pre		= servo_mid;
		int val_accel_pre	= servo_mid;
	
		printf("begin loop \n");
		while(1){
			// Joystickの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()\n");
				throw 0;
			}
			
			int val_yaw		= val_yaw_pre;
			int val_accel	= val_accel_pre;
			bool isChanged = false;
			
			if( pJoystick->isChangedAxis(DUALSHOCK_ANALOG_LEFT_X)==1 ){
				int joy_yaw		= pJoystick->getAxisState(DUALSHOCK_ANALOG_LEFT_X);
				if(! LinearTransformation::convertValueMinMidMax(	val_yaw,
																	joy_yaw,
																	axis_min,
																	axis_mid,
																	axis_max,
																	servo_min,
																	servo_mid,
																	servo_max,
																	false		) ){
					throw 0;					
				}
				if((servo_mid-SERVO_DEG_CLEARANCE)>val_yaw){
					val_yaw = (servo_mid-SERVO_DEG_CLEARANCE);
				}else if( (servo_mid+SERVO_DEG_CLEARANCE)<val_yaw){
					val_yaw = (servo_mid+SERVO_DEG_CLEARANCE);
				}
				
				if(val_yaw_pre != val_yaw){
					isChanged = true;
					val_yaw_pre = val_yaw;
				}
			}
			
			if( pJoystick->isChangedAxis(DUALSHOCK_ANALOG_RIGHT_Y)==1 ){
				int joy_accel	= pJoystick->getAxisState(DUALSHOCK_ANALOG_RIGHT_Y);
				if(! LinearTransformation::convertValueMinMidMax(	val_accel,
																	joy_accel,
																	axis_min,
																	axis_mid,
																	axis_max,
																	servo_min,
																	servo_mid,
																	servo_max,
																	true		 ) ){
					throw 0;					
				}
				if(val_accel_pre != val_accel){
					isChanged = true;
					val_accel_pre = val_accel;
				}
			}
			
			if(isChanged){
				if(! sendMotorParam( pSerial, val_yaw, val_accel)){
					printf("failed to sendMotorParam()\n");
					throw 0;
				}
				printf("val_yaw=%d, val_accel=%d\n",val_yaw,val_accel);
			}
			
			//Maru for Exit-Loop.
			if( pJoystick->isChangedButton(JOY_MARU)==1 ){
				if(pJoystick->getButtonState(JOY_MARU) == BUTTON_ON){
					printf("pushed Maru-Button\n");
					break;
				}
			}

			sleep(0);
		}
		printf("end loop\n");
		
		// Reset Motor-Param.
		if(! sendMotorParam( pSerial, servo_mid, servo_mid)){
			printf("failed to sendMotorParam()\n");
			throw 0;
		}
		
		if(pSerial){
			delete pSerial;
			pSerial = NULL;
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
		if(pSerial){
			delete pSerial;
			pSerial = NULL;
		}
		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}

	return iRet;
}
