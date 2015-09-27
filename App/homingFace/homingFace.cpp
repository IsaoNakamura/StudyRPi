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

#define GPIO_EXIT		(23)
#define GPIO_HALT		(22)

#define GPIO_MONOEYE	(4)


#include <cv.h>
#include <highgui.h>
#define CASCADE	("/usr/local/share/OpenCV/haarcascades/haarcascade_frontalface_default.xml")
#define	DISP_WIN	("faceOpenCV")
#define	WIN_WIDTH		(320.0)
#define	WIN_HEIGHT		(240.0)
#define	WIN_WIDTH_HALF	(WIN_WIDTH / 2.0)
#define	WIN_HEIGHT_HALF	(WIN_HEIGHT / 2.0)

#define USE_WIN				(0)
#define USE_TALK			(1)
#define USE_TALK_TEST		(1)
#define HOMING_DELAY_MSEC	(3000)
#define CENTER_AREA_RATIO	(0.6)
#define SERVO_OVER_MAX		(10)
#define NONFACE_CNT_MAX		(10)

#include <sys/time.h>

CvSize minsiz ={0,0};

#include "../../Lib/CamAngleConverter/CamAngleConverter.h"
#define ANGLE_DIAGONAL	(60.0)

#define DELAY_SEC	(1)

enum HomingStatus
{
	HOMING_NONE = 0,
	HOMING_HOMING,
	HOMING_DELAY,
	HOMING_CENTER,
	HOMING_KEEP
};

#define TALK_REASON_NUM	(12)
bool talkReason( const int& talkType)
{
	switch( talkType )
	{
	case 0:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"うれしなみだで　よくみえないや\" | aplay");
		break;
	case 1:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"きょうは めでたい\" | aplay");
		break;
	case 2:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"ふぅ しあわせすぎて ためいきがでる\" | aplay");
		break;
	case 3:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"きんちょうしてきた\" | aplay");
		break;
	case 4:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"しゅーへいくん うまく しゃべれるかな?\" | aplay");
		break;
	case 5:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"わたしは 商品開発課の なかむらによって 休みの合間を縫って開発されました\" | aplay");
		break;
	case 6:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"ペガサスあーーーーーーーい\" | aplay");
		break;
	case 7:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"うぉーーーく あーーーーーーーい\" | aplay");
		break;
	case 8:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"あいだっぷぅぅぅぅーー\" | aplay");
		break;
	case 9:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"われむぅぅぅぅーーーーーーー\" | aplay");
		break;
	case 10:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"がれむぅぅぅぅーーーーーーー\" | aplay");
		break;
	case 11:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"でぇーーさーーん　でぇーじいー\" | aplay");
		break;
	default:
		break;
	}
	return true;
}

#define TALK_WELCOME_NUM	(23)
bool talkWelcome( const int& talkType)
{
	switch( talkType )
	{
	case 0:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"うぇるかーむ\" | aplay");
		break;
	case 1:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"ようこそおいでくださいました\" | aplay");
		break;
	case 2:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"ゆっくりしていってね\" | aplay");
		break;
	case 3:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"きてくれて ありがとう\" | aplay");
		break;
	case 4:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60  \"しゅうへいと りさも よろこんでおります\" | aplay");
		break;
	case 5:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60  \"やまをめざそう\" | aplay");
		break;
	case 6:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60  \"あなべる へ ようこそ\" | aplay");
		break;
	case 7:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60  \"なまえはかきましたか？\" | aplay");
		break;
	case 8:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60  \"ちゃぺるまで ごあんないします うそです うごけません\" | aplay");
		break;
	case 9:
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60  \"めがあいましたね うふ\" | aplay");
		break;
	case 10:
		system("aplay /home/pi/shuheyVoice/00_7315651.wav");
		break;
	case 11:
		system("aplay /home/pi/shuheyVoice/01-_7315652.wav");
		break;
	case 12:
		system("aplay /home/pi/shuheyVoice/02-_7315653.wav");
		break;
	case 13:
		system("aplay /home/pi/shuheyVoice/03-_7315654.wav");
		break;
	case 14:
		system("aplay /home/pi/shuheyVoice/04-_7315655.wav");
		break;
	case 15:
		system("aplay /home/pi/shuheyVoice/05-_7315656.wav");
		break;
	case 16:
		system("aplay /home/pi/shuheyVoice/06-_7315657.wav");
		break;
	case 17:
		system("aplay /home/pi/shuheyVoice/07-_7315658.wav");
		break;
	case 18:
		system("aplay /home/pi/shuheyVoice/08-_7315659.wav");
		break;
	case 19:
		system("aplay /home/pi/shuheyVoice/99-_7315651.wav");
		break;
	case 20:
		system("aplay /home/pi/shuheyVoice/DouzoMinasama.wav");
		break;
	case 21:
		system("aplay /home/pi/shuheyVoice/Jitsuha.wav");
		break;
	case 22:
		system("aplay /home/pi/shuheyVoice/WelcomeSpeach.wav");
		break;
	default:
		break;
	}
	return true;
}

int main(int argc, char* argv[])
{
#if ( USE_TALK > 0 )
	system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"しゅうへいどろいど を きどうします\" | aplay");
#endif

#if ( USE_TALK_TEST > 0 )
	int i=0;
	for(i=0;i<TALK_REASON_NUM;i++){
		talkReason(i);
	}

	for(i=0;i<TALK_WELCOME_NUM;i++){
		talkWelcome(i);
	}
#endif

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
				
		pinMode(GPIO_EXIT, INPUT);
		pinMode(GPIO_HALT, INPUT);
		
		pinMode(GPIO_MONOEYE, OUTPUT);

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
		unsigned int msec = HOMING_DELAY_MSEC;
		gettimeofday(&stNow, NULL);
		stLen.tv_sec = msec / 1000;
		stLen.tv_usec = msec % 1000;
		timeradd(&stNow, &stLen, &stEnd);
		
		srand(stNow.tv_usec);

		// 前値保存用のサーボ角度
		int _servo_yaw		= servo_mid;
		int _servo_pitch	= servo_mid;

		// スクリーン中心らへんの範囲
		double center_area_x = 60.0;
		double center_area_y = 60.0;
		
		int homing_state = HOMING_NONE;

		int over_cnt = 0;
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
						printf("face(%f,%f) deg_yaw=%f deg_pitch=%f servo(%d,%d)\n",face_x,face_y,deg_yaw,deg_pitch,_servo_yaw,_servo_pitch);

						// サーボ値を入れる変数　初期値は前回の結果
						int servo_yaw	= _servo_yaw;
						int servo_pitch	= _servo_pitch;
				
						// ヨー角用サーボ制御
						//servo_yaw = servo_mid - static_cast<int>(deg_yaw / ratio_deg); // カメラ固定だとこれでよい
						servo_yaw = _servo_yaw  - static_cast<int>(deg_yaw / ratio_deg);
						if(servo_yaw > servo_max){
							over_cnt++;
							printf("yaw is over max. cnt=%d ######## \n", over_cnt);
						}else if(servo_yaw < servo_min){
							over_cnt++;
							printf("yaw is under min. cnt=%d ######## \n",over_cnt);
							servo_yaw = servo_min;
						}
						//printf("face_x=%f deg_yaw=%f servo_yaw=%d \n",face_x,deg_yaw,servo_yaw);
				
						// ピッチ角用サーボ制御
						//servo_pitch = servo_mid - static_cast<int>(deg_pitch / ratio_deg); // カメラ固定だとこれでよい
						servo_pitch = _servo_pitch - static_cast<int>(deg_pitch / ratio_deg);
						if(servo_pitch > servo_max){
							over_cnt++;
							printf("pitch is over max ######## \n");
							servo_pitch = servo_max;
						}else if(servo_pitch < servo_min){
							over_cnt++;
							printf("pitch is under min ######## \n");
							servo_pitch = servo_min;
						}
						//printf("pwmWrite(%d,%d,%f)\n",servo_yaw,servo_pitch,ratio_deg);

						bool isPwmWrite = false;
						
						// SERVO_OVER_MAXフレーム分の間、サーボ角度が最大が続くのであれば、サーボ角度を中間にもどす。
						if( over_cnt > SERVO_OVER_MAX){
							servo_yaw=servo_mid;
							servo_pitch=servo_mid;
							over_cnt = 0;
						}

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
				// NONFACE_CNT_MAXフレーム分の間、顔検出されなければ、サーボ角度を中間にもどす。
				nonface_cnt++;
				if( nonface_cnt > NONFACE_CNT_MAX ){
					nonface_cnt = 0;
					int servo_yaw = servo_mid;
					int servo_pitch = servo_mid;
					// サーボの角度設定
					printf("pwmWrite(GPIO_YAW, %d) for non-face. \n",servo_yaw);
					pwmWrite(GPIO_YAW, servo_yaw);
					//isPwmWrite = true;
					// 前値保存
					_servo_yaw = servo_yaw;

					// サーボの角度設定
					printf("pwmWrite(GPIO_PITCH, %d) for non-face. \n",servo_pitch);
					pwmWrite(GPIO_PITCH, servo_pitch);
					//isPwmWrite = true;
					// 前値保存
					_servo_pitch = servo_pitch;
				}
			}
			
			// ホーミング状態を更新
			if(homing_state != wrk_homing_state){
				int talkType = 0;
				switch( wrk_homing_state )
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
					//system("/home/pi/aquestalkpi/AquesTalkPi \"あ\" | aplay");
#endif
					break;
				case HOMING_DELAY:
					printf("[STATE] delay.\n");
					break;
				case HOMING_CENTER:
					printf("[STATE] face is center.\n");
#if ( USE_TALK > 0 )
					digitalWrite(GPIO_MONOEYE,HIGH);
					talkType = rand() % TALK_WELCOME_NUM;
					talkWelcome(talkType);
					digitalWrite(GPIO_MONOEYE,LOW);
#endif
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
			char c = cvWaitKey(DELAY_SEC);
			if( c==27 ){ // ESC-Key
				printf("exit program.\n");
#if ( USE_TALK > 0 )
				system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"ぷろぐらむを しゅうりょう します\" | aplay");
#endif
				break;
			}
			
			if( digitalRead(GPIO_EXIT) == LOW ){
				printf("exit program.\n");
#if ( USE_TALK > 0 )
				system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"ぷろぐらむを しゅうりょう します\" | aplay");
#endif
				break;
			}
			if( digitalRead(GPIO_HALT) == LOW ){
#if ( USE_TALK > 0 )
				system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"しすてむを しゃっとだうん します\" | aplay");
#endif
				printf("shutdown system.\n");
				system("sudo halt");
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

#if ( USE_TALK > 0 )
	system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"しゅうへいどろいど は 正常に終了しました\" | aplay");
#endif

		// ここまでくれば成功
		iRet = 0;
	}
	catch(...)
	{
#if ( USE_TALK > 0 )
		system("/home/pi/aquestalkpi/AquesTalkPi -g 60 \"しゅうへいどろいど に ちめいてきな エラーが 発生しました\" | aplay");
#endif
		iRet = -1;
	}

	return iRet;
}
