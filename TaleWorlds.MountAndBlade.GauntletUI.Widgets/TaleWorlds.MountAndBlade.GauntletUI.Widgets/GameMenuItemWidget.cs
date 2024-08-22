using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class GameMenuItemWidget : Widget
{
	public Action OnOptionStateChanged;

	private string _latestTextWidgetState = "";

	private bool _firstFrame = true;

	private int _itemType;

	private bool _isWaitActive;

	private bool _isMainStoryQuest;

	private BrushWidget _waitStateWidget;

	private BrushWidget _leaveTypeIcon;

	private int _leaveType = -1;

	private int _questType = -1;

	private int _issueType = -1;

	private BrushWidget _questIconWidget;

	private BrushWidget _issueIconWidget;

	private ButtonWidget _parentButton;

	private string _gameMenuStringId;

	private int _battleSize;

	public Brush DefaultTextBrush { get; set; }

	public Brush HoveredTextBrush { get; set; }

	public Brush PressedTextBrush { get; set; }

	public Brush DisabledTextBrush { get; set; }

	public Brush NormalQuestBrush { get; set; }

	public Brush MainStoryQuestBrush { get; set; }

	public RichTextWidget ItemRichTextWidget { get; set; }

	public int ItemType
	{
		get
		{
			return _itemType;
		}
		set
		{
			if (_itemType != value)
			{
				_itemType = value;
				OnPropertyChanged(value, "ItemType");
			}
		}
	}

	public BrushWidget QuestIconWidget
	{
		get
		{
			return _questIconWidget;
		}
		set
		{
			if (_questIconWidget != value)
			{
				_questIconWidget = value;
				OnPropertyChanged(value, "QuestIconWidget");
			}
		}
	}

	public BrushWidget IssueIconWidget
	{
		get
		{
			return _issueIconWidget;
		}
		set
		{
			if (_issueIconWidget != value)
			{
				_issueIconWidget = value;
				OnPropertyChanged(value, "IssueIconWidget");
			}
		}
	}

	public int LeaveType
	{
		get
		{
			return _leaveType;
		}
		set
		{
			if (_leaveType != value)
			{
				_leaveType = value;
				OnPropertyChanged(value, "LeaveType");
				SetLeaveTypeIcon(value);
				SetLeaveTypeSound();
			}
		}
	}

	public bool IsMainStoryQuest
	{
		get
		{
			return _isMainStoryQuest;
		}
		set
		{
			if (_isMainStoryQuest != value)
			{
				_isMainStoryQuest = value;
				OnPropertyChanged(value, "IsMainStoryQuest");
				SetProgressIconType(QuestType, QuestIconWidget);
			}
		}
	}

	public int QuestType
	{
		get
		{
			return _questType;
		}
		set
		{
			if (_questType != value)
			{
				_questType = value;
				OnPropertyChanged(value, "QuestType");
				SetProgressIconType(value, QuestIconWidget);
			}
		}
	}

	public int IssueType
	{
		get
		{
			return _issueType;
		}
		set
		{
			if (_issueType != value)
			{
				_issueType = value;
				OnPropertyChanged(value, "IssueType");
				SetProgressIconType(value, IssueIconWidget);
			}
		}
	}

	public bool IsWaitActive
	{
		get
		{
			return _isWaitActive;
		}
		set
		{
			if (_isWaitActive != value)
			{
				_isWaitActive = value;
				OnPropertyChanged(value, "IsWaitActive");
			}
		}
	}

	public BrushWidget LeaveTypeIcon
	{
		get
		{
			return _leaveTypeIcon;
		}
		set
		{
			if (_leaveTypeIcon != value)
			{
				_leaveTypeIcon = value;
				OnPropertyChanged(value, "LeaveTypeIcon");
				if (value != null)
				{
					LeaveTypeIcon.IsVisible = false;
				}
			}
		}
	}

	public BrushWidget WaitStateWidget
	{
		get
		{
			return _waitStateWidget;
		}
		set
		{
			if (_waitStateWidget != value)
			{
				_waitStateWidget = value;
				OnPropertyChanged(value, "WaitStateWidget");
			}
		}
	}

	public ButtonWidget ParentButton
	{
		get
		{
			return _parentButton;
		}
		set
		{
			if (value != _parentButton)
			{
				_parentButton = value;
				OnPropertyChanged(value, "ParentButton");
				_parentButton.boolPropertyChanged += ParentButton_PropertyChanged;
			}
		}
	}

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
				OnPropertyChanged(value, "GameMenuStringId");
				SetLeaveTypeSound();
			}
		}
	}

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
				OnPropertyChanged(value, "BattleSize");
				SetLeaveTypeSound();
			}
		}
	}

	public GameMenuItemWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (_firstFrame)
		{
			base.GamepadNavigationIndex = GetSiblingIndex();
			_firstFrame = false;
		}
		if (_latestTextWidgetState != ItemRichTextWidget.CurrentState)
		{
			if (ItemRichTextWidget.CurrentState == "Default")
			{
				ItemRichTextWidget.Brush = DefaultTextBrush;
			}
			else if (ItemRichTextWidget.CurrentState == "Hovered")
			{
				ItemRichTextWidget.Brush = HoveredTextBrush;
			}
			else if (ItemRichTextWidget.CurrentState == "Pressed")
			{
				ItemRichTextWidget.Brush = PressedTextBrush;
			}
			else if (ItemRichTextWidget.CurrentState == "Disabled")
			{
				ItemRichTextWidget.Brush = DisabledTextBrush;
			}
			_latestTextWidgetState = ItemRichTextWidget.CurrentState;
		}
	}

	private void SetLeaveTypeIcon(int type)
	{
		string text = string.Empty;
		switch (type)
		{
		case 0:
			text = "Default";
			break;
		case 1:
			text = "Mission";
			break;
		case 2:
			text = "SubMenu";
			break;
		case 3:
			text = "BribeAndEscape";
			break;
		case 4:
			text = "Escape";
			break;
		case 5:
			text = "Craft";
			break;
		case 6:
			text = "ForceToGiveGoods";
			break;
		case 7:
			text = "ForceToGiveTroops";
			break;
		case 8:
			text = "Bribe";
			break;
		case 9:
			text = "LeaveTroopsAndFlee";
			break;
		case 10:
			text = "OrderTroopsToAttack";
			break;
		case 11:
			text = "Raid";
			break;
		case 12:
			text = "HostileAction";
			break;
		case 13:
			text = "Recruit";
			break;
		case 14:
			text = "Trade";
			break;
		case 15:
			text = "Wait";
			break;
		case 16:
			text = "Leave";
			break;
		case 17:
			text = "Continue";
			break;
		case 18:
			text = "Manage";
			break;
		case 19:
			text = "ManageHideoutTroops";
			break;
		case 20:
			text = "WaitQuest";
			break;
		case 21:
			text = "Surrender";
			break;
		case 22:
			text = "Conversation";
			break;
		case 23:
			text = "DefendAction";
			break;
		case 24:
			text = "Devastate";
			break;
		case 25:
			text = "Pillage";
			break;
		case 26:
			text = "ShowMercy";
			break;
		case 27:
			text = "Leaderboard";
			break;
		case 28:
			text = "OpenStash";
			break;
		case 29:
			text = "ManageGarrison";
			break;
		case 30:
			text = "StagePrisonBreak";
			break;
		case 31:
			text = "ManagePrisoners";
			break;
		case 32:
			text = "Ransom";
			break;
		case 33:
			text = "PracticeFight";
			break;
		case 34:
			text = "BesiegeTown";
			break;
		case 35:
			text = "SneakIn";
			break;
		case 36:
			text = "LeadAssault";
			break;
		case 37:
			text = "DonateTroops";
			break;
		case 38:
			text = "DonatePrisoners";
			break;
		case 39:
			text = "SiegeAmbush";
			break;
		case 40:
			text = "Warehouse";
			break;
		}
		if (!string.IsNullOrEmpty(text) && type != 0)
		{
			LeaveTypeIcon.SetState(text);
			LeaveTypeIcon.IsVisible = true;
		}
	}

	private void SetLeaveTypeSound()
	{
		AudioProperty audioProperty = ParentButton?.Brush.SoundProperties.GetEventAudioProperty("Click");
		if (audioProperty == null)
		{
			return;
		}
		audioProperty.AudioName = "default";
		switch (LeaveType)
		{
		case 1:
			if (GameMenuStringId == "menu_siege_strategies")
			{
				audioProperty.AudioName = "panels/siege/sally_out";
			}
			break;
		case 9:
			if (GameMenuStringId == "encounter" || GameMenuStringId == "encounter_interrupted_siege_preparations" || GameMenuStringId == "menu_siege_strategies")
			{
				audioProperty.AudioName = "panels/battle/retreat";
			}
			break;
		case 12:
			if (GameMenuStringId == "encounter")
			{
				if (BattleSize < 50)
				{
					audioProperty.AudioName = "panels/battle/attack_small";
				}
				else if (BattleSize < 100)
				{
					audioProperty.AudioName = "panels/battle/attack_medium";
				}
				else
				{
					audioProperty.AudioName = "panels/battle/attack_large";
				}
			}
			break;
		case 21:
			if (GameMenuStringId == "encounter")
			{
				audioProperty.AudioName = "panels/battle/retreat";
			}
			break;
		case 24:
		case 25:
			audioProperty.AudioName = "panels/siege/raid";
			break;
		case 34:
			audioProperty.AudioName = "panels/siege/besiege";
			break;
		case 36:
			audioProperty.AudioName = "panels/siege/lead_assault";
			break;
		}
	}

	private void SetProgressIconType(int type, Widget progressWidget)
	{
		string empty = string.Empty;
		empty = type switch
		{
			0 => "Default", 
			1 => "Available", 
			2 => "Active", 
			3 => "Completed", 
			_ => "", 
		};
		if (progressWidget == QuestIconWidget)
		{
			QuestIconWidget.Brush = (IsMainStoryQuest ? MainStoryQuestBrush : NormalQuestBrush);
		}
		if (!string.IsNullOrEmpty(empty) && type != 0)
		{
			progressWidget.SetState(empty);
			progressWidget.IsVisible = true;
		}
	}

	private void ParentButton_PropertyChanged(PropertyOwnerObject widget, string propertyName, bool propertyValue)
	{
		if (propertyName == "IsDisabled" || propertyName == "IsHighlightEnabled")
		{
			OnOptionStateChanged?.Invoke();
		}
	}
}
