using System;
using System.Collections.Generic;
using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

public sealed class ManagedScriptHolder : DotNetObject
{
	public static object AddRemoveLockObject = new object();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToTick = new List<ScriptComponentBehavior>(512);

	private readonly List<ScriptComponentBehavior> _scriptComponentsToParallelTick = new List<ScriptComponentBehavior>(64);

	private readonly List<ScriptComponentBehavior> _scriptComponentsToParallelTick2 = new List<ScriptComponentBehavior>(512);

	private readonly List<ScriptComponentBehavior> _scriptComponentsToTickOccasionally = new List<ScriptComponentBehavior>(512);

	private readonly List<ScriptComponentBehavior> _scriptComponentsToTickForEditor = new List<ScriptComponentBehavior>(512);

	private int _nextIndexToTickOccasionally;

	private readonly List<ScriptComponentBehavior> _scriptComponentsToAddToTick = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToRemoveFromTick = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToAddToParallelTick = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToRemoveFromParallelTick = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToAddToParallelTick2 = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToRemoveFromParallelTick2 = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToAddToTickOccasionally = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToRemoveFromTickOccasionally = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToAddToTickForEditor = new List<ScriptComponentBehavior>();

	private readonly List<ScriptComponentBehavior> _scriptComponentsToRemoveFromTickForEditor = new List<ScriptComponentBehavior>();

	private readonly TWParallel.ParallelForWithDtAuxPredicate TickComponentsParallelAuxMTPredicate;

	private readonly TWParallel.ParallelForWithDtAuxPredicate TickComponentsParallel2AuxMTPredicate;

	private readonly TWParallel.ParallelForWithDtAuxPredicate TickComponentsOccasionallyParallelAuxMTPredicate;

	[EngineCallback]
	internal static ManagedScriptHolder CreateManagedScriptHolder()
	{
		return new ManagedScriptHolder();
	}

	public ManagedScriptHolder()
	{
		TickComponentsParallelAuxMTPredicate = TickComponentsParallelAuxMT;
		TickComponentsParallel2AuxMTPredicate = TickComponentsParallel2AuxMT;
		TickComponentsOccasionallyParallelAuxMTPredicate = TickComponentsOccasionallyParallelAuxMT;
	}

	[EngineCallback]
	public void SetScriptComponentHolder(ScriptComponentBehavior sc)
	{
		sc.SetOwnerManagedScriptHolder(this);
		if (_scriptComponentsToRemoveFromTickForEditor.IndexOf(sc) != -1)
		{
			_scriptComponentsToRemoveFromTickForEditor.Remove(sc);
		}
		else
		{
			_scriptComponentsToAddToTickForEditor.Add(sc);
		}
		sc.SetScriptComponentToTick(sc.GetTickRequirement());
	}

	public void AddScriptComponentToTickOccasionallyList(ScriptComponentBehavior sc)
	{
		int num = _scriptComponentsToRemoveFromTickOccasionally.IndexOf(sc);
		if (num != -1)
		{
			_scriptComponentsToRemoveFromTickOccasionally.RemoveAt(num);
		}
		else
		{
			_scriptComponentsToAddToTickOccasionally.Add(sc);
		}
	}

	public void AddScriptComponentToTickList(ScriptComponentBehavior sc)
	{
		int num = _scriptComponentsToRemoveFromTick.IndexOf(sc);
		if (num != -1)
		{
			_scriptComponentsToRemoveFromTick.RemoveAt(num);
		}
		else
		{
			_scriptComponentsToAddToTick.Add(sc);
		}
	}

	public void AddScriptComponentToParallelTickList(ScriptComponentBehavior sc)
	{
		int num = _scriptComponentsToRemoveFromParallelTick.IndexOf(sc);
		if (num != -1)
		{
			_scriptComponentsToRemoveFromParallelTick.RemoveAt(num);
		}
		else
		{
			_scriptComponentsToAddToParallelTick.Add(sc);
		}
	}

	public void AddScriptComponentToParallelTick2List(ScriptComponentBehavior sc)
	{
		int num = _scriptComponentsToRemoveFromParallelTick2.IndexOf(sc);
		if (num != -1)
		{
			_scriptComponentsToRemoveFromParallelTick2.RemoveAt(num);
		}
		else
		{
			_scriptComponentsToAddToParallelTick2.Add(sc);
		}
	}

	[EngineCallback]
	public void RemoveScriptComponentFromAllTickLists(ScriptComponentBehavior sc)
	{
		lock (AddRemoveLockObject)
		{
			sc.SetScriptComponentToTickMT(ScriptComponentBehavior.TickRequirement.None);
			if (_scriptComponentsToAddToTickForEditor.IndexOf(sc) != -1)
			{
				_scriptComponentsToAddToTickForEditor.Remove(sc);
			}
			else if (_scriptComponentsToRemoveFromTickForEditor.IndexOf(sc) == -1)
			{
				_scriptComponentsToRemoveFromTickForEditor.Add(sc);
			}
		}
	}

	public void RemoveScriptComponentFromTickList(ScriptComponentBehavior sc)
	{
		if (_scriptComponentsToAddToTick.IndexOf(sc) >= 0)
		{
			_scriptComponentsToAddToTick.Remove(sc);
		}
		else if (_scriptComponentsToRemoveFromTick.IndexOf(sc) == -1 && _scriptComponentsToTick.IndexOf(sc) != -1)
		{
			_scriptComponentsToRemoveFromTick.Add(sc);
		}
	}

	public void RemoveScriptComponentFromParallelTickList(ScriptComponentBehavior sc)
	{
		if (_scriptComponentsToAddToParallelTick.IndexOf(sc) >= 0)
		{
			_scriptComponentsToAddToParallelTick.Remove(sc);
		}
		else if (_scriptComponentsToRemoveFromParallelTick.IndexOf(sc) == -1 && _scriptComponentsToParallelTick.IndexOf(sc) != -1)
		{
			_scriptComponentsToRemoveFromParallelTick.Add(sc);
		}
	}

	public void RemoveScriptComponentFromParallelTick2List(ScriptComponentBehavior sc)
	{
		if (_scriptComponentsToAddToParallelTick2.IndexOf(sc) >= 0)
		{
			_scriptComponentsToAddToParallelTick2.Remove(sc);
		}
		else if (_scriptComponentsToRemoveFromParallelTick2.IndexOf(sc) == -1 && _scriptComponentsToParallelTick2.IndexOf(sc) != -1)
		{
			_scriptComponentsToRemoveFromParallelTick2.Add(sc);
		}
	}

	public void RemoveScriptComponentFromTickOccasionallyList(ScriptComponentBehavior sc)
	{
		if (_scriptComponentsToAddToTickOccasionally.IndexOf(sc) >= 0)
		{
			_scriptComponentsToAddToTickOccasionally.Remove(sc);
		}
		else if (_scriptComponentsToRemoveFromTickOccasionally.IndexOf(sc) == -1 && _scriptComponentsToTickOccasionally.IndexOf(sc) != -1)
		{
			_scriptComponentsToRemoveFromTickOccasionally.Add(sc);
		}
	}

	[EngineCallback]
	internal int GetNumberOfScripts()
	{
		return _scriptComponentsToTick.Count;
	}

	private void TickComponentsParallelAuxMT(int startInclusive, int endExclusive, float dt)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			_scriptComponentsToParallelTick[i].OnTickParallel(dt);
		}
	}

	private void TickComponentsParallel2AuxMT(int startInclusive, int endExclusive, float dt)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			_scriptComponentsToParallelTick2[i].OnTickParallel2(dt);
		}
	}

	private void TickComponentsOccasionallyParallelAuxMT(int startInclusive, int endExclusive, float dt)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			_scriptComponentsToTickOccasionally[i].OnTickOccasionally(dt);
		}
	}

	[EngineCallback]
	internal void TickComponents(float dt)
	{
		foreach (ScriptComponentBehavior item in _scriptComponentsToRemoveFromParallelTick)
		{
			_scriptComponentsToParallelTick.Remove(item);
		}
		_scriptComponentsToRemoveFromParallelTick.Clear();
		foreach (ScriptComponentBehavior item2 in _scriptComponentsToAddToParallelTick)
		{
			_scriptComponentsToParallelTick.Add(item2);
		}
		_scriptComponentsToAddToParallelTick.Clear();
		TWParallel.For(0, _scriptComponentsToParallelTick.Count, dt, TickComponentsParallelAuxMTPredicate, 1);
		foreach (ScriptComponentBehavior item3 in _scriptComponentsToRemoveFromParallelTick2)
		{
			_scriptComponentsToParallelTick2.Remove(item3);
		}
		_scriptComponentsToRemoveFromParallelTick2.Clear();
		foreach (ScriptComponentBehavior item4 in _scriptComponentsToAddToParallelTick2)
		{
			_scriptComponentsToParallelTick2.Add(item4);
		}
		_scriptComponentsToAddToParallelTick2.Clear();
		TWParallel.For(0, _scriptComponentsToParallelTick2.Count, dt, TickComponentsParallel2AuxMTPredicate, 8);
		foreach (ScriptComponentBehavior item5 in _scriptComponentsToRemoveFromTick)
		{
			_scriptComponentsToTick.Remove(item5);
		}
		_scriptComponentsToRemoveFromTick.Clear();
		foreach (ScriptComponentBehavior item6 in _scriptComponentsToAddToTick)
		{
			_scriptComponentsToTick.Add(item6);
		}
		_scriptComponentsToAddToTick.Clear();
		foreach (ScriptComponentBehavior item7 in _scriptComponentsToTick)
		{
			item7.OnTick(dt);
		}
		foreach (ScriptComponentBehavior item8 in _scriptComponentsToRemoveFromTickOccasionally)
		{
			_scriptComponentsToTickOccasionally.Remove(item8);
		}
		_nextIndexToTickOccasionally = TaleWorlds.Library.MathF.Max(0, _nextIndexToTickOccasionally - _scriptComponentsToRemoveFromTickOccasionally.Count);
		_scriptComponentsToRemoveFromTickOccasionally.Clear();
		foreach (ScriptComponentBehavior item9 in _scriptComponentsToAddToTickOccasionally)
		{
			_scriptComponentsToTickOccasionally.Add(item9);
		}
		_scriptComponentsToAddToTickOccasionally.Clear();
		int num = _scriptComponentsToTickOccasionally.Count / 10 + 1;
		int num2 = Math.Min(_nextIndexToTickOccasionally + num, _scriptComponentsToTickOccasionally.Count);
		if (_nextIndexToTickOccasionally < num2)
		{
			TWParallel.For(_nextIndexToTickOccasionally, num2, dt, TickComponentsOccasionallyParallelAuxMTPredicate, 8);
			_nextIndexToTickOccasionally = ((num2 < _scriptComponentsToTickOccasionally.Count) ? num2 : 0);
		}
		else
		{
			_nextIndexToTickOccasionally = 0;
		}
	}

	[EngineCallback]
	internal void TickComponentsEditor(float dt)
	{
		for (int i = 0; i < _scriptComponentsToRemoveFromTickForEditor.Count; i++)
		{
			_scriptComponentsToTickForEditor.Remove(_scriptComponentsToRemoveFromTickForEditor[i]);
		}
		_scriptComponentsToRemoveFromTickForEditor.Clear();
		for (int j = 0; j < _scriptComponentsToAddToTickForEditor.Count; j++)
		{
			_scriptComponentsToTickForEditor.Add(_scriptComponentsToAddToTickForEditor[j]);
		}
		_scriptComponentsToAddToTickForEditor.Clear();
		for (int k = 0; k < _scriptComponentsToTickForEditor.Count; k++)
		{
			if (_scriptComponentsToRemoveFromTickForEditor.IndexOf(_scriptComponentsToTickForEditor[k]) == -1)
			{
				_scriptComponentsToTickForEditor[k].OnEditorTick(dt);
			}
		}
	}
}
