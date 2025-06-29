using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(PhotoModeView))]
public class MissionGauntletPhotoMode : MissionView
{
	private readonly TextObject _screenShotTakenMessage = new TextObject("{=1e12bdjj}Screenshot has been saved in {PATH}");

	private const float _cameraRollAmount = 0.1f;

	private const float _cameraFocusAmount = 0.1f;

	private GauntletLayer _gauntletLayer;

	private PhotoModeVM _dataSource;

	private bool _registered;

	private SpriteCategory _photoModeCategory;

	private float _cameraRoll;

	private bool _photoModeOrbitState;

	private bool _suspended = true;

	private bool _vignetteMode;

	private bool _hideAgentsMode;

	private int _takePhoto = -1;

	private bool _saveAmbientOcclusionPass;

	private bool _saveObjectIdPass;

	private bool _saveShadowPass;

	private bool _prevUIDisabled;

	private bool _prevMouseEnabled;

	private Scene _missionScene => base.MissionScreen.Mission.Scene;

	private InputContext _input => base.MissionScreen.SceneLayer.Input;

	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_photoModeCategory = spriteData.SpriteCategories["ui_photomode"];
		_photoModeCategory.Load(resourceContext, uIResourceDepot);
		_dataSource = new PhotoModeVM(_missionScene, () => _vignetteMode, () => _hideAgentsMode);
		_cameraRoll = 0f;
		_photoModeOrbitState = _missionScene.GetPhotoModeOrbit();
		_vignetteMode = false;
		_hideAgentsMode = false;
		_saveAmbientOcclusionPass = false;
		_saveObjectIdPass = false;
		_saveShadowPass = false;
		_dataSource.AddHotkeyWithForcedName(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("ToggleEscapeMenu"), new TextObject("{=3CsACce8}Exit"));
		_dataSource.AddHotkey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetHotKey("FasterCamera"));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(93));
		if (Utilities.EditModeEnabled)
		{
			_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(94));
		}
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(90));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(96));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(95));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(97));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(98));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(91));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(92));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(105));
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (_takePhoto != -1)
		{
			if (Utilities.GetNumberOfShaderCompilationsInProgress() > 0)
			{
				_takePhoto++;
			}
			else if (_takePhoto > 6)
			{
				if (_saveObjectIdPass)
				{
					string variable = _missionScene.TakePhotoModePicture(saveAmbientOcclusionPass: false, savingObjectIdPass: true, saveShadowPass: false);
					MBDebug.DisableAllUI = _prevUIDisabled;
					_screenShotTakenMessage.SetTextVariable("PATH", variable);
					InformationManager.DisplayMessage(new InformationMessage(_screenShotTakenMessage.ToString()));
					Utilities.SetForceDrawEntityID(value: false);
					Utilities.SetRenderMode(Utilities.EngineRenderDisplayMode.ShowNone);
				}
				_takePhoto = -1;
			}
			else if (_takePhoto == 2)
			{
				string variable2 = _missionScene.TakePhotoModePicture(_saveAmbientOcclusionPass, savingObjectIdPass: false, _saveShadowPass);
				_screenShotTakenMessage.SetTextVariable("PATH", variable2);
				InformationManager.DisplayMessage(new InformationMessage(_screenShotTakenMessage.ToString()));
				if (_saveObjectIdPass)
				{
					Utilities.SetForceDrawEntityID(value: true);
					Utilities.SetRenderMode(Utilities.EngineRenderDisplayMode.ShowMeshId);
					_takePhoto++;
				}
				else
				{
					MBDebug.DisableAllUI = _prevUIDisabled;
					_takePhoto = -1;
				}
			}
			else
			{
				_takePhoto++;
			}
		}
		if (base.MissionScreen.IsPhotoModeEnabled)
		{
			if (_takePhoto != -1)
			{
				return;
			}
			if (!_registered)
			{
				GameKeyContext category = HotKeyManager.GetCategory("GenericPanelGameKeyCategory");
				if (!_input.IsCategoryRegistered(category))
				{
					_input.RegisterHotKeyCategory(category);
				}
				GameKeyContext category2 = HotKeyManager.GetCategory("PhotoModeHotKeyCategory");
				if (!_input.IsCategoryRegistered(category2))
				{
					_input.RegisterHotKeyCategory(category2);
				}
				_registered = true;
			}
			if (_suspended)
			{
				_suspended = false;
				_gauntletLayer = new GauntletLayer(2147483645);
				_dataSource.RefreshValues();
				_gauntletLayer.LoadMovie("PhotoMode", _dataSource);
				_gauntletLayer.InputRestrictions.SetInputRestrictions(isMouseVisible: false, InputUsageMask.Mouse);
				base.MissionScreen.AddLayer(_gauntletLayer);
				GauntletChatLogView.Current.SetCanFocusWhileInMission(canFocusInMission: false);
			}
			if (_input.IsGameKeyPressed(93) && GetCanTakePicture())
			{
				_prevUIDisabled = MBDebug.DisableAllUI;
				MBDebug.DisableAllUI = true;
				_saveAmbientOcclusionPass = false;
				_saveObjectIdPass = false;
				_saveShadowPass = false;
				_takePhoto = 0;
			}
			else if (Utilities.EditModeEnabled && _input.IsGameKeyPressed(94))
			{
				_prevUIDisabled = MBDebug.DisableAllUI;
				MBDebug.DisableAllUI = true;
				_saveAmbientOcclusionPass = true;
				_saveObjectIdPass = Utilities.EditModeEnabled;
				_saveShadowPass = true;
				_takePhoto = 0;
			}
			else if (_input.IsGameKeyPressed(90))
			{
				MBDebug.DisableAllUI = !MBDebug.DisableAllUI;
			}
			else if (_input.IsGameKeyPressed(95))
			{
				_photoModeOrbitState = !_photoModeOrbitState;
				_missionScene.SetPhotoModeOrbit(_photoModeOrbitState);
			}
			else if (_input.IsGameKeyPressed(96))
			{
				base.MissionScreen.SetPhotoModeRequiresMouse(!base.MissionScreen.PhotoModeRequiresMouse);
			}
			else if (_input.IsGameKeyPressed(97))
			{
				_vignetteMode = !_vignetteMode;
				_missionScene.SetPhotoModeVignette(_vignetteMode);
			}
			else if (_input.IsGameKeyPressed(98))
			{
				_hideAgentsMode = !_hideAgentsMode;
				Utilities.SetRenderAgents(!_hideAgentsMode);
			}
			else if (_input.IsGameKeyPressed(105))
			{
				ResetChanges();
			}
			else if (base.MissionScreen.SceneLayer.Input.IsKeyPressed(InputKey.RightMouseButton))
			{
				_prevMouseEnabled = base.MissionScreen.PhotoModeRequiresMouse;
				base.MissionScreen.SetPhotoModeRequiresMouse(isRequired: false);
			}
			else if (base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.RightMouseButton))
			{
				base.MissionScreen.SetPhotoModeRequiresMouse(_prevMouseEnabled);
			}
			if (_input.IsGameKeyDown(91))
			{
				_cameraRoll -= 0.1f;
				_missionScene.SetPhotoModeRoll(_cameraRoll);
			}
			else if (_input.IsGameKeyDown(92))
			{
				_cameraRoll += 0.1f;
				_missionScene.SetPhotoModeRoll(_cameraRoll);
			}
		}
		else if (!_suspended)
		{
			_suspended = true;
			if (_gauntletLayer != null)
			{
				base.MissionScreen.RemoveLayer(_gauntletLayer);
				_gauntletLayer = null;
			}
			GauntletChatLogView.Current.SetCanFocusWhileInMission(canFocusInMission: true);
		}
	}

	private bool GetCanTakePicture()
	{
		if (_gauntletLayer.UIContext.EventManager.IsControllerActive)
		{
			return !base.MissionScreen.PhotoModeRequiresMouse;
		}
		return true;
	}

	private void ResetChanges()
	{
		_photoModeOrbitState = false;
		_missionScene.SetPhotoModeOrbit(_photoModeOrbitState);
		_vignetteMode = false;
		_hideAgentsMode = false;
		_saveAmbientOcclusionPass = false;
		_saveObjectIdPass = false;
		_saveShadowPass = false;
		_missionScene.SetPhotoModeFocus(0f, 0f, 0f, 0f);
		_missionScene.SetPhotoModeVignette(_vignetteMode);
		Utilities.SetRenderAgents(!_hideAgentsMode);
		_cameraRoll = 0f;
		_missionScene.SetPhotoModeRoll(_cameraRoll);
		_dataSource.Reset();
	}

	public override bool OnEscape()
	{
		if (base.MissionScreen.IsPhotoModeEnabled)
		{
			base.MissionScreen.SetPhotoModeEnabled(isEnabled: false);
			base.Mission.IsInPhotoMode = false;
			MBDebug.DisableAllUI = false;
			ResetChanges();
			return true;
		}
		return false;
	}

	public override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return !base.MissionScreen.IsPhotoModeEnabled;
	}

	public override void OnMissionScreenFinalize()
	{
		_gauntletLayer = null;
		_dataSource.OnFinalize();
		_dataSource = null;
		_photoModeCategory.Unload();
		base.OnMissionScreenFinalize();
	}
}
