using Galaxy.Api;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.PlatformService.GOG;

public static class SteamPlayerIdExtensions
{
	public static PlayerId ToPlayerId(this GalaxyID galaxyID)
	{
		return new PlayerId(5, 0uL, galaxyID.ToUint64());
	}

	public static GalaxyID ToGOGID(this PlayerId playerId)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		if (!playerId.IsValidGOGId())
		{
			return new GalaxyID(0uL);
		}
		return new GalaxyID(playerId.Part4);
	}

	public static bool IsValidGOGId(this PlayerId playerId)
	{
		if (playerId.IsValid)
		{
			return playerId.ProvidedType == PlayerIdProvidedTypes.GOG;
		}
		return false;
	}
}
