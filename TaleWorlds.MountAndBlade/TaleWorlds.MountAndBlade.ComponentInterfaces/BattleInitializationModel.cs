using System.Collections.Generic;
using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.ComponentInterfaces;

public abstract class BattleInitializationModel : GameModel
{
	public const int MinimumTroopCountForPlayerDeployment = 20;

	private bool _cachedCanPlayerSideDeployWithOOB;

	private bool _isCanPlayerSideDeployWithOOBCached;

	private bool _isInitialized;

	public bool BypassPlayerDeployment { get; private set; }

	public abstract List<FormationClass> GetAllAvailableTroopTypes();

	protected abstract bool CanPlayerSideDeployWithOrderOfBattleAux();

	public bool CanPlayerSideDeployWithOrderOfBattle()
	{
		if (!_isCanPlayerSideDeployWithOOBCached)
		{
			_cachedCanPlayerSideDeployWithOOB = !BypassPlayerDeployment && CanPlayerSideDeployWithOrderOfBattleAux();
			_isCanPlayerSideDeployWithOOBCached = true;
		}
		return _cachedCanPlayerSideDeployWithOOB;
	}

	public void InitializeModel()
	{
		_isCanPlayerSideDeployWithOOBCached = false;
		_isInitialized = true;
	}

	public void FinalizeModel()
	{
		_isInitialized = false;
	}

	public void SetBypassPlayerDeployment(bool value)
	{
		BypassPlayerDeployment = value;
	}
}
