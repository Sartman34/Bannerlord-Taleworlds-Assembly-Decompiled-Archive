using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public class MissionMultiplayerFFAClient : MissionMultiplayerGameModeBaseClient
{
	public override bool IsGameModeUsingGold => false;

	public override bool IsGameModeTactical => false;

	public override bool IsGameModeUsingRoundCountdown => false;

	public override MultiplayerGameType GameType => MultiplayerGameType.FreeForAll;

	public override int GetGoldAmount()
	{
		return 0;
	}

	public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
	{
	}

	public override void AfterStart()
	{
		base.Mission.SetMissionMode(MissionMode.Battle, atStart: true);
	}
}
