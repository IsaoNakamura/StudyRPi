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
my $execTrade = 1;


# パラメタ:ライフサイクル
# 100=約17秒
# 1000=170秒=約3分
my $range = 5000;

# パラメタ:MIN,MAX更新時の遊び時間
my $countNum = 100;
my $countdown = $countNum;

# 状態情報
my @tickerArray;
my $max = 0;
my $min = 0;
my $short_entry = 0;
my $long_entry = 0;
my $profit_sum = 0;

# ポジション
my $position = "NONE";
my $pre_position = $position;

# 前値保存用
my $pre_tick_id = 0;

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
        
        $best_bid = $res_json->{"best_bid"}; # 買値
        $best_ask = $res_json->{"best_ask"}; # 売値(買値より高い)
        $tick_id = $res_json->{"tick_id"};
        $timestamp = $res_json->{"timestamp"};

        if($pre_tick_id eq $tick_id){
            next;
        }
        #if( exists $tickerHash{$tick_id}){
        #    next;
        #}
        #$tickerHash{$tick_id} = $res_json;
        push(@tickerArray, $res_json);
        $pre_tick_id = $tick_id;

        # MAX,MIN初期値設定
        if($cycle_cnt==0){
            if($max==0){
                $max = $best_ask;
                $countdown = $countNum;
            }
            if($min==0){
                $min = $best_bid;
                $countdown = $countNum;
            }
        }

        # MAX,MIN更新
        if(@tickerArray > $range){
            # Ticker配列がレンジ数を超えた場合
            # 末尾を削除
            my $shift_json = shift(@tickerArray);
            my $shift_bid = $shift_json->{"best_bid"};
            my $shift_ask = $shift_json->{"best_ask"};
            if($shift_ask == $max || $shift_bid == $min){
                # 削除したものがMAXまたはMINだった場合
                # MAX,MINを再計算する
                print "SHIFT($shift_bid,$shift_ask)\n";
                for(my $i=0; $i<@tickerArray; $i++){
                    my $elem_json = $tickerArray[$i];
                    my $elem_bid = $elem_json->{"best_bid"};
                    my $elem_ask = $elem_json->{"best_ask"};
                    if($i==0){
                        $max = $elem_ask;
                        $min = $elem_bid;
                        $countdown = $countNum;
                        next;
                    }
                    if($max < $elem_ask){
                        $max = $elem_ask;
                        $countdown = $countNum;
                    }
                    if($min > $elem_bid){
                        $min = $elem_bid;
                        $countdown = $countNum;
                    }
                }
                print "UPD($min,$max)\n";
            }
        }else{
            if($max < $best_ask){
                $max = $best_ask;
                $countdown = $countNum;
            }
            if($min > $best_bid){
                $min = $best_bid;
                $countdown = $countNum;
            }
        }

        my $profit = 0;
        if($execTrade==1){
            if($position eq "NONE" && $countdown == 0){
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
                my $shortRange = $short_entry - $min;
                my $shortProfit = $shortRange / 2;
                my $shortLC = $shortRange / 2;
                if( $profit <= -$shortLC ){
                    # SHORTロスカット
                    print "SHORT-LOSSCUT:$best_bid($profit)\n";
                    $position = "NONE";
                    $short_entry = 0;
                    $profit_sum += $profit;
                }elsif( ($min >= $best_bid) || ($profit >= $shortProfit ) ){
                    # SHORT利確
                    print "SHORT-RIKAKU:$best_bid($profit)\n";
                    $position = "NONE";
                    $short_entry = 0;
                    $profit_sum += $profit;

                    if( ($min >= $best_bid)  && ($countdown == 0) ){
                        # ドテンLONGエントリー
                        $position = "LONG";
                        $long_entry = $best_bid;
                        print "DOTEN-LONG-ENTRY:$best_bid\n";
                    }
                }
            }elsif($position eq "LONG"){
                $profit = $best_ask - $long_entry;
                my $longRange = $max - $long_entry;
                my $longProfit = $longRange / 2;
                my $longLC = $longRange / 2;
                if( $profit <= -$longLC ){
                    # LONGロスカット
                    print "LONG-LOSSCUT:$best_ask($profit)\n";
                    $position = "NONE";
                    $long_entry = 0;
                    $profit_sum += $profit;
                }elsif( ($max <= $best_ask) || ($profit >= $longProfit) ){
                    # LONG利確
                    print "LONG-RIKAKU:$best_ask($profit)\n";
                    $position = "NONE";
                    $long_entry = 0;
                    $profit_sum += $profit;

                    if( ($max <= $best_ask) && ($countdown == 0) ){
                        # ドテンSHORTエントリー
                        $position = "SHORT";
                        $short_entry = $best_ask;
                        print "DOTEN-SHORT-ENTRY:$best_ask\n";
                    }
                }
            }
        }

        #if($position ne $pre_position ){
            # 情報出力
            my $array_cnt = @tickerArray;
            my $info_str = sprintf("[%05d]: TID=%8d: BID=%7d: ASK=%7d: MIN=%7d: MAX=%7d: POS=%5s: PRF=%7d: DWN=%3d: SUM=%7d TIME=%s: \n"
                , $cycle_cnt
                , $tick_id
                , $best_bid
                , $best_ask
                , $min
                , $max
                , $position
                , $profit
                , $countdown
                , $profit_sum
                , $timestamp
            );
            print $info_str;
        #}

        # カウントダウン
        if($countdown!=0){
            $countdown--;
            if($countdown<0){
                $countdown = 0;
            }
        }

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
