using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem.Definition;
using TaleWorlds.SaveSystem.Load;
using TaleWorlds.SaveSystem.Save;

namespace TaleWorlds.SaveSystem;

public static class SaveManager
{
	public const string SaveFileExtension = "sav";

	private const int CurrentVersion = 1;

	private static DefinitionContext _definitionContext;

	public static void InitializeGlobalDefinitionContext()
	{
		_definitionContext = new DefinitionContext();
		_definitionContext.FillWithCurrentTypes();
	}

	public static List<Type> CheckSaveableTypes()
	{
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			foreach (Type item in assemblies[i].GetTypesSafe())
			{
				PropertyInfo[] properties = item.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				FieldInfo[] fields = item.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					Attribute[] array = fieldInfo.GetCustomAttributesSafe(typeof(SaveableFieldAttribute)).ToArray();
					if (array.Length != 0)
					{
						_ = (SaveableFieldAttribute)array[0];
						Type fieldType = fieldInfo.FieldType;
						if (!_definitionContext.HasDefinition(fieldType) && !list.Contains(fieldType) && !fieldType.IsInterface && fieldType.FullName != null)
						{
							list.Add(fieldType);
						}
					}
				}
				PropertyInfo[] array2 = properties;
				foreach (PropertyInfo propertyInfo in array2)
				{
					Attribute[] array3 = propertyInfo.GetCustomAttributesSafe(typeof(SaveablePropertyAttribute)).ToArray();
					if (array3.Length != 0)
					{
						_ = (SaveablePropertyAttribute)array3[0];
						Type propertyType = propertyInfo.PropertyType;
						if (!_definitionContext.HasDefinition(propertyType) && !list.Contains(propertyType) && !propertyType.IsInterface && propertyType.FullName != null)
						{
							list.Add(propertyType);
						}
					}
				}
			}
		}
		return list;
	}

	public static SaveOutput Save(object target, MetaData metaData, string saveName, ISaveDriver driver)
	{
		if (_definitionContext == null)
		{
			InitializeGlobalDefinitionContext();
		}
		SaveOutput saveOutput = null;
		if (_definitionContext.GotError)
		{
			List<SaveError> list = new List<SaveError>();
			foreach (string error in _definitionContext.Errors)
			{
				list.Add(new SaveError(error));
			}
			return SaveOutput.CreateFailed(list, SaveResult.GeneralFailure);
		}
		Debug.Print("------Saving with new context. Save name: " + saveName + "------");
		SaveContext saveContext;
		if ((saveContext = new SaveContext(_definitionContext)).Save(target, metaData, out var errorMessage))
		{
			try
			{
				Task<SaveResultWithMessage> task = driver.Save(saveName, 1, metaData, saveContext.SaveData);
				if (task.IsCompleted)
				{
					if (task.Result.SaveResult == SaveResult.Success)
					{
						return SaveOutput.CreateSuccessful(saveContext.SaveData);
					}
					return SaveOutput.CreateFailed(new SaveError[1]
					{
						new SaveError(task.Result.Message)
					}, task.Result.SaveResult);
				}
				return SaveOutput.CreateContinuing(task);
			}
			catch (Exception ex)
			{
				return SaveOutput.CreateFailed(new SaveError[1]
				{
					new SaveError(ex.Message)
				}, SaveResult.GeneralFailure);
			}
		}
		return SaveOutput.CreateFailed(new SaveError[1]
		{
			new SaveError(errorMessage)
		}, SaveResult.GeneralFailure);
	}

	public static MetaData LoadMetaData(string saveName, ISaveDriver driver)
	{
		return driver.LoadMetaData(saveName);
	}

	public static LoadResult Load(string saveName, ISaveDriver driver)
	{
		return Load(saveName, driver, loadAsLateInitialize: false);
	}

	public static LoadResult Load(string saveName, ISaveDriver driver, bool loadAsLateInitialize)
	{
		DefinitionContext definitionContext = new DefinitionContext();
		definitionContext.FillWithCurrentTypes();
		LoadResult loadResult = null;
		LoadContext loadContext = new LoadContext(definitionContext, driver);
		LoadData loadData = driver.Load(saveName);
		if (loadContext.Load(loadData, loadAsLateInitialize))
		{
			LoadCallbackInitializator loadCallbackInitializator = null;
			if (loadAsLateInitialize)
			{
				loadCallbackInitializator = loadContext.CreateLoadCallbackInitializator(loadData);
			}
			return LoadResult.CreateSuccessful(loadContext.RootObject, loadData.MetaData, loadCallbackInitializator);
		}
		return LoadResult.CreateFailed(new LoadError[1]
		{
			new LoadError("Not implemented")
		});
	}
}
