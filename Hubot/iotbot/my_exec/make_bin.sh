#!/bin/sh

# cd /home/pi/GitHub/StudyRPi/App/breakInMotor
cd /home/pi/GitHub/StudyRPi/$1
make clean
make
ls /home/pi/GitHub/StudyRPi/Bin/obj
cd /home/pi

