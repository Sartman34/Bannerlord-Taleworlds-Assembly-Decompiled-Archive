using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem;

public class Army
{
	public enum AIBehaviorFlags
	{
		Unassigned,
		PreGathering,
		Gathering,
		WaitingForArmyMembers,
		TravellingToAssignment,
		Besieging,
		AssaultingTown,
		Raiding,
		Defending,
		Patrolling,
		GoToSettlement,
		NumberOfAIBehaviorFlags
	}

	public enum ArmyTypes
	{
		Besieger,
		Raider,
		Defender,
		Patrolling,
		NumberOfArmyTypes
	}

	private enum MainPartyCurrentAction
	{
		Idle,
		GatherAroundHero,
		GatherAroundSettlement,
		GoToSettlement,
		RaidSettlement,
		BesiegeSettlement,
		PatrolAroundSettlement,
		DefendingSettlement
	}

	public enum ArmyDispersionReason
	{
		Unknown,
		DismissalRequestedWithInfluence,
		NotEnoughParty,
		KingdomChanged,
		CohesionDepleted,
		ObjectiveFinished,
		LeaderPartyRemoved,
		PlayerTakenPrisoner,
		CannotElectNewLeader,
		LeaderCannotArrivePointOnTime,
		ArmyLeaderIsDead,
		FoodProblem,
		NotEnoughTroop,
		NoActiveWar
	}

	public enum ArmyLeaderThinkReason
	{
		Unknown,
		FromGatheringToWaiting,
		FromTravellingToBesieging,
		FromWaitingToTravelling,
		ChangingTarget,
		FromTravellingToRaiding,
		FromTravellingToDefending,
		FromRaidingToTravelling,
		FromBesiegingToTravelling,
		FromDefendingToTravelling,
		FromPatrollingToDefending,
		FromBesiegingToDefending,
		FromDefendingToBesieging,
		FromDefendingToPatrolling,
		FromUnassignedToPatrolling,
		FromUnassignedToTravelling
	}

	private const float MaximumWaitTime = 72f;

	private const float ArmyGatheringConcludingTickFrequency = 1f;

	private const float GatheringDistance = 3.5f;

	private const float DefaultGatheringWaitTime = 24f;

	private const float MinimumDistanceWhileGatheringAsAttackerArmy = 40f;

	private const float CheckingForBoostingCohesionThreshold = 50f;

	private const float DisbandCohesionThreshold = 30f;

	private const float StrengthThresholdRatioForGathering = 0.7f;

	[SaveableField(1)]
	private readonly MBList<MobileParty> _parties;

	[SaveableField(19)]
	private CampaignTime _creationTime;

	[SaveableField(7)]
	private float _armyGatheringTime;

	[SaveableField(9)]
	private float _waitTimeStart;

	[SaveableField(10)]
	private bool _armyIsDispersing;

	[SaveableField(11)]
	private int _numberOfBoosts;

	[SaveableField(15)]
	private Kingdom _kingdom;

	[SaveableField(16)]
	private IMapPoint _aiBehaviorObject;

	[CachedData]
	private MBCampaignEvent _hourlyTickEvent;

	[CachedData]
	private MBCampaignEvent _tickEvent;

	public MBReadOnlyList<MobileParty> Parties => _parties;

	public TextObject EncyclopediaLinkWithName => ArmyOwner.EncyclopediaLinkWithName;

	[SaveableProperty(2)]
	public AIBehaviorFlags AIBehavior { get; set; }

	[SaveableProperty(3)]
	public ArmyTypes ArmyType { get; set; }

	[SaveableProperty(4)]
	public Hero ArmyOwner { get; set; }

	[SaveableProperty(5)]
	public float Cohesion { get; set; }

	public float DailyCohesionChange => Campaign.Current.Models.ArmyManagementCalculationModel.CalculateDailyCohesionChange(this).ResultNumber;

	public ExplainedNumber DailyCohesionChangeExplanation => Campaign.Current.Models.ArmyManagementCalculationModel.CalculateDailyCohesionChange(this, includeDescriptions: true);

	public int CohesionThresholdForDispersion => Campaign.Current.Models.ArmyManagementCalculationModel.CohesionThresholdForDispersion;

	[SaveableProperty(13)]
	public float Morale { get; private set; }

	[SaveableProperty(14)]
	public MobileParty LeaderParty { get; private set; }

	public int LeaderPartyAndAttachedPartiesCount => LeaderParty.AttachedParties.Count + 1;

	public float TotalStrength
	{
		get
		{
			float num = LeaderParty.Party.TotalStrength;
			foreach (MobileParty attachedParty in LeaderParty.AttachedParties)
			{
				num += attachedParty.Party.TotalStrength;
			}
			return num;
		}
	}

	public Kingdom Kingdom
	{
		get
		{
			return _kingdom;
		}
		set
		{
			if (value != _kingdom)
			{
				_kingdom?.RemoveArmyInternal(this);
				_kingdom = value;
				_kingdom?.AddArmyInternal(this);
			}
		}
	}

	public IMapPoint AiBehaviorObject
	{
		get
		{
			return _aiBehaviorObject;
		}
		set
		{
			if (value != _aiBehaviorObject && Parties.Contains(MobileParty.MainParty) && LeaderParty != MobileParty.MainParty)
			{
				StopTrackingTargetSettlement();
				StartTrackingTargetSettlement(value);
			}
			if (value == null)
			{
				AIBehavior = AIBehaviorFlags.Unassigned;
			}
			_aiBehaviorObject = value;
		}
	}

	[SaveableProperty(17)]
	public TextObject Name { get; private set; }

	public int TotalHealthyMembers => LeaderParty.Party.NumberOfHealthyMembers + LeaderParty.AttachedParties.Sum((MobileParty mobileParty) => mobileParty.Party.NumberOfHealthyMembers);

	public int TotalManCount => LeaderParty.Party.MemberRoster.TotalManCount + LeaderParty.AttachedParties.Sum((MobileParty mobileParty) => mobileParty.Party.MemberRoster.TotalManCount);

	public int TotalRegularCount => LeaderParty.Party.MemberRoster.TotalRegulars + LeaderParty.AttachedParties.Sum((MobileParty mobileParty) => mobileParty.Party.MemberRoster.TotalRegulars);

	public override string ToString()
	{
		return Name.ToString();
	}

	public Army(Kingdom kingdom, MobileParty leaderParty, ArmyTypes armyType)
	{
		Kingdom = kingdom;
		_parties = new MBList<MobileParty>();
		_armyGatheringTime = 0f;
		_creationTime = CampaignTime.Now;
		LeaderParty = leaderParty;
		LeaderParty.Army = this;
		ArmyOwner = LeaderParty.LeaderHero;
		UpdateName();
		ArmyType = armyType;
		AddEventHandlers();
		Cohesion = 100f;
	}

	public void UpdateName()
	{
		Name = new TextObject("{=nbmctMLk}{LEADER_NAME}{.o} Army");
		Name.SetTextVariable("LEADER_NAME", (ArmyOwner != null) ? ArmyOwner.Name : ((LeaderParty.Owner != null) ? LeaderParty.Owner.Name : TextObject.Empty));
	}

	private void AddEventHandlers()
	{
		if (_creationTime == default(CampaignTime))
		{
			_creationTime = CampaignTime.HoursFromNow(MBRandom.RandomFloat - 2f);
		}
		CampaignTime campaignTime = CampaignTime.Now - _creationTime;
		CampaignTime initialWait = CampaignTime.Hours(1f + (float)(int)campaignTime.ToHours) - campaignTime;
		_hourlyTickEvent = CampaignPeriodicEventManager.CreatePeriodicEvent(CampaignTime.Hours(1f), initialWait);
		_hourlyTickEvent.AddHandler(HourlyTick);
		_tickEvent = CampaignPeriodicEventManager.CreatePeriodicEvent(CampaignTime.Hours(0.1f), CampaignTime.Hours(1f));
		_tickEvent.AddHandler(Tick);
	}

	internal void OnAfterLoad()
	{
		AddEventHandlers();
	}

	[LoadInitializationCallback]
	private void OnLoad(MetaData metaData)
	{
		if (AiBehaviorObject == null)
		{
			AIBehavior = AIBehaviorFlags.Unassigned;
		}
	}

	public bool DoesLeaderPartyAndAttachedPartiesContain(MobileParty party)
	{
		if (LeaderParty != party)
		{
			return LeaderParty.AttachedParties.IndexOf(party) >= 0;
		}
		return true;
	}

	public void BoostCohesionWithInfluence(float cohesionToGain, int cost)
	{
		if (LeaderParty.LeaderHero.Clan.Influence >= (float)cost)
		{
			ChangeClanInfluenceAction.Apply(LeaderParty.LeaderHero.Clan, -cost);
			Cohesion += cohesionToGain;
			_numberOfBoosts++;
		}
	}

	private void ThinkAboutCohesionBoost()
	{
		float num = 0f;
		foreach (MobileParty party in Parties)
		{
			float partySizeRatio = party.PartySizeRatio;
			num += partySizeRatio;
		}
		float b = num / (float)Parties.Count;
		float num2 = TaleWorlds.Library.MathF.Min(1f, b);
		float num3 = Campaign.Current.Models.TargetScoreCalculatingModel.CurrentObjectiveValue(LeaderParty);
		if (!(num3 > 0.01f))
		{
			return;
		}
		num3 *= num2;
		num3 *= ((_numberOfBoosts == 0) ? 1f : (1f / TaleWorlds.Library.MathF.Pow(1f + (float)_numberOfBoosts, 0.7f)));
		ArmyManagementCalculationModel armyManagementCalculationModel = Campaign.Current.Models.ArmyManagementCalculationModel;
		float num4 = TaleWorlds.Library.MathF.Min(100f, 100f - Cohesion);
		int num5 = armyManagementCalculationModel.CalculateTotalInfluenceCost(this, num4);
		if (!(LeaderParty.Party.Owner.Clan.Influence > (float)num5))
		{
			return;
		}
		float num6 = TaleWorlds.Library.MathF.Min(9f, TaleWorlds.Library.MathF.Sqrt(LeaderParty.Party.Owner.Clan.Influence / (float)num5));
		float num7 = ((LeaderParty.BesiegedSettlement != null) ? 2f : 1f);
		if (LeaderParty.BesiegedSettlement == null && LeaderParty.DefaultBehavior == AiBehavior.BesiegeSettlement)
		{
			float num8 = LeaderParty.Position2D.Distance(LeaderParty.TargetSettlement.Position2D);
			if (num8 < 125f)
			{
				num7 += (1f - num8 / 125f) * (1f - num8 / 125f);
			}
		}
		float num9 = num3 * num7 * 0.25f * num6;
		if (MBRandom.RandomFloat < num9)
		{
			BoostCohesionWithInfluence(num4, num5);
		}
	}

	public void RecalculateArmyMorale()
	{
		float num = 0f;
		foreach (MobileParty party in Parties)
		{
			num += party.Morale;
		}
		Morale = num / (float)Parties.Count;
	}

	private void HourlyTick(MBCampaignEvent campaignEvent, object[] delegateParams)
	{
		bool flag = LeaderParty.CurrentSettlement != null && LeaderParty.CurrentSettlement.SiegeEvent != null;
		if (LeaderParty.MapEvent != null || flag)
		{
			return;
		}
		RecalculateArmyMorale();
		Cohesion += DailyCohesionChange / 24f;
		if (LeaderParty == MobileParty.MainParty)
		{
			CheckMainPartyGathering();
			CheckMainPartyTravelingToAssignment();
		}
		else
		{
			MoveLeaderToGatheringLocationIfNeeded();
			if (Cohesion < 50f)
			{
				ThinkAboutCohesionBoost();
				if (Cohesion < 30f && LeaderParty.MapEvent == null && LeaderParty.SiegeEvent == null)
				{
					DisbandArmyAction.ApplyByCohesionDepleted(this);
					return;
				}
			}
			switch (AIBehavior)
			{
			case AIBehaviorFlags.Gathering:
				ThinkAboutConcludingArmyGathering();
				break;
			case AIBehaviorFlags.WaitingForArmyMembers:
				ThinkAboutTravelingToAssignment();
				break;
			case AIBehaviorFlags.Defending:
				switch (ArmyType)
				{
				case ArmyTypes.Besieger:
					if (AnyoneBesiegingTarget())
					{
						FinishArmyObjective();
					}
					else
					{
						IsAtSiegeLocation();
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case ArmyTypes.Raider:
				case ArmyTypes.Defender:
				case ArmyTypes.Patrolling:
				case ArmyTypes.NumberOfArmyTypes:
					break;
				}
				break;
			case AIBehaviorFlags.TravellingToAssignment:
				if (ArmyType == ArmyTypes.Besieger)
				{
					IsAtSiegeLocation();
				}
				break;
			}
		}
		CheckArmyDispersion();
		CallArmyMembersToArmyIfNeeded();
		ApplyHostileActionInfluenceAwards();
	}

	private void Tick(MBCampaignEvent campaignevent, object[] delegateparams)
	{
		foreach (MobileParty party in _parties)
		{
			if (party.AttachedTo == null && party.Army != null && party.ShortTermTargetParty == LeaderParty && party.MapEvent == null && (party.Position2D - LeaderParty.Position2D).LengthSquared < Campaign.Current.Models.EncounterModel.NeededMaximumDistanceForEncounteringMobileParty)
			{
				AddPartyToMergedParties(party);
				if (party.IsMainParty)
				{
					Campaign.Current.CameraFollowParty = LeaderParty.Party;
				}
				CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
			}
		}
	}

	private void CheckArmyDispersion()
	{
		if (LeaderParty == MobileParty.MainParty)
		{
			if (Cohesion <= 0.1f)
			{
				DisbandArmyAction.ApplyByCohesionDepleted(this);
				GameMenu.ActivateGameMenu("army_dispersed");
				MBTextManager.SetTextVariable("ARMY_DISPERSE_REASON", new TextObject("{=rJBgDaxe}Your army has disbanded due to lack of cohesion."));
			}
			return;
		}
		int num = (LeaderParty.Party.IsStarving ? 1 : 0);
		foreach (MobileParty attachedParty in LeaderParty.AttachedParties)
		{
			if (attachedParty.Party.IsStarving)
			{
				num++;
			}
		}
		if ((float)num / (float)LeaderPartyAndAttachedPartiesCount > 0.5f)
		{
			DisbandArmyAction.ApplyByFoodProblem(this);
		}
		else if (MBRandom.RandomFloat < 0.25f && !FactionManager.GetEnemyFactions(LeaderParty.MapFaction as Kingdom).AnyQ((IFaction x) => x.Fiefs.Any()))
		{
			DisbandArmyAction.ApplyByNoActiveWar(this);
		}
		else if (Cohesion <= 0.1f)
		{
			DisbandArmyAction.ApplyByCohesionDepleted(this);
		}
		else if (!LeaderParty.IsActive)
		{
			DisbandArmyAction.ApplyByUnknownReason(this);
		}
	}

	private void MoveLeaderToGatheringLocationIfNeeded()
	{
		if (AiBehaviorObject != null && (AIBehavior == AIBehaviorFlags.Gathering || AIBehavior == AIBehaviorFlags.WaitingForArmyMembers) && LeaderParty.MapEvent == null && LeaderParty.ShortTermBehavior == AiBehavior.Hold)
		{
			Settlement settlement = AiBehaviorObject as Settlement;
			Vec2 centerPosition = (settlement.IsFortification ? settlement.GatePosition : settlement.Position2D);
			if (!settlement.IsUnderSiege && !settlement.IsUnderRaid)
			{
				LeaderParty.SendPartyToReachablePointAroundPosition(centerPosition, 6f, 3f);
			}
		}
	}

	private void CheckMainPartyTravelingToAssignment()
	{
		if (AIBehavior == AIBehaviorFlags.Gathering && AiBehaviorObject != null && !Campaign.Current.Models.MapDistanceModel.GetDistance(AiBehaviorObject, MobileParty.MainParty, 3.5f, out var _))
		{
			AIBehavior = AIBehaviorFlags.TravellingToAssignment;
		}
	}

	private void CallArmyMembersToArmyIfNeeded()
	{
		for (int num = Parties.Count - 1; num >= 0; num--)
		{
			MobileParty mobileParty = Parties[num];
			if (mobileParty != LeaderParty && !DoesLeaderPartyAndAttachedPartiesContain(mobileParty) && mobileParty != MobileParty.MainParty)
			{
				if (mobileParty.MapEvent == null && mobileParty.TargetParty != LeaderParty && (mobileParty.CurrentSettlement == null || !mobileParty.CurrentSettlement.IsUnderSiege))
				{
					mobileParty.Ai.SetMoveEscortParty(LeaderParty);
				}
				if (mobileParty.Party.IsStarving)
				{
					mobileParty.Army = null;
				}
			}
		}
	}

	private void ApplyHostileActionInfluenceAwards()
	{
		if (LeaderParty.LeaderHero != null && LeaderParty.MapEvent != null)
		{
			if (LeaderParty.MapEvent.IsRaid && LeaderParty.MapEvent.DefenderSide.TroopCount == 0)
			{
				float hourlyInfluenceAwardForRaidingEnemyVillage = Campaign.Current.Models.DiplomacyModel.GetHourlyInfluenceAwardForRaidingEnemyVillage(LeaderParty);
				GainKingdomInfluenceAction.ApplyForRaidingEnemyVillage(LeaderParty, hourlyInfluenceAwardForRaidingEnemyVillage);
			}
			else if (LeaderParty.BesiegedSettlement != null && LeaderParty.MapFaction.IsAtWarWith(LeaderParty.BesiegedSettlement.MapFaction))
			{
				float hourlyInfluenceAwardForBesiegingEnemyFortification = Campaign.Current.Models.DiplomacyModel.GetHourlyInfluenceAwardForBesiegingEnemyFortification(LeaderParty);
				GainKingdomInfluenceAction.ApplyForBesiegingEnemySettlement(LeaderParty, hourlyInfluenceAwardForBesiegingEnemyFortification);
			}
		}
	}

	private void CheckMainPartyGathering()
	{
		if (AIBehavior == AIBehaviorFlags.PreGathering && AiBehaviorObject != null && Campaign.Current.Models.MapDistanceModel.GetDistance(AiBehaviorObject, MobileParty.MainParty, 3.5f, out var _))
		{
			AIBehavior = AIBehaviorFlags.Gathering;
		}
	}

	private MainPartyCurrentAction GetMainPartyCurrentAction()
	{
		if (PlayerEncounter.EncounterSettlement == null)
		{
			return MainPartyCurrentAction.PatrolAroundSettlement;
		}
		Settlement encounterSettlement = PlayerEncounter.EncounterSettlement;
		if (MobileParty.MainParty.IsActive)
		{
			if (encounterSettlement.IsUnderSiege)
			{
				if (encounterSettlement.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction))
				{
					return MainPartyCurrentAction.BesiegeSettlement;
				}
				return MainPartyCurrentAction.DefendingSettlement;
			}
			if (encounterSettlement.IsUnderRaid)
			{
				if (encounterSettlement.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction))
				{
					return MainPartyCurrentAction.RaidSettlement;
				}
				return MainPartyCurrentAction.DefendingSettlement;
			}
		}
		return MainPartyCurrentAction.GoToSettlement;
	}

	public static ArmyLeaderThinkReason GetBehaviorChangeExplanation(AIBehaviorFlags previousBehavior, AIBehaviorFlags currentBehavior)
	{
		switch (previousBehavior)
		{
		case AIBehaviorFlags.Gathering:
			if (currentBehavior == AIBehaviorFlags.WaitingForArmyMembers)
			{
				return ArmyLeaderThinkReason.FromGatheringToWaiting;
			}
			break;
		case AIBehaviorFlags.WaitingForArmyMembers:
			if (currentBehavior == AIBehaviorFlags.TravellingToAssignment)
			{
				return ArmyLeaderThinkReason.FromWaitingToTravelling;
			}
			break;
		case AIBehaviorFlags.TravellingToAssignment:
			switch (currentBehavior)
			{
			case AIBehaviorFlags.TravellingToAssignment:
				return ArmyLeaderThinkReason.ChangingTarget;
			case AIBehaviorFlags.Besieging:
				return ArmyLeaderThinkReason.FromTravellingToBesieging;
			case AIBehaviorFlags.Raiding:
				return ArmyLeaderThinkReason.FromTravellingToRaiding;
			case AIBehaviorFlags.Defending:
				return ArmyLeaderThinkReason.FromTravellingToDefending;
			}
			break;
		case AIBehaviorFlags.Besieging:
			switch (currentBehavior)
			{
			case AIBehaviorFlags.TravellingToAssignment:
				return ArmyLeaderThinkReason.FromBesiegingToTravelling;
			case AIBehaviorFlags.Defending:
				return ArmyLeaderThinkReason.FromBesiegingToDefending;
			}
			break;
		case AIBehaviorFlags.Defending:
			switch (currentBehavior)
			{
			case AIBehaviorFlags.TravellingToAssignment:
				return ArmyLeaderThinkReason.FromDefendingToTravelling;
			case AIBehaviorFlags.Besieging:
				return ArmyLeaderThinkReason.FromDefendingToBesieging;
			case AIBehaviorFlags.Patrolling:
				return ArmyLeaderThinkReason.FromDefendingToPatrolling;
			}
			break;
		case AIBehaviorFlags.Raiding:
			if (currentBehavior == AIBehaviorFlags.TravellingToAssignment)
			{
				return ArmyLeaderThinkReason.FromRaidingToTravelling;
			}
			break;
		case AIBehaviorFlags.Patrolling:
			switch (currentBehavior)
			{
			case AIBehaviorFlags.Patrolling:
				return ArmyLeaderThinkReason.ChangingTarget;
			case AIBehaviorFlags.Defending:
				return ArmyLeaderThinkReason.FromPatrollingToDefending;
			}
			break;
		case AIBehaviorFlags.Unassigned:
			switch (currentBehavior)
			{
			case AIBehaviorFlags.Patrolling:
				return ArmyLeaderThinkReason.FromUnassignedToPatrolling;
			case AIBehaviorFlags.TravellingToAssignment:
				return ArmyLeaderThinkReason.FromUnassignedToTravelling;
			}
			break;
		}
		return ArmyLeaderThinkReason.Unknown;
	}

	public TextObject GetNotificationText()
	{
		if (LeaderParty != MobileParty.MainParty)
		{
			TextObject textObject = GameTexts.FindText("str_army_gather");
			StringHelpers.SetCharacterProperties("ARMY_LEADER", LeaderParty.LeaderHero.CharacterObject, textObject);
			textObject.SetTextVariable("SETTLEMENT_NAME", AiBehaviorObject.Name);
			return textObject;
		}
		return null;
	}

	public TextObject GetBehaviorText(bool setWithLink = false)
	{
		if (LeaderParty == MobileParty.MainParty)
		{
			MainPartyCurrentAction mainPartyCurrentAction = GetMainPartyCurrentAction();
			TextObject variable = ((!setWithLink) ? PlayerEncounter.EncounterSettlement?.Name : PlayerEncounter.EncounterSettlement?.EncyclopediaLinkWithName);
			TextObject textObject;
			switch (mainPartyCurrentAction)
			{
			case MainPartyCurrentAction.Idle:
				return new TextObject("{=sBahcJcl}Idle.");
			case MainPartyCurrentAction.GatherAroundHero:
				textObject = GameTexts.FindText("str_army_gathering_around_hero");
				textObject.SetTextVariable("PARTY_NAME", MobileParty.MainParty.Name);
				break;
			case MainPartyCurrentAction.GatherAroundSettlement:
				textObject = GameTexts.FindText("str_army_gathering");
				break;
			case MainPartyCurrentAction.GoToSettlement:
				textObject = ((Settlement.CurrentSettlement == null) ? GameTexts.FindText("str_army_going_to_settlement") : GameTexts.FindText("str_army_waiting_in_settlement"));
				break;
			case MainPartyCurrentAction.RaidSettlement:
				textObject = GameTexts.FindText("str_army_raiding_settlement");
				textObject.SetTextVariable("RAIDING_PROCESS", (int)(100f * (1f - PlayerEncounter.EncounterSettlement?.SettlementHitPoints)).Value);
				break;
			case MainPartyCurrentAction.BesiegeSettlement:
				textObject = GameTexts.FindText("str_army_besieging_settlement");
				break;
			case MainPartyCurrentAction.PatrolAroundSettlement:
			{
				Settlement settlement = null;
				float num = Campaign.MapDiagonalSquared;
				foreach (Settlement item in Settlement.All)
				{
					if (!item.IsHideout)
					{
						float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(MobileParty.MainParty, item);
						if (distance < num)
						{
							num = distance;
							settlement = item;
						}
					}
				}
				variable = (setWithLink ? settlement.EncyclopediaLinkWithName : settlement.Name);
				textObject = GameTexts.FindText("str_army_patrolling_travelling");
				break;
			}
			case MainPartyCurrentAction.DefendingSettlement:
				textObject = GameTexts.FindText("str_army_defending");
				textObject.SetTextVariable("SETTLEMENT_NAME", variable);
				break;
			default:
				return new TextObject("{=av14a64q}Thinking");
			}
			textObject.SetTextVariable("SETTLEMENT_NAME", variable);
			return textObject;
		}
		float distance2;
		switch (AIBehavior)
		{
		case AIBehaviorFlags.PreGathering:
		case AIBehaviorFlags.Gathering:
		case AIBehaviorFlags.WaitingForArmyMembers:
		{
			TextObject textObject;
			if (LeaderParty != MobileParty.MainParty)
			{
				textObject = GameTexts.FindText("str_army_gathering");
				textObject.SetTextVariable("SETTLEMENT_NAME", setWithLink ? ((Settlement)AiBehaviorObject).EncyclopediaLinkWithName : AiBehaviorObject.Name);
			}
			else
			{
				textObject = GameTexts.FindText("str_army_gathering_around_hero");
				textObject.SetTextVariable("PARTY_NAME", MobileParty.MainParty.Name);
			}
			return textObject;
		}
		case AIBehaviorFlags.GoToSettlement:
		{
			TextObject textObject = ((LeaderParty.CurrentSettlement == null) ? GameTexts.FindText("str_army_going_to_settlement") : GameTexts.FindText("str_army_waiting_in_settlement"));
			textObject.SetTextVariable("SETTLEMENT_NAME", (setWithLink && AiBehaviorObject is Settlement) ? ((Settlement)AiBehaviorObject).EncyclopediaLinkWithName : (AiBehaviorObject.Name ?? LeaderParty.Ai.AiBehaviorPartyBase.Name));
			return textObject;
		}
		case AIBehaviorFlags.TravellingToAssignment:
		{
			TextObject textObject;
			if (LeaderParty.MapEvent != null && LeaderParty.MapEvent.MapEventSettlement != null && AiBehaviorObject != null && LeaderParty.MapEvent.MapEventSettlement == AiBehaviorObject)
			{
				switch (ArmyType)
				{
				case ArmyTypes.Besieger:
					textObject = GameTexts.FindText("str_army_besieging_settlement");
					break;
				case ArmyTypes.Raider:
				{
					Settlement settlement2 = (Settlement)AiBehaviorObject;
					textObject = GameTexts.FindText("str_army_raiding_settlement");
					textObject.SetTextVariable("RAIDING_PROCESS", (int)(100f * (1f - settlement2.SettlementHitPoints)));
					break;
				}
				case ArmyTypes.Patrolling:
					textObject = GameTexts.FindText("str_army_patrolling_travelling");
					break;
				case ArmyTypes.Defender:
					textObject = GameTexts.FindText("str_army_defending_travelling");
					break;
				default:
					return new TextObject("{=av14a64q}Thinking");
				}
				textObject.SetTextVariable("SETTLEMENT_NAME", setWithLink ? ((Settlement)AiBehaviorObject).EncyclopediaLinkWithName : AiBehaviorObject.Name);
				return textObject;
			}
			switch (ArmyType)
			{
			case ArmyTypes.Besieger:
				textObject = GameTexts.FindText("str_army_besieging_travelling");
				break;
			case ArmyTypes.Raider:
				textObject = GameTexts.FindText("str_army_raiding_travelling");
				break;
			case ArmyTypes.Patrolling:
				textObject = GameTexts.FindText("str_army_patrolling_travelling");
				break;
			case ArmyTypes.Defender:
				textObject = GameTexts.FindText("str_army_defending_travelling");
				break;
			default:
				return new TextObject("{=av14a64q}Thinking");
			}
			textObject.SetTextVariable("SETTLEMENT_NAME", setWithLink ? ((Settlement)AiBehaviorObject).EncyclopediaLinkWithName : AiBehaviorObject.Name);
			return textObject;
		}
		case AIBehaviorFlags.Besieging:
		{
			TextObject textObject = ((!Campaign.Current.Models.MapDistanceModel.GetDistance(AiBehaviorObject, MobileParty.MainParty, 15f, out distance2)) ? GameTexts.FindText("str_army_besieging_travelling") : GameTexts.FindText("str_army_besieging"));
			Settlement settlement3 = (Settlement)AiBehaviorObject;
			if (settlement3.IsVillage)
			{
				textObject = GameTexts.FindText("str_army_patrolling_travelling");
			}
			textObject.SetTextVariable("SETTLEMENT_NAME", setWithLink ? settlement3.EncyclopediaLinkWithName : AiBehaviorObject.Name);
			return textObject;
		}
		case AIBehaviorFlags.Raiding:
		{
			TextObject textObject = ((!Campaign.Current.Models.MapDistanceModel.GetDistance(AiBehaviorObject, MobileParty.MainParty, 15f, out distance2)) ? GameTexts.FindText("str_army_raiding_travelling") : GameTexts.FindText("str_army_raiding"));
			textObject.SetTextVariable("SETTLEMENT_NAME", setWithLink ? ((Settlement)AiBehaviorObject).EncyclopediaLinkWithName : AiBehaviorObject.Name);
			return textObject;
		}
		case AIBehaviorFlags.Defending:
		{
			TextObject textObject = ((!Campaign.Current.Models.MapDistanceModel.GetDistance(AiBehaviorObject, MobileParty.MainParty, 15f, out distance2)) ? GameTexts.FindText("str_army_defending_travelling") : GameTexts.FindText("str_army_defending"));
			textObject.SetTextVariable("SETTLEMENT_NAME", setWithLink ? ((Settlement)AiBehaviorObject).EncyclopediaLinkWithName : AiBehaviorObject.Name);
			return textObject;
		}
		case AIBehaviorFlags.Patrolling:
		{
			TextObject textObject = GameTexts.FindText("str_army_patrolling_travelling");
			textObject.SetTextVariable("SETTLEMENT_NAME", setWithLink ? ((Settlement)AiBehaviorObject).EncyclopediaLinkWithName : AiBehaviorObject.Name);
			return textObject;
		}
		default:
			return new TextObject("{=av14a64q}Thinking");
		}
	}

	public void Gather(Settlement initialHostileSettlement)
	{
		_armyGatheringTime = Campaign.CurrentTime;
		if (LeaderParty != MobileParty.MainParty)
		{
			Settlement settlement = (Settlement)(AiBehaviorObject = FindBestInitialGatheringSettlement(initialHostileSettlement));
			Vec2 centerPosition = (settlement.IsFortification ? settlement.GatePosition : settlement.Position2D);
			LeaderParty.SendPartyToReachablePointAroundPosition(centerPosition, 6f, 3f);
			CallPartiesToArmy();
		}
		else
		{
			AiBehaviorObject = SettlementHelper.FindNearestSettlement((Settlement x) => x.IsFortification || x.IsVillage);
		}
		GatherArmyAction.Apply(LeaderParty, (Settlement)AiBehaviorObject);
	}

	private Settlement FindBestInitialGatheringSettlement(Settlement initialHostileTargetSettlement)
	{
		Settlement settlement = null;
		Hero leaderHero = LeaderParty.LeaderHero;
		float num = 0f;
		if (leaderHero != null && leaderHero.IsActive)
		{
			foreach (Settlement settlement2 in Kingdom.Settlements)
			{
				if (!settlement2.IsVillage && !settlement2.IsFortification)
				{
					continue;
				}
				float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(initialHostileTargetSettlement, settlement2);
				if (!(distance > 40f))
				{
					continue;
				}
				float num2 = 0f;
				if (settlement == null)
				{
					num2 += 0.001f;
				}
				if (settlement2 == initialHostileTargetSettlement || settlement2.Party.MapEvent != null)
				{
					continue;
				}
				if (settlement2.MapFaction == Kingdom)
				{
					num2 += 10f;
				}
				else if (!FactionManager.IsAtWarAgainstFaction(settlement2.MapFaction, Kingdom))
				{
					num2 += 2f;
				}
				bool flag = false;
				foreach (Army army in Kingdom.Armies)
				{
					if (army != this && army.AiBehaviorObject == settlement2)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					num2 += 10f;
				}
				float num3 = distance / (Campaign.MapDiagonal * 0.1f);
				float num4 = 20f * (1f - num3);
				float num5 = (settlement2.Position2D - LeaderParty.Position2D).Length / (Campaign.MapDiagonal * 0.1f);
				float num6 = 5f * (1f - num5);
				float num7 = num2 + num4 * 0.5f + num6 * 0.1f;
				if (num7 < 0f)
				{
					num7 = 0f;
				}
				if (num7 > num)
				{
					num = num7;
					settlement = settlement2;
				}
			}
		}
		else
		{
			settlement = (Settlement)AiBehaviorObject;
		}
		if (settlement == null)
		{
			settlement = Kingdom.Settlements.FirstOrDefault() ?? LeaderParty.HomeSettlement;
		}
		return settlement;
	}

	private void CallPartiesToArmy()
	{
		foreach (MobileParty item in Campaign.Current.Models.ArmyManagementCalculationModel.GetMobilePartiesToCallToArmy(LeaderParty))
		{
			SetPartyAiAction.GetActionForEscortingParty(item, LeaderParty);
		}
	}

	public void ThinkAboutConcludingArmyGathering()
	{
		float currentTime = Campaign.CurrentTime;
		float num = 0f;
		float num2 = ((ArmyType == ArmyTypes.Defender) ? 1f : 2f);
		float num3 = currentTime - _armyGatheringTime;
		if (num3 > 24f)
		{
			num = 1f * ((num3 - 24f) / (num2 * 24f));
		}
		else if (num3 > (num2 + 1f) * 24f)
		{
			num = 1f;
		}
		if (MBRandom.RandomFloat < num)
		{
			_waitTimeStart = Campaign.CurrentTime;
			AIBehavior = AIBehaviorFlags.WaitingForArmyMembers;
			if (Parties.Count <= 1)
			{
				DisbandArmyAction.ApplyByNotEnoughParty(this);
			}
		}
	}

	public void ThinkAboutTravelingToAssignment()
	{
		bool flag = false;
		if (Campaign.CurrentTime - _waitTimeStart < 72f)
		{
			if (LeaderParty.Position2D.DistanceSquared(AiBehaviorObject.Position2D) < 100f)
			{
				flag = TotalStrength / Parties.SumQ((MobileParty x) => x.Party.TotalStrength) > 0.7f;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			AIBehavior = AIBehaviorFlags.TravellingToAssignment;
		}
	}

	private bool AnyoneBesiegingTarget()
	{
		Settlement settlement = (Settlement)AiBehaviorObject;
		if (ArmyType == ArmyTypes.Besieger && settlement.IsUnderSiege)
		{
			return !settlement.SiegeEvent.BesiegerCamp.IsBesiegerSideParty(LeaderParty);
		}
		return false;
	}

	private void IsAtSiegeLocation()
	{
		if (LeaderParty.Position2D.DistanceSquared(AiBehaviorObject.Position2D) < 100f && AIBehavior != AIBehaviorFlags.Besieging)
		{
			if (LeaderParty.Army.Parties.ContainsQ(MobileParty.MainParty))
			{
				Debug.Print(string.Concat(LeaderParty.LeaderHero.StringId, ": ", LeaderParty.LeaderHero.Name, " is besieging ", AiBehaviorObject.Name, " of ", AiBehaviorObject.MapFaction.StringId, ": ", AiBehaviorObject.MapFaction.Name, "\n"), 0, Debug.DebugColor.Cyan);
			}
			AIBehavior = AIBehaviorFlags.Besieging;
		}
	}

	public void FinishArmyObjective()
	{
		AIBehavior = AIBehaviorFlags.Unassigned;
		AiBehaviorObject = null;
	}

	internal void DisperseInternal(ArmyDispersionReason reason = ArmyDispersionReason.Unknown)
	{
		if (_armyIsDispersing)
		{
			return;
		}
		CampaignEventDispatcher.Instance.OnArmyDispersed(this, reason, Parties.Contains(MobileParty.MainParty));
		_armyIsDispersing = true;
		int num = 0;
		for (int num2 = Parties.Count - 1; num2 >= num; num2--)
		{
			MobileParty mobileParty = Parties[num2];
			bool num3 = mobileParty.AttachedTo == LeaderParty;
			mobileParty.Army = null;
			if (num3 && mobileParty.CurrentSettlement == null && mobileParty.IsActive)
			{
				mobileParty.Position2D = MobilePartyHelper.FindReachablePointAroundPosition(LeaderParty.Position2D, 1f);
			}
		}
		_parties.Clear();
		Kingdom = null;
		if (LeaderParty == MobileParty.MainParty && Game.Current.GameStateManager.ActiveState is MapState mapState)
		{
			mapState.OnDispersePlayerLeadedArmy();
		}
		_hourlyTickEvent.DeletePeriodicEvent();
		_tickEvent.DeletePeriodicEvent();
		_armyIsDispersing = false;
	}

	public Vec2 GetRelativePositionForParty(MobileParty mobileParty, Vec2 armyFacing)
	{
		float num = 0.5f;
		float num2 = (float)TaleWorlds.Library.MathF.Ceiling(-1f + TaleWorlds.Library.MathF.Sqrt(1f + 8f * (float)(LeaderParty.AttachedParties.Count - 1))) / 4f * num * 0.5f + num;
		int num3 = -1;
		for (int i = 0; i < LeaderParty.AttachedParties.Count; i++)
		{
			if (LeaderParty.AttachedParties[i] == mobileParty)
			{
				num3 = i;
				break;
			}
		}
		int num4 = TaleWorlds.Library.MathF.Ceiling((-1f + TaleWorlds.Library.MathF.Sqrt(1f + 8f * (float)(num3 + 2))) / 2f) - 1;
		int num5 = num3 + 1 - num4 * (num4 + 1) / 2;
		bool flag = (num4 & 1) != 0;
		num5 = (((((uint)num5 & (true ? 1u : 0u)) != 0) ? (-1 - num5) : num5) >> 1) * ((!flag) ? 1 : (-1));
		float num6 = 1.25f;
		Vec2 vec = LeaderParty.VisualPosition2DWithoutError + -armyFacing * 0.1f * LeaderParty.AttachedParties.Count;
		Vec2 vec2 = vec - TaleWorlds.Library.MathF.Sign((float)num5 - ((((uint)num4 & (true ? 1u : 0u)) != 0) ? 0.5f : 0f)) * armyFacing.LeftVec() * num2;
		PathFaceRecord faceIndex = Campaign.Current.MapSceneWrapper.GetFaceIndex(vec);
		if (vec != vec2)
		{
			Vec2 lastPointOnNavigationMeshFromPositionToDestination = Campaign.Current.MapSceneWrapper.GetLastPointOnNavigationMeshFromPositionToDestination(faceIndex, vec, vec2);
			if ((vec2 - lastPointOnNavigationMeshFromPositionToDestination).LengthSquared > 2.25E-06f)
			{
				num = num * (vec - lastPointOnNavigationMeshFromPositionToDestination).Length / num2;
				num6 = num6 * (vec - lastPointOnNavigationMeshFromPositionToDestination).Length / (num2 / 1.5f);
			}
		}
		return new Vec2((flag ? ((0f - num) * 0.5f) : 0f) + (float)num5 * num + mobileParty.Party.RandomFloat(-0.25f, 0.25f) * 0.6f * num, ((float)(-num4) + mobileParty.Party.RandomFloatWithSeed(1u, -0.25f, 0.25f)) * num6 * 0.3f);
	}

	public void AddPartyToMergedParties(MobileParty mobileParty)
	{
		mobileParty.AttachedTo = LeaderParty;
		if (mobileParty.IsMainParty)
		{
			(GameStateManager.Current.ActiveState as MapState)?.OnJoinArmy();
			Hero leaderHero = LeaderParty.LeaderHero;
			if (leaderHero != null && leaderHero != Hero.MainHero && !leaderHero.HasMet)
			{
				leaderHero.SetHasMet();
			}
		}
	}

	internal void OnRemovePartyInternal(MobileParty mobileParty)
	{
		mobileParty.Ai.SetInitiative(1f, 1f, 24f);
		_parties.Remove(mobileParty);
		CampaignEventDispatcher.Instance.OnPartyRemovedFromArmy(mobileParty);
		if (this == MobileParty.MainParty.Army)
		{
			CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
		}
		mobileParty.AttachedTo = null;
		if (LeaderParty == mobileParty && !_armyIsDispersing)
		{
			DisbandArmyAction.ApplyByLeaderPartyRemoved(this);
		}
		mobileParty.OnRemovedFromArmyInternal();
		if (mobileParty == MobileParty.MainParty)
		{
			Campaign.Current.CameraFollowParty = MobileParty.MainParty.Party;
			StopTrackingTargetSettlement();
		}
		if (mobileParty.Army?.LeaderParty == mobileParty)
		{
			FinishArmyObjective();
			if (!_armyIsDispersing)
			{
				if (mobileParty.Army?.LeaderParty.LeaderHero == null)
				{
					DisbandArmyAction.ApplyByArmyLeaderIsDead(mobileParty.Army);
				}
				else
				{
					DisbandArmyAction.ApplyByObjectiveFinished(mobileParty.Army);
				}
			}
		}
		else if (Parties.Count == 0 && !_armyIsDispersing)
		{
			if (mobileParty.Army != null && MobileParty.MainParty.Army != null && mobileParty.Army == MobileParty.MainParty.Army && Hero.MainHero.IsPrisoner)
			{
				DisbandArmyAction.ApplyByPlayerTakenPrisoner(this);
			}
			else
			{
				DisbandArmyAction.ApplyByNotEnoughParty(this);
			}
		}
		mobileParty.Party.SetVisualAsDirty();
		mobileParty.Party.UpdateVisibilityAndInspected();
	}

	internal void OnAddPartyInternal(MobileParty mobileParty)
	{
		_parties.Add(mobileParty);
		CampaignEventDispatcher.Instance.OnPartyJoinedArmy(mobileParty);
		if (this == MobileParty.MainParty.Army && LeaderParty != MobileParty.MainParty)
		{
			StartTrackingTargetSettlement(AiBehaviorObject);
			CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
		}
		if (mobileParty != MobileParty.MainParty && LeaderParty != MobileParty.MainParty && LeaderParty.LeaderHero != null)
		{
			int num = -Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(LeaderParty, mobileParty);
			ChangeClanInfluenceAction.Apply(LeaderParty.LeaderHero.Clan, num);
		}
	}

	private void StartTrackingTargetSettlement(IMapPoint targetObject)
	{
		if (targetObject is Settlement obj)
		{
			Campaign.Current.VisualTrackerManager.RegisterObject(obj);
		}
	}

	private void StopTrackingTargetSettlement()
	{
		if (AiBehaviorObject is Settlement obj)
		{
			Campaign.Current.VisualTrackerManager.RemoveTrackedObject(obj);
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsArmy(object o, List<object> collectedObjects)
	{
		((Army)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected virtual void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		collectedObjects.Add(_parties);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(_creationTime, collectedObjects);
		collectedObjects.Add(_kingdom);
		collectedObjects.Add(_aiBehaviorObject);
		collectedObjects.Add(ArmyOwner);
		collectedObjects.Add(LeaderParty);
		collectedObjects.Add(Name);
	}

	internal static object AutoGeneratedGetMemberValueAIBehavior(object o)
	{
		return ((Army)o).AIBehavior;
	}

	internal static object AutoGeneratedGetMemberValueArmyType(object o)
	{
		return ((Army)o).ArmyType;
	}

	internal static object AutoGeneratedGetMemberValueArmyOwner(object o)
	{
		return ((Army)o).ArmyOwner;
	}

	internal static object AutoGeneratedGetMemberValueCohesion(object o)
	{
		return ((Army)o).Cohesion;
	}

	internal static object AutoGeneratedGetMemberValueMorale(object o)
	{
		return ((Army)o).Morale;
	}

	internal static object AutoGeneratedGetMemberValueLeaderParty(object o)
	{
		return ((Army)o).LeaderParty;
	}

	internal static object AutoGeneratedGetMemberValueName(object o)
	{
		return ((Army)o).Name;
	}

	internal static object AutoGeneratedGetMemberValue_parties(object o)
	{
		return ((Army)o)._parties;
	}

	internal static object AutoGeneratedGetMemberValue_creationTime(object o)
	{
		return ((Army)o)._creationTime;
	}

	internal static object AutoGeneratedGetMemberValue_armyGatheringTime(object o)
	{
		return ((Army)o)._armyGatheringTime;
	}

	internal static object AutoGeneratedGetMemberValue_waitTimeStart(object o)
	{
		return ((Army)o)._waitTimeStart;
	}

	internal static object AutoGeneratedGetMemberValue_armyIsDispersing(object o)
	{
		return ((Army)o)._armyIsDispersing;
	}

	internal static object AutoGeneratedGetMemberValue_numberOfBoosts(object o)
	{
		return ((Army)o)._numberOfBoosts;
	}

	internal static object AutoGeneratedGetMemberValue_kingdom(object o)
	{
		return ((Army)o)._kingdom;
	}

	internal static object AutoGeneratedGetMemberValue_aiBehaviorObject(object o)
	{
		return ((Army)o)._aiBehaviorObject;
	}
}
