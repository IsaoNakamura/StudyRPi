#!/usr/bin/perl

use strict;

use warnings;
use Getopt::Std;
use LWP::UserAgent;
use JSON;

use Time::Local;
use Time::Piece;

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
print "price is " . $curr_mid . "\n";


my $key_date;
getDate(\$key_date);
print "date is " . $key_date . "\n";

my $priceHash_ref;
if(-e $dest){
	# dest-file is exist.

	# Load from Json.
	if( readJson(\$priceHash_ref, $dest)==0){
		# print "FileOpenSuccess. $dest\n";
	}else{
		print "FileOpenError. $dest\n";
		exit -1;
	}
}

if(exists $priceHash_ref->{$key_date}){
	# keys is exist.
	print "$key_date is exist.\n";
}else{
	# Add key-value.
	$priceHash_ref->{$key_date} = $curr_mid;
}

# Save to Json.
if(writeJson(\$priceHash_ref, $dest, ">")!=0){
	print "FileSaveError. $dest\n";
	exit -1;
}

# Display Keys.
#my @price_keys = keys( %{$priceHash_ref} );
#@price_keys = sort { $a cmp $b } @price_keys;
#for(my $i=0; $i<@price_keys; $i++){
#	print "date[$i]:$price_keys[$i] price:$priceHash_ref->{$price_keys[$i]}\n";
#}

exit 0;

sub writeJson {
	my $hash_ref = shift; #IN
	my $filePath = shift; #IN
	my $mode = shift;

	# save to Json.
	open (OUT, $mode, $filePath) || return(1);
	print OUT to_json($$hash_ref, {pretty=>1});
	close(OUT);
	return (0);
}

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
