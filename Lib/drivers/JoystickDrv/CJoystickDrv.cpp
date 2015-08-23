#include <stdio.h>
#include <stdlib.h>

//for sleep()
#include <unistd.h>

#include <fcntl.h>

#include <linux/joystick.h>

#include "CJoystickDrv.h"

CJoystickDrv::CJoystickDrv()
{
	this->init();
}

CJoystickDrv::~CJoystickDrv()
{
	this->destroy();
}

CJoystickDrv* CJoystickDrv::createInstance()
{
	CJoystickDrv* pObj = NULL;
	pObj = new CJoystickDrv;
	if(!pObj){
		printf("failed to create CJoystickDrv's instance\n");
		return NULL;
	}
	return pObj;
}

void CJoystickDrv::init()
{
	this->destroy();
	m_pAxis = NULL;
	m_pButton = NULL;
	m_hJoy = -1;
	m_iNumAxis = 0;
	m_iNumButton = 0;
}

void CJoystickDrv::destroy()
{
	if(m_pAxis)
	{
		free(m_pAxis);
		m_pAxis = NULL;
	}
	if(m_pButton)
	{
		free(m_pButton);
		m_pButton = NULL;
	}

	if( m_hJoy >= 0 )
	{
		close(m_hJoy);
	}

	return;
}

int CJoystickDrv::connectJoystick()
{
	int iRet = -1;

	char name_of_joystick[80];
	int retry_max = 10;
	int retry_cnt = 0;
	int i=0;

	try
	{
		while(1)
		{
			m_hJoy = open( JOY_DEV, O_RDONLY);
			if( m_hJoy < 0 )
			{
				printf("Couldn't open joystick\n" );
				retry_cnt++;
				if(retry_cnt > retry_max)
				{
					iRet=-1;
					return iRet;
				}
				else
				{
					sleep(1);
				}
			}
			else
			{
				break;
			}
		}

		ioctl( m_hJoy, JSIOCGAXES, &m_iNumAxis );
		ioctl( m_hJoy, JSIOCGBUTTONS, &m_iNumButton );
		ioctl( m_hJoy, JSIOCGNAME(80), &name_of_joystick );

		m_pAxis = (int*)calloc(m_iNumAxis, sizeof( int ) );
		if(!m_pAxis)
		{
			printf("failed to create m_pAxis\n");
			throw 0;
		}

		m_pButton = (stButtonState*)calloc(m_iNumButton, sizeof( stButtonState ) );
		if(!m_pButton)
		{
			printf("failed to create m_pButton\n");
			throw 0;
		}
		
		printf("Joystick detected: %s\n\t%d axis\n\t%d buttons\n\n"
	 			, name_of_joystick
				, m_iNumAxis
				, m_iNumButton );

		fcntl( m_hJoy, F_SETFL, O_NONBLOCK );
		for(i=0;i<m_iNumButton;i++)
		{
			m_pButton[i].iCur = BUTTON_OFF;
			m_pButton[i].iOld = BUTTON_OFF;
		}
		for(i=0;i<m_iNumAxis;i++)
		{
			m_pAxis[i] = 0;
		}

		iRet=0;
	}
	catch(...)
	{
		iRet = -1;
		this->destroy();
	}

	return iRet;
}

int CJoystickDrv::readJoystick()
{
	int iRet = -1;
	struct js_event js;

	read(m_hJoy, &js, sizeof(js_event));
	switch( js.type & ~JS_EVENT_INIT)
	{
		case JS_EVENT_AXIS:
			if(m_pAxis)
			{
				m_pAxis[js.number] = js.value;
			}
			break;
		case JS_EVENT_BUTTON:
			if(m_pButton)
			{
				m_pButton[js.number].pValue = js.value;
			}
			break;
	}

	// ボタン状態更新
	updateButtonState();

	iRet=0;

	return iRet;
}

void CJoystickDrv::updateButtonState()
{
	if(!m_pButton)
	{
		return;
	}
	
	int btn_idx=0;
	for(btn_idx=0;btn_idx<m_iNumButton;btn_idx++)
	{
		if(m_pButton[btn_idx].pValue == BUTTON_ON)
		{
			if(m_pButton[btn_idx].iOld == BUTTON_ON)
			{
				m_pButton[btn_idx].iCur = BUTTON_OFF;
			}
			else
			{
				m_pButton[btn_idx].iCur = BUTTON_ON;
			}
			m_pButton[btn_idx].iOld = BUTTON_ON;
		}
		else
		{
			m_pButton[btn_idx].iCur = BUTTON_OFF;
			m_pButton[btn_idx].iOld = BUTTON_OFF;
		}
	}
}

int CJoystickDrv::getButtonState(const int& btn_idx) const
{
	if(!m_pButton)
	{
		return BUTTON_OFF;
	}
	
	if( btn_idx >= m_iNumButton )
	{
		return BUTTON_OFF;
	}
	
	return m_pButton[btn_idx].iCur;
}

int CJoystickDrv::getAxisState(const int& axis_idx) const
{
	if(!m_pAxis)
	{
		return 0;
	}
	
	if( axis_idx >= m_iNumAxis )
	{
		return 0;
	}
	
	return m_pAxis[axis_idx];
}
