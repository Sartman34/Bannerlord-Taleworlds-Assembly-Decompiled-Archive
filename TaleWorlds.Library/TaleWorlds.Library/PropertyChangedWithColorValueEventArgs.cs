namespace TaleWorlds.Library;

public class PropertyChangedWithColorValueEventArgs
{
	public string PropertyName { get; }

	public Color Value { get; }

	public PropertyChangedWithColorValueEventArgs(string propertyName, Color value)
	{
		PropertyName = propertyName;
		Value = value;
	}
}
