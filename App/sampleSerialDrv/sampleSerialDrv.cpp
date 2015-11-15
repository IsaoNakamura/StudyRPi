/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>

#include "../../Lib/drivers/SerialDrv/CSerialDrv.h"

int main(int argc, char* argv[])
{
	int iRet = -1;
	
	printf("Input '0' to Exit.\n");
	
	CSerialDrv* pSerial = NULL;
	
	try
	{
		pSerial = CSerialDrv::createInstance();
		if(!pSerial){
			throw 0;
		}
		
		while(1){
			unsigned char sendBuf[1] = {0};
			std::cin >> sendBuf;
			std::cout << sendBuf << std::endl;
			printf("sendBuf=0x%x\n",sendBuf[0]);
			
			if( pSerial->sendData(sendBuf, 1) != 0 ){
				throw 0;
			}
			
			if(sendBuf[0]==0){
				break;
			}
		}
		if(pSerial){
			delete pSerial;
			pSerial = NULL;
		}
		iRet = 0;
	}
	catch(...)
	{
		if(pSerial){
			delete pSerial;
			pSerial = NULL;
		}
		iRet = -1;
	}

	return iRet;
}
