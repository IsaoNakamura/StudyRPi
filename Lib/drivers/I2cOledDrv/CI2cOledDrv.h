/*
 * CI2cOledDrv.h
 *
 *  Created on: 2013/06/23
 *      Author: isao
 */
#ifndef COLEDDRV_H_
#define COLEDDRV_H_


#define OLDE_ADDRESS (0x3c)
//#define OLDE_ROWS 64
//#define OLDE_COLS 128
#define OLDE_COLS 16
#define OLDE_PAGES 8
#define OLDE_MAXPAGE (OLDE_PAGES - 1)
#define OLDE_MAXCOL (OLDE_COLS - 1)
#define OLED_RS_CMD (0x00)
//#define OLED_RS_CMD (0x80)
#define OLED_RS_DATA (0x40)

#define CMD_DISPLAY_OFF (0xae)
#define CMD_DISPLAY_ON (0xaf)
#define CMD_DISPLAY_NORMAL (0xa6)
#define CMD_DISPLAY_INVERSE (0xa7)
#define CMD_ACTIVATE_SCROLL (0x2f)
#define CMD_DECTIVATE_SCROLL (0x2e)
#define CMD_DISPLAY_ENTRE_ON (0xa5)
#define CMD_SET_COLUMN_ADDRESS (0x21)
#define CMD_SET_PAGE_ADDRESS (0x22)
#define CMD_SET_SEGMENT_REMAP (0xa1)
#define CMD_SET_COM_PINS_CONFIG (0xda)
#define MODE_COM_PINS_RESET (0x12)
#define CMD_REMAPPED_MODE (0xc8)
#define CMD_SET_MULTIPLEX_RATIO (0xa8)
#define MODE_MULTIPLEX_RATIO_RESET (0x3f)
#define CMD_SET_DISPLAY_CLK_DIV (0xd5)
#define MODE_DISPLAY_CLK_DIV_RESET (0x80) //RESET 0x80 == 10000000b
#define CMD_SET_CONTRAST_CONTROL (0x81)
#define CMD_SET_PRE_CHARGE_PERIOD (0xd9)
#define MODE_PRE_CHARGE_PERIOD_RESET
#define CMD_SET_MEMORY_ADDR_MODE (0x20)
#define ADDR_MODE_HORIZONTAL (0x00)
#define ADDR_MODE_VERTICAL (0x01)
#define ADDR_MODE_PAGE (0x02)
#define CMD_SET_V_DESELECT_LV (0xdb)
#define CMD_EXT_OR_INT_SELECTION (0xad)
#define CMD_ENTIRE_DISPLAY_OFF (0xa4)
#define CMD_ENTIRE_DISPLAY_ON (0xa5)
#define CMD_SET_NORMAL_DISPLAY (0xa6)
#define CMD_SET_INVERSE_DISPLAY (0xa7)

class CI2cOledDrv {
public:
	virtual ~CI2cOledDrv();
private:
	CI2cOledDrv();
	int m_fd;
	int m_address;

public:
	static CI2cOledDrv* createInstance(const int& fd,const int& address=OLDE_ADDRESS);

private:
	void initInstance();
	void destroyInstance();
	int startInstance();

public:
	int useDevice();
	void initDisplay();
	void startDisplay();
	void clearDisplay();
	void writeData(const char& rs, const char& data);
	void setCursor(const char& col, const char& row);
	void setPageAddress(const char& startPage, const char& endPage);
	void setColumnAddress(const char& startCol, const char& endCol);
	void setMemoryAdressingMode(const char& mode);
	void setCOMPinsHardConfig(const char& mode);
	void writeDataArg2(const char& rs, const char& cmd, const char& arg);
	void writeChar(const char& chr);
	void writeString(const char* str);
	void displayON();
	void displayOFF();
	void writeTetrisBlock();
	void writeTetrisWall();
};

#endif /* COLEDDRV_H_ */
