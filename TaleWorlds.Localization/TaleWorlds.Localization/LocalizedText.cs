using System.Collections.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.Localization;

internal class LocalizedText
{
	private readonly Dictionary<string, string> _localizedTextDictionary;

	public LocalizedText()
	{
		_localizedTextDictionary = new Dictionary<string, string>();
	}

	public void AddTranslation(string language, string translation)
	{
		if (!_localizedTextDictionary.ContainsKey(language))
		{
			_localizedTextDictionary.Add(language, translation);
		}
	}

	public string GetTranslatedText(string languageId)
	{
		if (_localizedTextDictionary.TryGetValue(languageId, out var value))
		{
			return value;
		}
		if (_localizedTextDictionary.TryGetValue("English", out value))
		{
			return value;
		}
		return null;
	}

	public bool CheckValidity(string id, out string errorLine)
	{
		errorLine = null;
		bool flag = false;
		foreach (KeyValuePair<string, string> item in _localizedTextDictionary)
		{
			string value = item.Value;
			int num = 0;
			int num2 = 0;
			string text = value;
			for (int i = 0; i < text.Length; i++)
			{
				switch (text[i])
				{
				case '{':
					num++;
					break;
				case '}':
					num2++;
					break;
				}
			}
			int num3 = 0;
			int num4 = 0;
			string text2 = value;
			while (true)
			{
				int num5 = text2.IndexOf("{?");
				if (num5 == -1)
				{
					break;
				}
				num5 = MathF.Min(num5 + 1, text2.Length - 1);
				text2 = text2.Substring(num5);
				if (text2.Length > 2 && text2[1] != '}')
				{
					num3++;
				}
			}
			string text3 = value;
			while (true)
			{
				int num6 = text3.IndexOf("{\\?}");
				if (num6 == -1)
				{
					break;
				}
				num4++;
				num6 = MathF.Min(num6 + 1, value.Length - 1);
				text3 = text3.Substring(num6);
			}
			if (num != num2)
			{
				errorLine = $"{id} | {value}\n";
				flag = true;
			}
			else if (num3 != num4)
			{
				errorLine = $"{id} | {value}\n";
				flag = true;
			}
			else if (!flag)
			{
				try
				{
					MBTextManager.ProcessTextToString(new TextObject("{=" + id + "}" + LocalizedTextManager.GetTranslatedText(MBTextManager.ActiveTextLanguage, id)), shouldClear: true);
				}
				catch
				{
					errorLine = $"{id} | {value}\n";
				}
			}
		}
		return flag;
	}
}
