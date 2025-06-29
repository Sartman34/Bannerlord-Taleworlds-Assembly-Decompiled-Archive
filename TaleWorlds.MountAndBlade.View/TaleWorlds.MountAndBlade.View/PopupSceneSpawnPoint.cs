using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Scripts;

namespace TaleWorlds.MountAndBlade.View;

public class PopupSceneSpawnPoint : ScriptComponentBehavior
{
	public string InitialAction = "";

	public string NegativeAction = "";

	public string InitialFaceAnimCode = "";

	public string PositiveFaceAnimCode = "";

	public string NegativeFaceAnimCode = "";

	public string PositiveAction = "";

	public string LeftHandWieldedItem = "";

	public string RightHandWieldedItem = "";

	public string BannerTagToUseForAddedPrefab = "";

	public bool StartWithRandomProgress;

	public Vec3 AttachedPrefabOffset = Vec3.Zero;

	public string PrefabItem = "";

	public HumanBone PrefabBone = HumanBone.ItemR;

	private AgentVisuals _mountAgentVisuals;

	private AgentVisuals _humanAgentVisuals;

	private ActionIndexCache _initialStateActionCode;

	private ActionIndexCache _positiveStateActionCode;

	private ActionIndexCache _negativeStateActionCode;

	private static readonly ActionIndexCache default_act_horse_stand = ActionIndexCache.Create("act_horse_stand_1");

	private static readonly ActionIndexCache default_act_camel_stand = ActionIndexCache.Create("act_camel_stand_1");

	public CompositeComponent AddedPrefabComponent { get; private set; }

	protected override void OnInit()
	{
		base.OnInit();
		SetScriptComponentToTick(GetTickRequirement());
	}

	public void InitializeWithAgentVisuals(AgentVisuals humanVisuals, AgentVisuals mountVisuals = null)
	{
		_humanAgentVisuals = humanVisuals;
		_mountAgentVisuals = mountVisuals;
		_initialStateActionCode = ActionIndexCache.Create(InitialAction);
		_positiveStateActionCode = ((PositiveAction == "") ? _initialStateActionCode : ActionIndexCache.Create(PositiveAction));
		_negativeStateActionCode = ((NegativeAction == "") ? _initialStateActionCode : ActionIndexCache.Create(NegativeAction));
		bool flag = !string.IsNullOrEmpty(RightHandWieldedItem);
		bool flag2 = !string.IsNullOrEmpty(LeftHandWieldedItem);
		if (flag2 || flag)
		{
			AgentVisualsData copyAgentVisualsData = _humanAgentVisuals.GetCopyAgentVisualsData();
			Equipment equipment = _humanAgentVisuals.GetEquipment().Clone();
			if (flag)
			{
				equipment[EquipmentIndex.WeaponItemBeginSlot] = new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>(RightHandWieldedItem));
			}
			if (flag2)
			{
				equipment[EquipmentIndex.Weapon1] = new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>(LeftHandWieldedItem));
			}
			int rightWieldedItemIndex = ((!flag) ? (-1) : 0);
			int leftWieldedItemIndex = (flag2 ? 1 : (-1));
			copyAgentVisualsData.RightWieldedItemIndex(rightWieldedItemIndex).LeftWieldedItemIndex(leftWieldedItemIndex).Equipment(equipment);
			_humanAgentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData);
		}
		else if (!flag2 && !flag2)
		{
			AgentVisualsData copyAgentVisualsData2 = _humanAgentVisuals.GetCopyAgentVisualsData();
			Equipment equipment2 = _humanAgentVisuals.GetEquipment().Clone();
			copyAgentVisualsData2.RightWieldedItemIndex(-1).LeftWieldedItemIndex(-1).Equipment(equipment2);
			_humanAgentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData2);
		}
		if (PrefabItem != "")
		{
			if (!GameEntity.PrefabExists(PrefabItem))
			{
				MBDebug.ShowWarning("Cannot find prefab '" + PrefabItem + "' for popup agent '" + base.GameEntity.Name + "'");
			}
			else
			{
				AddedPrefabComponent = _humanAgentVisuals.AddPrefabToAgentVisualBoneByBoneType(PrefabItem, PrefabBone);
				if (AddedPrefabComponent != null)
				{
					MatrixFrame frame = AddedPrefabComponent.Frame;
					MatrixFrame identity = MatrixFrame.Identity;
					identity.origin = AttachedPrefabOffset;
					AddedPrefabComponent.Frame = identity * frame;
				}
			}
		}
		foreach (GameEntity item in base.GameEntity.Scene.FindEntitiesWithTag(base.GameEntity.Name))
		{
			item.GetFirstScriptOfType<PopupSceneSequence>()?.InitializeWithAgentVisuals(humanVisuals);
		}
		_humanAgentVisuals?.GetVisuals()?.CheckResources(addToQueue: true);
		_mountAgentVisuals?.GetVisuals()?.CheckResources(addToQueue: true);
		_mountAgentVisuals?.Tick(null, 0.0001f);
		_mountAgentVisuals?.GetEntity()?.Skeleton?.ForceUpdateBoneFrames();
		_humanAgentVisuals?.Tick(_mountAgentVisuals, 0.0001f);
		_humanAgentVisuals?.GetEntity()?.Skeleton?.ForceUpdateBoneFrames();
	}

	public void SetInitialState()
	{
		if (_initialStateActionCode != null)
		{
			float startProgress = (StartWithRandomProgress ? MBRandom.RandomFloatRanged(0.5f) : 0f);
			_humanAgentVisuals?.SetAction(_initialStateActionCode, startProgress);
			_mountAgentVisuals?.SetAction(_initialStateActionCode, startProgress);
		}
		if (!string.IsNullOrEmpty(InitialFaceAnimCode))
		{
			_humanAgentVisuals.GetVisuals().GetSkeleton().SetFacialAnimation(Agent.FacialAnimChannel.Mid, InitialFaceAnimCode, playSound: false, loop: true);
		}
		foreach (GameEntity item in base.GameEntity.Scene.FindEntitiesWithTag(base.GameEntity.Name))
		{
			item.GetFirstScriptOfType<PopupSceneSequence>()?.SetInitialState();
		}
	}

	public void SetPositiveState()
	{
		if (_positiveStateActionCode != null)
		{
			_humanAgentVisuals?.SetAction(_positiveStateActionCode);
			_mountAgentVisuals?.SetAction(_positiveStateActionCode);
		}
		if (!string.IsNullOrEmpty(PositiveFaceAnimCode))
		{
			_humanAgentVisuals.GetVisuals().GetSkeleton().SetFacialAnimation(Agent.FacialAnimChannel.Mid, PositiveFaceAnimCode, playSound: false, loop: true);
		}
		foreach (GameEntity item in base.GameEntity.Scene.FindEntitiesWithTag(base.GameEntity.Name))
		{
			item.GetFirstScriptOfType<PopupSceneSequence>()?.SetPositiveState();
		}
	}

	public void SetNegativeState()
	{
		if (_negativeStateActionCode != null)
		{
			_humanAgentVisuals?.SetAction(_negativeStateActionCode);
			_mountAgentVisuals?.SetAction(_negativeStateActionCode);
		}
		if (!string.IsNullOrEmpty(NegativeFaceAnimCode))
		{
			_humanAgentVisuals.GetVisuals().GetSkeleton().SetFacialAnimation(Agent.FacialAnimChannel.Mid, NegativeFaceAnimCode, playSound: false, loop: true);
		}
		foreach (GameEntity item in base.GameEntity.Scene.FindEntitiesWithTag(base.GameEntity.Name))
		{
			item.GetFirstScriptOfType<PopupSceneSequence>()?.SetNegativeState();
		}
	}

	public void Destroy()
	{
		_humanAgentVisuals?.Reset();
		_humanAgentVisuals = null;
		_mountAgentVisuals?.Reset();
		_mountAgentVisuals = null;
		_initialStateActionCode = null;
		_positiveStateActionCode = null;
		_negativeStateActionCode = null;
	}

	public override TickRequirement GetTickRequirement()
	{
		return TickRequirement.Tick | base.GetTickRequirement();
	}

	protected override void OnTick(float dt)
	{
		_mountAgentVisuals?.Tick(null, dt);
		_mountAgentVisuals?.GetEntity()?.Skeleton?.ForceUpdateBoneFrames();
		_humanAgentVisuals?.Tick(_mountAgentVisuals, dt);
		_humanAgentVisuals?.GetEntity()?.Skeleton?.ForceUpdateBoneFrames();
	}
}
