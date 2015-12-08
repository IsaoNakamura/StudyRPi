/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <math.h>

#include "LinearTransformation.h"

bool LinearTransformation::convertValueMinMidMax(	int&		dst_val,
													const int&	src_val,
													const int&	src_min,
													const int&	src_mid,
													const int&	src_max,
													const int&	dst_min,
													const int&	dst_mid,
													const int&	dst_max,
													const bool&	isReverse/*=false*/	)
{
	if(src_val==src_mid){ // 中間値
		dst_val = dst_mid;
	}else if(src_val > src_mid){ // 右
		double ratio = fabs( static_cast<double>(src_val) / static_cast<double>(src_max) );
		
		if(isReverse){
			int delta = static_cast<int>( static_cast<double>( dst_mid - dst_min) * ratio );
			dst_val = dst_mid - delta;
		}else{
			int delta = static_cast<int>( static_cast<double>( dst_max - dst_mid ) * ratio );
			dst_val = dst_mid + delta;
		}
		
	}else if(src_val < src_mid){ // 左
		double ratio = fabs( static_cast<double>(src_val) / static_cast<double>(src_min) );
		
		if(isReverse){
			int delta = static_cast<int>( static_cast<double>( dst_max - dst_mid ) * ratio );
			dst_val = dst_mid + delta;
		}else{
			int delta = static_cast<int>( static_cast<double>( dst_mid - dst_min) * ratio );
			dst_val = dst_mid - delta;
		}
		
	}
	return true;
}
