using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Handlers;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Order;

public class OrderTroopPlacer : MissionView
{
	public enum CursorState
	{
		Invisible,
		Normal,
		Ground,
		Enemy,
		Friend,
		Rotation,
		Count,
		OrderableEntity
	}

	private bool _suspendTroopPlacer;

	private bool _isMouseDown;

	private List<GameEntity> _orderPositionEntities;

	private List<GameEntity> _orderRotationEntities;

	private bool _formationDrawingMode;

	private Formation _mouseOverFormation;

	private Formation _clickedFormation;

	private Vec2 _lastMousePosition;

	private Vec2 _deltaMousePosition;

	private int _mouseOverDirection;

	private WorldPosition? _formationDrawingStartingPosition;

	private Vec2? _formationDrawingStartingPointOfMouse;

	private float? _formationDrawingStartingTime;

	private bool _restrictOrdersToDeploymentBoundaries;

	private OrderController PlayerOrderController;

	private Team PlayerTeam;

	private bool _initialized;

	private Timer formationDrawTimer;

	public bool IsDrawingForced;

	public bool IsDrawingFacing;

	public bool IsDrawingForming;

	private bool _wasDrawingForced;

	private bool _wasDrawingFacing;

	private bool _wasDrawingForming;

	private GameEntity widthEntityLeft;

	private GameEntity widthEntityRight;

	private bool isDrawnThisFrame;

	private bool wasDrawnPreviousFrame;

	public Action OnUnitDeployed;

	private static Material _meshMaterial;

	public bool SuspendTroopPlacer
	{
		get
		{
			return _suspendTroopPlacer;
		}
		set
		{
			_suspendTroopPlacer = value;
			if (value)
			{
				HideOrderPositionEntities();
			}
			else
			{
				_formationDrawingStartingPosition = null;
			}
			Reset();
		}
	}

	private bool IsDeployment
	{
		get
		{
			if (base.Mission.GetMissionBehavior<SiegeDeploymentHandler>() == null)
			{
				return base.Mission.GetMissionBehavior<BattleDeploymentHandler>() != null;
			}
			return true;
		}
	}

	public override void AfterStart()
	{
		base.AfterStart();
		_formationDrawingStartingPosition = null;
		_formationDrawingStartingPointOfMouse = null;
		_formationDrawingStartingTime = null;
		_orderRotationEntities = new List<GameEntity>();
		_orderPositionEntities = new List<GameEntity>();
		formationDrawTimer = new Timer(MBCommon.GetApplicationTime(), 1f / 30f);
		widthEntityLeft = GameEntity.CreateEmpty(base.Mission.Scene);
		widthEntityLeft.AddComponent(MetaMesh.GetCopy("order_arrow_a"));
		widthEntityLeft.SetVisibilityExcludeParents(visible: false);
		widthEntityRight = GameEntity.CreateEmpty(base.Mission.Scene);
		widthEntityRight.AddComponent(MetaMesh.GetCopy("order_arrow_a"));
		widthEntityRight.SetVisibilityExcludeParents(visible: false);
	}

	private void InitializeInADisgustingManner()
	{
		PlayerTeam = base.Mission.PlayerTeam;
		PlayerOrderController = PlayerTeam.PlayerOrderController;
	}

	public override void OnMissionTick(float dt)
	{
		base.OnMissionTick(dt);
		if (!_initialized)
		{
			MissionPeer missionPeer = (GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>() : null);
			if (base.Mission.PlayerTeam != null || (missionPeer != null && (missionPeer.Team == base.Mission.AttackerTeam || missionPeer.Team == base.Mission.DefenderTeam)))
			{
				InitializeInADisgustingManner();
				_initialized = true;
			}
		}
	}

	public void RestrictOrdersToDeploymentBoundaries(bool enabled)
	{
		_restrictOrdersToDeploymentBoundaries = enabled;
	}

	private void UpdateFormationDrawingForFacingOrder(bool giveOrder)
	{
		isDrawnThisFrame = true;
		Vec2 asVec = base.MissionScreen.GetOrderFlagPosition().AsVec2;
		Vec2 orderLookAtDirection = OrderController.GetOrderLookAtDirection(PlayerOrderController.SelectedFormations, asVec);
		PlayerOrderController.SimulateNewFacingOrder(orderLookAtDirection, out var simulationAgentFrames);
		int num = 0;
		HideOrderPositionEntities();
		foreach (WorldPosition item in simulationAgentFrames)
		{
			int entityIndex = num;
			Vec3 groundPosition = item.GetGroundVec3();
			AddOrderPositionEntity(entityIndex, in groundPosition, giveOrder);
			num++;
		}
	}

	private void UpdateFormationDrawingForDestination(bool giveOrder)
	{
		isDrawnThisFrame = true;
		PlayerOrderController.SimulateDestinationFrames(out var simulationAgentFrames);
		int num = 0;
		HideOrderPositionEntities();
		foreach (WorldPosition item in simulationAgentFrames)
		{
			int entityIndex = num;
			Vec3 groundPosition = item.GetGroundVec3();
			AddOrderPositionEntity(entityIndex, in groundPosition, giveOrder, 0.7f);
			num++;
		}
	}

	private void UpdateFormationDrawingForFormingOrder(bool giveOrder)
	{
		isDrawnThisFrame = true;
		MatrixFrame orderFlagFrame = base.MissionScreen.GetOrderFlagFrame();
		Vec3 origin = orderFlagFrame.origin;
		Vec2 asVec = orderFlagFrame.rotation.f.AsVec2;
		float orderFormCustomWidth = OrderController.GetOrderFormCustomWidth(PlayerOrderController.SelectedFormations, origin);
		PlayerOrderController.SimulateNewCustomWidthOrder(orderFormCustomWidth, out var simulationAgentFrames);
		Formation formation = PlayerOrderController.SelectedFormations.MaxBy((Formation f) => f.CountOfUnits);
		int num = 0;
		HideOrderPositionEntities();
		foreach (WorldPosition item in simulationAgentFrames)
		{
			int entityIndex = num;
			Vec3 groundPosition = item.GetGroundVec3();
			AddOrderPositionEntity(entityIndex, in groundPosition, giveOrder);
			num++;
		}
		float unitDiameter = formation.UnitDiameter;
		float interval = formation.Interval;
		int num2 = TaleWorlds.Library.MathF.Max(0, (int)((orderFormCustomWidth - unitDiameter) / (interval + unitDiameter) + 1E-05f)) + 1;
		float num3 = (float)(num2 - 1) * (interval + unitDiameter);
		for (int i = 0; i < num2; i++)
		{
			Vec2 a = new Vec2((float)i * (interval + unitDiameter) - num3 / 2f, 0f);
			Vec2 vec = asVec.TransformToParentUnitF(a);
			WorldPosition worldPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, origin, hasValidZ: false);
			worldPosition.SetVec2(worldPosition.AsVec2 + vec);
			int entityIndex2 = num++;
			Vec3 groundPosition = worldPosition.GetGroundVec3();
			AddOrderPositionEntity(entityIndex2, in groundPosition, fadeOut: false);
		}
	}

	private void UpdateFormationDrawing(bool giveOrder)
	{
		isDrawnThisFrame = true;
		HideOrderPositionEntities();
		if (!_formationDrawingStartingPosition.HasValue)
		{
			return;
		}
		WorldPosition worldPosition = WorldPosition.Invalid;
		bool flag = false;
		if (base.MissionScreen.MouseVisible && _formationDrawingStartingPointOfMouse.HasValue)
		{
			Vec2 vec = _formationDrawingStartingPointOfMouse.Value - base.Input.GetMousePositionPixel();
			if (TaleWorlds.Library.MathF.Abs(vec.x) < 10f && TaleWorlds.Library.MathF.Abs(vec.y) < 10f)
			{
				flag = true;
				worldPosition = _formationDrawingStartingPosition.Value;
			}
		}
		if (base.MissionScreen.MouseVisible && _formationDrawingStartingTime.HasValue && base.Mission.CurrentTime - _formationDrawingStartingTime.Value < 0.3f)
		{
			flag = true;
			worldPosition = _formationDrawingStartingPosition.Value;
		}
		if (!flag)
		{
			base.MissionScreen.ScreenPointToWorldRay(GetScreenPoint(), out var rayBegin, out var rayEnd);
			if (!base.Mission.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, out var collisionDistance, 0.3f, BodyFlags.CommonFocusRayCastExcludeFlags | BodyFlags.BodyOwnerFlora))
			{
				return;
			}
			Vec3 vec2 = rayEnd - rayBegin;
			vec2.Normalize();
			worldPosition = new WorldPosition(base.Mission.Scene, UIntPtr.Zero, rayBegin + vec2 * collisionDistance, hasValidZ: false);
		}
		WorldPosition worldPosition2;
		if (_mouseOverDirection == 1)
		{
			worldPosition2 = worldPosition;
			worldPosition = _formationDrawingStartingPosition.Value;
		}
		else
		{
			worldPosition2 = _formationDrawingStartingPosition.Value;
		}
		if (!OrderFlag.IsPositionOnValidGround(worldPosition2))
		{
			return;
		}
		if (_restrictOrdersToDeploymentBoundaries)
		{
			IMissionDeploymentPlan deploymentPlan = base.Mission.DeploymentPlan;
			BattleSideEnum side = base.Mission.PlayerTeam.Side;
			Vec2 position = worldPosition2.AsVec2;
			if (!deploymentPlan.IsPositionInsideDeploymentBoundaries(side, in position))
			{
				return;
			}
		}
		bool isFormationLayoutVertical = !base.DebugInput.IsControlDown();
		UpdateFormationDrawingForMovementOrder(giveOrder, worldPosition2, worldPosition, isFormationLayoutVertical);
		_deltaMousePosition *= TaleWorlds.Library.MathF.Max(1f - (base.Input.GetMousePositionRanged() - _lastMousePosition).Length * 10f, 0f);
		_lastMousePosition = base.Input.GetMousePositionRanged();
	}

	private void UpdateFormationDrawingForMovementOrder(bool giveOrder, WorldPosition formationRealStartingPosition, WorldPosition formationRealEndingPosition, bool isFormationLayoutVertical)
	{
		isDrawnThisFrame = true;
		PlayerOrderController.SimulateNewOrderWithPositionAndDirection(formationRealStartingPosition, formationRealEndingPosition, out var simulationAgentFrames, isFormationLayoutVertical);
		if (giveOrder)
		{
			if (!isFormationLayoutVertical)
			{
				PlayerOrderController.SetOrderWithTwoPositions(OrderType.MoveToLineSegmentWithHorizontalLayout, formationRealStartingPosition, formationRealEndingPosition);
			}
			else
			{
				PlayerOrderController.SetOrderWithTwoPositions(OrderType.MoveToLineSegment, formationRealStartingPosition, formationRealEndingPosition);
			}
		}
		int num = 0;
		foreach (WorldPosition item in simulationAgentFrames)
		{
			int entityIndex = num;
			Vec3 groundPosition = item.GetGroundVec3();
			AddOrderPositionEntity(entityIndex, in groundPosition, giveOrder);
			num++;
		}
	}

	private void HandleMouseDown()
	{
		if (PlayerOrderController.SelectedFormations.IsEmpty() || _clickedFormation != null)
		{
			return;
		}
		switch (GetCursorState())
		{
		case CursorState.Enemy:
		case CursorState.Friend:
			_clickedFormation = _mouseOverFormation;
			break;
		case CursorState.Normal:
		{
			_formationDrawingMode = true;
			base.MissionScreen.ScreenPointToWorldRay(GetScreenPoint(), out var rayBegin, out var rayEnd);
			if (base.Mission.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, out var collisionDistance, 0.3f, BodyFlags.CommonFocusRayCastExcludeFlags | BodyFlags.BodyOwnerFlora))
			{
				Vec3 vec2 = rayEnd - rayBegin;
				vec2.Normalize();
				_formationDrawingStartingPosition = new WorldPosition(base.Mission.Scene, UIntPtr.Zero, rayBegin + vec2 * collisionDistance, hasValidZ: false);
				_formationDrawingStartingPointOfMouse = base.Input.GetMousePositionPixel();
				_formationDrawingStartingTime = base.Mission.CurrentTime;
			}
			else
			{
				_formationDrawingStartingPosition = null;
				_formationDrawingStartingPointOfMouse = null;
				_formationDrawingStartingTime = null;
			}
			break;
		}
		case CursorState.Rotation:
			if (_mouseOverFormation.CountOfUnits > 0)
			{
				HideNonSelectedOrderRotationEntities(_mouseOverFormation);
				PlayerOrderController.ClearSelectedFormations();
				PlayerOrderController.SelectFormation(_mouseOverFormation);
				_formationDrawingMode = true;
				WorldPosition worldPosition = _mouseOverFormation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.GroundVec3);
				Vec2 direction = _mouseOverFormation.Direction;
				direction.RotateCCW(-System.MathF.PI / 2f);
				_formationDrawingStartingPosition = worldPosition;
				_formationDrawingStartingPosition.Value.SetVec2(_formationDrawingStartingPosition.Value.AsVec2 + direction * ((_mouseOverDirection == 1) ? 0.5f : (-0.5f)) * _mouseOverFormation.Width);
				WorldPosition worldPosition2 = worldPosition;
				worldPosition2.SetVec2(worldPosition2.AsVec2 + direction * ((_mouseOverDirection == 1) ? (-0.5f) : 0.5f) * _mouseOverFormation.Width);
				Vec2 vec = base.MissionScreen.SceneView.WorldPointToScreenPoint(worldPosition2.GetGroundVec3());
				Vec2 screenPoint = GetScreenPoint();
				_deltaMousePosition = vec - screenPoint;
				_lastMousePosition = base.Input.GetMousePositionRanged();
			}
			break;
		case CursorState.Invisible:
		case CursorState.Ground:
			break;
		}
	}

	private void HandleMouseUp()
	{
		if (_clickedFormation != null)
		{
			if (_clickedFormation.CountOfUnits > 0 && _clickedFormation.Team == PlayerTeam)
			{
				Formation clickedFormation = _clickedFormation;
				_clickedFormation = null;
				GetCursorState();
				_clickedFormation = clickedFormation;
				HideNonSelectedOrderRotationEntities(_clickedFormation);
				PlayerOrderController.ClearSelectedFormations();
				PlayerOrderController.SelectFormation(_clickedFormation);
			}
			_clickedFormation = null;
		}
		else if (GetCursorState() == CursorState.Ground)
		{
			if (IsDrawingFacing || _wasDrawingFacing)
			{
				UpdateFormationDrawingForFacingOrder(giveOrder: true);
			}
			else if (IsDrawingForming || _wasDrawingForming)
			{
				UpdateFormationDrawingForFormingOrder(giveOrder: true);
			}
			else
			{
				UpdateFormationDrawing(giveOrder: true);
			}
			if (IsDeployment)
			{
				OnUnitDeployed?.Invoke();
				UISoundsHelper.PlayUISound("event:/ui/mission/deploy");
			}
		}
		_formationDrawingMode = false;
		_deltaMousePosition = Vec2.Zero;
	}

	private Vec2 GetScreenPoint()
	{
		if (!base.MissionScreen.MouseVisible)
		{
			return new Vec2(0.5f, 0.5f) + _deltaMousePosition;
		}
		return base.Input.GetMousePositionRanged() + _deltaMousePosition;
	}

	private CursorState GetCursorState()
	{
		CursorState cursorState = CursorState.Invisible;
		if (!PlayerOrderController.SelectedFormations.IsEmpty() && _clickedFormation == null)
		{
			base.MissionScreen.ScreenPointToWorldRay(GetScreenPoint(), out var rayBegin, out var rayEnd);
			if (!base.Mission.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, out float collisionDistance, out GameEntity collidedEntity, 0.3f, BodyFlags.CommonFocusRayCastExcludeFlags | BodyFlags.BodyOwnerFlora))
			{
				collisionDistance = 1000f;
			}
			if (cursorState == CursorState.Invisible && collisionDistance < 1000f)
			{
				if (!_formationDrawingMode && collidedEntity == null)
				{
					for (int i = 0; i < _orderRotationEntities.Count; i++)
					{
						GameEntity gameEntity = _orderRotationEntities[i];
						if (gameEntity.IsVisibleIncludeParents() && collidedEntity == gameEntity)
						{
							_mouseOverFormation = PlayerOrderController.SelectedFormations.ElementAt(i / 2);
							_mouseOverDirection = 1 - (i & 1);
							cursorState = CursorState.Rotation;
							break;
						}
					}
				}
				if (cursorState == CursorState.Invisible && base.MissionScreen.OrderFlag.FocusedOrderableObject != null)
				{
					cursorState = CursorState.OrderableEntity;
				}
				if (cursorState == CursorState.Invisible)
				{
					cursorState = IsCursorStateGroundOrNormal();
				}
			}
		}
		if (cursorState != CursorState.Ground && cursorState != CursorState.Rotation)
		{
			_mouseOverDirection = 0;
		}
		return cursorState;
	}

	private CursorState IsCursorStateGroundOrNormal()
	{
		if (!_formationDrawingMode)
		{
			return CursorState.Normal;
		}
		return CursorState.Ground;
	}

	private void AddOrderPositionEntity(int entityIndex, in Vec3 groundPosition, bool fadeOut, float alpha = -1f)
	{
		while (_orderPositionEntities.Count <= entityIndex)
		{
			GameEntity gameEntity = GameEntity.CreateEmpty(base.Mission.Scene);
			gameEntity.EntityFlags |= EntityFlags.NotAffectedBySeason;
			MetaMesh copy = MetaMesh.GetCopy("order_flag_small");
			if (_meshMaterial == null)
			{
				_meshMaterial = copy.GetMeshAtIndex(0).GetMaterial().CreateCopy();
				_meshMaterial.SetAlphaBlendMode(Material.MBAlphaBlendMode.Factor);
			}
			copy.SetMaterial(_meshMaterial);
			gameEntity.AddComponent(copy);
			gameEntity.SetVisibilityExcludeParents(visible: false);
			_orderPositionEntities.Add(gameEntity);
		}
		GameEntity gameEntity2 = _orderPositionEntities[entityIndex];
		MatrixFrame frame = new MatrixFrame(Mat3.Identity, groundPosition);
		gameEntity2.SetFrame(ref frame);
		if (alpha != -1f)
		{
			gameEntity2.SetVisibilityExcludeParents(visible: true);
			gameEntity2.SetAlpha(alpha);
		}
		else if (fadeOut)
		{
			gameEntity2.FadeOut(0.3f, isRemovingFromScene: false);
		}
		else
		{
			gameEntity2.FadeIn();
		}
	}

	private void HideNonSelectedOrderRotationEntities(Formation formation)
	{
		for (int i = 0; i < _orderRotationEntities.Count; i++)
		{
			GameEntity gameEntity = _orderRotationEntities[i];
			if (gameEntity == null && gameEntity.IsVisibleIncludeParents() && PlayerOrderController.SelectedFormations.ElementAt(i / 2) != formation)
			{
				gameEntity.SetVisibilityExcludeParents(visible: false);
				gameEntity.BodyFlag |= BodyFlags.Disabled;
			}
		}
	}

	private void HideOrderPositionEntities()
	{
		foreach (GameEntity orderPositionEntity in _orderPositionEntities)
		{
			orderPositionEntity.HideIfNotFadingOut();
		}
		for (int i = 0; i < _orderRotationEntities.Count; i++)
		{
			GameEntity gameEntity = _orderRotationEntities[i];
			gameEntity.SetVisibilityExcludeParents(visible: false);
			gameEntity.BodyFlag |= BodyFlags.Disabled;
		}
	}

	[Conditional("DEBUG")]
	private void DebugTick(float dt)
	{
		_ = _initialized;
	}

	private void Reset()
	{
		_isMouseDown = false;
		_formationDrawingMode = false;
		_formationDrawingStartingPosition = null;
		_formationDrawingStartingPointOfMouse = null;
		_formationDrawingStartingTime = null;
		_mouseOverFormation = null;
		_clickedFormation = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		if (!_initialized)
		{
			return;
		}
		base.OnMissionScreenTick(dt);
		if (PlayerOrderController.SelectedFormations.Count == 0)
		{
			return;
		}
		isDrawnThisFrame = false;
		if (SuspendTroopPlacer)
		{
			return;
		}
		if (base.Input.IsKeyPressed(InputKey.LeftMouseButton) || base.Input.IsKeyPressed(InputKey.ControllerRTrigger))
		{
			_isMouseDown = true;
			HandleMouseDown();
		}
		if ((base.Input.IsKeyReleased(InputKey.LeftMouseButton) || base.Input.IsKeyReleased(InputKey.ControllerRTrigger)) && _isMouseDown)
		{
			_isMouseDown = false;
			HandleMouseUp();
		}
		else if ((base.Input.IsKeyDown(InputKey.LeftMouseButton) || base.Input.IsKeyDown(InputKey.ControllerRTrigger)) && _isMouseDown)
		{
			if (formationDrawTimer.Check(MBCommon.GetApplicationTime()) && !IsDrawingFacing && !IsDrawingForming && IsCursorStateGroundOrNormal() == CursorState.Ground && GetCursorState() == CursorState.Ground)
			{
				UpdateFormationDrawing(giveOrder: false);
			}
		}
		else if (IsDrawingForced)
		{
			Reset();
			HandleMouseDown();
			UpdateFormationDrawing(giveOrder: false);
		}
		else if (IsDrawingFacing || _wasDrawingFacing)
		{
			if (IsDrawingFacing)
			{
				Reset();
				UpdateFormationDrawingForFacingOrder(giveOrder: false);
			}
		}
		else if (IsDrawingForming || _wasDrawingForming)
		{
			if (IsDrawingForming)
			{
				Reset();
				UpdateFormationDrawingForFormingOrder(giveOrder: false);
			}
		}
		else if (_wasDrawingForced)
		{
			Reset();
		}
		else
		{
			UpdateFormationDrawingForDestination(giveOrder: false);
		}
		foreach (GameEntity orderPositionEntity in _orderPositionEntities)
		{
			orderPositionEntity.SetPreviousFrameInvalid();
		}
		foreach (GameEntity orderRotationEntity in _orderRotationEntities)
		{
			orderRotationEntity.SetPreviousFrameInvalid();
		}
		_wasDrawingForced = IsDrawingForced;
		_wasDrawingFacing = IsDrawingFacing;
		_wasDrawingForming = IsDrawingForming;
		wasDrawnPreviousFrame = isDrawnThisFrame;
	}
}
