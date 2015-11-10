/*
 * CXBeeDrv.h
 *
 *  Created on: 2014/05/21
 *      Author: isao
 */

#ifndef CXBEEDRV_H_
#define CXBEEDRV_H_

class CSerialDrv;

class CXBeeDrv {
private:
	CXBeeDrv();
public:
	virtual ~CXBeeDrv();
public:
	static CXBeeDrv* createInstance(	const char*	serialPort,
										const int&	baudrate	);
	int mainLoop();
public:
	int receiveData(unsigned char* receiveBuf, int& bufNum);
	int sendData(const unsigned char* sendBuf, const int& bufNum);

private:
	void init();
	void destroy();
	int startInstance(	const char*	serialPort
						const int&	baudrate	);
private:
	CSerialDrv* m_pSerial;
};

#endif /* CXBEEDRV_H_ */
