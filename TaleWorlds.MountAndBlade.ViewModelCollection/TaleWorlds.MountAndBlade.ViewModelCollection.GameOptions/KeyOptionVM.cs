using System;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

public abstract class KeyOptionVM : ViewModel
{
	protected readonly string _groupId;

	protected readonly string _id;

	protected readonly Action<KeyOptionVM> _onKeybindRequest;

	private string _optionValueText;

	private string _name;

	private string _description;

	public Key CurrentKey { get; protected set; }

	public Key Key { get; protected set; }

	[DataSourceProperty]
	public string OptionValueText
	{
		get
		{
			return _optionValueText;
		}
		set
		{
			if (value != _optionValueText)
			{
				_optionValueText = value;
				OnPropertyChangedWithValue(value, "OptionValueText");
			}
		}
	}

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				OnPropertyChangedWithValue(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != _description)
			{
				_description = value;
				OnPropertyChangedWithValue(value, "Description");
			}
		}
	}

	public KeyOptionVM(string groupId, string id, Action<KeyOptionVM> onKeybindRequest)
	{
		_groupId = groupId;
		_id = id;
		_onKeybindRequest = onKeybindRequest;
	}

	public abstract void Set(InputKey newKey);

	public abstract void Update();

	public abstract void OnDone();

	internal abstract bool IsChanged();
}
