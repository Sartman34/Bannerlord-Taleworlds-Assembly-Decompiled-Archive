using System.Runtime.InteropServices;

namespace TaleWorlds.Library;

public struct AtmosphereInfo
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string AtmosphereName;

	public SunInformation SunInfo;

	public RainInformation RainInfo;

	public SnowInformation SnowInfo;

	public AmbientInformation AmbientInfo;

	public FogInformation FogInfo;

	public SkyInformation SkyInfo;

	public TimeInformation TimeInfo;

	public AreaInformation AreaInfo;

	public PostProcessInformation PostProInfo;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string InterpolatedAtmosphereName;

	public bool IsValid => !string.IsNullOrEmpty(AtmosphereName);

	public static AtmosphereInfo GetInvalidAtmosphereInfo()
	{
		AtmosphereInfo result = default(AtmosphereInfo);
		result.AtmosphereName = "";
		return result;
	}

	public void DeserializeFrom(IReader reader)
	{
		SunInfo.DeserializeFrom(reader);
		RainInfo.DeserializeFrom(reader);
		SnowInfo.DeserializeFrom(reader);
		AmbientInfo.DeserializeFrom(reader);
		FogInfo.DeserializeFrom(reader);
		SkyInfo.DeserializeFrom(reader);
		TimeInfo.DeserializeFrom(reader);
		AreaInfo.DeserializeFrom(reader);
		PostProInfo.DeserializeFrom(reader);
	}

	public void SerializeTo(IWriter writer)
	{
		SunInfo.SerializeTo(writer);
		RainInfo.SerializeTo(writer);
		SnowInfo.SerializeTo(writer);
		AmbientInfo.SerializeTo(writer);
		FogInfo.SerializeTo(writer);
		SkyInfo.SerializeTo(writer);
		TimeInfo.SerializeTo(writer);
		AreaInfo.SerializeTo(writer);
		PostProInfo.SerializeTo(writer);
	}
}
