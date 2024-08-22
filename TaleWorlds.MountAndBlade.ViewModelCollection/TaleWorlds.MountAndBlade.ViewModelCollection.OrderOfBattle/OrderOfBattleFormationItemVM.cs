using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

public class OrderOfBattleFormationItemVM : ViewModel
{
	private const int MaxShownHeroTroopCount = 8;

	private readonly Camera _missionCamera;

	private BannerBearerLogic _bannerBearerLogic;

	public static Action OnHeroesChanged;

	public static Action<OrderOfBattleFormationItemVM> OnFilterSelectionToggled;

	public static Action<OrderOfBattleFormationItemVM> OnClassSelectionToggled;

	public static Action<OrderOfBattleFormationItemVM> OnFilterUseToggled;

	public static Action<OrderOfBattleFormationItemVM> OnSelection;

	public static Action<OrderOfBattleFormationItemVM> OnDeselection;

	public static Func<DeploymentFormationClass, FormationFilterType, int> GetTotalTroopCountWithFilter;

	public static Func<Func<OrderOfBattleFormationItemVM, bool>, IEnumerable<OrderOfBattleFormationItemVM>> GetFormationWithCondition;

	public static Func<FormationClass, bool> HasAnyTroopWithClass;

	public static Action<OrderOfBattleFormationItemVM> OnAcceptCommander;

	public static Action<OrderOfBattleFormationItemVM> OnAcceptHeroTroops;

	private OrderOfBattleHeroItemVM _unassignedCommander;

	private readonly TextObject _formationTooltipTitleText = new TextObject("{=cZNA5Z6l}Formation {NUMBER}");

	private readonly TextObject _filteredTroopCountInfoText = new TextObject("{=yRIPADWl}{TROOP_COUNT}/{TOTAL_TROOP_COUNT}");

	private readonly TextObject _cantAdjustNotCommanderText = new TextObject("{=ZixS1b4u}You're not leading this battle.");

	private readonly TextObject _cantAdjustSingledOutText = new TextObject("{=7jhe9cT9}You need to have at least one more formation of this type to change this formation's type.");

	private readonly TextObject _commanderSlotHintText = new TextObject("{=RvKwdXWs}Commander");

	private readonly TextObject _heroTroopSlotHintText = new TextObject("{=VyMD4iRV}Hero Troops");

	private readonly TextObject _assignCommanderHintText = new TextObject("{=MssTzJJb}Assign as Commander");

	private readonly TextObject _assignHeroTroopHintText = new TextObject("{=ngyMTaqr}Assign as Hero Troop");

	private Vec3 _worldPosition;

	private float _latestX;

	private float _latestY;

	private float _latestW;

	private float _wPosAfterPositionCalculation;

	private bool _isSelected;

	private bool _hasFormation;

	private bool _hasCommander;

	private bool _isControlledByPlayer;

	private bool _hasHeroTroops;

	private bool _isSelectable;

	private bool _isFiltered;

	private bool _isAdjustable;

	private bool _isMarkerShown;

	private bool _isBeingFocused;

	private bool _isAcceptingCommander;

	private bool _isAcceptingHeroTroops;

	private bool _isHeroTroopsOverflowing;

	private bool _isFilterSelectionActive;

	private bool _isClassSelectionActive;

	private string _titleText;

	private string _formationIsEmptyText;

	private string _overflowHeroTroopCountText;

	private int _orderOfBattleFormationClassInt;

	private int _troopCount;

	private int _wSign;

	private Vec2 _screenPosition;

	private OrderOfBattleHeroItemVM _commander;

	private MBBindingList<OrderOfBattleHeroItemVM> _heroTroops;

	private MBBindingList<OrderOfBattleFormationClassVM> _classes;

	private SelectorVM<OrderOfBattleFormationClassSelectorItemVM> _formationClassSelector;

	private MBBindingList<OrderOfBattleFormationFilterSelectorItemVM> _filterItems;

	private MBBindingList<OrderOfBattleFormationFilterSelectorItemVM> _activeFilterItems;

	private BasicTooltipViewModel _tooltip;

	private HintViewModel _cantAdjustHint;

	private HintViewModel _commanderSlotHint;

	private HintViewModel _heroTroopSlotHint;

	private HintViewModel _assignCommanderHint;

	private HintViewModel _assignHeroTroopHint;

	private bool _isCaptainSlotHighlightActive;

	private bool _isTypeSelectionHighlightActive;

	public Formation Formation { get; private set; }

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool HasFormation
	{
		get
		{
			return _hasFormation;
		}
		set
		{
			if (value != _hasFormation)
			{
				_hasFormation = value;
				OnPropertyChangedWithValue(value, "HasFormation");
			}
		}
	}

	[DataSourceProperty]
	public bool HasCommander
	{
		get
		{
			return _hasCommander;
		}
		set
		{
			if (value != _hasCommander)
			{
				_hasCommander = value;
				OnPropertyChangedWithValue(value, "HasCommander");
			}
		}
	}

	[DataSourceProperty]
	public bool HasHeroTroops
	{
		get
		{
			return _hasHeroTroops;
		}
		set
		{
			if (value != _hasHeroTroops)
			{
				_hasHeroTroops = value;
				OnPropertyChangedWithValue(value, "HasHeroTroops");
			}
		}
	}

	[DataSourceProperty]
	public bool IsControlledByPlayer
	{
		get
		{
			return _isControlledByPlayer;
		}
		set
		{
			if (value != _isControlledByPlayer)
			{
				_isControlledByPlayer = value;
				OnPropertyChangedWithValue(value, "IsControlledByPlayer");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelectable
	{
		get
		{
			return _isSelectable;
		}
		set
		{
			if (value != _isSelectable)
			{
				_isSelectable = value;
				OnPropertyChangedWithValue(value, "IsSelectable");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFiltered
	{
		get
		{
			return _isFiltered;
		}
		set
		{
			if (value != _isFiltered)
			{
				_isFiltered = value;
				OnPropertyChangedWithValue(value, "IsFiltered");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAdjustable
	{
		get
		{
			return _isAdjustable;
		}
		set
		{
			if (value != _isAdjustable)
			{
				_isAdjustable = value;
				OnPropertyChangedWithValue(value, "IsAdjustable");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMarkerShown
	{
		get
		{
			return _isMarkerShown;
		}
		set
		{
			if (value != _isMarkerShown)
			{
				_isMarkerShown = value;
				OnPropertyChangedWithValue(value, "IsMarkerShown");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBeingFocused
	{
		get
		{
			return _isBeingFocused;
		}
		set
		{
			if (value != _isBeingFocused)
			{
				_isBeingFocused = value;
				OnPropertyChangedWithValue(value, "IsBeingFocused");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAcceptingCommander
	{
		get
		{
			return _isAcceptingCommander;
		}
		set
		{
			if (value != _isAcceptingCommander)
			{
				_isAcceptingCommander = value;
				OnPropertyChangedWithValue(value, "IsAcceptingCommander");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAcceptingHeroTroops
	{
		get
		{
			return _isAcceptingHeroTroops;
		}
		set
		{
			if (value != _isAcceptingHeroTroops)
			{
				_isAcceptingHeroTroops = value;
				OnPropertyChangedWithValue(value, "IsAcceptingHeroTroops");
			}
		}
	}

	[DataSourceProperty]
	public bool IsHeroTroopsOverflowing
	{
		get
		{
			return _isHeroTroopsOverflowing;
		}
		set
		{
			if (value != _isHeroTroopsOverflowing)
			{
				_isHeroTroopsOverflowing = value;
				OnPropertyChangedWithValue(value, "IsHeroTroopsOverflowing");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFilterSelectionActive
	{
		get
		{
			return _isFilterSelectionActive;
		}
		set
		{
			if (value != _isFilterSelectionActive)
			{
				_isFilterSelectionActive = value;
				OnPropertyChangedWithValue(value, "IsFilterSelectionActive");
				OnFilterSelectionToggled?.Invoke(this);
			}
		}
	}

	[DataSourceProperty]
	public bool IsClassSelectionActive
	{
		get
		{
			return _isClassSelectionActive;
		}
		set
		{
			if (value != _isClassSelectionActive)
			{
				_isClassSelectionActive = value;
				OnPropertyChangedWithValue(value, "IsClassSelectionActive");
				OnClassSelectionToggled?.Invoke(this);
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				OnPropertyChangedWithValue(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string FormationIsEmptyText
	{
		get
		{
			return _formationIsEmptyText;
		}
		set
		{
			if (value != _formationIsEmptyText)
			{
				_formationIsEmptyText = value;
				OnPropertyChangedWithValue(value, "FormationIsEmptyText");
			}
		}
	}

	[DataSourceProperty]
	public string OverflowHeroTroopCountText
	{
		get
		{
			return _overflowHeroTroopCountText;
		}
		set
		{
			if (value != _overflowHeroTroopCountText)
			{
				_overflowHeroTroopCountText = value;
				OnPropertyChangedWithValue(value, "OverflowHeroTroopCountText");
			}
		}
	}

	[DataSourceProperty]
	public int TroopCount
	{
		get
		{
			return _troopCount;
		}
		set
		{
			if (value != _troopCount)
			{
				_troopCount = value;
				OnPropertyChangedWithValue(value, "TroopCount");
			}
		}
	}

	[DataSourceProperty]
	public int OrderOfBattleFormationClassInt
	{
		get
		{
			return _orderOfBattleFormationClassInt;
		}
		set
		{
			if (value != _orderOfBattleFormationClassInt)
			{
				_orderOfBattleFormationClassInt = value;
				OnPropertyChangedWithValue(value, "OrderOfBattleFormationClassInt");
			}
		}
	}

	[DataSourceProperty]
	public int WSign
	{
		get
		{
			return _wSign;
		}
		set
		{
			if (value != _wSign)
			{
				_wSign = value;
				OnPropertyChangedWithValue(value, "WSign");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 ScreenPosition
	{
		get
		{
			return _screenPosition;
		}
		set
		{
			if (value.x != _screenPosition.x || value.y != _screenPosition.y)
			{
				_screenPosition = value;
				OnPropertyChangedWithValue(value, "ScreenPosition");
			}
		}
	}

	[DataSourceProperty]
	public OrderOfBattleHeroItemVM Commander
	{
		get
		{
			return _commander;
		}
		set
		{
			if (value != _commander)
			{
				_commander = value;
				OnPropertyChangedWithValue(value, "Commander");
				HandleCommanderAssignment(value);
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<OrderOfBattleHeroItemVM> HeroTroops
	{
		get
		{
			return _heroTroops;
		}
		set
		{
			if (value != _heroTroops)
			{
				_heroTroops = value;
				OnPropertyChangedWithValue(value, "HeroTroops");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<OrderOfBattleFormationClassVM> Classes
	{
		get
		{
			return _classes;
		}
		set
		{
			if (value != _classes)
			{
				_classes = value;
				OnPropertyChangedWithValue(value, "Classes");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<OrderOfBattleFormationClassSelectorItemVM> FormationClassSelector
	{
		get
		{
			return _formationClassSelector;
		}
		set
		{
			if (value != _formationClassSelector)
			{
				_formationClassSelector = value;
				OnPropertyChangedWithValue(value, "FormationClassSelector");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<OrderOfBattleFormationFilterSelectorItemVM> FilterItems
	{
		get
		{
			return _filterItems;
		}
		set
		{
			if (value != _filterItems)
			{
				_filterItems = value;
				OnPropertyChangedWithValue(value, "FilterItems");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<OrderOfBattleFormationFilterSelectorItemVM> ActiveFilterItems
	{
		get
		{
			return _activeFilterItems;
		}
		set
		{
			if (value != _activeFilterItems)
			{
				_activeFilterItems = value;
				OnPropertyChangedWithValue(value, "ActiveFilterItems");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel Tooltip
	{
		get
		{
			return _tooltip;
		}
		set
		{
			if (value != _tooltip)
			{
				_tooltip = value;
				OnPropertyChangedWithValue(value, "Tooltip");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CantAdjustHint
	{
		get
		{
			return _cantAdjustHint;
		}
		set
		{
			if (value != _cantAdjustHint)
			{
				_cantAdjustHint = value;
				OnPropertyChangedWithValue(value, "CantAdjustHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CommanderSlotHint
	{
		get
		{
			return _commanderSlotHint;
		}
		set
		{
			if (value != _commanderSlotHint)
			{
				_commanderSlotHint = value;
				OnPropertyChangedWithValue(value, "CommanderSlotHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel HeroTroopSlotHint
	{
		get
		{
			return _heroTroopSlotHint;
		}
		set
		{
			if (value != _heroTroopSlotHint)
			{
				_heroTroopSlotHint = value;
				OnPropertyChangedWithValue(value, "HeroTroopSlotHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AssignCommanderHint
	{
		get
		{
			return _assignCommanderHint;
		}
		set
		{
			if (value != _assignCommanderHint)
			{
				_assignCommanderHint = value;
				OnPropertyChangedWithValue(value, "AssignCommanderHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AssignHeroTroopHint
	{
		get
		{
			return _assignHeroTroopHint;
		}
		set
		{
			if (value != _assignHeroTroopHint)
			{
				_assignHeroTroopHint = value;
				OnPropertyChangedWithValue(value, "AssignHeroTroopHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCaptainSlotHighlightActive
	{
		get
		{
			return _isCaptainSlotHighlightActive;
		}
		set
		{
			if (value != _isCaptainSlotHighlightActive)
			{
				_isCaptainSlotHighlightActive = value;
				OnPropertyChangedWithValue(value, "IsCaptainSlotHighlightActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTypeSelectionHighlightActive
	{
		get
		{
			return _isTypeSelectionHighlightActive;
		}
		set
		{
			if (value != _isTypeSelectionHighlightActive)
			{
				_isTypeSelectionHighlightActive = value;
				OnPropertyChangedWithValue(value, "IsTypeSelectionHighlightActive");
			}
		}
	}

	public OrderOfBattleFormationItemVM(Camera missionCamera)
	{
		_missionCamera = missionCamera;
		Formation = null;
		_bannerBearerLogic = Mission.Current.GetMissionBehavior<BannerBearerLogic>();
		HasFormation = false;
		FilterItems = new MBBindingList<OrderOfBattleFormationFilterSelectorItemVM>();
		ActiveFilterItems = new MBBindingList<OrderOfBattleFormationFilterSelectorItemVM>();
		for (FormationFilterType formationFilterType = FormationFilterType.Shield; formationFilterType < FormationFilterType.NumberOfFilterTypes; formationFilterType++)
		{
			FilterItems.Add(new OrderOfBattleFormationFilterSelectorItemVM(formationFilterType, OnFilterToggled));
		}
		FormationClassSelector = new SelectorVM<OrderOfBattleFormationClassSelectorItemVM>(0, OnClassChanged);
		for (DeploymentFormationClass deploymentFormationClass = DeploymentFormationClass.Unset; deploymentFormationClass <= DeploymentFormationClass.CavalryAndHorseArcher; deploymentFormationClass++)
		{
			if (!Mission.Current.IsSiegeBattle || (deploymentFormationClass != DeploymentFormationClass.Cavalry && deploymentFormationClass != DeploymentFormationClass.HorseArcher && deploymentFormationClass != DeploymentFormationClass.CavalryAndHorseArcher))
			{
				FormationClassSelector.AddItem(new OrderOfBattleFormationClassSelectorItemVM(deploymentFormationClass));
			}
		}
		Classes = new MBBindingList<OrderOfBattleFormationClassVM>
		{
			new OrderOfBattleFormationClassVM(this),
			new OrderOfBattleFormationClassVM(this)
		};
		HeroTroops = new MBBindingList<OrderOfBattleHeroItemVM>();
		_unassignedCommander = new OrderOfBattleHeroItemVM();
		Commander = _unassignedCommander;
		Tooltip = new BasicTooltipViewModel(() => GetTooltip());
		IsControlledByPlayer = Mission.Current.PlayerTeam.IsPlayerGeneral;
		_worldPosition = Vec3.Zero;
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		FormationIsEmptyText = new TextObject("{=P3IWytsr}Formation is currently empty").ToString();
		CommanderSlotHint = new HintViewModel(_commanderSlotHintText);
		HeroTroopSlotHint = new HintViewModel(_heroTroopSlotHintText);
		AssignCommanderHint = new HintViewModel(_assignCommanderHintText);
		AssignHeroTroopHint = new HintViewModel(_assignHeroTroopHintText);
	}

	public void Tick()
	{
		Classes.ApplyActionOnAllItems(delegate(OrderOfBattleFormationClassVM c)
		{
			c.UpdateWeightAdjustable();
		});
		UpdateAdjustable();
		IsMarkerShown = Formation.CountOfUnits != 0 && Classes.Any((OrderOfBattleFormationClassVM c) => c.Class != FormationClass.NumberOfAllFormations);
		if (IsMarkerShown)
		{
			_latestX = 0f;
			_latestY = 0f;
			_latestW = 0f;
			MBWindowManager.WorldToScreenInsideUsableArea(_missionCamera, _worldPosition, ref _latestX, ref _latestY, ref _latestW);
			ScreenPosition = new Vec2(_latestX, _latestY);
			_wPosAfterPositionCalculation = ((_latestW < 0f) ? (-1f) : 1.1f);
			WSign = (int)_wPosAfterPositionCalculation;
		}
	}

	public void RefreshFormation(Formation formation, DeploymentFormationClass overriddenClass = DeploymentFormationClass.Unset, bool mustExist = false)
	{
		Formation = formation;
		if (formation.CountOfUnits != 0 || mustExist)
		{
			DeploymentFormationClass formationTypeToSet = DeploymentFormationClass.Unset;
			if (overriddenClass != 0)
			{
				formationTypeToSet = overriddenClass;
			}
			else
			{
				FormationClass formationClass = FormationClass.NumberOfAllFormations;
				if (formation.SecondaryLogicalClasses.Count() > 0)
				{
					formationClass = formation.SecondaryLogicalClasses.FirstOrDefault();
					if (formation.GetCountOfUnitsBelongingToLogicalClass(formationClass) == 0)
					{
						formationClass = FormationClass.NumberOfAllFormations;
					}
				}
				switch (formation.LogicalClass)
				{
				case FormationClass.Infantry:
					formationTypeToSet = ((formationClass != FormationClass.Ranged) ? DeploymentFormationClass.Infantry : DeploymentFormationClass.InfantryAndRanged);
					break;
				case FormationClass.Ranged:
					formationTypeToSet = ((formationClass == FormationClass.Infantry) ? DeploymentFormationClass.InfantryAndRanged : DeploymentFormationClass.Ranged);
					break;
				case FormationClass.Cavalry:
					formationTypeToSet = ((formationClass == FormationClass.HorseArcher) ? DeploymentFormationClass.CavalryAndHorseArcher : DeploymentFormationClass.Cavalry);
					break;
				case FormationClass.HorseArcher:
					formationTypeToSet = ((formationClass == FormationClass.Cavalry) ? DeploymentFormationClass.CavalryAndHorseArcher : DeploymentFormationClass.HorseArcher);
					break;
				default:
					Debug.FailedAssert("Formation doesn't have a proper primary class. Value : " + formation.PhysicalClass.GetName(), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\OrderOfBattle\\OrderOfBattleFormationItemVM.cs", "RefreshFormation", 175);
					break;
				}
			}
			OrderOfBattleFormationClassSelectorItemVM item = FormationClassSelector.ItemList.SingleOrDefault((OrderOfBattleFormationClassSelectorItemVM i) => i.FormationClass == formationTypeToSet);
			int num = FormationClassSelector.ItemList.IndexOf(item);
			if (num != -1)
			{
				FormationClassSelector.SelectedIndex = num;
			}
		}
		else
		{
			FormationClassSelector.SelectedIndex = 0;
		}
		TitleText = Common.ToRoman(Formation.Index + 1);
		OnSizeChanged();
	}

	public void RefreshMarkerWorldPosition()
	{
		if (Formation != null)
		{
			Agent medianAgent = Formation.GetMedianAgent(excludeDetachedUnits: false, excludePlayer: false, Formation.GetAveragePositionOfUnits(excludeDetachedUnits: false, excludePlayer: false));
			if (medianAgent != null)
			{
				_worldPosition = medianAgent.GetWorldPosition().GetGroundVec3();
				_worldPosition += new Vec3(0f, 0f, medianAgent.GetEyeGlobalHeight());
			}
		}
	}

	public void OnSizeChanged()
	{
		TroopCount = Formation?.CountOfUnits ?? 0;
		RefreshMarkerWorldPosition();
		IsSelectable = FormationClassSelector.SelectedIndex != 0 && IsControlledByPlayer && TroopCount > 0;
		if (!IsSelectable && IsSelected)
		{
			OnDeselection?.Invoke(this);
		}
		foreach (OrderOfBattleFormationClassVM @class in Classes)
		{
			@class.UpdateWeightText();
		}
		UpdateAdjustable();
	}

	private void OnClassChanged(SelectorVM<OrderOfBattleFormationClassSelectorItemVM> formationClassSelector)
	{
		if (Classes == null)
		{
			return;
		}
		switch ((DeploymentFormationClass)(OrderOfBattleFormationClassInt = (int)formationClassSelector.SelectedItem.FormationClass))
		{
		case DeploymentFormationClass.Unset:
		{
			Classes[0].Class = FormationClass.NumberOfAllFormations;
			Classes[1].Class = FormationClass.NumberOfAllFormations;
			if (Commander != _unassignedCommander)
			{
				UnassignCommander();
			}
			List<OrderOfBattleHeroItemVM> list = HeroTroops.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				RemoveHeroTroop(list[i]);
			}
			for (int num = ActiveFilterItems.Count - 1; num >= 0; num--)
			{
				ActiveFilterItems[num].IsActive = false;
			}
			break;
		}
		case DeploymentFormationClass.Infantry:
			Classes[0].Class = FormationClass.Infantry;
			Classes[1].Class = FormationClass.NumberOfAllFormations;
			break;
		case DeploymentFormationClass.Ranged:
			Classes[0].Class = FormationClass.Ranged;
			Classes[1].Class = FormationClass.NumberOfAllFormations;
			break;
		case DeploymentFormationClass.Cavalry:
			Classes[0].Class = FormationClass.Cavalry;
			Classes[1].Class = FormationClass.NumberOfAllFormations;
			break;
		case DeploymentFormationClass.HorseArcher:
			Classes[0].Class = FormationClass.HorseArcher;
			Classes[1].Class = FormationClass.NumberOfAllFormations;
			break;
		case DeploymentFormationClass.InfantryAndRanged:
			Classes[0].Class = FormationClass.Infantry;
			Classes[1].Class = FormationClass.Ranged;
			break;
		case DeploymentFormationClass.CavalryAndHorseArcher:
			Classes[0].Class = FormationClass.Cavalry;
			Classes[1].Class = FormationClass.HorseArcher;
			break;
		}
		foreach (OrderOfBattleFormationClassVM @class in Classes)
		{
			@class.IsLocked = false;
			@class.Weight = 0;
		}
		HasFormation = Classes.Any((OrderOfBattleFormationClassVM c) => c.Class != FormationClass.NumberOfAllFormations);
		UpdateAdjustable();
	}

	public DeploymentFormationClass GetOrderOfBattleClass()
	{
		if (Classes[0].Class == FormationClass.Infantry && Classes[1].Class == FormationClass.NumberOfAllFormations)
		{
			return DeploymentFormationClass.Infantry;
		}
		if (Classes[0].Class == FormationClass.Ranged && Classes[1].Class == FormationClass.NumberOfAllFormations)
		{
			return DeploymentFormationClass.Ranged;
		}
		if (Classes[0].Class == FormationClass.Cavalry && Classes[1].Class == FormationClass.NumberOfAllFormations)
		{
			return DeploymentFormationClass.Cavalry;
		}
		if (Classes[0].Class == FormationClass.HorseArcher && Classes[1].Class == FormationClass.NumberOfAllFormations)
		{
			return DeploymentFormationClass.HorseArcher;
		}
		if (Classes[0].Class == FormationClass.Infantry && Classes[1].Class == FormationClass.Ranged)
		{
			return DeploymentFormationClass.InfantryAndRanged;
		}
		if (Classes[0].Class == FormationClass.Cavalry && Classes[1].Class == FormationClass.HorseArcher)
		{
			return DeploymentFormationClass.CavalryAndHorseArcher;
		}
		return DeploymentFormationClass.Unset;
	}

	private void OnFilterToggled(OrderOfBattleFormationFilterSelectorItemVM filterItem)
	{
		if (filterItem.IsActive)
		{
			ActiveFilterItems.Add(filterItem);
		}
		else
		{
			ActiveFilterItems.Remove(filterItem);
		}
		IsFiltered = ActiveFilterItems.Count > 0;
		OnFilterUseToggled?.Invoke(this);
		ActiveFilterItems.Sort(new OrderOfBattleFormationFilterSelectorItemComparer());
	}

	private bool HasAnyActiveFilter()
	{
		if (!HasFilter(FormationFilterType.Shield) && !HasFilter(FormationFilterType.Spear) && !HasFilter(FormationFilterType.Thrown) && !HasFilter(FormationFilterType.Heavy) && !HasFilter(FormationFilterType.HighTier))
		{
			return HasFilter(FormationFilterType.LowTier);
		}
		return true;
	}

	public void UpdateAdjustable()
	{
		IsAdjustable = IsControlledByPlayer && Classes.All((OrderOfBattleFormationClassVM c) => c.Class == FormationClass.NumberOfAllFormations || c.IsAdjustable || !HasAnyTroopWithClass(c.Class));
		if (!IsControlledByPlayer)
		{
			CantAdjustHint = new HintViewModel(_cantAdjustNotCommanderText);
		}
		else if (!Classes.All((OrderOfBattleFormationClassVM c) => c.Class == FormationClass.NumberOfAllFormations || c.IsAdjustable))
		{
			CantAdjustHint = new HintViewModel(_cantAdjustSingledOutText);
		}
	}

	public bool HasFilter(FormationFilterType filter)
	{
		return FilterItems.Any((OrderOfBattleFormationFilterSelectorItemVM f) => f.IsActive && f.FilterType == filter);
	}

	public bool HasOnlyOneClass()
	{
		int num = 0;
		for (int i = 0; i < Classes.Count; i++)
		{
			if (!Classes[i].IsUnset)
			{
				num++;
			}
		}
		return num == 1;
	}

	public bool OnlyHasClass(FormationClass formationClass)
	{
		bool flag = false;
		for (int i = 0; i < Classes.Count; i++)
		{
			if (!flag && Classes[i].Class == formationClass && !Classes[i].IsUnset)
			{
				flag = true;
			}
			if (flag && Classes[i].Class != FormationClass.NumberOfAllFormations)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasClass(FormationClass formationClass)
	{
		for (int i = 0; i < Classes.Count; i++)
		{
			if (Classes[i].Class == formationClass && !Classes[i].IsUnset)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasClasses(FormationClass[] formationClasses)
	{
		FormationClass[] source = (from c in Classes
			select c.Class into c
			where c != FormationClass.NumberOfAllFormations
			select c).ToArray();
		return formationClasses.OrderBy((FormationClass c) => c).SequenceEqual(source.OrderBy((FormationClass c) => c));
	}

	private List<TooltipProperty> GetTooltip()
	{
		GameTexts.SetVariable("NUMBER", TitleText);
		List<TooltipProperty> list = new List<TooltipProperty>
		{
			new TooltipProperty(_formationTooltipTitleText.ToString(), string.Empty, 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.Title),
			new TooltipProperty(string.Empty, string.Empty, 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.DefaultSeperator)
		};
		if (FormationClassSelector.SelectedItem == null)
		{
			return list;
		}
		List<Agent> list2 = new List<Agent>();
		int[] array = new int[4];
		foreach (IFormationUnit allUnit in Formation.Arrangement.GetAllUnits())
		{
			if (!(allUnit is Agent agent2))
			{
				continue;
			}
			if (agent2.IsHero)
			{
				list2.Add(agent2);
			}
			else if (agent2.Banner == null)
			{
				FormationClass formationClass = agent2.Character.GetFormationClass();
				if (formationClass >= FormationClass.Infantry && formationClass < FormationClass.NumberOfDefaultFormations)
				{
					array[(int)formationClass]++;
				}
			}
		}
		foreach (Agent detachedUnit in Formation.DetachedUnits)
		{
			if (detachedUnit.IsHero)
			{
				list2.Add(detachedUnit);
			}
			else if (detachedUnit.Banner == null)
			{
				FormationClass formationClass2 = detachedUnit.Character.GetFormationClass();
				if (formationClass2 >= FormationClass.Infantry && formationClass2 < FormationClass.NumberOfDefaultFormations)
				{
					array[(int)formationClass2]++;
				}
			}
		}
		for (FormationClass formationClass3 = FormationClass.Infantry; formationClass3 < FormationClass.NumberOfDefaultFormations; formationClass3++)
		{
			int num = array[(int)formationClass3];
			List<Agent> list3 = new List<Agent>();
			for (int i = 0; i < list2.Count; i++)
			{
				Agent agent3 = list2[i];
				if ((formationClass3 == FormationClass.Infantry && QueryLibrary.IsInfantry(agent3)) || (formationClass3 == FormationClass.Ranged && QueryLibrary.IsRanged(agent3)) || (formationClass3 == FormationClass.Cavalry && QueryLibrary.IsCavalry(agent3)) || (formationClass3 == FormationClass.HorseArcher && QueryLibrary.IsRangedCavalry(agent3)))
				{
					list3.Add(agent3);
				}
			}
			if (num > 0 || list3.Count > 0)
			{
				int num2 = (int)formationClass3;
				list.Add(new TooltipProperty(GameTexts.FindText("str_troop_group_name", num2.ToString()).ToString(), num.ToString(), 0));
				for (int j = 0; j < list3.Count; j++)
				{
					list.Add(new TooltipProperty(list3[j].Name, " ", 0));
				}
				list.Add(new TooltipProperty(string.Empty, string.Empty, -1));
			}
		}
		List<Agent> formationBannerBearers = _bannerBearerLogic.GetFormationBannerBearers(Formation);
		if (formationBannerBearers.Count > 0)
		{
			list.Add(new TooltipProperty(new TextObject("{=scnSXrYC}Banner Bearers").ToString(), formationBannerBearers.Count.ToString(), 0));
		}
		if (HasAnyActiveFilter())
		{
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.DefaultSeperator));
		}
		DeploymentFormationClass formationClass4 = FormationClassSelector.SelectedItem.FormationClass;
		if (HasFilter(FormationFilterType.Shield))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Agent agent) => agent.HasShieldCached));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(formationClass4, FormationFilterType.Shield));
			list.Add(new TooltipProperty(FormationFilterType.Shield.GetFilterName().ToString(), _filteredTroopCountInfoText.ToString(), 0));
		}
		if (HasFilter(FormationFilterType.Spear))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Agent agent) => agent.HasSpearCached));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(formationClass4, FormationFilterType.Spear));
			list.Add(new TooltipProperty(FormationFilterType.Spear.GetFilterName().ToString(), _filteredTroopCountInfoText.ToString(), 0));
		}
		if (HasFilter(FormationFilterType.Thrown))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Agent agent) => agent.HasThrownCached));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(formationClass4, FormationFilterType.Thrown));
			list.Add(new TooltipProperty(FormationFilterType.Thrown.GetFilterName().ToString(), _filteredTroopCountInfoText.ToString(), 0));
		}
		if (HasFilter(FormationFilterType.Heavy))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Agent agent) => MissionGameModels.Current.AgentStatCalculateModel.HasHeavyArmor(agent)));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(formationClass4, FormationFilterType.Heavy));
			list.Add(new TooltipProperty(FormationFilterType.Heavy.GetFilterName().ToString(), _filteredTroopCountInfoText.ToString(), 0));
		}
		if (HasFilter(FormationFilterType.HighTier))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Agent agent) => agent.Character.GetBattleTier() >= 4));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(formationClass4, FormationFilterType.HighTier));
			list.Add(new TooltipProperty(FormationFilterType.HighTier.GetFilterName().ToString(), _filteredTroopCountInfoText.ToString(), 0));
		}
		if (HasFilter(FormationFilterType.LowTier))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Agent agent) => agent.Character.GetBattleTier() <= 3));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(formationClass4, FormationFilterType.LowTier));
			list.Add(new TooltipProperty(FormationFilterType.LowTier.GetFilterName().ToString(), _filteredTroopCountInfoText.ToString(), 0));
		}
		return list;
	}

	public void UnassignCommander()
	{
		if (Commander != _unassignedCommander)
		{
			Commander.CurrentAssignedFormationItem = null;
			Commander = _unassignedCommander;
		}
	}

	private void ExecuteSelection()
	{
		OnSelection?.Invoke(this);
	}

	private void HandleCommanderAssignment(OrderOfBattleHeroItemVM newCommander)
	{
		HasCommander = newCommander != _unassignedCommander;
		if (HasCommander)
		{
			Agent agent = newCommander.Agent;
			agent.Formation = Formation;
			Formation.Captain = agent;
			newCommander.CurrentAssignedFormationItem = this;
			_bannerBearerLogic?.SetFormationBanner(Formation, newCommander.BannerOfHero);
			if (agent.IsAIControlled)
			{
				agent.Team.DetachmentManager.RemoveScoresOfAgentFromDetachments(agent);
			}
		}
		else if (Formation != null)
		{
			Formation.Captain = null;
			_bannerBearerLogic?.SetFormationBanner(Formation, null);
		}
		RefreshFormation();
		OnSizeChanged();
		newCommander.RefreshInformation();
	}

	public void ExecuteAcceptCommander()
	{
		OnAcceptCommander?.Invoke(this);
	}

	public void ExecuteAcceptHeroTroops()
	{
		OnAcceptHeroTroops?.Invoke(this);
	}

	public void OnHeroSelectionUpdated(int selectedHeroCount, bool hasOwnHeroTroopInSelection)
	{
		if (IsControlledByPlayer)
		{
			IsAcceptingCommander = selectedHeroCount == 1 && HasFormation;
			if (!hasOwnHeroTroopInSelection)
			{
				IsAcceptingHeroTroops = selectedHeroCount >= 1 && HasFormation;
			}
		}
		else
		{
			IsAcceptingCommander = selectedHeroCount == 1 && HasFormation && (Commander == _unassignedCommander || !Commander.IsAssignedBeforePlayer);
		}
	}

	public void AddHeroTroop(OrderOfBattleHeroItemVM heroItem)
	{
		if (!HeroTroops.Contains(heroItem))
		{
			heroItem.CurrentAssignedFormationItem = this;
			heroItem.Agent.Formation = Formation;
			HeroTroops.Add(heroItem);
			RefreshFormation();
			OnSizeChanged();
		}
	}

	public void RemoveHeroTroop(OrderOfBattleHeroItemVM heroItem)
	{
		if (HeroTroops.Contains(heroItem))
		{
			heroItem.CurrentAssignedFormationItem = null;
			heroItem.Agent.Formation = heroItem.InitialFormation;
			HeroTroops.Remove(heroItem);
			RefreshFormation();
			OnSizeChanged();
		}
	}

	private void RefreshFormation()
	{
		HasHeroTroops = HeroTroops.Count > 0;
		IsHeroTroopsOverflowing = HeroTroops.Count > 8;
		OverflowHeroTroopCountText = (HeroTroops.Count - 8 + 1).ToString("+#;-#;0");
		Formation.Refresh();
		OnHeroesChanged?.Invoke();
	}
}
