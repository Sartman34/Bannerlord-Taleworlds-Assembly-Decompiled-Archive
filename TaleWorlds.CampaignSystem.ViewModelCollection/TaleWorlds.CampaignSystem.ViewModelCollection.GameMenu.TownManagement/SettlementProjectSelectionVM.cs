using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;

public class SettlementProjectSelectionVM : ViewModel
{
	private readonly Town _town;

	private readonly Settlement _settlement;

	private readonly Action _onAnyChangeInQueue;

	private SettlementDailyProjectVM _currentDailyDefault;

	private SettlementProjectVM _currentSelectedProject;

	private MBBindingList<SettlementDailyProjectVM> _dailyDefaultList;

	private MBBindingList<SettlementBuildingProjectVM> _currentDevelopmentQueue;

	private MBBindingList<SettlementBuildingProjectVM> _availableProjects;

	private string _projectsText;

	private string _queueText;

	private string _dailyDefaultsText;

	private string _dailyDefaultsExplanationText;

	public List<Building> LocalDevelopmentList { get; private set; }

	[DataSourceProperty]
	public string ProjectsText
	{
		get
		{
			return _projectsText;
		}
		set
		{
			if (value != _projectsText)
			{
				_projectsText = value;
				OnPropertyChangedWithValue(value, "ProjectsText");
			}
		}
	}

	[DataSourceProperty]
	public string QueueText
	{
		get
		{
			return _queueText;
		}
		set
		{
			if (value != _queueText)
			{
				_queueText = value;
				OnPropertyChangedWithValue(value, "QueueText");
			}
		}
	}

	[DataSourceProperty]
	public string DailyDefaultsText
	{
		get
		{
			return _dailyDefaultsText;
		}
		set
		{
			if (value != _dailyDefaultsText)
			{
				_dailyDefaultsText = value;
				OnPropertyChangedWithValue(value, "DailyDefaultsText");
			}
		}
	}

	[DataSourceProperty]
	public string DailyDefaultsExplanationText
	{
		get
		{
			return _dailyDefaultsExplanationText;
		}
		set
		{
			if (value != _dailyDefaultsExplanationText)
			{
				_dailyDefaultsExplanationText = value;
				OnPropertyChangedWithValue(value, "DailyDefaultsExplanationText");
			}
		}
	}

	[DataSourceProperty]
	public SettlementProjectVM CurrentSelectedProject
	{
		get
		{
			return _currentSelectedProject;
		}
		set
		{
			if (value != _currentSelectedProject)
			{
				_currentSelectedProject = value;
				OnPropertyChangedWithValue(value, "CurrentSelectedProject");
			}
		}
	}

	[DataSourceProperty]
	public SettlementDailyProjectVM CurrentDailyDefault
	{
		get
		{
			return _currentDailyDefault;
		}
		set
		{
			if (value != _currentDailyDefault)
			{
				_currentDailyDefault = value;
				OnPropertyChangedWithValue(value, "CurrentDailyDefault");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<SettlementBuildingProjectVM> AvailableProjects
	{
		get
		{
			return _availableProjects;
		}
		set
		{
			if (value != _availableProjects)
			{
				_availableProjects = value;
				OnPropertyChangedWithValue(value, "AvailableProjects");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<SettlementBuildingProjectVM> CurrentDevelopmentQueue
	{
		get
		{
			return _currentDevelopmentQueue;
		}
		set
		{
			if (value != _currentDevelopmentQueue)
			{
				_currentDevelopmentQueue = value;
				OnPropertyChangedWithValue(value, "CurrentDevelopmentQueue");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<SettlementDailyProjectVM> DailyDefaultList
	{
		get
		{
			return _dailyDefaultList;
		}
		set
		{
			if (value != _dailyDefaultList)
			{
				_dailyDefaultList = value;
				OnPropertyChangedWithValue(value, "DailyDefaultList");
			}
		}
	}

	public SettlementProjectSelectionVM(Settlement settlement, Action onAnyChangeInQueue)
	{
		_settlement = settlement;
		_town = settlement.Town;
		_onAnyChangeInQueue = onAnyChangeInQueue;
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		ProjectsText = new TextObject("{=LpsoPtOo}Projects").ToString();
		DailyDefaultsText = GameTexts.FindText("str_town_management_daily_defaults").ToString();
		DailyDefaultsExplanationText = GameTexts.FindText("str_town_management_daily_defaults_explanation").ToString();
		QueueText = GameTexts.FindText("str_town_management_queue").ToString();
		Refresh();
	}

	public void Refresh()
	{
		AvailableProjects = new MBBindingList<SettlementBuildingProjectVM>();
		DailyDefaultList = new MBBindingList<SettlementDailyProjectVM>();
		LocalDevelopmentList = new List<Building>();
		CurrentDevelopmentQueue = new MBBindingList<SettlementBuildingProjectVM>();
		AvailableProjects.Clear();
		for (int i = 0; i < _town.Buildings.Count; i++)
		{
			Building building = _town.Buildings[i];
			if (building.BuildingType.BuildingLocation != BuildingLocation.Daily)
			{
				SettlementBuildingProjectVM settlementBuildingProjectVM = new SettlementBuildingProjectVM(OnCurrentProjectSelection, OnCurrentProjectSet, OnResetCurrentProject, building, _settlement);
				AvailableProjects.Add(settlementBuildingProjectVM);
				if (settlementBuildingProjectVM.Building == _town.CurrentBuilding)
				{
					CurrentSelectedProject = settlementBuildingProjectVM;
				}
				continue;
			}
			SettlementDailyProjectVM settlementDailyProjectVM = new SettlementDailyProjectVM(OnCurrentProjectSelection, OnCurrentProjectSet, OnResetCurrentProject, building, _settlement);
			DailyDefaultList.Add(settlementDailyProjectVM);
			if (settlementDailyProjectVM.Building == _town.CurrentDefaultBuilding)
			{
				CurrentDailyDefault = settlementDailyProjectVM;
				CurrentDailyDefault.IsDefault = true;
				settlementDailyProjectVM.IsDefault = true;
			}
		}
		foreach (Building item in _town.BuildingsInProgress)
		{
			LocalDevelopmentList.Add(item);
		}
		RefreshDevelopmentsQueueIndex();
	}

	private void OnCurrentProjectSet(SettlementProjectVM selectedItem)
	{
		if (selectedItem != CurrentSelectedProject)
		{
			CurrentSelectedProject = selectedItem;
			CurrentSelectedProject.RefreshProductionText();
		}
	}

	private void OnCurrentProjectSelection(SettlementProjectVM selectedItem, bool isSetAsActiveDevelopment)
	{
		if (!selectedItem.IsDaily)
		{
			if (isSetAsActiveDevelopment)
			{
				LocalDevelopmentList.Clear();
				LocalDevelopmentList.Add(selectedItem.Building);
			}
			else if (LocalDevelopmentList.Exists((Building d) => d == selectedItem.Building))
			{
				LocalDevelopmentList.Remove(selectedItem.Building);
			}
			else
			{
				LocalDevelopmentList.Add(selectedItem.Building);
			}
		}
		else
		{
			CurrentDailyDefault.IsDefault = false;
			CurrentDailyDefault = selectedItem as SettlementDailyProjectVM;
			(selectedItem as SettlementDailyProjectVM).IsDefault = true;
		}
		RefreshDevelopmentsQueueIndex();
		if (LocalDevelopmentList.Count == 0)
		{
			CurrentSelectedProject = CurrentDailyDefault;
		}
		else if (selectedItem != CurrentSelectedProject)
		{
			CurrentSelectedProject = selectedItem;
		}
		_onAnyChangeInQueue?.Invoke();
	}

	private void OnResetCurrentProject()
	{
		CurrentSelectedProject = ((LocalDevelopmentList.Count > 0) ? ((SettlementProjectVM)AvailableProjects.First((SettlementBuildingProjectVM p) => p.Building == LocalDevelopmentList[0])) : ((SettlementProjectVM)CurrentDailyDefault));
		CurrentSelectedProject.RefreshProductionText();
	}

	private void RefreshDevelopmentsQueueIndex()
	{
		CurrentSelectedProject = null;
		CurrentDevelopmentQueue = new MBBindingList<SettlementBuildingProjectVM>();
		foreach (SettlementBuildingProjectVM item in AvailableProjects)
		{
			item.DevelopmentQueueIndex = -1;
			item.IsInQueue = LocalDevelopmentList.Any((Building d) => d.BuildingType == item.Building.BuildingType);
			item.IsCurrentActiveProject = false;
			if (item.IsInQueue)
			{
				int num = LocalDevelopmentList.IndexOf(item.Building);
				item.DevelopmentQueueIndex = num;
				if (num == 0)
				{
					CurrentSelectedProject = item;
					item.IsCurrentActiveProject = true;
				}
				CurrentDevelopmentQueue.Add(item);
			}
			Comparer<SettlementBuildingProjectVM> comparer = Comparer<SettlementBuildingProjectVM>.Create((SettlementBuildingProjectVM s1, SettlementBuildingProjectVM s2) => s1.DevelopmentQueueIndex.CompareTo(s2.DevelopmentQueueIndex));
			CurrentDevelopmentQueue.Sort(comparer);
			item.RefreshProductionText();
		}
	}
}
