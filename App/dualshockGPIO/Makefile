dualshockGPIO: dualshockGPIO.o RPiGpioDrv.o CJoystickDrv.o
	g++ -Wall -O2 -o dualshockGPIO dualshockGPIO.o ../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.o ../../Lib/drivers/JoystickDrv/CJoystickDrv.o

dualshockGPIO.o: dualshockGPIO.cpp
	g++ -c dualshockGPIO.cpp

RPiGpioDrv.o: RPiGpioDrv.cpp
	g++ -c ../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.cpp

CJoystickDrv.o: CJoystickDrv.cpp
	g++ -c ../../Lib/drivers/JoystickDrv/CJoystickDrv.cpp

.PHONY: clean
clean:
	rm -f dualshockGPIO dualshockGPIO.o ../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.o ../../Lib/drivers/JoystickDrv/CJoystickDrv.o
