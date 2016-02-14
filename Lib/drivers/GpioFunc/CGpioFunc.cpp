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

int CGpioFunc::wiringPiSetupGpio()
{
    return wiringPiSetupGpio();
}

int CGpioFunc::pinMode(int pin, int mode)
{
    return pinMode(pin, mode);
}

int CGpioFunc::digitalRead(int pin)
{
    return digitalRead(pin);
}

int CGpioFunc::digitalWrite(int pin, int level)
{
    return digitalWrite(pin, level);
}

int CGpioFunc::pwmSetMode(int mode)
{
    return pwmSetMode(mode);
}

int CGpioFunc::pwmSetClock(int clock)
{
    return pwmSetClock(clock);
}

int CGpioFunc::pwmSetRange(int range)
{
    return pwmSetRange(range);
}

int CGpioFunc::pwmWrite(int pin, int num)
{
    return pwmWrite(pin, num);
}