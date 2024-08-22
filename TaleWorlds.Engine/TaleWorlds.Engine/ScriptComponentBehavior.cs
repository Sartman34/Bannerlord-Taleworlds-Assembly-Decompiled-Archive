using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

public abstract class ScriptComponentBehavior : DotNetObject
{
	[Flags]
	public enum TickRequirement : uint
	{
		None = 0u,
		TickOccasionally = 1u,
		Tick = 2u,
		TickParallel = 4u,
		TickParallel2 = 8u
	}

	private static List<ScriptComponentBehavior> _prefabScriptComponents;

	private static List<ScriptComponentBehavior> _undoStackScriptComponents;

	private WeakNativeObjectReference _gameEntity;

	private WeakNativeObjectReference _scriptComponent;

	private TickRequirement _lastTickRequirement;

	private static readonly Dictionary<string, string[]> CachedFields;

	private WeakNativeObjectReference _scene;

	public GameEntity GameEntity
	{
		get
		{
			return _gameEntity?.GetNativeObject() as GameEntity;
		}
		private set
		{
			_gameEntity = new WeakNativeObjectReference(value);
		}
	}

	public ManagedScriptComponent ScriptComponent
	{
		get
		{
			return _scriptComponent?.GetNativeObject() as ManagedScriptComponent;
		}
		private set
		{
			_scriptComponent = new WeakNativeObjectReference(value);
		}
	}

	protected ManagedScriptHolder ManagedScriptHolder { get; private set; }

	public Scene Scene
	{
		get
		{
			return _scene?.GetNativeObject() as Scene;
		}
		private set
		{
			_scene = new WeakNativeObjectReference(value);
		}
	}

	protected void InvalidateWeakPointersIfValid()
	{
		_gameEntity.ManualInvalidate();
		_scriptComponent.ManualInvalidate();
	}

	static ScriptComponentBehavior()
	{
		_prefabScriptComponents = new List<ScriptComponentBehavior>();
		_undoStackScriptComponents = new List<ScriptComponentBehavior>();
		if (CachedFields == null)
		{
			CachedFields = new Dictionary<string, string[]>();
			CacheEditableFieldsForAllScriptComponents();
		}
	}

	internal void Construct(GameEntity myEntity, ManagedScriptComponent scriptComponent)
	{
		GameEntity = myEntity;
		Scene = myEntity.Scene;
		ScriptComponent = scriptComponent;
	}

	internal void SetOwnerManagedScriptHolder(ManagedScriptHolder managedScriptHolder)
	{
		ManagedScriptHolder = managedScriptHolder;
	}

	private void SetScriptComponentToTickAux(TickRequirement value)
	{
		if (_lastTickRequirement == value)
		{
			return;
		}
		if (value.HasAnyFlag(TickRequirement.Tick) != _lastTickRequirement.HasAnyFlag(TickRequirement.Tick))
		{
			if (_lastTickRequirement.HasAnyFlag(TickRequirement.Tick))
			{
				ManagedScriptHolder.RemoveScriptComponentFromTickList(this);
			}
			else
			{
				ManagedScriptHolder.AddScriptComponentToTickList(this);
			}
		}
		if (value.HasAnyFlag(TickRequirement.TickOccasionally) != _lastTickRequirement.HasAnyFlag(TickRequirement.TickOccasionally))
		{
			if (_lastTickRequirement.HasAnyFlag(TickRequirement.TickOccasionally))
			{
				ManagedScriptHolder.RemoveScriptComponentFromTickOccasionallyList(this);
			}
			else
			{
				ManagedScriptHolder.AddScriptComponentToTickOccasionallyList(this);
			}
		}
		if (value.HasAnyFlag(TickRequirement.TickParallel) != _lastTickRequirement.HasAnyFlag(TickRequirement.TickParallel))
		{
			if (_lastTickRequirement.HasAnyFlag(TickRequirement.TickParallel))
			{
				ManagedScriptHolder.RemoveScriptComponentFromParallelTickList(this);
			}
			else
			{
				ManagedScriptHolder.AddScriptComponentToParallelTickList(this);
			}
		}
		if (value.HasAnyFlag(TickRequirement.TickParallel2) != _lastTickRequirement.HasAnyFlag(TickRequirement.TickParallel2))
		{
			if (_lastTickRequirement.HasAnyFlag(TickRequirement.TickParallel2))
			{
				ManagedScriptHolder.RemoveScriptComponentFromParallelTick2List(this);
			}
			else
			{
				ManagedScriptHolder.AddScriptComponentToParallelTick2List(this);
			}
		}
		_lastTickRequirement = value;
	}

	public void SetScriptComponentToTick(TickRequirement value)
	{
		SetScriptComponentToTickAux(value);
	}

	public void SetScriptComponentToTickMT(TickRequirement value)
	{
		lock (ManagedScriptHolder.AddRemoveLockObject)
		{
			SetScriptComponentToTickAux(value);
		}
	}

	[EngineCallback]
	internal void AddScriptComponentToTick()
	{
		lock (_prefabScriptComponents)
		{
			if (!_prefabScriptComponents.Contains(this))
			{
				_prefabScriptComponents.Add(this);
			}
		}
	}

	[EngineCallback]
	internal void RegisterAsPrefabScriptComponent()
	{
		lock (_prefabScriptComponents)
		{
			if (!_prefabScriptComponents.Contains(this))
			{
				_prefabScriptComponents.Add(this);
			}
		}
	}

	[EngineCallback]
	internal void DeregisterAsPrefabScriptComponent()
	{
		lock (_prefabScriptComponents)
		{
			_prefabScriptComponents.Remove(this);
		}
	}

	[EngineCallback]
	internal void RegisterAsUndoStackScriptComponent()
	{
		if (!_undoStackScriptComponents.Contains(this))
		{
			_undoStackScriptComponents.Add(this);
		}
	}

	[EngineCallback]
	internal void DeregisterAsUndoStackScriptComponent()
	{
		if (_undoStackScriptComponents.Contains(this))
		{
			_undoStackScriptComponents.Remove(this);
		}
	}

	[EngineCallback]
	protected internal virtual void SetScene(Scene scene)
	{
		Scene = scene;
	}

	[EngineCallback]
	protected internal virtual void OnInit()
	{
	}

	[EngineCallback]
	protected internal virtual void HandleOnRemoved(int removeReason)
	{
		OnRemoved(removeReason);
		_scene = null;
		_gameEntity = null;
	}

	protected virtual void OnRemoved(int removeReason)
	{
	}

	public virtual TickRequirement GetTickRequirement()
	{
		return TickRequirement.None;
	}

	protected internal virtual void OnTick(float dt)
	{
		Debug.FailedAssert("This base function should never be called.", "C:\\Develop\\MB3\\Source\\Engine\\TaleWorlds.Engine\\ScriptComponentBehavior.cs", "OnTick", 256);
	}

	protected internal virtual void OnTickParallel(float dt)
	{
		Debug.FailedAssert("This base function should never be called.", "C:\\Develop\\MB3\\Source\\Engine\\TaleWorlds.Engine\\ScriptComponentBehavior.cs", "OnTickParallel", 262);
	}

	protected internal virtual void OnTickParallel2(float dt)
	{
	}

	protected internal virtual void OnTickOccasionally(float currentFrameDeltaTime)
	{
		Debug.FailedAssert("This base function should never be called.", "C:\\Develop\\MB3\\Source\\Engine\\TaleWorlds.Engine\\ScriptComponentBehavior.cs", "OnTickOccasionally", 274);
	}

	[EngineCallback]
	protected internal virtual void OnPreInit()
	{
	}

	[EngineCallback]
	protected internal virtual void OnEditorInit()
	{
	}

	[EngineCallback]
	protected internal virtual void OnEditorTick(float dt)
	{
	}

	[EngineCallback]
	protected internal virtual void OnEditorValidate()
	{
	}

	[EngineCallback]
	protected internal virtual bool IsOnlyVisual()
	{
		return false;
	}

	[EngineCallback]
	protected internal virtual bool MovesEntity()
	{
		return true;
	}

	[EngineCallback]
	protected internal virtual bool DisablesOroCreation()
	{
		return true;
	}

	[EngineCallback]
	protected internal virtual void OnEditorVariableChanged(string variableName)
	{
	}

	[EngineCallback]
	protected internal virtual void OnSceneSave(string saveFolder)
	{
	}

	[EngineCallback]
	protected internal virtual bool OnCheckForProblems()
	{
		return false;
	}

	[EngineCallback]
	protected internal virtual void OnPhysicsCollision(ref PhysicsContact contact)
	{
	}

	[EngineCallback]
	protected internal virtual void OnEditModeVisibilityChanged(bool currentVisibility)
	{
	}

	private static void CacheEditableFieldsForAllScriptComponents()
	{
		foreach (KeyValuePair<string, Type> moduleType in Managed.ModuleTypes)
		{
			string key = moduleType.Key;
			CachedFields.Add(key, CollectEditableFields(key));
		}
	}

	private static string[] CollectEditableFields(string className)
	{
		List<string> list = new List<string>();
		if (Managed.ModuleTypes.TryGetValue(className, out var value))
		{
			FieldInfo[] fields = value.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo fieldInfo = fields[i];
				object[] customAttributesSafe = fieldInfo.GetCustomAttributesSafe(typeof(EditableScriptComponentVariable), inherit: true);
				bool flag = false;
				if (customAttributesSafe.Length != 0)
				{
					flag = ((EditableScriptComponentVariable)customAttributesSafe[0]).Visible;
				}
				else if (!fieldInfo.IsPrivate && !fieldInfo.IsFamily)
				{
					flag = true;
				}
				if (flag)
				{
					list.Add(fields[i].Name);
				}
			}
		}
		return list.ToArray();
	}

	[EngineCallback]
	internal static string[] GetEditableFields(string className)
	{
		CachedFields.TryGetValue(className, out var value);
		return value;
	}
}
