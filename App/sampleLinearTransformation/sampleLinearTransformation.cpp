/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <math.h>

#include "../../Lib/utilities/LinearTransformation/LinearTransformation.h"

#define DST_MIN	(0)
#define DST_MID	(90)
#define DST_MAX	(180)

#define SRC_MIN	(-32767)
#define SRC_MID	(     0)
#define SRC_MAX	( 32767)

#define LOOP_STRIDE (363)

int main(int argc, char* argv[])
{
	int iRet = 0;
	try
	{
		printf("SRC_MIN=%d,SRC_MID=%d,SRC_MAX=%d\n",SRC_MIN,SRC_MID,SRC_MAX);
		printf("DST_MIN=%d,DST_MID=%d,DST_MAX=%d\n",DST_MIN,DST_MID,DST_MAX);
		printf("LOOP_STRIDE=%d\n",LOOP_STRIDE);
		
		bool isOutMid = false;
		bool isOutMax = false;
		
		for( int src_val=SRC_MIN; src_val<SRC_MAX; src_val+=LOOP_STRIDE){
			int dst_val = 0;
			if(! LinearTransformation::convertValueMinMidMax(	dst_val,
																src_val,
																SRC_MIN,
																SRC_MID,
																SRC_MAX,
																DST_MIN,
																DST_MID,
																DST_MAX,
																false		) ){
				throw 0;					
			}

			int dst_val_rvs = 0;
			if(! LinearTransformation::convertValueMinMidMax(	dst_val_rvs,
																src_val,
																SRC_MIN,
																SRC_MID,
																SRC_MAX,
																DST_MIN,
																DST_MID,
																DST_MAX,
																true		) ){
				throw 0;					
			}
			printf("src_val=%d, dst_val=%d, dst_val_rvs=%d\n", src_val, dst_val, dst_val_rvs);
			
			if( ((src_val+LOOP_STRIDE)>SRC_MID) && !isOutMid ){
				int dst_val = 0;
				if(! LinearTransformation::convertValueMinMidMax(	dst_val,
																	SRC_MID,
																	SRC_MIN,
																	SRC_MID,
																	SRC_MAX,
																	DST_MIN,
																	DST_MID,
																	DST_MAX,
																	false		) ){
					throw 0;					
				}
	
				int dst_val_rvs = 0;
				if(! LinearTransformation::convertValueMinMidMax(	dst_val_rvs,
																	SRC_MID,
																	SRC_MIN,
																	SRC_MID,
																	SRC_MAX,
																	DST_MIN,
																	DST_MID,
																	DST_MAX,
																	true		) ){
					throw 0;					
				}
				printf("src_val=%d, dst_val=%d, dst_val_rvs=%d\n", SRC_MID, dst_val, dst_val_rvs);
				isOutMid = true;
			}
			
			if( ((src_val+LOOP_STRIDE)>SRC_MAX) && !isOutMax ){
				int dst_val = 0;
				if(! LinearTransformation::convertValueMinMidMax(	dst_val,
																	SRC_MAX,
																	SRC_MIN,
																	SRC_MID,
																	SRC_MAX,
																	DST_MIN,
																	DST_MID,
																	DST_MAX,
																	false		) ){
					throw 0;					
				}
	
				int dst_val_rvs = 0;
				if(! LinearTransformation::convertValueMinMidMax(	dst_val_rvs,
																	SRC_MAX,
																	SRC_MIN,
																	SRC_MID,
																	SRC_MAX,
																	DST_MIN,
																	DST_MID,
																	DST_MAX,
																	true		) ){
					throw 0;					
				}
				printf("src_val=%d, dst_val=%d, dst_val_rvs=%d\n", SRC_MAX, dst_val, dst_val_rvs);
				isOutMid = true;
			}
		}
	}
	catch(...)
	{
		printf("catch!! \n");
		iRet = -1;
	}

	return iRet;
}
