using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;

public class GameMenuItemProgressVM : ViewModel
{
	private readonly MenuContext _context;

	private readonly GameMenuManager _gameMenuManager;

	private readonly int _virtualIndex;

	private string _text1 = "";

	private string _text2 = "";

	private string _text;

	private string _progressText;

	private float _progress;

	[DataSourceProperty]
	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (value != _text)
			{
				_text = value;
				OnPropertyChangedWithValue(value, "Text");
			}
		}
	}

	[DataSourceProperty]
	public string ProgressText
	{
		get
		{
			return _progressText;
		}
		set
		{
			if (value != _progressText)
			{
				_progressText = value;
				OnPropertyChangedWithValue(value, "ProgressText");
			}
		}
	}

	[DataSourceProperty]
	public float Progress
	{
		get
		{
			return _progress;
		}
		set
		{
			if (value != _progress)
			{
				_progress = value;
				OnPropertyChangedWithValue(value, "Progress");
			}
		}
	}

	public GameMenuItemProgressVM(MenuContext context, int virtualIndex)
	{
		_context = context;
		_gameMenuManager = Campaign.Current.GameMenuManager;
		_virtualIndex = virtualIndex;
		_text1 = Campaign.Current.GameMenuManager.GetVirtualMenuOptionText(_context, _virtualIndex).ToString();
		_text2 = Campaign.Current.GameMenuManager.GetVirtualMenuOptionText2(_context, _virtualIndex).ToString();
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		Refresh();
	}

	private void Refresh()
	{
		float num = 0f;
		switch (_gameMenuManager.GetVirtualMenuAndOptionType(_context))
		{
		case TaleWorlds.CampaignSystem.GameMenus.GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption:
			ProgressText = "";
			break;
		case TaleWorlds.CampaignSystem.GameMenus.GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption:
		{
			float virtualMenuTargetWaitHours = Campaign.Current.GameMenuManager.GetVirtualMenuTargetWaitHours(_context);
			if (virtualMenuTargetWaitHours > 1f)
			{
				GameTexts.SetVariable("PLURAL_HOURS", 1);
			}
			else
			{
				GameTexts.SetVariable("PLURAL_HOURS", 0);
			}
			GameTexts.SetVariable("HOUR", MathF.Round(virtualMenuTargetWaitHours).ToString("0.0"));
			ProgressText = GameTexts.FindText("str_hours").ToString();
			break;
		}
		default:
			Debug.FailedAssert("Shouldn't create game menu progress for normal options", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\GameMenu\\GameMenuItemProgressVM.cs", "Refresh", 62);
			return;
		}
		bool virtualMenuIsWaitActive = Campaign.Current.GameMenuManager.GetVirtualMenuIsWaitActive(_context);
		Text = (virtualMenuIsWaitActive ? _text2 : _text1);
		num = Campaign.Current.GameMenuManager.GetVirtualMenuProgress(_context);
		Progress = MathF.Round(num * 100f);
	}

	public void OnTick()
	{
		Refresh();
	}
}
