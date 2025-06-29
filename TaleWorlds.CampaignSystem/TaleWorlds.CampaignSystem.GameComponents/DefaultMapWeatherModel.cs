using System;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultMapWeatherModel : MapWeatherModel
{
	private struct SunPosition
	{
		public float Angle { get; private set; }

		public float Altitude { get; private set; }

		public SunPosition(float angle, float altitude)
		{
			this = default(SunPosition);
			Angle = angle;
			Altitude = altitude;
		}
	}

	private const float SunRiseNorm = 1f / 12f;

	private const float SunSetNorm = 11f / 12f;

	private const float DayTime = 20f;

	private const float MinSunAngle = 0f;

	private const float MaxSunAngle = 50f;

	private const float MinEnvironmentMultiplier = 0.001f;

	private const float DayEnvironmentMultiplier = 1f;

	private const float NightEnvironmentMultiplier = 0.001f;

	private const float SnowStartThreshold = 0.55f;

	private const float DenseSnowStartThreshold = 0.85f;

	private const float NoSnowDelta = 0.1f;

	private const float WetThreshold = 0.6f;

	private const float WetThresholdForTexture = 0.3f;

	private const float LightRainStartThreshold = 0.7f;

	private const float DenseRainStartThreshold = 0.85f;

	private const float SnowFrequencyModifier = 0.1f;

	private const float RainFrequencyModifier = 0.45f;

	private const float MaxSnowCoverage = 0.75f;

	private const int SnowAndRainDataTextureDimension = 1024;

	private const int WeatherNodeDimension = 32;

	private WeatherEvent[] _weatherDataCache = new WeatherEvent[1024];

	private AtmosphereGrid _atmosphereGrid;

	private byte[] _snowAndRainAmountData = new byte[2097152];

	private bool _sunIsMoon;

	public override CampaignTime WeatherUpdatePeriod => CampaignTime.Hours(4f);

	public override CampaignTime WeatherUpdateFrequency => new CampaignTime(WeatherUpdatePeriod.NumTicks / (DefaultWeatherNodeDimension * DefaultWeatherNodeDimension));

	public override int DefaultWeatherNodeDimension => 32;

	private CampaignTime PreviousRainDataCheckForWetness => CampaignTime.Hours(24f);

	private uint GetSeed(CampaignTime campaignTime, Vec2 position)
	{
		campaignTime += new CampaignTime(Campaign.Current.UniqueGameId.GetHashCode());
		GetNodePositionForWeather(position, out var xIndex, out var yIndex);
		uint num = (uint)(campaignTime.ToHours / WeatherUpdatePeriod.ToHours);
		if (campaignTime.ToSeconds % WeatherUpdatePeriod.ToSeconds < WeatherUpdateFrequency.ToSeconds * (double)(xIndex * DefaultWeatherNodeDimension + yIndex))
		{
			num--;
		}
		return num;
	}

	public override AtmosphereState GetInterpolatedAtmosphereState(CampaignTime timeOfYear, Vec3 pos)
	{
		if (_atmosphereGrid == null)
		{
			_atmosphereGrid = new AtmosphereGrid();
			_atmosphereGrid.Initialize();
		}
		return _atmosphereGrid.GetInterpolatedStateInfo(pos);
	}

	private Vec2 GetNodePositionForWeather(Vec2 pos, out int xIndex, out int yIndex)
	{
		if (Campaign.Current.MapSceneWrapper != null)
		{
			Vec2 terrainSize = Campaign.Current.MapSceneWrapper.GetTerrainSize();
			float num = terrainSize.X / (float)DefaultWeatherNodeDimension;
			float num2 = terrainSize.Y / (float)DefaultWeatherNodeDimension;
			xIndex = (int)(pos.x / num);
			yIndex = (int)(pos.y / num2);
			float a = (float)xIndex * num;
			float b = (float)yIndex * num2;
			return new Vec2(a, b);
		}
		xIndex = 0;
		yIndex = 0;
		return Vec2.Zero;
	}

	public override AtmosphereInfo GetAtmosphereModel(Vec3 position)
	{
		float hourOfDayNormalized = GetHourOfDayNormalized();
		GetSeasonTimeFactorOfCampaignTime(CampaignTime.Now, out var timeFactorForSnow, out var _);
		SunPosition sunPosition = GetSunPosition(hourOfDayNormalized, timeFactorForSnow);
		float environmentMultiplier = GetEnvironmentMultiplier(sunPosition);
		float modifiedEnvironmentMultiplier = GetModifiedEnvironmentMultiplier(environmentMultiplier);
		modifiedEnvironmentMultiplier = TaleWorlds.Library.MathF.Max(TaleWorlds.Library.MathF.Pow(modifiedEnvironmentMultiplier, 1.5f), 0.001f);
		Vec3 sunColor = GetSunColor(environmentMultiplier);
		AtmosphereState gridInfo = GetInterpolatedAtmosphereState(CampaignTime.Now, position);
		float temperature = GetTemperature(ref gridInfo, timeFactorForSnow);
		float humidity = GetHumidity(ref gridInfo, timeFactorForSnow);
		Campaign.Current.Models.MapWeatherModel.UpdateWeatherForPosition(position.AsVec2, CampaignTime.Now);
		(CampaignTime.Seasons, bool, float, float) seasonRainAndSnowDataForOpeningMission = GetSeasonRainAndSnowDataForOpeningMission(position.AsVec2);
		CampaignTime.Seasons item = seasonRainAndSnowDataForOpeningMission.Item1;
		bool item2 = seasonRainAndSnowDataForOpeningMission.Item2;
		float item3 = seasonRainAndSnowDataForOpeningMission.Item3;
		float item4 = seasonRainAndSnowDataForOpeningMission.Item4;
		string selectedAtmosphereId = GetSelectedAtmosphereId(item, item2, item4, item3);
		AtmosphereInfo result = default(AtmosphereInfo);
		result.SunInfo.Altitude = sunPosition.Altitude;
		result.SunInfo.Angle = sunPosition.Angle;
		result.SunInfo.Color = sunColor;
		result.SunInfo.Brightness = GetSunBrightness(environmentMultiplier);
		result.SunInfo.Size = GetSunSize(environmentMultiplier);
		result.SunInfo.RayStrength = GetSunRayStrength(environmentMultiplier);
		result.SunInfo.MaxBrightness = GetSunBrightness(1f, forceDay: true);
		result.RainInfo.Density = item3;
		result.SnowInfo.Density = item4;
		result.AmbientInfo.EnvironmentMultiplier = TaleWorlds.Library.MathF.Max(modifiedEnvironmentMultiplier * 0.5f, 0.001f);
		result.AmbientInfo.AmbientColor = GetAmbientFogColor(modifiedEnvironmentMultiplier);
		result.AmbientInfo.MieScatterStrength = GetMieScatterStrength(environmentMultiplier);
		result.AmbientInfo.RayleighConstant = GetRayleighConstant(environmentMultiplier);
		result.SkyInfo.Brightness = GetSkyBrightness(hourOfDayNormalized, environmentMultiplier);
		result.FogInfo.Density = GetFogDensity(environmentMultiplier, position);
		result.FogInfo.Color = GetFogColor(modifiedEnvironmentMultiplier);
		result.FogInfo.Falloff = 1.48f;
		result.TimeInfo.TimeOfDay = GetHourOfDay();
		result.TimeInfo.WinterTimeFactor = GetWinterTimeFactor(CampaignTime.Now);
		result.TimeInfo.DrynessFactor = GetDrynessFactor(CampaignTime.Now);
		result.TimeInfo.NightTimeFactor = GetNightTimeFactor();
		result.TimeInfo.Season = (int)item;
		result.AreaInfo.Temperature = temperature;
		result.AreaInfo.Humidity = humidity;
		result.PostProInfo.MinExposure = MBMath.Lerp(-3f, -2f, GetExposureCoefficientBetweenDayNight());
		result.PostProInfo.MaxExposure = MBMath.Lerp(2f, 0f, modifiedEnvironmentMultiplier);
		result.PostProInfo.BrightpassThreshold = MBMath.Lerp(0.7f, 0.9f, modifiedEnvironmentMultiplier);
		result.PostProInfo.MiddleGray = 0.1f;
		result.InterpolatedAtmosphereName = selectedAtmosphereId;
		return result;
	}

	public override void InitializeSnowAndRainAmountData(byte[] snowAndRainAmountData)
	{
		_snowAndRainAmountData = snowAndRainAmountData;
	}

	public override WeatherEvent UpdateWeatherForPosition(Vec2 position, CampaignTime ct)
	{
		var (num, rainValue) = GetSnowAndRainDataFromTexture(position, ct);
		if (num > 0.55f)
		{
			return SetIsBlizzardOrSnowFromFunction(num, ct, in position);
		}
		return SetIsRainingOrWetFromFunction(rainValue, ct, in position);
	}

	private WeatherEvent SetIsBlizzardOrSnowFromFunction(float snowValue, CampaignTime campaignTime, in Vec2 position)
	{
		int xIndex;
		int yIndex;
		Vec2 adjustedPosition = GetNodePositionForWeather(position, out xIndex, out yIndex);
		if (snowValue >= 0.65000004f)
		{
			float frequency = (snowValue - 0.55f) / 0.45f;
			uint seed = GetSeed(campaignTime, position);
			bool currentWeatherInAdjustedPosition = GetCurrentWeatherInAdjustedPosition(seed, frequency, 0.1f, in adjustedPosition);
			_weatherDataCache[yIndex * 32 + xIndex] = (currentWeatherInAdjustedPosition ? WeatherEvent.Blizzard : WeatherEvent.Snowy);
		}
		else
		{
			_weatherDataCache[yIndex * 32 + xIndex] = ((snowValue > 0.55f) ? WeatherEvent.Snowy : WeatherEvent.Clear);
		}
		return _weatherDataCache[yIndex * 32 + xIndex];
	}

	private WeatherEvent SetIsRainingOrWetFromFunction(float rainValue, CampaignTime campaignTime, in Vec2 position)
	{
		int xIndex;
		int yIndex;
		Vec2 adjustedPosition = GetNodePositionForWeather(position, out xIndex, out yIndex);
		if (rainValue >= 0.6f)
		{
			float frequency = (rainValue - 0.6f) / 0.39999998f;
			uint seed = GetSeed(campaignTime, position);
			_weatherDataCache[yIndex * 32 + xIndex] = WeatherEvent.Clear;
			if (GetCurrentWeatherInAdjustedPosition(seed, frequency, 0.45f, in adjustedPosition))
			{
				_weatherDataCache[yIndex * 32 + xIndex] = WeatherEvent.HeavyRain;
			}
			else
			{
				CampaignTime campaignTime2 = new CampaignTime(campaignTime.NumTicks - WeatherUpdatePeriod.NumTicks);
				uint seed2 = GetSeed(campaignTime2, position);
				float frequency2 = (GetSnowAndRainDataFromTexture(position, campaignTime2).Item2 - 0.6f) / 0.39999998f;
				while (campaignTime.NumTicks - campaignTime2.NumTicks < PreviousRainDataCheckForWetness.NumTicks)
				{
					if (GetCurrentWeatherInAdjustedPosition(seed2, frequency2, 0.45f, in adjustedPosition))
					{
						_weatherDataCache[yIndex * 32 + xIndex] = WeatherEvent.LightRain;
						break;
					}
					campaignTime2 = new CampaignTime(campaignTime2.NumTicks - WeatherUpdatePeriod.NumTicks);
					seed2 = GetSeed(campaignTime2, position);
					frequency2 = (GetSnowAndRainDataFromTexture(position, campaignTime2).Item2 - 0.6f) / 0.39999998f;
				}
			}
		}
		else
		{
			_weatherDataCache[yIndex * 32 + xIndex] = WeatherEvent.Clear;
		}
		return _weatherDataCache[yIndex * 32 + xIndex];
	}

	private bool GetCurrentWeatherInAdjustedPosition(uint seed, float frequency, float chanceModifier, in Vec2 adjustedPosition)
	{
		return frequency * chanceModifier > MBRandom.RandomFloatWithSeed(seed, (uint)(Campaign.MapDiagonal * adjustedPosition.X + adjustedPosition.Y));
	}

	private string GetSelectedAtmosphereId(CampaignTime.Seasons selectedSeason, bool isRaining, float snowValue, float rainValue)
	{
		string result = "semicloudy_field_battle";
		if (Settlement.CurrentSettlement != null && (Settlement.CurrentSettlement.IsFortification || Settlement.CurrentSettlement.IsVillage))
		{
			result = "semicloudy_" + Settlement.CurrentSettlement.Culture.StringId;
		}
		if (selectedSeason == CampaignTime.Seasons.Winter)
		{
			result = ((!(snowValue >= 0.85f)) ? "semi_snowy" : "dense_snowy");
		}
		else
		{
			if (rainValue > 0.6f)
			{
				result = "wet";
			}
			if (isRaining)
			{
				result = ((!(rainValue >= 0.85f)) ? "semi_rainy" : "dense_rainy");
			}
		}
		return result;
	}

	private (CampaignTime.Seasons, bool, float, float) GetSeasonRainAndSnowDataForOpeningMission(Vec2 position)
	{
		CampaignTime.Seasons seasons = CampaignTime.Now.GetSeasonOfYear;
		WeatherEvent weatherEventInPosition = GetWeatherEventInPosition(position);
		float item = 0f;
		float item2 = 0.85f;
		bool item3 = false;
		switch (weatherEventInPosition)
		{
		case WeatherEvent.Clear:
			if (seasons == CampaignTime.Seasons.Winter)
			{
				seasons = ((CampaignTime.Now.GetDayOfSeason <= 10) ? CampaignTime.Seasons.Autumn : CampaignTime.Seasons.Spring);
			}
			break;
		case WeatherEvent.LightRain:
			if (seasons == CampaignTime.Seasons.Winter)
			{
				seasons = ((CampaignTime.Now.GetDayOfSeason <= 10) ? CampaignTime.Seasons.Autumn : CampaignTime.Seasons.Spring);
			}
			item = 0.7f;
			break;
		case WeatherEvent.HeavyRain:
			if (seasons == CampaignTime.Seasons.Winter)
			{
				seasons = ((CampaignTime.Now.GetDayOfSeason <= 10) ? CampaignTime.Seasons.Autumn : CampaignTime.Seasons.Spring);
			}
			item3 = true;
			item = 0.85f + MBRandom.RandomFloatRanged(0f, 0.14999998f);
			break;
		case WeatherEvent.Snowy:
			seasons = CampaignTime.Seasons.Winter;
			item = 0.55f;
			item2 = 0.55f + MBRandom.RandomFloatRanged(0f, 0.3f);
			break;
		case WeatherEvent.Blizzard:
			seasons = CampaignTime.Seasons.Winter;
			item = 0.85f;
			item2 = 0.85f;
			break;
		}
		return (seasons, item3, item, item2);
	}

	private SunPosition GetSunPosition(float hourNorm, float seasonFactor)
	{
		float altitude;
		float angle;
		if (hourNorm >= 1f / 12f && hourNorm < 11f / 12f)
		{
			_sunIsMoon = false;
			float amount = (hourNorm - 1f / 12f) / 0.8333334f;
			altitude = MBMath.Lerp(0f, 180f, amount);
			angle = 50f * seasonFactor;
		}
		else
		{
			_sunIsMoon = true;
			if (hourNorm >= 11f / 12f)
			{
				hourNorm -= 1f;
			}
			float num = (hourNorm - -0.08333331f) / 0.16666666f;
			num = ((num < 0f) ? 0f : ((num > 1f) ? 1f : num));
			altitude = MBMath.Lerp(180f, 0f, num);
			angle = 50f * seasonFactor;
		}
		return new SunPosition(angle, altitude);
	}

	private Vec3 GetSunColor(float environmentMultiplier)
	{
		if (!_sunIsMoon)
		{
			return new Vec3(1f, 1f - (1f - TaleWorlds.Library.MathF.Pow(environmentMultiplier, 0.3f)) / 2f, 0.9f - (1f - TaleWorlds.Library.MathF.Pow(environmentMultiplier, 0.3f)) / 2.5f);
		}
		Vec3 v = new Vec3(0.85f - TaleWorlds.Library.MathF.Pow(environmentMultiplier, 0.4f), 0.8f - TaleWorlds.Library.MathF.Pow(environmentMultiplier, 0.5f), 0.8f - TaleWorlds.Library.MathF.Pow(environmentMultiplier, 0.8f));
		return Vec3.Vec3Max(v, new Vec3(0.05f, 0.05f, 0.1f));
	}

	private float GetSunBrightness(float environmentMultiplier, bool forceDay = false)
	{
		if (!_sunIsMoon || forceDay)
		{
			float a = TaleWorlds.Library.MathF.Sin(TaleWorlds.Library.MathF.Pow((environmentMultiplier - 0.001f) / 0.999f, 1.2f) * ((float)Math.PI / 2f)) * 85f;
			return TaleWorlds.Library.MathF.Min(TaleWorlds.Library.MathF.Max(a, 0.2f), 35f);
		}
		return 0.2f;
	}

	private float GetSunSize(float envMultiplier)
	{
		return 0.1f + (1f - envMultiplier) / 8f;
	}

	private float GetSunRayStrength(float envMultiplier)
	{
		return TaleWorlds.Library.MathF.Min(TaleWorlds.Library.MathF.Max(TaleWorlds.Library.MathF.Sin(TaleWorlds.Library.MathF.Pow((envMultiplier - 0.001f) / 0.999f, 0.4f) * (float)Math.PI / 2f) - 0.15f, 0.01f), 0.5f);
	}

	private float GetEnvironmentMultiplier(SunPosition sunPos)
	{
		float num = ((!_sunIsMoon) ? (sunPos.Altitude / 180f * 2f) : (sunPos.Altitude / 180f * 2f));
		num = ((num > 1f) ? (2f - num) : num);
		num = TaleWorlds.Library.MathF.Pow(num, 0.5f);
		float num2 = 1f - 1f / 90f * sunPos.Angle;
		float num3 = MBMath.ClampFloat(num * num2, 0f, 1f);
		return MBMath.ClampFloat(TaleWorlds.Library.MathF.Min(TaleWorlds.Library.MathF.Sin(num3 * num3) * 2f, 1f), 0f, 1f) * 0.999f + 0.001f;
	}

	private float GetModifiedEnvironmentMultiplier(float envMultiplier)
	{
		float num;
		if (!_sunIsMoon)
		{
			num = (envMultiplier - 0.001f) / 0.999f;
			return num * 0.999f + 0.001f;
		}
		num = (envMultiplier - 0.001f) / 0.999f;
		return num * 0f + 0.001f;
	}

	private float GetSkyBrightness(float hourNorm, float envMultiplier)
	{
		float num = 0f;
		float x = (envMultiplier - 0.001f) / 0.999f;
		if (!_sunIsMoon)
		{
			num = TaleWorlds.Library.MathF.Sin(TaleWorlds.Library.MathF.Pow(x, 1.3f) * ((float)Math.PI / 2f)) * 80f;
			num -= 1f;
			return TaleWorlds.Library.MathF.Min(TaleWorlds.Library.MathF.Max(num, 0.055f), 25f);
		}
		return 0.055f;
	}

	private float GetFogDensity(float environmentMultiplier, Vec3 pos)
	{
		float num = (_sunIsMoon ? 0.5f : 0.4f);
		float num2 = 1f - environmentMultiplier;
		float num3 = 1f - MBMath.ClampFloat((pos.z - 30f) / 200f, 0f, 0.9f);
		return TaleWorlds.Library.MathF.Min((0f + num * num2) * num3, 10f);
	}

	private Vec3 GetFogColor(float environmentMultiplier)
	{
		if (!_sunIsMoon)
		{
			return new Vec3(1f - (1f - environmentMultiplier) / 7f, 0.75f - environmentMultiplier / 4f, 0.55f - environmentMultiplier / 5f);
		}
		Vec3 v = new Vec3(1f - environmentMultiplier * 10f, 0.75f + environmentMultiplier * 1.5f, 0.65f + environmentMultiplier * 2f);
		return Vec3.Vec3Max(v, new Vec3(0.55f, 0.59f, 0.6f));
	}

	private Vec3 GetAmbientFogColor(float moddedEnvMul)
	{
		return Vec3.Vec3Min(new Vec3(0.15f, 0.3f, 0.5f) + new Vec3(moddedEnvMul / 3f, moddedEnvMul / 2f, moddedEnvMul / 1.5f), new Vec3(1f, 1f, 1f));
	}

	private float GetMieScatterStrength(float envMultiplier)
	{
		return (1f + (1f - envMultiplier)) * 10f;
	}

	private float GetRayleighConstant(float envMultiplier)
	{
		float num = (envMultiplier - 0.001f) / 0.999f;
		return TaleWorlds.Library.MathF.Min(TaleWorlds.Library.MathF.Max(1f - TaleWorlds.Library.MathF.Sin(TaleWorlds.Library.MathF.Pow(num, 0.45f) * (float)Math.PI / 2f) + (0.14f + num * 2f), 0.65f), 0.99f);
	}

	private float GetHourOfDay()
	{
		return (float)(CampaignTime.Now.ToHours % 24.0);
	}

	private float GetHourOfDayNormalized()
	{
		return GetHourOfDay() / 24f;
	}

	private float GetNightTimeFactor()
	{
		float num = GetHourOfDay() - 2f;
		for (num %= 24f; num < 0f; num += 24f)
		{
		}
		num = TaleWorlds.Library.MathF.Max(num - 20f, 0f);
		return TaleWorlds.Library.MathF.Min(num / 0.1f, 1f);
	}

	private float GetExposureCoefficientBetweenDayNight()
	{
		float hourOfDay = GetHourOfDay();
		float result = 0f;
		if (hourOfDay > 2f && hourOfDay < 4f)
		{
			result = 1f - (hourOfDay - 2f) / 2f;
		}
		if (hourOfDay < 22f && hourOfDay > 20f)
		{
			result = (hourOfDay - 20f) / 2f;
		}
		if (hourOfDay > 22f || hourOfDay < 2f)
		{
			result = 1f;
		}
		return result;
	}

	private int GetTextureDataIndexForPosition(Vec2 position)
	{
		Vec2 terrainSize = Campaign.Current.MapSceneWrapper.GetTerrainSize();
		int value = TaleWorlds.Library.MathF.Floor(position.x / terrainSize.X * 1024f);
		int value2 = TaleWorlds.Library.MathF.Floor(position.y / terrainSize.Y * 1024f);
		value = MBMath.ClampIndex(value, 0, 1024);
		return MBMath.ClampIndex(value2, 0, 1024) * 1024 + value;
	}

	public (float, float) GetSnowAndRainDataFromTexture(Vec2 position, CampaignTime ct)
	{
		int xIndex;
		int yIndex;
		Vec2 nodePositionForWeather = GetNodePositionForWeather(position, out xIndex, out yIndex);
		int textureDataIndexForPosition = GetTextureDataIndexForPosition(position);
		int textureDataIndexForPosition2 = GetTextureDataIndexForPosition(nodePositionForWeather);
		byte b = _snowAndRainAmountData[textureDataIndexForPosition * 2];
		byte num = _snowAndRainAmountData[textureDataIndexForPosition2 * 2 + 1];
		float value = (float)(int)b / 255f;
		float value2 = (float)(int)num / 255f;
		Campaign.Current.Models.MapWeatherModel.GetSeasonTimeFactorOfCampaignTime(ct, out var timeFactorForSnow, out var timeFactorForRain);
		float num2 = MBMath.Lerp(0.55f, -0.1f, timeFactorForSnow);
		float num3 = MBMath.Lerp(0.7f, 0.3f, timeFactorForRain);
		float num4 = MBMath.SmoothStep(num2 - 0.65f, num2 + 0.65f, value);
		float item = MBMath.SmoothStep(num3 - 0.45f, num3 + 0.45f, value2);
		return (MBMath.Lerp(0f, num4, num4), item);
	}

	public override WeatherEvent GetWeatherEventInPosition(Vec2 pos)
	{
		GetNodePositionForWeather(pos, out var xIndex, out var yIndex);
		return _weatherDataCache[yIndex * 32 + xIndex];
	}

	public override WeatherEventEffectOnTerrain GetWeatherEffectOnTerrainForPosition(Vec2 pos)
	{
		return GetWeatherEventInPosition(pos) switch
		{
			WeatherEvent.Clear => WeatherEventEffectOnTerrain.Default, 
			WeatherEvent.LightRain => WeatherEventEffectOnTerrain.Wet, 
			WeatherEvent.HeavyRain => WeatherEventEffectOnTerrain.Wet, 
			WeatherEvent.Snowy => WeatherEventEffectOnTerrain.Wet, 
			WeatherEvent.Blizzard => WeatherEventEffectOnTerrain.Wet, 
			_ => WeatherEventEffectOnTerrain.Default, 
		};
	}

	private float GetWinterTimeFactor(CampaignTime timeOfYear)
	{
		float result = 0f;
		if (timeOfYear.GetSeasonOfYear == CampaignTime.Seasons.Winter)
		{
			float amount = TaleWorlds.Library.MathF.Abs((float)Math.IEEERemainder(CampaignTime.Now.ToSeasons, 1.0));
			result = MBMath.SplitLerp(0f, 0.75f, 0f, 0.5f, amount, 1E-05f);
		}
		return result;
	}

	private float GetDrynessFactor(CampaignTime timeOfYear)
	{
		float result = 0f;
		float num = TaleWorlds.Library.MathF.Abs((float)Math.IEEERemainder(CampaignTime.Now.ToSeasons, 1.0));
		switch (timeOfYear.GetSeasonOfYear)
		{
		case CampaignTime.Seasons.Summer:
		{
			float amount = MBMath.ClampFloat(num * 2f, 0f, 1f);
			result = MBMath.Lerp(0f, 1f, amount);
			break;
		}
		case CampaignTime.Seasons.Autumn:
			result = 1f;
			break;
		case CampaignTime.Seasons.Winter:
			result = MBMath.Lerp(1f, 0f, num);
			break;
		}
		return result;
	}

	public override void GetSeasonTimeFactorOfCampaignTime(CampaignTime ct, out float timeFactorForSnow, out float timeFactorForRain, bool snapCampaignTimeToWeatherPeriod = true)
	{
		if (snapCampaignTimeToWeatherPeriod)
		{
			ct = CampaignTime.Hours((int)(ct.ToHours / WeatherUpdatePeriod.ToHours / 2.0) * (int)WeatherUpdatePeriod.ToHours * 2);
		}
		float yearProgress = (float)ct.ToSeasons % 4f;
		timeFactorForSnow = CalculateTimeFactorForSnow(yearProgress);
		timeFactorForRain = CalculateTimeFactorForRain(yearProgress);
	}

	private float CalculateTimeFactorForSnow(float yearProgress)
	{
		float result = 0f;
		if (yearProgress > 1.5f && (double)yearProgress <= 3.5)
		{
			result = MBMath.Map(yearProgress, 1.5f, 3.5f, 0f, 1f);
		}
		else if (yearProgress <= 1.5f)
		{
			result = MBMath.Map(yearProgress, 0f, 1.5f, 0.75f, 0f);
		}
		else if (yearProgress > 3.5f)
		{
			result = MBMath.Map(yearProgress, 3.5f, 4f, 1f, 0.75f);
		}
		return result;
	}

	private float CalculateTimeFactorForRain(float yearProgress)
	{
		float result = 0f;
		if (yearProgress > 1f && (double)yearProgress <= 2.5)
		{
			result = MBMath.Map(yearProgress, 1f, 2.5f, 0f, 1f);
		}
		else if (yearProgress <= 1f)
		{
			result = MBMath.Map(yearProgress, 0f, 1f, 1f, 0f);
		}
		else if (yearProgress > 2.5f)
		{
			result = 1f;
		}
		return result;
	}

	private float GetTemperature(ref AtmosphereState gridInfo, float seasonFactor)
	{
		if (gridInfo == null)
		{
			return 0f;
		}
		float temperatureAverage = gridInfo.TemperatureAverage;
		float num = (seasonFactor - 0.5f) * -2f;
		float num2 = gridInfo.TemperatureVariance * num;
		return temperatureAverage + num2;
	}

	private float GetHumidity(ref AtmosphereState gridInfo, float seasonFactor)
	{
		if (gridInfo == null)
		{
			return 0f;
		}
		float humidityAverage = gridInfo.HumidityAverage;
		float num = (seasonFactor - 0.5f) * 2f;
		float num2 = gridInfo.HumidityVariance * num;
		return MBMath.ClampFloat(humidityAverage + num2, 0f, 100f);
	}
}
