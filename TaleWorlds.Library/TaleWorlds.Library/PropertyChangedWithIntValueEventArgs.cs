namespace TaleWorlds.Library;

public class PropertyChangedWithIntValueEventArgs
{
	public string PropertyName { get; }

	public int Value { get; }

	public PropertyChangedWithIntValueEventArgs(string propertyName, int value)
	{
		PropertyName = propertyName;
		Value = value;
	}
}
