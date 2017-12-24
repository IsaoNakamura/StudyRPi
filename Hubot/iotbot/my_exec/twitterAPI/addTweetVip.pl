#!/usr/bin/perl

use strict;

use warnings;

use JSON;

use utf8;
use Encode;

use Time::Local;
use Time::Piece;

#use List::Util qw(max min);
use Fcntl;

use Encode 'decode';
use Encode 'encode';

use Net::SSL;
use Net::Twitter::Lite::WithAPIv1_1;

use Date::Manip;

use Mozilla::CA;
$ENV{HTTPS_CA_FILE} = Mozilla::CA::SSL_ca_file();

use Furl;
use HTTP::Request::Common;

use DateTime::Format::HTTP;

#my $filePath = shift;
my $name = shift;

#my $env_path="./my_exec/twitterAPI";
my $env_path=".";

my $filePath = "$env_path/vipBCH_test.json";
my $include_rts = "true";
my $since_id = 0;
my $imgURL = "";

my $authTwitter;
if(readJson(\$authTwitter, "$env_path/AuthTwitter.json")!=0){
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
                        screen_name => $name,
                        include_entities => 0,
                    }
                );

my $name_users = sprintf("%s/DEST/%s_users.json",$env_path,$name);
if(writeJson(\$res_users, $name_users, ">")!=0){
    print "FileWriteError. $name_users.\n";
}

for(my $i=0;$i<@{$res_users};$i++){
    my $user_ref = $res_users->[$i];
    if(!exists $user_ref->{"profile_image_url_https"}){
        next;
    }
    if($name ne $user_ref->{"screen_name"}){
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
}

my $vipBCH;
if(readJson(\$vipBCH, $filePath)!=0){
    print "FileReadError. vipBCH.\n";
    exit -1;
}

#%{$vipBCH} = {};
#open( OUT, '+>', $filePath) || exit(-1);
#binmode(OUT, ":utf8");
#local $/ = undef;
#my $json_text = <OUT>;
#$vipBCH = decode_json($json_text );

$vipBCH->{$name}->{"include_rts"} = $include_rts;
if(int($since_id)!=0){
    $vipBCH->{$name}->{"since_id"} = int($since_id);
}
if($imgURL ne ""){
    $vipBCH->{$name}->{"profile_image_url_https"} = $imgURL;
}

#print OUT to_json($vipBCH, {pretty=>1});
#close(OUT);

if(writeJson(\$vipBCH, $filePath, ">")!=0){
    print "FileWriteError. vipBCH.\n";
    exit -1;
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

sub getDate {
       my $date_ref = shift; # OUT

       my $sec;
       my $min;
       my $hour;
       my $mday;
       my $mon;
       my $year;
       my $wday;
       my $yday;
       my $isdst;

       ($sec,$min,$hour,$mday,$mon,$year,$wday,$yday,$isdst) = localtime(time);
       $$date_ref = sprintf("%04d-%02d-%02d %02d:%02d:%02d",$year+1900,$mon+1,$mday,$hour,$min,$sec);

       return (0);
}

sub convertTimeGMTtoJST{
	my $jst_ref	= shift; # OUT
	my $gmt_str	= shift; #  IN

    
    # $tm =~ /^(\s+) (\s+)-(\d+)T(\d+):(\d+):(\d+)/;
	
	my $tm = $gmt_str;
    # 2017-07-19T01:42:20Z
	$tm =~ /^(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)/;
	my $utm = timegm($6, $5, $4, $3, $2-1, $1);
	$$jst_ref = localtime($utm);
	
	return (0);
	#my $tm = $org_closed_on;
	#print "\t\t\t\t\t tm=$tm\n";
	#$tm =~ /^(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)/;
	#my $utm = timegm($6, $5, $4, $3, $2-1, $1);
	#print "\t\t\t\t\t utm=$utm\n";
	#my $jst = localtime($utm);
	#my $jst_str = strftime("%Y-%m-%d %H:%M:%S",localtime($utm));
	#print "\t\t\t\t\t jst=$jst_str\n";
}

sub convertTimeTZtoJST{
	my $jst_ref	= shift; # OUT
	my $tz_str	= shift; #  IN

    # Date_Init("TZ=JST");

    # Wed Dec 20 06:07:43 +0000 2017
    my $date = ParseDate($tz_str);
    $$jst_ref = UnixDate($date,"%Y/%m/%d %H:%M:%S");
	
	return (0);
}
