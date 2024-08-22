using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI;

public class ResourceTextureProvider : TextureProvider
{
	public override Texture GetTexture(TwoDimensionContext twoDimensionContext, string name)
	{
		return twoDimensionContext.LoadTexture(name);
	}
}
