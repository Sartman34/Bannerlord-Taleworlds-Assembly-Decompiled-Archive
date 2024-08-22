using System;
using System.Collections.Generic;
using System.Numerics;

namespace TaleWorlds.TwoDimension;

public class TwoDimensionDrawContext
{
	public struct SpriteCacheKey : IEquatable<SpriteCacheKey>
	{
		public Sprite Sprite;

		public SpriteDrawData DrawData;

		public SpriteCacheKey(Sprite sprite, SpriteDrawData drawData)
		{
			Sprite = sprite;
			DrawData = drawData;
		}

		public bool Equals(SpriteCacheKey other)
		{
			if (other.Sprite == Sprite)
			{
				return other.DrawData == DrawData;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Sprite.GetHashCode() * 397) ^ DrawData.GetHashCode();
		}
	}

	private List<ScissorTestInfo> _scissorStack;

	private bool _scissorTestEnabled;

	private bool _circularMaskEnabled;

	private float _circularMaskRadius;

	private float _circularMaskSmoothingRadius;

	private Vector2 _circularMaskCenter;

	private List<TwoDimensionDrawLayer> _layers;

	private int _usedLayersCount;

	private int _totalDrawCount;

	private Dictionary<SpriteCacheKey, DrawObject2D> _cachedDrawObjects;

	private MaterialPool<SimpleMaterial> _simpleMaterialPool = new MaterialPool<SimpleMaterial>(8);

	private MaterialPool<TextMaterial> _textMaterialPool = new MaterialPool<TextMaterial>(8);

	public bool ScissorTestEnabled => _scissorTestEnabled;

	public bool CircularMaskEnabled => _circularMaskEnabled;

	public Vector2 CircularMaskCenter => _circularMaskCenter;

	public float CircularMaskRadius => _circularMaskRadius;

	public float CircularMaskSmoothingRadius => _circularMaskSmoothingRadius;

	public ScissorTestInfo CurrentScissor => _scissorStack[_scissorStack.Count - 1];

	public TwoDimensionDrawContext()
	{
		_scissorStack = new List<ScissorTestInfo>();
		_scissorTestEnabled = false;
		_layers = new List<TwoDimensionDrawLayer>();
		_cachedDrawObjects = new Dictionary<SpriteCacheKey, DrawObject2D>();
	}

	public void Reset()
	{
		_scissorStack.Clear();
		_scissorTestEnabled = false;
		for (int i = 0; i < _usedLayersCount; i++)
		{
			_layers[i].Reset();
		}
		_usedLayersCount = 0;
		_simpleMaterialPool.ResetAll();
		_textMaterialPool.ResetAll();
	}

	public SimpleMaterial CreateSimpleMaterial()
	{
		return _simpleMaterialPool.New();
	}

	public TextMaterial CreateTextMaterial()
	{
		return _textMaterialPool.New();
	}

	public void PushScissor(int x, int y, int width, int height)
	{
		ScissorTestInfo item = new ScissorTestInfo(x, y, width, height);
		if (_scissorStack.Count > 0)
		{
			ScissorTestInfo scissorTestInfo = _scissorStack[_scissorStack.Count - 1];
			if (width != -1)
			{
				int num = scissorTestInfo.X + scissorTestInfo.Width;
				int num2 = x + width;
				item.X = ((item.X > scissorTestInfo.X) ? item.X : scissorTestInfo.X);
				int num3 = ((num > num2) ? num2 : num);
				item.Width = num3 - item.X;
			}
			else
			{
				item.X = scissorTestInfo.X;
				item.Width = scissorTestInfo.Width;
			}
			if (height != -1)
			{
				int num4 = scissorTestInfo.Y + scissorTestInfo.Height;
				int num5 = y + height;
				item.Y = ((item.Y > scissorTestInfo.Y) ? item.Y : scissorTestInfo.Y);
				int num6 = ((num4 > num5) ? num5 : num4);
				item.Height = num6 - item.Y;
			}
			else
			{
				item.Y = scissorTestInfo.Y;
				item.Height = scissorTestInfo.Height;
			}
		}
		else
		{
			if (width == -1)
			{
				item.Width = int.MaxValue;
			}
			if (height == -1)
			{
				item.Height = int.MaxValue;
			}
		}
		_scissorStack.Add(item);
		_scissorTestEnabled = true;
	}

	public void PopScissor()
	{
		_scissorStack.RemoveAt(_scissorStack.Count - 1);
		if (_scissorTestEnabled && _scissorStack.Count == 0)
		{
			_scissorTestEnabled = false;
		}
	}

	public void SetCircualMask(Vector2 position, float radius, float smoothingRadius)
	{
		_circularMaskEnabled = true;
		_circularMaskCenter = position;
		_circularMaskRadius = radius;
		_circularMaskSmoothingRadius = smoothingRadius;
	}

	public void ClearCircualMask()
	{
		_circularMaskEnabled = false;
	}

	public void DrawTo(TwoDimensionContext twoDimensionContext)
	{
		for (int i = 0; i < _usedLayersCount; i++)
		{
			_layers[i].DrawTo(twoDimensionContext, i + 1);
		}
	}

	public void DrawSprite(Sprite sprite, SimpleMaterial material, float x, float y, float scale, float width, float height, bool horizontalFlip, bool verticalFlip)
	{
		SpriteDrawData spriteDrawData = new SpriteDrawData(0f, 0f, scale, width, height, horizontalFlip, verticalFlip);
		DrawObject2D value = null;
		SpriteCacheKey key = new SpriteCacheKey(sprite, spriteDrawData);
		if (!_cachedDrawObjects.TryGetValue(key, out value))
		{
			value = sprite.GetArrays(spriteDrawData);
			_cachedDrawObjects.Add(key, value);
		}
		material.Texture = sprite.Texture;
		if (_circularMaskEnabled)
		{
			material.CircularMaskingEnabled = true;
			material.CircularMaskingCenter = _circularMaskCenter;
			material.CircularMaskingRadius = _circularMaskRadius;
			material.CircularMaskingSmoothingRadius = _circularMaskSmoothingRadius;
		}
		Draw(x, y, material, value, width, height);
	}

	public void Draw(float x, float y, Material material, DrawObject2D drawObject2D, float width, float height)
	{
		TwoDimensionDrawData drawData = new TwoDimensionDrawData(_scissorTestEnabled, _scissorTestEnabled ? CurrentScissor : default(ScissorTestInfo), x, y, material, drawObject2D, width, height);
		TwoDimensionDrawLayer twoDimensionDrawLayer = null;
		if (_layers.Count == 0)
		{
			twoDimensionDrawLayer = new TwoDimensionDrawLayer();
			_layers.Add(twoDimensionDrawLayer);
			_usedLayersCount++;
		}
		else
		{
			for (int num = _usedLayersCount - 1; num >= 0; num--)
			{
				TwoDimensionDrawLayer twoDimensionDrawLayer2 = _layers[num];
				if (twoDimensionDrawLayer2.IsIntersects(drawData.Rectangle))
				{
					break;
				}
				twoDimensionDrawLayer = twoDimensionDrawLayer2;
			}
			if (twoDimensionDrawLayer == null)
			{
				if (_usedLayersCount == _layers.Count)
				{
					twoDimensionDrawLayer = new TwoDimensionDrawLayer();
					_layers.Add(twoDimensionDrawLayer);
				}
				else
				{
					twoDimensionDrawLayer = _layers[_usedLayersCount];
				}
				_usedLayersCount++;
			}
		}
		_totalDrawCount++;
		twoDimensionDrawLayer.AddDrawData(drawData);
	}

	public void Draw(Text text, TextMaterial material, float x, float y, float width, float height)
	{
		text.UpdateSize((int)width, (int)height);
		DrawObject2D drawObject2D = text.DrawObject2D;
		if (drawObject2D != null)
		{
			material.Texture = text.Font.FontSprite.Texture;
			material.ScaleFactor = text.FontSize;
			material.SmoothingConstant = text.Font.SmoothingConstant;
			material.Smooth = text.Font.Smooth;
			DrawObject2D drawObject2D2 = drawObject2D;
			if (material.GlowRadius > 0f || material.Blur > 0f || material.OutlineAmount > 0f)
			{
				TextMaterial textMaterial = CreateTextMaterial();
				textMaterial.CopyFrom(material);
				Draw(x, y, textMaterial, drawObject2D2, (int)width, (int)height);
			}
			material.GlowRadius = 0f;
			material.Blur = 0f;
			material.OutlineAmount = 0f;
			Draw(x, y, material, drawObject2D2, (int)width, (int)height);
		}
	}
}
