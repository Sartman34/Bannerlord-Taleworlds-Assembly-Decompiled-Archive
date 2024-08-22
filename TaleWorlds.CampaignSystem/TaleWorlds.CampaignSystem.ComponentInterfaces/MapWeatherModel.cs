using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class MapWeatherModel : GameModel
{
	public enum WeatherEvent
	{
		Clear,
		LightRain,
		HeavyRain,
		Snowy,
		Blizzard
	}

	public enum WeatherEventEffectOnTerrain
	{
		Default,
		Wet
	}

	public abstract CampaignTime WeatherUpdateFrequency { get; }

	public abstract CampaignTime WeatherUpdatePeriod { get; }

	public abstract int DefaultWeatherNodeDimension { get; }

	public abstract AtmosphereState GetInterpolatedAtmosphereState(CampaignTime timeOfYear, Vec3 pos);

	public abstract AtmosphereInfo GetAtmosphereModel(Vec3 position);

	public abstract void GetSeasonTimeFactorOfCampaignTime(CampaignTime ct, out float timeFactorForSnow, out float timeFactorForRain, bool snapCampaignTimeToWeatherPeriod = true);

	public abstract WeatherEvent UpdateWeatherForPosition(Vec2 position, CampaignTime ct);

	public abstract WeatherEvent GetWeatherEventInPosition(Vec2 pos);

	public abstract WeatherEventEffectOnTerrain GetWeatherEffectOnTerrainForPosition(Vec2 pos);

	public abstract void InitializeSnowAndRainAmountData(byte[] bytes);
}
