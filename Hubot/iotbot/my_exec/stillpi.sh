#!/bin/sh

if [ $# -ne 3 ]; then
  echo "指定された引数は$#個です。"
  echo "実行するには3個の引数が必要です。"
fi

DATE=$(date +"%Y-%m-%d_%H%M")
echo "arg_num : $#"
echo "date    : $DATE"
if [ $# -eq 0]
then
    echo "no arg"
    raspistill -o /home/pi/picam/$DATE.jpg
else
    echo "raspistill $@ -o /home/pi/picam/$DATE.jpg"
    raspistill $@ -o /home/pi/picam/$DATE.jpg
fi
