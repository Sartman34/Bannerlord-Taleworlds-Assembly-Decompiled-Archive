using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Election;

public class SettlementClaimantDecision : KingdomDecision
{
	public class ClanAsDecisionOutcome : DecisionOutcome
	{
		[SaveableField(300)]
		public readonly Clan Clan;

		internal static void AutoGeneratedStaticCollectObjectsClanAsDecisionOutcome(object o, List<object> collectedObjects)
		{
			((ClanAsDecisionOutcome)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(Clan);
		}

		internal static object AutoGeneratedGetMemberValueClan(object o)
		{
			return ((ClanAsDecisionOutcome)o).Clan;
		}

		public override TextObject GetDecisionTitle()
		{
			return Clan.Leader.Name;
		}

		public override TextObject GetDecisionDescription()
		{
			TextObject textObject = new TextObject("{=QKIxepj5}The lordship of this fief should go to the {RECIPIENT.CLAN}");
			StringHelpers.SetCharacterProperties("RECIPIENT", Clan.Leader.CharacterObject, textObject, includeDetails: true);
			return textObject;
		}

		public override string GetDecisionLink()
		{
			return Clan.Leader.EncyclopediaLink.ToString();
		}

		public override ImageIdentifier GetDecisionImageIdentifier()
		{
			return new ImageIdentifier(CharacterCode.CreateFrom(Clan.Leader.CharacterObject));
		}

		public ClanAsDecisionOutcome(Clan clan)
		{
			Clan = clan;
		}
	}

	[SaveableField(300)]
	public readonly Settlement Settlement;

	[SaveableField(301)]
	public readonly Clan ClanToExclude;

	[SaveableField(302)]
	private readonly Hero _capturerHero;

	internal static void AutoGeneratedStaticCollectObjectsSettlementClaimantDecision(object o, List<object> collectedObjects)
	{
		((SettlementClaimantDecision)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(Settlement);
		collectedObjects.Add(ClanToExclude);
		collectedObjects.Add(_capturerHero);
	}

	internal static object AutoGeneratedGetMemberValueSettlement(object o)
	{
		return ((SettlementClaimantDecision)o).Settlement;
	}

	internal static object AutoGeneratedGetMemberValueClanToExclude(object o)
	{
		return ((SettlementClaimantDecision)o).ClanToExclude;
	}

	internal static object AutoGeneratedGetMemberValue_capturerHero(object o)
	{
		return ((SettlementClaimantDecision)o)._capturerHero;
	}

	public SettlementClaimantDecision(Clan proposerClan, Settlement settlement, Hero capturerHero, Clan clanToExclude)
		: base(proposerClan)
	{
		Settlement = settlement;
		_capturerHero = capturerHero;
		ClanToExclude = clanToExclude;
	}

	public override bool IsAllowed()
	{
		return Campaign.Current.Models.KingdomDecisionPermissionModel.IsAnnexationDecisionAllowed(Settlement);
	}

	public override int GetProposalInfluenceCost()
	{
		return Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfAnnexation(base.ProposerClan);
	}

	public override TextObject GetSupportTitle()
	{
		TextObject textObject = new TextObject("{=Of7XnP5c}Vote for the new owner of {SETTLEMENT_NAME}");
		textObject.SetTextVariable("SETTLEMENT_NAME", Settlement.Name);
		return textObject;
	}

	public override TextObject GetGeneralTitle()
	{
		TextObject textObject = new TextObject("{=2qZ81jPG}Owner of {SETTLEMENT_NAME}");
		textObject.SetTextVariable("SETTLEMENT_NAME", Settlement.Name);
		return textObject;
	}

	public override TextObject GetSupportDescription()
	{
		TextObject textObject = new TextObject("{=J4UMplzb}{FACTION_LEADER} will decide who will own {SETTLEMENT_NAME}. You can give your support to one of the candidates.");
		textObject.SetTextVariable("FACTION_LEADER", DetermineChooser().Leader.Name);
		textObject.SetTextVariable("SETTLEMENT_NAME", Settlement.Name);
		return textObject;
	}

	public override TextObject GetChooseTitle()
	{
		TextObject textObject = new TextObject("{=2qZ81jPG}Owner of {SETTLEMENT_NAME}");
		textObject.SetTextVariable("SETTLEMENT_NAME", Settlement.Name);
		return textObject;
	}

	public override TextObject GetChooseDescription()
	{
		TextObject textObject = new TextObject("{=xzq78nVm}As {?IS_FEMALE}queen{?}king{\\?} you must decide who will own {SETTLEMENT_NAME}.");
		textObject.SetTextVariable("IS_FEMALE", DetermineChooser().Leader.IsFemale ? 1 : 0);
		textObject.SetTextVariable("SETTLEMENT_NAME", Settlement.Name);
		return textObject;
	}

	protected override bool ShouldBeCancelledInternal()
	{
		return Settlement.MapFaction != base.Kingdom;
	}

	protected override bool CanProposerClanChangeOpinion()
	{
		return true;
	}

	public override float CalculateMeritOfOutcome(DecisionOutcome candidateOutcome)
	{
		ClanAsDecisionOutcome clanAsDecisionOutcome = (ClanAsDecisionOutcome)candidateOutcome;
		Clan clan = clanAsDecisionOutcome.Clan;
		float num = 0f;
		int num2 = 0;
		float num3 = Campaign.MapDiagonal + 1f;
		float num4 = Campaign.MapDiagonal + 1f;
		foreach (Settlement item in Settlement.All)
		{
			if (item.OwnerClan != clanAsDecisionOutcome.Clan || !item.IsFortification || Settlement == item)
			{
				continue;
			}
			num += item.GetSettlementValueForFaction(clanAsDecisionOutcome.Clan.Kingdom);
			if (Campaign.Current.Models.MapDistanceModel.GetDistance(item, Settlement, num4, out var distance))
			{
				if (distance < num3)
				{
					num4 = num3;
					num3 = distance;
				}
				else if (distance < num4)
				{
					num4 = distance;
				}
			}
			num2++;
		}
		float num5 = Campaign.AverageDistanceBetweenTwoFortifications * 1.5f;
		float a = num5 * 0.25f;
		float b = num5;
		if (num4 < Campaign.MapDiagonal)
		{
			b = (num4 + num3) / 2f;
		}
		else if (num3 < Campaign.MapDiagonal)
		{
			b = num3;
		}
		float num6 = TaleWorlds.Library.MathF.Pow(num5 / TaleWorlds.Library.MathF.Max(a, TaleWorlds.Library.MathF.Min(400f, b)), 0.5f);
		float num7 = clan.TotalStrength;
		if (Settlement.OwnerClan == clan && Settlement.Town != null && Settlement.Town.GarrisonParty != null)
		{
			num7 -= Settlement.Town.GarrisonParty.Party.TotalStrength;
			if (num7 < 0f)
			{
				num7 = 0f;
			}
		}
		float settlementValueForFaction = Settlement.GetSettlementValueForFaction(clanAsDecisionOutcome.Clan.Kingdom);
		bool num8 = clanAsDecisionOutcome.Clan.Leader == clanAsDecisionOutcome.Clan.Kingdom.Leader;
		float num9 = ((num2 == 0) ? 30f : 0f);
		float num10 = (num8 ? 60f : 0f);
		float num11 = ((Settlement.Town != null && Settlement.Town.LastCapturedBy == clanAsDecisionOutcome.Clan) ? 30f : 0f);
		float num12 = ((clanAsDecisionOutcome.Clan.Leader == Hero.MainHero) ? 30f : 0f);
		float num13 = ((clanAsDecisionOutcome.Clan.Leader.Gold < 30000) ? TaleWorlds.Library.MathF.Min(30f, 30f - (float)clanAsDecisionOutcome.Clan.Leader.Gold / 1000f) : 0f);
		return ((float)clan.Tier * 30f + num7 / 10f + num9 + num11 + num10 + num13 + num12) / (num + settlementValueForFaction) * num6 * 200000f;
	}

	public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
	{
		Kingdom obj = (Kingdom)Settlement.MapFaction;
		List<ClanAsDecisionOutcome> list = new List<ClanAsDecisionOutcome>();
		foreach (Clan clan in obj.Clans)
		{
			if (clan != ClanToExclude && !clan.IsUnderMercenaryService && !clan.IsEliminated && !clan.Leader.IsDead)
			{
				list.Add(new ClanAsDecisionOutcome(clan));
			}
		}
		return list;
	}

	public override Clan DetermineChooser()
	{
		return ((Kingdom)Settlement.MapFaction).RulingClan;
	}

	public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
	{
		ClanAsDecisionOutcome clanAsDecisionOutcome = (ClanAsDecisionOutcome)possibleOutcome;
		float initialMerit = clanAsDecisionOutcome.InitialMerit;
		int traitLevel = clan.Leader.GetTraitLevel(DefaultTraits.Honor);
		initialMerit *= TaleWorlds.Library.MathF.Clamp(1f + (float)traitLevel, 0f, 2f);
		if (clanAsDecisionOutcome.Clan == clan)
		{
			float settlementValueForFaction = Settlement.GetSettlementValueForFaction(clan);
			initialMerit += 0.2f * settlementValueForFaction * Campaign.Current.Models.DiplomacyModel.DenarsToInfluence();
		}
		else
		{
			float num = ((clanAsDecisionOutcome.Clan != clan) ? ((float)FactionManager.GetRelationBetweenClans(clanAsDecisionOutcome.Clan, clan)) : 100f);
			int traitLevel2 = clan.Leader.GetTraitLevel(DefaultTraits.Calculating);
			initialMerit *= TaleWorlds.Library.MathF.Clamp(1f + (float)traitLevel2, 0f, 2f);
			float num2 = num * 0.2f * (float)traitLevel2;
			initialMerit += num2;
		}
		int traitLevel3 = clan.Leader.GetTraitLevel(DefaultTraits.Calculating);
		float num3 = ((traitLevel3 > 0) ? (0.4f - (float)TaleWorlds.Library.MathF.Min(2, traitLevel3) * 0.1f) : (0.4f + (float)TaleWorlds.Library.MathF.Min(2, TaleWorlds.Library.MathF.Abs(traitLevel3)) * 0.1f));
		float num4 = 1f - num3 * 1.5f;
		initialMerit *= num4;
		float num5 = ((clan == clanAsDecisionOutcome.Clan) ? 2f : 1f);
		return initialMerit * num5;
	}

	public override void DetermineSponsors(MBReadOnlyList<DecisionOutcome> possibleOutcomes)
	{
		foreach (DecisionOutcome possibleOutcome in possibleOutcomes)
		{
			possibleOutcome.SetSponsor(((ClanAsDecisionOutcome)possibleOutcome).Clan);
		}
	}

	public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
	{
		ChangeOwnerOfSettlementAction.ApplyByKingDecision(((ClanAsDecisionOutcome)chosenOutcome).Clan.Leader, Settlement);
	}

	protected override int GetInfluenceCostOfSupportInternal(Supporter.SupportWeights supportWeight)
	{
		switch (supportWeight)
		{
		case Supporter.SupportWeights.SlightlyFavor:
			return 20;
		case Supporter.SupportWeights.StronglyFavor:
			return 60;
		case Supporter.SupportWeights.FullyPush:
			return 100;
		default:
			throw new ArgumentOutOfRangeException("supportWeight", supportWeight, null);
		case Supporter.SupportWeights.Choose:
		case Supporter.SupportWeights.StayNeutral:
			return 0;
		}
	}

	public override TextObject GetSecondaryEffects()
	{
		return new TextObject("{=bHNU9uz2}All supporters gains some relation with the supported candidate clan and might lose with the others.");
	}

	public override void ApplySecondaryEffects(MBReadOnlyList<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
	{
	}

	public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
	{
		TextObject empty = TextObject.Empty;
		bool flag = ((ClanAsDecisionOutcome)chosenOutcome).Clan.Leader == Settlement.MapFaction.Leader;
		empty = ((supportStatus == SupportStatus.Majority && flag) ? new TextObject("{=Zckbdm4Z}{RULER.NAME} of the {KINGDOM} takes {SETTLEMENT} as {?RULER.GENDER}her{?}his{\\?} fief with {?RULER.GENDER}her{?}his{\\?} council's support.") : ((supportStatus == SupportStatus.Minority && flag) ? new TextObject("{=qa4FlTWS}{RULER.NAME} of the {KINGDOM} takes {SETTLEMENT} as {?RULER.GENDER}her{?}his{\\?} fief despite {?RULER.GENDER}her{?}his{\\?} council's opposition.") : (flag ? new TextObject("{=5bBAOHmC}{RULER.NAME} of the {KINGDOM} takes {SETTLEMENT} as {?RULER.GENDER}her{?}his{\\?} fief, with {?RULER.GENDER}her{?}his{\\?} council evenly split.") : (supportStatus switch
		{
			SupportStatus.Majority => new TextObject("{=0nhqJewP}{RULER.NAME} of the {KINGDOM} grants {SETTLEMENT} to {LEADER.NAME} with {?RULER.GENDER}her{?}his{\\?} council's support."), 
			SupportStatus.Minority => new TextObject("{=Ktpia7Pa}{RULER.NAME} of the {KINGDOM} grants {SETTLEMENT} to {LEADER.NAME} despite {?RULER.GENDER}her{?}his{\\?} council's opposition."), 
			_ => new TextObject("{=l5H9x7Lo}{RULER.NAME} of the {KINGDOM} grants {SETTLEMENT} to {LEADER.NAME}, with {?RULER.GENDER}her{?}his{\\?} council evenly split."), 
		}))));
		empty.SetTextVariable("SETTLEMENT", Settlement.Name);
		StringHelpers.SetCharacterProperties("LEADER", ((ClanAsDecisionOutcome)chosenOutcome).Clan.Leader.CharacterObject, empty);
		StringHelpers.SetCharacterProperties("RULER", Settlement.MapFaction.Leader.CharacterObject, empty);
		empty.SetTextVariable("KINGDOM", Settlement.MapFaction.InformalName);
		empty.SetTextVariable("CLAN", ((ClanAsDecisionOutcome)chosenOutcome).Clan.Name);
		return empty;
	}

	public override DecisionOutcome GetQueriedDecisionOutcome(MBReadOnlyList<DecisionOutcome> possibleOutcomes)
	{
		return possibleOutcomes.OrderByDescending((DecisionOutcome t) => t.Merit).FirstOrDefault();
	}
}