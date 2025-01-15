#pragma once


namespace rew
{
	//--------------------------------------------------------------------------------------------------------------------------------
	//------------------------------------------------------- 数学 -------------------------------------------------------------------
	//--------------------------------------------------------------------------------------------------------------------------------

	inline float Abs(float x);
	inline float Sqrt(float x);
	inline float Sqrt(float x, float tolerance, int max_iterations = 100);

	//--------------------------------------------------------------------------------------------------------------------------------
	//------------------------------------------------------- 几何 -------------------------------------------------------------------
	//--------------------------------------------------------------------------------------------------------------------------------

	struct Vector2;
	struct Vector3;
	struct Quaternion;
	struct AABB;
	struct AABB6f;


	//二维向量
	struct Vector2
	{
	public:
		float x;
		float y;
		Vector2() { x = 0; y = 0; }
		Vector2(float newx, float newy) { x = newx; y = newy; }
	};

	//三维向量
	struct Vector3
	{
	public:
		float x;
		float y;
		float z;
		Vector3() : x(0), y(0), z(0) {}
		Vector3(float newx, float newy, float newz) : x(newx), y(newy), z(newz) {}

		inline float Magnitude() const;
		inline float SqrMagnitude() const;


		static inline float Dot(const Vector3& v1, const Vector3& v2);
		static inline Vector3 Cross(const Vector3& v1, const Vector3& v2);
		static inline Vector3 Normalize(const Vector3& v);



		Vector3 operator+(const Vector3& other) const
		{
			return Vector3(x + other.x, y + other.y, z + other.z);
		}

		Vector3 operator-(const Vector3& other) const
		{
			return Vector3(x - other.x, y - other.y, z - other.z);
		}

		Vector3 operator*(float scalar) const
		{
			return Vector3(x * scalar, y * scalar, z * scalar);
		}

		Vector3 operator/(float scalar) const
		{
			return Vector3(x / scalar, y / scalar, z / scalar);
		}
		Vector3& operator+=(const Vector3& other)
		{
			x += other.x;
			y += other.y;
			z += other.z;
			return *this;
		}

		Vector3& operator-=(const Vector3& other)
		{
			x -= other.x;
			y -= other.y;
			z -= other.z;
			return *this;
		}

		Vector3& operator*=(float scalar)
		{
			x *= scalar;
			y *= scalar;
			z *= scalar;
			return *this;
		}

		Vector3& operator/=(float scalar)
		{
			x /= scalar;
			y /= scalar;
			z /= scalar;
			return *this;
		}

		bool operator==(const Vector3& other) const
		{
			return x == other.x && y == other.y && z == other.z;
		}

	};

	//四元数
	struct Quaternion
	{
	public:
		float x;
		float y;
		float z;
		float w;

		Quaternion() { x = 0; y = 0; z = 0; w = 0; }
	};

	//平面  
	struct Plane
	{
	public:
		Vector3 normal;
		float distance;
		Plane() : normal(Vector3()), distance(0) {}
		Plane(Vector3 norm, float dist) : normal(norm), distance(dist) {}
	};


	//AABB
	struct AABB
	{
	public:
		rew::Vector3 center;
		rew::Vector3 half_length;
	public:
		AABB() {}
		AABB(AABB6f b);
	};

	struct AABB6f
	{
	public:
		float min_x;
		float min_y;
		float min_z;
		float max_x;
		float max_y;
		float max_z;
	public:
		void ExtendTo(float newx, float newy, float newz);
	};





}







inline float rew::Abs(float x)
{
	if (x < 0) return -x;
	return x;
}
inline float rew::Sqrt(float x)
{
	// 检查输入是否非负
	if (x < 0) {
		return -1;
	}

	// 处理零的特殊情况
	if (x == 0) {
		return 0;
	}

	// 初始化近似值
	float approx = x;

	// 迭代逼近平方根
	const float tolerance = (float)1e-6; // 精度要求

	// 迭代逼近平方根
	for (int i = 0; i < 200; ++i) {
		float betterApprox = 0.5f * (approx + x / approx);
		if (fabs(betterApprox - approx) < tolerance) {
			return betterApprox;
		}
		approx = betterApprox;
	}
	// 如果达到最大迭代次数仍未达到精度要求，返回近似值
	return approx;
}

inline float rew::Sqrt(float x, float tolerance, int max_iterations) {
	// 检查输入是否非负
	if (x < 0) {
		std::cerr << "Error: Cannot compute the square root of a negative number." << std::endl;
		return -1;
	}

	// 处理零的特殊情况
	if (x == 0) {
		return 0;
	}

	// 初始化近似值
	float approx = x;

	// 迭代逼近平方根
	for (int i = 0; i < max_iterations; ++i) {
		float betterApprox = 0.5f * (approx + x / approx);
		if (fabs(betterApprox - approx) < tolerance) {
			return betterApprox;
		}
		approx = betterApprox;
	}

	// 如果达到最大迭代次数仍未达到精度要求，返回近似值
	return approx;
}








inline float rew::Vector3::Magnitude() const
{
	return rew::Sqrt(x * x + y * y + z * z);
};
inline float rew::Vector3::SqrMagnitude() const
{
	return (x * x + y * y + z * z);
};



inline float rew::Vector3::Dot(const rew::Vector3& v1, const rew::Vector3& v2)
{
	return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
}

inline rew::Vector3 rew::Vector3::Cross(const rew::Vector3& v1, const rew::Vector3& v2)
{
	return Vector3(
		v1.y * v2.z - v1.z * v2.y,
		v1.z * v2.x - v1.x * v2.z,
		v1.x * v2.y - v1.y * v2.x
	);
}

inline rew::Vector3 rew::Vector3::Normalize(const rew::Vector3& v)
{
	float mag = v.Magnitude();
	if (mag == 0) return rew::Vector3(0, 0, 0);
	return v / mag;
}


