using System.Collections.Generic;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade.GameKeyCategory;

public sealed class PhotoModeHotKeyCategory : GameKeyContext
{
	public const string CategoryId = "PhotoModeHotKeyCategory";

	public const int HideUI = 90;

	public const int CameraRollLeft = 91;

	public const int CameraRollRight = 92;

	public const int ToggleCameraFollowMode = 95;

	public const int TakePicture = 93;

	public const int TakePictureWithAdditionalPasses = 94;

	public const int ToggleMouse = 96;

	public const int ToggleVignette = 97;

	public const int ToggleCharacters = 98;

	public const int Reset = 105;

	public const string FasterCamera = "FasterCamera";

	public PhotoModeHotKeyCategory()
		: base("PhotoModeHotKeyCategory", 108)
	{
		RegisterHotKeys();
		RegisterGameKeys();
		RegisterGameAxisKeys();
	}

	private void RegisterHotKeys()
	{
		List<Key> keys = new List<Key>
		{
			new Key(InputKey.LeftShift),
			new Key(InputKey.ControllerRTrigger)
		};
		RegisterHotKey(new HotKey("FasterCamera", "PhotoModeHotKeyCategory", keys));
	}

	private void RegisterGameKeys()
	{
		RegisterGameKey(new GameKey(90, "HideUI", "PhotoModeHotKeyCategory", InputKey.H, InputKey.ControllerRUp, GameKeyMainCategories.PhotoModeCategory));
		RegisterGameKey(new GameKey(91, "CameraRollLeft", "PhotoModeHotKeyCategory", InputKey.Q, InputKey.ControllerLBumper, GameKeyMainCategories.PhotoModeCategory));
		RegisterGameKey(new GameKey(92, "CameraRollRight", "PhotoModeHotKeyCategory", InputKey.E, InputKey.ControllerRBumper, GameKeyMainCategories.PhotoModeCategory));
		RegisterGameKey(new GameKey(95, "ToggleCameraFollowMode", "PhotoModeHotKeyCategory", InputKey.V, InputKey.ControllerRLeft, GameKeyMainCategories.PhotoModeCategory));
		RegisterGameKey(new GameKey(93, "TakePicture", "PhotoModeHotKeyCategory", InputKey.Enter, InputKey.ControllerRDown, GameKeyMainCategories.PhotoModeCategory));
		RegisterGameKey(new GameKey(94, "TakePictureWithAdditionalPasses", "PhotoModeHotKeyCategory", InputKey.BackSpace, InputKey.ControllerRBumper, GameKeyMainCategories.PhotoModeCategory));
		RegisterGameKey(new GameKey(96, "ToggleMouse", "PhotoModeHotKeyCategory", InputKey.C, InputKey.ControllerLThumb, GameKeyMainCategories.PhotoModeCategory));
		RegisterGameKey(new GameKey(97, "ToggleVignette", "PhotoModeHotKeyCategory", InputKey.X, InputKey.ControllerRThumb, GameKeyMainCategories.PhotoModeCategory));
		RegisterGameKey(new GameKey(98, "ToggleCharacters", "PhotoModeHotKeyCategory", InputKey.B, InputKey.ControllerRRight, GameKeyMainCategories.PhotoModeCategory));
		if (NativeOptions.GetConfig(NativeOptions.NativeOptionsType.EnableTouchpadMouse) != 0f)
		{
			RegisterGameKey(new GameKey(105, "Reset", "PhotoModeHotKeyCategory", InputKey.T, InputKey.ControllerLOptionTap, GameKeyMainCategories.PhotoModeCategory));
		}
		else
		{
			RegisterGameKey(new GameKey(105, "Reset", "PhotoModeHotKeyCategory", InputKey.T, InputKey.ControllerLOption, GameKeyMainCategories.PhotoModeCategory));
		}
	}

	private void RegisterGameAxisKeys()
	{
	}
}
