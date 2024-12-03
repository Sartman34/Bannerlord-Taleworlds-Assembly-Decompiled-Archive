using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace TaleWorlds.Diamond;

public class MessageJsonConverter : JsonConverter
{
	private static readonly Dictionary<string, Type> _knownTypes;

	public override bool CanWrite => true;

	static MessageJsonConverter()
	{
		_knownTypes = (from t in (from a in AppDomain.CurrentDomain.GetAssemblies()
				where !a.GlobalAssemblyCache
				select a).SelectMany((Assembly a) => a.GetTypes())
			where t.IsSubclassOf(typeof(Message))
			select t).ToDictionary((Type item) => item.FullName, (Type item) => item);
	}

	public override bool CanConvert(Type objectType)
	{
		return typeof(Message).IsAssignableFrom(objectType);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		serializer.Error += OnSerializerError;
		JObject val = JObject.Load(reader);
		string text = (string)val["_type"];
		if (text.StartsWith("Messages.") && _knownTypes.TryGetValue(text, out var value))
		{
			Message message = (Message)Activator.CreateInstance(value);
			serializer.Populate(((JToken)val).CreateReader(), (object)message);
			return message;
		}
		return null;
	}

	private void OnSerializerError(object sender, ErrorEventArgs e)
	{
		if (e.ErrorContext != null)
		{
			_ = e.ErrorContext.Error;
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		JProperty val = new JProperty("_type", (object)value.GetType().FullName);
		JObject val2 = new JObject();
		((JContainer)val2).Add((object)val);
		PropertyInfo[] properties = value.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanRead)
			{
				object value2 = propertyInfo.GetValue(value);
				if (value2 != null)
				{
					val2.Add(propertyInfo.Name, JToken.FromObject(value2, serializer));
				}
			}
		}
		((JToken)val2).WriteTo(writer, Array.Empty<JsonConverter>());
	}
}
