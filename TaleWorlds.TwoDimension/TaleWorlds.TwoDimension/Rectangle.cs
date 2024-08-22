using System.Numerics;

namespace TaleWorlds.TwoDimension;

public struct Rectangle
{
	public float X;

	public float Y;

	public float X2;

	public float Y2;

	public float Width => X2 - X;

	public float Height => Y2 - Y;

	public Rectangle(float x, float y, float width, float height)
	{
		X = x;
		Y = y;
		X2 = x + width;
		Y2 = y + height;
	}

	public bool IsCollide(Rectangle other)
	{
		if (!(other.X > X2) && !(other.X2 < X) && !(other.Y > Y2))
		{
			return !(other.Y2 < Y);
		}
		return false;
	}

	public bool IsSubRectOf(Rectangle other)
	{
		if (other.X <= X && other.X2 >= X2 && other.Y <= Y)
		{
			return other.Y2 >= Y2;
		}
		return false;
	}

	public bool IsValid()
	{
		if (Width > 0f)
		{
			return Height > 0f;
		}
		return false;
	}

	public bool IsPointInside(Vector2 point)
	{
		if (point.X >= X && point.Y >= Y && point.X <= X2)
		{
			return point.Y <= Y2;
		}
		return false;
	}
}
