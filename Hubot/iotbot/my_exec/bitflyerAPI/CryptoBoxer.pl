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
my $rikaku_retry_num = 10;
my $entry_retry_num = 0;
my $execTrade = 0;

my $FAR_UNDER_LIMIT = 1000;


# パラメタ:ライフサイクル
# 100=約17秒
# 1000=170秒=約3分
my $range = 5000;#4000;#5000;

# パラメタ:MIN,MAX更新時の遊び時間
my $countNum = 100;
my $countdown = $countNum;

# 状態情報
my @tickerArray;
my $min = 0;
my $max = 0;
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
my $pre_min = 0;
my $pre_max = 0;
my $pre_ema = 0;
my $pre_value = 0;
my $pre_delta = 0;

my $max_keep = 0;
my $min_keep = 0;

my $stopCodeFile = "./StopCode.txt";
my $logFilePath = './CryptoBoxer.log';
open( OUT, '>',$logFilePath) or die( "Cannot open filepath:$logFilePath $!" );
my $header_str = "SEQ\tTID\tVAL\tMIN\tMAX\tEMA\tSHORT\tLONG\tDIF\tRNG\tPOS\tPRF\tDWN\tSUM\tRATE\tTIME\n";
print OUT $header_str;

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
        my $isNext = -1;
        eval{
            if( MyModule::UtilityBitflyer::getTicker(
                                                        \$res_json,
                                                        \$ua,
                                                        \$authBitflyer,
                                                        "FX_BTC_JPY"
                                                    )==0
            ){
                #next;
                $isNext = 0;
            }
        };
        if($isNext!=0){
            next;
        }
        
        $best_bid = $res_json->{"best_bid"}; # 買値
        $best_ask = $res_json->{"best_ask"}; # 売値(買値より高い)
        $tick_id = $res_json->{"tick_id"};
        my $volume = $res_json->{"volume_by_product"};
        $cur_value = $res_json->{"ltp"};
        my $total_bid_depth = $res_json->{"total_bid_depth"};
        my $total_ask_depth = $res_json->{"total_ask_depth"};
        my $timestamp_utc = $res_json->{"timestamp"};
        MyModule::UtilityTime::convertTimeGMTtoJST(\$timestamp, $timestamp_utc);

        if($pre_tick_id eq $tick_id){
            next;
        }
        push(@tickerArray, $res_json);
        
        # EMA
        my $isMinit = 0;
        if( ($tick_id - $ema_tick_id) > 1800 ){
            $isMinit = 1;
            # 一分間ごとに計算する
            $ema_cnt++;
            $ema = int($cur_value * 2 / ($ema_cnt+1) + $ema * ($ema_cnt+1-2) / ($ema_cnt + 1));
            $ema_tick_id = $tick_id;
        }

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

        my $delta = ($cur_value - $pre_value) + ($pre_delta * 0.5);
        my $rate = ($delta / (($max-$min) / 2)) * 100;

        #my $res_info = sprintf("CUR=%7d, DLT=%7d, RATE=%.2f\n"
        #                    ,$cur_value
        #                    ,$delta
        #                    ,$rate
        #                );
        #print $res_info;

        # トレード開始は一定数データをとってから
        if($execTrade==0){
            if($cycle_cnt > $range){
                print "START TRADE. cycle_cnt=$cycle_cnt, range=$range\n";
                $execTrade=1;
            }
        }

        my $isUpdateMinMax = 0;
        if(@tickerArray > $range ){
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

        # MIN/MAXを更新
        if($max < $best_ask){
            $max = $best_ask;
            $isUpdateMinMax++;
        }
        if($min > $best_bid){
            $min = $best_bid;
            $isUpdateMinMax++;
        }

        # 更新したらトレード無しサイクル数をセット
        if($isUpdateMinMax>0){
            $countdown = $countNum;
        }

        my $profit = 0;
        my $maxminNear = ($max - $min) / 8;
        my $shortEmaFar = ($max - $ema) / 2;
        my $shortEmaNear = $shortEmaFar / 4;
        my $longEmaFar = ($ema - $min) / 2;
        my $longEmaNear = $longEmaFar / 4;

        if($position eq "NONE" && $countdown == 0){
            if(($max-$best_ask) < $maxminNear){
                $max_keep++;
            }else{
                $max_keep--;
                if($max_keep<0){
                    $max_keep = 0;
                }
            }
            if(($best_bid-$min) < $maxminNear){
                $min_keep++;
            }else{
                $min_keep--;
                if($min_keep<0){
                    $min_keep = 0;
                }
            }
        }else{
            $max_keep = 0;
            $min_keep = 0;
        }

        if($execTrade==1){
            if($position eq "NONE" && $countdown == 0){
                if( 
                    ($shortEmaFar > $FAR_UNDER_LIMIT ) && 
                    (($best_ask - $ema) > $shortEmaFar) && 
                    (($max-$best_ask) < $maxminNear) &&
                    ($max_keep >= 50)
                ){
                    # EMA値より一定値上にあったら
                    # SHORTエントリー
                    print "ACK=SHORT-ENTRY, ASK=$best_ask, EMA=$ema, Far=$shortEmaFar\n";
                    my $res_json;
                    if( sellMarket(\$res_json, $entry_retry_num)==0 ){
                        # 注文成功
                        # SHORTポジションへ
                        $position = "SHORT";
                        $short_entry = $best_ask;
                    }
                }elsif(
                    ($longEmaFar > $FAR_UNDER_LIMIT ) &&
                    (($ema - $best_bid) > $longEmaFar) &&
                    (($best_bid-$min) < $maxminNear) &&
                    ($min_keep > 50)
                ){
                    # EMA値より一定値下にあったら
                    # LONGエントリー
                    print "ACK=LONG-ENTRY, BID=$best_bid, EMA=$ema, FAR=$longEmaFar\n";
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
                my $shortLC = $shortEmaFar + $shortEmaFar / 4;
                if( $profit <= -$shortLC ){
                    # SHORTロスカット
                    print "ACK=SHORT-LOSSCUT, BID=$best_bid, PRF=$profit, LC=$shortLC\n";
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
                        last;
                    }
                }elsif( 
                    (abs($ema-$best_bid) < $shortEmaNear) || 
                    #($min >= $best_bid) || 
                    ($ema >= $best_bid) 
                ){
                    # EMAに近づいたら、または、MIN,EMA以下
                    # MIN以下、EMAに近づいたら
                    # SHORT利確
                    my $length = abs($ema-$best_bid);
                    print "ACK=SHORT-RIKAKU, BID=$best_bid, PRF=$profit, MIN=$min, EMA=$ema, LNG=$length, NEAR=$shortEmaNear\n";
                    my $res_json;
                    if( buyMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $short_entry = 0;
                        $profit_sum += $profit;
                    }else{
                        # 注文失敗
                        last;
                    }

                    if(
                        ($longEmaFar > $FAR_UNDER_LIMIT ) &&
                        (($ema - $best_bid) > $longEmaFar) &&
                        ($countdown == 0)
                    ){
                        # ドテンLONGエントリー
                        print "ACK=DOTEN-LONG-ENTRY, BID=$best_bid, EMA=$ema, FAR=$longEmaFar\n";
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
                my $longLC = $longEmaFar + $longEmaFar / 4;
                if( $profit <= -$longLC ){
                    # LONGロスカット
                    print "ACK=LONG-LOSSCUT, ASK=$best_ask, PRF=$profit, LC=$longLC\n";
                    my $res_json;
                    if( sellMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $long_entry = 0;
                        $profit_sum += $profit;
                        $countdown = $countNum;
                    }
                }elsif(
                    (abs($ema-$best_ask) < $longEmaNear) ||
                    #($max <= $best_ask) ||
                    ($ema <= $best_ask)
                ){
                    # EMAに近づいたら、または、MAX,EMA以上
                    # LONG利確
                    my $length = abs($ema-$best_ask);
                    print "ACK=LONG-RIKAKU, ASK=$best_ask, PRF=$profit, MAX=$max, EMA=$ema, LNG=$length, NEAR=$longEmaNear\n";
                    my $res_json;
                    if( sellMarket(\$res_json, $rikaku_retry_num)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $long_entry = 0;
                        $profit_sum += $profit;
                    }

                    if(
                        ($shortEmaFar > $FAR_UNDER_LIMIT ) &&
                        (($best_ask - $ema) > $shortEmaFar) &&
                        ($countdown == 0)
                    ){
                        # ドテンSHORTエントリー
                        print "ACK=DOTEN-SHORT-ENTRY, ASK=$best_ask, EMA=$ema, FAR=$shortEmaFar\n";
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


        # 情報出力
        my $oldest = "";
        my $oldest_wrk = $tickerArray[0]->{"timestamp"};
        MyModule::UtilityTime::convertTimeGMTtoJST(\$oldest, $oldest_wrk);
        
        my $short_entry = $ema;
        my $long_entry = $ema;
        my $near = 0;
        if($position eq "SHORT"){
            $short_entry = $best_bid;
            $near = $shortEmaNear;
        }elsif($position eq "LONG"){
            $long_entry = $best_ask;
            $near = $longEmaNear;
        }

        if(
            ($position ne $pre_position) ||
            ($pre_min != $pre_min) ||
            ($pre_max != $pre_max) ||
            ($pre_ema != $pre_ema) ||
            ($isMinit > 0)
        ){
            my $log_str = sprintf("%05d\t%8d\t%7d\t%7d\t%7d\t%7d\t%7d\t%7d\t%5d\t%5d\t%5s\t%5d\t%3d\t%5d\t%5.1f\t%2d\t%2d\t%s\t%s\n"
                , $cycle_cnt
                , $tick_id
                , $cur_value
                , $min
                , $max
                , $ema
                , $short_entry
                , $long_entry
                , ($cur_value - $ema )
                , $near
                , $position
                , $profit
                , $countdown
                , $profit_sum
                , $rate
                , $max_keep
                , $min_keep
                , $oldest
                , $timestamp
            );
            print OUT $log_str;

            my $info_str = sprintf("SEQ=%05d,CUR=%7d,MIN=%7d,MAX=%7d,EMA=%7d,DIF=%5d,POS=%5s,PRF=%5d,DWN=%3d,SUM=%5d,RATE=%5.1f,XKP=%2d,NKP=%2d,TIME=%s\n"
                , $cycle_cnt
                , $cur_value
                , $min
                , $max
                , $ema
                , ($cur_value - $ema )
                , $position
                , $profit
                , $countdown
                , $profit_sum
                , $rate
                , $max_keep
                , $min_keep
                , $timestamp
            ); 
            print $info_str;
        }

        # カウントダウン
        if($countdown!=0){
            $countdown--;
            if($countdown<0){
                $countdown = 0;
            }
        }

        # 前値保存
        $pre_position = $position;
        $pre_tick_id = $tick_id;
        $pre_min = $min;
        $pre_max = $max;
        $pre_ema = $ema;
        $pre_value = $cur_value;
        $pre_delta = $delta;

    #};

    if(-e $stopCodeFile){
        print "recieved stopCode:$stopCodeFile\n";
        unlink $stopCodeFile;
        last;
    }

    sleep($cycle_sec);
    $cycle_cnt++;
}

close OUT;

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
