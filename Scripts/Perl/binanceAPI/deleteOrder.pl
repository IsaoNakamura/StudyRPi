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
my $orderId = shift;

#print $url . "\n";
#print $authFilePath . "\n";
#print $dest . "\n";
print "orderId=$orderId\n";

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
#print $timestamp . "\n";


my $symbol = "BCCUSDT";
my $params = sprintf("symbol=%s&timestamp=%s&orderId=%d"
                , $symbol
                , $timestamp
                , $orderId
            );

my $signature = hmac_sha256_hex($params, $authBinance->{"secret"});
#print $signature . "\n";

my $del_url  =  sprintf("%s?%s&signature=%s"
                        , $url
                        , $params
                        , $signature
                    );
#print $del_url . "\n";
# exit 0;

my $res =   $ua->delete
            (
                $del_url
                #$url
                #, 'Content-Type' => 'application/json;charset=utf-8'
                #, 'Content' => $post_content
            );

if ($res->is_error)
{
    print $del_url . "\n";
    die $res->message;
}
print $res->message . "\n";
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
