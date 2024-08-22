using System;

namespace TaleWorlds.TwoDimension;

public readonly struct SpriteDrawData : IEquatable<SpriteDrawData>
{
	internal readonly float MapX;

	internal readonly float MapY;

	internal readonly float Scale;

	internal readonly float Width;

	internal readonly float Height;

	internal readonly bool HorizontalFlip;

	internal readonly bool VerticalFlip;

	private readonly int _hashCode;

	internal SpriteDrawData(float mapX, float mapY, float scale, float width, float height, bool horizontalFlip, bool verticalFlip)
	{
		MapX = mapX;
		MapY = mapY;
		Scale = scale;
		Width = width;
		Height = height;
		HorizontalFlip = horizontalFlip;
		VerticalFlip = verticalFlip;
		int hashCode = MapX.GetHashCode();
		hashCode = (hashCode * 397) ^ MapY.GetHashCode();
		hashCode = (hashCode * 397) ^ Scale.GetHashCode();
		hashCode = (hashCode * 397) ^ Width.GetHashCode();
		hashCode = (hashCode * 397) ^ Height.GetHashCode();
		hashCode = (hashCode * 397) ^ HorizontalFlip.GetHashCode();
		hashCode = (hashCode * 397) ^ VerticalFlip.GetHashCode();
		_hashCode = hashCode;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		SpriteDrawData spriteDrawData = (SpriteDrawData)obj;
		if (MapX == spriteDrawData.MapX && MapY == spriteDrawData.MapY && Scale == spriteDrawData.Scale && Width == spriteDrawData.Width && Height == spriteDrawData.Height && HorizontalFlip == spriteDrawData.HorizontalFlip)
		{
			return VerticalFlip == spriteDrawData.VerticalFlip;
		}
		return false;
	}

	public bool Equals(SpriteDrawData other)
	{
		if (MapX == other.MapX && MapY == other.MapY && Scale == other.Scale && Width == other.Width && Height == other.Height && HorizontalFlip == other.HorizontalFlip)
		{
			return VerticalFlip == other.VerticalFlip;
		}
		return false;
	}

	public static bool operator ==(SpriteDrawData a, SpriteDrawData b)
	{
		if (a.MapX == b.MapX && a.MapY == b.MapY && a.Scale == b.Scale && a.Width == b.Width && a.Height == b.Height && a.HorizontalFlip == b.HorizontalFlip)
		{
			return a.VerticalFlip == b.VerticalFlip;
		}
		return false;
	}

	public static bool operator !=(SpriteDrawData a, SpriteDrawData b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}
}
