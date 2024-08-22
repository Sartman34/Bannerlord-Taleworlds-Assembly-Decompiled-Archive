using TaleWorlds.Library;

namespace TaleWorlds.GauntletUI.Data;

internal class ViewBindDataInfo
{
	internal GauntletView Owner { get; private set; }

	internal string Property { get; private set; }

	internal BindingPath Path { get; private set; }

	internal ViewBindDataInfo(GauntletView view, string property, BindingPath path)
	{
		Owner = view;
		Property = property;
		Path = path;
	}
}
