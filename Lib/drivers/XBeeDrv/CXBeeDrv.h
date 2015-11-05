/*
 * CXBeeDrv.h
 *
 *  Created on: 2014/05/21
 *      Author: isao
 */

#ifndef CXBEEDRV_H_
#define CXBEEDRV_H_

class CXBeeDrv {
private:
	CXBeeDrv();
public:
	virtual ~CXBeeDrv();
public:
	static CXBeeDrv* createInstance();
	int mainLoop();
private:
	void _initInstance();
	void _destroyInstance();
	int _startInstance();
private:
	int m_fd;
	int m_address;
};

#endif /* CXBEEDRV_H_ */
