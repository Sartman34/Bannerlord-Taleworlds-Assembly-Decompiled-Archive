using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

public class ItemMenuVM : ViewModel
{
	private readonly TextObject _swingDamageText = GameTexts.FindText("str_swing_damage");

	private readonly TextObject _swingSpeedText = new TextObject("{=345a87fcc69f626ae3916939ef2fc135}Swing Speed: ");

	private readonly TextObject _weaponTierText = new TextObject("{=weaponTier}Weapon Tier: ");

	private readonly TextObject _armorTierText = new TextObject("{=armorTier}Armor Tier: ");

	private readonly TextObject _horseTierText = new TextObject("{=mountTier}Mount Tier: ");

	private readonly TextObject _horseTypeText = new TextObject("{=9sxECG6e}Mount Type: ");

	private readonly TextObject _chargeDamageText = new TextObject("{=c7638a0869219ae845de0f660fd57a9d}Charge Damage: ");

	private readonly TextObject _hitPointsText = GameTexts.FindText("str_hit_points");

	private readonly TextObject _speedText = new TextObject("{=74dc1908cb0b990e80fb977b5a0ef10d}Speed: ");

	private readonly TextObject _maneuverText = new TextObject("{=3025020b83b218707499f0de3135ed0a}Maneuver: ");

	private readonly TextObject _thrustSpeedText = GameTexts.FindText("str_thrust_speed");

	private readonly TextObject _thrustDamageText = GameTexts.FindText("str_thrust_damage");

	private readonly TextObject _lengthText = new TextObject("{=c6e4c8588ca9e42f6e1b47b11f0f367b}Length: ");

	private readonly TextObject _weightText = GameTexts.FindText("str_weight_text");

	private readonly TextObject _handlingText = new TextObject("{=ca8b1e8956057b831dfc665f54bae4b0}Handling: ");

	private readonly TextObject _weaponLengthText = new TextObject("{=5fa36d2798479803b4518a64beb4d732}Weapon Length: ");

	private readonly TextObject _damageText = new TextObject("{=c9c5dfed2ca6bcb7a73d905004c97b23}Damage: ");

	private readonly TextObject _missileSpeedText = GameTexts.FindText("str_missile_speed");

	private readonly TextObject _accuracyText = new TextObject("{=5dec16fa0be433ade3c4cb0074ef366d}Accuracy: ");

	private readonly TextObject _stackAmountText = new TextObject("{=05fdfc6e238429753ef282f2ce97c1f8}Stack Amount: ");

	private readonly TextObject _ammoLimitText = new TextObject("{=6adabc1f82216992571c3e22abc164d7}Ammo Limit: ");

	private readonly TextObject _requiresText = new TextObject("{=154a34f8caccfc833238cc89d38861e8}Requires: ");

	private readonly TextObject _foodText = new TextObject("{=qSi4DlT4}Food");

	private readonly TextObject _partyMoraleText = new TextObject("{=a241aacb1780599430c79fd9f667b67f}Party Morale: ");

	private readonly TextObject _typeText = new TextObject("{=08abd5af7774d311cadc3ed900b47754}Type: ");

	private readonly TextObject _tradeRumorsText = new TextObject("{=f2971dc587a9777223ad2d7be236fb05}Trade Rumors");

	private readonly TextObject _classText = new TextObject("{=8cad4a279770f269c4bb0dc7a357ee1e}Class: ");

	private readonly TextObject _headArmorText = GameTexts.FindText("str_head_armor");

	private readonly TextObject _horseArmorText = new TextObject("{=305cf7f98458b22e9af72b60a131714f}Horse Armor: ");

	private readonly TextObject _bodyArmorText = GameTexts.FindText("str_body_armor");

	private readonly TextObject _legArmorText = GameTexts.FindText("str_leg_armor");

	private readonly TextObject _armArmorText = new TextObject("{=cf61cce254c7dca65be9bebac7fb9bf5}Arm Armor: ");

	private readonly TextObject _bannerEffectText = new TextObject("{=DbXZjPdf}Banner Effect: ");

	private readonly TextObject _noneText = new TextObject("{=koX9okuG}None");

	private readonly string GoldIcon = "<img src=\"General\\Icons\\Coin@2x\" extend=\"8\"/>";

	private readonly Color ConsumableColor = Color.FromUint(4290873921u);

	private readonly Color TitleColor = Color.FromUint(4293446041u);

	private TooltipProperty _costProperty;

	private InventoryLogic _inventoryLogic;

	private Action<ItemVM, int> _resetComparedItems;

	private readonly Func<WeaponComponentData, ItemObject.ItemUsageSetFlags> _getItemUsageSetFlags;

	private readonly Func<EquipmentIndex, SPItemVM> _getEquipmentAtIndex;

	private ItemVM _targetItem;

	private bool _isComparing;

	private ItemVM _comparedItem;

	private bool _isPlayerItem;

	private BasicCharacterObject _character;

	private ImageIdentifierVM _imageIdentifier;

	private ImageIdentifierVM _comparedImageIdentifier;

	private string _itemName;

	private string _comparedItemName;

	private MBBindingList<ItemMenuTooltipPropertyVM> _comparedItemProperties;

	private MBBindingList<ItemMenuTooltipPropertyVM> _targetItemProperties;

	private bool _isInitializationOver;

	private int _transactionTotalCost = -1;

	private MBBindingList<ItemFlagVM> _targetItemFlagList;

	private MBBindingList<ItemFlagVM> _comparedItemFlagList;

	private int _alternativeUsageIndex;

	private MBBindingList<StringItemWithHintVM> _alternativeUsages;

	private ITradeRumorCampaignBehavior _tradeRumorsBehavior;

	[DataSourceProperty]
	public bool IsComparing
	{
		get
		{
			return _isComparing;
		}
		set
		{
			if (value != _isComparing)
			{
				_isComparing = value;
				OnPropertyChangedWithValue(value, "IsComparing");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerItem
	{
		get
		{
			return _isPlayerItem;
		}
		set
		{
			if (value != _isPlayerItem)
			{
				_isPlayerItem = value;
				OnPropertyChangedWithValue(value, "IsPlayerItem");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM ImageIdentifier
	{
		get
		{
			return _imageIdentifier;
		}
		set
		{
			if (value != _imageIdentifier)
			{
				_imageIdentifier = value;
				OnPropertyChangedWithValue(value, "ImageIdentifier");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM ComparedImageIdentifier
	{
		get
		{
			return _comparedImageIdentifier;
		}
		set
		{
			if (value != _comparedImageIdentifier)
			{
				_comparedImageIdentifier = value;
				OnPropertyChangedWithValue(value, "ComparedImageIdentifier");
			}
		}
	}

	[DataSourceProperty]
	public int TransactionTotalCost
	{
		get
		{
			return _transactionTotalCost;
		}
		set
		{
			if (value != _transactionTotalCost)
			{
				_transactionTotalCost = value;
				OnPropertyChangedWithValue(value, "TransactionTotalCost");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInitializationOver
	{
		get
		{
			return _isInitializationOver;
		}
		set
		{
			if (value != _isInitializationOver)
			{
				_isInitializationOver = value;
				OnPropertyChangedWithValue(value, "IsInitializationOver");
			}
		}
	}

	[DataSourceProperty]
	public string ItemName
	{
		get
		{
			return _itemName;
		}
		set
		{
			if (value != _itemName)
			{
				_itemName = value;
				OnPropertyChangedWithValue(value, "ItemName");
			}
		}
	}

	[DataSourceProperty]
	public string ComparedItemName
	{
		get
		{
			return _comparedItemName;
		}
		set
		{
			if (value != _comparedItemName)
			{
				_comparedItemName = value;
				OnPropertyChangedWithValue(value, "ComparedItemName");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ItemMenuTooltipPropertyVM> TargetItemProperties
	{
		get
		{
			return _targetItemProperties;
		}
		set
		{
			if (value != _targetItemProperties)
			{
				_targetItemProperties = value;
				OnPropertyChangedWithValue(value, "TargetItemProperties");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ItemMenuTooltipPropertyVM> ComparedItemProperties
	{
		get
		{
			return _comparedItemProperties;
		}
		set
		{
			if (value != _comparedItemProperties)
			{
				_comparedItemProperties = value;
				OnPropertyChangedWithValue(value, "ComparedItemProperties");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ItemFlagVM> TargetItemFlagList
	{
		get
		{
			return _targetItemFlagList;
		}
		set
		{
			if (value != _targetItemFlagList)
			{
				_targetItemFlagList = value;
				OnPropertyChangedWithValue(value, "TargetItemFlagList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ItemFlagVM> ComparedItemFlagList
	{
		get
		{
			return _comparedItemFlagList;
		}
		set
		{
			if (value != _comparedItemFlagList)
			{
				_comparedItemFlagList = value;
				OnPropertyChangedWithValue(value, "ComparedItemFlagList");
			}
		}
	}

	[DataSourceProperty]
	public int AlternativeUsageIndex
	{
		get
		{
			return _alternativeUsageIndex;
		}
		set
		{
			if (value != _alternativeUsageIndex)
			{
				_alternativeUsageIndex = value;
				OnPropertyChangedWithValue(value, "AlternativeUsageIndex");
				AlternativeUsageIndexUpdated();
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringItemWithHintVM> AlternativeUsages
	{
		get
		{
			return _alternativeUsages;
		}
		set
		{
			if (value != _alternativeUsages)
			{
				_alternativeUsages = value;
				OnPropertyChangedWithValue(value, "AlternativeUsages");
			}
		}
	}

	public ItemMenuVM(Action<ItemVM, int> resetComparedItems, InventoryLogic inventoryLogic, Func<WeaponComponentData, ItemObject.ItemUsageSetFlags> getItemUsageSetFlags, Func<EquipmentIndex, SPItemVM> getEquipmentAtIndex)
	{
		_resetComparedItems = resetComparedItems;
		_inventoryLogic = inventoryLogic;
		_comparedItemProperties = new MBBindingList<ItemMenuTooltipPropertyVM>();
		_targetItemProperties = new MBBindingList<ItemMenuTooltipPropertyVM>();
		_getItemUsageSetFlags = getItemUsageSetFlags;
		_getEquipmentAtIndex = getEquipmentAtIndex;
		TargetItemFlagList = new MBBindingList<ItemFlagVM>();
		ComparedItemFlagList = new MBBindingList<ItemFlagVM>();
		AlternativeUsages = new MBBindingList<StringItemWithHintVM>();
		_tradeRumorsBehavior = Campaign.Current.GetCampaignBehavior<ITradeRumorCampaignBehavior>();
	}

	public void SetItem(SPItemVM item, ItemVM comparedItem = null, BasicCharacterObject character = null, int alternativeUsageIndex = 0)
	{
		IsInitializationOver = false;
		_targetItem = item;
		_comparedItem = comparedItem;
		IsComparing = _comparedItem?.ItemRosterElement.EquipmentElement.Item != null;
		IsPlayerItem = item.InventorySide == InventoryLogic.InventorySide.PlayerInventory;
		_character = character;
		ImageIdentifier = item.ImageIdentifier;
		ComparedImageIdentifier = comparedItem?.ImageIdentifier;
		ItemName = item.ItemDescription;
		ComparedItemName = comparedItem?.ItemRosterElement.EquipmentElement.GetModifiedItemName().ToString();
		TargetItemProperties.Clear();
		ComparedItemProperties.Clear();
		TargetItemFlagList.Clear();
		ComparedItemFlagList.Clear();
		AlternativeUsages.Clear();
		SetGeneralComponentTooltip();
		if (_inventoryLogic.CurrentSettlementComponent is Town town && Game.Current.IsDevelopmentMode)
		{
			CreateProperty(TargetItemProperties, "Category:", item.ItemRosterElement.EquipmentElement.Item.ItemCategory.GetName().ToString());
			CreateProperty(TargetItemProperties, "Supply:", town.MarketData.GetSupply(item.ItemRosterElement.EquipmentElement.Item.ItemCategory).ToString());
			CreateProperty(TargetItemProperties, "Demand:", town.MarketData.GetDemand(item.ItemRosterElement.EquipmentElement.Item.ItemCategory).ToString());
			CreateProperty(TargetItemProperties, "Price Index:", town.MarketData.GetPriceFactor(item.ItemRosterElement.EquipmentElement.Item.ItemCategory).ToString());
		}
		if (item.ItemRosterElement.EquipmentElement.Item.HasArmorComponent)
		{
			SetArmorComponentTooltip();
		}
		else if (item.ItemRosterElement.EquipmentElement.Item.WeaponComponent != null)
		{
			EquipmentElement targetWeapon = _targetItem.ItemRosterElement.EquipmentElement;
			SetWeaponComponentTooltip(in targetWeapon, alternativeUsageIndex, EquipmentElement.Invalid, -1, isInit: true);
		}
		else if (item.ItemRosterElement.EquipmentElement.Item.HasHorseComponent)
		{
			SetHorseComponentTooltip();
		}
		else if (item.ItemRosterElement.EquipmentElement.Item.IsFood)
		{
			SetFoodTooltip();
		}
		if (InventoryManager.GetInventoryItemTypeOfItem(item.ItemRosterElement.EquipmentElement.Item) == InventoryItemType.Goods)
		{
			SetMerchandiseComponentTooltip();
		}
		if (IsComparing && !TaleWorlds.InputSystem.Input.IsGamepadActive)
		{
			for (EquipmentIndex equipmentIndex = _comparedItem.ItemType + 1; equipmentIndex != _comparedItem.ItemType; equipmentIndex = (EquipmentIndex)((int)(equipmentIndex + 1) % 12))
			{
				SPItemVM sPItemVM = _getEquipmentAtIndex(equipmentIndex);
				if (sPItemVM != null && ItemHelper.CheckComparability(sPItemVM.ItemRosterElement.EquipmentElement.Item, comparedItem.ItemRosterElement.EquipmentElement.Item))
				{
					TextObject textObject = new TextObject("{=8fqFGxD9}Press {KEY} to compare with: {ITEM}");
					textObject.SetTextVariable("KEY", GameTexts.FindText("str_game_key_text", "anyalt"));
					textObject.SetTextVariable("ITEM", sPItemVM.ItemDescription);
					CreateProperty(TargetItemProperties, "", textObject.ToString());
					CreateProperty(ComparedItemProperties, "", "");
					break;
				}
			}
		}
		IsInitializationOver = true;
		Game.Current?.EventManager.TriggerEvent(new InventoryItemInspectedEvent(item.ItemRosterElement, item.InventorySide));
	}

	private int CompareValues(float currentValue, float comparedValue)
	{
		int num = (int)(currentValue * 10f);
		int num2 = (int)(comparedValue * 10f);
		if ((num != 0 && !((float)TaleWorlds.Library.MathF.Abs(num) > TaleWorlds.Library.MathF.Abs(currentValue))) || (num2 != 0 && !((float)TaleWorlds.Library.MathF.Abs(num2) > TaleWorlds.Library.MathF.Abs(comparedValue))))
		{
			return 0;
		}
		return CompareValues(num, num2);
	}

	private int CompareValues(int currentValue, int comparedValue)
	{
		if (_comparedItem != null && currentValue != comparedValue)
		{
			if (currentValue <= comparedValue)
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}

	private void AlternativeUsageIndexUpdated()
	{
		if (AlternativeUsageIndex >= 0 && IsInitializationOver && _targetItem.ItemRosterElement.EquipmentElement.Item.WeaponComponent != null)
		{
			WeaponComponentData weaponComponentData = _targetItem.ItemRosterElement.EquipmentElement.Item.Weapons[AlternativeUsageIndex];
			GetComparedWeapon(weaponComponentData.WeaponDescriptionId, out var comparedWeapon, out var comparedUsageIndex);
			if (!comparedWeapon.IsEmpty)
			{
				TargetItemProperties.Clear();
				ComparedItemProperties.Clear();
				SetGeneralComponentTooltip(isInit: false);
				EquipmentElement targetWeapon = _targetItem.ItemRosterElement.EquipmentElement;
				SetWeaponComponentTooltip(in targetWeapon, AlternativeUsageIndex, comparedWeapon, comparedUsageIndex, isInit: false);
				TargetItemFlagList.Clear();
				ComparedItemFlagList.Clear();
				AddWeaponItemFlags(TargetItemFlagList, weaponComponentData);
				AddWeaponItemFlags(ComparedItemFlagList, weaponComponentData);
			}
			else
			{
				_resetComparedItems(_targetItem, AlternativeUsageIndex);
			}
		}
	}

	private void GetComparedWeapon(string weaponUsageId, out EquipmentElement comparedWeapon, out int comparedUsageIndex)
	{
		comparedWeapon = EquipmentElement.Invalid;
		comparedUsageIndex = -1;
		if (IsComparing && _comparedItem != null && ItemHelper.IsWeaponComparableWithUsage(_comparedItem.ItemRosterElement.EquipmentElement.Item, weaponUsageId, out var comparableUsageIndex))
		{
			comparedWeapon = _comparedItem.ItemRosterElement.EquipmentElement;
			comparedUsageIndex = comparableUsageIndex;
		}
	}

	private void SetGeneralComponentTooltip(bool isInit = true)
	{
		if (_targetItem.ItemCost >= 0)
		{
			if (_targetItem.ItemRosterElement.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.Goods || _targetItem.ItemRosterElement.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.Animal || _targetItem.ItemRosterElement.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.Horse)
			{
				GameTexts.SetVariable("PERCENTAGE", (int)TaleWorlds.Library.MathF.Abs((float)(_targetItem.ItemCost - _targetItem.ItemRosterElement.EquipmentElement.Item.Value) / (float)_targetItem.ItemRosterElement.EquipmentElement.Item.Value * 100f));
				ItemCategory itemCategory = _targetItem.ItemRosterElement.EquipmentElement.Item.ItemCategory;
				float num = 0f;
				float num2 = 0f;
				if (Town.AllTowns != null)
				{
					foreach (Town allTown in Town.AllTowns)
					{
						num += allTown.GetItemCategoryPriceIndex(itemCategory);
						num2 += 1f;
					}
				}
				num /= num2;
				if ((float)_targetItem.ItemCost / (float)_targetItem.ItemRosterElement.EquipmentElement.Item.Value > num * 1.3f)
				{
					_costProperty = CreateColoredProperty(TargetItemProperties, "", _targetItem.ItemCost + GoldIcon, UIColors.NegativeIndicator, 1, new HintViewModel(GameTexts.FindText("str_inventory_cost_higher")), TooltipProperty.TooltipPropertyFlags.Cost);
				}
				else if ((float)_targetItem.ItemCost / (float)_targetItem.ItemRosterElement.EquipmentElement.Item.Value < num * 0.8f)
				{
					_costProperty = CreateColoredProperty(TargetItemProperties, "", _targetItem.ItemCost + GoldIcon, UIColors.PositiveIndicator, 1, new HintViewModel(GameTexts.FindText("str_inventory_cost_lower")), TooltipProperty.TooltipPropertyFlags.Cost);
				}
				else
				{
					_costProperty = CreateColoredProperty(TargetItemProperties, "", _targetItem.ItemCost + GoldIcon, UIColors.Gold, 1, new HintViewModel(GameTexts.FindText("str_inventory_cost_normal")), TooltipProperty.TooltipPropertyFlags.Cost);
				}
			}
			else
			{
				_costProperty = CreateColoredProperty(TargetItemProperties, "", _targetItem.ItemCost + GoldIcon, UIColors.Gold, 1, null, TooltipProperty.TooltipPropertyFlags.Cost);
			}
		}
		if (IsComparing)
		{
			CreateColoredProperty(ComparedItemProperties, "", _comparedItem.ItemCost + GoldIcon, UIColors.Gold, 2, null, TooltipProperty.TooltipPropertyFlags.Cost);
		}
		if (Game.Current.IsDevelopmentMode)
		{
			if (_targetItem.ItemRosterElement.EquipmentElement.Item.Culture != null)
			{
				CreateColoredProperty(TargetItemProperties, "Culture: ", _targetItem.ItemRosterElement.EquipmentElement.Item.Culture.StringId, UIColors.Gold);
			}
			else
			{
				CreateColoredProperty(TargetItemProperties, "Culture: ", "No Culture", UIColors.Gold);
			}
			CreateColoredProperty(TargetItemProperties, "ID: ", _targetItem.ItemRosterElement.EquipmentElement.Item.StringId, UIColors.Gold);
		}
		float equipmentWeightMultiplier = 1f;
		bool num3 = _character is CharacterObject characterObject && characterObject.GetPerkValue(DefaultPerks.Athletics.FormFittingArmor);
		SPItemVM sPItemVM = _getEquipmentAtIndex(_targetItem.ItemType);
		bool flag = sPItemVM != null && sPItemVM.ItemType != EquipmentIndex.None && sPItemVM.ItemType != EquipmentIndex.HorseHarness && _targetItem.ItemRosterElement.EquipmentElement.Item.HasArmorComponent;
		if (num3 && flag)
		{
			equipmentWeightMultiplier += DefaultPerks.Athletics.FormFittingArmor.PrimaryBonus;
		}
		AddFloatProperty(_weightText, (EquipmentElement x) => x.GetEquipmentElementWeight() * equipmentWeightMultiplier, reversedCompare: true);
		ItemObject item = _targetItem.ItemRosterElement.EquipmentElement.Item;
		if (item.RelevantSkill != null && (item.Difficulty > 0 || (IsComparing && _comparedItem.ItemRosterElement.EquipmentElement.Item.Difficulty > 0)))
		{
			AddSkillRequirement(_targetItem, TargetItemProperties, isComparison: false);
			if (IsComparing)
			{
				AddSkillRequirement(_comparedItem, ComparedItemProperties, isComparison: true);
			}
		}
		if (isInit)
		{
			AddGeneralItemFlags(TargetItemFlagList, item);
			if (IsComparing)
			{
				AddGeneralItemFlags(ComparedItemFlagList, _comparedItem.ItemRosterElement.EquipmentElement.Item);
			}
		}
	}

	private void AddSkillRequirement(ItemVM itemVm, MBBindingList<ItemMenuTooltipPropertyVM> itemProperties, bool isComparison)
	{
		ItemObject item = itemVm.ItemRosterElement.EquipmentElement.Item;
		string value = "";
		if (item.Difficulty > 0)
		{
			value = item.RelevantSkill.Name.ToString();
			value += " ";
			value += item.Difficulty;
		}
		string definition = "";
		if (!isComparison)
		{
			definition = _requiresText.ToString();
		}
		CreateColoredProperty(itemProperties, definition, value, GetColorFromBool(_character == null || CharacterHelper.CanUseItemBasedOnSkill(_character, itemVm.ItemRosterElement.EquipmentElement)));
	}

	private void AddGeneralItemFlags(MBBindingList<ItemFlagVM> list, ItemObject item)
	{
		if (item.IsUniqueItem)
		{
			list.Add(new ItemFlagVM("GeneralFlagIcons\\unique", GameTexts.FindText("str_inventory_flag_unique")));
		}
		if (item.IsCivilian)
		{
			list.Add(new ItemFlagVM("GeneralFlagIcons\\civillian", GameTexts.FindText("str_inventory_flag_civillian")));
		}
		if (item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByFemale))
		{
			list.Add(new ItemFlagVM("GeneralFlagIcons\\male_only", GameTexts.FindText("str_inventory_flag_male_only")));
		}
		if (item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByMale))
		{
			list.Add(new ItemFlagVM("GeneralFlagIcons\\female_only", GameTexts.FindText("str_inventory_flag_female_only")));
		}
	}

	private void AddFoodItemFlags(MBBindingList<ItemFlagVM> list, ItemObject item)
	{
		list.Add(new ItemFlagVM("GoodsFlagIcons\\consumable", GameTexts.FindText("str_inventory_flag_consumable")));
	}

	private void AddWeaponItemFlags(MBBindingList<ItemFlagVM> list, WeaponComponentData weapon)
	{
		if (weapon == null)
		{
			Debug.FailedAssert("Trying to add flags for a null weapon", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\Inventory\\ItemMenuVM.cs", "AddWeaponItemFlags", 412);
			return;
		}
		ItemObject.ItemUsageSetFlags itemUsageFlags = _getItemUsageSetFlags(weapon);
		foreach (var item in CampaignUIHelper.GetFlagDetailsForWeapon(weapon, itemUsageFlags, _character as CharacterObject))
		{
			list.Add(new ItemFlagVM(item.Item1, item.Item2));
		}
	}

	private Color GetColorFromBool(bool booleanValue)
	{
		if (!booleanValue)
		{
			return UIColors.NegativeIndicator;
		}
		return UIColors.PositiveIndicator;
	}

	private void SetFoodTooltip()
	{
		CreateColoredProperty(TargetItemProperties, "", _foodText.ToString(), ConsumableColor, 1);
		AddFoodItemFlags(TargetItemFlagList, _targetItem.ItemRosterElement.EquipmentElement.Item);
	}

	private void SetHorseComponentTooltip()
	{
		HorseComponent horseComponent = _targetItem.ItemRosterElement.EquipmentElement.Item.HorseComponent;
		HorseComponent horse = (IsComparing ? _comparedItem.ItemRosterElement.EquipmentElement.Item.HorseComponent : null);
		CreateProperty(TargetItemProperties, _typeText.ToString(), GameTexts.FindText("str_inventory_type_" + (int)_targetItem.ItemRosterElement.EquipmentElement.Item.Type).ToString());
		AddHorseItemFlags(TargetItemFlagList, _targetItem.ItemRosterElement.EquipmentElement.Item, horseComponent);
		if (IsComparing)
		{
			CreateProperty(ComparedItemProperties, " ", GameTexts.FindText("str_inventory_type_" + (int)_comparedItem.ItemRosterElement.EquipmentElement.Item.Type).ToString());
			AddHorseItemFlags(ComparedItemFlagList, _comparedItem.ItemRosterElement.EquipmentElement.Item, horse);
		}
		if (!_targetItem.ItemRosterElement.EquipmentElement.Item.IsMountable)
		{
			return;
		}
		AddIntProperty(_horseTierText, (int)(_targetItem.ItemRosterElement.EquipmentElement.Item.Tier + 1), (IsComparing && _comparedItem != null) ? new int?((int)(_comparedItem.ItemRosterElement.EquipmentElement.Item.Tier + 1)) : null);
		AddIntProperty(_chargeDamageText, _targetItem.ItemRosterElement.EquipmentElement.GetModifiedMountCharge(in EquipmentElement.Invalid), (IsComparing && _comparedItem != null) ? new int?(_comparedItem.ItemRosterElement.EquipmentElement.GetModifiedMountCharge(in EquipmentElement.Invalid)) : null);
		AddIntProperty(_speedText, _targetItem.ItemRosterElement.EquipmentElement.GetModifiedMountSpeed(in EquipmentElement.Invalid), (IsComparing && _comparedItem != null) ? new int?(_comparedItem.ItemRosterElement.EquipmentElement.GetModifiedMountSpeed(in EquipmentElement.Invalid)) : null);
		AddIntProperty(_maneuverText, _targetItem.ItemRosterElement.EquipmentElement.GetModifiedMountManeuver(in EquipmentElement.Invalid), (IsComparing && _comparedItem != null) ? new int?(_comparedItem.ItemRosterElement.EquipmentElement.GetModifiedMountManeuver(in EquipmentElement.Invalid)) : null);
		AddIntProperty(_hitPointsText, _targetItem.ItemRosterElement.EquipmentElement.GetModifiedMountHitPoints(), (IsComparing && _comparedItem != null) ? new int?(_comparedItem.ItemRosterElement.EquipmentElement.GetModifiedMountHitPoints()) : null);
		if (_targetItem.ItemRosterElement.EquipmentElement.Item.HasHorseComponent && _targetItem.ItemRosterElement.EquipmentElement.Item.HorseComponent.IsMount)
		{
			AddComparableStringProperty(_horseTypeText, (EquipmentElement x) => x.Item.ItemCategory.GetName().ToString(), (EquipmentElement x) => GetHorseCategoryValue(x.Item.ItemCategory));
		}
	}

	private void AddHorseItemFlags(MBBindingList<ItemFlagVM> list, ItemObject item, HorseComponent horse)
	{
		if (!horse.IsLiveStock)
		{
			if (item.ItemCategory == DefaultItemCategories.PackAnimal)
			{
				list.Add(new ItemFlagVM("MountFlagIcons\\weight_carrying_mount", GameTexts.FindText("str_inventory_flag_carrying_mount")));
			}
			else
			{
				list.Add(new ItemFlagVM("MountFlagIcons\\speed_mount", GameTexts.FindText("str_inventory_flag_speed_mount")));
			}
		}
		if (_inventoryLogic.IsSlaughterable(item))
		{
			list.Add(new ItemFlagVM("MountFlagIcons\\slaughterable", GameTexts.FindText("str_inventory_flag_slaughterable")));
		}
	}

	private void SetMerchandiseComponentTooltip()
	{
		if (Campaign.Current.GameMode != CampaignGameMode.Campaign || _tradeRumorsBehavior == null)
		{
			return;
		}
		IEnumerable<TradeRumor> tradeRumors = _tradeRumorsBehavior.TradeRumors;
		bool flag = true;
		IMarketData marketData = _inventoryLogic.MarketData;
		foreach (TradeRumor item in from x in tradeRumors
			orderby x.SellPrice descending, x.BuyPrice descending
			select x)
		{
			bool flag2 = false;
			bool flag3 = false;
			if (_targetItem.ItemRosterElement.EquipmentElement.Item != item.ItemCategory)
			{
				continue;
			}
			if ((float)item.BuyPrice < 0.9f * (float)marketData.GetPrice(item.ItemCategory, MobileParty.MainParty, isSelling: true, _inventoryLogic.OtherParty))
			{
				flag3 = true;
			}
			if ((float)item.SellPrice > 1.1f * (float)marketData.GetPrice(item.ItemCategory, MobileParty.MainParty, isSelling: false, _inventoryLogic.OtherParty))
			{
				flag2 = true;
			}
			if ((Settlement.CurrentSettlement != null && Settlement.CurrentSettlement == item.Settlement) || _targetItem.ItemRosterElement.EquipmentElement.Item != item.ItemCategory || !(flag3 || flag2))
			{
				continue;
			}
			if (flag)
			{
				CreateColoredProperty(TargetItemProperties, "", _tradeRumorsText.ToString(), TitleColor, 1);
				if (IsComparing)
				{
					CreateProperty(ComparedItemProperties, "", "");
					CreateProperty(ComparedItemProperties, "", "");
				}
				flag = false;
			}
			MBTextManager.SetTextVariable("SETTLEMENT_NAME", item.Settlement.Name);
			MBTextManager.SetTextVariable("SELL_PRICE", item.SellPrice);
			MBTextManager.SetTextVariable("BUY_PRICE", item.BuyPrice);
			float alpha = CalculateTradeRumorOldnessFactor(item);
			Color color = new Color(TitleColor.Red, TitleColor.Green, TitleColor.Blue, alpha);
			TextObject textObject = (flag3 ? GameTexts.FindText("str_trade_rumors_text_buy") : GameTexts.FindText("str_trade_rumors_text_sell"));
			CreateColoredProperty(TargetItemProperties, "", textObject.ToString(), color);
			if (IsComparing)
			{
				CreateProperty(ComparedItemProperties, "", "");
			}
		}
	}

	private float CalculateTradeRumorOldnessFactor(TradeRumor rumor)
	{
		return TaleWorlds.Library.MathF.Clamp((float)(int)rumor.RumorEndTime.RemainingDaysFromNow / 5f, 0.5f, 1f);
	}

	private void SetWeaponComponentTooltip(in EquipmentElement targetWeapon, int targetWeaponUsageIndex, EquipmentElement comparedWeapon, int comparedWeaponUsageIndex, bool isInit)
	{
		WeaponComponentData weaponWithUsageIndex = targetWeapon.Item.GetWeaponWithUsageIndex(targetWeaponUsageIndex);
		if (IsComparing && _comparedItem != null && comparedWeapon.IsEmpty)
		{
			GetComparedWeapon(weaponWithUsageIndex.WeaponDescriptionId, out comparedWeapon, out comparedWeaponUsageIndex);
		}
		WeaponComponentData weaponComponentData = (comparedWeapon.IsEmpty ? null : comparedWeapon.Item.GetWeaponWithUsageIndex(comparedWeaponUsageIndex));
		if (isInit)
		{
			AddWeaponItemFlags(TargetItemFlagList, weaponWithUsageIndex);
			if (IsComparing)
			{
				AddWeaponItemFlags(ComparedItemFlagList, weaponComponentData);
			}
			if (targetWeaponUsageIndex == 0)
			{
				AlternativeUsageIndex = -1;
			}
			foreach (WeaponComponentData weapon in targetWeapon.Item.Weapons)
			{
				if (CampaignUIHelper.IsItemUsageApplicable(weapon))
				{
					AlternativeUsages.Add(new StringItemWithHintVM(GameTexts.FindText("str_weapon_usage", weapon.WeaponDescriptionId).ToString(), GameTexts.FindText("str_inventory_alternative_usage_hint")));
				}
			}
			AlternativeUsageIndex = targetWeaponUsageIndex;
		}
		CreateProperty(TargetItemProperties, _classText.ToString(), GameTexts.FindText("str_inventory_weapon", ((int)weaponWithUsageIndex.WeaponClass).ToString()).ToString());
		if (!comparedWeapon.IsEmpty)
		{
			CreateProperty(ComparedItemProperties, " ", GameTexts.FindText("str_inventory_weapon", ((int)weaponWithUsageIndex.WeaponClass).ToString()).ToString());
		}
		else if (IsComparing)
		{
			CreateProperty(ComparedItemProperties, "", "");
		}
		if (targetWeapon.Item.BannerComponent == null)
		{
			int value = 0;
			if (!comparedWeapon.IsEmpty)
			{
				value = (int)(comparedWeapon.Item.Tier + 1);
			}
			AddIntProperty(_weaponTierText, (int)(targetWeapon.Item.Tier + 1), value);
		}
		ItemObject.ItemTypeEnum itemTypeFromWeaponClass = WeaponComponentData.GetItemTypeFromWeaponClass(weaponWithUsageIndex.WeaponClass);
		ItemObject.ItemTypeEnum itemTypeEnum = ((!comparedWeapon.IsEmpty) ? WeaponComponentData.GetItemTypeFromWeaponClass(weaponWithUsageIndex.WeaponClass) : ItemObject.ItemTypeEnum.Invalid);
		if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.OneHandedWeapon || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.TwoHandedWeapon || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Polearm || itemTypeEnum == ItemObject.ItemTypeEnum.OneHandedWeapon || itemTypeEnum == ItemObject.ItemTypeEnum.TwoHandedWeapon || itemTypeEnum == ItemObject.ItemTypeEnum.Polearm)
		{
			if (weaponWithUsageIndex.SwingDamageType != DamageTypes.Invalid)
			{
				AddIntProperty(_swingSpeedText, targetWeapon.GetModifiedSwingSpeedForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedSwingSpeedForUsage(comparedWeaponUsageIndex)));
				AddSwingDamageProperty(_swingDamageText, in targetWeapon, targetWeaponUsageIndex, in comparedWeapon, comparedWeaponUsageIndex);
			}
			if (weaponWithUsageIndex.ThrustDamageType != DamageTypes.Invalid)
			{
				AddIntProperty(_thrustSpeedText, targetWeapon.GetModifiedThrustSpeedForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedThrustSpeedForUsage(comparedWeaponUsageIndex)));
				AddThrustDamageProperty(_thrustDamageText, in targetWeapon, targetWeaponUsageIndex, in comparedWeapon, comparedWeaponUsageIndex);
			}
			AddIntProperty(_lengthText, weaponWithUsageIndex.WeaponLength, weaponComponentData?.WeaponLength);
			AddIntProperty(_handlingText, targetWeapon.GetModifiedHandlingForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedHandlingForUsage(comparedWeaponUsageIndex)));
		}
		if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Thrown || itemTypeEnum == ItemObject.ItemTypeEnum.Thrown)
		{
			AddIntProperty(_weaponLengthText, weaponWithUsageIndex.WeaponLength, weaponComponentData?.WeaponLength);
			AddMissileDamageProperty(_damageText, in targetWeapon, targetWeaponUsageIndex, in comparedWeapon, comparedWeaponUsageIndex);
			AddIntProperty(_missileSpeedText, targetWeapon.GetModifiedMissileSpeedForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedMissileSpeedForUsage(comparedWeaponUsageIndex)));
			AddIntProperty(_accuracyText, weaponWithUsageIndex.Accuracy, weaponComponentData?.Accuracy);
			AddIntProperty(_stackAmountText, targetWeapon.GetModifiedStackCountForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedStackCountForUsage(comparedWeaponUsageIndex)));
		}
		if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Shield || itemTypeEnum == ItemObject.ItemTypeEnum.Shield)
		{
			AddIntProperty(_speedText, targetWeapon.GetModifiedSwingSpeedForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedSwingSpeedForUsage(comparedWeaponUsageIndex)));
			AddIntProperty(_hitPointsText, targetWeapon.GetModifiedMaximumHitPointsForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedMaximumHitPointsForUsage(comparedWeaponUsageIndex)));
		}
		if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Bow || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Crossbow || itemTypeEnum == ItemObject.ItemTypeEnum.Bow || itemTypeEnum == ItemObject.ItemTypeEnum.Crossbow)
		{
			AddIntProperty(_speedText, targetWeapon.GetModifiedSwingSpeedForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedSwingSpeedForUsage(comparedWeaponUsageIndex)));
			AddThrustDamageProperty(_damageText, in targetWeapon, targetWeaponUsageIndex, in comparedWeapon, comparedWeaponUsageIndex);
			AddIntProperty(_accuracyText, weaponWithUsageIndex.Accuracy, weaponComponentData?.Accuracy);
			AddIntProperty(_missileSpeedText, targetWeapon.GetModifiedMissileSpeedForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedMissileSpeedForUsage(comparedWeaponUsageIndex)));
			if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Crossbow || itemTypeEnum == ItemObject.ItemTypeEnum.Crossbow)
			{
				AddIntProperty(_ammoLimitText, weaponWithUsageIndex.MaxDataValue, weaponComponentData?.MaxDataValue);
			}
		}
		if (weaponWithUsageIndex.IsAmmo || (weaponComponentData != null && weaponComponentData.IsAmmo))
		{
			if ((itemTypeFromWeaponClass != ItemObject.ItemTypeEnum.Arrows && itemTypeFromWeaponClass != ItemObject.ItemTypeEnum.Bolts) || (weaponComponentData != null && itemTypeEnum != ItemObject.ItemTypeEnum.Arrows && itemTypeEnum != ItemObject.ItemTypeEnum.Bolts))
			{
				AddIntProperty(_accuracyText, weaponWithUsageIndex.Accuracy, weaponComponentData?.Accuracy);
			}
			AddThrustDamageProperty(_damageText, in targetWeapon, targetWeaponUsageIndex, in comparedWeapon, comparedWeaponUsageIndex);
			AddIntProperty(_stackAmountText, targetWeapon.GetModifiedStackCountForUsage(targetWeaponUsageIndex), comparedWeapon.IsEmpty ? null : new int?(comparedWeapon.GetModifiedStackCountForUsage(comparedWeaponUsageIndex)));
		}
		ItemObject item = targetWeapon.Item;
		if (item == null || !item.HasBannerComponent)
		{
			ItemObject item2 = comparedWeapon.Item;
			if (item2 == null || !item2.HasBannerComponent)
			{
				goto IL_0717;
			}
		}
		Func<EquipmentElement, string> valueAsStringFunc = delegate(EquipmentElement x)
		{
			if (x.Item?.BannerComponent?.BannerEffect != null)
			{
				GameTexts.SetVariable("RANK", x.Item.BannerComponent.BannerEffect.Name);
				string content = string.Empty;
				if (x.Item.BannerComponent.BannerEffect.IncrementType == BannerEffect.EffectIncrementType.AddFactor)
				{
					TextObject textObject = GameTexts.FindText("str_NUMBER_percent");
					textObject.SetTextVariable("NUMBER", ((int)Math.Abs(x.Item.BannerComponent.GetBannerEffectBonus() * 100f)).ToString());
					content = textObject.ToString();
				}
				else if (x.Item.BannerComponent.BannerEffect.IncrementType == BannerEffect.EffectIncrementType.Add)
				{
					content = x.Item.BannerComponent.GetBannerEffectBonus().ToString();
				}
				GameTexts.SetVariable("NUMBER", content);
				return GameTexts.FindText("str_RANK_with_NUM_between_parenthesis").ToString();
			}
			return _noneText.ToString();
		};
		AddComparableStringProperty(_bannerEffectText, valueAsStringFunc, (EquipmentElement x) => 0);
		goto IL_0717;
		IL_0717:
		AddDonationXpTooltip();
	}

	private void AddIntProperty(TextObject description, int targetValue, int? comparedValue)
	{
		string value = targetValue.ToString();
		if (IsComparing && comparedValue.HasValue)
		{
			string value2 = comparedValue.Value.ToString();
			int result = CompareValues(targetValue, comparedValue.Value);
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(result, isCompared: false));
			CreateColoredProperty(ComparedItemProperties, " ", value2, GetColorFromComparison(result, isCompared: true));
		}
		else
		{
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(0, isCompared: false));
		}
	}

	private void AddFloatProperty(TextObject description, Func<EquipmentElement, float> func, bool reversedCompare = false)
	{
		float targetValue = func(_targetItem.ItemRosterElement.EquipmentElement);
		float? comparedValue = null;
		if (IsComparing && _comparedItem != null)
		{
			comparedValue = func(_comparedItem.ItemRosterElement.EquipmentElement);
		}
		AddFloatProperty(description, targetValue, comparedValue, reversedCompare);
	}

	private void AddFloatProperty(TextObject description, float targetValue, float? comparedValue, bool reversedCompare = false)
	{
		string formattedItemPropertyText = CampaignUIHelper.GetFormattedItemPropertyText(targetValue, typeRequiresInteger: false);
		if (IsComparing && comparedValue.HasValue)
		{
			string formattedItemPropertyText2 = CampaignUIHelper.GetFormattedItemPropertyText(comparedValue.Value, typeRequiresInteger: false);
			int num = CompareValues(targetValue, comparedValue.Value);
			if (reversedCompare)
			{
				num *= -1;
			}
			CreateColoredProperty(TargetItemProperties, description.ToString(), formattedItemPropertyText, GetColorFromComparison(num, isCompared: false));
			CreateColoredProperty(ComparedItemProperties, " ", formattedItemPropertyText2, GetColorFromComparison(num, isCompared: true));
		}
		else
		{
			CreateColoredProperty(TargetItemProperties, description.ToString(), formattedItemPropertyText, GetColorFromComparison(0, isCompared: false));
		}
	}

	private void AddComparableStringProperty(TextObject description, Func<EquipmentElement, string> valueAsStringFunc, Func<EquipmentElement, int> valueAsIntFunc)
	{
		string value = valueAsStringFunc(_targetItem.ItemRosterElement.EquipmentElement);
		int currentValue = valueAsIntFunc(_targetItem.ItemRosterElement.EquipmentElement);
		if (IsComparing && _comparedItem != null)
		{
			int comparedValue = valueAsIntFunc(_comparedItem.ItemRosterElement.EquipmentElement);
			int result = CompareValues(currentValue, comparedValue);
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(result, isCompared: false));
			CreateColoredProperty(ComparedItemProperties, " ", valueAsStringFunc(_comparedItem.ItemRosterElement.EquipmentElement), GetColorFromComparison(result, isCompared: true));
		}
		else
		{
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(0, isCompared: false));
		}
	}

	private void AddSwingDamageProperty(TextObject description, in EquipmentElement targetWeapon, int targetWeaponUsageIndex, in EquipmentElement comparedWeapon, int comparedWeaponUsageIndex)
	{
		int modifiedSwingDamageForUsage = targetWeapon.GetModifiedSwingDamageForUsage(targetWeaponUsageIndex);
		string value = ItemHelper.GetSwingDamageText(targetWeapon.Item.GetWeaponWithUsageIndex(targetWeaponUsageIndex), targetWeapon.ItemModifier).ToString();
		if (IsComparing && !comparedWeapon.IsEmpty)
		{
			int modifiedSwingDamageForUsage2 = comparedWeapon.GetModifiedSwingDamageForUsage(comparedWeaponUsageIndex);
			string value2 = ItemHelper.GetSwingDamageText(comparedWeapon.Item.GetWeaponWithUsageIndex(comparedWeaponUsageIndex), comparedWeapon.ItemModifier).ToString();
			int result = CompareValues(modifiedSwingDamageForUsage, modifiedSwingDamageForUsage2);
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(result, isCompared: false));
			CreateColoredProperty(ComparedItemProperties, " ", value2, GetColorFromComparison(result, isCompared: true));
		}
		else
		{
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(0, isCompared: true));
		}
	}

	private void AddMissileDamageProperty(TextObject description, in EquipmentElement targetWeapon, int targetWeaponUsageIndex, in EquipmentElement comparedWeapon, int comparedWeaponUsageIndex)
	{
		int modifiedMissileDamageForUsage = targetWeapon.GetModifiedMissileDamageForUsage(targetWeaponUsageIndex);
		string value = ItemHelper.GetMissileDamageText(targetWeapon.Item.GetWeaponWithUsageIndex(targetWeaponUsageIndex), targetWeapon.ItemModifier).ToString();
		if (IsComparing && !comparedWeapon.IsEmpty)
		{
			int modifiedMissileDamageForUsage2 = comparedWeapon.GetModifiedMissileDamageForUsage(comparedWeaponUsageIndex);
			string value2 = ItemHelper.GetMissileDamageText(comparedWeapon.Item.GetWeaponWithUsageIndex(comparedWeaponUsageIndex), comparedWeapon.ItemModifier).ToString();
			int result = CompareValues(modifiedMissileDamageForUsage, modifiedMissileDamageForUsage2);
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(result, isCompared: false));
			CreateColoredProperty(ComparedItemProperties, " ", value2, GetColorFromComparison(result, isCompared: true));
		}
		else
		{
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(0, isCompared: true));
		}
	}

	private void AddThrustDamageProperty(TextObject description, in EquipmentElement targetWeapon, int targetWeaponUsageIndex, in EquipmentElement comparedWeapon, int comparedWeaponUsageIndex)
	{
		int modifiedThrustDamageForUsage = targetWeapon.GetModifiedThrustDamageForUsage(targetWeaponUsageIndex);
		string value = ItemHelper.GetThrustDamageText(targetWeapon.Item.GetWeaponWithUsageIndex(targetWeaponUsageIndex), targetWeapon.ItemModifier).ToString();
		if (IsComparing && !comparedWeapon.IsEmpty)
		{
			int modifiedThrustDamageForUsage2 = comparedWeapon.GetModifiedThrustDamageForUsage(comparedWeaponUsageIndex);
			string value2 = ItemHelper.GetThrustDamageText(comparedWeapon.Item.GetWeaponWithUsageIndex(comparedWeaponUsageIndex), comparedWeapon.ItemModifier).ToString();
			int result = CompareValues(modifiedThrustDamageForUsage, modifiedThrustDamageForUsage2);
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(result, isCompared: false));
			CreateColoredProperty(ComparedItemProperties, " ", value2, GetColorFromComparison(result, isCompared: true));
		}
		else
		{
			CreateColoredProperty(TargetItemProperties, description.ToString(), value, GetColorFromComparison(0, isCompared: true));
		}
	}

	private void SetArmorComponentTooltip()
	{
		int num = 0;
		int value = 0;
		if (_comparedItem != null && _comparedItem.ItemRosterElement.EquipmentElement.Item != null)
		{
			value = (int)(_comparedItem.ItemRosterElement.EquipmentElement.Item.Tier + 1);
		}
		AddIntProperty(_armorTierText, (int)(_targetItem.ItemRosterElement.EquipmentElement.Item.Tier + 1), value);
		CreateProperty(TargetItemProperties, _typeText.ToString(), GameTexts.FindText("str_inventory_type_" + (int)_targetItem.ItemRosterElement.EquipmentElement.Item.Type).ToString());
		if (IsComparing)
		{
			CreateProperty(ComparedItemProperties, " ", GameTexts.FindText("str_inventory_type_" + (int)_targetItem.ItemRosterElement.EquipmentElement.Item.Type).ToString());
		}
		ArmorComponent armorComponent = _targetItem.ItemRosterElement.EquipmentElement.Item.ArmorComponent;
		ArmorComponent armorComponent2 = (IsComparing ? _comparedItem.ItemRosterElement.EquipmentElement.Item.ArmorComponent : null);
		if (armorComponent.HeadArmor != 0 || (IsComparing && armorComponent2.HeadArmor != 0))
		{
			num = (IsComparing ? CompareValues(_targetItem.ItemRosterElement.EquipmentElement.GetModifiedHeadArmor(), _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedHeadArmor()) : 0);
			CreateColoredProperty(TargetItemProperties, _headArmorText.ToString(), _targetItem.ItemRosterElement.EquipmentElement.GetModifiedHeadArmor().ToString(), GetColorFromComparison(num, isCompared: false));
			if (IsComparing)
			{
				CreateColoredProperty(ComparedItemProperties, " ", _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedHeadArmor().ToString(), GetColorFromComparison(num, isCompared: true));
			}
		}
		if (armorComponent.BodyArmor != 0 || (IsComparing && _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedBodyArmor() != 0))
		{
			if (_targetItem.ItemType == EquipmentIndex.HorseHarness)
			{
				num = (IsComparing ? CompareValues(_targetItem.ItemRosterElement.EquipmentElement.GetModifiedMountBodyArmor(), _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedMountBodyArmor()) : 0);
				CreateColoredProperty(TargetItemProperties, _horseArmorText.ToString(), _targetItem.ItemRosterElement.EquipmentElement.GetModifiedMountBodyArmor().ToString(), GetColorFromComparison(num, isCompared: false));
				if (IsComparing)
				{
					CreateColoredProperty(ComparedItemProperties, " ", _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedMountBodyArmor().ToString(), GetColorFromComparison(num, isCompared: true));
				}
			}
			else
			{
				num = (IsComparing ? CompareValues(_targetItem.ItemRosterElement.EquipmentElement.GetModifiedBodyArmor(), _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedBodyArmor()) : 0);
				CreateColoredProperty(TargetItemProperties, _bodyArmorText.ToString(), _targetItem.ItemRosterElement.EquipmentElement.GetModifiedBodyArmor().ToString(), GetColorFromComparison(num, isCompared: false));
				if (IsComparing)
				{
					CreateColoredProperty(ComparedItemProperties, " ", _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedBodyArmor().ToString(), GetColorFromComparison(num, isCompared: true));
				}
			}
		}
		if (_targetItem.ItemRosterElement.EquipmentElement.GetModifiedLegArmor() != 0 || (IsComparing && _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedLegArmor() != 0))
		{
			num = (IsComparing ? CompareValues(_targetItem.ItemRosterElement.EquipmentElement.GetModifiedLegArmor(), _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedLegArmor()) : 0);
			CreateColoredProperty(TargetItemProperties, _legArmorText.ToString(), _targetItem.ItemRosterElement.EquipmentElement.GetModifiedLegArmor().ToString(), GetColorFromComparison(num, isCompared: false));
			if (IsComparing)
			{
				CreateColoredProperty(ComparedItemProperties, " ", _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedLegArmor().ToString(), GetColorFromComparison(num, isCompared: true));
			}
		}
		if (_targetItem.ItemRosterElement.EquipmentElement.GetModifiedArmArmor() != 0 || (IsComparing && _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedArmArmor() != 0))
		{
			num = (IsComparing ? CompareValues(_targetItem.ItemRosterElement.EquipmentElement.GetModifiedArmArmor(), _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedArmArmor()) : 0);
			CreateColoredProperty(TargetItemProperties, _armArmorText.ToString(), _targetItem.ItemRosterElement.EquipmentElement.GetModifiedArmArmor().ToString(), GetColorFromComparison(num, isCompared: false));
			if (IsComparing)
			{
				CreateColoredProperty(ComparedItemProperties, " ", _comparedItem.ItemRosterElement.EquipmentElement.GetModifiedArmArmor().ToString(), GetColorFromComparison(num, isCompared: true));
			}
		}
		AddDonationXpTooltip();
	}

	private void AddDonationXpTooltip()
	{
		ItemDiscardModel itemDiscardModel = Campaign.Current.Models.ItemDiscardModel;
		int xpBonusForDiscardingItem = itemDiscardModel.GetXpBonusForDiscardingItem(_targetItem.ItemRosterElement.EquipmentElement.Item);
		int num = (IsComparing ? itemDiscardModel.GetXpBonusForDiscardingItem(_comparedItem.ItemRosterElement.EquipmentElement.Item) : 0);
		if (xpBonusForDiscardingItem <= 0 && (!IsComparing || num <= 0))
		{
			return;
		}
		InventoryLogic inventoryLogic = _inventoryLogic;
		if (inventoryLogic != null && inventoryLogic.IsDiscardDonating)
		{
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_donation_item_hint").ToString());
			int result = (IsComparing ? CompareValues(xpBonusForDiscardingItem, num) : 0);
			CreateColoredProperty(TargetItemProperties, GameTexts.FindText("str_LEFT_colon").ToString(), xpBonusForDiscardingItem.ToString(), GetColorFromComparison(result, isCompared: false));
			if (IsComparing)
			{
				CreateColoredProperty(ComparedItemProperties, " ", num.ToString(), GetColorFromComparison(result, isCompared: true));
			}
		}
	}

	private Color GetColorFromComparison(int result, bool isCompared)
	{
		switch (result)
		{
		case -1:
			if (!isCompared)
			{
				return UIColors.NegativeIndicator;
			}
			return UIColors.PositiveIndicator;
		case 1:
			if (!isCompared)
			{
				return UIColors.PositiveIndicator;
			}
			return UIColors.NegativeIndicator;
		default:
			return Colors.Black;
		}
	}

	private int GetHorseCategoryValue(ItemCategory itemCategory)
	{
		if (itemCategory.IsAnimal)
		{
			if (itemCategory == DefaultItemCategories.PackAnimal)
			{
				return 1;
			}
			if (itemCategory == DefaultItemCategories.Horse)
			{
				return 2;
			}
			if (itemCategory == DefaultItemCategories.WarHorse)
			{
				return 3;
			}
			if (itemCategory == DefaultItemCategories.NobleHorse)
			{
				return 4;
			}
		}
		Debug.FailedAssert("This horse item category is not defined", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\Inventory\\ItemMenuVM.cs", "GetHorseCategoryValue", 1378);
		return -1;
	}

	private ItemMenuTooltipPropertyVM CreateProperty(MBBindingList<ItemMenuTooltipPropertyVM> targetList, string definition, string value, int textHeight = 0, HintViewModel hint = null)
	{
		ItemMenuTooltipPropertyVM itemMenuTooltipPropertyVM = new ItemMenuTooltipPropertyVM(definition, value, textHeight, onlyShowWhenExtended: false, hint);
		targetList.Add(itemMenuTooltipPropertyVM);
		return itemMenuTooltipPropertyVM;
	}

	private ItemMenuTooltipPropertyVM CreateColoredProperty(MBBindingList<ItemMenuTooltipPropertyVM> targetList, string definition, string value, Color color, int textHeight = 0, HintViewModel hint = null, TooltipProperty.TooltipPropertyFlags propertyFlags = TooltipProperty.TooltipPropertyFlags.None)
	{
		if (color == Colors.Black)
		{
			CreateProperty(targetList, definition, value, textHeight, hint);
			return null;
		}
		ItemMenuTooltipPropertyVM itemMenuTooltipPropertyVM = new ItemMenuTooltipPropertyVM(definition, value, textHeight, color, onlyShowWhenExtended: false, hint, propertyFlags);
		targetList.Add(itemMenuTooltipPropertyVM);
		return itemMenuTooltipPropertyVM;
	}

	public void SetTransactionCost(int getItemTotalPrice, int maxIndividualPrice)
	{
		TransactionTotalCost = getItemTotalPrice;
		if (_targetItem.ItemCost != maxIndividualPrice)
		{
			if (_targetItem.ItemCost < maxIndividualPrice)
			{
				_costProperty.ValueLabel = _targetItem.ItemCost + " - " + maxIndividualPrice + GoldIcon;
			}
			else
			{
				_costProperty.ValueLabel = maxIndividualPrice + " - " + _targetItem.ItemCost + GoldIcon;
			}
		}
		else
		{
			_costProperty.ValueLabel = _targetItem.ItemCost + GoldIcon;
		}
	}
}
