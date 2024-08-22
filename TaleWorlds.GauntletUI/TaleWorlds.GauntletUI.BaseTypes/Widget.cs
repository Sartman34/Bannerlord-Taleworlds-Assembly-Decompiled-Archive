using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.BaseTypes;

public class Widget : PropertyOwnerObject
{
	private Color _color = Color.White;

	private string _id;

	protected Vector2 _cachedGlobalPosition;

	private Widget _parent;

	private List<Widget> _children;

	private bool _doNotUseCustomScaleAndChildren;

	protected bool _calculateSizeFirstFrame = true;

	private float _suggestedWidth;

	private float _suggestedHeight;

	private bool _tweenPosition;

	private string _hoveredCursorState;

	private bool _alternateClickEventHasSpecialEvent;

	private Vector2 _positionOffset;

	private float _marginTop;

	private float _marginLeft;

	private float _marginBottom;

	private float _marginRight;

	private VerticalAlignment _verticalAlignment;

	private HorizontalAlignment _horizontalAlignment;

	private Vector2 _topLeft;

	private bool _forcePixelPerfectRenderPlacement;

	private SizePolicy _widthSizePolicy;

	private SizePolicy _heightSizePolicy;

	private Widget _dragWidget;

	private bool _isHovered;

	private bool _isDisabled;

	private bool _isFocusable;

	private bool _isFocused;

	private bool _restartAnimationFirstFrame;

	private bool _doNotPassEventsToChildren;

	private bool _doNotAcceptEvents;

	public Func<Widget, Widget, bool> AcceptDropHandler;

	private bool _isPressed;

	private bool _isHidden;

	private Sprite _sprite;

	private VisualDefinition _visualDefinition;

	private List<string> _states;

	protected float _stateTimer;

	protected VisualState _startVisualState;

	protected VisualStateAnimationState _currentVisualStateAnimationState;

	private bool _updateChildrenStates;

	protected int _seed;

	private bool _seedSet;

	private float _maxWidth;

	private float _maxHeight;

	private float _minWidth;

	private float _minHeight;

	private bool _gotMaxWidth;

	private bool _gotMaxHeight;

	private bool _gotMinWidth;

	private bool _gotMinHeight;

	private List<WidgetComponent> _components;

	private bool _isInParallelOperation;

	private List<Action<Widget, string, object[]>> _eventTargets;

	private bool _doNotAcceptNavigation;

	private bool _isUsingNavigation;

	private bool _useSiblingIndexForNavigation;

	protected internal int _gamepadNavigationIndex = -1;

	private GamepadNavigationTypes _usedNavigationMovements;

	public Action<Widget> OnGamepadNavigationFocusGained;

	public float ColorFactor { get; set; } = 1f;


	public float AlphaFactor { get; set; } = 1f;


	public float ValueFactor { get; set; }

	public float SaturationFactor { get; set; }

	public float ExtendLeft { get; set; }

	public float ExtendRight { get; set; }

	public float ExtendTop { get; set; }

	public float ExtendBottom { get; set; }

	public bool VerticalFlip { get; set; }

	public bool HorizontalFlip { get; set; }

	public bool FrictionEnabled { get; set; }

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			if (_color != value)
			{
				_color = value;
			}
		}
	}

	[Editor(false)]
	public string Id
	{
		get
		{
			return _id;
		}
		set
		{
			if (_id != value)
			{
				_id = value;
				OnPropertyChanged(value, "Id");
			}
		}
	}

	public Vector2 LocalPosition { get; private set; }

	public Vector2 GlobalPosition
	{
		get
		{
			if (ParentWidget != null)
			{
				return LocalPosition + ParentWidget.GlobalPosition;
			}
			return LocalPosition;
		}
	}

	[Editor(false)]
	public bool DoNotUseCustomScaleAndChildren
	{
		get
		{
			return _doNotUseCustomScaleAndChildren;
		}
		set
		{
			if (_doNotUseCustomScaleAndChildren != value)
			{
				_doNotUseCustomScaleAndChildren = value;
				OnPropertyChanged(value, "DoNotUseCustomScaleAndChildren");
				DoNotUseCustomScale = value;
				_children.ForEach(delegate(Widget _)
				{
					_.DoNotUseCustomScaleAndChildren = value;
				});
			}
		}
	}

	public bool DoNotUseCustomScale { get; set; }

	protected float _scaleToUse
	{
		get
		{
			if (!DoNotUseCustomScale)
			{
				return Context.CustomScale;
			}
			return Context.Scale;
		}
	}

	protected float _inverseScaleToUse
	{
		get
		{
			if (!DoNotUseCustomScale)
			{
				return Context.CustomInverseScale;
			}
			return Context.InverseScale;
		}
	}

	public Vector2 Size { get; private set; }

	[Editor(false)]
	public float SuggestedWidth
	{
		get
		{
			return _suggestedWidth;
		}
		set
		{
			if (_suggestedWidth != value)
			{
				SetMeasureAndLayoutDirty();
				_suggestedWidth = value;
				OnPropertyChanged(value, "SuggestedWidth");
			}
		}
	}

	[Editor(false)]
	public float SuggestedHeight
	{
		get
		{
			return _suggestedHeight;
		}
		set
		{
			if (_suggestedHeight != value)
			{
				SetMeasureAndLayoutDirty();
				_suggestedHeight = value;
				OnPropertyChanged(value, "SuggestedHeight");
			}
		}
	}

	public float ScaledSuggestedWidth
	{
		get
		{
			return _scaleToUse * SuggestedWidth;
		}
		set
		{
			SuggestedWidth = value * _inverseScaleToUse;
		}
	}

	public float ScaledSuggestedHeight
	{
		get
		{
			return _scaleToUse * SuggestedHeight;
		}
		set
		{
			SuggestedHeight = value * _inverseScaleToUse;
		}
	}

	[Editor(false)]
	public bool TweenPosition
	{
		get
		{
			return _tweenPosition;
		}
		set
		{
			if (_tweenPosition != value)
			{
				bool tweenPosition = _tweenPosition;
				_tweenPosition = value;
				if (ConnectedToRoot && (!tweenPosition || !_tweenPosition))
				{
					EventManager.OnWidgetTweenPositionChanged(this);
				}
			}
		}
	}

	[Editor(false)]
	public string HoveredCursorState
	{
		get
		{
			return _hoveredCursorState;
		}
		set
		{
			if (_hoveredCursorState != value)
			{
				_ = _hoveredCursorState;
				_hoveredCursorState = value;
			}
		}
	}

	[Editor(false)]
	public bool AlternateClickEventHasSpecialEvent
	{
		get
		{
			return _alternateClickEventHasSpecialEvent;
		}
		set
		{
			if (_alternateClickEventHasSpecialEvent != value)
			{
				_ = _alternateClickEventHasSpecialEvent;
				_alternateClickEventHasSpecialEvent = value;
			}
		}
	}

	public Vector2 PosOffset
	{
		get
		{
			return _positionOffset;
		}
		set
		{
			if (_positionOffset != value)
			{
				SetLayoutDirty();
				_positionOffset = value;
				OnPropertyChanged(value, "PosOffset");
			}
		}
	}

	public Vector2 ScaledPositionOffset => _positionOffset * _scaleToUse;

	[Editor(false)]
	public float PositionXOffset
	{
		get
		{
			return _positionOffset.X;
		}
		set
		{
			if (_positionOffset.X != value)
			{
				SetLayoutDirty();
				_positionOffset.X = value;
				OnPropertyChanged(value, "PositionXOffset");
			}
		}
	}

	[Editor(false)]
	public float PositionYOffset
	{
		get
		{
			return _positionOffset.Y;
		}
		set
		{
			if (_positionOffset.Y != value)
			{
				SetLayoutDirty();
				_positionOffset.Y = value;
				OnPropertyChanged(value, "PositionYOffset");
			}
		}
	}

	public float ScaledPositionXOffset
	{
		get
		{
			return _positionOffset.X * _scaleToUse;
		}
		set
		{
			float num = value * _inverseScaleToUse;
			if (num != _positionOffset.X)
			{
				SetLayoutDirty();
				_positionOffset.X = num;
			}
		}
	}

	public float ScaledPositionYOffset
	{
		get
		{
			return _positionOffset.Y * _scaleToUse;
		}
		set
		{
			float num = value * _inverseScaleToUse;
			if (num != _positionOffset.Y)
			{
				SetLayoutDirty();
				_positionOffset.Y = num;
			}
		}
	}

	public Widget ParentWidget
	{
		get
		{
			return _parent;
		}
		set
		{
			if (ParentWidget == value)
			{
				return;
			}
			if (_parent != null)
			{
				_parent.OnChildRemoved(this);
				if (ConnectedToRoot)
				{
					EventManager.OnWidgetDisconnectedFromRoot(this);
				}
				_parent._children.Remove(this);
				_parent.OnAfterChildRemoved(this);
			}
			_parent = value;
			if (_parent != null)
			{
				_parent._children.Add(this);
				if (ConnectedToRoot)
				{
					EventManager.OnWidgetConnectedToRoot(this);
				}
				_parent.OnChildAdded(this);
			}
			SetMeasureAndLayoutDirty();
		}
	}

	public EventManager EventManager => Context.EventManager;

	public IGamepadNavigationContext GamepadNavigationContext => Context.GamepadNavigation;

	public UIContext Context { get; private set; }

	public Vector2 MeasuredSize { get; private set; }

	[Editor(false)]
	public float MarginTop
	{
		get
		{
			return _marginTop;
		}
		set
		{
			if (_marginTop != value)
			{
				SetMeasureAndLayoutDirty();
				_marginTop = value;
				OnPropertyChanged(value, "MarginTop");
			}
		}
	}

	[Editor(false)]
	public float MarginLeft
	{
		get
		{
			return _marginLeft;
		}
		set
		{
			if (_marginLeft != value)
			{
				SetMeasureAndLayoutDirty();
				_marginLeft = value;
				OnPropertyChanged(value, "MarginLeft");
			}
		}
	}

	[Editor(false)]
	public float MarginBottom
	{
		get
		{
			return _marginBottom;
		}
		set
		{
			if (_marginBottom != value)
			{
				SetMeasureAndLayoutDirty();
				_marginBottom = value;
				OnPropertyChanged(value, "MarginBottom");
			}
		}
	}

	[Editor(false)]
	public float MarginRight
	{
		get
		{
			return _marginRight;
		}
		set
		{
			if (_marginRight != value)
			{
				SetMeasureAndLayoutDirty();
				_marginRight = value;
				OnPropertyChanged(value, "MarginRight");
			}
		}
	}

	public float ScaledMarginTop => _scaleToUse * MarginTop;

	public float ScaledMarginLeft => _scaleToUse * MarginLeft;

	public float ScaledMarginBottom => _scaleToUse * MarginBottom;

	public float ScaledMarginRight => _scaleToUse * MarginRight;

	[Editor(false)]
	public VerticalAlignment VerticalAlignment
	{
		get
		{
			return _verticalAlignment;
		}
		set
		{
			if (_verticalAlignment != value)
			{
				SetMeasureAndLayoutDirty();
				_verticalAlignment = value;
				OnPropertyChanged(Enum.GetName(typeof(VerticalAlignment), value), "VerticalAlignment");
			}
		}
	}

	[Editor(false)]
	public HorizontalAlignment HorizontalAlignment
	{
		get
		{
			return _horizontalAlignment;
		}
		set
		{
			if (_horizontalAlignment != value)
			{
				SetMeasureAndLayoutDirty();
				_horizontalAlignment = value;
				OnPropertyChanged(Enum.GetName(typeof(HorizontalAlignment), value), "HorizontalAlignment");
			}
		}
	}

	public float Left
	{
		get
		{
			return _topLeft.X;
		}
		private set
		{
			if (value != _topLeft.X)
			{
				EventManager.SetPositionsDirty();
				_topLeft.X = value;
			}
		}
	}

	public float Top
	{
		get
		{
			return _topLeft.Y;
		}
		private set
		{
			if (value != _topLeft.Y)
			{
				EventManager.SetPositionsDirty();
				_topLeft.Y = value;
			}
		}
	}

	public float Right => _topLeft.X + Size.X;

	public float Bottom => _topLeft.Y + Size.Y;

	public int ChildCount => _children.Count;

	[Editor(false)]
	public bool ForcePixelPerfectRenderPlacement
	{
		get
		{
			return _forcePixelPerfectRenderPlacement;
		}
		set
		{
			if (_forcePixelPerfectRenderPlacement != value)
			{
				_forcePixelPerfectRenderPlacement = value;
				OnPropertyChanged(value, "ForcePixelPerfectRenderPlacement");
			}
		}
	}

	public bool UseGlobalTimeForAnimation { get; set; }

	[Editor(false)]
	public SizePolicy WidthSizePolicy
	{
		get
		{
			return _widthSizePolicy;
		}
		set
		{
			if (value != _widthSizePolicy)
			{
				SetMeasureAndLayoutDirty();
				_widthSizePolicy = value;
			}
		}
	}

	[Editor(false)]
	public SizePolicy HeightSizePolicy
	{
		get
		{
			return _heightSizePolicy;
		}
		set
		{
			if (value != _heightSizePolicy)
			{
				SetMeasureAndLayoutDirty();
				_heightSizePolicy = value;
			}
		}
	}

	[Editor(false)]
	public bool AcceptDrag { get; set; }

	[Editor(false)]
	public bool AcceptDrop { get; set; }

	[Editor(false)]
	public bool HideOnDrag { get; set; } = true;


	[Editor(false)]
	public Widget DragWidget
	{
		get
		{
			return _dragWidget;
		}
		set
		{
			if (_dragWidget != value)
			{
				if (value != null)
				{
					_dragWidget = value;
					_dragWidget.IsVisible = false;
				}
				else
				{
					_dragWidget = null;
				}
			}
		}
	}

	[Editor(false)]
	public bool ClipContents
	{
		get
		{
			if (ClipVerticalContent)
			{
				return ClipHorizontalContent;
			}
			return false;
		}
		set
		{
			ClipHorizontalContent = value;
			ClipVerticalContent = value;
		}
	}

	[Editor(false)]
	public bool ClipHorizontalContent { get; set; }

	[Editor(false)]
	public bool ClipVerticalContent { get; set; }

	[Editor(false)]
	public bool CircularClipEnabled { get; set; }

	[Editor(false)]
	public float CircularClipRadius { get; set; }

	[Editor(false)]
	public bool IsCircularClipRadiusHalfOfWidth { get; set; }

	[Editor(false)]
	public bool IsCircularClipRadiusHalfOfHeight { get; set; }

	[Editor(false)]
	public float CircularClipSmoothingRadius { get; set; }

	[Editor(false)]
	public float CircularClipXOffset { get; set; }

	[Editor(false)]
	public float CircularClipYOffset { get; set; }

	[Editor(false)]
	public bool RenderLate { get; set; }

	[Editor(false)]
	public bool DoNotRenderIfNotFullyInsideScissor { get; set; }

	public bool FixedWidth => WidthSizePolicy == SizePolicy.Fixed;

	public bool FixedHeight => HeightSizePolicy == SizePolicy.Fixed;

	public bool IsHovered
	{
		get
		{
			return _isHovered;
		}
		private set
		{
			if (_isHovered != value)
			{
				_isHovered = value;
				RefreshState();
				OnPropertyChanged(value, "IsHovered");
			}
		}
	}

	[Editor(false)]
	public bool IsDisabled
	{
		get
		{
			return _isDisabled;
		}
		set
		{
			if (_isDisabled != value)
			{
				_isDisabled = value;
				OnPropertyChanged(value, "IsDisabled");
				RefreshState();
			}
		}
	}

	[Editor(false)]
	public bool IsFocusable
	{
		get
		{
			return _isFocusable;
		}
		set
		{
			if (_isFocusable != value)
			{
				_isFocusable = value;
				if (ConnectedToRoot)
				{
					OnPropertyChanged(value, "IsFocusable");
					RefreshState();
				}
			}
		}
	}

	public bool IsFocused
	{
		get
		{
			return _isFocused;
		}
		private set
		{
			if (_isFocused != value)
			{
				_isFocused = value;
				RefreshState();
			}
		}
	}

	[Editor(false)]
	public bool IsEnabled
	{
		get
		{
			return !IsDisabled;
		}
		set
		{
			if (value == IsDisabled)
			{
				IsDisabled = !value;
				OnPropertyChanged(value, "IsEnabled");
			}
		}
	}

	[Editor(false)]
	public bool RestartAnimationFirstFrame
	{
		get
		{
			return _restartAnimationFirstFrame;
		}
		set
		{
			if (_restartAnimationFirstFrame != value)
			{
				_restartAnimationFirstFrame = value;
			}
		}
	}

	[Editor(false)]
	public bool DoNotPassEventsToChildren
	{
		get
		{
			return _doNotPassEventsToChildren;
		}
		set
		{
			if (_doNotPassEventsToChildren != value)
			{
				_doNotPassEventsToChildren = value;
				OnPropertyChanged(value, "DoNotPassEventsToChildren");
			}
		}
	}

	[Editor(false)]
	public bool DoNotAcceptEvents
	{
		get
		{
			return _doNotAcceptEvents;
		}
		set
		{
			if (_doNotAcceptEvents != value)
			{
				_doNotAcceptEvents = value;
				OnPropertyChanged(value, "DoNotAcceptEvents");
			}
		}
	}

	[Editor(false)]
	public bool CanAcceptEvents
	{
		get
		{
			return !DoNotAcceptEvents;
		}
		set
		{
			DoNotAcceptEvents = !value;
		}
	}

	public bool IsPressed
	{
		get
		{
			return _isPressed;
		}
		internal set
		{
			if (_isPressed != value)
			{
				_isPressed = value;
				RefreshState();
			}
		}
	}

	[Editor(false)]
	public bool IsHidden
	{
		get
		{
			return _isHidden;
		}
		set
		{
			if (_isHidden != value)
			{
				SetMeasureAndLayoutDirty();
				_isHidden = value;
				RefreshState();
				OnPropertyChanged(value, "IsHidden");
				OnPropertyChanged(!value, "IsVisible");
				if (this.OnVisibilityChanged != null)
				{
					this.OnVisibilityChanged(this);
				}
			}
		}
	}

	[Editor(false)]
	public bool IsVisible
	{
		get
		{
			return !_isHidden;
		}
		set
		{
			if (value == _isHidden)
			{
				IsHidden = !value;
			}
		}
	}

	[Editor(false)]
	public Sprite Sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			if (value != _sprite)
			{
				_sprite = value;
			}
		}
	}

	[Editor(false)]
	public VisualDefinition VisualDefinition
	{
		get
		{
			return _visualDefinition;
		}
		set
		{
			if (_visualDefinition != value)
			{
				VisualDefinition visualDefinition = _visualDefinition;
				_visualDefinition = value;
				_stateTimer = 0f;
				if (ConnectedToRoot && (visualDefinition == null || _visualDefinition == null))
				{
					EventManager.OnWidgetVisualDefinitionChanged(this);
				}
			}
		}
	}

	public string CurrentState { get; protected set; } = "";


	[Editor(false)]
	public bool UpdateChildrenStates
	{
		get
		{
			return _updateChildrenStates;
		}
		set
		{
			if (_updateChildrenStates != value)
			{
				_updateChildrenStates = value;
				OnPropertyChanged(value, "UpdateChildrenStates");
				if (value && ChildCount > 0)
				{
					SetState(CurrentState);
				}
			}
		}
	}

	public object Tag { get; set; }

	public ILayout LayoutImp { get; protected set; }

	[Editor(false)]
	public bool DropEventHandledManually { get; set; }

	internal WidgetInfo WidgetInfo { get; private set; }

	public IEnumerable<Widget> AllChildrenAndThis
	{
		get
		{
			yield return this;
			foreach (Widget child in _children)
			{
				foreach (Widget allChildrenAndThi in child.AllChildrenAndThis)
				{
					yield return allChildrenAndThi;
				}
			}
		}
	}

	public IEnumerable<Widget> AllChildren
	{
		get
		{
			foreach (Widget widget in _children)
			{
				yield return widget;
				foreach (Widget allChild in widget.AllChildren)
				{
					yield return allChild;
				}
			}
		}
	}

	public List<Widget> Children => _children;

	public IEnumerable<Widget> Parents
	{
		get
		{
			for (Widget parent = ParentWidget; parent != null; parent = parent.ParentWidget)
			{
				yield return parent;
			}
		}
	}

	internal bool ConnectedToRoot
	{
		get
		{
			if (Id == "Root")
			{
				return true;
			}
			if (ParentWidget == null)
			{
				return false;
			}
			return ParentWidget.ConnectedToRoot;
		}
	}

	internal int OnUpdateListIndex { get; set; } = -1;


	internal int OnLateUpdateListIndex { get; set; } = -1;


	internal int OnUpdateBrushesIndex { get; set; } = -1;


	internal int OnParallelUpdateListIndex { get; set; } = -1;


	internal int OnVisualDefinitionListIndex { get; set; } = -1;


	internal int OnTweenPositionListIndex { get; set; } = -1;


	[Editor(false)]
	public float MaxWidth
	{
		get
		{
			return _maxWidth;
		}
		set
		{
			if (_maxWidth != value)
			{
				_maxWidth = value;
				_gotMaxWidth = true;
				OnPropertyChanged(value, "MaxWidth");
			}
		}
	}

	[Editor(false)]
	public float MaxHeight
	{
		get
		{
			return _maxHeight;
		}
		set
		{
			if (_maxHeight != value)
			{
				_maxHeight = value;
				_gotMaxHeight = true;
				OnPropertyChanged(value, "MaxHeight");
			}
		}
	}

	[Editor(false)]
	public float MinWidth
	{
		get
		{
			return _minWidth;
		}
		set
		{
			if (_minWidth != value)
			{
				_minWidth = value;
				_gotMinWidth = true;
				OnPropertyChanged(value, "MinWidth");
			}
		}
	}

	[Editor(false)]
	public float MinHeight
	{
		get
		{
			return _minHeight;
		}
		set
		{
			if (_minHeight != value)
			{
				_minHeight = value;
				_gotMinHeight = true;
				OnPropertyChanged(value, "MinHeight");
			}
		}
	}

	public float ScaledMaxWidth => _scaleToUse * _maxWidth;

	public float ScaledMaxHeight => _scaleToUse * _maxHeight;

	public float ScaledMinWidth => _scaleToUse * _minWidth;

	public float ScaledMinHeight => _scaleToUse * _minHeight;

	public bool DisableRender { get; set; }

	public float ExtendCursorAreaTop { get; set; }

	public float ExtendCursorAreaRight { get; set; }

	public float ExtendCursorAreaBottom { get; set; }

	public float ExtendCursorAreaLeft { get; set; }

	public float CursorAreaXOffset { get; set; }

	public float CursorAreaYOffset { get; set; }

	public bool AcceptNavigation
	{
		get
		{
			return !DoNotAcceptNavigation;
		}
		set
		{
			if (value == DoNotAcceptNavigation)
			{
				DoNotAcceptNavigation = !value;
			}
		}
	}

	public bool DoNotAcceptNavigation
	{
		get
		{
			return _doNotAcceptNavigation;
		}
		set
		{
			if (value != _doNotAcceptNavigation)
			{
				_doNotAcceptNavigation = value;
				GamepadNavigationContext.OnWidgetNavigationStatusChanged(this);
			}
		}
	}

	public bool IsUsingNavigation
	{
		get
		{
			return _isUsingNavigation;
		}
		set
		{
			if (value != _isUsingNavigation)
			{
				_isUsingNavigation = value;
				OnPropertyChanged(value, "IsUsingNavigation");
			}
		}
	}

	public bool UseSiblingIndexForNavigation
	{
		get
		{
			return _useSiblingIndexForNavigation;
		}
		set
		{
			if (value != _useSiblingIndexForNavigation)
			{
				_useSiblingIndexForNavigation = value;
				if (value)
				{
					GamepadNavigationIndex = GetSiblingIndex();
				}
			}
		}
	}

	public int GamepadNavigationIndex
	{
		get
		{
			return _gamepadNavigationIndex;
		}
		set
		{
			if (value != _gamepadNavigationIndex)
			{
				_gamepadNavigationIndex = value;
				GamepadNavigationContext.OnWidgetNavigationIndexUpdated(this);
				OnGamepadNavigationIndexUpdated(value);
				OnPropertyChanged(value, "GamepadNavigationIndex");
			}
		}
	}

	public GamepadNavigationTypes UsedNavigationMovements
	{
		get
		{
			return _usedNavigationMovements;
		}
		set
		{
			if (value != _usedNavigationMovements)
			{
				_usedNavigationMovements = value;
				Context.GamepadNavigation.OnWidgetUsedNavigationMovementsUpdated(this);
			}
		}
	}

	public event Action<Widget, string, object[]> EventFire
	{
		add
		{
			if (_eventTargets == null)
			{
				_eventTargets = new List<Action<Widget, string, object[]>>();
			}
			_eventTargets.Add(value);
		}
		remove
		{
			if (_eventTargets != null)
			{
				_eventTargets.Remove(value);
			}
		}
	}

	public event Action<Widget> OnVisibilityChanged;

	public void ApplyActionOnAllChildren(Action<Widget> action)
	{
		foreach (Widget child in _children)
		{
			action(child);
			child.ApplyActionOnAllChildren(action);
		}
	}

	public Widget(UIContext context)
	{
		DropEventHandledManually = true;
		LayoutImp = new DefaultLayout();
		_children = new List<Widget>();
		Context = context;
		_states = new List<string>();
		WidgetInfo = WidgetInfo.GetWidgetInfo(GetType());
		Sprite = null;
		_stateTimer = 0f;
		_currentVisualStateAnimationState = VisualStateAnimationState.None;
		_isFocusable = false;
		_seed = 0;
		_components = new List<WidgetComponent>();
		AddState("Default");
		SetState("Default");
	}

	public T GetComponent<T>() where T : WidgetComponent
	{
		for (int i = 0; i < _components.Count; i++)
		{
			WidgetComponent widgetComponent = _components[i];
			if (widgetComponent is T)
			{
				return (T)widgetComponent;
			}
		}
		return null;
	}

	public void AddComponent(WidgetComponent component)
	{
		_components.Add(component);
	}

	protected void SetMeasureAndLayoutDirty()
	{
		SetMeasureDirty();
		SetLayoutDirty();
	}

	protected void SetMeasureDirty()
	{
		EventManager.SetMeasureDirty();
	}

	protected void SetLayoutDirty()
	{
		EventManager.SetLayoutDirty();
	}

	public void AddState(string stateName)
	{
		if (!_states.Contains(stateName))
		{
			_states.Add(stateName);
		}
	}

	public bool ContainsState(string stateName)
	{
		return _states.Contains(stateName);
	}

	public virtual void SetState(string stateName)
	{
		if (CurrentState != stateName)
		{
			CurrentState = stateName;
			_stateTimer = 0f;
			if (_currentVisualStateAnimationState != 0)
			{
				_startVisualState = new VisualState("@StartState");
				_startVisualState.FillFromWidget(this);
			}
			_currentVisualStateAnimationState = VisualStateAnimationState.PlayingBasicTranisition;
		}
		if (!UpdateChildrenStates)
		{
			return;
		}
		for (int i = 0; i < ChildCount; i++)
		{
			Widget child = GetChild(i);
			if (!(child is ImageWidget) || !((ImageWidget)child).OverrideDefaultStateSwitchingEnabled)
			{
				child.SetState(CurrentState);
			}
		}
	}

	public Widget FindChild(BindingPath path)
	{
		string firstNode = path.FirstNode;
		BindingPath subPath = path.SubPath;
		if (firstNode == "..")
		{
			return ParentWidget.FindChild(subPath);
		}
		if (firstNode == ".")
		{
			return this;
		}
		foreach (Widget child in _children)
		{
			if (!string.IsNullOrEmpty(child.Id) && child.Id == firstNode)
			{
				if (subPath == null)
				{
					return child;
				}
				return child.FindChild(subPath);
			}
		}
		return null;
	}

	public Widget FindChild(string singlePathNode)
	{
		if (singlePathNode == "..")
		{
			return ParentWidget;
		}
		if (singlePathNode == ".")
		{
			return this;
		}
		foreach (Widget child in _children)
		{
			if (!string.IsNullOrEmpty(child.Id) && child.Id == singlePathNode)
			{
				return child;
			}
		}
		return null;
	}

	public Widget FindChild(WidgetSearchDelegate widgetSearchDelegate)
	{
		for (int i = 0; i < _children.Count; i++)
		{
			Widget widget = _children[i];
			if (widgetSearchDelegate(widget))
			{
				return widget;
			}
		}
		return null;
	}

	public Widget FindChild(string id, bool includeAllChildren = false)
	{
		IEnumerable<Widget> enumerable;
		if (!includeAllChildren)
		{
			IEnumerable<Widget> children = _children;
			enumerable = children;
		}
		else
		{
			enumerable = AllChildren;
		}
		foreach (Widget item in enumerable)
		{
			if (!string.IsNullOrEmpty(item.Id) && item.Id == id)
			{
				return item;
			}
		}
		return null;
	}

	public void RemoveAllChildren()
	{
		while (_children.Count > 0)
		{
			_children[0].ParentWidget = null;
		}
	}

	private static float GetEaseOutBack(float t)
	{
		float num = 0.5f;
		float num2 = num + 1f;
		return 1f + num2 * TaleWorlds.Library.MathF.Pow(t - 1f, 3f) + num * TaleWorlds.Library.MathF.Pow(t - 1f, 2f);
	}

	internal void UpdateVisualDefinitions(float dt)
	{
		if (VisualDefinition != null && _currentVisualStateAnimationState == VisualStateAnimationState.PlayingBasicTranisition)
		{
			if (_startVisualState == null)
			{
				_startVisualState = new VisualState("@StartState");
				_startVisualState.FillFromWidget(this);
			}
			VisualState visualState = VisualDefinition.GetVisualState(CurrentState);
			if (visualState != null)
			{
				float num = (visualState.GotTransitionDuration ? visualState.TransitionDuration : VisualDefinition.TransitionDuration);
				float delayOnBegin = VisualDefinition.DelayOnBegin;
				if (_stateTimer < num)
				{
					if (_stateTimer >= delayOnBegin)
					{
						float num2 = (_stateTimer - delayOnBegin) / (num - delayOnBegin);
						if (VisualDefinition.EaseIn)
						{
							num2 = GetEaseOutBack(num2);
						}
						PositionXOffset = (visualState.GotPositionXOffset ? Mathf.Lerp(_startVisualState.PositionXOffset, visualState.PositionXOffset, num2) : PositionXOffset);
						PositionYOffset = (visualState.GotPositionYOffset ? Mathf.Lerp(_startVisualState.PositionYOffset, visualState.PositionYOffset, num2) : PositionYOffset);
						SuggestedWidth = (visualState.GotSuggestedWidth ? Mathf.Lerp(_startVisualState.SuggestedWidth, visualState.SuggestedWidth, num2) : SuggestedWidth);
						SuggestedHeight = (visualState.GotSuggestedHeight ? Mathf.Lerp(_startVisualState.SuggestedHeight, visualState.SuggestedHeight, num2) : SuggestedHeight);
						MarginTop = (visualState.GotMarginTop ? Mathf.Lerp(_startVisualState.MarginTop, visualState.MarginTop, num2) : MarginTop);
						MarginBottom = (visualState.GotMarginBottom ? Mathf.Lerp(_startVisualState.MarginBottom, visualState.MarginBottom, num2) : MarginBottom);
						MarginLeft = (visualState.GotMarginLeft ? Mathf.Lerp(_startVisualState.MarginLeft, visualState.MarginLeft, num2) : MarginLeft);
						MarginRight = (visualState.GotMarginRight ? Mathf.Lerp(_startVisualState.MarginRight, visualState.MarginRight, num2) : MarginRight);
					}
				}
				else
				{
					PositionXOffset = (visualState.GotPositionXOffset ? visualState.PositionXOffset : PositionXOffset);
					PositionYOffset = (visualState.GotPositionYOffset ? visualState.PositionYOffset : PositionYOffset);
					SuggestedWidth = (visualState.GotSuggestedWidth ? visualState.SuggestedWidth : SuggestedWidth);
					SuggestedHeight = (visualState.GotSuggestedHeight ? visualState.SuggestedHeight : SuggestedHeight);
					MarginTop = (visualState.GotMarginTop ? visualState.MarginTop : MarginTop);
					MarginBottom = (visualState.GotMarginBottom ? visualState.MarginBottom : MarginBottom);
					MarginLeft = (visualState.GotMarginLeft ? visualState.MarginLeft : MarginLeft);
					MarginRight = (visualState.GotMarginRight ? visualState.MarginRight : MarginRight);
					_startVisualState = visualState;
					_currentVisualStateAnimationState = VisualStateAnimationState.None;
				}
			}
			else
			{
				_currentVisualStateAnimationState = VisualStateAnimationState.None;
			}
		}
		_stateTimer += dt;
	}

	internal void Update(float dt)
	{
		OnUpdate(dt);
	}

	internal void LateUpdate(float dt)
	{
		OnLateUpdate(dt);
	}

	internal void ParallelUpdate(float dt)
	{
		if (!_isInParallelOperation)
		{
			_isInParallelOperation = true;
			OnParallelUpdate(dt);
			_isInParallelOperation = false;
		}
	}

	protected virtual void OnUpdate(float dt)
	{
	}

	protected virtual void OnParallelUpdate(float dt)
	{
	}

	protected virtual void OnLateUpdate(float dt)
	{
	}

	protected virtual void RefreshState()
	{
	}

	public virtual void UpdateAnimationPropertiesSubTask(float alphaFactor)
	{
		AlphaFactor = alphaFactor;
		foreach (Widget child in Children)
		{
			child.UpdateAnimationPropertiesSubTask(alphaFactor);
		}
	}

	public void Measure(Vector2 measureSpec)
	{
		if (IsHidden)
		{
			MeasuredSize = Vector2.Zero;
		}
		else
		{
			OnMeasure(measureSpec);
		}
	}

	private Vector2 ProcessSizeWithBoundaries(Vector2 input)
	{
		Vector2 result = input;
		if (_gotMinWidth && input.X < ScaledMinWidth)
		{
			result.X = ScaledMinWidth;
		}
		if (_gotMinHeight && input.Y < ScaledMinHeight)
		{
			result.Y = ScaledMinHeight;
		}
		if (_gotMaxWidth && input.X > ScaledMaxWidth)
		{
			result.X = ScaledMaxWidth;
		}
		if (_gotMaxHeight && input.Y > ScaledMaxHeight)
		{
			result.Y = ScaledMaxHeight;
		}
		return result;
	}

	private void OnMeasure(Vector2 measureSpec)
	{
		if (WidthSizePolicy == SizePolicy.Fixed)
		{
			measureSpec.X = ScaledSuggestedWidth;
		}
		else if (WidthSizePolicy == SizePolicy.StretchToParent)
		{
			measureSpec.X -= ScaledMarginLeft + ScaledMarginRight;
		}
		else
		{
			_ = WidthSizePolicy;
			_ = 2;
		}
		if (HeightSizePolicy == SizePolicy.Fixed)
		{
			measureSpec.Y = ScaledSuggestedHeight;
		}
		else if (HeightSizePolicy == SizePolicy.StretchToParent)
		{
			measureSpec.Y -= ScaledMarginTop + ScaledMarginBottom;
		}
		else
		{
			_ = HeightSizePolicy;
			_ = 2;
		}
		measureSpec = ProcessSizeWithBoundaries(measureSpec);
		Vector2 vector = MeasureChildren(measureSpec);
		Vector2 input = new Vector2(0f, 0f);
		if (WidthSizePolicy == SizePolicy.Fixed)
		{
			input.X = ScaledSuggestedWidth;
		}
		else if (WidthSizePolicy == SizePolicy.CoverChildren)
		{
			input.X = vector.X;
		}
		else if (WidthSizePolicy == SizePolicy.StretchToParent)
		{
			input.X = measureSpec.X;
		}
		if (HeightSizePolicy == SizePolicy.Fixed)
		{
			input.Y = ScaledSuggestedHeight;
		}
		else if (HeightSizePolicy == SizePolicy.CoverChildren)
		{
			input.Y = vector.Y;
		}
		else if (HeightSizePolicy == SizePolicy.StretchToParent)
		{
			input.Y = measureSpec.Y;
		}
		input = ProcessSizeWithBoundaries(input);
		MeasuredSize = input;
	}

	public bool CheckIsMyChildRecursive(Widget child)
	{
		for (Widget widget = child?.ParentWidget; widget != null; widget = widget.ParentWidget)
		{
			if (widget == this)
			{
				return true;
			}
		}
		return false;
	}

	private Vector2 MeasureChildren(Vector2 measureSpec)
	{
		return LayoutImp.MeasureChildren(this, measureSpec, Context.SpriteData, _scaleToUse);
	}

	public void AddChild(Widget widget)
	{
		widget.ParentWidget = this;
	}

	public void AddChildAtIndex(Widget widget, int index)
	{
		widget.ParentWidget = this;
		widget.SetSiblingIndex(index);
	}

	public void SwapChildren(Widget widget1, Widget widget2)
	{
		int index = _children.IndexOf(widget1);
		int index2 = _children.IndexOf(widget2);
		Widget value = _children[index];
		_children[index] = _children[index2];
		_children[index2] = value;
	}

	protected virtual void OnChildAdded(Widget child)
	{
		EventFired("ItemAdd", child);
		if (DoNotUseCustomScaleAndChildren)
		{
			child.DoNotUseCustomScaleAndChildren = true;
		}
		if (UpdateChildrenStates && (!(child is ImageWidget) || !((ImageWidget)child).OverrideDefaultStateSwitchingEnabled))
		{
			child.SetState(CurrentState);
		}
	}

	public void RemoveChild(Widget widget)
	{
		widget.ParentWidget = null;
	}

	public virtual void OnBeforeRemovedChild(Widget widget)
	{
		if (IsHovered)
		{
			EventFired("HoverEnd");
		}
		Children.ForEach(delegate(Widget c)
		{
			c.OnBeforeRemovedChild(widget);
		});
	}

	public bool HasChild(Widget widget)
	{
		return _children.Contains(widget);
	}

	protected virtual void OnChildRemoved(Widget child)
	{
		EventFired("ItemRemove", child);
	}

	protected virtual void OnAfterChildRemoved(Widget child)
	{
		EventFired("AfterItemRemove", child);
	}

	public virtual void UpdateBrushes(float dt)
	{
	}

	public int GetChildIndex(Widget child)
	{
		return _children.IndexOf(child);
	}

	public int GetVisibleChildIndex(Widget child)
	{
		int result = -1;
		List<Widget> list = _children.Where((Widget c) => c.IsVisible).ToList();
		if (list.Count > 0)
		{
			result = list.IndexOf(child);
		}
		return result;
	}

	public int GetFilterChildIndex(Widget child, Func<Widget, bool> childrenFilter)
	{
		int result = -1;
		List<Widget> list = _children.Where((Widget c) => childrenFilter(c)).ToList();
		if (list.Count > 0)
		{
			result = list.IndexOf(child);
		}
		return result;
	}

	public Widget GetChild(int i)
	{
		if (i < _children.Count)
		{
			return _children[i];
		}
		return null;
	}

	public void Layout(float left, float bottom, float right, float top)
	{
		if (IsVisible)
		{
			SetLayout(left, bottom, right, top);
			Vector2 scaledPositionOffset = ScaledPositionOffset;
			Left += scaledPositionOffset.X;
			Top += scaledPositionOffset.Y;
			OnLayout(Left, Bottom, Right, Top);
		}
	}

	private void SetLayout(float left, float bottom, float right, float top)
	{
		left += ScaledMarginLeft;
		right -= ScaledMarginRight;
		top += ScaledMarginTop;
		bottom -= ScaledMarginBottom;
		float num = right - left;
		float num2 = bottom - top;
		float left2 = ((HorizontalAlignment == HorizontalAlignment.Left) ? left : ((HorizontalAlignment != HorizontalAlignment.Center) ? (right - MeasuredSize.X) : (left + num / 2f - MeasuredSize.X / 2f)));
		float top2 = ((VerticalAlignment == VerticalAlignment.Top) ? top : ((VerticalAlignment != VerticalAlignment.Center) ? (bottom - MeasuredSize.Y) : (top + num2 / 2f - MeasuredSize.Y / 2f)));
		Left = left2;
		Top = top2;
		Size = MeasuredSize;
	}

	private void OnLayout(float left, float bottom, float right, float top)
	{
		LayoutImp.OnLayout(this, left, bottom, right, top);
	}

	internal void DoTweenPosition(float dt)
	{
		if (IsVisible && dt > 0f)
		{
			float num = Left - LocalPosition.X;
			float num2 = Top - LocalPosition.Y;
			if (Mathf.Abs(num) + Mathf.Abs(num2) < 0.003f)
			{
				LocalPosition = new Vector2(Left, Top);
				return;
			}
			num = Mathf.Clamp(num, -100f, 100f);
			num2 = Mathf.Clamp(num2, -100f, 100f);
			float num3 = Mathf.Min(dt * 18f, 1f);
			LocalPosition = new Vector2(LocalPosition.X + num3 * num, LocalPosition.Y + num3 * num2);
		}
	}

	internal void ParallelUpdateChildPositions(Vector2 globalPosition)
	{
		TWParallel.For(0, _children.Count, UpdateChildPositionMT);
		void UpdateChildPositionMT(int startInclusive, int endExclusive)
		{
			for (int i = startInclusive; i < endExclusive; i++)
			{
				_children[i].UpdatePosition(globalPosition);
			}
		}
	}

	internal void UpdatePosition(Vector2 parentPosition)
	{
		if (!IsVisible)
		{
			return;
		}
		if (!TweenPosition)
		{
			LocalPosition = new Vector2(Left, Top);
		}
		Vector2 vector = LocalPosition + parentPosition;
		if (_children.Count >= 64)
		{
			ParallelUpdateChildPositions(vector);
		}
		else
		{
			for (int i = 0; i < _children.Count; i++)
			{
				_children[i].UpdatePosition(vector);
			}
		}
		_cachedGlobalPosition = vector;
	}

	public virtual void HandleInput(IReadOnlyList<int> lastKeysPressed)
	{
	}

	public bool IsPointInsideMeasuredArea(Vector2 p)
	{
		Vector2 globalPosition = GlobalPosition;
		if (globalPosition.X <= p.X && globalPosition.Y <= p.Y && globalPosition.X + Size.X >= p.X)
		{
			return globalPosition.Y + Size.Y >= p.Y;
		}
		return false;
	}

	public bool IsPointInsideGamepadCursorArea(Vector2 p)
	{
		Vector2 globalPosition = GlobalPosition;
		globalPosition.X -= ExtendCursorAreaLeft;
		globalPosition.Y -= ExtendCursorAreaTop;
		Vector2 size = Size;
		size.X += ExtendCursorAreaLeft + ExtendCursorAreaRight;
		size.Y += ExtendCursorAreaTop + ExtendCursorAreaBottom;
		if (p.X >= globalPosition.X && p.Y > globalPosition.Y && p.X < globalPosition.X + size.X)
		{
			return p.Y < globalPosition.Y + size.Y;
		}
		return false;
	}

	public void Hide()
	{
		IsHidden = true;
	}

	public void Show()
	{
		IsHidden = false;
	}

	public Vector2 GetLocalPoint(Vector2 globalPoint)
	{
		return globalPoint - GlobalPosition;
	}

	public void SetSiblingIndex(int index, bool force = false)
	{
		int siblingIndex = GetSiblingIndex();
		if (siblingIndex != index || force)
		{
			ParentWidget._children.RemoveAt(siblingIndex);
			ParentWidget._children.Insert(index, this);
			SetMeasureAndLayoutDirty();
			EventFired("SiblingIndexChanged");
		}
	}

	public int GetSiblingIndex()
	{
		return ParentWidget?.GetChildIndex(this) ?? (-1);
	}

	public int GetVisibleSiblingIndex()
	{
		return ParentWidget.GetVisibleChildIndex(this);
	}

	public void Render(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		if (!IsHidden && !DisableRender)
		{
			Vector2 cachedGlobalPosition = _cachedGlobalPosition;
			if (ClipHorizontalContent || ClipVerticalContent)
			{
				int width = (ClipHorizontalContent ? ((int)Size.X) : (-1));
				int height = (ClipVerticalContent ? ((int)Size.Y) : (-1));
				drawContext.PushScissor((int)cachedGlobalPosition.X, (int)cachedGlobalPosition.Y, width, height);
			}
			if (CircularClipEnabled)
			{
				if (IsCircularClipRadiusHalfOfHeight)
				{
					CircularClipRadius = Size.Y / 2f * _inverseScaleToUse;
				}
				else if (IsCircularClipRadiusHalfOfWidth)
				{
					CircularClipRadius = Size.X / 2f * _inverseScaleToUse;
				}
				Vector2 position = new Vector2(cachedGlobalPosition.X + Size.X * 0.5f + CircularClipXOffset * _scaleToUse, cachedGlobalPosition.Y + Size.Y * 0.5f + CircularClipYOffset * _scaleToUse);
				drawContext.SetCircualMask(position, CircularClipRadius * _scaleToUse, CircularClipSmoothingRadius * _scaleToUse);
			}
			bool flag = false;
			if (drawContext.ScissorTestEnabled)
			{
				ScissorTestInfo currentScissor = drawContext.CurrentScissor;
				Rectangle rectangle = new Rectangle(cachedGlobalPosition.X, cachedGlobalPosition.Y, Size.X, Size.Y);
				Rectangle other = new Rectangle(currentScissor.X, currentScissor.Y, currentScissor.Width, currentScissor.Height);
				if (rectangle.IsCollide(other) || _calculateSizeFirstFrame)
				{
					flag = !DoNotRenderIfNotFullyInsideScissor || rectangle.IsSubRectOf(other);
				}
			}
			else
			{
				Rectangle rectangle2 = new Rectangle(_cachedGlobalPosition.X, _cachedGlobalPosition.Y, MeasuredSize.X, MeasuredSize.Y);
				Rectangle other2 = new Rectangle(EventManager.LeftUsableAreaStart, EventManager.TopUsableAreaStart, EventManager.PageSize.X, EventManager.PageSize.Y);
				if (rectangle2.IsCollide(other2) || _calculateSizeFirstFrame)
				{
					flag = true;
				}
			}
			if (flag)
			{
				OnRender(twoDimensionContext, drawContext);
				for (int i = 0; i < _children.Count; i++)
				{
					Widget widget = _children[i];
					if (!widget.RenderLate)
					{
						widget.Render(twoDimensionContext, drawContext);
					}
				}
				for (int j = 0; j < _children.Count; j++)
				{
					Widget widget2 = _children[j];
					if (widget2.RenderLate)
					{
						widget2.Render(twoDimensionContext, drawContext);
					}
				}
			}
			if (CircularClipEnabled)
			{
				drawContext.ClearCircualMask();
			}
			if (ClipHorizontalContent || ClipVerticalContent)
			{
				drawContext.PopScissor();
			}
		}
		_calculateSizeFirstFrame = false;
	}

	protected virtual void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		Vector2 globalPosition = GlobalPosition;
		if (ForcePixelPerfectRenderPlacement)
		{
			globalPosition.X = TaleWorlds.Library.MathF.Round(globalPosition.X);
			globalPosition.Y = TaleWorlds.Library.MathF.Round(globalPosition.Y);
		}
		if (_sprite == null)
		{
			return;
		}
		Texture texture = _sprite.Texture;
		if (texture != null)
		{
			float x = globalPosition.X;
			float y = globalPosition.Y;
			SimpleMaterial simpleMaterial = drawContext.CreateSimpleMaterial();
			simpleMaterial.OverlayEnabled = false;
			simpleMaterial.CircularMaskingEnabled = false;
			simpleMaterial.Texture = texture;
			simpleMaterial.Color = Color;
			simpleMaterial.ColorFactor = ColorFactor;
			simpleMaterial.AlphaFactor = AlphaFactor * Context.ContextAlpha;
			simpleMaterial.HueFactor = 0f;
			simpleMaterial.SaturationFactor = SaturationFactor;
			simpleMaterial.ValueFactor = ValueFactor;
			float num = ExtendLeft;
			if (HorizontalFlip)
			{
				num = ExtendRight;
			}
			float x2 = Size.X;
			x2 += (ExtendRight + ExtendLeft) * _scaleToUse;
			x -= num * _scaleToUse;
			float y2 = Size.Y;
			float num2 = ExtendTop;
			if (HorizontalFlip)
			{
				num2 = ExtendBottom;
			}
			y2 += (ExtendTop + ExtendBottom) * _scaleToUse;
			y -= num2 * _scaleToUse;
			drawContext.DrawSprite(_sprite, simpleMaterial, x, y, _scaleToUse, x2, y2, HorizontalFlip, VerticalFlip);
		}
	}

	protected void EventFired(string eventName, params object[] args)
	{
		if (_eventTargets != null)
		{
			for (int i = 0; i < _eventTargets.Count; i++)
			{
				_eventTargets[i](this, eventName, args);
			}
		}
	}

	public bool IsRecursivelyVisible()
	{
		for (Widget widget = this; widget != null; widget = widget.ParentWidget)
		{
			if (!widget.IsVisible)
			{
				return false;
			}
		}
		return true;
	}

	internal void HandleOnDisconnectedFromRoot()
	{
		OnDisconnectedFromRoot();
		if (IsHovered)
		{
			EventFired("HoverEnd");
		}
	}

	internal void HandleOnConnectedToRoot()
	{
		if (!_seedSet)
		{
			_seed = GetSiblingIndex();
			_seedSet = true;
		}
		OnConnectedToRoot();
	}

	protected virtual void OnDisconnectedFromRoot()
	{
	}

	protected virtual void OnConnectedToRoot()
	{
		EventFired("ConnectedToRoot");
	}

	public override string ToString()
	{
		return this.GetFullIDPath();
	}

	protected virtual void OnGamepadNavigationIndexUpdated(int newIndex)
	{
	}

	public void OnGamepadNavigationFocusGain()
	{
		OnGamepadNavigationFocusGained?.Invoke(this);
	}

	internal bool PreviewEvent(GauntletEvent gauntletEvent)
	{
		bool result = false;
		switch (gauntletEvent)
		{
		case GauntletEvent.MousePressed:
			result = OnPreviewMousePressed();
			break;
		case GauntletEvent.MouseReleased:
			result = OnPreviewMouseReleased();
			break;
		case GauntletEvent.MouseAlternatePressed:
			result = OnPreviewMouseAlternatePressed();
			break;
		case GauntletEvent.MouseAlternateReleased:
			result = OnPreviewMouseAlternateReleased();
			break;
		case GauntletEvent.MouseMove:
			result = OnPreviewMouseMove();
			break;
		case GauntletEvent.DragHover:
			result = OnPreviewDragHover();
			break;
		case GauntletEvent.DragBegin:
			result = OnPreviewDragBegin();
			break;
		case GauntletEvent.DragEnd:
			result = OnPreviewDragEnd();
			break;
		case GauntletEvent.Drop:
			result = OnPreviewDrop();
			break;
		case GauntletEvent.MouseScroll:
			result = OnPreviewMouseScroll();
			break;
		case GauntletEvent.RightStickMovement:
			result = OnPreviewRightStickMovement();
			break;
		}
		return result;
	}

	protected virtual bool OnPreviewMousePressed()
	{
		return true;
	}

	protected virtual bool OnPreviewMouseReleased()
	{
		return true;
	}

	protected virtual bool OnPreviewMouseAlternatePressed()
	{
		return true;
	}

	protected virtual bool OnPreviewMouseAlternateReleased()
	{
		return true;
	}

	protected virtual bool OnPreviewDragBegin()
	{
		return AcceptDrag;
	}

	protected virtual bool OnPreviewDragEnd()
	{
		return AcceptDrag;
	}

	protected virtual bool OnPreviewDrop()
	{
		return AcceptDrop;
	}

	protected virtual bool OnPreviewMouseScroll()
	{
		return false;
	}

	protected virtual bool OnPreviewRightStickMovement()
	{
		return false;
	}

	protected virtual bool OnPreviewMouseMove()
	{
		return true;
	}

	protected virtual bool OnPreviewDragHover()
	{
		return false;
	}

	protected internal virtual void OnMousePressed()
	{
		IsPressed = true;
		EventFired("MouseDown");
	}

	protected internal virtual void OnMouseReleased()
	{
		IsPressed = false;
		EventFired("MouseUp");
	}

	protected internal virtual void OnMouseAlternatePressed()
	{
		EventFired("MouseAlternateDown");
	}

	protected internal virtual void OnMouseAlternateReleased()
	{
		EventFired("MouseAlternateUp");
	}

	protected internal virtual void OnMouseMove()
	{
		EventFired("MouseMove");
	}

	protected internal virtual void OnHoverBegin()
	{
		IsHovered = true;
		EventFired("HoverBegin");
	}

	protected internal virtual void OnHoverEnd()
	{
		EventFired("HoverEnd");
		IsHovered = false;
	}

	protected internal virtual void OnDragBegin()
	{
		EventManager.BeginDragging(this);
		EventFired("DragBegin");
	}

	protected internal virtual void OnDragEnd()
	{
		EventFired("DragEnd");
	}

	protected internal virtual bool OnDrop()
	{
		if (AcceptDrop)
		{
			bool flag = true;
			if (AcceptDropHandler != null)
			{
				flag = AcceptDropHandler(this, EventManager.DraggedWidget);
			}
			if (flag)
			{
				Widget widget = EventManager.ReleaseDraggedWidget();
				int num = -1;
				if (!DropEventHandledManually)
				{
					widget.ParentWidget = this;
				}
				EventFired("Drop", widget, num);
				return true;
			}
		}
		return false;
	}

	protected internal virtual void OnMouseScroll()
	{
		EventFired("MouseScroll");
	}

	protected internal virtual void OnRightStickMovement()
	{
	}

	protected internal virtual void OnDragHoverBegin()
	{
		EventFired("DragHoverBegin");
	}

	protected internal virtual void OnDragHoverEnd()
	{
		EventFired("DragHoverEnd");
	}

	protected internal virtual void OnGainFocus()
	{
		IsFocused = true;
		EventFired("FocusGained");
	}

	protected internal virtual void OnLoseFocus()
	{
		IsFocused = false;
		EventFired("FocusLost");
	}

	protected internal virtual void OnMouseOverBegin()
	{
		EventFired("MouseOverBegin");
	}

	protected internal virtual void OnMouseOverEnd()
	{
		EventFired("MouseOverEnd");
	}
}
