/////////////////////////////////////////////
// this class is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>

#include "CMassunDroid.h"

CMassunDroid::CMassunDroid() {
	// TODO 自動生成されたコンストラクター・スタブ
	init();

}

CMassunDroid::~CMassunDroid() {
	// TODO Auto-generated destructor stub
	destroy();
}

CMassunDroid* CMassunDroid::createInstance()
{
	CMassunDroid* pObj = NULL;
	try
	{
		pObj = new CMassunDroid();
		if(!pObj)
		{
			throw 0;
		}

		if(pObj->startInstance()!=0)
		{
			throw 0;
		}
	}
	catch(...)
	{
		if(pObj)
		{
			delete pObj;
			pObj = NULL;
		}
	}

	return pObj;
}


void CMassunDroid::init()
{
	return;
}

void CMassunDroid::destroy()
{
	return;
}


int CMassunDroid::startInstance()
{
	int iRet = -1;
	try
	{
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}
