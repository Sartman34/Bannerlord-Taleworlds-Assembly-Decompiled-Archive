using System.Linq;

namespace TaleWorlds.TwoDimension;

public class SpriteGeneric : Sprite
{
	private float[] _vertices;

	private float[] _uvs;

	private uint[] _indices;

	public override Texture Texture => SpritePart.Texture;

	public SpritePart SpritePart { get; private set; }

	public SpriteGeneric(string name, SpritePart spritePart)
		: base(name, spritePart.Width, spritePart.Height)
	{
		SpritePart = spritePart;
		_vertices = new float[8];
		_uvs = new float[8];
		_indices = new uint[6];
		_indices[0] = 0u;
		_indices[1] = 1u;
		_indices[2] = 2u;
		_indices[3] = 0u;
		_indices[4] = 2u;
		_indices[5] = 3u;
	}

	public override float GetScaleToUse(float width, float height, float scale)
	{
		return scale;
	}

	protected internal override DrawObject2D GetArrays(SpriteDrawData spriteDrawData)
	{
		if (CachedDrawObject != null && CachedDrawData == spriteDrawData)
		{
			return CachedDrawObject;
		}
		if (spriteDrawData.MapX == 0f && spriteDrawData.MapY == 0f)
		{
			float num = spriteDrawData.Width / (float)SpritePart.Width;
			float num2 = spriteDrawData.Height / (float)SpritePart.Height;
			float width = (float)base.Width * 1f * num;
			float height = (float)base.Height * 1f * num2;
			SpritePart.DrawSpritePart(spriteDrawData.MapX, spriteDrawData.MapY, _vertices, _uvs, 0, 0, 1f, spriteDrawData.Width, spriteDrawData.Height, spriteDrawData.HorizontalFlip, spriteDrawData.VerticalFlip);
			DrawObject2D drawObject2D = new DrawObject2D(MeshTopology.Triangles, _vertices.ToArray(), _uvs.ToArray(), _indices.ToArray(), 6);
			drawObject2D.DrawObjectType = DrawObjectType.Quad;
			drawObject2D.Width = width;
			drawObject2D.Height = height;
			drawObject2D.MinU = SpritePart.MinU;
			drawObject2D.MaxU = SpritePart.MaxU;
			if (spriteDrawData.HorizontalFlip)
			{
				drawObject2D.MinU = SpritePart.MaxU;
				drawObject2D.MaxU = SpritePart.MinU;
			}
			drawObject2D.MinV = SpritePart.MinV;
			drawObject2D.MaxV = SpritePart.MaxV;
			if (spriteDrawData.VerticalFlip)
			{
				drawObject2D.MinV = SpritePart.MaxV;
				drawObject2D.MaxV = SpritePart.MinV;
			}
			CachedDrawData = spriteDrawData;
			CachedDrawObject = drawObject2D;
			return drawObject2D;
		}
		SpritePart.DrawSpritePart(spriteDrawData.MapX, spriteDrawData.MapY, _vertices, _uvs, 0, 0, 1f, spriteDrawData.Width, spriteDrawData.Height, spriteDrawData.HorizontalFlip, spriteDrawData.VerticalFlip);
		DrawObject2D drawObject2D2 = new DrawObject2D(MeshTopology.Triangles, _vertices.ToArray(), _uvs.ToArray(), _indices.ToArray(), 6);
		drawObject2D2.DrawObjectType = DrawObjectType.Mesh;
		CachedDrawData = spriteDrawData;
		CachedDrawObject = drawObject2D2;
		return drawObject2D2;
	}
}
