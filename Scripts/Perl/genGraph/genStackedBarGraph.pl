#!/usr/bin/perl
use strict;

use GD::Graph::bars;
use GD::Graph::hbars;

my $dest_path = '.\stackedBarGraph.png';

my @xLabels  = qw( 1st 2nd 3rd 4th 5th 6th 7th 8th 9th );
my @data2002 = qw(  11  12  15  16   3 1.5   1   3   4 );
my @data2003 = qw(   5  12  24  15  19   8   6  15  21 );
my @data2004 = qw(  12   3   1   5  12   9  16  25  11 );

#my @data     = ( \@xLabels, \@data2002, \@data2003, \@data2004 );

my @data = ( 
    ["1st","2nd","3rd","4th","5th","6th","7th", "8th", "9th"],
    [   11,   12,   15,   16,    3,  1.5,    1,     3,     4], # インデント用データ
    [    5,   12,   24,   15,   19,    8,    6,    15,    21],
    [    12,   3,    1,   5,    12,    9,   16,    25,    11],
);

my $graph = GD::Graph::hbars->new( 800, 600 );

$graph->set( title            => "Stacked Bars (incremental)",
             x_label          => "X Label",
             y_label          => "Y Label",

             #types            => [ qw(bars bars
             #                         linespoints linespoints) ],

             # y_max_value      => 28,
             # y_tick_number    => 14,
             # y_label_skip     => 2,
             
             cumulate         => 1,
             dclrs           => [ undef, qw(dgreen green) ],
             borderclrs      => [ undef, qw(black black) ],
             bar_width        => 10,
             bar_spacing      => 4,
             transparent      => 0
);

$graph->set_legend( undef, qw(low high));

my $image = $graph->plot( \@data ) or die( "Cannot create image" );


open( OUT, '>',$dest_path) or die( "Cannot open filepath:$dest_path $!" );

binmode OUT;
print OUT $image->png();
close OUT;
