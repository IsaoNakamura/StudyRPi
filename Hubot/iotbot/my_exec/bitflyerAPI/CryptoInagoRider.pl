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

#phantomjs --webdriver=9999
#system "start phantomjs --webdriver=9999 &";
#my $wait_cnt = 0;
#while(1){
#    print "$wait_cnt\[sec\]\n";
#    sleep(1);
#    $wait_cnt++;
#    if($wait_cnt>5){
#        last;
#    }
#}

my $driver = Selenium::Remote::Driver->new(
    remote_server_addr => '127.0.0.1',
    port => 9999,
);

my $res = $driver->get('https://inagoflyer.appspot.com/btcmac');
sleep(5);

my $title = $driver->get_title();
$title = encode('Shift_JIS', $title);
print "$title\n";

my $elements = $driver->find_elements("#chart_table_tbody input[type='checkbox']", "css");
for(my $i=1; $i<@{$elements}; $i++){
    my $elem_id = $elements->[$i]->get_attribute('id');
    if($elem_id eq "bitFlyer_BTCJPY_checkbox"){
        next;
    }
    $elements->[$i]->click();
    print "[$i]:" . $elem_id . " is clicked\n";
}

my $ammount = 0.005;
my $cycle_sec = 3;
my $position = "NONE";
my $cycle_cnt =0;

my $sell_id = "sellVolumePerMeasurementTime";
my $buy_id = "buyVolumePerMeasurementTime";


my $pos_best_bid = 0;
my $pre_best_bid = 0;
my $pre_sell = 0;
my $pre_buy = 0;
my $pre_effective = 0.0;



# 0に戻ろうとする特性をもつ(1サイクルにつきX%半減)
my $pre_indicator = 0.0;
my $force = 0.6;
my $threshold = 50.0;

while(1){
    eval{
        # Ticker(相場)を取得
        my $best_bid = 0;
        my $best_ask = 0;
        my $res_json;
        #my $ret_req =   MyModule::UtilityBitflyer::getTicker(
        #                    \$res_json,
        #                    \$ua,
        #                    \$authBitflyer,
        #                    "FX_BTC_JPY"
        #                );
        #if( $ret_req==0 ){
        #    $best_bid = $res_json->{"best_bid"};
        #    $best_ask = $res_json->{"best_ask"};
        #}

        # イナゴフライヤーの値を取得
        my $sell_volume = $driver->find_element_by_id($sell_id)->get_text;
        my $buy_volume = $driver->find_element_by_id($buy_id)->get_text;

        # 実効値算出
        my $effective = $buy_volume - $sell_volume;

        # 利益用(シミュレート用)
        my $profit = 0;
        
        # インジケータの値を算出
        my $indicator = 0.0;
        $indicator+=$effective;
        if(abs($indicator) < 0.0001){
            # 0なら維持
        }
        elsif($indicator>0.0){
            # 正なら減らす
            #$indicator-=$force;
            #$indicator = $indicator - ($indicator * $force);
            $indicator = $indicator * $force;
            if($indicator<=0.0){
                $indicator=0.0;
            }
        }else{
            # 負なら増やす
            #$indicator+=$force;
            $indicator = $indicator * $force;
            if($indicator>=0.0){
                $indicator=0.0;
            }
        }

        # インジケータの前回との比率を算出
        my $rate = 0.0;
        if(abs($pre_indicator) > 0.0001){
            $rate = ($indicator / $pre_indicator) * 100.0;
            #$rate = ( ($indicator / $pre_indicator) - 1.0 ) * 100.0;
        }

        if($cycle_cnt>=4){
            if( $indicator > 0 && $pre_indicator > 0){
                # 両方正⇒同じ方向
                if( $position eq "LONG" ){
                    if($rate > $threshold){
                        # LONG維持
                    }else{
                        # LONG利確
                        my $retry_num = 0;
                        while(1){
                            my $res_json;
                            my $ret_req =   MyModule::UtilityBitflyer::shortByMarket(
                                                \$res_json,
                                                \$ua,
                                                \$authBitflyer,
                                                "FX_BTC_JPY",
                                                $ammount
                                            );


                            print "ret_req=$ret_req\n";
                            if( $ret_req==0 ){
                                # 注文成功
                                my $fileName = sprintf("%05d_%s_RIKAKU.json",$cycle_cnt,$position);
                                $position = "NONE";
                                $profit = $pre_best_bid - $pos_best_bid;
                                
                                print "writeJson. $fileName\n";
                                #if(MyModule::UtilityJson::writeJson(\$res_json, $fileName, ">")!=0){
                                #    print "FileSaveError. $fileName\n";
                                #    exit -1;
                                #}
                                last;
                            }else{
                                if($retry_num < 3){
                                    sleep(1);
                                    print "retry:$retry_num\n";
                                }else{
                                    exit -1;
                                }
                                $retry_num++;
                            }
                        }

                    }
                }
            }elsif( $indicator < 0 && $pre_indicator < 0 ){
                # 両方負⇒同じ方向
                if( $position eq "SHORT" ){
                    if($rate > $threshold){
                        # SHORT維持
                    }else{
                        # SHORT利確
                        my $retry_num = 0;
                        while(1){
                            my $res_json;
                            my $ret_req =   MyModule::UtilityBitflyer::longByMarket(
                                                \$res_json,
                                                \$ua,
                                                \$authBitflyer,
                                                "FX_BTC_JPY",
                                                $ammount
                                            );

                            print "ret_req=$ret_req\n";
                            if( $ret_req==0 ){
                                # 注文成功
                                my $fileName = sprintf("%05d_%s_RIKAKU.json",$cycle_cnt,$position);
                                $position = "NONE";
                                $profit = $pos_best_bid - $pre_best_bid;
                                
                                print "writeJson. $fileName\n";
                                #if(MyModule::UtilityJson::writeJson(\$res_json, $fileName, ">")!=0){
                                #    print "FileSaveError. $fileName\n";
                                #    exit -1;
                                #}
                                last;
                            }else{
                                if($retry_num < 3){
                                    sleep(1);
                                    print "retry:$retry_num\n";
                                }else{
                                    exit -1;
                                }
                                $retry_num++;
                            }
                        }
                    }
                }
            }else{
                # 逆方向
                if( $position eq "LONG" ){
                    # LONG利確(ドテンショート)
                    {
                        # LONG利確
                        my $retry_num = 0;
                        while(1){
                            my $res_json;
                            my $ret_req =   MyModule::UtilityBitflyer::shortByMarket(
                                                \$res_json,
                                                \$ua,
                                                \$authBitflyer,
                                                "FX_BTC_JPY",
                                                $ammount
                                            );


                            print "ret_req=$ret_req\n";
                            if( $ret_req==0 ){
                                # 注文成功
                                my $fileName = sprintf("%05d_%s_RIKAKU.json",$cycle_cnt,$position);
                                $position = "NONE";
                                $profit = $pre_best_bid - $pos_best_bid;
                                
                                print "writeJson. $fileName\n";
                                #if(MyModule::UtilityJson::writeJson(\$res_json, $fileName, ">")!=0){
                                #    print "FileSaveError. $fileName\n";
                                #    exit -1;
                                #}
                                last;
                            }else{
                                if($retry_num < 3){
                                    sleep(1);
                                    print "retry:$retry_num\n";
                                }else{
                                    exit -1;
                                }
                                $retry_num++;
                            }
                        }
                    }
                    {
                        # SHORTエントリー
                        my $res_json;
                        my $ret_req =   MyModule::UtilityBitflyer::shortByMarket(
                                            \$res_json,
                                            \$ua,
                                            \$authBitflyer,
                                            "FX_BTC_JPY",
                                            $ammount
                                        );


                        print "ret_req=$ret_req\n";
                        if( $ret_req==0 ){
                            # 注文成功
                            $position = "SHORT";
                            $pos_best_bid = $best_bid;
                            my $fileName = sprintf("%05d_%s_ENTRY.json",$cycle_cnt,$position);

                            print "writeJson. $fileName\n";
                            #if(MyModule::UtilityJson::writeJson(\$res_json, $fileName, ">")!=0){
                            #    print "FileSaveError. $fileName\n";
                            #    exit -1;
                            #}
                        }
                    }
                }elsif( $position eq "SHORT"){
                    # SHORT利確(ドテンロング)
                    {
                        # SHORT利確
                        my $retry_num = 0;
                        while(1){
                            my $res_json;
                            my $ret_req =   MyModule::UtilityBitflyer::longByMarket(
                                                \$res_json,
                                                \$ua,
                                                \$authBitflyer,
                                                "FX_BTC_JPY",
                                                $ammount
                                            );

                            print "ret_req=$ret_req\n";
                            if( $ret_req==0 ){
                                # 注文成功
                                my $fileName = sprintf("%05d_%s_RIKAKU.json",$cycle_cnt,$position);
                                $position = "NONE";
                                $profit = $pos_best_bid - $pre_best_bid;

                                print "writeJson. $fileName\n";
                                #if(MyModule::UtilityJson::writeJson(\$res_json, $fileName, ">")!=0){
                                #    print "FileSaveError. $fileName\n";
                                #    exit -1;
                                #}
                                last;
                            }else{
                                if($retry_num < 3){
                                    sleep(1);
                                    print "retry:$retry_num\n";
                                }else{
                                    exit -1;
                                }
                                $retry_num++;
                            }
                        }
                    }
                    {
                        # LONGエントリー
                        my $res_json;
                        my $ret_req =   MyModule::UtilityBitflyer::longByMarket(
                                            \$res_json,
                                            \$ua,
                                            \$authBitflyer,
                                            "FX_BTC_JPY",
                                            $ammount
                                        );

                        print "ret_req=$ret_req\n";
                        if( $ret_req==0 ){
                            # 注文成功
                            $position = "LONG";
                            $pos_best_bid = $best_bid;
                            my $fileName = sprintf("%05d_%s_ENTRY.json",$cycle_cnt,$position);

                            print "writeJson. $fileName\n";
                            #if(MyModule::UtilityJson::writeJson(\$res_json, $fileName, ">")!=0){
                            #    print "FileSaveError. $fileName\n";
                            #    exit -1;
                            #}
                        }
                    }
                }elsif($position eq "NONE"){
                    if($indicator > 0){
                        # LONGエントリー
                        my $res_json;
                        my $ret_req =   MyModule::UtilityBitflyer::longByMarket(
                                            \$res_json,
                                            \$ua,
                                            \$authBitflyer,
                                            "FX_BTC_JPY",
                                            $ammount
                                        );

                        print "ret_req=$ret_req\n";
                        if( $ret_req==0 ){
                            # 注文成功
                            $position = "LONG";
                            $pos_best_bid = $best_bid;
                            my $fileName = sprintf("%05d_%s_ENTRY.json",$cycle_cnt,$position);

                            print "writeJson. $fileName\n";
                            #if(MyModule::UtilityJson::writeJson(\$res_json, $fileName, ">")!=0){
                            #    print "FileSaveError. $fileName\n";
                            #    exit -1;
                            #}
                        }
                    }else{
                        # SHORTエントリー
                        my $res_json;
                        my $ret_req =   MyModule::UtilityBitflyer::shortByMarket(
                                            \$res_json,
                                            \$ua,
                                            \$authBitflyer,
                                            "FX_BTC_JPY",
                                            $ammount
                                        );


                        print "ret_req=$ret_req\n";
                        if( $ret_req==0 ){
                            # 注文成功
                            $position = "SHORT";
                            $pos_best_bid = $best_bid;
                            my $fileName = sprintf("%05d_%s_ENTRY.json",$cycle_cnt,$position);

                            print "writeJson. $fileName\n";
                            #if(MyModule::UtilityJson::writeJson(\$res_json, $fileName, ">")!=0){
                            #    print "FileSaveError. $fileName\n";
                            #    exit -1;
                            #}
                        }

                    }
                }
            }
        }

        # 情報出力
        my $volume_str = sprintf("[%05d]: SELL=%7s vs BUY=%7s: EFE=%7.2f: IND=%7.2f(%5.0f): BID=%9d(%5d): %5s: PFT=%9d:\n"
            , $cycle_cnt
            , $sell_volume
            , $buy_volume
            , $effective
            , $indicator
            , $rate
            , $best_bid
            , ($best_bid - $pre_best_bid)
            , $position
            , $profit
        );
        print $volume_str;

        # 前値保存
        $pre_best_bid = $best_bid;
        $pre_sell = $sell_volume;
        $pre_buy  = $buy_volume;
        $pre_effective = $effective;
        $pre_indicator = $indicator;
    };
    sleep($cycle_sec);
    $cycle_cnt++;
}

exit 0;

