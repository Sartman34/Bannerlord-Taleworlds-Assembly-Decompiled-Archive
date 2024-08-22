using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.Inventory;

public class InventoryManager
{
	public enum InventoryCategoryType
	{
		None = -1,
		All,
		Armors,
		Weapon,
		Shield,
		HorseCategory,
		Goods,
		CategoryTypeAmount
	}

	public delegate void DoneLogicExtrasDelegate();

	private class CaravanInventoryListener : InventoryListener
	{
		private MobileParty _caravan;

		public CaravanInventoryListener(MobileParty caravan)
		{
			_caravan = caravan;
		}

		public override int GetGold()
		{
			return _caravan.PartyTradeGold;
		}

		public override TextObject GetTraderName()
		{
			if (_caravan.LeaderHero == null)
			{
				return _caravan.Name;
			}
			return _caravan.LeaderHero.Name;
		}

		public override void SetGold(int gold)
		{
			_caravan.PartyTradeGold = gold;
		}

		public override PartyBase GetOppositeParty()
		{
			return _caravan.Party;
		}

		public override void OnTransaction()
		{
			throw new NotImplementedException();
		}
	}

	private class MerchantInventoryListener : InventoryListener
	{
		private SettlementComponent _settlementComponent;

		public MerchantInventoryListener(SettlementComponent settlementComponent)
		{
			_settlementComponent = settlementComponent;
		}

		public override TextObject GetTraderName()
		{
			return _settlementComponent.Owner.Name;
		}

		public override PartyBase GetOppositeParty()
		{
			return _settlementComponent.Owner;
		}

		public override int GetGold()
		{
			return _settlementComponent.Gold;
		}

		public override void SetGold(int gold)
		{
			_settlementComponent.ChangeGold(gold - _settlementComponent.Gold);
		}

		public override void OnTransaction()
		{
			throw new NotImplementedException();
		}
	}

	private InventoryMode _currentMode;

	private InventoryLogic _inventoryLogic;

	private DoneLogicExtrasDelegate _doneLogicExtrasDelegate;

	public InventoryMode CurrentMode => _currentMode;

	public static InventoryManager Instance => Campaign.Current.InventoryManager;

	public static InventoryLogic InventoryLogic => Instance._inventoryLogic;

	public void PlayerAcceptTradeOffer()
	{
		_inventoryLogic?.SetPlayerAcceptTraderOffer();
	}

	public void CloseInventoryPresentation(bool fromCancel)
	{
		if (_inventoryLogic.DoneLogic())
		{
			Game.Current.GameStateManager.PopState();
			_doneLogicExtrasDelegate?.Invoke();
			_doneLogicExtrasDelegate = null;
			_inventoryLogic = null;
		}
	}

	private void OpenInventoryPresentation(TextObject leftRosterName)
	{
		ItemRoster itemRoster = new ItemRoster();
		if (Game.Current.CheatMode)
		{
			TestCommonBase baseInstance = TestCommonBase.BaseInstance;
			if (baseInstance == null || !baseInstance.IsTestEnabled)
			{
				MBReadOnlyList<ItemObject> objectTypeList = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
				for (int i = 0; i != objectTypeList.Count; i++)
				{
					ItemObject item = objectTypeList[i];
					itemRoster.AddToCounts(item, 10);
				}
			}
		}
		_inventoryLogic = new InventoryLogic(null);
		_inventoryLogic.Initialize(itemRoster, MobileParty.MainParty, isTrading: false, isSpecialActionsPermitted: true, CharacterObject.PlayerCharacter, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false, leftRosterName);
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(_inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	private static IMarketData GetCurrentMarketData()
	{
		IMarketData marketData = null;
		if (Campaign.Current.GameMode == CampaignGameMode.Campaign)
		{
			Settlement settlement = MobileParty.MainParty.CurrentSettlement;
			if (settlement == null)
			{
				settlement = SettlementHelper.FindNearestTown();
			}
			if (settlement != null)
			{
				if (settlement.IsVillage)
				{
					marketData = settlement.Village.MarketData;
				}
				else if (settlement.IsTown)
				{
					marketData = settlement.Town.MarketData;
				}
			}
		}
		if (marketData == null)
		{
			marketData = new FakeMarketData();
		}
		return marketData;
	}

	public static void OpenScreenAsInventoryOfSubParty(MobileParty rightParty, MobileParty leftParty, DoneLogicExtrasDelegate doneLogicExtrasDelegate)
	{
		InventoryLogic inventoryLogic = new InventoryLogic(rightParty, rightParty.LeaderHero?.CharacterObject, leftParty.Party);
		inventoryLogic.Initialize(leftParty.ItemRoster, rightParty.ItemRoster, rightParty.MemberRoster, isTrading: false, isSpecialActionsPermitted: false, rightParty.LeaderHero?.CharacterObject, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false);
		Instance._doneLogicExtrasDelegate = doneLogicExtrasDelegate;
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void OpenScreenAsInventoryForCraftedItemDecomposition(MobileParty party, CharacterObject character, DoneLogicExtrasDelegate doneLogicExtrasDelegate)
	{
		Instance._inventoryLogic = new InventoryLogic(null);
		Instance._inventoryLogic.Initialize(new ItemRoster(), party.ItemRoster, party.MemberRoster, isTrading: false, isSpecialActionsPermitted: false, character, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false);
		Instance._doneLogicExtrasDelegate = doneLogicExtrasDelegate;
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void OpenScreenAsInventoryOf(MobileParty party, CharacterObject character)
	{
		Instance._inventoryLogic = new InventoryLogic(null);
		Instance._inventoryLogic.Initialize(new ItemRoster(), party.ItemRoster, party.MemberRoster, isTrading: false, isSpecialActionsPermitted: true, character, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false);
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void OpenScreenAsInventoryOf(PartyBase rightParty, PartyBase leftParty)
	{
		Instance._inventoryLogic = new InventoryLogic(leftParty);
		Instance._inventoryLogic.Initialize(leftParty.ItemRoster, rightParty.ItemRoster, rightParty.MemberRoster, isTrading: false, isSpecialActionsPermitted: false, rightParty.LeaderHero?.CharacterObject, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false, null, leftParty.MemberRoster);
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void OpenScreenAsInventory(DoneLogicExtrasDelegate doneLogicExtrasDelegate = null)
	{
		Instance._currentMode = InventoryMode.Default;
		Instance.OpenInventoryPresentation(new TextObject("{=02c5bQSM}Discard"));
		Instance._doneLogicExtrasDelegate = doneLogicExtrasDelegate;
	}

	public static void OpenCampaignBattleLootScreen()
	{
		OpenScreenAsLoot(new Dictionary<PartyBase, ItemRoster> { 
		{
			PartyBase.MainParty,
			MapEvent.PlayerMapEvent.ItemRosterForPlayerLootShare(PartyBase.MainParty)
		} });
	}

	public static void OpenScreenAsLoot(Dictionary<PartyBase, ItemRoster> itemRostersToLoot)
	{
		ItemRoster leftItemRoster = itemRostersToLoot[PartyBase.MainParty];
		Instance._currentMode = InventoryMode.Loot;
		Instance._inventoryLogic = new InventoryLogic(null);
		Instance._inventoryLogic.Initialize(leftItemRoster, MobileParty.MainParty.ItemRoster, MobileParty.MainParty.MemberRoster, isTrading: false, isSpecialActionsPermitted: true, CharacterObject.PlayerCharacter, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false);
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void OpenScreenAsStash(ItemRoster stash)
	{
		Instance._currentMode = InventoryMode.Stash;
		Instance._inventoryLogic = new InventoryLogic(null);
		Instance._inventoryLogic.Initialize(stash, MobileParty.MainParty, isTrading: false, isSpecialActionsPermitted: false, CharacterObject.PlayerCharacter, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false, new TextObject("{=nZbaYvVx}Stash"));
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void OpenScreenAsWarehouse(ItemRoster stash, InventoryLogic.CapacityData otherSideCapacity)
	{
		Instance._currentMode = InventoryMode.Warehouse;
		Instance._inventoryLogic = new InventoryLogic(null);
		Instance._inventoryLogic.Initialize(stash, MobileParty.MainParty, isTrading: false, isSpecialActionsPermitted: false, CharacterObject.PlayerCharacter, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false, new TextObject("{=anTRftmb}Warehouse"), null, otherSideCapacity);
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void OpenScreenAsReceiveItems(ItemRoster items, TextObject leftRosterName, DoneLogicExtrasDelegate doneLogicDelegate = null)
	{
		Instance._currentMode = InventoryMode.Default;
		Instance._inventoryLogic = new InventoryLogic(null);
		Instance._inventoryLogic.Initialize(items, MobileParty.MainParty.ItemRoster, MobileParty.MainParty.MemberRoster, isTrading: false, isSpecialActionsPermitted: true, CharacterObject.PlayerCharacter, InventoryCategoryType.None, GetCurrentMarketData(), useBasePrices: false, leftRosterName);
		Instance._doneLogicExtrasDelegate = doneLogicDelegate;
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void OpenTradeWithCaravanOrAlleyParty(MobileParty caravan, InventoryCategoryType merchantItemType = InventoryCategoryType.None)
	{
		Instance._currentMode = InventoryMode.Trade;
		Instance._inventoryLogic = new InventoryLogic(caravan.Party);
		Instance._inventoryLogic.Initialize(caravan.Party.ItemRoster, PartyBase.MainParty.ItemRoster, PartyBase.MainParty.MemberRoster, isTrading: true, isSpecialActionsPermitted: true, CharacterObject.PlayerCharacter, merchantItemType, GetCurrentMarketData(), useBasePrices: false);
		Instance._inventoryLogic.SetInventoryListener(new CaravanInventoryListener(caravan));
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
	}

	public static void ActivateTradeWithCurrentSettlement()
	{
		OpenScreenAsTrade(Settlement.CurrentSettlement.ItemRoster, Settlement.CurrentSettlement.SettlementComponent);
	}

	public static void OpenScreenAsTrade(ItemRoster leftRoster, SettlementComponent settlementComponent, InventoryCategoryType merchantItemType = InventoryCategoryType.None, DoneLogicExtrasDelegate doneLogicExtrasDelegate = null)
	{
		Instance._currentMode = InventoryMode.Trade;
		Instance._inventoryLogic = new InventoryLogic(settlementComponent.Owner);
		Instance._inventoryLogic.Initialize(leftRoster, PartyBase.MainParty.ItemRoster, PartyBase.MainParty.MemberRoster, isTrading: true, isSpecialActionsPermitted: true, CharacterObject.PlayerCharacter, merchantItemType, GetCurrentMarketData(), useBasePrices: false);
		Instance._inventoryLogic.SetInventoryListener(new MerchantInventoryListener(settlementComponent));
		Instance._doneLogicExtrasDelegate = doneLogicExtrasDelegate;
		InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
		inventoryState.InitializeLogic(Instance._inventoryLogic);
		Game.Current.GameStateManager.PushState(inventoryState);
		if (inventoryState.Handler != null)
		{
			inventoryState.Handler.FilterInventoryAtOpening(merchantItemType);
		}
		else
		{
			Debug.FailedAssert("Inventory State handler is not initialized when filtering inventory", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Inventory\\InventoryManager.cs", "OpenScreenAsTrade", 395);
		}
	}

	public static InventoryItemType GetInventoryItemTypeOfItem(ItemObject item)
	{
		if (item != null)
		{
			switch (item.ItemType)
			{
			case ItemObject.ItemTypeEnum.Horse:
				return InventoryItemType.Horse;
			case ItemObject.ItemTypeEnum.OneHandedWeapon:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.TwoHandedWeapon:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Polearm:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Arrows:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Bolts:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Shield:
				return InventoryItemType.Shield;
			case ItemObject.ItemTypeEnum.Bow:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Crossbow:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Thrown:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Goods:
				return InventoryItemType.Goods;
			case ItemObject.ItemTypeEnum.HeadArmor:
				return InventoryItemType.HeadArmor;
			case ItemObject.ItemTypeEnum.BodyArmor:
				return InventoryItemType.BodyArmor;
			case ItemObject.ItemTypeEnum.LegArmor:
				return InventoryItemType.LegArmor;
			case ItemObject.ItemTypeEnum.HandArmor:
				return InventoryItemType.HandArmor;
			case ItemObject.ItemTypeEnum.Pistol:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Musket:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Bullets:
				return InventoryItemType.Weapon;
			case ItemObject.ItemTypeEnum.Animal:
				return InventoryItemType.Animal;
			case ItemObject.ItemTypeEnum.Book:
				return InventoryItemType.Book;
			case ItemObject.ItemTypeEnum.HorseHarness:
				return InventoryItemType.HorseHarness;
			case ItemObject.ItemTypeEnum.Cape:
				return InventoryItemType.Cape;
			case ItemObject.ItemTypeEnum.Banner:
				return InventoryItemType.Banner;
			}
		}
		return InventoryItemType.None;
	}
}
