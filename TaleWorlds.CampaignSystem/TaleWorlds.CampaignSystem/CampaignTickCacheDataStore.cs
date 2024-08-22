using System;
using System.Collections.Generic;
using System.Threading;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem;

public class CampaignTickCacheDataStore
{
	private struct PartyTickCachePerParty
	{
		internal MobileParty MobileParty;

		internal MobileParty.CachedPartyVariables LocalVariables;
	}

	private class MobilePartyComparer : IComparer<MobileParty>
	{
		public int Compare(MobileParty x, MobileParty y)
		{
			return x.Id.InternalValue.CompareTo(y.Id.InternalValue);
		}
	}

	private PartyTickCachePerParty[] _cacheData;

	private MobileParty[] _gridChangeMobilePartyList;

	private MobileParty[] _exitingSettlementMobilePartyList;

	private int[] _movingPartyIndices;

	private int _currentFrameMovingPartyCount;

	private int[] _stationaryPartyIndices;

	private int _currentFrameStationaryPartyCount;

	private int[] _movingArmyLeaderPartyIndices;

	private int _currentFrameMovingArmyLeaderCount;

	private int _currentTotalMobilePartyCapacity;

	private int _gridChangeCount;

	private int _exitingSettlementCount;

	private float _currentDt;

	private float _currentRealDt;

	private readonly TWParallel.ParallelForAuxPredicate _parallelInitializeCachedPartyVariablesPredicate;

	private readonly TWParallel.ParallelForAuxPredicate _parallelCacheTargetPartyVariablesAtFrameStartPredicate;

	private readonly TWParallel.ParallelForAuxPredicate _parallelArrangePartyIndicesPredicate;

	private readonly TWParallel.ParallelForAuxPredicate _parallelTickArmiesPredicate;

	private readonly TWParallel.ParallelForAuxPredicate _parallelTickMovingPartiesPredicate;

	private readonly TWParallel.ParallelForAuxPredicate _parallelTickStationaryPartiesPredicate;

	private readonly TWParallel.ParallelForAuxPredicate _parallelCheckExitingSettlementsPredicate;

	private readonly MobilePartyComparer _mobilePartyComparer;

	internal CampaignTickCacheDataStore()
	{
		_mobilePartyComparer = new MobilePartyComparer();
		_parallelInitializeCachedPartyVariablesPredicate = ParallelInitializeCachedPartyVariables;
		_parallelCacheTargetPartyVariablesAtFrameStartPredicate = ParallelCacheTargetPartyVariablesAtFrameStart;
		_parallelArrangePartyIndicesPredicate = ParallelArrangePartyIndices;
		_parallelTickArmiesPredicate = ParallelTickArmies;
		_parallelTickMovingPartiesPredicate = ParallelTickMovingParties;
		_parallelTickStationaryPartiesPredicate = ParallelTickStationaryParties;
		_parallelCheckExitingSettlementsPredicate = ParallelCheckExitingSettlements;
	}

	internal void ValidateMobilePartyTickDataCache(int currentTotalMobilePartyCount)
	{
		if (_currentTotalMobilePartyCapacity <= currentTotalMobilePartyCount)
		{
			InitializeCacheArrays();
		}
		_currentFrameMovingPartyCount = -1;
		_currentFrameStationaryPartyCount = -1;
		_currentFrameMovingArmyLeaderCount = -1;
		_gridChangeCount = -1;
		_exitingSettlementCount = -1;
	}

	private void InitializeCacheArrays()
	{
		int num = (int)((float)_currentTotalMobilePartyCapacity * 2f);
		_cacheData = new PartyTickCachePerParty[num];
		_gridChangeMobilePartyList = new MobileParty[num];
		_exitingSettlementMobilePartyList = new MobileParty[num];
		_currentTotalMobilePartyCapacity = num;
		_movingPartyIndices = new int[num];
		_stationaryPartyIndices = new int[num];
		_movingArmyLeaderPartyIndices = new int[num];
	}

	internal void InitializeDataCache()
	{
		_currentFrameMovingArmyLeaderCount = Campaign.Current.MobileParties.Count;
		_currentTotalMobilePartyCapacity = Campaign.Current.MobileParties.Count;
		_currentFrameStationaryPartyCount = Campaign.Current.MobileParties.Count;
		InitializeCacheArrays();
	}

	private void ParallelCheckExitingSettlements(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			Campaign.Current.MobileParties[i].CheckExitingSettlementParallel(ref _exitingSettlementCount, ref _exitingSettlementMobilePartyList);
		}
	}

	private void ParallelInitializeCachedPartyVariables(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			MobileParty mobileParty = Campaign.Current.MobileParties[i];
			_cacheData[i].MobileParty = mobileParty;
			mobileParty.InitializeCachedPartyVariables(ref _cacheData[i].LocalVariables);
		}
	}

	private void ParallelCacheTargetPartyVariablesAtFrameStart(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			_cacheData[i].MobileParty.Ai.CacheTargetPartyVariablesAtFrameStart(ref _cacheData[i].LocalVariables);
		}
	}

	private void ParallelArrangePartyIndices(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			MobileParty.CachedPartyVariables localVariables = _cacheData[i].LocalVariables;
			if (localVariables.IsMoving)
			{
				if (localVariables.IsArmyLeader)
				{
					int num = Interlocked.Increment(ref _currentFrameMovingArmyLeaderCount);
					_movingArmyLeaderPartyIndices[num] = i;
				}
				else
				{
					int num2 = Interlocked.Increment(ref _currentFrameMovingPartyCount);
					_movingPartyIndices[num2] = i;
				}
			}
			else
			{
				int num3 = Interlocked.Increment(ref _currentFrameStationaryPartyCount);
				_stationaryPartyIndices[num3] = i;
			}
		}
	}

	private void ParallelTickArmies(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			int num = _movingArmyLeaderPartyIndices[i];
			PartyTickCachePerParty partyTickCachePerParty = _cacheData[num];
			MobileParty mobileParty = partyTickCachePerParty.MobileParty;
			MobileParty.CachedPartyVariables variables = partyTickCachePerParty.LocalVariables;
			mobileParty.TickForMovingArmyLeader(ref variables, _currentDt, _currentRealDt);
			mobileParty.TickForMobileParty2(ref variables, _currentRealDt, ref _gridChangeCount, ref _gridChangeMobilePartyList);
			mobileParty.ValidateSpeed();
		}
	}

	private void ParallelTickMovingParties(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			int num = _movingPartyIndices[i];
			PartyTickCachePerParty partyTickCachePerParty = _cacheData[num];
			MobileParty mobileParty = partyTickCachePerParty.MobileParty;
			MobileParty.CachedPartyVariables variables = partyTickCachePerParty.LocalVariables;
			mobileParty.TickForMovingMobileParty(ref variables, _currentDt, _currentRealDt);
			mobileParty.TickForMobileParty2(ref variables, _currentRealDt, ref _gridChangeCount, ref _gridChangeMobilePartyList);
		}
	}

	private void ParallelTickStationaryParties(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			int num = _stationaryPartyIndices[i];
			PartyTickCachePerParty partyTickCachePerParty = _cacheData[num];
			MobileParty mobileParty = partyTickCachePerParty.MobileParty;
			MobileParty.CachedPartyVariables variables = partyTickCachePerParty.LocalVariables;
			mobileParty.TickForStationaryMobileParty(ref variables, _currentDt, _currentRealDt);
			mobileParty.TickForMobileParty2(ref variables, _currentRealDt, ref _gridChangeCount, ref _gridChangeMobilePartyList);
		}
	}

	internal void Tick()
	{
		TWParallel.For(0, Campaign.Current.MobileParties.Count, _parallelCheckExitingSettlementsPredicate);
		Array.Sort(_exitingSettlementMobilePartyList, 0, _exitingSettlementCount + 1, _mobilePartyComparer);
		for (int i = 0; i < _exitingSettlementCount + 1; i++)
		{
			LeaveSettlementAction.ApplyForParty(_exitingSettlementMobilePartyList[i]);
		}
	}

	internal void RealTick(float dt, float realDt)
	{
		_currentDt = dt;
		_currentRealDt = realDt;
		ValidateMobilePartyTickDataCache(Campaign.Current.MobileParties.Count);
		int count = Campaign.Current.MobileParties.Count;
		TWParallel.For(0, count, _parallelInitializeCachedPartyVariablesPredicate);
		TWParallel.For(0, count, _parallelCacheTargetPartyVariablesAtFrameStartPredicate);
		TWParallel.For(0, count, _parallelArrangePartyIndicesPredicate);
		TWParallel.For(0, _currentFrameMovingArmyLeaderCount + 1, _parallelTickArmiesPredicate);
		TWParallel.For(0, _currentFrameMovingPartyCount + 1, _parallelTickMovingPartiesPredicate);
		TWParallel.For(0, _currentFrameStationaryPartyCount + 1, _parallelTickStationaryPartiesPredicate);
		UpdateVisibilitiesAroundMainParty();
		Array.Sort(_gridChangeMobilePartyList, 0, _gridChangeCount + 1, _mobilePartyComparer);
		Campaign current = Campaign.Current;
		for (int i = 0; i < _gridChangeCount + 1; i++)
		{
			current.MobilePartyLocator.UpdateLocator(_gridChangeMobilePartyList[i]);
		}
	}

	private void UpdateVisibilitiesAroundMainParty()
	{
		if (!MobileParty.MainParty.CurrentNavigationFace.IsValid() || Campaign.Current.GetSimplifiedTimeControlMode() == CampaignTimeControlMode.Stop)
		{
			return;
		}
		float seeingRange = MobileParty.MainParty.SeeingRange;
		LocatableSearchData<MobileParty> data = MobileParty.StartFindingLocatablesAroundPosition(MobileParty.MainParty.Position2D, seeingRange + 25f);
		for (MobileParty mobileParty = MobileParty.FindNextLocatable(ref data); mobileParty != null; mobileParty = MobileParty.FindNextLocatable(ref data))
		{
			if (!mobileParty.IsMilitia && !mobileParty.IsGarrison)
			{
				mobileParty.Party.UpdateVisibilityAndInspected(seeingRange);
			}
		}
		LocatableSearchData<Settlement> data2 = Settlement.StartFindingLocatablesAroundPosition(MobileParty.MainParty.Position2D, seeingRange + 25f);
		for (Settlement settlement = Settlement.FindNextLocatable(ref data2); settlement != null; settlement = Settlement.FindNextLocatable(ref data2))
		{
			settlement.Party.UpdateVisibilityAndInspected(seeingRange);
		}
	}
}
