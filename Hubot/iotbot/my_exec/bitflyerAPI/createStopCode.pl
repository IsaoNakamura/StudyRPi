#!/usr/bin/perl

use strict;

my $stopCode = shift;

open( OUT, '>',$stopCode) or die( "Cannot open filepath:$stopCode $!" );
print OUT "StopCode\n";
close OUT;

exit 0;

