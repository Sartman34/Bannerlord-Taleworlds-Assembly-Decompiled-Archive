using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade;

public class MissionMultiplayerFFA : MissionMultiplayerGameModeBase
{
	public override bool IsGameModeHidingAllAgentVisuals => true;

	public override bool IsGameModeUsingOpposingTeams => false;

	public override MultiplayerGameType GetMissionType()
	{
		return MultiplayerGameType.FreeForAll;
	}

	public override void AfterStart()
	{
		BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
		Banner banner = new Banner(@object.BannerKey, @object.BackgroundColor1, @object.ForegroundColor1);
		Team team = base.Mission.Teams.Add(BattleSideEnum.Attacker, @object.BackgroundColor1, @object.ForegroundColor1, banner, isPlayerGeneral: false);
		team.SetIsEnemyOf(team, isEnemyOf: true);
	}

	protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
	{
		networkPeer.AddComponent<FFAMissionRepresentative>();
	}

	protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
	{
		MissionPeer component = networkPeer.GetComponent<MissionPeer>();
		component.Team = base.Mission.AttackerTeam;
		component.Culture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
	}
}
