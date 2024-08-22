using TaleWorlds.Library;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.Core;

public class ImageIdentifierVM : ViewModel
{
	private ImageIdentifier _imageIdentifierCode;

	[DataSourceProperty]
	public string Id
	{
		get
		{
			return _imageIdentifierCode.Id;
		}
		private set
		{
			if (_imageIdentifierCode.Id != value)
			{
				_imageIdentifierCode.Id = value;
				OnPropertyChangedWithValue(value, "Id");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEmpty
	{
		get
		{
			if (_imageIdentifierCode.ImageTypeCode != 0)
			{
				return string.IsNullOrEmpty(_imageIdentifierCode.Id);
			}
			return false;
		}
	}

	[DataSourceProperty]
	public bool IsValid => !IsEmpty;

	[DataSourceProperty]
	public string AdditionalArgs => _imageIdentifierCode.AdditionalArgs;

	[DataSourceProperty]
	public int ImageTypeCode => (int)_imageIdentifierCode.ImageTypeCode;

	public ImageIdentifierVM(ImageIdentifierType imageType = ImageIdentifierType.Null)
	{
		_imageIdentifierCode = new ImageIdentifier(imageType);
	}

	public ImageIdentifierVM(ItemObject itemObject, string bannerCode = "")
	{
		_imageIdentifierCode = new ImageIdentifier(itemObject, bannerCode);
	}

	public ImageIdentifierVM(CharacterCode characterCode)
	{
		_imageIdentifierCode = new ImageIdentifier(characterCode);
	}

	public ImageIdentifierVM(CraftingPiece craftingPiece, string pieceUsageID)
	{
		_imageIdentifierCode = new ImageIdentifier(craftingPiece, pieceUsageID);
	}

	public ImageIdentifierVM(BannerCode bannerCode, bool nineGrid = false)
	{
		_imageIdentifierCode = new ImageIdentifier(bannerCode, nineGrid);
	}

	public ImageIdentifierVM(Banner banner)
	{
		_imageIdentifierCode = new ImageIdentifier(banner);
	}

	public ImageIdentifierVM(ImageIdentifier code)
	{
		_imageIdentifierCode = new ImageIdentifier(code);
	}

	public ImageIdentifierVM(PlayerId id, int forcedAvatarIndex = -1)
	{
		_imageIdentifierCode = new ImageIdentifier(id, forcedAvatarIndex);
	}

	public ImageIdentifierVM(string id, ImageIdentifierType type)
	{
		_imageIdentifierCode = new ImageIdentifier(id, type);
	}

	public ImageIdentifierVM Clone()
	{
		return new ImageIdentifierVM(Id, (ImageIdentifierType)ImageTypeCode);
	}

	public bool Equals(ImageIdentifierVM target)
	{
		if (_imageIdentifierCode != null || target._imageIdentifierCode != null)
		{
			return _imageIdentifierCode?.Equals(target._imageIdentifierCode) ?? false;
		}
		return true;
	}
}
