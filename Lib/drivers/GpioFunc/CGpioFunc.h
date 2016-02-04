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

private:

};

#endif
