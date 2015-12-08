/*
 * CSerialDrv.h
 *
 *  Created on: 2015/11/09
 *      Author: isao
 */

#ifndef CSERIALDRV_H_
#define CSERIALDRV_H_

#define _USE_USB (1)
#if _USE_USB
#define DEF_SERIAL_PORT "/dev/ttyUSB0"
#else
#define DEF_SERIAL_PORT "/dev/ttyAMA0"
#endif //_USE_USB

#define DEF_SERIAL_BAUDRATE  (9600)

class CSerialDrv {
private:
	CSerialDrv();
public:
	virtual ~CSerialDrv();
public:
	static CSerialDrv* createInstance(	const char*	serialPort=DEF_SERIAL_PORT,
										const int&	baudrate=DEF_SERIAL_BAUDRATE	);
private:
	static bool convBaurate(unsigned long& cfalg_baudrate, const int& baudrate);

public:
	int receiveData(unsigned char* receiveBuf, int& bufNum);
	int sendData(const unsigned char* sendBuf, const int& bufNum);

private:
	void init();
	void destroy();
	int startInstance(	const char*	serialPort,
						const int&	baudrate	);
private:
	int m_fd;
	int m_address;
};

#endif /* CSERIALDRV_H_ */
