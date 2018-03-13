#!/usr/bin/perl

use strict;
use warnings;

use LWP::UserAgent;

use MyModule::UtilityJson;
use MyModule::UtilityBitflyer;

my $authFilePath = "./AuthBitflyer.json";

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

my %bodyHash = ();
$bodyHash{"product_code"} = "FX_BTC_JPY";

my $ret_req =   MyModule::UtilityBitflyer::postCancelAllChildOrders(
                    \$ua,
                    \$authBitflyer,
                    \%bodyHash
                );

print "ret_req=$ret_req\n";

exit 0;

