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
#define	DISP_WIN	("MassunDroid")
#define	WIN_WIDTH		(320.0)
#define	WIN_HEIGHT		(240.0)
#define	WIN_WIDTH_HALF	(WIN_WIDTH / 2.0)
#define	WIN_HEIGHT_HALF	(WIN_HEIGHT / 2.0)
CvSize minsiz ={0,0};

#define USE_WIN				(0)
#define USE_TALK			(1)
#define USE_TALK_TEST		(0)
#define HOMING_DELAY_MSEC	(3000)
#define CENTER_AREA_RATIO	(0.8)
#define SERVO_OVER_MAX		(10)
#define NONFACE_CNT_MAX		(50)
#define SILENT_CNT			(30)

#define ANGLE_DIAGONAL	(60.0)

#define SERVO_MID   (76)
#define SERVO_MIN   (36)
#define SERVO_MAX   (122)
#define SERVO_MIN_DEG   (0.0)
#define SERVO_MAX_DEG   (180.0)

#define SERVO_PITCH_LIMIT_MAX   (SERVO_MAX - 22)
#define SERVO_PITCH_LIMIT_MIN   (SERVO_MIN + 34)

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
    m_homing_state = HOMING_NONE;
    m_gpioPitch = GPIO_PITCH;
    m_gpioYaw = GPIO_YAW;
    m_gpioExit = GPIO_EXIT;
    m_gpioHalt = GPIO_HALT;
    m_servo_mid = SERVO_MID;
    m_servo_min = SERVO_MIN;
    m_servo_max = SERVO_MAX;
    m_servo_min_deg = SERVO_MIN_DEG;
    m_servo_max_deg = SERVO_MAX_DEG;
    m_ratio_deg = 0.0;
    m_pitch_limit_max = SERVO_PITCH_LIMIT_MAX;
    m_pitch_limit_min = SERVO_PITCH_LIMIT_MIN;
    m_width_win = WIN_WIDTH;
    m_height_win = WIN_HEIGHT;
    m_capture = NULL;
    m_cvHCC = NULL;
    m_cvMStr = NULL;
    m_camAngCvt = NULL;
    m_over_cnt = 0;
    m_nonface_cnt = 0;
	m_silent_cnt = 0;
    m_exit = 0;
    m_face_area_x = 0.0;
    m_face_area_y = 0.0;
    m_face_scrn_x = 0.0;
    m_face_scrn_y = 0.0;
    
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
		CvHaarClassifierCascade* m_cvHCC = (CvHaarClassifierCascade*)cvLoad(CASCADE, NULL,NULL,NULL);
        if(!m_cvHCC){
            printf("failed to load CvHaarClassifierCascade.\n");
            throw 0;
        }
	
		// 検出に必要なメモリストレージを用意する
		CvMemStorage* m_cvMStr = cvCreateMemStorage(0);
        if(!m_cvMStr){
            printf("failed to create CvMemStorage.\n");
            throw 0;
        }
        
		iRet = 0;
	}
	catch(...)
	{
        printf("failed to CMassunDroid::setupCv().\n");
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
        if(setUp()!=0){
            printf("failed to CMassunDroid::setUp()\n");
            throw 0;
        }
        if(mainLoop()!=0){
            printf("failed to CMassunDroid::mainLoop()\n");
            throw 0;
        }
        if(finalize()!=0){
            printf("failed to CMassunDroid::finalize()\n");
            throw 0;
        }
        if(exitAction()!=0){
            printf("failed to CMassunDroid::exitAction()\n");
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

int CMassunDroid::mainLoop()
{
	int iRet = -1;
	try
	{
		struct timeval stNow;	// 現時刻取得用
		struct timeval stLen;	// 任意時間
		struct timeval stEnd;	// 現時刻から任意時間経過した時刻
		timerclear(&stNow);
		timerclear(&stLen);
		timerclear(&stEnd);
		unsigned int msec = HOMING_DELAY_MSEC;
		gettimeofday(&stNow, NULL);
		stLen.tv_sec = msec / 1000;
		stLen.tv_usec = msec % 1000;
		timeradd(&stNow, &stLen, &stEnd);
        
        srand(stNow.tv_usec);
        
        while(1){
            int wrk_homing_state = HOMING_NONE;
            // カメラ画像を取得
            IplImage* frame = raspiCamCvQueryFrame(m_capture);
			if(!frame){
				printf("failed to query frame.\n");
				throw 0;
			}
            
            // カメラ画像から顔を検出
            CvSeq* face = NULL;
            if(detectFace(face, frame)!=0){
                printf("failed to CMassunDroid::detectFace()\n");
                throw 0;
            }
            
            if( face->total > 0 ){
                m_nonface_cnt = 0;
                // 顔に矩形を描画
                if(drawRectFace(frame, face)!=0){
                    throw 0;
                }
                
                if( isInsideFaceCenter() ){
                    wrk_homing_state = HOMING_CENTER;
                }else{
                    // 現在時刻を取得
					gettimeofday(&stNow, NULL);
                    if( timercmp(&stNow, &stEnd, >) ){
                        // 任意時間経てば処理を行う
                        // サーボでカメラを顔に向ける
                        if( servoHomingFace() > 0 ){
							// サーボの値を設定したら現時刻から任意時間プラスして、サーボの角度設定しない終了時刻を更新
							timerclear(&stEnd);
							timeradd(&stNow, &stLen, &stEnd);
							wrk_homing_state = HOMING_HOMING;
                        }else{
                            wrk_homing_state = HOMING_KEEP;
                        }
                    }else{
                        wrk_homing_state = HOMING_DELAY;
                    }
                }
            }else{
                wrk_homing_state = HOMING_NONE;
                // NONFACE_CNT_MAXフレーム分の間、顔検出されなければ、サーボ角度を中間にもどす。
				m_nonface_cnt++;
				m_silent_cnt++;
            }
            
            if(voiceAction(wrk_homing_state)!=0){
                printf("failed to CMassunDroid::voiceAction()\n");
                throw 0;
            }
            if(updateHomingState(wrk_homing_state)!=0){
                printf("failed to CMassunDroid::updateHomingState()\n");
                throw 0;
            }
            if(updateView()!=0){
                throw 0;
            }
            
            if(keyAction()!=0){
                throw 0;
            }
            if(m_exit>0){
                break;
            }
        }
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
        // サーボ角度を中間に設定
		pwmWrite(m_gpioYaw, m_servo_mid);
		pwmWrite(m_gpioPitch, m_servo_mid);
        
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
		// 用意したメモリストレージを解放
        if(m_cvMStr){
    		cvReleaseMemStorage(&m_cvMStr);
            m_cvMStr = NULL;
        }
	
		// カスケード識別器の解放
        if(m_cvHCC){
		    cvReleaseHaarClassifierCascade(&m_cvHCC);
            m_cvHCC = NULL;
        }
	
        if(m_capture){
    		raspiCamCvReleaseCapture(&m_capture);
            m_capture = NULL;
        }
        
		cvDestroyWindow(DISP_WIN);
        
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::voiceAction(const int& homing_state)
{
	int iRet = -1;
	try
	{
        if(m_homing_state != homing_state){
            int talkType = 0;
            switch( homing_state )
            {
            case HOMING_NONE:
                printf("[STATE] no detected face.\n");
                #if ( USE_TALK > 0 )
                if( homing_state != HOMING_DELAY ){
                    digitalWrite(GPIO_MONOEYE,HIGH);
                    talkType = rand() % TALK_REASON_NUM;
                    talkReason(talkType);
                    digitalWrite(GPIO_MONOEYE,LOW);
                }
                #endif
                break;
            case HOMING_HOMING:
                printf("[STATE] homing.\n");
                #if ( USE_TALK > 0 )
                talkType = rand() % TALK_WELCOME_NUM;
                talkWelcome(talkType);
                m_silent_cnt = 0;
                #endif
                break;
            case HOMING_DELAY:
                printf("[STATE] delay.\n");
                break;
            case HOMING_CENTER:
                printf("[STATE] face is center.\n");
                break;
            case HOMING_KEEP:
                printf("[STATE] keep.\n");
                break;
            default:
                break;
            } // switch( homing_state )
        } // if(m_homing_state != homing_state)
        iRet = 0;
    } // try
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}


int CMassunDroid::updateHomingState(const int& homing_state)
{
	int iRet = -1;
	try
	{
        // ホーミング状態を更新
        if(m_homing_state != homing_state){
            m_homing_state = homing_state;
        }
        iRet = 0;
    } // try
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::updateView()
{
	int iRet = -1;
	try
	{
        #if ( USE_WIN > 0 )
        // 画面表示更新
        cvShowImage( DISP_WIN, frame);
        #endif
        
        iRet = 0;
    } // try
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::detectFace(CvSeq* face, const IplImage* frame)
{
	int iRet = -1;
	try
	{
        if(!frame){
            printf("frame is NULL.\n");
            throw 0;
        }
        // 画像中から検出対象の情報を取得する
        face = cvHaarDetectObjects(	  frame
                                    , m_cvHCC
                                    , m_cvMStr
                                    , 1.2
                                    , 2
                                    , CV_HAAR_DO_CANNY_PRUNING
                                    , minsiz
                                    , minsiz
        );
        if(!face){
            printf("failed to detect objects.\n");
            throw 0;
        }
        iRet = 0;
    } // try
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::exitAction()
{
	int iRet = -1;
	try
	{
        #if ( USE_TALK > 0 )
        system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"まっすんどろいど は 正常に終了しました\" | aplay");
        #endif
        
        if( m_exit==1 || m_exit==2 ){
            printf("exit program.\n");
            #if ( USE_TALK > 0 )
			system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"ぷろぐらむを しゅうりょう します\" | aplay");
            #endif
        }else if(m_exit==3){
            printf("shutdown system.\n");
            #if ( USE_TALK > 0 )
			system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"しすてむを しゃっとだうん します\" | aplay");
            #endif
			system("sudo halt");
        }
        iRet = 0;
    } // try
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::keyAction()
{
	int iRet = -1;
	try
	{
        // 負荷分散のためDelay
        char c = cvWaitKey(DELAY_SEC);
        if( c==27 ){ // ESC-Key
            m_exit = 1;
        }
        if( digitalRead(GPIO_EXIT) == LOW ){
            m_exit = 2;
        }
        if( digitalRead(GPIO_HALT) == LOW ){
            m_exit = 3;
        }

        iRet = 0;
    } // try
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CMassunDroid::drawRectFace(IplImage* frame, const CvSeq* face)
{
	int iRet = -1;
	try
	{
        i=0; // 最初のひとつの顔だけ追尾ターゲットにする
        
        // 検出情報から顔の位置情報を取得
        CvRect* faceRect = (CvRect*)cvGetSeqElem(face, i);
        if(!faceRect){
            printf("failed to get Face-Rect.\n");
            break;
        }
        m_face_area_x = faceRect->width / 2.0 * CENTER_AREA_RATIO;
        m_face_area_y = faceRect->height / 2.0 * CENTER_AREA_RATIO;
        #if ( USE_WIN > 0 )
        // スクリーン中心らへん矩形描画を行う
        cvRectangle(	  frame
                        , cvPoint( (WIN_WIDTH_HALF - center_area_x), (WIN_HEIGHT_HALF - center_area_x) )
                        , cvPoint( (WIN_WIDTH_HALF + center_area_y), (WIN_HEIGHT_HALF +center_area_y) )
                        , CV_RGB(0, 255 ,0)
                        , 2
                        , CV_AA
                        , 0
        );
        // 取得した顔の位置情報に基づき、矩形描画を行う
        cvRectangle(	  frame
                        , cvPoint(faceRect->x, faceRect->y)
                        , cvPoint(faceRect->x + faceRect->width, faceRect->y + faceRect->height)
                        , CV_RGB(255, 0 ,0)
                        , 2
                        , CV_AA
                        , 0
        );
        #endif

        // 顔のスクリーン座標を算出
        m_face_scrn_x = faceRect->x + (faceRect->width / 2.0);
        m_face_scrn_y = faceRect->y + (faceRect->height / 2.0);
        
        iRet = 0;
    } // try
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}