using TaleWorlds.PlayerServices;

namespace TaleWorlds.Core;

public class ImageIdentifier
{
	public string Id;

	public ImageIdentifierType ImageTypeCode { get; private set; }

	public string AdditionalArgs { get; private set; }

	public ImageIdentifier(ImageIdentifierType imageType = ImageIdentifierType.Null)
	{
		ImageTypeCode = imageType;
		Id = "";
		AdditionalArgs = "";
	}

	public ImageIdentifier(ItemObject itemObject, string bannerCode = "")
	{
		ImageTypeCode = ImageIdentifierType.Item;
		Id = itemObject.StringId;
		AdditionalArgs = bannerCode;
	}

	public ImageIdentifier(CharacterCode characterCode)
	{
		ImageTypeCode = ImageIdentifierType.Character;
		Id = characterCode.Code;
		AdditionalArgs = "";
	}

	public ImageIdentifier(CraftingPiece craftingPiece, string pieceUsageId)
	{
		ImageTypeCode = ImageIdentifierType.CraftingPiece;
		Id = ((craftingPiece != null) ? (craftingPiece.StringId + "$" + pieceUsageId) : "");
		AdditionalArgs = "";
	}

	public ImageIdentifier(BannerCode bannerCode, bool nineGrid = false)
	{
		ImageTypeCode = (nineGrid ? ImageIdentifierType.BannerCodeNineGrid : ImageIdentifierType.BannerCode);
		Id = ((bannerCode != null) ? bannerCode.Code : "");
		AdditionalArgs = "";
	}

	public ImageIdentifier(PlayerId playerId, int forcedAvatarIndex)
	{
		ImageTypeCode = ImageIdentifierType.MultiplayerAvatar;
		Id = playerId.ToString();
		AdditionalArgs = $"{forcedAvatarIndex}";
	}

	public ImageIdentifier(Banner banner)
	{
		ImageTypeCode = ImageIdentifierType.BannerCode;
		AdditionalArgs = "";
		if (banner != null)
		{
			BannerCode bannerCode = BannerCode.CreateFrom(banner);
			Id = bannerCode.Code;
		}
		else
		{
			Id = "";
		}
	}

	public ImageIdentifier(ImageIdentifier code)
	{
		ImageTypeCode = code.ImageTypeCode;
		Id = code.Id;
		AdditionalArgs = code.AdditionalArgs;
	}

	public ImageIdentifier(string id, ImageIdentifierType type, string additionalArgs = "")
	{
		ImageTypeCode = type;
		Id = id;
		AdditionalArgs = additionalArgs;
	}

	public bool Equals(ImageIdentifier target)
	{
		if (target != null && ImageTypeCode == target.ImageTypeCode && Id.Equals(target.Id))
		{
			return AdditionalArgs.Equals(target.AdditionalArgs);
		}
		return false;
	}
}
