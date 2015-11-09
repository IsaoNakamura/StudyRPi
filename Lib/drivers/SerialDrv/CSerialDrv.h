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
#define SERIAL_PORT "/dev/ttyUSB0"
#else
#define SERIAL_PORT "/dev/ttyAMA0"
#endif //_USE_USB

#define SERIAL_BAUDRATE  B115200

class CSerialDrv {
private:
	CSerialDrv();
public:
	virtual ~CSerialDrv();
public:
	static CSerialDrv* createInstance(	const char*				serialPort=SERIAL_PORT,
										const unsigned char&	baudrate=SERIAL_BAUDRATE	);
;
public:
	int receiveData(unsigned char* receiveBuf, int& bufNum);
	int sendData(const unsigned char* sendBuf, const int& bufNum);

private:
	void init();
	void destroy();
	int startInstance(	const char*				serialPort,
						const unsigned char&	baudrate	);
private:
	int m_fd;
	int m_address;
};

#endif /* CSERIALDRV_H_ */
