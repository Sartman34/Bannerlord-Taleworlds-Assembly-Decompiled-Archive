using System;
using System.IO;
using TaleWorlds.Library;

namespace TaleWorlds.SaveSystem;

[Serializable]
public class GameData
{
	public byte[] Header { get; private set; }

	public byte[] Strings { get; private set; }

	public byte[][] ObjectData { get; private set; }

	public byte[][] ContainerData { get; private set; }

	public int TotalSize
	{
		get
		{
			int num = Header.Length;
			num += Strings.Length;
			for (int i = 0; i < ObjectData.Length; i++)
			{
				num += ObjectData[i].Length;
			}
			for (int j = 0; j < ContainerData.Length; j++)
			{
				num += ContainerData[j].Length;
			}
			return num;
		}
	}

	public GameData(byte[] header, byte[] strings, byte[][] objectData, byte[][] containerData)
	{
		Header = header;
		Strings = strings;
		ObjectData = objectData;
		ContainerData = containerData;
	}

	public void Inspect()
	{
		Debug.Print($"Header Size: {Header.Length} Strings Size: {Strings.Length} Object Size: {ObjectData.Length} Container Size: {ContainerData.Length}");
		float num = (float)TotalSize / 1048576f;
		Debug.Print($"Total size: {num:##.00} MB");
	}

	public static GameData CreateFrom(byte[] readBytes)
	{
		return (GameData)Common.DeserializeObject(readBytes);
	}

	public byte[] GetData()
	{
		return Common.SerializeObject(this);
	}

	public static void Write(System.IO.BinaryWriter writer, GameData gameData)
	{
		writer.Write(gameData.Header.Length);
		writer.Write(gameData.Header);
		writer.Write(gameData.ObjectData.Length);
		byte[][] objectData = gameData.ObjectData;
		foreach (byte[] array in objectData)
		{
			writer.Write(array.Length);
			writer.Write(array);
		}
		writer.Write(gameData.ContainerData.Length);
		objectData = gameData.ContainerData;
		foreach (byte[] array2 in objectData)
		{
			writer.Write(array2.Length);
			writer.Write(array2);
		}
		writer.Write(gameData.Strings.Length);
		writer.Write(gameData.Strings);
	}

	public static GameData Read(System.IO.BinaryReader reader)
	{
		int count = reader.ReadInt32();
		byte[] header = reader.ReadBytes(count);
		int num = reader.ReadInt32();
		byte[][] array = new byte[num][];
		for (int i = 0; i < num; i++)
		{
			int count2 = reader.ReadInt32();
			array[i] = reader.ReadBytes(count2);
		}
		int num2 = reader.ReadInt32();
		byte[][] array2 = new byte[num2][];
		for (int j = 0; j < num2; j++)
		{
			int count3 = reader.ReadInt32();
			array2[j] = reader.ReadBytes(count3);
		}
		int count4 = reader.ReadInt32();
		byte[] strings = reader.ReadBytes(count4);
		return new GameData(header, strings, array, array2);
	}

	public bool IsEqualTo(GameData gameData)
	{
		bool num = CompareByteArrays(Header, gameData.Header, "Header");
		bool flag = CompareByteArrays(Strings, gameData.Strings, "Strings");
		bool flag2 = CompareByteArrays(ObjectData, gameData.ObjectData, "ObjectData");
		bool flag3 = CompareByteArrays(ContainerData, gameData.ContainerData, "ContainerData");
		return num && flag && flag2 && flag3;
	}

	private bool CompareByteArrays(byte[] arr1, byte[] arr2, string name)
	{
		if (arr1.Length != arr2.Length)
		{
			Debug.FailedAssert(name + " failed length comparison.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.SaveSystem\\GameData.cs", "CompareByteArrays", 142);
			return false;
		}
		for (int i = 0; i < arr1.Length; i++)
		{
			if (arr1[i] != arr2[i])
			{
				Debug.FailedAssert($"{name} failed byte comparison at index {i}.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.SaveSystem\\GameData.cs", "CompareByteArrays", 150);
				return false;
			}
		}
		return true;
	}

	private bool CompareByteArrays(byte[][] arr1, byte[][] arr2, string name)
	{
		if (arr1.Length != arr2.Length)
		{
			Debug.FailedAssert(name + " failed length comparison.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.SaveSystem\\GameData.cs", "CompareByteArrays", 161);
			return false;
		}
		for (int i = 0; i < arr1.Length; i++)
		{
			if (arr1[i].Length != arr2[i].Length)
			{
				Debug.FailedAssert(name + " failed length comparison.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.SaveSystem\\GameData.cs", "CompareByteArrays", 168);
				return false;
			}
			for (int j = 0; j < arr1[i].Length; j++)
			{
				if (arr1[i][j] != arr2[i][j])
				{
					Debug.FailedAssert($"{name} failed byte comparison at index {i}-{j}.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.SaveSystem\\GameData.cs", "CompareByteArrays", 176);
				}
			}
		}
		return true;
	}
}
