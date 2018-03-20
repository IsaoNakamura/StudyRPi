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
my $execTrade = 0;


# パラメタ:ライフサイクル
# 100=約17秒
# 1000=170秒=約3分
my $range = 5000;#4000;#5000;
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
my $ema = 0;
my $ema_cnt = 0;
my $ema_tick_id = 0;

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
        my $cur_value = 0;
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
        $cur_value = int(($best_bid + $best_ask) / 2.0);
        

        # EMA
        #print "EMA calc start.\n";
        if( ($tick_id - $ema_tick_id) > 1800 ){
            # 一分間ごとに計算する
            $ema_cnt++;
            $ema = int($cur_value * 2 / ($ema_cnt+1) + $ema * ($ema_cnt+1-2) / ($ema_cnt + 1));
            $ema_tick_id = $tick_id;
        }
        #print "EMA calc end.\n";

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
        #$range = int(($max - $min) * $range_prm);
        #if($range_wrk > 300)
        #$X = $range / ($max - $min) = 9000 / 25000 = 0.36;
        my $isUpdateMinMax = 0;
        if(@tickerArray > $range ){
            $execTrade = 1;
            # Ticker配列がレンジ数を超えた場合

            # 先頭を削除
            my $shift_json = shift(@tickerArray);
            my $shift_bid = $shift_json->{"best_bid"};
            my $shift_ask = $shift_json->{"best_ask"};
            if($shift_ask == $max || $shift_bid == $min){
                # 削除したものがMAXまたはMINだった場合
                # MAX,MINを再計算する
                for(my $i=0; $i<@tickerArray; $i++){
                    my $elem_json = $tickerArray[$i];
                    my $elem_bid = $elem_json->{"best_bid"};
                    my $elem_ask = $elem_json->{"best_ask"};
                    if($i==0){
                        $max = $elem_ask;
                        $min = $elem_bid;
                        $isUpdateMinMax++;
                        next;
                    }
                    if($max < $elem_ask){
                        $max = $elem_ask;
                        $isUpdateMinMax++;
                    }
                    if($min > $elem_bid){
                        $min = $elem_bid;
                        $isUpdateMinMax++;
                    }
                }
            }
        }
        if($max < $best_ask){
            $max = $best_ask;
            $isUpdateMinMax++;
        }
        if($min > $best_bid){
            $min = $best_bid;
            $isUpdateMinMax++;
        }

        if($isUpdateMinMax>0){
            $countdown = $countNum;
        }

        my $profit = 0;
        my $emaRange = ($max - $min) / 4;
        if($execTrade==1){
            if($position eq "NONE" && $countdown == 0){
                my $noneRange = ($max - $min) / 8;
                if(abs($best_ask-$max) < $noneRange){
                    # MAXに値が近づいたら
                    if( ($best_ask - $ema) > $emaRange ){
                        # EMA値より上にあったら
                        # SHORTエントリー
                        print "ACK=SHORT-ENTRY,$best_ask\n";
                        my $res_json;
                        if( sellMarket(\$res_json, $entry_retry_num)==0 ){
                            # 注文成功
                            # SHORTポジションへ
                            $position = "SHORT";
                            $short_entry = $best_ask;
                        }
                    }
                }elsif(abs($best_bid-$min) < $noneRange){
                    # MINに値が近づいたら
                    if( ($ema - $best_bid) > $emaRange ){
                        # EMA値より下にあったら
                        # LONGエントリー
                        print "ACK=LONG-ENTRY,$best_ask\n";
                        my $res_json;
                        if( buyMarket(\$res_json, $entry_retry_num)==0 ){
                            # 注文成功
                            # LONGポジションへ
                            $position = "LONG";
                            $long_entry = $best_bid;
                        }
                    }
                }
            }elsif($position eq "SHORT"){
                $profit = $short_entry - $best_bid;
                my $shortRange = $short_entry - $min;
                my $shortProfit = $shortRange / 2;
                my $shortLC = $shortRange / 2;
                if( $profit <= -$shortLC ){
                    # SHORTロスカット
                    print "ACK=SHORT-LOSSCUT,$best_bid($profit)\n";
                    my $res_json;
                    if( buyMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $short_entry = 0;
                        $profit_sum += $profit;
                        $countdown = $countNum;
                    }else{
                        # 注文失敗
                        exit -1;
                    }
                }elsif( (abs($ema-$best_bid) < $emaRange) || ($min >= $best_bid) || ($ema >= $best_bid) ){
                #}elsif( abs($ema-$best_bid) < $emaRange ){
                    # EMAに近づいたら、または、MIN,EMA以下
                    # MIN以下、EMAに近づいたら
                    # SHORT利確
                    print "ACK=SHORT-RIKAKU,$best_bid($profit)\n";
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
                        if( ($ema - $best_bid) > $emaRange ){
                            # ドテンLONGエントリー
                            print "ACK=DOTEN-LONG-ENTRY,$best_bid\n";
                            my $res_json;
                            if( buyMarket(\$res_json, $entry_retry_num)==0 ){
                                # 注文成功
                                # LONGポジションへ
                                $position = "LONG";
                                $long_entry = $best_bid;
                            }
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
                    print "ACK=LONG-LOSSCUT,$best_ask($profit)\n";
                    my $res_json;
                    if( sellMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $long_entry = 0;
                        $profit_sum += $profit;
                        $countdown = $countNum;
                    }
                }elsif( (abs($ema-$best_ask) < $emaRange) || ($max <= $best_ask) || ($ema <= $best_ask) ){
                #}elsif( abs($ema-$best_ask) < $emaRange ){
                    # EMAに近づいたら、または、MAX,EMA以上
                    # LONG利確
                    print "ACK=LONG-RIKAKU,$best_ask($profit)\n";
                    my $res_json;
                    if( sellMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $long_entry = 0;
                        $profit_sum += $profit;
                    }

                    if( ($max <= $best_ask) && ($countdown == 0) ){
                        if( ($best_ask - $ema) > $emaRange ){
                            # ドテンSHORTエントリー
                            print "ACK=DOTEN-SHORT-ENTRY,$best_ask\n";
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
        }

        #if($position ne $pre_position ){
            # 情報出力
            my $oldest = "";
            my $oldest_wrk = $tickerArray[0]->{"timestamp"};
            MyModule::UtilityTime::convertTimeGMTtoJST(\$oldest, $oldest_wrk);
            
            my $short_entry = $ema;
            my $long_entry = $ema;
            if($position eq "SHORT"){
                $short_entry = $best_bid;
            }elsif($position eq "LONG"){
                $long_entry = $best_ask;
            }
            my $array_cnt = @tickerArray;
            my $info_str = sprintf("SEQ=%05d,TID=%8d,BID=%7d,ASK=%7d,MIN=%7d,MAX=%7d,EMA=%7d,SHORT=%7d,LONG=%7d,DIF=%5d,RNG=%5d,POS=%5s,PRF=%5d,DWN=%3d,SUM=%5d,TIME=%s\n"
                , $cycle_cnt
                , $tick_id
                , $best_bid
                , $best_ask
                , $min
                , $max
                , $ema
                , $short_entry
                , $long_entry
                , ($cur_value - $ema )
                , $emaRange
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
        $pre_tick_id = $tick_id;

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
