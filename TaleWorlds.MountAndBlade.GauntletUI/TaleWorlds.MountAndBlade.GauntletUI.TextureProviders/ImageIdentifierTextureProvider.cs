using System.Linq;
using TaleWorlds.Avatar.PlayerServices;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;
using TaleWorlds.PlayerServices.Avatar;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;

public class ImageIdentifierTextureProvider : TextureProvider
{
	private const float AvatarFailWaitTime = 5f;

	private ItemObject _itemObject;

	private CraftingPiece _craftingPiece;

	private BannerCode _bannerCode;

	private CharacterCode _characterCode;

	private TaleWorlds.Engine.Texture _texture;

	private TaleWorlds.TwoDimension.Texture _providedTexture;

	private string _imageId;

	private string _additionalArgs;

	private int _imageTypeCode;

	private bool _isBig;

	private bool _isReleased;

	private AvatarData _receivedAvatarData;

	private float _timeSinceAvatarFail;

	private bool _textureRequiresRefreshing;

	private bool _handleNewlyCreatedTexture;

	private bool _creatingTexture;

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
				_textureRequiresRefreshing = true;
			}
		}
	}

	public bool IsReleased
	{
		get
		{
			return _isReleased;
		}
		set
		{
			if (_isReleased != value)
			{
				_isReleased = value;
				if (_isReleased)
				{
					ReleaseCache();
				}
			}
		}
	}

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
				_textureRequiresRefreshing = true;
			}
		}
	}

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
				_textureRequiresRefreshing = true;
			}
		}
	}

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
				_textureRequiresRefreshing = true;
			}
		}
	}

	public ImageIdentifierTextureProvider()
	{
		_textureRequiresRefreshing = true;
		_receivedAvatarData = null;
		_timeSinceAvatarFail = 6f;
	}

	private void CheckTexture()
	{
		if (_textureRequiresRefreshing && (!_creatingTexture || ImageTypeCode == 4))
		{
			if (_receivedAvatarData != null)
			{
				if (_receivedAvatarData.Status == AvatarData.DataStatus.Ready)
				{
					AvatarData receivedAvatarData = _receivedAvatarData;
					_receivedAvatarData = null;
					OnAvatarLoaded(ImageId + "." + AdditionalArgs, receivedAvatarData);
				}
				else if (_receivedAvatarData.Status == AvatarData.DataStatus.Failed)
				{
					_receivedAvatarData = null;
					OnTextureCreated(null);
					_textureRequiresRefreshing = true;
					_timeSinceAvatarFail = 0f;
				}
			}
			else if (ImageTypeCode != 0)
			{
				if (_timeSinceAvatarFail > 5f)
				{
					CreateImageWithId(ImageId, ImageTypeCode, AdditionalArgs);
				}
			}
			else
			{
				OnTextureCreated(null);
			}
		}
		if (!_handleNewlyCreatedTexture)
		{
			return;
		}
		TaleWorlds.Engine.Texture texture = null;
		if (_providedTexture?.PlatformTexture is EngineTexture engineTexture)
		{
			texture = engineTexture.Texture;
		}
		if (_texture != texture)
		{
			if (_texture != null)
			{
				EngineTexture platformTexture = new EngineTexture(_texture);
				_providedTexture = new TaleWorlds.TwoDimension.Texture(platformTexture);
			}
			else
			{
				_providedTexture = null;
			}
		}
		_handleNewlyCreatedTexture = false;
	}

	public override TaleWorlds.TwoDimension.Texture GetTexture(TwoDimensionContext twoDimensionContext, string name)
	{
		CheckTexture();
		return _providedTexture;
	}

	public override void Tick(float dt)
	{
		base.Tick(dt);
		_timeSinceAvatarFail += dt;
		CheckTexture();
	}

	public void CreateImageWithId(string id, int typeAsInt, string additionalArgs)
	{
		if (string.IsNullOrEmpty(id))
		{
			if (typeAsInt == 5)
			{
				CharacterCode characterCode = _characterCode;
				if (characterCode == null || characterCode.IsEmpty)
				{
					OnTextureCreated(TableauCacheManager.Current.GetCachedHeroSilhouetteTexture());
					return;
				}
			}
			OnTextureCreated(null);
			return;
		}
		switch (typeAsInt)
		{
		case 1:
			_itemObject = MBObjectManager.Instance.GetObject<ItemObject>(id);
			Debug.Print("Render Requested: " + id);
			if (_itemObject == null)
			{
				Debug.FailedAssert("WRONG Item IMAGE IDENTIFIER ID", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\TextureProviders\\ImageIdentifierTextureProvider.cs", "CreateImageWithId", 214);
				OnTextureCreated(null);
			}
			else
			{
				_creatingTexture = true;
				TableauCacheManager.Current.BeginCreateItemTexture(_itemObject, additionalArgs, OnTextureCreated);
			}
			break;
		case 2:
			_craftingPiece = MBObjectManager.Instance.GetObject<CraftingPiece>(id.Split(new char[1] { '$' })[0]);
			if (_craftingPiece == null)
			{
				Debug.FailedAssert("WRONG CraftingPiece IMAGE IDENTIFIER ID", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\TextureProviders\\ImageIdentifierTextureProvider.cs", "CreateImageWithId", 225);
				OnTextureCreated(null);
			}
			else
			{
				_creatingTexture = true;
				TableauCacheManager.Current.BeginCreateCraftingPieceTexture(_craftingPiece, id.Split(new char[1] { '$' })[1], OnTextureCreated);
			}
			break;
		case 3:
			_bannerCode = BannerCode.CreateFrom(id);
			_creatingTexture = true;
			TableauCacheManager.Current.BeginCreateBannerTexture(_bannerCode, OnTextureCreated);
			break;
		case 6:
			_bannerCode = BannerCode.CreateFrom(id);
			_creatingTexture = true;
			TableauCacheManager.Current.BeginCreateBannerTexture(_bannerCode, OnTextureCreated, isTableauOrNineGrid: true, IsBig);
			break;
		case 4:
		{
			_creatingTexture = true;
			PlayerId playerId = PlayerId.FromString(id);
			int num = -1;
			AvatarData avatarData = AvatarServices.GetPlayerAvatar(forcedIndex: additionalArgs.IsEmpty() ? (GameNetwork.NetworkPeers.FirstOrDefault((NetworkCommunicator np) => np.VirtualPlayer.Id == playerId)?.ForcedAvatarIndex ?? (-1)) : int.Parse(additionalArgs), playerId: playerId);
			if (avatarData != null)
			{
				_receivedAvatarData = avatarData;
			}
			else
			{
				_timeSinceAvatarFail = 0f;
			}
			break;
		}
		case 5:
			_characterCode = CharacterCode.CreateFrom(id);
			if (TaleWorlds.Core.FaceGen.GetMaturityTypeWithAge(_characterCode.BodyProperties.Age) <= BodyMeshMaturityType.Child)
			{
				OnTextureCreated(null);
				break;
			}
			_creatingTexture = true;
			TableauCacheManager.Current.BeginCreateCharacterTexture(_characterCode, OnTextureCreated, IsBig);
			break;
		case 0:
			OnTextureCreated(null);
			break;
		default:
			Debug.FailedAssert("WRONG IMAGE IDENTIFIER ID", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\TextureProviders\\ImageIdentifierTextureProvider.cs", "CreateImageWithId", 284);
			break;
		}
	}

	private void OnAvatarLoaded(string avatarID, AvatarData avatarData)
	{
		if (avatarData != null)
		{
			_creatingTexture = true;
			TaleWorlds.Engine.Texture texture = TableauCacheManager.Current.CreateAvatarTexture(avatarID, avatarData.Image, avatarData.Width, avatarData.Height, avatarData.Type);
			OnTextureCreated(texture);
		}
	}

	public override void Clear(bool clearNextFrame)
	{
		base.Clear(clearNextFrame);
		_providedTexture = null;
		_textureRequiresRefreshing = true;
		_itemObject = null;
		_craftingPiece = null;
		_bannerCode = null;
		_characterCode = null;
		_creatingTexture = false;
	}

	public void ReleaseCache()
	{
		switch ((ImageIdentifierType)ImageTypeCode)
		{
		case ImageIdentifierType.Item:
			if (_itemObject != null)
			{
				TableauCacheManager.Current.ReleaseTextureWithId(_itemObject);
			}
			break;
		case ImageIdentifierType.CraftingPiece:
			if (_craftingPiece != null)
			{
				TableauCacheManager.Current.ReleaseTextureWithId(_craftingPiece, ImageId.Split(new char[1] { '$' })[1]);
			}
			break;
		case ImageIdentifierType.BannerCode:
			if (_bannerCode != null)
			{
				TableauCacheManager.Current.ReleaseTextureWithId(_bannerCode);
			}
			break;
		case ImageIdentifierType.Character:
			if (_characterCode != null && TaleWorlds.Core.FaceGen.GetMaturityTypeWithAge(_characterCode.BodyProperties.Age) > BodyMeshMaturityType.Child)
			{
				TableauCacheManager.Current.ReleaseTextureWithId(_characterCode, IsBig);
			}
			break;
		case ImageIdentifierType.BannerCodeNineGrid:
			TableauCacheManager.Current.ReleaseTextureWithId(_bannerCode, isTableau: true, IsBig);
			break;
		case ImageIdentifierType.Null:
		case ImageIdentifierType.MultiplayerAvatar:
			break;
		}
	}

	private void OnTextureCreated(TaleWorlds.Engine.Texture texture)
	{
		_texture = texture;
		_textureRequiresRefreshing = false;
		_handleNewlyCreatedTexture = true;
		_creatingTexture = false;
	}
}
