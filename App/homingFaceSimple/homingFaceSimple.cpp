/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>
#include <math.h>

#include <wiringPi.h>
#include "../../Lib/drivers/ServoDrv/CServoDrv.h"

#define GPIO_YAW		(12)	// PWM-Channel0 is on gpios 12 or 18.
#define GPIO_PITCH		(13)	// PWM-Channel1 is on gpios 13 or 19.

#define GPIO_EXIT		(23)

#define SERVO_MIN	(36)
#define SERVO_MID	(76)
#define SERVO_MAX	(122)
#define SERVO_RANGE	(180)

#include <cv.h>
#include <highgui.h>
#define CASCADE	("/usr/local/share/OpenCV/haarcascades/haarcascade_frontalface_default.xml")
#define	DISP_WIN	("faceOpenCV")
#define	WIN_WIDTH		(320.0)
#define	WIN_HEIGHT		(240.0)
#define	WIN_WIDTH_HALF	(WIN_WIDTH / 2.0)
#define	WIN_HEIGHT_HALF	(WIN_HEIGHT / 2.0)

#define USE_WIN				(0)
#define HOMING_DELAY_MSEC	(500)
#define CENTER_AREA_RATIO	(0.4)
#define NONFACE_CNT_MAX		(50)

#include <sys/time.h>

CvSize minsiz ={0,0};

#include "../../Lib/utilities/CamAngleConverter/CamAngleConverter.h"
#define ANGLE_DIAGONAL	(60.0)

#define DELAY_MSEC	(1)

enum HomingStatus
{
	HOMING_NONE = 0,
	HOMING_HOMING,
	HOMING_DELAY,
	HOMING_CENTER,
	HOMING_KEEP
};

int main(int argc, char* argv[])
{
	printf("Press Esc-Key to Exit Process.\n");
	int iRet = -1;

	CServoDrv* pServoYaw	= NULL;
	CServoDrv* pServoPitch	= NULL;

	try
	{
		// ready GPIO
		if(! CServoDrv::setupGpio() ){
			printf("failed to CServoDrv::setupGpio()\n");
			return 1;
		}
				
		pinMode(GPIO_EXIT, INPUT);

		const int servo_mid = SERVO_MID;
		const int servo_min = SERVO_MIN;
		const int servo_max = SERVO_MAX;
		const int servo_range = SERVO_RANGE;
		
		const int yaw_limit_max = servo_mid + 15;//91
		const int yaw_limit_min = servo_mid - 15;//61
		const int pitch_limit_max = servo_max - 22;//100
		const int pitch_limit_min = servo_min + 34;//70

		// ready Servo-Obj
		pServoYaw = CServoDrv::createInstance(	GPIO_YAW,
												servo_min,
												servo_max,
												servo_range	);
		if(!pServoYaw){
			printf("failed to create pServoYaw\n");
			throw 0;
		}
		if(!pServoYaw->setLimitAngleValue(yaw_limit_min,yaw_limit_max)){
			printf("failed to setLimitAngleValue pServoYaw\n");
			throw 0;
		}
		if(!pServoYaw->setMidAngleValue(servo_mid)){
			printf("failed to setLimitAngleValue() pServoYaw\n");
			throw 0;
		}

		pServoPitch = CServoDrv::createInstance(	GPIO_PITCH,
													servo_min,
													servo_max,
													servo_range	);
		if(!pServoPitch){
			printf("failed to create pServoPitch\n");
			throw 0;
		}
		if(!pServoPitch->setLimitAngleValue(pitch_limit_min,pitch_limit_max)){
			printf("failed to setLimitAngleValue pServoPitch\n");
			throw 0;
		}
		if(!pServoPitch->setMidAngleValue(servo_mid)){
			printf("failed to setLimitAngleValue() pServoPitch\n");
			throw 0;
		}
		
		// reflesh Servo-Angle.
		pServoYaw->refleshServo();
		pServoPitch->refleshServo();
	
#if ( USE_WIN > 0 )
		cvNamedWindow( DISP_WIN , CV_WINDOW_AUTOSIZE );
#endif
	
		CvCapture* capture = NULL;
		if (argc > 1){
			capture = cvCreateFileCapture( argv[1] );
		}else{
			capture = cvCreateCameraCapture( -1 );
			if(!capture){
				printf("failed to create capture\n");
				throw 0;
			}
			// キャプチャサイズを設定する．
			double w = WIN_WIDTH;
			double h = WIN_HEIGHT;
			cvSetCaptureProperty (capture, CV_CAP_PROP_FRAME_WIDTH, w);
			cvSetCaptureProperty (capture, CV_CAP_PROP_FRAME_HEIGHT, h);
		}
	
		// 正面顔検出器の読み込み
		CvHaarClassifierCascade* cvHCC = (CvHaarClassifierCascade*)cvLoad(CASCADE, NULL,NULL,NULL);
	
		// 検出に必要なメモリストレージを用意する
		CvMemStorage* cvMStr = cvCreateMemStorage(0);
		
		// スクリーン座標からカメラのピッチ角とヨー角を算出するオブジェクトを初期化
		DF::CamAngleConverter camAngCvt(	static_cast<int>(WIN_WIDTH),
											static_cast<int>(WIN_HEIGHT),
											ANGLE_DIAGONAL					);
		
		if (!camAngCvt.Initialized()) {
			printf("failed to initialize CamAngleConverter.\n");
			throw 0;
		}

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

		// スクリーン中心らへんの範囲
		double center_area_x = 60.0;
		double center_area_y = 60.0;
		
		int homing_state = HOMING_NONE;

		int nonface_cnt = 0;

		// メインループ
		while(1){
			int wrk_homing_state = HOMING_NONE;
			
			IplImage* frame = cvQueryFrame(capture);
			if(!frame){
				printf("failed to query frame.\n");
				break;
			}

			// 画像中から検出対象の情報を取得する
			CvSeq* face = cvHaarDetectObjects(	  frame
												, cvHCC
												, cvMStr
												, 1.2
												, 2
												, CV_HAAR_DO_CANNY_PRUNING
												, minsiz
												, minsiz
			);
			if(!face){
				printf("failed to detect objects.\n");
				break;
			}
	
			int i=0;
			//for(i = 0; i < face->total; i++) {
			if( face->total > 0 ){
				nonface_cnt = 0;
				i=0; // 最初のひとつの顔だけ追尾ターゲットにする
				
				// 検出情報から顔の位置情報を取得
				CvRect* faceRect = (CvRect*)cvGetSeqElem(face, i);
				if(!faceRect){
					printf("failed to get Face-Rect.\n");
					break;
				}
				center_area_x = faceRect->width / 2.0 * CENTER_AREA_RATIO;
				center_area_y = faceRect->height / 2.0 * CENTER_AREA_RATIO;
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
				double face_x = faceRect->x + (faceRect->width / 2.0);
				double face_y = faceRect->y + (faceRect->height / 2.0);

				if(	   face_x >= (WIN_WIDTH_HALF - center_area_x) && face_x <= (WIN_WIDTH_HALF + center_area_x)
					&& face_y >= (WIN_HEIGHT_HALF - center_area_y) && face_y <= (WIN_HEIGHT_HALF + center_area_y)	){
					wrk_homing_state = HOMING_CENTER;
				}else{
					// 顔がスクリーン中心らへんになければ処理を行う

					// 現在時刻を取得
					gettimeofday(&stNow, NULL);

					if( timercmp(&stNow, &stEnd, >) ){
						// 任意時間経てば処理を行う
						
						// スクリーン座標からカメラのピッチ・ヨー角を算出
						double deg_yaw		= 0.0;
						double deg_pitch	= 0.0;
						if( camAngCvt.ScreenToCameraAngle(deg_yaw, deg_pitch, face_x, face_y) != 0 ){
							continue;
						}

						bool isPwmWrite = false;
						
						// write to Servo.
						bool bIsWriteYaw = pServoYaw->writeAngleDegOffset(deg_yaw * -1.0);
						bool bIsWritePitch = pServoPitch->writeAngleDegOffset(deg_pitch * -1.0);
						
						if(bIsWriteYaw || bIsWritePitch){
							isPwmWrite = true;
						}

						if( isPwmWrite ){
							// サーボの値を設定したら現時刻から任意時間プラスして、サーボの角度設定しない終了時刻を更新
							timerclear(&stEnd);
							timeradd(&stNow, &stLen, &stEnd);
							wrk_homing_state = HOMING_HOMING;
						}else{
							wrk_homing_state = HOMING_KEEP;
						}

					}else{ // if( timercmp(&stNow, &stEnd, >) )
						wrk_homing_state = HOMING_DELAY;
					}
				} // if(face_x ... ){}else
			}else{ // if( face->total > 0 )
				wrk_homing_state = HOMING_NONE;
				// NONFACE_CNT_MAXフレーム分の間、顔検出されなければ、サーボ角度を中間にもどす。
				nonface_cnt++;

				if( nonface_cnt > NONFACE_CNT_MAX ){
					nonface_cnt = 0;

					// サーボの角度設定
					printf("reflesh Servo-Angle for non-face. \n");
					// reflesh Servo-Angle.
					pServoYaw->refleshServo();
					pServoPitch->refleshServo();

				}
			}
			
			// ホーミング状態を更新
			if(homing_state != wrk_homing_state){
				switch( wrk_homing_state )
				{
				case HOMING_NONE:
					printf("[STATE] no detected face.\n");
					break;
				case HOMING_HOMING:
					printf("[STATE] homing.\n");
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
				}
				homing_state = wrk_homing_state;
			}

#if ( USE_WIN > 0 )
			// 画面表示更新
			cvShowImage( DISP_WIN, frame);
#endif

			// 負荷分散のためDelay
			char c = cvWaitKey(DELAY_MSEC);
			if( c==27 ){ // ESC-Key
				printf("exit program.\n");
				break;
			}
			
			if( digitalRead(GPIO_EXIT) == LOW ){
				printf("exit program.\n");
				break;
			}
		} // while(1)
		
		// reflesh Servo-Angle.
		pServoYaw->refleshServo();
		pServoPitch->refleshServo();
		if(pServoYaw){
			delete pServoYaw;
			pServoYaw = NULL;
		}
		if(pServoPitch){
			delete pServoPitch;
			pServoPitch = NULL;
		}
	
		// 用意したメモリストレージを解放
		cvReleaseMemStorage(&cvMStr);
	
		// カスケード識別器の解放
		cvReleaseHaarClassifierCascade(&cvHCC);
	
		cvReleaseCapture(&capture);
		cvDestroyWindow(DISP_WIN);

		// ここまでくれば成功
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
		if(pServoYaw){
			delete pServoYaw;
			pServoYaw = NULL;
		}
		if(pServoPitch){
			delete pServoPitch;
			pServoPitch = NULL;
		}
	}

	return iRet;
}
