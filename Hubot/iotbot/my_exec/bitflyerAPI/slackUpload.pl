#!/usr/bin/perl

use strict;
use Furl;
use HTTP::Request::Common;

my $host = shift;
my $token = shift;
my $channel_id = shift;
my $filePath = shift;

open( IN, '<', $filePath) || exit -1;
# binmode IN;

my $opts = {
    token    => $token,
    channels => $channel_id,
    filename => $filePath,
    file     => <IN>
};

my $req = POST ($host,[
        token    => $token,
        channels => $channel_id,
        filename => $filePath,
        file     => @$filePath,
        filetype => 'javascript'
    ]);

#my $req = POST ($host,
#    'Content-Type' => 'form-data',
#    'Content' => [
#        token    => $token,
#        channels => $channel_id,
#        filename => $filePath
#        file     => <IN>
#    ]);
#    'Content' => [
#        %{$opts}
#    ]);
my $res = Furl->new->request($req);
my $res_code = $res->code;
my $res_msg = $res->message;
print "response-code:$res_code\n";
print "response-msg:$res_msg\n";


close(IN);

print "host=$host\n";
print "token=$token\n";
print "channel_id=$channel_id\n";
print "filepath=$filePath\n";

exit 0;
