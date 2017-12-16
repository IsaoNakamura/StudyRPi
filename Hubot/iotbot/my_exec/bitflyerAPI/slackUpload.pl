#!/usr/bin/perl

use strict;
use Furl;
use HTTP::Request::Common;

my $host = shift;
my $token = shift;
my $channel_id = shift;
my $filePath = shift;

open( IN, '<', $filePath) || exit -1;

my $opts = {
    token    => $token,
    channels => $channel_id,
    filename => $filePath,
    file     => <IN>
};

my $req = POST ($host,
    'Content' => [
        %$opts
    ]);
my $res = Furl->new->request($req);

close(IN);

print "host=$host\n";
print "token=$token\n";
print "channel_id=$channel_id\n";
print "filepath=$filePath\n";

exit 0;
