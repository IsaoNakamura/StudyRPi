#include <stdio.h>
#include <stdlib.h>
#include <math.h>

#include "CamAngleConverter.h"


DF::CamAngleConverter::CamAngleConverter(
	const int&       sc_width,
	const int&       sc_height,
	const double&    angle_diagonal
	) :
	_sc_width(0),
	_sc_height(0),
	_angle_horz(0.0),
	_angle_vert(0.0),
	_angle_diagonal(0.0),
	_initialized(false)
{
	Initialize(sc_width, sc_height, angle_diagonal);
}

int DF::CamAngleConverter::Initialize(
	const int&       sc_width,
	const int&       sc_height,
	const double&    angle_diagonal
	) {
	_initialized = false;
	_sc_width = _sc_height = 0;
	_angle_horz = _angle_vert = _angle_diagonal = 0.0;

	if (!(IsValid(sc_width) && IsValid(sc_height) && IsValid(angle_diagonal))) {
		return -1;
	}

	double angle_horz, angle_vert;
	int ret = MakeViewAngle(angle_horz, angle_vert, sc_width, sc_height, angle_diagonal);
	if (ret != 0) {
		return ret;
	}

	_sc_width = sc_width;
	_sc_height = sc_height;
	_angle_diagonal = angle_diagonal;
	_angle_horz = angle_horz;
	_angle_vert = angle_vert;
	_initialized = true;
	return ret;
}


// �X�N���[�����W����J�����̃s�b�`�p�ƃ��[�p���Z�o���� 
int DF::CamAngleConverter::ScreenToCameraAngle
	(
		double&       camera_yaw,
		double&       camera_pitch,
		const int&    src_u,
		const int&    src_v
		)
{

	// �ԓ��̈�̏����� 
	camera_pitch = 0.0;
	camera_yaw = 0.0;

	// ���͒l�`�F�b�N 
	if (src_u < 0 && _sc_width < src_u) {
		return -1;
	}
	if (src_v < 0 && _sc_height < src_v) {
		return -1;
	}

	// �J�����̃s�b�`�p�ƃ��[�p���Z�o 
	camera_yaw = (static_cast<double>(src_u) / _sc_width * _angle_horz) - (_angle_horz / 2.0);
	camera_pitch = (_angle_vert / 2.0) - (static_cast<double>(src_v) / _sc_height * _angle_vert);

	return 0;
}

// �����E������p���Z�o���� 
int DF::MakeViewAngle(
	double&       angle_horz,
	double&       angle_vert,
	const int&    sc_width,
	const int&    sc_height,
	const double& angle_diagonal) {

	// �ԓ��̈�̏����� 
	angle_horz = 0.0;
	angle_vert = 0.0;

	// d(�Ίp��) 
	double sc_diagonal = sqrt(sc_width*sc_width + sc_height*sc_height);
	if (sc_diagonal <= 1.0e-06) {
		return -1;
	}

	// d�� 
	double rate_width = sc_width / sc_diagonal;
	double rate_height = sc_height / sc_diagonal;

	// �����E������p���Z�o���� 
	angle_horz = angle_diagonal * rate_width;
	angle_vert = angle_diagonal * rate_height;

	return 0;
}

bool DF::IsValid(const double &param) {
	return param > 1.0e-06;
}

bool DF::IsValid(const int &param) {
	return param > 0;
}

