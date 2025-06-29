using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionCrosshair))]
public class MissionGauntletCrosshair : MissionGauntletBattleUIBase
{
	private GauntletLayer _layer;

	private CrosshairVM _dataSource;

	private IGauntletMovie _movie;

	private double[] _targetGadgetOpacities = new double[4];

	protected override void OnCreateView()
	{
		CombatLogManager.OnGenerateCombatLog += OnCombatLogGenerated;
		_dataSource = new CrosshairVM();
		_layer = new GauntletLayer(1);
		_movie = _layer.LoadMovie("Crosshair", _dataSource);
		if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene)
		{
			base.MissionScreen.AddLayer(_layer);
		}
	}

	protected override void OnDestroyView()
	{
		CombatLogManager.OnGenerateCombatLog -= OnCombatLogGenerated;
		if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene)
		{
			base.MissionScreen.RemoveLayer(_layer);
		}
		_dataSource = null;
		_movie = null;
		_layer = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (base.DebugInput.IsKeyReleased(InputKey.F5) && base.IsViewActive)
		{
			OnDestroyView();
			OnCreateView();
		}
		if (!base.IsViewActive)
		{
			return;
		}
		_dataSource.IsVisible = GetShouldCrosshairBeVisible();
		bool flag = true;
		bool isTargetInvalid = false;
		for (int i = 0; i < _targetGadgetOpacities.Length; i++)
		{
			_targetGadgetOpacities[i] = 0.0;
		}
		if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene && base.Mission.Mode != MissionMode.Deployment && base.Mission.MainAgent != null && !base.MissionScreen.IsViewingCharacter() && !IsMissionScreenUsingCustomCamera())
		{
			_dataSource.CrosshairType = BannerlordConfig.CrosshairType;
			Agent mainAgent = base.Mission.MainAgent;
			double num = base.MissionScreen.CameraViewAngle * ((float)Math.PI / 180f);
			double accuracy = 2.0 * Math.Tan((double)(mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) * (0.5 / Math.Tan(num * 0.5)));
			_dataSource.SetProperties(accuracy, 1f + (base.MissionScreen.CombatCamera.HorizontalFov - (float)Math.PI / 2f) / ((float)Math.PI / 2f));
			WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand);
			float numberToCheck = MBMath.WrapAngle(mainAgent.LookDirection.AsVec2.RotationInRadians - mainAgent.GetMovementDirection().RotationInRadians);
			if (wieldedWeaponInfo.IsValid && wieldedWeaponInfo.IsRangedWeapon && BannerlordConfig.DisplayTargetingReticule)
			{
				Agent.ActionCodeType currentActionType = mainAgent.GetCurrentActionType(1);
				MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;
				if (wieldedWeapon.ReloadPhaseCount > 1 && wieldedWeapon.IsReloading && currentActionType == Agent.ActionCodeType.Reload)
				{
					StackArray.StackArray10FloatFloatTuple reloadPhases = default(StackArray.StackArray10FloatFloatTuple);
					ActionIndexValueCache itemUsageReloadActionCode = MBItem.GetItemUsageReloadActionCode(wieldedWeapon.CurrentUsageItem.ItemUsage, 9, mainAgent.HasMount, -1, mainAgent.GetIsLeftStance());
					FillReloadDurationsFromActions(ref reloadPhases, wieldedWeapon.ReloadPhaseCount, mainAgent, itemUsageReloadActionCode);
					float num2 = mainAgent.GetCurrentActionProgress(1);
					ActionIndexValueCache currentActionValue = mainAgent.GetCurrentActionValue(1);
					if (currentActionValue != ActionIndexValueCache.act_none)
					{
						float num3 = 1f - MBActionSet.GetActionBlendOutStartProgress(mainAgent.ActionSet, currentActionValue);
						num2 += num3;
					}
					float animationParameter = MBAnimation.GetAnimationParameter2(mainAgent.AgentVisuals.GetSkeleton().GetAnimationAtChannel(1));
					bool flag2 = num2 > animationParameter;
					float item = (flag2 ? 1f : (num2 / animationParameter));
					short reloadPhase = wieldedWeapon.ReloadPhase;
					for (int j = 0; j < reloadPhase; j++)
					{
						reloadPhases[j] = (1f, reloadPhases[j].Item2);
					}
					if (!flag2)
					{
						reloadPhases[reloadPhase] = (item, reloadPhases[reloadPhase].Item2);
						_dataSource.SetReloadProperties(in reloadPhases, wieldedWeapon.ReloadPhaseCount);
					}
					flag = false;
				}
				if (currentActionType == Agent.ActionCodeType.ReadyRanged)
				{
					Vec2 bodyRotationConstraint = mainAgent.GetBodyRotationConstraint();
					isTargetInvalid = base.Mission.MainAgent.MountAgent != null && !MBMath.IsBetween(numberToCheck, bodyRotationConstraint.x, bodyRotationConstraint.y) && (bodyRotationConstraint.x < -0.1f || bodyRotationConstraint.y > 0.1f);
				}
			}
			else if (!wieldedWeaponInfo.IsValid || wieldedWeaponInfo.IsMeleeWeapon)
			{
				Agent.ActionCodeType currentActionType2 = mainAgent.GetCurrentActionType(1);
				Agent.UsageDirection currentActionDirection = mainAgent.GetCurrentActionDirection(1);
				if (BannerlordConfig.DisplayAttackDirection && (currentActionType2 == Agent.ActionCodeType.ReadyMelee || MBMath.IsBetween((int)currentActionType2, 1, 15)))
				{
					if (currentActionType2 == Agent.ActionCodeType.ReadyMelee)
					{
						switch (mainAgent.AttackDirection)
						{
						case Agent.UsageDirection.AttackUp:
							_targetGadgetOpacities[0] = 0.7;
							break;
						case Agent.UsageDirection.AttackRight:
							_targetGadgetOpacities[1] = 0.7;
							break;
						case Agent.UsageDirection.AttackDown:
							_targetGadgetOpacities[2] = 0.7;
							break;
						case Agent.UsageDirection.AttackLeft:
							_targetGadgetOpacities[3] = 0.7;
							break;
						}
					}
					else
					{
						isTargetInvalid = true;
						switch (currentActionDirection)
						{
						case Agent.UsageDirection.AttackEnd:
							_targetGadgetOpacities[0] = 0.7;
							break;
						case Agent.UsageDirection.DefendRight:
							_targetGadgetOpacities[1] = 0.7;
							break;
						case Agent.UsageDirection.DefendDown:
							_targetGadgetOpacities[2] = 0.7;
							break;
						case Agent.UsageDirection.DefendLeft:
							_targetGadgetOpacities[3] = 0.7;
							break;
						}
					}
				}
				else if (BannerlordConfig.DisplayAttackDirection)
				{
					Agent.UsageDirection usageDirection = mainAgent.PlayerAttackDirection();
					switch (usageDirection)
					{
					case Agent.UsageDirection.AttackUp:
						_targetGadgetOpacities[0] = 0.7;
						break;
					case Agent.UsageDirection.AttackRight:
						_targetGadgetOpacities[1] = 0.7;
						break;
					case Agent.UsageDirection.AttackDown:
						_targetGadgetOpacities[2] = 0.7;
						break;
					case Agent.UsageDirection.AttackLeft:
						if (usageDirection == Agent.UsageDirection.AttackLeft)
						{
							_targetGadgetOpacities[3] = 0.7;
						}
						break;
					}
				}
			}
		}
		if (flag)
		{
			StackArray.StackArray10FloatFloatTuple reloadPhases2 = default(StackArray.StackArray10FloatFloatTuple);
			_dataSource.SetReloadProperties(in reloadPhases2, 0);
		}
		_dataSource.SetArrowProperties(_targetGadgetOpacities[0], _targetGadgetOpacities[1], _targetGadgetOpacities[2], _targetGadgetOpacities[3]);
		_dataSource.IsTargetInvalid = isTargetInvalid;
	}

	private bool GetShouldCrosshairBeVisible()
	{
		if (base.Mission.MainAgent != null)
		{
			MissionWeapon wieldedWeapon = base.Mission.MainAgent.WieldedWeapon;
			if (BannerlordConfig.DisplayTargetingReticule && base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene && !ScreenManager.GetMouseVisibility() && !wieldedWeapon.IsEmpty && wieldedWeapon.CurrentUsageItem.IsRangedWeapon && !base.MissionScreen.IsViewingCharacter() && !IsMissionScreenUsingCustomCamera())
			{
				if (wieldedWeapon.CurrentUsageItem.WeaponClass == WeaponClass.Crossbow)
				{
					return !wieldedWeapon.IsReloading;
				}
				return true;
			}
		}
		return false;
	}

	private bool IsMissionScreenUsingCustomCamera()
	{
		return base.MissionScreen.CustomCamera != null;
	}

	private void OnCombatLogGenerated(CombatLogData logData)
	{
		bool isAttackerAgentMine = logData.IsAttackerAgentMine;
		bool flag = !logData.IsVictimAgentSameAsAttackerAgent && !logData.IsFriendlyFire;
		bool isHumanoidHeadShot = logData.IsAttackerAgentHuman && logData.BodyPartHit == BoneBodyPartType.Head;
		if (isAttackerAgentMine && flag && logData.TotalDamage > 0)
		{
			_dataSource?.ShowHitMarker(logData.IsFatalDamage, isHumanoidHeadShot);
		}
	}

	private void FillReloadDurationsFromActions(ref StackArray.StackArray10FloatFloatTuple reloadPhases, int reloadPhaseCount, Agent mainAgent, ActionIndexValueCache reloadAction)
	{
		float num = 0f;
		for (int i = 0; i < reloadPhaseCount; i++)
		{
			if (reloadAction != ActionIndexValueCache.act_none)
			{
				float num2 = MBAnimation.GetAnimationParameter2(MBActionSet.GetAnimationIndexOfAction(mainAgent.ActionSet, reloadAction)) * MBActionSet.GetActionAnimationDuration(mainAgent.ActionSet, reloadAction);
				reloadPhases[i] = (reloadPhases[i].Item1, num2);
				if (num2 > num)
				{
					num = num2;
				}
				reloadAction = MBActionSet.GetActionAnimationContinueToAction(mainAgent.ActionSet, reloadAction);
			}
		}
		if (num > 1E-05f)
		{
			for (int j = 0; j < reloadPhaseCount; j++)
			{
				reloadPhases[j] = (reloadPhases[j].Item1, reloadPhases[j].Item2 / num);
			}
		}
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (base.IsViewActive)
		{
			_layer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (base.IsViewActive)
		{
			_layer.UIContext.ContextAlpha = 1f;
		}
	}
}
