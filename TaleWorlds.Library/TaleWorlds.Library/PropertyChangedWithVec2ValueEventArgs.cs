namespace TaleWorlds.Library;

public class PropertyChangedWithVec2ValueEventArgs
{
	public string PropertyName { get; }

	public Vec2 Value { get; }

	public PropertyChangedWithVec2ValueEventArgs(string propertyName, Vec2 value)
	{
		PropertyName = propertyName;
		Value = value;
	}
}
