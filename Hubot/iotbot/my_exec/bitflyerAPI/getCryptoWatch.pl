#!/usr/bin/perl

use strict;
use warnings;

use utf8;
use Encode;

use Encode 'decode';
use Encode 'encode';

use JSON;

use LWP::UserAgent;

use HTTP::Request;

use HTTP::Date;

use MyModule::UtilityJson;
use MyModule::UtilityTime;
use MyModule::UtilityCryptowatch;

my $dest = "./candleStickAfter.json";

my $ua = new LWP::UserAgent;
$ua->timeout(10); # default: 180sec
$ua->ssl_opts( verify_hostname => 0 ); # skip hostname verification


my $symbol    = "btcfxjpy";
my $periods   = 60;
my $after     = time() - (30 * 60); # 30分前の値を取得
my $path      = "ohlc";
my $endPoint  = "https://api.cryptowat.ch/markets/bitflyer";

my $res_json;
if( MyModule::UtilityCryptowatch::getCandleStickAfter(
                                                        \$res_json,
                                                        \$ua,
                                                        $symbol,
                                                        $periods,
                                                        $after,
                                                        $path,
                                                        $endPoint
                                                    )!=0
){
    exit -1;
}

if(MyModule::UtilityJson::writeJson(\$res_json, $dest, ">")!=0){
    print "FileWriteError. $dest.\n";
}


my $candleArray_ref = $res_json->{"result"}->{"60"};
for(my $i=0; $i<@{$candleArray_ref}; $i++){
    my $candle = $candleArray_ref->[$i];
    #        [0],       [1],       [2],      [3],        [4],    [5]
    #[ CloseTime, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume ]
    my $close_time = $candle->[0];
    my $close_price = $candle->[4];
    print "[$i]: close_time=$close_time, close_price=$close_price \n";
}

exit 0;

1;
