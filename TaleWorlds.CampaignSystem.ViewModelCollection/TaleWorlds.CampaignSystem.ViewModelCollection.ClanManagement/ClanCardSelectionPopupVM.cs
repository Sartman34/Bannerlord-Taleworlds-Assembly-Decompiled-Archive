using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;

public class ClanCardSelectionPopupVM : ViewModel
{
	private TextObject _titleText;

	private bool _isMultiSelection;

	private ClanCardSelectionPopupItemVM _lastSelectedItem;

	private Action<List<object>, Action> _onClosed;

	private MBBindingList<ClanCardSelectionPopupItemVM> _items;

	private InputKeyItemVM _doneInputKey;

	private string _title;

	private string _actionResult;

	private string _doneLbl;

	private bool _isVisible;

	[DataSourceProperty]
	public MBBindingList<ClanCardSelectionPopupItemVM> Items
	{
		get
		{
			return _items;
		}
		set
		{
			if (value != _items)
			{
				_items = value;
				OnPropertyChangedWithValue(value, "Items");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				OnPropertyChangedWithValue(value, "DoneInputKey");
			}
		}
	}

	[DataSourceProperty]
	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != _title)
			{
				_title = value;
				OnPropertyChangedWithValue(value, "Title");
			}
		}
	}

	[DataSourceProperty]
	public string ActionResult
	{
		get
		{
			return _actionResult;
		}
		set
		{
			if (value != _actionResult)
			{
				_actionResult = value;
				OnPropertyChangedWithValue(value, "ActionResult");
			}
		}
	}

	[DataSourceProperty]
	public string DoneLbl
	{
		get
		{
			return _doneLbl;
		}
		set
		{
			if (value != _doneLbl)
			{
				_doneLbl = value;
				OnPropertyChangedWithValue(value, "DoneLbl");
			}
		}
	}

	[DataSourceProperty]
	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			if (value != _isVisible)
			{
				_isVisible = value;
				OnPropertyChangedWithValue(value, "IsVisible");
			}
		}
	}

	public ClanCardSelectionPopupVM()
	{
		_titleText = TextObject.Empty;
		Items = new MBBindingList<ClanCardSelectionPopupItemVM>();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		if (!_isMultiSelection)
		{
			ActionResult = _lastSelectedItem?.ActionResultText?.ToString() ?? string.Empty;
		}
		DoneLbl = GameTexts.FindText("str_done").ToString();
		Title = _titleText?.ToString() ?? string.Empty;
		Items.ApplyActionOnAllItems(delegate(ClanCardSelectionPopupItemVM x)
		{
			x.RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		DoneInputKey?.OnFinalize();
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}

	public void Open(ClanCardSelectionInfo info)
	{
		_isMultiSelection = info.IsMultiSelection;
		_titleText = info.Title;
		_onClosed = info.OnClosedAction;
		foreach (ClanCardSelectionItemInfo item in info.Items)
		{
			ClanCardSelectionItemInfo info2 = item;
			Items.Add(new ClanCardSelectionPopupItemVM(in info2, OnItemSelected));
		}
		RefreshValues();
		IsVisible = true;
	}

	public void ExecuteCancel()
	{
		_onClosed?.Invoke(new List<object>(), null);
		Close();
	}

	public void ExecuteDone()
	{
		List<object> selectedItems = new List<object>();
		Items.ApplyActionOnAllItems(delegate(ClanCardSelectionPopupItemVM x)
		{
			if (x.IsSelected)
			{
				selectedItems.Add(x.Identifier);
			}
		});
		_onClosed?.Invoke(selectedItems, Close);
	}

	private void Close()
	{
		IsVisible = false;
		_lastSelectedItem = null;
		_titleText = TextObject.Empty;
		ActionResult = string.Empty;
		Title = string.Empty;
		_onClosed = null;
		Items.Clear();
	}

	private void OnItemSelected(ClanCardSelectionPopupItemVM item)
	{
		if (_isMultiSelection)
		{
			item.IsSelected = !item.IsSelected;
		}
		else if (item != _lastSelectedItem)
		{
			if (_lastSelectedItem != null)
			{
				_lastSelectedItem.IsSelected = false;
			}
			item.IsSelected = true;
			ActionResult = item.ActionResultText?.ToString() ?? string.Empty;
		}
		_lastSelectedItem = item;
	}
}
