using Steamworks;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.PlatformService.Steam;

public static class SteamPlayerIdExtensions
{
	public static PlayerId ToPlayerId(this CSteamID steamId)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return new PlayerId(2, 0uL, steamId.m_SteamID);
	}

	public static CSteamID ToSteamId(this PlayerId playerId)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (playerId.IsValidSteamId())
		{
			return new CSteamID(playerId.Part4);
		}
		return new CSteamID(0uL);
	}

	public static bool IsValidSteamId(this PlayerId playerId)
	{
		if (playerId.IsValid)
		{
			return playerId.ProvidedType == PlayerIdProvidedTypes.Steam;
		}
		return false;
	}
}
