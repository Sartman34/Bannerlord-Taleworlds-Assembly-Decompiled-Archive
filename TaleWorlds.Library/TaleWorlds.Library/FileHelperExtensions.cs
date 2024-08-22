using System.Xml;

namespace TaleWorlds.Library;

public static class FileHelperExtensions
{
	public static void Load(this XmlDocument document, PlatformFilePath path)
	{
		string fileContentString = FileHelper.GetFileContentString(path);
		if (!string.IsNullOrEmpty(fileContentString))
		{
			document.LoadXml(fileContentString);
		}
	}

	public static void Save(this XmlDocument document, PlatformFilePath path)
	{
		string outerXml = document.OuterXml;
		FileHelper.SaveFileString(path, outerXml);
	}
}
