/*
 * LinearTransformation.h
 *
 *  Created on: 2015/12/08
 *      Author: isao
 */

#ifndef LINEARTRANSFORMATION_H_
#define LINEARTRANSFORMATION_H_

class LinearTransformation {
private:
	LinearTransformation();
	virtual ~LinearTransformation();

public:
	static bool convertValueMinMidMax(	int&		dst_val,
										const int&	src_val,
										const int&	src_min,
										const int&	src_mid,
										const int&	src_max,
										const int&	dst_min,
										const int&	dst_mid,
										const int&	dst_max,
										const bool&	isReverse=false	);
	};

#endif /* LINEARTRANSFORMATION_H_ */
