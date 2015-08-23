/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include "../../Lib/drivers/JoystickDrv/CJoystickDrv.h"

int main(int argc, char* argv[])
{
	int iRet = 0;

	CJoystickDrv* pJoystick = NULL;

	try
	{
		// ready Joystick
		pJoystick = CJoystickDrv::createInstance();
		if(!pJoystick){
			throw 0;
		}
		if(pJoystick->connectJoystick()!=0){
			printf("failed to connectJoystick()¥n");
			throw 0;
		}
		printf("begin loop \n");

		int i=0;
		while(1){
			// Joystickの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()¥n");
				throw 0;
			}
			
			// Axis
			int axis_idx = 0;
			for(axis_idx=0; axis_idx<pJoystick->getNumAxis(); axis_idx++){
				int axis_stat = pJoystick->getAxisState(axis_idx);
				printf("axis[%02d]:%d ",axis_idx,axis_stat);
			}
			
			// Button
			int btn_idx = 0;
			for(btn_idx=0; btn_idx<pJoystick->getNumButton(); btn_idx++){
				int btn_stat = pJoystick->getButtonState(btn_idx);
				printf("btn[%02d]:%d ",btn_idx,btn_stat);
			}
			printf("¥n");

			i++;
			sleep(0);
		}
		printf("end loop¥n");

		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}
	catch(...)
	{
		printf("catch!! ¥n");
		iRet = -1;
		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}

	return iRet;
}
