using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Issues;

public class BettingFraudIssueBehavior : CampaignBehaviorBase
{
	public class BettingFraudIssue : IssueBase
	{
		private const int NeededTournamentCount = 5;

		private const int IssueDuration = 45;

		private const int MainHeroSkillLimit = 50;

		private const int MainClanRenownLimit = 50;

		private const int RelationLimitWithIssueOwner = -10;

		private const float IssueOwnerPowerPenaltyForIssueEffect = -0.2f;

		public override TextObject IssueBriefByIssueGiver => new TextObject("{=kru5Vpog}Yes. I'm glad to have the chance to talk to you. I keep an eye on the careers of champions like yourself for professional reasons, and I have a proposal that might interest a good fighter like you. Interested?[ib:confident3][if:convo_bemused]");

		public override TextObject IssueAcceptByPlayer => new TextObject("{=YWXkgDSd}What kind of a partnership are we talking about?");

		public override TextObject IssueQuestSolutionExplanationByIssueGiver => new TextObject("{=vLaoZhkF}I follow tournaments, you see, and like to both place and take bets. But of course I need someone who can not only win those tournaments but lose if necessary... if you understand what I mean. Not all the time. That would be too obvious. Here's what I propose. We enter into a partnership for five tournaments. Don't bother memorizing which ones you win and which ones you lose. Before each fight, an associate of my mine will let you know how you should place. Follow my instructions and I promise you will be rewarded handsomely. What do you say?[if:convo_bemused][ib:demure2]");

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=cL9BX7ph}As long as the payment is good, I agree.");

		public override bool IsThereAlternativeSolution => false;

		public override bool IsThereLordSolution => false;

		public override TextObject Title => new TextObject("{=xhVrxgC4}Betting Fraud");

		public override TextObject Description
		{
			get
			{
				TextObject textObject = new TextObject("{=3j8pV58L}{ISSUE_GIVER.NAME} offers you a deal to fix {TOURNAMENT_COUNT} tournaments and share the profit from the bet winnings.");
				textObject.SetCharacterProperties("ISSUE_GIVER", base.IssueOwner.CharacterObject);
				textObject.SetTextVariable("TOURNAMENT_COUNT", 5);
				return textObject;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsBettingFraudIssue(object o, List<object> collectedObjects)
		{
			((BettingFraudIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		public BettingFraudIssue(Hero issueOwner)
			: base(issueOwner, CampaignTime.DaysFromNow(45f))
		{
		}

		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.IssueOwnerPower)
			{
				return -0.2f;
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
			return new BettingFraudQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(45f), 0);
		}

		public override IssueFrequency GetFrequency()
		{
			return IssueFrequency.Rare;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueOwner, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
		{
			relationHero = null;
			skill = null;
			flag = PreconditionFlags.None;
			if (Clan.PlayerClan.Renown < 50f)
			{
				flag |= PreconditionFlags.Renown;
			}
			if (issueOwner.GetRelationWithPlayer() < -10f)
			{
				flag |= PreconditionFlags.Relation;
				relationHero = issueOwner;
			}
			if (Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) < 50 && Hero.MainHero.GetSkillValue(DefaultSkills.TwoHanded) < 50 && Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) < 50 && Hero.MainHero.GetSkillValue(DefaultSkills.Bow) < 50 && Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow) < 50 && Hero.MainHero.GetSkillValue(DefaultSkills.Throwing) < 50)
			{
				if (Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) < 50)
				{
					flag |= PreconditionFlags.Skill;
					skill = DefaultSkills.OneHanded;
				}
				else if (Hero.MainHero.GetSkillValue(DefaultSkills.TwoHanded) < 50)
				{
					flag |= PreconditionFlags.Skill;
					skill = DefaultSkills.TwoHanded;
				}
				else if (Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) < 50)
				{
					flag |= PreconditionFlags.Skill;
					skill = DefaultSkills.Polearm;
				}
				else if (Hero.MainHero.GetSkillValue(DefaultSkills.Bow) < 50)
				{
					flag |= PreconditionFlags.Skill;
					skill = DefaultSkills.Bow;
				}
				else if (Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow) < 50)
				{
					flag |= PreconditionFlags.Skill;
					skill = DefaultSkills.Crossbow;
				}
				else if (Hero.MainHero.GetSkillValue(DefaultSkills.Throwing) < 50)
				{
					flag |= PreconditionFlags.Skill;
					skill = DefaultSkills.Throwing;
				}
			}
			return flag == PreconditionFlags.None;
		}

		public override bool IssueStayAliveConditions()
		{
			return true;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}
	}

	public class BettingFraudQuest : QuestBase
	{
		private enum Directives
		{
			None = 0,
			LoseAt3RdRound = 2,
			LoseAt4ThRound = 3,
			WinTheTournament = 4
		}

		private enum AfterTournamentConversationState
		{
			None,
			SmallReward,
			BigReward,
			MinorOffense,
			SecondMinorOffense,
			MajorOffense
		}

		private const int TournamentFixCount = 5;

		private const int MinorOffensiveLimit = 2;

		private const int SmallReward = 250;

		private const int BigReward = 2500;

		private const int CounterOfferReward = 4500;

		private const string MaleThug = "betting_fraud_thug_male";

		private const string FemaleThug = "betting_fraud_thug_female";

		[SaveableField(100)]
		private JournalLog _startLog;

		[SaveableField(1)]
		private Hero _counterOfferNotable;

		[SaveableField(10)]
		internal readonly CharacterObject _thug;

		[SaveableField(20)]
		private int _fixedTournamentCount;

		[SaveableField(30)]
		private int _minorOffensiveCount;

		private Directives _currentDirective;

		private AfterTournamentConversationState _afterTournamentConversationState;

		private bool _counterOfferAccepted;

		private bool _readyToStartTournament;

		private bool _startTournamentEndConversation;

		[SaveableField(40)]
		private bool _counterOfferConversationDone;

		public override TextObject Title => new TextObject("{=xhVrxgC4}Betting Fraud");

		public override bool IsRemainingTimeHidden => false;

		private TextObject StartLog
		{
			get
			{
				TextObject textObject = new TextObject("{=6rweIvZS}{QUEST_GIVER.LINK}, a gang leader from {SETTLEMENT} offers you to fix 5 tournaments together and share the profit.\n {?QUEST_GIVER.GENDER}She{?}He{\\?} asked you to enter 5 tournaments and follow the instructions given by {?QUEST_GIVER.GENDER}her{?}his{\\?} associate.");
				textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
				textObject.SetTextVariable("SETTLEMENT", base.QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
				return textObject;
			}
		}

		private TextObject CurrentDirectiveLog
		{
			get
			{
				TextObject textObject = new TextObject("{=dnZekyZI}Directive from {QUEST_GIVER.LINK}: {DIRECTIVE}");
				textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
				textObject.SetTextVariable("DIRECTIVE", GetDirectiveText());
				return textObject;
			}
		}

		private TextObject QuestFailedWithTimeOutLog
		{
			get
			{
				TextObject textObject = new TextObject("{=2brAaeFh}You failed to complete tournaments in time. {QUEST_GIVER.LINK} will certainly be disappointed.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsBettingFraudQuest(object o, List<object> collectedObjects)
		{
			((BettingFraudQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_thug);
			collectedObjects.Add(_startLog);
			collectedObjects.Add(_counterOfferNotable);
		}

		internal static object AutoGeneratedGetMemberValue_thug(object o)
		{
			return ((BettingFraudQuest)o)._thug;
		}

		internal static object AutoGeneratedGetMemberValue_startLog(object o)
		{
			return ((BettingFraudQuest)o)._startLog;
		}

		internal static object AutoGeneratedGetMemberValue_counterOfferNotable(object o)
		{
			return ((BettingFraudQuest)o)._counterOfferNotable;
		}

		internal static object AutoGeneratedGetMemberValue_fixedTournamentCount(object o)
		{
			return ((BettingFraudQuest)o)._fixedTournamentCount;
		}

		internal static object AutoGeneratedGetMemberValue_minorOffensiveCount(object o)
		{
			return ((BettingFraudQuest)o)._minorOffensiveCount;
		}

		internal static object AutoGeneratedGetMemberValue_counterOfferConversationDone(object o)
		{
			return ((BettingFraudQuest)o)._counterOfferConversationDone;
		}

		public BettingFraudQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold)
			: base(questId, questGiver, duration, rewardGold)
		{
			_counterOfferNotable = null;
			_fixedTournamentCount = 0;
			_minorOffensiveCount = 0;
			_counterOfferAccepted = false;
			_readyToStartTournament = false;
			_startTournamentEndConversation = false;
			_counterOfferConversationDone = false;
			_currentDirective = Directives.None;
			_afterTournamentConversationState = AfterTournamentConversationState.None;
			_thug = MBObjectManager.Instance.GetObject<CharacterObject>((MBRandom.RandomFloat > 0.5f) ? "betting_fraud_thug_male" : "betting_fraud_thug_female");
			_startLog = null;
			SetDialogs();
			InitializeQuestOnCreation();
		}

		protected override void InitializeQuestOnGameLoad()
		{
			SetDialogs();
		}

		protected override void HourlyTick()
		{
		}

		private void SelectCounterOfferNotable(Settlement settlement)
		{
			_counterOfferNotable = settlement.Notables.GetRandomElement();
		}

		private void IncreaseMinorOffensive()
		{
			_minorOffensiveCount++;
			_currentDirective = Directives.None;
			if (_minorOffensiveCount >= 2)
			{
				_afterTournamentConversationState = AfterTournamentConversationState.SecondMinorOffense;
			}
			else
			{
				_afterTournamentConversationState = AfterTournamentConversationState.MinorOffense;
			}
		}

		private void IncreaseFixedTournamentCount()
		{
			_fixedTournamentCount++;
			_startLog.UpdateCurrentProgress(_fixedTournamentCount);
			_currentDirective = Directives.None;
			if (_fixedTournamentCount >= 5)
			{
				_afterTournamentConversationState = AfterTournamentConversationState.BigReward;
			}
			else
			{
				_afterTournamentConversationState = AfterTournamentConversationState.SmallReward;
			}
		}

		private void SetCurrentDirective()
		{
			_currentDirective = ((MBRandom.RandomFloat <= 0.33f) ? Directives.LoseAt3RdRound : ((MBRandom.RandomFloat < 0.5f) ? Directives.LoseAt4ThRound : Directives.WinTheTournament));
			AddLog(CurrentDirectiveLog);
		}

		private void StartTournamentMission()
		{
			TournamentGame tournamentGame = Campaign.Current.TournamentManager.GetTournamentGame(Settlement.CurrentSettlement.Town);
			GameMenu.SwitchToMenu("town");
			tournamentGame.PrepareForTournamentGame(isPlayerParticipating: true);
			Campaign.Current.TournamentManager.OnPlayerJoinTournament(tournamentGame.GetType(), Settlement.CurrentSettlement);
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.PlayerEliminatedFromTournament.AddNonSerializedListener(this, OnPlayerEliminatedFromTournament);
			CampaignEvents.TournamentFinished.AddNonSerializedListener(this, OnTournamentFinished);
			CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, OnGameMenuOpened);
		}

		private void OnPlayerEliminatedFromTournament(int round, Town town)
		{
			_startTournamentEndConversation = true;
			if (round == (int)_currentDirective)
			{
				IncreaseFixedTournamentCount();
			}
			else if (round < (int)_currentDirective)
			{
				IncreaseMinorOffensive();
			}
			else if (round > (int)_currentDirective)
			{
				_afterTournamentConversationState = AfterTournamentConversationState.MajorOffense;
			}
		}

		private void OnTournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town, ItemObject prize)
		{
			if (!participants.Contains(CharacterObject.PlayerCharacter) || _currentDirective == Directives.None)
			{
				return;
			}
			_startTournamentEndConversation = true;
			if (_currentDirective == Directives.WinTheTournament)
			{
				if (winner == CharacterObject.PlayerCharacter)
				{
					IncreaseFixedTournamentCount();
				}
				else
				{
					IncreaseMinorOffensive();
				}
			}
			else if (winner == CharacterObject.PlayerCharacter)
			{
				_afterTournamentConversationState = AfterTournamentConversationState.MajorOffense;
			}
		}

		private void OnGameMenuOpened(MenuCallbackArgs args)
		{
			if (args.MenuContext.GameMenu.StringId == "menu_town_tournament_join")
			{
				GameMenu.SwitchToMenu("menu_town_tournament_join_betting_fraud");
			}
			if (args.MenuContext.GameMenu.StringId == "menu_town_tournament_join_betting_fraud")
			{
				if (_readyToStartTournament)
				{
					if (_fixedTournamentCount == 4 && !_counterOfferConversationDone && _counterOfferNotable != null && _currentDirective != Directives.WinTheTournament)
					{
						CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter), new ConversationCharacterData(_counterOfferNotable.CharacterObject));
					}
					else
					{
						StartTournamentMission();
						_readyToStartTournament = false;
					}
				}
				if (_fixedTournamentCount == 4 && (_counterOfferNotable == null || _counterOfferNotable.CurrentSettlement != Settlement.CurrentSettlement))
				{
					SelectCounterOfferNotable(Settlement.CurrentSettlement);
				}
			}
			if (_startTournamentEndConversation)
			{
				CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter), new ConversationCharacterData(_thug));
			}
		}

		protected override void OnTimedOut()
		{
			base.OnTimedOut();
			PlayerDidNotCompleteTournaments();
		}

		protected override void SetDialogs()
		{
			OfferDialogFlow = GetOfferDialogFlow();
			DiscussDialogFlow = GetDiscussDialogFlow();
			Campaign.Current.ConversationManager.AddDialogFlow(GetDialogWithThugStart(), this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDialogWithThugEnd(), this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetCounterOfferDialog(), this);
		}

		private DialogFlow GetOfferDialogFlow()
		{
			return DialogFlow.CreateDialogFlow("issue_classic_quest_start").NpcLine(new TextObject("{=sp52g5AQ}Very good, very good. Try to enter five tournaments over the next 45 days or so. Right before the fight you'll hear from my associate how far I want you to go in the rankings before you lose.[if:convo_delighted][ib:hip]")).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver)
				.NpcLine(new TextObject("{=ADIYnC4u}Now, I know you can't win every fight, so if you underperform once or twice, I'd understand. But if you lose every time, or worse, if you overperform, well, then I'll be a bit angry.[if:convo_nonchalant][ib:normal2]"))
				.NpcLine(new TextObject("{=1hOPCf8I}But I'm sure you won't disappoint me. Enjoy your riches![if:convo_focused_happy][ib:confident]"))
				.Consequence(OfferDialogFlowConsequence)
				.CloseDialog();
		}

		private void OfferDialogFlowConsequence()
		{
			StartQuest();
			_startLog = AddDiscreteLog(StartLog, new TextObject("{=dLfWFa61}Fix 5 Tournaments"), 0, 5);
		}

		private DialogFlow GetDiscussDialogFlow()
		{
			return DialogFlow.CreateDialogFlow("quest_discuss").NpcLine(new TextObject("{=!}{RESPONSE_TEXT}")).Condition(DiscussDialogCondition)
				.BeginPlayerOptions()
				.PlayerOption(new TextObject("{=abLgPWzf}I will continue to honor our deal. Do not forget to do your end, that's all."))
				.BeginNpcOptions()
				.NpcOption(new TextObject("{=ZLPEsMUx}Well, there are tournament happening in {NEARBY_TOURNAMENTS_LIST} right now. You can go there and do the job. Your denars will be waiting for you."), NpcTournamentLocationCondition)
				.CloseDialog()
				.NpcDefaultOption("{=sUfSCLQx}Sadly, I've heard no news of an upcoming tournament. I am sure one will be held before too long.")
				.CloseDialog()
				.EndNpcOptions()
				.CloseDialog()
				.PlayerOption(new TextObject("{=XUS5wNsD}I feel like I do all the job and you get your denars."))
				.BeginNpcOptions()
				.NpcOption(new TextObject("{=ZLPEsMUx}Well, there are tournament happening in {NEARBY_TOURNAMENTS_LIST} right now. You can go there and do the job. Your denars will be waiting for you."), NpcTournamentLocationCondition)
				.CloseDialog()
				.NpcDefaultOption("{=sUfSCLQx}Sadly, I've heard no news of an upcoming tournament. I am sure one will be held before too long.")
				.CloseDialog()
				.EndNpcOptions()
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private bool DiscussDialogCondition()
		{
			bool num = Hero.OneToOneConversationHero == base.QuestGiver;
			if (num)
			{
				if (_minorOffensiveCount > 0)
				{
					MBTextManager.SetTextVariable("RESPONSE_TEXT", new TextObject("{=7SPwGYvf}I had expected better of you. But even the best can fail sometimes. Just make sure it does not happen again.[if:convo_bored][ib:closed2] "));
					return num;
				}
				MBTextManager.SetTextVariable("RESPONSE_TEXT", new TextObject("{=vo0uhUsZ}I have high hopes for you, friend. Just follow my directives and we will be rich.[if:convo_relaxed_happy][ib:demure2]"));
			}
			return num;
		}

		private bool NpcTournamentLocationCondition()
		{
			List<Town> source = Town.AllTowns.Where((Town x) => Campaign.Current.TournamentManager.GetTournamentGame(x) != null && x != Settlement.CurrentSettlement.Town).ToList();
			source = source.OrderBy((Town x) => x.Settlement.Position2D.DistanceSquared(Settlement.CurrentSettlement.Position2D)).ToList();
			if (source.Count > 0)
			{
				MBTextManager.SetTextVariable("NEARBY_TOURNAMENTS_LIST", source[0].Name);
				return true;
			}
			return false;
		}

		private DialogFlow GetDialogWithThugStart()
		{
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=!}{GREETING_LINE}")).Condition(DialogWithThugStartCondition)
				.BeginPlayerOptions()
				.PlayerOption(new TextObject("{=!}{POSITIVE_OPTION}"))
				.Condition(PositiveOptionCondition)
				.Consequence(PositiveOptionConsequences)
				.CloseDialog()
				.PlayerOption(new TextObject("{=!}{NEGATIVE_OPTION}"))
				.Condition(NegativeOptionCondition)
				.Consequence(NegativeOptionConsequence)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private bool DialogWithThugStartCondition()
		{
			int num;
			if (CharacterObject.OneToOneConversationCharacter == _thug)
			{
				num = ((!_startTournamentEndConversation) ? 1 : 0);
				if (num != 0)
				{
					SetCurrentDirective();
					if (_fixedTournamentCount < 2)
					{
						TextObject textObject = new TextObject("{=xYu4yVRU}Hey there friend. So... You don't need to know my name, but suffice to say that we're both friends of {QUEST_GIVER.LINK}. Here's {?QUEST_GIVER.GENDER}her{?}his{\\?} message for you: {DIRECTIVE}.[ib:confident][if:convo_nonchalant]");
						textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
						textObject.SetTextVariable("DIRECTIVE", GetDirectiveText());
						MBTextManager.SetTextVariable("GREETING_LINE", textObject);
						return (byte)num != 0;
					}
					if (_fixedTournamentCount < 4)
					{
						TextObject textObject2 = new TextObject("{=cQE9tQOy}My friend! Good to see you. You did very well in that last fight. People definitely won't be expecting you to \"{DIRECTIVE}\". What a surprise that would be. Well, I should not keep you from your tournament. You know what to do.[if:convo_happy][ib:closed2]");
						textObject2.SetTextVariable("DIRECTIVE", GetDirectiveText());
						MBTextManager.SetTextVariable("GREETING_LINE", textObject2);
						return (byte)num != 0;
					}
					TextObject textObject3 = new TextObject("{=RVLPQ4rm}My friend. I am almost sad that these meetings are going to come to an end. Well, a deal is a deal. I won't beat around the bush. Here's your final message: {DIRECTIVE}. I wish you luck, right up until the moment that you have to go down.[if:convo_mocking_teasing][ib:closed]");
					textObject3.SetTextVariable("DIRECTIVE", GetDirectiveText());
					MBTextManager.SetTextVariable("GREETING_LINE", textObject3);
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}

		private bool PositiveOptionCondition()
		{
			if (_fixedTournamentCount < 2)
			{
				MBTextManager.SetTextVariable("POSITIVE_OPTION", new TextObject("{=PrUauabl}As long as the payment is as we talked, you got nothing to worry about."));
			}
			else if (_fixedTournamentCount < 4)
			{
				MBTextManager.SetTextVariable("POSITIVE_OPTION", new TextObject("{=TKRsPVMU}Yes, I did. Be around when the tournament is over."));
			}
			else
			{
				MBTextManager.SetTextVariable("POSITIVE_OPTION", new TextObject("{=26XPQw2v}I will miss this little deal we had. See you at the end"));
			}
			return true;
		}

		private void PositiveOptionConsequences()
		{
			_readyToStartTournament = true;
		}

		private bool NegativeOptionCondition()
		{
			bool num = _fixedTournamentCount >= 4;
			if (num)
			{
				MBTextManager.SetTextVariable("NEGATIVE_OPTION", new TextObject("{=vapdvRQO}This deal was a mistake. We will not talk again after this last tournament."));
			}
			return num;
		}

		private void NegativeOptionConsequence()
		{
			_readyToStartTournament = true;
		}

		private TextObject GetDirectiveText()
		{
			if (_currentDirective == Directives.LoseAt3RdRound)
			{
				return new TextObject("{=aHlcBLYB}Lose this tournament at 3rd round");
			}
			if (_currentDirective == Directives.LoseAt4ThRound)
			{
				return new TextObject("{=hc1mnqOx}Lose this tournament at 4th round");
			}
			if (_currentDirective == Directives.WinTheTournament)
			{
				return new TextObject("{=hl4pTsaO}Win this tournament");
			}
			return TextObject.Empty;
		}

		private DialogFlow GetDialogWithThugEnd()
		{
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=!}{GREETING_LINE}")).Condition(DialogWithThugEndCondition)
				.Consequence(DialogWithThugEndConsequence)
				.CloseDialog();
		}

		private bool DialogWithThugEndCondition()
		{
			bool flag = CharacterObject.OneToOneConversationCharacter == _thug && _startTournamentEndConversation;
			if (flag)
			{
				TextObject textObject = TextObject.Empty;
				switch (_afterTournamentConversationState)
				{
				case AfterTournamentConversationState.SmallReward:
					textObject = new TextObject("{=ZM8t4ZW2}We are very impressed, my friend. Here is the payment as promised. I hope we can continue this profitable partnership. See you at the next tournament.[if:convo_happy][ib:demure]");
					GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, 250);
					break;
				case AfterTournamentConversationState.BigReward:
					textObject = new TextObject("{=9vOZWY25}What an exciting result! I will definitely miss these tournaments. Well, maybe after some time goes by and memories get a little hazy we can continue. Here is the last payment. Very well deserved.[if:convo_happy][ib:demure]");
					break;
				case AfterTournamentConversationState.MinorOffense:
					textObject = new TextObject("{=d8bGHJnZ}This was not we were expecting. We lost some money. Well, Lady Fortune always casts her ballot too in these contests. But try to reassure us that this was her plan, and not yours, eh?[if:convo_grave][ib:closed2]");
					break;
				case AfterTournamentConversationState.SecondMinorOffense:
					textObject = new TextObject("{=bNAG2t8S}Well, my friend, either you're playing us false or you're just not very good at this. Either way, {QUEST_GIVER.LINK} wishes to tell you that {?QUEST_GIVER.GENDER}her{?}his{\\?} association with you is over.[if:convo_predatory][ib:closed2]");
					textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
					break;
				case AfterTournamentConversationState.MajorOffense:
					textObject = new TextObject("{=Lyqx3NYE}Well... What happened back there... That wasn't bad luck or incompetence. {QUEST_GIVER.LINK} trusted in you and {?QUEST_GIVER.GENDER}She{?}He{\\?} doesn't take well to betrayal.[if:convo_angry][ib:warrior]");
					textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
					break;
				default:
					Debug.FailedAssert("After tournament conversation state is not set!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Issues\\BettingFraudIssueBehavior.cs", "DialogWithThugEndCondition", 722);
					break;
				}
				MBTextManager.SetTextVariable("GREETING_LINE", textObject);
			}
			return flag;
		}

		private void DialogWithThugEndConsequence()
		{
			_startTournamentEndConversation = false;
			switch (_afterTournamentConversationState)
			{
			case AfterTournamentConversationState.BigReward:
				MainHeroSuccessfullyFixedTournaments();
				break;
			case AfterTournamentConversationState.SecondMinorOffense:
				MainHeroFailToFixTournaments();
				break;
			case AfterTournamentConversationState.MajorOffense:
				if (_counterOfferAccepted)
				{
					MainHeroAcceptsCounterOffer();
				}
				else
				{
					MainHeroChooseNotToFixTournaments();
				}
				break;
			case AfterTournamentConversationState.SmallReward:
			case AfterTournamentConversationState.MinorOffense:
				break;
			}
		}

		private DialogFlow GetCounterOfferDialog()
		{
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=bUfBHSsz}Hold on a moment, friend. I need to talk to you.[ib:aggressive]")).Condition(CounterOfferConversationStartCondition)
				.Consequence(CounterOfferConversationStartConsequence)
				.PlayerLine(new TextObject("{=PZfR7hEK}What do you want? I have a tournament to prepare for."))
				.NpcLine(new TextObject("{=GN9F316V}Oh of course you do. {QUEST_GIVER.LINK}'s people have been running around placing bets - we know all about your arrangement, you see. And let me tell you something: as these arrangements go, {QUEST_GIVER.LINK} is getting you cheap. Do you want to see real money? Win this tournament and I will pay you what you're worth. And isn't it better to win than to lose?[if:convo_mocking_aristocratic][ib:confident2]"))
				.Condition(AccusationCondition)
				.BeginPlayerOptions()
				.PlayerOption(new TextObject("{=MacG8ikN}I will think about it."))
				.Consequence(CounterOfferAcceptedConsequence)
				.CloseDialog()
				.PlayerOption(new TextObject("{=bT279pk9}I have no idea what you talking about. Be on your way, friend."))
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private bool CounterOfferConversationStartCondition()
		{
			if (_counterOfferNotable != null)
			{
				return CharacterObject.OneToOneConversationCharacter == _counterOfferNotable.CharacterObject;
			}
			return false;
		}

		private void CounterOfferConversationStartConsequence()
		{
			_counterOfferConversationDone = true;
		}

		private bool AccusationCondition()
		{
			StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
			return true;
		}

		private void CounterOfferAcceptedConsequence()
		{
			_counterOfferAccepted = true;
		}

		private void MainHeroSuccessfullyFixedTournaments()
		{
			TextObject textObject = new TextObject("{=aCA83avL}You have placed in the tournaments as {QUEST_GIVER.LINK} wished.");
			textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
			AddLog(textObject);
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, 2500);
			Clan.PlayerClan.AddRenown(2f);
			base.QuestGiver.AddPower(10f);
			base.QuestGiver.CurrentSettlement.Town.Security += -20f;
			RelationshipChangeWithQuestGiver = 5;
			CompleteQuestWithSuccess();
		}

		private void MainHeroFailToFixTournaments()
		{
			TextObject textObject = new TextObject("{=ETbToaZC}You have failed to place in the tournaments as {QUEST_GIVER.LINK} wished.");
			textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
			AddLog(textObject);
			base.QuestGiver.AddPower(-10f);
			base.QuestGiver.CurrentSettlement.Town.Security += 10f;
			RelationshipChangeWithQuestGiver = -5;
			CompleteQuestWithFail();
		}

		private void MainHeroChooseNotToFixTournaments()
		{
			TextObject textObject = new TextObject("{=52smwnzz}You have chosen not to place in the tournaments as {QUEST_GIVER.LINK} wished.");
			textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
			AddLog(textObject);
			base.QuestGiver.AddPower(-15f);
			base.QuestGiver.CurrentSettlement.Town.Security += 15f;
			RelationshipChangeWithQuestGiver = -10;
			CompleteQuestWithFail();
		}

		private void MainHeroAcceptsCounterOffer()
		{
			TextObject textObject = new TextObject("{=nb0wqaGA}You have made a deal with {NOTABLE.LINK} to betray {QUEST_GIVER.LINK}.");
			textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
			textObject.SetCharacterProperties("NOTABLE", _counterOfferNotable.CharacterObject);
			AddLog(textObject);
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, 4500);
			base.QuestGiver.AddPower(-15f);
			base.QuestGiver.CurrentSettlement.Town.Security += 15f;
			ChangeRelationAction.ApplyPlayerRelation(_counterOfferNotable, 2);
			RelationshipChangeWithQuestGiver = -10;
			CompleteQuestWithFail();
		}

		private void PlayerDidNotCompleteTournaments()
		{
			AddLog(QuestFailedWithTimeOutLog);
			ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, -5);
		}
	}

	public class BettingFraudIssueTypeDefiner : SaveableTypeDefiner
	{
		public BettingFraudIssueTypeDefiner()
			: base(600327)
		{
		}

		protected override void DefineClassTypes()
		{
			AddClassDefinition(typeof(BettingFraudIssue), 1);
			AddClassDefinition(typeof(BettingFraudQuest), 2);
		}
	}

	private const IssueBase.IssueFrequency BettingFraudIssueFrequency = IssueBase.IssueFrequency.Rare;

	private const string JoinTournamentMenuId = "menu_town_tournament_join";

	private const string JoinTournamentForBettingFraudQuestMenuId = "menu_town_tournament_join_betting_fraud";

	private const int SettlementSecurityLimit = 50;

	private BettingFraudQuest _cachedQuest;

	private static BettingFraudQuest Instance
	{
		get
		{
			BettingFraudIssueBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<BettingFraudIssueBehavior>();
			if (campaignBehavior._cachedQuest != null && campaignBehavior._cachedQuest.IsOngoing)
			{
				return campaignBehavior._cachedQuest;
			}
			foreach (QuestBase quest in Campaign.Current.QuestManager.Quests)
			{
				if (quest is BettingFraudQuest cachedQuest)
				{
					campaignBehavior._cachedQuest = cachedQuest;
					return campaignBehavior._cachedQuest;
				}
			}
			return null;
		}
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, CheckForIssue);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
	}

	private void OnSessionLaunched(CampaignGameStarter gameStarter)
	{
		gameStarter.AddGameMenu("menu_town_tournament_join_betting_fraud", "{=5Adr6toM}{MENU_TEXT}", game_menu_tournament_join_on_init, GameOverlays.MenuOverlayType.SettlementWithBoth);
		gameStarter.AddGameMenuOption("menu_town_tournament_join_betting_fraud", "mno_tournament_event_1", "{=es0Y3Bxc}Join", delegate(MenuCallbackArgs args)
		{
			args.optionLeaveType = GameMenuOption.LeaveType.Mission;
			args.OptionQuestData = GameMenuOption.IssueQuestFlags.ActiveIssue;
			return true;
		}, game_menu_tournament_join_current_game_on_consequence);
		gameStarter.AddGameMenuOption("menu_town_tournament_join_betting_fraud", "mno_tournament_leave", "{=3sRdGQou}Leave", delegate(MenuCallbackArgs args)
		{
			args.optionLeaveType = GameMenuOption.LeaveType.Leave;
			return true;
		}, delegate
		{
			GameMenu.SwitchToMenu("town_arena");
		}, isLeave: true);
	}

	private void game_menu_tournament_join_on_init(MenuCallbackArgs args)
	{
		TournamentGame tournamentGame = Campaign.Current.TournamentManager.GetTournamentGame(Settlement.CurrentSettlement.Town);
		tournamentGame.UpdateTournamentPrize(includePlayer: true);
		GameTexts.SetVariable("MENU_TEXT", tournamentGame.GetMenuText());
	}

	private void game_menu_tournament_join_current_game_on_consequence(MenuCallbackArgs args)
	{
		CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter), new ConversationCharacterData(Instance._thug));
	}

	[GameMenuInitializationHandler("menu_town_tournament_join_betting_fraud")]
	private static void game_menu_ui_town_ui_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		args.MenuContext.SetBackgroundMeshName(currentSettlement.Town.WaitMeshName);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void CheckForIssue(Hero hero)
	{
		if (ConditionsHold(hero))
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(OnStartIssue, typeof(BettingFraudIssue), IssueBase.IssueFrequency.Rare));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(BettingFraudIssue), IssueBase.IssueFrequency.Rare));
		}
	}

	private bool ConditionsHold(Hero issueGiver)
	{
		if (issueGiver.IsGangLeader && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.Town != null)
		{
			return issueGiver.CurrentSettlement.Town.Security < 50f;
		}
		return false;
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		return new BettingFraudIssue(issueOwner);
	}
}
