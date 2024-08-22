using System;
using System.Text;
using Steamworks;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.Diamond.AccessProvider.Steam;

public class SteamLoginAccessProvider : ILoginAccessProvider
{
	private string _steamUserName;

	private ulong _steamId;

	private PlatformInitParams _initParams;

	private uint _appId;

	private string _appTicket;

	private int AppId => Convert.ToInt32(_appId);

	void ILoginAccessProvider.Initialize(string preferredUserName, PlatformInitParams initParams)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (SteamAPI.Init() && Packsize.Test())
		{
			_appId = SteamUtils.GetAppID().m_AppId;
			_steamId = SteamUser.GetSteamID().m_SteamID;
			_steamUserName = SteamFriends.GetPersonaName();
			_initParams = initParams;
			byte[] array = new byte[1042];
			SteamAPICall_t val = SteamUser.RequestEncryptedAppTicket(Encoding.UTF8.GetBytes(""), array.Length);
			CallResult<EncryptedAppTicketResponse_t>.Create((APIDispatchDelegate<EncryptedAppTicketResponse_t>)EncryptedAppTicketResponseReceived).Set(val, (APIDispatchDelegate<EncryptedAppTicketResponse_t>)null);
			while (_appTicket == null)
			{
				SteamAPI.RunCallbacks();
			}
		}
	}

	string ILoginAccessProvider.GetUserName()
	{
		return _steamUserName;
	}

	PlayerId ILoginAccessProvider.GetPlayerId()
	{
		return new PlayerId(2, 0uL, _steamId);
	}

	AccessObjectResult ILoginAccessProvider.CreateAccessObject()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamAPI.IsSteamRunning())
		{
			return AccessObjectResult.CreateFailed(new TextObject("{=uunRVBPN}Steam is not running."));
		}
		byte[] array = new byte[1024];
		uint num = default(uint);
		if (SteamUser.GetAuthSessionTicket(array, 1024, ref num) == HAuthTicket.Invalid)
		{
			return AccessObjectResult.CreateFailed(new TextObject("{=XSU8Bbbb}Invalid Steam session."));
		}
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		string externalAccessToken = stringBuilder.ToString();
		return AccessObjectResult.CreateSuccess(new SteamAccessObject(_steamUserName, externalAccessToken, AppId, _appTicket));
	}

	private void EncryptedAppTicketResponseReceived(EncryptedAppTicketResponse_t response, bool bIOFailure)
	{
		byte[] array = new byte[2048];
		uint num = default(uint);
		SteamUser.GetEncryptedAppTicket(array, 2048, ref num);
		byte[] array2 = new byte[num];
		Array.Copy(array, array2, num);
		_appTicket = BitConverter.ToString(array2).Replace("-", "");
	}
}
