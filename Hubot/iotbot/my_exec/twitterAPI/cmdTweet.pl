#!/usr/bin/perl

use strict;
use warnings;

use JSON;

use utf8;
use Encode;

use Encode 'decode';
use Encode 'encode';

use Net::SSL;
use Net::Twitter::Lite::WithAPIv1_1;

use Mozilla::CA;
$ENV{HTTPS_CA_FILE} = Mozilla::CA::SSL_ca_file();

use Furl;
use HTTP::Request::Common;

my $envPath = shift;
my $cmd = shift;
my $value = shift;
my $channel_id = shift;

#print "envPath=$envPath\n";
#print "cmd=$cmd\n";
#print "value=$value\n";
#print "channel_id=$channel_id\n";

my $filePath = "$envPath/DEST/cmdCode.json";

if(-e $filePath){
    print "Command-Request is exist. retry please. $filePath\n";
    exit -1;
}

my %cmdCode = ();

if($cmd eq "add"){
    my $authTwitter;
    #if(readJson(\$authTwitter, "./my_exec/twitterAPI/AuthTwitter.json")!=0){
    if(readJson(\$authTwitter, "$envPath/AuthTwitter.json")!=0){
        print "FileReadError. Auth.\n";
        exit -1;
    }

    my $nt = Net::Twitter::Lite::WithAPIv1_1->new(
        consumer_key    =>  $authTwitter->{"consumer_key"},
        consumer_secret =>  $authTwitter->{"consumer_secret"},
        access_token    =>  $authTwitter->{"access_token"},
        access_token_secret => $authTwitter->{"access_token_secret"},
        ssl =>  1,
    );
    my $res_users =  $nt->lookup_users(
                        {
                            screen_name => $value,
                            include_entities => 0,
                        }
                    );


    for(my $i=0;$i<@{$res_users};$i++){
        my $user_ref = $res_users->[$i];

        my $since_id = 0;
        my $imgURL = "";
        my $name = "";

        if(!exists $user_ref->{"profile_image_url_https"}){
            next;
        }
        if($value ne $user_ref->{"screen_name"}){
            next;
        }
        
        if(exists $user_ref->{"profile_image_url_https"}){
            $imgURL = $user_ref->{"profile_image_url_https"};
        }
        if(exists $user_ref->{"status"}){
            if(exists $user_ref->{"status"}->{"id"}){
                $since_id = $user_ref->{"status"}->{"id"};
            }
        }
        if(exists $user_ref->{"name"}){
            $name = $user_ref->{"name"};
        }
        $cmdCode{$cmd}->{"screen_name"} = $value;
        $cmdCode{$cmd}->{"profile_image_url_https"} = $imgURL;
        $cmdCode{$cmd}->{"since_id"} = $since_id;
        $cmdCode{$cmd}->{"name"} = $name;
        $cmdCode{$cmd}->{"channel_id"} = $channel_id;
    }


}elsif($cmd eq "delete"){
    $cmdCode{$cmd}->{"screen_name"} = $value;
}elsif($cmd eq "stop"){
    $cmdCode{$cmd} = $value;
}else{
}

if(exists $cmdCode{$cmd}){
    if(writeJson(\\%cmdCode, $filePath, ">")!=0){
        print "FileSaveError. $filePath \n";
        exit -1;
    }
    print "FileSaveSuccess. $cmd to $filePath \n";
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
