using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class MissionEquipment
{
	private struct MissionEquipmentCache
	{
		public enum CachedBool
		{
			ContainsMeleeWeapon,
			ContainsShield,
			ContainsSpear,
			ContainsNonConsumableRangedWeaponWithAmmo,
			ContainsThrownWeapon,
			Count
		}

		public enum CachedFloat
		{
			TotalWeightOfWeapons,
			Count
		}

		private const int CachedBoolCount = 5;

		private const int CachedFloatCount = 1;

		private const int TotalCachedCount = 6;

		private float _cachedFloat;

		private StackArray.StackArray5Bool _cachedBool;

		private StackArray.StackArray6Bool _validity;

		public void Initialize()
		{
			_cachedBool = default(StackArray.StackArray5Bool);
			_validity = default(StackArray.StackArray6Bool);
		}

		public bool IsValid(CachedBool queriedData)
		{
			return _validity[(int)queriedData];
		}

		public void UpdateAndMarkValid(CachedBool data, bool value)
		{
			_cachedBool[(int)data] = value;
			_validity[(int)data] = true;
		}

		public bool GetValue(CachedBool data)
		{
			return _cachedBool[(int)data];
		}

		public bool IsValid(CachedFloat queriedData)
		{
			return _validity[(int)(5 + queriedData)];
		}

		public void UpdateAndMarkValid(CachedFloat data, float value)
		{
			_cachedFloat = value;
			_validity[(int)(5 + data)] = true;
		}

		public float GetValue(CachedFloat data)
		{
			return _cachedFloat;
		}

		public void InvalidateOnWeaponSlotUpdated()
		{
			_validity[0] = false;
			_validity[1] = false;
			_validity[2] = false;
			_validity[3] = false;
			_validity[4] = false;
			_validity[5] = false;
		}

		public void InvalidateOnWeaponUsageIndexUpdated()
		{
		}

		public void InvalidateOnWeaponAmmoUpdated()
		{
			_validity[5] = false;
		}

		public void InvalidateOnWeaponAmmoAvailabilityChanged()
		{
			_validity[3] = false;
		}

		public void InvalidateOnWeaponHitPointsUpdated()
		{
			_validity[5] = false;
		}

		public void InvalidateOnWeaponDestroyed()
		{
			_validity[1] = false;
		}
	}

	private readonly MissionWeapon[] _weaponSlots;

	private MissionEquipmentCache _cache;

	public MissionWeapon this[int index]
	{
		get
		{
			return _weaponSlots[index];
		}
		set
		{
			_weaponSlots[index] = value;
			_cache.InvalidateOnWeaponSlotUpdated();
		}
	}

	public MissionWeapon this[EquipmentIndex index]
	{
		get
		{
			return _weaponSlots[(int)index];
		}
		set
		{
			this[(int)index] = value;
		}
	}

	public MissionEquipment()
	{
		_weaponSlots = new MissionWeapon[5];
		_cache = default(MissionEquipmentCache);
		_cache.Initialize();
	}

	public MissionEquipment(Equipment spawnEquipment, Banner banner)
		: this()
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			_weaponSlots[(int)equipmentIndex] = new MissionWeapon(spawnEquipment[equipmentIndex].Item, spawnEquipment[equipmentIndex].ItemModifier, banner);
		}
	}

	public void FillFrom(Equipment sourceEquipment, Banner banner)
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			this[equipmentIndex] = new MissionWeapon(sourceEquipment[equipmentIndex].Item, sourceEquipment[equipmentIndex].ItemModifier, banner);
		}
	}

	private float CalculateGetTotalWeightOfWeapons()
	{
		float num = 0f;
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			MissionWeapon missionWeapon = this[equipmentIndex];
			if (missionWeapon.IsEmpty)
			{
				continue;
			}
			if (missionWeapon.CurrentUsageItem.IsShield)
			{
				if (missionWeapon.HitPoints > 0)
				{
					num += missionWeapon.GetWeight();
				}
			}
			else
			{
				num += missionWeapon.GetWeight();
			}
		}
		return num;
	}

	public float GetTotalWeightOfWeapons()
	{
		if (!_cache.IsValid(MissionEquipmentCache.CachedFloat.TotalWeightOfWeapons))
		{
			_cache.UpdateAndMarkValid(MissionEquipmentCache.CachedFloat.TotalWeightOfWeapons, CalculateGetTotalWeightOfWeapons());
		}
		return _cache.GetValue(MissionEquipmentCache.CachedFloat.TotalWeightOfWeapons);
	}

	public static EquipmentIndex SelectWeaponPickUpSlot(Agent agentPickingUp, MissionWeapon weaponBeingPickedUp, bool isStuckMissile)
	{
		EquipmentIndex equipmentIndex = EquipmentIndex.None;
		if (weaponBeingPickedUp.Item.ItemFlags.HasAnyFlag(ItemFlags.DropOnWeaponChange | ItemFlags.DropOnAnyAction))
		{
			equipmentIndex = EquipmentIndex.ExtraWeaponSlot;
		}
		else
		{
			Agent.HandIndex handIndex = (weaponBeingPickedUp.Item.ItemFlags.HasAnyFlag(ItemFlags.HeldInOffHand) ? Agent.HandIndex.OffHand : Agent.HandIndex.MainHand);
			EquipmentIndex wieldedItemIndex = agentPickingUp.GetWieldedItemIndex(handIndex);
			MissionWeapon missionWeapon = ((wieldedItemIndex != EquipmentIndex.None) ? agentPickingUp.Equipment[wieldedItemIndex] : MissionWeapon.Invalid);
			if (isStuckMissile)
			{
				bool flag = false;
				bool flag2 = false;
				bool isConsumable = weaponBeingPickedUp.Item.PrimaryWeapon.IsConsumable;
				if (isConsumable)
				{
					flag = !missionWeapon.IsEmpty && missionWeapon.IsEqualTo(weaponBeingPickedUp) && missionWeapon.HasEnoughSpaceForAmount(weaponBeingPickedUp.Amount);
					flag2 = !missionWeapon.IsEmpty && missionWeapon.IsSameType(weaponBeingPickedUp) && missionWeapon.HasEnoughSpaceForAmount(weaponBeingPickedUp.Amount);
				}
				EquipmentIndex equipmentIndex2 = EquipmentIndex.None;
				EquipmentIndex equipmentIndex3 = EquipmentIndex.None;
				EquipmentIndex equipmentIndex4 = EquipmentIndex.None;
				for (EquipmentIndex equipmentIndex5 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex5 < EquipmentIndex.ExtraWeaponSlot; equipmentIndex5++)
				{
					if (isConsumable)
					{
						if (equipmentIndex3 != EquipmentIndex.None && !agentPickingUp.Equipment[equipmentIndex5].IsEmpty && agentPickingUp.Equipment[equipmentIndex5].IsEqualTo(weaponBeingPickedUp) && agentPickingUp.Equipment[equipmentIndex5].HasEnoughSpaceForAmount(weaponBeingPickedUp.Amount))
						{
							equipmentIndex3 = equipmentIndex5;
							continue;
						}
						if (equipmentIndex4 == EquipmentIndex.None && !agentPickingUp.Equipment[equipmentIndex5].IsEmpty && agentPickingUp.Equipment[equipmentIndex5].IsSameType(weaponBeingPickedUp) && agentPickingUp.Equipment[equipmentIndex5].HasEnoughSpaceForAmount(weaponBeingPickedUp.Amount))
						{
							equipmentIndex4 = equipmentIndex5;
							continue;
						}
					}
					if (equipmentIndex2 == EquipmentIndex.None && agentPickingUp.Equipment[equipmentIndex5].IsEmpty)
					{
						equipmentIndex2 = equipmentIndex5;
					}
				}
				if (flag)
				{
					equipmentIndex = wieldedItemIndex;
				}
				else if (equipmentIndex3 != EquipmentIndex.None)
				{
					equipmentIndex = equipmentIndex4;
				}
				else if (flag2)
				{
					equipmentIndex = wieldedItemIndex;
				}
				else if (equipmentIndex4 != EquipmentIndex.None)
				{
					equipmentIndex = equipmentIndex4;
				}
				else if (equipmentIndex2 != EquipmentIndex.None)
				{
					equipmentIndex = equipmentIndex2;
				}
			}
			else
			{
				bool isConsumable2 = weaponBeingPickedUp.Item.PrimaryWeapon.IsConsumable;
				if (isConsumable2 && weaponBeingPickedUp.Amount == 0)
				{
					equipmentIndex = EquipmentIndex.None;
				}
				else
				{
					if (handIndex == Agent.HandIndex.OffHand && wieldedItemIndex != EquipmentIndex.None)
					{
						for (int i = 0; i < 4; i++)
						{
							if (i != (int)wieldedItemIndex && !agentPickingUp.Equipment[i].IsEmpty && agentPickingUp.Equipment[i].Item.ItemFlags.HasAnyFlag(ItemFlags.HeldInOffHand))
							{
								equipmentIndex = wieldedItemIndex;
								break;
							}
						}
					}
					if (equipmentIndex == EquipmentIndex.None && isConsumable2)
					{
						for (EquipmentIndex equipmentIndex6 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex6 < EquipmentIndex.ExtraWeaponSlot; equipmentIndex6++)
						{
							if (!agentPickingUp.Equipment[equipmentIndex6].IsEmpty && agentPickingUp.Equipment[equipmentIndex6].IsSameType(weaponBeingPickedUp) && agentPickingUp.Equipment[equipmentIndex6].Amount < agentPickingUp.Equipment[equipmentIndex6].ModifiedMaxAmount)
							{
								equipmentIndex = equipmentIndex6;
								break;
							}
						}
					}
					if (equipmentIndex == EquipmentIndex.None)
					{
						for (EquipmentIndex equipmentIndex7 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex7 < EquipmentIndex.ExtraWeaponSlot; equipmentIndex7++)
						{
							if (agentPickingUp.Equipment[equipmentIndex7].IsEmpty)
							{
								equipmentIndex = equipmentIndex7;
								break;
							}
						}
					}
					if (equipmentIndex == EquipmentIndex.None)
					{
						for (EquipmentIndex equipmentIndex8 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex8 < EquipmentIndex.ExtraWeaponSlot; equipmentIndex8++)
						{
							if (!agentPickingUp.Equipment[equipmentIndex8].IsEmpty && agentPickingUp.Equipment[equipmentIndex8].IsAnyConsumable() && agentPickingUp.Equipment[equipmentIndex8].Amount == 0)
							{
								equipmentIndex = equipmentIndex8;
								break;
							}
						}
					}
					if (equipmentIndex == EquipmentIndex.None && !missionWeapon.IsEmpty)
					{
						equipmentIndex = wieldedItemIndex;
					}
					if (equipmentIndex == EquipmentIndex.None)
					{
						equipmentIndex = EquipmentIndex.WeaponItemBeginSlot;
					}
				}
			}
		}
		return equipmentIndex;
	}

	public bool HasAmmo(EquipmentIndex equipmentIndex, out int rangedUsageIndex, out bool hasLoadedAmmo, out bool noAmmoInThisSlot)
	{
		hasLoadedAmmo = false;
		noAmmoInThisSlot = false;
		MissionWeapon missionWeapon = _weaponSlots[(int)equipmentIndex];
		rangedUsageIndex = missionWeapon.GetRangedUsageIndex();
		if (rangedUsageIndex >= 0)
		{
			if (missionWeapon.Ammo > 0)
			{
				hasLoadedAmmo = true;
				return true;
			}
			noAmmoInThisSlot = missionWeapon.IsAnyConsumable() && missionWeapon.Amount == 0;
			for (EquipmentIndex equipmentIndex2 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex2 < EquipmentIndex.NumAllWeaponSlots; equipmentIndex2++)
			{
				MissionWeapon missionWeapon2 = this[(int)equipmentIndex2];
				if (!missionWeapon2.IsEmpty && missionWeapon2.HasAnyUsageWithWeaponClass(missionWeapon.GetWeaponComponentDataForUsage(rangedUsageIndex).AmmoClass) && this[(int)equipmentIndex2].ModifiedMaxAmount > 1 && missionWeapon2.Amount > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetAmmoAmount(EquipmentIndex weaponIndex)
	{
		if (this[weaponIndex].IsAnyConsumable() && this[weaponIndex].ModifiedMaxAmount <= 1)
		{
			return this[weaponIndex].ModifiedMaxAmount;
		}
		int num = 0;
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			if (!this[(int)equipmentIndex].IsEmpty && this[(int)equipmentIndex].CurrentUsageItem.WeaponClass == this[weaponIndex].CurrentUsageItem.AmmoClass && this[(int)equipmentIndex].ModifiedMaxAmount > 1)
			{
				num += this[(int)equipmentIndex].Amount;
			}
		}
		return num;
	}

	public int GetMaxAmmo(EquipmentIndex weaponIndex)
	{
		if (this[weaponIndex].IsAnyConsumable() && this[weaponIndex].ModifiedMaxAmount <= 1)
		{
			return this[weaponIndex].ModifiedMaxAmount;
		}
		int num = 0;
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			if (!this[(int)equipmentIndex].IsEmpty && this[(int)equipmentIndex].CurrentUsageItem.WeaponClass == this[weaponIndex].CurrentUsageItem.AmmoClass && this[(int)equipmentIndex].ModifiedMaxAmount > 1)
			{
				num += this[(int)equipmentIndex].ModifiedMaxAmount;
			}
		}
		return num;
	}

	public void GetAmmoCountAndIndexOfType(ItemObject.ItemTypeEnum itemType, out int ammoCount, out EquipmentIndex eIndex, EquipmentIndex equippedIndex = EquipmentIndex.None)
	{
		ItemObject.ItemTypeEnum ammoTypeForItemType = ItemObject.GetAmmoTypeForItemType(itemType);
		ItemObject itemObject;
		if (equippedIndex != EquipmentIndex.None)
		{
			itemObject = this[equippedIndex].Item;
			ammoCount = 0;
		}
		else
		{
			itemObject = null;
			ammoCount = -1;
		}
		eIndex = equippedIndex;
		if (ammoTypeForItemType == ItemObject.ItemTypeEnum.Invalid)
		{
			return;
		}
		for (EquipmentIndex equipmentIndex = EquipmentIndex.Weapon3; equipmentIndex >= EquipmentIndex.WeaponItemBeginSlot; equipmentIndex--)
		{
			if (!this[equipmentIndex].IsEmpty && this[equipmentIndex].Item.Type == ammoTypeForItemType)
			{
				int amount = this[equipmentIndex].Amount;
				if (amount > 0)
				{
					if (itemObject == null)
					{
						eIndex = equipmentIndex;
						itemObject = this[equipmentIndex].Item;
						ammoCount = amount;
					}
					else if (itemObject.Id == this[equipmentIndex].Item.Id)
					{
						ammoCount += amount;
					}
				}
			}
		}
	}

	public static bool DoesWeaponFitToSlot(EquipmentIndex slotIndex, MissionWeapon weapon)
	{
		if (weapon.IsEmpty)
		{
			return true;
		}
		if (weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.DropOnWeaponChange | ItemFlags.DropOnAnyAction))
		{
			return slotIndex == EquipmentIndex.ExtraWeaponSlot;
		}
		return slotIndex >= EquipmentIndex.WeaponItemBeginSlot && slotIndex < EquipmentIndex.ExtraWeaponSlot;
	}

	public void CheckLoadedAmmos()
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			if (!this[equipmentIndex].IsEmpty && this[equipmentIndex].Item.PrimaryWeapon.WeaponClass == WeaponClass.Crossbow)
			{
				GetAmmoCountAndIndexOfType(this[equipmentIndex].Item.Type, out var _, out var eIndex);
				if (eIndex != EquipmentIndex.None)
				{
					MissionWeapon ammoWeapon = _weaponSlots[(int)eIndex].Consume(MathF.Min(this[equipmentIndex].MaxAmmo, _weaponSlots[(int)eIndex].Amount));
					_weaponSlots[(int)equipmentIndex].ReloadAmmo(ammoWeapon, _weaponSlots[(int)equipmentIndex].ReloadPhaseCount);
				}
			}
		}
		_cache.InvalidateOnWeaponAmmoUpdated();
	}

	public void SetUsageIndexOfSlot(EquipmentIndex slotIndex, int usageIndex)
	{
		_weaponSlots[(int)slotIndex].CurrentUsageIndex = usageIndex;
		_cache.InvalidateOnWeaponUsageIndexUpdated();
	}

	public void SetReloadPhaseOfSlot(EquipmentIndex slotIndex, short reloadPhase)
	{
		_weaponSlots[(int)slotIndex].ReloadPhase = reloadPhase;
	}

	public void SetAmountOfSlot(EquipmentIndex slotIndex, short dataValue, bool addOverflowToMaxAmount = false)
	{
		if (addOverflowToMaxAmount)
		{
			short num = (short)(dataValue - _weaponSlots[(int)slotIndex].Amount);
			if (num > 0)
			{
				_weaponSlots[(int)slotIndex].AddExtraModifiedMaxValue(num);
			}
		}
		short amount = _weaponSlots[(int)slotIndex].Amount;
		_weaponSlots[(int)slotIndex].Amount = dataValue;
		_cache.InvalidateOnWeaponAmmoUpdated();
		if ((amount != 0 && dataValue == 0) || (amount == 0 && dataValue != 0))
		{
			_cache.InvalidateOnWeaponAmmoAvailabilityChanged();
		}
	}

	public void SetHitPointsOfSlot(EquipmentIndex slotIndex, short dataValue, bool addOverflowToMaxHitPoints = false)
	{
		if (addOverflowToMaxHitPoints)
		{
			short num = (short)(dataValue - _weaponSlots[(int)slotIndex].HitPoints);
			if (num > 0)
			{
				_weaponSlots[(int)slotIndex].AddExtraModifiedMaxValue(num);
			}
		}
		_weaponSlots[(int)slotIndex].HitPoints = dataValue;
		_cache.InvalidateOnWeaponHitPointsUpdated();
		if (dataValue == 0)
		{
			_cache.InvalidateOnWeaponDestroyed();
		}
	}

	public void SetReloadedAmmoOfSlot(EquipmentIndex slotIndex, EquipmentIndex ammoSlotIndex, short totalAmmo)
	{
		if (ammoSlotIndex == EquipmentIndex.None)
		{
			_weaponSlots[(int)slotIndex].SetAmmo(MissionWeapon.Invalid);
		}
		else
		{
			MissionWeapon ammo = _weaponSlots[(int)ammoSlotIndex];
			ammo.Amount = totalAmmo;
			_weaponSlots[(int)slotIndex].SetAmmo(ammo);
		}
		_cache.InvalidateOnWeaponAmmoUpdated();
	}

	public void SetConsumedAmmoOfSlot(EquipmentIndex slotIndex, short count)
	{
		_weaponSlots[(int)slotIndex].ConsumeAmmo(count);
		_cache.InvalidateOnWeaponAmmoUpdated();
	}

	public void AttachWeaponToWeaponInSlot(EquipmentIndex slotIndex, ref MissionWeapon weapon, ref MatrixFrame attachLocalFrame)
	{
		_weaponSlots[(int)slotIndex].AttachWeapon(weapon, ref attachLocalFrame);
	}

	public bool HasShield()
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			WeaponComponentData currentUsageItem = _weaponSlots[(int)equipmentIndex].CurrentUsageItem;
			if (currentUsageItem != null && currentUsageItem.IsShield)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyWeapon()
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			if (_weaponSlots[(int)equipmentIndex].CurrentUsageItem != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyWeaponWithFlags(WeaponFlags flags)
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			WeaponComponentData currentUsageItem = _weaponSlots[(int)equipmentIndex].CurrentUsageItem;
			if (currentUsageItem != null && currentUsageItem.WeaponFlags.HasAllFlags(flags))
			{
				return true;
			}
		}
		return false;
	}

	public ItemObject GetBanner()
	{
		ItemObject result = null;
		MissionWeapon missionWeapon = _weaponSlots[4];
		ItemObject item = missionWeapon.Item;
		if (item != null && item.IsBannerItem && item.BannerComponent != null)
		{
			result = item;
		}
		return result;
	}

	public bool HasRangedWeapon(WeaponClass requiredAmmoClass = WeaponClass.Undefined)
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			WeaponComponentData currentUsageItem = _weaponSlots[(int)equipmentIndex].CurrentUsageItem;
			if (currentUsageItem != null && currentUsageItem.IsRangedWeapon && (requiredAmmoClass == WeaponClass.Undefined || currentUsageItem.AmmoClass == requiredAmmoClass))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsNonConsumableRangedWeaponWithAmmo()
	{
		if (!_cache.IsValid(MissionEquipmentCache.CachedBool.ContainsNonConsumableRangedWeaponWithAmmo))
		{
			GatherInformationAndUpdateCache();
		}
		return _cache.GetValue(MissionEquipmentCache.CachedBool.ContainsNonConsumableRangedWeaponWithAmmo);
	}

	public bool ContainsMeleeWeapon()
	{
		if (!_cache.IsValid(MissionEquipmentCache.CachedBool.ContainsMeleeWeapon))
		{
			GatherInformationAndUpdateCache();
		}
		return _cache.GetValue(MissionEquipmentCache.CachedBool.ContainsMeleeWeapon);
	}

	public bool ContainsShield()
	{
		if (!_cache.IsValid(MissionEquipmentCache.CachedBool.ContainsShield))
		{
			GatherInformationAndUpdateCache();
		}
		return _cache.GetValue(MissionEquipmentCache.CachedBool.ContainsShield);
	}

	public bool ContainsSpear()
	{
		if (!_cache.IsValid(MissionEquipmentCache.CachedBool.ContainsSpear))
		{
			GatherInformationAndUpdateCache();
		}
		return _cache.GetValue(MissionEquipmentCache.CachedBool.ContainsSpear);
	}

	public bool ContainsThrownWeapon()
	{
		if (!_cache.IsValid(MissionEquipmentCache.CachedBool.ContainsThrownWeapon))
		{
			GatherInformationAndUpdateCache();
		}
		return _cache.GetValue(MissionEquipmentCache.CachedBool.ContainsThrownWeapon);
	}

	private void GatherInformationAndUpdateCache()
	{
		GatherInformation(out var containsMeleeWeapon, out var containsShield, out var containsSpear, out var containsNonConsumableRangedWeaponWithAmmo, out var containsThrownWeapon);
		_cache.UpdateAndMarkValid(MissionEquipmentCache.CachedBool.ContainsMeleeWeapon, containsMeleeWeapon);
		_cache.UpdateAndMarkValid(MissionEquipmentCache.CachedBool.ContainsShield, containsShield);
		_cache.UpdateAndMarkValid(MissionEquipmentCache.CachedBool.ContainsSpear, containsSpear);
		_cache.UpdateAndMarkValid(MissionEquipmentCache.CachedBool.ContainsNonConsumableRangedWeaponWithAmmo, containsNonConsumableRangedWeaponWithAmmo);
		_cache.UpdateAndMarkValid(MissionEquipmentCache.CachedBool.ContainsThrownWeapon, containsThrownWeapon);
	}

	private void GatherInformation(out bool containsMeleeWeapon, out bool containsShield, out bool containsSpear, out bool containsNonConsumableRangedWeaponWithAmmo, out bool containsThrownWeapon)
	{
		containsMeleeWeapon = false;
		containsShield = false;
		containsSpear = false;
		containsNonConsumableRangedWeaponWithAmmo = false;
		containsThrownWeapon = false;
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			_weaponSlots[(int)equipmentIndex].GatherInformationFromWeapon(out var weaponHasMelee, out var weaponHasShield, out var weaponHasPolearm, out var weaponHasNonConsumableRanged, out var weaponHasThrown, out var _);
			containsMeleeWeapon |= weaponHasMelee;
			containsShield |= weaponHasShield;
			containsSpear |= weaponHasPolearm;
			containsThrownWeapon |= weaponHasThrown;
			if (weaponHasNonConsumableRanged)
			{
				containsNonConsumableRangedWeaponWithAmmo = containsNonConsumableRangedWeaponWithAmmo || GetAmmoAmount(equipmentIndex) > 0;
			}
		}
	}

	public void SetGlossMultipliersOfWeaponsRandomly(int seed)
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			_weaponSlots[(int)equipmentIndex].SetRandomGlossMultiplier(seed);
		}
	}
}
