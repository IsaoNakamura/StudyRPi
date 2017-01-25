#!/bin/sh

cd /home/pi/aquestalkpi
./AquesTalkPi -v f1 $@ | aplay
cd /home/pi
