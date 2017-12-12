#!/bin/sh

dir=/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/bitflyerAPI
url=https://bitflyer.jp/api/echo/price

echo dir=$dir
echo url=$url

$dir/getPriceDiff.pl $url $dir/DEST/result.json $@

