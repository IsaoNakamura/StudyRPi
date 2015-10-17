#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include "CServoDrv.h"

#include <wiringPi.h>
#define DEF_PWM_CLOCK	(400)
#define DEF_PWM_RANGE	(1024)

#define GPIO_PWM_CH0_0	(12)
#define GPIO_PWM_CH0_1	(18)
#define GPIO_PWM_CH1_0	(13)
#define GPIO_PWM_CH1_1	(19)

#define DELAY_USEC	(100000)	// 100000usec = 100msec = 0.1sec
#define DELAY_MSEC	(100)

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
	pObj->m_valueMinLimit	= valueMin;
	pObj->m_valueMaxLimit	= valueMax;
	pObj->m_valueMid		= valueMin + (valueMax - valueMin)/2;
	pObj->m_movementRange	= movementRange;
	pObj->m_valueCur		= pObj->m_valueMid;
	
	if(! calcRatioDeg2Value(	pObj->m_ratioDeg2Value,
								pObj->m_valueMin,
								pObj->m_valueMax,
								pObj->m_movementRange	) ){
		delete pObj;
		return NULL;						
	}
	
	// using WiringPi.
	pinMode(pObj->m_gpioPin, PWM_OUTPUT);
	pwmSetMode(PWM_MODE_MS);
	pwmSetClock(DEF_PWM_CLOCK);
	pwmSetRange(DEF_PWM_RANGE);
		
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
	m_valueCur			= -1;
	m_valuePre			= -1;
}

void CServoDrv::destroy()
{
	return;
}

bool CServoDrv::calcRatioDeg2Value(	double&		ratioDeg2Value,
									const int&	valueMin,
									const int&	valueMax,
									const int&	movementRange	)
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
								const double&	ratioDeg2Value	)
{
	// 返答領域の初期化
	servo_value = 0;
	
	if( ratioDeg2Value <= 1.0e-6 ){ // need fabs?
		return false;
	}
	servo_value = static_cast<int>(deg / ratioDeg2Value);
	return true;
}

void CServoDrv::convValue2Deg(	double&			deg,
								const int&		servo_value,
								const double&	ratioDeg2Value	)
{
	// 返答領域の初期化
	deg = 0.0;
	
	deg = static_cast<double>(servo_value) * ratioDeg2Value;
	return;
}

bool CServoDrv::setAngleDeg(const double& deg)
{
	int dest_value = 0;
	if( !CServoDrv::convDeg2Value(dest_value, deg, m_ratioDeg2Value)){
		return false;
	}
	setAngleValue(dest_value);
	return true;
}

bool CServoDrv::setAngleDegOffset(const double& deg_offset)
{
	int dest_value = 0;
	if( !CServoDrv::convDeg2Value(dest_value, deg_offset, m_ratioDeg2Value)){
		return false;
	}
	setAngleValueOffset(dest_value);
	return true;
}

bool CServoDrv::setAngleValue(const int& val)
{
	int valueWrk = val;
	if(valueWrk < m_valueMinLimit){
		valueWrk = m_valueMinLimit;
	}
	else if(valueWrk > m_valueMaxLimit){
		valueWrk = m_valueMaxLimit;
	}
	m_valueCur = valueWrk;
	return true;
}

bool CServoDrv::setAngleValueOffset(const int& val)
{
	int valueWrk = m_valueCur + val;
	return setAngleValue(valueWrk);
}

bool CServoDrv::flushServo()
{
	if(m_valueCur < 0){
		return false;
	}

	if(m_valueCur == m_valuePre){
		return true;
	}

	// using WiringPi
	pwmWrite(m_gpioPin, m_valueCur);
	//usleep(DELAY_USEC);
	//delay(DELAY_MSEC); //msec
	usleep(0);
	m_valuePre = m_valueCur;
	return true;
}

bool CServoDrv::writeAngleDeg(const double& deg)
{
	if(! setAngleDeg(deg) ){
		return false;
	}
	flushServo();
	return true;
}

bool CServoDrv::writeAngleDegOffset(const double& deg_offset)
{
	if(! setAngleDegOffset(deg_offset) ){
		return false;
	}
	flushServo();
	return true;
}

void CServoDrv::resetAngle()
{
	m_valueCur = m_valueMid;
	return;
}

void CServoDrv::refleshServo()
{
	resetAngle();
	flushServo();
	return;
}

double CServoDrv::getAngleDeg() const
{
	double retDeg = 0.0;
	convValue2Deg(retDeg, m_valueCur, m_ratioDeg2Value);
	return retDeg;
}

int CServoDrv::getAngleValue() const
{
	return m_valueCur;
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

bool CServoDrv::setLimitAngleDeg(const double& degMinLimit, const double& degMaxLimit)
{
	int valueMinLimit = 0;
	int valueMaxLimit = 0;
	if( !CServoDrv::convDeg2Value(valueMinLimit, degMinLimit, m_ratioDeg2Value)){
		return false;
	}
	if( !CServoDrv::convDeg2Value(valueMaxLimit, degMaxLimit, m_ratioDeg2Value)){
		return false;
	}
	return setLimitAngleValue(valueMinLimit,valueMaxLimit);
}

bool CServoDrv::setLimitAngleValue(const int& valueMinLimit, const int& valueMaxLimit)
{
	if(valueMinLimit < m_valueMin || valueMinLimit > m_valueMax){
		return false;
	}
	if(valueMaxLimit > m_valueMax || valueMaxLimit < m_valueMin){
		return false;
	}
	m_valueMinLimit = valueMinLimit;
	m_valueMaxLimit = valueMaxLimit;
	return true;
}
