using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class ArrowBarrel : UsableMachine
{
	private static readonly ActionIndexCache act_pickup_down_begin = ActionIndexCache.Create("act_pickup_down_begin");

	private static readonly ActionIndexCache act_pickup_down_begin_left_stance = ActionIndexCache.Create("act_pickup_down_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_down_end = ActionIndexCache.Create("act_pickup_down_end");

	private static readonly ActionIndexCache act_pickup_down_end_left_stance = ActionIndexCache.Create("act_pickup_down_end_left_stance");

	private static int _pickupArrowSoundFromBarrel = -1;

	private bool _isVisible = true;

	private bool _needsSingleThreadTickOnce;

	private static int _pickupArrowSoundFromBarrelCache
	{
		get
		{
			if (_pickupArrowSoundFromBarrel == -1)
			{
				_pickupArrowSoundFromBarrel = SoundEvent.GetEventIdFromString("event:/mission/combat/pickup_arrows");
			}
			return _pickupArrowSoundFromBarrel;
		}
	}

	protected ArrowBarrel()
	{
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		foreach (StandingPoint standingPoint in base.StandingPoints)
		{
			(standingPoint as StandingPointWithWeaponRequirement).InitRequiredWeaponClasses(WeaponClass.Arrow, WeaponClass.Bolt);
		}
		SetScriptComponentToTick(GetTickRequirement());
		MakeVisibilityCheck = false;
		_isVisible = base.GameEntity.IsVisibleIncludeParents();
	}

	public override void AfterMissionStart()
	{
		if (base.StandingPoints == null)
		{
			return;
		}
		foreach (StandingPoint standingPoint in base.StandingPoints)
		{
			standingPoint.LockUserFrames = true;
		}
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		TextObject textObject = new TextObject("{=bNYm3K6b}{KEY} Pick Up");
		textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
		return textObject;
	}

	public override string GetDescriptionText(GameEntity gameEntity = null)
	{
		return new TextObject("{=bWi4aMO9}Arrow Barrel").ToString();
	}

	public override TickRequirement GetTickRequirement()
	{
		if (GameNetwork.IsClientOrReplay)
		{
			return base.GetTickRequirement();
		}
		return TickRequirement.Tick | TickRequirement.TickParallel | base.GetTickRequirement();
	}

	protected internal override void OnTickParallel(float dt)
	{
		TickAux(isParallel: true);
	}

	protected internal override void OnTick(float dt)
	{
		base.OnTick(dt);
		if (_needsSingleThreadTickOnce)
		{
			_needsSingleThreadTickOnce = false;
			TickAux(isParallel: false);
		}
	}

	private void TickAux(bool isParallel)
	{
		if (!_isVisible || GameNetwork.IsClientOrReplay)
		{
			return;
		}
		foreach (StandingPoint standingPoint in base.StandingPoints)
		{
			if (!standingPoint.HasUser)
			{
				continue;
			}
			Agent userAgent = standingPoint.UserAgent;
			ActionIndexValueCache currentActionValue = userAgent.GetCurrentActionValue(0);
			ActionIndexValueCache currentActionValue2 = userAgent.GetCurrentActionValue(1);
			if (currentActionValue2 == ActionIndexValueCache.act_none && (currentActionValue == act_pickup_down_begin || currentActionValue == act_pickup_down_begin_left_stance))
			{
				continue;
			}
			if (currentActionValue2 == ActionIndexValueCache.act_none && (currentActionValue == act_pickup_down_end || currentActionValue == act_pickup_down_end_left_stance))
			{
				if (isParallel)
				{
					_needsSingleThreadTickOnce = true;
					continue;
				}
				for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
				{
					if (!userAgent.Equipment[equipmentIndex].IsEmpty && (userAgent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Arrow || userAgent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Bolt) && userAgent.Equipment[equipmentIndex].Amount < userAgent.Equipment[equipmentIndex].ModifiedMaxAmount)
					{
						userAgent.SetWeaponAmountInSlot(equipmentIndex, userAgent.Equipment[equipmentIndex].ModifiedMaxAmount, enforcePrimaryItem: true);
						Mission.Current.MakeSoundOnlyOnRelatedPeer(_pickupArrowSoundFromBarrelCache, userAgent.Position, userAgent.Index);
					}
				}
				userAgent.StopUsingGameObject();
			}
			else if (currentActionValue2 != ActionIndexValueCache.act_none || !userAgent.SetActionChannel(0, userAgent.GetIsLeftStance() ? act_pickup_down_begin_left_stance : act_pickup_down_begin, ignorePriority: false, 0uL))
			{
				if (isParallel)
				{
					_needsSingleThreadTickOnce = true;
				}
				else
				{
					userAgent.StopUsingGameObject();
				}
			}
		}
	}

	public override OrderType GetOrder(BattleSideEnum side)
	{
		return OrderType.None;
	}
}
