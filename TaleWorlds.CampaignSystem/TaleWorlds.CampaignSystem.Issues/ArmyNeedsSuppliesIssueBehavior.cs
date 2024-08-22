using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Issues;

public class ArmyNeedsSuppliesIssueBehavior : CampaignBehaviorBase
{
	public class ArmyNeedsSuppliesIssue : IssueBase
	{
		[SaveableField(70)]
		private int NumberOfManInArmy;

		private int GrainAmount => MathF.Ceiling((float)(NumberOfManInArmy / 20) * 5f);

		private int LiveStockAmount => MathF.Ceiling((float)(NumberOfManInArmy / 20) * 0.5f);

		private int WineAmount => MathF.Ceiling((float)(NumberOfManInArmy / 20) * 0.5f);

		public override bool IsThereAlternativeSolution => false;

		public override bool IsThereLordSolution => false;

		public override TextObject IssueBriefByIssueGiver => new TextObject("{=BW7v0g6q}We are about to go on campaign but my quartermaster reports that our food supplies will not be enough to keep us in the field for very long. I can't spare any of my lords, so I need someone else to bring us a large amount of supplies as soon as possible. Can you do this?[if:convo_grave][ib:closed]");

		public override TextObject IssueAcceptByPlayer
		{
			get
			{
				TextObject textObject = new TextObject("{=YaZc08oa}I can bring supplies, your {?QUEST_GIVER.GENDER}ladyship{?}lordship{\\?}. Just tell me what you need.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject);
				return textObject;
			}
		}

		public override TextObject IssueQuestSolutionExplanationByIssueGiver
		{
			get
			{
				TextObject textObject = new TextObject("{=444rcko0}We need {GRAIN_AMOUNT} sacks of grain to meet our basic needs. And if you can find {LIVESTOCK_AMOUNT} live stocks and {WINE_AMOUNT} barrels of wine that would be a great favor. Men fight better after a good meal.");
				textObject.SetTextVariable("GRAIN_AMOUNT", GrainAmount);
				textObject.SetTextVariable("LIVESTOCK_AMOUNT", LiveStockAmount);
				textObject.SetTextVariable("WINE_AMOUNT", WineAmount);
				return textObject;
			}
		}

		public override TextObject IssueQuestSolutionAcceptByPlayer
		{
			get
			{
				TextObject textObject = new TextObject("{=ppO0hoT6}I'll deliver {GRAIN_AMOUNT} sacks of grain as soon as possible {?QUEST_GIVER.GENDER}lady{?}sir{\\?}, and try to find some livestock and wine as you requested.");
				textObject.SetTextVariable("GRAIN_AMOUNT", GrainAmount);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject);
				return textObject;
			}
		}

		public override TextObject Title => new TextObject("{=wVyqTlpS}Army Needs Supply");

		public override TextObject Description
		{
			get
			{
				TextObject result = new TextObject("{=iMq7M0bo}{QUEST_GIVER.LINK} asks you to provide them supplies for their military campaign.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject);
				return result;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsArmyNeedsSuppliesIssue(object o, List<object> collectedObjects)
		{
			((ArmyNeedsSuppliesIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		internal static object AutoGeneratedGetMemberValueNumberOfManInArmy(object o)
		{
			return ((ArmyNeedsSuppliesIssue)o).NumberOfManInArmy;
		}

		public ArmyNeedsSuppliesIssue(Hero issueOwner)
			: base(issueOwner, CampaignTime.DaysFromNow(15f))
		{
			NumberOfManInArmy = base.IssueOwner.PartyBelongedTo.Army.TotalRegularCount;
		}

		protected override void OnGameLoad()
		{
		}

		protected override void HourlyTick()
		{
		}

		protected override QuestBase GenerateIssueQuest(string questId)
		{
			return new ArmyNeedsSuppliesIssueQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(15f), RewardGold, GrainAmount, LiveStockAmount, WineAmount);
		}

		public override IssueFrequency GetFrequency()
		{
			return IssueFrequency.VeryCommon;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flags, out Hero relationHero, out SkillObject skill)
		{
			relationHero = null;
			skill = null;
			flags = PreconditionFlags.None;
			if (issueGiver.GetRelationWithPlayer() < -10f)
			{
				flags |= PreconditionFlags.Relation;
				relationHero = issueGiver;
			}
			if (Hero.MainHero.IsKingdomLeader)
			{
				flags |= PreconditionFlags.MainHeroIsKingdomLeader;
			}
			if (issueGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				flags |= PreconditionFlags.AtWar;
			}
			if (Clan.PlayerClan.Tier < 1)
			{
				flags |= PreconditionFlags.ClanTier;
			}
			if (Clan.PlayerClan.Kingdom != issueGiver.MapFaction)
			{
				flags |= PreconditionFlags.NotInSameFaction;
			}
			return flags == PreconditionFlags.None;
		}

		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.ClanInfluence)
			{
				return -0.1f;
			}
			return 0f;
		}

		public override bool IssueStayAliveConditions()
		{
			if (base.IssueOwner.PartyBelongedTo != null && base.IssueOwner.PartyBelongedTo.Army != null && base.IssueOwner.PartyBelongedTo.Army.ArmyOwner == base.IssueOwner && base.IssueOwner.PartyBelongedTo.Army.Cohesion > 40f && base.IssueOwner.Clan != Clan.PlayerClan && base.IssueOwner.MapFaction.IsKingdomFaction)
			{
				NumberOfManInArmy = base.IssueOwner.PartyBelongedTo.Army.TotalRegularCount;
				return true;
			}
			return false;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}
	}

	public class ArmyNeedsSuppliesIssueQuest : QuestBase
	{
		[SaveableField(10)]
		private int _requestedGrainAmount;

		[SaveableField(20)]
		private int _requestedLiveStockAmount;

		[SaveableField(30)]
		private int _requestedWineAmount;

		[SaveableField(40)]
		private int _currentGrainAmount;

		[SaveableField(50)]
		private int _currentLiveStockAmount;

		[SaveableField(60)]
		private int _currentWineAmount;

		[SaveableField(70)]
		private JournalLog _grainLog;

		[SaveableField(80)]
		private JournalLog _liveStockLog;

		[SaveableField(90)]
		private JournalLog _wineLog;

		public override bool IsRemainingTimeHidden => false;

		public override TextObject Title => new TextObject("{=wVyqTlpS}Army Needs Supply");

		private TextObject _playerStartsQuestLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=GiaTh92Q}{QUEST_GIVER.LINK}, commanding an army of the {QUEST_GIVER_FACTION}, has told you that they need food supplies for their upcoming military campaign. {?QUEST_GIVER.GENDER}She{?}He{\\?} wanted you to deliver {GRAIN_AMOUNT} sacks of grain and although it's not necessary, to provide {LIVESTOCK_AMOUNT} live stocks and {WINE_AMOUNT} barrels of wine {?QUEST_GIVER.GENDER}she{?}he{\\?} would appreciate it.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				textObject.SetTextVariable("QUEST_GIVER_FACTION", base.QuestGiver.MapFaction.EncyclopediaLinkWithName);
				textObject.SetTextVariable("GRAIN_AMOUNT", _requestedGrainAmount);
				textObject.SetTextVariable("LIVESTOCK_AMOUNT", _requestedLiveStockAmount);
				textObject.SetTextVariable("WINE_AMOUNT", _requestedWineAmount);
				return textObject;
			}
		}

		private TextObject _successQuestLogText => new TextObject("{=z9pbB0K5}You have successfully delivered the supplies as requested.");

		private TextObject _failQuestLogText => new TextObject("{=k5HJ3Ld6}You have failed to deliver the supplies in time.");

		private TextObject _questCanceledWarDeclaredLog
		{
			get
			{
				TextObject textObject = new TextObject("{=vW6kBki9}Your clan is now at war with {QUEST_GIVER.LINK}'s realm. Your agreement with {QUEST_GIVER.LINK} is canceled.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		private TextObject _playerDeclaredWarQuestLogText
		{
			get
			{
				TextObject textObject = new TextObject("{=bqeWVVEE}Your actions have started a war with {QUEST_GIVER.LINK}'s faction. {?QUEST_GIVER.GENDER}She{?}He{\\?} cancels your agreement and the quest is a failure.");
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				return textObject;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsArmyNeedsSuppliesIssueQuest(object o, List<object> collectedObjects)
		{
			((ArmyNeedsSuppliesIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			base.AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_grainLog);
			collectedObjects.Add(_liveStockLog);
			collectedObjects.Add(_wineLog);
		}

		internal static object AutoGeneratedGetMemberValue_requestedGrainAmount(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._requestedGrainAmount;
		}

		internal static object AutoGeneratedGetMemberValue_requestedLiveStockAmount(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._requestedLiveStockAmount;
		}

		internal static object AutoGeneratedGetMemberValue_requestedWineAmount(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._requestedWineAmount;
		}

		internal static object AutoGeneratedGetMemberValue_currentGrainAmount(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._currentGrainAmount;
		}

		internal static object AutoGeneratedGetMemberValue_currentLiveStockAmount(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._currentLiveStockAmount;
		}

		internal static object AutoGeneratedGetMemberValue_currentWineAmount(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._currentWineAmount;
		}

		internal static object AutoGeneratedGetMemberValue_grainLog(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._grainLog;
		}

		internal static object AutoGeneratedGetMemberValue_liveStockLog(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._liveStockLog;
		}

		internal static object AutoGeneratedGetMemberValue_wineLog(object o)
		{
			return ((ArmyNeedsSuppliesIssueQuest)o)._wineLog;
		}

		public ArmyNeedsSuppliesIssueQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, int grainAmount, int liveStockAmount, int wineAmount)
			: base(questId, questGiver, duration, rewardGold)
		{
			_requestedGrainAmount = grainAmount;
			_requestedLiveStockAmount = liveStockAmount;
			_requestedWineAmount = wineAmount;
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

		protected override void SetDialogs()
		{
			OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start").NpcLine(new TextObject("{=64RFlBFr}Very well. Don't worry, all your expenses will be covered.[if:convo_approving][ib:hip] ")).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver)
				.Consequence(QuestAcceptedConsequences)
				.CloseDialog();
			DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss").NpcLine(new TextObject("{=bGbSbqTG}Have you brought our supplies?")).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver)
				.Consequence(delegate
				{
					CalculateAndUpdateRequestedItemsCountInPlayer(notifyPlayer: false);
				})
				.BeginPlayerOptions()
				.PlayerOption(new TextObject("{=FmjZfigu}Yes. I have brought your grain, livestock and wine as you requested."))
				.Condition(CheckIfPlayerCollectedEverything)
				.NpcLine(new TextObject("{=UjS8JnH5}Splendid. I will never forget your service, my friend."))
				.Consequence(CollectedEverythingConsequence)
				.CloseDialog()
				.PlayerOption(new TextObject("{=ISOHhXxW}Yes. I have brought your grain and wine as you requested."))
				.Condition(CheckIfPlayerCollectedGrainWine)
				.NpcLine(new TextObject("{=1atg831t}Thank you. Your service will be remembered."))
				.Consequence(CollectedGrainAndWineConsequence)
				.CloseDialog()
				.PlayerOption(new TextObject("{=YbsVaZkb}Yes. I have brought your grain and livestock as you requested."))
				.Condition(CheckIfPlayerCollectedGrainLiveStock)
				.NpcLine(new TextObject("{=1atg831t}Thank you. Your service will be remembered."))
				.Consequence(CollectedGrainAndLiveStockConsequence)
				.CloseDialog()
				.PlayerOption(new TextObject("{=m9a3ZalO}Yes. I have brought your grain as you requested."))
				.Condition(CheckIfPlayerCollectedGrain)
				.NpcLine(new TextObject("{=1atg831t}Thank you. Your service will be remembered."))
				.Consequence(CollectedGrainConsequence)
				.CloseDialog()
				.PlayerOption(new TextObject("{=PAbVhuKi}Not yet. I'm still working on it."))
				.NpcLine(new TextObject("{=AV5xVGM5}Good. We need them as soon as possible.[if:convo_undecided_open][ib:closed2]"))
				.Consequence(MapEventHelper.OnConversationEnd)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private void CollectedGrainConsequence()
		{
			GainRenownAction.Apply(Hero.MainHero, 2f);
			GainKingdomInfluenceAction.ApplyForGivingFood(Hero.MainHero, base.QuestGiver, 5f);
			RelationshipChangeWithQuestGiver = 2;
			int amount = DefaultItems.Grain.Value * _requestedGrainAmount;
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, amount);
			MobileParty.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, -_requestedGrainAmount);
			base.QuestGiver.PartyBelongedTo.ItemRoster.AddToCounts(DefaultItems.Grain, _requestedGrainAmount);
			CompleteQuestWithSuccess();
			MapEventHelper.OnConversationEnd();
		}

		private void CollectedGrainAndLiveStockConsequence()
		{
			GainRenownAction.Apply(Hero.MainHero, 2f);
			GainKingdomInfluenceAction.ApplyForGivingFood(Hero.MainHero, base.QuestGiver, 5f);
			RelationshipChangeWithQuestGiver = 3;
			int num = DefaultItems.Grain.Value * _requestedGrainAmount;
			num += MBObjectManager.Instance.GetObject<ItemObject>("sheep").Value * _requestedLiveStockAmount;
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, num);
			MobileParty.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, -_requestedGrainAmount);
			base.QuestGiver.PartyBelongedTo.ItemRoster.AddToCounts(DefaultItems.Grain, _requestedGrainAmount);
			RemoveLiveStocksFromPlayer();
			CompleteQuestWithSuccess();
			MapEventHelper.OnConversationEnd();
		}

		private void CollectedGrainAndWineConsequence()
		{
			GainRenownAction.Apply(Hero.MainHero, 2f);
			GainKingdomInfluenceAction.ApplyForGivingFood(Hero.MainHero, base.QuestGiver, 5f);
			RelationshipChangeWithQuestGiver = 4;
			int num = DefaultItems.Grain.Value * _requestedGrainAmount;
			num += MBObjectManager.Instance.GetObject<ItemObject>("wine").Value * _requestedLiveStockAmount;
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, num);
			MobileParty.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, -_requestedGrainAmount);
			base.QuestGiver.PartyBelongedTo.ItemRoster.AddToCounts(DefaultItems.Grain, _requestedGrainAmount);
			ItemObject @object = MBObjectManager.Instance.GetObject<ItemObject>("wine");
			MobileParty.MainParty.ItemRoster.AddToCounts(@object, -_requestedWineAmount);
			base.QuestGiver.PartyBelongedTo.ItemRoster.AddToCounts(@object, _requestedWineAmount);
			CompleteQuestWithSuccess();
			MapEventHelper.OnConversationEnd();
		}

		private void CollectedEverythingConsequence()
		{
			GainRenownAction.Apply(Hero.MainHero, 2f);
			GainKingdomInfluenceAction.ApplyForGivingFood(Hero.MainHero, base.QuestGiver, 8f);
			RelationshipChangeWithQuestGiver = 6;
			int num = DefaultItems.Grain.Value * _requestedGrainAmount;
			num += MBObjectManager.Instance.GetObject<ItemObject>("wine").Value * _requestedLiveStockAmount;
			num += MBObjectManager.Instance.GetObject<ItemObject>("sheep").Value * _requestedLiveStockAmount;
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, num);
			MobileParty.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, -_requestedGrainAmount);
			base.QuestGiver.PartyBelongedTo.ItemRoster.AddToCounts(DefaultItems.Grain, _requestedGrainAmount);
			ItemObject @object = MBObjectManager.Instance.GetObject<ItemObject>("wine");
			MobileParty.MainParty.ItemRoster.AddToCounts(@object, -_requestedWineAmount);
			base.QuestGiver.PartyBelongedTo.ItemRoster.AddToCounts(@object, _requestedWineAmount);
			RemoveLiveStocksFromPlayer();
			CompleteQuestWithSuccess();
			MapEventHelper.OnConversationEnd();
		}

		private void RemoveLiveStocksFromPlayer()
		{
			int num = _requestedLiveStockAmount;
			while (num > 0)
			{
				for (int i = 0; i < MobileParty.MainParty.ItemRoster.Count; i++)
				{
					ItemRosterElement itemRosterElement = MobileParty.MainParty.ItemRoster[i];
					if (itemRosterElement.IsEmpty)
					{
						continue;
					}
					ItemObject item = itemRosterElement.EquipmentElement.Item;
					if (item.HasHorseComponent && item.HorseComponent.IsLiveStock)
					{
						if (num < itemRosterElement.Amount)
						{
							MobileParty.MainParty.ItemRoster.AddToCounts(item, -num);
							num = 0;
							break;
						}
						MobileParty.MainParty.ItemRoster.AddToCounts(item, -itemRosterElement.Amount);
						num -= itemRosterElement.Amount;
					}
				}
			}
			base.QuestGiver.PartyBelongedTo.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("meat"), _requestedLiveStockAmount * 2);
		}

		private bool CheckIfPlayerCollectedGrain()
		{
			return _currentGrainAmount >= _requestedGrainAmount;
		}

		private bool CheckIfPlayerCollectedGrainLiveStock()
		{
			if (_currentGrainAmount >= _requestedGrainAmount)
			{
				return _currentLiveStockAmount >= _requestedLiveStockAmount;
			}
			return false;
		}

		private bool CheckIfPlayerCollectedGrainWine()
		{
			if (_currentGrainAmount >= _requestedGrainAmount)
			{
				return _currentWineAmount >= _requestedWineAmount;
			}
			return false;
		}

		private bool CheckIfPlayerCollectedEverything()
		{
			if (_currentGrainAmount >= _requestedGrainAmount && _currentWineAmount >= _requestedWineAmount)
			{
				return _currentLiveStockAmount >= _requestedLiveStockAmount;
			}
			return false;
		}

		private void QuestAcceptedConsequences()
		{
			StartQuest();
			AddLog(_playerStartsQuestLogText);
			CalculateAndUpdateRequestedItemsCountInPlayer();
			_grainLog = AddDiscreteLog(TextObject.Empty, new TextObject("{=yGxjOnYb}Collected Grain Amount"), _currentGrainAmount, _requestedGrainAmount);
			_liveStockLog = AddDiscreteLog(TextObject.Empty, new TextObject("{=aIxX2s8n}Collected Livestock Amount (Optional)"), _currentLiveStockAmount, _requestedLiveStockAmount);
			_wineLog = AddDiscreteLog(TextObject.Empty, new TextObject("{=ENS8Ig1o}Collected Wine Amount (Optional)"), _currentWineAmount, _requestedWineAmount);
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
			CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, OnInventoryExchange);
			CampaignEvents.ArmyDispersed.AddNonSerializedListener(this, OnArmyDispersed);
			CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
		}

		private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
		{
			if (clan == Clan.PlayerClan && oldKingdom == base.QuestGiver.MapFaction)
			{
				TextObject textObject = new TextObject("{=aQVdW6aC}You have left {QUEST_GIVER_FACTION}. Your agreement with {QUEST_GIVER.LINK} is terminated.");
				textObject.SetTextVariable("QUEST_GIVER_FACTION", base.QuestGiver.MapFaction.EncyclopediaLinkWithName);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
				CompleteQuestWithCancel(textObject);
			}
			else if (base.QuestGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				CompleteQuestWithCancel(_questCanceledWarDeclaredLog);
			}
		}

		private void OnArmyDispersed(Army army, Army.ArmyDispersionReason reason, bool isPlayersArmy)
		{
			if (army.ArmyOwner == base.QuestGiver)
			{
				AddLog(new TextObject("{=K2gsZOMb}Army is disbanded and your mission was canceled."));
				CompleteQuestWithCancel();
			}
		}

		private void OnInventoryExchange(List<(ItemRosterElement, int)> purchasedItems, List<(ItemRosterElement, int)> soldItems, bool isTrading)
		{
			CalculateAndUpdateRequestedItemsCountInPlayer();
		}

		private void CalculateAndUpdateRequestedItemsCountInPlayer(bool notifyPlayer = true)
		{
			ItemObject grain = DefaultItems.Grain;
			ItemObject @object = MBObjectManager.Instance.GetObject<ItemObject>("wine");
			_currentGrainAmount = MobileParty.MainParty.ItemRoster.GetItemNumber(grain);
			_currentLiveStockAmount = MobileParty.MainParty.ItemRoster.NumberOfLivestockAnimals;
			_currentWineAmount = MobileParty.MainParty.ItemRoster.GetItemNumber(@object);
			if (notifyPlayer)
			{
				if (_currentGrainAmount >= _requestedGrainAmount && _currentLiveStockAmount >= _requestedLiveStockAmount && _currentWineAmount >= _requestedWineAmount)
				{
					MBInformationManager.AddQuickInformation(new TextObject("{=uEV6kKdU}You have collected all the supplies that the army commander requested. Return to army and deliver the supplies"));
				}
				else if (_currentGrainAmount >= _requestedGrainAmount)
				{
					MBInformationManager.AddQuickInformation(new TextObject("{=8XpTvx1i}You have collected enough grains that the army commander requested. Return to army and deliver the supplies"));
				}
			}
			_currentGrainAmount = (int)MathF.Clamp(_currentGrainAmount, 0f, _requestedGrainAmount);
			_currentLiveStockAmount = (int)MathF.Clamp(_currentLiveStockAmount, 0f, _requestedLiveStockAmount);
			_currentWineAmount = (int)MathF.Clamp(_currentWineAmount, 0f, _requestedWineAmount);
			_grainLog?.UpdateCurrentProgress(_currentGrainAmount);
			_liveStockLog?.UpdateCurrentProgress(_currentLiveStockAmount);
			_wineLog?.UpdateCurrentProgress(_currentWineAmount);
		}

		private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
		{
			QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest(this, faction1, faction2, detail, _playerDeclaredWarQuestLogText, _questCanceledWarDeclaredLog);
		}

		public override void OnFailed()
		{
			RelationshipChangeWithQuestGiver = -2;
		}

		protected override void OnCompleteWithSuccess()
		{
			AddLog(_successQuestLogText);
		}

		protected override void OnTimedOut()
		{
			OnFailed();
			AddLog(_failQuestLogText);
		}

		public override void OnCanceled()
		{
		}
	}

	public class ArmyNeedsSuppliesIssueTypeDefiner : SaveableTypeDefiner
	{
		public ArmyNeedsSuppliesIssueTypeDefiner()
			: base(585800)
		{
		}

		protected override void DefineClassTypes()
		{
			AddClassDefinition(typeof(ArmyNeedsSuppliesIssue), 1);
			AddClassDefinition(typeof(ArmyNeedsSuppliesIssueQuest), 2);
		}
	}

	private const IssueBase.IssueFrequency ArmyNeedsSuppliesIssueFrequency = IssueBase.IssueFrequency.VeryCommon;

	private const float QuestDurationInDays = 15f;

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, OnCheckForIssue);
	}

	public void OnCheckForIssue(Hero hero)
	{
		if (ConditionsHold(hero))
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(OnStartIssue, typeof(ArmyNeedsSuppliesIssue), IssueBase.IssueFrequency.VeryCommon));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(ArmyNeedsSuppliesIssue), IssueBase.IssueFrequency.VeryCommon));
		}
	}

	private bool ConditionsHold(Hero issueGiver)
	{
		if (issueGiver.IsLord && issueGiver.MapFaction.IsKingdomFaction && issueGiver.PartyBelongedTo != null && issueGiver.PartyBelongedTo.Army != null && issueGiver.PartyBelongedTo.Army.ArmyOwner == issueGiver)
		{
			return issueGiver.PartyBelongedTo.Army.Cohesion > 80f;
		}
		return false;
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		return new ArmyNeedsSuppliesIssue(issueOwner);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}