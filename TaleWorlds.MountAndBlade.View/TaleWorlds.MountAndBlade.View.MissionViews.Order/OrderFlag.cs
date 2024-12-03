using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View.Screens;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Order;

public class OrderFlag
{
	private readonly GameEntity _entity;

	private readonly GameEntity _flag;

	private readonly GameEntity _gear;

	private readonly GameEntity _arrow;

	private readonly GameEntity _width;

	private readonly GameEntity _attack;

	private readonly GameEntity _flagUnavailable;

	private readonly GameEntity _widthLeft;

	private readonly GameEntity _widthRight;

	public bool IsTroop = true;

	private bool _isWidthVisible;

	private float _customWidth;

	private GameEntity _current;

	private readonly IEnumerable<IOrderableWithInteractionArea> _orderablesWithInteractionArea;

	private readonly Mission _mission;

	private readonly MissionScreen _missionScreen;

	private readonly float _arrowLength;

	private bool _isArrowVisible;

	private Vec2 _arrowDirection;

	private GameEntity Current
	{
		get
		{
			return _current;
		}
		set
		{
			if (_current != value)
			{
				_current = value;
				_flag.SetVisibilityExcludeParents(visible: false);
				_gear.SetVisibilityExcludeParents(visible: false);
				_arrow.SetVisibilityExcludeParents(visible: false);
				_width.SetVisibilityExcludeParents(visible: false);
				_attack.SetVisibilityExcludeParents(visible: false);
				_flagUnavailable.SetVisibilityExcludeParents(visible: false);
				_current.SetVisibilityExcludeParents(visible: true);
				if (_current == _arrow || _current == _flagUnavailable)
				{
					_flag.SetVisibilityExcludeParents(visible: true);
				}
			}
		}
	}

	public IOrderable FocusedOrderableObject { get; private set; }

	public int LatestUpdateFrameNo { get; private set; }

	public Vec3 Position
	{
		get
		{
			return _entity.GlobalPosition;
		}
		private set
		{
			MatrixFrame frame = _entity.GetFrame();
			frame.origin = value;
			_entity.SetFrame(ref frame);
		}
	}

	public MatrixFrame Frame => _entity.GetGlobalFrame();

	public bool IsVisible
	{
		get
		{
			return _entity.IsVisibleIncludeParents();
		}
		set
		{
			_entity.SetVisibilityExcludeParents(value);
			if (!value)
			{
				FocusedOrderableObject = null;
			}
		}
	}

	public OrderFlag(Mission mission, MissionScreen missionScreen)
	{
		_mission = mission;
		_missionScreen = missionScreen;
		_entity = GameEntity.CreateEmpty(_mission.Scene);
		_flag = GameEntity.CreateEmpty(_mission.Scene);
		_gear = GameEntity.CreateEmpty(_mission.Scene);
		_arrow = GameEntity.CreateEmpty(_mission.Scene);
		_width = GameEntity.CreateEmpty(_mission.Scene);
		_attack = GameEntity.CreateEmpty(_mission.Scene);
		_flagUnavailable = GameEntity.CreateEmpty(_mission.Scene);
		_widthLeft = GameEntity.CreateEmpty(_mission.Scene);
		_widthRight = GameEntity.CreateEmpty(_mission.Scene);
		_entity.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_flag.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_gear.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_arrow.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_width.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_attack.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_flagUnavailable.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_widthLeft.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_widthRight.EntityFlags |= EntityFlags.NotAffectedBySeason;
		_flag.AddComponent(MetaMesh.GetCopy("order_flag_a"));
		MatrixFrame frame = _flag.GetFrame();
		frame.Scale(new Vec3(10f, 10f, 10f));
		_flag.SetFrame(ref frame);
		_gear.AddComponent(MetaMesh.GetCopy("order_gear"));
		MatrixFrame frame2 = _gear.GetFrame();
		frame2.Scale(new Vec3(10f, 10f, 10f));
		_gear.SetFrame(ref frame2);
		_arrow.AddComponent(MetaMesh.GetCopy("order_arrow_a"));
		_widthLeft.AddComponent(MetaMesh.GetCopy("order_arrow_a"));
		_widthRight.AddComponent(MetaMesh.GetCopy("order_arrow_a"));
		MatrixFrame frame3 = MatrixFrame.Identity;
		frame3.rotation.RotateAboutUp(-System.MathF.PI / 2f);
		_widthLeft.SetFrame(ref frame3);
		frame3 = MatrixFrame.Identity;
		frame3.rotation.RotateAboutUp(System.MathF.PI / 2f);
		_widthRight.SetFrame(ref frame3);
		_width.AddChild(_widthLeft);
		_width.AddChild(_widthRight);
		MetaMesh copy = MetaMesh.GetCopy("destroy_icon");
		copy.RecomputeBoundingBox(recomputeMeshes: true);
		MatrixFrame frame4 = copy.Frame;
		frame4.Scale(new Vec3(0.15f, 0.15f, 0.15f));
		frame4.Elevate(10f);
		copy.Frame = frame4;
		_attack.AddMultiMesh(copy);
		_flagUnavailable.AddComponent(MetaMesh.GetCopy("order_unavailable"));
		_entity.AddChild(_flag);
		_entity.AddChild(_gear);
		_entity.AddChild(_arrow);
		_entity.AddChild(_width);
		_entity.AddChild(_attack);
		_entity.AddChild(_flagUnavailable);
		_flag.SetVisibilityExcludeParents(visible: false);
		_gear.SetVisibilityExcludeParents(visible: false);
		_arrow.SetVisibilityExcludeParents(visible: false);
		_width.SetVisibilityExcludeParents(visible: false);
		_attack.SetVisibilityExcludeParents(visible: false);
		_flagUnavailable.SetVisibilityExcludeParents(visible: false);
		Current = _flag;
		BoundingBox boundingBox = _arrow.GetMetaMesh(0).GetBoundingBox();
		_arrowLength = boundingBox.max.y - boundingBox.min.y;
		UpdateFrame(out var _, checkForTargetEntity: false);
		_orderablesWithInteractionArea = _mission.MissionObjects.OfType<IOrderableWithInteractionArea>();
	}

	public void Tick(float dt)
	{
		FocusedOrderableObject = null;
		GameEntity collidedEntity = GetCollidedEntity();
		if (collidedEntity != null)
		{
			BattleSideEnum side = Mission.Current.PlayerTeam.Side;
			IOrderable orderable = (IOrderable)collidedEntity.GetScriptComponents().First((ScriptComponentBehavior sc) => sc is IOrderable orderable2 && orderable2.GetOrder(side) != OrderType.None);
			if (orderable.GetOrder(side) != 0)
			{
				FocusedOrderableObject = orderable;
			}
		}
		UpdateFrame(out var isOnValidGround, collidedEntity != null);
		LatestUpdateFrameNo = Utilities.EngineFrameNo;
		if (!IsVisible)
		{
			return;
		}
		if (FocusedOrderableObject == null)
		{
			FocusedOrderableObject = _orderablesWithInteractionArea.FirstOrDefault((IOrderableWithInteractionArea o) => ((ScriptComponentBehavior)o).GameEntity.IsVisibleIncludeParents() && o.IsPointInsideInteractionArea(Position));
			if (FocusedOrderableObject is ScriptComponentBehavior scriptComponentBehavior && scriptComponentBehavior.GameEntity.Scene == null)
			{
				FocusedOrderableObject = null;
			}
		}
		UpdateCurrentMesh(isOnValidGround);
		if (Current == _flag || Current == _flagUnavailable)
		{
			MatrixFrame frame = _flag.GetFrame();
			float num = TaleWorlds.Library.MathF.Sin(MBCommon.GetApplicationTime() * 2f) + 1f;
			num *= 0.25f;
			frame.origin.z = num;
			_flag.SetFrame(ref frame);
			_flagUnavailable.SetFrame(ref frame);
		}
	}

	private void UpdateCurrentMesh(bool isOnValidGround)
	{
		if (FocusedOrderableObject != null)
		{
			BattleSideEnum side = Mission.Current.PlayerTeam.Side;
			if (FocusedOrderableObject.GetOrder(side) == OrderType.AttackEntity)
			{
				Current = _attack;
				return;
			}
			OrderType order = FocusedOrderableObject.GetOrder(side);
			if (order == OrderType.Use || order == OrderType.FollowEntity)
			{
				Current = _gear;
				return;
			}
		}
		if (_isArrowVisible)
		{
			Current = _arrow;
		}
		else if (_isWidthVisible)
		{
			Current = _width;
		}
		else
		{
			Current = (isOnValidGround ? _flag : _flagUnavailable);
		}
	}

	public void SetArrowVisibility(bool isVisible, Vec2 arrowDirection)
	{
		_isArrowVisible = isVisible;
		_arrowDirection = arrowDirection;
	}

	private void UpdateFrame(out bool isOnValidGround, bool checkForTargetEntity)
	{
		if (_missionScreen.GetProjectedMousePositionOnGround(out var groundPosition, out var groundNormal, BodyFlags.BodyOwnerFlora, checkOccludedSurface: true))
		{
			WorldPosition orderPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, groundPosition, hasValidZ: false);
			isOnValidGround = ((!IsVisible || checkForTargetEntity) ? Mission.Current.IsOrderPositionAvailable(in orderPosition, Mission.Current.PlayerTeam) : IsPositionOnValidGround(orderPosition));
		}
		else
		{
			isOnValidGround = false;
			groundPosition = new Vec3(0f, 0f, -100000f);
		}
		Position = groundPosition;
		_missionScreen.ScreenPointToWorldRay(Vec2.One * 0.5f, out var rayBegin, out groundNormal);
		float a = ((_missionScreen.LastFollowedAgent == null) ? _missionScreen.CombatCamera.Frame.rotation.f.RotationZ : (rayBegin - Position).AsVec2.RotationInRadians);
		MatrixFrame frame = _entity.GetFrame();
		frame.rotation = Mat3.Identity;
		frame.rotation.RotateAboutUp(a);
		_entity.SetFrame(ref frame);
		if (_isArrowVisible)
		{
			a = _arrowDirection.RotationInRadians;
			Mat3 identity = Mat3.Identity;
			identity.RotateAboutUp(a);
			MatrixFrame frame2 = MatrixFrame.Identity;
			frame2.rotation = frame.rotation.TransformToLocal(identity);
			frame2.Advance(0f - _arrowLength);
			_arrow.SetFrame(ref frame2);
		}
		if (_isWidthVisible)
		{
			_widthLeft.SetLocalPosition(Vec3.Side * (_customWidth * 0.5f - 0f));
			_widthRight.SetLocalPosition(Vec3.Side * (_customWidth * -0.5f + 0f));
			_widthLeft.SetLocalPosition(Vec3.Side * (_customWidth * 0.5f - _arrowLength));
			_widthRight.SetLocalPosition(Vec3.Side * (_customWidth * -0.5f + _arrowLength));
		}
	}

	public static bool IsPositionOnValidGround(WorldPosition worldPosition)
	{
		return Mission.Current.IsFormationUnitPositionAvailable(ref worldPosition, Mission.Current.PlayerTeam);
	}

	public static bool IsOrderPositionValid(WorldPosition orderPosition)
	{
		return Mission.Current.IsOrderPositionAvailable(in orderPosition, Mission.Current.PlayerTeam);
	}

	private GameEntity GetCollidedEntity()
	{
		Vec2 screenPoint = ((Mission.Current.GetMissionBehavior<BattleDeploymentHandler>() != null) ? Input.MousePositionRanged : new Vec2(0.5f, 0.5f));
		_missionScreen.ScreenPointToWorldRay(screenPoint, out var rayBegin, out var rayEnd);
		_mission.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, out float _, out GameEntity collidedEntity, 0.3f, BodyFlags.CommonFocusRayCastExcludeFlags | BodyFlags.BodyOwnerFlora);
		while (collidedEntity != null && !collidedEntity.GetScriptComponents().Any((ScriptComponentBehavior sc) => sc is IOrderable orderable && orderable.GetOrder(Mission.Current.PlayerTeam.Side) != OrderType.None))
		{
			collidedEntity = collidedEntity.Parent;
		}
		return collidedEntity;
	}

	public void SetWidthVisibility(bool isVisible, float width)
	{
		_isWidthVisible = isVisible;
		_customWidth = width;
	}
}
