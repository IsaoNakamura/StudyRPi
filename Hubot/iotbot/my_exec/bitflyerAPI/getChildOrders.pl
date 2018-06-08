#!/usr/bin/perl

use strict;
use warnings;

use LWP::UserAgent;

use MyModule::UtilityJson;
use MyModule::UtilityBitflyer;

my $authFilePath = "./AuthBitflyer.json";
my $dest = "./DEST/executions.json";

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

# LONGエントリー
=pod
my $resBuy_json;
my $retBuy_req = MyModule::UtilityBitflyer::sellMarket(
                    \$resBuy_json,
                    \$ua,
                    \$authBitflyer,
                    "FX_BTC_JPY",
                    0.001
                );
print "retBuy_req=$retBuy_req\n";
if( $retBuy_req!=0 ){
    print "failed to buyMarket\n";
    exit -1;
}
#my $acceptance_id = $resBuy_json->{"child_order_acceptance_id"};
=cut


#my $acceptance_id = "JRF20180603-185700-719766"; 
my $acceptance_id = "JRF20180603-190001-721214";
print "acceptance_id=$acceptance_id\n";

my $retry_cnt=0;
while(1){
    sleep(1);
    my $res_json;
    my $ret_req =   MyModule::UtilityBitflyer::getChildOrdersAcceptance(
                        \$res_json,
                        \$ua,
                        \$authBitflyer,
                        "FX_BTC_JPY",
                        $acceptance_id
                    );

    print "ret_req=$ret_req, retry=$retry_cnt\n";
    if( $ret_req==0 ){
        my $orders_cnt = @{$res_json};
        print "orders_cnt=$orders_cnt\n";
        if($orders_cnt>0){
            my $order_ref = $res_json->[0];
            my $price = $order_ref->{"average_price"};
            print "price=$price\n";
            print "writeJson. $dest\n";
            if(MyModule::UtilityJson::writeJson(\$res_json, $dest, ">")!=0){
                print "FileSaveError. $dest\n";
                exit -1;
            }
            last;
        }
    }
    $retry_cnt++;
    if($retry_cnt>10){
        last;
    }
}

exit 0;

