using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;

public class GameMenuPlunderItemVM : ViewModel
{
	private ItemRosterElement _item;

	private ImageIdentifierVM _visual;

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

	public GameMenuPlunderItemVM(ItemRosterElement item)
	{
		_item = item;
		Visual = new ImageIdentifierVM(item.EquipmentElement.Item);
	}

	public void ExecuteBeginTooltip()
	{
		if (_item.EquipmentElement.Item != null)
		{
			InformationManager.ShowTooltip(typeof(ItemObject), _item.EquipmentElement);
		}
	}

	public void ExecuteEndTooltip()
	{
		MBInformationManager.HideInformations();
	}
}
