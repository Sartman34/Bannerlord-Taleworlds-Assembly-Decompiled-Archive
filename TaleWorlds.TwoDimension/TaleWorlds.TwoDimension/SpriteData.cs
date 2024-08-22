using System;
using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Library;

namespace TaleWorlds.TwoDimension;

public class SpriteData
{
	public Dictionary<string, SpritePart> SpritePartNames { get; private set; }

	public Dictionary<string, Sprite> SpriteNames { get; private set; }

	public Dictionary<string, SpriteCategory> SpriteCategories { get; private set; }

	public string Name { get; private set; }

	public SpriteData(string name)
	{
		Name = name;
		SpritePartNames = new Dictionary<string, SpritePart>();
		SpriteNames = new Dictionary<string, Sprite>();
		SpriteCategories = new Dictionary<string, SpriteCategory>();
	}

	public Sprite GetSprite(string name)
	{
		if (SpriteNames.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public bool SpriteExists(string spriteName)
	{
		return GetSprite(spriteName) != null;
	}

	private void LoadFromDepot(ResourceDepot resourceDepot)
	{
		XmlDocument spriteData = new XmlDocument();
		foreach (string item in resourceDepot.GetFilesEndingWith(Name + ".xml"))
		{
			try
			{
				LoadSpriteDataFromFile(spriteData, item);
			}
			catch (Exception)
			{
				Debug.FailedAssert("Failed to load sprite data from file: " + item, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.TwoDimension\\SpriteData.cs", "LoadFromDepot", 58);
			}
		}
	}

	private void LoadSpriteDataFromFile(XmlDocument spriteData, string filePath)
	{
		spriteData.Load(filePath);
		XmlElement xmlElement = spriteData["SpriteData"];
		XmlNode xmlNode = xmlElement["SpriteCategories"];
		XmlNode xmlNode2 = xmlElement["SpriteParts"];
		XmlNode xmlNode3 = xmlElement["Sprites"];
		foreach (XmlNode item in xmlNode)
		{
			string innerText = item["Name"].InnerText;
			int num = Convert.ToInt32(item["SpriteSheetCount"].InnerText);
			bool alwaysLoad = false;
			Vec2i[] array = new Vec2i[num];
			foreach (XmlNode childNode in item.ChildNodes)
			{
				if (childNode.Name == "SpriteSheetSize")
				{
					int num2 = Convert.ToInt32(childNode.Attributes["ID"].InnerText);
					int x = Convert.ToInt32(childNode.Attributes["Width"].InnerText);
					int y = Convert.ToInt32(childNode.Attributes["Height"].InnerText);
					array[num2 - 1] = new Vec2i(x, y);
				}
				else if (childNode.Name == "AlwaysLoad")
				{
					alwaysLoad = true;
				}
			}
			SpriteCategory spriteCategory = new SpriteCategory(innerText, this, num, alwaysLoad)
			{
				SheetSizes = array
			};
			SpriteCategories[spriteCategory.Name] = spriteCategory;
		}
		foreach (XmlNode item2 in xmlNode2)
		{
			string innerText2 = item2["Name"].InnerText;
			int width = Convert.ToInt32(item2["Width"].InnerText);
			int height = Convert.ToInt32(item2["Height"].InnerText);
			string innerText3 = item2["CategoryName"].InnerText;
			SpriteCategory category = SpriteCategories[innerText3];
			SpritePart spritePart = new SpritePart(innerText2, category, width, height)
			{
				SheetID = Convert.ToInt32(item2["SheetID"].InnerText),
				SheetX = Convert.ToInt32(item2["SheetX"].InnerText),
				SheetY = Convert.ToInt32(item2["SheetY"].InnerText)
			};
			SpritePartNames[spritePart.Name] = spritePart;
			spritePart.UpdateInitValues();
		}
		foreach (XmlNode item3 in xmlNode3)
		{
			Sprite sprite = null;
			if (item3.Name == "GenericSprite")
			{
				string innerText4 = item3["Name"].InnerText;
				string innerText5 = item3["SpritePartName"].InnerText;
				SpritePart spritePart2 = SpritePartNames[innerText5];
				sprite = new SpriteGeneric(innerText4, spritePart2);
			}
			else if (item3.Name == "NineRegionSprite")
			{
				string innerText6 = item3["Name"].InnerText;
				string innerText7 = item3["SpritePartName"].InnerText;
				sprite = new SpriteNineRegion(leftWidth: Convert.ToInt32(item3["LeftWidth"].InnerText), rightWidth: Convert.ToInt32(item3["RightWidth"].InnerText), topHeight: Convert.ToInt32(item3["TopHeight"].InnerText), bottomHeight: Convert.ToInt32(item3["BottomHeight"].InnerText), name: innerText6, baseSprite: SpritePartNames[innerText7]);
			}
			SpriteNames[sprite.Name] = sprite;
		}
	}

	public void Load(ResourceDepot resourceDepot)
	{
		LoadFromDepot(resourceDepot);
	}
}
