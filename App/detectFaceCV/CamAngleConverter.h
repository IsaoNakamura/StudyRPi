#pragma once
#include <map>

namespace DF {

	template<typename T>
	struct XY {
		T x;
		T y;
		XY() {};
		XY(T _x, T _y) :x(_x), y(_y) {}
		XY& operator=(const XY& r) {
			x = r.x;
			y = r.y;
			return *this;
		}
		bool operator<(const XY& r) const {
			if (x < r.x) { return true; }
			return y < r.y;
		}
	};

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

		std::map< XY<int>, XY<double> > _cache;
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
