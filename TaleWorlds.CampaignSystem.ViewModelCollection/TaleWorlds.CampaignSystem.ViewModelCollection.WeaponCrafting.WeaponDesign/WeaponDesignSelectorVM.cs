using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;

public class WeaponDesignSelectorVM : ViewModel
{
	private Action<WeaponDesignSelectorVM> _onSelection;

	private ItemObject _generatedVisualItem;

	private bool _isSelected;

	private string _name;

	private string _weaponTypeCode;

	private ImageIdentifierVM _visual;

	private BasicTooltipViewModel _hint;

	public TaleWorlds.Core.WeaponDesign Design { get; }

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
	public string WeaponTypeCode
	{
		get
		{
			return _weaponTypeCode;
		}
		set
		{
			if (value != _weaponTypeCode)
			{
				_weaponTypeCode = value;
				OnPropertyChangedWithValue(value, "WeaponTypeCode");
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

	[DataSourceProperty]
	public BasicTooltipViewModel Hint
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

	public WeaponDesignSelectorVM(TaleWorlds.Core.WeaponDesign design, Action<WeaponDesignSelectorVM> onSelection)
	{
		_onSelection = onSelection;
		Design = design;
		TextObject textObject = new TextObject("{=uZhHh7pm}Crafted {CURR_TEMPLATE_NAME}");
		textObject.SetTextVariable("CURR_TEMPLATE_NAME", design.Template.TemplateName);
		TextObject textObject2 = design.WeaponName ?? textObject;
		Name = textObject2.ToString();
		_generatedVisualItem = new ItemObject();
		Crafting.GenerateItem(design, textObject2, Hero.MainHero.Culture, design.Template.ItemModifierGroup, ref _generatedVisualItem);
		MBObjectManager.Instance.RegisterObject(_generatedVisualItem);
		Visual = new ImageIdentifierVM(_generatedVisualItem);
		WeaponTypeCode = design.Template.StringId;
		Hint = new BasicTooltipViewModel(() => GetHint());
	}

	private List<TooltipProperty> GetHint()
	{
		List<TooltipProperty> list = new List<TooltipProperty>();
		list.Add(new TooltipProperty("", _generatedVisualItem.Name.ToString(), 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.Title));
		foreach (CraftingStatData item in Crafting.GetStatDatasFromTemplate(0, _generatedVisualItem, Design.Template))
		{
			if (item.IsValid && item.CurValue > 0f && item.MaxValue > 0f)
			{
				list.Add(new TooltipProperty(item.DescriptionText.ToString(), item.CurValue.ToString(), 0));
			}
		}
		return list;
	}

	public void ExecuteSelect()
	{
		_onSelection?.Invoke(this);
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		MBObjectManager.Instance.UnregisterObject(_generatedVisualItem);
	}
}
