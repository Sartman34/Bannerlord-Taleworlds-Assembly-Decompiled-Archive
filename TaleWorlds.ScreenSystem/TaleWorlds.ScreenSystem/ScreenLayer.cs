using System;
using System.Collections.Generic;
using System.Numerics;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.ScreenSystem;

public abstract class ScreenLayer : IComparable
{
	public readonly string _categoryId;

	public float Scale => ScreenManager.Scale;

	public Vec2 UsableArea => ScreenManager.UsableArea;

	public InputContext Input { get; private set; }

	public InputRestrictions InputRestrictions { get; private set; }

	public string Name { get; set; }

	public bool LastActiveState { get; set; }

	public bool Finalized { get; private set; }

	public bool IsActive { get; private set; }

	public bool MouseEnabled { get; protected internal set; }

	public bool KeyboardEnabled { get; protected internal set; }

	public bool GamepadEnabled { get; protected internal set; }

	public bool IsHitThisFrame { get; internal set; }

	public bool IsFocusLayer { get; set; }

	public CursorType ActiveCursor { get; set; }

	protected InputType _usedInputs { get; set; }

	protected bool? _isMousePressedByThisLayer { get; set; }

	public int ScreenOrderInLastFrame { get; internal set; }

	public InputUsageMask InputUsageMask => InputRestrictions.InputUsageMask;

	protected ScreenLayer(int localOrder, string categoryId)
	{
		InputRestrictions = new InputRestrictions(localOrder);
		Input = new InputContext();
		_categoryId = categoryId;
		Name = "ScreenLayer";
		LastActiveState = true;
		Finalized = false;
		IsActive = false;
		IsFocusLayer = false;
		_usedInputs = InputType.None;
		_isMousePressedByThisLayer = null;
		ActiveCursor = CursorType.Default;
	}

	protected internal virtual void Tick(float dt)
	{
	}

	protected internal virtual void LateTick(float dt)
	{
	}

	protected internal virtual void OnLateUpdate(float dt)
	{
	}

	protected internal virtual void Update(IReadOnlyList<int> lastKeysPressed)
	{
	}

	internal void HandleFinalize()
	{
		OnFinalize();
		Finalized = true;
	}

	protected virtual void OnActivate()
	{
		Finalized = false;
	}

	protected virtual void OnDeactivate()
	{
	}

	protected internal virtual void OnLoseFocus()
	{
	}

	internal void HandleActivate()
	{
		IsActive = true;
		OnActivate();
	}

	internal void HandleDeactivate()
	{
		OnDeactivate();
		IsActive = false;
		ScreenManager.TryLoseFocus(this);
	}

	protected virtual void OnFinalize()
	{
	}

	protected internal virtual void RefreshGlobalOrder(ref int currentOrder)
	{
	}

	public virtual void DrawDebugInfo()
	{
		ScreenManager.EngineInterface.DrawDebugText($"Order: {InputRestrictions.Order}");
		ScreenManager.EngineInterface.DrawDebugText($"Keys Allowed: {Input.IsKeysAllowed}");
		ScreenManager.EngineInterface.DrawDebugText($"Controller Allowed: {Input.IsControllerAllowed}");
		ScreenManager.EngineInterface.DrawDebugText($"Mouse Button Allowed: {Input.IsMouseButtonAllowed}");
		ScreenManager.EngineInterface.DrawDebugText($"Mouse Wheel Allowed: {Input.IsMouseWheelAllowed}");
	}

	public virtual void EarlyProcessEvents(InputType handledInputs, bool? isMousePressed)
	{
		_usedInputs = handledInputs;
		_isMousePressedByThisLayer = isMousePressed;
		if (isMousePressed == true)
		{
			Input.MouseOnMe = true;
		}
		if (Input.MouseOnMe)
		{
			_usedInputs |= InputType.MouseButton;
		}
	}

	public virtual void ProcessEvents()
	{
		Input.IsKeysAllowed = _usedInputs.HasAnyFlag(InputType.Key);
		Input.IsMouseButtonAllowed = _usedInputs.HasAnyFlag(InputType.MouseButton);
		Input.IsMouseWheelAllowed = _usedInputs.HasAnyFlag(InputType.MouseWheel);
	}

	public virtual void LateProcessEvents()
	{
		if (_isMousePressedByThisLayer == false)
		{
			Input.MouseOnMe = false;
		}
	}

	public virtual bool HitTest(Vector2 position)
	{
		return false;
	}

	public virtual bool HitTest()
	{
		return false;
	}

	public virtual bool FocusTest()
	{
		return false;
	}

	public virtual bool IsFocusedOnInput()
	{
		return false;
	}

	public virtual void OnOnScreenKeyboardDone(string inputText)
	{
	}

	public virtual void OnOnScreenKeyboardCanceled()
	{
	}

	public int CompareTo(object obj)
	{
		if (!(obj is ScreenLayer screenLayer))
		{
			return 1;
		}
		if (screenLayer == this)
		{
			return 0;
		}
		if (InputRestrictions.Order == screenLayer.InputRestrictions.Order)
		{
			return InputRestrictions.Id.CompareTo(screenLayer.InputRestrictions.Id);
		}
		return InputRestrictions.Order.CompareTo(screenLayer.InputRestrictions.Order);
	}

	public virtual void UpdateLayout()
	{
	}
}
