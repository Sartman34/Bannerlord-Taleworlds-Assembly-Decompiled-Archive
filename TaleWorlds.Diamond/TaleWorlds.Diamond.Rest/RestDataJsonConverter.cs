using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaleWorlds.Diamond.Rest;

public class RestDataJsonConverter : JsonConverter<RestData>
{
	public override bool CanWrite => false;

	public override bool CanRead => true;

	private RestData Create(Type objectType, JObject jObject)
	{
		if (jObject == null)
		{
			throw new ArgumentNullException("jObject");
		}
		string text = null;
		if (jObject["TypeName"] != null)
		{
			text = Extensions.Value<string>((IEnumerable<JToken>)jObject["TypeName"]);
		}
		else if (jObject["typeName"] != null)
		{
			text = Extensions.Value<string>((IEnumerable<JToken>)jObject["typeName"]);
		}
		if (text != null)
		{
			return Activator.CreateInstance(Type.GetType(text)) as RestData;
		}
		return null;
	}

	public T ReadJson<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json);
	}

	public override RestData ReadJson(JsonReader reader, Type objectType, RestData existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (serializer == null)
		{
			throw new ArgumentNullException("serializer");
		}
		if ((int)reader.TokenType == 11)
		{
			return null;
		}
		JObject val = JObject.Load(reader);
		RestData restData = Create(objectType, val);
		serializer.Populate(((JToken)val).CreateReader(), (object)restData);
		return restData;
	}

	public override void WriteJson(JsonWriter writer, RestData value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
