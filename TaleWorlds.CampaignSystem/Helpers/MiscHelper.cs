using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using TaleWorlds.Library;

namespace Helpers;

public static class MiscHelper
{
	public static XmlDocument LoadXmlFile(string path)
	{
		Debug.Print("opening " + path);
		XmlDocument xmlDocument = new XmlDocument();
		StreamReader streamReader = new StreamReader(path);
		string xml = streamReader.ReadToEnd();
		xmlDocument.LoadXml(xml);
		streamReader.Close();
		return xmlDocument;
	}

	public static string GenerateCampaignId(int length)
	{
		using MD5 mD = MD5.Create();
		string s = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
		byte[] bytes = Encoding.ASCII.GetBytes(s);
		byte[] array = mD.ComputeHash(bytes);
		MBStringBuilder mBStringBuilder = default(MBStringBuilder);
		mBStringBuilder.Initialize(16, "GenerateCampaignId");
		for (int i = 0; i < array.Length; i++)
		{
			if (mBStringBuilder.Length >= length)
			{
				break;
			}
			mBStringBuilder.Append(array[i].ToString("x2"));
		}
		return mBStringBuilder.ToStringAndRelease();
	}
}
