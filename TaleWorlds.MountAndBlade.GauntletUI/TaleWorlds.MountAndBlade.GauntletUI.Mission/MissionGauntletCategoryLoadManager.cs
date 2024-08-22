using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[DefaultView]
public class MissionGauntletCategoryLoadManager : MissionView, IMissionListener
{
	private SpriteCategory _fullBackgroundCategory;

	private SpriteCategory _backgroundCategory;

	private SpriteCategory _fullscreensCategory;

	private SpriteCategory _mapBarCategory;

	private SpriteCategory _encyclopediaCategory;

	private MissionGauntletOptionsUIHandler _optionsView;

	private ITwoDimensionResourceContext _resourceContext => UIResourceManager.ResourceContext;

	private ResourceDepot _resourceDepot => UIResourceManager.UIResourceDepot;

	private SpriteData _spriteData => UIResourceManager.SpriteData;

	public override void AfterStart()
	{
		base.AfterStart();
		if (_fullBackgroundCategory == null)
		{
			_fullBackgroundCategory = _spriteData.SpriteCategories["ui_fullbackgrounds"];
		}
		if (_backgroundCategory == null)
		{
			_backgroundCategory = _spriteData.SpriteCategories["ui_backgrounds"];
		}
		if (_fullscreensCategory == null)
		{
			_fullscreensCategory = _spriteData.SpriteCategories["ui_fullscreens"];
		}
		if (_encyclopediaCategory == null)
		{
			_encyclopediaCategory = _spriteData.SpriteCategories["ui_encyclopedia"];
		}
		if (_mapBarCategory == null && _spriteData.SpriteCategories.ContainsKey("ui_mapbar") && _spriteData.SpriteCategories["ui_mapbar"].IsLoaded)
		{
			_mapBarCategory = _spriteData.SpriteCategories["ui_mapbar"];
		}
		if (_optionsView == null)
		{
			_optionsView = base.Mission.GetMissionBehavior<MissionGauntletOptionsUIHandler>();
			base.Mission.AddListener(this);
		}
		HandleCategoryLoadingUnloading();
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		_optionsView = null;
		base.Mission.RemoveListener(this);
		LoadUnloadAllCategories(load: true);
	}

	public override void OnMissionTick(float dt)
	{
		base.OnMissionTick(dt);
		HandleCategoryLoadingUnloading();
	}

	private void HandleCategoryLoadingUnloading()
	{
		bool load = true;
		if (base.Mission != null)
		{
			load = IsBackgroundsUsedInMission(base.Mission);
		}
		LoadUnloadAllCategories(load);
	}

	private void LoadUnloadAllCategories(bool load)
	{
		if (load)
		{
			if (!_fullBackgroundCategory.IsLoaded)
			{
				_fullBackgroundCategory.Load(_resourceContext, _resourceDepot);
			}
			if (!_backgroundCategory.IsLoaded)
			{
				_backgroundCategory.Load(_resourceContext, _resourceDepot);
			}
			if (!_fullscreensCategory.IsLoaded)
			{
				_fullscreensCategory.Load(_resourceContext, _resourceDepot);
			}
			if (!_encyclopediaCategory.IsLoaded)
			{
				_encyclopediaCategory.Load(_resourceContext, _resourceDepot);
			}
			SpriteCategory mapBarCategory = _mapBarCategory;
			if (mapBarCategory != null && !mapBarCategory.IsLoaded)
			{
				_mapBarCategory.Load(_resourceContext, _resourceDepot);
			}
			return;
		}
		if (_fullBackgroundCategory.IsLoaded)
		{
			_fullBackgroundCategory.Unload();
		}
		if (_backgroundCategory.IsLoaded)
		{
			_backgroundCategory.Unload();
		}
		if (_fullscreensCategory.IsLoaded && !_optionsView.IsEnabled)
		{
			_fullscreensCategory.Unload();
		}
		if (_encyclopediaCategory.IsLoaded)
		{
			TaleWorlds.MountAndBlade.Mission mission = base.Mission;
			if (mission == null || mission.Mode != MissionMode.Conversation)
			{
				_encyclopediaCategory.Unload();
			}
		}
		SpriteCategory mapBarCategory2 = _mapBarCategory;
		if (mapBarCategory2 != null && mapBarCategory2.IsLoaded)
		{
			_mapBarCategory.Unload();
		}
	}

	private bool IsBackgroundsUsedInMission(TaleWorlds.MountAndBlade.Mission mission)
	{
		if (!mission.IsInventoryAccessAllowed && !mission.IsCharacterWindowAccessAllowed && !mission.IsClanWindowAccessAllowed && !mission.IsKingdomWindowAccessAllowed && !mission.IsQuestScreenAccessAllowed && !mission.IsPartyWindowAccessAllowed)
		{
			return mission.IsEncyclopediaWindowAccessAllowed;
		}
		return true;
	}

	void IMissionListener.OnEquipItemsFromSpawnEquipmentBegin(Agent agent, Agent.CreationType creationType)
	{
	}

	void IMissionListener.OnEquipItemsFromSpawnEquipment(Agent agent, Agent.CreationType creationType)
	{
	}

	void IMissionListener.OnEndMission()
	{
	}

	void IMissionListener.OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		HandleCategoryLoadingUnloading();
	}

	void IMissionListener.OnConversationCharacterChanged()
	{
	}

	void IMissionListener.OnResetMission()
	{
	}

	void IMissionListener.OnInitialDeploymentPlanMade(BattleSideEnum battleSide, bool isFirstPlan)
	{
	}
}
