#!/usr/bin/perl

use strict;
use warnings;

use utf8;
use Encode;

use Encode 'decode';
use Encode 'encode';

use JSON;

use LWP::UserAgent;

use MyModule::UtilityJson;
use MyModule::UtilityBitflyer;

use Selenium::Remote::Driver;

use Data::Dumper;

my $authFilePath = "./AuthBitflyer.json";
my $dest = "./DEST/CryptoInagoRider.json";
my $ammount = 0.005;

if(!(-f $authFilePath)){
    print "not exists AuthFile. $authFilePath\n";
    exit -1;
}

my $authBitflyer;
if(MyModule::UtilityJson::readJson(\$authBitflyer, $authFilePath)!=0){
    print "FileReadError. Auth.\n";
    exit -1;
}

my $ua = new LWP::UserAgent;
$ua->timeout(10); # default: 180sec
$ua->ssl_opts( verify_hostname => 0 ); # skip hostname verification




# パラメタ
my $cycle_sec = 0;
my $rikaku_retry_num = 3;
my $entry_retry_num = 0;
my $force = 0.7;
my $threshold = 50.0;
my $execTrade = 1;


# インジケータ
my $indicator = 0.0;

# 前値保存用
my $pre_effective = 0.0;
my $pre_indicator = 0.0;

# ポジション
my $position = "NONE";
my $pre_position = $position;

my %tickerHash;
my $max = 863600;
my $min = 859000;
my $LC_VALUE = 1000;
my $short_entry = 0;
my $long_entry = 0;

# メインループ
my $cycle_cnt =0;
while(1){
    #eval{
        # Ticker(相場)を取得
        my $best_bid = 0;
        my $best_ask = 0;
        my $tick_id = 0;
        my $timestamp = "";
        my $res_json;
        if( MyModule::UtilityBitflyer::getTicker(
                                                    \$res_json,
                                                    \$ua,
                                                    \$authBitflyer,
                                                    "FX_BTC_JPY"
                                                )!=0
        ){
            next;
        }
        
        $best_bid = $res_json->{"best_bid"};
        $best_ask = $res_json->{"best_ask"};
        $tick_id = $res_json->{"tick_id"};
        $timestamp = $res_json->{"timestamp"};
        if( exists $tickerHash{$tick_id}){
            next;
        }
        $tickerHash{$tick_id} = $res_json;

        my $profit = 0;
        if($position eq "NONE"){
            if(abs($best_ask-$max) < 1000){
                # SHORTエントリー
                $position = "SHORT";
                $short_entry = $best_ask;
                print "SHORT-ENTRY:$best_ask\n";
                
            }elsif(abs($best_bid-$min) < 1000){
                # LONGエントリー
                $position = "LONG";
                $long_entry = $best_bid;
                print "LONG-ENTRY:$best_ask\n";
            }
        }elsif($position eq "SHORT"){
            $profit = $short_entry - $best_bid;
            #if( ($short_entry+$LC_VALUE) < $best_ask){
            if( $profit <= -1000 ){
                # SHORTロスカット
                print "SHORT-LOSSCUT:$best_ask($profit)\n";
                $position = "NONE";
                $short_entry = 0;
            #}elsif( ($min >= $best_bid) || ($profit >= 4000) ){
            }elsif( $profit >= 4000 ){
                {
                    # SHORT利確
                    print "SHORT-RIKAKU:$best_bid($profit)\n";
                    $position = "NONE";
                    $short_entry = 0;
                }
                {
                    # ドテンLONGエントリー
                    $position = "LONG";
                    $long_entry = $best_bid;
                    print "DOTEN-LONG-ENTRY:$best_ask\n";
                }
            }
        }elsif($position eq "LONG"){
            $profit = $best_ask - $long_entry;
            #if( ($long_entry-$LC_VALUE) > $best_bid){
            if( $profit <= -1000 ){
                # LONGロスカット
                print "LONG-LOSSCUT:$best_bid($profit)\n";
                $position = "NONE";
                $long_entry = 0;
            #}elsif( ($max >= $best_ask) || ($profit >= 4000) ){
            }elsif( $profit >= 4000 ){
                # LONG利確
                print "LONG-RIKAKU:$best_ask($profit)\n";
                $position = "NONE";
                $long_entry = 0;
            }
        }

        #if($max < $best_ask){
        #    $max = $best_ask;
        #}

        #if($min > $best_bid){
        #    $min = $best_bid;
        #}

        #if($position ne $pre_position ){
            # 情報出力
            my $info_str = sprintf("[%05d]: TID=%8d: BID=%7d: ASK=%7d: POS=%5s: PRF=%7d: TIME=%s: \n"
                , $cycle_cnt
                , $tick_id
                , $best_bid
                , $best_ask
                , $position
                , $profit
                , $timestamp
            );
            print $info_str;
        #}


        # 前値保存
        $pre_position = $position;

    #};
    sleep($cycle_sec);
    $cycle_cnt++;
}

sub buyMarket{
    my $resultJson_ref = shift;
    my $retry_num      = shift;

    my $result = -1;
    my $retry_cnt = 0;

    while(1){
        my $ret_req =   MyModule::UtilityBitflyer::buyMarket(
                            $resultJson_ref,
                            \$ua,
                            \$authBitflyer,
                            "FX_BTC_JPY",
                            $ammount
                        );
        print "ret_req=$ret_req\n";
        $result = $ret_req;
        if( $ret_req==0 ){
            # 注文成功
            last;
        }else{
            if($retry_cnt <= $retry_num){
                sleep(2);
                print "retry:$retry_cnt\n";
            }else{
                last;
            }
        }
        $retry_cnt++;
    }
    return($result);
}

sub sellMarket{
    my $resultJson_ref = shift;
    my $retry_num      = shift;

    my $result = -1;
    my $retry_cnt = 0;
    while(1){
        my $ret_req =   MyModule::UtilityBitflyer::sellMarket(
                            $resultJson_ref,
                            \$ua,
                            \$authBitflyer,
                            "FX_BTC_JPY",
                            $ammount
                        );
        print "ret_req=$ret_req\n";
        $result = $ret_req;
        if( $ret_req==0 ){
            # 注文成功
            last;
        }else{
            if($retry_cnt <= $retry_num){
                sleep(2);
                print "retry:$retry_cnt\n";
            }else{
                last;
            }
        }
        $retry_cnt++;
    }
    return($result);
}

1;
