namespace TaleWorlds.TwoDimension;

public abstract class Sprite
{
	protected DrawObject2D CachedDrawObject;

	protected SpriteDrawData CachedDrawData;

	public abstract Texture Texture { get; }

	public string Name { get; private set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	protected Sprite(string name, int width, int height)
	{
		Name = name;
		Width = width;
		Height = height;
		CachedDrawObject = null;
	}

	public abstract float GetScaleToUse(float width, float height, float scale);

	protected internal abstract DrawObject2D GetArrays(SpriteDrawData spriteDrawData);

	public override string ToString()
	{
		if (string.IsNullOrEmpty(Name))
		{
			return base.ToString();
		}
		return Name;
	}
}
