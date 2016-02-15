/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include <sys/time.h>

#include <wiringPi.h>

#define GPIO_A		(17)
#define GPIO_B		(18)

#define DEF_FORWARD_SEC   (10)  //30sec
#define DEF_BACKWARD_SEC  (10)  //30sec
#define DEF_INTERVAL_SEC   (5)  //180sec=3min
#define DEF_LOOP_NUM       (5)

enum DriveStatus
{
    DRIVE_FORWARD = 0,
    DRIVE_BACKWARD
};

int main(int argc, char* argv[])
{
	// WiringPiを初期化
	if( wiringPiSetupGpio() == -1 ){
		printf("failed to wiringPiSetupGpio()\n");
		return 1;
	}
	
	// GPIO_A を出力に設定
	pinMode(GPIO_A, OUTPUT);

	// GPIO_Bを入力に設定
	pinMode(GPIO_B, OUTPUT);
    
    struct timeval stNow;	// 現時刻取得用
    struct timeval stLen;	// 任意時間
    struct timeval stEnd;	// 現時刻から任意時間経過した時刻
    timerclear(&stNow);
    timerclear(&stLen);
    timerclear(&stEnd);
    
    int drive_state = DRIVE_FORWARD;
    int loop_cnt = 0;
    printf("forward(time=%d[s]) state(loop=%d).\n",DEF_FORWARD_SEC,loop_cnt);
    gettimeofday(&stNow, NULL);
    stLen.tv_sec = DEF_FORWARD_SEC;
    stLen.tv_usec = 0;
    timeradd(&stNow, &stLen, &stEnd);
    
    digitalWrite(GPIO_A, HIGH);
    digitalWrite(GPIO_B, LOW);
    


	while(1){ 
        // 現在時刻を取得
        gettimeofday(&stNow, NULL);
        if( timercmp(&stNow, &stEnd, >) ){
            // 任意時間経過
            
            // 任意時間インターバル
            // STOP
            printf("interval(time=%d[s]) state(loop=%d).\n",DEF_INTERVAL_SEC, loop_cnt);
            digitalWrite(GPIO_A, LOW);
            digitalWrite(GPIO_B, LOW);
            sleep(DEF_INTERVAL_SEC);
            
            timerclear(&stEnd);
            
            switch(drive_state)
            {
            case DRIVE_FORWARD:
                printf("change to backward(time=%d[s]) state(loop=%d).\n",DEF_BACKWARD_SEC, loop_cnt);
                drive_state = DRIVE_BACKWARD;
                stLen.tv_sec = DEF_BACKWARD_SEC;
                // BACKWARD
                digitalWrite(GPIO_A, LOW);
                digitalWrite(GPIO_B, HIGH);
                loop_cnt++;
                break;
                
            case DRIVE_BACKWARD:
                printf("change to forward(time=%d[s]) state(loop=%d).\n",DEF_FORWARD_SEC, loop_cnt);
                drive_state = DRIVE_FORWARD;
                stLen.tv_sec = DEF_FORWARD_SEC;    
                // FORWARD
                digitalWrite(GPIO_A, HIGH);
                digitalWrite(GPIO_B, LOW);
                break;
                
            default:
                break;
            }
            timeradd(&stNow, &stLen, &stEnd);   
        }
        
        if(loop_cnt>=DEF_LOOP_NUM){
            printf("exit loop.\n");
            break;
        }
	}
    
    digitalWrite(GPIO_A, LOW);
    digitalWrite(GPIO_B, LOW);

    printf("end process.\n");
	return 0;
}
