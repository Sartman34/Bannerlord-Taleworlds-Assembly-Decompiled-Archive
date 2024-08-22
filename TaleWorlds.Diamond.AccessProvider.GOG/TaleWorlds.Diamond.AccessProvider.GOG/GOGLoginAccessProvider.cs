using System;
using System.Text;
using System.Threading;
using Galaxy.Api;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.Diamond.AccessProvider.GOG;

public class GOGLoginAccessProvider : ILoginAccessProvider
{
	private string _gogUserName;

	private ulong _gogId;

	private ulong _oldId;

	private PlatformInitParams _initParams;

	void ILoginAccessProvider.Initialize(string preferredUserName, PlatformInitParams initParams)
	{
		_initParams = initParams;
		if (GalaxyInstance.User().IsLoggedOn())
		{
			IUser val = GalaxyInstance.User();
			IFriends val2 = GalaxyInstance.Friends();
			_gogId = val.GetGalaxyID().GetRealID();
			_oldId = val.GetGalaxyID().ToUint64();
			_gogUserName = val2.GetPersonaName();
		}
	}

	string ILoginAccessProvider.GetUserName()
	{
		return _gogUserName;
	}

	PlayerId ILoginAccessProvider.GetPlayerId()
	{
		return new PlayerId(8, 0uL, _gogId);
	}

	AccessObjectResult ILoginAccessProvider.CreateAccessObject()
	{
		if (!GalaxyInstance.User().IsLoggedOn())
		{
			return AccessObjectResult.CreateFailed(new TextObject("{=hU361b7v}Not logged in on GOG."));
		}
		IUser val = GalaxyInstance.User();
		if (val.IsLoggedOn())
		{
			EncryptedAppTicketListener encryptedAppTicketListener = new EncryptedAppTicketListener();
			val.RequestEncryptedAppTicket((byte[])null, 0u, (IEncryptedAppTicketListener)(object)encryptedAppTicketListener);
			while (!encryptedAppTicketListener.GotResult)
			{
				GalaxyInstance.ProcessData();
				Thread.Sleep(5);
			}
			byte[] array = new byte[4096];
			uint num = 0u;
			val.GetEncryptedAppTicket(array, (uint)array.Length, ref num);
			byte[] array2 = new byte[num];
			Array.Copy(array, array2, num);
			string @string = Encoding.ASCII.GetString(array2);
			return AccessObjectResult.CreateSuccess(new GOGAccessObject(_gogUserName, _gogId, _oldId, @string));
		}
		return AccessObjectResult.CreateFailed(new TextObject("{=hU361b7v}Not logged in on GOG."));
	}
}
