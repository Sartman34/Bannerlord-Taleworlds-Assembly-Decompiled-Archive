using System;
using System.Collections.Generic;
using System.Reflection;

namespace TaleWorlds.Library;

public static class Extensions
{
	public static List<Type> GetTypesSafe(this Assembly assembly, Func<Type, bool> func = null)
	{
		List<Type> list = new List<Type>();
		Type[] array;
		try
		{
			array = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			array = ex.Types;
			Debug.Print(ex.Message + " " + ex.GetType());
			foreach (object value in ex.Data.Values)
			{
				Debug.Print(value.ToString());
			}
		}
		catch (Exception ex2)
		{
			array = new Type[0];
			Debug.Print(ex2.Message);
		}
		foreach (Type type in array)
		{
			if (type != null && (func == null || func(type)))
			{
				list.Add(type);
			}
		}
		return list;
	}

	public static object[] GetCustomAttributesSafe(this Type type, Type attributeType, bool inherit)
	{
		try
		{
			return type.GetCustomAttributes(attributeType, inherit);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes (" + attributeType.Name + ") for type: " + type.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 59);
		}
		return new object[0];
	}

	public static object[] GetCustomAttributesSafe(this Type type, bool inherit)
	{
		try
		{
			return type.GetCustomAttributes(inherit);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for type: " + type.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 74);
		}
		return new object[0];
	}

	public static IEnumerable<Attribute> GetCustomAttributesSafe(this Type type, Type attributeType)
	{
		try
		{
			return type.GetCustomAttributes(attributeType);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes (" + attributeType.Name + ") for type: " + type.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 89);
		}
		return new List<Attribute>();
	}

	public static object[] GetCustomAttributesSafe(this PropertyInfo property, Type attributeType, bool inherit)
	{
		try
		{
			return property.GetCustomAttributes(attributeType, inherit);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes (" + attributeType.Name + ") for property: " + property.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 104);
		}
		return new object[0];
	}

	public static object[] GetCustomAttributesSafe(this PropertyInfo property, bool inherit)
	{
		try
		{
			return property.GetCustomAttributes(inherit);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for property: " + property.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 119);
		}
		return new object[0];
	}

	public static IEnumerable<Attribute> GetCustomAttributesSafe(this PropertyInfo property, Type attributeType)
	{
		try
		{
			return property.GetCustomAttributes(attributeType);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for property: " + property.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 134);
		}
		return new List<Attribute>();
	}

	public static object[] GetCustomAttributesSafe(this FieldInfo field, Type attributeType, bool inherit)
	{
		try
		{
			return field.GetCustomAttributes(attributeType, inherit: false);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes (" + attributeType.Name + ") for field: " + field.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 149);
		}
		return new object[0];
	}

	public static object[] GetCustomAttributesSafe(this FieldInfo field, bool inherit)
	{
		try
		{
			return field.GetCustomAttributes(inherit);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for field: " + field.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 164);
		}
		return new object[0];
	}

	public static IEnumerable<Attribute> GetCustomAttributesSafe(this FieldInfo field, Type attributeType)
	{
		try
		{
			return field.GetCustomAttributes(attributeType);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for field: " + field.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 179);
		}
		return new List<Attribute>();
	}

	public static object[] GetCustomAttributesSafe(this MethodInfo method, Type attributeType, bool inherit)
	{
		try
		{
			return method.GetCustomAttributes(attributeType, inherit: false);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes (" + attributeType.Name + ") for method: " + method.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 194);
		}
		return new object[0];
	}

	public static object[] GetCustomAttributesSafe(this MethodInfo method, bool inherit)
	{
		try
		{
			return method.GetCustomAttributes(inherit);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for method: " + method.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 209);
		}
		return new object[0];
	}

	public static IEnumerable<Attribute> GetCustomAttributesSafe(this MethodInfo method, Type attributeType)
	{
		try
		{
			return method.GetCustomAttributes(attributeType);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for method: " + method.Name + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 224);
		}
		return new List<Attribute>();
	}

	public static object[] GetCustomAttributesSafe(this Assembly assembly, Type attributeType, bool inherit)
	{
		try
		{
			return assembly.GetCustomAttributes(attributeType, inherit: false);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes (" + attributeType.Name + ") for assembly: " + assembly.FullName + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 239);
		}
		return new object[0];
	}

	public static object[] GetCustomAttributesSafe(this Assembly assembly, bool inherit)
	{
		try
		{
			return assembly.GetCustomAttributes(inherit);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for assembly: " + assembly.FullName + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 254);
		}
		return new object[0];
	}

	public static IEnumerable<Attribute> GetCustomAttributesSafe(this Assembly assembly, Type attributeType)
	{
		try
		{
			return assembly.GetCustomAttributes(attributeType);
		}
		catch (Exception ex)
		{
			Debug.FailedAssert("Failed to get custom attributes for assembly: " + assembly.FullName + ". Exception: " + ex.Message, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\Extensions.cs", "GetCustomAttributesSafe", 269);
		}
		return new List<Attribute>();
	}

	public static MBList<T> ToMBList<T>(this T[] source)
	{
		MBList<T> mBList = new MBList<T>(source.Length);
		mBList.AddRange(source);
		return mBList;
	}

	public static MBList<T> ToMBList<T>(this List<T> source)
	{
		MBList<T> mBList = new MBList<T>(source.Count);
		mBList.AddRange(source);
		return mBList;
	}

	public static MBList<T> ToMBList<T>(this IEnumerable<T> source)
	{
		if (source is T[] source2)
		{
			return source2.ToMBList();
		}
		if (source is List<T> source3)
		{
			return source3.ToMBList();
		}
		MBList<T> mBList = new MBList<T>();
		mBList.AddRange(source);
		return mBList;
	}

	public static void AppendList<T>(this List<T> list1, List<T> list2)
	{
		if (list1.Count + list2.Count > list1.Capacity)
		{
			list1.Capacity = list1.Count + list2.Count;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			list1.Add(list2[i]);
		}
	}

	public static MBReadOnlyDictionary<TKey, TValue> GetReadOnlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
	{
		return new MBReadOnlyDictionary<TKey, TValue>(dictionary);
	}

	public static bool HasAnyFlag<T>(this T p1, T p2) where T : struct
	{
		return EnumHelper<T>.HasAnyFlag(p1, p2);
	}

	public static bool HasAllFlags<T>(this T p1, T p2) where T : struct
	{
		return EnumHelper<T>.HasAllFlags(p1, p2);
	}

	public static int GetDeterministicHashCode(this string text)
	{
		int num = 5381;
		for (int i = 0; i < text.Length; i++)
		{
			num = (num << 5) + num + text[i];
		}
		return num;
	}

	public static int IndexOfMin<TSource>(this IReadOnlyList<TSource> self, Func<TSource, int> func)
	{
		int num = int.MaxValue;
		int result = -1;
		for (int i = 0; i < self.Count; i++)
		{
			int num2 = func(self[i]);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static int IndexOfMin<TSource>(this MBReadOnlyList<TSource> self, Func<TSource, int> func)
	{
		int num = int.MaxValue;
		int result = -1;
		for (int i = 0; i < self.Count; i++)
		{
			int num2 = func(self[i]);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static int IndexOfMax<TSource>(this IReadOnlyList<TSource> self, Func<TSource, int> func)
	{
		int num = int.MinValue;
		int result = -1;
		for (int i = 0; i < self.Count; i++)
		{
			int num2 = func(self[i]);
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static int IndexOfMax<TSource>(this MBReadOnlyList<TSource> self, Func<TSource, int> func)
	{
		int num = int.MinValue;
		int result = -1;
		for (int i = 0; i < self.Count; i++)
		{
			int num2 = func(self[i]);
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static int IndexOf<TValue>(this TValue[] source, TValue item)
	{
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i].Equals(item))
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindIndex<TValue>(this IReadOnlyList<TValue> source, Func<TValue, bool> predicate)
	{
		for (int i = 0; i < source.Count; i++)
		{
			if (predicate(source[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindIndex<TValue>(this MBReadOnlyList<TValue> source, Func<TValue, bool> predicate)
	{
		for (int i = 0; i < source.Count; i++)
		{
			if (predicate(source[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindLastIndex<TValue>(this IReadOnlyList<TValue> source, Func<TValue, bool> predicate)
	{
		for (int num = source.Count - 1; num >= 0; num--)
		{
			if (predicate(source[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public static int FindLastIndex<TValue>(this MBReadOnlyList<TValue> source, Func<TValue, bool> predicate)
	{
		for (int num = source.Count - 1; num >= 0; num--)
		{
			if (predicate(source[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public static void Randomize<T>(this IList<T> array)
	{
		Random random = new Random();
		int num = array.Count;
		while (num > 1)
		{
			num--;
			int index = random.Next(0, num + 1);
			T value = array[index];
			array[index] = array[num];
			array[num] = value;
		}
	}
}
