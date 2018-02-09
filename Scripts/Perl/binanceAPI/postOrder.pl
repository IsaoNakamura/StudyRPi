#!/usr/bin/perl

use strict;

use warnings;
use Getopt::Std;
use LWP::UserAgent;
use JSON;
# use open ':locale';

use Digest::SHA qw(hmac_sha256_hex);

my $url = shift;
my $authFilePath = shift;
my $dest = shift;
my $symbol = shift;#"BCCUSDT";
my $side = shift;#"BUY";
my $type = shift;#"LIMIT";
my $timeInfForce = shift;#"GTC";
my $quantity = shift;#"3.69143";
my $price = shift;#"976.00";
my $recvWindow = shift;#"5000";

#print $url . "\n";
#print $authFilePath . "\n";
#print $dest . "\n";
print "symbol:$symbol\n";
print "side:$side\n";
print "type:$type\n";
print "quantity:$quantity\n";
print "price:$price\n";

if(!(-f $authFilePath)){
    print "not exists AuthFile. $authFilePath\n";
    exit -1;
}

my $authBinance;
if(readJson(\$authBinance, $authFilePath)!=0){
    print "FileReadError. Auth.\n";
    exit -1;
}
#print $authBinance->{"api_key"} . "\n";
#print $authBinance->{"secret"} . "\n";

my $ua = new LWP::UserAgent;
$ua->timeout(10); # default: 180sec
$ua->ssl_opts( verify_hostname => 0 ); # skip hostname verification
$ua->default_header(
    'X-MBX-APIKEY' => $authBinance->{"api_key"}
);

my $res_time = $ua->get('https://api.binance.com/api/v1/time');
die $res_time->message if $res_time->is_error;

my $time_content = $res_time->content;
utf8::decode($time_content); 
$time_content = from_json($time_content);

my $timestamp = $time_content->{"serverTime"};
# print $timestamp . "\n";

my $params = sprintf("symbol=%s&side=%s&type=%s&timeInForce=%s&quantity=%s&price=%s&recvWindow=%s&timestamp=%s"
                , $symbol
                , $side
                , $type
                , $timeInfForce
                , $quantity
                , $price
                , $recvWindow
                , $timestamp
            );

my $signature = hmac_sha256_hex($params, $authBinance->{"secret"});
# print $signature . "\n";

my $post_url  =  sprintf("%s?%s&signature=%s"
                        , $url
                        , $params
                        , $signature
                    );
#print $post_url . "\n";
# exit 0;

my $res =   $ua->post
            (
                $post_url
                #$url
                #, 'Content-Type' => 'application/json;charset=utf-8'
                #, 'Content' => $post_content
            );

if ($res->is_error)
{
    print $post_url . "\n";
    die $res->message;
}
print $res->message . "\n";

die $res->message if $res->is_error;

my $content = $res->content;

# convert UTF-8 binary to text
utf8::decode($content); 
my $content_ref = from_json($content);

open (OUT, '>', $dest) || die('File Open Error');
print OUT to_json($content_ref, {pretty=>1});
close(OUT);

exit 0;

sub writeJson {
       my $hash_ref = shift; #IN
       my $filePath = shift; #IN
       my $mode = shift;

       # save to Json.
       # utf8::encode($$hash_ref);

       open (OUT, $mode, $filePath) || return(1);
       binmode(OUT, ":utf8");
       print OUT to_json($$hash_ref, {pretty=>1});
       close(OUT);
       return (0);
}

sub readJson {
       my $hash_ref = shift; # OUT
       my $filePath = shift; #IN

       %{$$hash_ref} = ();

       open( IN, '<', $filePath) || return(1);
       eval{
              local $/ = undef;
              my $json_text = <IN>;
              close(IN);
              $$hash_ref = decode_json($json_text );
       };
       return (0);
}
