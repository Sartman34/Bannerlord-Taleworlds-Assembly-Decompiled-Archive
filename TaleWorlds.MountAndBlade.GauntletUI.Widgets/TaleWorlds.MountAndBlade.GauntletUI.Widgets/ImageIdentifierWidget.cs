using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class ImageIdentifierWidget : TextureWidget
{
	private string _imageId;

	private string _additionalArgs;

	private int _imageTypeCode;

	private bool _isBig;

	private bool _hideWhenNull;

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
				if (!string.IsNullOrEmpty(_imageId))
				{
					SetTextureProviderProperty("IsReleased", true);
				}
				_imageId = value;
				OnPropertyChanged(value, "ImageId");
				SetTextureProviderProperty("ImageId", value);
				SetTextureProviderProperty("IsReleased", false);
				RefreshVisibility();
				base.Texture = null;
				base.TextureProvider?.Clear(clearNextFrame: true);
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
				if (!string.IsNullOrEmpty(_additionalArgs))
				{
					SetTextureProviderProperty("IsReleased", true);
				}
				_additionalArgs = value;
				OnPropertyChanged(value, "AdditionalArgs");
				SetTextureProviderProperty("AdditionalArgs", value);
				SetTextureProviderProperty("IsReleased", false);
				RefreshVisibility();
				base.Texture = null;
				base.TextureProvider?.Clear(clearNextFrame: true);
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
				RefreshVisibility();
				base.Texture = null;
				base.TextureProvider?.Clear(clearNextFrame: true);
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
				RefreshVisibility();
				base.Texture = null;
				base.TextureProvider?.Clear(clearNextFrame: true);
			}
		}
	}

	[Editor(false)]
	public bool HideWhenNull
	{
		get
		{
			return _hideWhenNull;
		}
		set
		{
			if (_hideWhenNull != value)
			{
				_hideWhenNull = value;
				OnPropertyChanged(value, "HideWhenNull");
				RefreshVisibility();
				base.Texture = null;
				base.TextureProvider?.Clear(clearNextFrame: true);
			}
		}
	}

	public ImageIdentifierWidget(UIContext context)
		: base(context)
	{
		base.TextureProviderName = "ImageIdentifierTextureProvider";
		_calculateSizeFirstFrame = false;
	}

	private void RefreshVisibility()
	{
		if (HideWhenNull)
		{
			base.IsVisible = ImageTypeCode != 0;
		}
	}

	protected override void OnDisconnectedFromRoot()
	{
		SetTextureProviderProperty("IsReleased", true);
		base.OnDisconnectedFromRoot();
	}
}
