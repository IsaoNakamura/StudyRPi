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
use MyModule::UtilityTime;

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
my $range = 300;#4000;#5000;
my $range_prm = 0.36;
my $pre_range = $range;

# パラメタ:MIN,MAX更新時の遊び時間
my $countNum = 15;
my $countdown = $countNum;

# 状態情報
my @tickerArray;
my $min = 0;#895869;
my $max = 0;#903500;
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
        my $timestamp_wrk = $res_json->{"timestamp"};
        MyModule::UtilityTime::convertTimeGMTtoJST(\$timestamp, $timestamp_wrk);

        if($pre_tick_id eq $tick_id){
            next;
        }
        push(@tickerArray, $res_json);
        $pre_tick_id = $tick_id;

        # MAX,MIN初期値設定
        if($cycle_cnt==0){
            if($max==0){
                $max = $best_ask;
                $countdown = 100;#$countNum;
            }
            if($min==0){
                $min = $best_bid;
                $countdown = 100;#$countNum;
            }
        }

        # レンジをMAX,MINから計算
        $range = int(($max - $min) * $range_prm);
        #if($range_wrk > 300)
        #$X = $range / ($max - $min) = 9000 / 25000 = 0.36;

        if(@tickerArray > $range ){
            # Ticker配列がレンジ数を超えた場合
            my $doRecalc = 0;
            if($pre_range != $range){
                # RANGEが変更された場合
                my $beg_idx = 0;#($range - 1);
                my $end_idx = (@tickerArray - 1) - ($range - 1);
                splice(@tickerArray, $beg_idx, $end_idx );
                $doRecalc = 1;
            }else{
                # 先頭を削除
                my $shift_json = shift(@tickerArray);
                my $shift_bid = $shift_json->{"best_bid"};
                my $shift_ask = $shift_json->{"best_ask"};
                if($shift_ask == $max || $shift_bid == $min){
                    # 削除したものがMAXまたはMINだった場合
                    $doRecalc = 1;
                }
            }

            if($doRecalc>0){
                # MAX,MINを再計算する
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
                my $noneRange = ($max - $min) / 8;
                if(abs($best_ask-$max) < $noneRange){
                    # SHORTエントリー
                    print "SHORT-ENTRY:$best_ask\n";
                    my $res_json;
                    if( sellMarket(\$res_json, $entry_retry_num)==0 ){
                        # 注文成功
                        # SHORTポジションへ
                        $position = "SHORT";
                        $short_entry = $best_ask;
                    }
                }elsif(abs($best_bid-$min) < $noneRange){
                    # LONGエントリー
                    print "LONG-ENTRY:$best_ask\n";
                    my $res_json;
                    if( buyMarket(\$res_json, $entry_retry_num)==0 ){
                        # 注文成功
                        # LONGポジションへ
                        $position = "LONG";
                        $long_entry = $best_bid;
                    }
                }
            }elsif($position eq "SHORT"){
                $profit = $short_entry - $best_bid;
                my $shortRange = $short_entry - $min;
                my $shortProfit = $shortRange / 2;
                my $shortLC = $shortRange / 2;
                if( $profit <= -$shortLC ){
                    # SHORTロスカット
                    print "SHORT-LOSSCUT:$best_bid($profit)\n";
                    my $res_json;
                    if( buyMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $short_entry = 0;
                        $profit_sum += $profit;
                        $countdown = 50;
                    }else{
                        # 注文失敗
                        exit -1;
                    }
                }elsif( ($min >= $best_bid) || ($profit >= $shortProfit ) ){
                    # SHORT利確
                    print "SHORT-RIKAKU:$best_bid($profit)\n";
                    my $res_json;
                    if( buyMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $short_entry = 0;
                        $profit_sum += $profit;
                    }else{
                        # 注文失敗
                        exit -1;
                    }

                    if( ($min >= $best_bid)  && ($countdown == 0) ){
                        # ドテンLONGエントリー
                        print "DOTEN-LONG-ENTRY:$best_bid\n";
                        my $res_json;
                        if( buyMarket(\$res_json, $entry_retry_num)==0 ){
                            # 注文成功
                            # LONGポジションへ
                            $position = "LONG";
                            $long_entry = $best_bid;
                        }
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
                    my $res_json;
                    if( sellMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $long_entry = 0;
                        $profit_sum += $profit;
                        $countdown = 50;
                    }
                }elsif( ($max <= $best_ask) || ($profit >= $longProfit) ){
                    # LONG利確
                    print "LONG-RIKAKU:$best_ask($profit)\n";
                    my $res_json;
                    if( sellMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $long_entry = 0;
                        $profit_sum += $profit;
                    }

                    if( ($max <= $best_ask) && ($countdown == 0) ){
                        # ドテンSHORTエントリー
                        print "DOTEN-SHORT-ENTRY:$best_ask\n";
                        my $res_json;
                        if( sellMarket(\$res_json, $entry_retry_num)==0 ){
                            # 注文成功
                            # SHORTポジションへ
                            $position = "SHORT";
                            $short_entry = $best_ask;
                        }
                    }
                }
            }
        }

        #if($position ne $pre_position ){
            # 情報出力
            my $oldest = "";
            my $oldest_wrk = $tickerArray[0]->{"timestamp"};
            MyModule::UtilityTime::convertTimeGMTtoJST(\$oldest, $oldest_wrk);

            my $array_cnt = @tickerArray;
            my $info_str = sprintf("[%05d]: TID=%8d: BID=%7d: ASK=%7d: MIN=%7d: MAX=%7d: RNG=%7d(%5d): POS=%5s: PRF=%7d: DWN=%3d: SUM=%7d TIME=%s: \n"
                , $cycle_cnt
                , $tick_id
                , $best_bid
                , $best_ask
                , $min
                , $max
                , $range
                , ($max - $min)
                , $position
                , $profit
                , $countdown
                , $profit_sum
                , $oldest
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
        $pre_range = $range;

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
