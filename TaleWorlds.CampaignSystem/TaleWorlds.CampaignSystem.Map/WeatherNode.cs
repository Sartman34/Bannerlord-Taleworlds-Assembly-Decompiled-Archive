using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.Map;

public class WeatherNode
{
	public Vec2 Position;

	public MapWeatherModel.WeatherEvent CurrentWeatherEvent;

	public bool IsVisuallyDirty { get; private set; }

	public WeatherNode(Vec2 position)
	{
		Position = position;
		CurrentWeatherEvent = MapWeatherModel.WeatherEvent.Clear;
	}

	public void SetVisualDirty()
	{
		IsVisuallyDirty = true;
	}

	public void OnVisualUpdated()
	{
		IsVisuallyDirty = false;
	}
}
