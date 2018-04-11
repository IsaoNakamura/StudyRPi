#!/usr/bin/perl

package MyModule::UtilityCryptowatch;

use strict;
use warnings;

use JSON;

use utf8;
use Encode;

use Encode 'decode';
use Encode 'encode';

use LWP::UserAgent;
#use HTTP::Request;

sub getCandleStickAfter{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $symbol         = shift; # btcfxjpy "FX_BTC_JPY"
    my $periods        = shift; # 60 1m
    my $after          = shift; # #Unix-timestamp
    my $path           = shift; # ohlc
    my $endPoint       = shift; # https://api.cryptowat.ch/markets/bitflyer

    # https://api.cryptowat.ch/markets/bitflyer/btcjpy/ohlc?periods=86400&after=1483196400
    my $url = sprintf("%s/%s/%s?periods=%d&after=%d",$endPoint, $symbol, $path, $periods ,$after);
    print $url . "\n";

    my $res = $$userAgent_ref->get($url);
    
    if ($res->is_error)
    {
        my $error_msg = sprintf("ERR, message=%s, url=%s\n", $res->message, $url );
        print $error_msg;
        return(-1);
    }
    print $res->message  . "\n";
    
    my $content = $res->content;
    if(length($content)>0)
    {
        $$resultJson_ref = decode_json($content);
    }
    return(0);
}

1;
__END__