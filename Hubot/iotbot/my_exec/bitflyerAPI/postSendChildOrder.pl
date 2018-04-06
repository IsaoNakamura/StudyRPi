#!/usr/bin/perl

use strict;
use warnings;

use LWP::UserAgent;

use MyModule::UtilityJson;
use MyModule::UtilityBitflyer;

my $authFilePath = "./AuthBitflyer.json";
my $dest = "./DEST/SendParentOrder.json";

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

my %bodyHash =  (
    "product_code"     => "FX_BTC_JPY",
    "child_order_type" => "LIMIT",
    "side"             => "BUY",
    "price"            => 900000,
    "size"             => 0.001,
    "minute_to_expire" => 10000,
    "time_in_force"    => "GTC"
);

my $res_json;
my $ret_req =   MyModule::UtilityBitflyer::postSendChildOrder(
                    \$res_json,
                    \$ua,
                    \$authBitflyer,
                    \%bodyHash
                );


print "ret_req=$ret_req\n";
if( $ret_req==0 ){



    print "writeJson. $dest\n";
    if(MyModule::UtilityJson::writeJson(\$res_json, $dest, ">")!=0){
        print "FileSaveError. $dest\n";
        exit -1;
    }
}
exit 0;

