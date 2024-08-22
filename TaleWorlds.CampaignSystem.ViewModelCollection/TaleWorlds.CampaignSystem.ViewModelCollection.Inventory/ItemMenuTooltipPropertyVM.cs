using System;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

public class ItemMenuTooltipPropertyVM : TooltipProperty
{
	private HintViewModel _propertyHint;

	[DataSourceProperty]
	public HintViewModel PropertyHint
	{
		get
		{
			return _propertyHint;
		}
		set
		{
			if (value != _propertyHint)
			{
				_propertyHint = value;
				OnPropertyChangedWithValue(value, "PropertyHint");
			}
		}
	}

	public ItemMenuTooltipPropertyVM()
	{
	}

	public ItemMenuTooltipPropertyVM(string definition, string value, int textHeight, bool onlyShowWhenExtended = false, HintViewModel propertyHint = null)
		: base(definition, value, textHeight, onlyShowWhenExtended)
	{
		PropertyHint = propertyHint;
	}

	public ItemMenuTooltipPropertyVM(string definition, Func<string> _valueFunc, int textHeight, bool onlyShowWhenExtended = false, HintViewModel propertyHint = null)
		: base(definition, _valueFunc, textHeight, onlyShowWhenExtended)
	{
		PropertyHint = propertyHint;
	}

	public ItemMenuTooltipPropertyVM(Func<string> _definitionFunc, Func<string> _valueFunc, int textHeight, bool onlyShowWhenExtended = false, HintViewModel propertyHint = null)
		: base(_definitionFunc, _valueFunc, textHeight, onlyShowWhenExtended)
	{
		PropertyHint = propertyHint;
	}

	public ItemMenuTooltipPropertyVM(Func<string> _definitionFunc, Func<string> _valueFunc, object[] valueArgs, int textHeight, bool onlyShowWhenExtended = false, HintViewModel propertyHint = null)
		: base(_definitionFunc, _valueFunc, valueArgs, textHeight, onlyShowWhenExtended)
	{
		PropertyHint = propertyHint;
	}

	public ItemMenuTooltipPropertyVM(string definition, string value, int textHeight, Color color, bool onlyShowWhenExtended = false, HintViewModel propertyHint = null, TooltipPropertyFlags propertyFlags = TooltipPropertyFlags.None)
		: base(definition, value, textHeight, color, onlyShowWhenExtended)
	{
		PropertyHint = propertyHint;
	}

	public ItemMenuTooltipPropertyVM(string definition, Func<string> _valueFunc, int textHeight, Color color, bool onlyShowWhenExtended = false, HintViewModel propertyHint = null)
		: base(definition, _valueFunc, textHeight, color, onlyShowWhenExtended)
	{
		PropertyHint = propertyHint;
	}

	public ItemMenuTooltipPropertyVM(Func<string> _definitionFunc, Func<string> _valueFunc, int textHeight, Color color, bool onlyShowWhenExtended = false, HintViewModel propertyHint = null)
		: base(_definitionFunc, _valueFunc, textHeight, color, onlyShowWhenExtended)
	{
		PropertyHint = propertyHint;
	}

	public ItemMenuTooltipPropertyVM(TooltipProperty property, HintViewModel propertyHint = null)
		: base(property)
	{
		PropertyHint = propertyHint;
	}
}
