using System;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;

public class CharacterCreationOptionVM : StringItemWithActionVM
{
	private bool _isSelected;

	[DataSourceProperty]
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
				OnPropertyChangedWithValue(value, "IsSelected");
				ExecuteAction();
			}
		}
	}

	public CharacterCreationOptionVM(Action<object> onExecute, string item, object identifier)
		: base(onExecute, item, identifier)
	{
	}
}
