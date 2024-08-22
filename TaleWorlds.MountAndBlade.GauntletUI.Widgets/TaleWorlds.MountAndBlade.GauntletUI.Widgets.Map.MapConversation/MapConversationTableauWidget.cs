using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Map.MapConversation;

public class MapConversationTableauWidget : TextureWidget
{
	private object _data;

	private bool _isTableauEnabled;

	[Editor(false)]
	public object Data
	{
		get
		{
			return _data;
		}
		set
		{
			if (value != _data)
			{
				_data = value;
				OnPropertyChanged(value, "Data");
				SetTextureProviderProperty("Data", value);
			}
		}
	}

	[Editor(false)]
	public bool IsTableauEnabled
	{
		get
		{
			return _isTableauEnabled;
		}
		set
		{
			if (value != _isTableauEnabled)
			{
				_isTableauEnabled = value;
				OnPropertyChanged(value, "IsTableauEnabled");
				SetTextureProviderProperty("IsEnabled", value);
				if (_isTableauEnabled)
				{
					_isRenderRequestedPreviousFrame = true;
				}
			}
		}
	}

	public MapConversationTableauWidget(UIContext context)
		: base(context)
	{
		base.TextureProviderName = "MapConversationTextureProvider";
		_isRenderRequestedPreviousFrame = false;
		UpdateTextureWidget();
		base.EventManager.AddAfterFinalizedCallback(OnEventManagerIsFinalized);
	}

	private void OnEventManagerIsFinalized()
	{
		if (!_setForClearNextFrame)
		{
			base.TextureProvider?.Clear(clearNextFrame: false);
		}
	}

	protected override void OnDisconnectedFromRoot()
	{
	}
}
