using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultBanditDensityModel : BanditDensityModel
{
	public override int NumberOfMaximumLooterParties => 150;

	public override int NumberOfMinimumBanditPartiesInAHideoutToInfestIt => 2;

	public override int NumberOfMaximumBanditPartiesInEachHideout => 4;

	public override int NumberOfMaximumBanditPartiesAroundEachHideout => 8;

	public override int NumberOfMaximumHideoutsAtEachBanditFaction => 10;

	public override int NumberOfInitialHideoutsAtEachBanditFaction => 3;

	public override int NumberOfMinimumBanditTroopsInHideoutMission => 10;

	public override int NumberOfMaximumTroopCountForFirstFightInHideout => MathF.Floor(6f * (2f + Campaign.Current.PlayerProgress));

	public override int NumberOfMaximumTroopCountForBossFightInHideout => MathF.Floor(1f + 5f * (1f + Campaign.Current.PlayerProgress));

	public override float SpawnPercentageForFirstFightInHideoutMission => 0.75f;

	public override int GetPlayerMaximumTroopCountForHideoutMission(MobileParty party)
	{
		float num = 10f;
		if (party.HasPerk(DefaultPerks.Tactics.SmallUnitTactics))
		{
			num += DefaultPerks.Tactics.SmallUnitTactics.PrimaryBonus;
		}
		return MathF.Round(num);
	}
}
