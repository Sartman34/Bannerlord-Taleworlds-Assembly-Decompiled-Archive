using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.BarterSystem.Barterables;

public class SafePassageBarterable : Barterable
{
	private readonly Hero _otherHero;

	private readonly PartyBase _otherParty;

	public override string StringID => "safe_passage_barterable";

	public override TextObject Name
	{
		get
		{
			TextObject textObject;
			if (_otherHero != null)
			{
				StringHelpers.SetCharacterProperties("HERO", _otherHero.CharacterObject);
				textObject = new TextObject("{=BJbbahYe}Let {HERO.NAME} Go");
			}
			else
			{
				textObject = new TextObject("{=QKNWsJRb}Let {PARTY} Go");
				textObject.SetTextVariable("PARTY", _otherParty.Name);
			}
			return textObject;
		}
	}

	public SafePassageBarterable(Hero originalOwner, Hero otherHero, PartyBase ownerParty, PartyBase otherParty)
		: base(originalOwner, ownerParty)
	{
		_otherHero = otherHero;
		_otherParty = otherParty;
	}

	public override int GetUnitValueForFaction(IFaction faction)
	{
		float num = MathF.Clamp(PlayerEncounter.Current.GetPlayerStrengthRatioInEncounter(), 0f, 1f);
		int num2 = (int)MathF.Clamp(Hero.MainHero.Gold + PartyBase.MainParty.ItemRoster.Sum((ItemRosterElement t) => t.EquipmentElement.Item.Value * t.Amount), 0f, 2.1474836E+09f);
		float num3 = ((num < 1f) ? (0.05f + (1f - num) * 0.2f) : 0.1f);
		float num4 = ((faction.Leader == null) ? 1f : MathF.Clamp((50f + (float)faction.Leader.GetRelation(_otherHero)) / 50f, 0.05f, 1.1f));
		if (!PlayerEncounter.EncounteredParty.IsMobile || !PlayerEncounter.EncounteredParty.MobileParty.IsBandit)
		{
			num2 += 3000 + (int)(Hero.MainHero.Clan.Renown * 50f);
			num3 *= 1.5f;
		}
		if (MobileParty.MainParty.MapEvent != null || MobileParty.MainParty.SiegeEvent != null)
		{
			num3 *= 1.2f;
		}
		int num5 = (int)((float)num2 * num3 + 1000f);
		MobileParty mobileParty = PlayerEncounter.EncounteredParty.MobileParty;
		if (mobileParty != null && mobileParty.IsBandit)
		{
			num5 /= 8;
			if (Hero.MainHero.GetPerkValue(DefaultPerks.Roguery.SweetTalker))
			{
				num5 += MathF.Round((float)num5 * DefaultPerks.Roguery.SweetTalker.PrimaryBonus);
			}
		}
		else
		{
			num5 /= 2;
			num5 += (int)(0.3f * num3 * Campaign.Current.Models.ValuationModel.GetMilitaryValueOfParty(_otherParty.MobileParty));
			num5 += (int)(0.3f * num3 * Campaign.Current.Models.ValuationModel.GetValueOfHero(_otherHero));
		}
		if (Hero.MainHero.GetPerkValue(DefaultPerks.Trade.MarketDealer))
		{
			num5 += MathF.Round((float)num5 * DefaultPerks.Trade.MarketDealer.PrimaryBonus);
		}
		if (faction == base.OriginalOwner?.Clan || faction == base.OriginalOwner?.MapFaction || faction == base.OriginalParty.MapFaction)
		{
			return -(int)((float)num5 / (num4 * num4));
		}
		if (faction == _otherHero?.Clan || faction == _otherHero?.MapFaction || faction == _otherParty.MapFaction)
		{
			return (int)(0.9f * (float)num5);
		}
		return num5;
	}

	public override bool IsCompatible(Barterable barterable)
	{
		return true;
	}

	public override ImageIdentifier GetVisualIdentifier()
	{
		return null;
	}

	public override void Apply()
	{
		if (PlayerEncounter.Current != null)
		{
			List<MobileParty> partiesToJoinPlayerSide = new List<MobileParty>();
			List<MobileParty> partiesToJoinEnemySide = new List<MobileParty> { base.OriginalParty.MobileParty };
			PlayerEncounter.Current.FindAllNpcPartiesWhoWillJoinEvent(ref partiesToJoinPlayerSide, ref partiesToJoinEnemySide);
			if (base.OriginalParty?.SiegeEvent != null && base.OriginalParty.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(base.OriginalParty) && _otherParty != null && base.OriginalParty.SiegeEvent.BesiegedSettlement.HasInvolvedPartyForEventType(_otherParty))
			{
				if (base.OriginalParty.SiegeEvent.BesiegedSettlement.MapFaction == Hero.MainHero.MapFaction)
				{
					GainKingdomInfluenceAction.ApplyForSiegeSafePassageBarter(MobileParty.MainParty, -10f);
				}
				Campaign.Current.GameMenuManager.SetNextMenu("menu_siege_safe_passage_accepted");
				PlayerSiege.ClosePlayerSiege();
				{
					foreach (MobileParty item in partiesToJoinEnemySide)
					{
						item.Ai.SetDoNotAttackMainParty(32);
					}
					return;
				}
			}
			Settlement settlement = (from t in Settlement.All.Where((Settlement t) => base.OriginalParty.MobileParty.IsBandit == t.IsHideout && !base.OriginalParty.MobileParty.MapFaction.IsAtWarWith(t.MapFaction)).ToList()
				orderby t.GatePosition.DistanceSquared(base.OriginalParty.Position2D)
				select t).First();
			foreach (MobileParty item2 in partiesToJoinEnemySide)
			{
				item2.Ai.SetDoNotAttackMainParty(32);
				item2.Ai.SetMoveModeHold();
				item2.IgnoreForHours(32f);
				item2.Ai.SetInitiative(0f, 0.8f, 8f);
				if (settlement != null)
				{
					item2.Ai.SetMovePatrolAroundSettlement(settlement);
				}
			}
			PlayerEncounter.LeaveEncounter = true;
			if (MobileParty.MainParty.SiegeEvent != null && MobileParty.MainParty.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(PartyBase.MainParty))
			{
				MobileParty.MainParty.BesiegerCamp = null;
			}
			if (base.OriginalParty?.MobileParty?.Ai.AiBehaviorPartyBase != null && base.OriginalParty != PartyBase.MainParty)
			{
				base.OriginalParty.MobileParty.Ai.SetMoveModeHold();
				if (base.OriginalParty.MobileParty.Army != null && MobileParty.MainParty.Army != base.OriginalParty.MobileParty.Army)
				{
					base.OriginalParty.MobileParty.Army.LeaderParty.Ai.SetMoveModeHold();
				}
			}
		}
		else
		{
			Debug.FailedAssert("Can not find player encounter for safe passage barterable", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\BarterSystem\\Barterables\\SafePassageBarterable.cs", "Apply", 189);
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsSafePassageBarterable(object o, List<object> collectedObjects)
	{
		((SafePassageBarterable)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
	}
}
