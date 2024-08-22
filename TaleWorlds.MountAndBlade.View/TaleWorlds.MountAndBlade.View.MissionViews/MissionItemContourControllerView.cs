using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionItemContourControllerView : MissionView
{
	private GameEntity[] _tempPickableEntities = new GameEntity[128];

	private UIntPtr[] _pickableItemsId = new UIntPtr[128];

	private List<GameEntity> _contourItems;

	private GameEntity _focusedGameEntity;

	private IFocusable _currentFocusedObject;

	private bool _isContourAppliedToAllItems;

	private bool _isContourAppliedToFocusedItem;

	private uint _nonFocusedDefaultContourColor = new Color(0.85f, 0.85f, 0.85f).ToUnsignedInteger();

	private uint _nonFocusedAmmoContourColor = new Color(0f, 0.73f, 1f).ToUnsignedInteger();

	private uint _nonFocusedThrowableContourColor = new Color(0.051f, 0.988f, 0.18f).ToUnsignedInteger();

	private uint _nonFocusedBannerContourColor = new Color(0.521f, 0.988f, 0.521f).ToUnsignedInteger();

	private uint _focusedContourColor = new Color(1f, 0.84f, 0.35f).ToUnsignedInteger();

	private float _lastItemQueryTime;

	private float _sceneItemQueryFreq = 1f;

	private bool _isAllowedByOption
	{
		get
		{
			if (BannerlordConfig.HideBattleUI)
			{
				return GameNetwork.IsMultiplayer;
			}
			return true;
		}
	}

	public MissionItemContourControllerView()
	{
		_contourItems = new List<GameEntity>();
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (!_isAllowedByOption)
		{
			return;
		}
		if (Agent.Main != null && base.MissionScreen.InputManager.IsGameKeyDown(5))
		{
			RemoveContourFromAllItems();
			PopulateContourListWithNearbyItems();
			ApplyContourToAllItems();
			_lastItemQueryTime = base.Mission.CurrentTime;
		}
		else
		{
			RemoveContourFromAllItems();
			_contourItems.Clear();
		}
		if (_isContourAppliedToAllItems)
		{
			float currentTime = base.Mission.CurrentTime;
			if (currentTime - _lastItemQueryTime > _sceneItemQueryFreq)
			{
				RemoveContourFromAllItems();
				PopulateContourListWithNearbyItems();
				_lastItemQueryTime = currentTime;
			}
		}
	}

	public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
	{
		base.OnFocusGained(agent, focusableObject, isInteractable);
		if (!(_isAllowedByOption && focusableObject != _currentFocusedObject && isInteractable))
		{
			return;
		}
		_currentFocusedObject = focusableObject;
		if (focusableObject is UsableMissionObject usableMissionObject)
		{
			if (usableMissionObject is SpawnedItemEntity spawnedItemEntity)
			{
				_focusedGameEntity = spawnedItemEntity.GameEntity;
			}
			else if (!string.IsNullOrEmpty(usableMissionObject.ActionMessage.ToString()) && !string.IsNullOrEmpty(usableMissionObject.DescriptionMessage.ToString()))
			{
				_focusedGameEntity = usableMissionObject.GameEntity;
			}
			else
			{
				UsableMachine usableMachineFromPoint = GetUsableMachineFromPoint(usableMissionObject);
				if (usableMachineFromPoint != null)
				{
					_focusedGameEntity = usableMachineFromPoint.GameEntity;
				}
			}
		}
		AddContourToFocusedItem();
	}

	public override void OnFocusLost(Agent agent, IFocusable focusableObject)
	{
		base.OnFocusLost(agent, focusableObject);
		if (_isAllowedByOption)
		{
			RemoveContourFromFocusedItem();
			_currentFocusedObject = null;
			_focusedGameEntity = null;
		}
	}

	private void PopulateContourListWithNearbyItems()
	{
		_contourItems.Clear();
		float num = (GameNetwork.IsSessionActive ? 1f : 3f);
		float num2 = Agent.Main.MaximumForwardUnlimitedSpeed * num;
		Vec3 boundingBoxMin = Agent.Main.Position - new Vec3(num2, num2, 1f);
		Vec3 boundingBoxMax = Agent.Main.Position + new Vec3(num2, num2, 1.8f);
		int num3 = base.Mission.Scene.SelectEntitiesInBoxWithScriptComponent<SpawnedItemEntity>(ref boundingBoxMin, ref boundingBoxMax, _tempPickableEntities, _pickableItemsId);
		for (int i = 0; i < num3; i++)
		{
			SpawnedItemEntity firstScriptOfType = _tempPickableEntities[i].GetFirstScriptOfType<SpawnedItemEntity>();
			if (firstScriptOfType == null)
			{
				continue;
			}
			if (firstScriptOfType.IsBanner())
			{
				if (MissionGameModels.Current.BattleBannerBearersModel.IsInteractableFormationBanner(firstScriptOfType, Agent.Main))
				{
					_contourItems.Add(firstScriptOfType.GameEntity);
				}
			}
			else
			{
				_contourItems.Add(firstScriptOfType.GameEntity);
			}
		}
		int num4 = base.Mission.Scene.SelectEntitiesInBoxWithScriptComponent<UsableMachine>(ref boundingBoxMin, ref boundingBoxMax, _tempPickableEntities, _pickableItemsId);
		for (int j = 0; j < num4; j++)
		{
			UsableMachine firstScriptOfType2 = _tempPickableEntities[j].GetFirstScriptOfType<UsableMachine>();
			if (firstScriptOfType2 != null && !firstScriptOfType2.IsDisabled)
			{
				GameEntity validStandingPointForAgentWithoutDistanceCheck = firstScriptOfType2.GetValidStandingPointForAgentWithoutDistanceCheck(Agent.Main);
				if (validStandingPointForAgentWithoutDistanceCheck != null && !(validStandingPointForAgentWithoutDistanceCheck.GetFirstScriptOfType<UsableMissionObject>() is SpawnedItemEntity) && validStandingPointForAgentWithoutDistanceCheck.GetScriptComponents().FirstOrDefault((ScriptComponentBehavior sc) => sc is IFocusable) is IFocusable focusable && focusable is UsableMissionObject)
				{
					_contourItems.Add(firstScriptOfType2.GameEntity);
				}
			}
		}
	}

	private void ApplyContourToAllItems()
	{
		if (_isContourAppliedToAllItems)
		{
			return;
		}
		foreach (GameEntity contourItem in _contourItems)
		{
			uint nonFocusedColor = GetNonFocusedColor(contourItem);
			uint value = ((contourItem == _focusedGameEntity) ? _focusedContourColor : nonFocusedColor);
			contourItem.SetContourColor(value);
		}
		_isContourAppliedToAllItems = true;
	}

	private uint GetNonFocusedColor(GameEntity entity)
	{
		ItemObject obj = entity.GetFirstScriptOfType<SpawnedItemEntity>()?.WeaponCopy.Item;
		WeaponComponentData weaponComponentData = obj?.PrimaryWeapon;
		ItemObject.ItemTypeEnum? itemTypeEnum = obj?.ItemType;
		if (obj != null && obj.HasBannerComponent)
		{
			return _nonFocusedBannerContourColor;
		}
		if ((weaponComponentData != null && weaponComponentData.IsAmmo) || itemTypeEnum == ItemObject.ItemTypeEnum.Arrows || itemTypeEnum == ItemObject.ItemTypeEnum.Bolts || itemTypeEnum == ItemObject.ItemTypeEnum.Bullets)
		{
			return _nonFocusedAmmoContourColor;
		}
		if (itemTypeEnum == ItemObject.ItemTypeEnum.Thrown)
		{
			return _nonFocusedThrowableContourColor;
		}
		return _nonFocusedDefaultContourColor;
	}

	private void RemoveContourFromAllItems()
	{
		if (!_isContourAppliedToAllItems)
		{
			return;
		}
		foreach (GameEntity contourItem in _contourItems)
		{
			if (_focusedGameEntity == null || contourItem != _focusedGameEntity)
			{
				contourItem.SetContourColor(null);
			}
		}
		_isContourAppliedToAllItems = false;
	}

	private void AddContourToFocusedItem()
	{
		if (_focusedGameEntity != null && !_isContourAppliedToFocusedItem)
		{
			_focusedGameEntity.SetContourColor(_focusedContourColor);
			_isContourAppliedToFocusedItem = true;
		}
	}

	private void RemoveContourFromFocusedItem()
	{
		if (_focusedGameEntity != null && _isContourAppliedToFocusedItem)
		{
			if (_contourItems.Contains(_focusedGameEntity))
			{
				_focusedGameEntity.SetContourColor(_nonFocusedDefaultContourColor);
			}
			else
			{
				_focusedGameEntity.SetContourColor(null);
			}
			_isContourAppliedToFocusedItem = false;
		}
	}

	private UsableMachine GetUsableMachineFromPoint(UsableMissionObject standingPoint)
	{
		GameEntity gameEntity = standingPoint.GameEntity;
		while ((object)gameEntity != null && !gameEntity.HasScriptOfType<UsableMachine>())
		{
			gameEntity = gameEntity.Parent;
		}
		if (gameEntity != null)
		{
			UsableMachine firstScriptOfType = gameEntity.GetFirstScriptOfType<UsableMachine>();
			if (firstScriptOfType != null)
			{
				return firstScriptOfType;
			}
		}
		return null;
	}
}
