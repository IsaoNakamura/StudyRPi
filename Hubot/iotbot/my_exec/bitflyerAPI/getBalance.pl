#!/usr/bin/perl

use strict;
use warnings;

use JSON;
use utf8;
use Encode;
use Encode 'decode';
use Encode 'encode';

use Getopt::Std;
use LWP::UserAgent;
use HTTP::Request;

use Digest::SHA qw(hmac_sha256_hex);

use MyModule::UtilityJson;

my $authFilePath = "./AuthBitflyer.json";
my $dest = "./DEST/Balance.json";

my $url = "https://api.bitflyer.jp";
my $path = "/v1/me/getbalance";
my $method = "GET";

print "url = " . $url . "\n";
print "path= " . $path . "\n";
print "auth= " . $authFilePath . "\n";
print "dest= " . $dest . "\n";

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

my $timestamp = time ; #Unix-timestamp
#print "timestamp=$timestamp\n";

my $params = $timestamp . $method . $path;
#print "params=$params\n";

my $signature = hmac_sha256_hex($params, $authBitflyer->{"ACCESS-SECRET"});
#print "signature=$signature\n";

my $request = HTTP::Request->new($method, "$url$path",[
    'ACCESS-KEY' => $authBitflyer->{"ACCESS-KEY"},
    'ACCESS-TIMESTAMP' => $timestamp,
    'ACCESS-SIGN' => $signature
]);

$ua->agent('');
my $res = $ua->request($request);

if ($res->is_error)
{
    print $params . "\n";
    die $res->message;
}
print $res->message . "\n";

my $content = $res->content;

# convert UTF-8 binary to text
utf8::decode($content); 
my $content_ref = from_json($content);

if(MyModule::UtilityJson::writeJson(\$content_ref, $dest, ">")!=0){
    print "FileSaveError. $dest\n";
    exit -1;
}

exit 0;