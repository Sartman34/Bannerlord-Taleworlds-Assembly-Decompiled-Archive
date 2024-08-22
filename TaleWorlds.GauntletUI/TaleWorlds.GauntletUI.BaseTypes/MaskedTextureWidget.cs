using System.Numerics;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.BaseTypes;

public class MaskedTextureWidget : TextureWidget
{
	private Texture _textureCache;

	private string _imageId;

	private string _additionalArgs;

	private int _imageTypeCode;

	private bool _isBig;

	private SpriteFromTexture _overlaySpriteCache;

	private int _overlaySpriteSizeCache;

	[Editor(false)]
	public string ImageId
	{
		get
		{
			return _imageId;
		}
		set
		{
			if (_imageId != value)
			{
				_imageId = value;
				OnPropertyChanged(value, "ImageId");
				SetTextureProviderProperty("ImageId", value);
			}
		}
	}

	[Editor(false)]
	public string AdditionalArgs
	{
		get
		{
			return _additionalArgs;
		}
		set
		{
			if (_additionalArgs != value)
			{
				_additionalArgs = value;
				OnPropertyChanged(value, "AdditionalArgs");
				SetTextureProviderProperty("AdditionalArgs", value);
			}
		}
	}

	[Editor(false)]
	public int ImageTypeCode
	{
		get
		{
			return _imageTypeCode;
		}
		set
		{
			if (_imageTypeCode != value)
			{
				_imageTypeCode = value;
				OnPropertyChanged(value, "ImageTypeCode");
				SetTextureProviderProperty("ImageTypeCode", value);
			}
		}
	}

	[Editor(false)]
	public bool IsBig
	{
		get
		{
			return _isBig;
		}
		set
		{
			if (_isBig != value)
			{
				_isBig = value;
				OnPropertyChanged(value, "IsBig");
				SetTextureProviderProperty("IsBig", value);
			}
		}
	}

	[Editor(false)]
	public float OverlayTextureScale { get; set; }

	public MaskedTextureWidget(UIContext context)
		: base(context)
	{
		base.TextureProviderName = "ImageIdentifierTextureProvider";
		OverlayTextureScale = 1f;
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		_isRenderRequestedPreviousFrame = true;
		if (base.TextureProvider == null)
		{
			return;
		}
		Texture texture = base.TextureProvider.GetTexture(twoDimensionContext, "ui_backgrounds_1");
		bool flag = false;
		if (texture != _textureCache)
		{
			base.Brush.DefaultLayer.OverlayMethod = BrushOverlayMethod.CoverWithTexture;
			_textureCache = texture;
			flag = true;
			HandleUpdateNeededOnRender();
		}
		if (_textureCache == null)
		{
			return;
		}
		int num;
		int num2;
		if (ImageTypeCode != 3)
		{
			num = ((ImageTypeCode == 6) ? 1 : 0);
			if (num == 0)
			{
				num2 = (int)(((base.Size.X > base.Size.Y) ? base.Size.X : base.Size.Y) * OverlayTextureScale);
				goto IL_00ea;
			}
		}
		else
		{
			num = 1;
		}
		num2 = (int)(((base.Size.X > base.Size.Y) ? base.Size.Y : base.Size.X) * 2.5f * OverlayTextureScale);
		goto IL_00ea;
		IL_00ea:
		int num3 = num2;
		Vector2 overlayOffset = default(Vector2);
		if (num != 0)
		{
			float x = ((float)num3 - base.Size.X) * 0.5f - base.Brush.DefaultLayer.OverlayXOffset;
			float y = ((float)num3 - base.Size.Y) * 0.5f - base.Brush.DefaultLayer.OverlayYOffset;
			overlayOffset = new Vector2(x, y) * base._inverseScaleToUse;
		}
		if (_overlaySpriteCache == null || flag || _overlaySpriteSizeCache != num3)
		{
			_overlaySpriteSizeCache = num3;
			_overlaySpriteCache = new SpriteFromTexture(_textureCache, _overlaySpriteSizeCache, _overlaySpriteSizeCache);
		}
		base.Brush.DefaultLayer.OverlaySprite = _overlaySpriteCache;
		base.BrushRenderer.Render(drawContext, base.GlobalPosition, base.Size, base._scaleToUse, base.Context.ContextAlpha, overlayOffset);
	}

	protected override void OnDisconnectedFromRoot()
	{
		base.OnDisconnectedFromRoot();
		_textureCache = null;
	}
}
