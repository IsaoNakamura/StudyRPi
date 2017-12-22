#!/usr/bin/perl

use strict;

use warnings;
#use Getopt::Std;
#use LWP::UserAgent;
use JSON;

use utf8;
use Encode;

use Time::Local;
use Time::Piece;

#use GD::Graph::mixed;
#use GD::Graph::colour qw( :files );
#use GD::Text;

#use List::Util qw(max min);
use Fcntl;

use Encode 'decode';
use Encode 'encode';

use Net::SSL;
use Net::Twitter::Lite::WithAPIv1_1;
use Data::Dumper;

use Date::Manip;

use Mozilla::CA;
$ENV{HTTPS_CA_FILE} = Mozilla::CA::SSL_ca_file();

use Furl;
use HTTP::Request::Common;

use DateTime::Format::HTTP;

my $host = shift;
my $token = shift;
my $channel = shift;
my $cycle_sec = shift;
my $stopCode = shift;

if(-e $stopCode){
    unlink $stopCode;
    last;
}

my $authTwitter;
if(readJson(\$authTwitter, "./my_exec/twitterAPI/AuthTwitter.json")!=0){
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

while(1){
    eval{
        my $vipBCH;
        if(readJson(\$vipBCH, "./my_exec/twitterAPI/vipBCH.json")!=0){
            print "FileReadError. vipBCH.\n";
            exit -1;
        }

        my @keys_BCH = keys %{$vipBCH};

        for(my $i=0; $i<@keys_BCH; $i++){
            # print "keys_BCH[$i]:$keys_BCH[$i]\n";
            #print $vipBCH->{$keys_BCH[$i]}->{"profile_image_url_https"} . "\n";
            #if($keys_BCH[$i] ne "hoge"){
            #    next;
            #}

            my $res_timeline =  $nt->user_timeline(
                                    {
                                        screen_name => $keys_BCH[$i],
                                        count       => 2,
                                        since_id    => $vipBCH->{$keys_BCH[$i]}->{"since_id"},
                                        include_rts => $vipBCH->{$keys_BCH[$i]}->{"include_rts"},   # 0:RTが含まれない
                                        exclude_replies => "true",                                  # 0:リプライを含む
                                        trim_user => "true",                                        # 0:ユーザ情報を含む
                                        include_entities => "false",                                # 0:entities情報が含まれない
                                        contributor_details => "false",                             # 0:貢献者のscreen_nameが含まれない
                                        page => 0,
                                    }
                                );

            for(my $j=0; $j<@{$res_timeline}; $j++){
                my $tweet_ref = \%{ $res_timeline->[$j] };
                my $id = $tweet_ref->{"id"};
                my $tweet_text = "https://twitter.com/$keys_BCH[$i]/status/$id\n";

                if( exists $tweet_ref->{"created_at"} ){
                    my $created_at = $tweet_ref->{"created_at"};
                    # 終了時間をGMTからJSTに変換して現時間との差を計算する。
                    my $jst_date="";
                    convertTimeTZtoJST(\$jst_date, $created_at);
                    # convertTimeGMTtoJST(\$jst, $created_at);
                    $tweet_text .= "$jst_date\n";
                }

                if( exists $tweet_ref->{"text"} ){
                    my $text = $tweet_ref->{"text"};
                    # print $text . "\n";
                    $tweet_text =  "```" . $tweet_text . $text . "```" ."\n";
                    #$tweet_text =  $tweet_text . $text ."\n";
                    # print $tweet_text;

                    # 添付ファイルURL取得
                    # print "exists extended_entities\n";
                    if( exists $tweet_ref->{"extended_entities"} ){
                        # print "exists media\n";
                        my $extended = $tweet_ref->{"extended_entities"};
                        if( exists $extended->{"media"} ){
                            my $media = $extended->{"media"};
                            for(my $k=0; $k<@{$media}; $k++){
                                my $mediaInfo = \%{ $media->[$k] };
                                if( exists $mediaInfo->{"media_url_https"}){
                                    my $media_url = $mediaInfo->{"media_url_https"};
                                    $tweet_text = $tweet_text . $media_url . "\n";
                                }
                            }
                        }
                    }

                    my $req = POST ($host,
                        'Content' => [
                            token => $token,
                            channel => $channel,
                            # icon_url => $tweet_ref->{"user"}->{"profile_image_url_https"},
                            username => $keys_BCH[$i],
                            icon_url => $vipBCH->{$keys_BCH[$i]}->{"profile_image_url_https"},
                            text => $tweet_text
                        ]);
                    my $res = Furl->new->request($req);
                }

                $vipBCH->{$keys_BCH[$i]}->{"since_id"} = int($id);
                if(writeJson(\$vipBCH, "./my_exec/twitterAPI/vipBCH.json", ">")!=0){
                    print "FileWriteError. vipBCH.\n";
                    next;
                }
                
                if(writeJson(\$tweet_ref, "./my_exec/twitterAPI/DEST/$keys_BCH[$i]_tweet.json", ">")!=0){
                    print "FileWriteError. $keys_BCH[$i].\n";
                    next;
                }

                sleep(3);
            }
        }
    };

    if(-e $stopCode){
        print "recieved StopCode:$stopCode\n";
        unlink $stopCode;
        last;
    }

    sleep($cycle_sec);

}



exit 0;

sub get_followers {  # Usage: @ids = get_followers($screen_name) ;
    my %arg ;
    $_[0] and $arg{'screen_name'} = $_[0] ;  # 省略時は自分になる
    $arg{'cursor'} = -1 ;  # 1ページ目は -1 を指定
    my @ids ;
    while ($arg{'cursor'}){ # 一度に5000までしか取得できないのでcursorを書き換えながら取得を繰り返す
        my $followers_ref = $nt->followers_ids({%arg}) ;
        my $ids_ref = $followers_ref->{'ids'} ;
        push @ids, @$ids_ref ;
        $arg{'cursor'} = $followers_ref->{'next_cursor'} ;
        print STDERR "Fetched: ids=", scalar @$ids_ref, ",next_cursor=$arg{'cursor'}\n" ;
    }
    return @ids ;
} ;

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
