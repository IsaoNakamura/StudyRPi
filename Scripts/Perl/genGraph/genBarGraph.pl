#!/usr/bin/perl
use strict;

use GD::Graph::bars;

my $dest_path = '.\barGraph.png';

my @xLabels  = qw( Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec );
my @data2002 = qw(  17  19  26  38  56  64  67  53  40  29  21  13 );
my @data2003 = qw(  19  24  27  41  56  69  75  60  44  33  22  15 );
my @data     = ( \@xLabels, \@data2002, \@data2003 );

my $graph = GD::Graph::bars->new( 800, 600 );

$graph->set( title   => "Rainfall 2002/2003",
             y_label => "Millimetres" );

my $image = $graph->plot( \@data ) or die( "Cannot create image" );

open( OUT, '>',$dest_path) or die( "Cannot open filepath:$dest_path $!" );

binmode OUT;
print OUT $image->png();
close OUT;
