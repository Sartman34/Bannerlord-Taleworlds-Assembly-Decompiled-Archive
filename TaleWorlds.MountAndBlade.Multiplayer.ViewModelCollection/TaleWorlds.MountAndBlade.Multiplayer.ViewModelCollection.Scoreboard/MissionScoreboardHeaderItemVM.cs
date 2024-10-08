using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

public class MissionScoreboardHeaderItemVM : BindingListStringItem
{
	private readonly MissionScoreboardSideVM _side;

	private string _headerID = "";

	private bool _isIrregularStat;

	private bool _isAvatarStat;

	[DataSourceProperty]
	public string HeaderID
	{
		get
		{
			return _headerID;
		}
		set
		{
			if (value != _headerID)
			{
				_headerID = value;
				OnPropertyChangedWithValue(value, "HeaderID");
			}
		}
	}

	[DataSourceProperty]
	public bool IsIrregularStat
	{
		get
		{
			return _isIrregularStat;
		}
		set
		{
			if (value != _isIrregularStat)
			{
				_isIrregularStat = value;
				OnPropertyChangedWithValue(value, "IsIrregularStat");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAvatarStat
	{
		get
		{
			return _isAvatarStat;
		}
		set
		{
			if (value != _isAvatarStat)
			{
				_isAvatarStat = value;
				OnPropertyChangedWithValue(value, "IsAvatarStat");
			}
		}
	}

	[DataSourceProperty]
	public MissionScoreboardPlayerSortControllerVM PlayerSortController => _side.PlayerSortController;

	public MissionScoreboardHeaderItemVM(MissionScoreboardSideVM side, string headerID, string value, bool isAvatarStat, bool isIrregularStat)
		: base(value)
	{
		_side = side;
		HeaderID = headerID;
		IsAvatarStat = isAvatarStat;
		IsIrregularStat = isIrregularStat;
	}
}
