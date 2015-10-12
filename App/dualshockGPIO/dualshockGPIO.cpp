/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include "../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.h"
#include "../../Lib/drivers/JoystickDrv/CJoystickDrv.h"

#define EXEC_CNT	(1000)
#define DELAY_USEC	(100000)	// 100000usec = 100msec = 0.1sec
#define GPIO_A		(17)
#define GPIO_B		(18)

int main(int argc, char* argv[])
{
	int iRet = 0;

	CJoystickDrv* pJoystick = NULL;

	try
	{
		// GPIOを初期化
		if( RPiGpioDrv::init(RPI_VER_TWO) != 0 ){
			printf("failed to RPiGpioDrv::init().\n");
			throw 0;
		}

		// GPIO_Aを出力に設定
		if( RPiGpioDrv::setPinMode(GPIO_A, GPIO_OUTPUT) != 0 ){
			printf("failed to RPiGpioDrv::setPinMode(GPIO_OUTPUT).\n");
			throw 0;
		}

		// GPIO_Bを入力に設定
		if( RPiGpioDrv::setPinMode(GPIO_B, GPIO_INPUT) != 0 ){
			printf("failed to RPiGpioDrv::setPinMode(GPIO_INPUT).\n");
			throw 0;
		}

		// ready DUALSHOCK
		pJoystick = CJoystickDrv::createInstance();
		if(!pJoystick){
			throw 0;
		}
		if(pJoystick->connectJoystick()!=0){
			printf("failed to connectJoystick()\n");
			throw 0;
		}
		printf("begin loop \n");

		// EXEC_CNT分処理を繰り返す
		int i=0;
		while(1){//(i<=EXEC_CNT){
			// DUALSHOCKの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()\n");
				throw 0;
			}

			//Left
			if(pJoystick->getButtonState(JOY_LEFT) == BUTTON_ON)
			{
				printf("pushed Left-Button\n");

				// GPIO_AをON(Highレベル)にする
				if( RPiGpioDrv::setOutLevel(GPIO_A, GPIO_LV_HIGH) != 0 ){
					printf("failed to setOutLevel(GPIO_LV_HIGH).\n");
					throw 0;
				}
				usleep(DELAY_USEC);
			}

			//Right
			if(pJoystick->getButtonState(JOY_RIGHT) == BUTTON_ON)
			{
				printf("pushed Right-Button\n");

				// GPIO_AをOFF(Lowレベル)にする
				if( RPiGpioDrv::setOutLevel(GPIO_A, GPIO_LV_LOW) != 0 ){
					printf("failed to setOutLevel(GPIO_LV_LOW).\n");
					throw 0;
				}
				usleep(DELAY_USEC);
			}

			//Up
			if(pJoystick->getButtonState(JOY_UP) == BUTTON_ON)
			{
				printf("pushed Up-Button\n");
			}

			//Down
			if(pJoystick->getButtonState(JOY_DOWN) == BUTTON_ON)
			{
				printf("pushed Down-Button\n");
			}

			//Maru
			if(pJoystick->getButtonState(JOY_MARU) == BUTTON_ON)
			{
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
