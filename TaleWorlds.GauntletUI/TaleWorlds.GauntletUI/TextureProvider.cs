using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI;

public abstract class TextureProvider
{
	private Dictionary<string, MethodInfo> _getGetMethodCache = new Dictionary<string, MethodInfo>();

	public virtual void SetTargetSize(int width, int height)
	{
	}

	public abstract Texture GetTexture(TwoDimensionContext twoDimensionContext, string name);

	public virtual void Tick(float dt)
	{
	}

	public virtual void Clear(bool clearNextFrame)
	{
		_getGetMethodCache.Clear();
	}

	public void SetProperty(string name, object value)
	{
		PropertyInfo property = GetType().GetProperty(name);
		if (property != null)
		{
			property.GetSetMethod().Invoke(this, new object[1] { value });
		}
	}

	public object GetProperty(string name)
	{
		if (_getGetMethodCache.TryGetValue(name, out var value))
		{
			return value.Invoke(this, null);
		}
		PropertyInfo property = GetType().GetProperty(name);
		if (property != null)
		{
			MethodInfo getMethod = property.GetGetMethod();
			_getGetMethodCache.Add(name, getMethod);
			return getMethod.Invoke(this, null);
		}
		return null;
	}
}
