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


#include <highgui.h>
#define CASCADE	("/usr/local/share/OpenCV/haarcascades/haarcascade_frontalface_default.xml")
#define	DISP_WIN	("MassunDroid")
#define	WIN_WIDTH		(320.0)
#define	WIN_HEIGHT		(240.0)
#define	WIN_WIDTH_HALF	(WIN_WIDTH / 2.0)
#define	WIN_HEIGHT_HALF	(WIN_HEIGHT / 2.0)
CvSize minsiz ={0,0};

#include <sys/time.h>

#define KEEP_FILE           (1)
#define DISP_FB             (1) // 0:HDMI 1:PiTFT
#define DISP_FACE_SEC       (4)
#define USE_WIN				(0)
#define USE_TALK			(1)
#define USE_TALK_TEST		(0)
#define HOMING_DELAY_MSEC	(1000)
#define CENTER_AREA_RATIO	(1.2)
#define SERVO_OVER_MAX		(10)
#define NONFACE_CNT_MAX		(50)
#define SILENT_CNT			(30)
#define RECT_THICKNESS      (1)

#define ANGLE_DIAGONAL	(60.0)

#define DELAY_MSEC	(1)

#define SERVO_MID   (76)
#define SERVO_MIN   (36)
#define SERVO_MAX   (122)
#define SERVO_MIN_DEG   (0.0)
#define SERVO_MAX_DEG   (180.0)

#define SERVO_PITCH_LIMIT_MAX   (SERVO_MAX - 22)
#define SERVO_PITCH_LIMIT_MIN   (SERVO_MIN + 34)

#define CAMERA_ROTATE   (270)

#define TALK_REASON_NUM	(25)
bool CMassunDroid::talkReason( const int& talkType)
{
	printf("called talkReason(%d)\n",talkType);
	switch( talkType )
	{
	case 0:
		system("aplay /home/pi/MassunVoice/05.wav");
		break;
	case 1:
		system("aplay /home/pi/MassunVoice/06.wav");
		break;
	case 2:
		system("aplay /home/pi/MassunVoice/07.wav");
		break;
	case 3:
		system("aplay /home/pi/MassunVoice/08.wav");
		break;
	case 4:
		system("aplay /home/pi/MassunVoice/09.wav");
		break;
	case 5:
		system("aplay /home/pi/MassunVoice/11.wav");
		break;
	case 6:
		system("aplay /home/pi/MassunVoice/12.wav");
		break;
	case 7:
		system("aplay /home/pi/MassunVoice/13.wav");
		break;
	case 8:
		system("aplay /home/pi/MassunVoice/16.wav");
		break;
	case 9:
		system("aplay /home/pi/MassunVoice/17.wav");
		break;
	case 10:
		system("aplay /home/pi/MassunVoice/18.wav");
		break;
	case 11:
		system("aplay /home/pi/MassunVoice/20.wav");
		break;
	case 12:
		system("aplay /home/pi/MassunVoice/27.wav");
		break;
	case 13:
		system("aplay /home/pi/MassunVoice/28.wav");
		break;
	case 14:
		system("aplay /home/pi/MassunVoice/31.wav");
		break;
	case 15:
		system("aplay /home/pi/MassunVoice/32.wav");
		break;
	case 16:
		system("aplay /home/pi/MassunVoice/33.wav");
		break;
	case 17:
		system("aplay /home/pi/MassunVoice/34.wav");
		break;
	case 18:
		system("aplay /home/pi/MassunVoice/35.wav");
		break;
	case 19:
		system("aplay /home/pi/MassunVoice/36.wav");
		break;
	case 20:
		system("aplay /home/pi/MassunVoice/39.wav");
		break;
	case 21:
		system("aplay /home/pi/MassunVoice/40.wav");
		break;
	case 22:
		system("aplay /home/pi/MassunVoice/41.wav");
		break;
	case 23:
		system("aplay /home/pi/MassunVoice/42.wav");
		break;
	case 24:
		system("aplay /home/pi/MassunVoice/43.wav");
		break;
	default:
		break;
	}
	return true;
}

#define TALK_WELCOME_NUM	(19)
bool CMassunDroid::talkWelcome( const int& talkType)
{
	printf("called talkWelcome(%d)\n",talkType);
	switch( talkType )
	{
	case 0:
		system("aplay /home/pi/MassunVoice/01.wav");
		break;
	case 1:
		system("aplay /home/pi/MassunVoice/02.wav");
		break;
	case 2:
		system("aplay /home/pi/MassunVoice/03.wav");
		break;
	case 3:
		system("aplay /home/pi/MassunVoice/04.wav");
		break;
	case 4:
		system("aplay /home/pi/MassunVoice/10.wav");
		break;
	case 5:
		system("aplay /home/pi/MassunVoice/14.wav");
		break;
	case 6:
		system("aplay /home/pi/MassunVoice/15.wav");
		break;
	case 7:
		system("aplay /home/pi/MassunVoice/19.wav");
		break;
	case 8:
		system("aplay /home/pi/MassunVoice/21.wav");
		break;
	case 9:
		system("aplay /home/pi/MassunVoice/22.wav");
		break;
	case 10:
		system("aplay /home/pi/MassunVoice/23.wav");
		break;
	case 11:
		system("aplay /home/pi/MassunVoice/24.wav");
		break;
	case 12:
		system("aplay /home/pi/MassunVoice/25.wav");
		break;
	case 13:
		system("aplay /home/pi/MassunVoice/26.wav");
		break;
	case 14:
		system("aplay /home/pi/MassunVoice/29.wav");
		break;
	case 15:
		system("aplay /home/pi/MassunVoice/30.wav");
		break;
	case 16:
		system("aplay /home/pi/MassunVoice/37.wav");
		break;
	case 17:
		system("aplay /home/pi/MassunVoice/38.wav");
		break;
	case 18:
		system("aplay /home/pi/MassunVoice/44.wav");
		break;
	default:
		break;
	}
	return true;
}

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
    m_servo_yaw = SERVO_MID;
    m_servo_pitch = SERVO_MID;
    
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

int CMassunDroid::setup()
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
        
        servoResetMid();
        
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
        config->rotation=CAMERA_ROTATE;
        
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
        m_cvHCC = (CvHaarClassifierCascade*)cvLoad(CASCADE, NULL,NULL,NULL);
        if(!m_cvHCC){
            printf("failed to load CvHaarClassifierCascade.\n");
            throw 0;
        }
	
        // 検出に必要なメモリストレージを用意する
        m_cvMStr = cvCreateMemStorage(0);
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
        m_ratio_deg = ( m_servo_max_deg - m_servo_min_deg ) / ( m_servo_max - m_servo_min );

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
        #if ( USE_TALK_TEST > 0 )
	    testTalk();
        #endif

        #if ( USE_TALK > 0 )
        system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"まっすんどろいど を 起動します\" | aplay");
        #endif
        
		if(setup()!=0){
		    printf("failed to CMassunDroid::setup()\n");
		    throw 0;
		}
        #if ( USE_TALK > 0 )
        system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"まっすんどろいど は ウェルカムモード に 移行します\" | aplay");
        #endif
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
		printf("catch!! exec()\n");
		iRet = -1;
	}
	return iRet;
}

void CMassunDroid::testTalk()
{
	int i=0;
	for(i=0;i<TALK_REASON_NUM;i++){
		talkReason(i);
	}

	for(i=0;i<TALK_WELCOME_NUM;i++){
		talkWelcome(i);
	}
    return;
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
            HomingStatus wrk_homing_state = HOMING_NONE;
            // カメラ画像を取得
            IplImage* frame = raspiCamCvQueryFrame(m_capture);
			if(!frame){
				printf("failed to query frame.\n");
				throw 0;
			}
            // カメラ画像から顔を検出
            CvSeq* face = NULL;
            face = cvHaarDetectObjects(	  frame
                                        , m_cvHCC
                                        , m_cvMStr
                                        , 1.2
                                        , 2
                                        , CV_HAAR_DO_CANNY_PRUNING
                                        , minsiz
                                        , minsiz
            );
           // if(detectFace(face, frame)!=0){
           //     printf("failed to CMassunDroid::detectFace()\n");
           //     throw 0;
           // }
            if(!face){
	            printf("face is NULL\n");
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
                    
                    // フレーム画像を保存
                    if(wrk_homing_state!=m_homing_state){
                        #if ( KEEP_FILE > 0 )
                        saveFaceImage(frame, DISP_FB, DISP_FACE_SEC);
                        #else
                        cvSaveImage("/home/pi/face_image.jpg",frame);
                        printf("save face-image.\n");
                        // HDMI:  /dev/fb0
                        // PiTFT: /dev/fb1
                        system("sudo fbi -T 2 -d /dev/fb0 -t 2 -once -a /home/pi/face_image.jpg");
                        #endif
                    }
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
				m_nonface_cnt++;
                // NONFACE_CNT_MAXフレーム分の間、顔検出されなければ、サーボ角度を中間にもどす。
                if( m_nonface_cnt > NONFACE_CNT_MAX ){
                    m_nonface_cnt = 0;
                    servoResetMid();
                }
                m_silent_cnt++;
                if( m_silent_cnt > SILENT_CNT ){
                    m_silent_cnt = 0;
                    dispMassunImage(DISP_FB, DISP_FACE_SEC);
                    #if ( USE_TALK > 0 )
                    int talkType = rand() % TALK_REASON_NUM;
                    talkReason(talkType);
                    #endif
                }
            }
            if(homingAction(wrk_homing_state)!=0){
                printf("failed to CMassunDroid::homingAction()\n");
                throw 0;
            }
            if(updateHomingState(wrk_homing_state)!=0){
                printf("failed to CMassunDroid::updateHomingState()\n");
                throw 0;
            }
            if(updateView(frame)!=0){
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
		printf("catch!! at mainLoop()\n");
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
		servoResetMid();
        
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

int CMassunDroid::homingAction(const int& homing_state)
{
	int iRet = -1;
	try
	{
        if(m_homing_state != homing_state){
            int talkType = 0;
            switch( homing_state )
            {
            case HOMING_NONE:
                printf("[STATE] no detected face cont=%d.\n",m_silent_cnt);
                break;
            case HOMING_HOMING:
                printf("[STATE] homing.\n");
                // m_silent_cnt = 0;
                break;
            case HOMING_DELAY:
                printf("[STATE] delay.\n");
                // m_silent_cnt = 0;
                break;
            case HOMING_CENTER:
                printf("[STATE] face is center.\n");
                #if ( USE_TALK > 0 )
                talkType = rand() % TALK_WELCOME_NUM;
                talkWelcome(talkType);
                m_silent_cnt = 0;
                #endif
                break;
            case HOMING_KEEP:
                printf("[STATE] keep.\n");
                // m_silent_cnt = 0;
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


int CMassunDroid::updateHomingState(const HomingStatus& homing_state)
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

int CMassunDroid::updateView(const IplImage* frame)
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
        if(!m_cvHCC){
            printf("m_cvHCC is NULL.\n");
            throw 0;
        }
        if(!m_cvMStr){
            printf("m_cvMStris NULL.\n");
            
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
        char c = cvWaitKey(DELAY_MSEC);
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
        int i=0; // 最初のひとつの顔だけ追尾ターゲットにする
        
        // 検出情報から顔の位置情報を取得
        CvRect* faceRect = (CvRect*)cvGetSeqElem(face, i);
        if(!faceRect){
            printf("failed to get Face-Rect.\n");
            throw 0;
        }

        m_face_area_x = faceRect->width / 2.0 * CENTER_AREA_RATIO;
        m_face_area_y = faceRect->height / 2.0 * CENTER_AREA_RATIO;

        #if ( USE_WIN > 0 )
        // スクリーン中心らへん矩形描画を行う
        cvRectangle(	  frame
                        , cvPoint( (WIN_WIDTH_HALF - m_face_area_x), (WIN_HEIGHT_HALF - m_face_area_x) )
                        , cvPoint( (WIN_WIDTH_HALF + m_face_area_y), (WIN_HEIGHT_HALF +m_face_area_y) )
                        , CV_RGB(0, 255 ,0)
                        , RECT_THICKNESS
                        , CV_AA
                        , 0
        );
        #endif
        // 取得した顔の位置情報に基づき、矩形描画を行う
        cvRectangle(	  frame
                        , cvPoint(faceRect->x, faceRect->y)
                        , cvPoint(faceRect->x + faceRect->width, faceRect->y + faceRect->height)
                        , CV_RGB(255, 0 ,0)
                        , RECT_THICKNESS
                        , CV_AA
                        , 0
        );


        // 顔のスクリーン座標を算出
        m_face_scrn_x = faceRect->x + (faceRect->width / 2.0);
        m_face_scrn_y = faceRect->y + (faceRect->height / 2.0);

        iRet = 0;
    } // try
	catch(...)
	{
		printf("catch!! drawRectFace()\n");
		iRet = -1;
	}
	return iRet;
}

bool CMassunDroid::isInsideFaceCenter()
{
	if(	   m_face_scrn_x >= (WIN_WIDTH_HALF - m_face_area_x)
        && m_face_scrn_x <= (WIN_WIDTH_HALF + m_face_area_x)
		&& m_face_scrn_y >= (WIN_HEIGHT_HALF - m_face_area_y)
        && m_face_scrn_y <= (WIN_HEIGHT_HALF + m_face_area_y)	){
            return true;
	}
	return false;
}

int CMassunDroid::servoHomingFace()
{
    int iRet = -1;
    // スクリーン座標からカメラのピッチ・ヨー角を算出
    double deg_yaw		= 0.0;
    double deg_pitch	= 0.0;
    if( m_camAngCvt->ScreenToCameraAngle(deg_yaw, deg_pitch, m_face_scrn_x, m_face_scrn_y) != 0 ){
        return iRet;
    }
    // printf("face(%f,%f) deg_yaw=%f deg_pitch=%f servo(%d,%d)\n",m_face_scrn_x,m_face_scrn_y,deg_yaw,deg_pitch,m_servo_yaw,m_servo_pitch);

    // サーボ値を入れる変数　初期値は前回の結果
    int servo_yaw	= m_servo_yaw;
    int servo_pitch	= m_servo_pitch;

    // ヨー角用サーボ制御
    servo_yaw = m_servo_yaw  - static_cast<int>(deg_yaw / m_ratio_deg);
    if(servo_yaw > m_servo_max){
        m_over_cnt++;
        // printf("yaw is over max. cnt=%d ######## \n", m_over_cnt);
        servo_yaw = m_servo_max;
    }else if(servo_yaw < m_servo_min){
        m_over_cnt++;
        // printf("yaw is under min. cnt=%d ######## \n",m_over_cnt);
        servo_yaw = m_servo_min;
    }
    //printf("face_x=%f deg_yaw=%f servo_yaw=%d \n",face_x,deg_yaw,servo_yaw);

    // ピッチ角用サーボ制御
    servo_pitch = m_servo_pitch - static_cast<int>(deg_pitch / m_ratio_deg);
    if(servo_pitch > m_pitch_limit_max){
        m_over_cnt++;
        // printf("pitch is over max ######## \n");
        servo_pitch = m_pitch_limit_max;
    }else if(servo_pitch < m_pitch_limit_min){
        m_over_cnt++;
        // printf("pitch is under min ######## \n");
        servo_pitch = m_pitch_limit_min;
    }
    //printf("pwmWrite(%d,%d,%f)\n",servo_yaw,servo_pitch,ratio_deg);
    
    // SERVO_OVER_MAXフレーム分の間、サーボ角度が最大が続くのであれば、サーボ角度を中間にもどす。
    if( m_over_cnt > SERVO_OVER_MAX){
        servo_yaw=m_servo_mid;
        servo_pitch=m_servo_mid;
        m_over_cnt = 0;
    }
    iRet = 0;

    // 前回と同じサーボ値ならスキップ
    if(servo_yaw!=m_servo_yaw){
        // サーボの角度設定
        // printf("pwmWrite(GPIO_YAW, %d)\n",servo_yaw);
        pwmWrite(GPIO_YAW, servo_yaw);
        iRet = 1;
        // 前値保存
        m_servo_yaw = servo_yaw;
    }
    if(servo_pitch!=m_servo_pitch){
        // サーボの角度設定
        // printf("pwmWrite(GPIO_PITCH, %d)\n",servo_pitch);
        pwmWrite(GPIO_PITCH, servo_pitch);
        iRet = 1;
        // 前値保存
        m_servo_pitch = servo_pitch;
    }
	return iRet;
}

void CMassunDroid::servoResetMid()
{
    int servo_yaw	= SERVO_MID;
    int servo_pitch	= SERVO_MID;
    pwmWrite(GPIO_YAW, servo_yaw);
    pwmWrite(GPIO_PITCH, servo_pitch);
    m_servo_yaw = servo_yaw;
    m_servo_pitch = servo_pitch;
    return;
}

int CMassunDroid::saveFaceImage(const IplImage* frame, const int& fbNo, const int& dispTime)
{
	int iRet = -1;
	time_t timer=0;
	struct tm *t_st=NULL;
	char strFile[64]={0};

	try
	{
		time(&timer);
		//printf("%s\n",ctime(&timer));

		t_st=localtime(&timer);
		sprintf(strFile,
				"/home/pi/faceImage/face_%04d%02d%02d%02d%02d%02d.jpg"
				, t_st->tm_year+1900
				, t_st->tm_mon+1
				, t_st->tm_mday
				, t_st->tm_hour
				, t_st->tm_min
				, t_st->tm_sec
		);

        // save picture.
		printf("save file=%s.\n",strFile);
        cvSaveImage(strFile,frame);
        
        // disp picture.
        if( dispImage(strFile, fbNo, dispTime)!=0 ){
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

int CMassunDroid::dispImage(const char* filePath, const int& fbNo, const int& dispTime)
{
	int iRet = -1;
    char strCmd[128]={0};

	try
	{
        if(!filePath){
            printf("dispImage() filePath is NULL\n");
            throw 0;
        }
        // save picture.
		printf("disp file=%s.\n",filePath);
        
        // display picture.
        // HDMI:  /dev/fb0
        // PiTFT: /dev/fb1
        sprintf(strCmd,
                "sudo fbi -T 2 -d /dev/fb%d -t %d -once -a %s"
                , fbNo
                , dispTime
                , filePath
        );
        system(strCmd);

		//ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

#define MASSUN_IMG_NUM  (4)
int CMassunDroid::dispMassunImage(const int& fbNo, const int& dispTime)
{
	int iRet = -1;
    char strFile[64]={0};

	try
	{
        int imageType = rand() % MASSUN_IMG_NUM;
        
		sprintf(strFile,
				"/home/pi/massunImage/massun_%02d.jpg"
				, imageType
		);
        
        if(dispImage(strFile, fbNo, dispTime)!=0){
            printf("failed to dispImage(%s)\n",strFile);
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