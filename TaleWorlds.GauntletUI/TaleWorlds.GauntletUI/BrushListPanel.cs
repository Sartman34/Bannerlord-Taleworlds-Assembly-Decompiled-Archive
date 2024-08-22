using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI;

public class BrushListPanel : ListPanel
{
	private Brush _originalBrush;

	private Brush _clonedBrush;

	private bool _animRestarted;

	private bool _isInsideCache;

	[Editor(false)]
	public Brush Brush
	{
		get
		{
			if (_originalBrush == null)
			{
				_originalBrush = base.Context.DefaultBrush;
				_clonedBrush = _originalBrush.Clone();
				if (BrushRenderer != null)
				{
					BrushRenderer.Brush = ReadOnlyBrush;
				}
			}
			else if (_clonedBrush == null)
			{
				_clonedBrush = _originalBrush.Clone();
				if (BrushRenderer != null)
				{
					BrushRenderer.Brush = ReadOnlyBrush;
				}
			}
			return _clonedBrush;
		}
		set
		{
			if (_originalBrush != value)
			{
				_originalBrush = value;
				_clonedBrush = null;
				OnBrushChanged();
				OnPropertyChanged(value, "Brush");
			}
		}
	}

	public Brush ReadOnlyBrush
	{
		get
		{
			if (_clonedBrush != null)
			{
				return _clonedBrush;
			}
			if (_originalBrush == null)
			{
				_originalBrush = base.Context.DefaultBrush;
			}
			return _originalBrush;
		}
	}

	[Editor(false)]
	public new Sprite Sprite
	{
		get
		{
			return ReadOnlyBrush.DefaultStyle.GetLayer("Default").Sprite;
		}
		set
		{
			Brush.DefaultStyle.GetLayer("Default").Sprite = value;
		}
	}

	public BrushRenderer BrushRenderer { get; private set; }

	public void ForceUseBrush(Brush brush)
	{
		_clonedBrush = brush;
	}

	public BrushListPanel(UIContext context)
		: base(context)
	{
		BrushRenderer = new BrushRenderer();
		base.EventFire += BrushWidget_EventFire;
	}

	private void BrushWidget_EventFire(Widget arg1, string eventName, object[] arg3)
	{
		if (ReadOnlyBrush != null)
		{
			AudioProperty eventAudioProperty = Brush.SoundProperties.GetEventAudioProperty(eventName);
			if (eventAudioProperty != null && eventAudioProperty.AudioName != null && !eventAudioProperty.AudioName.Equals(""))
			{
				base.EventManager.Context.TwoDimensionContext.PlaySound(eventAudioProperty.AudioName);
			}
		}
	}

	public override void UpdateBrushes(float dt)
	{
		if (base.IsVisible)
		{
			Rectangle rectangle = new Rectangle(_cachedGlobalPosition.X, _cachedGlobalPosition.Y, base.MeasuredSize.X, base.MeasuredSize.Y);
			Rectangle other = new Rectangle(base.EventManager.LeftUsableAreaStart, base.EventManager.TopUsableAreaStart, base.EventManager.PageSize.X, base.EventManager.PageSize.Y);
			_isInsideCache = rectangle.IsCollide(other);
			if (_isInsideCache)
			{
				UpdateBrushRendererInternal(dt);
			}
		}
		if (!base.IsVisible || !_isInsideCache || !BrushRenderer.IsUpdateNeeded())
		{
			UnRegisterUpdateBrushes();
		}
	}

	private void UpdateBrushRendererInternal(float dt)
	{
		BrushRenderer.ForcePixelPerfectPlacement = base.ForcePixelPerfectRenderPlacement;
		BrushRenderer.UseLocalTimer = !base.UseGlobalTimeForAnimation;
		BrushRenderer.Brush = ReadOnlyBrush;
		BrushRenderer.CurrentState = base.CurrentState;
		BrushRenderer.Update(base.Context.TwoDimensionContext.Platform.ApplicationTime, dt);
		if (!base.RestartAnimationFirstFrame || _animRestarted)
		{
			return;
		}
		base.EventManager.AddLateUpdateAction(this, delegate
		{
			if (base.RestartAnimationFirstFrame)
			{
				BrushRenderer.RestartAnimation();
			}
		}, 5);
		_animRestarted = true;
	}

	public override void SetState(string stateName)
	{
		if (base.CurrentState != stateName)
		{
			if (base.EventManager != null && ReadOnlyBrush != null)
			{
				AudioProperty stateAudioProperty = Brush.SoundProperties.GetStateAudioProperty(stateName);
				if (stateAudioProperty != null)
				{
					if (stateAudioProperty.AudioName != null && !stateAudioProperty.AudioName.Equals(""))
					{
						base.EventManager.Context.TwoDimensionContext.PlaySound(stateAudioProperty.AudioName);
					}
					else
					{
						Debug.FailedAssert("Widget with id \"" + base.Id + "\" has a sound having no audioName for event \"" + stateName + "\"!", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\Brush\\BrushListPanel.cs", "SetState", 180);
					}
				}
			}
			RegisterUpdateBrushes();
		}
		base.SetState(stateName);
	}

	protected override void RefreshState()
	{
		base.RefreshState();
		RegisterUpdateBrushes();
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		if (!_isInsideCache || BrushRenderer.IsUpdateNeeded())
		{
			HandleUpdateNeededOnRender();
		}
		BrushRenderer.Render(drawContext, _cachedGlobalPosition, base.Size, base._scaleToUse, base.Context.ContextAlpha);
	}

	protected void HandleUpdateNeededOnRender()
	{
		UpdateBrushRendererInternal(base.EventManager.CachedDt);
		if (BrushRenderer.IsUpdateNeeded())
		{
			RegisterUpdateBrushes();
		}
		_isInsideCache = true;
	}

	protected override void OnConnectedToRoot()
	{
		base.OnConnectedToRoot();
		BrushRenderer.SetSeed(_seed);
	}

	public override void UpdateAnimationPropertiesSubTask(float alphaFactor)
	{
		Brush.GlobalAlphaFactor = alphaFactor;
		foreach (Widget child in base.Children)
		{
			child.UpdateAnimationPropertiesSubTask(alphaFactor);
		}
	}

	public virtual void OnBrushChanged()
	{
		RegisterUpdateBrushes();
	}

	private void RegisterUpdateBrushes()
	{
		base.EventManager.RegisterWidgetForEvent(WidgetContainer.ContainerType.UpdateBrushes, this);
	}

	private void UnRegisterUpdateBrushes()
	{
		base.EventManager.UnRegisterWidgetForEvent(WidgetContainer.ContainerType.UpdateBrushes, this);
	}
}
