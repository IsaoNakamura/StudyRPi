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
									const int&	movementRange	) const;

	static bool convDeg2Value(	int&			servo_value,
								const double&	deg,
								const double&	ratioDeg2Value	) const;

public:
	bool setAngleDeg(const double& deg);
	bool setAngleDegOffset(const double& deg_offset);
	bool resetAngle();
	bool setMidAngleValue(const int& val);
	bool setMidAngleDeg(const double& deg);


	double	getAngleDeg() const;
	int		getAngleValue() const;
	
	bool flushServo();
	bool refleshServo();
	bool writeAngleDeg(const double& deg);
	bool writeAngleDegOffset(const double& deg_offset);

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
};

#endif
