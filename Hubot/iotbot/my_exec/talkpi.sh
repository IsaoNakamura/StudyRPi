#!/bin/sh

cd /home/pi/aquestalkpi
./AquesTalkPi -v f1 $@ | aplay -D plughw
cd /home/pi
