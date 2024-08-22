using TaleWorlds.GauntletUI.PrefabSystem;

namespace TaleWorlds.GauntletUI.TooltipExtensions;

public class WidgetAttributeKeyTypeHint : WidgetAttributeKeyType
{
	public override bool CheckKeyType(string key)
	{
		return key == "Hint";
	}

	public override string GetKeyName(string key)
	{
		return "Hint";
	}

	public override string GetSerializedKey(string key)
	{
		return "Hint";
	}
}
