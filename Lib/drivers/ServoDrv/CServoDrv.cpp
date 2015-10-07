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
	pObj->m_valueMid		= valueMin + (valueMax - valueMin)/2;
	pObj->m_movementRange	= movementRange;
	pObj->m_valueCur		= m_valueMid;
	
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
	m_gpioPin			= -1;
	m_pwmClock			= DEF_PWM_CLOCK;
	m_pwmRange			= DEF_PWM_RANGE;
	m_valueMin			= -1;
	m_valueMax			= -1;
	m_valueMid			= -1;
	m_movementRange		= 180.0;
	m_valueMinLimit		= m_valueMin;
	m_valueMaxLimit		= m_valueMax;
	m_ratioDeg2Value	= 1.0;
	m_valueCur		= -1;
}

void CServoDrv::destroy()
{
	return;
}

bool CServoDrv::calcRatioDeg2Value(	double&		ratioDeg2Value,
									const int&	valueMin,
									const int&	valueMax,
									const int&	movementRange	) const
{
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

bool CServoDrv::convDeg2Value(	int&			servo_value,
								const double&	deg,
								const double&	ratioDeg2Value	) const
{
	// 返答領域の初期化
	servo_value = m_valueMid;
	
	if( fabs(ratioDeg2Value) <= 1.0e-6 ){
		return false;
	}
	servo_value = static_cast<int>(deg / ratioDeg2Value);
	return true;
}

bool CServoDrv::setAngleDeg(const double& deg)
{
	int dest_value = 0;
	if( !CServoDrv::convDeg2Value(dest_value, deg, m_ratioDeg2Value)){
		return false;
	}
	m_valueCur = dest_value;
	return true;
}

bool CServoDrv::setAngleDegOffset(const double& deg_offset)
{
	return true;
}

bool CServoDrv::resetAngle()
{
	m_valueCur = m_valueMid
	return true;
}

bool CServoDrv::setMidAngleValue(const int& val)
{
	if( val <= m_valueMin ){
		return false;
	}
	if( val >= m_valueMax ){
		return false;
	}
	m_valueMid = val;
	return true;
}

double CServoDrv::getAngleDeg() const
{
	return 0.0;
}

int CServoDrv::getAngleValue() const
{
	return m_valueCur;
}

bool CServoDrv::flushServo()
{
	// using WiringPi
	pwmWrite(m_gpioPin, m_valueCur);
	return true;
}

bool CServoDrv::refleshServo()
{
	// using WiringPi
	pwmWrite(m_gpioPin, m_valueMid);
	return true;
}

bool CServoDrv::writeAngleDeg(const double& deg)
{
	return true;
}

bool CServoDrv::writeAngleDegOffset(const double& deg_offset)
{
	return true;
}
