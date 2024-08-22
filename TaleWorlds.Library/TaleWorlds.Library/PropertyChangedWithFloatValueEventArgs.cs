namespace TaleWorlds.Library;

public class PropertyChangedWithFloatValueEventArgs
{
	public string PropertyName { get; }

	public float Value { get; }

	public PropertyChangedWithFloatValueEventArgs(string propertyName, float value)
	{
		PropertyName = propertyName;
		Value = value;
	}
}
