using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ManagedCallbacks;

internal static class ScriptingInterfaceObjects
{
	public static Dictionary<string, object> GetObjects()
	{
		return new Dictionary<string, object>
		{
			{
				"TaleWorlds.MountAndBlade.IMBActionSet",
				new ScriptingInterfaceOfIMBActionSet()
			},
			{
				"TaleWorlds.MountAndBlade.IMBAgent",
				new ScriptingInterfaceOfIMBAgent()
			},
			{
				"TaleWorlds.MountAndBlade.IMBAgentVisuals",
				new ScriptingInterfaceOfIMBAgentVisuals()
			},
			{
				"TaleWorlds.MountAndBlade.IMBAnimation",
				new ScriptingInterfaceOfIMBAnimation()
			},
			{
				"TaleWorlds.MountAndBlade.IMBBannerlordChecker",
				new ScriptingInterfaceOfIMBBannerlordChecker()
			},
			{
				"TaleWorlds.MountAndBlade.IMBBannerlordConfig",
				new ScriptingInterfaceOfIMBBannerlordConfig()
			},
			{
				"TaleWorlds.MountAndBlade.IMBBannerlordTableauManager",
				new ScriptingInterfaceOfIMBBannerlordTableauManager()
			},
			{
				"TaleWorlds.MountAndBlade.IMBDebugExtensions",
				new ScriptingInterfaceOfIMBDebugExtensions()
			},
			{
				"TaleWorlds.MountAndBlade.IMBDelegate",
				new ScriptingInterfaceOfIMBDelegate()
			},
			{
				"TaleWorlds.MountAndBlade.IMBEditor",
				new ScriptingInterfaceOfIMBEditor()
			},
			{
				"TaleWorlds.MountAndBlade.IMBFaceGen",
				new ScriptingInterfaceOfIMBFaceGen()
			},
			{
				"TaleWorlds.MountAndBlade.IMBGame",
				new ScriptingInterfaceOfIMBGame()
			},
			{
				"TaleWorlds.MountAndBlade.IMBGameEntityExtensions",
				new ScriptingInterfaceOfIMBGameEntityExtensions()
			},
			{
				"TaleWorlds.MountAndBlade.IMBItem",
				new ScriptingInterfaceOfIMBItem()
			},
			{
				"TaleWorlds.MountAndBlade.IMBMapScene",
				new ScriptingInterfaceOfIMBMapScene()
			},
			{
				"TaleWorlds.MountAndBlade.IMBMessageManager",
				new ScriptingInterfaceOfIMBMessageManager()
			},
			{
				"TaleWorlds.MountAndBlade.IMBMission",
				new ScriptingInterfaceOfIMBMission()
			},
			{
				"TaleWorlds.MountAndBlade.IMBMultiplayerData",
				new ScriptingInterfaceOfIMBMultiplayerData()
			},
			{
				"TaleWorlds.MountAndBlade.IMBNetwork",
				new ScriptingInterfaceOfIMBNetwork()
			},
			{
				"TaleWorlds.MountAndBlade.IMBPeer",
				new ScriptingInterfaceOfIMBPeer()
			},
			{
				"TaleWorlds.MountAndBlade.IMBScreen",
				new ScriptingInterfaceOfIMBScreen()
			},
			{
				"TaleWorlds.MountAndBlade.IMBSkeletonExtensions",
				new ScriptingInterfaceOfIMBSkeletonExtensions()
			},
			{
				"TaleWorlds.MountAndBlade.IMBSoundEvent",
				new ScriptingInterfaceOfIMBSoundEvent()
			},
			{
				"TaleWorlds.MountAndBlade.IMBTeam",
				new ScriptingInterfaceOfIMBTeam()
			},
			{
				"TaleWorlds.MountAndBlade.IMBTestRun",
				new ScriptingInterfaceOfIMBTestRun()
			},
			{
				"TaleWorlds.MountAndBlade.IMBVoiceManager",
				new ScriptingInterfaceOfIMBVoiceManager()
			},
			{
				"TaleWorlds.MountAndBlade.IMBWindowManager",
				new ScriptingInterfaceOfIMBWindowManager()
			},
			{
				"TaleWorlds.MountAndBlade.IMBWorld",
				new ScriptingInterfaceOfIMBWorld()
			}
		};
	}

	public static void SetFunctionPointer(int id, IntPtr pointer)
	{
		switch (id)
		{
		case 0:
			ScriptingInterfaceOfIMBActionSet.call_AreActionsAlternativesDelegate = (ScriptingInterfaceOfIMBActionSet.AreActionsAlternativesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.AreActionsAlternativesDelegate));
			break;
		case 1:
			ScriptingInterfaceOfIMBActionSet.call_GetAnimationNameDelegate = (ScriptingInterfaceOfIMBActionSet.GetAnimationNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.GetAnimationNameDelegate));
			break;
		case 2:
			ScriptingInterfaceOfIMBActionSet.call_GetBoneHasParentBoneDelegate = (ScriptingInterfaceOfIMBActionSet.GetBoneHasParentBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.GetBoneHasParentBoneDelegate));
			break;
		case 3:
			ScriptingInterfaceOfIMBActionSet.call_GetBoneIndexWithIdDelegate = (ScriptingInterfaceOfIMBActionSet.GetBoneIndexWithIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.GetBoneIndexWithIdDelegate));
			break;
		case 4:
			ScriptingInterfaceOfIMBActionSet.call_GetIndexWithIDDelegate = (ScriptingInterfaceOfIMBActionSet.GetIndexWithIDDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.GetIndexWithIDDelegate));
			break;
		case 5:
			ScriptingInterfaceOfIMBActionSet.call_GetNameWithIndexDelegate = (ScriptingInterfaceOfIMBActionSet.GetNameWithIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.GetNameWithIndexDelegate));
			break;
		case 6:
			ScriptingInterfaceOfIMBActionSet.call_GetNumberOfActionSetsDelegate = (ScriptingInterfaceOfIMBActionSet.GetNumberOfActionSetsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.GetNumberOfActionSetsDelegate));
			break;
		case 7:
			ScriptingInterfaceOfIMBActionSet.call_GetNumberOfMonsterUsageSetsDelegate = (ScriptingInterfaceOfIMBActionSet.GetNumberOfMonsterUsageSetsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.GetNumberOfMonsterUsageSetsDelegate));
			break;
		case 8:
			ScriptingInterfaceOfIMBActionSet.call_GetSkeletonNameDelegate = (ScriptingInterfaceOfIMBActionSet.GetSkeletonNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBActionSet.GetSkeletonNameDelegate));
			break;
		case 9:
			ScriptingInterfaceOfIMBAgent.call_AddMeshToBoneDelegate = (ScriptingInterfaceOfIMBAgent.AddMeshToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.AddMeshToBoneDelegate));
			break;
		case 10:
			ScriptingInterfaceOfIMBAgent.call_AddPrefabToAgentBoneDelegate = (ScriptingInterfaceOfIMBAgent.AddPrefabToAgentBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.AddPrefabToAgentBoneDelegate));
			break;
		case 11:
			ScriptingInterfaceOfIMBAgent.call_AttachWeaponToBoneDelegate = (ScriptingInterfaceOfIMBAgent.AttachWeaponToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.AttachWeaponToBoneDelegate));
			break;
		case 12:
			ScriptingInterfaceOfIMBAgent.call_AttachWeaponToWeaponInSlotDelegate = (ScriptingInterfaceOfIMBAgent.AttachWeaponToWeaponInSlotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.AttachWeaponToWeaponInSlotDelegate));
			break;
		case 13:
			ScriptingInterfaceOfIMBAgent.call_AttackDirectionToMovementFlagDelegate = (ScriptingInterfaceOfIMBAgent.AttackDirectionToMovementFlagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.AttackDirectionToMovementFlagDelegate));
			break;
		case 14:
			ScriptingInterfaceOfIMBAgent.call_BuildDelegate = (ScriptingInterfaceOfIMBAgent.BuildDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.BuildDelegate));
			break;
		case 15:
			ScriptingInterfaceOfIMBAgent.call_CanMoveDirectlyToPositionDelegate = (ScriptingInterfaceOfIMBAgent.CanMoveDirectlyToPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.CanMoveDirectlyToPositionDelegate));
			break;
		case 16:
			ScriptingInterfaceOfIMBAgent.call_CheckPathToAITargetAgentPassesThroughNavigationFaceIdFromDirectionDelegate = (ScriptingInterfaceOfIMBAgent.CheckPathToAITargetAgentPassesThroughNavigationFaceIdFromDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.CheckPathToAITargetAgentPassesThroughNavigationFaceIdFromDirectionDelegate));
			break;
		case 17:
			ScriptingInterfaceOfIMBAgent.call_ClearEquipmentDelegate = (ScriptingInterfaceOfIMBAgent.ClearEquipmentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.ClearEquipmentDelegate));
			break;
		case 18:
			ScriptingInterfaceOfIMBAgent.call_ClearHandInverseKinematicsDelegate = (ScriptingInterfaceOfIMBAgent.ClearHandInverseKinematicsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.ClearHandInverseKinematicsDelegate));
			break;
		case 19:
			ScriptingInterfaceOfIMBAgent.call_ClearTargetFrameDelegate = (ScriptingInterfaceOfIMBAgent.ClearTargetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.ClearTargetFrameDelegate));
			break;
		case 20:
			ScriptingInterfaceOfIMBAgent.call_ComputeAnimationDisplacementDelegate = (ScriptingInterfaceOfIMBAgent.ComputeAnimationDisplacementDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.ComputeAnimationDisplacementDelegate));
			break;
		case 21:
			ScriptingInterfaceOfIMBAgent.call_CreateBloodBurstAtLimbDelegate = (ScriptingInterfaceOfIMBAgent.CreateBloodBurstAtLimbDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.CreateBloodBurstAtLimbDelegate));
			break;
		case 22:
			ScriptingInterfaceOfIMBAgent.call_DebugMoreDelegate = (ScriptingInterfaceOfIMBAgent.DebugMoreDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.DebugMoreDelegate));
			break;
		case 23:
			ScriptingInterfaceOfIMBAgent.call_DefendDirectionToMovementFlagDelegate = (ScriptingInterfaceOfIMBAgent.DefendDirectionToMovementFlagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.DefendDirectionToMovementFlagDelegate));
			break;
		case 24:
			ScriptingInterfaceOfIMBAgent.call_DeleteAttachedWeaponFromBoneDelegate = (ScriptingInterfaceOfIMBAgent.DeleteAttachedWeaponFromBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.DeleteAttachedWeaponFromBoneDelegate));
			break;
		case 25:
			ScriptingInterfaceOfIMBAgent.call_DieDelegate = (ScriptingInterfaceOfIMBAgent.DieDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.DieDelegate));
			break;
		case 26:
			ScriptingInterfaceOfIMBAgent.call_DisableLookToPointOfInterestDelegate = (ScriptingInterfaceOfIMBAgent.DisableLookToPointOfInterestDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.DisableLookToPointOfInterestDelegate));
			break;
		case 27:
			ScriptingInterfaceOfIMBAgent.call_DisableScriptedCombatMovementDelegate = (ScriptingInterfaceOfIMBAgent.DisableScriptedCombatMovementDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.DisableScriptedCombatMovementDelegate));
			break;
		case 28:
			ScriptingInterfaceOfIMBAgent.call_DisableScriptedMovementDelegate = (ScriptingInterfaceOfIMBAgent.DisableScriptedMovementDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.DisableScriptedMovementDelegate));
			break;
		case 29:
			ScriptingInterfaceOfIMBAgent.call_DropItemDelegate = (ScriptingInterfaceOfIMBAgent.DropItemDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.DropItemDelegate));
			break;
		case 30:
			ScriptingInterfaceOfIMBAgent.call_EnforceShieldUsageDelegate = (ScriptingInterfaceOfIMBAgent.EnforceShieldUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.EnforceShieldUsageDelegate));
			break;
		case 31:
			ScriptingInterfaceOfIMBAgent.call_FadeInDelegate = (ScriptingInterfaceOfIMBAgent.FadeInDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.FadeInDelegate));
			break;
		case 32:
			ScriptingInterfaceOfIMBAgent.call_FadeOutDelegate = (ScriptingInterfaceOfIMBAgent.FadeOutDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.FadeOutDelegate));
			break;
		case 33:
			ScriptingInterfaceOfIMBAgent.call_ForceAiBehaviorSelectionDelegate = (ScriptingInterfaceOfIMBAgent.ForceAiBehaviorSelectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.ForceAiBehaviorSelectionDelegate));
			break;
		case 34:
			ScriptingInterfaceOfIMBAgent.call_GetActionChannelCurrentActionWeightDelegate = (ScriptingInterfaceOfIMBAgent.GetActionChannelCurrentActionWeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetActionChannelCurrentActionWeightDelegate));
			break;
		case 35:
			ScriptingInterfaceOfIMBAgent.call_GetActionChannelWeightDelegate = (ScriptingInterfaceOfIMBAgent.GetActionChannelWeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetActionChannelWeightDelegate));
			break;
		case 36:
			ScriptingInterfaceOfIMBAgent.call_GetActionDirectionDelegate = (ScriptingInterfaceOfIMBAgent.GetActionDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetActionDirectionDelegate));
			break;
		case 37:
			ScriptingInterfaceOfIMBAgent.call_GetActionSetNoDelegate = (ScriptingInterfaceOfIMBAgent.GetActionSetNoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetActionSetNoDelegate));
			break;
		case 38:
			ScriptingInterfaceOfIMBAgent.call_GetAgentFacialAnimationDelegate = (ScriptingInterfaceOfIMBAgent.GetAgentFacialAnimationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAgentFacialAnimationDelegate));
			break;
		case 39:
			ScriptingInterfaceOfIMBAgent.call_GetAgentFlagsDelegate = (ScriptingInterfaceOfIMBAgent.GetAgentFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAgentFlagsDelegate));
			break;
		case 40:
			ScriptingInterfaceOfIMBAgent.call_GetAgentScaleDelegate = (ScriptingInterfaceOfIMBAgent.GetAgentScaleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAgentScaleDelegate));
			break;
		case 41:
			ScriptingInterfaceOfIMBAgent.call_GetAgentVisualsDelegate = (ScriptingInterfaceOfIMBAgent.GetAgentVisualsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAgentVisualsDelegate));
			break;
		case 42:
			ScriptingInterfaceOfIMBAgent.call_GetAgentVoiceDefinitionDelegate = (ScriptingInterfaceOfIMBAgent.GetAgentVoiceDefinitionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAgentVoiceDefinitionDelegate));
			break;
		case 43:
			ScriptingInterfaceOfIMBAgent.call_GetAimingTimerDelegate = (ScriptingInterfaceOfIMBAgent.GetAimingTimerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAimingTimerDelegate));
			break;
		case 44:
			ScriptingInterfaceOfIMBAgent.call_GetAIStateFlagsDelegate = (ScriptingInterfaceOfIMBAgent.GetAIStateFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAIStateFlagsDelegate));
			break;
		case 45:
			ScriptingInterfaceOfIMBAgent.call_GetAttackDirectionDelegate = (ScriptingInterfaceOfIMBAgent.GetAttackDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAttackDirectionDelegate));
			break;
		case 46:
			ScriptingInterfaceOfIMBAgent.call_GetAttackDirectionUsageDelegate = (ScriptingInterfaceOfIMBAgent.GetAttackDirectionUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAttackDirectionUsageDelegate));
			break;
		case 47:
			ScriptingInterfaceOfIMBAgent.call_GetAverageVelocityDelegate = (ScriptingInterfaceOfIMBAgent.GetAverageVelocityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetAverageVelocityDelegate));
			break;
		case 48:
			ScriptingInterfaceOfIMBAgent.call_GetBodyRotationConstraintDelegate = (ScriptingInterfaceOfIMBAgent.GetBodyRotationConstraintDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetBodyRotationConstraintDelegate));
			break;
		case 49:
			ScriptingInterfaceOfIMBAgent.call_GetChestGlobalPositionDelegate = (ScriptingInterfaceOfIMBAgent.GetChestGlobalPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetChestGlobalPositionDelegate));
			break;
		case 50:
			ScriptingInterfaceOfIMBAgent.call_GetCollisionCapsuleDelegate = (ScriptingInterfaceOfIMBAgent.GetCollisionCapsuleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCollisionCapsuleDelegate));
			break;
		case 51:
			ScriptingInterfaceOfIMBAgent.call_GetControllerDelegate = (ScriptingInterfaceOfIMBAgent.GetControllerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetControllerDelegate));
			break;
		case 52:
			ScriptingInterfaceOfIMBAgent.call_GetCrouchModeDelegate = (ScriptingInterfaceOfIMBAgent.GetCrouchModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCrouchModeDelegate));
			break;
		case 53:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentActionDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentActionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentActionDelegate));
			break;
		case 54:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentActionDirectionDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentActionDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentActionDirectionDelegate));
			break;
		case 55:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentActionPriorityDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentActionPriorityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentActionPriorityDelegate));
			break;
		case 56:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentActionProgressDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentActionProgressDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentActionProgressDelegate));
			break;
		case 57:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentActionStageDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentActionStageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentActionStageDelegate));
			break;
		case 58:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentActionTypeDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentActionTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentActionTypeDelegate));
			break;
		case 59:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentAimingErrorDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentAimingErrorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentAimingErrorDelegate));
			break;
		case 60:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentAimingTurbulanceDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentAimingTurbulanceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentAimingTurbulanceDelegate));
			break;
		case 61:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentAnimationFlagsDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentAnimationFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentAnimationFlagsDelegate));
			break;
		case 62:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentGuardModeDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentGuardModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentGuardModeDelegate));
			break;
		case 63:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentNavigationFaceIdDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentNavigationFaceIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentNavigationFaceIdDelegate));
			break;
		case 64:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentSpeedLimitDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentSpeedLimitDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentSpeedLimitDelegate));
			break;
		case 65:
			ScriptingInterfaceOfIMBAgent.call_GetCurrentVelocityDelegate = (ScriptingInterfaceOfIMBAgent.GetCurrentVelocityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurrentVelocityDelegate));
			break;
		case 66:
			ScriptingInterfaceOfIMBAgent.call_GetCurWeaponOffsetDelegate = (ScriptingInterfaceOfIMBAgent.GetCurWeaponOffsetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetCurWeaponOffsetDelegate));
			break;
		case 67:
			ScriptingInterfaceOfIMBAgent.call_GetDefendMovementFlagDelegate = (ScriptingInterfaceOfIMBAgent.GetDefendMovementFlagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetDefendMovementFlagDelegate));
			break;
		case 68:
			ScriptingInterfaceOfIMBAgent.call_GetEventControlFlagsDelegate = (ScriptingInterfaceOfIMBAgent.GetEventControlFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetEventControlFlagsDelegate));
			break;
		case 69:
			ScriptingInterfaceOfIMBAgent.call_GetEyeGlobalHeightDelegate = (ScriptingInterfaceOfIMBAgent.GetEyeGlobalHeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetEyeGlobalHeightDelegate));
			break;
		case 70:
			ScriptingInterfaceOfIMBAgent.call_GetEyeGlobalPositionDelegate = (ScriptingInterfaceOfIMBAgent.GetEyeGlobalPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetEyeGlobalPositionDelegate));
			break;
		case 71:
			ScriptingInterfaceOfIMBAgent.call_GetFiringOrderDelegate = (ScriptingInterfaceOfIMBAgent.GetFiringOrderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetFiringOrderDelegate));
			break;
		case 72:
			ScriptingInterfaceOfIMBAgent.call_GetHeadCameraModeDelegate = (ScriptingInterfaceOfIMBAgent.GetHeadCameraModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetHeadCameraModeDelegate));
			break;
		case 73:
			ScriptingInterfaceOfIMBAgent.call_GetImmediateEnemyDelegate = (ScriptingInterfaceOfIMBAgent.GetImmediateEnemyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetImmediateEnemyDelegate));
			break;
		case 74:
			ScriptingInterfaceOfIMBAgent.call_GetIsDoingPassiveAttackDelegate = (ScriptingInterfaceOfIMBAgent.GetIsDoingPassiveAttackDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetIsDoingPassiveAttackDelegate));
			break;
		case 75:
			ScriptingInterfaceOfIMBAgent.call_GetIsLeftStanceDelegate = (ScriptingInterfaceOfIMBAgent.GetIsLeftStanceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetIsLeftStanceDelegate));
			break;
		case 76:
			ScriptingInterfaceOfIMBAgent.call_GetIsLookDirectionLockedDelegate = (ScriptingInterfaceOfIMBAgent.GetIsLookDirectionLockedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetIsLookDirectionLockedDelegate));
			break;
		case 77:
			ScriptingInterfaceOfIMBAgent.call_GetIsPassiveUsageConditionsAreMetDelegate = (ScriptingInterfaceOfIMBAgent.GetIsPassiveUsageConditionsAreMetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetIsPassiveUsageConditionsAreMetDelegate));
			break;
		case 78:
			ScriptingInterfaceOfIMBAgent.call_GetLastTargetVisibilityStateDelegate = (ScriptingInterfaceOfIMBAgent.GetLastTargetVisibilityStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetLastTargetVisibilityStateDelegate));
			break;
		case 79:
			ScriptingInterfaceOfIMBAgent.call_GetLookAgentDelegate = (ScriptingInterfaceOfIMBAgent.GetLookAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetLookAgentDelegate));
			break;
		case 80:
			ScriptingInterfaceOfIMBAgent.call_GetLookDirectionDelegate = (ScriptingInterfaceOfIMBAgent.GetLookDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetLookDirectionDelegate));
			break;
		case 81:
			ScriptingInterfaceOfIMBAgent.call_GetLookDirectionAsAngleDelegate = (ScriptingInterfaceOfIMBAgent.GetLookDirectionAsAngleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetLookDirectionAsAngleDelegate));
			break;
		case 82:
			ScriptingInterfaceOfIMBAgent.call_GetLookDownLimitDelegate = (ScriptingInterfaceOfIMBAgent.GetLookDownLimitDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetLookDownLimitDelegate));
			break;
		case 83:
			ScriptingInterfaceOfIMBAgent.call_GetMaximumForwardUnlimitedSpeedDelegate = (ScriptingInterfaceOfIMBAgent.GetMaximumForwardUnlimitedSpeedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMaximumForwardUnlimitedSpeedDelegate));
			break;
		case 84:
			ScriptingInterfaceOfIMBAgent.call_GetMaximumNumberOfAgentsDelegate = (ScriptingInterfaceOfIMBAgent.GetMaximumNumberOfAgentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMaximumNumberOfAgentsDelegate));
			break;
		case 85:
			ScriptingInterfaceOfIMBAgent.call_GetMaximumSpeedLimitDelegate = (ScriptingInterfaceOfIMBAgent.GetMaximumSpeedLimitDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMaximumSpeedLimitDelegate));
			break;
		case 86:
			ScriptingInterfaceOfIMBAgent.call_GetMissileRangeDelegate = (ScriptingInterfaceOfIMBAgent.GetMissileRangeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMissileRangeDelegate));
			break;
		case 87:
			ScriptingInterfaceOfIMBAgent.call_GetMissileRangeWithHeightDifferenceDelegate = (ScriptingInterfaceOfIMBAgent.GetMissileRangeWithHeightDifferenceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMissileRangeWithHeightDifferenceDelegate));
			break;
		case 88:
			ScriptingInterfaceOfIMBAgent.call_GetMonsterUsageIndexDelegate = (ScriptingInterfaceOfIMBAgent.GetMonsterUsageIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMonsterUsageIndexDelegate));
			break;
		case 89:
			ScriptingInterfaceOfIMBAgent.call_GetMountAgentDelegate = (ScriptingInterfaceOfIMBAgent.GetMountAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMountAgentDelegate));
			break;
		case 90:
			ScriptingInterfaceOfIMBAgent.call_GetMovementDirectionDelegate = (ScriptingInterfaceOfIMBAgent.GetMovementDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMovementDirectionDelegate));
			break;
		case 91:
			ScriptingInterfaceOfIMBAgent.call_GetMovementDirectionAsAngleDelegate = (ScriptingInterfaceOfIMBAgent.GetMovementDirectionAsAngleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMovementDirectionAsAngleDelegate));
			break;
		case 92:
			ScriptingInterfaceOfIMBAgent.call_GetMovementFlagsDelegate = (ScriptingInterfaceOfIMBAgent.GetMovementFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMovementFlagsDelegate));
			break;
		case 93:
			ScriptingInterfaceOfIMBAgent.call_GetMovementInputVectorDelegate = (ScriptingInterfaceOfIMBAgent.GetMovementInputVectorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMovementInputVectorDelegate));
			break;
		case 94:
			ScriptingInterfaceOfIMBAgent.call_GetMovementLockedStateDelegate = (ScriptingInterfaceOfIMBAgent.GetMovementLockedStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMovementLockedStateDelegate));
			break;
		case 95:
			ScriptingInterfaceOfIMBAgent.call_GetMovementVelocityDelegate = (ScriptingInterfaceOfIMBAgent.GetMovementVelocityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetMovementVelocityDelegate));
			break;
		case 96:
			ScriptingInterfaceOfIMBAgent.call_GetNativeActionIndexDelegate = (ScriptingInterfaceOfIMBAgent.GetNativeActionIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetNativeActionIndexDelegate));
			break;
		case 97:
			ScriptingInterfaceOfIMBAgent.call_GetPathDistanceToPointDelegate = (ScriptingInterfaceOfIMBAgent.GetPathDistanceToPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetPathDistanceToPointDelegate));
			break;
		case 98:
			ScriptingInterfaceOfIMBAgent.call_GetPositionDelegate = (ScriptingInterfaceOfIMBAgent.GetPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetPositionDelegate));
			break;
		case 99:
			ScriptingInterfaceOfIMBAgent.call_GetRenderCheckEnabledDelegate = (ScriptingInterfaceOfIMBAgent.GetRenderCheckEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetRenderCheckEnabledDelegate));
			break;
		case 100:
			ScriptingInterfaceOfIMBAgent.call_GetRetreatPosDelegate = (ScriptingInterfaceOfIMBAgent.GetRetreatPosDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetRetreatPosDelegate));
			break;
		case 101:
			ScriptingInterfaceOfIMBAgent.call_GetRiderAgentDelegate = (ScriptingInterfaceOfIMBAgent.GetRiderAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetRiderAgentDelegate));
			break;
		case 102:
			ScriptingInterfaceOfIMBAgent.call_GetRidingOrderDelegate = (ScriptingInterfaceOfIMBAgent.GetRidingOrderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetRidingOrderDelegate));
			break;
		case 103:
			ScriptingInterfaceOfIMBAgent.call_GetRotationFrameDelegate = (ScriptingInterfaceOfIMBAgent.GetRotationFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetRotationFrameDelegate));
			break;
		case 104:
			ScriptingInterfaceOfIMBAgent.call_GetRunningSimulationDataUntilMaximumSpeedReachedDelegate = (ScriptingInterfaceOfIMBAgent.GetRunningSimulationDataUntilMaximumSpeedReachedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetRunningSimulationDataUntilMaximumSpeedReachedDelegate));
			break;
		case 105:
			ScriptingInterfaceOfIMBAgent.call_GetScriptedCombatFlagsDelegate = (ScriptingInterfaceOfIMBAgent.GetScriptedCombatFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetScriptedCombatFlagsDelegate));
			break;
		case 106:
			ScriptingInterfaceOfIMBAgent.call_GetScriptedFlagsDelegate = (ScriptingInterfaceOfIMBAgent.GetScriptedFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetScriptedFlagsDelegate));
			break;
		case 107:
			ScriptingInterfaceOfIMBAgent.call_GetSelectedMountIndexDelegate = (ScriptingInterfaceOfIMBAgent.GetSelectedMountIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetSelectedMountIndexDelegate));
			break;
		case 108:
			ScriptingInterfaceOfIMBAgent.call_GetStateFlagsDelegate = (ScriptingInterfaceOfIMBAgent.GetStateFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetStateFlagsDelegate));
			break;
		case 109:
			ScriptingInterfaceOfIMBAgent.call_GetSteppedEntityIdDelegate = (ScriptingInterfaceOfIMBAgent.GetSteppedEntityIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetSteppedEntityIdDelegate));
			break;
		case 110:
			ScriptingInterfaceOfIMBAgent.call_GetTargetAgentDelegate = (ScriptingInterfaceOfIMBAgent.GetTargetAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetTargetAgentDelegate));
			break;
		case 111:
			ScriptingInterfaceOfIMBAgent.call_GetTargetDirectionDelegate = (ScriptingInterfaceOfIMBAgent.GetTargetDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetTargetDirectionDelegate));
			break;
		case 112:
			ScriptingInterfaceOfIMBAgent.call_GetTargetFormationIndexDelegate = (ScriptingInterfaceOfIMBAgent.GetTargetFormationIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetTargetFormationIndexDelegate));
			break;
		case 113:
			ScriptingInterfaceOfIMBAgent.call_GetTargetPositionDelegate = (ScriptingInterfaceOfIMBAgent.GetTargetPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetTargetPositionDelegate));
			break;
		case 114:
			ScriptingInterfaceOfIMBAgent.call_GetTeamDelegate = (ScriptingInterfaceOfIMBAgent.GetTeamDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetTeamDelegate));
			break;
		case 115:
			ScriptingInterfaceOfIMBAgent.call_GetTurnSpeedDelegate = (ScriptingInterfaceOfIMBAgent.GetTurnSpeedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetTurnSpeedDelegate));
			break;
		case 116:
			ScriptingInterfaceOfIMBAgent.call_GetVisualPositionDelegate = (ScriptingInterfaceOfIMBAgent.GetVisualPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetVisualPositionDelegate));
			break;
		case 117:
			ScriptingInterfaceOfIMBAgent.call_GetWalkModeDelegate = (ScriptingInterfaceOfIMBAgent.GetWalkModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetWalkModeDelegate));
			break;
		case 118:
			ScriptingInterfaceOfIMBAgent.call_GetWalkSpeedLimitOfMountableDelegate = (ScriptingInterfaceOfIMBAgent.GetWalkSpeedLimitOfMountableDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetWalkSpeedLimitOfMountableDelegate));
			break;
		case 119:
			ScriptingInterfaceOfIMBAgent.call_GetWeaponEntityFromEquipmentSlotDelegate = (ScriptingInterfaceOfIMBAgent.GetWeaponEntityFromEquipmentSlotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetWeaponEntityFromEquipmentSlotDelegate));
			break;
		case 120:
			ScriptingInterfaceOfIMBAgent.call_GetWieldedItemIndexDelegate = (ScriptingInterfaceOfIMBAgent.GetWieldedItemIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetWieldedItemIndexDelegate));
			break;
		case 121:
			ScriptingInterfaceOfIMBAgent.call_GetWieldedWeaponInfoDelegate = (ScriptingInterfaceOfIMBAgent.GetWieldedWeaponInfoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetWieldedWeaponInfoDelegate));
			break;
		case 122:
			ScriptingInterfaceOfIMBAgent.call_GetWorldPositionDelegate = (ScriptingInterfaceOfIMBAgent.GetWorldPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.GetWorldPositionDelegate));
			break;
		case 123:
			ScriptingInterfaceOfIMBAgent.call_HandleBlowAuxDelegate = (ScriptingInterfaceOfIMBAgent.HandleBlowAuxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.HandleBlowAuxDelegate));
			break;
		case 124:
			ScriptingInterfaceOfIMBAgent.call_HasPathThroughNavigationFaceIdFromDirectionDelegate = (ScriptingInterfaceOfIMBAgent.HasPathThroughNavigationFaceIdFromDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.HasPathThroughNavigationFaceIdFromDirectionDelegate));
			break;
		case 125:
			ScriptingInterfaceOfIMBAgent.call_HasPathThroughNavigationFacesIDFromDirectionDelegate = (ScriptingInterfaceOfIMBAgent.HasPathThroughNavigationFacesIDFromDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.HasPathThroughNavigationFacesIDFromDirectionDelegate));
			break;
		case 126:
			ScriptingInterfaceOfIMBAgent.call_InitializeAgentRecordDelegate = (ScriptingInterfaceOfIMBAgent.InitializeAgentRecordDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.InitializeAgentRecordDelegate));
			break;
		case 127:
			ScriptingInterfaceOfIMBAgent.call_InvalidateAIWeaponSelectionsDelegate = (ScriptingInterfaceOfIMBAgent.InvalidateAIWeaponSelectionsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.InvalidateAIWeaponSelectionsDelegate));
			break;
		case 128:
			ScriptingInterfaceOfIMBAgent.call_InvalidateTargetAgentDelegate = (ScriptingInterfaceOfIMBAgent.InvalidateTargetAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.InvalidateTargetAgentDelegate));
			break;
		case 129:
			ScriptingInterfaceOfIMBAgent.call_IsEnemyDelegate = (ScriptingInterfaceOfIMBAgent.IsEnemyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.IsEnemyDelegate));
			break;
		case 130:
			ScriptingInterfaceOfIMBAgent.call_IsFadingOutDelegate = (ScriptingInterfaceOfIMBAgent.IsFadingOutDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.IsFadingOutDelegate));
			break;
		case 131:
			ScriptingInterfaceOfIMBAgent.call_IsFriendDelegate = (ScriptingInterfaceOfIMBAgent.IsFriendDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.IsFriendDelegate));
			break;
		case 132:
			ScriptingInterfaceOfIMBAgent.call_IsLookRotationInSlowMotionDelegate = (ScriptingInterfaceOfIMBAgent.IsLookRotationInSlowMotionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.IsLookRotationInSlowMotionDelegate));
			break;
		case 133:
			ScriptingInterfaceOfIMBAgent.call_IsOnLandDelegate = (ScriptingInterfaceOfIMBAgent.IsOnLandDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.IsOnLandDelegate));
			break;
		case 134:
			ScriptingInterfaceOfIMBAgent.call_IsRetreatingDelegate = (ScriptingInterfaceOfIMBAgent.IsRetreatingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.IsRetreatingDelegate));
			break;
		case 135:
			ScriptingInterfaceOfIMBAgent.call_IsRunningAwayDelegate = (ScriptingInterfaceOfIMBAgent.IsRunningAwayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.IsRunningAwayDelegate));
			break;
		case 136:
			ScriptingInterfaceOfIMBAgent.call_IsSlidingDelegate = (ScriptingInterfaceOfIMBAgent.IsSlidingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.IsSlidingDelegate));
			break;
		case 137:
			ScriptingInterfaceOfIMBAgent.call_KickClearDelegate = (ScriptingInterfaceOfIMBAgent.KickClearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.KickClearDelegate));
			break;
		case 138:
			ScriptingInterfaceOfIMBAgent.call_LockAgentReplicationTableDataWithCurrentReliableSequenceNoDelegate = (ScriptingInterfaceOfIMBAgent.LockAgentReplicationTableDataWithCurrentReliableSequenceNoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.LockAgentReplicationTableDataWithCurrentReliableSequenceNoDelegate));
			break;
		case 139:
			ScriptingInterfaceOfIMBAgent.call_MakeDeadDelegate = (ScriptingInterfaceOfIMBAgent.MakeDeadDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.MakeDeadDelegate));
			break;
		case 140:
			ScriptingInterfaceOfIMBAgent.call_MakeVoiceDelegate = (ScriptingInterfaceOfIMBAgent.MakeVoiceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.MakeVoiceDelegate));
			break;
		case 141:
			ScriptingInterfaceOfIMBAgent.call_PlayerAttackDirectionDelegate = (ScriptingInterfaceOfIMBAgent.PlayerAttackDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.PlayerAttackDirectionDelegate));
			break;
		case 142:
			ScriptingInterfaceOfIMBAgent.call_PreloadForRenderingDelegate = (ScriptingInterfaceOfIMBAgent.PreloadForRenderingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.PreloadForRenderingDelegate));
			break;
		case 143:
			ScriptingInterfaceOfIMBAgent.call_PrepareWeaponForDropInEquipmentSlotDelegate = (ScriptingInterfaceOfIMBAgent.PrepareWeaponForDropInEquipmentSlotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.PrepareWeaponForDropInEquipmentSlotDelegate));
			break;
		case 144:
			ScriptingInterfaceOfIMBAgent.call_RemoveMeshFromBoneDelegate = (ScriptingInterfaceOfIMBAgent.RemoveMeshFromBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.RemoveMeshFromBoneDelegate));
			break;
		case 145:
			ScriptingInterfaceOfIMBAgent.call_ResetEnemyCachesDelegate = (ScriptingInterfaceOfIMBAgent.ResetEnemyCachesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.ResetEnemyCachesDelegate));
			break;
		case 146:
			ScriptingInterfaceOfIMBAgent.call_ResetGuardDelegate = (ScriptingInterfaceOfIMBAgent.ResetGuardDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.ResetGuardDelegate));
			break;
		case 147:
			ScriptingInterfaceOfIMBAgent.call_SetActionChannelDelegate = (ScriptingInterfaceOfIMBAgent.SetActionChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetActionChannelDelegate));
			break;
		case 148:
			ScriptingInterfaceOfIMBAgent.call_SetActionSetDelegate = (ScriptingInterfaceOfIMBAgent.SetActionSetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetActionSetDelegate));
			break;
		case 149:
			ScriptingInterfaceOfIMBAgent.call_SetAgentExcludeStateForFaceGroupIdDelegate = (ScriptingInterfaceOfIMBAgent.SetAgentExcludeStateForFaceGroupIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAgentExcludeStateForFaceGroupIdDelegate));
			break;
		case 150:
			ScriptingInterfaceOfIMBAgent.call_SetAgentFacialAnimationDelegate = (ScriptingInterfaceOfIMBAgent.SetAgentFacialAnimationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAgentFacialAnimationDelegate));
			break;
		case 151:
			ScriptingInterfaceOfIMBAgent.call_SetAgentFlagsDelegate = (ScriptingInterfaceOfIMBAgent.SetAgentFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAgentFlagsDelegate));
			break;
		case 152:
			ScriptingInterfaceOfIMBAgent.call_SetAgentScaleDelegate = (ScriptingInterfaceOfIMBAgent.SetAgentScaleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAgentScaleDelegate));
			break;
		case 153:
			ScriptingInterfaceOfIMBAgent.call_SetAIBehaviorParamsDelegate = (ScriptingInterfaceOfIMBAgent.SetAIBehaviorParamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAIBehaviorParamsDelegate));
			break;
		case 154:
			ScriptingInterfaceOfIMBAgent.call_SetAIStateFlagsDelegate = (ScriptingInterfaceOfIMBAgent.SetAIStateFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAIStateFlagsDelegate));
			break;
		case 155:
			ScriptingInterfaceOfIMBAgent.call_SetAllAIBehaviorParamsDelegate = (ScriptingInterfaceOfIMBAgent.SetAllAIBehaviorParamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAllAIBehaviorParamsDelegate));
			break;
		case 156:
			ScriptingInterfaceOfIMBAgent.call_SetAttackStateDelegate = (ScriptingInterfaceOfIMBAgent.SetAttackStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAttackStateDelegate));
			break;
		case 157:
			ScriptingInterfaceOfIMBAgent.call_SetAutomaticTargetSelectionDelegate = (ScriptingInterfaceOfIMBAgent.SetAutomaticTargetSelectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAutomaticTargetSelectionDelegate));
			break;
		case 158:
			ScriptingInterfaceOfIMBAgent.call_SetAveragePingInMillisecondsDelegate = (ScriptingInterfaceOfIMBAgent.SetAveragePingInMillisecondsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetAveragePingInMillisecondsDelegate));
			break;
		case 159:
			ScriptingInterfaceOfIMBAgent.call_SetBodyArmorMaterialTypeDelegate = (ScriptingInterfaceOfIMBAgent.SetBodyArmorMaterialTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetBodyArmorMaterialTypeDelegate));
			break;
		case 160:
			ScriptingInterfaceOfIMBAgent.call_SetColumnwiseFollowAgentDelegate = (ScriptingInterfaceOfIMBAgent.SetColumnwiseFollowAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetColumnwiseFollowAgentDelegate));
			break;
		case 161:
			ScriptingInterfaceOfIMBAgent.call_SetControllerDelegate = (ScriptingInterfaceOfIMBAgent.SetControllerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetControllerDelegate));
			break;
		case 162:
			ScriptingInterfaceOfIMBAgent.call_SetCourageDelegate = (ScriptingInterfaceOfIMBAgent.SetCourageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetCourageDelegate));
			break;
		case 163:
			ScriptingInterfaceOfIMBAgent.call_SetCurrentActionProgressDelegate = (ScriptingInterfaceOfIMBAgent.SetCurrentActionProgressDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetCurrentActionProgressDelegate));
			break;
		case 164:
			ScriptingInterfaceOfIMBAgent.call_SetCurrentActionSpeedDelegate = (ScriptingInterfaceOfIMBAgent.SetCurrentActionSpeedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetCurrentActionSpeedDelegate));
			break;
		case 165:
			ScriptingInterfaceOfIMBAgent.call_SetDirectionChangeTendencyDelegate = (ScriptingInterfaceOfIMBAgent.SetDirectionChangeTendencyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetDirectionChangeTendencyDelegate));
			break;
		case 166:
			ScriptingInterfaceOfIMBAgent.call_SetEventControlFlagsDelegate = (ScriptingInterfaceOfIMBAgent.SetEventControlFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetEventControlFlagsDelegate));
			break;
		case 167:
			ScriptingInterfaceOfIMBAgent.call_SetFiringOrderDelegate = (ScriptingInterfaceOfIMBAgent.SetFiringOrderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetFiringOrderDelegate));
			break;
		case 168:
			ScriptingInterfaceOfIMBAgent.call_SetFormationFrameDisabledDelegate = (ScriptingInterfaceOfIMBAgent.SetFormationFrameDisabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetFormationFrameDisabledDelegate));
			break;
		case 169:
			ScriptingInterfaceOfIMBAgent.call_SetFormationFrameEnabledDelegate = (ScriptingInterfaceOfIMBAgent.SetFormationFrameEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetFormationFrameEnabledDelegate));
			break;
		case 170:
			ScriptingInterfaceOfIMBAgent.call_SetFormationInfoDelegate = (ScriptingInterfaceOfIMBAgent.SetFormationInfoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetFormationInfoDelegate));
			break;
		case 171:
			ScriptingInterfaceOfIMBAgent.call_SetFormationIntegrityDataDelegate = (ScriptingInterfaceOfIMBAgent.SetFormationIntegrityDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetFormationIntegrityDataDelegate));
			break;
		case 172:
			ScriptingInterfaceOfIMBAgent.call_SetFormationNoDelegate = (ScriptingInterfaceOfIMBAgent.SetFormationNoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetFormationNoDelegate));
			break;
		case 173:
			ScriptingInterfaceOfIMBAgent.call_SetGuardedAgentIndexDelegate = (ScriptingInterfaceOfIMBAgent.SetGuardedAgentIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetGuardedAgentIndexDelegate));
			break;
		case 174:
			ScriptingInterfaceOfIMBAgent.call_SetHandInverseKinematicsFrameDelegate = (ScriptingInterfaceOfIMBAgent.SetHandInverseKinematicsFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetHandInverseKinematicsFrameDelegate));
			break;
		case 175:
			ScriptingInterfaceOfIMBAgent.call_SetHandInverseKinematicsFrameForMissionObjectUsageDelegate = (ScriptingInterfaceOfIMBAgent.SetHandInverseKinematicsFrameForMissionObjectUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetHandInverseKinematicsFrameForMissionObjectUsageDelegate));
			break;
		case 176:
			ScriptingInterfaceOfIMBAgent.call_SetHeadCameraModeDelegate = (ScriptingInterfaceOfIMBAgent.SetHeadCameraModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetHeadCameraModeDelegate));
			break;
		case 177:
			ScriptingInterfaceOfIMBAgent.call_SetInitialFrameDelegate = (ScriptingInterfaceOfIMBAgent.SetInitialFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetInitialFrameDelegate));
			break;
		case 178:
			ScriptingInterfaceOfIMBAgent.call_SetInteractionAgentDelegate = (ScriptingInterfaceOfIMBAgent.SetInteractionAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetInteractionAgentDelegate));
			break;
		case 179:
			ScriptingInterfaceOfIMBAgent.call_SetIsLookDirectionLockedDelegate = (ScriptingInterfaceOfIMBAgent.SetIsLookDirectionLockedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetIsLookDirectionLockedDelegate));
			break;
		case 180:
			ScriptingInterfaceOfIMBAgent.call_SetLookAgentDelegate = (ScriptingInterfaceOfIMBAgent.SetLookAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetLookAgentDelegate));
			break;
		case 181:
			ScriptingInterfaceOfIMBAgent.call_SetLookDirectionDelegate = (ScriptingInterfaceOfIMBAgent.SetLookDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetLookDirectionDelegate));
			break;
		case 182:
			ScriptingInterfaceOfIMBAgent.call_SetLookDirectionAsAngleDelegate = (ScriptingInterfaceOfIMBAgent.SetLookDirectionAsAngleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetLookDirectionAsAngleDelegate));
			break;
		case 183:
			ScriptingInterfaceOfIMBAgent.call_SetLookToPointOfInterestDelegate = (ScriptingInterfaceOfIMBAgent.SetLookToPointOfInterestDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetLookToPointOfInterestDelegate));
			break;
		case 184:
			ScriptingInterfaceOfIMBAgent.call_SetMaximumSpeedLimitDelegate = (ScriptingInterfaceOfIMBAgent.SetMaximumSpeedLimitDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetMaximumSpeedLimitDelegate));
			break;
		case 185:
			ScriptingInterfaceOfIMBAgent.call_SetMonoObjectDelegate = (ScriptingInterfaceOfIMBAgent.SetMonoObjectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetMonoObjectDelegate));
			break;
		case 186:
			ScriptingInterfaceOfIMBAgent.call_SetMountAgentDelegate = (ScriptingInterfaceOfIMBAgent.SetMountAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetMountAgentDelegate));
			break;
		case 187:
			ScriptingInterfaceOfIMBAgent.call_SetMovementDirectionDelegate = (ScriptingInterfaceOfIMBAgent.SetMovementDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetMovementDirectionDelegate));
			break;
		case 188:
			ScriptingInterfaceOfIMBAgent.call_SetMovementFlagsDelegate = (ScriptingInterfaceOfIMBAgent.SetMovementFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetMovementFlagsDelegate));
			break;
		case 189:
			ScriptingInterfaceOfIMBAgent.call_SetMovementInputVectorDelegate = (ScriptingInterfaceOfIMBAgent.SetMovementInputVectorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetMovementInputVectorDelegate));
			break;
		case 190:
			ScriptingInterfaceOfIMBAgent.call_SetNetworkPeerDelegate = (ScriptingInterfaceOfIMBAgent.SetNetworkPeerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetNetworkPeerDelegate));
			break;
		case 191:
			ScriptingInterfaceOfIMBAgent.call_SetPositionDelegate = (ScriptingInterfaceOfIMBAgent.SetPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetPositionDelegate));
			break;
		case 192:
			ScriptingInterfaceOfIMBAgent.call_SetReloadAmmoInSlotDelegate = (ScriptingInterfaceOfIMBAgent.SetReloadAmmoInSlotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetReloadAmmoInSlotDelegate));
			break;
		case 193:
			ScriptingInterfaceOfIMBAgent.call_SetRenderCheckEnabledDelegate = (ScriptingInterfaceOfIMBAgent.SetRenderCheckEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetRenderCheckEnabledDelegate));
			break;
		case 194:
			ScriptingInterfaceOfIMBAgent.call_SetRetreatModeDelegate = (ScriptingInterfaceOfIMBAgent.SetRetreatModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetRetreatModeDelegate));
			break;
		case 195:
			ScriptingInterfaceOfIMBAgent.call_SetRidingOrderDelegate = (ScriptingInterfaceOfIMBAgent.SetRidingOrderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetRidingOrderDelegate));
			break;
		case 196:
			ScriptingInterfaceOfIMBAgent.call_SetScriptedCombatFlagsDelegate = (ScriptingInterfaceOfIMBAgent.SetScriptedCombatFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetScriptedCombatFlagsDelegate));
			break;
		case 197:
			ScriptingInterfaceOfIMBAgent.call_SetScriptedFlagsDelegate = (ScriptingInterfaceOfIMBAgent.SetScriptedFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetScriptedFlagsDelegate));
			break;
		case 198:
			ScriptingInterfaceOfIMBAgent.call_SetScriptedPositionDelegate = (ScriptingInterfaceOfIMBAgent.SetScriptedPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetScriptedPositionDelegate));
			break;
		case 199:
			ScriptingInterfaceOfIMBAgent.call_SetScriptedPositionAndDirectionDelegate = (ScriptingInterfaceOfIMBAgent.SetScriptedPositionAndDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetScriptedPositionAndDirectionDelegate));
			break;
		case 200:
			ScriptingInterfaceOfIMBAgent.call_SetScriptedTargetEntityDelegate = (ScriptingInterfaceOfIMBAgent.SetScriptedTargetEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetScriptedTargetEntityDelegate));
			break;
		case 201:
			ScriptingInterfaceOfIMBAgent.call_SetSelectedMountIndexDelegate = (ScriptingInterfaceOfIMBAgent.SetSelectedMountIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetSelectedMountIndexDelegate));
			break;
		case 202:
			ScriptingInterfaceOfIMBAgent.call_SetShouldCatchUpWithFormationDelegate = (ScriptingInterfaceOfIMBAgent.SetShouldCatchUpWithFormationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetShouldCatchUpWithFormationDelegate));
			break;
		case 203:
			ScriptingInterfaceOfIMBAgent.call_SetStateFlagsDelegate = (ScriptingInterfaceOfIMBAgent.SetStateFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetStateFlagsDelegate));
			break;
		case 204:
			ScriptingInterfaceOfIMBAgent.call_SetTargetAgentDelegate = (ScriptingInterfaceOfIMBAgent.SetTargetAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetTargetAgentDelegate));
			break;
		case 205:
			ScriptingInterfaceOfIMBAgent.call_SetTargetFormationIndexDelegate = (ScriptingInterfaceOfIMBAgent.SetTargetFormationIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetTargetFormationIndexDelegate));
			break;
		case 206:
			ScriptingInterfaceOfIMBAgent.call_SetTargetPositionDelegate = (ScriptingInterfaceOfIMBAgent.SetTargetPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetTargetPositionDelegate));
			break;
		case 207:
			ScriptingInterfaceOfIMBAgent.call_SetTargetPositionAndDirectionDelegate = (ScriptingInterfaceOfIMBAgent.SetTargetPositionAndDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetTargetPositionAndDirectionDelegate));
			break;
		case 208:
			ScriptingInterfaceOfIMBAgent.call_SetTeamDelegate = (ScriptingInterfaceOfIMBAgent.SetTeamDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetTeamDelegate));
			break;
		case 209:
			ScriptingInterfaceOfIMBAgent.call_SetUsageIndexOfWeaponInSlotAsClientDelegate = (ScriptingInterfaceOfIMBAgent.SetUsageIndexOfWeaponInSlotAsClientDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetUsageIndexOfWeaponInSlotAsClientDelegate));
			break;
		case 210:
			ScriptingInterfaceOfIMBAgent.call_SetWeaponAmmoAsClientDelegate = (ScriptingInterfaceOfIMBAgent.SetWeaponAmmoAsClientDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetWeaponAmmoAsClientDelegate));
			break;
		case 211:
			ScriptingInterfaceOfIMBAgent.call_SetWeaponAmountInSlotDelegate = (ScriptingInterfaceOfIMBAgent.SetWeaponAmountInSlotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetWeaponAmountInSlotDelegate));
			break;
		case 212:
			ScriptingInterfaceOfIMBAgent.call_SetWeaponHitPointsInSlotDelegate = (ScriptingInterfaceOfIMBAgent.SetWeaponHitPointsInSlotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetWeaponHitPointsInSlotDelegate));
			break;
		case 213:
			ScriptingInterfaceOfIMBAgent.call_SetWeaponReloadPhaseAsClientDelegate = (ScriptingInterfaceOfIMBAgent.SetWeaponReloadPhaseAsClientDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetWeaponReloadPhaseAsClientDelegate));
			break;
		case 214:
			ScriptingInterfaceOfIMBAgent.call_SetWieldedItemIndexAsClientDelegate = (ScriptingInterfaceOfIMBAgent.SetWieldedItemIndexAsClientDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.SetWieldedItemIndexAsClientDelegate));
			break;
		case 215:
			ScriptingInterfaceOfIMBAgent.call_StartFadingOutDelegate = (ScriptingInterfaceOfIMBAgent.StartFadingOutDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.StartFadingOutDelegate));
			break;
		case 216:
			ScriptingInterfaceOfIMBAgent.call_StartSwitchingWeaponUsageIndexAsClientDelegate = (ScriptingInterfaceOfIMBAgent.StartSwitchingWeaponUsageIndexAsClientDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.StartSwitchingWeaponUsageIndexAsClientDelegate));
			break;
		case 217:
			ScriptingInterfaceOfIMBAgent.call_TickActionChannelsDelegate = (ScriptingInterfaceOfIMBAgent.TickActionChannelsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.TickActionChannelsDelegate));
			break;
		case 218:
			ScriptingInterfaceOfIMBAgent.call_TryGetImmediateEnemyAgentMovementDataDelegate = (ScriptingInterfaceOfIMBAgent.TryGetImmediateEnemyAgentMovementDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.TryGetImmediateEnemyAgentMovementDataDelegate));
			break;
		case 219:
			ScriptingInterfaceOfIMBAgent.call_TryToSheathWeaponInHandDelegate = (ScriptingInterfaceOfIMBAgent.TryToSheathWeaponInHandDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.TryToSheathWeaponInHandDelegate));
			break;
		case 220:
			ScriptingInterfaceOfIMBAgent.call_TryToWieldWeaponInSlotDelegate = (ScriptingInterfaceOfIMBAgent.TryToWieldWeaponInSlotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.TryToWieldWeaponInSlotDelegate));
			break;
		case 221:
			ScriptingInterfaceOfIMBAgent.call_UpdateDrivenPropertiesDelegate = (ScriptingInterfaceOfIMBAgent.UpdateDrivenPropertiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.UpdateDrivenPropertiesDelegate));
			break;
		case 222:
			ScriptingInterfaceOfIMBAgent.call_UpdateWeaponsDelegate = (ScriptingInterfaceOfIMBAgent.UpdateWeaponsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.UpdateWeaponsDelegate));
			break;
		case 223:
			ScriptingInterfaceOfIMBAgent.call_WeaponEquippedDelegate = (ScriptingInterfaceOfIMBAgent.WeaponEquippedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.WeaponEquippedDelegate));
			break;
		case 224:
			ScriptingInterfaceOfIMBAgent.call_WieldNextWeaponDelegate = (ScriptingInterfaceOfIMBAgent.WieldNextWeaponDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgent.WieldNextWeaponDelegate));
			break;
		case 225:
			ScriptingInterfaceOfIMBAgentVisuals.call_AddChildEntityDelegate = (ScriptingInterfaceOfIMBAgentVisuals.AddChildEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.AddChildEntityDelegate));
			break;
		case 226:
			ScriptingInterfaceOfIMBAgentVisuals.call_AddHorseReinsClothMeshDelegate = (ScriptingInterfaceOfIMBAgentVisuals.AddHorseReinsClothMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.AddHorseReinsClothMeshDelegate));
			break;
		case 227:
			ScriptingInterfaceOfIMBAgentVisuals.call_AddMeshDelegate = (ScriptingInterfaceOfIMBAgentVisuals.AddMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.AddMeshDelegate));
			break;
		case 228:
			ScriptingInterfaceOfIMBAgentVisuals.call_AddMultiMeshDelegate = (ScriptingInterfaceOfIMBAgentVisuals.AddMultiMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.AddMultiMeshDelegate));
			break;
		case 229:
			ScriptingInterfaceOfIMBAgentVisuals.call_AddPrefabToAgentVisualBoneByBoneTypeDelegate = (ScriptingInterfaceOfIMBAgentVisuals.AddPrefabToAgentVisualBoneByBoneTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.AddPrefabToAgentVisualBoneByBoneTypeDelegate));
			break;
		case 230:
			ScriptingInterfaceOfIMBAgentVisuals.call_AddPrefabToAgentVisualBoneByRealBoneIndexDelegate = (ScriptingInterfaceOfIMBAgentVisuals.AddPrefabToAgentVisualBoneByRealBoneIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.AddPrefabToAgentVisualBoneByRealBoneIndexDelegate));
			break;
		case 231:
			ScriptingInterfaceOfIMBAgentVisuals.call_AddSkinMeshesToAgentEntityDelegate = (ScriptingInterfaceOfIMBAgentVisuals.AddSkinMeshesToAgentEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.AddSkinMeshesToAgentEntityDelegate));
			break;
		case 232:
			ScriptingInterfaceOfIMBAgentVisuals.call_AddWeaponToAgentEntityDelegate = (ScriptingInterfaceOfIMBAgentVisuals.AddWeaponToAgentEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.AddWeaponToAgentEntityDelegate));
			break;
		case 233:
			ScriptingInterfaceOfIMBAgentVisuals.call_ApplySkeletonScaleDelegate = (ScriptingInterfaceOfIMBAgentVisuals.ApplySkeletonScaleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.ApplySkeletonScaleDelegate));
			break;
		case 234:
			ScriptingInterfaceOfIMBAgentVisuals.call_BatchLastLodMeshesDelegate = (ScriptingInterfaceOfIMBAgentVisuals.BatchLastLodMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.BatchLastLodMeshesDelegate));
			break;
		case 235:
			ScriptingInterfaceOfIMBAgentVisuals.call_CheckResourcesDelegate = (ScriptingInterfaceOfIMBAgentVisuals.CheckResourcesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.CheckResourcesDelegate));
			break;
		case 236:
			ScriptingInterfaceOfIMBAgentVisuals.call_ClearAllWeaponMeshesDelegate = (ScriptingInterfaceOfIMBAgentVisuals.ClearAllWeaponMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.ClearAllWeaponMeshesDelegate));
			break;
		case 237:
			ScriptingInterfaceOfIMBAgentVisuals.call_ClearVisualComponentsDelegate = (ScriptingInterfaceOfIMBAgentVisuals.ClearVisualComponentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.ClearVisualComponentsDelegate));
			break;
		case 238:
			ScriptingInterfaceOfIMBAgentVisuals.call_ClearWeaponMeshesDelegate = (ScriptingInterfaceOfIMBAgentVisuals.ClearWeaponMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.ClearWeaponMeshesDelegate));
			break;
		case 239:
			ScriptingInterfaceOfIMBAgentVisuals.call_CreateAgentRendererSceneControllerDelegate = (ScriptingInterfaceOfIMBAgentVisuals.CreateAgentRendererSceneControllerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.CreateAgentRendererSceneControllerDelegate));
			break;
		case 240:
			ScriptingInterfaceOfIMBAgentVisuals.call_CreateAgentVisualsDelegate = (ScriptingInterfaceOfIMBAgentVisuals.CreateAgentVisualsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.CreateAgentVisualsDelegate));
			break;
		case 241:
			ScriptingInterfaceOfIMBAgentVisuals.call_CreateParticleSystemAttachedToBoneDelegate = (ScriptingInterfaceOfIMBAgentVisuals.CreateParticleSystemAttachedToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.CreateParticleSystemAttachedToBoneDelegate));
			break;
		case 242:
			ScriptingInterfaceOfIMBAgentVisuals.call_DestructAgentRendererSceneControllerDelegate = (ScriptingInterfaceOfIMBAgentVisuals.DestructAgentRendererSceneControllerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.DestructAgentRendererSceneControllerDelegate));
			break;
		case 243:
			ScriptingInterfaceOfIMBAgentVisuals.call_DisableContourDelegate = (ScriptingInterfaceOfIMBAgentVisuals.DisableContourDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.DisableContourDelegate));
			break;
		case 244:
			ScriptingInterfaceOfIMBAgentVisuals.call_FillEntityWithBodyMeshesWithoutAgentVisualsDelegate = (ScriptingInterfaceOfIMBAgentVisuals.FillEntityWithBodyMeshesWithoutAgentVisualsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.FillEntityWithBodyMeshesWithoutAgentVisualsDelegate));
			break;
		case 245:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetAttachedWeaponEntityDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetAttachedWeaponEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetAttachedWeaponEntityDelegate));
			break;
		case 246:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetBoneEntitialFrameDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetBoneEntitialFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetBoneEntitialFrameDelegate));
			break;
		case 247:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetBoneTypeDataDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetBoneTypeDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetBoneTypeDataDelegate));
			break;
		case 248:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetCurrentHelmetScalingFactorDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetCurrentHelmetScalingFactorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetCurrentHelmetScalingFactorDelegate));
			break;
		case 249:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetCurrentRagdollStateDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetCurrentRagdollStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetCurrentRagdollStateDelegate));
			break;
		case 250:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetEntityDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetEntityDelegate));
			break;
		case 251:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetFrameDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetFrameDelegate));
			break;
		case 252:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetGlobalFrameDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetGlobalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetGlobalFrameDelegate));
			break;
		case 253:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetGlobalStableEyePointDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetGlobalStableEyePointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetGlobalStableEyePointDelegate));
			break;
		case 254:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetGlobalStableNeckPointDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetGlobalStableNeckPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetGlobalStableNeckPointDelegate));
			break;
		case 255:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetRealBoneIndexDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetRealBoneIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetRealBoneIndexDelegate));
			break;
		case 256:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetSkeletonDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetSkeletonDelegate));
			break;
		case 257:
			ScriptingInterfaceOfIMBAgentVisuals.call_GetVisibleDelegate = (ScriptingInterfaceOfIMBAgentVisuals.GetVisibleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.GetVisibleDelegate));
			break;
		case 258:
			ScriptingInterfaceOfIMBAgentVisuals.call_IsValidDelegate = (ScriptingInterfaceOfIMBAgentVisuals.IsValidDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.IsValidDelegate));
			break;
		case 259:
			ScriptingInterfaceOfIMBAgentVisuals.call_LazyUpdateAgentRendererDataDelegate = (ScriptingInterfaceOfIMBAgentVisuals.LazyUpdateAgentRendererDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.LazyUpdateAgentRendererDataDelegate));
			break;
		case 260:
			ScriptingInterfaceOfIMBAgentVisuals.call_MakeVoiceDelegate = (ScriptingInterfaceOfIMBAgentVisuals.MakeVoiceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.MakeVoiceDelegate));
			break;
		case 261:
			ScriptingInterfaceOfIMBAgentVisuals.call_RemoveChildEntityDelegate = (ScriptingInterfaceOfIMBAgentVisuals.RemoveChildEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.RemoveChildEntityDelegate));
			break;
		case 262:
			ScriptingInterfaceOfIMBAgentVisuals.call_RemoveMeshDelegate = (ScriptingInterfaceOfIMBAgentVisuals.RemoveMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.RemoveMeshDelegate));
			break;
		case 263:
			ScriptingInterfaceOfIMBAgentVisuals.call_RemoveMultiMeshDelegate = (ScriptingInterfaceOfIMBAgentVisuals.RemoveMultiMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.RemoveMultiMeshDelegate));
			break;
		case 264:
			ScriptingInterfaceOfIMBAgentVisuals.call_ResetDelegate = (ScriptingInterfaceOfIMBAgentVisuals.ResetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.ResetDelegate));
			break;
		case 265:
			ScriptingInterfaceOfIMBAgentVisuals.call_ResetNextFrameDelegate = (ScriptingInterfaceOfIMBAgentVisuals.ResetNextFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.ResetNextFrameDelegate));
			break;
		case 266:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetAgentLocalSpeedDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetAgentLocalSpeedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetAgentLocalSpeedDelegate));
			break;
		case 267:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetAgentLodMakeZeroOrMaxDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetAgentLodMakeZeroOrMaxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetAgentLodMakeZeroOrMaxDelegate));
			break;
		case 268:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetAsContourEntityDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetAsContourEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetAsContourEntityDelegate));
			break;
		case 269:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetClothComponentKeepStateOfAllMeshesDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetClothComponentKeepStateOfAllMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetClothComponentKeepStateOfAllMeshesDelegate));
			break;
		case 270:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetClothWindToWeaponAtIndexDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetClothWindToWeaponAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetClothWindToWeaponAtIndexDelegate));
			break;
		case 271:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetContourStateDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetContourStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetContourStateDelegate));
			break;
		case 272:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetDoTimerBasedForcedSkeletonUpdatesDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetDoTimerBasedForcedSkeletonUpdatesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetDoTimerBasedForcedSkeletonUpdatesDelegate));
			break;
		case 273:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetEnableOcclusionCullingDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetEnableOcclusionCullingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetEnableOcclusionCullingDelegate));
			break;
		case 274:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetEnforcedVisibilityForAllAgentsDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetEnforcedVisibilityForAllAgentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetEnforcedVisibilityForAllAgentsDelegate));
			break;
		case 275:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetEntityDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetEntityDelegate));
			break;
		case 276:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetFaceGenerationParamsDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetFaceGenerationParamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetFaceGenerationParamsDelegate));
			break;
		case 277:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetFrameDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetFrameDelegate));
			break;
		case 278:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetLodAtlasShadingIndexDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetLodAtlasShadingIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetLodAtlasShadingIndexDelegate));
			break;
		case 279:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetLookDirectionDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetLookDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetLookDirectionDelegate));
			break;
		case 280:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetSetupMorphNodeDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetSetupMorphNodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetSetupMorphNodeDelegate));
			break;
		case 281:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetSkeletonDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetSkeletonDelegate));
			break;
		case 282:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetVisibleDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetVisibleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetVisibleDelegate));
			break;
		case 283:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetVoiceDefinitionIndexDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetVoiceDefinitionIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetVoiceDefinitionIndexDelegate));
			break;
		case 284:
			ScriptingInterfaceOfIMBAgentVisuals.call_SetWieldedWeaponIndicesDelegate = (ScriptingInterfaceOfIMBAgentVisuals.SetWieldedWeaponIndicesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.SetWieldedWeaponIndicesDelegate));
			break;
		case 285:
			ScriptingInterfaceOfIMBAgentVisuals.call_StartRhubarbRecordDelegate = (ScriptingInterfaceOfIMBAgentVisuals.StartRhubarbRecordDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.StartRhubarbRecordDelegate));
			break;
		case 286:
			ScriptingInterfaceOfIMBAgentVisuals.call_TickDelegate = (ScriptingInterfaceOfIMBAgentVisuals.TickDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.TickDelegate));
			break;
		case 287:
			ScriptingInterfaceOfIMBAgentVisuals.call_UpdateQuiverMeshesWithoutAgentDelegate = (ScriptingInterfaceOfIMBAgentVisuals.UpdateQuiverMeshesWithoutAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.UpdateQuiverMeshesWithoutAgentDelegate));
			break;
		case 288:
			ScriptingInterfaceOfIMBAgentVisuals.call_UpdateSkeletonScaleDelegate = (ScriptingInterfaceOfIMBAgentVisuals.UpdateSkeletonScaleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.UpdateSkeletonScaleDelegate));
			break;
		case 289:
			ScriptingInterfaceOfIMBAgentVisuals.call_UseScaledWeaponsDelegate = (ScriptingInterfaceOfIMBAgentVisuals.UseScaledWeaponsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.UseScaledWeaponsDelegate));
			break;
		case 290:
			ScriptingInterfaceOfIMBAgentVisuals.call_ValidateAgentVisualsResetedDelegate = (ScriptingInterfaceOfIMBAgentVisuals.ValidateAgentVisualsResetedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAgentVisuals.ValidateAgentVisualsResetedDelegate));
			break;
		case 291:
			ScriptingInterfaceOfIMBAnimation.call_AnimationIndexOfActionCodeDelegate = (ScriptingInterfaceOfIMBAnimation.AnimationIndexOfActionCodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.AnimationIndexOfActionCodeDelegate));
			break;
		case 292:
			ScriptingInterfaceOfIMBAnimation.call_CheckAnimationClipExistsDelegate = (ScriptingInterfaceOfIMBAnimation.CheckAnimationClipExistsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.CheckAnimationClipExistsDelegate));
			break;
		case 293:
			ScriptingInterfaceOfIMBAnimation.call_GetActionAnimationDurationDelegate = (ScriptingInterfaceOfIMBAnimation.GetActionAnimationDurationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetActionAnimationDurationDelegate));
			break;
		case 294:
			ScriptingInterfaceOfIMBAnimation.call_GetActionBlendOutStartProgressDelegate = (ScriptingInterfaceOfIMBAnimation.GetActionBlendOutStartProgressDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetActionBlendOutStartProgressDelegate));
			break;
		case 295:
			ScriptingInterfaceOfIMBAnimation.call_GetActionCodeWithNameDelegate = (ScriptingInterfaceOfIMBAnimation.GetActionCodeWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetActionCodeWithNameDelegate));
			break;
		case 296:
			ScriptingInterfaceOfIMBAnimation.call_GetActionNameWithCodeDelegate = (ScriptingInterfaceOfIMBAnimation.GetActionNameWithCodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetActionNameWithCodeDelegate));
			break;
		case 297:
			ScriptingInterfaceOfIMBAnimation.call_GetActionTypeDelegate = (ScriptingInterfaceOfIMBAnimation.GetActionTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetActionTypeDelegate));
			break;
		case 298:
			ScriptingInterfaceOfIMBAnimation.call_GetAnimationBlendInPeriodDelegate = (ScriptingInterfaceOfIMBAnimation.GetAnimationBlendInPeriodDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetAnimationBlendInPeriodDelegate));
			break;
		case 299:
			ScriptingInterfaceOfIMBAnimation.call_GetAnimationContinueToActionDelegate = (ScriptingInterfaceOfIMBAnimation.GetAnimationContinueToActionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetAnimationContinueToActionDelegate));
			break;
		case 300:
			ScriptingInterfaceOfIMBAnimation.call_GetAnimationDurationDelegate = (ScriptingInterfaceOfIMBAnimation.GetAnimationDurationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetAnimationDurationDelegate));
			break;
		case 301:
			ScriptingInterfaceOfIMBAnimation.call_GetAnimationFlagsDelegate = (ScriptingInterfaceOfIMBAnimation.GetAnimationFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetAnimationFlagsDelegate));
			break;
		case 302:
			ScriptingInterfaceOfIMBAnimation.call_GetAnimationNameDelegate = (ScriptingInterfaceOfIMBAnimation.GetAnimationNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetAnimationNameDelegate));
			break;
		case 303:
			ScriptingInterfaceOfIMBAnimation.call_GetAnimationParameter1Delegate = (ScriptingInterfaceOfIMBAnimation.GetAnimationParameter1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetAnimationParameter1Delegate));
			break;
		case 304:
			ScriptingInterfaceOfIMBAnimation.call_GetAnimationParameter2Delegate = (ScriptingInterfaceOfIMBAnimation.GetAnimationParameter2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetAnimationParameter2Delegate));
			break;
		case 305:
			ScriptingInterfaceOfIMBAnimation.call_GetAnimationParameter3Delegate = (ScriptingInterfaceOfIMBAnimation.GetAnimationParameter3Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetAnimationParameter3Delegate));
			break;
		case 306:
			ScriptingInterfaceOfIMBAnimation.call_GetDisplacementVectorDelegate = (ScriptingInterfaceOfIMBAnimation.GetDisplacementVectorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetDisplacementVectorDelegate));
			break;
		case 307:
			ScriptingInterfaceOfIMBAnimation.call_GetIDWithIndexDelegate = (ScriptingInterfaceOfIMBAnimation.GetIDWithIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetIDWithIndexDelegate));
			break;
		case 308:
			ScriptingInterfaceOfIMBAnimation.call_GetIndexWithIDDelegate = (ScriptingInterfaceOfIMBAnimation.GetIndexWithIDDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetIndexWithIDDelegate));
			break;
		case 309:
			ScriptingInterfaceOfIMBAnimation.call_GetNumActionCodesDelegate = (ScriptingInterfaceOfIMBAnimation.GetNumActionCodesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetNumActionCodesDelegate));
			break;
		case 310:
			ScriptingInterfaceOfIMBAnimation.call_GetNumAnimationsDelegate = (ScriptingInterfaceOfIMBAnimation.GetNumAnimationsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.GetNumAnimationsDelegate));
			break;
		case 311:
			ScriptingInterfaceOfIMBAnimation.call_IsAnyAnimationLoadingFromDiskDelegate = (ScriptingInterfaceOfIMBAnimation.IsAnyAnimationLoadingFromDiskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.IsAnyAnimationLoadingFromDiskDelegate));
			break;
		case 312:
			ScriptingInterfaceOfIMBAnimation.call_PrefetchAnimationClipDelegate = (ScriptingInterfaceOfIMBAnimation.PrefetchAnimationClipDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBAnimation.PrefetchAnimationClipDelegate));
			break;
		case 313:
			ScriptingInterfaceOfIMBBannerlordChecker.call_GetEngineStructMemberOffsetDelegate = (ScriptingInterfaceOfIMBBannerlordChecker.GetEngineStructMemberOffsetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBBannerlordChecker.GetEngineStructMemberOffsetDelegate));
			break;
		case 314:
			ScriptingInterfaceOfIMBBannerlordChecker.call_GetEngineStructSizeDelegate = (ScriptingInterfaceOfIMBBannerlordChecker.GetEngineStructSizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBBannerlordChecker.GetEngineStructSizeDelegate));
			break;
		case 315:
			ScriptingInterfaceOfIMBBannerlordConfig.call_ValidateOptionsDelegate = (ScriptingInterfaceOfIMBBannerlordConfig.ValidateOptionsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBBannerlordConfig.ValidateOptionsDelegate));
			break;
		case 316:
			ScriptingInterfaceOfIMBBannerlordTableauManager.call_GetNumberOfPendingTableauRequestsDelegate = (ScriptingInterfaceOfIMBBannerlordTableauManager.GetNumberOfPendingTableauRequestsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBBannerlordTableauManager.GetNumberOfPendingTableauRequestsDelegate));
			break;
		case 317:
			ScriptingInterfaceOfIMBBannerlordTableauManager.call_InitializeCharacterTableauRenderSystemDelegate = (ScriptingInterfaceOfIMBBannerlordTableauManager.InitializeCharacterTableauRenderSystemDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBBannerlordTableauManager.InitializeCharacterTableauRenderSystemDelegate));
			break;
		case 318:
			ScriptingInterfaceOfIMBBannerlordTableauManager.call_RequestCharacterTableauRenderDelegate = (ScriptingInterfaceOfIMBBannerlordTableauManager.RequestCharacterTableauRenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBBannerlordTableauManager.RequestCharacterTableauRenderDelegate));
			break;
		case 319:
			ScriptingInterfaceOfIMBDebugExtensions.call_OverrideNativeParameterDelegate = (ScriptingInterfaceOfIMBDebugExtensions.OverrideNativeParameterDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBDebugExtensions.OverrideNativeParameterDelegate));
			break;
		case 320:
			ScriptingInterfaceOfIMBDebugExtensions.call_ReloadNativeParametersDelegate = (ScriptingInterfaceOfIMBDebugExtensions.ReloadNativeParametersDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBDebugExtensions.ReloadNativeParametersDelegate));
			break;
		case 321:
			ScriptingInterfaceOfIMBDebugExtensions.call_RenderDebugArcOnTerrainDelegate = (ScriptingInterfaceOfIMBDebugExtensions.RenderDebugArcOnTerrainDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBDebugExtensions.RenderDebugArcOnTerrainDelegate));
			break;
		case 322:
			ScriptingInterfaceOfIMBDebugExtensions.call_RenderDebugCircleOnTerrainDelegate = (ScriptingInterfaceOfIMBDebugExtensions.RenderDebugCircleOnTerrainDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBDebugExtensions.RenderDebugCircleOnTerrainDelegate));
			break;
		case 323:
			ScriptingInterfaceOfIMBDebugExtensions.call_RenderDebugLineOnTerrainDelegate = (ScriptingInterfaceOfIMBDebugExtensions.RenderDebugLineOnTerrainDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBDebugExtensions.RenderDebugLineOnTerrainDelegate));
			break;
		case 324:
			ScriptingInterfaceOfIMBEditor.call_ActivateSceneEditorPresentationDelegate = (ScriptingInterfaceOfIMBEditor.ActivateSceneEditorPresentationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.ActivateSceneEditorPresentationDelegate));
			break;
		case 325:
			ScriptingInterfaceOfIMBEditor.call_AddEditorWarningDelegate = (ScriptingInterfaceOfIMBEditor.AddEditorWarningDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.AddEditorWarningDelegate));
			break;
		case 326:
			ScriptingInterfaceOfIMBEditor.call_AddEntityWarningDelegate = (ScriptingInterfaceOfIMBEditor.AddEntityWarningDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.AddEntityWarningDelegate));
			break;
		case 327:
			ScriptingInterfaceOfIMBEditor.call_BorderHelpersEnabledDelegate = (ScriptingInterfaceOfIMBEditor.BorderHelpersEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.BorderHelpersEnabledDelegate));
			break;
		case 328:
			ScriptingInterfaceOfIMBEditor.call_DeactivateSceneEditorPresentationDelegate = (ScriptingInterfaceOfIMBEditor.DeactivateSceneEditorPresentationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.DeactivateSceneEditorPresentationDelegate));
			break;
		case 329:
			ScriptingInterfaceOfIMBEditor.call_EnterEditMissionModeDelegate = (ScriptingInterfaceOfIMBEditor.EnterEditMissionModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.EnterEditMissionModeDelegate));
			break;
		case 330:
			ScriptingInterfaceOfIMBEditor.call_EnterEditModeDelegate = (ScriptingInterfaceOfIMBEditor.EnterEditModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.EnterEditModeDelegate));
			break;
		case 331:
			ScriptingInterfaceOfIMBEditor.call_ExitEditModeDelegate = (ScriptingInterfaceOfIMBEditor.ExitEditModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.ExitEditModeDelegate));
			break;
		case 332:
			ScriptingInterfaceOfIMBEditor.call_GetAllPrefabsAndChildWithTagDelegate = (ScriptingInterfaceOfIMBEditor.GetAllPrefabsAndChildWithTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.GetAllPrefabsAndChildWithTagDelegate));
			break;
		case 333:
			ScriptingInterfaceOfIMBEditor.call_GetEditorSceneViewDelegate = (ScriptingInterfaceOfIMBEditor.GetEditorSceneViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.GetEditorSceneViewDelegate));
			break;
		case 334:
			ScriptingInterfaceOfIMBEditor.call_HelpersEnabledDelegate = (ScriptingInterfaceOfIMBEditor.HelpersEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.HelpersEnabledDelegate));
			break;
		case 335:
			ScriptingInterfaceOfIMBEditor.call_IsEditModeDelegate = (ScriptingInterfaceOfIMBEditor.IsEditModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.IsEditModeDelegate));
			break;
		case 336:
			ScriptingInterfaceOfIMBEditor.call_IsEditModeEnabledDelegate = (ScriptingInterfaceOfIMBEditor.IsEditModeEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.IsEditModeEnabledDelegate));
			break;
		case 337:
			ScriptingInterfaceOfIMBEditor.call_IsEntitySelectedDelegate = (ScriptingInterfaceOfIMBEditor.IsEntitySelectedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.IsEntitySelectedDelegate));
			break;
		case 338:
			ScriptingInterfaceOfIMBEditor.call_IsReplayManagerRecordingDelegate = (ScriptingInterfaceOfIMBEditor.IsReplayManagerRecordingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.IsReplayManagerRecordingDelegate));
			break;
		case 339:
			ScriptingInterfaceOfIMBEditor.call_IsReplayManagerRenderingDelegate = (ScriptingInterfaceOfIMBEditor.IsReplayManagerRenderingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.IsReplayManagerRenderingDelegate));
			break;
		case 340:
			ScriptingInterfaceOfIMBEditor.call_IsReplayManagerReplayingDelegate = (ScriptingInterfaceOfIMBEditor.IsReplayManagerReplayingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.IsReplayManagerReplayingDelegate));
			break;
		case 341:
			ScriptingInterfaceOfIMBEditor.call_LeaveEditMissionModeDelegate = (ScriptingInterfaceOfIMBEditor.LeaveEditMissionModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.LeaveEditMissionModeDelegate));
			break;
		case 342:
			ScriptingInterfaceOfIMBEditor.call_LeaveEditModeDelegate = (ScriptingInterfaceOfIMBEditor.LeaveEditModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.LeaveEditModeDelegate));
			break;
		case 343:
			ScriptingInterfaceOfIMBEditor.call_RenderEditorMeshDelegate = (ScriptingInterfaceOfIMBEditor.RenderEditorMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.RenderEditorMeshDelegate));
			break;
		case 344:
			ScriptingInterfaceOfIMBEditor.call_SetLevelVisibilityDelegate = (ScriptingInterfaceOfIMBEditor.SetLevelVisibilityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.SetLevelVisibilityDelegate));
			break;
		case 345:
			ScriptingInterfaceOfIMBEditor.call_SetUpgradeLevelVisibilityDelegate = (ScriptingInterfaceOfIMBEditor.SetUpgradeLevelVisibilityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.SetUpgradeLevelVisibilityDelegate));
			break;
		case 346:
			ScriptingInterfaceOfIMBEditor.call_TickEditModeDelegate = (ScriptingInterfaceOfIMBEditor.TickEditModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.TickEditModeDelegate));
			break;
		case 347:
			ScriptingInterfaceOfIMBEditor.call_TickSceneEditorPresentationDelegate = (ScriptingInterfaceOfIMBEditor.TickSceneEditorPresentationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.TickSceneEditorPresentationDelegate));
			break;
		case 348:
			ScriptingInterfaceOfIMBEditor.call_UpdateSceneTreeDelegate = (ScriptingInterfaceOfIMBEditor.UpdateSceneTreeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.UpdateSceneTreeDelegate));
			break;
		case 349:
			ScriptingInterfaceOfIMBEditor.call_ZoomToPositionDelegate = (ScriptingInterfaceOfIMBEditor.ZoomToPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBEditor.ZoomToPositionDelegate));
			break;
		case 350:
			ScriptingInterfaceOfIMBFaceGen.call_EnforceConstraintsDelegate = (ScriptingInterfaceOfIMBFaceGen.EnforceConstraintsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.EnforceConstraintsDelegate));
			break;
		case 351:
			ScriptingInterfaceOfIMBFaceGen.call_GetDeformKeyDataDelegate = (ScriptingInterfaceOfIMBFaceGen.GetDeformKeyDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetDeformKeyDataDelegate));
			break;
		case 352:
			ScriptingInterfaceOfIMBFaceGen.call_GetFaceGenInstancesLengthDelegate = (ScriptingInterfaceOfIMBFaceGen.GetFaceGenInstancesLengthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetFaceGenInstancesLengthDelegate));
			break;
		case 353:
			ScriptingInterfaceOfIMBFaceGen.call_GetHairColorCountDelegate = (ScriptingInterfaceOfIMBFaceGen.GetHairColorCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetHairColorCountDelegate));
			break;
		case 354:
			ScriptingInterfaceOfIMBFaceGen.call_GetHairColorGradientPointsDelegate = (ScriptingInterfaceOfIMBFaceGen.GetHairColorGradientPointsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetHairColorGradientPointsDelegate));
			break;
		case 355:
			ScriptingInterfaceOfIMBFaceGen.call_GetMaturityTypeDelegate = (ScriptingInterfaceOfIMBFaceGen.GetMaturityTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetMaturityTypeDelegate));
			break;
		case 356:
			ScriptingInterfaceOfIMBFaceGen.call_GetNumEditableDeformKeysDelegate = (ScriptingInterfaceOfIMBFaceGen.GetNumEditableDeformKeysDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetNumEditableDeformKeysDelegate));
			break;
		case 357:
			ScriptingInterfaceOfIMBFaceGen.call_GetParamsFromKeyDelegate = (ScriptingInterfaceOfIMBFaceGen.GetParamsFromKeyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetParamsFromKeyDelegate));
			break;
		case 358:
			ScriptingInterfaceOfIMBFaceGen.call_GetParamsMaxDelegate = (ScriptingInterfaceOfIMBFaceGen.GetParamsMaxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetParamsMaxDelegate));
			break;
		case 359:
			ScriptingInterfaceOfIMBFaceGen.call_GetRaceIdsDelegate = (ScriptingInterfaceOfIMBFaceGen.GetRaceIdsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetRaceIdsDelegate));
			break;
		case 360:
			ScriptingInterfaceOfIMBFaceGen.call_GetRandomBodyPropertiesDelegate = (ScriptingInterfaceOfIMBFaceGen.GetRandomBodyPropertiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetRandomBodyPropertiesDelegate));
			break;
		case 361:
			ScriptingInterfaceOfIMBFaceGen.call_GetScaleFromKeyDelegate = (ScriptingInterfaceOfIMBFaceGen.GetScaleFromKeyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetScaleFromKeyDelegate));
			break;
		case 362:
			ScriptingInterfaceOfIMBFaceGen.call_GetSkinColorCountDelegate = (ScriptingInterfaceOfIMBFaceGen.GetSkinColorCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetSkinColorCountDelegate));
			break;
		case 363:
			ScriptingInterfaceOfIMBFaceGen.call_GetSkinColorGradientPointsDelegate = (ScriptingInterfaceOfIMBFaceGen.GetSkinColorGradientPointsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetSkinColorGradientPointsDelegate));
			break;
		case 364:
			ScriptingInterfaceOfIMBFaceGen.call_GetTatooColorCountDelegate = (ScriptingInterfaceOfIMBFaceGen.GetTatooColorCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetTatooColorCountDelegate));
			break;
		case 365:
			ScriptingInterfaceOfIMBFaceGen.call_GetTatooColorGradientPointsDelegate = (ScriptingInterfaceOfIMBFaceGen.GetTatooColorGradientPointsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetTatooColorGradientPointsDelegate));
			break;
		case 366:
			ScriptingInterfaceOfIMBFaceGen.call_GetVoiceRecordsCountDelegate = (ScriptingInterfaceOfIMBFaceGen.GetVoiceRecordsCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetVoiceRecordsCountDelegate));
			break;
		case 367:
			ScriptingInterfaceOfIMBFaceGen.call_GetVoiceTypeUsableForPlayerDataDelegate = (ScriptingInterfaceOfIMBFaceGen.GetVoiceTypeUsableForPlayerDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetVoiceTypeUsableForPlayerDataDelegate));
			break;
		case 368:
			ScriptingInterfaceOfIMBFaceGen.call_GetZeroProbabilitiesDelegate = (ScriptingInterfaceOfIMBFaceGen.GetZeroProbabilitiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.GetZeroProbabilitiesDelegate));
			break;
		case 369:
			ScriptingInterfaceOfIMBFaceGen.call_ProduceNumericKeyWithDefaultValuesDelegate = (ScriptingInterfaceOfIMBFaceGen.ProduceNumericKeyWithDefaultValuesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.ProduceNumericKeyWithDefaultValuesDelegate));
			break;
		case 370:
			ScriptingInterfaceOfIMBFaceGen.call_ProduceNumericKeyWithParamsDelegate = (ScriptingInterfaceOfIMBFaceGen.ProduceNumericKeyWithParamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.ProduceNumericKeyWithParamsDelegate));
			break;
		case 371:
			ScriptingInterfaceOfIMBFaceGen.call_TransformFaceKeysToDefaultFaceDelegate = (ScriptingInterfaceOfIMBFaceGen.TransformFaceKeysToDefaultFaceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBFaceGen.TransformFaceKeysToDefaultFaceDelegate));
			break;
		case 372:
			ScriptingInterfaceOfIMBGame.call_LoadModuleDataDelegate = (ScriptingInterfaceOfIMBGame.LoadModuleDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBGame.LoadModuleDataDelegate));
			break;
		case 373:
			ScriptingInterfaceOfIMBGame.call_StartNewDelegate = (ScriptingInterfaceOfIMBGame.StartNewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBGame.StartNewDelegate));
			break;
		case 374:
			ScriptingInterfaceOfIMBGameEntityExtensions.call_CreateFromWeaponDelegate = (ScriptingInterfaceOfIMBGameEntityExtensions.CreateFromWeaponDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBGameEntityExtensions.CreateFromWeaponDelegate));
			break;
		case 375:
			ScriptingInterfaceOfIMBGameEntityExtensions.call_FadeInDelegate = (ScriptingInterfaceOfIMBGameEntityExtensions.FadeInDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBGameEntityExtensions.FadeInDelegate));
			break;
		case 376:
			ScriptingInterfaceOfIMBGameEntityExtensions.call_FadeOutDelegate = (ScriptingInterfaceOfIMBGameEntityExtensions.FadeOutDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBGameEntityExtensions.FadeOutDelegate));
			break;
		case 377:
			ScriptingInterfaceOfIMBGameEntityExtensions.call_HideIfNotFadingOutDelegate = (ScriptingInterfaceOfIMBGameEntityExtensions.HideIfNotFadingOutDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBGameEntityExtensions.HideIfNotFadingOutDelegate));
			break;
		case 378:
			ScriptingInterfaceOfIMBItem.call_GetHolsterFrameByIndexDelegate = (ScriptingInterfaceOfIMBItem.GetHolsterFrameByIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBItem.GetHolsterFrameByIndexDelegate));
			break;
		case 379:
			ScriptingInterfaceOfIMBItem.call_GetItemHolsterIndexDelegate = (ScriptingInterfaceOfIMBItem.GetItemHolsterIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBItem.GetItemHolsterIndexDelegate));
			break;
		case 380:
			ScriptingInterfaceOfIMBItem.call_GetItemIsPassiveUsageDelegate = (ScriptingInterfaceOfIMBItem.GetItemIsPassiveUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBItem.GetItemIsPassiveUsageDelegate));
			break;
		case 381:
			ScriptingInterfaceOfIMBItem.call_GetItemUsageIndexDelegate = (ScriptingInterfaceOfIMBItem.GetItemUsageIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBItem.GetItemUsageIndexDelegate));
			break;
		case 382:
			ScriptingInterfaceOfIMBItem.call_GetItemUsageReloadActionCodeDelegate = (ScriptingInterfaceOfIMBItem.GetItemUsageReloadActionCodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBItem.GetItemUsageReloadActionCodeDelegate));
			break;
		case 383:
			ScriptingInterfaceOfIMBItem.call_GetItemUsageSetFlagsDelegate = (ScriptingInterfaceOfIMBItem.GetItemUsageSetFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBItem.GetItemUsageSetFlagsDelegate));
			break;
		case 384:
			ScriptingInterfaceOfIMBItem.call_GetItemUsageStrikeTypeDelegate = (ScriptingInterfaceOfIMBItem.GetItemUsageStrikeTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBItem.GetItemUsageStrikeTypeDelegate));
			break;
		case 385:
			ScriptingInterfaceOfIMBItem.call_GetMissileRangeDelegate = (ScriptingInterfaceOfIMBItem.GetMissileRangeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBItem.GetMissileRangeDelegate));
			break;
		case 386:
			ScriptingInterfaceOfIMBMapScene.call_GetAccessiblePointNearPositionDelegate = (ScriptingInterfaceOfIMBMapScene.GetAccessiblePointNearPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.GetAccessiblePointNearPositionDelegate));
			break;
		case 387:
			ScriptingInterfaceOfIMBMapScene.call_GetBattleSceneIndexMapDelegate = (ScriptingInterfaceOfIMBMapScene.GetBattleSceneIndexMapDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.GetBattleSceneIndexMapDelegate));
			break;
		case 388:
			ScriptingInterfaceOfIMBMapScene.call_GetBattleSceneIndexMapResolutionDelegate = (ScriptingInterfaceOfIMBMapScene.GetBattleSceneIndexMapResolutionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.GetBattleSceneIndexMapResolutionDelegate));
			break;
		case 389:
			ScriptingInterfaceOfIMBMapScene.call_GetColorGradeGridDataDelegate = (ScriptingInterfaceOfIMBMapScene.GetColorGradeGridDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.GetColorGradeGridDataDelegate));
			break;
		case 390:
			ScriptingInterfaceOfIMBMapScene.call_GetFaceIndexForMultiplePositionsDelegate = (ScriptingInterfaceOfIMBMapScene.GetFaceIndexForMultiplePositionsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.GetFaceIndexForMultiplePositionsDelegate));
			break;
		case 391:
			ScriptingInterfaceOfIMBMapScene.call_GetMouseVisibleDelegate = (ScriptingInterfaceOfIMBMapScene.GetMouseVisibleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.GetMouseVisibleDelegate));
			break;
		case 392:
			ScriptingInterfaceOfIMBMapScene.call_GetSeasonTimeFactorDelegate = (ScriptingInterfaceOfIMBMapScene.GetSeasonTimeFactorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.GetSeasonTimeFactorDelegate));
			break;
		case 393:
			ScriptingInterfaceOfIMBMapScene.call_LoadAtmosphereDataDelegate = (ScriptingInterfaceOfIMBMapScene.LoadAtmosphereDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.LoadAtmosphereDataDelegate));
			break;
		case 394:
			ScriptingInterfaceOfIMBMapScene.call_RemoveZeroCornerBodiesDelegate = (ScriptingInterfaceOfIMBMapScene.RemoveZeroCornerBodiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.RemoveZeroCornerBodiesDelegate));
			break;
		case 395:
			ScriptingInterfaceOfIMBMapScene.call_SendMouseKeyEventDelegate = (ScriptingInterfaceOfIMBMapScene.SendMouseKeyEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.SendMouseKeyEventDelegate));
			break;
		case 396:
			ScriptingInterfaceOfIMBMapScene.call_SetFrameForAtmosphereDelegate = (ScriptingInterfaceOfIMBMapScene.SetFrameForAtmosphereDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.SetFrameForAtmosphereDelegate));
			break;
		case 397:
			ScriptingInterfaceOfIMBMapScene.call_SetMousePosDelegate = (ScriptingInterfaceOfIMBMapScene.SetMousePosDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.SetMousePosDelegate));
			break;
		case 398:
			ScriptingInterfaceOfIMBMapScene.call_SetMouseVisibleDelegate = (ScriptingInterfaceOfIMBMapScene.SetMouseVisibleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.SetMouseVisibleDelegate));
			break;
		case 399:
			ScriptingInterfaceOfIMBMapScene.call_SetPoliticalColorDelegate = (ScriptingInterfaceOfIMBMapScene.SetPoliticalColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.SetPoliticalColorDelegate));
			break;
		case 400:
			ScriptingInterfaceOfIMBMapScene.call_SetSeasonTimeFactorDelegate = (ScriptingInterfaceOfIMBMapScene.SetSeasonTimeFactorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.SetSeasonTimeFactorDelegate));
			break;
		case 401:
			ScriptingInterfaceOfIMBMapScene.call_SetTerrainDynamicParamsDelegate = (ScriptingInterfaceOfIMBMapScene.SetTerrainDynamicParamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.SetTerrainDynamicParamsDelegate));
			break;
		case 402:
			ScriptingInterfaceOfIMBMapScene.call_TickAmbientSoundsDelegate = (ScriptingInterfaceOfIMBMapScene.TickAmbientSoundsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.TickAmbientSoundsDelegate));
			break;
		case 403:
			ScriptingInterfaceOfIMBMapScene.call_TickStepSoundDelegate = (ScriptingInterfaceOfIMBMapScene.TickStepSoundDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.TickStepSoundDelegate));
			break;
		case 404:
			ScriptingInterfaceOfIMBMapScene.call_TickVisualsDelegate = (ScriptingInterfaceOfIMBMapScene.TickVisualsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.TickVisualsDelegate));
			break;
		case 405:
			ScriptingInterfaceOfIMBMapScene.call_ValidateTerrainSoundIdsDelegate = (ScriptingInterfaceOfIMBMapScene.ValidateTerrainSoundIdsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMapScene.ValidateTerrainSoundIdsDelegate));
			break;
		case 406:
			ScriptingInterfaceOfIMBMessageManager.call_DisplayMessageDelegate = (ScriptingInterfaceOfIMBMessageManager.DisplayMessageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMessageManager.DisplayMessageDelegate));
			break;
		case 407:
			ScriptingInterfaceOfIMBMessageManager.call_DisplayMessageWithColorDelegate = (ScriptingInterfaceOfIMBMessageManager.DisplayMessageWithColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMessageManager.DisplayMessageWithColorDelegate));
			break;
		case 408:
			ScriptingInterfaceOfIMBMessageManager.call_SetMessageManagerDelegate = (ScriptingInterfaceOfIMBMessageManager.SetMessageManagerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMessageManager.SetMessageManagerDelegate));
			break;
		case 409:
			ScriptingInterfaceOfIMBMission.call_AddAiDebugTextDelegate = (ScriptingInterfaceOfIMBMission.AddAiDebugTextDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.AddAiDebugTextDelegate));
			break;
		case 410:
			ScriptingInterfaceOfIMBMission.call_AddBoundaryDelegate = (ScriptingInterfaceOfIMBMission.AddBoundaryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.AddBoundaryDelegate));
			break;
		case 411:
			ScriptingInterfaceOfIMBMission.call_AddMissileDelegate = (ScriptingInterfaceOfIMBMission.AddMissileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.AddMissileDelegate));
			break;
		case 412:
			ScriptingInterfaceOfIMBMission.call_AddMissileSingleUsageDelegate = (ScriptingInterfaceOfIMBMission.AddMissileSingleUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.AddMissileSingleUsageDelegate));
			break;
		case 413:
			ScriptingInterfaceOfIMBMission.call_AddParticleSystemBurstByNameDelegate = (ScriptingInterfaceOfIMBMission.AddParticleSystemBurstByNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.AddParticleSystemBurstByNameDelegate));
			break;
		case 414:
			ScriptingInterfaceOfIMBMission.call_AddSoundAlarmFactorToAgentsDelegate = (ScriptingInterfaceOfIMBMission.AddSoundAlarmFactorToAgentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.AddSoundAlarmFactorToAgentsDelegate));
			break;
		case 415:
			ScriptingInterfaceOfIMBMission.call_AddTeamDelegate = (ScriptingInterfaceOfIMBMission.AddTeamDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.AddTeamDelegate));
			break;
		case 416:
			ScriptingInterfaceOfIMBMission.call_BackupRecordToFileDelegate = (ScriptingInterfaceOfIMBMission.BackupRecordToFileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.BackupRecordToFileDelegate));
			break;
		case 417:
			ScriptingInterfaceOfIMBMission.call_BatchFormationUnitPositionsDelegate = (ScriptingInterfaceOfIMBMission.BatchFormationUnitPositionsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.BatchFormationUnitPositionsDelegate));
			break;
		case 418:
			ScriptingInterfaceOfIMBMission.call_ClearAgentActionsDelegate = (ScriptingInterfaceOfIMBMission.ClearAgentActionsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ClearAgentActionsDelegate));
			break;
		case 419:
			ScriptingInterfaceOfIMBMission.call_ClearCorpsesDelegate = (ScriptingInterfaceOfIMBMission.ClearCorpsesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ClearCorpsesDelegate));
			break;
		case 420:
			ScriptingInterfaceOfIMBMission.call_ClearMissilesDelegate = (ScriptingInterfaceOfIMBMission.ClearMissilesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ClearMissilesDelegate));
			break;
		case 421:
			ScriptingInterfaceOfIMBMission.call_ClearRecordBuffersDelegate = (ScriptingInterfaceOfIMBMission.ClearRecordBuffersDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ClearRecordBuffersDelegate));
			break;
		case 422:
			ScriptingInterfaceOfIMBMission.call_ClearResourcesDelegate = (ScriptingInterfaceOfIMBMission.ClearResourcesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ClearResourcesDelegate));
			break;
		case 423:
			ScriptingInterfaceOfIMBMission.call_ClearSceneDelegate = (ScriptingInterfaceOfIMBMission.ClearSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ClearSceneDelegate));
			break;
		case 424:
			ScriptingInterfaceOfIMBMission.call_ComputeExactMissileRangeAtHeightDifferenceDelegate = (ScriptingInterfaceOfIMBMission.ComputeExactMissileRangeAtHeightDifferenceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ComputeExactMissileRangeAtHeightDifferenceDelegate));
			break;
		case 425:
			ScriptingInterfaceOfIMBMission.call_CreateAgentDelegate = (ScriptingInterfaceOfIMBMission.CreateAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.CreateAgentDelegate));
			break;
		case 426:
			ScriptingInterfaceOfIMBMission.call_CreateMissionDelegate = (ScriptingInterfaceOfIMBMission.CreateMissionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.CreateMissionDelegate));
			break;
		case 427:
			ScriptingInterfaceOfIMBMission.call_EndOfRecordDelegate = (ScriptingInterfaceOfIMBMission.EndOfRecordDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.EndOfRecordDelegate));
			break;
		case 428:
			ScriptingInterfaceOfIMBMission.call_FastForwardMissionDelegate = (ScriptingInterfaceOfIMBMission.FastForwardMissionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.FastForwardMissionDelegate));
			break;
		case 429:
			ScriptingInterfaceOfIMBMission.call_FinalizeMissionDelegate = (ScriptingInterfaceOfIMBMission.FinalizeMissionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.FinalizeMissionDelegate));
			break;
		case 430:
			ScriptingInterfaceOfIMBMission.call_FindAgentWithIndexDelegate = (ScriptingInterfaceOfIMBMission.FindAgentWithIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.FindAgentWithIndexDelegate));
			break;
		case 431:
			ScriptingInterfaceOfIMBMission.call_FindConvexHullDelegate = (ScriptingInterfaceOfIMBMission.FindConvexHullDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.FindConvexHullDelegate));
			break;
		case 432:
			ScriptingInterfaceOfIMBMission.call_GetAgentCountAroundPositionDelegate = (ScriptingInterfaceOfIMBMission.GetAgentCountAroundPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetAgentCountAroundPositionDelegate));
			break;
		case 433:
			ScriptingInterfaceOfIMBMission.call_GetAlternatePositionForNavmeshlessOrOutOfBoundsPositionDelegate = (ScriptingInterfaceOfIMBMission.GetAlternatePositionForNavmeshlessOrOutOfBoundsPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetAlternatePositionForNavmeshlessOrOutOfBoundsPositionDelegate));
			break;
		case 434:
			ScriptingInterfaceOfIMBMission.call_GetAtmosphereNameForReplayDelegate = (ScriptingInterfaceOfIMBMission.GetAtmosphereNameForReplayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetAtmosphereNameForReplayDelegate));
			break;
		case 435:
			ScriptingInterfaceOfIMBMission.call_GetAtmosphereSeasonForReplayDelegate = (ScriptingInterfaceOfIMBMission.GetAtmosphereSeasonForReplayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetAtmosphereSeasonForReplayDelegate));
			break;
		case 436:
			ScriptingInterfaceOfIMBMission.call_GetAverageFpsDelegate = (ScriptingInterfaceOfIMBMission.GetAverageFpsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetAverageFpsDelegate));
			break;
		case 437:
			ScriptingInterfaceOfIMBMission.call_GetAverageMoraleOfAgentsDelegate = (ScriptingInterfaceOfIMBMission.GetAverageMoraleOfAgentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetAverageMoraleOfAgentsDelegate));
			break;
		case 438:
			ScriptingInterfaceOfIMBMission.call_GetBestSlopeAngleHeightPosForDefendingDelegate = (ScriptingInterfaceOfIMBMission.GetBestSlopeAngleHeightPosForDefendingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetBestSlopeAngleHeightPosForDefendingDelegate));
			break;
		case 439:
			ScriptingInterfaceOfIMBMission.call_GetBestSlopeTowardsDirectionDelegate = (ScriptingInterfaceOfIMBMission.GetBestSlopeTowardsDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetBestSlopeTowardsDirectionDelegate));
			break;
		case 440:
			ScriptingInterfaceOfIMBMission.call_GetBiggestAgentCollisionPaddingDelegate = (ScriptingInterfaceOfIMBMission.GetBiggestAgentCollisionPaddingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetBiggestAgentCollisionPaddingDelegate));
			break;
		case 441:
			ScriptingInterfaceOfIMBMission.call_GetBoundaryCountDelegate = (ScriptingInterfaceOfIMBMission.GetBoundaryCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetBoundaryCountDelegate));
			break;
		case 442:
			ScriptingInterfaceOfIMBMission.call_GetBoundaryNameDelegate = (ScriptingInterfaceOfIMBMission.GetBoundaryNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetBoundaryNameDelegate));
			break;
		case 443:
			ScriptingInterfaceOfIMBMission.call_GetBoundaryPointsDelegate = (ScriptingInterfaceOfIMBMission.GetBoundaryPointsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetBoundaryPointsDelegate));
			break;
		case 444:
			ScriptingInterfaceOfIMBMission.call_GetBoundaryRadiusDelegate = (ScriptingInterfaceOfIMBMission.GetBoundaryRadiusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetBoundaryRadiusDelegate));
			break;
		case 445:
			ScriptingInterfaceOfIMBMission.call_GetCameraFrameDelegate = (ScriptingInterfaceOfIMBMission.GetCameraFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetCameraFrameDelegate));
			break;
		case 446:
			ScriptingInterfaceOfIMBMission.call_GetClearSceneTimerElapsedTimeDelegate = (ScriptingInterfaceOfIMBMission.GetClearSceneTimerElapsedTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetClearSceneTimerElapsedTimeDelegate));
			break;
		case 447:
			ScriptingInterfaceOfIMBMission.call_GetClosestAllyDelegate = (ScriptingInterfaceOfIMBMission.GetClosestAllyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetClosestAllyDelegate));
			break;
		case 448:
			ScriptingInterfaceOfIMBMission.call_GetClosestBoundaryPositionDelegate = (ScriptingInterfaceOfIMBMission.GetClosestBoundaryPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetClosestBoundaryPositionDelegate));
			break;
		case 449:
			ScriptingInterfaceOfIMBMission.call_GetClosestEnemyDelegate = (ScriptingInterfaceOfIMBMission.GetClosestEnemyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetClosestEnemyDelegate));
			break;
		case 450:
			ScriptingInterfaceOfIMBMission.call_GetCombatTypeDelegate = (ScriptingInterfaceOfIMBMission.GetCombatTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetCombatTypeDelegate));
			break;
		case 451:
			ScriptingInterfaceOfIMBMission.call_GetDebugAgentDelegate = (ScriptingInterfaceOfIMBMission.GetDebugAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetDebugAgentDelegate));
			break;
		case 452:
			ScriptingInterfaceOfIMBMission.call_GetEnemyAlarmStateIndicatorDelegate = (ScriptingInterfaceOfIMBMission.GetEnemyAlarmStateIndicatorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetEnemyAlarmStateIndicatorDelegate));
			break;
		case 453:
			ScriptingInterfaceOfIMBMission.call_GetGameTypeForReplayDelegate = (ScriptingInterfaceOfIMBMission.GetGameTypeForReplayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetGameTypeForReplayDelegate));
			break;
		case 454:
			ScriptingInterfaceOfIMBMission.call_GetIsLoadingFinishedDelegate = (ScriptingInterfaceOfIMBMission.GetIsLoadingFinishedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetIsLoadingFinishedDelegate));
			break;
		case 455:
			ScriptingInterfaceOfIMBMission.call_GetMissileCollisionPointDelegate = (ScriptingInterfaceOfIMBMission.GetMissileCollisionPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetMissileCollisionPointDelegate));
			break;
		case 456:
			ScriptingInterfaceOfIMBMission.call_GetMissileHasRigidBodyDelegate = (ScriptingInterfaceOfIMBMission.GetMissileHasRigidBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetMissileHasRigidBodyDelegate));
			break;
		case 457:
			ScriptingInterfaceOfIMBMission.call_GetMissileRangeDelegate = (ScriptingInterfaceOfIMBMission.GetMissileRangeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetMissileRangeDelegate));
			break;
		case 458:
			ScriptingInterfaceOfIMBMission.call_GetMissileVerticalAimCorrectionDelegate = (ScriptingInterfaceOfIMBMission.GetMissileVerticalAimCorrectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetMissileVerticalAimCorrectionDelegate));
			break;
		case 459:
			ScriptingInterfaceOfIMBMission.call_GetNavigationPointsDelegate = (ScriptingInterfaceOfIMBMission.GetNavigationPointsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetNavigationPointsDelegate));
			break;
		case 460:
			ScriptingInterfaceOfIMBMission.call_GetNearbyAgentsAuxDelegate = (ScriptingInterfaceOfIMBMission.GetNearbyAgentsAuxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetNearbyAgentsAuxDelegate));
			break;
		case 461:
			ScriptingInterfaceOfIMBMission.call_GetNumberOfTeamsDelegate = (ScriptingInterfaceOfIMBMission.GetNumberOfTeamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetNumberOfTeamsDelegate));
			break;
		case 462:
			ScriptingInterfaceOfIMBMission.call_GetPauseAITickDelegate = (ScriptingInterfaceOfIMBMission.GetPauseAITickDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetPauseAITickDelegate));
			break;
		case 463:
			ScriptingInterfaceOfIMBMission.call_GetPlayerAlarmIndicatorDelegate = (ScriptingInterfaceOfIMBMission.GetPlayerAlarmIndicatorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetPlayerAlarmIndicatorDelegate));
			break;
		case 464:
			ScriptingInterfaceOfIMBMission.call_GetPositionOfMissileDelegate = (ScriptingInterfaceOfIMBMission.GetPositionOfMissileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetPositionOfMissileDelegate));
			break;
		case 465:
			ScriptingInterfaceOfIMBMission.call_GetSceneLevelsForReplayDelegate = (ScriptingInterfaceOfIMBMission.GetSceneLevelsForReplayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetSceneLevelsForReplayDelegate));
			break;
		case 466:
			ScriptingInterfaceOfIMBMission.call_GetSceneNameForReplayDelegate = (ScriptingInterfaceOfIMBMission.GetSceneNameForReplayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetSceneNameForReplayDelegate));
			break;
		case 467:
			ScriptingInterfaceOfIMBMission.call_GetStraightPathToTargetDelegate = (ScriptingInterfaceOfIMBMission.GetStraightPathToTargetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetStraightPathToTargetDelegate));
			break;
		case 468:
			ScriptingInterfaceOfIMBMission.call_GetTimeDelegate = (ScriptingInterfaceOfIMBMission.GetTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetTimeDelegate));
			break;
		case 469:
			ScriptingInterfaceOfIMBMission.call_GetVelocityOfMissileDelegate = (ScriptingInterfaceOfIMBMission.GetVelocityOfMissileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetVelocityOfMissileDelegate));
			break;
		case 470:
			ScriptingInterfaceOfIMBMission.call_GetWaterLevelAtPositionDelegate = (ScriptingInterfaceOfIMBMission.GetWaterLevelAtPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetWaterLevelAtPositionDelegate));
			break;
		case 471:
			ScriptingInterfaceOfIMBMission.call_GetWeightedPointOfEnemiesDelegate = (ScriptingInterfaceOfIMBMission.GetWeightedPointOfEnemiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.GetWeightedPointOfEnemiesDelegate));
			break;
		case 472:
			ScriptingInterfaceOfIMBMission.call_HasAnyAgentsOfTeamAroundDelegate = (ScriptingInterfaceOfIMBMission.HasAnyAgentsOfTeamAroundDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.HasAnyAgentsOfTeamAroundDelegate));
			break;
		case 473:
			ScriptingInterfaceOfIMBMission.call_IdleTickDelegate = (ScriptingInterfaceOfIMBMission.IdleTickDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.IdleTickDelegate));
			break;
		case 474:
			ScriptingInterfaceOfIMBMission.call_InitializeMissionDelegate = (ScriptingInterfaceOfIMBMission.InitializeMissionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.InitializeMissionDelegate));
			break;
		case 475:
			ScriptingInterfaceOfIMBMission.call_IsAgentInProximityMapDelegate = (ScriptingInterfaceOfIMBMission.IsAgentInProximityMapDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.IsAgentInProximityMapDelegate));
			break;
		case 476:
			ScriptingInterfaceOfIMBMission.call_IsFormationUnitPositionAvailableDelegate = (ScriptingInterfaceOfIMBMission.IsFormationUnitPositionAvailableDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.IsFormationUnitPositionAvailableDelegate));
			break;
		case 477:
			ScriptingInterfaceOfIMBMission.call_IsPositionInsideAnyBlockerNavMeshFace2DDelegate = (ScriptingInterfaceOfIMBMission.IsPositionInsideAnyBlockerNavMeshFace2DDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.IsPositionInsideAnyBlockerNavMeshFace2DDelegate));
			break;
		case 478:
			ScriptingInterfaceOfIMBMission.call_IsPositionInsideBoundariesDelegate = (ScriptingInterfaceOfIMBMission.IsPositionInsideBoundariesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.IsPositionInsideBoundariesDelegate));
			break;
		case 479:
			ScriptingInterfaceOfIMBMission.call_MakeSoundDelegate = (ScriptingInterfaceOfIMBMission.MakeSoundDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.MakeSoundDelegate));
			break;
		case 480:
			ScriptingInterfaceOfIMBMission.call_MakeSoundOnlyOnRelatedPeerDelegate = (ScriptingInterfaceOfIMBMission.MakeSoundOnlyOnRelatedPeerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.MakeSoundOnlyOnRelatedPeerDelegate));
			break;
		case 481:
			ScriptingInterfaceOfIMBMission.call_MakeSoundWithParameterDelegate = (ScriptingInterfaceOfIMBMission.MakeSoundWithParameterDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.MakeSoundWithParameterDelegate));
			break;
		case 482:
			ScriptingInterfaceOfIMBMission.call_PauseMissionSceneSoundsDelegate = (ScriptingInterfaceOfIMBMission.PauseMissionSceneSoundsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.PauseMissionSceneSoundsDelegate));
			break;
		case 483:
			ScriptingInterfaceOfIMBMission.call_PrepareMissileWeaponForDropDelegate = (ScriptingInterfaceOfIMBMission.PrepareMissileWeaponForDropDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.PrepareMissileWeaponForDropDelegate));
			break;
		case 484:
			ScriptingInterfaceOfIMBMission.call_ProcessRecordUntilTimeDelegate = (ScriptingInterfaceOfIMBMission.ProcessRecordUntilTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ProcessRecordUntilTimeDelegate));
			break;
		case 485:
			ScriptingInterfaceOfIMBMission.call_ProximityMapBeginSearchDelegate = (ScriptingInterfaceOfIMBMission.ProximityMapBeginSearchDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ProximityMapBeginSearchDelegate));
			break;
		case 486:
			ScriptingInterfaceOfIMBMission.call_ProximityMapFindNextDelegate = (ScriptingInterfaceOfIMBMission.ProximityMapFindNextDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ProximityMapFindNextDelegate));
			break;
		case 487:
			ScriptingInterfaceOfIMBMission.call_ProximityMapMaxSearchRadiusDelegate = (ScriptingInterfaceOfIMBMission.ProximityMapMaxSearchRadiusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ProximityMapMaxSearchRadiusDelegate));
			break;
		case 488:
			ScriptingInterfaceOfIMBMission.call_RayCastForClosestAgentDelegate = (ScriptingInterfaceOfIMBMission.RayCastForClosestAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.RayCastForClosestAgentDelegate));
			break;
		case 489:
			ScriptingInterfaceOfIMBMission.call_RayCastForClosestAgentsLimbsDelegate = (ScriptingInterfaceOfIMBMission.RayCastForClosestAgentsLimbsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.RayCastForClosestAgentsLimbsDelegate));
			break;
		case 490:
			ScriptingInterfaceOfIMBMission.call_RayCastForGivenAgentsLimbsDelegate = (ScriptingInterfaceOfIMBMission.RayCastForGivenAgentsLimbsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.RayCastForGivenAgentsLimbsDelegate));
			break;
		case 491:
			ScriptingInterfaceOfIMBMission.call_RecordCurrentStateDelegate = (ScriptingInterfaceOfIMBMission.RecordCurrentStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.RecordCurrentStateDelegate));
			break;
		case 492:
			ScriptingInterfaceOfIMBMission.call_RemoveBoundaryDelegate = (ScriptingInterfaceOfIMBMission.RemoveBoundaryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.RemoveBoundaryDelegate));
			break;
		case 493:
			ScriptingInterfaceOfIMBMission.call_RemoveMissileDelegate = (ScriptingInterfaceOfIMBMission.RemoveMissileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.RemoveMissileDelegate));
			break;
		case 494:
			ScriptingInterfaceOfIMBMission.call_ResetFirstThirdPersonViewDelegate = (ScriptingInterfaceOfIMBMission.ResetFirstThirdPersonViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ResetFirstThirdPersonViewDelegate));
			break;
		case 495:
			ScriptingInterfaceOfIMBMission.call_ResetTeamsDelegate = (ScriptingInterfaceOfIMBMission.ResetTeamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ResetTeamsDelegate));
			break;
		case 496:
			ScriptingInterfaceOfIMBMission.call_RestartRecordDelegate = (ScriptingInterfaceOfIMBMission.RestartRecordDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.RestartRecordDelegate));
			break;
		case 497:
			ScriptingInterfaceOfIMBMission.call_RestoreRecordFromFileDelegate = (ScriptingInterfaceOfIMBMission.RestoreRecordFromFileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.RestoreRecordFromFileDelegate));
			break;
		case 498:
			ScriptingInterfaceOfIMBMission.call_ResumeMissionSceneSoundsDelegate = (ScriptingInterfaceOfIMBMission.ResumeMissionSceneSoundsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ResumeMissionSceneSoundsDelegate));
			break;
		case 499:
			ScriptingInterfaceOfIMBMission.call_SetBowMissileSpeedModifierDelegate = (ScriptingInterfaceOfIMBMission.SetBowMissileSpeedModifierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetBowMissileSpeedModifierDelegate));
			break;
		case 500:
			ScriptingInterfaceOfIMBMission.call_SetCameraFrameDelegate = (ScriptingInterfaceOfIMBMission.SetCameraFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetCameraFrameDelegate));
			break;
		case 501:
			ScriptingInterfaceOfIMBMission.call_SetCameraIsFirstPersonDelegate = (ScriptingInterfaceOfIMBMission.SetCameraIsFirstPersonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetCameraIsFirstPersonDelegate));
			break;
		case 502:
			ScriptingInterfaceOfIMBMission.call_SetCombatTypeDelegate = (ScriptingInterfaceOfIMBMission.SetCombatTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetCombatTypeDelegate));
			break;
		case 503:
			ScriptingInterfaceOfIMBMission.call_SetCrossbowMissileSpeedModifierDelegate = (ScriptingInterfaceOfIMBMission.SetCrossbowMissileSpeedModifierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetCrossbowMissileSpeedModifierDelegate));
			break;
		case 504:
			ScriptingInterfaceOfIMBMission.call_SetDebugAgentDelegate = (ScriptingInterfaceOfIMBMission.SetDebugAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetDebugAgentDelegate));
			break;
		case 505:
			ScriptingInterfaceOfIMBMission.call_SetLastMovementKeyPressedDelegate = (ScriptingInterfaceOfIMBMission.SetLastMovementKeyPressedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetLastMovementKeyPressedDelegate));
			break;
		case 506:
			ScriptingInterfaceOfIMBMission.call_SetMissileRangeModifierDelegate = (ScriptingInterfaceOfIMBMission.SetMissileRangeModifierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetMissileRangeModifierDelegate));
			break;
		case 507:
			ScriptingInterfaceOfIMBMission.call_SetMissionCorpseFadeOutTimeInSecondsDelegate = (ScriptingInterfaceOfIMBMission.SetMissionCorpseFadeOutTimeInSecondsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetMissionCorpseFadeOutTimeInSecondsDelegate));
			break;
		case 508:
			ScriptingInterfaceOfIMBMission.call_SetNavigationFaceCostWithIdAroundPositionDelegate = (ScriptingInterfaceOfIMBMission.SetNavigationFaceCostWithIdAroundPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetNavigationFaceCostWithIdAroundPositionDelegate));
			break;
		case 509:
			ScriptingInterfaceOfIMBMission.call_SetPauseAITickDelegate = (ScriptingInterfaceOfIMBMission.SetPauseAITickDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetPauseAITickDelegate));
			break;
		case 510:
			ScriptingInterfaceOfIMBMission.call_SetRandomDecideTimeOfAgentsDelegate = (ScriptingInterfaceOfIMBMission.SetRandomDecideTimeOfAgentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetRandomDecideTimeOfAgentsDelegate));
			break;
		case 511:
			ScriptingInterfaceOfIMBMission.call_SetReportStuckAgentsModeDelegate = (ScriptingInterfaceOfIMBMission.SetReportStuckAgentsModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetReportStuckAgentsModeDelegate));
			break;
		case 512:
			ScriptingInterfaceOfIMBMission.call_SetThrowingMissileSpeedModifierDelegate = (ScriptingInterfaceOfIMBMission.SetThrowingMissileSpeedModifierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.SetThrowingMissileSpeedModifierDelegate));
			break;
		case 513:
			ScriptingInterfaceOfIMBMission.call_StartRecordingDelegate = (ScriptingInterfaceOfIMBMission.StartRecordingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.StartRecordingDelegate));
			break;
		case 514:
			ScriptingInterfaceOfIMBMission.call_TickDelegate = (ScriptingInterfaceOfIMBMission.TickDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.TickDelegate));
			break;
		case 515:
			ScriptingInterfaceOfIMBMission.call_tickAgentsAndTeamsAsyncDelegate = (ScriptingInterfaceOfIMBMission.tickAgentsAndTeamsAsyncDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.tickAgentsAndTeamsAsyncDelegate));
			break;
		case 516:
			ScriptingInterfaceOfIMBMission.call_ToggleDisableFallAvoidDelegate = (ScriptingInterfaceOfIMBMission.ToggleDisableFallAvoidDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBMission.ToggleDisableFallAvoidDelegate));
			break;
		case 517:
			ScriptingInterfaceOfIMBNetwork.call_AddNewBotOnServerDelegate = (ScriptingInterfaceOfIMBNetwork.AddNewBotOnServerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.AddNewBotOnServerDelegate));
			break;
		case 518:
			ScriptingInterfaceOfIMBNetwork.call_AddNewPlayerOnServerDelegate = (ScriptingInterfaceOfIMBNetwork.AddNewPlayerOnServerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.AddNewPlayerOnServerDelegate));
			break;
		case 519:
			ScriptingInterfaceOfIMBNetwork.call_AddPeerToDisconnectDelegate = (ScriptingInterfaceOfIMBNetwork.AddPeerToDisconnectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.AddPeerToDisconnectDelegate));
			break;
		case 520:
			ScriptingInterfaceOfIMBNetwork.call_BeginBroadcastModuleEventDelegate = (ScriptingInterfaceOfIMBNetwork.BeginBroadcastModuleEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.BeginBroadcastModuleEventDelegate));
			break;
		case 521:
			ScriptingInterfaceOfIMBNetwork.call_BeginModuleEventAsClientDelegate = (ScriptingInterfaceOfIMBNetwork.BeginModuleEventAsClientDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.BeginModuleEventAsClientDelegate));
			break;
		case 522:
			ScriptingInterfaceOfIMBNetwork.call_CanAddNewPlayersOnServerDelegate = (ScriptingInterfaceOfIMBNetwork.CanAddNewPlayersOnServerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.CanAddNewPlayersOnServerDelegate));
			break;
		case 523:
			ScriptingInterfaceOfIMBNetwork.call_ClearReplicationTableStatisticsDelegate = (ScriptingInterfaceOfIMBNetwork.ClearReplicationTableStatisticsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ClearReplicationTableStatisticsDelegate));
			break;
		case 524:
			ScriptingInterfaceOfIMBNetwork.call_ElapsedTimeSinceLastUdpPacketArrivedDelegate = (ScriptingInterfaceOfIMBNetwork.ElapsedTimeSinceLastUdpPacketArrivedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ElapsedTimeSinceLastUdpPacketArrivedDelegate));
			break;
		case 525:
			ScriptingInterfaceOfIMBNetwork.call_EndBroadcastModuleEventDelegate = (ScriptingInterfaceOfIMBNetwork.EndBroadcastModuleEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.EndBroadcastModuleEventDelegate));
			break;
		case 526:
			ScriptingInterfaceOfIMBNetwork.call_EndModuleEventAsClientDelegate = (ScriptingInterfaceOfIMBNetwork.EndModuleEventAsClientDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.EndModuleEventAsClientDelegate));
			break;
		case 527:
			ScriptingInterfaceOfIMBNetwork.call_GetActiveUdpSessionsIpAddressDelegate = (ScriptingInterfaceOfIMBNetwork.GetActiveUdpSessionsIpAddressDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.GetActiveUdpSessionsIpAddressDelegate));
			break;
		case 528:
			ScriptingInterfaceOfIMBNetwork.call_GetAveragePacketLossRatioDelegate = (ScriptingInterfaceOfIMBNetwork.GetAveragePacketLossRatioDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.GetAveragePacketLossRatioDelegate));
			break;
		case 529:
			ScriptingInterfaceOfIMBNetwork.call_GetDebugUploadsInBitsDelegate = (ScriptingInterfaceOfIMBNetwork.GetDebugUploadsInBitsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.GetDebugUploadsInBitsDelegate));
			break;
		case 530:
			ScriptingInterfaceOfIMBNetwork.call_GetMultiplayerDisabledDelegate = (ScriptingInterfaceOfIMBNetwork.GetMultiplayerDisabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.GetMultiplayerDisabledDelegate));
			break;
		case 531:
			ScriptingInterfaceOfIMBNetwork.call_InitializeClientSideDelegate = (ScriptingInterfaceOfIMBNetwork.InitializeClientSideDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.InitializeClientSideDelegate));
			break;
		case 532:
			ScriptingInterfaceOfIMBNetwork.call_InitializeServerSideDelegate = (ScriptingInterfaceOfIMBNetwork.InitializeServerSideDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.InitializeServerSideDelegate));
			break;
		case 533:
			ScriptingInterfaceOfIMBNetwork.call_IsDedicatedServerDelegate = (ScriptingInterfaceOfIMBNetwork.IsDedicatedServerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.IsDedicatedServerDelegate));
			break;
		case 534:
			ScriptingInterfaceOfIMBNetwork.call_PrepareNewUdpSessionDelegate = (ScriptingInterfaceOfIMBNetwork.PrepareNewUdpSessionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.PrepareNewUdpSessionDelegate));
			break;
		case 535:
			ScriptingInterfaceOfIMBNetwork.call_PrintDebugStatsDelegate = (ScriptingInterfaceOfIMBNetwork.PrintDebugStatsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.PrintDebugStatsDelegate));
			break;
		case 536:
			ScriptingInterfaceOfIMBNetwork.call_PrintReplicationTableStatisticsDelegate = (ScriptingInterfaceOfIMBNetwork.PrintReplicationTableStatisticsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.PrintReplicationTableStatisticsDelegate));
			break;
		case 537:
			ScriptingInterfaceOfIMBNetwork.call_ReadByteArrayFromPacketDelegate = (ScriptingInterfaceOfIMBNetwork.ReadByteArrayFromPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ReadByteArrayFromPacketDelegate));
			break;
		case 538:
			ScriptingInterfaceOfIMBNetwork.call_ReadFloatFromPacketDelegate = (ScriptingInterfaceOfIMBNetwork.ReadFloatFromPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ReadFloatFromPacketDelegate));
			break;
		case 539:
			ScriptingInterfaceOfIMBNetwork.call_ReadIntFromPacketDelegate = (ScriptingInterfaceOfIMBNetwork.ReadIntFromPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ReadIntFromPacketDelegate));
			break;
		case 540:
			ScriptingInterfaceOfIMBNetwork.call_ReadLongFromPacketDelegate = (ScriptingInterfaceOfIMBNetwork.ReadLongFromPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ReadLongFromPacketDelegate));
			break;
		case 541:
			ScriptingInterfaceOfIMBNetwork.call_ReadStringFromPacketDelegate = (ScriptingInterfaceOfIMBNetwork.ReadStringFromPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ReadStringFromPacketDelegate));
			break;
		case 542:
			ScriptingInterfaceOfIMBNetwork.call_ReadUintFromPacketDelegate = (ScriptingInterfaceOfIMBNetwork.ReadUintFromPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ReadUintFromPacketDelegate));
			break;
		case 543:
			ScriptingInterfaceOfIMBNetwork.call_ReadUlongFromPacketDelegate = (ScriptingInterfaceOfIMBNetwork.ReadUlongFromPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ReadUlongFromPacketDelegate));
			break;
		case 544:
			ScriptingInterfaceOfIMBNetwork.call_RemoveBotOnServerDelegate = (ScriptingInterfaceOfIMBNetwork.RemoveBotOnServerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.RemoveBotOnServerDelegate));
			break;
		case 545:
			ScriptingInterfaceOfIMBNetwork.call_ResetDebugUploadsDelegate = (ScriptingInterfaceOfIMBNetwork.ResetDebugUploadsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ResetDebugUploadsDelegate));
			break;
		case 546:
			ScriptingInterfaceOfIMBNetwork.call_ResetDebugVariablesDelegate = (ScriptingInterfaceOfIMBNetwork.ResetDebugVariablesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ResetDebugVariablesDelegate));
			break;
		case 547:
			ScriptingInterfaceOfIMBNetwork.call_ResetMissionDataDelegate = (ScriptingInterfaceOfIMBNetwork.ResetMissionDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ResetMissionDataDelegate));
			break;
		case 548:
			ScriptingInterfaceOfIMBNetwork.call_ServerPingDelegate = (ScriptingInterfaceOfIMBNetwork.ServerPingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.ServerPingDelegate));
			break;
		case 549:
			ScriptingInterfaceOfIMBNetwork.call_SetServerBandwidthLimitInMbpsDelegate = (ScriptingInterfaceOfIMBNetwork.SetServerBandwidthLimitInMbpsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.SetServerBandwidthLimitInMbpsDelegate));
			break;
		case 550:
			ScriptingInterfaceOfIMBNetwork.call_SetServerFrameRateDelegate = (ScriptingInterfaceOfIMBNetwork.SetServerFrameRateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.SetServerFrameRateDelegate));
			break;
		case 551:
			ScriptingInterfaceOfIMBNetwork.call_SetServerTickRateDelegate = (ScriptingInterfaceOfIMBNetwork.SetServerTickRateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.SetServerTickRateDelegate));
			break;
		case 552:
			ScriptingInterfaceOfIMBNetwork.call_TerminateClientSideDelegate = (ScriptingInterfaceOfIMBNetwork.TerminateClientSideDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.TerminateClientSideDelegate));
			break;
		case 553:
			ScriptingInterfaceOfIMBNetwork.call_TerminateServerSideDelegate = (ScriptingInterfaceOfIMBNetwork.TerminateServerSideDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.TerminateServerSideDelegate));
			break;
		case 554:
			ScriptingInterfaceOfIMBNetwork.call_WriteByteArrayToPacketDelegate = (ScriptingInterfaceOfIMBNetwork.WriteByteArrayToPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.WriteByteArrayToPacketDelegate));
			break;
		case 555:
			ScriptingInterfaceOfIMBNetwork.call_WriteFloatToPacketDelegate = (ScriptingInterfaceOfIMBNetwork.WriteFloatToPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.WriteFloatToPacketDelegate));
			break;
		case 556:
			ScriptingInterfaceOfIMBNetwork.call_WriteIntToPacketDelegate = (ScriptingInterfaceOfIMBNetwork.WriteIntToPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.WriteIntToPacketDelegate));
			break;
		case 557:
			ScriptingInterfaceOfIMBNetwork.call_WriteLongToPacketDelegate = (ScriptingInterfaceOfIMBNetwork.WriteLongToPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.WriteLongToPacketDelegate));
			break;
		case 558:
			ScriptingInterfaceOfIMBNetwork.call_WriteStringToPacketDelegate = (ScriptingInterfaceOfIMBNetwork.WriteStringToPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.WriteStringToPacketDelegate));
			break;
		case 559:
			ScriptingInterfaceOfIMBNetwork.call_WriteUintToPacketDelegate = (ScriptingInterfaceOfIMBNetwork.WriteUintToPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.WriteUintToPacketDelegate));
			break;
		case 560:
			ScriptingInterfaceOfIMBNetwork.call_WriteUlongToPacketDelegate = (ScriptingInterfaceOfIMBNetwork.WriteUlongToPacketDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBNetwork.WriteUlongToPacketDelegate));
			break;
		case 561:
			ScriptingInterfaceOfIMBPeer.call_BeginModuleEventDelegate = (ScriptingInterfaceOfIMBPeer.BeginModuleEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.BeginModuleEventDelegate));
			break;
		case 562:
			ScriptingInterfaceOfIMBPeer.call_EndModuleEventDelegate = (ScriptingInterfaceOfIMBPeer.EndModuleEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.EndModuleEventDelegate));
			break;
		case 563:
			ScriptingInterfaceOfIMBPeer.call_GetAverageLossPercentDelegate = (ScriptingInterfaceOfIMBPeer.GetAverageLossPercentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.GetAverageLossPercentDelegate));
			break;
		case 564:
			ScriptingInterfaceOfIMBPeer.call_GetAveragePingInMillisecondsDelegate = (ScriptingInterfaceOfIMBPeer.GetAveragePingInMillisecondsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.GetAveragePingInMillisecondsDelegate));
			break;
		case 565:
			ScriptingInterfaceOfIMBPeer.call_GetHostDelegate = (ScriptingInterfaceOfIMBPeer.GetHostDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.GetHostDelegate));
			break;
		case 566:
			ScriptingInterfaceOfIMBPeer.call_GetIsSynchronizedDelegate = (ScriptingInterfaceOfIMBPeer.GetIsSynchronizedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.GetIsSynchronizedDelegate));
			break;
		case 567:
			ScriptingInterfaceOfIMBPeer.call_GetPortDelegate = (ScriptingInterfaceOfIMBPeer.GetPortDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.GetPortDelegate));
			break;
		case 568:
			ScriptingInterfaceOfIMBPeer.call_GetReversedHostDelegate = (ScriptingInterfaceOfIMBPeer.GetReversedHostDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.GetReversedHostDelegate));
			break;
		case 569:
			ScriptingInterfaceOfIMBPeer.call_IsActiveDelegate = (ScriptingInterfaceOfIMBPeer.IsActiveDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.IsActiveDelegate));
			break;
		case 570:
			ScriptingInterfaceOfIMBPeer.call_SendExistingObjectsDelegate = (ScriptingInterfaceOfIMBPeer.SendExistingObjectsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.SendExistingObjectsDelegate));
			break;
		case 571:
			ScriptingInterfaceOfIMBPeer.call_SetControlledAgentDelegate = (ScriptingInterfaceOfIMBPeer.SetControlledAgentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.SetControlledAgentDelegate));
			break;
		case 572:
			ScriptingInterfaceOfIMBPeer.call_SetIsSynchronizedDelegate = (ScriptingInterfaceOfIMBPeer.SetIsSynchronizedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.SetIsSynchronizedDelegate));
			break;
		case 573:
			ScriptingInterfaceOfIMBPeer.call_SetRelevantGameOptionsDelegate = (ScriptingInterfaceOfIMBPeer.SetRelevantGameOptionsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.SetRelevantGameOptionsDelegate));
			break;
		case 574:
			ScriptingInterfaceOfIMBPeer.call_SetTeamDelegate = (ScriptingInterfaceOfIMBPeer.SetTeamDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.SetTeamDelegate));
			break;
		case 575:
			ScriptingInterfaceOfIMBPeer.call_SetUserDataDelegate = (ScriptingInterfaceOfIMBPeer.SetUserDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBPeer.SetUserDataDelegate));
			break;
		case 576:
			ScriptingInterfaceOfIMBScreen.call_OnEditModeEnterPressDelegate = (ScriptingInterfaceOfIMBScreen.OnEditModeEnterPressDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBScreen.OnEditModeEnterPressDelegate));
			break;
		case 577:
			ScriptingInterfaceOfIMBScreen.call_OnEditModeEnterReleaseDelegate = (ScriptingInterfaceOfIMBScreen.OnEditModeEnterReleaseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBScreen.OnEditModeEnterReleaseDelegate));
			break;
		case 578:
			ScriptingInterfaceOfIMBScreen.call_OnExitButtonClickDelegate = (ScriptingInterfaceOfIMBScreen.OnExitButtonClickDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBScreen.OnExitButtonClickDelegate));
			break;
		case 579:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_CreateAgentSkeletonDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.CreateAgentSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.CreateAgentSkeletonDelegate));
			break;
		case 580:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_CreateSimpleSkeletonDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.CreateSimpleSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.CreateSimpleSkeletonDelegate));
			break;
		case 581:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_CreateWithActionSetDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.CreateWithActionSetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.CreateWithActionSetDelegate));
			break;
		case 582:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_DoesActionContinueWithCurrentActionAtChannelDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.DoesActionContinueWithCurrentActionAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.DoesActionContinueWithCurrentActionAtChannelDelegate));
			break;
		case 583:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_GetActionAtChannelDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.GetActionAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.GetActionAtChannelDelegate));
			break;
		case 584:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_GetBoneEntitialFrameDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.GetBoneEntitialFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.GetBoneEntitialFrameDelegate));
			break;
		case 585:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_GetBoneEntitialFrameAtAnimationProgressDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.GetBoneEntitialFrameAtAnimationProgressDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.GetBoneEntitialFrameAtAnimationProgressDelegate));
			break;
		case 586:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_GetSkeletonFaceAnimationNameDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.GetSkeletonFaceAnimationNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.GetSkeletonFaceAnimationNameDelegate));
			break;
		case 587:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_GetSkeletonFaceAnimationTimeDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.GetSkeletonFaceAnimationTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.GetSkeletonFaceAnimationTimeDelegate));
			break;
		case 588:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_SetAgentActionChannelDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.SetAgentActionChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.SetAgentActionChannelDelegate));
			break;
		case 589:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_SetAnimationAtChannelDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.SetAnimationAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.SetAnimationAtChannelDelegate));
			break;
		case 590:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_SetFacialAnimationOfChannelDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.SetFacialAnimationOfChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.SetFacialAnimationOfChannelDelegate));
			break;
		case 591:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_SetSkeletonFaceAnimationTimeDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.SetSkeletonFaceAnimationTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.SetSkeletonFaceAnimationTimeDelegate));
			break;
		case 592:
			ScriptingInterfaceOfIMBSkeletonExtensions.call_TickActionChannelsDelegate = (ScriptingInterfaceOfIMBSkeletonExtensions.TickActionChannelsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSkeletonExtensions.TickActionChannelsDelegate));
			break;
		case 593:
			ScriptingInterfaceOfIMBSoundEvent.call_CreateEventFromExternalFileDelegate = (ScriptingInterfaceOfIMBSoundEvent.CreateEventFromExternalFileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSoundEvent.CreateEventFromExternalFileDelegate));
			break;
		case 594:
			ScriptingInterfaceOfIMBSoundEvent.call_CreateEventFromSoundBufferDelegate = (ScriptingInterfaceOfIMBSoundEvent.CreateEventFromSoundBufferDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSoundEvent.CreateEventFromSoundBufferDelegate));
			break;
		case 595:
			ScriptingInterfaceOfIMBSoundEvent.call_PlaySoundDelegate = (ScriptingInterfaceOfIMBSoundEvent.PlaySoundDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSoundEvent.PlaySoundDelegate));
			break;
		case 596:
			ScriptingInterfaceOfIMBSoundEvent.call_PlaySoundWithIntParamDelegate = (ScriptingInterfaceOfIMBSoundEvent.PlaySoundWithIntParamDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSoundEvent.PlaySoundWithIntParamDelegate));
			break;
		case 597:
			ScriptingInterfaceOfIMBSoundEvent.call_PlaySoundWithParamDelegate = (ScriptingInterfaceOfIMBSoundEvent.PlaySoundWithParamDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSoundEvent.PlaySoundWithParamDelegate));
			break;
		case 598:
			ScriptingInterfaceOfIMBSoundEvent.call_PlaySoundWithStrParamDelegate = (ScriptingInterfaceOfIMBSoundEvent.PlaySoundWithStrParamDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBSoundEvent.PlaySoundWithStrParamDelegate));
			break;
		case 599:
			ScriptingInterfaceOfIMBTeam.call_IsEnemyDelegate = (ScriptingInterfaceOfIMBTeam.IsEnemyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTeam.IsEnemyDelegate));
			break;
		case 600:
			ScriptingInterfaceOfIMBTeam.call_SetIsEnemyDelegate = (ScriptingInterfaceOfIMBTeam.SetIsEnemyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTeam.SetIsEnemyDelegate));
			break;
		case 601:
			ScriptingInterfaceOfIMBTestRun.call_AutoContinueDelegate = (ScriptingInterfaceOfIMBTestRun.AutoContinueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.AutoContinueDelegate));
			break;
		case 602:
			ScriptingInterfaceOfIMBTestRun.call_CloseSceneDelegate = (ScriptingInterfaceOfIMBTestRun.CloseSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.CloseSceneDelegate));
			break;
		case 603:
			ScriptingInterfaceOfIMBTestRun.call_EnterEditModeDelegate = (ScriptingInterfaceOfIMBTestRun.EnterEditModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.EnterEditModeDelegate));
			break;
		case 604:
			ScriptingInterfaceOfIMBTestRun.call_GetFPSDelegate = (ScriptingInterfaceOfIMBTestRun.GetFPSDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.GetFPSDelegate));
			break;
		case 605:
			ScriptingInterfaceOfIMBTestRun.call_LeaveEditModeDelegate = (ScriptingInterfaceOfIMBTestRun.LeaveEditModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.LeaveEditModeDelegate));
			break;
		case 606:
			ScriptingInterfaceOfIMBTestRun.call_NewSceneDelegate = (ScriptingInterfaceOfIMBTestRun.NewSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.NewSceneDelegate));
			break;
		case 607:
			ScriptingInterfaceOfIMBTestRun.call_OpenDefaultSceneDelegate = (ScriptingInterfaceOfIMBTestRun.OpenDefaultSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.OpenDefaultSceneDelegate));
			break;
		case 608:
			ScriptingInterfaceOfIMBTestRun.call_OpenSceneDelegate = (ScriptingInterfaceOfIMBTestRun.OpenSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.OpenSceneDelegate));
			break;
		case 609:
			ScriptingInterfaceOfIMBTestRun.call_SaveSceneDelegate = (ScriptingInterfaceOfIMBTestRun.SaveSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.SaveSceneDelegate));
			break;
		case 610:
			ScriptingInterfaceOfIMBTestRun.call_StartMissionDelegate = (ScriptingInterfaceOfIMBTestRun.StartMissionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBTestRun.StartMissionDelegate));
			break;
		case 611:
			ScriptingInterfaceOfIMBVoiceManager.call_GetVoiceDefinitionCountWithMonsterSoundAndCollisionInfoClassNameDelegate = (ScriptingInterfaceOfIMBVoiceManager.GetVoiceDefinitionCountWithMonsterSoundAndCollisionInfoClassNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBVoiceManager.GetVoiceDefinitionCountWithMonsterSoundAndCollisionInfoClassNameDelegate));
			break;
		case 612:
			ScriptingInterfaceOfIMBVoiceManager.call_GetVoiceDefinitionListWithMonsterSoundAndCollisionInfoClassNameDelegate = (ScriptingInterfaceOfIMBVoiceManager.GetVoiceDefinitionListWithMonsterSoundAndCollisionInfoClassNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBVoiceManager.GetVoiceDefinitionListWithMonsterSoundAndCollisionInfoClassNameDelegate));
			break;
		case 613:
			ScriptingInterfaceOfIMBVoiceManager.call_GetVoiceTypeIndexDelegate = (ScriptingInterfaceOfIMBVoiceManager.GetVoiceTypeIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBVoiceManager.GetVoiceTypeIndexDelegate));
			break;
		case 614:
			ScriptingInterfaceOfIMBWindowManager.call_DontChangeCursorPosDelegate = (ScriptingInterfaceOfIMBWindowManager.DontChangeCursorPosDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWindowManager.DontChangeCursorPosDelegate));
			break;
		case 615:
			ScriptingInterfaceOfIMBWindowManager.call_EraseMessageLinesDelegate = (ScriptingInterfaceOfIMBWindowManager.EraseMessageLinesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWindowManager.EraseMessageLinesDelegate));
			break;
		case 616:
			ScriptingInterfaceOfIMBWindowManager.call_PreDisplayDelegate = (ScriptingInterfaceOfIMBWindowManager.PreDisplayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWindowManager.PreDisplayDelegate));
			break;
		case 617:
			ScriptingInterfaceOfIMBWindowManager.call_ScreenToWorldDelegate = (ScriptingInterfaceOfIMBWindowManager.ScreenToWorldDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWindowManager.ScreenToWorldDelegate));
			break;
		case 618:
			ScriptingInterfaceOfIMBWindowManager.call_WorldToScreenDelegate = (ScriptingInterfaceOfIMBWindowManager.WorldToScreenDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWindowManager.WorldToScreenDelegate));
			break;
		case 619:
			ScriptingInterfaceOfIMBWindowManager.call_WorldToScreenWithFixedZDelegate = (ScriptingInterfaceOfIMBWindowManager.WorldToScreenWithFixedZDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWindowManager.WorldToScreenWithFixedZDelegate));
			break;
		case 620:
			ScriptingInterfaceOfIMBWorld.call_CheckResourceModificationsDelegate = (ScriptingInterfaceOfIMBWorld.CheckResourceModificationsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.CheckResourceModificationsDelegate));
			break;
		case 621:
			ScriptingInterfaceOfIMBWorld.call_FixSkeletonsDelegate = (ScriptingInterfaceOfIMBWorld.FixSkeletonsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.FixSkeletonsDelegate));
			break;
		case 622:
			ScriptingInterfaceOfIMBWorld.call_GetGameTypeDelegate = (ScriptingInterfaceOfIMBWorld.GetGameTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.GetGameTypeDelegate));
			break;
		case 623:
			ScriptingInterfaceOfIMBWorld.call_GetGlobalTimeDelegate = (ScriptingInterfaceOfIMBWorld.GetGlobalTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.GetGlobalTimeDelegate));
			break;
		case 624:
			ScriptingInterfaceOfIMBWorld.call_GetLastMessagesDelegate = (ScriptingInterfaceOfIMBWorld.GetLastMessagesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.GetLastMessagesDelegate));
			break;
		case 625:
			ScriptingInterfaceOfIMBWorld.call_PauseGameDelegate = (ScriptingInterfaceOfIMBWorld.PauseGameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.PauseGameDelegate));
			break;
		case 626:
			ScriptingInterfaceOfIMBWorld.call_SetBodyUsedDelegate = (ScriptingInterfaceOfIMBWorld.SetBodyUsedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.SetBodyUsedDelegate));
			break;
		case 627:
			ScriptingInterfaceOfIMBWorld.call_SetGameTypeDelegate = (ScriptingInterfaceOfIMBWorld.SetGameTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.SetGameTypeDelegate));
			break;
		case 628:
			ScriptingInterfaceOfIMBWorld.call_SetMaterialUsedDelegate = (ScriptingInterfaceOfIMBWorld.SetMaterialUsedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.SetMaterialUsedDelegate));
			break;
		case 629:
			ScriptingInterfaceOfIMBWorld.call_SetMeshUsedDelegate = (ScriptingInterfaceOfIMBWorld.SetMeshUsedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.SetMeshUsedDelegate));
			break;
		case 630:
			ScriptingInterfaceOfIMBWorld.call_UnpauseGameDelegate = (ScriptingInterfaceOfIMBWorld.UnpauseGameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMBWorld.UnpauseGameDelegate));
			break;
		}
	}
}
