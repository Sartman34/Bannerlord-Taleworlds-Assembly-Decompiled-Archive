using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI;

public class FontFactory
{
	private readonly Dictionary<string, Font> _bitmapFonts;

	private readonly ResourceDepot _resourceDepot;

	private readonly Dictionary<string, Language> _fontLanguageMap;

	private SpriteData _latestSpriteData;

	public string DefaultLangageID { get; private set; }

	public string CurrentLangageID { get; private set; }

	public Font DefaultFont => GetCurrentLanguage().DefaultFont;

	public FontFactory(ResourceDepot resourceDepot)
	{
		_resourceDepot = resourceDepot;
		_bitmapFonts = new Dictionary<string, Font>();
		_fontLanguageMap = new Dictionary<string, Language>();
		_resourceDepot.OnResourceChange += OnResourceChange;
	}

	private void OnResourceChange()
	{
		CheckForUpdates();
	}

	public void LoadAllFonts(SpriteData spriteData)
	{
		string[] files = _resourceDepot.GetFiles("Fonts", ".fnt");
		foreach (string path in files)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			TryAddFontDefinition(Path.GetDirectoryName(path) + "/", fileNameWithoutExtension, spriteData);
		}
		files = _resourceDepot.GetFiles("Fonts", ".xml");
		foreach (string text in files)
		{
			if (Path.GetFileNameWithoutExtension(text).EndsWith("Languages"))
			{
				try
				{
					LoadLocalizationValues(text);
				}
				catch (Exception)
				{
					Debug.FailedAssert("Failed to load language at path: " + text, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\FontFactory.cs", "LoadAllFonts", 71);
				}
			}
		}
		if (string.IsNullOrEmpty(DefaultLangageID))
		{
			DefaultLangageID = "English";
			CurrentLangageID = DefaultLangageID;
		}
		_latestSpriteData = spriteData;
	}

	public bool TryAddFontDefinition(string fontPath, string fontName, SpriteData spriteData)
	{
		Font font = new Font(fontName);
		string path = fontPath + fontName + ".fnt";
		bool num = font.TryLoadFontFromPath(path, spriteData);
		if (num)
		{
			_bitmapFonts.Add(fontName, font);
		}
		return num;
	}

	public void LoadLocalizationValues(string sourceXMLPath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(sourceXMLPath);
		XmlElement xmlElement = xmlDocument["Languages"];
		if (!string.IsNullOrEmpty(xmlElement.Attributes["DefaultLanguage"]?.InnerText))
		{
			DefaultLangageID = xmlElement.Attributes["DefaultLanguage"]?.InnerText ?? "English";
			CurrentLangageID = DefaultLangageID;
		}
		foreach (XmlNode item in xmlElement)
		{
			if (item.NodeType == XmlNodeType.Element && item.Name == "Language")
			{
				Language language = Language.CreateFrom(item, this);
				if (_fontLanguageMap.TryGetValue(language.LanguageID, out var _))
				{
					_fontLanguageMap[language.LanguageID] = language;
				}
				else
				{
					_fontLanguageMap.Add(language.LanguageID, language);
				}
			}
		}
	}

	public Language GetCurrentLanguage()
	{
		Language language = null;
		if (_fontLanguageMap.TryGetValue(CurrentLangageID, out var value))
		{
			language = value;
		}
		else if (DefaultLangageID != null && _fontLanguageMap.TryGetValue(DefaultLangageID, out value))
		{
			Debug.Print("Couldn't find language in language map: " + CurrentLangageID);
			Debug.FailedAssert("Couldn't find language in language map: " + CurrentLangageID, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\FontFactory.cs", "GetCurrentLanguage", 148);
			language = value;
		}
		else if (_fontLanguageMap.TryGetValue("English", out value))
		{
			Debug.Print("Couldn't find default language(" + (DefaultLangageID ?? "INVALID") + ") in language map.");
			Debug.FailedAssert("Couldn't find default language(" + (DefaultLangageID ?? "INVALID") + ") in language map.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\FontFactory.cs", "GetCurrentLanguage", 154);
			language = value;
		}
		else
		{
			Debug.Print("Couldn't find English language in language map.");
			Debug.FailedAssert("Couldn't find English language in language map.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\FontFactory.cs", "GetCurrentLanguage", 160);
			language = _fontLanguageMap.FirstOrDefault().Value;
		}
		if (language == null)
		{
			Debug.Print("There are no languages in language map");
			Debug.FailedAssert("There are no languages in language map", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\FontFactory.cs", "GetCurrentLanguage", 167);
		}
		return language;
	}

	public Font GetFont(string fontName)
	{
		if (_bitmapFonts.ContainsKey(fontName))
		{
			return _bitmapFonts[fontName];
		}
		return DefaultFont;
	}

	public IEnumerable<Font> GetFonts()
	{
		return _bitmapFonts.Values;
	}

	public string GetFontName(Font font)
	{
		return _bitmapFonts.FirstOrDefault((KeyValuePair<string, Font> f) => f.Value == font).Key;
	}

	public Font GetMappedFontForLocalization(string englishFontName)
	{
		if (string.IsNullOrEmpty(englishFontName))
		{
			return DefaultFont;
		}
		if (CurrentLangageID != DefaultLangageID)
		{
			Language currentLanguage = GetCurrentLanguage();
			if (currentLanguage.FontMapHasKey(englishFontName))
			{
				return currentLanguage.GetMappedFont(englishFontName);
			}
			return DefaultFont;
		}
		return GetFont(englishFontName);
	}

	public void OnLanguageChange(string newLanguageCode)
	{
		CurrentLangageID = newLanguageCode;
	}

	public Font GetUsableFontForCharacter(int characterCode)
	{
		for (int i = 0; i < _fontLanguageMap.Values.Count; i++)
		{
			if (_fontLanguageMap.ElementAt(i).Value.DefaultFont.Characters.ContainsKey(characterCode))
			{
				return _fontLanguageMap.ElementAt(i).Value.DefaultFont;
			}
		}
		return null;
	}

	public void CheckForUpdates()
	{
		string currentLangageID = CurrentLangageID;
		CurrentLangageID = null;
		DefaultLangageID = null;
		_bitmapFonts.Clear();
		_fontLanguageMap.Clear();
		LoadAllFonts(_latestSpriteData);
		CurrentLangageID = currentLangageID;
	}
}
