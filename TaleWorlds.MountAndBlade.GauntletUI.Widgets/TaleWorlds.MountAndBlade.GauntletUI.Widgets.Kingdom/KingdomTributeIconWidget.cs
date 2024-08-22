using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Kingdom;

public class KingdomTributeIconWidget : Widget
{
	private Widget _payIcon;

	private Widget _receiveIcon;

	public int Tribute
	{
		set
		{
			UpdateIcons(value);
		}
	}

	public Widget PayIcon
	{
		get
		{
			return _payIcon;
		}
		set
		{
			if (value != _payIcon)
			{
				_payIcon = value;
			}
		}
	}

	public Widget ReceiveIcon
	{
		get
		{
			return _receiveIcon;
		}
		set
		{
			if (value != _receiveIcon)
			{
				_receiveIcon = value;
			}
		}
	}

	public KingdomTributeIconWidget(UIContext context)
		: base(context)
	{
	}

	public void UpdateIcons(int tribute)
	{
		if (PayIcon != null && ReceiveIcon != null)
		{
			PayIcon.IsVisible = tribute > 0;
			ReceiveIcon.IsVisible = tribute < 0;
		}
	}
}
