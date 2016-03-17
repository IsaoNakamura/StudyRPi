/////////////////////////////////////////////
// this class is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>

#include "CMassunDroid.h"

#include <wiringPi.h>
#define GPIO_YAW		(12)	// PWM-Channel0 is on gpios 12 or 18.
#define GPIO_PITCH		(13)	// PWM-Channel1 is on gpios 13 or 19.
#define GPIO_EXIT		(23)
#define GPIO_HALT		(22)

#include "RaspiCamCV.h"
#include <cv.h>
#include <highgui.h>
#define CASCADE	("/usr/local/share/OpenCV/haarcascades/haarcascade_frontalface_default.xml")
#define	DISP_WIN	("faceOpenCV")
#define	WIN_WIDTH		(320.0)
#define	WIN_HEIGHT		(240.0)
#define	WIN_WIDTH_HALF	(WIN_WIDTH / 2.0)
#define	WIN_HEIGHT_HALF	(WIN_HEIGHT / 2.0)
CvSize minsiz ={0,0};

#define USE_WIN				(0)

#include "../../Lib/utilities/CamAngleConverter/CamAngleConverter.h"
#define ANGLE_DIAGONAL	(60.0)

CMassunDroid::CMassunDroid() {
	// TODO 自動生成されたコンストラクター・スタブ
	init();

}

CMassunDroid::~CMassunDroid() {
	// TODO Auto-generated destructor stub
	destroy();
}

CMassunDroid* CMassunDroid::createInstance()
{
	CMassunDroid* pObj = NULL;
	try
	{
		pObj = new CMassunDroid();
		if(!pObj)
		{
			throw 0;
		}

		if(pObj->startInstance()!=0)
		{
			throw 0;
		}
	}
	catch(...)
	{
		if(pObj)
		{
			delete pObj;
			pObj = NULL;
		}
	}

	return pObj;
}


void CMassunDroid::init()
{
	return;
}

void CMassunDroid::destroy()
{
	return;
}


int CMassunDroid::startInstance()
{
	int iRet = -1;
	try
	{
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::setUp()
{
	int iRet = -1;
	try
	{
        if( setupGpio()!=0 ){
            throw 0;
        }
        if( setupCv()!=0 ){
            throw 0;
        }
        if ( setupCamAngCvt()!=0 ){
            throw 0;
        }
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::setupGpio()
{
	int iRet = -1;
	try
	{
        // ready GPIO
        if( wiringPiSetupGpio() == -1 ){
            printf("failed to wiringPiSetupGpio()\n");
            throw 0;
        }
        
        // ready PWM
        pinMode(GPIO_PITCH, PWM_OUTPUT);
        pinMode(GPIO_YAW, PWM_OUTPUT);
        pwmSetMode(PWM_MODE_MS);
        pwmSetClock(400);
        pwmSetRange(1024);
        
		pinMode(GPIO_EXIT, INPUT);
		pinMode(GPIO_HALT, INPUT);
        
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::setupCv()
{
	int iRet = -1;
	try
	{
        RASPIVID_CONFIG * config = new RASPIVID_CONFIG();
        if(!config){
            printf("failed to create RASPIDVID_CONFIG.\n");
            throw 0;
        }
        config->width=static_cast<int>(WIN_WIDTH);
        config->height=static_cast<int>(WIN_HEIGHT);
        config->bitrate=0;	// zero: leave as default
        config->framerate=0;
        config->monochrome=0;
        
#if ( USE_WIN > 0 )
		cvNamedWindow( DISP_WIN , CV_WINDOW_AUTOSIZE );
#endif
        m_capture = raspiCamCvCreateCameraCapture2( 0, config );
        if(config){
            delete config;
            config = NULL;
        }
        if(!m_capture){
            printf("failed to create capture\n");
            throw 0;
        }
        // キャプチャサイズを設定する．
        double w = WIN_WIDTH;
        double h = WIN_HEIGHT;
        raspiCamCvSetCaptureProperty (m_capture, RPI_CAP_PROP_FRAME_WIDTH, w);
        raspiCamCvSetCaptureProperty (m_capture, RPI_CAP_PROP_FRAME_HEIGHT, h);
	
		// 正面顔検出器の読み込み
		CvHaarClassifierCascade* cvHCC = (CvHaarClassifierCascade*)cvLoad(CASCADE, NULL,NULL,NULL);
	
		// 検出に必要なメモリストレージを用意する
		CvMemStorage* cvMStr = cvCreateMemStorage(0);
        
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}
int CMassunDroid::setupCamAngCvt()
{
	int iRet = -1;
	try
	{
        // スクリーン座標からカメラのピッチ角とヨー角を算出するオブジェクトを初期化
        m_camAngCvt = new DF::CamAngleConverter(	static_cast<int>(WIN_WIDTH),
									           		static_cast<int>(WIN_HEIGHT),
									           		ANGLE_DIAGONAL					);

	    if(!m_camAngCvt){
            throw 0;
        }
		
		if (!m_camAngCvt->Initialized()) {
			printf("failed to initialize CamAngleConverter.\n");
			throw 0;
		}
		iRet = 0;
	}
	catch(...)
	{
	    if(m_camAngCvt){
            delete m_camAngCvt;
            m_camAngCvt = NULL;
        }
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::exec()
{
	int iRet = -1;
	try
	{
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::finalize()
{
	int iRet = -1;
	try
	{
        int iServoRet = finalizeServo();
        if(iServoRet!=0){
            printf("failed to finalizeServo()\n");
        }
        int iCvRet = finalizeCv();
        if(iCvRet!=0){
            printf("failed to finalizeCv()\n");
        }
		if( (iServoRet + iCvRet) == 0 ){
            iRet = 0;
        }
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::finalizeServo()
{
	int iRet = -1;
	try
	{
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::finalizeCv()
{
	int iRet = -1;
	try
	{
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}