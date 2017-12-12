#!/bin/sh

dir=/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/bitflyerAPI
url=https://bitflyer.jp/api/echo/price

echo dir=$dir >&1
echo url=$url >&1

sudo $dir/getPriceList.pl $url $dir/DEST/PriceList.json $@

