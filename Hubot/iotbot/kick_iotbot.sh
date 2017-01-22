#!/bin/sh

# /etc/rc.local に
# 	sudo -u pi /home/pi/GitHub/StudyRPi/App/homingFace/homingFace.sh
# というような本スクリプトを実行する命令を exit 0 の前に記載すれば
# 起動時に実行できるようになる。

cd /home/pi/GitHub/StudyRPi/Hubot/iotbot
sudo ./bin/hubot -a slack
cd /home
