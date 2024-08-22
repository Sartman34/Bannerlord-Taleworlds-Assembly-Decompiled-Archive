using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TaleWorlds.Library;

namespace TaleWorlds.TwoDimension;

public class TwoDimensionContext
{
	private List<ScissorTestInfo> _scissorStack;

	private bool _scissorTestEnabled;

	private bool _circularMaskEnabled;

	private float _circularMaskRadius;

	private float _circularMaskSmoothingRadius;

	private Vector2 _circularMaskCenter;

	public float Width => Platform.Width;

	public float Height => Platform.Height;

	public ITwoDimensionPlatform Platform { get; private set; }

	public ITwoDimensionResourceContext ResourceContext { get; private set; }

	public ResourceDepot ResourceDepot { get; private set; }

	public bool ScissorTestEnabled => _scissorTestEnabled;

	public bool CircularMaskEnabled => _circularMaskEnabled;

	public Vector2 CircularMaskCenter => _circularMaskCenter;

	public float CircularMaskRadius => _circularMaskRadius;

	public float CircularMaskSmoothingRadius => _circularMaskSmoothingRadius;

	public ScissorTestInfo CurrentScissor => _scissorStack[_scissorStack.Count - 1];

	public bool IsDebugModeEnabled => Platform.IsDebugModeEnabled();

	public TwoDimensionContext(ITwoDimensionPlatform platform, ITwoDimensionResourceContext resourceContext, ResourceDepot resourceDepot)
	{
		ResourceDepot = resourceDepot;
		_scissorStack = new List<ScissorTestInfo>();
		_scissorTestEnabled = false;
		Platform = platform;
		ResourceContext = resourceContext;
	}

	public void PlaySound(string soundName)
	{
		Platform.PlaySound(soundName);
	}

	public void CreateSoundEvent(string soundName)
	{
		Platform.CreateSoundEvent(soundName);
	}

	public void StopAndRemoveSoundEvent(string soundName)
	{
		Platform.StopAndRemoveSoundEvent(soundName);
	}

	public void PlaySoundEvent(string soundName)
	{
		Platform.PlaySoundEvent(soundName);
	}

	public void Draw(float x, float y, Material material, DrawObject2D drawObject2D, int layer = 0)
	{
		Platform.Draw(x, y, material, drawObject2D, layer);
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
			DrawObject2D drawObject2D2 = new DrawObject2D(drawObject2D.Topology, drawObject2D.Vertices.ToArray(), drawObject2D.TextureCoordinates, drawObject2D.Indices, drawObject2D.VertexCount);
			Draw(x, y, material, drawObject2D2);
		}
	}

	public void BeginDebugPanel(string panelTitle)
	{
		Platform.BeginDebugPanel(panelTitle);
	}

	public void EndDebugPanel()
	{
		Platform.EndDebugPanel();
	}

	public void DrawDebugText(string text)
	{
		Platform.DrawDebugText(text);
	}

	public bool DrawDebugTreeNode(string text)
	{
		return Platform.DrawDebugTreeNode(text);
	}

	public void PopDebugTreeNode()
	{
		Platform.PopDebugTreeNode();
	}

	public void DrawCheckbox(string label, ref bool isChecked)
	{
		Platform.DrawCheckbox(label, ref isChecked);
	}

	public bool IsDebugItemHovered()
	{
		return Platform.IsDebugItemHovered();
	}

	public Texture LoadTexture(string name)
	{
		return ResourceContext.LoadTexture(ResourceDepot, name);
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

	public void DrawSprite(Sprite sprite, SimpleMaterial material, float x, float y, float scale, float width, float height, bool horizontalFlip, bool verticalFlip)
	{
		DrawObject2D arrays = sprite.GetArrays(new SpriteDrawData(0f, 0f, scale, width, height, horizontalFlip, verticalFlip));
		material.Texture = sprite.Texture;
		if (_circularMaskEnabled)
		{
			material.CircularMaskingEnabled = true;
			material.CircularMaskingCenter = _circularMaskCenter;
			material.CircularMaskingRadius = _circularMaskRadius;
			material.CircularMaskingSmoothingRadius = _circularMaskSmoothingRadius;
		}
		Draw(x, y, material, arrays);
	}

	public void SetScissor(int x, int y, int width, int height)
	{
		ScissorTestInfo scissor = new ScissorTestInfo(x, y, width, height);
		SetScissor(scissor);
	}

	public void SetScissor(ScissorTestInfo scissor)
	{
		Platform.SetScissor(scissor);
	}

	public void ResetScissor()
	{
		Platform.ResetScissor();
	}

	public void PushScissor(int x, int y, int width, int height)
	{
		ScissorTestInfo scissorTestInfo = new ScissorTestInfo(x, y, width, height);
		if (_scissorStack.Count > 0)
		{
			ScissorTestInfo scissorTestInfo2 = _scissorStack[_scissorStack.Count - 1];
			int num = scissorTestInfo2.X + scissorTestInfo2.Width;
			int num2 = scissorTestInfo2.Y + scissorTestInfo2.Height;
			int num3 = x + width;
			int num4 = y + height;
			scissorTestInfo.X = ((scissorTestInfo.X > scissorTestInfo2.X) ? scissorTestInfo.X : scissorTestInfo2.X);
			scissorTestInfo.Y = ((scissorTestInfo.Y > scissorTestInfo2.Y) ? scissorTestInfo.Y : scissorTestInfo2.Y);
			int num5 = ((num > num3) ? num3 : num);
			int num6 = ((num2 > num4) ? num4 : num2);
			scissorTestInfo.Width = num5 - scissorTestInfo.X;
			scissorTestInfo.Height = num6 - scissorTestInfo.Y;
		}
		_scissorStack.Add(scissorTestInfo);
		_scissorTestEnabled = true;
		Platform.SetScissor(scissorTestInfo);
	}

	public void PopScissor()
	{
		_scissorStack.RemoveAt(_scissorStack.Count - 1);
		if (_scissorTestEnabled)
		{
			if (_scissorStack.Count > 0)
			{
				ScissorTestInfo scissor = _scissorStack[_scissorStack.Count - 1];
				Platform.SetScissor(scissor);
			}
			else
			{
				Platform.ResetScissor();
				_scissorTestEnabled = false;
			}
		}
	}
}
