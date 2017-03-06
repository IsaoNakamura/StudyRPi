#!/usr/bin/perl
use strict;

use GD::Graph::mixed;
use GD::Graph::colour qw( :files );
use GD::Text;

my $dest_path = '.\mixGraph.png';
# my $rgb_path = '.\rgb.txt';


my @xLabels  = qw( 00-03 03-06 06-09 09-12 12-15 15-18 18-21 21-00 );
my @avDown   = qw(     3     3     6    24    16    16    16    12 );
my @avUp     = qw(     1     1     2     3     2     2     1     1 );
my @down     = qw(   4.2   3.5  9.25    22  17.5  14.8  12.25  8.3 );
my @up       = qw(   0.25 1.75  1.65   3.25  2.1  1.85   0.95  0.3 );
my @data     = ( \@xLabels, \@avDown, \@avUp, \@down, \@up );

my $graph = GD::Graph::mixed->new( 800, 600 );

#GD::Graph::colour::read_rgb( $rgb_path ) or
#  die( "Can't read colours" );

$graph->set( title            => "Data flow 6th April 2004",
             t_margin         => 10,
             b_margin         => 10,
             l_margin         => 10,
             r_margin         => 10,
             x_label          => "Time (3 hour blocks)",
             x_label_position => 0.5,
             y_label          => "MB/sec",
             types            => [ qw(bars bars
                                      linespoints linespoints) ],
#             dclrs            => [ qw(LightYellow1 LightYellow4
#                                      orange1 orange2) ],
             y_max_value      => 28,
             y_tick_number    => 14,
             y_label_skip     => 2,
             line_width       => 2,
             long_ticks       => 1,
             bar_width        => 10,
             bar_spacing      => 3,
             markers          => [ 5, 5 ],
             legend_placement => "RT" );

$graph->set_legend( "Budgeted download",
                    "Budgeted upload",
                    "Actual download",
                    "Actual upload" );

# GD::Text->font_path( "/usr/lib/X11/fonts/truetype/" );
# $graph->set_title_font( "luximr", 16 );
# $graph->set_legend_font( "luximr", 10 );
# $graph->set_x_axis_font( "luximr", 9 );
# $graph->set_x_label_font( "luximr", 11 );
# $graph->set_y_axis_font( "luximr", 9 );
# $graph->set_y_label_font( "luximr", 11 );

my $image = $graph->plot( \@data ) or die( "Cannot create image" );

open( OUT, '>',$dest_path) or die( "Cannot open filepath:$dest_path $!" );

binmode OUT;
print OUT $image->png();
close OUT;