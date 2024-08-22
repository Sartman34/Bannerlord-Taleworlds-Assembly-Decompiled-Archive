using System.IO;
using TaleWorlds.Library;

namespace psai.net;

internal class PlatformLayerStandalone : IPlatformLayer
{
	private Logik m_logik;

	public PlatformLayerStandalone(Logik logik)
	{
		m_logik = logik;
	}

	public void Initialize()
	{
	}

	public void Release()
	{
	}

	public string ConvertFilePathForPlatform(string filepath)
	{
		string text = filepath.Replace('\\', '/');
		string text2 = "";
		text2 = ((!text.Contains("/")) ? Path.GetFileNameWithoutExtension(text) : (Path.GetDirectoryName(text) + "/" + Path.GetFileNameWithoutExtension(text)));
		if (ApplicationPlatform.CurrentPlatform == Platform.Orbis)
		{
			return "PS4/" + text2 + ".fsb";
		}
		if (ApplicationPlatform.CurrentPlatform == Platform.Durango)
		{
			return "XboxOne/" + text2 + ".fsb";
		}
		return "PC/" + text2 + ".ogg";
	}

	public Stream GetStreamOnPsaiSoundtrackFile(string filepath)
	{
		if (Logik.CheckIfFileExists(filepath))
		{
			return File.OpenRead(filepath);
		}
		return null;
	}
}
