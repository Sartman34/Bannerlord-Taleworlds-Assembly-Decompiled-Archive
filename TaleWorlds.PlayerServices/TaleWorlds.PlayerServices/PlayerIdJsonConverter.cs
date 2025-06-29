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
		JProperty content = new JProperty("_playerId", ((PlayerId)value).ToString());
		JObject jObject = new JObject();
		jObject.Add(content);
		jObject.WriteTo(writer);
	}
}
