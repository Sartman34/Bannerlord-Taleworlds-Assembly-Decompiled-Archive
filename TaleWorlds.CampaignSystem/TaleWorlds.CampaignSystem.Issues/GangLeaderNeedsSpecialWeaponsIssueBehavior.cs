using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.CraftingSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Issues;

public class GangLeaderNeedsSpecialWeaponsIssueBehavior : CampaignBehaviorBase
{
	public class GangLeaderNeedsSpecialWeaponsIssue : IssueBase
	{
		private const int IssueAndQuestDuration = 30;

		public int NumberOfDaggersRequested => 2 + TaleWorlds.Library.MathF.Ceiling(4f * base.IssueDifficultyMultiplier);

		public int BaseGoldRewardPerDagger => 200 + TaleWorlds.Library.MathF.Ceiling(500f * base.IssueDifficultyMultiplier);

		public override bool IsThereAlternativeSolution => false;

		public override bool IsThereLordSolution => false;

		protected override bool IssueQuestCanBeDuplicated => false;

		public override TextObject IssueBriefByIssueGiver => new TextObject("{=fmjWsUB6}Yeah... I've heard about your skills as a crafter of weapons. Now, nothing inspires my lads to shed my enemies' blood than the feel of a really well-made blade in their hands. Could you make me some? I'll pay well.");

		public override TextObject IssueAcceptByPlayer => new TextObject("{=UdnXHLSh}What do you want exactly?");

		public override TextObject IssueQuestSolutionExplanationByIssueGiver
		{
			get
			{
				TextObject textObject = new TextObject("{=xmETCOam}Daggers. Good, sharp, light daggers that they can hide under their clothes, so the guards won't stop them. We want to order {REQUESTED_AMOUNT} daggers and I guarantee a minimum of {REWARD_PER_DAGGER} denars for each, more if they're of exceptional quality. Also I can arrange things with the smith of {QUEST_SETTLEMENT} for you to work in his workshop.");
				textObject.SetTextVariable("REQUESTED_AMOUNT", NumberOfDaggersRequested);
				textObject.SetTextVariable("REWARD_PER_DAGGER", BaseGoldRewardPerDagger);
				textObject.SetTextVariable("QUEST_SETTLEMENT", base.IssueOwner.CurrentSettlement.Name);
				return textObject;
			}
		}

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=goLdeLsT}Alright I will forge your daggers.");

		public override TextObject Title => new TextObject("{=tm9PiOMA}Special Weapon Order");

		public override TextObject Description
		{
			get
			{
				TextObject textObject = new TextObject("{=auIV2JoK}{ISSUE_GIVER.LINK} is looking for someone to craft special weapons for {?ISSUE_GIVER.GENDER}her{?}his{\\?} men.");
				textObject.SetCharacterProperties("ISSUE_GIVER", base.IssueOwner.CharacterObject);
				return textObject;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsGangLeaderNeedsSpecialWeaponsIssue(object o, List<object> collectedObjects)
		{
			((GangLeaderNeedsSpecialWeaponsIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		public GangLeaderNeedsSpecialWeaponsIssue(Hero issueOwner)
			: base(issueOwner, CampaignTime.DaysFromNow(30f))
		{
		}

		public override IssueFrequency GetFrequency()
		{
			return IssueFrequency.VeryCommon;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
		{
			flag = PreconditionFlags.None;
			relationHero = issueGiver;
			skill = DefaultSkills.Crafting;
			if (issueGiver.GetRelationWithPlayer() < -10f)
			{
				flag |= PreconditionFlags.Relation;
			}
			if (FactionManager.IsAtWarAgainstFaction(issueGiver.MapFaction, Hero.MainHero.MapFaction))
			{
				flag |= PreconditionFlags.AtWar;
			}
			if (Hero.MainHero.GetSkillValue(skill) < 30)
			{
				flag |= PreconditionFlags.Skill;
			}
			return flag == PreconditionFlags.None;
		}

		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.IssueOwnerPower)
			{
				return -0.1f;
			}
			if (issueEffect == DefaultIssueEffects.SettlementSecurity)
			{
				return 0.5f;
			}
			return 0f;
		}

		public override bool IssueStayAliveConditions()
		{
			return true;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}

		protected override void OnGameLoad()
		{
		}

		protected override QuestBase GenerateIssueQuest(string questId)
		{
			return new GangLeaderNeedsSpecialWeaponsIssueQuest(questId, base.IssueOwner, NumberOfDaggersRequested, BaseGoldRewardPerDagger, CampaignTime.DaysFromNow(30f), RewardGold);
		}

		protected override void HourlyTick()
		{
		}
	}

	public class GangLeaderNeedsSpecialWeaponsIssueQuest : QuestBase
	{
		[SaveableField(1)]
		private readonly int _numberOfDaggersRequested;

		[SaveableField(2)]
		private readonly int _baseGoldRewardPerDagger;

		[SaveableField(3)]
		private CraftingOrder _currentCraftingOrder;

		[SaveableField(4)]
		private int _completedCraftingOrders;

		[SaveableField(5)]
		private JournalLog _playerAcceptedQuestLog;

		[SaveableField(6)]
		private JournalLog _playerHasNeededItemsLog;

		private const int SuccessRelationBonus = 5;

		private const int SuccessPowerBonus = 10;

		private const int FailRelationPenalty = -5;

		private const int FailPowerPenalty = -10;

		private const int MaxCraftingOrderDifficulty = 100;

		private const int CraftingOrderDifficultyVariance = 10;

		private const int MaxNumberOfCraftingOrdersAvailable = 10;

		private const string DaggerCraftingTemplateId = "Dagger";

		private ICraftingCampaignBehavior _craftingBehavior;

		public override bool IsRemainingTimeHidden => false;

		private TextObject QuestStartedLog
		{
			get
			{
				TextObject textObject = new TextObject("{=zo1shYCL}{QUEST_GIVER.LINK} told you that {?QUEST_GIVER.GENDER}her{?}his{\\?} men need a special dagger that should be light and small enough to hide from the city guards. {?QUEST_GIVER.GENDER}She{?}He{\\?} asked you to forge {REQUESTED_AMOUNT} daggers at the smith of {QUEST_SETTLEMENT}. {?QUEST_GIVER.GENDER}She{?}He{\\?} guaranteed to pay at least {REWARD_PER_ITEM} denars for each dagger and he is ready to pay extra depending on the quality of the weapons.");
				textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
				textObject.SetTextVariable("REQUESTED_AMOUNT", _numberOfDaggersRequested);
				textObject.SetTextVariable("REWARD_PER_ITEM", _baseGoldRewardPerDagger);
				textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
				return textObject;
			}
		}

		private TextObject QuestSuccessLog
		{
			get
			{
				TextObject textObject = new TextObject("{=3uvbVxfx}You have delivered the weapons to {QUEST_GIVER.LINK} as promised.");
				textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
				return textObject;
			}
		}

		private TextObject QuestCanceledWarDeclaredLog
		{
			get
			{
				TextObject textObject = new TextObject("{=vW6kBki9}Your clan is now at war with {QUEST_GIVER.LINK}'s realm. Your agreement with {QUEST_GIVER.LINK} is canceled.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject QuestFailedLog
		{
			get
			{
				TextObject textObject = new TextObject("{=iTgVn26a}You have failed to bring the weapons to {QUEST_GIVER.LINK} in time.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		public override TextObject Title => new TextObject("{=tm9PiOMA}Special Weapon Order");

		private TextObject PlayerHasNeededItemsLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=kHio2hlU}You now have enough daggers to complete the quest. Return to {QUEST_GIVER.LINK} to hand them over.");
				textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
				return textObject;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsGangLeaderNeedsSpecialWeaponsIssueQuest(object o, List<object> collectedObjects)
		{
			((GangLeaderNeedsSpecialWeaponsIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_currentCraftingOrder);
			collectedObjects.Add(_playerAcceptedQuestLog);
			collectedObjects.Add(_playerHasNeededItemsLog);
		}

		internal static object AutoGeneratedGetMemberValue_numberOfDaggersRequested(object o)
		{
			return ((GangLeaderNeedsSpecialWeaponsIssueQuest)o)._numberOfDaggersRequested;
		}

		internal static object AutoGeneratedGetMemberValue_baseGoldRewardPerDagger(object o)
		{
			return ((GangLeaderNeedsSpecialWeaponsIssueQuest)o)._baseGoldRewardPerDagger;
		}

		internal static object AutoGeneratedGetMemberValue_currentCraftingOrder(object o)
		{
			return ((GangLeaderNeedsSpecialWeaponsIssueQuest)o)._currentCraftingOrder;
		}

		internal static object AutoGeneratedGetMemberValue_completedCraftingOrders(object o)
		{
			return ((GangLeaderNeedsSpecialWeaponsIssueQuest)o)._completedCraftingOrders;
		}

		internal static object AutoGeneratedGetMemberValue_playerAcceptedQuestLog(object o)
		{
			return ((GangLeaderNeedsSpecialWeaponsIssueQuest)o)._playerAcceptedQuestLog;
		}

		internal static object AutoGeneratedGetMemberValue_playerHasNeededItemsLog(object o)
		{
			return ((GangLeaderNeedsSpecialWeaponsIssueQuest)o)._playerHasNeededItemsLog;
		}

		public GangLeaderNeedsSpecialWeaponsIssueQuest(string questId, Hero questGiver, int numberOfDaggersRequested, int baseGoldRewardPerDagger, CampaignTime duration, int rewardGold)
			: base(questId, questGiver, duration, rewardGold)
		{
			_numberOfDaggersRequested = numberOfDaggersRequested;
			_baseGoldRewardPerDagger = baseGoldRewardPerDagger;
			_craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
			SetDialogs();
			InitializeQuestOnCreation();
		}

		protected override void SetDialogs()
		{
			TextObject npcText = new TextObject("{=siofh72D}Thank you, my friend. I'm looking forward to giving my boys their new toys.");
			TextObject npcText2 = new TextObject("{=cJOGUpSS}Any news about my orders?");
			TextObject text = new TextObject("{=R9NDaOhb}The daggers are almost ready. They just need a little more work...");
			TextObject npcText3 = new TextObject("{=CDXUehf0}Good, good.");
			TextObject text2 = new TextObject("{=wErSpkjy}I'm still working on it.");
			TextObject npcText4 = new TextObject("{=r2g61BjX}Well, my lads are anxiously waiting...");
			TextObject text3 = new TextObject("{=TBuyyh2S}There you go, that should be enough daggers for your men.");
			TextObject npcText5 = new TextObject("{=QCzB8DDX}Ah excellent, these will come in handy.");
			OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start").NpcLine(npcText).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver)
				.Consequence(QuestAcceptedConsequences)
				.CloseDialog();
			DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss").NpcLine(npcText2).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver)
				.BeginPlayerOptions()
				.PlayerOption(text)
				.NpcLine(npcText3)
				.CloseDialog()
				.PlayerOption(text2)
				.NpcLine(npcText4)
				.CloseDialog()
				.PlayerOption(text3)
				.ClickableCondition(CheckPlayerHasCompletedEnoughOrdersClickableCondition)
				.NpcLine(npcText5)
				.Consequence(SucceedQuest)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private bool CheckPlayerHasCompletedEnoughOrdersClickableCondition(out TextObject explanation)
		{
			if (_completedCraftingOrders >= _numberOfDaggersRequested)
			{
				explanation = TextObject.Empty;
				return true;
			}
			explanation = new TextObject("{=mAvJcyY1}You haven't completed enough crafting orders yet.");
			return false;
		}

		private void QuestAcceptedConsequences()
		{
			StartQuest();
			_currentCraftingOrder = GetDaggerCraftingOrder();
			_playerAcceptedQuestLog = AddDiscreteLog(QuestStartedLog, new TextObject("{=scjHmuyF}Complete Crafting Orders"), _completedCraftingOrders, _numberOfDaggersRequested);
		}

		private void CheckIfPlayerReadyToReturnItems()
		{
			if (_playerHasNeededItemsLog == null && _playerAcceptedQuestLog.CurrentProgress >= _numberOfDaggersRequested)
			{
				_playerHasNeededItemsLog = AddLog(PlayerHasNeededItemsLogText);
			}
			else if (_playerHasNeededItemsLog != null && _playerAcceptedQuestLog.CurrentProgress < _numberOfDaggersRequested)
			{
				RemoveLog(_playerHasNeededItemsLog);
				_playerHasNeededItemsLog = null;
			}
		}

		protected override void OnTimedOut()
		{
			if (_playerHasNeededItemsLog != null && _playerHasNeededItemsLog.CurrentProgress >= _numberOfDaggersRequested)
			{
				SucceedQuest();
			}
			else
			{
				FailQuest();
			}
		}

		private void SucceedQuest()
		{
			AddLog(QuestSuccessLog);
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, 30)
			});
			RelationshipChangeWithQuestGiver = 5;
			base.QuestGiver.AddPower(10f);
			if (_currentCraftingOrder != null)
			{
				_craftingBehavior.CancelCustomOrder(base.QuestGiver.CurrentSettlement.Town, _currentCraftingOrder);
			}
			GiveGoldAction.ApplyForQuestBetweenCharacters(base.QuestGiver, Hero.MainHero, _baseGoldRewardPerDagger * _completedCraftingOrders);
			CompleteQuestWithSuccess();
		}

		private void FailQuest()
		{
			RelationshipChangeWithQuestGiver = -5;
			base.QuestGiver.AddPower(-10f);
			_craftingBehavior.CancelCustomOrder(base.QuestGiver.CurrentSettlement.Town, _currentCraftingOrder);
			CompleteQuestWithFail(QuestFailedLog);
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
			CampaignEvents.OnCraftingOrderCompletedEvent.AddNonSerializedListener(this, OnCraftingOrderCompleted);
		}

		private void OnCraftingOrderCompleted(Town town, CraftingOrder craftingOrder, ItemObject craftedItem, Hero completerHero)
		{
			if (craftingOrder == _currentCraftingOrder)
			{
				_completedCraftingOrders++;
				if (_completedCraftingOrders == _numberOfDaggersRequested)
				{
					TextObject textObject = new TextObject("{=T4q1DkfF}You have completed {QUEST_GIVER.NAME}'s request, you can go back to receive your reward or keep working on more orders.");
					textObject.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
					MBInformationManager.AddQuickInformation(textObject);
				}
				if (_completedCraftingOrders < 10)
				{
					_playerAcceptedQuestLog.UpdateCurrentProgress(_completedCraftingOrders);
					CheckIfPlayerReadyToReturnItems();
					_currentCraftingOrder = GetDaggerCraftingOrder();
				}
				else if (_completedCraftingOrders == 10)
				{
					_currentCraftingOrder = null;
					TextObject textObject2 = new TextObject("{=1WbsW7I7}{QUEST_GIVER.NAME} won’t need anymore daggers. You can go back to {?QUEST_GIVER.GENDER}her{?}him{\\?} to get your reward.");
					textObject2.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject);
					MBInformationManager.AddQuickInformation(textObject2);
				}
			}
		}

		private CraftingOrder GetDaggerCraftingOrder()
		{
			CraftingTemplate randomElementWithPredicate = CraftingTemplate.All.GetRandomElementWithPredicate((CraftingTemplate x) => x.TemplateName.ToString() == "Dagger");
			CraftingOrder craftingOrder = _craftingBehavior.CreateCustomOrderForHero(base.QuestGiver, GetCraftingDifficulty(), null, randomElementWithPredicate);
			AddTrackedObject(craftingOrder);
			return craftingOrder;
		}

		private float GetCraftingDifficulty()
		{
			return TaleWorlds.Library.MathF.Clamp(TaleWorlds.Library.MathF.Min(Hero.MainHero.GetSkillValue(DefaultSkills.Crafting), 100), 10f, 100f) + (float)MBRandom.RandomInt(-10, 10);
		}

		private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail declareWarDetail)
		{
			QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest(this, faction1, faction2, declareWarDetail, QuestCanceledWarDeclaredLog, QuestCanceledWarDeclaredLog);
		}

		protected override void InitializeQuestOnGameLoad()
		{
			SetDialogs();
			_craftingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
			if (_craftingBehavior == null)
			{
				CompleteQuestWithCancel();
			}
		}

		protected override void HourlyTick()
		{
		}
	}

	public class GangLeaderNeedsSpecialWeaponsIssueTypeDefiner : SaveableTypeDefiner
	{
		public GangLeaderNeedsSpecialWeaponsIssueTypeDefiner()
			: base(596061)
		{
		}

		protected override void DefineClassTypes()
		{
			AddClassDefinition(typeof(GangLeaderNeedsSpecialWeaponsIssue), 1);
			AddClassDefinition(typeof(GangLeaderNeedsSpecialWeaponsIssueQuest), 2);
		}
	}

	private const IssueBase.IssueFrequency SpecialWeaponOrderIssueFrequency = IssueBase.IssueFrequency.VeryCommon;

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, OnCheckForIssue);
	}

	private void OnCheckForIssue(Hero hero)
	{
		Campaign.Current.IssueManager.AddPotentialIssueData(hero, ConditionsHold(hero) ? new PotentialIssueData(OnStartIssue, typeof(GangLeaderNeedsSpecialWeaponsIssue), IssueBase.IssueFrequency.VeryCommon) : new PotentialIssueData(typeof(GangLeaderNeedsSpecialWeaponsIssue), IssueBase.IssueFrequency.VeryCommon));
	}

	private bool ConditionsHold(Hero issueGiver)
	{
		if (issueGiver.CurrentSettlement != null && issueGiver.IsGangLeader && issueGiver.CurrentSettlement.IsTown)
		{
			return Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>() != null;
		}
		return false;
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		return new GangLeaderNeedsSpecialWeaponsIssue(issueOwner);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
