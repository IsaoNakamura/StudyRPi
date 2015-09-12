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

CvSize minsiz ={0,0};

#include "../../Lib/CamAngleConverter/CamAngleConverter.h"
#define ANGLE_DIAGONAL	(60.0)

#define DELAY_SEC	(1)

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
		const int servo_mid = 73;
		const int servo_min = servo_mid - 15;
		const int servo_max = servo_mid + 15;
		const double servo_min_deg = 0.0;
		const double servo_max_deg = 180.0;
		const double ratio_deg = ( servo_max - servo_min ) / ( servo_max_deg - servo_min_deg );
	
		cvNamedWindow( DISP_WIN , CV_WINDOW_AUTOSIZE );
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
		
		int _servo_yaw		= servo_mid;
		int _servo_pitch	= servo_mid;
	
		while(1){
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
				// 取得した顔の位置情報に基づき、矩形描画を行う
				cvRectangle(	  frame
								, cvPoint(faceRect->x, faceRect->y)
								, cvPoint(faceRect->x + faceRect->width, faceRect->y + faceRect->height)
								, CV_RGB(255, 0 ,0)
								, 2
								, CV_AA
								, 0
				);
				
				// 顔のスクリーン座標を算出
				double face_x = faceRect->x + (faceRect->width / 2.0);
				double face_y = faceRect->y + (faceRect->height / 2.0);
				
				// スクリーン座標からカメラのピッチ・ヨー角を算出
				double deg_yaw		= 0.0;
				double deg_pitch	= 0.0;
				if( camAngCvt.ScreenToCameraAngle(deg_pitch, deg_yaw, face_x, face_y) != 0 ){
					continue;
				}
				printf("deg_yaw=%f deg_pitch=%f \n",deg_yaw,deg_pitch);
				
				// 前回と同じピッチ・ヨー角ならスキップ
				
				// サーボ値を入れる変数　初期値は前回の結果
				int servo_yaw	= _servo_yaw;
				int servo_pitch	= _servo_pitch;
				
				// ヨー角用サーボ制御
				servo_yaw = static_cast<int>(deg_yaw * ratio_deg);
				if(servo_yaw > servo_max){
					servo_yaw = servo_max;
				}else if(servo_yaw < servo_min){
					servo_yaw = servo_min;
				}
				
				// ピッチ角用サーボ制御
				servo_pitch = static_cast<int>(deg_pitch * ratio_deg);
				if(servo_pitch > servo_max){
					servo_pitch = servo_max;
				}else if(servo_pitch < servo_min){
					servo_pitch = servo_min;
				}
				
				// 前回と同じサーボ値ならスキップ
				if(servo_yaw!=_servo_yaw){
					// サーボの角度設定
					printf("pwmWrite(GPIO_YAW, %d)\n",servo_yaw);
					pwmWrite(GPIO_YAW, servo_yaw);
					// 前値保存
					_servo_yaw = servo_yaw;
				}
				if(servo_pitch!=_servo_pitch){
					// サーボの角度設定
					printf("pwmWrite(GPIO_PITCH, %d)\n",servo_pitch);
					pwmWrite(GPIO_PITCH, servo_pitch);
					// 前値保存
					_servo_pitch = servo_pitch;
				}
			}
			cvShowImage( DISP_WIN, frame);
			char c = cvWaitKey(DELAY_SEC);
			if( c==27 ){ // ESC-Key
				break;
			}
		}
		
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
