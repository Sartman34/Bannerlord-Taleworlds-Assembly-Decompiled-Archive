using System.Collections.Generic;
using System.Xml;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.PrefabSystem;

namespace TaleWorlds.GauntletUI.TooltipExtensions;

public class PrefabTooltipExtension : PrefabExtension
{
	private static WidgetAttributeTemplate GetHint(WidgetTemplate widgetTemplate)
	{
		return widgetTemplate.GetFirstAttributeIfExist<WidgetAttributeKeyTypeHint>();
	}

	protected override void RegisterAttributeTypes(WidgetAttributeContext widgetAttributeContext)
	{
		WidgetAttributeKeyTypeHint keyType = new WidgetAttributeKeyTypeHint();
		widgetAttributeContext.RegisterKeyType(keyType);
	}

	protected override void OnWidgetCreated(WidgetCreationData widgetCreationData, WidgetInstantiationResult widgetInstantiationResult, int childCount)
	{
	}

	protected override void OnSave(PrefabExtensionContext prefabExtensionContext, XmlNode node, WidgetTemplate widgetTemplate)
	{
	}

	protected override void OnAttributesSet(WidgetCreationData widgetCreationData, WidgetInstantiationResult widgetInstantiationResult, Dictionary<string, WidgetAttributeTemplate> parameters)
	{
	}

	protected override void DoLoading(PrefabExtensionContext prefabExtensionContext, WidgetAttributeContext widgetAttributeContext, WidgetTemplate template, XmlNode node)
	{
	}

	protected override void OnLoadingFinished(WidgetPrefab widgetPrefab)
	{
	}

	protected override void AfterAttributesSet(WidgetCreationData widgetCreationData, WidgetInstantiationResult widgetInstantiationResult, Dictionary<string, WidgetAttributeTemplate> parameters)
	{
		WidgetAttributeTemplate hint = GetHint(widgetInstantiationResult.Template);
		if (hint != null)
		{
			GauntletMovie extensionData = widgetCreationData.GetExtensionData<GauntletMovie>();
			UIContext context = widgetCreationData.Context;
			Widget widget = widgetInstantiationResult.Widget;
			WidgetFactory widgetFactory = widgetCreationData.WidgetFactory;
			WidgetPrefab customType = widgetFactory.GetCustomType("Hint");
			TooltipExtensionWidget tooltipExtensionWidget = new TooltipExtensionWidget(context);
			widget.AddChild(tooltipExtensionWidget);
			if (extensionData != null)
			{
				GauntletView component = widget.GetComponent<GauntletView>();
				tooltipExtensionWidget.WidthSizePolicy = SizePolicy.CoverChildren;
				tooltipExtensionWidget.HeightSizePolicy = SizePolicy.CoverChildren;
				tooltipExtensionWidget.IsEnabled = false;
				GauntletView gauntletView = new GauntletView(extensionData, component, tooltipExtensionWidget);
				component.AddChild(gauntletView);
				tooltipExtensionWidget.AddComponent(gauntletView);
			}
			WidgetCreationData widgetCreationData2 = new WidgetCreationData(context, widgetFactory, tooltipExtensionWidget);
			if (extensionData != null)
			{
				widgetCreationData2.AddExtensionData(extensionData);
			}
			Dictionary<string, WidgetAttributeTemplate> dictionary = new Dictionary<string, WidgetAttributeTemplate>();
			WidgetAttributeTemplate widgetAttributeTemplate = new WidgetAttributeTemplate();
			widgetAttributeTemplate.Key = "HintText";
			widgetAttributeTemplate.KeyType = new WidgetAttributeKeyTypeAttribute();
			widgetAttributeTemplate.Value = hint.Value;
			widgetAttributeTemplate.ValueType = hint.ValueType;
			dictionary.Add("HintText", widgetAttributeTemplate);
			customType.Instantiate(widgetCreationData2, dictionary);
		}
		foreach (WidgetInstantiationResult child in widgetInstantiationResult.Children)
		{
			AfterAttributesSet(widgetCreationData, child, parameters);
		}
	}
}
