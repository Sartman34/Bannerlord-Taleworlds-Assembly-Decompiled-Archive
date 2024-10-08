using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Issues;

public class LordsNeedsTutorIssueBehavior : CampaignBehaviorBase
{
	public class LordsNeedsTutorIssue : IssueBase
	{
		[SaveableField(1)]
		private Hero _youngHero;

		private const int MinimumRelationToTakeQuest = -10;

		public override bool IsThereAlternativeSolution => false;

		public override bool IsThereLordSolution => false;

		public override TextObject IssueBriefByIssueGiver
		{
			get
			{
				TextObject textObject = new TextObject("{=VtSg9OYK}I've heard good things about you. They say that you are an accomplished warrior. [if:convo_thinking][ib:closed]Some also say an accomplished commander. I have a proposal for you. There is a young lord from my clan. He is an aspiring warrior and I must say he quite admires you. He wants to learn from you. What do you say? Are you willing to take him under your wings for a while? Let's say {QUEST_DURATION} days?");
				textObject.SetTextVariable("QUEST_DURATION", 200);
				return textObject;
			}
		}

		public override TextObject IssueAcceptByPlayer => new TextObject("{=a1n2zCaD}What exactly do you wish from me?");

		public override TextObject IssueQuestSolutionExplanationByIssueGiver => new TextObject("{=JlFfuFEC}How about you take him with you? [if:convo_merry][ib:confident3]Feed and protect him, sure, but don't treat him any differently from your companions. Let him stay with you for a year, and then let him return to us? I will send you a worthwhile gift and, perhaps more valuably, you will gain my lifelong friendship, assuming everything goes well?");

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=iTYoUNdC}All right. I'll take him with me.");

		public override TextObject Title
		{
			get
			{
				TextObject textObject = new TextObject("{=XmGjTZz0}{?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} Needs A Tutor");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject);
				return textObject;
			}
		}

		public override TextObject Description
		{
			get
			{
				TextObject textObject = new TextObject("{=MSr3HRa1}{QUEST_GIVER.NAME}, a {?QUEST_GIVER.GENDER}lady{?}lord{\\?} of the {CLAN}, wants young member of the clan trained in the arts of war.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject);
				textObject.SetTextVariable("CLAN", base.IssueOwner.Clan.Name);
				return textObject;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsLordsNeedsTutorIssue(object o, List<object> collectedObjects)
		{
			((LordsNeedsTutorIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_youngHero);
		}

		internal static object AutoGeneratedGetMemberValue_youngHero(object o)
		{
			return ((LordsNeedsTutorIssue)o)._youngHero;
		}

		public LordsNeedsTutorIssue(Hero issueOwner, Hero youngHero)
			: base(issueOwner, CampaignTime.DaysFromNow(200f))
		{
			_youngHero = youngHero;
		}

		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.ClanInfluence)
			{
				return -0.1f;
			}
			return 0f;
		}

		protected override void OnGameLoad()
		{
		}

		protected override void HourlyTick()
		{
		}

		protected override QuestBase GenerateIssueQuest(string questId)
		{
			return new LordsNeedsTutorIssueQuest(questId, base.IssueOwner, _youngHero);
		}

		public override IssueFrequency GetFrequency()
		{
			return IssueFrequency.Common;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
		{
			relationHero = null;
			skill = null;
			flag = PreconditionFlags.None;
			if (issueGiver.GetRelationWithPlayer() < -10f)
			{
				flag |= PreconditionFlags.Relation;
				relationHero = issueGiver;
			}
			if (Hero.MainHero.IsKingdomLeader)
			{
				flag |= PreconditionFlags.MainHeroIsKingdomLeader;
			}
			if (issueGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				flag |= PreconditionFlags.AtWar;
			}
			if (Clan.PlayerClan.Tier < 2)
			{
				flag |= PreconditionFlags.ClanTier;
			}
			return flag == PreconditionFlags.None;
		}

		public override bool IssueStayAliveConditions()
		{
			if (_youngHero.Age >= 18f && _youngHero.Age < 21f && _youngHero.PartyBelongedTo == null && _youngHero.PartyBelongedToAsPrisoner == null && _youngHero.IsActive && _youngHero.CurrentSettlement != null && _youngHero.Level <= 20 && base.IssueOwner.Clan != Clan.PlayerClan && _youngHero.Clan != Clan.PlayerClan)
			{
				if (_youngHero.Spouse != null)
				{
					return _youngHero.Children.IsEmpty();
				}
				return true;
			}
			return false;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}
	}

	public class LordsNeedsTutorIssueQuest : QuestBase
	{
		private delegate bool ExperienceIncreaseCondition();

		private const int MinimumSkillGainToComment = 15;

		private const int TargetSkillPointDelta = 30;

		[SaveableField(10)]
		private Hero _youngHero;

		[SaveableField(20)]
		private bool _checkForMissionEnd;

		[SaveableField(30)]
		private bool _firstConversationInitialized;

		[SaveableField(40)]
		private bool _questCompletedStartConversation;

		[SaveableField(50)]
		private int _youngHeroBeginningSkillPoints;

		[SaveableField(60)]
		private int _randomForQuestReward;

		private int _targetSkillGain;

		[SaveableField(70)]
		private Dictionary<SkillObject, int> _oldSkillValues;

		private bool _doNotForceYoungHeroOutFromClan;

		private bool _showQuestFailedConversation;

		[SaveableField(80)]
		private JournalLog _startQuestLog;

		public override bool IsRemainingTimeHidden => false;

		public override TextObject Title
		{
			get
			{
				TextObject textObject = new TextObject("{=XmGjTZz0}{?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} Needs A Tutor");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject PlayerStartsQuestLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=Celvao0J}{QUEST_GIVER.LINK} a {?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} from {CLAN} clan has asked you to take a young clan member under your wings and train him for {DURATION} days. You have accepted to train him personally.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				textObject.SetTextVariable("DURATION", 200);
				textObject.SetTextVariable("CLAN", base.QuestGiver.Clan.EncyclopediaLinkWithName);
				return textObject;
			}
		}

		private TextObject SuccessQuestLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=j0QKnNqK}The training of {YOUNG_HERO.LINK} has been completed successfully. Both He and his clan is grateful to you.");
				StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject FailTimedOutQuestLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=taz5cAtw}You failed to train the {YOUNG_HERO.LINK} enough as {QUEST_GIVER.LINK} expected. {?QUEST_GIVER.GENDER}She{?}He{\\?} is disappointed.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject PupilHasDiedLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=abmSQFR5}{YOUNG_HERO.LINK} has died under your care. {QUEST_GIVER.LINK} is furious.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject PupilEscapedLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=dL8196qY}{YOUNG_HERO.LINK} has escaped. Find him to continue your quest.");
				StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject WarDeclaredQuestCancel
		{
			get
			{
				TextObject textObject = new TextObject("{=HkbK8cqw}Your clan is now at war with the {QUEST_GIVER.LINK}'s faction. Your agreement with {QUEST_GIVER.LINK} was terminated.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject PlayerDeclaredWarQuestLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=bqeWVVEE}Your actions have started a war with {QUEST_GIVER.LINK}'s faction. {?QUEST_GIVER.GENDER}She{?}He{\\?} cancels your agreement and the quest is a failure.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject PlayerKickedYoungHeroFromClan
		{
			get
			{
				TextObject textObject = new TextObject("{=x3f1w3Gc}You have kicked {YOUNG_HERO.LINK} from your clan. Your agreement with {QUEST_GIVER.LINK} was terminated.");
				StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject, textObject);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject PlayerBecameClanLeader
		{
			get
			{
				TextObject textObject = new TextObject("{=JiD05yDw}{YOUNG_HERO.LINK} now becomes the leader of his clan and your agreement with {QUEST_GIVER.LINK} was terminated.");
				StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject, textObject);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject PlayerMarriedYoungHero
		{
			get
			{
				TextObject textObject = new TextObject("{=QGigAhFW}You have married with {YOUNG_HERO.LINK} who is the young noble you agreed to be his tutor. {QUEST_GIVER.LINK} no longer expects you to tutor {YOUNG_HERO.LINK} and wishes you a joyful marriage.");
				StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject, textObject);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsLordsNeedsTutorIssueQuest(object o, List<object> collectedObjects)
		{
			((LordsNeedsTutorIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_youngHero);
			collectedObjects.Add(_oldSkillValues);
			collectedObjects.Add(_startQuestLog);
		}

		internal static object AutoGeneratedGetMemberValue_youngHero(object o)
		{
			return ((LordsNeedsTutorIssueQuest)o)._youngHero;
		}

		internal static object AutoGeneratedGetMemberValue_checkForMissionEnd(object o)
		{
			return ((LordsNeedsTutorIssueQuest)o)._checkForMissionEnd;
		}

		internal static object AutoGeneratedGetMemberValue_firstConversationInitialized(object o)
		{
			return ((LordsNeedsTutorIssueQuest)o)._firstConversationInitialized;
		}

		internal static object AutoGeneratedGetMemberValue_questCompletedStartConversation(object o)
		{
			return ((LordsNeedsTutorIssueQuest)o)._questCompletedStartConversation;
		}

		internal static object AutoGeneratedGetMemberValue_youngHeroBeginningSkillPoints(object o)
		{
			return ((LordsNeedsTutorIssueQuest)o)._youngHeroBeginningSkillPoints;
		}

		internal static object AutoGeneratedGetMemberValue_randomForQuestReward(object o)
		{
			return ((LordsNeedsTutorIssueQuest)o)._randomForQuestReward;
		}

		internal static object AutoGeneratedGetMemberValue_oldSkillValues(object o)
		{
			return ((LordsNeedsTutorIssueQuest)o)._oldSkillValues;
		}

		internal static object AutoGeneratedGetMemberValue_startQuestLog(object o)
		{
			return ((LordsNeedsTutorIssueQuest)o)._startQuestLog;
		}

		public LordsNeedsTutorIssueQuest(string questId, Hero questGiver, Hero youngHero)
			: base(questId, questGiver, CampaignTime.DaysFromNow(200f), 0)
		{
			_youngHero = youngHero;
			_firstConversationInitialized = false;
			_oldSkillValues = new Dictionary<SkillObject, int>();
			_youngHero.SetHasMet();
			_randomForQuestReward = MBRandom.RandomInt(2, 5);
			if (_youngHero.GovernorOf != null)
			{
				ChangeGovernorAction.RemoveGovernorOf(_youngHero);
			}
			SetDialogs();
			InitializeQuestOnCreation();
		}

		protected override void InitializeQuestOnGameLoad()
		{
			SetDialogs();
			_targetSkillGain = _youngHeroBeginningSkillPoints + 30;
		}

		protected override void SetDialogs()
		{
			Campaign.Current.ConversationManager.AddDialogFlow(GetYoungHeroFirstDialogFlow(), this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetYoungHeroSecondDialogFlow(), this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetYoungHeroFailedDialogFlow(), this);
			OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start").NpcLine(new TextObject("{=1WxrzNXx}He'll be delighted. I'll tell him to join you as soon as possible.")).Condition(NotableDialogCondition)
				.Consequence(QuestAcceptedConsequences)
				.CloseDialog();
			DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss").NpcLine(new TextObject("{=OoNULWKy}How is the training going? Are you happy with your student?[if:convo_delighted][ib:hip]")).Condition(NotableDialogCondition)
				.Consequence(delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += MapEventHelper.OnConversationEnd;
				})
				.BeginPlayerOptions()
				.PlayerOption(new TextObject("{=jyxo4YgW}Yes, he is a promising boy."))
				.NpcLine(new TextObject("{=QsL6qcDb}That's very good to hear! Thank you.[if:convo_merry]"))
				.CloseDialog()
				.PlayerOption(new TextObject("{=SbbAhpTu}He is yet to prove himself actually."))
				.NpcLine(new TextObject("{=aHid0t6n}Give him some chance I'm sure he will prove himself soon."))
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private bool NotableDialogCondition()
		{
			StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject);
			return Hero.OneToOneConversationHero == base.QuestGiver;
		}

		private void QuestAcceptedConsequences()
		{
			StartQuest();
			AddCompanionAction.Apply(Clan.PlayerClan, _youngHero);
			MobileParty.MainParty.MemberRoster.AddToCounts(_youngHero.CharacterObject, 1);
			_youngHeroBeginningSkillPoints = _youngHero.HeroDeveloper.GetTotalSkillPoints();
			_targetSkillGain = _youngHeroBeginningSkillPoints + 30;
			TextObject textObject = new TextObject("{=8GbGTDtL}{YOUNG_HERO.LINK}'s experience progress");
			StringHelpers.SetCharacterProperties("YOUNG_HERO", _youngHero.CharacterObject, textObject);
			_startQuestLog = AddDiscreteLog(PlayerStartsQuestLogText, textObject, _youngHero.HeroDeveloper.GetTotalSkillPoints() - _youngHeroBeginningSkillPoints, 30);
			_checkForMissionEnd = true;
			foreach (SkillObject item in Skills.All)
			{
				_oldSkillValues.Add(item, _youngHero.GetSkillValue(item));
				_youngHero.HeroDeveloper.AddFocus(item, 1, checkUnspentFocusPoints: false);
			}
			SetAlreadyChosenHero(_youngHero);
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
			CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnPrisonerTaken);
			CampaignEvents.HeroGainedSkill.AddNonSerializedListener(this, OnHeroGainedSkill);
			CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
			CampaignEvents.OnClanLeaderChangedEvent.AddNonSerializedListener(this, OnClanLeaderChanged);
			CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
			CampaignEvents.CompanionRemoved.AddNonSerializedListener(this, OnCompanionRemoved);
			CampaignEvents.HeroesMarried.AddNonSerializedListener(this, OnHeroesMarried);
			CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
			CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
			CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinished);
		}

		private void OnGameLoadFinished()
		{
			if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.HeroesWithoutParty.Contains(_youngHero))
			{
				SpawnYoungHeroInLordsHall();
			}
		}

		private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
		{
			if (party != null && party.IsMainParty && settlement.HeroesWithoutParty.Contains(_youngHero))
			{
				SpawnYoungHeroInLordsHall();
			}
		}

		public override void OnHeroCanBeSelectedInInventoryInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanHavePartyRoleOrBeGovernorInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanLeadPartyInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanHaveQuestOrIssueInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanMarryInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		private void CommonRestrictionInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _youngHero)
			{
				result = false;
			}
		}

		private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
		{
			if (QuestHelper.CheckMinorMajorCoercion(this, mapEvent, attackerParty))
			{
				QuestHelper.ApplyGenericMinorMajorCoercionConsequences(this, mapEvent);
			}
		}

		private void OnHeroesMarried(Hero hero1, Hero hero2, bool showNotification = true)
		{
			if ((hero1 == Hero.MainHero && hero2 == _youngHero) || (hero2 == Hero.MainHero && hero1 == _youngHero))
			{
				_doNotForceYoungHeroOutFromClan = true;
				CompleteQuestWithCancel(PlayerMarriedYoungHero);
			}
		}

		private void OnClanLeaderChanged(Hero oldLeader, Hero newLeader)
		{
			if (base.IsOngoing && newLeader == _youngHero)
			{
				AddLog(PlayerBecameClanLeader);
				CompleteQuestWithFail();
			}
		}

		private void OnCompanionRemoved(Hero companion, RemoveCompanionAction.RemoveCompanionDetail detail)
		{
			if (base.IsOngoing && companion == _youngHero)
			{
				AddLog(PlayerKickedYoungHeroFromClan);
				CompleteQuestWithFail();
			}
		}

		private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
		{
			if (base.QuestGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				CompleteQuestWithCancel(WarDeclaredQuestCancel);
			}
		}

		private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
		{
			QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest(this, faction1, faction2, detail, PlayerDeclaredWarQuestLogText, WarDeclaredQuestCancel);
		}

		public void OnHeroGainedSkill(Hero hero, SkillObject skill, int change = 1, bool shouldNotify = true)
		{
			if (hero == _youngHero)
			{
				int progress = MBMath.ClampInt(_youngHero.HeroDeveloper.GetTotalSkillPoints() - _youngHeroBeginningSkillPoints, 0, 30);
				_startQuestLog.UpdateCurrentProgress(progress);
				if (_youngHero.HeroDeveloper.GetTotalSkillPoints() >= _targetSkillGain)
				{
					_questCompletedStartConversation = true;
				}
			}
		}

		private void OnPrisonerTaken(PartyBase capturer, Hero prisoner)
		{
			if (prisoner == Hero.MainHero && _youngHero.IsPlayerCompanion && MobileParty.MainParty.MapEventSide != null && MobileParty.MainParty.MapEventSide.Parties.First((MapEventParty x) => x.Party == PartyBase.MainParty).Troops.FindIndexOfCharacter(_youngHero.CharacterObject).IsValid)
			{
				AddLog(PupilEscapedLogText);
				EndCaptivityAction.ApplyByEscape(_youngHero);
			}
		}

		private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
		{
			if (victim == _youngHero)
			{
				AddLog(PupilHasDiedLogText);
				RelationshipChangeWithQuestGiver = -40;
				CompleteQuestWithFail();
			}
		}

		protected override void HourlyTick()
		{
			if (base.IsOngoing && !Hero.MainHero.IsPrisoner && Settlement.CurrentSettlement == null && PlayerEncounter.Current == null && MapEvent.PlayerMapEvent == null && _youngHero.PartyBelongedTo == MobileParty.MainParty && GameStateManager.Current.ActiveState is MapState && ((_checkForMissionEnd && !_firstConversationInitialized) || _questCompletedStartConversation))
			{
				CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty), new ConversationCharacterData(_youngHero.CharacterObject, PartyBase.MainParty));
				if (_checkForMissionEnd)
				{
					_checkForMissionEnd = false;
				}
			}
		}

		private void SpawnYoungHeroInLordsHall()
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(_youngHero.CharacterObject.Race, "_settlement");
			(string, Monster) tuple = (ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, _youngHero.CharacterObject.IsFemale, "_lord"), monsterWithSuffix);
			uint color = (uint)(((int?)_youngHero.MapFaction?.Color) ?? (-3357781));
			uint color2 = (uint)(((int?)_youngHero.MapFaction?.Color) ?? (-3357781));
			AgentData agentData = new AgentData(new SimpleAgentOrigin(_youngHero.CharacterObject)).Monster(tuple.Item2).NoHorses(noHorses: true).ClothingColor1(color)
				.ClothingColor2(color2);
			LocationComplex.Current.GetLocationWithId("lordshall").AddCharacter(new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "sp_notable", fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, tuple.Item1, useCivilianEquipment: true));
		}

		private DialogFlow GetYoungHeroFirstDialogFlow()
		{
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=XH66Leg5}Greetings, my {?PLAYER.GENDER}lady{?}lord{\\?}. I have heard much of your deeds. Thank you for agreeing to train me. I hope I won't disappoint you.[if:convo_grateful][ib:demure]")).Condition(() => Hero.OneToOneConversationHero == _youngHero && !_firstConversationInitialized)
				.Consequence(delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += FirstConversationEndConsequence;
				})
				.BeginPlayerOptions()
				.PlayerOption(new TextObject("{=oJJiKTSL}You are welcome."))
				.NpcLine(new TextObject("{=wlKtDR2z}Thank you, {?PLAYER.GENDER}my lady{?}sir{\\?}."))
				.CloseDialog()
				.PlayerOption(new TextObject("{=FHeJ8bsX}We will see about that."))
				.NpcLine(new TextObject("{=kc3RfwFb}I'll try to be useful to you, {?PLAYER.GENDER}my lady{?}sir{\\?}."))
				.EndPlayerOptions()
				.PlayerLine(new TextObject("{=kJwpbptU}Well, try to stay close to me at all times and try to learn as much as you can."))
				.NpcLine(new TextObject("{=EaifHOao}Yes, {?PLAYER.GENDER}my lady{?}sir{\\?}, I will."))
				.CloseDialog();
		}

		private DialogFlow GetYoungHeroFailedDialogFlow()
		{
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=vbbc6sIU}I regret to tell you that my progress under your tutelage is not satisfactory. I should return to my clan to resume my studies. Thank you for your trouble anyway.")).Condition(() => Hero.OneToOneConversationHero == _youngHero && _showQuestFailedConversation)
				.CloseDialog();
		}

		private void FirstConversationEndConsequence()
		{
			_youngHero.SetHasMet();
			_firstConversationInitialized = true;
		}

		private DialogFlow GetYoungHeroSecondDialogFlow()
		{
			return DialogFlow.CreateDialogFlow("start", 125).BeginNpcOptions().NpcOption(new TextObject("{=APEBfqyW}Greetings my {?PLAYER.GENDER}lady{?}lord{\\?}. Do you wish something from me?[if:convo_innocent_smile][ib:normal2]"), default_conversation_with_young_hero_condition)
				.BeginPlayerOptions()
				.PlayerOption(new TextObject("{=BO0f1Klt}So - how do you find life in our company? Is it all you expected?."))
				.NpcLine(new TextObject("{=e3e79n9B}It is all I expected and more, captain. I am glad that you took me with you.[if:convo_grateful][ib:normal2]"))
				.PlayerLine(new TextObject("{=dbG3PGXL}I'm glad you think that way. Combat aside, have you learned anything special?"))
				.NpcLine(new TextObject("{=8L9W34D6}{NPC_EXPERIENCE_LINE}"))
				.Condition(npc_experience_line_condition)
				.PlayerLine(new TextObject("{=Rh0DlvvE}I'm glad you see it that way. Go on. Continue your training."))
				.NpcLine(new TextObject("{=dnvPDnzS}I will, my {?PLAYER.GENDER}lady{?}lord{\\?}. Thank you[if:convo_grateful][ib:demure]"))
				.CloseDialog()
				.PlayerOption(new TextObject("{=Lk6ln3sR}We seem to have got separated but I have found you. Join me, as we need to continue your training."))
				.Condition(PupilJoinMeCondition)
				.NpcLine(new TextObject("{=0coOJAvg}Yes, {?PLAYER.GENDER}madam{?}sir{\\?}. Thank you."))
				.Consequence(delegate
				{
					MobileParty.MainParty.MemberRoster.AddToCounts(_youngHero.CharacterObject, 1);
				})
				.CloseDialog()
				.EndPlayerOptions()
				.NpcOption(new TextObject("{=kUbovNbE}My {?PLAYER.GENDER}lady{?}lord{\\?}. The agreed training time with you is over. I thank you for everything. It's been a very productive for me.[if:convo_delighted][ib:demure]"), quest_finished_conversation_with_young_hero_condition)
				.PlayerLine(new TextObject("{=bS0bBgp3}I'm happy to hear this. Tell me, what is the most important lesson you've learned from me?"))
				.NpcLine(new TextObject("{=8L9W34D6}{NPC_EXPERIENCE_LINE}"))
				.Condition(npc_experience_line_condition)
				.PlayerLine(new TextObject("{=orprhyYl}I'm glad you see it that way. Very well then, off you go. Send my regards to your family. I hope to see you again one day. I am sure you will make an excellent commander."))
				.NpcLine(new TextObject("{=IBXfCLMp}I certainly hope too {?PLAYER.GENDER}lady{?}lord{\\?}! Again, I want to thank you for everything, before I go, please accept this gift as a humble gratitude.[if:convo_calm_friendly]"))
				.Consequence(QuestCompletedWithSuccessAfterConversation)
				.CloseDialog()
				.EndNpcOptions();
		}

		private bool PupilJoinMeCondition()
		{
			return MobileParty.MainParty.MemberRoster.GetTroopRoster().All((TroopRosterElement x) => x.Character != _youngHero.CharacterObject);
		}

		private void QuestCompletedWithSuccessAfterConversation()
		{
			RelationshipChangeWithQuestGiver = 25;
			ChangeRelationAction.ApplyPlayerRelation(_youngHero, 15);
			CompleteQuestWithSuccess();
			AddLog(SuccessQuestLogText);
			ItemObject randomElementWithPredicate = Items.All.GetRandomElementWithPredicate((ItemObject x) => x.IsTradeGood && x.ItemCategory == DefaultItemCategories.Jewelry);
			TextObject textObject = new TextObject("{=lWvuM5aj}{GIFT_NUMBER} pieces of {JEWELRY} have been added to your inventory.");
			textObject.SetTextVariable("GIFT_NUMBER", _randomForQuestReward);
			textObject.SetTextVariable("JEWELRY", randomElementWithPredicate.Name);
			MBInformationManager.AddQuickInformation(textObject);
			MobileParty.MainParty.ItemRoster.AddToCounts(randomElementWithPredicate, _randomForQuestReward);
			GainRenownAction.Apply(Hero.MainHero, 5f);
		}

		private bool npc_experience_line_condition()
		{
			List<KeyValuePair<ExperienceIncreaseCondition, TextObject>> obj = new List<KeyValuePair<ExperienceIncreaseCondition, TextObject>>
			{
				new KeyValuePair<ExperienceIncreaseCondition, TextObject>(strength_increased_condition, new TextObject("{=hMiJvlJ5}Yes, Since we last spoke, I've learned a lot about hand-to-hand combat, my {?PLAYER.GENDER}lady{?}lord{\\?}. Correct timing and putting your whole body behind the blow means a lot more than I initially thought.[if:convo_calm_friendly][ib:warrior]")),
				new KeyValuePair<ExperienceIncreaseCondition, TextObject>(perception_increased_condition, new TextObject("{=TrQzYwVD}Yes, {?PLAYER.GENDER}my lady{?}lord{\\?}. Since we last spoke I had chance to increase my understanding of ranged combat. How to breath, calculate distance, and lead the target if necessary.[if:convo_calm_friendly][ib:normal]")),
				new KeyValuePair<ExperienceIncreaseCondition, TextObject>(endurance_increased_condition, new TextObject("{=XF7shG4k}Since we last spoke I've been training vigorously. I feel tougher, much more energetic and alive now.[if:convo_calm_friendly][ib:warrior2]")),
				new KeyValuePair<ExperienceIncreaseCondition, TextObject>(cunning_increased_condition, new TextObject("{=H6l9cm7I}I've been paying attention to your subtler methods, {?PLAYER.GENDER}my lady{?}lord{\\?}, I've been observing that courage and strength in numbers is not enough to win most engagements. You have to be aware of the situation and seize the opportunities when they present themselves.[if:convo_calm_friendly][ib:hip]")),
				new KeyValuePair<ExperienceIncreaseCondition, TextObject>(social_increased_condition, new TextObject("{=bNAbWn4E}I've been watching how you deal with different kinds of folk: how to present yourself, how to address people from various walks of life properly, how to inspire greatness to those who trust in you.[if:convo_calm_friendly][ib:confident]")),
				new KeyValuePair<ExperienceIncreaseCondition, TextObject>(intelligence_increased_condition, new TextObject("{=9bd0jJD8}I've been studying a lot, {?PLAYER.GENDER}my lady{?}lord{\\?}. The manuscripts I've acquired on the way on various subjects are invaluable. Seeing professionals in action complements the theoretical knowledge I've learned from the manuscripts.[if:convo_calm_friendly][ib:demure2]"))
			};
			MBList<TextObject> mBList = new MBList<TextObject>();
			foreach (KeyValuePair<ExperienceIncreaseCondition, TextObject> item in obj)
			{
				if (item.Key())
				{
					mBList.Add(item.Value);
				}
			}
			if (mBList.Count != 0)
			{
				MBTextManager.SetTextVariable("NPC_EXPERIENCE_LINE", mBList.GetRandomElement());
				return true;
			}
			MBTextManager.SetTextVariable("NPC_EXPERIENCE_LINE", new TextObject("{=XFafAocV}Nothing specific, captain. But I'm paying close attention to everything you do.[if:convo_excited][ib:nervous]"));
			return true;
		}

		private bool default_conversation_with_young_hero_condition()
		{
			if (Hero.OneToOneConversationHero == _youngHero && _firstConversationInitialized && !_questCompletedStartConversation)
			{
				return !_showQuestFailedConversation;
			}
			return false;
		}

		private bool quest_finished_conversation_with_young_hero_condition()
		{
			if (Hero.OneToOneConversationHero == _youngHero && _firstConversationInitialized && _questCompletedStartConversation)
			{
				return !_showQuestFailedConversation;
			}
			return false;
		}

		private bool strength_increased_condition()
		{
			int num = 0;
			foreach (KeyValuePair<SkillObject, int> oldSkillValue in _oldSkillValues)
			{
				if (oldSkillValue.Key.CharacterAttribute == DefaultCharacterAttributes.Vigor)
				{
					num += _youngHero.GetSkillValue(oldSkillValue.Key) - oldSkillValue.Value;
				}
			}
			return num > 15;
		}

		private bool perception_increased_condition()
		{
			int num = 0;
			foreach (KeyValuePair<SkillObject, int> oldSkillValue in _oldSkillValues)
			{
				if (oldSkillValue.Key.CharacterAttribute == DefaultCharacterAttributes.Control)
				{
					num += _youngHero.GetSkillValue(oldSkillValue.Key) - oldSkillValue.Value;
				}
			}
			return num > 15;
		}

		private bool endurance_increased_condition()
		{
			int num = 0;
			foreach (KeyValuePair<SkillObject, int> oldSkillValue in _oldSkillValues)
			{
				if (oldSkillValue.Key.CharacterAttribute == DefaultCharacterAttributes.Endurance)
				{
					num += _youngHero.GetSkillValue(oldSkillValue.Key) - oldSkillValue.Value;
				}
			}
			return num > 15;
		}

		private bool cunning_increased_condition()
		{
			int num = 0;
			foreach (KeyValuePair<SkillObject, int> oldSkillValue in _oldSkillValues)
			{
				if (oldSkillValue.Key.CharacterAttribute == DefaultCharacterAttributes.Cunning)
				{
					num += _youngHero.GetSkillValue(oldSkillValue.Key) - oldSkillValue.Value;
				}
			}
			return num > 15;
		}

		private bool social_increased_condition()
		{
			int num = 0;
			foreach (KeyValuePair<SkillObject, int> oldSkillValue in _oldSkillValues)
			{
				if (oldSkillValue.Key.CharacterAttribute == DefaultCharacterAttributes.Social)
				{
					num += _youngHero.GetSkillValue(oldSkillValue.Key) - oldSkillValue.Value;
				}
			}
			return num > 15;
		}

		private bool intelligence_increased_condition()
		{
			int num = 0;
			foreach (KeyValuePair<SkillObject, int> oldSkillValue in _oldSkillValues)
			{
				if (oldSkillValue.Key.CharacterAttribute == DefaultCharacterAttributes.Intelligence)
				{
					num += _youngHero.GetSkillValue(oldSkillValue.Key) - oldSkillValue.Value;
				}
			}
			return num > 15;
		}

		protected override void OnFinalize()
		{
			if (!_youngHero.IsAlive)
			{
				return;
			}
			if (_youngHero.CompanionOf == Clan.PlayerClan)
			{
				RemoveCompanionAction.ApplyAfterQuest(Clan.PlayerClan, _youngHero);
				if (!_doNotForceYoungHeroOutFromClan)
				{
					_youngHero.Clan = base.QuestGiver.Clan;
				}
				foreach (Hero child in _youngHero.Children)
				{
					child.Clan = _youngHero.Clan;
				}
				if (_youngHero.Spouse != null)
				{
					_youngHero.Spouse.Clan = _youngHero.Clan;
				}
			}
			foreach (SkillObject item in Skills.All)
			{
				_youngHero.HeroDeveloper.AddFocus(item, -1, checkUnspentFocusPoints: false);
			}
		}

		protected override void OnCompleteWithSuccess()
		{
		}

		protected override void OnTimedOut()
		{
			AddLog(FailTimedOutQuestLogText);
			RelationshipChangeWithQuestGiver = -10;
			_showQuestFailedConversation = true;
			if (Settlement.CurrentSettlement == null && PlayerEncounter.Current == null && MapEvent.PlayerMapEvent == null && GameStateManager.Current.ActiveState is MapState)
			{
				CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty), new ConversationCharacterData(_youngHero.CharacterObject, PartyBase.MainParty));
			}
		}
	}

	public class LordsNeedsTutorIssueTypeDefiner : SaveableTypeDefiner
	{
		public LordsNeedsTutorIssueTypeDefiner()
			: base(80120)
		{
		}

		protected override void DefineClassTypes()
		{
			AddClassDefinition(typeof(LordsNeedsTutorIssue), 1);
			AddClassDefinition(typeof(LordsNeedsTutorIssueQuest), 2);
		}
	}

	private const int YoungHeroAgeMinValue = 18;

	private const int YoungHeroAgeMaxValue = 23;

	private const IssueBase.IssueFrequency LordsNeedsTutorIssueFrequency = IssueBase.IssueFrequency.Common;

	private const int QuestDurationInDays = 200;

	private static List<Hero> _alreadyChosenHeroes = new List<Hero>();

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, OnCheckForIssue);
	}

	public void OnCheckForIssue(Hero hero)
	{
		if (ConditionsHold(hero, out var youngHero))
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(OnStartIssue, typeof(LordsNeedsTutorIssue), IssueBase.IssueFrequency.Common, youngHero));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(LordsNeedsTutorIssue), IssueBase.IssueFrequency.Common));
		}
	}

	private bool SuitableCondition(Hero hero)
	{
		if (hero.Age >= 18f && hero.Age < 23f && hero.PartyBelongedTo == null && hero.PartyBelongedToAsPrisoner == null && hero.IsActive && hero.CurrentSettlement != null && hero.Level <= 15 && !hero.IsFemale && hero.Clan != Clan.PlayerClan)
		{
			return !_alreadyChosenHeroes.Contains(hero);
		}
		return false;
	}

	private bool ConditionsHold(Hero issueGiver, out Hero youngHero)
	{
		youngHero = null;
		if (issueGiver.IsLord && issueGiver.Clan != Clan.PlayerClan && issueGiver.Age > 30f && !issueGiver.IsMinorFactionHero)
		{
			youngHero = issueGiver.Clan.Lords.FirstOrDefault(SuitableCondition);
			if (youngHero != null && youngHero != issueGiver && !SuitableCondition(issueGiver))
			{
				return true;
			}
		}
		return false;
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		return new LordsNeedsTutorIssue(issueOwner, pid.RelatedObject as Hero);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_alreadyChosenHeroes", ref _alreadyChosenHeroes);
	}

	private static void SetAlreadyChosenHero(Hero hero)
	{
		if (_alreadyChosenHeroes == null)
		{
			_alreadyChosenHeroes = new List<Hero> { hero };
		}
		else if (!_alreadyChosenHeroes.Contains(hero))
		{
			_alreadyChosenHeroes.Add(hero);
		}
	}
}
