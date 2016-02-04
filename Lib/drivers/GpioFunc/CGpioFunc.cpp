#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include "CGpioFunc.h"

#include <wiringPi.h>

CGpioFunc::CGpioFunc()
{
	this->init();
}

CGpioFunc::~CGpioFunc()
{
	this->destroy();
}

void CGpioFunc::init()
{
	return;
}

void CGpioFunc::destroy()
{
	return;
}

CGpioFunc* CGpioFunc::createInstance()
{
	CGpioFunc* pObj = NULL;
	pObj = new CGpioFunc;
	if(!pObj){
		printf("failed to create CGpioFunc's instance\n");
		return NULL;
	}
		
	return pObj;
}

bool CGpioFunc::create()
{
	return;
}

