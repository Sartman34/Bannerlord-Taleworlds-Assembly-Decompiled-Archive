using System;

namespace TaleWorlds.MountAndBlade;

public class ActionIndexCache : IEquatable<ActionIndexCache>, IEquatable<ActionIndexValueCache>
{
	private const int UnresolvedActionIndex = -2;

	private string _name;

	private int _actionIndex;

	private bool _isValidAction;

	public static ActionIndexCache act_none { get; private set; } = new ActionIndexCache(-1, "act_none");


	public int Index
	{
		get
		{
			if (!_isValidAction)
			{
				return act_none._actionIndex;
			}
			if (_actionIndex == -2)
			{
				ResolveIndex();
			}
			return _actionIndex;
		}
	}

	public string Name
	{
		get
		{
			if (!_isValidAction)
			{
				return act_none._name;
			}
			if (_name == null)
			{
				ResolveName();
			}
			return _name;
		}
	}

	public static ActionIndexCache Create(string actName)
	{
		if (string.IsNullOrWhiteSpace(actName))
		{
			return act_none;
		}
		return new ActionIndexCache(actName);
	}

	private ActionIndexCache(string actName)
	{
		_name = actName;
		_actionIndex = -2;
		_isValidAction = true;
	}

	private ActionIndexCache(int actionIndex, string actName)
	{
		_name = actName;
		_actionIndex = actionIndex;
		_isValidAction = false;
	}

	internal ActionIndexCache(int actionIndex)
	{
		_name = null;
		_actionIndex = actionIndex;
		_isValidAction = actionIndex >= 0;
	}

	private void ResolveIndex()
	{
		_actionIndex = MBAnimation.GetActionCodeWithName(_name);
	}

	private void ResolveName()
	{
		_name = MBAnimation.GetActionNameWithCode(_actionIndex);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is ActionIndexCache)
		{
			return Equals((ActionIndexCache)obj);
		}
		return Equals((ActionIndexValueCache)obj);
	}

	public bool Equals(ActionIndexCache other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return Index == other.Index;
	}

	public bool Equals(ActionIndexValueCache other)
	{
		return Index == other.Index;
	}

	public static bool operator ==(ActionIndexValueCache action0, ActionIndexCache action1)
	{
		return action0.Equals(action1);
	}

	public static bool operator !=(ActionIndexValueCache action0, ActionIndexCache action1)
	{
		return !(action0 == action1);
	}

	public static bool operator ==(ActionIndexCache action0, ActionIndexCache action1)
	{
		if ((object)action0 == null && (object)action1 != null)
		{
			return false;
		}
		if ((object)action1 == null && (object)action0 != null)
		{
			return false;
		}
		if ((object)action0 != action1)
		{
			return action0.Equals(action1);
		}
		return true;
	}

	public static bool operator !=(ActionIndexCache action0, ActionIndexCache action1)
	{
		return !(action0 == action1);
	}

	public override int GetHashCode()
	{
		return Index.GetHashCode();
	}
}
