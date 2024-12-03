using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaleWorlds.PlayerServices;

public class PlayerIdJsonConverter : JsonConverter
{
	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return typeof(PlayerId).IsAssignableFrom(objectType);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return PlayerId.FromString((string)JObject.Load(reader)["_playerId"]);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		JProperty val = new JProperty("_playerId", (object)((PlayerId)value).ToString());
		JObject val2 = new JObject();
		((JContainer)val2).Add((object)val);
		((JToken)val2).WriteTo(writer, Array.Empty<JsonConverter>());
	}
}
