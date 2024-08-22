using TaleWorlds.TwoDimension;

namespace TaleWorlds.Engine.GauntletUI;

public class EngineTexture : ITexture
{
	public Texture Texture { get; private set; }

	int ITexture.Width => Texture.Width;

	int ITexture.Height => Texture.Height;

	public EngineTexture(Texture engineTexture)
	{
		Texture = engineTexture;
	}

	bool ITexture.IsLoaded()
	{
		return Texture.IsLoaded();
	}

	void ITexture.Release()
	{
		Texture.Release();
	}
}
