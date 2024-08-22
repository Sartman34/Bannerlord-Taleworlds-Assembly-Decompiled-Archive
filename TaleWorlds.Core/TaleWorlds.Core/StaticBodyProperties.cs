using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.Core;

[Serializable]
public struct StaticBodyProperties : ISerializableObject
{
	public const int WeightKeyNo = 59;

	public const int BuildKeyNo = 60;

	[SaveableProperty(1)]
	public ulong KeyPart1 { get; private set; }

	[SaveableProperty(2)]
	public ulong KeyPart2 { get; private set; }

	[SaveableProperty(3)]
	public ulong KeyPart3 { get; private set; }

	[SaveableProperty(4)]
	public ulong KeyPart4 { get; private set; }

	[SaveableProperty(5)]
	public ulong KeyPart5 { get; private set; }

	[SaveableProperty(6)]
	public ulong KeyPart6 { get; private set; }

	[SaveableProperty(7)]
	public ulong KeyPart7 { get; private set; }

	[SaveableProperty(8)]
	public ulong KeyPart8 { get; private set; }

	public static void AutoGeneratedStaticCollectObjectsStaticBodyProperties(object o, List<object> collectedObjects)
	{
		((StaticBodyProperties)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	private void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
	}

	internal static object AutoGeneratedGetMemberValueKeyPart1(object o)
	{
		return ((StaticBodyProperties)o).KeyPart1;
	}

	internal static object AutoGeneratedGetMemberValueKeyPart2(object o)
	{
		return ((StaticBodyProperties)o).KeyPart2;
	}

	internal static object AutoGeneratedGetMemberValueKeyPart3(object o)
	{
		return ((StaticBodyProperties)o).KeyPart3;
	}

	internal static object AutoGeneratedGetMemberValueKeyPart4(object o)
	{
		return ((StaticBodyProperties)o).KeyPart4;
	}

	internal static object AutoGeneratedGetMemberValueKeyPart5(object o)
	{
		return ((StaticBodyProperties)o).KeyPart5;
	}

	internal static object AutoGeneratedGetMemberValueKeyPart6(object o)
	{
		return ((StaticBodyProperties)o).KeyPart6;
	}

	internal static object AutoGeneratedGetMemberValueKeyPart7(object o)
	{
		return ((StaticBodyProperties)o).KeyPart7;
	}

	internal static object AutoGeneratedGetMemberValueKeyPart8(object o)
	{
		return ((StaticBodyProperties)o).KeyPart8;
	}

	public StaticBodyProperties(ulong keyPart1, ulong keyPart2, ulong keyPart3, ulong keyPart4, ulong keyPart5, ulong keyPart6, ulong keyPart7, ulong keyPart8)
	{
		KeyPart1 = keyPart1;
		KeyPart2 = keyPart2;
		KeyPart3 = keyPart3;
		KeyPart4 = keyPart4;
		KeyPart5 = keyPart5;
		KeyPart6 = keyPart6;
		KeyPart7 = keyPart7;
		KeyPart8 = keyPart8;
	}

	public static bool FromXmlNode(XmlNode node, out StaticBodyProperties staticBodyProperties)
	{
		string value = node.Attributes["key"].Value;
		if (value.Length != 128)
		{
			staticBodyProperties = default(StaticBodyProperties);
			return false;
		}
		ulong.TryParse(value.Substring(0, 16), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var result);
		ulong.TryParse(value.Substring(16, 16), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var result2);
		ulong.TryParse(value.Substring(32, 16), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var result3);
		ulong.TryParse(value.Substring(48, 16), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var result4);
		ulong.TryParse(value.Substring(64, 16), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var result5);
		ulong.TryParse(value.Substring(80, 16), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var result6);
		ulong.TryParse(value.Substring(96, 16), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var result7);
		ulong.TryParse(value.Substring(112, 16), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var result8);
		staticBodyProperties = new StaticBodyProperties(result, result2, result3, result4, result5, result6, result7, result8);
		return true;
	}

	public override int GetHashCode()
	{
		int num = (((((((((((((((((((((((((((-2128831035 ^ (int)(KeyPart1 << 32)) * 16777619) ^ (int)(KeyPart2 << 32)) * 16777619) ^ (int)(KeyPart3 << 32)) * 16777619) ^ (int)(KeyPart4 << 32)) * 16777619) ^ (int)(KeyPart5 << 32)) * 16777619) ^ (int)(KeyPart6 << 32)) * 16777619) ^ (int)(KeyPart7 << 32)) * 16777619) ^ (int)KeyPart1) * 16777619) ^ (int)KeyPart2) * 16777619) ^ (int)KeyPart3) * 16777619) ^ (int)KeyPart4) * 16777619) ^ (int)KeyPart5) * 16777619) ^ (int)KeyPart6) * 16777619) ^ (int)KeyPart7) * 16777619;
		int num2 = num + (num << 13);
		int num3 = num2 ^ (num2 >>> 7);
		int num4 = num3 + (num3 << 3);
		int num5 = num4 ^ (num4 >>> 17);
		return num5 + (num5 << 5);
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public static bool operator ==(StaticBodyProperties a, StaticBodyProperties b)
	{
		if ((object)a == (object)b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		if (a.KeyPart1 == b.KeyPart1 && a.KeyPart2 == b.KeyPart2 && a.KeyPart3 == b.KeyPart3 && a.KeyPart4 == b.KeyPart4 && a.KeyPart5 == b.KeyPart5 && a.KeyPart6 == b.KeyPart6 && a.KeyPart7 == b.KeyPart7)
		{
			return a.KeyPart8 == b.KeyPart8;
		}
		return false;
	}

	public static bool operator !=(StaticBodyProperties a, StaticBodyProperties b)
	{
		return !(a == b);
	}

	public override string ToString()
	{
		MBStringBuilder mBStringBuilder = default(MBStringBuilder);
		mBStringBuilder.Initialize(150, "ToString");
		mBStringBuilder.Append("key=\"");
		mBStringBuilder.Append(KeyPart1.ToString("X").PadLeft(16, '0'));
		mBStringBuilder.Append(KeyPart2.ToString("X").PadLeft(16, '0'));
		mBStringBuilder.Append(KeyPart3.ToString("X").PadLeft(16, '0'));
		mBStringBuilder.Append(KeyPart4.ToString("X").PadLeft(16, '0'));
		mBStringBuilder.Append(KeyPart5.ToString("X").PadLeft(16, '0'));
		mBStringBuilder.Append(KeyPart6.ToString("X").PadLeft(16, '0'));
		mBStringBuilder.Append(KeyPart7.ToString("X").PadLeft(16, '0'));
		mBStringBuilder.Append(KeyPart8.ToString("X").PadLeft(16, '0'));
		mBStringBuilder.Append("\" ");
		return mBStringBuilder.ToStringAndRelease();
	}

	void ISerializableObject.DeserializeFrom(IReader reader)
	{
		KeyPart1 = reader.ReadULong();
		KeyPart2 = reader.ReadULong();
		KeyPart3 = reader.ReadULong();
		KeyPart4 = reader.ReadULong();
		KeyPart5 = reader.ReadULong();
		KeyPart6 = reader.ReadULong();
		KeyPart7 = reader.ReadULong();
		KeyPart8 = reader.ReadULong();
	}

	void ISerializableObject.SerializeTo(IWriter writer)
	{
		writer.WriteULong(KeyPart1);
		writer.WriteULong(KeyPart2);
		writer.WriteULong(KeyPart3);
		writer.WriteULong(KeyPart4);
		writer.WriteULong(KeyPart5);
		writer.WriteULong(KeyPart6);
		writer.WriteULong(KeyPart7);
		writer.WriteULong(KeyPart8);
	}

	public static StaticBodyProperties GetRandomStaticBodyProperties()
	{
		Random random = new Random((int)DateTime.Now.Ticks);
		StaticBodyProperties result = default(StaticBodyProperties);
		result.KeyPart1 = (ulong)random.Next();
		result.KeyPart1 <<= 32;
		result.KeyPart1 += (ulong)random.Next();
		result.KeyPart2 = (ulong)random.Next();
		result.KeyPart2 <<= 32;
		result.KeyPart2 += (ulong)random.Next();
		result.KeyPart3 = (ulong)random.Next();
		result.KeyPart3 <<= 32;
		result.KeyPart3 += (ulong)random.Next();
		result.KeyPart4 = (ulong)random.Next();
		result.KeyPart4 <<= 32;
		result.KeyPart4 += (ulong)random.Next();
		result.KeyPart5 = (ulong)random.Next();
		result.KeyPart5 <<= 32;
		result.KeyPart5 += (ulong)random.Next();
		result.KeyPart6 = (ulong)random.Next();
		result.KeyPart6 <<= 32;
		result.KeyPart6 += (ulong)random.Next();
		result.KeyPart7 = (ulong)random.Next();
		result.KeyPart7 <<= 32;
		result.KeyPart7 += (ulong)random.Next();
		result.KeyPart8 = (ulong)random.Next();
		result.KeyPart8 <<= 32;
		result.KeyPart8 += (ulong)random.Next();
		return result;
	}
}