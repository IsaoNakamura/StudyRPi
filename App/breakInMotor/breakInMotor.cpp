/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include <sys/time.h>
#include <iostream>
#include <sstream>
#include <string>

#include <wiringPi.h>

#define GPIO_A		(17)
#define GPIO_B		(18)

#define DEF_FORWARD_SEC   (30)  //30sec
#define DEF_BACKWARD_SEC  (30)  //30sec
#define DEF_INTERVAL_SEC (180)  //180sec=3min
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
    
    int forward_sec     = DEF_FORWARD_SEC;
    int backward_sec    = DEF_BACKWARD_SEC;
    int interval_sec    = DEF_INTERVAL_SEC;
    int loop_num        = DEF_LOOP_NUM;
    std::cout << "input forward sec(DEF:" << forward_sec << "):";
    {
        std::string input;
        std::getline( std::cin, input);
        if( !input.empty() ){
            std::istringstream stream(input);
            stream >> forward_sec;
        }
    }

    std::cout << "input backward sec(DEF:" << backward_sec << "):";
    {
        std::string input;
        std::getline( std::cin, input);
        if( !input.empty() ){
            std::istringstream stream(input);
            stream >> backward_sec;
        }
    }

    std::cout << "input interval sec(DEF:" << interval_sec << "):";
    {
        std::string input;
        std::getline( std::cin, input);
        if( !input.empty() ){
            std::istringstream stream(input);
            stream >> interval_sec;
        }
    }

    std::cout << "input loop num(DEF:" << loop_num << "):";
    {
        std::string input;
        std::getline( std::cin, input);
        if( !input.empty() ){
            std::istringstream stream(input);
            stream >> loop_num;
        }
    }
    
    std::cout << "forward:" << forward_sec << "[sec]" << std::endl;
    std::cout << "backward:" << backward_sec << "[sec]" << std::endl;
    std::cout << "interval:" << interval_sec << "[sec]" << std::endl;
    std::cout << "loop:" << loop_num << "[times]" << std::endl;
    
    int drive_state = DRIVE_FORWARD;
    int loop_cnt = 0;
    printf("forward(time=%d[s]) state(loop=%d).\n",forward_sec,loop_cnt);
    gettimeofday(&stNow, NULL);
    stLen.tv_sec = forward_sec;
    stLen.tv_usec = 0;
    timeradd(&stNow, &stLen, &stEnd);
    
    digitalWrite(GPIO_A, HIGH);
    digitalWrite(GPIO_B, LOW);

	while(1){ 
        // 現在時刻を取得
        gettimeofday(&stNow, NULL);
        if( timercmp(&stNow, &stEnd, >) ){
            // 任意時間経過
            if(loop_cnt>=loop_num){
                printf("exit loop.\n");
                break;
            }
            
            // 任意時間インターバル
            // STOP
            printf("interval(time=%d[s]) state(loop=%d).\n",interval_sec, loop_cnt);
            digitalWrite(GPIO_A, LOW);
            digitalWrite(GPIO_B, LOW);
            sleep(interval_sec);

            switch(drive_state)
            {
            case DRIVE_FORWARD:
                printf("backward(time=%d[s]) state(loop=%d).\n",backward_sec, loop_cnt);
                drive_state = DRIVE_BACKWARD;
                stLen.tv_sec = backward_sec;
                // BACKWARD
                digitalWrite(GPIO_A, LOW);
                digitalWrite(GPIO_B, HIGH);
                loop_cnt++;
                break;
                
            case DRIVE_BACKWARD:
                printf("forward(time=%d[s]) state(loop=%d).\n",forward_sec, loop_cnt);
                drive_state = DRIVE_FORWARD;
                stLen.tv_sec = forward_sec;    
                // FORWARD
                digitalWrite(GPIO_A, HIGH);
                digitalWrite(GPIO_B, LOW);
                break;
                
            default:
                break;
            }
            timerclear(&stEnd);
            gettimeofday(&stNow, NULL);
            timeradd(&stNow, &stLen, &stEnd);
        }
	}
    
    digitalWrite(GPIO_A, LOW);
    digitalWrite(GPIO_B, LOW);

    printf("end process.\n");
	return 0;
}
