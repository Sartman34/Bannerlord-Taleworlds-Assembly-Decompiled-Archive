using TaleWorlds.Library;

namespace TaleWorlds.Core.ViewModelCollection;

public class CharacterEquipmentItemVM : ViewModel
{
	private readonly ItemObject _item;

	private int _type;

	private bool _hasItem;

	[DataSourceProperty]
	public int Type
	{
		get
		{
			return _type;
		}
		set
		{
			if (value != _type)
			{
				_type = value;
				OnPropertyChangedWithValue(value, "Type");
			}
		}
	}

	[DataSourceProperty]
	public bool HasItem
	{
		get
		{
			return _hasItem;
		}
		set
		{
			if (value != _hasItem)
			{
				_hasItem = value;
				OnPropertyChangedWithValue(value, "HasItem");
			}
		}
	}

	public CharacterEquipmentItemVM(ItemObject item)
	{
		_item = item;
		if (_item == null)
		{
			HasItem = false;
			Type = 0;
		}
		else
		{
			HasItem = true;
			Type = (int)_item.Type;
		}
	}

	public virtual void ExecuteBeginHint()
	{
		InformationManager.ShowTooltip(typeof(ItemObject), new EquipmentElement(_item));
	}

	public virtual void ExecuteEndHint()
	{
		MBInformationManager.HideInformations();
	}
}
