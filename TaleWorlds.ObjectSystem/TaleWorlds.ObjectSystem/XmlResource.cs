using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.ModuleManager;

namespace TaleWorlds.ObjectSystem;

public static class XmlResource
{
	public static List<MbObjectXmlInformation> XmlInformationList = new List<MbObjectXmlInformation>();

	public static List<MbObjectXmlInformation> MbprojXmls = new List<MbObjectXmlInformation>();

	public static void InitializeXmlInformationList(List<MbObjectXmlInformation> xmlInformation)
	{
		XmlInformationList = xmlInformation;
	}

	public static void GetMbprojxmls(string moduleName)
	{
		string mbprojPath = ModuleHelper.GetMbprojPath(moduleName);
		if (mbprojPath.Length <= 0 || !File.Exists(mbprojPath))
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(mbprojPath);
		XmlNodeList xmlNodeList = xmlDocument.SelectSingleNode("base").SelectNodes("file");
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item2 in xmlNodeList)
		{
			string innerText = item2.Attributes["id"].InnerText;
			string innerText2 = item2.Attributes["name"].InnerText;
			MbObjectXmlInformation mbObjectXmlInformation = default(MbObjectXmlInformation);
			mbObjectXmlInformation.Id = innerText;
			mbObjectXmlInformation.Name = innerText2;
			mbObjectXmlInformation.ModuleName = moduleName;
			mbObjectXmlInformation.GameTypesIncluded = new List<string>();
			MbObjectXmlInformation item = mbObjectXmlInformation;
			MbprojXmls.Add(item);
		}
	}

	public static void GetXmlListAndApply(string moduleName)
	{
		string path = ModuleHelper.GetPath(moduleName);
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = true;
		using (XmlReader.Create(path, xmlReaderSettings))
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(path);
			XmlNodeList xmlNodeList = xmlDocument.SelectSingleNode("Module").SelectNodes("Xmls/XmlNode");
			if (xmlNodeList == null)
			{
				return;
			}
			foreach (XmlNode item2 in xmlNodeList)
			{
				XmlNode? xmlNode = item2.SelectSingleNode("XmlName");
				string innerText = xmlNode.Attributes["id"].InnerText;
				string innerText2 = xmlNode.Attributes["path"].InnerText;
				List<string> list = new List<string>();
				XmlNode xmlNode2 = item2.SelectSingleNode("IncludedGameTypes");
				if (xmlNode2 != null)
				{
					foreach (XmlNode childNode in xmlNode2.ChildNodes)
					{
						list.Add(childNode.Attributes["value"].InnerText);
					}
				}
				MbObjectXmlInformation mbObjectXmlInformation = default(MbObjectXmlInformation);
				mbObjectXmlInformation.Id = innerText;
				mbObjectXmlInformation.Name = innerText2;
				mbObjectXmlInformation.ModuleName = moduleName;
				mbObjectXmlInformation.GameTypesIncluded = list;
				MbObjectXmlInformation item = mbObjectXmlInformation;
				XmlInformationList.Add(item);
			}
		}
	}
}
