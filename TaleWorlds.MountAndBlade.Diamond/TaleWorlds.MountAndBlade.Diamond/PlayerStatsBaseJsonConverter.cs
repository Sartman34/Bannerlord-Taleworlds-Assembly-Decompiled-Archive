using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaleWorlds.Diamond;

namespace TaleWorlds.MountAndBlade.Diamond;

public class PlayerStatsBaseJsonConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return typeof(AccessObject).IsAssignableFrom(objectType);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject val = JObject.Load(reader);
		string text = (string)val["gameType"];
		if (text == null)
		{
			text = (string)val["GameType"];
		}
		PlayerStatsBase playerStatsBase;
		switch (text)
		{
		case "Skirmish":
			playerStatsBase = new PlayerStatsSkirmish();
			break;
		case "Captain":
			playerStatsBase = new PlayerStatsCaptain();
			break;
		case "TeamDeathmatch":
			playerStatsBase = new PlayerStatsTeamDeathmatch();
			break;
		case "Siege":
			playerStatsBase = new PlayerStatsSiege();
			break;
		case "Duel":
			playerStatsBase = new PlayerStatsDuel();
			break;
		case "Battle":
			playerStatsBase = new PlayerStatsBattle();
			break;
		default:
			return null;
		}
		serializer.Populate(((JToken)val).CreateReader(), (object)playerStatsBase);
		return playerStatsBase;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
	}
}
