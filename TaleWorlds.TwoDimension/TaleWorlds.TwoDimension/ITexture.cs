namespace TaleWorlds.TwoDimension;

public interface ITexture
{
	int Width { get; }

	int Height { get; }

	void Release();

	bool IsLoaded();
}
