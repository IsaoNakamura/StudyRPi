#!/usr/bin/perl

use strict;

use warnings;
use Getopt::Std;
use LWP::UserAgent;
use JSON;
# use open ':locale';

my $url = shift;
my $dest = shift;

####################################
my $ua = new LWP::UserAgent;
$ua->timeout(10); # default: 180sec
$ua->ssl_opts( verify_hostname => 0 ); # skip hostname verification

my $res = $ua->get($url);

die $res->message if $res->is_error;

my $content = $res->content;

# convert UTF-8 binary to text
utf8::decode($content); 

my $content_ref = from_json($content);

print "MiddleRate:" . $content_ref->{"mid"} . "[BTC/JPY]\n";
print "BuyRate   :" . $content_ref->{"ask"} . "[BTC/JPY]\n";
print "SellRate  :" . $content_ref->{"bid"} . "[BTC/JPY]\n";

open (OUT, '>', $dest) || die('File Open Error');
print OUT to_json($content_ref, {pretty=>1});
close(OUT);

exit 0;
