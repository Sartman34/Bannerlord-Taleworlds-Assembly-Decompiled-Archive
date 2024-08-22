using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Library.EventSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI;

public class EventManager
{
	public const int MinParallelUpdateCount = 64;

	private const int DirtyCount = 2;

	private const float DragStartThreshold = 100f;

	private const float ScrollScale = 0.4f;

	private List<Action> _onAfterFinalizedCallbacks;

	private Widget _focusedWidget;

	private Widget _hoveredView;

	private List<Widget> _mouseOveredViews;

	private Widget _dragHoveredView;

	private Widget _latestMouseDownWidget;

	private Widget _latestMouseUpWidget;

	private Widget _latestMouseAlternateDownWidget;

	private Widget _latestMouseAlternateUpWidget;

	private int _measureDirty;

	private int _layoutDirty;

	private bool _positionsDirty;

	private const int _stickMovementScaleAmount = 3000;

	private Vector2 _lastClickPosition;

	private bool _mouseIsDown;

	private Vector2 _lastAlternateClickPosition;

	private bool _mouseAlternateIsDown;

	private Vector2 _dragOffset = new Vector2(0f, 0f);

	private Widget _draggedWidgetPreviousParent;

	private int _draggedWidgetIndex;

	private DragCarrierWidget _dragCarrier;

	private object _lateUpdateActionLocker;

	private Dictionary<int, List<UpdateAction>> _lateUpdateActions;

	private Dictionary<int, List<UpdateAction>> _lateUpdateActionsRunning;

	private WidgetContainer _widgetsWithUpdateContainer;

	private WidgetContainer _widgetsWithLateUpdateContainer;

	private WidgetContainer _widgetsWithParallelUpdateContainer;

	private WidgetContainer _widgetsWithVisualDefinitionsContainer;

	private WidgetContainer _widgetsWithTweenPositionsContainer;

	private WidgetContainer _widgetsWithUpdateBrushesContainer;

	private const int UpdateActionOrderCount = 5;

	private volatile bool _doingParallelTask;

	private TwoDimensionDrawContext _drawContext;

	private Action _widgetsWithUpdateContainerDoDefragmentationDelegate;

	private Action _widgetsWithParallelUpdateContainerDoDefragmentationDelegate;

	private Action _widgetsWithLateUpdateContainerDoDefragmentationDelegate;

	private Action _widgetsWithUpdateBrushesContainerDoDefragmentationDelegate;

	private readonly TWParallel.ParallelForWithDtAuxPredicate ParallelUpdateWidgetPredicate;

	private readonly TWParallel.ParallelForWithDtAuxPredicate UpdateBrushesWidgetPredicate;

	private readonly TWParallel.ParallelForWithDtAuxPredicate WidgetDoTweenPositionAuxPredicate;

	private float _lastSetFrictionValue = 1f;

	public Func<bool> OnGetIsHitThisFrame;

	public float Time { get; private set; }

	public Vec2 UsableArea { get; set; } = new Vec2(1f, 1f);


	public float LeftUsableAreaStart { get; private set; }

	public float TopUsableAreaStart { get; private set; }

	public static TaleWorlds.Library.EventSystem.EventManager UIEventManager { get; private set; }

	public Vector2 MousePositionInReferenceResolution => MousePosition * Context.CustomInverseScale;

	public bool IsControllerActive { get; private set; }

	public Vector2 PageSize { get; private set; }

	public UIContext Context { get; private set; }

	public IInputService InputService { get; internal set; }

	public IInputContext InputContext { get; internal set; }

	public Widget Root { get; private set; }

	public Widget FocusedWidget
	{
		get
		{
			return _focusedWidget;
		}
		private set
		{
			if (value != null && value.ConnectedToRoot)
			{
				_focusedWidget = value;
			}
			else
			{
				_focusedWidget = null;
			}
		}
	}

	public Widget HoveredView
	{
		get
		{
			return _hoveredView;
		}
		private set
		{
			if (value != null && value.ConnectedToRoot)
			{
				_hoveredView = value;
			}
			else
			{
				_hoveredView = null;
			}
		}
	}

	public List<Widget> MouseOveredViews
	{
		get
		{
			return _mouseOveredViews;
		}
		private set
		{
			if (value != null)
			{
				_mouseOveredViews = value;
			}
			else
			{
				_mouseOveredViews = null;
			}
		}
	}

	public Widget DragHoveredView
	{
		get
		{
			return _dragHoveredView;
		}
		private set
		{
			if (value != null && value.ConnectedToRoot)
			{
				_dragHoveredView = value;
			}
			else
			{
				_dragHoveredView = null;
			}
		}
	}

	public Widget DraggedWidget { get; private set; }

	public Vector2 DraggedWidgetPosition
	{
		get
		{
			if (DraggedWidget != null)
			{
				return _dragCarrier.GlobalPosition * Context.CustomScale - new Vector2(LeftUsableAreaStart, TopUsableAreaStart);
			}
			return MousePositionInReferenceResolution;
		}
	}

	public Widget LatestMouseDownWidget
	{
		get
		{
			return _latestMouseDownWidget;
		}
		private set
		{
			if (value != null && value.ConnectedToRoot)
			{
				_latestMouseDownWidget = value;
			}
			else
			{
				_latestMouseDownWidget = null;
			}
		}
	}

	public Widget LatestMouseUpWidget
	{
		get
		{
			return _latestMouseUpWidget;
		}
		private set
		{
			if (value != null && value.ConnectedToRoot)
			{
				_latestMouseUpWidget = value;
			}
			else
			{
				_latestMouseUpWidget = null;
			}
		}
	}

	public Widget LatestMouseAlternateDownWidget
	{
		get
		{
			return _latestMouseAlternateDownWidget;
		}
		private set
		{
			if (value != null && value.ConnectedToRoot)
			{
				_latestMouseAlternateDownWidget = value;
			}
			else
			{
				_latestMouseAlternateDownWidget = null;
			}
		}
	}

	public Widget LatestMouseAlternateUpWidget
	{
		get
		{
			return _latestMouseAlternateUpWidget;
		}
		private set
		{
			if (value != null && value.ConnectedToRoot)
			{
				_latestMouseAlternateUpWidget = value;
			}
			else
			{
				_latestMouseAlternateUpWidget = null;
			}
		}
	}

	public Vector2 MousePosition { get; private set; }

	private bool IsDragging => DraggedWidget != null;

	public float DeltaMouseScroll => InputContext.GetDeltaMouseScroll() * 0.4f;

	public float RightStickVerticalScrollAmount
	{
		get
		{
			float y = Input.GetKeyState(InputKey.ControllerRStick).Y;
			return 3000f * y * 0.4f * CachedDt;
		}
	}

	public float RightStickHorizontalScrollAmount
	{
		get
		{
			float x = Input.GetKeyState(InputKey.ControllerRStick).X;
			return 3000f * x * 0.4f * CachedDt;
		}
	}

	internal float CachedDt { get; private set; }

	public event Action OnDragStarted;

	public event Action OnDragEnded;

	public event Action LoseFocus;

	public event Action GainFocus;

	internal EventManager(UIContext context)
	{
		Context = context;
		Root = new Widget(context)
		{
			Id = "Root"
		};
		if (UIEventManager == null)
		{
			UIEventManager = new TaleWorlds.Library.EventSystem.EventManager();
		}
		_widgetsWithUpdateContainer = new WidgetContainer(context, 64, WidgetContainer.ContainerType.Update);
		_widgetsWithParallelUpdateContainer = new WidgetContainer(context, 64, WidgetContainer.ContainerType.ParallelUpdate);
		_widgetsWithLateUpdateContainer = new WidgetContainer(context, 64, WidgetContainer.ContainerType.LateUpdate);
		_widgetsWithTweenPositionsContainer = new WidgetContainer(context, 64, WidgetContainer.ContainerType.TweenPosition);
		_widgetsWithVisualDefinitionsContainer = new WidgetContainer(context, 64, WidgetContainer.ContainerType.VisualDefinition);
		_widgetsWithUpdateBrushesContainer = new WidgetContainer(context, 64, WidgetContainer.ContainerType.UpdateBrushes);
		_lateUpdateActionLocker = new object();
		_lateUpdateActions = new Dictionary<int, List<UpdateAction>>();
		_lateUpdateActionsRunning = new Dictionary<int, List<UpdateAction>>();
		_onAfterFinalizedCallbacks = new List<Action>();
		for (int i = 1; i <= 5; i++)
		{
			_lateUpdateActions.Add(i, new List<UpdateAction>(32));
			_lateUpdateActionsRunning.Add(i, new List<UpdateAction>(32));
		}
		_drawContext = new TwoDimensionDrawContext();
		MouseOveredViews = new List<Widget>();
		ParallelUpdateWidgetPredicate = ParallelUpdateWidget;
		WidgetDoTweenPositionAuxPredicate = WidgetDoTweenPositionAux;
		UpdateBrushesWidgetPredicate = UpdateBrushesWidget;
		IsControllerActive = Input.IsControllerConnected && !Input.IsMouseActive;
	}

	internal void OnFinalize()
	{
		if (!_lastSetFrictionValue.ApproximatelyEqualsTo(1f))
		{
			_lastSetFrictionValue = 1f;
			Input.SetCursorFriction(_lastSetFrictionValue);
		}
		_widgetsWithLateUpdateContainer.Clear();
		_widgetsWithParallelUpdateContainer.Clear();
		_widgetsWithTweenPositionsContainer.Clear();
		_widgetsWithUpdateBrushesContainer.Clear();
		_widgetsWithUpdateContainer.Clear();
		_widgetsWithVisualDefinitionsContainer.Clear();
		for (int i = 0; i < _onAfterFinalizedCallbacks.Count; i++)
		{
			_onAfterFinalizedCallbacks[i]?.Invoke();
		}
		_onAfterFinalizedCallbacks.Clear();
		_onAfterFinalizedCallbacks = null;
		_widgetsWithLateUpdateContainer = null;
		_widgetsWithParallelUpdateContainer = null;
		_widgetsWithTweenPositionsContainer = null;
		_widgetsWithUpdateBrushesContainer = null;
		_widgetsWithUpdateContainer = null;
		_widgetsWithVisualDefinitionsContainer = null;
	}

	public void AddAfterFinalizedCallback(Action callback)
	{
		_onAfterFinalizedCallbacks.Add(callback);
	}

	internal void OnWidgetConnectedToRoot(Widget widget)
	{
		widget.HandleOnConnectedToRoot();
		foreach (Widget allChildrenAndThi in widget.AllChildrenAndThis)
		{
			allChildrenAndThi.HandleOnConnectedToRoot();
			RegisterWidgetForEvent(WidgetContainer.ContainerType.Update, allChildrenAndThi);
			RegisterWidgetForEvent(WidgetContainer.ContainerType.LateUpdate, allChildrenAndThi);
			RegisterWidgetForEvent(WidgetContainer.ContainerType.UpdateBrushes, allChildrenAndThi);
			RegisterWidgetForEvent(WidgetContainer.ContainerType.ParallelUpdate, allChildrenAndThi);
			RegisterWidgetForEvent(WidgetContainer.ContainerType.VisualDefinition, allChildrenAndThi);
			RegisterWidgetForEvent(WidgetContainer.ContainerType.TweenPosition, allChildrenAndThi);
		}
	}

	internal void OnWidgetDisconnectedFromRoot(Widget widget)
	{
		widget.HandleOnDisconnectedFromRoot();
		if (widget == DraggedWidget && DraggedWidget.DragWidget != null)
		{
			ReleaseDraggedWidget();
			ClearDragObject();
		}
		GauntletGamepadNavigationManager.Instance.OnWidgetDisconnectedFromRoot(widget);
		foreach (Widget allChildrenAndThi in widget.AllChildrenAndThis)
		{
			allChildrenAndThi.HandleOnDisconnectedFromRoot();
			UnRegisterWidgetForEvent(WidgetContainer.ContainerType.Update, allChildrenAndThi);
			UnRegisterWidgetForEvent(WidgetContainer.ContainerType.LateUpdate, allChildrenAndThi);
			UnRegisterWidgetForEvent(WidgetContainer.ContainerType.UpdateBrushes, allChildrenAndThi);
			UnRegisterWidgetForEvent(WidgetContainer.ContainerType.ParallelUpdate, allChildrenAndThi);
			UnRegisterWidgetForEvent(WidgetContainer.ContainerType.VisualDefinition, allChildrenAndThi);
			UnRegisterWidgetForEvent(WidgetContainer.ContainerType.TweenPosition, allChildrenAndThi);
			GauntletGamepadNavigationManager.Instance.OnWidgetDisconnectedFromRoot(allChildrenAndThi);
			allChildrenAndThi.GamepadNavigationIndex = -1;
			allChildrenAndThi.UsedNavigationMovements = GamepadNavigationTypes.None;
			allChildrenAndThi.IsUsingNavigation = false;
		}
	}

	internal void RegisterWidgetForEvent(WidgetContainer.ContainerType type, Widget widget)
	{
		switch (type)
		{
		case WidgetContainer.ContainerType.Update:
			lock (_widgetsWithUpdateContainer)
			{
				if (widget.WidgetInfo.GotCustomUpdate && widget.OnUpdateListIndex < 0)
				{
					widget.OnUpdateListIndex = _widgetsWithUpdateContainer.Add(widget);
				}
				break;
			}
		case WidgetContainer.ContainerType.ParallelUpdate:
			lock (_widgetsWithParallelUpdateContainer)
			{
				if (widget.WidgetInfo.GotCustomParallelUpdate && widget.OnParallelUpdateListIndex < 0)
				{
					widget.OnParallelUpdateListIndex = _widgetsWithParallelUpdateContainer.Add(widget);
				}
				break;
			}
		case WidgetContainer.ContainerType.LateUpdate:
			lock (_widgetsWithLateUpdateContainer)
			{
				if (widget.WidgetInfo.GotCustomLateUpdate && widget.OnLateUpdateListIndex < 0)
				{
					widget.OnLateUpdateListIndex = _widgetsWithLateUpdateContainer.Add(widget);
				}
				break;
			}
		case WidgetContainer.ContainerType.VisualDefinition:
			lock (_widgetsWithVisualDefinitionsContainer)
			{
				if (widget.VisualDefinition != null && widget.OnVisualDefinitionListIndex < 0)
				{
					widget.OnVisualDefinitionListIndex = _widgetsWithVisualDefinitionsContainer.Add(widget);
				}
				break;
			}
		case WidgetContainer.ContainerType.TweenPosition:
			lock (_widgetsWithTweenPositionsContainer)
			{
				if (widget.TweenPosition && widget.OnTweenPositionListIndex < 0)
				{
					widget.OnTweenPositionListIndex = _widgetsWithTweenPositionsContainer.Add(widget);
				}
				break;
			}
		case WidgetContainer.ContainerType.UpdateBrushes:
			lock (_widgetsWithUpdateBrushesContainer)
			{
				if (widget.WidgetInfo.GotUpdateBrushes && widget.OnUpdateBrushesIndex < 0)
				{
					widget.OnUpdateBrushesIndex = _widgetsWithUpdateBrushesContainer.Add(widget);
				}
				break;
			}
		case WidgetContainer.ContainerType.None:
			break;
		}
	}

	internal void UnRegisterWidgetForEvent(WidgetContainer.ContainerType type, Widget widget)
	{
		switch (type)
		{
		case WidgetContainer.ContainerType.Update:
			lock (_widgetsWithUpdateContainer)
			{
				if (widget.WidgetInfo.GotCustomUpdate && widget.OnUpdateListIndex != -1)
				{
					_widgetsWithUpdateContainer.RemoveFromIndex(widget.OnUpdateListIndex);
					widget.OnUpdateListIndex = -1;
				}
				break;
			}
		case WidgetContainer.ContainerType.ParallelUpdate:
			lock (_widgetsWithParallelUpdateContainer)
			{
				if (widget.WidgetInfo.GotCustomParallelUpdate && widget.OnParallelUpdateListIndex != -1)
				{
					_widgetsWithParallelUpdateContainer.RemoveFromIndex(widget.OnParallelUpdateListIndex);
					widget.OnParallelUpdateListIndex = -1;
				}
				break;
			}
		case WidgetContainer.ContainerType.LateUpdate:
			lock (_widgetsWithLateUpdateContainer)
			{
				if (widget.WidgetInfo.GotCustomLateUpdate && widget.OnLateUpdateListIndex != -1)
				{
					_widgetsWithLateUpdateContainer.RemoveFromIndex(widget.OnLateUpdateListIndex);
					widget.OnLateUpdateListIndex = -1;
				}
				break;
			}
		case WidgetContainer.ContainerType.VisualDefinition:
			lock (_widgetsWithVisualDefinitionsContainer)
			{
				if (widget.VisualDefinition != null && widget.OnVisualDefinitionListIndex != -1)
				{
					_widgetsWithVisualDefinitionsContainer.RemoveFromIndex(widget.OnVisualDefinitionListIndex);
					widget.OnVisualDefinitionListIndex = -1;
				}
				break;
			}
		case WidgetContainer.ContainerType.TweenPosition:
			lock (_widgetsWithTweenPositionsContainer)
			{
				if (widget.TweenPosition && widget.OnTweenPositionListIndex != -1)
				{
					_widgetsWithTweenPositionsContainer.RemoveFromIndex(widget.OnTweenPositionListIndex);
					widget.OnTweenPositionListIndex = -1;
				}
				break;
			}
		case WidgetContainer.ContainerType.UpdateBrushes:
			lock (_widgetsWithUpdateBrushesContainer)
			{
				if (widget.WidgetInfo.GotUpdateBrushes && widget.OnUpdateBrushesIndex != -1)
				{
					_widgetsWithUpdateBrushesContainer.RemoveFromIndex(widget.OnUpdateBrushesIndex);
					widget.OnUpdateBrushesIndex = -1;
				}
				break;
			}
		case WidgetContainer.ContainerType.None:
			break;
		}
	}

	internal void OnWidgetVisualDefinitionChanged(Widget widget)
	{
		if (widget.VisualDefinition != null)
		{
			RegisterWidgetForEvent(WidgetContainer.ContainerType.VisualDefinition, widget);
		}
		else
		{
			UnRegisterWidgetForEvent(WidgetContainer.ContainerType.VisualDefinition, widget);
		}
	}

	internal void OnWidgetTweenPositionChanged(Widget widget)
	{
		if (widget.TweenPosition)
		{
			RegisterWidgetForEvent(WidgetContainer.ContainerType.TweenPosition, widget);
		}
		else
		{
			UnRegisterWidgetForEvent(WidgetContainer.ContainerType.TweenPosition, widget);
		}
	}

	private void MeasureAll()
	{
		Root.Measure(PageSize);
	}

	private void LayoutAll(float left, float bottom, float right, float top)
	{
		Root.Layout(left, bottom, right, top);
	}

	private void UpdatePositions()
	{
		Root.UpdatePosition(Vector2.Zero);
	}

	private void WidgetDoTweenPositionAux(int startInclusive, int endExclusive, float deltaTime)
	{
		List<Widget> currentList = _widgetsWithParallelUpdateContainer.GetCurrentList();
		for (int i = startInclusive; i < endExclusive; i++)
		{
			currentList[i].DoTweenPosition(deltaTime);
		}
	}

	private void ParallelDoTweenPositions(float dt)
	{
		TWParallel.For(0, _widgetsWithTweenPositionsContainer.Count, dt, WidgetDoTweenPositionAuxPredicate);
	}

	private void TweenPositions(float dt)
	{
		if (_widgetsWithTweenPositionsContainer.CheckFragmentation())
		{
			lock (_widgetsWithTweenPositionsContainer)
			{
				_widgetsWithTweenPositionsContainer.DoDefragmentation();
			}
		}
		if (_widgetsWithTweenPositionsContainer.Count > 64)
		{
			ParallelDoTweenPositions(dt);
			return;
		}
		List<Widget> currentList = _widgetsWithTweenPositionsContainer.GetCurrentList();
		for (int i = 0; i < currentList.Count; i++)
		{
			currentList[i].DoTweenPosition(dt);
		}
	}

	internal void CalculateCanvas(Vector2 pageSize, float dt)
	{
		if (_measureDirty > 0 || _layoutDirty > 0)
		{
			PageSize = pageSize;
			Vec2 vec = new Vec2(pageSize.X / UsableArea.X, pageSize.Y / UsableArea.Y);
			LeftUsableAreaStart = (vec.X - vec.X * UsableArea.X) / 2f;
			TopUsableAreaStart = (vec.Y - vec.Y * UsableArea.Y) / 2f;
			if (_measureDirty > 0)
			{
				MeasureAll();
			}
			LayoutAll(LeftUsableAreaStart, PageSize.Y, PageSize.X, TopUsableAreaStart);
			TweenPositions(dt);
			UpdatePositions();
			if (_measureDirty > 0)
			{
				_measureDirty--;
			}
			if (_layoutDirty > 0)
			{
				_layoutDirty--;
			}
			_positionsDirty = false;
		}
	}

	internal void RecalculateCanvas()
	{
		if (_measureDirty == 2 || _layoutDirty == 2)
		{
			if (_measureDirty == 2)
			{
				MeasureAll();
			}
			LayoutAll(LeftUsableAreaStart, PageSize.Y, PageSize.X, TopUsableAreaStart);
			if (_positionsDirty)
			{
				UpdatePositions();
				_positionsDirty = false;
			}
		}
	}

	internal void MouseDown()
	{
		_mouseIsDown = true;
		_lastClickPosition = new Vector2(InputContext.GetPointerX(), InputContext.GetPointerY());
		Widget widgetAtPositionForEvent = GetWidgetAtPositionForEvent(GauntletEvent.MousePressed, _lastClickPosition);
		if (widgetAtPositionForEvent != null)
		{
			DispatchEvent(widgetAtPositionForEvent, GauntletEvent.MousePressed);
		}
	}

	internal void MouseUp()
	{
		_mouseIsDown = false;
		MousePosition = new Vector2(InputContext.GetPointerX(), InputContext.GetPointerY());
		if (IsDragging)
		{
			if (DraggedWidget.PreviewEvent(GauntletEvent.DragEnd))
			{
				DispatchEvent(DraggedWidget, GauntletEvent.DragEnd);
			}
			Widget widgetAtPositionForEvent = GetWidgetAtPositionForEvent(GauntletEvent.Drop, MousePosition);
			if (widgetAtPositionForEvent != null)
			{
				DispatchEvent(widgetAtPositionForEvent, GauntletEvent.Drop);
			}
			else
			{
				CancelAndReturnDrag();
			}
			if (DraggedWidget != null)
			{
				ClearDragObject();
			}
		}
		else
		{
			Widget widgetAtPositionForEvent2 = GetWidgetAtPositionForEvent(GauntletEvent.MouseReleased, new Vector2(InputContext.GetPointerX(), InputContext.GetPointerY()));
			DispatchEvent(widgetAtPositionForEvent2, GauntletEvent.MouseReleased);
			LatestMouseUpWidget = widgetAtPositionForEvent2;
		}
	}

	internal void MouseAlternateDown()
	{
		_mouseAlternateIsDown = true;
		_lastAlternateClickPosition = new Vector2(InputContext.GetPointerX(), InputContext.GetPointerY());
		Widget widgetAtPositionForEvent = GetWidgetAtPositionForEvent(GauntletEvent.MouseAlternatePressed, _lastAlternateClickPosition);
		if (widgetAtPositionForEvent != null)
		{
			DispatchEvent(widgetAtPositionForEvent, GauntletEvent.MouseAlternatePressed);
		}
	}

	internal void MouseAlternateUp()
	{
		_mouseAlternateIsDown = false;
		MousePosition = new Vector2(InputContext.GetPointerX(), InputContext.GetPointerY());
		Widget widgetAtPositionForEvent = GetWidgetAtPositionForEvent(GauntletEvent.MouseAlternateReleased, _lastAlternateClickPosition);
		DispatchEvent(widgetAtPositionForEvent, GauntletEvent.MouseAlternateReleased);
		LatestMouseAlternateUpWidget = widgetAtPositionForEvent;
	}

	internal void MouseScroll()
	{
		if (TaleWorlds.Library.MathF.Abs(DeltaMouseScroll) > 0.001f)
		{
			Widget widgetAtPositionForEvent = GetWidgetAtPositionForEvent(GauntletEvent.MouseScroll, MousePosition);
			if (widgetAtPositionForEvent != null)
			{
				DispatchEvent(widgetAtPositionForEvent, GauntletEvent.MouseScroll);
			}
		}
	}

	internal void RightStickMovement()
	{
		if (Input.GetKeyState(InputKey.ControllerRStick).X != 0f || Input.GetKeyState(InputKey.ControllerRStick).Y != 0f)
		{
			Widget widgetAtPositionForEvent = GetWidgetAtPositionForEvent(GauntletEvent.RightStickMovement, MousePosition);
			if (widgetAtPositionForEvent != null)
			{
				DispatchEvent(widgetAtPositionForEvent, GauntletEvent.RightStickMovement);
			}
		}
	}

	public void ClearFocus()
	{
		SetWidgetFocused(null);
		SetHoveredView(null);
	}

	private void CancelAndReturnDrag()
	{
		if (_draggedWidgetPreviousParent != null)
		{
			DraggedWidget.ParentWidget = _draggedWidgetPreviousParent;
			DraggedWidget.SetSiblingIndex(_draggedWidgetIndex);
			DraggedWidget.PosOffset = new Vector2(0f, 0f);
			if (DraggedWidget.DragWidget != null)
			{
				DraggedWidget.DragWidget.ParentWidget = DraggedWidget;
				DraggedWidget.DragWidget.IsVisible = false;
			}
		}
		else
		{
			ReleaseDraggedWidget();
		}
		_draggedWidgetPreviousParent = null;
		_draggedWidgetIndex = -1;
	}

	private void ClearDragObject()
	{
		DraggedWidget = null;
		this.OnDragEnded?.Invoke();
		_dragOffset = new Vector2(0f, 0f);
		_dragCarrier.ParentWidget = null;
		_dragCarrier = null;
	}

	internal void UpdateMousePosition(Vector2 mousePos)
	{
		MousePosition = mousePos;
	}

	internal void MouseMove()
	{
		if (_mouseIsDown)
		{
			if (IsDragging)
			{
				Widget widgetAtPositionForEvent = GetWidgetAtPositionForEvent(GauntletEvent.DragHover, MousePosition);
				if (widgetAtPositionForEvent != null)
				{
					DispatchEvent(widgetAtPositionForEvent, GauntletEvent.DragHover);
				}
				else
				{
					SetDragHoveredView(null);
				}
			}
			else if (LatestMouseDownWidget != null)
			{
				if (LatestMouseDownWidget.PreviewEvent(GauntletEvent.MouseMove))
				{
					DispatchEvent(LatestMouseDownWidget, GauntletEvent.MouseMove);
				}
				if (!IsDragging && LatestMouseDownWidget.PreviewEvent(GauntletEvent.DragBegin))
				{
					Vector2 vector = _lastClickPosition - MousePosition;
					if (new Vector2(vector.X, vector.Y).LengthSquared() > 100f * Context.Scale)
					{
						DispatchEvent(LatestMouseDownWidget, GauntletEvent.DragBegin);
					}
				}
			}
		}
		else if (!_mouseAlternateIsDown)
		{
			Widget widgetAtPositionForEvent2 = GetWidgetAtPositionForEvent(GauntletEvent.MouseMove, MousePosition);
			if (widgetAtPositionForEvent2 != null)
			{
				DispatchEvent(widgetAtPositionForEvent2, GauntletEvent.MouseMove);
			}
		}
		List<Widget> list = new List<Widget>();
		foreach (Widget item in AllVisibleWidgetsAt(Root, MousePosition))
		{
			if (!MouseOveredViews.Contains(item))
			{
				item.OnMouseOverBegin();
				GauntletGamepadNavigationManager.Instance?.OnWidgetHoverBegin(item);
			}
			list.Add(item);
		}
		foreach (Widget item2 in MouseOveredViews.Except(list))
		{
			item2.OnMouseOverEnd();
			if (item2.GamepadNavigationIndex != -1)
			{
				GauntletGamepadNavigationManager.Instance?.OnWidgetHoverEnd(item2);
			}
		}
		MouseOveredViews = list;
	}

	private static bool IsPointInsideMeasuredArea(Widget w, Vector2 p)
	{
		return w.IsPointInsideMeasuredArea(p);
	}

	private Widget GetWidgetAtPositionForEvent(GauntletEvent gauntletEvent, Vector2 pointerPosition)
	{
		Widget result = null;
		foreach (Widget item in AllEnabledWidgetsAt(Root, pointerPosition))
		{
			if (item.PreviewEvent(gauntletEvent))
			{
				result = item;
				break;
			}
		}
		return result;
	}

	private void DispatchEvent(Widget selectedWidget, GauntletEvent gauntletEvent)
	{
		if (gauntletEvent != GauntletEvent.MouseReleased)
		{
			_ = 4;
		}
		switch (gauntletEvent)
		{
		case GauntletEvent.MousePressed:
			LatestMouseDownWidget = selectedWidget;
			selectedWidget.OnMousePressed();
			if (FocusedWidget == selectedWidget)
			{
				break;
			}
			if (FocusedWidget != null)
			{
				FocusedWidget.OnLoseFocus();
				this.LoseFocus?.Invoke();
			}
			if (selectedWidget.IsFocusable)
			{
				selectedWidget.OnGainFocus();
				FocusedWidget = selectedWidget;
				this.GainFocus?.Invoke();
			}
			else
			{
				FocusedWidget = null;
			}
			if (selectedWidget is EditableTextWidget editableTextWidget2 && IsControllerActive)
			{
				string initialText2 = editableTextWidget2.Text ?? string.Empty;
				string descriptionText2 = editableTextWidget2.KeyboardInfoText ?? string.Empty;
				int maxLength2 = editableTextWidget2.MaxLength;
				int keyboardTypeEnum2 = (editableTextWidget2.IsObfuscationEnabled ? 2 : 0);
				if (FocusedWidget is IntegerInputTextWidget || FocusedWidget is FloatInputTextWidget)
				{
					keyboardTypeEnum2 = 1;
				}
				Context.TwoDimensionContext.Platform.OpenOnScreenKeyboard(initialText2, descriptionText2, maxLength2, keyboardTypeEnum2);
			}
			break;
		case GauntletEvent.MouseReleased:
			if (LatestMouseDownWidget != null && LatestMouseDownWidget != selectedWidget)
			{
				LatestMouseDownWidget.OnMouseReleased();
			}
			selectedWidget?.OnMouseReleased();
			break;
		case GauntletEvent.MouseAlternatePressed:
			LatestMouseAlternateDownWidget = selectedWidget;
			selectedWidget.OnMouseAlternatePressed();
			if (FocusedWidget == selectedWidget)
			{
				break;
			}
			if (FocusedWidget != null)
			{
				FocusedWidget.OnLoseFocus();
				this.LoseFocus?.Invoke();
			}
			if (selectedWidget.IsFocusable)
			{
				selectedWidget.OnGainFocus();
				FocusedWidget = selectedWidget;
				this.GainFocus?.Invoke();
			}
			else
			{
				FocusedWidget = null;
			}
			if (selectedWidget is EditableTextWidget editableTextWidget && IsControllerActive)
			{
				string initialText = editableTextWidget.Text ?? string.Empty;
				string descriptionText = editableTextWidget.KeyboardInfoText ?? string.Empty;
				int maxLength = editableTextWidget.MaxLength;
				int keyboardTypeEnum = (editableTextWidget.IsObfuscationEnabled ? 2 : 0);
				if (FocusedWidget is IntegerInputTextWidget || FocusedWidget is FloatInputTextWidget)
				{
					keyboardTypeEnum = 1;
				}
				Context.TwoDimensionContext.Platform.OpenOnScreenKeyboard(initialText, descriptionText, maxLength, keyboardTypeEnum);
			}
			break;
		case GauntletEvent.MouseAlternateReleased:
			if (LatestMouseAlternateDownWidget != null && LatestMouseAlternateDownWidget != selectedWidget)
			{
				LatestMouseAlternateDownWidget.OnMouseAlternateReleased();
			}
			selectedWidget?.OnMouseAlternateReleased();
			break;
		case GauntletEvent.MouseMove:
			selectedWidget.OnMouseMove();
			SetHoveredView(selectedWidget);
			break;
		case GauntletEvent.DragHover:
			SetDragHoveredView(selectedWidget);
			break;
		case GauntletEvent.DragBegin:
			selectedWidget.OnDragBegin();
			break;
		case GauntletEvent.DragEnd:
			selectedWidget.OnDragEnd();
			break;
		case GauntletEvent.Drop:
			selectedWidget.OnDrop();
			break;
		case GauntletEvent.MouseScroll:
			selectedWidget.OnMouseScroll();
			break;
		case GauntletEvent.RightStickMovement:
			selectedWidget.OnRightStickMovement();
			break;
		}
	}

	public static bool HitTest(Widget widget, Vector2 position)
	{
		if (widget == null)
		{
			Debug.FailedAssert("Calling HitTest using null widget!", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\EventManager.cs", "HitTest", 1141);
			return false;
		}
		return AnyWidgetsAt(widget, position);
	}

	public bool FocusTest(Widget root)
	{
		for (Widget widget = FocusedWidget; widget != null; widget = widget.ParentWidget)
		{
			if (root == widget)
			{
				return true;
			}
		}
		return false;
	}

	private static bool AnyWidgetsAt(Widget widget, Vector2 position)
	{
		if (widget.IsEnabled && widget.IsVisible)
		{
			if (!widget.DoNotAcceptEvents && IsPointInsideMeasuredArea(widget, position))
			{
				return true;
			}
			if (!widget.DoNotPassEventsToChildren)
			{
				for (int num = widget.ChildCount - 1; num >= 0; num--)
				{
					Widget child = widget.GetChild(num);
					if (!child.IsHidden && !child.IsDisabled && AnyWidgetsAt(child, position))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private static IEnumerable<Widget> AllEnabledWidgetsAt(Widget widget, Vector2 position)
	{
		if (!widget.IsEnabled || !widget.IsVisible)
		{
			yield break;
		}
		if (!widget.DoNotPassEventsToChildren)
		{
			for (int i = widget.ChildCount - 1; i >= 0; i--)
			{
				Widget child = widget.GetChild(i);
				if (!child.IsHidden && !child.IsDisabled && IsPointInsideMeasuredArea(child, position))
				{
					foreach (Widget item in AllEnabledWidgetsAt(child, position))
					{
						yield return item;
					}
				}
			}
		}
		if (!widget.DoNotAcceptEvents && IsPointInsideMeasuredArea(widget, position))
		{
			yield return widget;
		}
	}

	private static IEnumerable<Widget> AllVisibleWidgetsAt(Widget widget, Vector2 position)
	{
		if (!widget.IsVisible)
		{
			yield break;
		}
		for (int i = widget.ChildCount - 1; i >= 0; i--)
		{
			Widget child = widget.GetChild(i);
			if (child.IsVisible && IsPointInsideMeasuredArea(child, position))
			{
				foreach (Widget item in AllVisibleWidgetsAt(child, position))
				{
					yield return item;
				}
			}
		}
		if (IsPointInsideMeasuredArea(widget, position))
		{
			yield return widget;
		}
	}

	internal void ManualAddRange(List<Widget> list, LinkedList<Widget> linked_list)
	{
		if (list.Capacity < linked_list.Count)
		{
			list.Capacity = linked_list.Count;
		}
		for (LinkedListNode<Widget> linkedListNode = linked_list.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			list.Add(linkedListNode.Value);
		}
	}

	private void ParallelUpdateWidget(int startInclusive, int endExclusive, float dt)
	{
		List<Widget> currentList = _widgetsWithParallelUpdateContainer.GetCurrentList();
		for (int i = startInclusive; i < endExclusive; i++)
		{
			currentList[i].ParallelUpdate(dt);
		}
	}

	internal void ParallelUpdateWidgets(float dt)
	{
		TWParallel.For(0, _widgetsWithParallelUpdateContainer.Count, dt, ParallelUpdateWidgetPredicate);
	}

	internal void Update(float dt)
	{
		Time += dt;
		CachedDt = dt;
		IsControllerActive = Input.IsControllerConnected && !Input.IsMouseActive;
		int realCount = _widgetsWithUpdateContainer.RealCount;
		int realCount2 = _widgetsWithParallelUpdateContainer.RealCount;
		int realCount3 = _widgetsWithLateUpdateContainer.RealCount;
		int num = TaleWorlds.Library.MathF.Max(_widgetsWithUpdateBrushesContainer.RealCount, TaleWorlds.Library.MathF.Max(realCount, TaleWorlds.Library.MathF.Max(realCount2, realCount3)));
		if (_widgetsWithUpdateContainerDoDefragmentationDelegate == null)
		{
			_widgetsWithUpdateContainerDoDefragmentationDelegate = delegate
			{
				lock (_widgetsWithUpdateContainer)
				{
					_widgetsWithUpdateContainer.DoDefragmentation();
				}
			};
			_widgetsWithParallelUpdateContainerDoDefragmentationDelegate = delegate
			{
				lock (_widgetsWithParallelUpdateContainer)
				{
					_widgetsWithParallelUpdateContainer.DoDefragmentation();
				}
			};
			_widgetsWithLateUpdateContainerDoDefragmentationDelegate = delegate
			{
				lock (_widgetsWithLateUpdateContainer)
				{
					_widgetsWithLateUpdateContainer.DoDefragmentation();
				}
			};
			_widgetsWithUpdateBrushesContainerDoDefragmentationDelegate = delegate
			{
				lock (_widgetsWithUpdateBrushesContainer)
				{
					_widgetsWithUpdateBrushesContainer.DoDefragmentation();
				}
			};
		}
		bool flag = _widgetsWithUpdateContainer.CheckFragmentation() || _widgetsWithParallelUpdateContainer.CheckFragmentation() || _widgetsWithLateUpdateContainer.CheckFragmentation() || _widgetsWithUpdateBrushesContainer.CheckFragmentation();
		Task task = null;
		Task task2 = null;
		Task task3 = null;
		Task task4 = null;
		if (flag && num > 64)
		{
			task = Task.Run(_widgetsWithUpdateContainerDoDefragmentationDelegate);
			task2 = Task.Run(_widgetsWithParallelUpdateContainerDoDefragmentationDelegate);
			task3 = Task.Run(_widgetsWithLateUpdateContainerDoDefragmentationDelegate);
			task4 = Task.Run(_widgetsWithUpdateBrushesContainerDoDefragmentationDelegate);
		}
		UpdateDragCarrier();
		if (_widgetsWithVisualDefinitionsContainer.CheckFragmentation())
		{
			lock (_widgetsWithVisualDefinitionsContainer)
			{
				_widgetsWithVisualDefinitionsContainer.DoDefragmentation();
			}
		}
		List<Widget> currentList = _widgetsWithVisualDefinitionsContainer.GetCurrentList();
		for (int i = 0; i < currentList.Count; i++)
		{
			currentList[i].UpdateVisualDefinitions(dt);
		}
		if (flag)
		{
			if (num > 64)
			{
				Task.WaitAll(task, task2, task3, task4);
			}
			else
			{
				_widgetsWithUpdateContainerDoDefragmentationDelegate();
				_widgetsWithParallelUpdateContainerDoDefragmentationDelegate();
				_widgetsWithLateUpdateContainerDoDefragmentationDelegate();
				_widgetsWithUpdateBrushesContainerDoDefragmentationDelegate();
			}
		}
		UIContext.MouseCursors activeCursorOfContext = ((HoveredView?.HoveredCursorState == null) ? UIContext.MouseCursors.Default : ((UIContext.MouseCursors)Enum.Parse(typeof(UIContext.MouseCursors), HoveredView.HoveredCursorState)));
		Context.ActiveCursorOfContext = activeCursorOfContext;
		List<Widget> currentList2 = _widgetsWithUpdateContainer.GetCurrentList();
		for (int j = 0; j < currentList2.Count; j++)
		{
			currentList2[j].Update(dt);
		}
		_doingParallelTask = true;
		if (_widgetsWithParallelUpdateContainer.Count > 64)
		{
			ParallelUpdateWidgets(dt);
		}
		else
		{
			List<Widget> currentList3 = _widgetsWithParallelUpdateContainer.GetCurrentList();
			for (int k = 0; k < currentList3.Count; k++)
			{
				currentList3[k].ParallelUpdate(dt);
			}
		}
		_doingParallelTask = false;
	}

	internal void ParallelUpdateBrushes(float dt)
	{
		TWParallel.For(0, _widgetsWithUpdateBrushesContainer.Count, dt, UpdateBrushesWidgetPredicate);
	}

	internal void UpdateBrushes(float dt)
	{
		if (_widgetsWithUpdateBrushesContainer.Count > 64)
		{
			ParallelUpdateBrushes(dt);
			return;
		}
		List<Widget> currentList = _widgetsWithUpdateBrushesContainer.GetCurrentList();
		for (int i = 0; i < currentList.Count; i++)
		{
			currentList[i].UpdateBrushes(dt);
		}
	}

	private void UpdateBrushesWidget(int startInclusive, int endExclusive, float dt)
	{
		List<Widget> currentList = _widgetsWithUpdateBrushesContainer.GetCurrentList();
		for (int i = startInclusive; i < endExclusive; i++)
		{
			currentList[i].UpdateBrushes(dt);
		}
	}

	public void AddLateUpdateAction(Widget owner, Action<float> action, int order)
	{
		UpdateAction item = default(UpdateAction);
		item.Target = owner;
		item.Action = action;
		item.Order = order;
		if (_doingParallelTask)
		{
			lock (_lateUpdateActionLocker)
			{
				_lateUpdateActions[order].Add(item);
				return;
			}
		}
		_lateUpdateActions[order].Add(item);
	}

	internal void LateUpdate(float dt)
	{
		List<Widget> currentList = _widgetsWithLateUpdateContainer.GetCurrentList();
		for (int i = 0; i < currentList.Count; i++)
		{
			currentList[i].LateUpdate(dt);
		}
		Dictionary<int, List<UpdateAction>> lateUpdateActions = _lateUpdateActions;
		_lateUpdateActions = _lateUpdateActionsRunning;
		_lateUpdateActionsRunning = lateUpdateActions;
		for (int j = 1; j <= 5; j++)
		{
			List<UpdateAction> list = _lateUpdateActionsRunning[j];
			foreach (UpdateAction item in list)
			{
				item.Action(dt);
			}
			list.Clear();
		}
		if (!IsControllerActive)
		{
			return;
		}
		if (HoveredView != null && HoveredView.IsRecursivelyVisible())
		{
			if (HoveredView.FrictionEnabled && DraggedWidget == null)
			{
				_lastSetFrictionValue = 0.45f;
			}
			else
			{
				_lastSetFrictionValue = 1f;
			}
			Input.SetCursorFriction(_lastSetFrictionValue);
		}
		if (!_lastSetFrictionValue.ApproximatelyEqualsTo(1f) && HoveredView == null)
		{
			_lastSetFrictionValue = 1f;
			Input.SetCursorFriction(_lastSetFrictionValue);
		}
	}

	public void SetWidgetFocused(Widget widget, bool fromClick = false)
	{
		if (FocusedWidget == widget)
		{
			return;
		}
		FocusedWidget?.OnLoseFocus();
		widget?.OnGainFocus();
		FocusedWidget = widget;
		if (FocusedWidget is EditableTextWidget editableTextWidget && IsControllerActive)
		{
			string initialText = editableTextWidget.Text ?? string.Empty;
			string descriptionText = editableTextWidget.KeyboardInfoText ?? string.Empty;
			int maxLength = editableTextWidget.MaxLength;
			int keyboardTypeEnum = (editableTextWidget.IsObfuscationEnabled ? 2 : 0);
			if (FocusedWidget is IntegerInputTextWidget || FocusedWidget is FloatInputTextWidget)
			{
				keyboardTypeEnum = 1;
			}
			Context.TwoDimensionContext.Platform.OpenOnScreenKeyboard(initialText, descriptionText, maxLength, keyboardTypeEnum);
		}
	}

	private void UpdateDragCarrier()
	{
		if (_dragCarrier != null)
		{
			_dragCarrier.PosOffset = MousePositionInReferenceResolution + _dragOffset - new Vector2(LeftUsableAreaStart, TopUsableAreaStart) * Context.InverseScale;
		}
	}

	public void SetHoveredView(Widget view)
	{
		if (HoveredView != view)
		{
			if (HoveredView != null)
			{
				HoveredView.OnHoverEnd();
			}
			HoveredView = view;
			if (HoveredView != null)
			{
				HoveredView.OnHoverBegin();
			}
		}
	}

	internal bool SetDragHoveredView(Widget view)
	{
		if (DragHoveredView != view)
		{
			DragHoveredView?.OnDragHoverEnd();
		}
		DragHoveredView = view;
		if (DragHoveredView != null && DragHoveredView.AcceptDrop)
		{
			DragHoveredView.OnDragHoverBegin();
			return true;
		}
		DragHoveredView = null;
		return false;
	}

	internal void BeginDragging(Widget draggedObject)
	{
		if (DraggedWidget != null)
		{
			Debug.FailedAssert("Trying to BeginDragging while there is already a dragged object.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\EventManager.cs", "BeginDragging", 1628);
			ClearDragObject();
		}
		if (!draggedObject.ConnectedToRoot)
		{
			Debug.FailedAssert("Trying to drag a widget with no parent, possibly a widget which is already being dragged", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\EventManager.cs", "BeginDragging", 1634);
			return;
		}
		draggedObject.IsPressed = false;
		_draggedWidgetPreviousParent = null;
		_draggedWidgetIndex = -1;
		Widget parentWidget = draggedObject.ParentWidget;
		DraggedWidget = draggedObject;
		Vector2 globalPosition = DraggedWidget.GlobalPosition;
		_dragCarrier = new DragCarrierWidget(Context);
		_dragCarrier.ParentWidget = Root;
		if (draggedObject.DragWidget != null)
		{
			Widget dragWidget = draggedObject.DragWidget;
			_dragCarrier.WidthSizePolicy = SizePolicy.CoverChildren;
			_dragCarrier.HeightSizePolicy = SizePolicy.CoverChildren;
			_dragOffset = Vector2.Zero;
			dragWidget.IsVisible = true;
			dragWidget.ParentWidget = _dragCarrier;
			if (DraggedWidget.HideOnDrag)
			{
				DraggedWidget.IsVisible = false;
			}
			_draggedWidgetPreviousParent = null;
		}
		else
		{
			_dragOffset = (globalPosition - MousePosition) * Context.InverseScale;
			_dragCarrier.WidthSizePolicy = SizePolicy.Fixed;
			_dragCarrier.HeightSizePolicy = SizePolicy.Fixed;
			if (DraggedWidget.WidthSizePolicy == SizePolicy.StretchToParent)
			{
				_dragCarrier.ScaledSuggestedWidth = DraggedWidget.Size.X + (DraggedWidget.MarginRight + DraggedWidget.MarginLeft) * Context.Scale;
				_dragOffset += new Vector2(0f - DraggedWidget.MarginLeft, 0f);
			}
			else
			{
				_dragCarrier.ScaledSuggestedWidth = DraggedWidget.Size.X;
			}
			if (DraggedWidget.HeightSizePolicy == SizePolicy.StretchToParent)
			{
				_dragCarrier.ScaledSuggestedHeight = DraggedWidget.Size.Y + (DraggedWidget.MarginTop + DraggedWidget.MarginBottom) * Context.Scale;
				_dragOffset += new Vector2(0f, 0f - DraggedWidget.MarginTop);
			}
			else
			{
				_dragCarrier.ScaledSuggestedHeight = DraggedWidget.Size.Y;
			}
			if (parentWidget != null)
			{
				_draggedWidgetPreviousParent = parentWidget;
				_draggedWidgetIndex = draggedObject.GetSiblingIndex();
			}
			DraggedWidget.ParentWidget = _dragCarrier;
		}
		_dragCarrier.PosOffset = MousePositionInReferenceResolution + _dragOffset - new Vector2(LeftUsableAreaStart, TopUsableAreaStart) * Context.InverseScale;
		this.OnDragStarted?.Invoke();
	}

	internal Widget ReleaseDraggedWidget()
	{
		Widget draggedWidget = DraggedWidget;
		if (_draggedWidgetPreviousParent != null)
		{
			DraggedWidget.ParentWidget = _draggedWidgetPreviousParent;
			_draggedWidgetIndex = TaleWorlds.Library.MathF.Max(0, TaleWorlds.Library.MathF.Min(TaleWorlds.Library.MathF.Max(0, DraggedWidget.ParentWidget.ChildCount - 1), _draggedWidgetIndex));
			DraggedWidget.SetSiblingIndex(_draggedWidgetIndex);
		}
		else
		{
			DraggedWidget.IsVisible = true;
		}
		SetDragHoveredView(null);
		return draggedWidget;
	}

	internal void Render(TwoDimensionContext twoDimensionContext)
	{
		_drawContext.Reset();
		Root.Render(twoDimensionContext, _drawContext);
		_drawContext.DrawTo(twoDimensionContext);
	}

	public void UpdateLayout()
	{
		SetMeasureDirty();
		SetLayoutDirty();
	}

	internal void SetMeasureDirty()
	{
		_measureDirty = 2;
	}

	internal void SetLayoutDirty()
	{
		_layoutDirty = 2;
	}

	internal void SetPositionsDirty()
	{
		_positionsDirty = true;
	}

	public bool GetIsHitThisFrame()
	{
		return OnGetIsHitThisFrame();
	}
}
