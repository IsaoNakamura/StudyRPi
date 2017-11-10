#!/bin/sh

cd /home/pi/aquestalkpi

# AquesTalkPi's Options
#  -v f1/f2  : Type
#  -s 50-300 : Speed
#  -g 0-100  : Volume
#  -b        : Bouyomi
#  -o a.wav  : Output
./AquesTalkPi -s 65 -v f1 $@ | aplay -D plughw
cd /home/pi
