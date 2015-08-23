#ifndef _C_JOYSTICK_DRV_H_
#define _C_JOYSTICK_DRV_H_

#define JOY_DEV "/dev/input/js0"

#define BUTTON_OFF 0
#define BUTTON_ON 1

#define JOY_SELECT	0
#define JOY_START	3
#define JOY_PS		16
#define JOY_UP		4
#define JOY_RIGHT	5
#define JOY_DOWN	6
#define JOY_LEFT	7
#define JOY_SANKAKU	12
#define JOY_MARU	13
#define JOY_BATSU	14
#define JOY_SHIKAKU	15
#define JOY_LEFT2	8
#define JOY_LEFT1	10
#define JOY_RIGHT2	9
#define JOY_RIHGT1	11

struct stButtonState
{
	char pValue;
	int iCur;
	int iOld;
};

class CJoystickDrv
{
private:
	CJoystickDrv();
public:
	static CJoystickDrv* createInstance();
	virtual ~CJoystickDrv();
private:
	void init();
	void destroy();
private:
	int* m_pAxis;
	stButtonState* m_pButton;
	int m_hJoy;
	int m_iNumAxis;
	int m_iNumButton;
	void updateButtonState();
public:
	int getNumAxis() const { return m_iNumAxis; }
	int getNumButton() const { return m_iNumButton; }
public:
	int connectJoystick();
	int readJoystick();
	int getButtonState(const int& btn_idx) const;
	int getAxisState(const int& axis_idx) const;
};

#endif
