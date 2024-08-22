namespace TaleWorlds.TwoDimension;

public struct ScissorTestInfo
{
	public int X;

	public int Y;

	public int Width;

	public int Height;

	public ScissorTestInfo(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public static explicit operator Quad(ScissorTestInfo scissor)
	{
		Quad result = default(Quad);
		result.X = scissor.X;
		result.Y = scissor.Y;
		result.Width = scissor.Width;
		result.Height = scissor.Height;
		return result;
	}
}
