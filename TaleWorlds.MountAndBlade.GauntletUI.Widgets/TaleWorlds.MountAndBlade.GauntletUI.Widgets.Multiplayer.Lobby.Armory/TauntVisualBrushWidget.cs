using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer.Lobby.Armory;

public class TauntVisualBrushWidget : BrushWidget
{
	private Brush _tauntIconsBrush;

	private string _tauntId;

	private bool _isSelected;

	public Brush TauntIconsBrush
	{
		get
		{
			return _tauntIconsBrush;
		}
		set
		{
			if (value != _tauntIconsBrush)
			{
				_tauntIconsBrush = value;
				OnPropertyChanged(value, "TauntIconsBrush");
			}
		}
	}

	public string TauntID
	{
		get
		{
			return _tauntId;
		}
		set
		{
			if (value != _tauntId)
			{
				_tauntId = value;
				OnPropertyChanged(value, "TauntID");
				UpdateTauntVisual();
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				OnPropertyChanged(value, "IsSelected");
				SetState(value ? "Selected" : "Default");
			}
		}
	}

	public TauntVisualBrushWidget(UIContext context)
		: base(context)
	{
	}

	private void UpdateTauntVisual()
	{
		Sprite sprite = TauntIconsBrush?.GetLayer(TauntID)?.Sprite;
		if (base.Brush == null)
		{
			return;
		}
		base.Brush.Sprite = sprite;
		foreach (BrushLayer layer in base.Brush.Layers)
		{
			layer.Sprite = sprite;
		}
	}
}
