using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public class StandingPointWithWeaponRequirement : StandingPoint
{
	private ItemObject _requiredWeapon;

	private ItemObject _givenWeapon;

	private WeaponClass _requiredWeaponClass1;

	private WeaponClass _requiredWeaponClass2;

	private bool _hasAlternative;

	public StandingPointWithWeaponRequirement()
	{
		AutoSheathWeapons = false;
		_requiredWeaponClass1 = WeaponClass.Undefined;
		_requiredWeaponClass2 = WeaponClass.Undefined;
		_hasAlternative = base.HasAlternative();
	}

	protected internal override void OnInit()
	{
		base.OnInit();
	}

	public void InitRequiredWeaponClasses(WeaponClass requiredWeaponClass1, WeaponClass requiredWeaponClass2 = WeaponClass.Undefined)
	{
		_requiredWeaponClass1 = requiredWeaponClass1;
		_requiredWeaponClass2 = requiredWeaponClass2;
	}

	public void InitRequiredWeapon(ItemObject weapon)
	{
		_requiredWeapon = weapon;
	}

	public void InitGivenWeapon(ItemObject weapon)
	{
		_givenWeapon = weapon;
	}

	public override bool IsDisabledForAgent(Agent agent)
	{
		EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
		if (_requiredWeapon != null)
		{
			if (wieldedItemIndex != EquipmentIndex.None && agent.Equipment[wieldedItemIndex].Item == _requiredWeapon)
			{
				return base.IsDisabledForAgent(agent);
			}
		}
		else if (_givenWeapon != null)
		{
			if (wieldedItemIndex == EquipmentIndex.None || agent.Equipment[wieldedItemIndex].Item != _givenWeapon)
			{
				return base.IsDisabledForAgent(agent);
			}
		}
		else if ((_requiredWeaponClass1 != 0 || _requiredWeaponClass2 != 0) && wieldedItemIndex != EquipmentIndex.None)
		{
			for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
			{
				if (!agent.Equipment[equipmentIndex].IsEmpty && (agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == _requiredWeaponClass1 || agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == _requiredWeaponClass2) && (!agent.Equipment[equipmentIndex].CurrentUsageItem.IsConsumable || agent.Equipment[equipmentIndex].Amount < agent.Equipment[equipmentIndex].ModifiedMaxAmount || equipmentIndex == EquipmentIndex.ExtraWeaponSlot))
				{
					return base.IsDisabledForAgent(agent);
				}
			}
		}
		return true;
	}

	public void SetHasAlternative(bool hasAlternative)
	{
		_hasAlternative = hasAlternative;
	}

	public override bool HasAlternative()
	{
		return _hasAlternative;
	}

	public void SetUsingBattleSide(BattleSideEnum side)
	{
		StandingPointSide = side;
	}
}
