using TaleWorlds.Library;

namespace TaleWorlds.SaveSystem.Definition;

internal class FloatBasicTypeSerializer : IBasicTypeSerializer
{
	void IBasicTypeSerializer.Serialize(IWriter writer, object value)
	{
		writer.WriteFloat((float)value);
	}

	object IBasicTypeSerializer.Deserialize(IReader reader)
	{
		return reader.ReadFloat();
	}
}
