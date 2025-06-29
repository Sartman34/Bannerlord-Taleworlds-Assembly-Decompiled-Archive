using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;

public class ClanPartiesSortControllerVM : ViewModel
{
	public abstract class ItemComparerBase : IComparer<ClanPartyItemVM>
	{
		protected bool _isAcending;

		public void SetSortMode(bool isAcending)
		{
			_isAcending = isAcending;
		}

		public abstract int Compare(ClanPartyItemVM x, ClanPartyItemVM y);
	}

	public class ItemNameComparer : ItemComparerBase
	{
		public override int Compare(ClanPartyItemVM x, ClanPartyItemVM y)
		{
			if (_isAcending)
			{
				return y.Name.CompareTo(x.Name) * -1;
			}
			return y.Name.CompareTo(x.Name);
		}
	}

	public class ItemLocationComparer : ItemComparerBase
	{
		public override int Compare(ClanPartyItemVM x, ClanPartyItemVM y)
		{
			if (_isAcending)
			{
				return y.Party.MobileParty.GetTrackDistanceToMainAgent().CompareTo(x.Party.MobileParty.GetTrackDistanceToMainAgent()) * -1;
			}
			return y.Party.MobileParty.GetTrackDistanceToMainAgent().CompareTo(x.Party.MobileParty.GetTrackDistanceToMainAgent());
		}
	}

	public class ItemSizeComparer : ItemComparerBase
	{
		public override int Compare(ClanPartyItemVM x, ClanPartyItemVM y)
		{
			if (_isAcending)
			{
				return y.Party.MobileParty.MemberRoster.TotalManCount.CompareTo(x.Party.MobileParty.MemberRoster.TotalManCount) * -1;
			}
			return y.Party.MobileParty.MemberRoster.TotalManCount.CompareTo(x.Party.MobileParty.MemberRoster.TotalManCount);
		}
	}

	private readonly MBBindingList<MBBindingList<ClanPartyItemVM>> _listsToControl;

	private readonly ItemNameComparer _nameComparer;

	private readonly ItemLocationComparer _locationComparer;

	private readonly ItemSizeComparer _sizeComparer;

	private int _nameState;

	private int _locationState;

	private int _sizeState;

	private bool _isNameSelected;

	private bool _isLocationSelected;

	private bool _isSizeSelected;

	private string _nameText;

	private string _locationText;

	private string _sizeText;

	[DataSourceProperty]
	public int NameState
	{
		get
		{
			return _nameState;
		}
		set
		{
			if (value != _nameState)
			{
				_nameState = value;
				OnPropertyChangedWithValue(value, "NameState");
			}
		}
	}

	[DataSourceProperty]
	public int LocationState
	{
		get
		{
			return _locationState;
		}
		set
		{
			if (value != _locationState)
			{
				_locationState = value;
				OnPropertyChangedWithValue(value, "LocationState");
			}
		}
	}

	[DataSourceProperty]
	public int SizeState
	{
		get
		{
			return _sizeState;
		}
		set
		{
			if (value != _sizeState)
			{
				_sizeState = value;
				OnPropertyChangedWithValue(value, "SizeState");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNameSelected
	{
		get
		{
			return _isNameSelected;
		}
		set
		{
			if (value != _isNameSelected)
			{
				_isNameSelected = value;
				OnPropertyChangedWithValue(value, "IsNameSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsLocationSelected
	{
		get
		{
			return _isLocationSelected;
		}
		set
		{
			if (value != _isLocationSelected)
			{
				_isLocationSelected = value;
				OnPropertyChangedWithValue(value, "IsLocationSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSizeSelected
	{
		get
		{
			return _isSizeSelected;
		}
		set
		{
			if (value != _isSizeSelected)
			{
				_isSizeSelected = value;
				OnPropertyChangedWithValue(value, "IsSizeSelected");
			}
		}
	}

	[DataSourceProperty]
	public string NameText
	{
		get
		{
			return _nameText;
		}
		set
		{
			if (value != _nameText)
			{
				_nameText = value;
				OnPropertyChangedWithValue(value, "NameText");
			}
		}
	}

	[DataSourceProperty]
	public string LocationText
	{
		get
		{
			return _locationText;
		}
		set
		{
			if (value != _locationText)
			{
				_locationText = value;
				OnPropertyChangedWithValue(value, "LocationText");
			}
		}
	}

	[DataSourceProperty]
	public string SizeText
	{
		get
		{
			return _sizeText;
		}
		set
		{
			if (value != _sizeText)
			{
				_sizeText = value;
				OnPropertyChangedWithValue(value, "SizeText");
			}
		}
	}

	public ClanPartiesSortControllerVM(MBBindingList<MBBindingList<ClanPartyItemVM>> listsToControl)
	{
		_listsToControl = listsToControl;
		_nameComparer = new ItemNameComparer();
		_locationComparer = new ItemLocationComparer();
		_sizeComparer = new ItemSizeComparer();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		NameText = GameTexts.FindText("str_sort_by_name_label").ToString();
		LocationText = GameTexts.FindText("str_tooltip_label_location").ToString();
		SizeText = GameTexts.FindText("str_clan_party_size").ToString();
	}

	public void ExecuteSortByName()
	{
		int nameState = NameState;
		SetAllStates(CampaignUIHelper.SortState.Default);
		NameState = (nameState + 1) % 3;
		if (NameState == 0)
		{
			NameState++;
		}
		_nameComparer.SetSortMode(NameState == 1);
		foreach (MBBindingList<ClanPartyItemVM> item in _listsToControl)
		{
			item.Sort(_nameComparer);
		}
		IsNameSelected = true;
	}

	public void ExecuteSortByLocation()
	{
		int locationState = LocationState;
		SetAllStates(CampaignUIHelper.SortState.Default);
		LocationState = (locationState + 1) % 3;
		if (LocationState == 0)
		{
			LocationState++;
		}
		_locationComparer.SetSortMode(LocationState == 1);
		foreach (MBBindingList<ClanPartyItemVM> item in _listsToControl)
		{
			item.Sort(_locationComparer);
		}
		IsLocationSelected = true;
	}

	public void ExecuteSortBySize()
	{
		int sizeState = SizeState;
		SetAllStates(CampaignUIHelper.SortState.Default);
		SizeState = (sizeState + 1) % 3;
		if (SizeState == 0)
		{
			SizeState++;
		}
		_sizeComparer.SetSortMode(SizeState == 1);
		foreach (MBBindingList<ClanPartyItemVM> item in _listsToControl)
		{
			item.Sort(_sizeComparer);
		}
		IsSizeSelected = true;
	}

	private void SetAllStates(CampaignUIHelper.SortState state)
	{
		NameState = (int)state;
		LocationState = (int)state;
		SizeState = (int)state;
		IsNameSelected = false;
		IsLocationSelected = false;
		IsSizeSelected = false;
	}

	public void ResetAllStates()
	{
		SetAllStates(CampaignUIHelper.SortState.Default);
	}
}
