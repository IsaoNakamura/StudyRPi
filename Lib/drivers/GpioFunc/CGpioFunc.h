#ifndef _C_GPIO_FUNC_H_
#define _C_GPIO_FUNC_H_

class CGpioFunc
{
private:
	CGpioFunc();
public:
	static CGpioFunc* createInstance();
	virtual ~CGpioFunc();
private:
	void init();
	void destroy();
	bool create();

public:
	// wrapper to wiringPi.
	static int wiringPiSetupGpio();
	static int pinMode(int pin, int mode);
	static int digitalRead(int pin);
	static int digitalWrite(int pin, int level);
	static int pwmSetMode(int mode);
	static int pwmSetClock(int clock);
	static int pwmSetRange(int range);
	static int pwmWrite(int pin, int num);
private:

};

#endif
