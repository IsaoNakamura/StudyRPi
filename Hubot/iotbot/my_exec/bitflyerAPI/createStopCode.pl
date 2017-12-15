#!/usr/bin/perl

use strict;

my $stopCode = shift;

# my $graphPath = '.\priceGraph.png';
open( OUT, '>',$stopCode) or die( "Cannot open filepath:$stopCode $!" );
print OUT "StopCode\n";
close OUT;

exit 0;

