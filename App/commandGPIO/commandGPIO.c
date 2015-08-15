#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#define EXEC_CNT (10)
#define DELAY_SEC (1)

int main(int argc, char* argv[])
{
	// GPIO17の使用開始
	system("echo \"17\" > /sys/class/gpio/export");

	// GPIO17を出力として設定する。
	system("echo \"out\" > /sys/class/gpio/gpio17/direction");

	// EXEC_CNT分、ONとOFFを繰り返す
	int i=0;
	for(i=0; i<EXEC_CNT; i++){
		if( (i%2) == 0 ){
			// 偶数の場合
			// GPIO17をON(Highレベル)にする。
			system("echo \"1\" > /sys/class/gpio/gpio17/value");
			printf("gpio17 is HighLevel\n");
		}else{ 
			// 奇数の場合
			// GPIO17をOFF(Lowレベル)にする。
			system("echo \"0\" > /sys/class/gpio/gpio17/value");
			printf("gpio17 is LowLevel\n");
		}

		// DELAY_TIME秒待つ
		sleep(DELAY_SEC);
	}

	// GPIO17の使用終了
	system("echo \"17\" > /sys/class/gpio/unexport");

	return 0;
}
