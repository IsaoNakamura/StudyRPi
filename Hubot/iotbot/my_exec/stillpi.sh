#!/bin/sh

# echo "arg_num : $#"

dir=/home/pi/picam
if [ -e $dir ]; then
    echo "dir is exist."
else
    echo "dir is-not exist."
    mkdir $dir
fi

if [ $# -eq 0 ]; then
    # echo "no arg"
    DATE=$(date +"%Y-%m-%d_%H%M")
    # echo "date    : $DATE"
    echo "raspistill -o /home/pi/picam/$DATE.jpg"
    sudo raspistill -o /home/pi/picam/$DATE.jpg
else
    echo "raspistill -o $@"
    sudo raspistill -o $@
fi
