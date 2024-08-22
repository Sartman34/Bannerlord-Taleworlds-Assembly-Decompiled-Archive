using System;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class KeybindingPopup
{
	private bool _isActiveFirstFrame;

	private GauntletLayer _gauntletLayer;

	private IGauntletMovie _movie;

	private BindingListStringItem _messageStr;

	private ScreenBase _targetScreen;

	private Action<Key> _onDone;

	public bool IsActive { get; private set; }

	public KeybindingPopup(Action<Key> onDone, ScreenBase targetScreen)
	{
		_onDone = onDone;
		_messageStr = new BindingListStringItem(new TextObject("{=hvaDkG4w}Press any key.").ToString());
		_targetScreen = targetScreen;
	}

	public void Tick()
	{
		if (!IsActive)
		{
			return;
		}
		if (!_isActiveFirstFrame)
		{
			InputKey firstKeyReleasedInRange = (InputKey)Input.GetFirstKeyReleasedInRange(0);
			if (firstKeyReleasedInRange != InputKey.Invalid)
			{
				_onDone(new Key(firstKeyReleasedInRange));
			}
		}
		else
		{
			_isActiveFirstFrame = false;
		}
	}

	public void OnToggle(bool isActive)
	{
		if (IsActive == isActive)
		{
			return;
		}
		IsActive = isActive;
		if (IsActive)
		{
			_gauntletLayer = new GauntletLayer(4005);
			ScreenManager.TrySetFocus(_gauntletLayer);
			_gauntletLayer.IsFocusLayer = true;
			_gauntletLayer.InputRestrictions.SetInputRestrictions();
			ScreenManager.SetSuspendLayer(_gauntletLayer, isSuspended: false);
			_movie = _gauntletLayer.LoadMovie("KeybindingPopup", _messageStr);
			_targetScreen.AddLayer(_gauntletLayer);
			_isActiveFirstFrame = true;
			return;
		}
		ScreenManager.TryLoseFocus(_gauntletLayer);
		_gauntletLayer.IsFocusLayer = false;
		_gauntletLayer.InputRestrictions.ResetInputRestrictions();
		ScreenManager.SetSuspendLayer(_gauntletLayer, isSuspended: true);
		if (_movie != null)
		{
			_gauntletLayer.ReleaseMovie(_movie);
			_movie = null;
		}
		_targetScreen.RemoveLayer(_gauntletLayer);
		_gauntletLayer = null;
	}
}
