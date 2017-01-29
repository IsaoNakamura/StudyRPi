#!/bin/sh

DATE=$(date +"%Y-%m-%d_%H%M")
# echo "arg_num : $#"
echo "date    : $DATE"

if [ $# -eq 0 ]; then
    # echo "no arg"
    echo "raspistill -o /home/pi/picam/$DATE.jpg"
    raspistill -o /home/pi/picam/$DATE.jpg
else
    echo "raspistill $@ -o /home/pi/picam/$DATE.jpg"
    raspistill $@ -o /home/pi/picam/$DATE.jpg
fi
