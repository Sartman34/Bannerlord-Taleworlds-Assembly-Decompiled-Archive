using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.Engine.GauntletUI;

public class TwoDimensionEngineResourceContext : ITwoDimensionResourceContext
{
	TaleWorlds.TwoDimension.Texture ITwoDimensionResourceContext.LoadTexture(ResourceDepot resourceDepot, string name)
	{
		string[] array = name.Split(new char[1] { '\\' });
		Texture fromResource = Texture.GetFromResource(array[array.Length - 1]);
		fromResource.SetTextureAsAlwaysValid();
		bool flag = true;
		flag = true;
		fromResource.PreloadTexture(flag);
		return new TaleWorlds.TwoDimension.Texture(new EngineTexture(fromResource));
	}
}
