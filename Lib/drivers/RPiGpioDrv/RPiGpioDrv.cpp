#include "RPiGpioDrv.h"

//  レジスタブロックの物理アドレス
#define PERI_BASE_RPI_ONE	(0x20000000)	// for RPi1(BCM2708)
#define PERI_BASE_RPI_TWO	(0x3F000000)	// for RPi2(BCM2835)

#define GPIO_BASE_OFFSET	(0x00200000)
#define PWM_BASE_OFFSET		(0x0020C000)

#define BLOCK_SIZE			(4096)

#define GPIO_PIN_MAX		(31)
#define GPIO_PIN_MIN		( 0)

// GPIOレジスタ (volatile:実メモリに必ずアクセス)
static volatile unsigned int *g_pGpio = NULL;
static volatile unsigned int *g_pPwm = NULL;

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
		unsigned int gpio_base	= 0x0;
		unsigned int pwm_base	= 0x0;
		if(RPiVer == RPI_VER_ONE){
			gpio_base	= PERI_BASE_RPI_ONE + GPIO_BASE_OFFSET;
			pwm_base	= PERI_BASE_RPI_ONE + PWM_BASE_OFFSET;
		}else if(RPiVer == RPI_VER_TWO){
			gpio_base	= PERI_BASE_RPI_TWO + GPIO_BASE_OFFSET;
			pwm_base	= PERI_BASE_RPI_TWO + PWM_BASE_OFFSET;
		}else{
			printf("@RPiGpioDrv::init() RPi's Version(%d) is not supported\n",RPiVer);
			throw 0;
		}

		//  /dev/memを開く（要sudo）
		fd = open("/dev/mem", O_RDWR | O_SYNC);
		if(fd == -1) {
			printf("@RPiGpioDrv::init() cannot open /dev/mem\n");
			throw 0;
		}

		//  GPIO初期化
		if(!g_pGpio){
			//  mmap で GPIO（物理メモリ）を gpio_map（仮想メモリ）に紐づける
			void *gpio_map = NULL;
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
		
			g_pGpio = (unsigned int *) gpio_map;
		}
		
		// PWM初期化
		if(!g_pPwm){
			void *pwm_map = NULL;
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
			
			g_pPwm = (unsigned int *) pwm_map;
		}
		
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
//		mode:	GPIO_INPUT, GPIO_OUTPUT,
//				GPIO_ALT0, GPIO_ALT1, GPIO_ALT2, GPIO_ALT3, GPIO_ALT4, GPIO_ALT5
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
	unsigned int mask = ~(0x7 << shift);
	//  GPFSEL0/1の該当するFSEL(3bit)のみを書き換え
	g_pGpio[index] = (g_pGpio[index] & mask) | ((mode & 0x7) << shift);
	
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
