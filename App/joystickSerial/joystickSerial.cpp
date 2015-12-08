/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <math.h>

#include "../../Lib/drivers/JoystickDrv/CJoystickDrv.h"
#include "../../Lib/drivers/SerialDrv/CSerialDrv.h"

#define DUALSHOCK_ANALOG_VAL_MAX	( 32767)
#define DUALSHOCK_ANALOG_VAL_MID	(     0)
#define DUALSHOCK_ANALOG_VAL_MIN	(-32767)

#define DUALSHOCK_ANALOG_LEFT_X		(0)
#define DUALSHOCK_ANALOG_LEFT_Y		(1)
#define DUALSHOCK_ANALOG_RIGHT_X	(2)
#define DUALSHOCK_ANALOG_RIGHT_Y	(3)

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
	
		printf("begin loop \n");
		while(1){
			// Joystickの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()\n");
				throw 0;
			}

			int joy_yaw		= pJoystick->getAxisState(DUALSHOCK_ANALOG_LEFT_X);

			unsigned char sendBuf[1] = {0};
			/*
			if(joy_yaw==axis_mid){ // 中間値
			}else if(joy_yaw > axis_mid){ // 右
				sendBuf[0] = 0x30;
				printf("sendBuf=0x%x, joy_yaw=%d\n",sendBuf[0], joy_yaw);
				if( pSerial->sendData(sendBuf, 1) != 0 ){
					throw 0;
				}
			}else if(joy_yaw < axis_mid){ // 左
				sendBuf[0] = 0x31;
				printf("sendBuf=0x%x, joy_yaw=%d\n",sendBuf[0], joy_yaw);
				if( pSerial->sendData(sendBuf, 1) != 0 ){
					throw 0;
				}
			}
			*/
			if( pJoystick->isChangedButton(JOY_SANKAKU)==1 ){
				if(pJoystick->getButtonState(JOY_SANKAKU) == BUTTON_ON){
					sendBuf[0] = 0x30;
					printf("sendBuf=0x%x, pushed Sankaku-Button\n\n",sendBuf[0]);
					if( pSerial->sendData(sendBuf, 1) != 0 ){
						throw 0;
					}
				}
			}
			if( pJoystick->isChangedButton(JOY_SHIKAKU)==1 ){
				if(pJoystick->getButtonState(JOY_SHIKAKU) == BUTTON_ON){
					sendBuf[0] = 0x31;
					printf("sendBuf=0x%x, pushed Shikaku-Button\n\n",sendBuf[0]);
					if( pSerial->sendData(sendBuf, 1) != 0 ){
						throw 0;
					}
				}
			}
				
			//Maru
			if(pJoystick->getButtonState(JOY_MARU) == BUTTON_ON){
				printf("pushed Maru-Button\n");
				break;
			}

			sleep(0);
		}
		printf("end loop\n");
		
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
