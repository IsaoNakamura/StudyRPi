#!/usr/bin/perl

use strict;
use warnings;

use utf8;
use Encode;

use Encode 'decode';
use Encode 'encode';

use JSON;

use LWP::UserAgent;

use Furl;
use HTTP::Request::Common;

use HTTP::Date;

use MyModule::UtilityJson;
use MyModule::UtilityBitflyer;
use MyModule::UtilityTime;
use MyModule::UtilityCryptowatch;

my $authBitflyerFilePath = "./AuthBitflyer.json";
my $authSlackFilePath = "./AuthSlack.json";
my $ammount = 0.005;

if(!(-f $authBitflyerFilePath)){
    print "not exists AuthFile. $authBitflyerFilePath\n";
    exit -1;
}

if(!(-f $authSlackFilePath)){
    print "not exists AuthFile. $authSlackFilePath\n";
    exit -1;
}

my $authBitflyer;
if(MyModule::UtilityJson::readJson(\$authBitflyer, $authBitflyerFilePath)!=0){
    print "FileReadError. Auth of Bitflyer.\n";
    exit -1;
}

my $authSlack;
if(MyModule::UtilityJson::readJson(\$authSlack, $authSlackFilePath)!=0){
    print "FileReadError. Auth of Slack.\n";
    exit -1;
}

my $ua = new LWP::UserAgent;
$ua->timeout(10); # default: 180sec
$ua->ssl_opts( verify_hostname => 0 ); # skip hostname verification

# パラメタ
my $CYCLE_SLEEP = 0;
my $RIKAKU_RETRY_NUM = 0;
my $ENTRY_RETRY_NUM = 0;

my $FAR_UNDER_LIMIT = 1500;
my $LC_RATE = 0.005;

# エントリーしてから利確する時間が長引いた場合の処理用
# 単位は分
my $RIKAKU_LIMIT_TIME = 30;

# MIN/MAX位置を一定期間キープできるかの判断に使用
my $KEEP_LIMIT = 50;

my $DELTA_LIMIT = 2000;

my $PROFIT_LIMIT = 5000;

my $PROFIT_MAX = 10000;


# パラメタ:ライフサイクル
# 100=約17秒
# 1000=170秒=約3分
my $RANGE = 15;#4000;#5000;
my $CANDLE_BUF = 60;

# パラメタ:MIN,MAX更新時の遊び時間
my $COUNTUP = 120;

my $VIX_CNTUP = 600;

my $EMA_SAMPLE_NUM = 60;
my $CANDLE_LIMIT = 200;

# 状態情報
my $minmax_cntdwn = 0;
my $vix_cntdwn = 0;
my $min = 0;
my $max = 0;
my $short_entry = 0;
my $long_entry = 0;
my $profit_sum = 0;
my $ema = 0;
my $short_tick = 0;
my $long_tick = 0;
my $execTrade = 0;

# ポジション
my $position = "NONE";
my $pre_position = $position;

# 前値保存用
my $pre_tick_id = 0;
my $pre_min = 0;
my $pre_max = 0;
my $pre_ema = 0;
my $pre_value = 0;

my $pre_time;

my $max_keep = 0;
my $min_keep = 0;

my $high_value = 0;
my $low_value  = 0;

my $highest_last = 0;
my $stddev = 0;
my $ma = 0;
my $boll_high = 0;
my $boll_low = 0;
my $rangeHigh = 0;

my $isVIX = 0;
my $pre_isVIX = 0;

my @candleArray = ();

my $begin_time = "";
if(MyModule::UtilityTime::getTime(\$begin_time)!=0){
    print "failed to getDate()\n";
}

my $stopCodeFile = "./StopCode.txt";

my $logFilePath = "./CryptoBoxer_$begin_time.log";
open( OUT, '>',$logFilePath) or die( "Cannot open filepath:$logFilePath $!" );
my $header_str = "SEQ\tTID\tVAL\tMIN\tMAX\tSHORT\tLONG\tEMA\tVIX\tDIF\tRNG\tPOS\tPRF\tDWN\tSUM\tVDWN\tXKP\tNKP\tTIME\n";
print OUT $header_str;

# 過去のキャンドルをCryptoWatchから取得
{
    my $symbol    = "btcfxjpy";
    my $periods   = 60; # 1分足
    my $after     = time() - ($CANDLE_BUF * 60); # CANDLE_BUF分前の値を取得
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
        print "failed to getCandleStickAfter from Cryptowatch. \n";
        exit -1;
    }

    my $candleArray_ref = $res_json->{"result"}->{"$periods"};
    for(my $i=0; $i<@{$candleArray_ref}; $i++){
        my $candle = $candleArray_ref->[$i];
        #        [0],       [1],       [2],      [3],        [4],    [5]
        #[ CloseTime, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume ]
        my $close_time  = $candle->[0];
        my $high_price  = $candle->[2];
        my $low_price   = $candle->[3];
        my $close_price = $candle->[4];

        # VIX算出
        my $vix = 0;
        my $highest_last = 0;
        getHighestCandle(\$highest_last, "LAST", \@candleArray, 22);
        if($highest_last>0){
            $vix = (($highest_last - $low_price) / $highest_last) * 100;
        }

        # キャンドルオブジェクト追加
        my %candle = (
            "TIME"      => $close_time,
            "LAST"      => $close_price,
            "HIGH"      => $high_price,
            "LOW"       => $low_price,
            "VIX"       => $vix
        );
        push(@candleArray, \%candle);
    }

    my $lowest_low = 0;
    my $highest_high = 0;
    getLowHighestCandle(\$lowest_low, \$highest_high, \@candleArray, $RANGE);
    if($lowest_low!=0){
        $min = $lowest_low;
    }
    if($highest_high!=0){
        $max = $highest_high;
    }
}

print "min=$min, max=$max\n";
#exit 0;

# メインループ
postSlack("IDLE-START. $begin_time\n");
my $cycle_cnt =0;
while(1){

    # 1分間に更新
    my $isMinit = 0;
    my $time = localtime();
    if($cycle_cnt==0){
        $pre_time = $time;
    }else{
        my $now = str2time($time);
        my $pre = str2time($pre_time);
        my $diff = $now - $pre;
        if($diff>=60){
            #print "diff=$diff, $time, $pre_time\n";
            $pre_time = $time;
            $isMinit = 1;
        }
    }

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
        $cur_value = $res_json->{"ltp"};
        #my $volume = $res_json->{"volume_by_product"};
        #my $total_bid_depth = $res_json->{"total_bid_depth"};
        #my $total_ask_depth = $res_json->{"total_ask_depth"};
        my $timestamp_utc = $res_json->{"timestamp"};
        MyModule::UtilityTime::convertTimeGMTtoJST(\$timestamp, $timestamp_utc);

        if($pre_tick_id eq $tick_id){
            next;
        }

        # 1分足キャンドル用最高低値更新
        if($high_value==0){
            $high_value = $cur_value;
        }elsif($high_value < $cur_value){
            $high_value = $cur_value;
        }
        if($low_value==0){
            $low_value = $cur_value;
        }elsif($low_value > $cur_value){
            $low_value = $cur_value;
        }

        # MIN/MAXを更新
        my $isUpdateMinMax = 0;
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
            $minmax_cntdwn = $COUNTUP;
        }

        # EMA
        $ema = getEmaCandle($cur_value, \@candleArray, $EMA_SAMPLE_NUM);

        # 一分間ごとに計算する
        if( $isMinit > 0 ){
            # 終値過去最大値を取得
            getHighestCandle(\$highest_last, "LAST", \@candleArray, 22);

            # VIX値の移動平均を取得
            getStdevCandle(\$stddev, \$ma, "VIX", \@candleArray, 20);
            $boll_high = $ma + (2.0*$stddev);
            $boll_low  = $ma - (2.0*$stddev);

            # VIX値の過去最大を取得
            getHighestCandle(\$rangeHigh, "VIX", \@candleArray, 50);
            $rangeHigh = $rangeHigh * 0.85;
        }


        # VIX
        my $wvf = 0;
        {   
            if($highest_last > $high_value){
                $wvf = (($highest_last - $low_value) / $highest_last) * 100;
            }else{
                $wvf = (($high_value - $low_value) / $high_value) * 100;
            }

            if($wvf>0){
                if( ($wvf >= $boll_high) || ($wvf >= $rangeHigh) ){
                    if($execTrade>0){
                        $isVIX = 1;
                        if($wvf >= 1.0){
                            $vix_cntdwn = int($VIX_CNTUP*$wvf);
                        }else{
                            $vix_cntdwn = $VIX_CNTUP;
                        }
                        if($pre_isVIX==0){
                            my $vix_str = sprintf("VIX ALERT ON!! wvf=%3.1f\n",$wvf);
                            postSlack($vix_str);
                        }
                    }
                }else{
                    if($execTrade>0){
                        if($pre_isVIX>0){
                            if($vix_cntdwn==0){
                                $isVIX = 0;
                                postSlack("VIX ALERT OFF.\n");
                            }else{
                                $isVIX = 1;
                                #postSlack("VIX ALERT KEEP. cntdwn=$vix_cntdwn\n");
                            }
                        }
                    }
                }
            }
        }

        if( $isMinit > 0 ){
            # 一分間ごとに計算する

            # キャンドルオブジェクト追加
            my %candle = (
                "TIME"      => $timestamp_utc,
                "LAST"      => $cur_value,
                "HIGH"      => $high_value,
                "LOW"       => $low_value,
                "VIX"       => $wvf
            );
            push(@candleArray, \%candle);

            # HIGH/LOWをリセット
            $high_value = 0;
            $low_value = 0;

            # トレード開始
            if($execTrade==0 ){
                $execTrade=1; 
                my $start_msg = sprintf("START TRADE. cycle_cnt=%d, range=%d\n",$cycle_cnt,$RANGE);
                postSlack($start_msg);
            }
        }

        my $profit = 0;
        my $maxminNear = ($max - $min) / 5;
        my $shortEmaFar = abs($max - $ema) / 2;
        my $shortEmaNear = $shortEmaFar / 4;
        my $longEmaFar = abs($ema - $min) / 2;
        my $longEmaNear = $longEmaFar / 4;

        # MIN/MAX付近での定着指数を算出
        if($position eq "NONE"  && ($minmax_cntdwn == 0) ){
            if( (($max-$best_ask) < $maxminNear)){
                $max_keep++;
            }else{
                $max_keep--;
                if($max_keep<0){
                    $max_keep = 0;
                }
            }
            if( (($best_bid-$min) < $maxminNear) ){
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

        # トレードロジック
        if($execTrade==1){
            if($position eq "NONE" ){
                if($isVIX <= 0){
                    # VIX-OFF
                    if( # SHORTエントリー条件
                        ($best_ask > $ema)                  &&   # 売値がEMAより大きい
                        #($shortEmaFar > $FAR_UNDER_LIMIT )  &&   # 売値とEMAの差が最小値より大きい
                        #(($best_ask - $ema) > $shortEmaFar) &&   # 売値とEMAが一定値より遠い
                        (($best_ask - $ema) > $FAR_UNDER_LIMIT) &&   # 売値とEMAが一定値より遠い
                        (($max-$best_ask) < $maxminNear)    &&   # 売値とMAXが一定値より近い
                        ($max_keep >= $KEEP_LIMIT)          &&   # 売値がMAX付近を一定時間維持
                        ($minmax_cntdwn == 0)               
                    ){
                        # SHORTエントリー
                        my $res_json;
                        if( sellMarket(\$res_json, $ENTRY_RETRY_NUM)==0 ){
                            # 注文成功
                            postSlack("SHORT-ENTRY, ASK=$best_ask, EMA=$ema, Far=$shortEmaFar\n");
                            # SHORTポジションへ
                            $position = "SHORT";
                            $short_entry = $best_ask;
                            $short_tick = $tick_id;
                        }else{
                            postSlack("SHORT-ENTRY IS FAILED!!\n");
                        }
                    }elsif( # LONGエントリー条件
                        ($best_bid < $ema)                 &&   # EMAが買値より大きい
                        #($longEmaFar > $FAR_UNDER_LIMIT )  &&   # EMAと買値の差が最小値より大きい
                        #(($ema - $best_bid) > $longEmaFar) &&   # EMAと買値が一定値より遠い
                        (($ema - $best_bid) > $FAR_UNDER_LIMIT) &&   # EMAと買値が一定値より遠い
                        (($best_bid-$min) < $maxminNear)   &&   # 買値とMINが一定値より近い
                        ($min_keep > $KEEP_LIMIT)          &&   # 買値がMIN付近を一定時間維持
                        ($minmax_cntdwn == 0)               
                    ){
                        # LONGエントリー
                        my $res_json;
                        if( buyMarket(\$res_json, $ENTRY_RETRY_NUM)==0 ){
                            # 注文成功
                            postSlack("LONG-ENTRY, BID=$best_bid, EMA=$ema, FAR=$longEmaFar\n");
                            # LONGポジションへ
                            $position = "LONG";
                            $long_entry = $best_bid;
                            $long_tick = $tick_id;
                        }else{
                            postSlack("LONG-ENTRY IS FAILED!!\n");
                        }
                    }
                }
            }elsif($position eq "SHORT"){
                $profit = $short_entry - $best_bid;
                my $shortLC = $short_entry * (1.0 + $LC_RATE);
                if( 
                    ($best_bid >= $shortLC) ||
                    ( ($isVIX > 0) && ($profit < 0) )
                ){
                    # SHORTロスカット
                    my $res_json;
                    if( buyMarket(\$res_json, $RIKAKU_RETRY_NUM)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $short_entry = 0;
                        $short_tick = 0;
                        $profit_sum += $profit;
                        postSlack("SHORT-LOSSCUT, PRF=$profit, SUM=$profit_sum, BID=$best_bid, LC=$shortLC\n");
                    }else{
                        # 注文失敗
                        postSlack("SHORT-LOSSCUT IS FAILED!!\n");
                        #last;
                    }
                }elsif( 
                    (abs($ema-$best_bid) < $shortEmaNear) || 
                    # ($min >= $best_bid) || 
                    ($ema >= $best_bid) ||
                    # ($profit >= $PROFIT_MAX) ||
                    ( abs($tick_id-$short_tick) > (1800*$RIKAKU_LIMIT_TIME) && ($profit >= $PROFIT_LIMIT ) )
                ){
                    # EMAに近づいたら、または、MIN,EMA以下
                    # MIN以下、EMAに近づいたら
                    # SHORT利確
                    my $length = abs($ema-$best_bid);
                    
                    my $res_json;
                    if( buyMarket(\$res_json, $RIKAKU_RETRY_NUM)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $short_entry = 0;
                        $short_tick = 0;
                        $profit_sum += $profit;
                        postSlack("SHORT-RIKAKU, PRF=$profit, SUM=$profit_sum, BID=$best_bid, MIN=$min, EMA=$ema, LEN=$length, NEAR=$shortEmaNear\n");
                    }else{
                        # 注文失敗
                        postSlack("SHORT-RIKAKU IS FAILED!!\n");
                        #last;
                    }
                }
            }elsif($position eq "LONG"){
                $profit = $best_ask - $long_entry;
                my $longLC = $long_entry * (1.0 - $LC_RATE);
                if(
                    ($best_ask <= $longLC) ||
                    ( ($isVIX > 0) && ($profit < 0) )
                ){
                    # LONGロスカット
                    my $res_json;
                    if( sellMarket(\$res_json, $RIKAKU_RETRY_NUM)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $long_entry = 0;
                        $long_tick = 0;
                        $profit_sum += $profit;
                        postSlack("LONG-LOSSCUT, PRF=$profit, SUM=$profit_sum, ASK=$best_ask, LC=$longLC\n");
                    }else{
                        postSlack("LONG-LOSSCUT IS FAILED!!\n");
                        #last;
                    }
                }elsif(
                    (abs($ema-$best_ask) < $longEmaNear) ||
                    # ($max <= $best_ask) ||
                    ($ema <= $best_ask) ||
                    #($profit >= $PROFIT_MAX) ||
                    ( abs($tick_id-$long_tick) > (1800*$RIKAKU_LIMIT_TIME) && ($profit >= $PROFIT_LIMIT ) )
                ){
                    # EMAに近づいたら、または、MAX,EMA以上
                    # LONG利確
                    my $length = abs($ema-$best_ask);
                    my $res_json;
                    if( sellMarket(\$res_json, $RIKAKU_RETRY_NUM)==0 ){
                        # 注文成功
                        # ノーポジションへ
                        $position = "NONE";
                        $long_entry = 0;
                        $long_tick = 0;
                        $profit_sum += $profit;
                        postSlack("LONG-RIKAKU, PRF=$profit, SUM=$profit_sum, ASK=$best_ask, MAX=$max, EMA=$ema, LEN=$length, NEAR=$longEmaNear\n");
                    }else{
                        postSlack("LONG-RIKAKU IS FAILED!!\n");
                        #last;
                    }
                }
            }
        }

        # 情報出力
        my $short = $ema;
        my $long = $ema;
        my $near = 0;
        if( ($position eq "SHORT") || ($position eq "SHORT_VIX") ){
            $short = $best_bid;
            $near = $shortEmaNear;
        }elsif($position eq "LONG"){
            $long = $best_ask;
            $near = $longEmaNear;
        }

        if(
            ($position ne $pre_position) ||
            ($min != $pre_min) ||
            ($max != $pre_max) ||
            #($ema != $pre_ema) ||
            ($isVIX != $pre_isVIX) ||
            #($max_keep != 0) ||
            #($min_keep != 0) ||
            ($isMinit > 0)
            #1
        ){
            my $log_str = sprintf("%05d\t%8d\t%7d\t%7d\t%7d\t%7d\t%7d\t%7d\t%5.1f\t%5d\t%5d\t%5s\t%5d\t%3d\t%5d\t%5d\t%2d\t%2d\t%s\n"
                , $cycle_cnt
                , $tick_id
                , $cur_value
                , $min
                , $max
                , $short
                , $long
                , $ema
                , ($wvf*$isVIX)
                , ($cur_value - $ema )
                , $near
                , $position
                , $profit
                , $minmax_cntdwn
                , $profit_sum
                , $vix_cntdwn
                , $max_keep
                , $min_keep
                , $timestamp
            );
            print OUT $log_str;

            my $info_str = sprintf("SEQ=%05d,CUR=%7d,MIN=%7d,MAX=%7d,EMA=%7d,VIX=%5.1f,DIF=%5d,POS=%5s,PRF=%5d,DWN=%3d,SUM=%5d,VDWN=%5d,XKP=%2d,NKP=%2d,TIME=%s\n"
                , $cycle_cnt
                , $cur_value
                , $min
                , $max
                , $ema
                , ($wvf*$isVIX)
                , ($cur_value - $ema )
                , $position
                , $profit
                , $minmax_cntdwn
                , $profit_sum
                , $vix_cntdwn
                , $max_keep
                , $min_keep
                , $timestamp
            ); 
            print $info_str;
        }

        # カウントダウン
        if($minmax_cntdwn!=0){
            $minmax_cntdwn--;
            if($minmax_cntdwn<0){
                $minmax_cntdwn = 0;
            }
        }
        if($vix_cntdwn!=0){
            $vix_cntdwn--;
            if($vix_cntdwn<0){
                $vix_cntdwn = 0;
            }
        }

        # 前値保存
        $pre_position = $position;
        $pre_tick_id = $tick_id;
        $pre_min = $min;
        $pre_max = $max;
        $pre_ema = $ema;
        $pre_value = $cur_value;
        $pre_isVIX = $isVIX;

        if($isMinit>0){
            # キャンドル追加に伴い、MIN,MAXの更新
            my $shift_idx = (@candleArray - $RANGE) - 1;
            if($shift_idx>=0){
                my $shift_candle = $candleArray[$shift_idx];
                if( $shift_candle->{"HIGH"}==$max || $shift_candle->{"LOW"}==$min ){
                    my $lowest_low = 0;
                    my $highest_high = 0;
                    getLowHighestCandle(\$lowest_low, \$highest_high, \@candleArray, $RANGE);
                    if($lowest_low!=0){
                        $min = $lowest_low;
                    }
                    if($highest_high!=0){
                        $max = $highest_high;
                    }
                }
            }
        }

        # Candle配列がバッファ数を超えた場合
        if(@candleArray > $CANDLE_BUF ){
            # 先頭を削除
            my $shift_candle = shift(@candleArray);
        }
    #};

    if(-e $stopCodeFile){
        postSlack("recieved stopCode. PROFIT_SUM=$profit_sum\n");

        my %candleHash =   ( "candleArray" => \@candleArray );
        if(MyModule::UtilityJson::writeJson(\\%candleHash, "./candleHash_$begin_time.json", ">")!=0){
            print "FileWriteError. candleHash.\n";
        }

        unlink $stopCodeFile;
        last;
    }

    sleep($CYCLE_SLEEP);
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

sub postSlack{
    my $post_text = shift;
    print $post_text;

    my $req = POST ($authSlack->{"host"},
        'Content' => [
            token    => $authSlack->{"token"},
            channel  => $authSlack->{"channel"},
            username => $authSlack->{"username"},
            icon_url => $authSlack->{"icon_url"},
            text     => $post_text
        ]);
    my $res = Furl->new->request($req);
}

sub getStdevCandle{
    my $stddev_ref = shift;
    my $ma_ref = shift;
    my $attr_str = shift;
    my $candleArray_ref = shift;
    my $sample_num = shift;

    my $beg_idx = 0;
    my $array_cnt = @{$candleArray_ref};
    if($array_cnt >= $sample_num){
        $beg_idx = $array_cnt - $sample_num;
    }

    my $value_sum = 0;
    my $square_sum = 0;
    my $elem_cnt = 0;
    for(my $i=$beg_idx; $i<$array_cnt; $i++){
        $elem_cnt++;
        my $elem = $candleArray_ref->[$i];
        my $elem_value = $elem->{$attr_str};

        $value_sum+=$elem_value;
        $square_sum+=($elem_value*$elem_value);
    }

    if($elem_cnt>0){
        $$stddev_ref = sqrt( ( ($elem_cnt * $square_sum) - ($value_sum * $value_sum) ) / ($elem_cnt * ($elem_cnt + 1) ) );
        $$ma_ref = $value_sum / $elem_cnt;
    }else{
        $$stddev_ref = 0;
        $$ma_ref = 0;
    }

    return(0);
}

sub getHighestCandle{
    my $highest_ref = shift;
    my $attr_str = shift;
    my $candleArray_ref = shift;
    my $sample_num = shift;

    my $beg_idx = 0;
    my $array_cnt = @{$candleArray_ref};
    if($array_cnt >= $sample_num){
        $beg_idx = $array_cnt - $sample_num;
    }

    my $elem_cnt = 0;
    my $highest = 0;
    for(my $i=$beg_idx; $i<$array_cnt; $i++){
        $elem_cnt++;
        my $elem = $candleArray_ref->[$i];
        my $elem_value = $elem->{$attr_str};

        if($i==$beg_idx){
            $highest = $elem_value;
        }else{
            if($highest < $elem_value){
                $highest = $elem_value;
            }
        }
    }
    $$highest_ref = $highest;

    return(0);
}

sub getLowestCandle{
    my $lowest_ref = shift;
    my $attr_str = shift;
    my $candleArray_ref = shift;
    my $sample_num = shift;

    my $beg_idx = 0;
    my $array_cnt = @{$candleArray_ref};
    if($array_cnt >= $sample_num){
        $beg_idx = $array_cnt - $sample_num;
    }

    my $elem_cnt = 0;
    my $lowest = 0;
    for(my $i=$beg_idx; $i<$array_cnt; $i++){
        $elem_cnt++;
        my $elem = $candleArray_ref->[$i];
        my $elem_value = $elem->{$attr_str};

        if($i==$beg_idx){
            $lowest = $elem_value;
        }else{
            if($lowest > $elem_value){
                $lowest = $elem_value;
            }
        }
    }
    $$lowest_ref = $lowest;

    return(0);
}

sub getLowHighestCandle{
    my $lowest_ref = shift;
    my $highest_ref = shift;
    my $candleArray_ref = shift;
    my $sample_num = shift;

    my $beg_idx = 0;
    my $array_cnt = @{$candleArray_ref};
    if($array_cnt >= $sample_num){
        $beg_idx = $array_cnt - $sample_num;
    }

    my $elem_cnt = 0;
    my $lowest = 0;
    my $highest = 0;
    for(my $i=$beg_idx; $i<$array_cnt; $i++){
        $elem_cnt++;
        my $elem = $candleArray_ref->[$i];
        my $elem_low = $elem->{"LOW"};
        my $elem_high = $elem->{"HIGH"};

        if($i==$beg_idx){
            $lowest = $elem_low;
            $highest = $elem_high;
        }else{
            if($lowest > $elem_low){
                $lowest = $elem_low;
            }
            if($highest < $elem_high){
                $highest = $elem_high;
            }
        }
    }
    $$lowest_ref = $lowest;
    $$highest_ref = $highest;

    return(0);
}

sub getEmaCandle{
    my $cur_value       = shift;
    my $candleArray_ref = shift;
    my $sample_num      = shift;

    my $beg_idx = 0;
    if(@{$candleArray_ref} >= $sample_num){
        $beg_idx = @{$candleArray_ref} - $sample_num;
    }
    my $last_ema = 0;
    my $elem_cnt = 0;
    for(my $i=$beg_idx; $i<@{$candleArray_ref}; $i++){
        $elem_cnt++;
        my $elem = $candleArray_ref->[$i];
        my $elem_value = $elem->{"LAST"};
        my $elem_ema = int($elem_value * 2 / ($elem_cnt+1) + $last_ema * ($elem_cnt+1-2) / ($elem_cnt + 1));
        $last_ema = $elem_ema;
    }
    $elem_cnt++;

    my $ema = int($cur_value * 2 / ($elem_cnt+1) + $last_ema * ($elem_cnt+1-2) / ($elem_cnt + 1));

    return($ema);
}


1;
