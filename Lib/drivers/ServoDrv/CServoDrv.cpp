#include <stdio.h>
#include <stdlib.h>

#include "CServoDrv.h"

#define DEF_PWM_CLOCK	(400);
#define DEF_PWM_RANGE	(1024);

#define GPIO_PWM_CH0_0	(12)
#define GPIO_PWM_CH0_1	(18)
#define GPIO_PWM_CH1_0	(13)
#define GPIO_PWM_CH1_1	(19)

CServoDrv::CServoDrv()
{
	this->init();
}

CServoDrv::~CServoDrv()
{
	this->destroy();
}

CServoDrv* CServoDrv::createInstance(	const int& gpioPin,
										const int& valueMin,
										const int& valueMax,
										const int& movementRange	)
{
	// 入力値チェック
	if(	   gpioPin != GPIO_PWM_CH0_0
		&& gpioPin != GPIO_PWM_CH0_1	
		&& gpioPin != GPIO_PWM_CH1_0
		&& gpioPin != GPIO_PWM_CH1_1	){
		return false;	
	}
	if(valueMax <= valueMin){
		return false;
	}
	if( movementRange < 0 ){
		return false;
	}
	
	CServoDrv* pObj = NULL;
	pObj = new CServoDrv;
	if(!pObj){
		printf("failed to create CServoDrv's instance\n");
		return NULL;
	}
	pObj->m_gpioPin			= gpioPin;
	pObj->m_valueMin		= valueMin;
	pObj->m_valueMax		= valueMax;
	pObj->m_movementRange	= movementRange;
	
	if(! calcRatioDeg2Value(	m_ratioDeg2Value,
								m_valueMin,
								m_valueMax,
								m_movementRange	) ){
		delete pObj;
		return NULL;						
	}
	
	return pObj;
}

void CServoDrv::init()
{
	m_gpioPin			= 0;
	m_pwmClock			= DEF_PWM_CLOCK;
	m_pwmRange			= DEF_PWM_RANGE;
	m_valueMin			= 0;
	m_valueMax			= 0;
	m_movementRange		= 180.0;
	m_valueMinLimit		= m_valueMin;
	m_valueMaxLimit		= m_valueMax;
	m_ratioDeg2Value	= 1.0;
}

void CServoDrv::destroy()
{
	return;
}

bool CServoDrv::calcRatioDeg2Value(	double&		ratioDeg2Value,
									const int&	valueMin,
									const int&	valueMax,
									const int&	movementRange	){
	// 返答領域を初期化
	ratioDeg2Value = 0;
	
	// 入力値チェック
	int deltaValue = valueMax - valueMin;
	if(deltaValue <= 0){
		return false;
	}

	ratioDeg2Value = movementRange / static_cast<double>(deltaValue);

	return true;								
}


