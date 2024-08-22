using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.Diamond.AccessProvider.Epic;

public class EpicLoginAccessProvider : ILoginAccessProvider
{
	private string _epicUserName;

	private PlatformInitParams _initParams;

	private PlatformInterface _platform;

	private EpicAccountId _epicAccountId;

	private string _accessToken;

	private TextObject _initFailReason;

	private string ExchangeCode => (string)_initParams["ExchangeCode"];

	private string EpicId
	{
		get
		{
			if ((Handle)(object)_epicAccountId == (Handle)null)
			{
				return null;
			}
			return ((object)_epicAccountId).ToString();
		}
	}

	public EpicLoginAccessProvider(PlatformInterface platform, EpicAccountId epicAccountId, string epicUserName, string accessToken, TextObject initFailReason)
	{
		_platform = platform;
		_epicAccountId = epicAccountId;
		_accessToken = accessToken;
		_epicUserName = epicUserName;
		_initFailReason = initFailReason;
	}

	void ILoginAccessProvider.Initialize(string preferredUserName, PlatformInitParams initParams)
	{
		_initParams = initParams;
	}

	string ILoginAccessProvider.GetUserName()
	{
		return _epicUserName;
	}

	PlayerId ILoginAccessProvider.GetPlayerId()
	{
		if (EpicId == null)
		{
			return PlayerId.Empty;
		}
		return new PlayerId(3, EpicId);
	}

	AccessObjectResult ILoginAccessProvider.CreateAccessObject()
	{
		if (_initFailReason != null)
		{
			return AccessObjectResult.CreateFailed(_initFailReason);
		}
		return AccessObjectResult.CreateSuccess(new EpicAccessObject
		{
			EpicId = EpicId,
			AccessToken = _accessToken
		});
	}
}
