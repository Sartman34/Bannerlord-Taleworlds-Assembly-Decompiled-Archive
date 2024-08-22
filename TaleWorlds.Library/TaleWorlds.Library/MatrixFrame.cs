using System;

namespace TaleWorlds.Library;

[Serializable]
public struct MatrixFrame
{
	public Mat3 rotation;

	public Vec3 origin;

	public static MatrixFrame Identity => new MatrixFrame(Mat3.Identity, new Vec3(0f, 0f, 0f, 1f));

	public static MatrixFrame Zero => new MatrixFrame(new Mat3(Vec3.Zero, Vec3.Zero, Vec3.Zero), new Vec3(0f, 0f, 0f, 1f));

	public bool IsIdentity
	{
		get
		{
			if (!origin.IsNonZero)
			{
				return rotation.IsIdentity();
			}
			return false;
		}
	}

	public bool IsZero
	{
		get
		{
			if (!origin.IsNonZero)
			{
				return rotation.IsZero();
			}
			return false;
		}
	}

	public float this[int i, int j]
	{
		get
		{
			return i switch
			{
				0 => rotation.s[j], 
				1 => rotation.f[j], 
				2 => rotation.u[j], 
				3 => origin[j], 
				_ => throw new IndexOutOfRangeException("MatrixFrame out of bounds."), 
			};
		}
		set
		{
			switch (i)
			{
			case 0:
				rotation.s[j] = value;
				break;
			case 1:
				rotation.f[j] = value;
				break;
			case 2:
				rotation.u[j] = value;
				break;
			case 3:
				origin[j] = value;
				break;
			default:
				throw new IndexOutOfRangeException("MatrixFrame out of bounds.");
			}
		}
	}

	public MatrixFrame(Mat3 rot, Vec3 o)
	{
		rotation = rot;
		origin = o;
	}

	public MatrixFrame(float _11, float _12, float _13, float _21, float _22, float _23, float _31, float _32, float _33, float _41, float _42, float _43)
	{
		rotation = new Mat3(_11, _12, _13, _21, _22, _23, _31, _32, _33);
		origin = new Vec3(_41, _42, _43);
	}

	public MatrixFrame(float _11, float _12, float _13, float _14, float _21, float _22, float _23, float _24, float _31, float _32, float _33, float _34, float _41, float _42, float _43, float _44)
	{
		rotation = default(Mat3);
		rotation.s = new Vec3(_11, _12, _13, _14);
		rotation.f = new Vec3(_21, _22, _23, _24);
		rotation.u = new Vec3(_31, _32, _33, _34);
		origin = new Vec3(_41, _42, _43, _44);
	}

	public Vec3 TransformToParent(Vec3 v)
	{
		return new Vec3(rotation.s.x * v.x + rotation.f.x * v.y + rotation.u.x * v.z + origin.x, rotation.s.y * v.x + rotation.f.y * v.y + rotation.u.y * v.z + origin.y, rotation.s.z * v.x + rotation.f.z * v.y + rotation.u.z * v.z + origin.z);
	}

	public Vec3 TransformToLocal(Vec3 v)
	{
		Vec3 vec = v - origin;
		return new Vec3(rotation.s.x * vec.x + rotation.s.y * vec.y + rotation.s.z * vec.z, rotation.f.x * vec.x + rotation.f.y * vec.y + rotation.f.z * vec.z, rotation.u.x * vec.x + rotation.u.y * vec.y + rotation.u.z * vec.z);
	}

	public Vec3 TransformToLocalNonUnit(Vec3 v)
	{
		Vec3 vec = v - origin;
		return new Vec3(rotation.s.x * vec.x + rotation.s.y * vec.y + rotation.s.z * vec.z, rotation.f.x * vec.x + rotation.f.y * vec.y + rotation.f.z * vec.z, rotation.u.x * vec.x + rotation.u.y * vec.y + rotation.u.z * vec.z);
	}

	public bool NearlyEquals(MatrixFrame rhs, float epsilon = 1E-05f)
	{
		if (rotation.NearlyEquals(rhs.rotation, epsilon))
		{
			return origin.NearlyEquals(rhs.origin, epsilon);
		}
		return false;
	}

	public Vec3 TransformToLocalNonOrthogonal(Vec3 v)
	{
		return new MatrixFrame(rotation.s.x, rotation.s.y, rotation.s.z, 0f, rotation.f.x, rotation.f.y, rotation.f.z, 0f, rotation.u.x, rotation.u.y, rotation.u.z, 0f, origin.x, origin.y, origin.z, 1f).Inverse().TransformToParent(v);
	}

	public MatrixFrame TransformToLocalNonOrthogonal(ref MatrixFrame frame)
	{
		return new MatrixFrame(rotation.s.x, rotation.s.y, rotation.s.z, 0f, rotation.f.x, rotation.f.y, rotation.f.z, 0f, rotation.u.x, rotation.u.y, rotation.u.z, 0f, origin.x, origin.y, origin.z, 1f).Inverse().TransformToParent(frame);
	}

	public static MatrixFrame Lerp(MatrixFrame m1, MatrixFrame m2, float alpha)
	{
		MatrixFrame result = default(MatrixFrame);
		result.rotation = Mat3.Lerp(m1.rotation, m2.rotation, alpha);
		result.origin = Vec3.Lerp(m1.origin, m2.origin, alpha);
		return result;
	}

	public static MatrixFrame Slerp(MatrixFrame m1, MatrixFrame m2, float alpha)
	{
		MatrixFrame result = default(MatrixFrame);
		result.origin = Vec3.Lerp(m1.origin, m2.origin, alpha);
		result.rotation = Quaternion.Slerp(Quaternion.QuaternionFromMat3(m1.rotation), Quaternion.QuaternionFromMat3(m2.rotation), alpha).ToMat3;
		return result;
	}

	public MatrixFrame TransformToParent(MatrixFrame m)
	{
		return new MatrixFrame(rotation.TransformToParent(m.rotation), TransformToParent(m.origin));
	}

	public MatrixFrame TransformToLocal(MatrixFrame m)
	{
		return new MatrixFrame(rotation.TransformToLocal(m.rotation), TransformToLocal(m.origin));
	}

	public Vec3 TransformToParentWithW(Vec3 _s)
	{
		return new Vec3(rotation.s.x * _s.x + rotation.f.x * _s.y + rotation.u.x * _s.z + origin.x * _s.w, rotation.s.y * _s.x + rotation.f.y * _s.y + rotation.u.y * _s.z + origin.y * _s.w, rotation.s.z * _s.x + rotation.f.z * _s.y + rotation.u.z * _s.z + origin.z * _s.w, rotation.s.w * _s.x + rotation.f.w * _s.y + rotation.u.w * _s.z + origin.w * _s.w);
	}

	public MatrixFrame GetUnitRotFrame(float removedScale)
	{
		return new MatrixFrame(rotation.GetUnitRotation(removedScale), origin);
	}

	public MatrixFrame Inverse()
	{
		AssertFilled();
		MatrixFrame matrix = default(MatrixFrame);
		float num = this[2, 2] * this[3, 3] - this[2, 3] * this[3, 2];
		float num2 = this[1, 2] * this[3, 3] - this[1, 3] * this[3, 2];
		float num3 = this[1, 2] * this[2, 3] - this[1, 3] * this[2, 2];
		float num4 = this[0, 2] * this[3, 3] - this[0, 3] * this[3, 2];
		float num5 = this[0, 2] * this[2, 3] - this[0, 3] * this[2, 2];
		float num6 = this[0, 2] * this[1, 3] - this[0, 3] * this[1, 2];
		float num7 = this[2, 1] * this[3, 3] - this[2, 3] * this[3, 1];
		float num8 = this[1, 1] * this[3, 3] - this[1, 3] * this[3, 1];
		float num9 = this[1, 1] * this[2, 3] - this[1, 3] * this[2, 1];
		float num10 = this[0, 1] * this[3, 3] - this[0, 3] * this[3, 1];
		float num11 = this[0, 1] * this[2, 3] - this[0, 3] * this[2, 1];
		float num12 = this[1, 1] * this[3, 3] - this[1, 3] * this[3, 1];
		float num13 = this[0, 1] * this[1, 3] - this[0, 3] * this[1, 1];
		float num14 = this[2, 1] * this[3, 2] - this[2, 2] * this[3, 1];
		float num15 = this[1, 1] * this[3, 2] - this[1, 2] * this[3, 1];
		float num16 = this[1, 1] * this[2, 2] - this[1, 2] * this[2, 1];
		float num17 = this[0, 1] * this[3, 2] - this[0, 2] * this[3, 1];
		float num18 = this[0, 1] * this[2, 2] - this[0, 2] * this[2, 1];
		float num19 = this[0, 1] * this[1, 2] - this[0, 2] * this[1, 1];
		matrix[0, 0] = this[1, 1] * num - this[2, 1] * num2 + this[3, 1] * num3;
		matrix[0, 1] = (0f - this[0, 1]) * num + this[2, 1] * num4 - this[3, 1] * num5;
		matrix[0, 2] = this[0, 1] * num2 - this[1, 1] * num4 + this[3, 1] * num6;
		matrix[0, 3] = (0f - this[0, 1]) * num3 + this[1, 1] * num5 - this[2, 1] * num6;
		matrix[1, 0] = (0f - this[1, 0]) * num + this[2, 0] * num2 - this[3, 0] * num3;
		matrix[1, 1] = this[0, 0] * num - this[2, 0] * num4 + this[3, 0] * num5;
		matrix[1, 2] = (0f - this[0, 0]) * num2 + this[1, 0] * num4 - this[3, 0] * num6;
		matrix[1, 3] = this[0, 0] * num3 - this[1, 0] * num5 + this[2, 0] * num6;
		matrix[2, 0] = this[1, 0] * num7 - this[2, 0] * num8 + this[3, 0] * num9;
		matrix[2, 1] = (0f - this[0, 0]) * num7 + this[2, 0] * num10 - this[3, 0] * num11;
		matrix[2, 2] = this[0, 0] * num12 - this[1, 0] * num10 + this[3, 0] * num13;
		matrix[2, 3] = (0f - this[0, 0]) * num9 + this[1, 0] * num11 - this[2, 0] * num13;
		matrix[3, 0] = (0f - this[1, 0]) * num14 + this[2, 0] * num15 - this[3, 0] * num16;
		matrix[3, 1] = this[0, 0] * num14 - this[2, 0] * num17 + this[3, 0] * num18;
		matrix[3, 2] = (0f - this[0, 0]) * num15 + this[1, 0] * num17 - this[3, 0] * num19;
		matrix[3, 3] = this[0, 0] * num16 - this[1, 0] * num18 + this[2, 0] * num19;
		float num20 = this[0, 0] * matrix[0, 0] + this[1, 0] * matrix[0, 1] + this[2, 0] * matrix[0, 2] + this[3, 0] * matrix[0, 3];
		if (num20 != 1f)
		{
			DivideWith(ref matrix, num20);
		}
		return matrix;
	}

	private static void DivideWith(ref MatrixFrame matrix, float w)
	{
		float num = 1f / w;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				matrix[i, j] *= num;
			}
		}
	}

	public void Rotate(float radian, Vec3 axis)
	{
		MathF.SinCos(radian, out var sa, out var ca);
		MatrixFrame matrixFrame = default(MatrixFrame);
		matrixFrame[0, 0] = axis.x * axis.x * (1f - ca) + ca;
		matrixFrame[1, 0] = axis.x * axis.y * (1f - ca) - axis.z * sa;
		matrixFrame[2, 0] = axis.x * axis.z * (1f - ca) + axis.y * sa;
		matrixFrame[3, 0] = 0f;
		matrixFrame[0, 1] = axis.y * axis.x * (1f - ca) + axis.z * sa;
		matrixFrame[1, 1] = axis.y * axis.y * (1f - ca) + ca;
		matrixFrame[2, 1] = axis.y * axis.z * (1f - ca) - axis.x * sa;
		matrixFrame[3, 1] = 0f;
		matrixFrame[0, 2] = axis.x * axis.z * (1f - ca) - axis.y * sa;
		matrixFrame[1, 2] = axis.y * axis.z * (1f - ca) + axis.x * sa;
		matrixFrame[2, 2] = axis.z * axis.z * (1f - ca) + ca;
		matrixFrame[3, 2] = 0f;
		matrixFrame[0, 3] = 0f;
		matrixFrame[1, 3] = 0f;
		matrixFrame[2, 3] = 0f;
		matrixFrame[3, 3] = 1f;
		origin = TransformToParent(matrixFrame.origin);
		rotation = rotation.TransformToParent(matrixFrame.rotation);
	}

	public static MatrixFrame operator *(MatrixFrame m1, MatrixFrame m2)
	{
		return m1.TransformToParent(m2);
	}

	public static bool operator ==(MatrixFrame m1, MatrixFrame m2)
	{
		if (m1.origin == m2.origin)
		{
			return m1.rotation == m2.rotation;
		}
		return false;
	}

	public static bool operator !=(MatrixFrame m1, MatrixFrame m2)
	{
		if (!(m1.origin != m2.origin))
		{
			return m1.rotation != m2.rotation;
		}
		return true;
	}

	public override string ToString()
	{
		string text = "MatrixFrame:\n";
		text += "Rotation:\n";
		text += rotation.ToString();
		return text + "Origin: " + origin.x + ", " + origin.y + ", " + origin.z + "\n";
	}

	public override bool Equals(object obj)
	{
		return this == (MatrixFrame)obj;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public MatrixFrame Strafe(float a)
	{
		origin += rotation.s * a;
		return this;
	}

	public MatrixFrame Advance(float a)
	{
		origin += rotation.f * a;
		return this;
	}

	public MatrixFrame Elevate(float a)
	{
		origin += rotation.u * a;
		return this;
	}

	public void Scale(Vec3 scalingVector)
	{
		MatrixFrame identity = Identity;
		identity.rotation.s.x = scalingVector.x;
		identity.rotation.f.y = scalingVector.y;
		identity.rotation.u.z = scalingVector.z;
		origin = TransformToParent(identity.origin);
		rotation = rotation.TransformToParent(identity.rotation);
	}

	public Vec3 GetScale()
	{
		return new Vec3(rotation.s.Length, rotation.f.Length, rotation.u.Length);
	}

	public static MatrixFrame CreateLookAt(Vec3 position, Vec3 target, Vec3 upVector)
	{
		Vec3 vec = target - position;
		vec.Normalize();
		Vec3 vec2 = Vec3.CrossProduct(upVector, vec);
		vec2.Normalize();
		Vec3 v = Vec3.CrossProduct(vec, vec2);
		float x = vec2.x;
		float x2 = v.x;
		float x3 = vec.x;
		float _ = 0f;
		float y = vec2.y;
		float y2 = v.y;
		float y3 = vec.y;
		float _2 = 0f;
		float z = vec2.z;
		float z2 = v.z;
		float z3 = vec.z;
		float _3 = 0f;
		float _4 = 0f - Vec3.DotProduct(vec2, position);
		float _5 = 0f - Vec3.DotProduct(v, position);
		float _6 = 0f - Vec3.DotProduct(vec, position);
		float _7 = 1f;
		return new MatrixFrame(x, x2, x3, _, y, y2, y3, _2, z, z2, z3, _3, _4, _5, _6, _7);
	}

	public static MatrixFrame CenterFrameOfTwoPoints(Vec3 p1, Vec3 p2, Vec3 upVector)
	{
		MatrixFrame result = default(MatrixFrame);
		result.origin = (p1 + p2) * 0.5f;
		result.rotation.s = p2 - p1;
		result.rotation.s.Normalize();
		if (MathF.Abs(Vec3.DotProduct(result.rotation.s, upVector)) > 0.95f)
		{
			upVector = new Vec3(0f, 1f);
		}
		result.rotation.u = upVector;
		result.rotation.f = Vec3.CrossProduct(result.rotation.u, result.rotation.s);
		result.rotation.f.Normalize();
		result.rotation.u = Vec3.CrossProduct(result.rotation.s, result.rotation.f);
		result.Fill();
		return result;
	}

	public void Fill()
	{
		rotation.s.w = 0f;
		rotation.f.w = 0f;
		rotation.u.w = 0f;
		origin.w = 1f;
	}

	private void AssertFilled()
	{
	}
}
