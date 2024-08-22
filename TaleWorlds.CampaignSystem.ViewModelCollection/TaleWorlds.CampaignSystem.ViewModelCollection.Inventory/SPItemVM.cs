using System;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

public class SPItemVM : ItemVM
{
	public enum ProfitTypes
	{
		HighLoss = -2,
		Loss,
		Default,
		Profit,
		HighProfit
	}

	public static Action<SPItemVM> OnFocus;

	public static Action<SPItemVM, bool> ProcessSellItem;

	public static Action<SPItemVM> ProcessItemSlaughter;

	public static Action<SPItemVM> ProcessItemDonate;

	public static Action<SPItemVM, bool> ProcessLockItem;

	private readonly string _fiveStackShortcutKeyText;

	private readonly string _entireStackShortcutKeyText;

	private readonly InventoryMode _usageType;

	private Concept _tradeGoodConceptObj;

	private Concept _itemConceptObj;

	private InventoryLogic _inventoryLogic;

	private bool _isFocused;

	private int _level;

	private bool _isTransferable;

	private bool _isCivilianItem;

	private bool _isGenderDifferent;

	private bool _isEquipableItem;

	private bool _canCharacterUseItem;

	private bool _isLocked;

	private bool _isArtifact;

	private bool _canBeSlaughtered;

	private bool _canBeDonated;

	private int _count;

	private int _profitType = -5;

	private int _transactionCount;

	private int _totalCost;

	private bool _isTransferButtonHighlighted;

	private bool _isItemHighlightEnabled;

	private bool _isNew;

	private InventoryTradeVM _tradeData;

	public InventoryLogic.InventorySide InventorySide { get; private set; }

	[DataSourceProperty]
	public bool IsFocused
	{
		get
		{
			return _isFocused;
		}
		set
		{
			if (value != _isFocused)
			{
				_isFocused = value;
				OnPropertyChangedWithValue(value, "IsFocused");
			}
		}
	}

	[DataSourceProperty]
	public bool IsArtifact
	{
		get
		{
			return _isArtifact;
		}
		set
		{
			if (value != _isArtifact)
			{
				_isArtifact = value;
				OnPropertyChangedWithValue(value, "IsArtifact");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTransferable
	{
		get
		{
			return _isTransferable;
		}
		set
		{
			if (value != _isTransferable)
			{
				_isTransferable = value;
				OnPropertyChangedWithValue(value, "IsTransferable");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTransferButtonHighlighted
	{
		get
		{
			return _isTransferButtonHighlighted;
		}
		set
		{
			if (value != _isTransferButtonHighlighted)
			{
				_isTransferButtonHighlighted = value;
				OnPropertyChangedWithValue(value, "IsTransferButtonHighlighted");
			}
		}
	}

	[DataSourceProperty]
	public bool IsItemHighlightEnabled
	{
		get
		{
			return _isItemHighlightEnabled;
		}
		set
		{
			if (value != _isItemHighlightEnabled)
			{
				_isItemHighlightEnabled = value;
				OnPropertyChangedWithValue(value, "IsItemHighlightEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCivilianItem
	{
		get
		{
			return _isCivilianItem;
		}
		set
		{
			if (value != _isCivilianItem)
			{
				_isCivilianItem = value;
				OnPropertyChangedWithValue(value, "IsCivilianItem");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNew
	{
		get
		{
			return _isNew;
		}
		set
		{
			if (value != _isNew)
			{
				_isNew = value;
				OnPropertyChangedWithValue(value, "IsNew");
			}
		}
	}

	[DataSourceProperty]
	public bool IsGenderDifferent
	{
		get
		{
			return _isGenderDifferent;
		}
		set
		{
			if (value != _isGenderDifferent)
			{
				_isGenderDifferent = value;
				OnPropertyChangedWithValue(value, "IsGenderDifferent");
			}
		}
	}

	[DataSourceProperty]
	public bool CanBeSlaughtered
	{
		get
		{
			return _canBeSlaughtered;
		}
		set
		{
			if (value != _canBeSlaughtered)
			{
				_canBeSlaughtered = value;
				OnPropertyChangedWithValue(value, "CanBeSlaughtered");
			}
		}
	}

	[DataSourceProperty]
	public bool CanBeDonated
	{
		get
		{
			return _canBeDonated;
		}
		set
		{
			if (value != _canBeDonated)
			{
				_canBeDonated = value;
				OnPropertyChangedWithValue(value, "CanBeDonated");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEquipableItem
	{
		get
		{
			return _isEquipableItem;
		}
		set
		{
			if (value != _isEquipableItem)
			{
				_isEquipableItem = value;
				OnPropertyChangedWithValue(value, "IsEquipableItem");
			}
		}
	}

	[DataSourceProperty]
	public bool CanCharacterUseItem
	{
		get
		{
			return _canCharacterUseItem;
		}
		set
		{
			if (value != _canCharacterUseItem)
			{
				_canCharacterUseItem = value;
				OnPropertyChangedWithValue(value, "CanCharacterUseItem");
			}
		}
	}

	[DataSourceProperty]
	public bool IsLocked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			if (value != _isLocked)
			{
				_isLocked = value;
				OnPropertyChangedWithValue(value, "IsLocked");
				ProcessLockItem(this, value);
			}
		}
	}

	[DataSourceProperty]
	public int ItemCount
	{
		get
		{
			return _count;
		}
		set
		{
			if (value != _count)
			{
				_count = value;
				OnPropertyChangedWithValue(value, "ItemCount");
				UpdateTotalCost();
				UpdateTradeData(forceUpdateAmounts: false);
			}
		}
	}

	[DataSourceProperty]
	public int ItemLevel
	{
		get
		{
			return _level;
		}
		set
		{
			if (value != _level)
			{
				_level = value;
				OnPropertyChangedWithValue(value, "ItemLevel");
			}
		}
	}

	[DataSourceProperty]
	public int ProfitType
	{
		get
		{
			return _profitType;
		}
		set
		{
			if (value != _profitType)
			{
				_profitType = value;
				OnPropertyChangedWithValue(value, "ProfitType");
			}
		}
	}

	[DataSourceProperty]
	public int TransactionCount
	{
		get
		{
			return _transactionCount;
		}
		set
		{
			if (value != _transactionCount)
			{
				_transactionCount = value;
				OnPropertyChangedWithValue(value, "TransactionCount");
				UpdateTotalCost();
			}
		}
	}

	[DataSourceProperty]
	public int TotalCost
	{
		get
		{
			return _totalCost;
		}
		set
		{
			if (value != _totalCost)
			{
				_totalCost = value;
				OnPropertyChangedWithValue(value, "TotalCost");
			}
		}
	}

	[DataSourceProperty]
	public InventoryTradeVM TradeData
	{
		get
		{
			return _tradeData;
		}
		set
		{
			if (value != _tradeData)
			{
				_tradeData = value;
				OnPropertyChangedWithValue(value, "TradeData");
			}
		}
	}

	public SPItemVM()
	{
		base.StringId = "";
		base.ImageIdentifier = new ImageIdentifierVM();
		_itemType = EquipmentIndex.None;
	}

	public SPItemVM(InventoryLogic inventoryLogic, bool isHeroFemale, bool canCharacterUseItem, InventoryMode usageType, ItemRosterElement newItem, InventoryLogic.InventorySide inventorySide, string fiveStackShortcutKeyText, string entireStackShortcutKeyText, int itemCost = 0, EquipmentIndex? itemType = EquipmentIndex.None)
	{
		if (newItem.EquipmentElement.Item != null)
		{
			_fiveStackShortcutKeyText = fiveStackShortcutKeyText;
			_entireStackShortcutKeyText = entireStackShortcutKeyText;
			_usageType = usageType;
			_tradeGoodConceptObj = Concept.All.SingleOrDefault((Concept c) => c.StringId == "str_game_objects_trade_goods");
			_itemConceptObj = Concept.All.SingleOrDefault((Concept c) => c.StringId == "str_game_objects_item");
			_inventoryLogic = inventoryLogic;
			ItemRosterElement = new ItemRosterElement(newItem.EquipmentElement, newItem.Amount);
			base.ItemCost = itemCost;
			ItemCount = newItem.Amount;
			TransactionCount = 1;
			ItemLevel = newItem.EquipmentElement.Item.Difficulty;
			InventorySide = inventorySide;
			if (itemType.HasValue && itemType != EquipmentIndex.None)
			{
				_itemType = itemType.Value;
			}
			SetItemTypeId();
			base.ItemDescription = newItem.EquipmentElement.GetModifiedItemName().ToString();
			base.StringId = CampaignUIHelper.GetItemLockStringID(newItem.EquipmentElement);
			base.ImageIdentifier = new ImageIdentifierVM(newItem.EquipmentElement.Item, Clan.PlayerClan?.Banner.Serialize());
			IsCivilianItem = newItem.EquipmentElement.Item.ItemFlags.HasAnyFlag(ItemFlags.Civilian);
			IsGenderDifferent = (isHeroFemale && ItemRosterElement.EquipmentElement.Item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByFemale)) || (!isHeroFemale && ItemRosterElement.EquipmentElement.Item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByMale));
			CanCharacterUseItem = canCharacterUseItem;
			IsArtifact = newItem.EquipmentElement.Item.IsUniqueItem;
			UpdateCanBeSlaughtered();
			UpdateHintTexts();
			CanBeDonated = _inventoryLogic?.CanDonateItem(ItemRosterElement, InventorySide) ?? false;
			TradeData = new InventoryTradeVM(_inventoryLogic, ItemRosterElement, inventorySide, OnTradeApplyTransaction);
			IsTransferable = !ItemRosterElement.EquipmentElement.IsQuestItem;
			TradeData.IsTradeable = IsTransferable;
			IsEquipableItem = (InventoryManager.GetInventoryItemTypeOfItem(newItem.EquipmentElement.Item) & InventoryItemType.Equipable) != 0;
			UpdateProfitType();
		}
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		if (ItemRosterElement.EquipmentElement.Item != null)
		{
			base.ItemDescription = ItemRosterElement.EquipmentElement.GetModifiedItemName()?.ToString() ?? "";
		}
		else
		{
			base.ItemDescription = "";
		}
	}

	public void RefreshWith(SPItemVM itemVM, InventoryLogic.InventorySide inventorySide)
	{
		InventorySide = inventorySide;
		if (itemVM == null)
		{
			Reset();
			return;
		}
		base.ItemDescription = itemVM.ItemDescription;
		base.ItemCost = itemVM.ItemCost;
		base.TypeId = itemVM.TypeId;
		_itemType = itemVM.ItemType;
		ItemCount = itemVM.ItemCount;
		TransactionCount = itemVM.TransactionCount;
		ItemLevel = itemVM.ItemLevel;
		base.StringId = itemVM.StringId;
		base.ImageIdentifier = itemVM.ImageIdentifier.Clone();
		ItemRosterElement = itemVM.ItemRosterElement;
		IsCivilianItem = itemVM.IsCivilianItem;
		IsGenderDifferent = itemVM.IsGenderDifferent;
		IsEquipableItem = itemVM.IsEquipableItem;
		CanCharacterUseItem = CanCharacterUseItem;
		IsArtifact = itemVM.IsArtifact;
		UpdateCanBeSlaughtered();
		UpdateHintTexts();
		CanBeDonated = _inventoryLogic?.CanDonateItem(ItemRosterElement, InventorySide) ?? false;
		TradeData = new InventoryTradeVM(_inventoryLogic, itemVM.ItemRosterElement, inventorySide, OnTradeApplyTransaction);
		UpdateProfitType();
	}

	private void Reset()
	{
		base.ItemDescription = "";
		base.ItemCost = 0;
		base.TypeId = 0;
		_itemType = EquipmentIndex.None;
		ItemCount = 0;
		TransactionCount = 0;
		ItemLevel = 0;
		base.StringId = "";
		base.ImageIdentifier = new ImageIdentifierVM();
		ItemRosterElement = default(ItemRosterElement);
		ProfitType = 0;
		IsCivilianItem = true;
		IsGenderDifferent = false;
		IsEquipableItem = true;
		IsArtifact = false;
		TradeData = new InventoryTradeVM(_inventoryLogic, ItemRosterElement, InventoryLogic.InventorySide.None, OnTradeApplyTransaction);
	}

	private void UpdateProfitType()
	{
		ProfitType = 0;
		if (Campaign.Current == null)
		{
			return;
		}
		if (InventorySide == InventoryLogic.InventorySide.PlayerInventory)
		{
			Hero mainHero = Hero.MainHero;
			if (mainHero == null || !mainHero.GetPerkValue(DefaultPerks.Trade.Appraiser))
			{
				Hero mainHero2 = Hero.MainHero;
				if (mainHero2 == null || !mainHero2.GetPerkValue(DefaultPerks.Trade.WholeSeller))
				{
					return;
				}
			}
			IPlayerTradeBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IPlayerTradeBehavior>();
			if (campaignBehavior != null)
			{
				int num = -campaignBehavior.GetProjectedProfit(ItemRosterElement, base.ItemCost) + base.ItemCost;
				ProfitType = (int)GetProfitTypeFromDiff(num, base.ItemCost);
			}
		}
		else
		{
			if (InventorySide != 0 || Settlement.CurrentSettlement == null || (!Settlement.CurrentSettlement.IsFortification && !Settlement.CurrentSettlement.IsVillage))
			{
				return;
			}
			Hero mainHero3 = Hero.MainHero;
			if (mainHero3 == null || !mainHero3.GetPerkValue(DefaultPerks.Trade.CaravanMaster))
			{
				Hero mainHero4 = Hero.MainHero;
				if (mainHero4 == null || !mainHero4.GetPerkValue(DefaultPerks.Trade.MarketDealer))
				{
					return;
				}
			}
			float averagePriceFactorItemCategory = _inventoryLogic.GetAveragePriceFactorItemCategory(ItemRosterElement.EquipmentElement.Item.ItemCategory);
			Town town = (Settlement.CurrentSettlement.IsVillage ? Settlement.CurrentSettlement.Village.Bound.Town : Settlement.CurrentSettlement.Town);
			if (averagePriceFactorItemCategory != -99f)
			{
				ProfitType = (int)GetProfitTypeFromDiff(town.MarketData.GetPriceFactor(ItemRosterElement.EquipmentElement.Item.ItemCategory), averagePriceFactorItemCategory);
			}
		}
	}

	public void ExecuteBuySingle()
	{
		TransactionCount = 1;
		ItemVM.ProcessBuyItem(this, arg2: false);
	}

	public void ExecuteSellSingle()
	{
		TransactionCount = 1;
		ProcessSellItem(this, arg2: false);
	}

	private void OnTradeApplyTransaction(int amount, bool isBuying)
	{
		TransactionCount = amount;
		if (isBuying)
		{
			ItemVM.ProcessBuyItem(this, arg2: true);
		}
		else
		{
			ProcessSellItem(this, arg2: true);
		}
	}

	public void ExecuteSellItem()
	{
		ProcessSellItem(this, arg2: false);
	}

	public void ExecuteConcept()
	{
		if (_tradeGoodConceptObj != null)
		{
			ItemObject item = ItemRosterElement.EquipmentElement.Item;
			if (item != null && item.Type == ItemObject.ItemTypeEnum.Goods)
			{
				Campaign.Current.EncyclopediaManager.GoToLink(_tradeGoodConceptObj.EncyclopediaLink);
				return;
			}
		}
		if (_itemConceptObj != null)
		{
			Campaign.Current.EncyclopediaManager.GoToLink(_itemConceptObj.EncyclopediaLink);
		}
	}

	public void ExecuteResetTrade()
	{
		TradeData.ExecuteReset();
	}

	private void UpdateTotalCost()
	{
		if (TransactionCount > 0 && _inventoryLogic != null && InventorySide != InventoryLogic.InventorySide.Equipment)
		{
			TotalCost = _inventoryLogic.GetItemTotalPrice(ItemRosterElement, TransactionCount, out var _, InventorySide == InventoryLogic.InventorySide.OtherInventory);
		}
	}

	public void UpdateTradeData(bool forceUpdateAmounts)
	{
		TradeData?.UpdateItemData(ItemRosterElement, InventorySide, forceUpdateAmounts);
		UpdateProfitType();
	}

	public void ExecuteSlaughterItem()
	{
		if (CanBeSlaughtered)
		{
			ProcessItemSlaughter(this);
		}
	}

	public void ExecuteDonateItem()
	{
		if (CanBeDonated)
		{
			ProcessItemDonate(this);
		}
	}

	public void ExecuteSetFocused()
	{
		IsFocused = true;
		OnFocus?.Invoke(this);
	}

	public void ExecuteSetUnfocused()
	{
		IsFocused = false;
		OnFocus?.Invoke(null);
	}

	public void UpdateCanBeSlaughtered()
	{
		InventoryLogic inventoryLogic = _inventoryLogic;
		CanBeSlaughtered = inventoryLogic != null && inventoryLogic.CanSlaughterItem(ItemRosterElement, InventorySide) && !ItemRosterElement.EquipmentElement.IsQuestItem;
	}

	private string GetStackModifierString()
	{
		GameTexts.SetVariable("newline", "\n");
		GameTexts.SetVariable("STR1", "");
		GameTexts.SetVariable("STR2", "");
		if (!string.IsNullOrEmpty(_entireStackShortcutKeyText))
		{
			GameTexts.SetVariable("KEY_NAME", _entireStackShortcutKeyText);
			string content = ((InventorySide == InventoryLogic.InventorySide.PlayerInventory) ? GameTexts.FindText("str_entire_stack_shortcut_discard_items").ToString() : GameTexts.FindText("str_entire_stack_shortcut_take_items").ToString());
			GameTexts.SetVariable("STR1", content);
			GameTexts.SetVariable("STR2", "");
			if (ItemCount >= 5 && !string.IsNullOrEmpty(_fiveStackShortcutKeyText))
			{
				GameTexts.SetVariable("KEY_NAME", _fiveStackShortcutKeyText);
				string content2 = ((InventorySide == InventoryLogic.InventorySide.PlayerInventory) ? GameTexts.FindText("str_five_stack_shortcut_discard_items").ToString() : GameTexts.FindText("str_five_stack_shortcut_take_items").ToString());
				GameTexts.SetVariable("STR2", content2);
			}
		}
		return GameTexts.FindText("str_string_newline_string").ToString();
	}

	public void UpdateHintTexts()
	{
		base.SlaughterHint = new BasicTooltipViewModel(delegate
		{
			string stackModifierString6 = GetStackModifierString();
			GameTexts.SetVariable("STR1", GameTexts.FindText("str_inventory_slaughter"));
			GameTexts.SetVariable("STR2", stackModifierString6);
			return GameTexts.FindText("str_string_newline_string").ToString();
		});
		base.DonateHint = new BasicTooltipViewModel(delegate
		{
			string stackModifierString5 = GetStackModifierString();
			GameTexts.SetVariable("STR1", GameTexts.FindText("str_inventory_donate"));
			GameTexts.SetVariable("STR2", stackModifierString5);
			return GameTexts.FindText("str_string_newline_string").ToString();
		});
		base.PreviewHint = new HintViewModel(GameTexts.FindText("str_inventory_preview"));
		base.EquipHint = new HintViewModel(GameTexts.FindText("str_inventory_equip"));
		base.LockHint = new HintViewModel(GameTexts.FindText("str_inventory_lock"));
		if (_usageType == InventoryMode.Loot || _usageType == InventoryMode.Stash)
		{
			base.BuyAndEquipHint = new BasicTooltipViewModel(() => GameTexts.FindText("str_inventory_take_and_equip").ToString());
			base.SellHint = new BasicTooltipViewModel(delegate
			{
				string stackModifierString4 = GetStackModifierString();
				GameTexts.SetVariable("STR1", GameTexts.FindText("str_inventory_give"));
				GameTexts.SetVariable("STR2", stackModifierString4);
				return GameTexts.FindText("str_string_newline_string").ToString();
			});
			base.BuyHint = new BasicTooltipViewModel(delegate
			{
				string stackModifierString3 = GetStackModifierString();
				GameTexts.SetVariable("STR1", GameTexts.FindText("str_inventory_take"));
				GameTexts.SetVariable("STR2", stackModifierString3);
				return GameTexts.FindText("str_string_newline_string").ToString();
			});
		}
		else if (_usageType == InventoryMode.Default)
		{
			base.BuyAndEquipHint = new BasicTooltipViewModel(() => GameTexts.FindText("str_inventory_take_and_equip").ToString());
			base.SellHint = new BasicTooltipViewModel(delegate
			{
				string stackModifierString2 = GetStackModifierString();
				GameTexts.SetVariable("STR1", GameTexts.FindText("str_inventory_discard"));
				GameTexts.SetVariable("STR2", stackModifierString2);
				return GameTexts.FindText("str_string_newline_string").ToString();
			});
			base.BuyHint = new BasicTooltipViewModel(delegate
			{
				string stackModifierString = GetStackModifierString();
				GameTexts.SetVariable("STR1", GameTexts.FindText("str_inventory_take"));
				GameTexts.SetVariable("STR2", stackModifierString);
				return GameTexts.FindText("str_string_newline_string").ToString();
			});
		}
		else
		{
			base.BuyAndEquipHint = new BasicTooltipViewModel(() => GameTexts.FindText("str_inventory_buy_and_equip").ToString());
			base.SellHint = new BasicTooltipViewModel(delegate
			{
				GameTexts.SetVariable("STR1", GameTexts.FindText("str_inventory_sell"));
				GameTexts.SetVariable("STR2", string.Empty);
				return GameTexts.FindText("str_string_newline_string").ToString();
			});
			base.BuyHint = new BasicTooltipViewModel(delegate
			{
				GameTexts.SetVariable("STR1", GameTexts.FindText("str_inventory_buy"));
				GameTexts.SetVariable("STR2", string.Empty);
				return GameTexts.FindText("str_string_newline_string").ToString();
			});
		}
	}

	public static ProfitTypes GetProfitTypeFromDiff(float averageValue, float currentValue)
	{
		if (averageValue != 0f)
		{
			if (averageValue < currentValue * 0.8f)
			{
				return ProfitTypes.HighProfit;
			}
			if (averageValue < currentValue * 0.95f)
			{
				return ProfitTypes.Profit;
			}
			if (averageValue > currentValue * 1.05f)
			{
				return ProfitTypes.Loss;
			}
			if (averageValue > currentValue * 1.2f)
			{
				return ProfitTypes.HighLoss;
			}
			return ProfitTypes.Default;
		}
		return ProfitTypes.Default;
	}
}
