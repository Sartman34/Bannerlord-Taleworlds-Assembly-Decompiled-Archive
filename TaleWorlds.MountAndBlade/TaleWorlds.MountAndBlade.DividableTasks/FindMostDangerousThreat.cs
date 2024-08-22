using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.DividableTasks;

public class FindMostDangerousThreat : DividableTask
{
	private Agent _targetAgent;

	private FormationSearchThreatTask _formationSearchThreatTask;

	private List<Threat> _threats;

	private RangedSiegeWeapon _weapon;

	private Threat _currentThreat;

	private bool _hasOngoingThreatTask;

	public FindMostDangerousThreat(DividableTask continueToTask = null)
		: base(continueToTask)
	{
		SetTaskFinished();
		_formationSearchThreatTask = new FormationSearchThreatTask();
	}

	protected override bool UpdateExtra()
	{
		bool flag = false;
		if (_hasOngoingThreatTask)
		{
			if (_formationSearchThreatTask.Update())
			{
				_hasOngoingThreatTask = false;
				if (!(flag = _formationSearchThreatTask.GetResult(out _targetAgent)))
				{
					_threats.Remove(_currentThreat);
					_currentThreat = null;
				}
			}
		}
		else
		{
			do
			{
				flag = true;
				int num = -1;
				float num2 = float.MinValue;
				for (int i = 0; i < _threats.Count; i++)
				{
					Threat threat = _threats[i];
					if (threat.ThreatValue > num2)
					{
						num2 = threat.ThreatValue;
						num = i;
					}
				}
				if (num >= 0)
				{
					_currentThreat = _threats[num];
					if (_currentThreat.Formation != null)
					{
						_formationSearchThreatTask.Prepare(_currentThreat.Formation, _weapon);
						_hasOngoingThreatTask = true;
						flag = false;
						break;
					}
					if ((_currentThreat.WeaponEntity == null && _currentThreat.Agent == null) || !_weapon.CanShootAtThreat(_currentThreat))
					{
						_currentThreat = null;
						_threats.RemoveAt(num);
						flag = false;
					}
				}
			}
			while (!flag);
		}
		if (!flag)
		{
			return _threats.Count == 0;
		}
		return true;
	}

	public void Prepare(List<Threat> threats, RangedSiegeWeapon weapon)
	{
		ResetTaskStatus();
		_hasOngoingThreatTask = false;
		_weapon = weapon;
		_threats = threats;
		foreach (Threat threat in _threats)
		{
			threat.ThreatValue *= 0.9f + MBRandom.RandomFloat * 0.2f;
		}
		if (_currentThreat != null)
		{
			_currentThreat = _threats.SingleOrDefault((Threat t) => t.Equals(_currentThreat));
			if (_currentThreat != null)
			{
				_currentThreat.ThreatValue *= 2f;
			}
		}
	}

	public Threat GetResult(out Agent targetAgent)
	{
		targetAgent = _targetAgent;
		return _currentThreat;
	}
}
