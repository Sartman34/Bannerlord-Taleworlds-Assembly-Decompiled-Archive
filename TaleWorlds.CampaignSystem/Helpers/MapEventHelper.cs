using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;

namespace Helpers;

public static class MapEventHelper
{
	public static PartyBase GetSallyOutDefenderLeader()
	{
		if (MobileParty.MainParty.CurrentSettlement.Town.GarrisonParty != null)
		{
			return MobileParty.MainParty.CurrentSettlement.Town.GarrisonParty.MapEvent.DefenderSide.LeaderParty;
		}
		if (MobileParty.MainParty.CurrentSettlement.Party?.MapEvent != null)
		{
			return MobileParty.MainParty.CurrentSettlement.Party.MapEvent.DefenderSide.LeaderParty;
		}
		return MobileParty.MainParty.CurrentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.Party;
	}

	public static bool CanLeaveBattle(MobileParty mobileParty)
	{
		if (mobileParty.MapEvent.DefenderSide.LeaderParty != mobileParty.Party && (!mobileParty.MapEvent.DefenderSide.LeaderParty.IsSettlement || mobileParty.CurrentSettlement != mobileParty.MapEvent.DefenderSide.LeaderParty.Settlement || mobileParty.MapFaction != mobileParty.MapEvent.DefenderSide.LeaderParty.MapFaction) && (mobileParty.MapEvent.PartiesOnSide(BattleSideEnum.Attacker).FindIndexQ((MapEventParty party) => party.Party == mobileParty.Party) < 0 || !mobileParty.MapEvent.IsRaid || mobileParty.Army == null || mobileParty.Army.LeaderParty == mobileParty))
		{
			if (mobileParty.MapEvent.PartiesOnSide(BattleSideEnum.Defender).FindIndexQ((MapEventParty party) => party.Party == mobileParty.Party) >= 0 && mobileParty.Army != null)
			{
				return mobileParty.Army.LeaderParty == mobileParty;
			}
			return true;
		}
		return false;
	}

	public static void OnConversationEnd()
	{
		if (PlayerEncounter.Current != null && ((PlayerEncounter.EncounteredMobileParty != null && PlayerEncounter.EncounteredMobileParty.MapFaction != null && !PlayerEncounter.EncounteredMobileParty.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction)) || (PlayerEncounter.EncounteredParty != null && PlayerEncounter.EncounteredParty.MapFaction != null && !PlayerEncounter.EncounteredParty.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))))
		{
			PlayerEncounter.LeaveEncounter = true;
		}
	}
}
