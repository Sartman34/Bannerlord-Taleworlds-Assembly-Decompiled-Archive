using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.Core;

public class GameTextManager
{
	public struct ChoiceTag
	{
		public string TagName { get; private set; }

		public uint Weight { get; private set; }

		public bool IsTagReversed { get; private set; }

		public ChoiceTag(string tagName, int weight)
		{
			this = default(ChoiceTag);
			TagName = tagName;
			Weight = (uint)TaleWorlds.Library.MathF.Abs(weight);
			IsTagReversed = weight < 0;
		}
	}

	private readonly Dictionary<string, GameText> _gameTexts;

	public GameTextManager()
	{
		_gameTexts = new Dictionary<string, GameText>();
	}

	public GameText GetGameText(string id)
	{
		if (_gameTexts.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public GameText AddGameText(string id)
	{
		if (!_gameTexts.TryGetValue(id, out var value))
		{
			value = new GameText(id);
			_gameTexts.Add(value.Id, value);
		}
		return value;
	}

	public bool TryGetText(string id, string variation, out TextObject text)
	{
		text = null;
		_gameTexts.TryGetValue(id, out var value);
		if (value != null)
		{
			if (variation == null)
			{
				text = value.DefaultText;
			}
			else
			{
				text = value.GetVariation(variation);
			}
			if (text != null)
			{
				text = text.CopyTextObject();
				text.AddIDToValue(id);
				return true;
			}
		}
		return false;
	}

	public TextObject FindText(string id, string variation = null)
	{
		if (TryGetText(id, variation, out var text))
		{
			return text;
		}
		if (variation == null)
		{
			return new TextObject("{=!}ERROR: Text with id " + id + " doesn't exist!");
		}
		return new TextObject("{=!}ERROR: Text with id " + id + " doesn't exist! Variation: " + variation);
	}

	public IEnumerable<TextObject> FindAllTextVariations(string id)
	{
		_gameTexts.TryGetValue(id, out var value);
		if (value == null)
		{
			yield break;
		}
		foreach (GameText.GameTextVariation variation in value.Variations)
		{
			yield return variation.Text;
		}
	}

	public void LoadGameTexts()
	{
		Game current = Game.Current;
		bool ignoreGameTypeInclusionCheck = false;
		string gameType = "";
		if (current != null)
		{
			ignoreGameTypeInclusionCheck = current.GameType.IsDevelopment;
			gameType = current.GameType.GetType().Name;
		}
		XmlDocument mergedXmlForManaged = MBObjectManager.GetMergedXmlForManaged("GameText", skipValidation: false, ignoreGameTypeInclusionCheck, gameType);
		try
		{
			LoadFromXML(mergedXmlForManaged);
		}
		catch (Exception)
		{
		}
	}

	public void LoadDefaultTexts()
	{
		string text = ModuleHelper.GetModuleFullPath("Native") + "ModuleData/global_strings.xml";
		Debug.Print("opening " + text);
		XmlDocument xmlDocument = new XmlDocument();
		StreamReader streamReader = new StreamReader(text);
		string xml = streamReader.ReadToEnd();
		xmlDocument.LoadXml(xml);
		streamReader.Close();
		LoadFromXML(xmlDocument);
	}

	private void LoadFromXML(XmlDocument doc)
	{
		XmlNode xmlNode = null;
		for (int i = 0; i < doc.ChildNodes.Count; i++)
		{
			XmlNode xmlNode2 = doc.ChildNodes[i];
			if (xmlNode2.NodeType != XmlNodeType.Comment && xmlNode2.Name == "strings" && xmlNode2.ChildNodes.Count > 0)
			{
				xmlNode = xmlNode2.ChildNodes[0];
				break;
			}
		}
		while (xmlNode != null)
		{
			if (xmlNode.Name == "string" && xmlNode.NodeType != XmlNodeType.Comment)
			{
				if (xmlNode.Attributes == null)
				{
					throw new TWXmlLoadException("Node attributes are null.");
				}
				string[] array = xmlNode.Attributes["id"].Value.Split(new char[1] { '.' });
				string id = array[0];
				GameText gameText = AddGameText(id);
				string variationId = "";
				if (array.Length > 1)
				{
					variationId = array[1];
				}
				TextObject textObject = new TextObject(xmlNode.Attributes["text"].Value);
				List<ChoiceTag> list = new List<ChoiceTag>();
				foreach (XmlNode childNode in xmlNode.ChildNodes)
				{
					if (!(childNode.Name == "tags"))
					{
						continue;
					}
					XmlNodeList childNodes = childNode.ChildNodes;
					for (int j = 0; j < childNodes.Count; j++)
					{
						XmlAttributeCollection attributes = childNodes[j].Attributes;
						if (attributes != null)
						{
							int result = 1;
							if (attributes["weight"] != null)
							{
								int.TryParse(attributes["weight"].Value, out result);
							}
							ChoiceTag item = new ChoiceTag(attributes["tag_name"].Value, result);
							list.Add(item);
						}
					}
				}
				textObject.CacheTokens();
				gameText.AddVariationWithId(variationId, textObject, list);
			}
			xmlNode = xmlNode.NextSibling;
		}
	}
}
