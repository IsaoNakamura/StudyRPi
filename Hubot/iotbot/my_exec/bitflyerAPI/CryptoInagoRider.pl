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

my $driver = Selenium::Remote::Driver->new(
    remote_server_addr => '127.0.0.1',
    port => 9999,
);

my $res = $driver->get('https://inagoflyer.appspot.com/btcmac');
sleep(5);

my $title = $driver->get_title();
$title = encode('Shift_JIS', $title);
print "$title\n";

#my $elements = $driver->find_elements("#chart_table_tbody input[type='checkbox']", "css");
#for(my $i=1; $i<@{$elements}; $i++){
#    my $elem_id = $elements->[$i]->get_attribute('id');
#    if($elem_id eq "bitFlyer_BTCJPY_checkbox"){
#        next;
#    }
#    $elements->[$i]->click();
#    print "[$i]:" . $elem_id . " is clicked\n";
#}

# パラメタ
my $cycle_sec = 3;
my $rikaku_retry_num = 3;
my $entry_retry_num = 0;
my $force = 0.7;
my $threshold = 80.0;
my $execTrade = 0;

# イナゴフライヤー取得部品ID
my $sell_id = "sellVolumePerMeasurementTime";
my $buy_id = "buyVolumePerMeasurementTime";

# インジケータ
my $indicator = 0.0;

# 前値保存用
my $pre_effective = 0.0;
my $pre_indicator = 0.0;

# ポジション
my $position = "NONE";

# メインループ
my $cycle_cnt =0;
while(1){
    eval{
        # イナゴフライヤーの値を取得
        my $sell_volume = $driver->find_element_by_id($sell_id)->get_text;
        my $buy_volume = $driver->find_element_by_id($buy_id)->get_text;

        # 実効値算出
        my $effective = $buy_volume - $sell_volume;
    
        # インジケータの値を算出(0に戻ろうとする特性をもつ(1サイクルにつきforce%半減))
        if(abs($indicator) < 0.0001){
            # 0なら維持
            $indicator = 0.0;
        }else{
            # 減衰
            $indicator = $indicator * $force;
        }
        $indicator+=$effective;

        # インジケータの前回との比率を算出
        my $rate = 0.0;
        my $delta = 0.0;
        if(abs($pre_indicator) > 0.0001){
            
            
            #$rate = $delta / $pre_indicator * 100.0;
            $delta = $indicator - $pre_indicator;
            if($pre_indicator<0.0 && $indicator>=0.0){
                $rate =  $delta / $indicator * 100.0;
            }elsif($pre_indicator>=0.0 && $indicator<0.0){
                $rate =  $delta / $pre_indicator * 100.0;
            }elsif($pre_indicator>=0.0 && $indicator>=0.0){
                $rate = $delta / $indicator * 100.0;
            }elsif($pre_indicator<0.0 && $indicator<0.0){
                $rate = $delta / $indicator * 100.0 * -1.0;
            }
        }

        # トレードロジック
        if( ($cycle_cnt>=4) && ($execTrade==1) ){
            if( $position eq "LONG" ){
                # LONGポジションの場合
                if( $indicator > 0 && $pre_indicator > 0){
                    # 前フレームから同じトレンド方向(上げ)の場合
                    if($rate > $threshold){
                        # LONG維持
                    }else{
                        # LONG利確
                        print "LONG-RIKAKU\n";
                        my $res_json;
                        if( sellMarket(\$res_json, $rikaku_retry_num)==0 ){
                            # 注文成功
                            # ノーポジションへ
                            $position = "NONE";
                        }else{
                            # 注文失敗
                            exit -1;
                        }
                    }
                }elsif( $indicator < 0 && $pre_indicator < 0 ){
                    # 前フレームから同じトレンド方向(下げ)の場合
                }else{
                    # 前フレームからトレンドが反転した場合
                    {
                        # LONG利確
                        print "LONG-RIKAKU\n";
                        my $res_json;
                        if( sellMarket(\$res_json, $rikaku_retry_num)==0 ){
                            # 注文成功
                            # ノーポジションへ
                            $position = "NONE";
                        }else{
                            # 注文失敗
                            exit -1;
                        }
                    }
                    if(abs($effective)>50){
                        # ドテンSHORTエントリー
                        print "SHORT-ENTRY(DOTEN)\n";
                        my $res_json;
                        if( sellMarket(\$res_json, $entry_retry_num)==0 ){
                            # 注文成功
                            # SHORTポジションへ
                            $position = "SHORT";
                        }
                    }
                }
            }elsif($position eq "SHORT"){
                if( $indicator > 0 && $pre_indicator > 0){
                    # 前フレームから同じトレンド方向(上げ)の場合
                }elsif( $indicator < 0 && $pre_indicator < 0 ){
                    # 前フレームから同じトレンド方向(下げ)の場合
                    if($rate > $threshold){
                        # SHORT維持
                    }else{
                        # SHORT利確
                        print "SHORT-RIKAKU\n";
                        my $res_json;
                        if( buyMarket(\$res_json, $rikaku_retry_num)==0 ){
                            # 注文成功
                            # ノーポジションへ
                            $position = "NONE";
                        }else{
                            # 注文失敗
                            exit -1;
                        }
                    }
                }else{
                    # 前フレームからトレンドが反転した場合
                    {
                        # SHORT利確
                        print "SHORT-RIKAKU\n";
                        my $res_json;
                        if( buyMarket(\$res_json, $rikaku_retry_num)==0 ){
                            # 注文成功
                            # ノーポジションへ
                            $position = "NONE";
                        }else{
                            # 注文失敗
                            exit -1;
                        }
                    }
                    if(abs($effective)>50){
                        # ドテンLONGエントリー
                        print "LONG-ENTRY(DOTEN)\n";
                        my $res_json;
                        if( buyMarket(\$res_json, $entry_retry_num)==0 ){
                            # 注文成功
                            # LONGポジションへ
                            $position = "LONG";
                        }
                    }
                }
            }elsif($position eq "NONE"){
                if( $indicator > 0 && $pre_indicator > 0){
                    # 前フレームから同じトレンド方向(上げ)の場合
                    # 強い上げが来た場合
                    #if( (abs($effective)>100.0) && (abs($rate) > 200.0) ){
                    if( abs($indicator)>100.0 ){
                        # LONGエントリー
                        print "LONG-ENTRY\n";
                        my $res_json;
                        if( buyMarket(\$res_json, $entry_retry_num)==0 ){
                            # 注文成功
                            # LONGポジションへ
                            $position = "LONG";
                        }
                    }
                }elsif( $indicator < 0 && $pre_indicator < 0 ){
                    # 前フレームから同じトレンド方向(下げ)の場合
                    # 強い下げが来た場合
                    #if( (abs($effective)>100.0) && (abs($rate) > 200.0) ){
                    if( abs($indicator)>100.0 ){
                        # SHORTエントリー
                        print "SHORT-ENTRY\n";
                        my $res_json;
                        if( sellMarket(\$res_json, $entry_retry_num)==0 ){
                            # 注文成功
                            # SHORTポジションへ
                            $position = "SHORT";
                        }
                    }
                }else{
                    # 前フレームからトレンドが反転した場合
                    if(abs($effective)>50){
                        if($indicator > 0){
                            # LONGエントリー
                            print "LONG-ENTRY(DOTEN)\n";
                            my $res_json;
                            if( buyMarket(\$res_json, $entry_retry_num)==0 ){
                                # 注文成功
                                # LONGポジションへ
                                $position = "LONG";
                            }
                        }else{
                            # SHORTエントリー
                            print "SHORT-ENTRY(DOTEN)\n";
                            my $res_json;
                            if( sellMarket(\$res_json, $entry_retry_num)==0 ){
                                # 注文成功
                                # SHORTポジションへ
                                $position = "SHORT";
                            }
                        }
                    }
                }
            }
        }

        # 情報出力
        my $volume_str = sprintf("[%05d]: SELL=%7s vs BUY=%7s: EFE=%7.2f: IND=%7.2f(%5.0f): %5s:\n"
            , $cycle_cnt
            , $sell_volume
            , $buy_volume
            , $effective
            , $indicator
            , $rate
            , $position
        );
        print $volume_str;

        # 前値保存
        $pre_effective = $effective;
        $pre_indicator = $indicator;
    };
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
