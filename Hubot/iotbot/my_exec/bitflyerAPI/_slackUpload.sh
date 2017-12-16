#!/bin/sh

host=$1
token=$2
channel=$3
filepath=$4

curl -s -S -F file=@$filepath -F channels=$channel -F token=$token $host