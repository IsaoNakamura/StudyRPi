outputGPIO: outputGPIO.o RPiGpioDrv.o CJoystickDrv.o
	g++ -Wall -O2 -o outputGPIO outputGPIO.o ../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.o ../../Lib/drivers/JoystickDrv/CJoystickDrv.o

outputGPIO.o: outputGPIO.cpp
	g++ -c outputGPIO.cpp

RPiGpioDrv.o: RPiGpioDrv.cpp
	g++ -c ../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.cpp

CJoystickDrv.o: CJoystickDrv.cpp
	g++ -c ../../Lib/drivers/JoystickDrv/CJoystickDrv.cpp

.PHONY: clean
clean:
	rm -f outputGPIO outputGPIO.o ../../Lib/drivers/RPiGpioDrv/RPiGpioDrv.o ../../Lib/drivers/JoystickDrv/CJoystickDrv.o
