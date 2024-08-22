using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission;

public class AgentHealthWidget : Widget
{
	public class HealthDropData
	{
		public BrushWidget Widget;

		public float LifeTime;

		public HealthDropData(BrushWidget widget, float lifeTime)
		{
			Widget = widget;
			LifeTime = lifeTime;
		}
	}

	private float AnimationDelay = 0.2f;

	private float AnimationDuration = 0.8f;

	private List<HealthDropData> _healtDrops;

	private int _health;

	private int _prevHealth = -1;

	private int _maxHealth;

	private bool _showHealthBar;

	private FillBarWidget _healthBar;

	private Widget _healthDropContainer;

	private Brush _healthDropBrush;

	[Editor(false)]
	public int Health
	{
		get
		{
			return _health;
		}
		set
		{
			if (_health != value)
			{
				_prevHealth = _health;
				_health = value;
				HealthChanged();
				OnPropertyChanged(value, "Health");
			}
		}
	}

	[Editor(false)]
	public int MaxHealth
	{
		get
		{
			return _maxHealth;
		}
		set
		{
			if (_maxHealth != value)
			{
				_maxHealth = value;
				HealthChanged(createDropVisual: false);
				OnPropertyChanged(value, "MaxHealth");
			}
		}
	}

	[Editor(false)]
	public FillBarWidget HealthBar
	{
		get
		{
			return _healthBar;
		}
		set
		{
			if (_healthBar != value)
			{
				_healthBar = value;
				OnPropertyChanged(value, "HealthBar");
			}
		}
	}

	[Editor(false)]
	public Widget HealthDropContainer
	{
		get
		{
			return _healthDropContainer;
		}
		set
		{
			if (_healthDropContainer != value)
			{
				_healthDropContainer = value;
				OnPropertyChanged(value, "HealthDropContainer");
			}
		}
	}

	[Editor(false)]
	public Brush HealthDropBrush
	{
		get
		{
			return _healthDropBrush;
		}
		set
		{
			if (_healthDropBrush != value)
			{
				_healthDropBrush = value;
				OnPropertyChanged(value, "HealthDropBrush");
			}
		}
	}

	[Editor(false)]
	public bool ShowHealthBar
	{
		get
		{
			return _showHealthBar;
		}
		set
		{
			if (_showHealthBar != value)
			{
				_showHealthBar = value;
				OnPropertyChanged(value, "ShowHealthBar");
			}
		}
	}

	public AgentHealthWidget(UIContext context)
		: base(context)
	{
		_healtDrops = new List<HealthDropData>();
		CheckVisibility();
	}

	private void CreateHealthDrop(Widget container, int preHealth, int currentHealth)
	{
		float num = container.Size.X / base._scaleToUse;
		float suggestedWidth = Mathf.Ceil(num * ((float)(preHealth - currentHealth) / (float)_maxHealth));
		float positionXOffset = Mathf.Floor(num * ((float)currentHealth / (float)_maxHealth));
		BrushWidget brushWidget = new BrushWidget(base.Context);
		brushWidget.WidthSizePolicy = SizePolicy.Fixed;
		brushWidget.HeightSizePolicy = SizePolicy.Fixed;
		brushWidget.Brush = HealthDropBrush;
		brushWidget.SuggestedWidth = suggestedWidth;
		brushWidget.SuggestedHeight = brushWidget.ReadOnlyBrush.Sprite.Height;
		brushWidget.HorizontalAlignment = HorizontalAlignment.Left;
		brushWidget.VerticalAlignment = VerticalAlignment.Center;
		brushWidget.PositionXOffset = positionXOffset;
		brushWidget.ParentWidget = container;
		_healtDrops.Add(new HealthDropData(brushWidget, AnimationDelay + AnimationDuration));
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		if (HealthBar != null && HealthBar.IsVisible)
		{
			for (int num = _healtDrops.Count - 1; num >= 0; num--)
			{
				HealthDropData healthDropData = _healtDrops[num];
				healthDropData.LifeTime -= dt;
				if (healthDropData.LifeTime <= 0f)
				{
					HealthDropContainer.RemoveChild(healthDropData.Widget);
					_healtDrops.RemoveAt(num);
				}
				else
				{
					float alphaFactor = Mathf.Min(1f, healthDropData.LifeTime / AnimationDuration);
					healthDropData.Widget.Brush.AlphaFactor = alphaFactor;
				}
			}
		}
		CheckVisibility();
	}

	private void HealthChanged(bool createDropVisual = true)
	{
		HealthBar.MaxAmount = _maxHealth;
		HealthBar.InitialAmount = Health;
		if (_prevHealth > Health)
		{
			CreateHealthDrop(HealthDropContainer, _prevHealth, Health);
		}
	}

	private void CheckVisibility()
	{
		bool flag = ShowHealthBar;
		if (flag)
		{
			flag = (float)_health > 0f || _healtDrops.Count > 0;
		}
		base.IsVisible = flag;
	}
}
