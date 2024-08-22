using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;

public class GameMenuItemVM : BindingListStringItem
{
	private MenuContext _menuContext;

	public int Index;

	private TextObject _nonWaitText;

	private TextObject _waitText;

	private TextObject _tooltip;

	private readonly GameMenuOption _gameMenuOption;

	private MBBindingList<QuestMarkerVM> _quests;

	private int _itemType = -1;

	private bool _isWaitActive;

	private bool _isEnabled;

	private HintViewModel _itemHint;

	private HintViewModel _questHint;

	private HintViewModel _issueHint;

	private bool _isHighlightEnabled;

	private int _optionLeaveType = -1;

	private string _gameMenuStringId;

	private int _battleSize = -1;

	private InputKeyItemVM _shortcutKey;

	public string OptionID { get; }

	[DataSourceProperty]
	public MBBindingList<QuestMarkerVM> Quests
	{
		get
		{
			return _quests;
		}
		set
		{
			if (value != _quests)
			{
				_quests = value;
				OnPropertyChangedWithValue(value, "Quests");
			}
		}
	}

	[DataSourceProperty]
	public int OptionLeaveType
	{
		get
		{
			return _optionLeaveType;
		}
		set
		{
			if (value != _optionLeaveType)
			{
				_optionLeaveType = value;
				OnPropertyChangedWithValue(value, "OptionLeaveType");
			}
		}
	}

	[DataSourceProperty]
	public int ItemType
	{
		get
		{
			return _itemType;
		}
		set
		{
			if (value != _itemType)
			{
				_itemType = value;
				OnPropertyChangedWithValue(value, "ItemType");
			}
		}
	}

	[DataSourceProperty]
	public bool IsWaitActive
	{
		get
		{
			return _isWaitActive;
		}
		set
		{
			if (value != _isWaitActive)
			{
				_isWaitActive = value;
				OnPropertyChangedWithValue(value, "IsWaitActive");
				base.Item = (value ? _waitText.ToString() : _nonWaitText.ToString());
			}
		}
	}

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsHighlightEnabled
	{
		get
		{
			return _isHighlightEnabled;
		}
		set
		{
			if (value != _isHighlightEnabled)
			{
				_isHighlightEnabled = value;
				OnPropertyChangedWithValue(value, "IsHighlightEnabled");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ItemHint
	{
		get
		{
			return _itemHint;
		}
		set
		{
			if (value != _itemHint)
			{
				_itemHint = value;
				OnPropertyChangedWithValue(value, "ItemHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel QuestHint
	{
		get
		{
			return _questHint;
		}
		set
		{
			if (value != _questHint)
			{
				_questHint = value;
				OnPropertyChangedWithValue(value, "QuestHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel IssueHint
	{
		get
		{
			return _issueHint;
		}
		set
		{
			if (value != _issueHint)
			{
				_issueHint = value;
				OnPropertyChangedWithValue(value, "IssueHint");
			}
		}
	}

	[DataSourceProperty]
	public string GameMenuStringId
	{
		get
		{
			return _gameMenuStringId;
		}
		set
		{
			if (value != _gameMenuStringId)
			{
				_gameMenuStringId = value;
				OnPropertyChangedWithValue(value, "GameMenuStringId");
			}
		}
	}

	[DataSourceProperty]
	public int BattleSize
	{
		get
		{
			return _battleSize;
		}
		set
		{
			if (value != _battleSize)
			{
				_battleSize = value;
				OnPropertyChangedWithValue(value, "BattleSize");
			}
		}
	}

	public InputKeyItemVM ShortcutKey
	{
		get
		{
			return _shortcutKey;
		}
		set
		{
			if (value != _shortcutKey)
			{
				_shortcutKey = value;
				OnPropertyChangedWithValue(value, "ShortcutKey");
			}
		}
	}

	public GameMenuItemVM(MenuContext menuContext, int index, TextObject text, TextObject text2, TextObject tooltip, TaleWorlds.CampaignSystem.GameMenus.GameMenu.MenuAndOptionType type, GameMenuOption gameMenuOption, GameKey shortcutKey)
		: base(text.ToString())
	{
		_gameMenuOption = gameMenuOption;
		ItemHint = new HintViewModel();
		Index = index;
		_menuContext = menuContext;
		_itemType = (int)type;
		_tooltip = tooltip;
		_nonWaitText = text;
		_waitText = text2;
		base.Item = _nonWaitText.ToString();
		ItemHint.HintText = _tooltip;
		OptionLeaveType = (int)gameMenuOption.OptionLeaveType;
		OptionID = gameMenuOption.IdString;
		Quests = new MBBindingList<QuestMarkerVM>();
		GameMenuOption.IssueQuestFlags[] issueQuestFlagsValues = GameMenuOption.IssueQuestFlagsValues;
		foreach (GameMenuOption.IssueQuestFlags issueQuestFlags in issueQuestFlagsValues)
		{
			if (issueQuestFlags != 0 && (gameMenuOption.OptionQuestData & issueQuestFlags) != 0)
			{
				CampaignUIHelper.IssueQuestFlags issueQuestFlag = (CampaignUIHelper.IssueQuestFlags)issueQuestFlags;
				Quests.Add(new QuestMarkerVM(issueQuestFlag));
			}
		}
		ShortcutKey = ((shortcutKey != null) ? InputKeyItemVM.CreateFromGameKey(shortcutKey, isConsoleOnly: true) : null);
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		Refresh();
	}

	public void UpdateMenuContext(MenuContext newMenuContext)
	{
		_menuContext = newMenuContext;
		Refresh();
	}

	public void ExecuteAction()
	{
		_menuContext?.InvokeConsequence(Index);
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		if (ShortcutKey != null)
		{
			ShortcutKey.OnFinalize();
		}
	}

	public void Refresh()
	{
		int itemType = _itemType;
		if (itemType != 0)
		{
			_ = itemType - 1;
			_ = 2;
		}
		IsWaitActive = Campaign.Current.GameMenuManager.GetVirtualMenuIsWaitActive(_menuContext);
		IsEnabled = Campaign.Current.GameMenuManager.GetVirtualMenuOptionIsEnabled(_menuContext, Index);
		ItemHint.HintText = Campaign.Current.GameMenuManager.GetVirtualMenuOptionTooltip(_menuContext, Index);
		GameMenuStringId = _menuContext.GameMenu.StringId;
		if (PlayerEncounter.Battle != null)
		{
			BattleSize = PlayerEncounter.Battle.AttackerSide.TroopCount + PlayerEncounter.Battle.DefenderSide.TroopCount;
		}
		else
		{
			BattleSize = -1;
		}
	}
}
