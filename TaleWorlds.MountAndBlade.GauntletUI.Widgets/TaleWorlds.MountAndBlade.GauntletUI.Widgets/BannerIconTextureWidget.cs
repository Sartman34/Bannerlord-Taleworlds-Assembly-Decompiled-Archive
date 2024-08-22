using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class BannerIconTextureWidget : TextureWidget
{
	private string _code;

	[Editor(false)]
	public string Code
	{
		get
		{
			return _code;
		}
		set
		{
			if (_code != value)
			{
				_code = value;
				OnPropertyChanged(value, "Code");
				SetTextureProviderProperty("Code", value);
			}
		}
	}

	public BannerIconTextureWidget(UIContext context)
		: base(context)
	{
		base.TextureProviderName = "GauntletBannerTableauProvider";
	}
}
