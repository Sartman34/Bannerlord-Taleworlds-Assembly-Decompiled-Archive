using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class BoolStateChangerWidget : Widget
{
	private bool _booleanCheck;

	private string _trueState;

	private string _falseState;

	private Widget _targetWidget;

	private bool _includeChildren;

	[Editor(false)]
	public bool BooleanCheck
	{
		get
		{
			return _booleanCheck;
		}
		set
		{
			if (_booleanCheck != value)
			{
				_booleanCheck = value;
				OnPropertyChanged(value, "BooleanCheck");
				TriggerUpdated();
			}
		}
	}

	[Editor(false)]
	public string TrueState
	{
		get
		{
			return _trueState;
		}
		set
		{
			if (_trueState != value)
			{
				_trueState = value;
				OnPropertyChanged(value, "TrueState");
			}
		}
	}

	[Editor(false)]
	public string FalseState
	{
		get
		{
			return _falseState;
		}
		set
		{
			if (_falseState != value)
			{
				_falseState = value;
				OnPropertyChanged(value, "FalseState");
			}
		}
	}

	[Editor(false)]
	public Widget TargetWidget
	{
		get
		{
			return _targetWidget;
		}
		set
		{
			if (_targetWidget != value)
			{
				_targetWidget = value;
				OnPropertyChanged(value, "TargetWidget");
			}
		}
	}

	[Editor(false)]
	public bool IncludeChildren
	{
		get
		{
			return _includeChildren;
		}
		set
		{
			if (_includeChildren != value)
			{
				_includeChildren = value;
				OnPropertyChanged(value, "IncludeChildren");
			}
		}
	}

	public BoolStateChangerWidget(UIContext context)
		: base(context)
	{
	}

	private void AddState(Widget widget, string state, bool includeChildren)
	{
		widget.AddState(state);
		if (includeChildren)
		{
			for (int i = 0; i < widget.ChildCount; i++)
			{
				AddState(widget.GetChild(i), state, includeChildren: true);
			}
		}
	}

	private void SetState(Widget widget, string state, bool includeChildren)
	{
		widget.SetState(state);
		if (includeChildren)
		{
			for (int i = 0; i < widget.ChildCount; i++)
			{
				SetState(widget.GetChild(i), state, includeChildren: true);
			}
		}
	}

	private void TriggerUpdated()
	{
		string state = (BooleanCheck ? TrueState : FalseState);
		Widget widget = TargetWidget ?? this;
		AddState(widget, state, IncludeChildren);
		SetState(widget, state, IncludeChildren);
	}
}
