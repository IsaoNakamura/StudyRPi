/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>
#include <math.h>


#include "../../Lib/contents/MassunDroid/CMassunDroid.h"

int main(int argc, char* argv[])
{
    int iRet = -1;
    CMassunDroid *pMassun = NULL;
    try
    {
        pMassun = CMassunDroid::createInstance();
        if(!pMassun){
            printf("failed to CMassunDroid::createInstance()\n");
            throw 0;
        }
        
        if(!pMassun->exec()){
            printf("failed to CMassunDroid::exec()\n");
            throw 0;
        }
        
        if(pMassun){
            delete pMassun;
            pMassun=NULL;
        }
		// ここまでくれば成功
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
        if(pMassun){
            delete pMassun;
            pMassun=NULL;
        }
	}

	return iRet;
}
