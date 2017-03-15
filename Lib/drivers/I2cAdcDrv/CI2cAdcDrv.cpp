/*
 * CI2cAdcDrv.cpp
 *
 *  Created on: 2017/03/15
 *      Author: isao
 */

#include "CI2cAdcDrv.h"

#include <stdio.h>
#include <unistd.h>

#include <linux/i2c-dev.h>
#include <fcntl.h>
#include <sys/ioctl.h>

#include <stdlib.h>

CI2cAdcDrv::CI2cAdcDrv() {
	// TODO 自動生成されたコンストラクター・スタブ
	initInstance();

}

CI2cAdcDrv::~CI2cAdcDrv() {
	// TODO Auto-generated destructor stub
	destroyInstance();
}

// static
CI2cAdcDrv* CI2cAdcDrv::createInstance(
	  int fd
	, int address/*=ADS1015_ADDRESS*/
	, unsigned int diff_threshold/*=5000*/
	, unsigned int continue_threshold/*=65535*/
)
{
	CI2cAdcDrv* pObj = NULL;

	try
	{
		if( fd==0 || address==0x00)
		{
			printf("Error: fd or Address is NULL \n");
			throw 0;
		}

		pObj = new CI2cAdcDrv;
		if(pObj==NULL)
		{
			throw 0;
		}

		pObj->m_fd = fd;
		pObj->m_address = address;
		pObj->m_diff_threshold = diff_threshold;
		pObj->m_continue_threshold = continue_threshold;

		if( pObj->startInstance()!=0 )
		{
			throw 0;
		}
	}
	catch(...)
	{
		if(pObj)
		{
			delete pObj;
			pObj=NULL;
		}
	}

	return pObj;
}

void CI2cAdcDrv::initInstance()
{
	m_fd = 0;
	m_address = 0;
	m_diff_threshold = 0;
	m_continue_threshold = 0;

	for(int i=0; i<CH_NUM; i++)
	{
		m_state[i].org_value = 0;
		m_state[i].diff_value = 0;
		m_state[i].btn_state = BUTTON_OFF;
		m_state[i].continue_off_cnt = 0;
		m_state[i].continue_on_cnt = 0;
		m_state[i].continue_cnt = 0;
		m_state[i].cnt_limit = CNT_LIMIT;
	}
}

void CI2cAdcDrv::destroyInstance()
{

}

int CI2cAdcDrv::startInstance()
{
	int iRet = -1;

	try
	{
		// デバイスの準備
		if(this->useDevice()!=0)
		{
			throw 0;
		}

		if(this->initAllChannelValue()!=0)
		{
			throw 0;
		}

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		printf("Catch: in CI2cAdcDrv::startInstance() \n");
		iRet = -1;
	}

	return iRet;
}

int CI2cAdcDrv::writeRegister(unsigned char reg, unsigned short value)
{
	int iRet = -1;
	unsigned char buf[3]={0};

	try
	{
		buf[0] = reg;
		buf[1] = (unsigned char)(value>>8);
		buf[2] = (unsigned char)(value & 0xFF);
		if (write(m_fd, buf, 3) != 3)
		{
			printf("Error writting to i2c slave1 in CI2cAdcDrv::writeData\n");
			throw 0;
		}

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}

	return iRet;
}

int CI2cAdcDrv::readRegister(unsigned char reg, unsigned short &value)
{
	int iRet = -1;
	unsigned char bufWrite[1]={0};
	unsigned char bufRead[2]={0};
	unsigned short dest = 0;

	try
	{
		bufWrite[0] = reg;
		//printf("write 0:%x\n",bufWrite[0]);
		if( write(m_fd, bufWrite,1) != 1)
		{
			printf("Error writting to i2c slave1\n");
			throw 0;
		}

		if (read(m_fd, bufRead, 2) != 2)
		{
			printf("Error reading to i2c slave1\n");
			throw 0;
		}
		//printf("read 0:%x 1:%x\n",bufRead[0],bufRead[1]);

		dest = (unsigned short)( (bufRead[0]<<8) | bufRead[1] );
		if(dest > 0x7FFF)
		{
			//printf("dest > 0x7FFF\n");
			//value = (unsigned short)(dest - 0xFFFF);
			value = dest;
		}
		else
		{
			value = dest;
		}
		//printf("readData=%d\n",dest);
		//printf("readData=%.6f\n",data*6144/32768.0);

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}

	return iRet;
}

int CI2cAdcDrv::readADC_SingledEnded(unsigned short &result, unsigned int channel)
{
	int iRet = -1;
	unsigned short config = 0;
	unsigned short dest = 0;

	try
	{
		if(channel >= CH_NUM)
		{
			throw 0;
		}

		config = 	ADS1015_REG_CONFIG_CQUE_NONE	|
					ADS1015_REG_CONFIG_CLAT_NONLAT	|
					ADS1015_REG_CONFIG_CPOL_ACTVLOW	|
					ADS1015_REG_CONFIG_CMODE_TRAD	|
					ADS1015_REG_CONFIG_DR_1600SPS	|
					ADS1015_REG_CONFIG_MODE_SINGLE	;

		config |= ADS1015_REG_CONFIG_PGA_6_144V;

		switch(channel)
		{
			case 0:
				config |= ADS1015_REG_CONFIG_MUX_SINGLE_0;
				break;
			case 1:
				config |= ADS1015_REG_CONFIG_MUX_SINGLE_1;
				break;
			case 2:
				config |= ADS1015_REG_CONFIG_MUX_SINGLE_2;
				break;
			case 3:
				config |= ADS1015_REG_CONFIG_MUX_SINGLE_3;
				break;
			default:
				break;
		}

		config |= ADS1015_REG_CONFIG_OS_SINGLE;

		if( writeRegister(ADS1015_REG_POINTER_CONFIG, config)!=0 )
		{
			throw 0;
		}

		usleep(ADS1115_CONVERSIONDELAY*1000);//8ミリ秒待つ

		if( readRegister(ADS1015_REG_POINTER_CONVERT, dest)!=0 )
		{
			throw 0;
		}

		//int pga=4096;
		if( dest > 0x7FFF)
		{
			result = (dest - 0xFFFF);
			//result = ( (dest - 0xFFFF) * pga / 32768.0 ) /1000;
		}
		else
		{
			result = dest;
			//result = ( dest * pga / 32768.0 ) /1000;
		}

		//ここまでくれば正常
		iRet = 0;

	}
	catch(...)
	{
		iRet = -1;
	}

	return iRet;
}

int CI2cAdcDrv::useDevice()
{
	int iRet = -1;
	try
	{
		// 通信先アドレスの設定。
		if (ioctl(m_fd, I2C_SLAVE, m_address) < 0)
		{
			printf("Error: Unable to get bus access to talk to slave\n");
			throw 0;
		}

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

//startInstanceでのみ呼ばれる
int CI2cAdcDrv::initAllChannelValue()
{
	int iRet = -1;
	try
	{
		for(int i=0; i<CH_NUM; i++){
			if(readADC_SingledEnded(m_state[i].org_value,i)!=0)
			{
				throw 0;
			}
		}

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

//static
int CI2cAdcDrv::countUpContinueCnt
(
		unsigned int&         continue_cnt
	,	const unsigned int    continue_threshold
)
{
	int iRet = -1;

	try
	{
		if(continue_cnt >= continue_threshold)
		{
			//ONカウントリセット
			continue_cnt = 0;
		}
		else
		{
			//ONカウントアップ
			continue_cnt++;
		}

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}

	return iRet;
}

int CI2cAdcDrv::updateChannelState(stChannelState &ret_state, unsigned int channel)
{
	int iRet = -1;
	unsigned short old_val=0;
	int wrk_btn_state = BUTTON_OFF;

	try
	{
		//現在の値を記録
		old_val = ret_state.org_value;

		//値を更新
		if(readADC_SingledEnded(ret_state.org_value,channel)!=0)
		{
			throw 0;
		}

		//差を求める
		ret_state.diff_value = old_val - ret_state.org_value;

		if( abs(ret_state.diff_value) > m_diff_threshold)
		{//しきい値より差が大きければ
			//状態をONにする
			//printf("wrk_state = ON \n");
			wrk_btn_state = BUTTON_ON;
		}
		else
		{//しきい値以下であれば
			//状態をOFFにする
			wrk_btn_state = BUTTON_OFF;
		}

		if(wrk_btn_state == BUTTON_ON)
		{//ONなら
			//ONカウントアップ
			countUpContinueCnt(ret_state.continue_on_cnt, m_continue_threshold);
			//printf("countUp ON \n");
			//OFFカウントリセット
			ret_state.continue_off_cnt = 0;
		}
		else
		{//OFFなら
			//OFFカウントアップ
			countUpContinueCnt(ret_state.continue_off_cnt, m_continue_threshold);
		}

		if(ret_state.continue_on_cnt >= ret_state.cnt_limit)
		{//ONカウントが限界を超えたら
			//ON
			ret_state.btn_state = BUTTON_ON;
			//ONカウントリセット
			ret_state.continue_on_cnt = 0;
		}
		else
		{
			//OFF
			ret_state.btn_state = BUTTON_OFF;
		}

		if(ret_state.continue_off_cnt > CNT_OFF_LIMIT)
		{
			ret_state.continue_on_cnt = 0;
		}

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CI2cAdcDrv::getChannelValue
(
		unsigned short		&ret_value
	,   int					&ret_state
	,   const unsigned int	channel
)
{
	int iRet = -1;
	try
	{
		if(channel >= CH_NUM)
		{
			throw 0;
		}

		switch(channel)
		{
		case CH_0:
			if( this->updateChannelState(m_state[CH_0], CH_0) != 0)
			{
				throw 0;
			}
			ret_value = m_state[CH_0].org_value;
			ret_state = m_state[CH_0].btn_state;
			if(ret_state == BUTTON_ON)
			{
				m_state[CH_1].continue_on_cnt = 0;
				m_state[CH_2].continue_on_cnt = 0;
				m_state[CH_3].continue_on_cnt = 0;
			}
			break;
		case CH_1:
			if( this->updateChannelState(m_state[CH_1], CH_1) != 0)
			{
				throw 0;
			}
			ret_value = m_state[CH_1].org_value;
			ret_state = m_state[CH_1].btn_state;
			if(ret_state == BUTTON_ON)
			{
				m_state[CH_0].continue_on_cnt = 0;
				m_state[CH_2].continue_on_cnt = 0;
				m_state[CH_3].continue_on_cnt = 0;
			}
			break;
		case CH_2:
			if( this->updateChannelState(m_state[CH_2], CH_2) != 0)
			{
				throw 0;
			}
			ret_value = m_state[CH_2].org_value;
			ret_state = m_state[CH_2].btn_state;
			if(ret_state == BUTTON_ON)
			{
				m_state[CH_0].continue_on_cnt = 0;
				m_state[CH_1].continue_on_cnt = 0;
				m_state[CH_3].continue_on_cnt = 0;
			}
			break;
		case CH_3:
			if( this->updateChannelState(m_state[CH_3], CH_3) != 0)
			{
				throw 0;
			}
			ret_value = m_state[CH_3].org_value;
			ret_state = m_state[CH_3].btn_state;
			if(ret_state == BUTTON_ON)
			{
				m_state[CH_0].continue_on_cnt = 0;
				m_state[CH_1].continue_on_cnt = 0;
				m_state[CH_2].continue_on_cnt = 0;
			}
			break;
		default:
			ret_value = 0;
			ret_state = BUTTON_OFF;
			break;
		}

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}

	return iRet;
}
