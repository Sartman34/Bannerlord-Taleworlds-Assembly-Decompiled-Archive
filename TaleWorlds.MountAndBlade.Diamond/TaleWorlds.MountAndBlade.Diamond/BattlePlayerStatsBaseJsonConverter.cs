using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaleWorlds.MountAndBlade.Diamond;

public class BattlePlayerStatsBaseJsonConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return typeof(BattlePlayerStatsBase).IsAssignableFrom(objectType);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)reader.TokenType == 11)
		{
			return null;
		}
		JObject val = JObject.Load(reader);
		BattlePlayerStatsBase battlePlayerStatsBase;
		switch ((string)val["GameType"])
		{
		case "Skirmish":
			battlePlayerStatsBase = new BattlePlayerStatsSkirmish();
			break;
		case "Captain":
			battlePlayerStatsBase = new BattlePlayerStatsCaptain();
			break;
		case "Siege":
			battlePlayerStatsBase = new BattlePlayerStatsSiege();
			break;
		case "TeamDeathmatch":
			battlePlayerStatsBase = new BattlePlayerStatsTeamDeathmatch();
			break;
		case "Duel":
			battlePlayerStatsBase = new BattlePlayerStatsDuel();
			break;
		case "Battle":
			battlePlayerStatsBase = new BattlePlayerStatsBattle();
			break;
		default:
			return null;
		}
		serializer.Populate(((JToken)val).CreateReader(), (object)battlePlayerStatsBase);
		return battlePlayerStatsBase;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
	}
}
