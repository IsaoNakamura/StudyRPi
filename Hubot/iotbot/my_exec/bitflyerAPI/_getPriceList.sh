#!/bin/sh

path=/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/bitflyerAPI
host=https://bitflyer.jp/api/echo/price
dest=$path/DEST/PriceList.json
graph=$path/DEST/PriceList.png
test=1

# sudo $path/getPriceList.pl $host $dest $graph $test $@
# sudo $path/getPriceDiff.pl $host $path/DEST/PriceDiff.json 0
# sudo $path/helloPerl.pl
# sudo $path/genPriceGraph.pl $path/DEST/PriceDiff.json $graph $@
sudo $path/getPrice.pl $host $path/DEST/Price.json

