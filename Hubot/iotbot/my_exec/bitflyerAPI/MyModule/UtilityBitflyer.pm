#!/usr/bin/perl

package MyModule::UtilityBitflyer;

use strict;
use warnings;

use JSON;

use utf8;
use Encode;

use Encode 'decode';
use Encode 'encode';

use LWP::UserAgent;
use HTTP::Request;

use Digest::SHA qw(hmac_sha256_hex);

sub requestBitflyer{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $endPoint       = shift;
    my $method         = shift;
    my $path           = shift;
    my $body           = shift;

    my $timestamp = time ; #Unix-timestamp
    my $params = $timestamp . $method . $path . $body;
    my $signature = hmac_sha256_hex($params, $$auth_ref->{"ACCESS-SECRET"});

    my $url = $endPoint . $path;
    my $request = HTTP::Request->new($method, $url,[
        'ACCESS-KEY' => $$auth_ref->{"ACCESS-KEY"},
        'ACCESS-TIMESTAMP' => $timestamp,
        'ACCESS-SIGN' => $signature
    ]);

    $$userAgent_ref->agent('');
    my $res = $$userAgent_ref->request($request);
    
    if ($res->is_error)
    {
        my $error_msg = sprintf("ERR, message=%s, params=%s\n", $res->message, $params );
        print $error_msg;
        return(-1);
    }
    print $res->message  . "\n";
    
    my $content = $res->content;

    $$resultJson_ref = decode_json($content);

    return(0);
}

sub getBalance{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $body           = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/getbalance";
    my $method   = "GET";

    print "endPoint = " . $endPoint . "\n";
    print "path     = " . $path . "\n";
    print "method   = " . $method . "\n";
    print "body     = " . $body . "\n";

    my $ret_req =   MyModule::UtilityBitflyer::requestBitflyer(
                        $resultJson_ref,
                        $userAgent_ref,
                        $auth_ref,
                        $endPoint,
                        $method,
                        $path,
                        $body
                    );
    return($ret_req);
}

1;
__END__