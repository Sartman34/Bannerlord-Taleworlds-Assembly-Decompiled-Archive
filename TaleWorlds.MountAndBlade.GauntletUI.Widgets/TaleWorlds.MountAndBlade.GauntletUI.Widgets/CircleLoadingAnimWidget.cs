using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class CircleLoadingAnimWidget : Widget
{
	public enum VisualState
	{
		FadeIn,
		Animating,
		FadeOut
	}

	private VisualState _visualState;

	private float _stayStartTime;

	private float _currentAngle;

	private bool _initialized;

	private float _totalTime;

	public float NumOfCirclesInASecond { get; set; } = 0.5f;


	public float NormalAlpha { get; set; } = 0.5f;


	public float FullAlpha { get; set; } = 1f;


	public float CircleRadius { get; set; } = 50f;


	public float StaySeconds { get; set; } = 2f;


	public bool IsMovementEnabled { get; set; } = true;


	public bool IsReverse { get; set; }

	public float FadeInSeconds { get; set; } = 0.2f;


	public float FadeOutSeconds { get; set; } = 0.2f;


	private float CurrentAlpha => GetChild(0).AlphaFactor;

	public CircleLoadingAnimWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnParallelUpdate(float dt)
	{
		base.OnParallelUpdate(dt);
		_totalTime += dt;
		if (!_initialized)
		{
			_visualState = VisualState.FadeIn;
			this.SetGlobalAlphaRecursively(0f);
			_initialized = true;
		}
		if (IsMovementEnabled && base.IsVisible)
		{
			Widget parentWidget = base.ParentWidget;
			if (parentWidget != null && parentWidget.IsVisible)
			{
				UpdateMovementValues(dt);
				UpdateAlphaValues(dt);
			}
		}
	}

	private void UpdateMovementValues(float dt)
	{
		if (IsMovementEnabled)
		{
			float num = 360f / (float)base.ChildCount;
			float num2 = _currentAngle;
			for (int i = 0; i < base.ChildCount; i++)
			{
				float num3 = TaleWorlds.Library.MathF.Cos(num2 * (System.MathF.PI / 180f)) * CircleRadius;
				float num4 = TaleWorlds.Library.MathF.Sin(num2 * (System.MathF.PI / 180f)) * CircleRadius;
				GetChild(i).PositionXOffset = (IsReverse ? num4 : num3);
				GetChild(i).PositionYOffset = (IsReverse ? num3 : num4);
				num2 += num;
				num2 %= 360f;
			}
			_currentAngle += dt * 360f * NumOfCirclesInASecond;
			_currentAngle %= 360f;
		}
	}

	private void UpdateAlphaValues(float dt)
	{
		float alphaFactor = 1f;
		if (_visualState == VisualState.FadeIn)
		{
			alphaFactor = Mathf.Lerp(CurrentAlpha, 1f, dt / FadeInSeconds);
			if (CurrentAlpha >= 0.9f)
			{
				_visualState = VisualState.Animating;
				_stayStartTime = _totalTime;
			}
		}
		else if (_visualState == VisualState.Animating)
		{
			alphaFactor = 1f;
			if (StaySeconds != -1f && _totalTime - _stayStartTime > StaySeconds)
			{
				_visualState = VisualState.FadeOut;
			}
		}
		else if (_visualState == VisualState.FadeOut)
		{
			alphaFactor = Mathf.Lerp(CurrentAlpha, 0f, dt / FadeOutSeconds);
			if (CurrentAlpha <= 0.01f && _totalTime - (_stayStartTime + StaySeconds + FadeOutSeconds) > 3f)
			{
				_visualState = VisualState.FadeIn;
			}
		}
		else
		{
			Debug.FailedAssert("This visual state is not enabled", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\CircleLoadingAnimWidget.cs", "UpdateAlphaValues", 115);
		}
		this.SetGlobalAlphaRecursively(alphaFactor);
	}
}
