#ifndef _RPIGPIO_DRV_H_
#define _RPIGPIO_DRV_H_

// RPi Version
#define RPI_VER_ONE	(1)
#define RPI_VER_TWO	(2)

// GPIO-Pin Mode
#define GPIO_INPUT	(0)	//  入力
#define GPIO_OUTPUT	(1)	//  出力
#define GPIO_PWM	(2) //  PWM

// PWM-Mode
#define PWM_MODE_MARKSPACE	(0)	// Using ServoMotor.
#define PWM_MODE_BALANCED	(1)	// Using LED.

#define GPIO_LV_LOW		(0)
#define GPIO_LV_HIGH	(1)

class RPiGpioDrv
{
private:
	RPiGpioDrv();
	virtual ~RPiGpioDrv();

public:
	// GPIO 初期化（最初に１度だけ呼び出すこと）
	static int init(const int& RPiVer=RPI_VER_TWO);
	
	// GPIOピンのモードの設定
	//		pin :	2,3,4,7,8,9,10,11,14,15,17,18,22,23,24,25,27,
	//				28,29,30,31
	//		mode:	GPIO_INPUT, GPIO_OUTPUT, GPIO_PWM
	static int setPinMode(const int& pin, const int& mode);
	
	// ピンの出力を1(HighLevel:3.3V)/0(LowLevel:0.0V)に設定
	static int setOutLevel(const int& pin, const int& level);
	
	// ピンの状態を1(HighLevel:3.3V)/0(LowLevel:0.0V)として取得
	static int getLevel(const int& pin, int& level);

	// 指定したマイクロ秒(1秒=1000000マイクロ秒)待つ
	static void delayMicroSec(const unsigned int& msec);
	
	// PWMモードを設定する
	// サーボモーターで使用するときは PWM_MODE_MARKSPACE を使うこと
	// LEDで使用するときは PWM_MODE_BALANCED を使うこと
	static int setPwmMode(const int& mode);
	
	// サーボモーターの仕様に合わせたrangeとclockを設定
	// SG90型のサーボモーターならrange1024,clock=400らしい
	static int setPwmRange(const unsigned int& range);
	static int setPwmClock(const int& clock);
	
	// サーボモータの回転角度パルス幅に合わせた値を設定する。
	// SG90型のサーボモーターならパルス幅0.5～2.4[ms]なので、
	// 19.2[MHz] / clock = 19.2[MHz] / 400 = 48[KHz]
	// 最小角度にするには 48[KHz] * 0.5[ms] = 24 を設定する
	// 最大角度にするには 48[KHz] * 2.4[ms] = 115 を設定する
	static int writePwmGpio(const int& pin, const int& val);

private:
	// 指定したマイクロ秒が100より小さい場合は無理やりループして止める
	static void delayMicroSecForce(const unsigned int& msec);
};

#endif
