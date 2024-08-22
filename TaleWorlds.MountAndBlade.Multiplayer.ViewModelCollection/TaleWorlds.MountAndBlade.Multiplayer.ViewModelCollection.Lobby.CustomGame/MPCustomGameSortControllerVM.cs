using System.Collections.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;

public class MPCustomGameSortControllerVM : ViewModel
{
	private enum SortState
	{
		Default,
		Ascending,
		Descending
	}

	public enum CustomServerSortOption
	{
		SortOptionsBeginExclusive = -1,
		Name,
		GameType,
		PlayerCount,
		PasswordProtection,
		FirstFaction,
		SecondFaction,
		Region,
		PremadeMatchType,
		Host,
		Ping,
		Favorite,
		SortOptionsEndExclusive
	}

	private abstract class ItemComparer : IComparer<MPCustomGameItemVM>
	{
		protected bool _isAscending;

		public void SetSortMode(bool isAscending)
		{
			_isAscending = isAscending;
		}

		public abstract int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y);
	}

	private class ServerNameComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (_isAscending)
			{
				return y.NameText.CompareTo(x.NameText) * -1;
			}
			return y.NameText.CompareTo(x.NameText);
		}
	}

	private class GameTypeComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (_isAscending)
			{
				return y.GameTypeText.CompareTo(x.GameTypeText) * -1;
			}
			return y.GameTypeText.CompareTo(x.GameTypeText);
		}
	}

	private class PlayerCountComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (_isAscending)
			{
				return y.PlayerCount.CompareTo(x.PlayerCount) * -1;
			}
			return y.PlayerCount.CompareTo(x.PlayerCount);
		}
	}

	private class PasswordComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (_isAscending)
			{
				return y.IsPasswordProtected.CompareTo(x.IsPasswordProtected) * -1;
			}
			return y.IsPasswordProtected.CompareTo(x.IsPasswordProtected);
		}
	}

	private class FirstFactionComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (_isAscending)
			{
				return y.FirstFactionName.CompareTo(x.FirstFactionName) * -1;
			}
			return y.FirstFactionName.CompareTo(x.FirstFactionName);
		}
	}

	private class SecondFactionComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (_isAscending)
			{
				return y.SecondFactionName.CompareTo(x.SecondFactionName) * -1;
			}
			return y.SecondFactionName.CompareTo(x.SecondFactionName);
		}
	}

	private class RegionComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (_isAscending)
			{
				return y.RegionName.CompareTo(x.RegionName) * -1;
			}
			return y.RegionName.CompareTo(x.RegionName);
		}
	}

	private class PremadeMatchTypeComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (_isAscending)
			{
				return y.PremadeMatchTypeText.CompareTo(x.PremadeMatchTypeText) * -1;
			}
			return y.PremadeMatchTypeText.CompareTo(x.PremadeMatchTypeText);
		}
	}

	private class HostComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			if (!(y.HostText == x.HostText))
			{
				return (y.HostText?.CompareTo(x.HostText) ?? (-1)) * ((!_isAscending) ? 1 : (-1));
			}
			return 0;
		}
	}

	private class PingComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			int num = ((!_isAscending) ? 1 : (-1));
			if (y.PingText == x.PingText)
			{
				return 0;
			}
			if (y.PingText == "-" || y.PingText == null)
			{
				return num;
			}
			if (x.PingText == "-" || x.PingText == null)
			{
				return num * -1;
			}
			return (int)(long.Parse(y.PingText) - long.Parse(x.PingText)) * num;
		}
	}

	private class FavoriteComparer : ItemComparer
	{
		public override int Compare(MPCustomGameItemVM x, MPCustomGameItemVM y)
		{
			return y.IsFavorite.CompareTo(x.IsFavorite) * ((!_isAscending) ? 1 : (-1));
		}
	}

	private MBBindingList<MPCustomGameItemVM> _listToControl;

	private readonly ItemComparer[] _sortComparers;

	private readonly int _numberOfSortOptions;

	private bool _isPremadeMatchesList;

	private bool _isPingInfoAvailable;

	private int _currentSortState;

	private bool _isFavoritesSelected;

	private bool _isServerNameSelected;

	private bool _isGameTypeSelected;

	private bool _isPlayerCountSelected;

	private bool _isPasswordSelected;

	private bool _isFirstFactionSelected;

	private bool _isSecondFactionSelected;

	private bool _isRegionSelected;

	private bool _isPremadeMatchTypeSelected;

	private bool _isHostSelected;

	private bool _isPingSelected;

	public CustomServerSortOption? CurrentSortOption { get; private set; }

	[DataSourceProperty]
	public bool IsPremadeMatchesList
	{
		get
		{
			return _isPremadeMatchesList;
		}
		set
		{
			if (value != _isPremadeMatchesList)
			{
				_isPremadeMatchesList = value;
				OnPropertyChanged("IsPremadeMatchesList");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPingInfoAvailable
	{
		get
		{
			return _isPingInfoAvailable;
		}
		set
		{
			if (value != _isPingInfoAvailable)
			{
				_isPingInfoAvailable = value;
				OnPropertyChanged("IsPingInfoAvailable");
			}
		}
	}

	[DataSourceProperty]
	public int CurrentSortState
	{
		get
		{
			return _currentSortState;
		}
		set
		{
			if (value != _currentSortState)
			{
				_currentSortState = value;
				OnPropertyChanged("CurrentSortState");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFavoritesSelected
	{
		get
		{
			return _isFavoritesSelected;
		}
		set
		{
			if (value != _isFavoritesSelected)
			{
				_isFavoritesSelected = value;
				OnPropertyChanged("IsFavoritesSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsServerNameSelected
	{
		get
		{
			return _isServerNameSelected;
		}
		set
		{
			if (value != _isServerNameSelected)
			{
				_isServerNameSelected = value;
				OnPropertyChanged("IsServerNameSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPasswordSelected
	{
		get
		{
			return _isPasswordSelected;
		}
		set
		{
			if (value != _isPasswordSelected)
			{
				_isPasswordSelected = value;
				OnPropertyChanged("IsPasswordSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerCountSelected
	{
		get
		{
			return _isPlayerCountSelected;
		}
		set
		{
			if (value != _isPlayerCountSelected)
			{
				_isPlayerCountSelected = value;
				OnPropertyChanged("IsPlayerCountSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFirstFactionSelected
	{
		get
		{
			return _isFirstFactionSelected;
		}
		set
		{
			if (value != _isFirstFactionSelected)
			{
				_isFirstFactionSelected = value;
				OnPropertyChanged("IsFirstFactionSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsGameTypeSelected
	{
		get
		{
			return _isGameTypeSelected;
		}
		set
		{
			if (value != _isGameTypeSelected)
			{
				_isGameTypeSelected = value;
				OnPropertyChanged("IsGameTypeSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSecondFactionSelected
	{
		get
		{
			return _isSecondFactionSelected;
		}
		set
		{
			if (value != _isSecondFactionSelected)
			{
				_isSecondFactionSelected = value;
				OnPropertyChanged("IsSecondFactionSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRegionSelected
	{
		get
		{
			return _isRegionSelected;
		}
		set
		{
			if (value != _isRegionSelected)
			{
				_isRegionSelected = value;
				OnPropertyChanged("IsRegionSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPremadeMatchTypeSelected
	{
		get
		{
			return _isPremadeMatchTypeSelected;
		}
		set
		{
			if (value != _isPremadeMatchTypeSelected)
			{
				_isPremadeMatchTypeSelected = value;
				OnPropertyChanged("IsPremadeMatchTypeSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsHostSelected
	{
		get
		{
			return _isHostSelected;
		}
		set
		{
			if (value != _isHostSelected)
			{
				_isHostSelected = value;
				OnPropertyChanged("IsHostSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPingSelected
	{
		get
		{
			return _isPingSelected;
		}
		set
		{
			if (value != _isPingSelected)
			{
				_isPingSelected = value;
				OnPropertyChanged("IsPingSelected");
			}
		}
	}

	public MPCustomGameSortControllerVM(ref MBBindingList<MPCustomGameItemVM> listToControl, MPCustomGameVM.CustomGameMode customGameMode)
	{
		_listToControl = listToControl;
		IsPremadeMatchesList = customGameMode == MPCustomGameVM.CustomGameMode.PremadeGame;
		IsPingInfoAvailable = MPCustomGameVM.IsPingInfoAvailable && !IsPremadeMatchesList;
		_numberOfSortOptions = 11;
		_sortComparers = new ItemComparer[_numberOfSortOptions];
		for (CustomServerSortOption customServerSortOption = CustomServerSortOption.Name; customServerSortOption < CustomServerSortOption.SortOptionsEndExclusive; customServerSortOption++)
		{
			ItemComparer sortComparer = GetSortComparer(customServerSortOption);
			if (sortComparer != null)
			{
				_sortComparers[(int)customServerSortOption] = sortComparer;
			}
			else
			{
				Debug.FailedAssert("No valid comparer for custom server sort option: " + customServerSortOption, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\CustomGame\\MPCustomGameSortControllerVM.cs", ".ctor", 59);
			}
		}
		RefreshValues();
	}

	private ItemComparer GetSortComparer(CustomServerSortOption option)
	{
		switch (option)
		{
		case CustomServerSortOption.SortOptionsBeginExclusive:
		case CustomServerSortOption.SortOptionsEndExclusive:
			return null;
		case CustomServerSortOption.Name:
			return _sortComparers[(int)option] ?? new ServerNameComparer();
		case CustomServerSortOption.GameType:
			return _sortComparers[(int)option] ?? new GameTypeComparer();
		case CustomServerSortOption.PlayerCount:
			return _sortComparers[(int)option] ?? new PlayerCountComparer();
		case CustomServerSortOption.PasswordProtection:
			return _sortComparers[(int)option] ?? new PasswordComparer();
		case CustomServerSortOption.FirstFaction:
			return _sortComparers[(int)option] ?? new FirstFactionComparer();
		case CustomServerSortOption.SecondFaction:
			return _sortComparers[(int)option] ?? new SecondFactionComparer();
		case CustomServerSortOption.Region:
			return _sortComparers[(int)option] ?? new RegionComparer();
		case CustomServerSortOption.PremadeMatchType:
			return _sortComparers[(int)option] ?? new PremadeMatchTypeComparer();
		case CustomServerSortOption.Host:
			return _sortComparers[(int)option] ?? new HostComparer();
		case CustomServerSortOption.Ping:
			return _sortComparers[(int)option] ?? new PingComparer();
		case CustomServerSortOption.Favorite:
			return _sortComparers[(int)option] ?? new FavoriteComparer();
		default:
			return null;
		}
	}

	public void SetSortOption(CustomServerSortOption? sortOption)
	{
		CustomServerSortOption? customServerSortOption = sortOption;
		if (customServerSortOption.HasValue)
		{
			switch (customServerSortOption.GetValueOrDefault())
			{
			case CustomServerSortOption.Name:
				ExecuteSortByServerName();
				break;
			case CustomServerSortOption.GameType:
				ExecuteSortByGameType();
				break;
			case CustomServerSortOption.PlayerCount:
				ExecuteSortByPlayerCount();
				break;
			case CustomServerSortOption.PasswordProtection:
				ExecuteSortByPassword();
				break;
			case CustomServerSortOption.FirstFaction:
				ExecuteSortByFirstFaction();
				break;
			case CustomServerSortOption.SecondFaction:
				ExecuteSortBySecondFaction();
				break;
			case CustomServerSortOption.Region:
				ExecuteSortByRegion();
				break;
			case CustomServerSortOption.PremadeMatchType:
				ExecuteSortByPremadeMatchType();
				break;
			case CustomServerSortOption.Host:
				ExecuteSortByHost();
				break;
			case CustomServerSortOption.Ping:
				ExecuteSortByPing();
				break;
			}
		}
	}

	public void SortByCurrentState()
	{
		ItemComparer sortComparer = GetSortComparer(CurrentSortOption ?? CustomServerSortOption.Name);
		if (sortComparer != null)
		{
			SortState currentSortState = (SortState)CurrentSortState;
			sortComparer.SetSortMode(currentSortState != SortState.Descending);
			_listToControl.Sort(sortComparer);
		}
	}

	public void ExecuteSortBy(CustomServerSortOption option)
	{
		if (GetSortComparer(option) != null)
		{
			if (CurrentSortOption == option)
			{
				CurrentSortState = (CurrentSortState + 1) % 3;
			}
			else
			{
				CurrentSortState = 1;
			}
			CurrentSortOption = option;
			ResetAllSelectedStates();
			SortByCurrentState();
		}
	}

	public void ExecuteSortByFavorites()
	{
		ExecuteSortBy(CustomServerSortOption.Favorite);
		IsFavoritesSelected = true;
	}

	public void ExecuteSortByServerName()
	{
		ExecuteSortBy(CustomServerSortOption.Name);
		IsServerNameSelected = true;
	}

	public void ExecuteSortByGameType()
	{
		ExecuteSortBy(CustomServerSortOption.GameType);
		IsGameTypeSelected = true;
	}

	public void ExecuteSortByPlayerCount()
	{
		ExecuteSortBy(CustomServerSortOption.PlayerCount);
		IsPlayerCountSelected = true;
	}

	public void ExecuteSortByPassword()
	{
		ExecuteSortBy(CustomServerSortOption.PasswordProtection);
		IsPasswordSelected = true;
	}

	public void ExecuteSortByFirstFaction()
	{
		ExecuteSortBy(CustomServerSortOption.FirstFaction);
		IsFirstFactionSelected = true;
	}

	public void ExecuteSortBySecondFaction()
	{
		ExecuteSortBy(CustomServerSortOption.SecondFaction);
		IsSecondFactionSelected = true;
	}

	public void ExecuteSortByRegion()
	{
		ExecuteSortBy(CustomServerSortOption.Region);
		IsRegionSelected = true;
	}

	public void ExecuteSortByPremadeMatchType()
	{
		ExecuteSortBy(CustomServerSortOption.PremadeMatchType);
		IsPremadeMatchTypeSelected = true;
	}

	public void ExecuteSortByHost()
	{
		ExecuteSortBy(CustomServerSortOption.Host);
		IsHostSelected = true;
	}

	public void ExecuteSortByPing()
	{
		ExecuteSortBy(CustomServerSortOption.Ping);
		IsPingSelected = true;
	}

	private void ResetAllSelectedStates()
	{
		IsFavoritesSelected = false;
		IsServerNameSelected = false;
		IsGameTypeSelected = false;
		IsPlayerCountSelected = false;
		IsPasswordSelected = false;
		IsFirstFactionSelected = false;
		IsSecondFactionSelected = false;
		IsRegionSelected = false;
		IsPremadeMatchTypeSelected = false;
		IsHostSelected = false;
		IsPingSelected = false;
	}
}
