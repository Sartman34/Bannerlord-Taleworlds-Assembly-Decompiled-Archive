using System.Collections.Generic;
using System.Xml;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.PrefabSystem;

public class VisualDefinitionTemplate
{
	public string Name { get; set; }

	public float TransitionDuration { get; set; }

	public float DelayOnBegin { get; set; }

	public bool EaseIn { get; set; }

	public Dictionary<string, VisualStateTemplate> VisualStates { get; private set; }

	public VisualDefinitionTemplate()
	{
		VisualStates = new Dictionary<string, VisualStateTemplate>();
		TransitionDuration = 0.2f;
	}

	public void AddVisualState(VisualStateTemplate visualState)
	{
		VisualStates.Add(visualState.State, visualState);
	}

	public VisualDefinition CreateVisualDefinition(BrushFactory brushFactory, SpriteData spriteData, Dictionary<string, VisualDefinitionTemplate> visualDefinitionTemplates, Dictionary<string, ConstantDefinition> constants, Dictionary<string, WidgetAttributeTemplate> parameters, Dictionary<string, string> defaultParameters)
	{
		VisualDefinition visualDefinition = new VisualDefinition(Name, TransitionDuration, DelayOnBegin, EaseIn);
		foreach (VisualStateTemplate value in VisualStates.Values)
		{
			VisualState visualState = value.CreateVisualState(brushFactory, spriteData, visualDefinitionTemplates, constants, parameters, defaultParameters);
			visualDefinition.AddVisualState(visualState);
		}
		return visualDefinition;
	}

	internal void Save(XmlNode rootNode)
	{
		XmlDocument ownerDocument = rootNode.OwnerDocument;
		XmlNode xmlNode = ownerDocument.CreateElement("VisualDefinition");
		XmlAttribute xmlAttribute = ownerDocument.CreateAttribute("Name");
		xmlAttribute.InnerText = Name;
		xmlNode.Attributes.Append(xmlAttribute);
		XmlAttribute xmlAttribute2 = ownerDocument.CreateAttribute("TransitionDuration");
		xmlAttribute2.InnerText = TransitionDuration.ToString();
		xmlNode.Attributes.Append(xmlAttribute2);
		XmlAttribute xmlAttribute3 = ownerDocument.CreateAttribute("DelayOnBegin");
		xmlAttribute3.InnerText = DelayOnBegin.ToString();
		xmlNode.Attributes.Append(xmlAttribute3);
		XmlAttribute xmlAttribute4 = ownerDocument.CreateAttribute("EaseIn");
		xmlAttribute4.InnerText = EaseIn.ToString();
		xmlNode.Attributes.Append(xmlAttribute4);
		foreach (VisualStateTemplate value in VisualStates.Values)
		{
			XmlNode xmlNode2 = ownerDocument.CreateElement("VisualState");
			XmlAttribute xmlAttribute5 = ownerDocument.CreateAttribute("State");
			xmlAttribute5.InnerText = value.State;
			xmlNode2.Attributes.Append(xmlAttribute5);
			foreach (KeyValuePair<string, string> attribute in value.GetAttributes())
			{
				XmlAttribute xmlAttribute6 = ownerDocument.CreateAttribute(attribute.Key);
				xmlAttribute6.InnerText = attribute.Value;
				xmlNode2.Attributes.Append(xmlAttribute6);
			}
			xmlNode.AppendChild(xmlNode2);
		}
		rootNode.AppendChild(xmlNode);
	}
}
