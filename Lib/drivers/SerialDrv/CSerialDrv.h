/*
 * CSerialDrv.h
 *
 *  Created on: 2015/11/09
 *      Author: isao
 */

#ifndef CSERIALDRV_H_
#define CSERIALDRV_H_

class CSeriallDrv {
private:
	CSeriallDrv();
public:
	virtual ~CSeriallDrv();
public:
	static CSeriallDrv* createInstance();
public:
	int receiveData(unsigned char* receiveBuf, int& bufNum);
	int sendData(const unsigned char* sendBuf, const int& bufNum);

private:
	void init();
	void destroy();
	int startInstance();
private:
	int m_fd;
	int m_address;
};

#endif /* CSERIALDRV_H_ */
