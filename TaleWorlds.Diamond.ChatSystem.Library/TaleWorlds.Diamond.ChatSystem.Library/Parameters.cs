namespace TaleWorlds.Diamond.ChatSystem.Library;

public class Parameters
{
	public static readonly byte[] Key = new byte[32]
	{
		210, 35, 47, 221, 44, 195, 138, 183, 114, 226,
		64, 143, 47, 204, 190, 129, 104, 210, 198, 66,
		99, 78, 92, 183, 124, 58, 176, 38, 253, 187,
		41, 216
	};

	public static readonly byte[] InitializationVector = new byte[16]
	{
		123, 104, 184, 253, 68, 167, 48, 150, 215, 188,
		239, 175, 81, 189, 20, 179
	};
}
