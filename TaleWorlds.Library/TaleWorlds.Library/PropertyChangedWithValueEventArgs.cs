namespace TaleWorlds.Library;

public class PropertyChangedWithValueEventArgs
{
	public string PropertyName { get; }

	public object Value { get; }

	public PropertyChangedWithValueEventArgs(string propertyName, object value)
	{
		PropertyName = propertyName;
		Value = value;
	}
}
