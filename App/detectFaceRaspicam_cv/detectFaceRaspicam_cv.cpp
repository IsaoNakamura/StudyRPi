/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>

#include "RaspiCamCV.h"

#include <cv.h>
#include <highgui.h>

#define CASCADE	("/usr/local/share/OpenCV/haarcascades/haarcascade_frontalface_default.xml")
#define	DISP_WIN	("faceOpenCV")
#define	WIN_WIDTH	(320.0)
#define	WIN_HEIGHT	(240.0)
CvSize minsiz ={0,0};

#define DELAY_MSEC	(1)

int main(int argc, char* argv[])
{
	printf("Press Esc-Key to Exit Process.\n");

	RASPIVID_CONFIG * config = new RASPIVID_CONFIG();
	if(!config){
		printf("failed to create RASPIDVID_CONFIG.\n");
		return -1;
	}
	config->width=static_cast<int>(WIN_WIDTH);
	config->height=static_cast<int>(WIN_HEIGHT);
	config->bitrate=0;	// zero: leave as default
	config->framerate=0;
	config->monochrome=0;

	cvNamedWindow( DISP_WIN , CV_WINDOW_AUTOSIZE );
	RaspiCamCvCapture* capture = NULL;

	capture = raspiCamCvCreateCameraCapture2( 0, config );
	if(config){
		delete config;
		config = NULL;
	}
	if(!capture){
		printf("failed to create capture\n");
		return -1;
	}
	// キャプチャサイズを設定する．
	double w = WIN_WIDTH;
	double h = WIN_HEIGHT;
	raspiCamCvSetCaptureProperty (capture, RPI_CAP_PROP_FRAME_WIDTH, w);
	raspiCamCvSetCaptureProperty (capture, RPI_CAP_PROP_FRAME_HEIGHT, h);

	// 正面顔検出器の読み込み
	CvHaarClassifierCascade* cvHCC = (CvHaarClassifierCascade*)cvLoad(CASCADE, NULL,NULL,NULL);

	// 検出に必要なメモリストレージを用意する
	CvMemStorage* cvMStr = cvCreateMemStorage(0);

	while(1){
		IplImage* frame = raspiCamCvQueryFrame(capture);
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
		for(i = 0; i < face->total; i++) {
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
		}
		cvShowImage( DISP_WIN, frame);
		char c = cvWaitKey(DELAY_MSEC);
		if( c==27 ){ // ESC-Key
			break;
		}
		sleep(0);
	}

	// 用意したメモリストレージを解放
	cvReleaseMemStorage(&cvMStr);

	// カスケード識別器の解放
	cvReleaseHaarClassifierCascade(&cvHCC);

	raspiCamCvReleaseCapture(&capture);
	cvDestroyWindow(DISP_WIN);

	return 0;
}
