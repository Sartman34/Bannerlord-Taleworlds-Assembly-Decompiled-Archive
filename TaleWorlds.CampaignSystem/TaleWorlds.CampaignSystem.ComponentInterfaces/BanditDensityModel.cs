using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class BanditDensityModel : GameModel
{
	public abstract int NumberOfMaximumLooterParties { get; }

	public abstract int NumberOfMinimumBanditPartiesInAHideoutToInfestIt { get; }

	public abstract int NumberOfMaximumBanditPartiesInEachHideout { get; }

	public abstract int NumberOfMaximumBanditPartiesAroundEachHideout { get; }

	public abstract int NumberOfMaximumHideoutsAtEachBanditFaction { get; }

	public abstract int NumberOfInitialHideoutsAtEachBanditFaction { get; }

	public abstract int NumberOfMinimumBanditTroopsInHideoutMission { get; }

	public abstract int NumberOfMaximumTroopCountForFirstFightInHideout { get; }

	public abstract int NumberOfMaximumTroopCountForBossFightInHideout { get; }

	public abstract float SpawnPercentageForFirstFightInHideoutMission { get; }

	public abstract int GetPlayerMaximumTroopCountForHideoutMission(MobileParty party);
}
