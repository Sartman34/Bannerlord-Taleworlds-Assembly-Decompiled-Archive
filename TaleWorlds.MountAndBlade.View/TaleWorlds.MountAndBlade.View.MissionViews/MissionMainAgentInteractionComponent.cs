using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionMainAgentInteractionComponent
{
	public delegate void MissionFocusGainedEventDelegate(Agent agent, IFocusable focusableObject, bool isInteractable);

	public delegate void MissionFocusLostEventDelegate(Agent agent, IFocusable focusableObject);

	public delegate void MissionFocusHealthChangeDelegate(IFocusable focusable, float healthPercentage, bool hideHealthbarWhenFull);

	private IFocusable _currentInteractableObject;

	private readonly MissionMainAgentController _mainAgentController;

	public IFocusable CurrentFocusedObject { get; private set; }

	public IFocusable CurrentFocusedMachine { get; private set; }

	private Mission CurrentMission => _mainAgentController.Mission;

	private MissionScreen CurrentMissionScreen => _mainAgentController.MissionScreen;

	private Scene CurrentMissionScene => _mainAgentController.Mission.Scene;

	public event MissionFocusGainedEventDelegate OnFocusGained;

	public event MissionFocusLostEventDelegate OnFocusLost;

	public event MissionFocusHealthChangeDelegate OnFocusHealthChanged;

	public void SetCurrentFocusedObject(IFocusable focusedObject, IFocusable focusedMachine, bool isInteractable)
	{
		if (CurrentFocusedObject != null && (CurrentFocusedObject != focusedObject || (_currentInteractableObject != null && !isInteractable) || (_currentInteractableObject == null && isInteractable)))
		{
			FocusLost(CurrentFocusedObject, CurrentFocusedMachine);
			_currentInteractableObject = null;
			CurrentFocusedObject = null;
			CurrentFocusedMachine = null;
		}
		if (CurrentFocusedObject == null && focusedObject != null)
		{
			if (focusedObject != CurrentFocusedObject)
			{
				FocusGained(focusedObject, focusedMachine, isInteractable);
			}
			if (isInteractable)
			{
				_currentInteractableObject = focusedObject;
			}
			CurrentFocusedObject = focusedObject;
			CurrentFocusedMachine = focusedMachine;
		}
	}

	public void ClearFocus()
	{
		if (CurrentFocusedObject != null)
		{
			FocusLost(CurrentFocusedObject, CurrentFocusedMachine);
		}
		_currentInteractableObject = null;
		CurrentFocusedObject = null;
		CurrentFocusedMachine = null;
	}

	public void OnClearScene()
	{
		ClearFocus();
	}

	public MissionMainAgentInteractionComponent(MissionMainAgentController mainAgentController)
	{
		_mainAgentController = mainAgentController;
	}

	private static float GetCollisionDistanceSquaredOfIntersectionFromMainAgentEye(Vec3 rayStartPoint, Vec3 rayDirection, float rayLength)
	{
		float result = rayLength * rayLength;
		Vec3 v = rayStartPoint + rayDirection * rayLength;
		Vec3 position = Agent.Main.Position;
		float eyeGlobalHeight = Agent.Main.GetEyeGlobalHeight();
		Vec3 vec = new Vec3(position.x, position.y, position.z + eyeGlobalHeight);
		float num = v.z - vec.z;
		if (num < 0f)
		{
			num = MBMath.ClampFloat(0f - num, 0f, (Agent.Main.HasMount ? (eyeGlobalHeight - Agent.Main.MountAgent.GetEyeGlobalHeight()) : eyeGlobalHeight) * 0.75f);
			vec.z -= num;
			result = vec.DistanceSquared(v);
		}
		return result;
	}

	private void FocusGained(IFocusable focusedObject, IFocusable focusedMachine, bool isInteractable)
	{
		focusedObject.OnFocusGain(Agent.Main);
		focusedMachine?.OnFocusGain(Agent.Main);
		foreach (MissionBehavior missionBehavior in CurrentMission.MissionBehaviors)
		{
			missionBehavior.OnFocusGained(Agent.Main, focusedObject, isInteractable);
		}
		this.OnFocusGained?.Invoke(Agent.Main, CurrentFocusedObject, isInteractable);
	}

	private void FocusLost(IFocusable focusedObject, IFocusable focusedMachine)
	{
		focusedObject.OnFocusLose(Agent.Main);
		focusedMachine?.OnFocusLose(Agent.Main);
		foreach (MissionBehavior missionBehavior in CurrentMission.MissionBehaviors)
		{
			missionBehavior.OnFocusLost(Agent.Main, focusedObject);
		}
		this.OnFocusLost?.Invoke(Agent.Main, CurrentFocusedObject);
	}

	public void FocusTick()
	{
		IFocusable focusable = null;
		UsableMachine usableMachine = null;
		bool flag = true;
		bool flag2 = true;
		if (Mission.Current.Mode != MissionMode.Conversation && Mission.Current.Mode != MissionMode.CutScene)
		{
			Agent main = Agent.Main;
			if (!CurrentMissionScreen.SceneLayer.Input.IsGameKeyDown(25) && main != null && main.IsOnLand())
			{
				float num = 10f;
				Vec3 direction = CurrentMissionScreen.CombatCamera.Direction;
				Vec3 vec = direction;
				Vec3 position = CurrentMissionScreen.CombatCamera.Position;
				Vec3 position2 = main.Position;
				float num2 = new Vec3(position.x, position.y).Distance(new Vec3(position2.x, position2.y));
				Vec3 vec2 = position * (1f - num2) + (position + direction) * num2;
				if (CurrentMissionScene.RayCastForClosestEntityOrTerrain(vec2, vec2 + vec * num, out var collisionDistance, 0.01f, BodyFlags.CommonFlagsThatDoNotBlocksRay))
				{
					num = collisionDistance;
				}
				float num3 = float.MaxValue;
				Agent agent = CurrentMission.RayCastForClosestAgent(vec2, vec2 + vec * (num + 0.01f), out collisionDistance, main.Index, 0.3f);
				if (agent != null && (!agent.IsMount || (agent.RiderAgent == null && main.MountAgent == null && main.CanReachAgent(agent))))
				{
					num3 = collisionDistance;
					focusable = agent;
					if (!main.CanInteractWithAgent(agent, CurrentMissionScreen.CameraElevation))
					{
						flag2 = false;
					}
				}
				float num4 = 3f;
				num += 0.1f;
				if ((CurrentMissionScene.RayCastForClosestEntityOrTerrain(vec2, vec2 + vec * num, out collisionDistance, out GameEntity collidedEntity, 0.2f, BodyFlags.CommonFocusRayCastExcludeFlags) && collidedEntity != null && collisionDistance < num3) || (CurrentMissionScene.RayCastForClosestEntityOrTerrain(vec2, vec2 + vec * num, out collisionDistance, out collidedEntity, 0.2f * num4, BodyFlags.CommonFocusRayCastExcludeFlags) && collidedEntity != null && collisionDistance < num3))
				{
					while (!collidedEntity.GetScriptComponents().Any((ScriptComponentBehavior sc) => sc is IFocusable) && collidedEntity.Parent != null)
					{
						collidedEntity = collidedEntity.Parent;
					}
					usableMachine = collidedEntity.GetFirstScriptOfType<UsableMachine>();
					if (usableMachine != null && !usableMachine.IsDisabled)
					{
						GameEntity validStandingPointForAgent = usableMachine.GetValidStandingPointForAgent(main);
						if (validStandingPointForAgent != null)
						{
							collidedEntity = validStandingPointForAgent;
						}
					}
					flag = false;
					UsableMissionObject firstScriptOfType = collidedEntity.GetFirstScriptOfType<UsableMissionObject>();
					if (firstScriptOfType is SpawnedItemEntity)
					{
						if (CurrentMission.IsMainAgentItemInteractionEnabled && main.CanReachObject(firstScriptOfType, GetCollisionDistanceSquaredOfIntersectionFromMainAgentEye(vec2, vec, collisionDistance)))
						{
							focusable = firstScriptOfType;
							if (main.CanUseObject(firstScriptOfType))
							{
								flag = true;
							}
						}
					}
					else if (firstScriptOfType != null)
					{
						focusable = firstScriptOfType;
						if (CurrentMission.IsMainAgentObjectInteractionEnabled && !main.IsUsingGameObject && main.IsOnLand() && main.ObjectHasVacantPosition(firstScriptOfType))
						{
							flag = true;
						}
					}
					else if (usableMachine != null)
					{
						focusable = usableMachine;
					}
					else if (collidedEntity.GetScriptComponents().FirstOrDefault((ScriptComponentBehavior sc) => sc is IFocusable) is IFocusable focusable2)
					{
						focusable = focusable2;
					}
				}
				if ((focusable == null || !flag) && main.MountAgent != null && main.CanInteractWithAgent(main.MountAgent, CurrentMissionScreen.CameraElevation))
				{
					focusable = main.MountAgent;
					flag = true;
				}
			}
			if (focusable == null)
			{
				ClearFocus();
				return;
			}
			bool isInteractable = ((focusable is Agent) ? flag2 : flag);
			SetCurrentFocusedObject(focusable, usableMachine, isInteractable);
		}
		else if (CurrentFocusedObject != null && Mission.Current.Mode != MissionMode.Conversation)
		{
			ClearFocus();
		}
	}

	public void FocusStateCheckTick()
	{
		if (!CurrentMissionScreen.SceneLayer.Input.IsGameKeyPressed(13) || (!CurrentMission.IsMainAgentObjectInteractionEnabled && !IsFocusMountable()) || CurrentMission.IsOrderMenuOpen)
		{
			return;
		}
		Agent main = Agent.Main;
		if (_currentInteractableObject is UsableMissionObject usableMissionObject)
		{
			if (!main.IsUsingGameObject && main.IsOnLand() && !(usableMissionObject is SpawnedItemEntity) && main.ObjectHasVacantPosition(usableMissionObject))
			{
				main.HandleStartUsingAction(usableMissionObject, -1);
			}
			return;
		}
		Agent agent = _currentInteractableObject as Agent;
		if (main.IsOnLand() && agent != null)
		{
			agent.OnUse(main);
		}
		else if (main.IsUsingGameObject && !(main.CurrentlyUsedGameObject is SpawnedItemEntity) && (agent == null || !(main.CurrentlyUsedGameObject is StandingPoint { PlayerStopsUsingWhenInteractsWithOther: not false })))
		{
			main.HandleStopUsingAction();
			ClearFocus();
		}
	}

	private bool IsFocusMountable()
	{
		if (_currentInteractableObject is Agent agent)
		{
			return agent.IsMount;
		}
		return false;
	}

	public void FocusedItemHealthTick()
	{
		if (CurrentFocusedObject is UsableMissionObject { GameEntity: var gameEntity })
		{
			while (gameEntity != null && !gameEntity.HasScriptOfType<UsableMachine>())
			{
				gameEntity = gameEntity.Parent;
			}
			if (gameEntity != null)
			{
				UsableMachine firstScriptOfType = gameEntity.GetFirstScriptOfType<UsableMachine>();
				if (firstScriptOfType?.DestructionComponent != null)
				{
					this.OnFocusHealthChanged?.Invoke(CurrentFocusedObject, firstScriptOfType.DestructionComponent.HitPoint / firstScriptOfType.DestructionComponent.MaxHitPoint, hideHealthbarWhenFull: true);
				}
			}
		}
		else if (CurrentFocusedObject is UsableMachine usableMachine)
		{
			if (usableMachine.DestructionComponent != null)
			{
				this.OnFocusHealthChanged?.Invoke(CurrentFocusedObject, usableMachine.DestructionComponent.HitPoint / usableMachine.DestructionComponent.MaxHitPoint, hideHealthbarWhenFull: true);
			}
		}
		else if (CurrentFocusedObject is DestructableComponent destructableComponent)
		{
			this.OnFocusHealthChanged?.Invoke(CurrentFocusedObject, destructableComponent.HitPoint / destructableComponent.MaxHitPoint, hideHealthbarWhenFull: true);
		}
	}
}
