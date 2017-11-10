#!/bin/sh

# echo "arg_num : $#"

if [ $# -eq 0 ]; then
    if [ -e /home/pi/picam ]; then
    else
        mkdir /home/pi/picam
    fi
    # echo "no arg"
    DATE=$(date +"%Y-%m-%d_%H%M")
    # echo "date    : $DATE"
    echo "raspistill -o /home/pi/picam/$DATE.jpg"
    raspistill -o /home/pi/picam/$DATE.jpg
else
    echo "raspistill -o $@"
    raspistill -o $@
fi
