#!/usr/bin/perl

use strict;
use Furl;
use HTTP::Request::Common;

my $host = shift;
my $token = shift;
my $channel = shift;
my $text = shift;

#my $req = POST "https://slack.com/api/chat.postMessage",
#    'Content' => [
#        token => "",
#        channel => "#general",
#        text => "post from Perl-Script."
#    ];
my $req = POST ($host,
    'Content' => [
        token => $token,
        channel => $channel,
        text => $test
    ]);
my $res = Furl->new->request($req);

print "host=$host\n";
print "token=$token\n";
print "channel=$channel\n";
print "text=$text\n";

exit 0;
