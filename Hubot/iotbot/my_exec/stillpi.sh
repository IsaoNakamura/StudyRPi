#!/bin/sh

DATE=$(date +"%Y-%m-%d_%H%M")
if [$# > 0]
then
    echo raspistill $@ -o /home/pi/picam/$DATE.jpg
    raspistill $@ -o /home/pi/picam/$DATE.jpg
else
    raspistill -o /home/pi/picam/$DATE.jpg
fi
