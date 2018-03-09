#!/usr/bin/perl

package MyModule::UtilityStr;

use strict;
use warnings;

use utf8;
use Encode;

use Encode 'decode';
use Encode 'encode';

sub getHttpStrArray{
	my $array_ref	= shift; # OUT
	my $text	    = shift; #  IN
    my $pos         = shift; #  IN

    # http抜き出し
    my $beg_pos = index($text, "http",$pos);
    #print "beg_pos=$beg_pos\n";
    if($beg_pos != -1){
        my $end_space = index($text, " ",$beg_pos+1);
        #print "end_space=$end_space space.\n";

        my $end_big = index($text, "　",$beg_pos+1);
        #print "end_big=$end_big big space.\n";

        my $end_ret = index($text, "\n",$beg_pos+1);
        #print "end_ret=$end_ret return.\n";

        my $end_pos = -1;
        if($end_space!=-1){
            $end_pos = $end_space - 1;
        }
        if($end_big!=-1){
            if($end_pos==-1){
                $end_pos = $end_big - 1;
            }else{
                if($end_pos>$end_big){
                    $end_pos = $end_big - 1;
                }
            }
        }
        if($end_ret!=-1){
            if($end_pos==-1){
                $end_pos = $end_ret - 1;
            }else{
                if($end_pos>$end_ret){
                    $end_pos = $end_ret - 1;
                }
            }
        }
        if($end_pos == -1){
            $end_pos = length($text)-1;
            #print "end_pos=$end_pos end of string.\n";
        }
        #print "!!! end_pos=$end_pos !!!\n";
        if($end_pos != -1){
            my $http_text = substr($text,$beg_pos,($end_pos-$beg_pos)+1);
            push(@{$array_ref}, $http_text );
            #print "push http_text=$http_text\n";
            if($end_pos != length($text)){
                getHttpStrArray($array_ref, $text, $end_pos+1);
            }
        }
    }
}

1;
__END__