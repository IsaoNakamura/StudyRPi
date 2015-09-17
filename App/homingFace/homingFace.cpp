/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>
#include <math.h>

#include <wiringPi.h>

#define GPIO_YAW		(12)	// PWM-Channel0 is on gpios 12 or 18.
#define GPIO_PITCH		(13)	// PWM-Channel1 is on gpios 13 or 19.


#include <cv.h>
#include <highgui.h>
#define CASCADE	("/usr/local/share/OpenCV/haarcascades/haarcascade_frontalface_default.xml")
#define	DISP_WIN	("faceOpenCV")
#define	WIN_WIDTH		(320.0)
#define	WIN_HEIGHT		(240.0)
#define	WIN_WIDTH_HALF	(WIN_WIDTH / 2.0)
#define	WIN_HEIGHT_HALF	(WIN_HEIGHT / 2.0)
#define USE_WIN			(0)

#include <sys/time.h>

CvSize minsiz ={0,0};

#include "../../Lib/CamAngleConverter/CamAngleConverter.h"
#define ANGLE_DIAGONAL	(60.0)

#define DELAY_SEC	(1)

enum HomingStatus
{
	HOMING_NONE = 0,
	HOMING_HOMING,
	HOMING_DELAY.
	HOMING_CENTER,
	HOMING_KEEP
};

int main(int argc, char* argv[])
{
	printf("Press Esc-Key to Exit Process.\n");
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
		
		// servoMotor GWS park hpx min25 mid74 max123
		const int servo_mid = 76;
		const int servo_min = 36; //servo_mid - 30;
		const int servo_max = 122; //servo_mid + 30;
		const double servo_min_deg = 0.0;
		const double servo_max_deg = 180.0;
		const double ratio_deg = ( servo_max_deg - servo_min_deg ) / ( servo_max - servo_min );
	
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
		
		// サーボ角度を中間に設定
		pwmWrite(GPIO_YAW, servo_mid);
		pwmWrite(GPIO_PITCH, servo_mid);
		
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
		unsigned int msec = 1000;//3000;
		gettimeofday(&stNow, NULL);
		stLen.tv_sec = msec / 1000;
		stLen.tv_usec = msec % 1000;
		timeradd(&stNow, &stLen, &stEnd);

		// 前値保存用のサーボ角度
		int _servo_yaw		= servo_mid;
		int _servo_pitch	= servo_mid;

		// スクリーン中心らへんの範囲
		double center_area_x = 60.0;
		double center_area_y = 60.0;
		
		int homing_state = HOMING_NONE;

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
				i=0; // 最初のひとつの顔だけ追尾ターゲットにする
				
				// 検出情報から顔の位置情報を取得
				CvRect* faceRect = (CvRect*)cvGetSeqElem(face, i);
				if(!faceRect){
					printf("failed to get Face-Rect.\n");
					break;
				}
				center_area_x = faceRect->width / 2.0 * 0.6;//1.2;
				center_area_y = faceRect->height / 2.0 * 0.6;//1.2;
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
						double deg_yaw	= 0.0;
						double deg_pitch	= 0.0;
						if( camAngCvt.ScreenToCameraAngle(deg_yaw, deg_pitch, face_x, face_y) != 0 ){
							continue;
						}
						printf("face(%f,%f) deg_yaw=%f deg_pitch=%f \n",face_x,face_y,deg_yaw,deg_pitch);

						// サーボ値を入れる変数　初期値は前回の結果
						int servo_yaw	= _servo_yaw;
						int servo_pitch	= _servo_pitch;
				
						// ヨー角用サーボ制御
						//servo_yaw = servo_mid - static_cast<int>(deg_yaw / ratio_deg); // カメラ固定だとこれでよい
						servo_yaw = _servo_yaw  - static_cast<int>(deg_yaw / ratio_deg);
						if(servo_yaw > servo_max){
							printf("yaw is over max ######## \n");
							servo_yaw = servo_max;
						}else if(servo_yaw < servo_min){
							printf("yaw is under min ######## \n");
							servo_yaw = servo_min;
						}
						//printf("face_x=%f deg_yaw=%f servo_yaw=%d \n",face_x,deg_yaw,servo_yaw);
				
						// ピッチ角用サーボ制御
						//servo_pitch = servo_mid - static_cast<int>(deg_pitch / ratio_deg); // カメラ固定だとこれでよい
						servo_pitch = _servo_pitch - static_cast<int>(deg_pitch / ratio_deg);
						if(servo_pitch > servo_max){
							printf("pitch is over max ######## \n");
							servo_pitch = servo_max;
						}else if(servo_pitch < servo_min){
							printf("pitch is under min ######## \n");
							servo_pitch = servo_min;
						}
						//printf("pwmWrite(%d,%d,%f)\n",servo_yaw,servo_pitch,ratio_deg);

						bool isPwmWrite = false;
						// 前回と同じサーボ値ならスキップ
						if(servo_yaw!=_servo_yaw){
							// サーボの角度設定
							printf("pwmWrite(GPIO_YAW, %d)\n",servo_yaw);
							pwmWrite(GPIO_YAW, servo_yaw);
							isPwmWrite = true;
							// 前値保存
							_servo_yaw = servo_yaw;
						}
						if(servo_pitch!=_servo_pitch){
							// サーボの角度設定
							printf("pwmWrite(GPIO_PITCH, %d)\n",servo_pitch);
							pwmWrite(GPIO_PITCH, servo_pitch);
							isPwmWrite = true;
							// 前値保存
							_servo_pitch = servo_pitch;
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
			}
			
			// ホーミング状態を更新
			if(homing_state != wrk_homing_state){
				homing_state = wrk_homing_state;
				switch( homing_state )
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
			}

#if ( USE_WIN > 0 )
			// 画面表示更新
			cvShowImage( DISP_WIN, frame);
#endif

			// 負荷分散のためDelay
			char c = cvWaitKey(DELAY_SEC);
			if( c==27 ){ // ESC-Key
				break;
			}
		} // while(1)
		
		// サーボ角度を中間に設定
		pwmWrite(GPIO_YAW, servo_mid);
		pwmWrite(GPIO_PITCH, servo_mid);
	
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
	}

	return iRet;
}