namespace TaleWorlds.SaveSystem.Definition;

public struct MemberTypeId
{
	public byte TypeLevel;

	public short LocalSaveId;

	public short SaveId => (short)((short)(TypeLevel << 8) + LocalSaveId);

	public static MemberTypeId Invalid => new MemberTypeId(0, -1);

	public override string ToString()
	{
		return "(" + TypeLevel + "," + LocalSaveId + ")";
	}

	public MemberTypeId(byte typeLevel, short localSaveId)
	{
		TypeLevel = typeLevel;
		LocalSaveId = localSaveId;
	}
}
