/*
 * LinearTransformation.h
 *
 *  Created on: 2015/12/08
 *      Author: isao
 */

#ifndef CMASSUNDROID_H_
#define CMASSUNDROID_H_

class RaspiCamCvCapture;
class CvHaarClassifierCascade;
class CvMemStorage;

class DF::CamAngleConverter;

class CMassunDroid {
private:
	CMassunDroid();
public:
	virtual ~CMassunDroid();
public:
	static CMassunDroid* createInstance();

private:
	void init();
	void destroy();
	int startInstance();
   
public:
    int setup();
    int exec();
    int finalize();

private:
    int setupGpio();
    int setupCv();
    int setupCamAngCvt();
    int finalizeServo();
    int finalizeCv();

private:
    int m_gpioPitch;
    int m_gpioYaw;
    int m_gpioExit;
    int m_gpioHalt;
    int m_servo_mid;
    int m_servo_min;
    int m_servo_max;
    double m_servo_min_deg;
    double m_servo_max_deg;
    double m_ratio_deg;
    int m_pitch_limit_max;
    int m_pitch_limit_min;
    int m_width_win;
    int m_height_win;
    
    // for OpenCV
    RaspiCamCvCapture*          m_capture;
    CvHaarClassifierCascade*    m_cvHCC;
    CvMemStorage*               m_cvMStr;

    // for CamAngleConverter
    DF::CamAngleConverter*      m_camAngCvt;
};

#endif /* CMASSUNDROID_H_ */
