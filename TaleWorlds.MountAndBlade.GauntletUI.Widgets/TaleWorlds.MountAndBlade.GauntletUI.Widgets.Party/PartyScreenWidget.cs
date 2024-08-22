using System.Collections.Generic;
using System.Linq;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.GauntletUI.GamepadNavigation;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Party;

public class PartyScreenWidget : Widget
{
	private PartyTroopTupleButtonWidget _currentMainTuple;

	private PartyTroopTupleButtonWidget _currentOtherTuple;

	private InputKeyVisualWidget _takeAllPrisonersInputKeyVisual;

	private InputKeyVisualWidget _dismissAllPrisonersInputKeyVisual;

	private PartyTroopTupleButtonWidget _newAddedTroop;

	private Widget _upgradePopupParent;

	private Widget _recruitPopupParent;

	private Widget _takeAllPrisonersInputKeyVisualParent;

	private Widget _dismissAllPrisonersInputKeyVisualParent;

	private int _mainPartyTroopSize;

	private bool _isPrisonerWarningEnabled;

	private bool _isTroopWarningEnabled;

	private bool _isOtherTroopWarningEnabled;

	private TextWidget _troopLabel;

	private TextWidget _prisonerLabel;

	private TextWidget _otherTroopLabel;

	private ListPanel _otherMemberList;

	private ListPanel _otherPrisonerList;

	private ListPanel _mainMemberList;

	private ListPanel _mainPrisonerList;

	public PartyTroopTupleButtonWidget CurrentMainTuple => _currentMainTuple;

	public PartyTroopTupleButtonWidget CurrentOtherTuple => _currentOtherTuple;

	public ScrollablePanel MainScrollPanel { get; set; }

	public ScrollablePanel OtherScrollPanel { get; set; }

	public InputKeyVisualWidget TransferInputKeyVisual { get; set; }

	public Widget UpgradePopupParent
	{
		get
		{
			return _upgradePopupParent;
		}
		set
		{
			if (value != _upgradePopupParent)
			{
				_upgradePopupParent = value;
			}
		}
	}

	public Widget RecruitPopupParent
	{
		get
		{
			return _recruitPopupParent;
		}
		set
		{
			if (value != _recruitPopupParent)
			{
				_recruitPopupParent = value;
			}
		}
	}

	public Widget TakeAllPrisonersInputKeyVisualParent
	{
		get
		{
			return _takeAllPrisonersInputKeyVisualParent;
		}
		set
		{
			if (value == _takeAllPrisonersInputKeyVisualParent)
			{
				return;
			}
			_takeAllPrisonersInputKeyVisualParent = value;
			if (_takeAllPrisonersInputKeyVisualParent != null)
			{
				_takeAllPrisonersInputKeyVisual = _takeAllPrisonersInputKeyVisualParent.Children.FirstOrDefault((Widget x) => x is InputKeyVisualWidget) as InputKeyVisualWidget;
			}
		}
	}

	public Widget DismissAllPrisonersInputKeyVisualParent
	{
		get
		{
			return _dismissAllPrisonersInputKeyVisualParent;
		}
		set
		{
			if (value == _dismissAllPrisonersInputKeyVisualParent)
			{
				return;
			}
			_dismissAllPrisonersInputKeyVisualParent = value;
			if (_dismissAllPrisonersInputKeyVisualParent != null)
			{
				_dismissAllPrisonersInputKeyVisual = _dismissAllPrisonersInputKeyVisualParent.Children.FirstOrDefault((Widget x) => x is InputKeyVisualWidget) as InputKeyVisualWidget;
			}
		}
	}

	[Editor(false)]
	public int MainPartyTroopSize
	{
		get
		{
			return _mainPartyTroopSize;
		}
		set
		{
			if (_mainPartyTroopSize != value)
			{
				_mainPartyTroopSize = value;
				OnPropertyChanged(value, "MainPartyTroopSize");
			}
		}
	}

	[Editor(false)]
	public bool IsPrisonerWarningEnabled
	{
		get
		{
			return _isPrisonerWarningEnabled;
		}
		set
		{
			if (_isPrisonerWarningEnabled != value)
			{
				_isPrisonerWarningEnabled = value;
				OnPropertyChanged(value, "IsPrisonerWarningEnabled");
				RefreshWarningStatuses();
			}
		}
	}

	[Editor(false)]
	public bool IsOtherTroopWarningEnabled
	{
		get
		{
			return _isOtherTroopWarningEnabled;
		}
		set
		{
			if (_isOtherTroopWarningEnabled != value)
			{
				_isOtherTroopWarningEnabled = value;
				OnPropertyChanged(value, "IsOtherTroopWarningEnabled");
				RefreshWarningStatuses();
			}
		}
	}

	[Editor(false)]
	public bool IsTroopWarningEnabled
	{
		get
		{
			return _isTroopWarningEnabled;
		}
		set
		{
			if (_isTroopWarningEnabled != value)
			{
				_isTroopWarningEnabled = value;
				OnPropertyChanged(value, "IsTroopWarningEnabled");
				RefreshWarningStatuses();
			}
		}
	}

	[Editor(false)]
	public TextWidget TroopLabel
	{
		get
		{
			return _troopLabel;
		}
		set
		{
			if (_troopLabel != value)
			{
				_troopLabel = value;
				OnPropertyChanged(value, "TroopLabel");
				RefreshWarningStatuses();
			}
		}
	}

	[Editor(false)]
	public TextWidget PrisonerLabel
	{
		get
		{
			return _prisonerLabel;
		}
		set
		{
			if (_prisonerLabel != value)
			{
				_prisonerLabel = value;
				OnPropertyChanged(value, "PrisonerLabel");
				RefreshWarningStatuses();
			}
		}
	}

	[Editor(false)]
	public TextWidget OtherTroopLabel
	{
		get
		{
			return _otherTroopLabel;
		}
		set
		{
			if (_otherTroopLabel != value)
			{
				_otherTroopLabel = value;
				OnPropertyChanged(value, "OtherTroopLabel");
				RefreshWarningStatuses();
			}
		}
	}

	[Editor(false)]
	public ListPanel OtherMemberList
	{
		get
		{
			return _otherMemberList;
		}
		set
		{
			if (_otherMemberList != value)
			{
				_otherMemberList = value;
				value.ItemAddEventHandlers.Add(OnNewTroopAdded);
			}
		}
	}

	[Editor(false)]
	public ListPanel OtherPrisonerList
	{
		get
		{
			return _otherPrisonerList;
		}
		set
		{
			if (_otherPrisonerList != value)
			{
				_otherPrisonerList = value;
				value.ItemAddEventHandlers.Add(OnNewTroopAdded);
			}
		}
	}

	[Editor(false)]
	public ListPanel MainMemberList
	{
		get
		{
			return _mainMemberList;
		}
		set
		{
			if (_mainMemberList != value)
			{
				_mainMemberList = value;
				value.ItemAddEventHandlers.Add(OnNewTroopAdded);
			}
		}
	}

	[Editor(false)]
	public ListPanel MainPrisonerList
	{
		get
		{
			return _mainPrisonerList;
		}
		set
		{
			if (_mainPrisonerList != value)
			{
				_mainPrisonerList = value;
				value.ItemAddEventHandlers.Add(OnNewTroopAdded);
			}
		}
	}

	public PartyScreenWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnConnectedToRoot()
	{
		base.Context.EventManager.OnDragStarted += OnDragStarted;
	}

	protected override void OnDisconnectedFromRoot()
	{
		base.Context.EventManager.OnDragStarted -= OnDragStarted;
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		Widget latestMouseUpWidget = base.EventManager.LatestMouseUpWidget;
		if (_currentMainTuple != null && latestMouseUpWidget != null && !(latestMouseUpWidget is PartyTroopTupleButtonWidget) && !_currentMainTuple.CheckIsMyChildRecursive(latestMouseUpWidget))
		{
			PartyTroopTupleButtonWidget currentOtherTuple = _currentOtherTuple;
			if (currentOtherTuple != null && !currentOtherTuple.CheckIsMyChildRecursive(latestMouseUpWidget) && IsWidgetChildOfType<PartyFormationDropdownWidget>(latestMouseUpWidget) == null)
			{
				SetCurrentTuple(null, isLeftSide: false);
			}
		}
		UpdateInputKeyVisualsVisibility();
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (_newAddedTroop != null)
		{
			if (_newAddedTroop.CharacterID == _currentMainTuple?.CharacterID && _newAddedTroop.IsPrisoner == _currentMainTuple?.IsPrisoner && _newAddedTroop.IsTupleLeftSide != _currentMainTuple?.IsTupleLeftSide)
			{
				_currentOtherTuple = _newAddedTroop;
				_currentOtherTuple.IsSelected = true;
				UpdateScrollTarget();
			}
			_newAddedTroop = null;
		}
	}

	public void SetCurrentTuple(PartyTroopTupleButtonWidget tuple, bool isLeftSide)
	{
		if (_currentMainTuple != null && _currentMainTuple != tuple)
		{
			_currentMainTuple.IsSelected = false;
			if (_currentOtherTuple != null)
			{
				_currentOtherTuple.IsSelected = false;
				_currentOtherTuple = null;
			}
		}
		if (tuple == null)
		{
			_currentMainTuple = null;
			RemoveZeroCountItems();
			if (_currentOtherTuple != null)
			{
				_currentOtherTuple.IsSelected = false;
				_currentOtherTuple = null;
			}
		}
		else if (tuple == _currentMainTuple || tuple == _currentOtherTuple)
		{
			SetCurrentTuple(null, isLeftSide: false);
		}
		else
		{
			_currentMainTuple = tuple;
			_currentOtherTuple = FindTupleWithTroopIDInList(_currentMainTuple.CharacterID, _currentMainTuple.IsTupleLeftSide, _currentMainTuple.IsPrisoner);
			if (_currentOtherTuple != null)
			{
				_currentOtherTuple.IsSelected = true;
				UpdateScrollTarget();
			}
		}
	}

	private void UpdateInputKeyVisualsVisibility()
	{
		if (base.EventManager.IsControllerActive)
		{
			bool flag = false;
			if (base.EventManager.HoveredView is PartyTroopTupleButtonWidget partyTroopTupleButtonWidget)
			{
				TransferInputKeyVisual.IsVisible = partyTroopTupleButtonWidget.IsTransferable;
				flag = true;
				if (partyTroopTupleButtonWidget.IsTupleLeftSide)
				{
					TransferInputKeyVisual.KeyID = _takeAllPrisonersInputKeyVisual.KeyID;
					TransferInputKeyVisual.ScaledPositionXOffset = partyTroopTupleButtonWidget.GlobalPosition.X + partyTroopTupleButtonWidget.Size.X - 65f * base._scaleToUse - base.EventManager.LeftUsableAreaStart;
					TransferInputKeyVisual.ScaledPositionYOffset = partyTroopTupleButtonWidget.GlobalPosition.Y - 13f * base._scaleToUse - base.EventManager.TopUsableAreaStart;
				}
				else
				{
					TransferInputKeyVisual.KeyID = _dismissAllPrisonersInputKeyVisual.KeyID;
					TransferInputKeyVisual.ScaledPositionXOffset = partyTroopTupleButtonWidget.GlobalPosition.X + 5f * base._scaleToUse - base.EventManager.LeftUsableAreaStart;
					TransferInputKeyVisual.ScaledPositionYOffset = partyTroopTupleButtonWidget.GlobalPosition.Y - 13f * base._scaleToUse - base.EventManager.TopUsableAreaStart;
				}
			}
			else
			{
				TransferInputKeyVisual.IsVisible = false;
				TransferInputKeyVisual.KeyID = "";
			}
			bool isVisible = !IsAnyPopupOpen() && !flag && !MainScrollPanel.InnerPanel.IsHovered && !OtherScrollPanel.InnerPanel.IsHovered && !GauntletGamepadNavigationManager.Instance.IsCursorMovingForNavigation;
			TakeAllPrisonersInputKeyVisualParent.IsVisible = isVisible;
			DismissAllPrisonersInputKeyVisualParent.IsVisible = isVisible;
		}
		else
		{
			TransferInputKeyVisual.IsVisible = false;
			TakeAllPrisonersInputKeyVisualParent.IsVisible = true;
			DismissAllPrisonersInputKeyVisualParent.IsVisible = true;
		}
	}

	private void RefreshWarningStatuses()
	{
		PrisonerLabel?.SetState(IsPrisonerWarningEnabled ? "OverLimit" : "Default");
		TroopLabel?.SetState(IsTroopWarningEnabled ? "OverLimit" : "Default");
		OtherTroopLabel?.SetState(IsOtherTroopWarningEnabled ? "OverLimit" : "Default");
	}

	private PartyTroopTupleButtonWidget FindTupleWithTroopIDInList(string troopID, bool searchMainList, bool isPrisoner)
	{
		IEnumerable<PartyTroopTupleButtonWidget> enumerable = null;
		enumerable = ((!searchMainList) ? (isPrisoner ? OtherPrisonerList.Children.Cast<PartyTroopTupleButtonWidget>() : OtherMemberList.Children.Cast<PartyTroopTupleButtonWidget>()) : (isPrisoner ? MainPrisonerList.Children.Cast<PartyTroopTupleButtonWidget>() : MainMemberList.Children.Cast<PartyTroopTupleButtonWidget>()));
		return enumerable.SingleOrDefault((PartyTroopTupleButtonWidget i) => i.CharacterID == troopID);
	}

	private void OnDragStarted()
	{
		RemoveZeroCountItems();
	}

	private void RemoveZeroCountItems()
	{
		EventFired("RemoveZeroCounts");
	}

	private void UpdateScrollTarget()
	{
		if (_currentOtherTuple?.ParentWidget != null)
		{
			(_currentOtherTuple.IsTupleLeftSide ? OtherScrollPanel : MainScrollPanel).ScrollToChild(_currentOtherTuple, -1f, 1f, 0, 400, 0.35f);
		}
	}

	private bool IsAnyPopupOpen()
	{
		Widget recruitPopupParent = RecruitPopupParent;
		if (recruitPopupParent == null || !recruitPopupParent.IsVisible)
		{
			return UpgradePopupParent?.IsVisible ?? false;
		}
		return true;
	}

	private void OnNewTroopAdded(Widget parent, Widget addedChild)
	{
		if (_currentMainTuple != null && addedChild is PartyTroopTupleButtonWidget newAddedTroop)
		{
			_newAddedTroop = newAddedTroop;
		}
	}

	private T IsWidgetChildOfType<T>(Widget currentWidget) where T : Widget
	{
		while (currentWidget != null)
		{
			if (currentWidget is T result)
			{
				return result;
			}
			currentWidget = currentWidget.ParentWidget;
		}
		return null;
	}
}
