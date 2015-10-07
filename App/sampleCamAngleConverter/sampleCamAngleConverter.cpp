//////////////////////////////////////////////
// this main() is written by shuhei_yamamoto//
//////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include "../../Lib/utilities/CamAngleConverter/CamAngleConverter.h"

int main(int argc, char* argv[])
{
	int iRet = 0;
	const int w = 320;
	const int h = 240;
	const double ad = 60.0;
	
	DF::CamAngleConverter camAngCvt(w, h, ad);
	
	if (camAngCvt.Initialized()) {
		
		for( int sx = 0; sx < w;  ++sx ){
			for( int sy = 0; sy < h; ++sy ){
				double p, y;
				int ret = camAngCvt.ScreenToCameraAngle(y, p, sx, sy);
				printf("ret=[%d] screenXY[%d,%d] = yaw-pitch[%f,%f]\n", ret, sx, sy ,y,p);
			}
		}

	}

	return iRet;
}
