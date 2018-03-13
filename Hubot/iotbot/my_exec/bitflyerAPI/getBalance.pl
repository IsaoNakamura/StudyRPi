#!/usr/bin/perl

use strict;
use warnings;

use LWP::UserAgent;

use MyModule::UtilityJson;
use MyModule::UtilityBitflyer;

my $authFilePath = "./AuthBitflyer.json";
my $dest = "./DEST/Balance.json";

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

my $res_json;
my $ret_req =   MyModule::UtilityBitflyer::getBalance(
                    \$res_json,
                    \$ua,
                    \$authBitflyer,
                    ""
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

