/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>
#include <math.h>

#include <wiringPi.h>

#define GPIO_PITCH		(12)
#define GPIO_YAW		(18)

#include <cv.h>
#include <highgui.h>
#define CASCADE	("/usr/local/share/OpenCV/haarcascades/haarcascade_frontalface_default.xml")
#define	DISP_WIN	("faceOpenCV")
#define	WIN_WIDTH		(320.0)
#define	WIN_HEIGHT		(240.0)
#define	WIN_WIDTH_HALF	(WIN_WIDTH / 2.0)
#define	WIN_HEIGHT_HALF	(WIN_HEIGHT / 2.0)

CvSize minsiz ={0,0};

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
				
				double face_x = faceRect->x + (faceRect->width / 2.0);
				double face_y = faceRect->y + (faceRect->height / 2.0);
				
				int yaw		= servo_mid;
				int pitch 	= servo_mid;
				
				// ヨー角用サーボ制御
				if( face_x == WIN_WIDTH_HALF ){
					// 中間値
					yaw = servo_mid;
				}else if( face_x < WIN_WIDTH_HALF){
					double rate = fabs( face_x / WIN_WIDTH_HALF );
					int delta = (int)( (double)( servo_mid - servo_min) * rate );
					yaw = servo_mid - delta;
					if(yaw < servo_min){
						yaw = servo_min;
					}
				}else if( face_x > WIN_WIDTH_HALF){
					double rate = fabs( (face_x - WIN_WIDTH_HALF) / WIN_WIDTH_HALF );
					int delta = (int)( (double)( servo_max - servo_mid ) * rate );
					yaw = servo_mid + delta;
					if(yaw > servo_max){
						yaw = servo_max;
					}
				}
				
				// ピッチ角用サーボ制御
				if( face_y == WIN_HEIGHT_HALF ){
					// 中間値
					pitch = servo_mid;
				}else if( face_y < WIN_HEIGHT_HALF){
					double rate = fabs( face_y / WIN_HEIGHT_HALF );
					int delta = (int)( (double)( servo_mid - servo_min) * rate );
					pitch = servo_mid - delta;
					if(pitch < servo_min){
						pitch = servo_min;
					}
				}else if( face_y > WIN_HEIGHT_HALF){
					double rate = fabs( (face_y - WIN_HEIGHT_HALF) / WIN_HEIGHT_HALF );
					int delta = (int)( (double)( servo_max - servo_mid ) * rate );
					pitch = servo_mid + delta;
					if(pitch > servo_max){
						pitch = servo_max;
					}
				}
				
				// サーボの角度設定
				pwmWrite(GPIO_YAW, yaw);
				pwmWrite(GPIO_PITCH, pitch);
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
