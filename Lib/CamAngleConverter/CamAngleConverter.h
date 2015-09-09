#pragma once
#include <map>

namespace DF {

	class CamAngleConverter {
	public:
		CamAngleConverter(
			const int&       sc_width,
			const int&       sc_height,
			const double&    angle_diagonal
			);
		CamAngleConverter() {}
		~CamAngleConverter() {}

		int ScreenToCameraAngle(
			double&       camera_pitch,
			double&       camera_yaw,
			const int&    src_u,
			const int&    src_v);

		int Initialize(
			const int&       sc_width,
			const int&       sc_height,
			const double&    angle_diagonal
			);

		bool Initialized() { return _initialized; }

	private:
		int       _sc_width;
		int       _sc_height;
		double    _angle_horz;
		double    _angle_vert;
		double    _angle_diagonal;
		bool      _initialized;

	};

	bool IsValid(const double& param);
	bool IsValid(const int&    param);

	int  MakeViewAngle(
		double&       angle_horz,
		double&       angle_vert,
		const int&    sc_width,
		const int&    sc_height,
		const double& angle_diagonal);
		
};
