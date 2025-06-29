using System;
using System.IO;
using System.Xml;
using TaleWorlds.Library;

namespace TaleWorlds.Core;

public sealed class ManagedParameters : IManagedParametersInitializer
{
	private readonly float[] _managedParametersArray = new float[68];

	public static ManagedParameters Instance { get; } = new ManagedParameters();


	public static float GetParameter(ManagedParametersEnum managedParameterType)
	{
		return Instance._managedParametersArray[(int)managedParameterType];
	}

	public static void SetParameter(ManagedParametersEnum managedParameterType, float newValue)
	{
		Instance._managedParametersArray[(int)managedParameterType] = newValue;
	}

	public void Initialize(string relativeXmlPath)
	{
		XmlDocument doc = LoadXmlFile(relativeXmlPath);
		LoadFromXml(doc);
	}

	private ManagedParameters()
	{
	}

	private static XmlDocument LoadXmlFile(string path)
	{
		Debug.Print("opening " + path);
		XmlDocument xmlDocument = new XmlDocument();
		StreamReader streamReader = new StreamReader(path);
		string xml = streamReader.ReadToEnd();
		xmlDocument.LoadXml(xml);
		streamReader.Close();
		return xmlDocument;
	}

	private void LoadFromXml(XmlNode doc)
	{
		Debug.Print("loading managed_core_parameters.xml");
		if (doc.ChildNodes.Count <= 1)
		{
			throw new TWXmlLoadException("Incorrect XML document format.");
		}
		if (doc.ChildNodes[1].Name != "base")
		{
			throw new TWXmlLoadException("Incorrect XML document format.");
		}
		if (doc.ChildNodes[1].ChildNodes[0].Name != "managed_core_parameters")
		{
			throw new TWXmlLoadException("Incorrect XML document format.");
		}
		XmlNode xmlNode = null;
		if (doc.ChildNodes[1].ChildNodes[0].Name == "managed_core_parameters")
		{
			xmlNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[0];
		}
		while (xmlNode != null)
		{
			if (xmlNode.Name == "managed_core_parameter" && xmlNode.NodeType != XmlNodeType.Comment && Enum.TryParse<ManagedParametersEnum>(xmlNode.Attributes["id"].Value, ignoreCase: true, out var result))
			{
				_managedParametersArray[(int)result] = float.Parse(xmlNode.Attributes["value"].Value);
			}
			xmlNode = xmlNode.NextSibling;
		}
	}

	public float GetManagedParameter(ManagedParametersEnum managedParameterEnum)
	{
		return _managedParametersArray[(int)managedParameterEnum];
	}
}
