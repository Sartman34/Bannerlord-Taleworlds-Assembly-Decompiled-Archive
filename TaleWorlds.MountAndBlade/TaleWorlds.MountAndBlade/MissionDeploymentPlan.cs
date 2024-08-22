using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class MissionDeploymentPlan : IMissionDeploymentPlan
{
	public const int NumFormationsWithUnset = 11;

	private readonly Mission _mission;

	private readonly BattleSideDeploymentPlan[] _battleSideDeploymentPlans = new BattleSideDeploymentPlan[2];

	private readonly WorldFrame?[] _playerSpawnFrames = new WorldFrame?[2];

	private FormationSceneSpawnEntry[,] _formationSceneSpawnEntries;

	public MissionDeploymentPlan(Mission mission)
	{
		_mission = mission;
		for (int i = 0; i < 2; i++)
		{
			BattleSideEnum side = (BattleSideEnum)i;
			_battleSideDeploymentPlans[i] = new BattleSideDeploymentPlan(mission, side);
			_playerSpawnFrames[i] = null;
		}
	}

	public void CreateReinforcementPlans()
	{
		for (int i = 0; i < 2; i++)
		{
			_battleSideDeploymentPlans[i].CreateReinforcementPlans();
		}
	}

	public void ClearDeploymentPlanForSide(BattleSideEnum battleSide, DeploymentPlanType planType)
	{
		_battleSideDeploymentPlans[(int)battleSide].ClearPlans(planType);
	}

	public bool HasPlayerSpawnFrame(BattleSideEnum battleSide)
	{
		return _playerSpawnFrames[(int)battleSide].HasValue;
	}

	public bool GetPlayerSpawnFrame(BattleSideEnum battleSide, out WorldPosition position, out Vec2 direction)
	{
		WorldFrame? worldFrame = _playerSpawnFrames[(int)battleSide];
		if (worldFrame.HasValue)
		{
			position = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, worldFrame.Value.Origin.GetGroundVec3(), hasValidZ: false);
			direction = worldFrame.Value.Rotation.f.AsVec2.Normalized();
			return true;
		}
		position = WorldPosition.Invalid;
		direction = Vec2.Invalid;
		return false;
	}

	public static bool HasSignificantMountedTroops(int footTroopCount, int mountedTroopCount)
	{
		return (float)mountedTroopCount / Math.Max(mountedTroopCount + footTroopCount, 1f) >= 0.1f;
	}

	public void ClearAddedTroopsForBattleSide(BattleSideEnum battleSide, DeploymentPlanType planType)
	{
		_battleSideDeploymentPlans[(int)battleSide].ClearAddedTroops(planType);
	}

	public void ClearAll()
	{
		for (int i = 0; i < 2; i++)
		{
			_battleSideDeploymentPlans[i].ClearAddedTroops(DeploymentPlanType.Initial);
			_battleSideDeploymentPlans[i].ClearPlans(DeploymentPlanType.Initial);
			_battleSideDeploymentPlans[i].ClearAddedTroops(DeploymentPlanType.Reinforcement);
			_battleSideDeploymentPlans[i].ClearPlans(DeploymentPlanType.Reinforcement);
		}
	}

	public void AddTroopsForBattleSide(BattleSideEnum battleSide, DeploymentPlanType planType, FormationClass formationClass, int footTroopCount, int mountedTroopCount)
	{
		_battleSideDeploymentPlans[(int)battleSide].AddTroops(formationClass, footTroopCount, mountedTroopCount, planType);
	}

	public void SetSpawnWithHorsesForSide(BattleSideEnum battleSide, bool spawnWithHorses)
	{
		_battleSideDeploymentPlans[(int)battleSide].SetSpawnWithHorses(spawnWithHorses);
	}

	public void PlanBattleDeployment(BattleSideEnum battleSide, DeploymentPlanType planType, float spawnPathOffset = 0f)
	{
		if (!battleSide.IsValid())
		{
			Debug.FailedAssert("Cannot make deployment plan. Battle side is not valid", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Deployment\\MissionDeploymentPlan.cs", "PlanBattleDeployment", 126);
			return;
		}
		BattleSideDeploymentPlan battleSideDeploymentPlan = _battleSideDeploymentPlans[(int)battleSide];
		if (_battleSideDeploymentPlans[(int)battleSide].IsPlanMade(planType))
		{
			battleSideDeploymentPlan.ClearPlans(planType);
		}
		if (!_mission.HasSpawnPath && _formationSceneSpawnEntries == null)
		{
			ReadSpawnEntitiesFromScene(_mission.IsFieldBattle);
		}
		battleSideDeploymentPlan.PlanBattleDeployment(_formationSceneSpawnEntries, planType, spawnPathOffset);
	}

	public bool IsPositionInsideDeploymentBoundaries(BattleSideEnum battleSide, in Vec2 position)
	{
		BattleSideDeploymentPlan battleSideDeploymentPlan = _battleSideDeploymentPlans[(int)battleSide];
		if (battleSideDeploymentPlan.HasDeploymentBoundaries())
		{
			return battleSideDeploymentPlan.IsPositionInsideDeploymentBoundaries(in position);
		}
		Debug.FailedAssert("Cannot check if position is within deployment boundaries as requested battle side does not have deployment boundaries.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Deployment\\MissionDeploymentPlan.cs", "IsPositionInsideDeploymentBoundaries", 155);
		return false;
	}

	public bool IsPositionInsideSiegeDeploymentBoundaries(in Vec2 position)
	{
		bool result = false;
		foreach (ICollection<Vec2> value in _mission.Boundaries.Values)
		{
			if (MBSceneUtilities.IsPointInsideBoundaries(in position, value.ToList()))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public Vec2 GetClosestDeploymentBoundaryPosition(BattleSideEnum battleSide, in Vec2 position, bool withNavMesh = false, float positionZ = 0f)
	{
		BattleSideDeploymentPlan battleSideDeploymentPlan = _battleSideDeploymentPlans[(int)battleSide];
		if (battleSideDeploymentPlan.HasDeploymentBoundaries())
		{
			return battleSideDeploymentPlan.GetClosestDeploymentBoundaryPosition(in position, withNavMesh, positionZ);
		}
		Debug.FailedAssert("Cannot retrieve closest deployment boundary position as requested battle side does not have deployment boundaries.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Deployment\\MissionDeploymentPlan.cs", "GetClosestDeploymentBoundaryPosition", 183);
		return position;
	}

	public int GetTroopCountForSide(BattleSideEnum side, DeploymentPlanType planType)
	{
		return _battleSideDeploymentPlans[(int)side].GetTroopCount(planType);
	}

	public float GetSpawnPathOffsetForSide(BattleSideEnum side, DeploymentPlanType planType)
	{
		return _battleSideDeploymentPlans[(int)side].GetSpawnPathOffset(planType);
	}

	public IFormationDeploymentPlan GetFormationPlan(BattleSideEnum side, FormationClass fClass, DeploymentPlanType planType)
	{
		return _battleSideDeploymentPlans[(int)side].GetFormationPlan(fClass, planType);
	}

	public bool IsPlanMadeForBattleSide(BattleSideEnum side, DeploymentPlanType planType)
	{
		return _battleSideDeploymentPlans[(int)side].IsPlanMade(planType);
	}

	public bool IsPlanMadeForBattleSide(BattleSideEnum side, out bool isFirstPlan, DeploymentPlanType planType)
	{
		isFirstPlan = false;
		if (_battleSideDeploymentPlans[(int)side].IsPlanMade(planType))
		{
			isFirstPlan = _battleSideDeploymentPlans[(int)side].IsFirstPlan(planType);
			return true;
		}
		return false;
	}

	public bool IsInitialPlanSuitableForFormations(BattleSideEnum side, (int, int)[] troopDataPerFormationClass)
	{
		return _battleSideDeploymentPlans[(int)side].IsInitialPlanSuitableForFormations(troopDataPerFormationClass);
	}

	public bool HasDeploymentBoundaries(BattleSideEnum side)
	{
		return _battleSideDeploymentPlans[(int)side].HasDeploymentBoundaries();
	}

	public MatrixFrame GetBattleSideDeploymentFrame(BattleSideEnum side)
	{
		if (IsPlanMadeForBattleSide(side, DeploymentPlanType.Initial))
		{
			return _battleSideDeploymentPlans[(int)side].GetDeploymentFrame();
		}
		Debug.FailedAssert("Cannot retrieve formation deployment frame as deployment plan is not made for this battle side.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Deployment\\MissionDeploymentPlan.cs", "GetBattleSideDeploymentFrame", 237);
		return MatrixFrame.Identity;
	}

	public Vec3 GetMeanPositionOfPlan(BattleSideEnum side, DeploymentPlanType planType)
	{
		if (IsPlanMadeForBattleSide(side, planType))
		{
			return _battleSideDeploymentPlans[(int)side].GetMeanPositionOfPlan(planType);
		}
		Debug.FailedAssert("Cannot retrieve formation deployment frame as deployment plan is not made for this battle side.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Deployment\\MissionDeploymentPlan.cs", "GetMeanPositionOfPlan", 250);
		return Vec3.Invalid;
	}

	public MBReadOnlyList<(string id, List<Vec2> points)> GetDeploymentBoundaries(BattleSideEnum side)
	{
		if (HasDeploymentBoundaries(side))
		{
			return _battleSideDeploymentPlans[(int)side].DeploymentBoundaries;
		}
		Debug.FailedAssert("Cannot retrieve battle side deployment boundaries as they are not available for this battle side.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Deployment\\MissionDeploymentPlan.cs", "GetDeploymentBoundaries", 263);
		return null;
	}

	public void UpdateReinforcementPlan(BattleSideEnum side)
	{
		_battleSideDeploymentPlans[(int)side].UpdateReinforcementPlans();
	}

	private void ReadSpawnEntitiesFromScene(bool isFieldBattle)
	{
		for (int i = 0; i < 2; i++)
		{
			_playerSpawnFrames[i] = null;
		}
		_formationSceneSpawnEntries = new FormationSceneSpawnEntry[2, 11];
		Scene scene = _mission.Scene;
		if (isFieldBattle)
		{
			for (int j = 0; j < 2; j++)
			{
				string text = ((j == 1) ? "attacker_" : "defender_");
				for (int k = 0; k < 11; k++)
				{
					FormationClass formationClass = (FormationClass)k;
					GameEntity gameEntity = scene.FindEntityWithTag(text + formationClass.GetName().ToLower());
					if (gameEntity == null)
					{
						FormationClass formationClass2 = formationClass.FallbackClass();
						int num = (int)formationClass2;
						GameEntity spawnEntity = _formationSceneSpawnEntries[j, num].SpawnEntity;
						gameEntity = ((spawnEntity != null) ? spawnEntity : scene.FindEntityWithTag(text + formationClass2.GetName().ToLower()));
						formationClass = ((gameEntity != null) ? formationClass2 : FormationClass.NumberOfAllFormations);
					}
					_formationSceneSpawnEntries[j, k] = new FormationSceneSpawnEntry(formationClass, gameEntity, gameEntity);
				}
			}
		}
		else
		{
			GameEntity gameEntity2 = null;
			if (_mission.IsSallyOutBattle)
			{
				gameEntity2 = scene.FindEntityWithTag("sally_out_ambush_battle_set");
			}
			if (gameEntity2 != null)
			{
				ReadSallyOutEntitiesFromScene(scene, gameEntity2);
			}
			else
			{
				ReadSiegeBattleEntitiesFromScene(scene, BattleSideEnum.Defender);
			}
			ReadSiegeBattleEntitiesFromScene(scene, BattleSideEnum.Attacker);
		}
	}

	private void ReadSallyOutEntitiesFromScene(Scene missionScene, GameEntity sallyOutSetEntity)
	{
		int num = 0;
		MatrixFrame globalFrame = sallyOutSetEntity.GetFirstChildEntityWithTag("sally_out_ambush_player").GetGlobalFrame();
		WorldPosition origin = new WorldPosition(_mission.Scene, UIntPtr.Zero, globalFrame.origin, hasValidZ: false);
		_playerSpawnFrames[num] = new WorldFrame(globalFrame.rotation, origin);
		GameEntity firstChildEntityWithTag = sallyOutSetEntity.GetFirstChildEntityWithTag("sally_out_ambush_infantry");
		GameEntity firstChildEntityWithTag2 = sallyOutSetEntity.GetFirstChildEntityWithTag("sally_out_ambush_archer");
		GameEntity firstChildEntityWithTag3 = sallyOutSetEntity.GetFirstChildEntityWithTag("sally_out_ambush_cavalry");
		for (int i = 0; i < 11; i++)
		{
			FormationClass formationClass = (FormationClass)i;
			FormationClass formationClass2 = formationClass.FallbackClass();
			GameEntity gameEntity = null;
			switch (formationClass2)
			{
			case FormationClass.Infantry:
				gameEntity = firstChildEntityWithTag;
				break;
			case FormationClass.Ranged:
				gameEntity = firstChildEntityWithTag2;
				break;
			case FormationClass.Cavalry:
			case FormationClass.HorseArcher:
				gameEntity = firstChildEntityWithTag3;
				break;
			}
			_formationSceneSpawnEntries[num, i] = new FormationSceneSpawnEntry(formationClass, gameEntity, gameEntity);
		}
	}

	private void ReadSiegeBattleEntitiesFromScene(Scene missionScene, BattleSideEnum battleSide)
	{
		int num = (int)battleSide;
		string text = battleSide.ToString().ToLower() + "_";
		for (int i = 0; i < 11; i++)
		{
			FormationClass formationClass = (FormationClass)i;
			string text2 = text + formationClass.GetName().ToLower();
			string tag = text2 + "_reinforcement";
			GameEntity gameEntity = missionScene.FindEntityWithTag(text2);
			GameEntity gameEntity2 = null;
			if (gameEntity == null)
			{
				FormationClass formationClass2 = formationClass.FallbackClass();
				int num2 = (int)formationClass2;
				FormationSceneSpawnEntry formationSceneSpawnEntry = _formationSceneSpawnEntries[num, num2];
				if (formationSceneSpawnEntry.SpawnEntity != null)
				{
					gameEntity = formationSceneSpawnEntry.SpawnEntity;
					gameEntity2 = formationSceneSpawnEntry.ReinforcementSpawnEntity;
				}
				else
				{
					text2 = text + formationClass2.GetName().ToLower();
					tag = text2 + "_reinforcement";
					gameEntity = missionScene.FindEntityWithTag(text2);
					gameEntity2 = missionScene.FindEntityWithTag(tag);
				}
				formationClass = ((gameEntity != null) ? formationClass2 : FormationClass.NumberOfAllFormations);
			}
			else
			{
				gameEntity2 = missionScene.FindEntityWithTag(tag);
			}
			if (gameEntity2 == null)
			{
				gameEntity2 = gameEntity;
			}
			_formationSceneSpawnEntries[num, i] = new FormationSceneSpawnEntry(formationClass, gameEntity, gameEntity2);
		}
	}

	bool IMissionDeploymentPlan.IsPositionInsideDeploymentBoundaries(BattleSideEnum battleSide, in Vec2 position)
	{
		return IsPositionInsideDeploymentBoundaries(battleSide, in position);
	}

	Vec2 IMissionDeploymentPlan.GetClosestDeploymentBoundaryPosition(BattleSideEnum battleSide, in Vec2 position, bool withNavMesh, float positionZ)
	{
		return GetClosestDeploymentBoundaryPosition(battleSide, in position, withNavMesh, positionZ);
	}
}
