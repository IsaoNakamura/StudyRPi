#include "RPiGpioDrv.h"

//  レジスタブロックの物理アドレス
#define GPIO_BASE_RPI_ONE	(0x20200000)	// for RPi1
#define GPIO_BASE_RPI_TWO	(0x3F200000)	// for RPi2
#define BLOCK_SIZE			(4096)

#define GPIO_PIN_MAX		(31)
#define GPIO_PIN_MIN		( 0)

//  GPIO 関連レジスタ (volatile＝必ず実メモリにアクセスさせる)
static volatile unsigned int *g_pGpio;

RPiGpioDrv::RPiGpioDrv()
{
}

RPiGpioDrv::~RPiGpioDrv()
{
}

//  GPIO 初期化
int RPiGpioDrv::init(const int& RPiVer/*=2*/)
{
    //  既に初期化済なら何もしない
    if(g_pGpio){
		return 0;
	}
	
	//  レジスタブロックの物理アドレス
	unsigned int register_base = 0x0;
	if(RPiVer == 1){
		register_base = GPIO_BASE_RPI_ONE;
	}else if(RPiVer == 2){
		register_base = GPIO_BASE_RPI_TWO;
	}else{
		printf("@RPiGpioDrv::init() RPi's Version(%d) is not supported\n",RPiVer);
		return -1;
	}

    //  ここから GPIO 初期化
    int fd;
    void *gpio_map;
    //  /dev/memを開く（要sudo）
    fd = open("/dev/mem", O_RDWR | O_SYNC);
    if(fd == -1) {
        printf("@RPiGpioDrv::init() cannot open /dev/mem\n");
        return -1;
    }

    //  mmap で GPIO（物理メモリ）を gpio_map（仮想メモリ）に紐づける
    gpio_map = mmap(NULL, BLOCK_SIZE,
                    PROT_READ | PROT_WRITE, MAP_SHARED,
                    fd, register_base );
    if((int) gpio_map == -1) {
        printf("@RPiGpioDrv::init() cannot mmap /dev/mem\n");
        return -1;
    }
    //  mmap()後はfd をクローズ
    close(fd);

    g_pGpio = (unsigned int *) gpio_map;
    
    // 正常終了
    return 0;
}

//  ピンモードの設定
//      pin : (P1) 2,3,4,7,8,9,10,11,14,15,17,18,22,23,24,25,27
//            (P5) 28,29,30,31
//      mode: GPIO_INPUT, _OUTPUT, _ALT0, _ALT1, _ALT2, _ALT3, _ALT4, _ALT5
void int RPiGpioDrv::setPinMode(const int& pin, const int& mode)
{
    //  check range of pin-number.
    if(pin < GPIO_PIN_MIN || pin > GPIO_PIN_MAX) {
        printf("@RPiGpioDrv::setPinMode() pin number out of range\n");
        return -1;
    }
    //  レジスタ番号（index）と３ビットマスクを生成
    int index = pin / 10;
    unsigned int mask = ~(0x7 << ((pin % 10) * 3));
    //  GPFSEL0/1 の該当する FSEL (3bit) のみを書き換え
    g_pGpio[index] = (g_pGpio[index] & mask) | ((mode & 0x7) << ((pin % 10) * 3));
}

//  ピンの出力を1(HighLevel:3.3V)/0(LowLevel:0.0V)に設定
int RPiGpioDrv::setOutLevel(const int& pin, const int& level)
{
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
	// clear output.
	level = 0;

    //  check range of pin-number.
    if(pin < GPIO_PIN_MIN || pin > GPIO_PIN_MAX) {
       printf("@RPiGpioDrv::setOutLevel() pin number out of range\n");
        return -1;
    }

    level = ( (g_pGpio[13] & (0x1 << pin)) != 0 );

    // success.
    return 0;
}
