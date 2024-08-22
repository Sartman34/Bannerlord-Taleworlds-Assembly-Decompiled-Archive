using System;
using System.Collections.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace TaleWorlds.Core.ViewModelCollection;

public class BattleResultVM : ViewModel
{
	private string _text;

	private BasicTooltipViewModel _hint;

	private ImageIdentifierVM _deadLordPortrait;

	private ImageIdentifierVM _deadLordClanBanner;

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
	public BasicTooltipViewModel Hint
	{
		get
		{
			return _hint;
		}
		set
		{
			if (value != _hint)
			{
				_hint = value;
				OnPropertyChangedWithValue(value, "Hint");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM DeadLordPortrait
	{
		get
		{
			return _deadLordPortrait;
		}
		set
		{
			if (value != _deadLordPortrait)
			{
				_deadLordPortrait = value;
				OnPropertyChangedWithValue(value, "DeadLordPortrait");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM DeadLordClanBanner
	{
		get
		{
			return _deadLordClanBanner;
		}
		set
		{
			if (value != _deadLordClanBanner)
			{
				_deadLordClanBanner = value;
				OnPropertyChangedWithValue(value, "DeadLordClanBanner");
			}
		}
	}

	public BattleResultVM(string text, Func<List<TooltipProperty>> propertyFunc, CharacterCode deadHeroCode = null)
	{
		Text = text;
		Hint = new BasicTooltipViewModel(propertyFunc);
		if (deadHeroCode != null)
		{
			DeadLordPortrait = new ImageIdentifierVM(deadHeroCode);
			DeadLordClanBanner = new ImageIdentifierVM(BannerCode.CreateFrom(deadHeroCode.Banner), nineGrid: true);
		}
		else
		{
			DeadLordPortrait = null;
			DeadLordClanBanner = null;
		}
	}
}
