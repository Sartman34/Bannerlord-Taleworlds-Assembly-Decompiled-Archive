using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class TrainingIcon : UsableMachine
{
	private static readonly ActionIndexCache act_pickup_middle_begin = ActionIndexCache.Create("act_pickup_middle_begin");

	private static readonly ActionIndexCache act_pickup_middle_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_middle_end = ActionIndexCache.Create("act_pickup_middle_end");

	private static readonly ActionIndexCache act_pickup_middle_end_left_stance = ActionIndexCache.Create("act_pickup_middle_end_left_stance");

	private static readonly string HighlightBeamTag = "highlight_beam";

	private bool _activated;

	private float _markerAlpha;

	private float _targetMarkerAlpha;

	private float _markerAlphaChangeAmount = 110f;

	private List<GameEntity> _weaponIcons = new List<GameEntity>();

	private GameEntity _markerBeam;

	[EditableScriptComponentVariable(true)]
	private string _descriptionTextOfIcon = "";

	[EditableScriptComponentVariable(true)]
	private string _trainingSubTypeTag = "";

	public bool Focused { get; private set; }

	protected internal override void OnInit()
	{
		base.OnInit();
		_markerBeam = base.GameEntity.GetFirstChildEntityWithTag(HighlightBeamTag);
		_weaponIcons = (from x in base.GameEntity.GetChildren()
			where !x.GetScriptComponents().Any() && x != _markerBeam
			select x).ToList();
		SetScriptComponentToTick(GetTickRequirement());
	}

	public override TickRequirement GetTickRequirement()
	{
		return TickRequirement.Tick | base.GetTickRequirement();
	}

	protected internal override void OnTick(float dt)
	{
		base.OnTick(dt);
		if (_markerBeam != null)
		{
			if (MathF.Abs(_markerAlpha - _targetMarkerAlpha) > dt * _markerAlphaChangeAmount)
			{
				_markerAlpha += dt * _markerAlphaChangeAmount * (float)MathF.Sign(_targetMarkerAlpha - _markerAlpha);
				_markerBeam.GetChild(0).GetFirstMesh().SetVectorArgument(_markerAlpha, 1f, 0.49f, 11.65f);
			}
			else
			{
				_markerAlpha = _targetMarkerAlpha;
				if (_targetMarkerAlpha == 0f)
				{
					_markerBeam?.SetVisibilityExcludeParents(visible: false);
				}
			}
		}
		foreach (StandingPoint standingPoint in base.StandingPoints)
		{
			if (!standingPoint.HasUser)
			{
				continue;
			}
			Agent userAgent = standingPoint.UserAgent;
			ActionIndexValueCache currentActionValue = userAgent.GetCurrentActionValue(0);
			ActionIndexValueCache currentActionValue2 = userAgent.GetCurrentActionValue(1);
			if (!(currentActionValue2 == ActionIndexValueCache.act_none) || (!(currentActionValue == act_pickup_middle_begin) && !(currentActionValue == act_pickup_middle_begin_left_stance)))
			{
				if (currentActionValue2 == ActionIndexValueCache.act_none && (currentActionValue == act_pickup_middle_end || currentActionValue == act_pickup_middle_end_left_stance))
				{
					_activated = true;
					userAgent.StopUsingGameObject();
				}
				else if (currentActionValue2 != ActionIndexValueCache.act_none || !userAgent.SetActionChannel(0, userAgent.GetIsLeftStance() ? act_pickup_middle_begin_left_stance : act_pickup_middle_begin, ignorePriority: false, 0uL))
				{
					userAgent.StopUsingGameObject();
				}
			}
		}
	}

	public void SetMarked(bool highlight)
	{
		if (highlight)
		{
			_targetMarkerAlpha = 75f;
			_markerBeam.GetChild(0).GetFirstMesh().SetVectorArgument(_markerAlpha, 1f, 0.49f, 11.65f);
			_markerBeam?.SetVisibilityExcludeParents(visible: true);
		}
		else
		{
			_targetMarkerAlpha = 0f;
		}
	}

	public bool GetIsActivated()
	{
		bool activated = _activated;
		_activated = false;
		return activated;
	}

	public string GetTrainingSubTypeTag()
	{
		return _trainingSubTypeTag;
	}

	public void DisableIcon()
	{
		foreach (GameEntity weaponIcon in _weaponIcons)
		{
			weaponIcon.SetVisibilityExcludeParents(visible: false);
		}
	}

	public void EnableIcon()
	{
		foreach (GameEntity weaponIcon in _weaponIcons)
		{
			weaponIcon.SetVisibilityExcludeParents(visible: true);
		}
	}

	public override string GetDescriptionText(GameEntity gameEntity = null)
	{
		TextObject textObject = new TextObject("{=!}{TRAINING_TYPE}");
		textObject.SetTextVariable("TRAINING_TYPE", GameTexts.FindText("str_tutorial_" + _descriptionTextOfIcon));
		return textObject.ToString();
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject = null)
	{
		TextObject textObject = new TextObject("{=wY1qP2qj}{KEY} Select");
		textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
		return textObject;
	}

	public override void OnFocusGain(Agent userAgent)
	{
		base.OnFocusGain(userAgent);
		Focused = true;
	}

	public override void OnFocusLose(Agent userAgent)
	{
		base.OnFocusLose(userAgent);
		Focused = false;
	}
}
