#!/usr/bin/perl

use strict;

use warnings;
#use Getopt::Std;
#use LWP::UserAgent;
use JSON;

use utf8;
use Encode;

use Fcntl;

use Encode 'decode';
use Encode 'encode';



my $env_path= shift;
my $cmdCode = "$envPath/DEST/cmdCode.json";

if(-e $cmdCode){
    print "Command-Request is exist. delete $cmdCode\n";
    unlink $cmdCode;
    exit -1;
}

if(-e $cmdCode){
    print "recieved cmdCode:$cmdCode\n";

    my $cmd_ref;
    if(readJson(\$cmd_ref, $cmdCode)!=0){
        print "FileReadError. $cmdCode \n";
        next;
    }

    my $vipBCH;
    if(readJson(\$vipBCH, "$env_path/vipBCH.json")!=0){
        print "FileReadError. vipBCH.\n";
        next;
    }

    my $isStop = 0;
    if(exists $cmd_ref->{"add"}){
        my $screen_name = $cmd_ref->{"add"}->{"screen_name"};
        my $imgUrl      = $cmd_ref->{"add"}->{"profile_image_url_https"};
        my $since_id    = $cmd_ref->{"add"}->{"since_id"};
        my $name        = $cmd_ref->{"add"}->{"name"};

        $vipBCH->{$screen_name}->{"include_rts"}="true";
        $vipBCH->{$screen_name}->{"profile_image_url_https"}=$imgUrl;
        $vipBCH->{$screen_name}->{"since_id"}=$since_id;
        $vipBCH->{$screen_name}->{"name"}=$name;

        if(writeJson(\$vipBCH, "$env_path/vipBCH.json", ">")!=0){
            print "FileWriteError. vipBCH.\n";
            next;
        }
    }elsif(exists $cmd_ref->{"delete"}){
        my $screen_name = $cmd_ref->{"delete"}->{"screen_name"};
        delete($vipBCH->{$screen_name});
        if(writeJson(\$vipBCH, "$env_path/vipBCH.json", ">")!=0){
            print "FileWriteError. vipBCH.\n";
            next;
        }
    }elsif(exists $cmd_ref->{"stop"}){
        $isStop = 1;
    }

    unlink $cmdCode;
    if($isStop>0){
        #last;
        print "escape loop .\n";
    }
}




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

