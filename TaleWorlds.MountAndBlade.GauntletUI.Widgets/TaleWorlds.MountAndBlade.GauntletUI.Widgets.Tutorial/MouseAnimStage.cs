using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Tutorial;

public class MouseAnimStage
{
	public enum AnimTypes
	{
		Movement,
		FadeInLocal,
		FadeOutLocal,
		FadeInGlobal,
		FadeOutGlobal,
		Stay
	}

	private float _totalTime;

	public bool IsCompleted { get; private set; }

	public float AnimTime { get; private set; }

	public Vec2 Direction { get; private set; }

	public AnimTypes AnimType { get; private set; }

	public Widget WidgetToManipulate { get; private set; }

	private MouseAnimStage()
	{
	}

	internal static MouseAnimStage CreateMovementStage(float movementTime, Vec2 direction, Widget widgetToManipulate)
	{
		return new MouseAnimStage
		{
			AnimTime = movementTime,
			Direction = direction,
			AnimType = AnimTypes.Movement,
			WidgetToManipulate = widgetToManipulate
		};
	}

	internal static MouseAnimStage CreateFadeInStage(float fadeInTime, Widget widgetToManipulate, bool isGlobal)
	{
		return new MouseAnimStage
		{
			AnimTime = fadeInTime,
			AnimType = ((!isGlobal) ? AnimTypes.FadeInLocal : AnimTypes.FadeInGlobal),
			WidgetToManipulate = widgetToManipulate
		};
	}

	internal static MouseAnimStage CreateFadeOutStage(float fadeOutTime, Widget widgetToManipulate, bool isGlobal)
	{
		return new MouseAnimStage
		{
			AnimTime = fadeOutTime,
			AnimType = (isGlobal ? AnimTypes.FadeOutGlobal : AnimTypes.FadeOutLocal),
			WidgetToManipulate = widgetToManipulate
		};
	}

	internal static MouseAnimStage CreateStayStage(float stayTime)
	{
		return new MouseAnimStage
		{
			AnimTime = stayTime,
			AnimType = AnimTypes.Stay,
			WidgetToManipulate = null
		};
	}

	public void Tick(float dt)
	{
		float num = MathF.Clamp(_totalTime / AnimTime, 0f, 1f);
		switch (AnimType)
		{
		case AnimTypes.Movement:
			WidgetToManipulate.PositionXOffset = ((Direction.X != 0f) ? MathF.Lerp(0f, Direction.X, num) : 0f);
			WidgetToManipulate.PositionYOffset = ((Direction.Y != 0f) ? MathF.Lerp(0f, Direction.Y, num) : 0f);
			IsCompleted = _totalTime > AnimTime;
			break;
		case AnimTypes.FadeInLocal:
			WidgetToManipulate.AlphaFactor = num;
			IsCompleted = WidgetToManipulate.AlphaFactor > 0.98f;
			break;
		case AnimTypes.FadeOutLocal:
			WidgetToManipulate.AlphaFactor = 1f - num;
			IsCompleted = WidgetToManipulate.AlphaFactor < 0.02f;
			break;
		case AnimTypes.FadeInGlobal:
			WidgetToManipulate.SetGlobalAlphaRecursively(num);
			IsCompleted = WidgetToManipulate.AlphaFactor > 0.98f;
			break;
		case AnimTypes.FadeOutGlobal:
			WidgetToManipulate.SetGlobalAlphaRecursively(1f - num);
			IsCompleted = WidgetToManipulate.AlphaFactor < 0.02f;
			break;
		case AnimTypes.Stay:
			IsCompleted = _totalTime > AnimTime;
			break;
		}
		_totalTime += dt;
	}
}
