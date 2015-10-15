#ifndef _C_SERVO_DRV_H_
#define _C_SERVO_DRV_H_

class CServoDrv
{
private:
	CServoDrv();
public:
	static CServoDrv* createInstance(	const int& gpioPin,
										const int& valueMin,
										const int& valueMax,
										const int& movementRange	);
	virtual ~CServoDrv();
private:
	void init();
	void destroy();

private:
	static bool calcRatioDeg2Value(	double&		ratioDeg2Value,
									const int&	valueMin,
									const int&	valueMax,
									const int&	movementRange	);

	static bool convDeg2Value(	int&			servo_value,
								const double&	deg,
								const double&	ratioDeg2Value	);

	static void convValue2Deg(	double&			deg,
								const int&		servo_value,
								const double&	ratioDeg2Value	);
private:
	bool flushServo();

public:
	bool setAngleDeg(const double& deg);
	bool setAngleDegOffset(const double& deg_offset);

	bool setAngleValue(const int& val);
	bool setAngleValueOffset(const int& val);

	bool writeAngleDeg(const double& deg);
	bool writeAngleDegOffset(const double& deg_offset);
	
	void resetAngle();
	void refleshServo();

	double	getAngleDeg() const;
	int		getAngleValue() const;

	bool setMidAngleValue(const int& val);
	
	bool setLimitAngleDeg(const double& degMinLimit, const double& degMaxLimit);
	bool setLimitAngleValue(const int& valueMinLimit, const int& valueMaxLimit);

private:
	// need for initialized
	int	m_gpioPin;
	int	m_pwmClock;	// 400
	int	m_pwmRange;	// 1024
	
	// spec of servoMotor
	int		m_valueMin;
	int		m_valueMax;
	int		m_valueMid;
	double	m_movementRange;	// 可動範囲(例：180度)
	int		m_valueMinLimit;
	int		m_valueMaxLimit;	
	double	m_ratioDeg2Value;
	
	int		m_valueCur;
	int		m_valuePre;
};

#endif
