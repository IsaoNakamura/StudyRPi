#!/usr/bin/perl

use strict;

use warnings;
use Getopt::Std;
use LWP::UserAgent;
use JSON;

sub HELP_MESSAGE {
	print STDERR <<"EOM";
usage : $0 url DEST_PATH DIFF_THRESHOLD
		-k API_KEY: API Access Key
EOM
	exit 0;
}

my $url = shift;
my $dest = shift;
my $threshold = shift;
HELP_MESSAGE() unless $url;

# print "KEY=$opt_k\n";
# print "URL=$url\n";
# print "DEST=$dest\n";


####################################
my $ua = new LWP::UserAgent;
$ua->timeout(10); # default: 180sec
$ua->ssl_opts( verify_hostname => 0 ); # skip hostname verification
# $ua->default_header('apikey' => $opt_k) if $opt_k;

my $res = $ua->get($url);

die $res->message if $res->is_error;

my $content = $res->content;

# convert UTF-8 binary to text
utf8::decode($content); 

my $content_ref = from_json($content);
my $curr_mid = $content_ref->{"mid"};

if(-e $dest){
	# dest-file is exist.

	# prev-result
	my $prev_result;
	if( readJson(\$prev_result, $dest)==0){
		my $prev_mid = $prev_result->{"mid"};
		my $diff_mid = $curr_mid - $prev_mid;
		my $diff_rate = $diff_mid / $prev_mid * 100.0;

		print "CurrMiddleRate:" . $curr_mid . "[BTC/JPY]\n";
		print "PrevMiddleRate:" . $prev_mid . "[BTC/JPY]\n";
		print "DiffMiddleRate:" . $diff_mid . "[BTC/JPY]\n";
		print "diff_rate:" . $diff_rate . "[%]\n";

		if(abs($diff_mid)>=$threshold){
			#print "MiddleRate:" . $content_ref->{"mid"} . "[BTC/JPY]\n";
			#print "BuyRate   :" . $content_ref->{"ask"} . "[BTC/JPY]\n";
			#print "SellRate  :" . $content_ref->{"bid"} . "[BTC/JPY]\n";

			#print "PrevMiddleRate:" . $prev_mid . "[BTC/JPY]\n";

			# print "DiffMiddleRate:" . $diff_mid . "[BTC/JPY]\n";

			#print "RateOfChange:" . $diff_rate . "[%]\n";

			my $report;
			if($diff_mid<0){
				$report = sprintf("%d[BTC/JPY] (  %d[BTC/JPY],  %.01f\[%%] ) Updated Checkpoint.\n",$curr_mid, $diff_mid, $diff_rate);
			}else{
				$report = sprintf("%d[BTC/JPY] ( +%d[BTC/JPY], +%.01f\[%%] ) Updated Checkpoint.\n",$curr_mid, $diff_mid, $diff_rate);
			}
			print $report;

			# save to Json.
			open (OUT, '>', $dest) || die('File Open Error');
			print OUT to_json($content_ref, {pretty=>1});
			close(OUT);
		}
	}else{
		print "FileOpenError. $dest\n";
	}
}else{
	# dest-file is-no exist.

	my $checkpoint = sprintf("Checkpoint: %d[BTC/JPY]\n",$curr_mid);
	print $checkpoint;
	
	# save to Json.
	open (OUT, '>', $dest) || die('File Open Error');
	print OUT to_json($content_ref, {pretty=>1});
	close(OUT);
}
exit 0;

sub readJson {
	my $hash_ref = shift; # OUT
	my $filePath = shift; #IN

	open( IN, '<', $filePath) || return(1);
	eval{
		local $/ = undef;
		my $json_text = <IN>;
		close(IN);
		$$hash_ref = decode_json($json_text );
	};
	return (0);
}
