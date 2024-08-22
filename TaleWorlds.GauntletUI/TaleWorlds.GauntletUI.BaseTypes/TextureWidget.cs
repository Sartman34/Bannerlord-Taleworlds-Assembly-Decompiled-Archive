using System.Collections.Generic;
using System.Numerics;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.BaseTypes;

public class TextureWidget : ImageWidget
{
	protected static TypeCollector<TextureProvider> _typeCollector;

	protected bool _setForClearNextFrame;

	private string _textureProviderName;

	private Texture _texture;

	private float _lastWidth;

	private float _lastHeight;

	protected bool _isTargetSizeDirty;

	private Dictionary<string, object> _textureProviderProperties;

	protected bool _isRenderRequestedPreviousFrame;

	protected DrawObject2D _cachedQuad;

	protected Vector2 _cachedQuadSize;

	public Widget LoadingIconWidget { get; set; }

	public TextureProvider TextureProvider { get; private set; }

	[Editor(false)]
	public string TextureProviderName
	{
		get
		{
			return _textureProviderName;
		}
		set
		{
			if (_textureProviderName != value)
			{
				_textureProviderName = value;
				OnPropertyChanged(value, "TextureProviderName");
			}
		}
	}

	public Texture Texture
	{
		get
		{
			return _texture;
		}
		protected set
		{
			if (value != _texture)
			{
				_texture = value;
				OnTextureUpdated();
			}
		}
	}

	internal static void RecollectProviderTypes()
	{
		_typeCollector.Collect();
	}

	static TextureWidget()
	{
		_typeCollector = new TypeCollector<TextureProvider>();
		_typeCollector.Collect();
	}

	public TextureWidget(UIContext context)
		: base(context)
	{
		TextureProviderName = "ResourceTextureProvider";
		TextureProvider = null;
		_textureProviderProperties = new Dictionary<string, object>();
		_cachedQuad = null;
		_cachedQuadSize = Vector2.Zero;
	}

	protected override void OnDisconnectedFromRoot()
	{
		base.OnDisconnectedFromRoot();
		TextureProvider?.Clear(clearNextFrame: true);
		_setForClearNextFrame = true;
	}

	private void SetTextureProviderProperties()
	{
		if (TextureProvider == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> textureProviderProperty in _textureProviderProperties)
		{
			TextureProvider.SetProperty(textureProviderProperty.Key, textureProviderProperty.Value);
		}
	}

	protected void SetTextureProviderProperty(string name, object value)
	{
		_textureProviderProperties[name] = value;
		if (TextureProvider != null)
		{
			TextureProvider.SetProperty(name, value);
		}
		Texture = null;
	}

	protected object GetTextureProviderProperty(string propertyName)
	{
		return TextureProvider?.GetProperty(propertyName);
	}

	protected void UpdateTextureWidget()
	{
		if (!_isRenderRequestedPreviousFrame)
		{
			return;
		}
		if (TextureProvider != null)
		{
			if (_lastWidth != base.Size.X || _lastHeight != base.Size.Y || _isTargetSizeDirty)
			{
				int width = MathF.Round(base.Size.X);
				int height = MathF.Round(base.Size.Y);
				TextureProvider.SetTargetSize(width, height);
				_lastWidth = base.Size.X;
				_lastHeight = base.Size.Y;
				_isTargetSizeDirty = false;
			}
		}
		else
		{
			TextureProvider = _typeCollector.Instantiate(TextureProviderName);
			SetTextureProviderProperties();
		}
	}

	protected virtual void OnTextureUpdated()
	{
		bool toShow = Texture == null;
		if (LoadingIconWidget != null)
		{
			LoadingIconWidget.IsVisible = toShow;
			LoadingIconWidget.ApplyActionOnAllChildren(delegate(Widget w)
			{
				w.IsVisible = toShow;
			});
		}
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		UpdateTextureWidget();
		if (_isRenderRequestedPreviousFrame)
		{
			TextureProvider?.Tick(dt);
		}
		_isRenderRequestedPreviousFrame = false;
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		_isRenderRequestedPreviousFrame = true;
		if (TextureProvider != null)
		{
			Texture = TextureProvider.GetTexture(twoDimensionContext, string.Empty);
			SimpleMaterial simpleMaterial = drawContext.CreateSimpleMaterial();
			StyleLayer[] layers = base.ReadOnlyBrush.GetStyleOrDefault(base.CurrentState).GetLayers();
			simpleMaterial.OverlayEnabled = false;
			simpleMaterial.CircularMaskingEnabled = false;
			simpleMaterial.Texture = Texture;
			if (layers != null && layers.Length != 0)
			{
				StyleLayer styleLayer = layers[0];
				simpleMaterial.AlphaFactor = styleLayer.AlphaFactor * base.ReadOnlyBrush.GlobalAlphaFactor * base.Context.ContextAlpha;
				simpleMaterial.ColorFactor = styleLayer.ColorFactor * base.ReadOnlyBrush.GlobalColorFactor;
				simpleMaterial.HueFactor = styleLayer.HueFactor;
				simpleMaterial.SaturationFactor = styleLayer.SaturationFactor;
				simpleMaterial.ValueFactor = styleLayer.ValueFactor;
				simpleMaterial.Color = styleLayer.Color * base.ReadOnlyBrush.GlobalColor;
			}
			else
			{
				simpleMaterial.AlphaFactor = base.ReadOnlyBrush.GlobalAlphaFactor * base.Context.ContextAlpha;
				simpleMaterial.ColorFactor = base.ReadOnlyBrush.GlobalColorFactor;
				simpleMaterial.HueFactor = 0f;
				simpleMaterial.SaturationFactor = 0f;
				simpleMaterial.ValueFactor = 0f;
				simpleMaterial.Color = Color.White * base.ReadOnlyBrush.GlobalColor;
			}
			Vector2 globalPosition = base.GlobalPosition;
			float x = globalPosition.X;
			float y = globalPosition.Y;
			DrawObject2D drawObject2D = null;
			if (_cachedQuad != null && _cachedQuadSize == base.Size)
			{
				drawObject2D = _cachedQuad;
			}
			if (drawObject2D == null)
			{
				drawObject2D = (_cachedQuad = DrawObject2D.CreateQuad(base.Size));
				_cachedQuadSize = base.Size;
			}
			if (drawContext.CircularMaskEnabled)
			{
				simpleMaterial.CircularMaskingEnabled = true;
				simpleMaterial.CircularMaskingCenter = drawContext.CircularMaskCenter;
				simpleMaterial.CircularMaskingRadius = drawContext.CircularMaskRadius;
				simpleMaterial.CircularMaskingSmoothingRadius = drawContext.CircularMaskSmoothingRadius;
			}
			drawContext.Draw(x, y, simpleMaterial, drawObject2D, base.Size.X, base.Size.Y);
		}
	}
}
