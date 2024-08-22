using System.Collections.Generic;
using System.Linq;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Tutorial;

public class TutorialStickWidget : Widget
{
	private const float LongStayTime = 1f;

	private const float ShortStayTime = 0.1f;

	private const float FadeInTime = 0.15f;

	private const float FadeOutTime = 0.15f;

	private const float SingleMovementDirection = 20f;

	private const float MovementTime = 0.15f;

	private const float ParentActiveAlpha = 0.5f;

	private Queue<List<MouseAnimStage>> _animQueue = new Queue<List<MouseAnimStage>>();

	private int _currentObjectiveType;

	public Widget GhostMouseVisualWidget { get; set; }

	public Widget LeftMouseClickVisualWidget { get; set; }

	public Widget RightMouseClickVisualWidget { get; set; }

	public Widget HorizontalArrowWidget { get; set; }

	public Widget VerticalArrowWidget { get; set; }

	[Editor(false)]
	public int CurrentObjectiveType
	{
		get
		{
			return _currentObjectiveType;
		}
		set
		{
			if (_currentObjectiveType != value)
			{
				_currentObjectiveType = value;
				OnPropertyChanged(value, "CurrentObjectiveType");
				ResetAll();
				UpdateAnimQueue();
			}
		}
	}

	public TutorialStickWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (_animQueue.Count > 0)
		{
			base.IsVisible = true;
			base.ParentWidget.ParentWidget.AlphaFactor = 0.5f;
			_animQueue.Peek().ForEach(delegate(MouseAnimStage a)
			{
				a.Tick(dt);
			});
			if (_animQueue.Peek().All((MouseAnimStage a) => a.IsCompleted))
			{
				_animQueue.Dequeue();
			}
		}
		else
		{
			UpdateAnimQueue();
		}
	}

	private void ResetAll()
	{
		_animQueue.Clear();
		ResetAnim();
		GhostMouseVisualWidget.SetGlobalAlphaRecursively(0f);
		HorizontalArrowWidget.SetGlobalAlphaRecursively(0f);
		VerticalArrowWidget.SetGlobalAlphaRecursively(0f);
		base.ParentWidget.ParentWidget.AlphaFactor = 0f;
	}

	private void ResetAnim()
	{
		base.PositionXOffset = 0f;
		base.PositionYOffset = 0f;
		this.SetGlobalAlphaRecursively(0f);
		RightMouseClickVisualWidget.SetGlobalAlphaRecursively(0f);
		LeftMouseClickVisualWidget.SetGlobalAlphaRecursively(0f);
	}

	private void UpdateAnimQueue()
	{
		ResetAnim();
		switch (CurrentObjectiveType)
		{
		case 1:
			HorizontalArrowWidget.HorizontalFlip = true;
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateFadeInStage(0.15f, this, isGlobal: true),
				MouseAnimStage.CreateFadeInStage(0.15f, GhostMouseVisualWidget, isGlobal: false),
				MouseAnimStage.CreateFadeInStage(0.15f, HorizontalArrowWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateMovementStage(0.15f, new Vec2(-20f, 0f), this),
				MouseAnimStage.CreateFadeInStage(0.15f, LeftMouseClickVisualWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage> { MouseAnimStage.CreateStayStage(1f) });
			break;
		case 2:
			HorizontalArrowWidget.HorizontalFlip = false;
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateFadeInStage(0.15f, this, isGlobal: true),
				MouseAnimStage.CreateFadeInStage(0.15f, GhostMouseVisualWidget, isGlobal: false),
				MouseAnimStage.CreateFadeInStage(0.15f, HorizontalArrowWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateMovementStage(0.15f, new Vec2(20f, 0f), this),
				MouseAnimStage.CreateFadeInStage(0.15f, LeftMouseClickVisualWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage> { MouseAnimStage.CreateStayStage(1f) });
			break;
		case 3:
			VerticalArrowWidget.VerticalFlip = true;
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateFadeInStage(0.15f, this, isGlobal: true),
				MouseAnimStage.CreateFadeInStage(0.15f, GhostMouseVisualWidget, isGlobal: false),
				MouseAnimStage.CreateFadeInStage(0.15f, VerticalArrowWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateMovementStage(0.15f, new Vec2(0f, -20f), this),
				MouseAnimStage.CreateFadeInStage(0.15f, LeftMouseClickVisualWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage> { MouseAnimStage.CreateStayStage(1f) });
			break;
		case 4:
			VerticalArrowWidget.VerticalFlip = false;
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateFadeInStage(0.15f, this, isGlobal: true),
				MouseAnimStage.CreateFadeInStage(0.15f, GhostMouseVisualWidget, isGlobal: false),
				MouseAnimStage.CreateFadeInStage(0.15f, VerticalArrowWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateMovementStage(0.15f, new Vec2(0f, 20f), this),
				MouseAnimStage.CreateFadeInStage(0.15f, LeftMouseClickVisualWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage> { MouseAnimStage.CreateStayStage(1f) });
			break;
		case 5:
			HorizontalArrowWidget.HorizontalFlip = true;
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateFadeInStage(0.15f, this, isGlobal: true),
				MouseAnimStage.CreateFadeInStage(0.15f, GhostMouseVisualWidget, isGlobal: false),
				MouseAnimStage.CreateFadeInStage(0.15f, HorizontalArrowWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateMovementStage(0.15f, new Vec2(-20f, 0f), this),
				MouseAnimStage.CreateFadeInStage(0.15f, RightMouseClickVisualWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage> { MouseAnimStage.CreateStayStage(2f) });
			break;
		case 6:
			HorizontalArrowWidget.HorizontalFlip = false;
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateFadeInStage(0.15f, this, isGlobal: true),
				MouseAnimStage.CreateFadeInStage(0.15f, GhostMouseVisualWidget, isGlobal: false),
				MouseAnimStage.CreateFadeInStage(0.15f, HorizontalArrowWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateMovementStage(0.15f, new Vec2(20f, 0f), this),
				MouseAnimStage.CreateFadeInStage(0.15f, RightMouseClickVisualWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage> { MouseAnimStage.CreateStayStage(2f) });
			break;
		case 7:
			VerticalArrowWidget.VerticalFlip = true;
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateFadeInStage(0.15f, this, isGlobal: true),
				MouseAnimStage.CreateFadeInStage(0.15f, GhostMouseVisualWidget, isGlobal: false),
				MouseAnimStage.CreateFadeInStage(0.15f, VerticalArrowWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateMovementStage(0.15f, new Vec2(0f, -20f), this),
				MouseAnimStage.CreateFadeInStage(0.15f, RightMouseClickVisualWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage> { MouseAnimStage.CreateStayStage(2f) });
			break;
		case 8:
			VerticalArrowWidget.VerticalFlip = false;
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateFadeInStage(0.15f, this, isGlobal: true),
				MouseAnimStage.CreateFadeInStage(0.15f, GhostMouseVisualWidget, isGlobal: false),
				MouseAnimStage.CreateFadeInStage(0.15f, VerticalArrowWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage>
			{
				MouseAnimStage.CreateMovementStage(0.15f, new Vec2(0f, 20f), this),
				MouseAnimStage.CreateFadeInStage(0.15f, RightMouseClickVisualWidget, isGlobal: false)
			});
			_animQueue.Enqueue(new List<MouseAnimStage> { MouseAnimStage.CreateStayStage(2f) });
			break;
		}
	}
}
