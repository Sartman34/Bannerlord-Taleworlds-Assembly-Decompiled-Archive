using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class FormationQuerySystem
{
	public struct FormationIntegrityDataGroup
	{
		public Vec2 AverageVelocityExcludeFarAgents;

		public float DeviationOfPositionsExcludeFarAgents;

		public float AverageMaxUnlimitedSpeedExcludeFarAgents;
	}

	public readonly Formation Formation;

	private readonly QueryData<float> _formationPower;

	private readonly QueryData<float> _formationMeleeFightingPower;

	private readonly QueryData<Vec2> _averagePosition;

	private readonly QueryData<Vec2> _currentVelocity;

	private float _lastAveragePositionCalculateTime;

	private readonly QueryData<Vec2> _estimatedDirection;

	private readonly QueryData<float> _estimatedInterval;

	private readonly QueryData<WorldPosition> _medianPosition;

	private readonly QueryData<Vec2> _averageAllyPosition;

	private readonly QueryData<float> _idealAverageDisplacement;

	private readonly QueryData<FormationIntegrityDataGroup> _formationIntegrityData;

	private readonly QueryData<MBList<Agent>> _localAllyUnits;

	private readonly QueryData<MBList<Agent>> _localEnemyUnits;

	private readonly QueryData<FormationClass> _mainClass;

	private readonly QueryData<float> _infantryUnitRatio;

	private readonly QueryData<float> _hasShieldUnitRatio;

	private readonly QueryData<float> _hasThrowingUnitRatio;

	private readonly QueryData<float> _rangedUnitRatio;

	private readonly QueryData<int> _insideCastleUnitCountIncludingUnpositioned;

	private readonly QueryData<int> _insideCastleUnitCountPositioned;

	private readonly QueryData<float> _cavalryUnitRatio;

	private readonly QueryData<float> _rangedCavalryUnitRatio;

	private readonly QueryData<bool> _isMeleeFormation;

	private readonly QueryData<bool> _isInfantryFormation;

	private readonly QueryData<bool> _hasShield;

	private readonly QueryData<bool> _hasThrowing;

	private readonly QueryData<bool> _isRangedFormation;

	private readonly QueryData<bool> _isCavalryFormation;

	private readonly QueryData<bool> _isRangedCavalryFormation;

	private readonly QueryData<float> _movementSpeedMaximum;

	private readonly QueryData<float> _movementSpeed;

	private readonly QueryData<float> _maximumMissileRange;

	private readonly QueryData<float> _missileRangeAdjusted;

	private readonly QueryData<float> _localInfantryUnitRatio;

	private readonly QueryData<float> _localRangedUnitRatio;

	private readonly QueryData<float> _localCavalryUnitRatio;

	private readonly QueryData<float> _localRangedCavalryUnitRatio;

	private readonly QueryData<float> _localAllyPower;

	private readonly QueryData<float> _localEnemyPower;

	private readonly QueryData<float> _localPowerRatio;

	private readonly QueryData<float> _casualtyRatio;

	private readonly QueryData<bool> _isUnderRangedAttack;

	private readonly QueryData<float> _underRangedAttackRatio;

	private readonly QueryData<float> _makingRangedAttackRatio;

	private readonly QueryData<Formation> _mainFormation;

	private readonly QueryData<float> _mainFormationReliabilityFactor;

	private readonly QueryData<Vec2> _weightedAverageEnemyPosition;

	private readonly QueryData<Agent> _closestEnemyAgent;

	private readonly QueryData<Formation> _closestEnemyFormation;

	private readonly QueryData<Formation> _closestSignificantlyLargeEnemyFormation;

	private readonly QueryData<Formation> _fastestSignificantlyLargeEnemyFormation;

	private readonly QueryData<Vec2> _highGroundCloseToForeseenBattleGround;

	public TeamQuerySystem Team => Formation.Team.QuerySystem;

	public float FormationPower => _formationPower.Value;

	public float FormationMeleeFightingPower => _formationMeleeFightingPower.Value;

	public Vec2 AveragePosition => _averagePosition.Value;

	public Vec2 CurrentVelocity => _currentVelocity.Value;

	public Vec2 EstimatedDirection => _estimatedDirection.Value;

	public float EstimatedInterval => _estimatedInterval.Value;

	public WorldPosition MedianPosition => _medianPosition.Value;

	public Vec2 AverageAllyPosition => _averageAllyPosition.Value;

	public float IdealAverageDisplacement => _idealAverageDisplacement.Value;

	public FormationIntegrityDataGroup FormationIntegrityData => _formationIntegrityData.Value;

	public MBList<Agent> LocalAllyUnits => _localAllyUnits.Value;

	public MBList<Agent> LocalEnemyUnits => _localEnemyUnits.Value;

	public FormationClass MainClass => _mainClass.Value;

	public float InfantryUnitRatio => _infantryUnitRatio.Value;

	public float HasShieldUnitRatio => _hasShieldUnitRatio.Value;

	public float HasThrowingUnitRatio => _hasThrowingUnitRatio.Value;

	public float RangedUnitRatio => _rangedUnitRatio.Value;

	public int InsideCastleUnitCountIncludingUnpositioned => _insideCastleUnitCountIncludingUnpositioned.Value;

	public int InsideCastleUnitCountPositioned => _insideCastleUnitCountPositioned.Value;

	public float CavalryUnitRatio => _cavalryUnitRatio.Value;

	public float GetCavalryUnitRatioWithoutExpiration => _cavalryUnitRatio.GetCachedValue();

	public float RangedCavalryUnitRatio => _rangedCavalryUnitRatio.Value;

	public float GetRangedCavalryUnitRatioWithoutExpiration => _rangedCavalryUnitRatio.GetCachedValue();

	public bool IsMeleeFormation => _isMeleeFormation.Value;

	public bool IsInfantryFormation => _isInfantryFormation.Value;

	public bool HasShield => _hasShield.Value;

	public bool HasThrowing => _hasThrowing.Value;

	public bool IsRangedFormation => _isRangedFormation.Value;

	public bool IsCavalryFormation => _isCavalryFormation.Value;

	public bool IsRangedCavalryFormation => _isRangedCavalryFormation.Value;

	public float MovementSpeedMaximum => _movementSpeedMaximum.Value;

	public float MovementSpeed => _movementSpeed.Value;

	public float MaximumMissileRange => _maximumMissileRange.Value;

	public float MissileRangeAdjusted => _missileRangeAdjusted.Value;

	public float LocalInfantryUnitRatio => _localInfantryUnitRatio.Value;

	public float LocalRangedUnitRatio => _localRangedUnitRatio.Value;

	public float LocalCavalryUnitRatio => _localCavalryUnitRatio.Value;

	public float LocalRangedCavalryUnitRatio => _localRangedCavalryUnitRatio.Value;

	public float LocalAllyPower => _localAllyPower.Value;

	public float LocalEnemyPower => _localEnemyPower.Value;

	public float LocalPowerRatio => _localPowerRatio.Value;

	public float CasualtyRatio => _casualtyRatio.Value;

	public bool IsUnderRangedAttack => _isUnderRangedAttack.Value;

	public float UnderRangedAttackRatio => _underRangedAttackRatio.Value;

	public float MakingRangedAttackRatio => _makingRangedAttackRatio.Value;

	public Formation MainFormation => _mainFormation.Value;

	public float MainFormationReliabilityFactor => _mainFormationReliabilityFactor.Value;

	public Vec2 WeightedAverageEnemyPosition => _weightedAverageEnemyPosition.Value;

	public Agent ClosestEnemyAgent => _closestEnemyAgent.Value;

	public FormationQuerySystem ClosestEnemyFormation
	{
		get
		{
			if (_closestEnemyFormation.Value == null || _closestEnemyFormation.Value.CountOfUnits == 0)
			{
				_closestEnemyFormation.Expire();
			}
			return _closestEnemyFormation.Value?.QuerySystem;
		}
	}

	public FormationQuerySystem ClosestSignificantlyLargeEnemyFormation
	{
		get
		{
			if (_closestSignificantlyLargeEnemyFormation.Value == null || _closestSignificantlyLargeEnemyFormation.Value.CountOfUnits == 0)
			{
				_closestSignificantlyLargeEnemyFormation.Expire();
			}
			return _closestSignificantlyLargeEnemyFormation.Value?.QuerySystem;
		}
	}

	public FormationQuerySystem FastestSignificantlyLargeEnemyFormation
	{
		get
		{
			if (_fastestSignificantlyLargeEnemyFormation.Value == null || _fastestSignificantlyLargeEnemyFormation.Value.CountOfUnits == 0)
			{
				_fastestSignificantlyLargeEnemyFormation.Expire();
			}
			return _fastestSignificantlyLargeEnemyFormation.Value?.QuerySystem;
		}
	}

	public Vec2 HighGroundCloseToForeseenBattleGround => _highGroundCloseToForeseenBattleGround.Value;

	public FormationQuerySystem(Formation formation)
	{
		FormationQuerySystem formationQuerySystem = this;
		Formation = formation;
		Mission mission = Mission.Current;
		_formationPower = new QueryData<float>(formation.GetFormationPower, 2.5f);
		_formationMeleeFightingPower = new QueryData<float>(formation.GetFormationMeleeFightingPower, 2.5f);
		_averagePosition = new QueryData<Vec2>(delegate
		{
			Vec2 vec5 = ((formation.CountOfUnitsWithoutDetachedOnes > 1) ? formation.GetAveragePositionOfUnits(excludeDetachedUnits: true, excludePlayer: true) : ((formation.CountOfUnitsWithoutDetachedOnes > 0) ? formation.GetAveragePositionOfUnits(excludeDetachedUnits: true, excludePlayer: false) : formation.OrderPosition));
			float currentTime3 = Mission.Current.CurrentTime;
			float num35 = currentTime3 - formationQuerySystem._lastAveragePositionCalculateTime;
			if (num35 > 0f)
			{
				formationQuerySystem._currentVelocity.SetValue((vec5 - formationQuerySystem._averagePosition.GetCachedValue()) * (1f / num35), currentTime3);
			}
			formationQuerySystem._lastAveragePositionCalculateTime = currentTime3;
			return vec5;
		}, 0.05f);
		_currentVelocity = new QueryData<Vec2>(delegate
		{
			formationQuerySystem._averagePosition.Evaluate(Mission.Current.CurrentTime);
			return formationQuerySystem._currentVelocity.GetCachedValue();
		}, 1f);
		_estimatedDirection = new QueryData<Vec2>(delegate
		{
			if (formation.CountOfUnitsWithoutDetachedOnes > 0)
			{
				Vec2 averagePositionOfUnits = formation.GetAveragePositionOfUnits(excludeDetachedUnits: true, excludePlayer: true);
				float num27 = 0f;
				float num28 = 0f;
				Vec2 orderLocalAveragePosition = formation.OrderLocalAveragePosition;
				int num29 = 0;
				foreach (Agent unitsWithoutLooseDetachedOne in formation.UnitsWithoutLooseDetachedOnes)
				{
					Vec2? localPositionOfUnitOrDefault2 = formation.Arrangement.GetLocalPositionOfUnitOrDefault(unitsWithoutLooseDetachedOne);
					if (localPositionOfUnitOrDefault2.HasValue)
					{
						Vec2 value = localPositionOfUnitOrDefault2.Value;
						Vec2 asVec = unitsWithoutLooseDetachedOne.Position.AsVec2;
						num27 += (value.x - orderLocalAveragePosition.x) * (asVec.x - averagePositionOfUnits.x) + (value.y - orderLocalAveragePosition.y) * (asVec.y - averagePositionOfUnits.y);
						num28 += (value.x - orderLocalAveragePosition.x) * (asVec.y - averagePositionOfUnits.y) - (value.y - orderLocalAveragePosition.y) * (asVec.x - averagePositionOfUnits.x);
						num29++;
					}
				}
				if (num29 > 0)
				{
					float num30 = 1f / (float)num29;
					num27 *= num30;
					num28 *= num30;
					float num31 = TaleWorlds.Library.MathF.Sqrt(num27 * num27 + num28 * num28);
					if (num31 > 0f)
					{
						float num32 = TaleWorlds.Library.MathF.Acos(MBMath.ClampFloat(num27 / num31, -1f, 1f));
						Vec2 result5 = Vec2.FromRotation(num32);
						Vec2 result6 = Vec2.FromRotation(0f - num32);
						float num33 = 0f;
						float num34 = 0f;
						foreach (Agent unitsWithoutLooseDetachedOne2 in formation.UnitsWithoutLooseDetachedOnes)
						{
							Vec2? localPositionOfUnitOrDefault3 = formation.Arrangement.GetLocalPositionOfUnitOrDefault(unitsWithoutLooseDetachedOne2);
							if (localPositionOfUnitOrDefault3.HasValue)
							{
								Vec2 vec3 = result5.TransformToParentUnitF(localPositionOfUnitOrDefault3.Value - orderLocalAveragePosition);
								Vec2 vec4 = result6.TransformToParentUnitF(localPositionOfUnitOrDefault3.Value - orderLocalAveragePosition);
								Vec2 asVec2 = unitsWithoutLooseDetachedOne2.Position.AsVec2;
								num33 += (vec3 - asVec2 + averagePositionOfUnits).LengthSquared;
								num34 += (vec4 - asVec2 + averagePositionOfUnits).LengthSquared;
							}
						}
						if (!(num33 < num34))
						{
							return result6;
						}
						return result5;
					}
				}
			}
			return new Vec2(0f, 1f);
		}, 0.2f);
		_estimatedInterval = new QueryData<float>(delegate
		{
			if (formation.CountOfUnitsWithoutDetachedOnes > 0)
			{
				Vec2 estimatedDirection = formation.QuerySystem.EstimatedDirection;
				Vec2 currentPosition = formation.CurrentPosition;
				float num23 = 0f;
				float num24 = 0f;
				foreach (Agent unitsWithoutLooseDetachedOne3 in formation.UnitsWithoutLooseDetachedOnes)
				{
					Vec2? localPositionOfUnitOrDefault = formation.Arrangement.GetLocalPositionOfUnitOrDefault(unitsWithoutLooseDetachedOne3);
					if (localPositionOfUnitOrDefault.HasValue)
					{
						Vec2 vec2 = estimatedDirection.TransformToLocalUnitF(unitsWithoutLooseDetachedOne3.Position.AsVec2 - currentPosition);
						Vec2 va = localPositionOfUnitOrDefault.Value - vec2;
						Vec2 vb = formation.Arrangement.GetLocalPositionOfUnitOrDefaultWithAdjustment(unitsWithoutLooseDetachedOne3, 1f).Value - localPositionOfUnitOrDefault.Value;
						if (vb.IsNonZero())
						{
							float num25 = vb.Normalize();
							float num26 = Vec2.DotProduct(va, vb);
							num23 += num26 * num25;
							num24 += num25 * num25;
						}
					}
				}
				if (num24 != 0f)
				{
					return Math.Max(0f, (0f - num23) / num24 + formation.Interval);
				}
			}
			return formation.Interval;
		}, 0.2f);
		_medianPosition = new QueryData<WorldPosition>(delegate
		{
			if (formation.CountOfUnitsWithoutDetachedOnes != 0)
			{
				if (formation.CountOfUnitsWithoutDetachedOnes != 1)
				{
					return formation.GetMedianAgent(excludeDetachedUnits: true, excludePlayer: true, formationQuerySystem.AveragePosition).GetWorldPosition();
				}
				return formation.GetMedianAgent(excludeDetachedUnits: true, excludePlayer: false, formationQuerySystem.AveragePosition).GetWorldPosition();
			}
			return (formation.CountOfUnits != 0) ? ((formation.CountOfUnits != 1) ? formation.GetMedianAgent(excludeDetachedUnits: false, excludePlayer: true, formationQuerySystem.AveragePosition).GetWorldPosition() : formation.GetFirstUnit().GetWorldPosition()) : formation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None);
		}, 0.05f);
		_averageAllyPosition = new QueryData<Vec2>(delegate
		{
			int num22 = 0;
			Vec2 zero2 = Vec2.Zero;
			foreach (Team team in mission.Teams)
			{
				if (team.IsFriendOf(formation.Team))
				{
					foreach (Formation item in formationQuerySystem.Formation.Team.FormationsIncludingSpecialAndEmpty)
					{
						if (item.CountOfUnits > 0 && item != formation)
						{
							num22 += item.CountOfUnits;
							zero2 += item.GetAveragePositionOfUnits(excludeDetachedUnits: false, excludePlayer: false) * item.CountOfUnits;
						}
					}
				}
			}
			return (num22 > 0) ? (zero2 * (1f / (float)num22)) : formationQuerySystem.AveragePosition;
		}, 5f);
		_idealAverageDisplacement = new QueryData<float>(() => TaleWorlds.Library.MathF.Sqrt(formation.Width * formation.Width * 0.5f * 0.5f + formation.Depth * formation.Depth * 0.5f * 0.5f) / 2f, 5f);
		_formationIntegrityData = new QueryData<FormationIntegrityDataGroup>(delegate
		{
			FormationIntegrityDataGroup result4 = default(FormationIntegrityDataGroup);
			if (formation.CountOfUnitsWithoutDetachedOnes > 0)
			{
				float num17 = 0f;
				MBReadOnlyList<IFormationUnit> allUnits = formation.Arrangement.GetAllUnits();
				int num18 = 0;
				float distanceBetweenAgentsAdjustment = formation.QuerySystem.EstimatedInterval - formation.Interval;
				foreach (Agent item2 in allUnits)
				{
					Vec2? localPositionOfUnitOrDefaultWithAdjustment = formation.Arrangement.GetLocalPositionOfUnitOrDefaultWithAdjustment(item2, distanceBetweenAgentsAdjustment);
					if (localPositionOfUnitOrDefaultWithAdjustment.HasValue)
					{
						Vec2 vec = formation.QuerySystem.EstimatedDirection.TransformToParentUnitF(localPositionOfUnitOrDefaultWithAdjustment.Value) + formation.CurrentPosition;
						num18++;
						num17 += (vec - item2.Position.AsVec2).LengthSquared;
					}
				}
				if (num18 > 0)
				{
					float num19 = num17 / (float)num18 * 4f;
					float num20 = 0f;
					Vec2 zero = Vec2.Zero;
					float num21 = 0f;
					num18 = 0;
					foreach (Agent item3 in allUnits)
					{
						Vec2? localPositionOfUnitOrDefaultWithAdjustment2 = formation.Arrangement.GetLocalPositionOfUnitOrDefaultWithAdjustment(item3, distanceBetweenAgentsAdjustment);
						if (localPositionOfUnitOrDefaultWithAdjustment2.HasValue)
						{
							float lengthSquared = (formation.QuerySystem.EstimatedDirection.TransformToParentUnitF(localPositionOfUnitOrDefaultWithAdjustment2.Value) + formation.CurrentPosition - item3.Position.AsVec2).LengthSquared;
							if (lengthSquared < num19)
							{
								num20 += lengthSquared;
								zero += item3.AverageVelocity.AsVec2;
								num21 += item3.MaximumForwardUnlimitedSpeed;
								num18++;
							}
						}
					}
					if (num18 > 0)
					{
						zero *= 1f / (float)num18;
						num20 /= (float)num18;
						num21 /= (float)num18;
						result4.AverageVelocityExcludeFarAgents = zero;
						result4.DeviationOfPositionsExcludeFarAgents = TaleWorlds.Library.MathF.Sqrt(num20);
						result4.AverageMaxUnlimitedSpeedExcludeFarAgents = num21;
						return result4;
					}
				}
			}
			result4.AverageVelocityExcludeFarAgents = Vec2.Zero;
			result4.DeviationOfPositionsExcludeFarAgents = 0f;
			result4.AverageMaxUnlimitedSpeedExcludeFarAgents = 0f;
			return result4;
		}, 1f);
		_localAllyUnits = new QueryData<MBList<Agent>>(() => mission.GetNearbyAllyAgents(formationQuerySystem.AveragePosition, 30f, formation.Team, formationQuerySystem._localAllyUnits.GetCachedValue()), 5f, new MBList<Agent>());
		_localEnemyUnits = new QueryData<MBList<Agent>>(() => mission.GetNearbyEnemyAgents(formationQuerySystem.AveragePosition, 30f, formation.Team, formationQuerySystem._localEnemyUnits.GetCachedValue()), 5f, new MBList<Agent>());
		_infantryUnitRatio = new QueryData<float>(() => (formation.CountOfUnits > 0) ? ((float)formation.GetCountOfUnitsBelongingToPhysicalClass(FormationClass.Infantry, excludeBannerBearers: false) / (float)formation.CountOfUnits) : 0f, 2.5f);
		_hasShieldUnitRatio = new QueryData<float>(() => (formation.CountOfUnits > 0) ? ((float)formation.GetCountOfUnitsWithCondition(QueryLibrary.HasShield) / (float)formation.CountOfUnits) : 0f, 2.5f);
		_hasThrowingUnitRatio = new QueryData<float>(() => (formation.CountOfUnits > 0) ? ((float)formation.GetCountOfUnitsWithCondition(QueryLibrary.HasThrown) / (float)formation.CountOfUnits) : 0f, 2.5f);
		_rangedUnitRatio = new QueryData<float>(() => (formation.CountOfUnits > 0) ? ((float)formation.GetCountOfUnitsBelongingToPhysicalClass(FormationClass.Ranged, excludeBannerBearers: false) / (float)formation.CountOfUnits) : 0f, 2.5f);
		_cavalryUnitRatio = new QueryData<float>(() => (formation.CountOfUnits > 0) ? ((float)formation.GetCountOfUnitsBelongingToPhysicalClass(FormationClass.Cavalry, excludeBannerBearers: false) / (float)formation.CountOfUnits) : 0f, 2.5f);
		_rangedCavalryUnitRatio = new QueryData<float>(() => (formation.CountOfUnits > 0) ? ((float)formation.GetCountOfUnitsBelongingToPhysicalClass(FormationClass.HorseArcher, excludeBannerBearers: false) / (float)formation.CountOfUnits) : 0f, 2.5f);
		_isMeleeFormation = new QueryData<bool>(() => formationQuerySystem.InfantryUnitRatio + formationQuerySystem.CavalryUnitRatio > formationQuerySystem.RangedUnitRatio + formationQuerySystem.RangedCavalryUnitRatio, 5f);
		_isInfantryFormation = new QueryData<bool>(() => formationQuerySystem.InfantryUnitRatio >= formationQuerySystem.RangedUnitRatio && formationQuerySystem.InfantryUnitRatio >= formationQuerySystem.CavalryUnitRatio && formationQuerySystem.InfantryUnitRatio >= formationQuerySystem.RangedCavalryUnitRatio, 5f);
		_hasShield = new QueryData<bool>(() => formationQuerySystem.HasShieldUnitRatio >= 0.4f, 5f);
		_hasThrowing = new QueryData<bool>(() => formationQuerySystem.HasThrowingUnitRatio >= 0.5f, 5f);
		_isRangedFormation = new QueryData<bool>(() => formationQuerySystem.RangedUnitRatio > formationQuerySystem.InfantryUnitRatio && formationQuerySystem.RangedUnitRatio >= formationQuerySystem.CavalryUnitRatio && formationQuerySystem.RangedUnitRatio >= formationQuerySystem.RangedCavalryUnitRatio, 5f);
		_isCavalryFormation = new QueryData<bool>(() => formationQuerySystem.CavalryUnitRatio > formationQuerySystem.InfantryUnitRatio && formationQuerySystem.CavalryUnitRatio > formationQuerySystem.RangedUnitRatio && formationQuerySystem.CavalryUnitRatio >= formationQuerySystem.RangedCavalryUnitRatio, 5f);
		_isRangedCavalryFormation = new QueryData<bool>(() => formationQuerySystem.RangedCavalryUnitRatio > formationQuerySystem.InfantryUnitRatio && formationQuerySystem.RangedCavalryUnitRatio > formationQuerySystem.RangedUnitRatio && formationQuerySystem.RangedCavalryUnitRatio > formationQuerySystem.CavalryUnitRatio, 5f);
		QueryData<float>.SetupSyncGroup(_infantryUnitRatio, _hasShieldUnitRatio, _rangedUnitRatio, _cavalryUnitRatio, _rangedCavalryUnitRatio, _isMeleeFormation, _isInfantryFormation, _hasShield, _isRangedFormation, _isCavalryFormation, _isRangedCavalryFormation);
		_movementSpeedMaximum = new QueryData<float>(formation.GetAverageMaximumMovementSpeedOfUnits, 10f);
		_movementSpeed = new QueryData<float>(formation.GetMovementSpeedOfUnits, 2f);
		_maximumMissileRange = new QueryData<float>(delegate
		{
			if (formation.CountOfUnits == 0)
			{
				return 0f;
			}
			float maximumRange = 0f;
			formation.ApplyActionOnEachUnit(delegate(Agent agent)
			{
				if (agent.MaximumMissileRange > maximumRange)
				{
					maximumRange = agent.MaximumMissileRange;
				}
			});
			return maximumRange;
		}, 10f);
		_missileRangeAdjusted = new QueryData<float>(delegate
		{
			if (formation.CountOfUnits == 0)
			{
				return 0f;
			}
			float sum = 0f;
			formation.ApplyActionOnEachUnit(delegate(Agent agent)
			{
				sum += agent.MissileRangeAdjusted;
			});
			return sum / (float)formation.CountOfUnits;
		}, 10f);
		_localInfantryUnitRatio = new QueryData<float>(() => (formationQuerySystem.LocalAllyUnits.Count != 0) ? (1f * (float)formationQuerySystem.LocalAllyUnits.Count(QueryLibrary.IsInfantry) / (float)formationQuerySystem.LocalAllyUnits.Count) : 0f, 15f);
		_localRangedUnitRatio = new QueryData<float>(() => (formationQuerySystem.LocalAllyUnits.Count != 0) ? (1f * (float)formationQuerySystem.LocalAllyUnits.Count(QueryLibrary.IsRanged) / (float)formationQuerySystem.LocalAllyUnits.Count) : 0f, 15f);
		_localCavalryUnitRatio = new QueryData<float>(() => (formationQuerySystem.LocalAllyUnits.Count != 0) ? (1f * (float)formationQuerySystem.LocalAllyUnits.Count(QueryLibrary.IsCavalry) / (float)formationQuerySystem.LocalAllyUnits.Count) : 0f, 15f);
		_localRangedCavalryUnitRatio = new QueryData<float>(() => (formationQuerySystem.LocalAllyUnits.Count != 0) ? (1f * (float)formationQuerySystem.LocalAllyUnits.Count(QueryLibrary.IsRangedCavalry) / (float)formationQuerySystem.LocalAllyUnits.Count) : 0f, 15f);
		QueryData<float>.SetupSyncGroup(_localInfantryUnitRatio, _localRangedUnitRatio, _localCavalryUnitRatio, _localRangedCavalryUnitRatio);
		_localAllyPower = new QueryData<float>(() => formationQuerySystem.LocalAllyUnits.Sum((Agent lau) => lau.CharacterPowerCached), 5f);
		_localEnemyPower = new QueryData<float>(() => formationQuerySystem.LocalEnemyUnits.Sum((Agent leu) => leu.CharacterPowerCached), 5f);
		_localPowerRatio = new QueryData<float>(() => MBMath.ClampFloat(TaleWorlds.Library.MathF.Sqrt((formationQuerySystem.LocalAllyUnits.Sum((Agent lau) => lau.CharacterPowerCached) + 1f) * 1f / (formationQuerySystem.LocalEnemyUnits.Sum((Agent leu) => leu.CharacterPowerCached) + 1f)), 0.5f, 1.75f), 5f);
		_casualtyRatio = new QueryData<float>(delegate
		{
			if (formation.CountOfUnits == 0)
			{
				return 0f;
			}
			int num16 = mission.GetMissionBehavior<CasualtyHandler>()?.GetCasualtyCountOfFormation(formation) ?? 0;
			return 1f - (float)num16 * 1f / (float)(num16 + formation.CountOfUnits);
		}, 10f);
		_isUnderRangedAttack = new QueryData<bool>(() => formation.GetUnderAttackTypeOfUnits(10f) == Agent.UnderAttackType.UnderRangedAttack, 3f);
		_underRangedAttackRatio = new QueryData<float>(delegate
		{
			float currentTime2 = MBCommon.GetTotalMissionTime();
			int countOfUnitsWithCondition2 = formation.GetCountOfUnitsWithCondition((Agent agent) => currentTime2 - agent.LastRangedHitTime < 10f);
			return (formation.CountOfUnits <= 0) ? 0f : ((float)countOfUnitsWithCondition2 / (float)formation.CountOfUnits);
		}, 3f);
		_makingRangedAttackRatio = new QueryData<float>(delegate
		{
			float currentTime = MBCommon.GetTotalMissionTime();
			int countOfUnitsWithCondition = formation.GetCountOfUnitsWithCondition((Agent agent) => currentTime - agent.LastRangedAttackTime < 10f);
			return (formation.CountOfUnits <= 0) ? 0f : ((float)countOfUnitsWithCondition / (float)formation.CountOfUnits);
		}, 3f);
		_closestEnemyAgent = new QueryData<Agent>(delegate
		{
			float num14 = float.MaxValue;
			Agent result3 = null;
			foreach (Team team2 in mission.Teams)
			{
				if (team2.IsEnemyOf(formation.Team))
				{
					foreach (Agent activeAgent in team2.ActiveAgents)
					{
						float num15 = activeAgent.Position.DistanceSquared(new Vec3(formationQuerySystem.AveragePosition, formationQuerySystem.MedianPosition.GetNavMeshZ()));
						if (num15 < num14)
						{
							num14 = num15;
							result3 = activeAgent;
						}
					}
				}
			}
			return result3;
		}, 1.5f);
		_closestEnemyFormation = new QueryData<Formation>(delegate
		{
			float num12 = float.MaxValue;
			Formation result2 = null;
			foreach (Team team3 in mission.Teams)
			{
				if (team3.IsEnemyOf(formation.Team))
				{
					foreach (Formation item4 in team3.FormationsIncludingSpecialAndEmpty)
					{
						if (item4.CountOfUnits > 0)
						{
							float num13 = item4.QuerySystem.MedianPosition.GetNavMeshVec3().DistanceSquared(new Vec3(formationQuerySystem.AveragePosition, formationQuerySystem.MedianPosition.GetNavMeshZ()));
							if (num13 < num12)
							{
								num12 = num13;
								result2 = item4;
							}
						}
					}
				}
			}
			return result2;
		}, 1.5f);
		_closestSignificantlyLargeEnemyFormation = new QueryData<Formation>(delegate
		{
			float num8 = float.MaxValue;
			Formation formation4 = null;
			float num9 = float.MaxValue;
			Formation formation5 = null;
			foreach (Team team4 in mission.Teams)
			{
				if (team4.IsEnemyOf(formation.Team))
				{
					foreach (Formation item5 in team4.FormationsIncludingSpecialAndEmpty)
					{
						if (item5.CountOfUnits > 0)
						{
							if (item5.QuerySystem.FormationPower / formationQuerySystem.FormationPower > 0.2f || item5.QuerySystem.FormationPower * formationQuerySystem.Team.TeamPower / (item5.Team.QuerySystem.TeamPower * formationQuerySystem.FormationPower) > 0.2f)
							{
								float num10 = item5.QuerySystem.MedianPosition.GetNavMeshVec3().DistanceSquared(new Vec3(formationQuerySystem.AveragePosition, formationQuerySystem.MedianPosition.GetNavMeshZ()));
								if (num10 < num8)
								{
									num8 = num10;
									formation4 = item5;
								}
							}
							else if (formation4 == null)
							{
								float num11 = item5.QuerySystem.MedianPosition.GetNavMeshVec3().DistanceSquared(new Vec3(formationQuerySystem.AveragePosition, formationQuerySystem.MedianPosition.GetNavMeshZ()));
								if (num11 < num9)
								{
									num9 = num11;
									formation5 = item5;
								}
							}
						}
					}
				}
			}
			return (formation4 != null) ? formation4 : formation5;
		}, 1.5f);
		_fastestSignificantlyLargeEnemyFormation = new QueryData<Formation>(delegate
		{
			float num4 = float.MaxValue;
			Formation formation2 = null;
			float num5 = float.MaxValue;
			Formation formation3 = null;
			foreach (Team team5 in mission.Teams)
			{
				if (team5.IsEnemyOf(formation.Team))
				{
					foreach (Formation item6 in team5.FormationsIncludingSpecialAndEmpty)
					{
						if (item6.CountOfUnits > 0)
						{
							if (item6.QuerySystem.FormationPower / formationQuerySystem.FormationPower > 0.2f || item6.QuerySystem.FormationPower * formationQuerySystem.Team.TeamPower / (item6.Team.QuerySystem.TeamPower * formationQuerySystem.FormationPower) > 0.2f)
							{
								float num6 = item6.QuerySystem.MedianPosition.GetNavMeshVec3().DistanceSquared(new Vec3(formationQuerySystem.AveragePosition, formationQuerySystem.MedianPosition.GetNavMeshZ())) / (item6.QuerySystem.MovementSpeed * item6.QuerySystem.MovementSpeed);
								if (num6 < num4)
								{
									num4 = num6;
									formation2 = item6;
								}
							}
							else if (formation2 == null)
							{
								float num7 = item6.QuerySystem.MedianPosition.GetNavMeshVec3().DistanceSquared(new Vec3(formationQuerySystem.AveragePosition, formationQuerySystem.MedianPosition.GetNavMeshZ())) / (item6.QuerySystem.MovementSpeed * item6.QuerySystem.MovementSpeed);
								if (num7 < num5)
								{
									num5 = num7;
									formation3 = item6;
								}
							}
						}
					}
				}
			}
			return (formation2 != null) ? formation2 : formation3;
		}, 1.5f);
		_mainClass = new QueryData<FormationClass>(delegate
		{
			FormationClass result = FormationClass.Infantry;
			float num3 = formationQuerySystem.InfantryUnitRatio;
			if (formationQuerySystem.RangedUnitRatio > num3)
			{
				result = FormationClass.Ranged;
				num3 = formationQuerySystem.RangedUnitRatio;
			}
			if (formationQuerySystem.CavalryUnitRatio > num3)
			{
				result = FormationClass.Cavalry;
				num3 = formationQuerySystem.CavalryUnitRatio;
			}
			if (formationQuerySystem.RangedCavalryUnitRatio > num3)
			{
				result = FormationClass.HorseArcher;
			}
			return result;
		}, 15f);
		_mainFormation = new QueryData<Formation>(() => formation.Team.FormationsIncludingSpecialAndEmpty.FirstOrDefault((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation && f != formation), 15f);
		_mainFormationReliabilityFactor = new QueryData<float>(delegate
		{
			if (formationQuerySystem.MainFormation == null)
			{
				return 0f;
			}
			float num = ((formationQuerySystem.MainFormation.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Charge || formationQuerySystem.MainFormation.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.ChargeToTarget || formationQuerySystem.MainFormation.GetReadonlyMovementOrderReference() == MovementOrder.MovementOrderRetreat) ? 0.5f : 1f);
			float num2 = ((formationQuerySystem.MainFormation.GetUnderAttackTypeOfUnits(10f) == Agent.UnderAttackType.UnderMeleeAttack) ? 0.8f : 1f);
			return num * num2;
		}, 5f);
		_weightedAverageEnemyPosition = new QueryData<Vec2>(() => formationQuerySystem.Formation.Team.GetWeightedAverageOfEnemies(formationQuerySystem.Formation.CurrentPosition), 0.5f);
		_highGroundCloseToForeseenBattleGround = new QueryData<Vec2>(delegate
		{
			WorldPosition center = formationQuerySystem.MedianPosition;
			center.SetVec2(formationQuerySystem.AveragePosition);
			WorldPosition referencePosition = formationQuerySystem.Team.MedianTargetFormationPosition;
			return mission.FindPositionWithBiggestSlopeTowardsDirectionInSquare(ref center, formationQuerySystem.AveragePosition.Distance(formationQuerySystem.Team.MedianTargetFormationPosition.AsVec2) * 0.5f, ref referencePosition).AsVec2;
		}, 10f);
		_insideCastleUnitCountIncludingUnpositioned = new QueryData<int>(() => formationQuerySystem.Formation.CountUnitsOnNavMeshIDMod10(1, includeOnlyPositionedUnits: false), 3f);
		_insideCastleUnitCountPositioned = new QueryData<int>(() => formationQuerySystem.Formation.CountUnitsOnNavMeshIDMod10(1, includeOnlyPositionedUnits: true), 3f);
		InitializeTelemetryScopeNames();
	}

	public void EvaluateAllPreliminaryQueryData()
	{
		float currentTime = Mission.Current.CurrentTime;
		_infantryUnitRatio.Evaluate(currentTime);
		_hasShieldUnitRatio.Evaluate(currentTime);
		_rangedUnitRatio.Evaluate(currentTime);
		_cavalryUnitRatio.Evaluate(currentTime);
		_rangedCavalryUnitRatio.Evaluate(currentTime);
		_isInfantryFormation.Evaluate(currentTime);
		_hasShield.Evaluate(currentTime);
		_isRangedFormation.Evaluate(currentTime);
		_isCavalryFormation.Evaluate(currentTime);
		_isRangedCavalryFormation.Evaluate(currentTime);
		_isMeleeFormation.Evaluate(currentTime);
	}

	public void ForceExpireCavalryUnitRatio()
	{
		_cavalryUnitRatio.Expire();
	}

	public void Expire()
	{
		_formationPower.Expire();
		_formationMeleeFightingPower.Expire();
		_averagePosition.Expire();
		_currentVelocity.Expire();
		_estimatedDirection.Expire();
		_medianPosition.Expire();
		_averageAllyPosition.Expire();
		_idealAverageDisplacement.Expire();
		_formationIntegrityData.Expire();
		_localAllyUnits.Expire();
		_localEnemyUnits.Expire();
		_mainClass.Expire();
		_infantryUnitRatio.Expire();
		_hasShieldUnitRatio.Expire();
		_rangedUnitRatio.Expire();
		_cavalryUnitRatio.Expire();
		_rangedCavalryUnitRatio.Expire();
		_isMeleeFormation.Expire();
		_isInfantryFormation.Expire();
		_hasShield.Expire();
		_isRangedFormation.Expire();
		_isCavalryFormation.Expire();
		_isRangedCavalryFormation.Expire();
		_movementSpeedMaximum.Expire();
		_movementSpeed.Expire();
		_maximumMissileRange.Expire();
		_missileRangeAdjusted.Expire();
		_localInfantryUnitRatio.Expire();
		_localRangedUnitRatio.Expire();
		_localCavalryUnitRatio.Expire();
		_localRangedCavalryUnitRatio.Expire();
		_localAllyPower.Expire();
		_localEnemyPower.Expire();
		_localPowerRatio.Expire();
		_casualtyRatio.Expire();
		_isUnderRangedAttack.Expire();
		_underRangedAttackRatio.Expire();
		_makingRangedAttackRatio.Expire();
		_mainFormation.Expire();
		_mainFormationReliabilityFactor.Expire();
		_weightedAverageEnemyPosition.Expire();
		_closestEnemyFormation.Expire();
		_closestSignificantlyLargeEnemyFormation.Expire();
		_fastestSignificantlyLargeEnemyFormation.Expire();
		_highGroundCloseToForeseenBattleGround.Expire();
	}

	public void ExpireAfterUnitAddRemove()
	{
		_formationPower.Expire();
		float currentTime = Mission.Current.CurrentTime;
		_infantryUnitRatio.Evaluate(currentTime);
		_hasShieldUnitRatio.Evaluate(currentTime);
		_rangedUnitRatio.Evaluate(currentTime);
		_cavalryUnitRatio.Evaluate(currentTime);
		_rangedCavalryUnitRatio.Evaluate(currentTime);
		_isMeleeFormation.Evaluate(currentTime);
		_isInfantryFormation.Evaluate(currentTime);
		_hasShield.Evaluate(currentTime);
		_isRangedFormation.Evaluate(currentTime);
		_isCavalryFormation.Evaluate(currentTime);
		_isRangedCavalryFormation.Evaluate(currentTime);
		_mainClass.Evaluate(currentTime);
		if (Formation.CountOfUnits == 0)
		{
			_infantryUnitRatio.SetValue(0f, currentTime);
			_hasShieldUnitRatio.SetValue(0f, currentTime);
			_rangedUnitRatio.SetValue(0f, currentTime);
			_cavalryUnitRatio.SetValue(0f, currentTime);
			_rangedCavalryUnitRatio.SetValue(0f, currentTime);
			_isMeleeFormation.SetValue(value: false, currentTime);
			_isInfantryFormation.SetValue(value: true, currentTime);
			_hasShield.SetValue(value: false, currentTime);
			_isRangedFormation.SetValue(value: false, currentTime);
			_isCavalryFormation.SetValue(value: false, currentTime);
			_isRangedCavalryFormation.SetValue(value: false, currentTime);
		}
	}

	private void InitializeTelemetryScopeNames()
	{
	}

	public Vec2 GetAveragePositionWithMaxAge(float age)
	{
		return _averagePosition.GetCachedValueWithMaxAge(age);
	}

	public float GetClassWeightedFactor(float infantryWeight, float rangedWeight, float cavalryWeight, float rangedCavalryWeight)
	{
		return InfantryUnitRatio * infantryWeight + RangedUnitRatio * rangedWeight + CavalryUnitRatio * cavalryWeight + RangedCavalryUnitRatio * rangedCavalryWeight;
	}
}
