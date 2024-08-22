using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

public static class ManagedExtensions
{
	[EngineCallback]
	internal static void SetObjectField(DotNetObject managedObject, string fieldName, ref ScriptComponentFieldHolder scriptComponentHolder, int type, int callFieldChangeEventAsInteger)
	{
		bool flag = callFieldChangeEventAsInteger != 0;
		FieldInfo fieldOfClass = Managed.GetFieldOfClass(managedObject.GetType().Name, fieldName);
		switch (type)
		{
		case 0:
			fieldOfClass.SetValue(managedObject, scriptComponentHolder.s);
			break;
		case 1:
			fieldOfClass.SetValue(managedObject, Convert.ChangeType(scriptComponentHolder.d, fieldOfClass.FieldType));
			break;
		case 2:
			fieldOfClass.SetValue(managedObject, Convert.ChangeType(scriptComponentHolder.f, fieldOfClass.FieldType));
			break;
		case 3:
		{
			bool flag2 = scriptComponentHolder.b > 0;
			fieldOfClass.SetValue(managedObject, flag2);
			break;
		}
		case 9:
		{
			object value = Enum.Parse(fieldOfClass.FieldType, scriptComponentHolder.enumValue);
			fieldOfClass.SetValue(managedObject, value);
			break;
		}
		case 13:
		{
			MatrixFrame matrixFrame = scriptComponentHolder.matrixFrame;
			fieldOfClass.SetValue(managedObject, matrixFrame);
			break;
		}
		case 4:
			fieldOfClass.SetValue(managedObject, Convert.ChangeType(scriptComponentHolder.i, fieldOfClass.FieldType));
			break;
		case 5:
		{
			Vec3 vec = new Vec3(scriptComponentHolder.v3.x, scriptComponentHolder.v3.y, scriptComponentHolder.v3.z, scriptComponentHolder.v3.w);
			fieldOfClass.SetValue(managedObject, vec);
			break;
		}
		case 6:
			fieldOfClass.SetValue(managedObject, (scriptComponentHolder.entityPointer != UIntPtr.Zero) ? new GameEntity(scriptComponentHolder.entityPointer) : null);
			break;
		case 7:
			fieldOfClass.SetValue(managedObject, (scriptComponentHolder.texturePointer != UIntPtr.Zero) ? new Texture(scriptComponentHolder.texturePointer) : null);
			break;
		case 8:
			fieldOfClass.SetValue(managedObject, (scriptComponentHolder.meshPointer != UIntPtr.Zero) ? new MetaMesh(scriptComponentHolder.meshPointer) : null);
			break;
		case 10:
			fieldOfClass.SetValue(managedObject, (scriptComponentHolder.materialPointer != UIntPtr.Zero) ? new Material(scriptComponentHolder.materialPointer) : null);
			break;
		}
		if (type != 11 && flag && managedObject is ScriptComponentBehavior)
		{
			((ScriptComponentBehavior)managedObject).OnEditorVariableChanged(fieldName);
		}
	}

	[EngineCallback]
	internal static void GetObjectField(DotNetObject managedObject, ref ScriptComponentFieldHolder scriptComponentFieldHolder, string fieldName, int type)
	{
		FieldInfo fieldOfClass = Managed.GetFieldOfClass(managedObject.GetType().Name, fieldName);
		switch (type)
		{
		case 0:
			scriptComponentFieldHolder.s = (string)fieldOfClass.GetValue(managedObject);
			break;
		case 1:
			scriptComponentFieldHolder.d = (double)Convert.ChangeType(fieldOfClass.GetValue(managedObject), typeof(double));
			break;
		case 2:
			scriptComponentFieldHolder.f = (float)Convert.ChangeType(fieldOfClass.GetValue(managedObject), typeof(float));
			break;
		case 3:
		{
			bool flag = (bool)fieldOfClass.GetValue(managedObject);
			scriptComponentFieldHolder.b = (flag ? 1 : 0);
			break;
		}
		case 4:
			scriptComponentFieldHolder.i = (int)Convert.ChangeType(fieldOfClass.GetValue(managedObject), typeof(int));
			break;
		case 5:
		{
			Vec3 c = (Vec3)fieldOfClass.GetValue(managedObject);
			scriptComponentFieldHolder.v3 = new Vec3(c, c.w);
			break;
		}
		case 13:
		{
			MatrixFrame matrixFrame = (MatrixFrame)fieldOfClass.GetValue(managedObject);
			scriptComponentFieldHolder.matrixFrame = matrixFrame;
			break;
		}
		case 6:
		{
			GameEntity gameEntity = (GameEntity)fieldOfClass.GetValue(managedObject);
			scriptComponentFieldHolder.entityPointer = ((gameEntity != null) ? ((UIntPtr)Convert.ChangeType(gameEntity.Pointer, typeof(UIntPtr))) : ((UIntPtr)0uL));
			break;
		}
		case 7:
		{
			Texture texture = (Texture)fieldOfClass.GetValue(managedObject);
			scriptComponentFieldHolder.texturePointer = ((texture != null) ? ((UIntPtr)Convert.ChangeType(texture.Pointer, typeof(UIntPtr))) : ((UIntPtr)0uL));
			break;
		}
		case 8:
		{
			MetaMesh metaMesh = (MetaMesh)fieldOfClass.GetValue(managedObject);
			scriptComponentFieldHolder.meshPointer = ((metaMesh != null) ? ((UIntPtr)Convert.ChangeType(metaMesh.Pointer, typeof(UIntPtr))) : ((UIntPtr)0uL));
			break;
		}
		case 9:
			scriptComponentFieldHolder.enumValue = fieldOfClass.GetValue(managedObject).ToString();
			break;
		case 10:
		{
			Material material = (Material)fieldOfClass.GetValue(managedObject);
			scriptComponentFieldHolder.materialPointer = ((material != null) ? ((UIntPtr)Convert.ChangeType(material.Pointer, typeof(UIntPtr))) : ((UIntPtr)0uL));
			break;
		}
		case 11:
		case 12:
			break;
		}
	}

	[EngineCallback]
	internal static void CopyObjectFieldsFrom(DotNetObject dst, DotNetObject src, string className, int callFieldChangeEventAsInteger)
	{
		bool flag = callFieldChangeEventAsInteger != 0;
		foreach (KeyValuePair<string, FieldInfo> item in Managed.GetEditableFieldsOfClass(className))
		{
			FieldInfo value = item.Value;
			value.SetValue(dst, value.GetValue(src));
			if (flag && dst is ScriptComponentBehavior)
			{
				((ScriptComponentBehavior)dst).OnEditorVariableChanged(item.Key);
			}
		}
	}

	[EngineCallback]
	internal static DotNetObject CreateScriptComponentInstance(string className, GameEntity entity, ManagedScriptComponent managedScriptComponent)
	{
		ScriptComponentBehavior scriptComponentBehavior = null;
		Func<ScriptComponentBehavior> func = (Func<ScriptComponentBehavior>)Managed.GetConstructorDelegateOfClass(className);
		if (func != null)
		{
			scriptComponentBehavior = func();
			scriptComponentBehavior?.Construct(entity, managedScriptComponent);
		}
		else
		{
			ConstructorInfo constructorOfClass = Managed.GetConstructorOfClass(className);
			if (constructorOfClass != null)
			{
				scriptComponentBehavior = constructorOfClass.Invoke(new object[0]) as ScriptComponentBehavior;
				scriptComponentBehavior?.Construct(entity, managedScriptComponent);
			}
			else
			{
				MBDebug.ShowWarning("CreateScriptComponentInstance failed: " + className);
			}
		}
		return scriptComponentBehavior;
	}

	[EngineCallback]
	internal static string GetScriptComponentClassNames()
	{
		List<Type> list = new List<Type>();
		foreach (Type value in Managed.ModuleTypes.Values)
		{
			if (!value.IsAbstract && typeof(ScriptComponentBehavior).IsAssignableFrom(value))
			{
				list.Add(value);
			}
		}
		string text = "";
		for (int i = 0; i < list.Count; i++)
		{
			Type type = list[i];
			text += type.Name;
			text += "-";
			text += type.BaseType.Name;
			if (i + 1 != list.Count)
			{
				text += " ";
			}
		}
		return text;
	}

	[EngineCallback]
	internal static bool GetEditorVisibilityOfField(string className, string fieldName)
	{
		object[] customAttributesSafe = Managed.GetFieldOfClass(className, fieldName).GetCustomAttributesSafe(typeof(EditorVisibleScriptComponentVariable), inherit: true);
		if (customAttributesSafe.Length != 0)
		{
			return (customAttributesSafe[0] as EditorVisibleScriptComponentVariable).Visible;
		}
		return true;
	}

	[EngineCallback]
	internal static int GetTypeOfField(string className, string fieldName)
	{
		FieldInfo fieldOfClass = Managed.GetFieldOfClass(className, fieldName);
		if (fieldOfClass == null)
		{
			return -1;
		}
		Type fieldType = fieldOfClass.FieldType;
		if (fieldOfClass.FieldType == typeof(string))
		{
			return 0;
		}
		if (fieldOfClass.FieldType == typeof(double))
		{
			return 1;
		}
		if (fieldOfClass.FieldType.IsEnum)
		{
			return 9;
		}
		if (fieldOfClass.FieldType == typeof(float))
		{
			return 2;
		}
		if (fieldOfClass.FieldType == typeof(bool))
		{
			return 3;
		}
		if (fieldType == typeof(byte) || fieldType == typeof(sbyte) || fieldType == typeof(short) || fieldType == typeof(ushort) || fieldType == typeof(int) || fieldType == typeof(uint) || fieldType == typeof(long) || fieldType == typeof(ulong))
		{
			return 4;
		}
		if (fieldOfClass.FieldType == typeof(Vec3))
		{
			return 5;
		}
		if (fieldOfClass.FieldType == typeof(GameEntity))
		{
			return 6;
		}
		if (fieldOfClass.FieldType == typeof(Texture))
		{
			return 7;
		}
		if (fieldOfClass.FieldType == typeof(MetaMesh))
		{
			return 8;
		}
		if (fieldOfClass.FieldType == typeof(Material))
		{
			return 10;
		}
		if (fieldOfClass.FieldType == typeof(SimpleButton))
		{
			return 11;
		}
		if (fieldOfClass.FieldType == typeof(MatrixFrame))
		{
			return 13;
		}
		return -1;
	}

	[EngineCallback]
	internal static void ForceGarbageCollect()
	{
		Utilities.FlushManagedObjectsMemory();
	}

	[EngineCallback]
	internal static void CollectCommandLineFunctions()
	{
		foreach (string item in CommandLineFunctionality.CollectCommandLineFunctions())
		{
			Utilities.AddCommandLineFunction(item);
		}
	}
}
