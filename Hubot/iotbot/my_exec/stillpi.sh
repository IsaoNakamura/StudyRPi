#!/bin/sh

DATE=$(date +"%Y-%m-%d_%H%M")
raspistill $@ -o /home/pi/picam/$DATE.jpg
