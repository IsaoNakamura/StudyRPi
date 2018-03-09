#!/usr/bin/perl

package MyModule::UtilityTime;

use strict;
use warnings;

use utf8;
use Encode;

use Encode 'decode';
use Encode 'encode';

use Date::Manip;
use DateTime::Format::HTTP;

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

1;
__END__