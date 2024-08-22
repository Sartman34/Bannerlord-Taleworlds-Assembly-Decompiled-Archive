using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

public class GameStateScreenManager : IGameStateManagerListener
{
	private Dictionary<Type, Type> _screenTypes;

	private GameStateManager GameStateManager => GameStateManager.Current;

	public GameStateScreenManager()
	{
		_screenTypes = new Dictionary<Type, Type>();
		Assembly[] viewAssemblies = GetViewAssemblies();
		Assembly assembly = typeof(GameStateScreen).Assembly;
		CheckAssemblyScreens(assembly);
		Assembly[] array = viewAssemblies;
		foreach (Assembly assembly2 in array)
		{
			CheckAssemblyScreens(assembly2);
		}
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
	{
		if (changedManagedOptionsType == ManagedOptions.ManagedOptionsType.ForceVSyncInMenus)
		{
			if (!BannerlordConfig.ForceVSyncInMenus)
			{
				Utilities.SetForceVsync(value: false);
			}
			else if (GameStateManager.ActiveState.IsMenuState)
			{
				Utilities.SetForceVsync(value: true);
			}
		}
	}

	private void CheckAssemblyScreens(Assembly assembly)
	{
		foreach (Type item in assembly.GetTypesSafe())
		{
			object[] customAttributesSafe = item.GetCustomAttributesSafe(typeof(GameStateScreen), inherit: false);
			if (customAttributesSafe == null || customAttributesSafe.Length == 0)
			{
				continue;
			}
			object[] array = customAttributesSafe;
			for (int i = 0; i < array.Length; i++)
			{
				GameStateScreen gameStateScreen = (GameStateScreen)array[i];
				if (_screenTypes.ContainsKey(gameStateScreen.GameStateType))
				{
					_screenTypes[gameStateScreen.GameStateType] = item;
				}
				else
				{
					_screenTypes.Add(gameStateScreen.GameStateType, item);
				}
			}
		}
	}

	public static Assembly[] GetViewAssemblies()
	{
		List<Assembly> list = new List<Assembly>();
		Assembly assembly = typeof(GameStateScreen).Assembly;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly2 in assemblies)
		{
			AssemblyName[] referencedAssemblies = assembly2.GetReferencedAssemblies();
			for (int j = 0; j < referencedAssemblies.Length; j++)
			{
				if (referencedAssemblies[j].ToString() == assembly.GetName().ToString())
				{
					list.Add(assembly2);
					break;
				}
			}
		}
		return list.ToArray();
	}

	public ScreenBase CreateScreen(GameState state)
	{
		Type value = null;
		if (_screenTypes.TryGetValue(state.GetType(), out value))
		{
			return Activator.CreateInstance(value, state) as ScreenBase;
		}
		return null;
	}

	public void BuildScreens()
	{
		int num = 0;
		foreach (GameState gameState in GameStateManager.GameStates)
		{
			ScreenBase screenBase = CreateScreen(gameState);
			gameState.RegisterListener(screenBase as IGameStateListener);
			if (screenBase != null)
			{
				if (num == 0)
				{
					ScreenManager.CleanAndPushScreen(screenBase);
				}
				else
				{
					ScreenManager.PushScreen(screenBase);
				}
			}
			num++;
		}
	}

	void IGameStateManagerListener.OnCreateState(GameState gameState)
	{
		ScreenBase screenBase = CreateScreen(gameState);
		gameState.RegisterListener(screenBase as IGameStateListener);
	}

	void IGameStateManagerListener.OnPushState(GameState gameState, bool isTopGameState)
	{
		if (!gameState.IsMenuState)
		{
			Utilities.ClearOldResourcesAndObjects();
		}
		if (gameState.IsMenuState && BannerlordConfig.ForceVSyncInMenus)
		{
			Utilities.SetForceVsync(value: true);
		}
		else if (!gameState.IsMenuState)
		{
			Utilities.SetForceVsync(value: false);
		}
		ScreenBase listenerOfType;
		if ((listenerOfType = gameState.GetListenerOfType<ScreenBase>()) != null)
		{
			if (isTopGameState)
			{
				ScreenManager.CleanAndPushScreen(listenerOfType);
			}
			else
			{
				ScreenManager.PushScreen(listenerOfType);
			}
		}
	}

	void IGameStateManagerListener.OnPopState(GameState gameState)
	{
		if (!gameState.IsMenuState)
		{
			Utilities.ClearOldResourcesAndObjects();
		}
		if (gameState.IsMenuState && BannerlordConfig.ForceVSyncInMenus)
		{
			Utilities.SetForceVsync(value: false);
		}
		if (GameStateManager.ActiveState != null && GameStateManager.ActiveState.IsMenuState && BannerlordConfig.ForceVSyncInMenus)
		{
			Utilities.SetForceVsync(value: true);
		}
		ScreenManager.PopScreen();
	}

	void IGameStateManagerListener.OnCleanStates()
	{
		ScreenManager.CleanScreens();
	}

	void IGameStateManagerListener.OnSavedGameLoadFinished()
	{
		BuildScreens();
	}
}
