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

# BitflyerAPIでリクエストを発行する
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
    my $request = HTTP::Request->new
                    (
                        $method,
                        $url,
                        [
                            'ACCESS-KEY'       => $$auth_ref->{"ACCESS-KEY"},
                            'ACCESS-TIMESTAMP' => $timestamp,
                            'ACCESS-SIGN'      => $signature,
                            'Content-Type'     => 'application/json',
                            'Content-Length'   => length($body)
                        ],
                        $body
                    );

    $$userAgent_ref->agent('');
    my $res = $$userAgent_ref->request($request);
    
    if ($res->is_error)
    {
        my $error_msg = sprintf("ERR, message=%s, params=%s\n", $res->message, $params );
        print $error_msg;
        return(-1);
    }
    #print $res->message  . "\n";
    
    my $content = $res->content;
    if(length($content)>0)
    {
        $$resultJson_ref = decode_json($content);
    }
    return(0);
}
 # 資産残高を取得
sub getBalance{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/getbalance";
    my $method   = "GET";
    my $body     = "";

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

# マーケットの一覧
sub getMarkets{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/getmarkets";
    my $method   = "GET";
    my $body     = "";

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

# Ticker
sub getTicker{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $product        = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/getticker";
    my $method   = "GET";
    my $body     = "";

    $path = sprintf("%s?product_code=%s", $path, $product);

    #print "endPoint = " . $endPoint . "\n";
    #print "path     = " . $path . "\n";
    #print "method   = " . $method . "\n";
    #print "body     = " . $body . "\n";

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

# すべての注文をキャンセルする
sub postCancelAllChildOrders{
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $bodyHash_ref   = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/cancelallchildorders";
    my $method   = "POST";
    my $body     = encode_json($bodyHash_ref);

    #print "endPoint = " . $endPoint . "\n";
    #print "path     = " . $path . "\n";
    #print "method   = " . $method . "\n";
    #print "body     = " . $body . "\n";

    my $resultJson;
    my $ret_req =   MyModule::UtilityBitflyer::requestBitflyer(
                        \$resultJson,
                        $userAgent_ref,
                        $auth_ref,
                        $endPoint,
                        $method,
                        $path,
                        $body
                    );
    return($ret_req);
}

# 新規の注文を出す
sub postSendChildOrder{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $bodyHash_ref   = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/sendchildorder";
    my $method   = "POST";
    my $body     = encode_json($bodyHash_ref);

    #print "endPoint = " . $endPoint . "\n";
    #print "path     = " . $path     . "\n";
    #print "method   = " . $method   . "\n";
    #print "body     = " . $body     . "\n";

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

# 新規の親注文を出す（特殊注文）
sub postSendParentOrder{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $bodyHash_ref   = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/sendparentorder";
    my $method   = "POST";
    my $body     = encode_json($bodyHash_ref);

    #print "endPoint = " . $endPoint . "\n";
    #print "path     = " . $path . "\n";
    #print "method   = " . $method . "\n";
    #print "body     = " . $body . "\n";

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


# 成り行き買い注文
sub buyMarket{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $product_code   = shift; #銘柄
    my $ammount        = shift; #数量

    my %bodyHash =  (
        "product_code"     => $product_code,
        "child_order_type" => "MARKET",
        "side"             => "BUY",
        "size"             => $ammount,
        "minute_to_expire" => 10000,
        "time_in_force"    => "GTC"
    );

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/sendchildorder";
    my $method   = "POST";
    my $body     = encode_json(\%bodyHash);

    my $ret_req =   MyModule::UtilityBitflyer::postSendChildOrder(
                        $resultJson_ref,
                        $userAgent_ref,
                        $auth_ref,
                        \%bodyHash
                    );

    return($ret_req);
}

# 成り行き売り注文
sub sellMarket{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $product_code   = shift; #銘柄
    my $ammount        = shift; #数量

    my %bodyHash =  (
        "product_code"     => $product_code,
        "child_order_type" => "MARKET",
        "side"             => "SELL",
        "size"             => $ammount,
        "minute_to_expire" => 10000,
        "time_in_force"    => "GTC"
    );

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/sendchildorder";
    my $method   = "POST";
    my $body     = encode_json(\%bodyHash);

    my $ret_req =   MyModule::UtilityBitflyer::postSendChildOrder(
                        $resultJson_ref,
                        $userAgent_ref,
                        $auth_ref,
                        \%bodyHash
                    );

    return($ret_req);
}

sub getExecutionsAcceptance{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $product        = shift;
    my $acceptance_id  = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/getexecutions";
    my $method   = "GET";
    my $body     = "";

    $path = sprintf("%s?product_code=%s&child_order_acceptance_id=%s", $path, $product,$acceptance_id);

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

sub getChildOrdersAcceptance{
    my $resultJson_ref = shift;
    my $userAgent_ref  = shift;
    my $auth_ref       = shift;
    my $product        = shift;
    my $acceptance_id  = shift;

    my $endPoint = "https://api.bitflyer.jp";
    my $path     = "/v1/me/getchildorders";
    my $method   = "GET";
    my $body     = "";

    $path = sprintf("%s?product_code=%s&child_order_acceptance_id=%s", $path, $product,$acceptance_id);

    #print "endPoint = " . $endPoint . "\n";
    #print "path     = " . $path . "\n";
    #print "method   = " . $method . "\n";
    #print "body     = " . $body . "\n";

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