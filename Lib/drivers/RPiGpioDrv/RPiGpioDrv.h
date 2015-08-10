#ifndef _RPIGPIO_DRV_H_
#define _RPIGPIO_DRV_H_

#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <sys/mman.h>
#include <unistd.h>

//  ピン機能（BCM2835）
#define GPIO_INPUT    0x0       //  入力
#define GPIO_OUTPUT   0x1       //  出力
#define GPIO_ALT0     0x4
#define GPIO_ALT1     0x5
#define GPIO_ALT2     0x6
#define GPIO_ALT3     0x7
#define GPIO_ALT4     0x3
#define GPIO_ALT5     0x2

class RPiGpioDrv
{
private:
	RPiGpioDrv();
	virtual ~RPiGpioDrv();

public:
	// GPIO 初期化（最初に１度だけ呼び出すこと）
	static int init(const int& RPiVer=2);
	
	// ピンモードの設定
	//      pin : (P1) 2,3,4,7,8,9,10,11,14,15,17,18,22,23,24,25,27
	//            (P5) 28,29,30,31
	//      mode: GPIO_INPUT, _OUTPUT, _ALT0, _ALT1, _ALT2, _ALT3, _ALT4, _ALT5
	static int setPinMode(const int& pin, const int& mode);
	
	// ピンの出力を1(HighLevel:3.3V)/0(LowLevel:0.0V)に設定
	static int setOutLevel(const int& pin, const int& level);
	
	// ピンの状態を1(HighLevel:3.3V)/0(LowLevel:0.0V)として取得
	static int getLevel(const int& pin, int& level);
};

#endif
