#!/bin/sh

# echo "arg_num : $#"

dir=/home/pi/picam

if [ $# -eq 0 ]; then
    if [ -e $dir ]; then
        echo "dir is exist."
    else
        echo "dir is-not exist."
        mkdir $dir
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
