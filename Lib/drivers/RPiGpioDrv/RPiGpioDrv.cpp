#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <sys/mman.h>
#include <unistd.h>
#include <time.h>
#include <sys/time.h>

#include "RPiGpioDrv.h"

//  レジスタブロックの物理アドレス
#define PERI_BASE_RPI_ONE	(0x20000000)	// for RPi1(BCM2708)
#define PERI_BASE_RPI_TWO	(0x3F000000)	// for RPi2(BCM2835)

#define GPIO_BASE_OFFSET	(0x00200000)
#define PWM_BASE_OFFSET		(0x0020C000)
#define CLOCK_BASE_OFFSET	(0x00101000)

#define BLOCK_SIZE			(4096)

#define GPIO_PIN_MAX		(31)
#define GPIO_PIN_MIN		( 0)

#define	BCM_PASSWORD	(0x5A000000)

//	Clock regsiter offsets
#define	PWMCLK_CNTL	(40)
#define	PWMCLK_DIV	(41)

// FSEL select bits
#define FSEL_INPUT	(0b000)	// 0x0
#define FSEL_OUTPUT	(0b001)	// 0x1
#define FSEL_ALT0	(0b100)	// 0x4
#define FSEL_ALT1	(0b101)	// 0x5
#define FSEL_ALT2	(0b110)	// 0x6
#define FSEL_ALT3	(0b111)	// 0x7
#define FSEL_ALT4	(0b011)	// 0x3
#define FSEL_ALT5	(0b010)	// 0x2

// PWM
//	Word offsets into the PWM control region
#define	PWM_CONTROL	(0)
#define	PWM_STATUS 	(1)
#define	PWM0_RANGE 	(4)
#define	PWM0_DATA	(5)
#define	PWM1_RANGE	(8)
#define	PWM1_DATA	(9)

#define	PWM0_MS_MODE	(0x0080)	// Run in MS mode
#define	PWM0_USEFIFO	(0x0020)	// Data from FIFO
#define	PWM0_REVPOLAR	(0x0010)	// Reverse polarity
#define	PWM0_OFFSTATE	(0x0008)	// Ouput Off state
#define	PWM0_REPEATFF	(0x0004)	// Repeat last value if FIFO empty
#define	PWM0_SERIAL		(0x0002)	// Run in serial mode
#define	PWM0_ENABLE		(0x0001)	// Channel Enable

#define	PWM1_MS_MODE	(0x0080)	// Run in MS mode
#define	PWM1_USEFIFO	(0x0020)	// Data from FIFO
#define	PWM1_REVPOLAR	(0x0010)	// Reverse polarity
#define	PWM1_OFFSTATE	(0x0008)	// Ouput Off state
#define	PWM1_REPEATFF	(0x0004)	// Repeat last value if FIFO empty
#define	PWM1_SERIAL		(0x0002)	// Run in serial mode
#define	PWM1_ENABLE		(0x0001)	// Channel Enable

// GPIOレジスタ (volatile:実メモリに必ずアクセス)
static volatile unsigned int *g_pGpio	= NULL;
static volatile unsigned int *g_pPwm	= NULL;
static volatile unsigned int *g_pClock	= NULL;

// gpioToPwmALT
//	the ALT value to put a GPIO pin into PWM mode
static unsigned char gpioToPwmALT[] =
{
          0,         0,         0,         0,         0,         0,         0,         0,	//  0 ->  7
          0,         0,         0,         0, FSEL_ALT0, FSEL_ALT0,         0,         0, 	//  8 -> 15
          0,         0, FSEL_ALT5, FSEL_ALT5,         0,         0,         0,         0, 	// 16 -> 23
          0,         0,         0,         0,         0,         0,         0,         0,	// 24 -> 31
          0,         0,         0,         0,         0,         0,         0,         0,	// 32 -> 39
  FSEL_ALT0, FSEL_ALT0,         0,         0,         0, FSEL_ALT0,         0,         0,	// 40 -> 47
          0,         0,         0,         0,         0,         0,         0,         0,	// 48 -> 55
          0,         0,         0,         0,         0,         0,         0,         0,	// 56 -> 63
} ;

// gpioToPwmPort
//	The port value to put a GPIO pin into PWM mode
static unsigned char gpioToPwmPort[] =
{
          0,         0,         0,         0,         0,         0,         0,         0,	//  0 ->  7
          0,         0,         0,         0, PWM0_DATA, PWM1_DATA,         0,         0, 	//  8 -> 15
          0,         0, PWM0_DATA, PWM1_DATA,         0,         0,         0,         0, 	// 16 -> 23
          0,         0,         0,         0,         0,         0,         0,         0,	// 24 -> 31
          0,         0,         0,         0,         0,         0,         0,         0,	// 32 -> 39
  PWM0_DATA, PWM1_DATA,         0,         0,         0, PWM1_DATA,         0,         0,	// 40 -> 47
          0,         0,         0,         0,         0,         0,         0,         0,	// 48 -> 55
          0,         0,         0,         0,         0,         0,         0,         0,	// 56 -> 63

} ;

RPiGpioDrv::RPiGpioDrv()
{
}

RPiGpioDrv::~RPiGpioDrv()
{
}

// 初期化
int RPiGpioDrv::init(const int& RPiVer/*=RPI_VER_TWO*/)
{
	int iRet = -1;
	int fd = -1;
	try
	{
		//  レジスタブロックの物理アドレス
		unsigned int gpio_base		= 0x0;
		unsigned int pwm_base		= 0x0;
		unsigned int clock_base	 	= 0x0;
		if(RPiVer == RPI_VER_ONE){
			gpio_base	= PERI_BASE_RPI_ONE + GPIO_BASE_OFFSET;
			pwm_base	= PERI_BASE_RPI_ONE + PWM_BASE_OFFSET;
			clock_base	= PERI_BASE_RPI_ONE + CLOCK_BASE_OFFSET;
		}else if(RPiVer == RPI_VER_TWO){
			gpio_base	= PERI_BASE_RPI_TWO + GPIO_BASE_OFFSET;
			pwm_base	= PERI_BASE_RPI_TWO + PWM_BASE_OFFSET;
			clock_base	= PERI_BASE_RPI_TWO + CLOCK_BASE_OFFSET;
		}else{
			printf("@RPiGpioDrv::init() RPi's Version(%d) is not supported\n",RPiVer);
			throw 0;
		}

		//  /dev/memを開く（要sudo）
		fd = open("/dev/mem", O_RDWR | O_SYNC | O_CLOEXEC );
		if(fd < 0) {
			printf("@RPiGpioDrv::init() cannot open /dev/mem\n");
			throw 0;
		}

		//  GPIO初期化
		if(!g_pGpio){
			//  mmap で GPIO（物理メモリ）を gpio_map（仮想メモリ）に紐づける
			void* gpio_map = NULL;
			gpio_map = mmap(	  NULL
								, BLOCK_SIZE
								, PROT_READ | PROT_WRITE
								, MAP_SHARED
								, fd
								, gpio_base
			);
			if((int)gpio_map == -1){
				printf("@RPiGpioDrv::init() cannot mmap GPIO\n");
				throw 0;
			}
		
			g_pGpio = (unsigned int *)gpio_map;
		}
		
		/*
		// PWM初期化
		if(!g_pPwm){
			void* pwm_map = NULL;
			pwm_map = mmap(	  NULL
							, BLOCK_SIZE
							, PROT_READ | PROT_WRITE
							, MAP_SHARED
							, fd
							, pwm_base
			);
			if((int)pwm_map == -1){
				printf("@RPiGpioDrv::init() cannot mmap PWM\n");
				throw 0;
			}
			
			g_pPwm = (unsigned int *)pwm_map;
		}
		
		// CLOCK初期化
		if(!g_pClock){
			void* clock_map = NULL;
			clock_map = mmap(	  NULL
								, BLOCK_SIZE
								, PROT_READ | PROT_WRITE
								, MAP_SHARED
								, fd
								, clock_base
			);
			if((int)clock_map == -1){
				printf("@RPiGpioDrv::init() cannot mmap CLOCK\n");
				throw 0;
			}
			
			g_pClock = (unsigned int *)clock_map;
		}
		*/
		//  mmap()後はfdをクローズ
		if(fd >= 0){
			close(fd);
		}
		
		// ここまでくれば正常
		iRet = 0;
	}
	catch(...)
	{
		// ここに来たら異常
		iRet = -1;
		
		if(fd >= 0){
			close(fd);
		}	
	}

	return iRet;
}

// GPIOピンのモードの設定
//		pin :	2,3,4,7,8,9,10,11,14,15,17,18,22,23,24,25,27,
//				28,29,30,31
//		mode:	GPIO_INPUT, GPIO_OUTPUT, GPIO_PWM
int RPiGpioDrv::setPinMode(const int& pin, const int& mode)
{
	if(!g_pGpio){
		return -1;
	}

	//  check range of pin-number.
	if(pin < GPIO_PIN_MIN || pin > GPIO_PIN_MAX) {
		printf("@RPiGpioDrv::setPinMode() pin number out of range\n");
		return -1;
	}
	//  レジスタ番号(index)と3bitマスクを生成
	int index = (int)(pin / 10);
	int shift = ((pin % 10) * 3);
	unsigned int mask = ~(0b111 << shift);
	
	if( mode == GPIO_INPUT ){
		//  GPFSELの該当するFSEL(3bit)のみを書き換え
		g_pGpio[index] = (g_pGpio[index] & mask); // | (FSEL_INPUT << shift);
		
	}else if( mode == GPIO_OUTPUT ){
		//  GPFSELの該当するFSEL(3bit)のみを書き換え
		g_pGpio[index] = (g_pGpio[index] & mask) | (FSEL_OUTPUT << shift);
		
	}else if( mode == GPIO_PWM ){
		return -1;
		/*
		unsigned char alt = gpioToPwmALT[pin];
		if( alt == 0){
			printf("this pin is not PwmALT.¥n");
			return -1;
		}
		//  GPFSELの該当するFSEL(3bit)のみを書き換え
		g_pGpio[index] = (g_pGpio[index] & mask) | (alt << shift);
	
		delayMicroSec(110);
		
		if(setPwmMode(PWM_MODE_BALANCED)!=0){
			printf("failed to  setPwmMode(PWM_MODE_BALANCED).¥n");
			return -1;
		}
		
		// Default range of 1024
		if(setPwmRange(1024)!=0){
			printf("failed to  setPwmRange().¥n");
			return -1;
		}
		
		// 19.2 / 32 = 600KHz - Also starts the PWM
		if(setPwmClock(32)!=0){
			printf("failed to  setPwmClock().¥n");
			return -1;
		}
		*/
	}else{
		return -1;
	}
	
	// success.
	return 0;
}

//  ピンの出力を1(HighLevel:3.3V)/0(LowLevel:0.0V)に設定
int RPiGpioDrv::setOutLevel(const int& pin, const int& level)
{
	if(!g_pGpio){
		return -1;
	}
	
	//  check range of pin-number.
	if(pin < GPIO_PIN_MIN || pin > GPIO_PIN_MAX) {
		printf("@RPiGpioDrv::setOutLevel() pin number out of range\n");
		return -1;
	}

	if(level==0){
		//  pin-level to LowLevel(0.0V).
		g_pGpio[10] = 0x1 << pin;
    }else{
		//  pin-level to HighLevel(3.3V).
    	g_pGpio[7] = 0x1 << pin;
	}

	// success.
	return 0;
}

// ピンの状態を1(HighLevel:3.3V)/0(LowLevel:0.0V)として取得
int RPiGpioDrv::getLevel(const int& pin, int& level)
{
	if(!g_pGpio){
		return -1;
	}

	// clear output.
	level = 0;

    //  check range of pin-number.
	if(pin < GPIO_PIN_MIN || pin > GPIO_PIN_MAX){
		printf("@RPiGpioDrv::setOutLevel() pin number out of range\n");
		return -1;
	}

	level = ( (g_pGpio[13] & (0x1 << pin)) != 0 );

    // success.
    return 0;
}

/*
int RPiGpioDrv::setPwmMode(const int& mode)
{
	if(!g_pPwm){
		return -1;
	}
	
	if( mode == PWM_MODE_MARKSPACE ){
		*(g_pPwm + PWM_CONTROL) = PWM0_ENABLE | PWM1_ENABLE | PWM0_MS_MODE | PWM1_MS_MODE;
	}else if( mode == PWM_MODE_BALANCED ){
		*(g_pPwm + PWM_CONTROL) = PWM0_ENABLE | PWM1_ENABLE;
	}else{
		return -1;
	}
	
	// success.
	return 0;
}
*/

/*
int RPiGpioDrv::setPwmRange(const unsigned int& range)
{
	if(!g_pPwm){
		return -1;
	}

	*(g_pPwm + PWM0_RANGE) = range;
	delayMicroSec(10);

	*(g_pPwm + PWM1_RANGE) = range;
	delayMicroSec(10);

	// success.
	return 0;
}
*/

/*
int RPiGpioDrv::setPwmClock(const int& clock)
{
	if(!g_pPwm){
		return -1;
	}
	
	if(!g_pClock){
		return -1;
	}
	
	unsigned int pwm_control = 0;
	int dest_clock = clock & 4095;
	
	// preserve PWM_CONTROL
	pwm_control = *(g_pPwm + PWM_CONTROL);

	// Stop PWM
	*(g_pPwm + PWM_CONTROL) = 0;
	
	// Stop PWM Clock
	*(g_pClock + PWMCLK_CNTL) = BCM_PASSWORD | 0x01;
	delayMicroSec(110);
	
	while((*(g_pClock + PWMCLK_CNTL) & 0x80) != 0){
		delayMicroSec(1);
	}
	
	// Set Clock
	*(g_pClock + PWMCLK_DIV)  = BCM_PASSWORD | (dest_clock << 12);
	
	// Start PWM clock
	*(g_pClock + PWMCLK_CNTL) = BCM_PASSWORD | 0x11;
	
	// restore PWM_CONTROL
	*(g_pPwm + PWM_CONTROL) = pwm_control;
	
	// success.
	return 0;
}
*/

/*
int RPiGpioDrv::writePwmGpio(const int& pin, const int& val)
{
	if(!g_pPwm){
		return -1;
	}
	
	int port = 0;
	int dest_pin = pin & 63;
	port = gpioToPwmPort[dest_pin];
	
	*(g_pPwm + port) = val;
	g_pPwm[port] = val;
	
	// success.
	return 0;
}
*/

// 指定したマイクロ秒(1秒=1000000マイクロ秒)待つ
/*
void RPiGpioDrv::delayMicroSec(const unsigned int& msec)
{
	struct timespec delaytime;
	unsigned int uSecs = msec % 1000000;
	unsigned int wSecs = msec / 1000000 ;
		
	if (msec == 0){
		return;
	}else if(msec < 100){
		delayMicroSecForce(msec);
	}else{
		delaytime.tv_sec	= wSecs ;
		delaytime.tv_nsec	= (long)(uSecs * 1000L) ;
		nanosleep(&delaytime, NULL) ;
	}
	
	return;
}
*/

// delayMicroSec()で指定したマイクロ秒が100より小さい場合は無理やりループして止める
/*
void RPiGpioDrv::delayMicroSecForce(const unsigned int& msec)
{
	struct timeval stNow;
	struct timeval stLen;
	struct timeval stEnd;

	gettimeofday(&stNow, NULL);

	stLen.tv_sec  = msec / 1000000;
	stLen.tv_usec = msec % 1000000;
	timeradd (&stNow, &stLen, &stEnd) ;

	while ( timercmp(&stNow, &stEnd, <) ){
		gettimeofday(&stNow, NULL);
	}

	return;
}
*/
