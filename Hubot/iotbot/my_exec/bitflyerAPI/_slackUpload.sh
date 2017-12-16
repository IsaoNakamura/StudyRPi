#!/bin/sh

host=$1
token=$2
channel=$3
filepath=$4

#curl -o /dev/null -w '%{http_code}\n' -s -F file=@$filepath -F channels=$channel -F token=$token $host
curl -o /dev/null -s -F file=@$filepath -F channels=$channel -F token=$token $host