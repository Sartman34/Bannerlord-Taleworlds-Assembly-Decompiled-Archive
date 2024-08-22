using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission.NameMarker;

public class NameMarkerScreenWidget : Widget
{
	private const float MinDistanceToFocusSquared = 3600f;

	private List<NameMarkerListPanel> _markers;

	private NameMarkerListPanel _lastFocusedWidget;

	private bool _isMarkersEnabled;

	private float _targetAlphaValue;

	private Widget _markersContainer;

	public bool IsMarkersEnabled
	{
		get
		{
			return _isMarkersEnabled;
		}
		set
		{
			if (_isMarkersEnabled != value)
			{
				_isMarkersEnabled = value;
				OnPropertyChanged(value, "IsMarkersEnabled");
			}
		}
	}

	public float TargetAlphaValue
	{
		get
		{
			return _targetAlphaValue;
		}
		set
		{
			if (_targetAlphaValue != value)
			{
				_targetAlphaValue = value;
				OnPropertyChanged(value, "TargetAlphaValue");
			}
		}
	}

	[Editor(false)]
	public Widget MarkersContainer
	{
		get
		{
			return _markersContainer;
		}
		set
		{
			if (value != _markersContainer)
			{
				if (_markersContainer != null)
				{
					_markersContainer.EventFire += OnMarkersChanged;
				}
				_markersContainer = value;
				if (_markersContainer != null)
				{
					_markersContainer.EventFire += OnMarkersChanged;
				}
				OnPropertyChanged(value, "MarkersContainer");
			}
		}
	}

	public NameMarkerScreenWidget(UIContext context)
		: base(context)
	{
		_markers = new List<NameMarkerListPanel>();
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		float end = (IsMarkersEnabled ? TargetAlphaValue : 0f);
		float amount = MathF.Clamp(dt * 10f, 0f, 1f);
		base.AlphaFactor = Mathf.Lerp(base.AlphaFactor, end, amount);
		bool flag = _markers.Count > 0;
		for (int i = 0; i < _markers.Count; i++)
		{
			_markers[i].Update(dt);
			flag &= _markers[i].TypeVisualWidget.AlphaFactor > 0f;
		}
		if (!flag)
		{
			return;
		}
		_markers.Sort((NameMarkerListPanel m1, NameMarkerListPanel m2) => m1.Rect.Left.CompareTo(m2.Rect.Left));
		for (int j = 0; j < _markers.Count; j++)
		{
			for (int k = j + 1; k < _markers.Count && !(_markers[k].Rect.Left - _markers[j].Rect.Left > _markers[j].Rect.Width); k++)
			{
				if (_markers[j].Rect.IsOverlapping(_markers[k].Rect))
				{
					_markers[k].ScaledPositionXOffset += _markers[j].Rect.Right - _markers[k].Rect.Left;
					_markers[k].UpdateRectangle();
				}
			}
		}
		NameMarkerListPanel nameMarkerListPanel = null;
		float num = 3600f;
		for (int l = 0; l < _markers.Count; l++)
		{
			if (_markers[l].IsInScreenBoundaries)
			{
				NameMarkerListPanel nameMarkerListPanel2 = _markers[l];
				float num2 = base.EventManager.PageSize.X / 2f;
				float num3 = base.EventManager.PageSize.Y / 2f;
				float num4 = Mathf.Abs(num2 - nameMarkerListPanel2.Rect.CenterX);
				float num5 = Mathf.Abs(num3 - nameMarkerListPanel2.Rect.CenterY);
				float num6 = num4 * num4 + num5 * num5;
				if (num6 < num)
				{
					num = num6;
					nameMarkerListPanel = nameMarkerListPanel2;
				}
			}
		}
		if (nameMarkerListPanel != _lastFocusedWidget)
		{
			if (_lastFocusedWidget != null)
			{
				_lastFocusedWidget.IsFocused = false;
			}
			_lastFocusedWidget = nameMarkerListPanel;
			if (_lastFocusedWidget != null)
			{
				_lastFocusedWidget.IsFocused = true;
			}
		}
	}

	private void OnMarkersChanged(Widget widget, string eventName, object[] args)
	{
		if (args.Length == 1 && args[0] is NameMarkerListPanel item)
		{
			if (eventName == "ItemAdd")
			{
				_markers.Add(item);
			}
			else if (eventName == "ItemRemove")
			{
				_markers.Remove(item);
			}
		}
	}
}
