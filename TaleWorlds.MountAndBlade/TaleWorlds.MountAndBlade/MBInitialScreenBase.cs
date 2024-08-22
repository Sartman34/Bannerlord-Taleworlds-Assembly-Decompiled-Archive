using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade;

public class MBInitialScreenBase : ScreenBase, IGameStateListener
{
	private Camera _camera;

	protected SceneLayer _sceneLayer;

	private int _frameCountSinceReadyToRender;

	private const int _numOfFramesToWaitAfterReadyToRender = 8;

	private GameEntity _cameraAnimationEntity;

	private Scene _scene;

	private bool _buttonInvokeMessage;

	private string _buttonToInvoke;

	protected InitialState _state { get; private set; }

	public MBInitialScreenBase(InitialState state)
	{
		_state = state;
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
	}

	void IGameStateListener.OnInitialize()
	{
		_state.OnInitialMenuOptionInvoked += OnExecutedInitialStateOption;
	}

	void IGameStateListener.OnFinalize()
	{
		_state.OnInitialMenuOptionInvoked -= OnExecutedInitialStateOption;
	}

	private void OnExecutedInitialStateOption(InitialStateOption target)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		_sceneLayer = new SceneLayer();
		AddLayer(_sceneLayer);
		_sceneLayer.SceneView.SetResolutionScaling(value: true);
		_sceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		_camera = Camera.CreateCamera();
		Common.MemoryCleanupGC();
		if (Game.Current != null)
		{
			Game.Current.Destroy();
		}
		MBMusicManager.Initialize();
	}

	protected override void OnFinalize()
	{
		_camera = null;
		_sceneLayer = null;
		_cameraAnimationEntity = null;
		_scene = null;
		base.OnFinalize();
	}

	protected sealed override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (_buttonInvokeMessage)
		{
			_buttonInvokeMessage = false;
			Module.CurrentModule.ExecuteInitialStateOptionWithId(_buttonToInvoke);
		}
		if (_sceneLayer == null)
		{
			Console.WriteLine("InitialScreen::OnFrameTick scene view null");
		}
		if (_scene == null)
		{
			return;
		}
		if (_sceneLayer != null && _sceneLayer.SceneView.ReadyToRender())
		{
			if (_frameCountSinceReadyToRender > 8)
			{
				Utilities.DisableGlobalLoadingWindow();
				LoadingWindow.DisableGlobalLoadingWindow();
			}
			else
			{
				_frameCountSinceReadyToRender++;
			}
		}
		if (_sceneLayer != null)
		{
			_sceneLayer.SetCamera(_camera);
		}
		SoundManager.SetListenerFrame(_camera.Frame);
		_scene.Tick(dt);
		if (Input.IsKeyDown(InputKey.LeftControl) && Input.IsKeyReleased(InputKey.E))
		{
			OnEditModeEnterPress();
		}
		if (ScreenManager.TopScreen == this)
		{
			OnInitialScreenTick(dt);
		}
	}

	protected virtual void OnInitialScreenTick(float dt)
	{
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (Utilities.renderingActive)
		{
			RefreshScene();
		}
		_frameCountSinceReadyToRender = 0;
		if (NativeConfig.DoLocalizationCheckAtStartup)
		{
			LocalizedTextManager.CheckValidity(new List<string>());
		}
	}

	private void RefreshScene()
	{
		if (_scene == null)
		{
			_scene = Scene.CreateNewScene(initialize_physics: true, enable_decals: false);
			_scene.SetName("MBInitialScreenBase");
			_scene.SetPlaySoundEventsAfterReadyToRender(value: true);
			SceneInitializationData initData = new SceneInitializationData(initializeWithDefaults: true);
			_scene.Read("main_menu_a", ref initData);
			for (int i = 0; i < 40; i++)
			{
				_scene.Tick(0.1f);
			}
			Vec3 dofParams = default(Vec3);
			_scene.FindEntityWithTag("camera_instance").GetCameraParamsFromCameraScript(_camera, ref dofParams);
		}
		SoundManager.SetListenerFrame(_camera.Frame);
		if (_sceneLayer != null)
		{
			_sceneLayer.SetScene(_scene);
			_sceneLayer.SceneView.SetEnable(value: true);
			_sceneLayer.SceneView.SetSceneUsesShadows(value: true);
		}
		_cameraAnimationEntity = GameEntity.CreateEmpty(_scene);
	}

	private void OnSceneEditorWindowOpen()
	{
		GameStateManager.Current.CleanAndPushState(GameStateManager.Current.CreateState<EditorState>());
	}

	protected override void OnDeactivate()
	{
		_sceneLayer.SceneView.SetEnable(value: false);
		_sceneLayer.SceneView.ClearAll(clearScene: true, removeTerrain: true);
		_scene.ManualInvalidate();
		_scene = null;
		base.OnDeactivate();
	}

	protected override void OnPause()
	{
		LoadingWindow.DisableGlobalLoadingWindow();
		base.OnPause();
		if (_scene != null)
		{
			_scene.FinishSceneSounds();
		}
	}

	protected override void OnResume()
	{
		base.OnResume();
		if (_scene != null)
		{
			_ = _frameCountSinceReadyToRender;
			_ = 0;
		}
	}

	public static void DoExitButtonAction()
	{
		MBAPI.IMBScreen.OnExitButtonClick();
	}

	public bool StartedRendering()
	{
		return _sceneLayer.SceneView.ReadyToRender();
	}

	public static void OnEditModeEnterPress()
	{
		MBAPI.IMBScreen.OnEditModeEnterPress();
	}

	public static void OnEditModeEnterRelease()
	{
		MBAPI.IMBScreen.OnEditModeEnterRelease();
	}
}
