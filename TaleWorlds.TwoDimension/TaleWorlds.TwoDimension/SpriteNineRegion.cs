using System.Collections.Generic;

namespace TaleWorlds.TwoDimension;

public class SpriteNineRegion : Sprite
{
	private int _centerWidth;

	private int _centerHeight;

	private List<float[]> _outUvs;

	private float[] _outVertices;

	private uint[] _outIndices;

	private int _verticesStartIndex;

	private int _uvsStartIndex;

	private uint _indicesStartIndex;

	private float _scale;

	private float _customWidth;

	private float _customHeight;

	public override Texture Texture => BaseSprite.Texture;

	public SpritePart BaseSprite { get; private set; }

	public int LeftWidth { get; private set; }

	public int RightWidth { get; private set; }

	public int TopHeight { get; private set; }

	public int BottomHeight { get; private set; }

	public SpriteNineRegion(string name, SpritePart baseSprite, int leftWidth, int rightWidth, int topHeight, int bottomHeight)
		: base(name, baseSprite.Width, baseSprite.Height)
	{
		BaseSprite = baseSprite;
		LeftWidth = leftWidth;
		RightWidth = rightWidth;
		TopHeight = topHeight;
		BottomHeight = bottomHeight;
		_centerWidth = baseSprite.Width - leftWidth - rightWidth;
		_centerHeight = baseSprite.Height - topHeight - bottomHeight;
	}

	public override float GetScaleToUse(float width, float height, float scale)
	{
		float num = 1f;
		float num2 = (float)(LeftWidth + RightWidth) * scale;
		if (width < num2)
		{
			num = width / num2;
		}
		float num3 = (float)(TopHeight + BottomHeight) * scale;
		float num4 = 1f;
		if (height < num3)
		{
			num4 = height / num3;
		}
		float num5 = ((num < num4) ? num : num4);
		if (_centerWidth == 0)
		{
			num = width / num2;
			num5 = ((num < num5) ? num5 : num);
		}
		if (_centerHeight == 0)
		{
			num4 = height / num3;
			num5 = ((num4 < num5) ? num5 : num4);
		}
		return scale * num5;
	}

	protected internal override DrawObject2D GetArrays(SpriteDrawData spriteDrawData)
	{
		if (CachedDrawObject != null && CachedDrawData == spriteDrawData)
		{
			return CachedDrawObject;
		}
		_outVertices = new float[32];
		_outIndices = new uint[54];
		_verticesStartIndex = 0;
		_uvsStartIndex = 0;
		_indicesStartIndex = 0u;
		_scale = GetScaleToUse(spriteDrawData.Width, spriteDrawData.Height, spriteDrawData.Scale);
		_customWidth = spriteDrawData.Width;
		_customHeight = spriteDrawData.Height;
		SetVerticesData(spriteDrawData.HorizontalFlip, spriteDrawData.VerticalFlip);
		SetIndicesData();
		if (_outUvs == null)
		{
			_outUvs = new List<float[]>();
			_outUvs.Add(new float[32]);
			_outUvs.Add(new float[32]);
			_outUvs.Add(new float[32]);
			_outUvs.Add(new float[32]);
			CalculateTextureCoordinates(_outUvs[GetUVArrayIndex(horizontalFlip: false, verticalFlip: false)], horizontalFlip: false, verticalFlip: false);
			CalculateTextureCoordinates(_outUvs[GetUVArrayIndex(horizontalFlip: true, verticalFlip: false)], horizontalFlip: true, verticalFlip: false);
			CalculateTextureCoordinates(_outUvs[GetUVArrayIndex(horizontalFlip: false, verticalFlip: true)], horizontalFlip: false, verticalFlip: true);
			CalculateTextureCoordinates(_outUvs[GetUVArrayIndex(horizontalFlip: true, verticalFlip: true)], horizontalFlip: true, verticalFlip: true);
		}
		for (int i = 0; i < 16; i++)
		{
			_outVertices[2 * i] += spriteDrawData.MapX;
			_outVertices[2 * i + 1] += spriteDrawData.MapY;
		}
		DrawObject2D drawObject2D = new DrawObject2D(MeshTopology.Triangles, _outVertices, _outUvs[GetUVArrayIndex(spriteDrawData.HorizontalFlip, spriteDrawData.VerticalFlip)], _outIndices, _outIndices.Length);
		drawObject2D.DrawObjectType = DrawObjectType.NineGrid;
		_outVertices = null;
		_outIndices = null;
		CachedDrawObject = drawObject2D;
		CachedDrawData = spriteDrawData;
		return drawObject2D;
	}

	private int GetUVArrayIndex(bool horizontalFlip, bool verticalFlip)
	{
		int num = 0;
		if (horizontalFlip && verticalFlip)
		{
			return 3;
		}
		if (verticalFlip)
		{
			return 2;
		}
		if (horizontalFlip)
		{
			return 1;
		}
		return 0;
	}

	private void SetVertexData(float x, float y)
	{
		_outVertices[_verticesStartIndex] = x;
		_verticesStartIndex++;
		_outVertices[_verticesStartIndex] = y;
		_verticesStartIndex++;
	}

	private void SetTextureData(float[] outUvs, float u, float v)
	{
		outUvs[_uvsStartIndex] = u;
		_uvsStartIndex++;
		outUvs[_uvsStartIndex] = v;
		_uvsStartIndex++;
	}

	private void AddQuad(uint i1, uint i2, uint i3, uint i4)
	{
		_outIndices[_indicesStartIndex] = i1;
		_indicesStartIndex++;
		_outIndices[_indicesStartIndex] = i2;
		_indicesStartIndex++;
		_outIndices[_indicesStartIndex] = i4;
		_indicesStartIndex++;
		_outIndices[_indicesStartIndex] = i1;
		_indicesStartIndex++;
		_outIndices[_indicesStartIndex] = i4;
		_indicesStartIndex++;
		_outIndices[_indicesStartIndex] = i3;
		_indicesStartIndex++;
	}

	private void SetIndicesData()
	{
		AddQuad(0u, 1u, 4u, 5u);
		AddQuad(1u, 2u, 5u, 6u);
		AddQuad(2u, 3u, 6u, 7u);
		AddQuad(4u, 5u, 8u, 9u);
		AddQuad(5u, 6u, 9u, 10u);
		AddQuad(6u, 7u, 10u, 11u);
		AddQuad(8u, 9u, 12u, 13u);
		AddQuad(9u, 10u, 13u, 14u);
		AddQuad(10u, 11u, 14u, 15u);
	}

	private void SetVerticesData(bool horizontalFlip, bool verticalFlip)
	{
		float num = LeftWidth;
		float num2 = RightWidth;
		float num3 = TopHeight;
		float num4 = BottomHeight;
		if (horizontalFlip)
		{
			num = RightWidth;
			num2 = LeftWidth;
		}
		if (verticalFlip)
		{
			num3 = BottomHeight;
			num4 = TopHeight;
		}
		float y = 0f;
		float y2 = num3 * _scale;
		float y3 = _customHeight - num4 * _scale;
		float customHeight = _customHeight;
		float x = 0f;
		float x2 = num * _scale;
		float x3 = _customWidth - num2 * _scale;
		float customWidth = _customWidth;
		SetVertexData(x, y);
		SetVertexData(x2, y);
		SetVertexData(x3, y);
		SetVertexData(customWidth, y);
		SetVertexData(x, y2);
		SetVertexData(x2, y2);
		SetVertexData(x3, y2);
		SetVertexData(customWidth, y2);
		SetVertexData(x, y3);
		SetVertexData(x2, y3);
		SetVertexData(x3, y3);
		SetVertexData(customWidth, y3);
		SetVertexData(x, customHeight);
		SetVertexData(x2, customHeight);
		SetVertexData(x3, customHeight);
		SetVertexData(customWidth, customHeight);
	}

	private void CalculateTextureCoordinates(float[] outUvs, bool horizontalFlip, bool verticalFlip)
	{
		_uvsStartIndex = 0;
		float minU = BaseSprite.MinU;
		float minV = BaseSprite.MinV;
		float maxU = BaseSprite.MaxU;
		float maxV = BaseSprite.MaxV;
		float u = minU;
		float u2 = minU + (maxU - minU) * ((float)LeftWidth / (float)base.Width);
		float u3 = minU + (maxU - minU) * ((float)(LeftWidth + _centerWidth) / (float)base.Width);
		float u4 = maxU;
		if (horizontalFlip)
		{
			u4 = minU;
			u3 = minU + (maxU - minU) * ((float)LeftWidth / (float)base.Width);
			u2 = minU + (maxU - minU) * ((float)(LeftWidth + _centerWidth) / (float)base.Width);
			u = maxU;
		}
		float v = minV;
		float v2 = minV + (maxV - minV) * ((float)TopHeight / (float)base.Height);
		float v3 = minV + (maxV - minV) * ((float)(TopHeight + _centerHeight) / (float)base.Height);
		float v4 = maxV;
		if (verticalFlip)
		{
			v4 = minV;
			v3 = minV + (maxV - minV) * ((float)TopHeight / (float)base.Height);
			v2 = minV + (maxV - minV) * ((float)(TopHeight + _centerHeight) / (float)base.Height);
			v = maxV;
		}
		SetTextureData(outUvs, u, v);
		SetTextureData(outUvs, u2, v);
		SetTextureData(outUvs, u3, v);
		SetTextureData(outUvs, u4, v);
		SetTextureData(outUvs, u, v2);
		SetTextureData(outUvs, u2, v2);
		SetTextureData(outUvs, u3, v2);
		SetTextureData(outUvs, u4, v2);
		SetTextureData(outUvs, u, v3);
		SetTextureData(outUvs, u2, v3);
		SetTextureData(outUvs, u3, v3);
		SetTextureData(outUvs, u4, v3);
		SetTextureData(outUvs, u, v4);
		SetTextureData(outUvs, u2, v4);
		SetTextureData(outUvs, u3, v4);
		SetTextureData(outUvs, u4, v4);
	}
}
