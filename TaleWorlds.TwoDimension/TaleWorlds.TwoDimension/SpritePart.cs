using TaleWorlds.Library;

namespace TaleWorlds.TwoDimension;

public class SpritePart
{
	private SpriteCategory _category;

	public string Name { get; private set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	public int SheetID { get; set; }

	public int SheetX { get; set; }

	public int SheetY { get; set; }

	public float MinU { get; private set; }

	public float MinV { get; private set; }

	public float MaxU { get; private set; }

	public float MaxV { get; private set; }

	public int SheetWidth { get; private set; }

	public int SheetHeight { get; private set; }

	public Texture Texture
	{
		get
		{
			if (_category != null && _category.IsLoaded && _category.SpriteSheets != null && _category.SpriteSheets.Count >= SheetID)
			{
				return _category.SpriteSheets[SheetID - 1];
			}
			return null;
		}
	}

	public SpriteCategory Category => _category;

	public SpritePart(string name, SpriteCategory category, int width, int height)
	{
		Name = name;
		Width = width;
		Height = height;
		_category = category;
		_category.SpriteParts.Add(this);
	}

	public void UpdateInitValues()
	{
		Vec2i vec2i = _category.SheetSizes[SheetID - 1];
		SheetWidth = vec2i.X;
		SheetHeight = vec2i.Y;
		double num = 1.0 / (double)SheetWidth;
		double num2 = 1.0 / (double)SheetHeight;
		double num3 = (double)SheetX * num;
		double num4 = (double)(SheetX + Width) * num;
		double num5 = (double)SheetY * num2;
		double num6 = (double)(SheetY + Height) * num2;
		MinU = (float)num3;
		MaxU = (float)num4;
		MinV = (float)num5;
		MaxV = (float)num6;
	}

	public void DrawSpritePart(float screenX, float screenY, float[] outVertices, float[] outUvs, int verticesStartIndex, int uvsStartIndex)
	{
		DrawSpritePart(screenX, screenY, outVertices, outUvs, verticesStartIndex, uvsStartIndex, 1f, Width, Height, horizontalFlip: false, verticalFlip: false);
	}

	public void DrawSpritePart(float screenX, float screenY, float[] outVertices, float[] outUvs, int verticesStartIndex, int uvsStartIndex, float scale, float customWidth, float customHeight, bool horizontalFlip, bool verticalFlip)
	{
		if (Texture != null)
		{
			float num = customWidth / (float)Width;
			float num2 = customHeight / (float)Height;
			float num3 = (float)Width * scale * num;
			float num4 = (float)Height * scale * num2;
			outVertices[verticesStartIndex] = screenX + 0f;
			outVertices[verticesStartIndex + 1] = screenY + 0f;
			outVertices[verticesStartIndex + 2] = screenX + 0f;
			outVertices[verticesStartIndex + 3] = screenY + num4;
			outVertices[verticesStartIndex + 4] = screenX + num3;
			outVertices[verticesStartIndex + 5] = screenY + num4;
			outVertices[verticesStartIndex + 6] = screenX + num3;
			outVertices[verticesStartIndex + 7] = screenY + 0f;
			FillTextureCoordinates(outUvs, uvsStartIndex, horizontalFlip, verticalFlip);
		}
	}

	public void FillTextureCoordinates(float[] outUVs, int uvsStartIndex, bool horizontalFlip, bool verticalFlip)
	{
		float num = MinU;
		float num2 = MaxU;
		if (horizontalFlip)
		{
			num = MaxU;
			num2 = MinU;
		}
		float num3 = MinV;
		float num4 = MaxV;
		if (verticalFlip)
		{
			num3 = MaxV;
			num4 = MinV;
		}
		outUVs[uvsStartIndex] = num;
		outUVs[uvsStartIndex + 1] = num3;
		outUVs[uvsStartIndex + 2] = num;
		outUVs[uvsStartIndex + 3] = num4;
		outUVs[uvsStartIndex + 4] = num2;
		outUVs[uvsStartIndex + 5] = num4;
		outUVs[uvsStartIndex + 6] = num2;
		outUVs[uvsStartIndex + 7] = num3;
	}
}
