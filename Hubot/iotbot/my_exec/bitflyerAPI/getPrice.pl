#!/usr/local/share/perl

use strict;

use warnings;
use Getopt::Std;
use LWP::UserAgent;
use JSON;
# use open ':locale';

sub HELP_MESSAGE {
	print STDERR <<"EOM";
usage : $0 -k API_KEY url DEST_PATH
		-k API_KEY: API Access Key
EOM
	exit 0;
}

our $opt_k;
getopts('k:') or HELP_MESSAGE();
my $url = shift;
my $dest = shift;
HELP_MESSAGE() unless $url;

# print "KEY=$opt_k\n";
# print "URL=$url\n";
# print "DEST=$dest\n";


####################################
my $ua = new LWP::UserAgent;
$ua->timeout(10); # default: 180sec
$ua->ssl_opts( verify_hostname => 0 ); # skip hostname verification
# $ua->default_header('apikey' => $opt_k) if $opt_k;

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
