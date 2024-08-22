using System.Collections.Generic;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameMenus;

public class GameMenu
{
	public enum MenuFlags
	{
		None,
		AutoSelectFirst
	}

	public enum MenuAndOptionType
	{
		RegularMenuOption,
		WaitMenuShowProgressAndHoursOption,
		WaitMenuShowOnlyProgressOption,
		WaitMenuHideProgressAndHoursOption
	}

	private TextObject _defaultText;

	public OnInitDelegate OnInit;

	public List<object> MenuRepeatObjects = new List<object>();

	public object LastSelectedMenuObject;

	private CampaignTime _previousTickTime;

	private readonly List<GameMenuOption> _menuItems;

	public MenuAndOptionType Type { get; private set; }

	public string StringId { get; private set; }

	public object RelatedObject { get; private set; }

	public TextObject MenuTitle { get; private set; }

	public GameOverlays.MenuOverlayType OverlayType { get; private set; }

	public bool IsReady { get; private set; }

	public int MenuItemAmount => _menuItems.Count;

	public object CurrentRepeatableObject
	{
		get
		{
			if (MenuRepeatObjects.Count <= CurrentRepeatableIndex)
			{
				return null;
			}
			return MenuRepeatObjects[CurrentRepeatableIndex];
		}
	}

	public bool IsWaitMenu { get; private set; }

	public bool IsWaitActive { get; private set; }

	public bool IsEmpty
	{
		get
		{
			if (MenuRepeatObjects.Count == 0)
			{
				return MenuItemAmount == 0;
			}
			return false;
		}
	}

	public float Progress { get; private set; }

	public float TargetWaitHours { get; private set; }

	public OnTickDelegate OnTick { get; private set; }

	public OnConditionDelegate OnCondition { get; private set; }

	public OnConsequenceDelegate OnConsequence { get; private set; }

	public int CurrentRepeatableIndex { get; set; }

	public IEnumerable<GameMenuOption> MenuOptions => _menuItems;

	public bool AutoSelectFirst { get; private set; }

	internal GameMenu(string idString)
	{
		StringId = idString;
		_menuItems = new List<GameMenuOption>();
	}

	internal void Initialize(TextObject text, OnInitDelegate initDelegate, GameOverlays.MenuOverlayType overlay, MenuFlags flags = MenuFlags.None, object relatedObject = null)
	{
		CurrentRepeatableIndex = 0;
		LastSelectedMenuObject = null;
		_defaultText = text;
		OnInit = initDelegate;
		OverlayType = overlay;
		AutoSelectFirst = (flags & MenuFlags.AutoSelectFirst) != 0;
		RelatedObject = relatedObject;
		IsReady = true;
	}

	internal void Initialize(TextObject text, OnInitDelegate initDelegate, OnConditionDelegate condition, OnConsequenceDelegate consequence, OnTickDelegate tick, MenuAndOptionType type, GameOverlays.MenuOverlayType overlay, float targetWaitHours = 0f, MenuFlags flags = MenuFlags.None, object relatedObject = null)
	{
		CurrentRepeatableIndex = 0;
		LastSelectedMenuObject = null;
		_defaultText = text;
		OnInit = initDelegate;
		OverlayType = overlay;
		AutoSelectFirst = (flags & MenuFlags.AutoSelectFirst) != 0;
		RelatedObject = relatedObject;
		OnConsequence = consequence;
		OnCondition = condition;
		Type = type;
		OnTick = tick;
		TargetWaitHours = targetWaitHours;
		IsWaitMenu = type != MenuAndOptionType.RegularMenuOption;
		IsReady = true;
	}

	private void AddOption(GameMenuOption newOption, int index = -1)
	{
		if (index >= 0 && _menuItems.Count >= index)
		{
			_menuItems.Insert(index, newOption);
		}
		else
		{
			_menuItems.Add(newOption);
		}
	}

	public bool GetMenuOptionConditionsHold(Game game, MenuContext menuContext, int menuItemNumber)
	{
		if (IsWaitMenu)
		{
			if (_menuItems[menuItemNumber].GetConditionsHold(game, menuContext))
			{
				return RunWaitMenuCondition(menuContext);
			}
			return false;
		}
		return _menuItems[menuItemNumber].GetConditionsHold(game, menuContext);
	}

	public TextObject GetMenuOptionText(int menuItemNumber)
	{
		return _menuItems[menuItemNumber].Text;
	}

	public GameMenuOption GetGameMenuOption(int menuItemNumber)
	{
		return _menuItems[menuItemNumber];
	}

	public TextObject GetMenuOptionText2(int menuItemNumber)
	{
		return _menuItems[menuItemNumber].Text2;
	}

	public string GetMenuOptionIdString(int menuItemNumber)
	{
		return _menuItems[menuItemNumber].IdString;
	}

	public TextObject GetMenuOptionTooltip(int menuItemNumber)
	{
		return _menuItems[menuItemNumber].Tooltip;
	}

	public bool GetMenuOptionIsLeave(int menuItemNumber)
	{
		return _menuItems[menuItemNumber].IsLeave;
	}

	public void SetProgressOfWaitingInMenu(float progress)
	{
		Progress = progress;
	}

	public void SetTargetedWaitingTimeAndInitialProgress(float targetedWaitingTime, float initialProgress)
	{
		TargetWaitHours = targetedWaitingTime;
		SetProgressOfWaitingInMenu(initialProgress);
	}

	public GameMenuOption GetLeaveMenuOption(Game game, MenuContext menuContext)
	{
		for (int i = 0; i < _menuItems.Count; i++)
		{
			if (_menuItems[i].IsLeave && _menuItems[i].IsEnabled && _menuItems[i].GetConditionsHold(game, menuContext))
			{
				return _menuItems[i];
			}
		}
		return null;
	}

	public void RunOnTick(MenuContext menuContext, float dt)
	{
		if (IsWaitMenu && IsWaitActive)
		{
			if (OnTick != null)
			{
				MenuCallbackArgs args = new MenuCallbackArgs(menuContext, MenuTitle);
				OnTick(args, CampaignTime.Now - _previousTickTime);
				_previousTickTime = CampaignTime.Now;
			}
			if (Progress >= 1f)
			{
				EndWait();
				RunWaitMenuConsequence(menuContext);
			}
		}
	}

	public bool RunWaitMenuCondition(MenuContext menuContext)
	{
		if (OnCondition != null)
		{
			MenuCallbackArgs args = new MenuCallbackArgs(menuContext, MenuTitle);
			bool num = OnCondition(args);
			if (num && !IsWaitActive)
			{
				menuContext.GameMenu.StartWait();
			}
			return num;
		}
		return true;
	}

	public void RunWaitMenuConsequence(MenuContext menuContext)
	{
		if (OnConsequence != null)
		{
			MenuCallbackArgs args = new MenuCallbackArgs(menuContext, MenuTitle);
			OnConsequence(args);
		}
	}

	public void RunMenuOptionConsequence(MenuContext menuContext, int menuItemNumber)
	{
		if (menuItemNumber >= _menuItems.Count)
		{
			menuItemNumber = _menuItems.Count - 1;
		}
		if (_menuItems[menuItemNumber].IsLeave && IsWaitMenu)
		{
			EndWait();
		}
		_menuItems[menuItemNumber].RunConsequence(menuContext);
	}

	public void StartWait()
	{
		_previousTickTime = CampaignTime.Now;
		IsWaitActive = true;
		Campaign.Current.TimeControlMode = CampaignTimeControlMode.UnstoppableFastForward;
	}

	public void EndWait()
	{
		IsWaitActive = false;
		Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
	}

	public void RunOnInit(Game game, MenuContext menuContext)
	{
		MenuCallbackArgs menuCallbackArgs = new MenuCallbackArgs(menuContext, MenuTitle);
		if (OnInit != null)
		{
			Debug.Print("[GAME MENU] " + menuContext.GameMenu.StringId);
			OnInit(menuCallbackArgs);
			MenuTitle = menuCallbackArgs.MenuTitle;
		}
		CampaignEventDispatcher.Instance.OnGameMenuOpened(menuCallbackArgs);
	}

	public void PreInit(MenuContext menuContext)
	{
		MenuCallbackArgs args = new MenuCallbackArgs(menuContext, MenuTitle);
		CampaignEventDispatcher.Instance.BeforeGameMenuOpened(args);
	}

	public void AfterInit(MenuContext menuContext)
	{
		MenuCallbackArgs args = new MenuCallbackArgs(menuContext, MenuTitle);
		CampaignEventDispatcher.Instance.AfterGameMenuOpened(args);
	}

	public TextObject GetText()
	{
		return _defaultText;
	}

	public static void ActivateGameMenu(string menuId)
	{
		Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
		if (Campaign.Current.CurrentMenuContext == null)
		{
			Campaign.Current.GameMenuManager.SetNextMenu(menuId);
			MapState mapState = Game.Current.GameStateManager.LastOrDefault<MapState>();
			mapState?.EnterMenuMode();
			if (mapState != null && mapState.MenuContext?.GameMenu?.IsWaitMenu == true)
			{
				mapState.MenuContext.GameMenu.StartWait();
			}
		}
		else
		{
			SwitchToMenu(menuId);
		}
	}

	public static void SwitchToMenu(string menuId)
	{
		Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
		MenuContext currentMenuContext = Campaign.Current.CurrentMenuContext;
		if (currentMenuContext != null)
		{
			currentMenuContext.SwitchToMenu(menuId);
			if (currentMenuContext.GameMenu.IsWaitMenu && Campaign.Current.TimeControlMode == CampaignTimeControlMode.Stop)
			{
				currentMenuContext.GameMenu.StartWait();
			}
			MenuContext currentMenuContext2 = Campaign.Current.CurrentMenuContext;
			if (currentMenuContext2 != null && currentMenuContext2.GameMenu.StringId == menuId)
			{
				currentMenuContext2.GameMenu.AfterInit(currentMenuContext2);
			}
		}
		else
		{
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\GameMenus\\GameMenu.cs", "SwitchToMenu", 363);
		}
	}

	public static void ExitToLast()
	{
		Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
		Campaign.Current.GameMenuManager.ExitToLast();
	}

	internal void AddOption(string optionId, TextObject optionText, GameMenuOption.OnConditionDelegate condition, GameMenuOption.OnConsequenceDelegate consequence, int index = -1, bool isLeave = false, bool isRepeatable = false, object relatedObject = null)
	{
		AddOption(new GameMenuOption(MenuAndOptionType.RegularMenuOption, optionId, optionText, optionText, condition, consequence, isLeave, isRepeatable, relatedObject), index);
	}

	internal void RemoveMenuOption(GameMenuOption option)
	{
		_menuItems.Remove(option);
	}
}
