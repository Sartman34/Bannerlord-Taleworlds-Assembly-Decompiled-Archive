using System.Collections.Generic;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade;

public static class CustomGameMutedPlayerManager
{
	private static List<PlayerId> _mutedPlayers = new List<PlayerId>();

	public static List<PlayerId> MutedPlayers => _mutedPlayers;

	public static void MutePlayer(PlayerId playerId)
	{
		_mutedPlayers.Add(playerId);
	}

	public static void UnmutePlayer(PlayerId playerId)
	{
		_mutedPlayers.Remove(playerId);
	}

	public static bool IsUserMuted(PlayerId playerId)
	{
		return _mutedPlayers.Contains(playerId);
	}

	public static void ClearMutedPlayers()
	{
		_mutedPlayers.Clear();
	}
}
