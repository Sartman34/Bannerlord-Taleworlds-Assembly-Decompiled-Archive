using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;

public class KingdomDiplomacyFactionItemVM : ViewModel
{
	private HintViewModel _hint;

	private ImageIdentifierVM _visual;

	[DataSourceProperty]
	public HintViewModel Hint
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
	public ImageIdentifierVM Visual
	{
		get
		{
			return _visual;
		}
		set
		{
			if (value != _visual)
			{
				_visual = value;
				OnPropertyChangedWithValue(value, "Visual");
			}
		}
	}

	public KingdomDiplomacyFactionItemVM(IFaction faction)
	{
		Hint = new HintViewModel(faction.Name);
		Visual = new ImageIdentifierVM(BannerCode.CreateFrom(faction.Banner), nineGrid: true);
	}
}
