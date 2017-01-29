#!/bin/sh

DATE=$(date +"%Y-%m-%d_%H%M")
# echo "arg_num : $#"
echo "date    : $DATE"

if [ $# -eq 0 ]; then
    # echo "no arg"
    echo "raspistill -o /home/pi/picam/$DATE.jpg"
    raspistill -o /home/pi/picam/$DATE.jpg
elif [ $# -eq 1 ]; then
    echo "raspistill -o $1"
    raspistill -o $1
else
    echo "raspistill ${2} -o $1"
    raspistill ${2} -o $1
fi
