using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class CompanionGrievanceBehavior : CampaignBehaviorBase
{
	public class CompanionGrievanceBehaviorTypeDefiner : SaveableTypeDefiner
	{
		public CompanionGrievanceBehaviorTypeDefiner()
			: base(80000)
		{
		}

		protected override void DefineEnumTypes()
		{
			AddEnumDefinition(typeof(GrievanceType), 1);
		}

		protected override void DefineClassTypes()
		{
			AddClassDefinition(typeof(Grievance), 10);
		}

		protected override void DefineContainerDefinitions()
		{
			ConstructContainerDefinition(typeof(Dictionary<Hero, Grievance>));
		}
	}

	internal class Grievance
	{
		[SaveableField(1)]
		public Hero GrievingHero;

		[SaveableField(2)]
		public CampaignTime NextGrievanceTime;

		[SaveableField(3)]
		public GrievanceType TypeOfGrievance;

		[SaveableField(5)]
		public bool HasBeenSettled;

		[SaveableField(6)]
		public int Count;

		[SaveableProperty(4)]
		public bool HaveGrievance { get; set; }

		public Grievance(Hero hero, CampaignTime time, GrievanceType type)
		{
			GrievingHero = hero;
			NextGrievanceTime = time;
			TypeOfGrievance = type;
			HasBeenSettled = false;
			Count = 1;
			HaveGrievance = true;
		}

		internal static void AutoGeneratedStaticCollectObjectsGrievance(object o, List<object> collectedObjects)
		{
			((Grievance)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected virtual void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			collectedObjects.Add(GrievingHero);
			CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(NextGrievanceTime, collectedObjects);
		}

		internal static object AutoGeneratedGetMemberValueHaveGrievance(object o)
		{
			return ((Grievance)o).HaveGrievance;
		}

		internal static object AutoGeneratedGetMemberValueGrievingHero(object o)
		{
			return ((Grievance)o).GrievingHero;
		}

		internal static object AutoGeneratedGetMemberValueNextGrievanceTime(object o)
		{
			return ((Grievance)o).NextGrievanceTime;
		}

		internal static object AutoGeneratedGetMemberValueTypeOfGrievance(object o)
		{
			return ((Grievance)o).TypeOfGrievance;
		}

		internal static object AutoGeneratedGetMemberValueHasBeenSettled(object o)
		{
			return ((Grievance)o).HasBeenSettled;
		}

		internal static object AutoGeneratedGetMemberValueCount(object o)
		{
			return ((Grievance)o).Count;
		}
	}

	internal enum GrievanceType
	{
		Invalid,
		NoWage,
		Starvation,
		VillageRaided,
		DesertedBattle
	}

	private Dictionary<Hero, Grievance> _heroGrievances = new Dictionary<Hero, Grievance>();

	private CampaignTime[] _nextGrievableTimeForComplaintType = new CampaignTime[Enum.GetValues(typeof(GrievanceType)).Length];

	private Grievance _currentGrievance;

	private const float _baseGrievanceFrequencyInHours = 100f;

	private const float _grievanceObsolescenceDurationInWeeks = 8f;

	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
		CampaignEvents.VillageLooted.AddNonSerializedListener(this, OnVillageRaided);
		CampaignEvents.PlayerDesertedBattleEvent.AddNonSerializedListener(this, OnPlayerDesertedBattle);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_heroGrievances", ref _heroGrievances);
		dataStore.SyncData("_currentGrievance", ref _currentGrievance);
		dataStore.SyncData("_nextGrievableTimeForComplaintType", ref _nextGrievableTimeForComplaintType);
	}

	private void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		campaignGameStarter.AddDialogLine("companion_start_grievance", "start", "grievance_received", "{=PVyZ9vNB}{TITLE}, there is something I wish to discuss with you.", companion_start_grievance_condition, null, 120);
		campaignGameStarter.AddPlayerLine("grievance_requested", "grievance_received", "grievance_noticed", "{=m72wpzG2}Go on, I'm listening.", null, null);
		campaignGameStarter.AddDialogLineWithVariation("companion_continue_grievance_desert_softspoken", "grievance_noticed", "grievance_listened", companion_grievance_desert_battle_condition, null).Variation("{=7ldEMGn6}I don't like running away from a battle like that.", "DefaultTag", 0).Variation("{=EqLQtlca}The way we just ran from the enemy back there... I don't want to get a name for being a coward.", "PersonaSoftspokenTag", 1)
			.Variation("{=sXwOLNo9}I don't like turning my back on the enemy like that. For me, death in battle is better than dishonor.", "PersonaCurtTag", 1)
			.Variation("{=TnW3i5ul}We ran back there. I despise running. I prefer to be the wolf, not a rabbit.", "PersonaCurtTag", 1, "KhuzaitTag", 1)
			.Variation("{=IDJbtGks}We ran back there. It is shameful to turn your back on the enemy.", "PersonaCurtTag", 1, "SturgianTag", 1)
			.Variation("{=PTj1WtJx}The way we ran back there... It shames me to think of it. Next time, let us fight and die rather than let men call us cowards!", "PersonaEarnestTag", 1)
			.Variation("{=UTRvswWE}As I recall, when you first hired me, it was to fight, not run away. Now, I'm sure what you did back there was sensible, but still, I've earned a bit of a reputation for bravery and I don't care to be called a coward. Those foes we can't beat - let's try to stay a little more clear of them next time, shall we?", "PersonaIronicTag", 1)
			.Variation("{=v7OCHday}I suppose back there we had to run away to fight another day, as the hero Cathalac once did. If you remember the story, though, for the next three years he sat by himself in a bog, unable to look anyone else in the eye. So let's not do that too often, shall we?", "PersonaIronicTag", 1, "BattanianTag", 1)
			.Variation("{=u9tAQLUf}We ran away back there. I hope word does not get around. Not looking forward to seeing the snickers and grins on people's faces the next time we walk into a tavern. Stings worse than arrows, that does.", "PersonaIronicTag", 1, "VlandianTag", 1)
			.Variation("{=gfoqoGTn}The way we ran away back there... I may have told you that I hoped one day the poets would write odes about me. I had intended that they would praise my heroism, not my ability to scamper to safety.", "PersonaIronicTag", 1, "AseraiTag", 1);
		campaignGameStarter.AddDialogLineWithVariation("companion_continue_grievance_wage_softspoken", "grievance_noticed", "grievance_listened", companion_grievance_wage_condition, null).Variation("{=yuqLzmL9}I should remind you that I expect to be paid as you had promised.", "DefaultTag", 1).Variation("{=zBfB5vw8}I hope you don't mind me saying this, but... Your men shed their blood for you. It bothers me to hear that their wages are late.", "PersonaSoftspokenTag", 1)
			.Variation("{=Bhtt6XPv}Your men's wages are late. That's not the kind of company I like to fight in.", "PersonaCurtTag", 1)
			.Variation("{=0mVwnCES}I must say something. Your men say their wages are late. We should take care that this doesn't happen.", "PersonaEarnestTag", 1)
			.Variation("{=TzqxgiQl}The men say their wages are late. Best uphold your end of the bargain with them, or they might not keep theirs. It would especially be tricky if they chose to void their contract during a battle, wouldn't you say?", "PersonaIronicTag", 1);
		campaignGameStarter.AddDialogLineWithVariation("companion_continue_grievance_starve_softspoken", "grievance_noticed", "grievance_listened", companion_grievance_starve_condition, null).Variation("{=IPLyqdVX}I hear we're running low on food. We should watch our stocks better.", "DefaultTag", 1).Variation("{=ITboR6C1}The men say we're running low on food. We should be more careful of that.", "PersonaSoftspokenTag", 1)
			.Variation("{=HkjaCc44}Your men say there's little to eat. They march, they fight. They deserve to eat.", "PersonaCurtTag", 1)
			.Variation("{=acOOsQaC}The food's running out. That's not fair to the men. We should take care that the food doesn't run out.", "PersonaEarnestTag", 1)
			.Variation("{=6UKdUrPs}The men say the food's running out. We expect them to die for us if needed. Least we can do is let them die on a full belly.", "PersonaIronicTag", 1)
			.Variation("{=gChji1JO}About the food... These men are ready to spill their blood for you, but there won't be much blood in their veins to shed if their bellies are empty.", "PersonaIronicTag", 1, "BattanianTag", 1)
			.Variation("{=IgGQUms4}About our food situation... The general Aricaros used to say that an army marches on its stomach. Can't get far on an empty one.", "PersonaIronicTag", 1, "EmpireTag", 1)
			.Variation("{=ZNaQrIaP}About our food situation... We shouldn't let the men go hungry. A man's courage comes from his stomach, they say.", "PersonaIronicTag", 1, "PersonaIronicTag", 1);
		campaignGameStarter.AddDialogLineWithVariation("companion_continue_grievance_raid_softspoken", "grievance_noticed", "grievance_listened", companion_grievance_raid_condition, null).Variation("{=zNvjSFaC}Pillaging villages is not what I signed up for.", "DefaultTag", 1).Variation("{=bpXgcBCp}What we did to that village... I don't like it. Those farmers, they're a lot like my people. I want to know it won't happen again.", "PersonaSoftspokenTag", 1)
			.Variation("{=4bkLDxIU}What we did back there, to that village... I don't do that. I want no part of it.", "PersonaCurtTag", 1)
			.Variation("{=VldAzBo5}I need to say something. What we did to that village - it was wrong. They're innocent farmers and they shouldn't have their homes and fields ransacked and burned like that. I won't do that again.", "PersonaEarnestTag", 1)
			.Variation("{=pDa7kOja}I know war is cruel, but I don't want to make it crueler than necessary. I'd rather not have the blood of innocents on my conscience, if you don't mind. Let's not raid villages like that.", "PersonaIronicTag", 1);
		campaignGameStarter.AddPlayerLine("grievance_1", "grievance_listened", "close_window", "{=OVeSBrhv}Very well, I will consider this when taking such actions.", null, companion_grievance_accepted_consequence);
		campaignGameStarter.AddPlayerLine("grievance_2", "grievance_listened", "close_window", "{=2wmKs6Is}As your leader I am able to decide the best course of action.", null, companion_grievance_consequence);
		campaignGameStarter.AddPlayerLine("grievance_3", "grievance_listened", "close_window", "{=fzKFQuFT}Perhaps you are not suitable for this party after all.", null, companion_grievance_rejected_consequence);
		campaignGameStarter.AddDialogLine("companion_repeat_grievance", "start", "grievance_repeated", "{=baeO5Zkk}{TITLE}... {GRIEVANCE_SHORT_DESCRIPTION}", companion_repeat_grievance_condition, null, 120);
		campaignGameStarter.AddDialogLine("companion_grievance_repetition_desert", "grievance_repeated", "close_window", "{=!}{GRIEVANCE_REPETITION}", companion_grievance_desert_battle_condition, companion_grievance_consequence);
		campaignGameStarter.AddDialogLine("companion_grievance_repetition_wage", "grievance_repeated", "close_window", "{=!}{GRIEVANCE_REPETITION}", companion_grievance_wage_condition, companion_grievance_consequence);
		campaignGameStarter.AddDialogLine("companion_grievance_repetition_starve", "grievance_repeated", "close_window", "{=!}{GRIEVANCE_REPETITION}", companion_grievance_starve_condition, companion_grievance_consequence);
		campaignGameStarter.AddDialogLine("companion_grievance_repetition_raid", "grievance_repeated", "close_window", "{=!}{GRIEVANCE_REPETITION}", companion_grievance_raid_condition, companion_grievance_consequence);
	}

	private bool companion_grievance_raid_condition()
	{
		if (_currentGrievance != null)
		{
			return _currentGrievance.TypeOfGrievance == GrievanceType.VillageRaided;
		}
		return false;
	}

	private bool companion_grievance_starve_condition()
	{
		if (_currentGrievance != null)
		{
			return _currentGrievance.TypeOfGrievance == GrievanceType.Starvation;
		}
		return false;
	}

	private bool companion_grievance_desert_battle_condition()
	{
		if (_currentGrievance != null)
		{
			return _currentGrievance.TypeOfGrievance == GrievanceType.DesertedBattle;
		}
		return false;
	}

	private bool companion_grievance_wage_condition()
	{
		if (_currentGrievance != null)
		{
			return _currentGrievance.TypeOfGrievance == GrievanceType.NoWage;
		}
		return false;
	}

	private bool companion_start_grievance_condition()
	{
		MBTextManager.SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"));
		if (_currentGrievance != null && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == _currentGrievance.GrievingHero && _currentGrievance.Count <= 1)
		{
			return !_currentGrievance.HasBeenSettled;
		}
		return false;
	}

	private bool companion_repeat_grievance_condition()
	{
		if (_currentGrievance != null && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == _currentGrievance.GrievingHero)
		{
			MBTextManager.SetTextVariable("TITLE", ConversationHelper.HeroRefersToHero(Hero.OneToOneConversationHero, Hero.MainHero, uppercaseFirst: true));
			MBTextManager.SetTextVariable("GRIEVANCE_SHORT_DESCRIPTION", "{=scJ2eVhS}What I said to you before... [default, should not appear]");
			if (_currentGrievance.TypeOfGrievance == GrievanceType.DesertedBattle)
			{
				MBTextManager.SetTextVariable("GRIEVANCE_SHORT_DESCRIPTION", "{=1G5M9nn2}What I mentioned about running from battle...");
			}
			else if (_currentGrievance.TypeOfGrievance == GrievanceType.NoWage)
			{
				MBTextManager.SetTextVariable("GRIEVANCE_SHORT_DESCRIPTION", "{=p78FaTqe}What I said about our wages being paid on time...");
			}
			else if (_currentGrievance.TypeOfGrievance == GrievanceType.Starvation)
			{
				MBTextManager.SetTextVariable("GRIEVANCE_SHORT_DESCRIPTION", "{=zfPQlDbQ}What I said about our food...");
			}
			else if (_currentGrievance.TypeOfGrievance == GrievanceType.VillageRaided)
			{
				MBTextManager.SetTextVariable("GRIEVANCE_SHORT_DESCRIPTION", "{=pQmUIjOQ}What I said about raiding villagers...");
			}
			MBTextManager.SetTextVariable("GRIEVANCE_REPETITION", "{=qNSOb7pJ}Once again, this is not something I'm happy with...");
			if (Hero.OneToOneConversationHero.CharacterObject.GetPersona() == DefaultTraits.PersonaEarnest)
			{
				MBTextManager.SetTextVariable("GRIEVANCE_REPETITION", "{=YWu5Xfgz}I don't feel you're taking my complaint seriously.");
			}
			else if (Hero.OneToOneConversationHero.CharacterObject.GetPersona() == DefaultTraits.PersonaIronic)
			{
				MBTextManager.SetTextVariable("GRIEVANCE_REPETITION", "{=wScKLt7F}Let me put things this way... It's not grown on me at all since the last time it happened.");
			}
			else if (Hero.OneToOneConversationHero.CharacterObject.GetPersona() == DefaultTraits.PersonaCurt)
			{
				MBTextManager.SetTextVariable("GRIEVANCE_REPETITION", "{=dpzbyUCa}I don't care for it any more than I did before.");
			}
			if (Hero.OneToOneConversationHero == _currentGrievance.GrievingHero)
			{
				return _currentGrievance.Count > 1;
			}
			return false;
		}
		return false;
	}

	private void companion_grievance_accepted_consequence()
	{
		Grievance value = _heroGrievances.FirstOrDefault((KeyValuePair<Hero, Grievance> t) => t.Value == _currentGrievance && t.Key == _currentGrievance.GrievingHero).Value;
		if (value != null)
		{
			value.HasBeenSettled = true;
			if (value.Count <= 1)
			{
				ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, _currentGrievance.GrievingHero, 10);
			}
		}
		_currentGrievance = null;
	}

	private void companion_grievance_consequence()
	{
		if (_currentGrievance.Count > 1)
		{
			int relationChange = (_currentGrievance.HasBeenSettled ? (-5) : (-2));
			ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, _currentGrievance.GrievingHero, relationChange);
		}
		_currentGrievance = null;
	}

	private void companion_grievance_rejected_consequence()
	{
		ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, _currentGrievance.GrievingHero, -15);
		_currentGrievance = null;
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
	}

	private void OnHourlyTick()
	{
		foreach (KeyValuePair<Hero, Grievance> heroGrievance in _heroGrievances)
		{
			Grievance value = heroGrievance.Value;
			if (value.GrievingHero.PartyBelongedTo == MobileParty.MainParty && value.HaveGrievance && GameStateManager.Current.ActiveState is MapState && MobileParty.MainParty.MapEvent == null && PlayerEncounter.Current == null)
			{
				_currentGrievance = value;
				CampaignMapConversation.OpenConversation(new ConversationCharacterData(Hero.MainHero.CharacterObject), new ConversationCharacterData(value.GrievingHero.CharacterObject));
				value.HaveGrievance = false;
				value.NextGrievanceTime = CampaignTime.HoursFromNow(100f + (float)MBRandom.RandomInt(100));
				break;
			}
		}
	}

	private void DecideCompanionGrievances(GrievanceType eventType)
	{
		if (_nextGrievableTimeForComplaintType[(int)eventType].IsFuture)
		{
			return;
		}
		foreach (Hero item in Hero.MainHero.CompanionsInParty)
		{
			_heroGrievances.TryGetValue(item, out var value);
			if (value == null)
			{
				GrievanceType grievanceTypeForCompanion = GetGrievanceTypeForCompanion(item, eventType);
				if (grievanceTypeForCompanion != 0)
				{
					value = new Grievance(item, CampaignTime.Now, grievanceTypeForCompanion);
					_heroGrievances.Add(item, value);
					_nextGrievableTimeForComplaintType[(int)eventType] = CampaignTime.HoursFromNow(100f);
					break;
				}
			}
			if (value != null && value.TypeOfGrievance == eventType && !value.HaveGrievance && value.NextGrievanceTime.IsPast)
			{
				value.HaveGrievance = true;
				_nextGrievableTimeForComplaintType[(int)eventType] = CampaignTime.HoursFromNow(100f);
				value.Count++;
				break;
			}
		}
	}

	private GrievanceType GetGrievanceTypeForCompanion(Hero companionHero, GrievanceType type)
	{
		if ((type == GrievanceType.DesertedBattle && companionHero.GetTraitLevel(DefaultTraits.Valor) > 0) || (type == GrievanceType.Starvation && companionHero.GetTraitLevel(DefaultTraits.Generosity) > 0) || (type == GrievanceType.NoWage && companionHero.GetTraitLevel(DefaultTraits.Generosity) > 0) || (type == GrievanceType.VillageRaided && companionHero.GetTraitLevel(DefaultTraits.Mercy) > 0))
		{
			return type;
		}
		return GrievanceType.Invalid;
	}

	private void OnPlayerDesertedBattle(int sacrificedMenCount)
	{
		DecideCompanionGrievances(GrievanceType.DesertedBattle);
	}

	private void OnVillageRaided(Village village)
	{
		MapEvent mapEvent = village.Settlement.Party?.MapEvent;
		if (mapEvent == null)
		{
			return;
		}
		foreach (PartyBase involvedParty in mapEvent.InvolvedParties)
		{
			if (involvedParty == PartyBase.MainParty)
			{
				DecideCompanionGrievances(GrievanceType.VillageRaided);
			}
		}
	}

	private void OnDailyTick()
	{
		if (PartyBase.MainParty.IsStarving)
		{
			DecideCompanionGrievances(GrievanceType.Starvation);
		}
		if (MobileParty.MainParty.HasUnpaidWages > 0f)
		{
			DecideCompanionGrievances(GrievanceType.NoWage);
		}
		foreach (KeyValuePair<Hero, Grievance> heroGrievance in _heroGrievances)
		{
			Grievance value = heroGrievance.Value;
			if (value.NextGrievanceTime.ElapsedWeeksUntilNow >= 8f)
			{
				value.HasBeenSettled = false;
				value.Count = 0;
			}
		}
	}
}
