using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

[GameStateScreen(typeof(MissionState))]
public class MissionScreen : ScreenBase, IMissionSystemHandler, IGameStateListener, IMissionScreen, IMissionListener
{
	public delegate void OnSpectateAgentDelegate(Agent followedAgent);

	public delegate List<Agent> GatherCustomAgentListToSpectateDelegate(Agent forcedAgentToInclude);

	public const int LoadingScreenFramesLeftInitial = 15;

	public Func<BasicCharacterObject> GetSpectatedCharacter;

	private GatherCustomAgentListToSpectateDelegate _gatherCustomAgentListToSpectate;

	public const float MinCameraAddedDistance = 0.7f;

	public const float MinCameraDistanceHardLimit = 0.48f;

	public const float MaxCameraAddedDistance = 2.4f;

	private const int _cheatTimeSpeedRequestId = 1121;

	private const string AttackerCameraEntityTag = "strategyCameraAttacker";

	private const string DefenderCameraEntityTag = "strategyCameraDefender";

	private const string CameraHeightLimiterTag = "camera_height_limiter";

	private float _cameraRayCastOffset;

	private bool _forceCanZoom;

	private ScreenLayer _emptyUILayer;

	public const float DefaultViewAngle = 65f;

	private Camera _customCamera;

	private Vec3[] _cameraNearPlanePoints = new Vec3[4];

	private Vec3[] _cameraBoxPoints = new Vec3[8];

	private Vec3 _cameraTarget;

	private float _cameraBearingDelta;

	private float _cameraElevationDelta;

	private float _cameraSpecialTargetAddedBearing;

	private float _cameraSpecialCurrentAddedBearing;

	private float _cameraSpecialTargetAddedElevation;

	private float _cameraSpecialCurrentAddedElevation;

	private Vec3 _cameraSpecialTargetPositionToAdd;

	private Vec3 _cameraSpecialCurrentPositionToAdd;

	private float _cameraSpecialTargetDistanceToAdd;

	private float _cameraSpecialCurrentDistanceToAdd;

	private bool _cameraAddSpecialMovement;

	private bool _cameraAddSpecialPositionalMovement;

	private bool _cameraApplySpecialMovementsInstantly;

	private float _cameraSpecialCurrentFOV;

	private float _cameraSpecialTargetFOV;

	private float _cameraTargetAddedHeight;

	private float _cameraDeploymentHeightToAdd;

	private float _lastCameraAddedDistance;

	private float _cameraAddedElevation;

	private float _cameraHeightLimit;

	private float _currentViewBlockingBodyCoeff;

	private float _targetViewBlockingBodyCoeff;

	private bool _applySmoothTransitionToVirtualEyeCamera;

	private Vec3 _cameraSpeed;

	private float _cameraSpeedMultiplier;

	private bool _cameraSmoothMode;

	private bool _fixCamera;

	private int _shiftSpeedMultiplier = 3;

	private bool _tickEditor;

	private bool _playerDeploymentCancelled;

	private const float LookUpLimit = (float)Math.PI * 5f / 14f;

	private const float LookDownLimit = -1.3659099f;

	public const float FirstPersonNearClippingDistance = 0.065f;

	public const float ThirdPersonNearClippingDistance = 0.1f;

	public const float FarClippingDistance = 12500f;

	private const float HoldTimeForCameraToggle = 0.5f;

	private bool _zoomToggled;

	private float _zoomToggleTime = float.MaxValue;

	private float _zoomAmount;

	private float _cameraToggleStartTime = float.MaxValue;

	private bool _displayingDialog;

	private MissionMainAgentController _missionMainAgentController;

	private ICameraModeLogic _missionCameraModeLogic;

	private MissionLobbyComponent _missionLobbyComponent;

	private bool _isPlayerAgentAdded = true;

	private bool _isRenderingStarted;

	private bool _onSceneRenderingStartedCalled;

	private int _loadingScreenFramesLeft = 15;

	private bool _resetDraggingMode;

	private bool _rightButtonDraggingMode;

	private Vec2 _clickedPositionPixel = Vec2.Zero;

	private Agent _agentToFollowOverride;

	private Agent _lastFollowedAgent;

	private MissionMultiplayerGameModeBaseClient _mpGameModeBase;

	private bool _isGamepadActive;

	private List<MissionView> _missionViews;

	private MissionState _missionState;

	private bool _isDeactivated;

	public bool LockCameraMovement { get; private set; }

	public OrderFlag OrderFlag { get; set; }

	public Camera CombatCamera { get; private set; }

	public Camera CustomCamera
	{
		get
		{
			return _customCamera;
		}
		set
		{
			_customCamera = value;
		}
	}

	public float CameraBearing { get; private set; }

	public float MaxCameraZoom { get; private set; } = 1f;


	public float CameraElevation { get; private set; }

	public float CameraResultDistanceToTarget { get; private set; }

	public float CameraViewAngle { get; private set; }

	public bool IsPhotoModeEnabled { get; private set; }

	public bool IsConversationActive { get; private set; }

	public bool IsDeploymentActive => Mission.Mode == MissionMode.Deployment;

	public SceneLayer SceneLayer { get; private set; }

	public SceneView SceneView => SceneLayer?.SceneView;

	public Mission Mission { get; private set; }

	public bool IsCheatGhostMode { get; set; }

	public bool IsRadialMenuActive { get; private set; }

	public IInputContext InputManager => Mission.InputManager;

	private bool IsOrderMenuOpen => Mission.IsOrderMenuOpen;

	private bool IsTransferMenuOpen => Mission.IsTransferMenuOpen;

	public Agent LastFollowedAgent
	{
		get
		{
			return _lastFollowedAgent;
		}
		private set
		{
			if (_lastFollowedAgent == value)
			{
				return;
			}
			Agent lastFollowedAgent = _lastFollowedAgent;
			_lastFollowedAgent = value;
			MissionPeer missionPeer = GameNetwork.MyPeer?.GetComponent<MissionPeer>();
			if (GameNetwork.IsMyPeerReady)
			{
				if (missionPeer != null)
				{
					missionPeer.FollowedAgent = _lastFollowedAgent;
				}
				else
				{
					Debug.FailedAssert("MyPeer.IsSynchronized but myMissionPeer == null", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Screens\\MissionScreen.cs", "LastFollowedAgent", 218);
				}
			}
			if (lastFollowedAgent != null)
			{
				this.OnSpectateAgentFocusOut?.Invoke(lastFollowedAgent);
			}
			if (_lastFollowedAgent != null)
			{
				if (_lastFollowedAgent == Mission.MainAgent)
				{
					Agent mainAgent = Mission.MainAgent;
					mainAgent.OnMainAgentWieldedItemChange = (Agent.OnMainAgentWieldedItemChangeDelegate)Delegate.Combine(mainAgent.OnMainAgentWieldedItemChange, new Agent.OnMainAgentWieldedItemChangeDelegate(OnMainAgentWeaponChanged));
					ResetMaxCameraZoom();
				}
				this.OnSpectateAgentFocusIn?.Invoke(_lastFollowedAgent);
			}
			if (_lastFollowedAgent == _agentToFollowOverride)
			{
				_agentToFollowOverride = null;
			}
		}
	}

	public IAgentVisual LastFollowedAgentVisuals { get; set; }

	public override bool MouseVisible => ScreenManager.GetMouseVisibility();

	public bool IsMissionTickable
	{
		get
		{
			if (base.IsActive && Mission != null)
			{
				if (Mission.CurrentState != Mission.State.Continuing)
				{
					return Mission.MissionEnded;
				}
				return true;
			}
			return false;
		}
	}

	public bool PhotoModeRequiresMouse { get; private set; }

	public bool IsFocusLost { get; private set; }

	public event OnSpectateAgentDelegate OnSpectateAgentFocusIn;

	public event OnSpectateAgentDelegate OnSpectateAgentFocusOut;

	float IMissionScreen.GetCameraElevation()
	{
		return CameraElevation;
	}

	public void SetOrderFlagVisibility(bool value)
	{
		if (OrderFlag != null)
		{
			OrderFlag.IsVisible = value;
		}
	}

	public string GetFollowText()
	{
		if (LastFollowedAgent == null)
		{
			return "";
		}
		return LastFollowedAgent.Name.ToString();
	}

	public string GetFollowPartyText()
	{
		if (LastFollowedAgent != null)
		{
			TextObject textObject = new TextObject("{=xsC8Ierj}({BATTLE_COMBATANT})");
			textObject.SetTextVariable("BATTLE_COMBATANT", LastFollowedAgent.Origin.BattleCombatant.Name);
			return textObject.ToString();
		}
		return "";
	}

	public bool SetDisplayDialog(bool value)
	{
		bool result = _displayingDialog != value;
		_displayingDialog = value;
		return result;
	}

	bool IMissionScreen.GetDisplayDialog()
	{
		return _displayingDialog;
	}

	public MissionScreen(MissionState missionState)
	{
		missionState.Handler = this;
		_emptyUILayer = new SceneLayer();
		((SceneLayer)_emptyUILayer).SceneView.SetEnable(value: false);
		_missionState = missionState;
		Mission = missionState.CurrentMission;
		CombatCamera = Camera.CreateCamera();
		_missionViews = new List<MissionView>();
	}

	protected override void OnInitialize()
	{
		MBDebug.Print("-------MissionScreen-OnInitialize");
		base.OnInitialize();
		Module.CurrentModule.SkinsXMLHasChanged += OnSkinsXMLChanged;
		CameraViewAngle = 65f;
		_cameraTarget = new Vec3(0f, 0f, 10f);
		CameraBearing = 0f;
		CameraElevation = -0.2f;
		_cameraBearingDelta = 0f;
		_cameraElevationDelta = 0f;
		_cameraSpecialTargetAddedBearing = 0f;
		_cameraSpecialCurrentAddedBearing = 0f;
		_cameraSpecialTargetAddedElevation = 0f;
		_cameraSpecialCurrentAddedElevation = 0f;
		_cameraSpecialTargetPositionToAdd = Vec3.Zero;
		_cameraSpecialCurrentPositionToAdd = Vec3.Zero;
		_cameraSpecialTargetDistanceToAdd = 0f;
		_cameraSpecialCurrentDistanceToAdd = 0f;
		_cameraSpecialCurrentFOV = 65f;
		_cameraSpecialTargetFOV = 65f;
		_cameraAddedElevation = 0f;
		_cameraTargetAddedHeight = 0f;
		_cameraDeploymentHeightToAdd = 0f;
		_lastCameraAddedDistance = 0f;
		CameraResultDistanceToTarget = 0f;
		_cameraSpeed = Vec3.Zero;
		_cameraSpeedMultiplier = 1f;
		_cameraHeightLimit = 0f;
		_cameraAddSpecialMovement = false;
		_cameraAddSpecialPositionalMovement = false;
		_cameraApplySpecialMovementsInstantly = false;
		_currentViewBlockingBodyCoeff = 1f;
		_targetViewBlockingBodyCoeff = 1f;
		_cameraSmoothMode = false;
		CustomCamera = null;
	}

	private void InitializeMissionView()
	{
		_missionState.Paused = false;
		SceneLayer = new SceneLayer();
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("CombatHotKeyCategory"));
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Cheats"));
		Mission.InputManager = SceneLayer.Input;
		AddLayer(SceneLayer);
		SceneView.SetScene(Mission.Scene);
		SceneView.SetSceneUsesShadows(value: true);
		SceneView.SetAcceptGlobalDebugRenderObjects(value: true);
		SceneView.SetResolutionScaling(value: true);
		_missionMainAgentController = Mission.GetMissionBehavior<MissionMainAgentController>();
		_missionLobbyComponent = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();
		_missionCameraModeLogic = Mission.MissionBehaviors.FirstOrDefault((MissionBehavior b) => b is ICameraModeLogic) as ICameraModeLogic;
		foreach (MissionBehavior missionBehavior in Mission.MissionBehaviors)
		{
			if (missionBehavior is MissionView missionView)
			{
				missionView.OnMissionScreenInitialize();
			}
		}
		Mission.AgentVisualCreator = new AgentVisualsCreator();
		_mpGameModeBase = Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		ActivateLoadingScreen();
		if (Mission != null && Mission.MissionEnded && ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			ScreenManager.TopScreen.DeactivateAllLayers();
			missionScreen.SceneView.SetEnable(value: false);
		}
	}

	protected override void OnResume()
	{
		base.OnResume();
		if (Mission != null && Mission.MissionEnded && ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			ScreenManager.TopScreen.DeactivateAllLayers();
			missionScreen.SceneView.SetEnable(value: false);
		}
	}

	public override void OnFocusChangeOnGameWindow(bool focusGained)
	{
		base.OnFocusChangeOnGameWindow(focusGained);
		if (!LoadingWindow.IsLoadingWindowActive)
		{
			Func<bool> isAnyInquiryActive = InformationManager.IsAnyInquiryActive;
			if (isAnyInquiryActive != null && !isAnyInquiryActive())
			{
				List<MissionBehavior> list = (from v in Mission?.MissionBehaviors?.Where((MissionBehavior v) => v is MissionView)
					orderby ((MissionView)v).ViewOrderPriority
					select v).ToList();
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						(list[i] as MissionView).OnFocusChangeOnGameWindow(focusGained);
					}
				}
			}
		}
		IsFocusLost = !focusGained;
	}

	public bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		List<MissionBehavior> list = (from v in Mission?.MissionBehaviors?.Where((MissionBehavior v) => v is MissionView)
			orderby ((MissionView)v).ViewOrderPriority
			select v).ToList();
		if (list != null)
		{
			foreach (MissionBehavior item in list)
			{
				if (!(item as MissionView).IsOpeningEscapeMenuOnFocusChangeAllowed())
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SetExtraCameraParameters(bool newForceCanZoom, float newCameraRayCastStartingPointOffset)
	{
		_forceCanZoom = newForceCanZoom;
		_cameraRayCastOffset = newCameraRayCastStartingPointOffset;
	}

	public void SetCustomAgentListToSpectateGatherer(GatherCustomAgentListToSpectateDelegate gatherer)
	{
		_gatherCustomAgentListToSpectate = gatherer;
	}

	public void UpdateFreeCamera(MatrixFrame frame)
	{
		CombatCamera.Frame = frame;
		Vec3 v = -frame.rotation.u;
		CameraBearing = v.RotationZ;
		Vec3 v2 = new Vec3(0f, 0f, 1f);
		CameraElevation = TaleWorlds.Library.MathF.Acos(Vec3.DotProduct(v2, v)) - (float)Math.PI / 2f;
	}

	protected override void OnFrameTick(float dt)
	{
		if (SceneLayer != null)
		{
			bool flag = MBDebug.IsErrorReportModeActive();
			if (flag)
			{
				_missionState.Paused = MBDebug.IsErrorReportModePauseMission();
			}
			if (base.DebugInput.IsHotKeyPressed("MissionScreenHotkeyFixCamera"))
			{
				_fixCamera = !_fixCamera;
			}
			flag = flag || _fixCamera;
			if (IsPhotoModeEnabled)
			{
				flag = flag || PhotoModeRequiresMouse;
			}
			SceneLayer.InputRestrictions.SetMouseVisibility(flag);
		}
		if (Mission == null)
		{
			return;
		}
		if (IsMissionTickable)
		{
			foreach (MissionView missionView in _missionViews)
			{
				missionView.OnMissionScreenTick(dt);
			}
		}
		HandleInputs();
	}

	private void ActivateMissionView()
	{
		MBDebug.Print("-------MissionScreen-OnActivate");
		Mission.OnMainAgentChanged += Mission_OnMainAgentChanged;
		Mission.OnBeforeAgentRemoved += Mission_OnBeforeAgentRemoved;
		_cameraBearingDelta = 0f;
		_cameraElevationDelta = 0f;
		SetCameraFrameToMapView();
		CheckForUpdateCamera(1E-05f);
		Mission.ResetFirstThirdPersonView();
		if (MBEditor.EditModeEnabled && MBEditor.IsEditModeOn)
		{
			MBEditor.EnterEditMissionMode(Mission);
		}
		foreach (MissionView missionView in _missionViews)
		{
			missionView.OnMissionScreenActivate();
		}
	}

	private void Mission_OnMainAgentChanged(object sender, PropertyChangedEventArgs e)
	{
		if (Mission.MainAgent != null)
		{
			_isPlayerAgentAdded = true;
		}
	}

	private void Mission_OnBeforeAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		if (affectedAgent == _agentToFollowOverride)
		{
			_agentToFollowOverride = null;
		}
		else if (affectedAgent == Mission.MainAgent)
		{
			_agentToFollowOverride = affectorAgent;
		}
	}

	public void OnMainAgentWeaponChanged()
	{
		ResetMaxCameraZoom();
	}

	private void ResetMaxCameraZoom()
	{
		if (LastFollowedAgent == null || LastFollowedAgent != Mission.MainAgent)
		{
			MaxCameraZoom = 1f;
		}
		else
		{
			MaxCameraZoom = ((Mission.Current != null) ? TaleWorlds.Library.MathF.Max(1f, Mission.Current.GetMainAgentMaxCameraZoom()) : 1f);
		}
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		MBDebug.Print("-------MissionScreen-OnDeactivate");
		if (Mission == null)
		{
			return;
		}
		Mission.OnMainAgentChanged -= Mission_OnMainAgentChanged;
		Mission.OnBeforeAgentRemoved -= Mission_OnBeforeAgentRemoved;
		foreach (MissionView missionView in _missionViews)
		{
			missionView.OnMissionScreenDeactivate();
		}
		_isRenderingStarted = false;
		_loadingScreenFramesLeft = 15;
	}

	protected override void OnFinalize()
	{
		MBDebug.Print("-------MissionScreen-OnFinalize");
		Module.CurrentModule.SkinsXMLHasChanged -= OnSkinsXMLChanged;
		LoadingWindow.EnableGlobalLoadingWindow();
		if (Mission != null)
		{
			Mission.InputManager = null;
		}
		Mission = null;
		OrderFlag = null;
		SceneLayer = null;
		_missionMainAgentController = null;
		CombatCamera = null;
		_customCamera = null;
		_missionState = null;
		base.OnFinalize();
	}

	private IEnumerable<MissionBehavior> AddDefaultMissionBehaviorsTo(Mission mission, IEnumerable<MissionBehavior> behaviors)
	{
		List<MissionBehavior> list = new List<MissionBehavior>();
		IEnumerable<MissionBehavior> collection = ViewCreatorManager.CreateDefaultMissionBehaviors(mission);
		list.AddRange(collection);
		return behaviors.Concat(list);
	}

	private void OnSkinsXMLChanged()
	{
		foreach (Agent agent in Mission.Current.Agents)
		{
			agent.EquipItemsFromSpawnEquipment(neededBatchedItems: true);
			agent.UpdateAgentProperties();
			agent.AgentVisuals.UpdateSkeletonScale((int)agent.SpawnEquipment.BodyDeformType);
		}
	}

	private void OnSceneRenderingStarted()
	{
		LoadingWindow.DisableGlobalLoadingWindow();
		Utilities.SetScreenTextRenderingState(state: true);
		foreach (MissionView missionView in _missionViews)
		{
			missionView.OnSceneRenderingStarted();
		}
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("fix_camera_toggle", "mission")]
	public static string ToggleFixedMissionCamera(List<string> strings)
	{
		if (ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			SetFixedMissionCameraActive(!missionScreen._fixCamera);
		}
		return "Done";
	}

	public static void SetFixedMissionCameraActive(bool active)
	{
		if (ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			missionScreen._fixCamera = active;
			missionScreen.SceneLayer.InputRestrictions.SetMouseVisibility(missionScreen._fixCamera);
		}
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_shift_camera_speed", "mission")]
	public static string SetShiftCameraSpeed(List<string> strings)
	{
		if (ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			if (strings.Count > 0 && int.TryParse(strings[0], out var result))
			{
				missionScreen._shiftSpeedMultiplier = result;
				return "Done";
			}
			return "Current multiplier is " + missionScreen._shiftSpeedMultiplier;
		}
		return "No Mission Available";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_camera_position", "mission")]
	public static string SetCameraPosition(List<string> strings)
	{
		if (!GameNetwork.IsSessionActive)
		{
			if (strings.Count < 3)
			{
				return "You need to enter 3 arguments.";
			}
			List<float> list = new List<float>();
			for (int i = 0; i < strings.Count; i++)
			{
				float result = 0f;
				if (float.TryParse(strings[i], out result))
				{
					list.Add(result);
					continue;
				}
				return "Argument " + (i + 1) + " is not valid.";
			}
			if (ScreenManager.TopScreen is MissionScreen missionScreen)
			{
				missionScreen.IsCheatGhostMode = true;
				missionScreen.LastFollowedAgent = null;
				missionScreen.CombatCamera.Position = new Vec3(list[0], list[1], list[2]);
				return "Camera position has been set to: " + strings[0] + ", " + strings[1] + ", " + strings[2];
			}
			return "Mission screen not found.";
		}
		return "Does not work on multiplayer.";
	}

	private void CheckForUpdateCamera(float dt)
	{
		if (_fixCamera && !IsPhotoModeEnabled)
		{
			return;
		}
		if (CustomCamera != null)
		{
			if (_zoomAmount > 0f)
			{
				_zoomAmount = MBMath.ClampFloat(_zoomAmount, 0f, 1f);
				float valueTo = 37f / MaxCameraZoom;
				CameraViewAngle = MBMath.Lerp(Mission.GetFirstPersonFov(), valueTo, _zoomAmount, 0.005f);
				CustomCamera.SetFovVertical(_cameraSpecialCurrentFOV * (CameraViewAngle / 65f) * ((float)Math.PI / 180f), Screen.AspectRatio, 0.065f, 12500f);
			}
			CombatCamera.FillParametersFrom(CustomCamera);
			if (CustomCamera.Entity != null)
			{
				MatrixFrame globalFrame = CustomCamera.Entity.GetGlobalFrame();
				globalFrame.rotation.MakeUnit();
				CombatCamera.Frame = globalFrame;
			}
			SceneView.SetCamera(CombatCamera);
			SoundManager.SetListenerFrame(CombatCamera.Frame);
			return;
		}
		bool flag = false;
		foreach (MissionBehavior missionBehavior in Mission.MissionBehaviors)
		{
			if (missionBehavior is MissionView missionView)
			{
				flag = flag || missionView.UpdateOverridenCamera(dt);
			}
		}
		if (!flag)
		{
			UpdateCamera(dt);
		}
	}

	private void UpdateDragData()
	{
		if (_resetDraggingMode)
		{
			_rightButtonDraggingMode = false;
			_resetDraggingMode = false;
		}
		else if (SceneLayer.Input.IsKeyReleased(InputKey.RightMouseButton))
		{
			_resetDraggingMode = true;
		}
		else if (SceneLayer.Input.IsKeyPressed(InputKey.RightMouseButton))
		{
			_clickedPositionPixel = SceneLayer.Input.GetMousePositionPixel();
		}
		else if (SceneLayer.Input.IsKeyDown(InputKey.RightMouseButton) && !SceneLayer.Input.IsKeyReleased(InputKey.RightMouseButton) && SceneLayer.Input.GetMousePositionPixel().DistanceSquared(_clickedPositionPixel) > 10f && !_rightButtonDraggingMode)
		{
			_rightButtonDraggingMode = true;
		}
	}

	private void UpdateCamera(float dt)
	{
		Scene scene = Mission.Scene;
		bool photoModeOrbit = scene.GetPhotoModeOrbit();
		float num = (IsPhotoModeEnabled ? scene.GetPhotoModeFov() : 0f);
		bool flag = _isGamepadActive && PhotoModeRequiresMouse;
		UpdateDragData();
		MatrixFrame cameraFrame = MatrixFrame.Identity;
		MissionPeer missionPeer = ((GameNetwork.MyPeer != null) ? GameNetwork.MyPeer.GetComponent<MissionPeer>() : null);
		Mission.SpectatorData spectatingData = GetSpectatingData(CombatCamera.Frame.origin);
		Agent agentToFollow = spectatingData.AgentToFollow;
		IAgentVisual agentVisualToFollow = spectatingData.AgentVisualToFollow;
		SpectatorCameraTypes cameraType = spectatingData.CameraType;
		bool flag2 = Mission.CameraIsFirstPerson && agentToFollow != null && agentToFollow == Mission.MainAgent;
		float num2 = (flag2 ? Mission.GetFirstPersonFov() : 65f);
		if (IsPhotoModeEnabled)
		{
			CameraViewAngle = num2;
		}
		else
		{
			_zoomAmount = MBMath.ClampFloat(_zoomAmount, 0f, 1f);
			float valueTo = 37f / MaxCameraZoom;
			CameraViewAngle = MBMath.Lerp(num2, valueTo, _zoomAmount, 0.005f);
		}
		if (_missionMainAgentController == null)
		{
			_missionMainAgentController = Mission.GetMissionBehavior<MissionMainAgentController>();
		}
		else
		{
			_missionMainAgentController.IsDisabled = true;
		}
		if (_missionMainAgentController != null && Mission.Mode != MissionMode.Deployment && Mission.MainAgent != null && Mission.MainAgent.IsCameraAttachable())
		{
			_missionMainAgentController.IsDisabled = false;
		}
		bool flag3 = _cameraApplySpecialMovementsInstantly;
		GameEntity collidedEntity;
		if ((IsPhotoModeEnabled && !photoModeOrbit) || (agentToFollow == null && agentVisualToFollow == null))
		{
			float a = 0f - scene.GetPhotoModeRoll();
			cameraFrame.rotation.RotateAboutSide((float)Math.PI / 2f);
			cameraFrame.rotation.RotateAboutForward(CameraBearing);
			cameraFrame.rotation.RotateAboutSide(CameraElevation);
			cameraFrame.rotation.RotateAboutUp(a);
			cameraFrame.origin = CombatCamera.Frame.origin;
			_cameraSpeed *= 1f - 5f * dt;
			_cameraSpeed.x = MBMath.ClampFloat(_cameraSpeed.x, -20f, 20f);
			_cameraSpeed.y = MBMath.ClampFloat(_cameraSpeed.y, -20f, 20f);
			_cameraSpeed.z = MBMath.ClampFloat(_cameraSpeed.z, -20f, 20f);
			if (Game.Current.CheatMode)
			{
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyIncreaseCameraSpeed"))
				{
					_cameraSpeedMultiplier *= 1.5f;
				}
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyDecreaseCameraSpeed"))
				{
					_cameraSpeedMultiplier *= 2f / 3f;
				}
				if (InputManager.IsHotKeyPressed("ResetCameraSpeed"))
				{
					_cameraSpeedMultiplier = 1f;
				}
				if (InputManager.IsControlDown())
				{
					float num3 = SceneLayer.Input.GetDeltaMouseScroll() * (1f / 120f);
					if (num3 > 0.01f)
					{
						_cameraSpeedMultiplier *= 1.25f;
					}
					else if (num3 < -0.01f)
					{
						_cameraSpeedMultiplier *= 0.8f;
					}
				}
			}
			float num4 = 10f * _cameraSpeedMultiplier * ((!IsPhotoModeEnabled) ? 1f : (flag ? 0f : 0.3f));
			if (Mission.Mode == MissionMode.Deployment)
			{
				float groundHeightAtPosition = scene.GetGroundHeightAtPosition(cameraFrame.origin);
				num4 *= TaleWorlds.Library.MathF.Max(1f, 1f + (cameraFrame.origin.z - groundHeightAtPosition - 5f) / 10f);
			}
			if ((!IsPhotoModeEnabled && SceneLayer.Input.IsGameKeyDown(24)) || (IsPhotoModeEnabled && !flag && SceneLayer.Input.IsHotKeyDown("FasterCamera")))
			{
				num4 *= (float)_shiftSpeedMultiplier;
			}
			if (!_cameraSmoothMode)
			{
				_cameraSpeed.x = 0f;
				_cameraSpeed.y = 0f;
				_cameraSpeed.z = 0f;
			}
			if ((!InputManager.IsControlDown() || !InputManager.IsAltDown()) && !LockCameraMovement)
			{
				bool num5 = !_isGamepadActive || Mission.Mode != MissionMode.Deployment || Input.IsKeyDown(InputKey.ControllerLTrigger);
				Vec3 zero = Vec3.Zero;
				if (num5)
				{
					zero.x = SceneLayer.Input.GetGameKeyAxis("MovementAxisX");
					zero.y = SceneLayer.Input.GetGameKeyAxis("MovementAxisY");
					if (TaleWorlds.Library.MathF.Abs(zero.x) < 0.2f)
					{
						zero.x = 0f;
					}
					if (TaleWorlds.Library.MathF.Abs(zero.y) < 0.2f)
					{
						zero.y = 0f;
					}
				}
				if (!_isGamepadActive || (!IsPhotoModeEnabled && Mission.Mode != MissionMode.Deployment && !IsOrderMenuOpen && !IsTransferMenuOpen))
				{
					if (SceneLayer.Input.IsGameKeyDown(14))
					{
						zero.z += 1f;
					}
					if (SceneLayer.Input.IsGameKeyDown(15))
					{
						zero.z -= 1f;
					}
				}
				else if (Mission.Mode == MissionMode.Deployment && SceneLayer.IsHitThisFrame)
				{
					if (SceneLayer.Input.IsKeyDown(InputKey.ControllerRBumper))
					{
						zero.z += 1f;
					}
					if (SceneLayer.Input.IsKeyDown(InputKey.ControllerLBumper))
					{
						zero.z -= 1f;
					}
				}
				if (zero.IsNonZero)
				{
					float val = zero.Normalize();
					zero *= num4 * Math.Min(1f, val);
					_cameraSpeed += zero;
				}
			}
			if (Mission.Mode == MissionMode.Deployment && !IsRadialMenuActive)
			{
				cameraFrame.origin += _cameraSpeed.x * new Vec3(cameraFrame.rotation.s.AsVec2).NormalizedCopy() * dt;
				cameraFrame.origin -= _cameraSpeed.y * new Vec3(cameraFrame.rotation.u.AsVec2).NormalizedCopy() * dt;
				cameraFrame.origin.z += _cameraSpeed.z * dt;
				if (!Game.Current.CheatMode || !InputManager.IsControlDown())
				{
					_cameraDeploymentHeightToAdd += 3f * SceneLayer.Input.GetDeltaMouseScroll() / 120f;
					if (SceneLayer.Input.IsHotKeyDown("DeploymentCameraIsActive"))
					{
						_cameraDeploymentHeightToAdd += 0.05f * Input.MouseMoveY;
					}
				}
				if (TaleWorlds.Library.MathF.Abs(_cameraDeploymentHeightToAdd) > 0.001f)
				{
					cameraFrame.origin.z += _cameraDeploymentHeightToAdd * dt * 10f;
					_cameraDeploymentHeightToAdd = TaleWorlds.Library.MathF.Lerp(_cameraDeploymentHeightToAdd, 0f, 1f - TaleWorlds.Library.MathF.Pow(0.0005f, dt));
				}
				else
				{
					cameraFrame.origin.z += _cameraDeploymentHeightToAdd;
					_cameraDeploymentHeightToAdd = 0f;
				}
			}
			else
			{
				cameraFrame.origin += _cameraSpeed.x * cameraFrame.rotation.s * dt;
				cameraFrame.origin -= _cameraSpeed.y * cameraFrame.rotation.u * dt;
				cameraFrame.origin += _cameraSpeed.z * cameraFrame.rotation.f * dt;
			}
			if (!MBEditor.IsEditModeOn)
			{
				if (!Mission.IsPositionInsideBoundaries(cameraFrame.origin.AsVec2))
				{
					cameraFrame.origin.AsVec2 = Mission.GetClosestBoundaryPosition(cameraFrame.origin.AsVec2);
				}
				if (!GameNetwork.IsMultiplayer && Mission.Mode == MissionMode.Deployment)
				{
					BattleSideEnum side = Mission.PlayerTeam.Side;
					IMissionDeploymentPlan deploymentPlan = Mission.DeploymentPlan;
					if (deploymentPlan.HasDeploymentBoundaries(side))
					{
						Vec2 position = cameraFrame.origin.AsVec2;
						if (!deploymentPlan.IsPositionInsideDeploymentBoundaries(side, in position))
						{
							ref Vec3 origin = ref cameraFrame.origin;
							position = cameraFrame.origin.AsVec2;
							origin.AsVec2 = deploymentPlan.GetClosestDeploymentBoundaryPosition(side, in position);
						}
					}
				}
				float groundHeightAtPosition2 = scene.GetGroundHeightAtPosition((Mission.Mode == MissionMode.Deployment) ? (cameraFrame.origin + new Vec3(0f, 0f, 100f)) : cameraFrame.origin);
				if (!IsCheatGhostMode && groundHeightAtPosition2 < 9999f)
				{
					cameraFrame.origin.z = TaleWorlds.Library.MathF.Max(cameraFrame.origin.z, groundHeightAtPosition2 + 0.5f);
				}
				if (cameraFrame.origin.z > groundHeightAtPosition2 + 80f)
				{
					cameraFrame.origin.z = groundHeightAtPosition2 + 80f;
				}
				if (_cameraHeightLimit > 0f && cameraFrame.origin.z > _cameraHeightLimit)
				{
					cameraFrame.origin.z = _cameraHeightLimit;
				}
				if (cameraFrame.origin.z < -100f)
				{
					cameraFrame.origin.z = -100f;
				}
			}
		}
		else if (flag2 && !IsPhotoModeEnabled)
		{
			Agent agent = agentToFollow;
			if (agentToFollow.AgentVisuals != null)
			{
				if (_cameraAddSpecialMovement)
				{
					if ((Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && _missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == FocusableObjectType.Agent)
					{
						Vec3 vec = (_missionMainAgentController.InteractionComponent.CurrentFocusedObject as Agent).Position - agentToFollow.Position;
						_cameraSpecialTargetFOV = ((65f / CameraViewAngle * TaleWorlds.Library.MathF.Abs(vec.z) < 2f) ? TaleWorlds.Library.MathF.Min((Mission.Mode == MissionMode.Barter) ? 48.75f : 32.5f, ((Mission.Mode == MissionMode.Barter) ? 75f : 50f) / vec.AsVec2.Length) : (160f / vec.AsVec2.Length));
					}
					else
					{
						_cameraSpecialTargetFOV = 65f;
					}
					if (flag3)
					{
						_cameraSpecialCurrentFOV = _cameraSpecialTargetFOV;
					}
				}
				MatrixFrame boneEntitialFrame = agentToFollow.AgentVisuals.GetBoneEntitialFrame(agentToFollow.Monster.ThoraxLookDirectionBoneIndex, useBoneMapping: true);
				MatrixFrame boneEntitialFrame2 = agentToFollow.AgentVisuals.GetBoneEntitialFrame(agentToFollow.Monster.HeadLookDirectionBoneIndex, useBoneMapping: true);
				boneEntitialFrame2.origin = boneEntitialFrame2.TransformToParent(agent.Monster.FirstPersonCameraOffsetWrtHead);
				MatrixFrame frame = agentToFollow.AgentVisuals.GetFrame();
				Vec3 vec2 = frame.TransformToParent(boneEntitialFrame2.origin);
				bool flag4 = (Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && _missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == FocusableObjectType.Agent;
				if ((agent.GetCurrentAnimationFlag(0) & AnimFlags.anf_lock_camera) != (AnimFlags)0uL || (agent.GetCurrentAnimationFlag(1) & AnimFlags.anf_lock_camera) != (AnimFlags)0uL)
				{
					MatrixFrame matrixFrame = frame.TransformToParent(boneEntitialFrame2);
					matrixFrame.rotation.MakeUnit();
					CameraBearing = matrixFrame.rotation.f.RotationZ;
					CameraElevation = matrixFrame.rotation.f.RotationX;
				}
				else if (flag4 || (agentToFollow.IsMainAgent && _missionMainAgentController != null && _missionMainAgentController.CustomLookDir.IsNonZero))
				{
					Vec3 vec3;
					if (flag4)
					{
						Agent agent2 = _missionMainAgentController.InteractionComponent.CurrentFocusedObject as Agent;
						vec3 = (new Vec3(agent2.Position.AsVec2, agent2.AgentVisuals.GetGlobalStableEyePoint(agent2.IsHuman).z) - vec2).NormalizedCopy();
						Vec3 vec4 = new Vec3(vec3.y, 0f - vec3.x).NormalizedCopy();
						vec3 = vec3.RotateAboutAnArbitraryVector(vec4, ((Mission.Mode == MissionMode.Conversation) ? (-0.003f) : (-0.0045f)) * _cameraSpecialCurrentFOV);
					}
					else
					{
						vec3 = _missionMainAgentController.CustomLookDir;
					}
					if (flag3)
					{
						CameraBearing = vec3.RotationZ;
						CameraElevation = vec3.RotationX;
					}
					else
					{
						Mat3 identity = Mat3.Identity;
						identity.RotateAboutUp(CameraBearing);
						identity.RotateAboutSide(CameraElevation);
						Vec3 f = identity.f;
						Vec3 vec5 = Vec3.CrossProduct(f, vec3);
						float num6 = vec5.Normalize();
						Vec3 vec6;
						if (num6 < 0.0001f)
						{
							vec6 = vec3;
						}
						else
						{
							vec6 = f;
							vec6 = vec6.RotateAboutAnArbitraryVector(vec5, num6 * dt * 5f);
						}
						CameraBearing = vec6.RotationZ;
						CameraElevation = vec6.RotationX;
					}
				}
				else
				{
					float num7 = MBMath.WrapAngle(CameraBearing);
					float num8 = MBMath.WrapAngle(CameraElevation);
					CalculateNewBearingAndElevationForFirstPerson(agentToFollow, num7, num8, out var newBearing, out var newElevation);
					CameraBearing = MBMath.LerpRadians(num7, newBearing, Math.Min(dt * 12f, 1f), 1E-05f, 0.5f);
					CameraElevation = MBMath.LerpRadians(num8, newElevation, Math.Min(dt * 12f, 1f), 1E-05f, 0.5f);
				}
				cameraFrame.rotation.RotateAboutSide((float)Math.PI / 2f);
				cameraFrame.rotation.RotateAboutForward(CameraBearing);
				cameraFrame.rotation.RotateAboutSide(CameraElevation);
				float actionChannelWeight = agentToFollow.GetActionChannelWeight(1);
				float f2 = MBMath.WrapAngle(CameraBearing - agentToFollow.MovementDirectionAsAngle);
				float num9 = 1f - (1f - actionChannelWeight) * MBMath.ClampFloat((TaleWorlds.Library.MathF.Abs(f2) - 1f) * 0.66f, 0f, 1f);
				float num10 = 0.25f;
				float num11 = 0.15f;
				float num12 = 0.15f;
				Vec3 vec7 = frame.rotation.u * num10;
				Vec3 vec8 = frame.rotation.u * num11 + Vec3.Forward * num12;
				vec8.RotateAboutX(MBMath.ClampFloat(CameraElevation, -0.35f, 0.35f));
				vec8.RotateAboutZ(CameraBearing);
				Vec3 vec9 = frame.TransformToParent(boneEntitialFrame.origin);
				vec9 += vec7;
				vec9 += vec8;
				if (actionChannelWeight > 0f)
				{
					_currentViewBlockingBodyCoeff = (_targetViewBlockingBodyCoeff = 1f);
					_applySmoothTransitionToVirtualEyeCamera = true;
				}
				else
				{
					Vec3 closestPoint = vec2 - vec9;
					Vec3 vec10 = closestPoint.NormalizedCopy();
					if (Vec3.DotProduct(cameraFrame.rotation.u, vec10) > 0f)
					{
						vec10 = -vec10;
					}
					float num13 = 0.97499996f;
					float num14 = TaleWorlds.Library.MathF.Lerp(0.55f, 0.7f, TaleWorlds.Library.MathF.Abs(cameraFrame.rotation.u.z));
					if (Mission.Scene.RayCastForClosestEntityOrTerrain(vec2 - vec10 * (num13 * num14), vec2 + vec10 * (num13 * (1f - num14)), out var collisionDistance, out closestPoint, out collidedEntity, 0.01f, BodyFlags.CameraCollisionRayCastExludeFlags | BodyFlags.DontCollideWithCamera))
					{
						float num15 = (num13 - collisionDistance) / 0.065f;
						_targetViewBlockingBodyCoeff = 1f / TaleWorlds.Library.MathF.Max(1f, num15 * num15 * num15);
					}
					else
					{
						_targetViewBlockingBodyCoeff = 1f;
					}
					if (_currentViewBlockingBodyCoeff < _targetViewBlockingBodyCoeff)
					{
						_currentViewBlockingBodyCoeff = TaleWorlds.Library.MathF.Min(_currentViewBlockingBodyCoeff + dt * 12f, _targetViewBlockingBodyCoeff);
					}
					else if (_currentViewBlockingBodyCoeff > _targetViewBlockingBodyCoeff)
					{
						if (_applySmoothTransitionToVirtualEyeCamera)
						{
							_currentViewBlockingBodyCoeff = TaleWorlds.Library.MathF.Max(_currentViewBlockingBodyCoeff - dt * 6f, _targetViewBlockingBodyCoeff);
						}
						else
						{
							_currentViewBlockingBodyCoeff = _targetViewBlockingBodyCoeff;
						}
					}
					else
					{
						_applySmoothTransitionToVirtualEyeCamera = false;
					}
					num9 *= _currentViewBlockingBodyCoeff;
				}
				cameraFrame.origin.x = MBMath.Lerp(vec9.x, vec2.x, num9);
				cameraFrame.origin.y = MBMath.Lerp(vec9.y, vec2.y, num9);
				cameraFrame.origin.z = MBMath.Lerp(vec9.z, vec2.z, actionChannelWeight);
			}
			else
			{
				cameraFrame = CombatCamera.Frame;
			}
		}
		else
		{
			float num16 = 0.6f;
			float num17 = 0f;
			bool num18 = agentVisualToFollow != null;
			float num19 = 1f;
			bool flag5 = false;
			float num21;
			if (num18)
			{
				_cameraSpecialTargetAddedBearing = 0f;
				_cameraSpecialTargetAddedElevation = 0f;
				_cameraSpecialTargetPositionToAdd = Vec3.Zero;
				_cameraSpecialTargetDistanceToAdd = 0f;
				num16 = 1.25f;
				flag3 = flag3 || agentVisualToFollow != LastFollowedAgentVisuals;
				if (agentVisualToFollow.GetEquipment().Horse.Item != null)
				{
					float num20 = (float)agentVisualToFollow.GetEquipment().Horse.Item.HorseComponent.BodyLength * 0.01f;
					num16 += 2f;
					num21 = 1f * num20 + 0.9f * num19 - 0.2f;
				}
				else
				{
					num21 = 1f * num19;
				}
				CameraBearing = MBMath.WrapAngle(agentVisualToFollow.GetFrame().rotation.f.RotationZ + (float)Math.PI);
				CameraElevation = 0.15f;
			}
			else
			{
				flag5 = agentToFollow.HasMount;
				num19 = agentToFollow.AgentScale;
				flag3 = flag3 || agentToFollow != LastFollowedAgent;
				if ((Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && _missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == FocusableObjectType.Agent)
				{
					Agent agent3 = _missionMainAgentController?.InteractionComponent.CurrentFocusedObject as Agent;
					num21 = (agent3.AgentVisuals.GetGlobalStableEyePoint(isHumanoid: true).z + agentToFollow.AgentVisuals.GetGlobalStableEyePoint(isHumanoid: true).z) * 0.5f - agentToFollow.Position.z;
					if (agent3.HasMount)
					{
						num16 += 0.1f;
					}
					if (Mission.Mode == MissionMode.Barter)
					{
						Vec2 vec11 = agent3.Position.AsVec2 - agentToFollow.Position.AsVec2;
						float length = vec11.Length;
						float num22 = TaleWorlds.Library.MathF.Max(num16 + Mission.CameraAddedDistance, 0.48f) * num19 + length * 0.5f;
						num21 += -0.004f * num22 * _cameraSpecialCurrentFOV;
						Vec3 globalStableEyePoint = agent3.AgentVisuals.GetGlobalStableEyePoint(agent3.IsHuman);
						Vec3 globalStableEyePoint2 = agentToFollow.AgentVisuals.GetGlobalStableEyePoint(agentToFollow.IsHuman);
						float num23 = vec11.RotationInRadians - TaleWorlds.Library.MathF.Min(0.47123894f, 0.4f / length);
						_cameraSpecialTargetAddedBearing = MBMath.WrapAngle(num23 - CameraBearing);
						Vec2 vec12 = new Vec2(globalStableEyePoint.z - globalStableEyePoint2.z, TaleWorlds.Library.MathF.Max(length, 1f));
						float num24 = (flag5 ? (-0.03f) : 0f) - vec12.RotationInRadians;
						_cameraSpecialTargetAddedElevation = num24 - CameraElevation + TaleWorlds.Library.MathF.Asin(-0.2f * (num22 - length * 0.5f) / num22);
					}
				}
				else if (!flag5)
				{
					num21 = ((agentToFollow.AgentVisuals.GetCurrentRagdollState() == RagdollState.Active) ? 0.5f : (((agentToFollow.GetCurrentAnimationFlag(0) & AnimFlags.anf_reset_camera_height) != (AnimFlags)0uL) ? 0.5f : ((!agentToFollow.CrouchMode && !agentToFollow.IsSitting()) ? ((agentToFollow.Monster.StandingEyeHeight + 0.2f) * num19) : ((agentToFollow.Monster.CrouchEyeHeight + 0.2f) * num19))));
				}
				else
				{
					num16 += 0.1f;
					Agent mountAgent = agentToFollow.MountAgent;
					Monster monster = mountAgent.Monster;
					num21 = (monster.RiderCameraHeightAdder + monster.BodyCapsulePoint1.z + monster.BodyCapsuleRadius) * mountAgent.AgentScale + agentToFollow.Monster.CrouchEyeHeight * num19;
				}
				if ((IsViewingCharacter() && (cameraType != SpectatorCameraTypes.LockToTeamMembersView || agentToFollow == Mission.MainAgent)) || IsPhotoModeEnabled)
				{
					num21 *= 0.5f;
					num16 += 0.5f;
				}
				else if (agentToFollow.HasMount && agentToFollow.IsDoingPassiveAttack && (cameraType != SpectatorCameraTypes.LockToTeamMembersView || agentToFollow == Mission.MainAgent))
				{
					num21 *= 1.1f;
				}
				if (_cameraAddSpecialMovement)
				{
					if ((Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && _missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == FocusableObjectType.Agent)
					{
						Agent obj = _missionMainAgentController.InteractionComponent.CurrentFocusedObject as Agent;
						Vec3 globalStableEyePoint3 = obj.AgentVisuals.GetGlobalStableEyePoint(isHumanoid: true);
						Vec3 globalStableEyePoint4 = agentToFollow.AgentVisuals.GetGlobalStableEyePoint(isHumanoid: true);
						Vec2 vec13 = obj.Position.AsVec2 - agentToFollow.Position.AsVec2;
						float length2 = vec13.Length;
						_cameraSpecialTargetPositionToAdd = new Vec3(vec13 * 0.5f);
						_cameraSpecialTargetDistanceToAdd = length2 * (flag5 ? 1.3f : 0.8f) - num16;
						float num25 = vec13.RotationInRadians - TaleWorlds.Library.MathF.Min(0.47123894f, 0.48f / length2);
						_cameraSpecialTargetAddedBearing = MBMath.WrapAngle(num25 - CameraBearing);
						Vec2 vec14 = new Vec2(globalStableEyePoint3.z - globalStableEyePoint4.z, TaleWorlds.Library.MathF.Max(length2, 1f));
						float num26 = (flag5 ? (-0.03f) : 0f) - vec14.RotationInRadians;
						_cameraSpecialTargetAddedElevation = num26 - CameraElevation;
						_cameraSpecialTargetFOV = TaleWorlds.Library.MathF.Min(32.5f, 50f / length2);
					}
					else
					{
						_cameraSpecialTargetPositionToAdd = Vec3.Zero;
						_cameraSpecialTargetDistanceToAdd = 0f;
						_cameraSpecialTargetAddedBearing = 0f;
						_cameraSpecialTargetAddedElevation = 0f;
						_cameraSpecialTargetFOV = 65f;
					}
					if (flag3)
					{
						_cameraSpecialCurrentPositionToAdd = _cameraSpecialTargetPositionToAdd;
						_cameraSpecialCurrentDistanceToAdd = _cameraSpecialTargetDistanceToAdd;
						_cameraSpecialCurrentAddedBearing = _cameraSpecialTargetAddedBearing;
						_cameraSpecialCurrentAddedElevation = _cameraSpecialTargetAddedElevation;
						_cameraSpecialCurrentFOV = _cameraSpecialTargetFOV;
					}
				}
				if (_cameraSpecialCurrentDistanceToAdd != _cameraSpecialTargetDistanceToAdd)
				{
					float num27 = _cameraSpecialTargetDistanceToAdd - _cameraSpecialCurrentDistanceToAdd;
					if (flag3 || TaleWorlds.Library.MathF.Abs(num27) < 0.0001f)
					{
						_cameraSpecialCurrentDistanceToAdd = _cameraSpecialTargetDistanceToAdd;
					}
					else
					{
						float num28 = num27 * 4f * dt;
						_cameraSpecialCurrentDistanceToAdd += num28;
					}
				}
				num16 += _cameraSpecialCurrentDistanceToAdd;
			}
			if (flag3)
			{
				_cameraTargetAddedHeight = num21;
			}
			else
			{
				_cameraTargetAddedHeight += (num21 - _cameraTargetAddedHeight) * dt * 6f * num19;
			}
			if (_cameraSpecialTargetAddedBearing != _cameraSpecialCurrentAddedBearing)
			{
				float num29 = _cameraSpecialTargetAddedBearing - _cameraSpecialCurrentAddedBearing;
				if (flag3 || TaleWorlds.Library.MathF.Abs(num29) < 0.0001f)
				{
					_cameraSpecialCurrentAddedBearing = _cameraSpecialTargetAddedBearing;
				}
				else
				{
					float num30 = num29 * 10f * dt;
					_cameraSpecialCurrentAddedBearing += num30;
				}
			}
			if (_cameraSpecialTargetAddedElevation != _cameraSpecialCurrentAddedElevation)
			{
				float num31 = _cameraSpecialTargetAddedElevation - _cameraSpecialCurrentAddedElevation;
				if (flag3 || TaleWorlds.Library.MathF.Abs(num31) < 0.0001f)
				{
					_cameraSpecialCurrentAddedElevation = _cameraSpecialTargetAddedElevation;
				}
				else
				{
					float num32 = num31 * 8f * dt;
					_cameraSpecialCurrentAddedElevation += num32;
				}
			}
			cameraFrame.rotation.RotateAboutSide((float)Math.PI / 2f);
			if (agentToFollow != null && !agentToFollow.IsMine && cameraType == SpectatorCameraTypes.LockToTeamMembersView)
			{
				Vec3 lookDirection = agentToFollow.LookDirection;
				cameraFrame.rotation.RotateAboutForward(lookDirection.AsVec2.RotationInRadians);
				cameraFrame.rotation.RotateAboutSide(TaleWorlds.Library.MathF.Asin(lookDirection.z));
			}
			else
			{
				cameraFrame.rotation.RotateAboutForward(CameraBearing + _cameraSpecialCurrentAddedBearing);
				cameraFrame.rotation.RotateAboutSide(CameraElevation + _cameraSpecialCurrentAddedElevation);
				if (IsPhotoModeEnabled)
				{
					float a2 = 0f - scene.GetPhotoModeRoll();
					cameraFrame.rotation.RotateAboutUp(a2);
				}
			}
			MatrixFrame matrixFrame2 = cameraFrame;
			float num33 = TaleWorlds.Library.MathF.Max(num16 + Mission.CameraAddedDistance, 0.48f) * num19;
			if (Mission.Mode != MissionMode.Conversation && Mission.Mode != MissionMode.Barter && agentToFollow != null && agentToFollow.IsActive() && BannerlordConfig.EnableVerticalAimCorrection)
			{
				WeaponComponentData currentUsageItem = agentToFollow.WieldedWeapon.CurrentUsageItem;
				if (currentUsageItem != null && currentUsageItem.IsRangedWeapon)
				{
					MatrixFrame frame2 = CombatCamera.Frame;
					frame2.rotation.RotateAboutSide(0f - _cameraAddedElevation);
					float num34;
					if (flag5)
					{
						Agent mountAgent2 = agentToFollow.MountAgent;
						Monster monster2 = mountAgent2.Monster;
						num34 = (monster2.RiderCameraHeightAdder + monster2.BodyCapsulePoint1.z + monster2.BodyCapsuleRadius) * mountAgent2.AgentScale + agentToFollow.Monster.CrouchEyeHeight * num19;
					}
					else
					{
						num34 = agentToFollow.Monster.StandingEyeHeight * num19;
					}
					if (currentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.UseHandAsThrowBase))
					{
						num34 *= 1.25f;
					}
					float num35;
					if (flag3)
					{
						Vec3 vec15 = agentToFollow.Position + cameraFrame.rotation.f * num19 * (0.7f * TaleWorlds.Library.MathF.Pow(TaleWorlds.Library.MathF.Cos(1f / ((num33 / num19 - 0.2f) * 30f + 20f)), 3500f));
						vec15.z += _cameraTargetAddedHeight;
						Vec3 vec16 = vec15 + cameraFrame.rotation.u * num33;
						num35 = vec16.z + (0f - matrixFrame2.rotation.u.z) * (vec16.AsVec2 - agentToFollow.Position.AsVec2).Length - (agentToFollow.Position.z + num34);
					}
					else
					{
						num35 = frame2.origin.z + (0f - frame2.rotation.u.z) * (frame2.origin.AsVec2 - agentToFollow.Position.AsVec2).Length - (agentToFollow.Position.z + num34);
					}
					num17 = ((!(num35 > 0f)) ? 0f : TaleWorlds.Library.MathF.Max(-0.15f, 0f - TaleWorlds.Library.MathF.Asin(TaleWorlds.Library.MathF.Min(1f, TaleWorlds.Library.MathF.Sqrt(19.6f * num35) / (float)agentToFollow.WieldedWeapon.GetModifiedMissileSpeedForCurrentUsage()))));
				}
				else
				{
					num17 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.MeleeAddedElevationForCrosshair);
				}
			}
			if (flag3 || IsPhotoModeEnabled)
			{
				_cameraAddedElevation = num17;
			}
			else
			{
				_cameraAddedElevation += (num17 - _cameraAddedElevation) * dt * 3f;
			}
			if (!IsPhotoModeEnabled)
			{
				cameraFrame.rotation.RotateAboutSide(_cameraAddedElevation);
			}
			bool flag6 = IsViewingCharacter() && !GameNetwork.IsSessionActive;
			bool flag7 = agentToFollow != null && agentToFollow.AgentVisuals != null && agentToFollow.AgentVisuals.GetCurrentRagdollState() != RagdollState.Disabled;
			bool flag8 = agentToFollow != null && agentToFollow.IsActive() && agentToFollow.GetCurrentActionType(0) == Agent.ActionCodeType.Mount;
			Vec2 vec17 = Vec2.Zero;
			Vec3 vec18;
			Vec3 vec19;
			if (num18)
			{
				vec18 = GetPlayerAgentVisuals(missionPeer).GetVisuals()?.GetGlobalFrame().origin ?? missionPeer.ControlledAgent.Position;
				vec19 = vec18;
			}
			else
			{
				vec18 = agentToFollow.VisualPosition;
				vec19 = (flag7 ? agentToFollow.AgentVisuals.GetFrame().origin : vec18);
				if (flag5)
				{
					vec17 = agentToFollow.MountAgent.GetMovementDirection() * agentToFollow.MountAgent.Monster.RiderBodyCapsuleForwardAdder;
					vec19 += vec17.ToVec3();
				}
			}
			if (_cameraAddSpecialPositionalMovement)
			{
				Vec3 vec20 = matrixFrame2.rotation.f * num19 * (0.7f * TaleWorlds.Library.MathF.Pow(TaleWorlds.Library.MathF.Cos(1f / ((num33 / num19 - 0.2f) * 30f + 20f)), 3500f));
				if (Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter)
				{
					_cameraSpecialCurrentPositionToAdd += vec20;
				}
				else
				{
					_cameraSpecialCurrentPositionToAdd -= vec20;
				}
			}
			if (_cameraSpecialCurrentPositionToAdd != _cameraSpecialTargetPositionToAdd)
			{
				Vec3 vec21 = _cameraSpecialTargetPositionToAdd - _cameraSpecialCurrentPositionToAdd;
				if (flag3 || vec21.LengthSquared < 1.0000001E-06f)
				{
					_cameraSpecialCurrentPositionToAdd = _cameraSpecialTargetPositionToAdd;
				}
				else
				{
					_cameraSpecialCurrentPositionToAdd += vec21 * 4f * dt;
				}
			}
			vec18 += _cameraSpecialCurrentPositionToAdd;
			vec19 += _cameraSpecialCurrentPositionToAdd;
			vec19.z += _cameraTargetAddedHeight;
			int num36 = 0;
			bool flag9 = agentToFollow != null;
			Vec3 supportRaycastPoint = ((!flag9) ? Vec3.Invalid : (flag5 ? agentToFollow.MountAgent.GetChestGlobalPosition() : agentToFollow.GetChestGlobalPosition()));
			bool flag10;
			do
			{
				Vec3 vec22 = vec19;
				if (Mission.Mode != MissionMode.Conversation && Mission.Mode != MissionMode.Barter)
				{
					vec22 += matrixFrame2.rotation.f * num19 * (0.7f * TaleWorlds.Library.MathF.Pow(TaleWorlds.Library.MathF.Cos(1f / ((num33 / num19 - 0.2f) * 30f + 20f)), 3500f));
				}
				Vec3 o = vec22 + matrixFrame2.rotation.u * num33;
				if (flag7 || flag8)
				{
					float num37 = 0f;
					if (flag8)
					{
						float currentActionProgress = agentToFollow.GetCurrentActionProgress(0);
						num37 = currentActionProgress * currentActionProgress * 20f;
					}
					vec22 = _cameraTarget + (vec22 - _cameraTarget) * (5f + num37) * dt;
				}
				flag10 = false;
				MatrixFrame cameraFrame2 = new MatrixFrame(cameraFrame.rotation, o);
				Camera.GetNearPlanePointsStatic(ref cameraFrame2, IsPhotoModeEnabled ? (num * ((float)Math.PI / 180f)) : (CameraViewAngle * ((float)Math.PI / 180f)), Screen.AspectRatio, 0.2f, 1f, _cameraNearPlanePoints);
				Vec3 zero2 = Vec3.Zero;
				for (int i = 0; i < 4; i++)
				{
					zero2 += _cameraNearPlanePoints[i];
				}
				zero2 *= 0.25f;
				Vec3 vec23 = new Vec3(vec18.AsVec2 + vec17, vec22.z);
				Vec3 vec24 = vec23 - zero2;
				for (int j = 0; j < 4; j++)
				{
					_cameraNearPlanePoints[j] += vec24;
				}
				_cameraBoxPoints[0] = _cameraNearPlanePoints[3] + cameraFrame2.rotation.u * 0.01f;
				_cameraBoxPoints[1] = _cameraNearPlanePoints[0];
				_cameraBoxPoints[2] = _cameraNearPlanePoints[3];
				_cameraBoxPoints[3] = _cameraNearPlanePoints[2];
				_cameraBoxPoints[4] = _cameraNearPlanePoints[1] + cameraFrame2.rotation.u * 0.01f;
				_cameraBoxPoints[5] = _cameraNearPlanePoints[0] + cameraFrame2.rotation.u * 0.01f;
				_cameraBoxPoints[6] = _cameraNearPlanePoints[1];
				_cameraBoxPoints[7] = _cameraNearPlanePoints[2] + cameraFrame2.rotation.u * 0.01f;
				float num38 = ((IsPhotoModeEnabled && !flag && photoModeOrbit) ? _zoomAmount : 0f);
				num33 += num38;
				if (scene.BoxCastOnlyForCamera(_cameraBoxPoints, vec23, flag9, supportRaycastPoint, cameraFrame2.rotation.u, num33 + 0.5f, out var collisionDistance2, out var closestPoint2, out collidedEntity))
				{
					collisionDistance2 = TaleWorlds.Library.MathF.Max(Vec3.DotProduct(cameraFrame2.rotation.u, closestPoint2 - vec22), 0.48f * num19);
					if (collisionDistance2 < num33)
					{
						flag10 = true;
						num33 = collisionDistance2;
					}
				}
				num36++;
			}
			while (!flag6 && num36 < 5 && flag10);
			num16 = num33 - Mission.CameraAddedDistance;
			if (flag3 || (CameraResultDistanceToTarget > num33 && num36 > 1))
			{
				CameraResultDistanceToTarget = num33;
			}
			else
			{
				float num39 = TaleWorlds.Library.MathF.Max(TaleWorlds.Library.MathF.Abs(Mission.CameraAddedDistance - _lastCameraAddedDistance) * num19, TaleWorlds.Library.MathF.Abs((num16 - (CameraResultDistanceToTarget - _lastCameraAddedDistance)) * dt * 3f * num19));
				CameraResultDistanceToTarget += MBMath.ClampFloat(num33 - CameraResultDistanceToTarget, 0f - num39, num39);
			}
			_lastCameraAddedDistance = Mission.CameraAddedDistance;
			_cameraTarget = vec19;
			if (Mission.Mode != MissionMode.Conversation && Mission.Mode != MissionMode.Barter)
			{
				_cameraTarget += matrixFrame2.rotation.f * num19 * (0.7f * TaleWorlds.Library.MathF.Pow(TaleWorlds.Library.MathF.Cos(1f / ((num33 / num19 - 0.2f) * 30f + 20f)), 3500f));
			}
			cameraFrame.origin = _cameraTarget + matrixFrame2.rotation.u * CameraResultDistanceToTarget;
		}
		if (_cameraSpecialCurrentFOV != _cameraSpecialTargetFOV)
		{
			float num40 = _cameraSpecialTargetFOV - _cameraSpecialCurrentFOV;
			if (flag3 || TaleWorlds.Library.MathF.Abs(num40) < 0.001f)
			{
				_cameraSpecialCurrentFOV = _cameraSpecialTargetFOV;
			}
			else
			{
				_cameraSpecialCurrentFOV += num40 * 3f * dt;
			}
		}
		float newDNear = (Mission.CameraIsFirstPerson ? 0.065f : 0.1f);
		CombatCamera.Frame = cameraFrame;
		if (IsPhotoModeEnabled)
		{
			float focusEnd = 0f;
			float focus = 0f;
			float focusStart = 0f;
			float exposure = 0f;
			bool vignetteOn = false;
			scene.GetPhotoModeFocus(ref focus, ref focusStart, ref focusEnd, ref exposure, ref vignetteOn);
			scene.SetDepthOfFieldFocus(focusEnd);
			scene.SetDepthOfFieldParameters(focus, focusStart, vignetteOn);
		}
		else if ((Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && _missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == FocusableObjectType.Agent)
		{
			Agent agent4 = _missionMainAgentController?.InteractionComponent.CurrentFocusedObject as Agent;
			scene.SetDepthOfFieldParameters(5f, 5f, isVignetteOn: false);
			scene.SetDepthOfFieldFocus((cameraFrame.origin - agent4.AgentVisuals.GetGlobalStableEyePoint(isHumanoid: true)).Length);
		}
		else if (!_zoomAmount.ApproximatelyEqualsTo(1f))
		{
			scene.SetDepthOfFieldParameters(0f, 0f, isVignetteOn: false);
			scene.SetDepthOfFieldFocus(0f);
		}
		CombatCamera.SetFovVertical(IsPhotoModeEnabled ? (num * ((float)Math.PI / 180f)) : (_cameraSpecialCurrentFOV * (CameraViewAngle / 65f) * ((float)Math.PI / 180f)), Screen.AspectRatio, newDNear, 12500f);
		SceneView.SetCamera(CombatCamera);
		Vec3 attenuationPosition = agentToFollow?.GetEyeGlobalPosition() ?? cameraFrame.origin;
		Mission.SetCameraFrame(ref cameraFrame, 65f / CameraViewAngle, ref attenuationPosition);
		if (LastFollowedAgent != null && LastFollowedAgent != Mission.MainAgent && (agentToFollow == Mission.MainAgent || agentToFollow == null))
		{
			this.OnSpectateAgentFocusOut?.Invoke(LastFollowedAgent);
		}
		LastFollowedAgent = agentToFollow;
		LastFollowedAgentVisuals = agentVisualToFollow;
		_cameraApplySpecialMovementsInstantly = false;
		_cameraAddSpecialMovement = false;
		_cameraAddSpecialPositionalMovement = false;
	}

	public bool IsViewingCharacter()
	{
		if (!Mission.CameraIsFirstPerson && !IsOrderMenuOpen)
		{
			return SceneLayer.Input.IsGameKeyDown(25);
		}
		return false;
	}

	private void SetCameraFrameToMapView()
	{
		MatrixFrame frame = MatrixFrame.Identity;
		bool flag = false;
		if (GameNetwork.IsMultiplayer)
		{
			GameEntity gameEntity = Mission.Scene.FindEntityWithTag("mp_camera_start_pos");
			if (gameEntity != null)
			{
				frame = gameEntity.GetGlobalFrame();
				frame.rotation.Orthonormalize();
				CameraBearing = frame.rotation.f.RotationZ;
				CameraElevation = frame.rotation.f.RotationX - (float)Math.PI / 2f;
			}
			else
			{
				Debug.FailedAssert("Multiplayer scene does not contain a camera frame", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Screens\\MissionScreen.cs", "SetCameraFrameToMapView", 2093);
				flag = true;
			}
		}
		else if (Mission.Mode == MissionMode.Deployment)
		{
			bool flag2 = Mission.PlayerTeam.Side == BattleSideEnum.Attacker;
			GameEntity gameEntity2 = ((!flag2) ? (Mission.Scene.FindEntityWithTag("strategyCameraDefender") ?? Mission.Scene.FindEntityWithTag("strategyCameraAttacker")) : (Mission.Scene.FindEntityWithTag("strategyCameraAttacker") ?? Mission.Scene.FindEntityWithTag("strategyCameraDefender")));
			if (gameEntity2 != null)
			{
				frame = gameEntity2.GetGlobalFrame();
				CameraBearing = frame.rotation.f.RotationZ;
				CameraElevation = frame.rotation.f.RotationX - (float)Math.PI / 2f;
			}
			else if (Mission.HasSpawnPath)
			{
				float battleSizeOffset = Mission.GetBattleSizeOffset(100, Mission.GetInitialSpawnPath());
				frame = Mission.GetBattleSideInitialSpawnPathFrame(flag2 ? BattleSideEnum.Attacker : BattleSideEnum.Defender, battleSizeOffset).ToGroundMatrixFrame();
				frame.origin.z += 25f;
				frame.origin -= 25f * frame.rotation.f;
				CameraBearing = frame.rotation.f.RotationZ;
				CameraElevation = -(float)Math.PI / 4f;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			Vec3 min = new Vec3(float.MaxValue, float.MaxValue);
			Vec3 max = new Vec3(float.MinValue, float.MinValue);
			if (Mission.Boundaries.ContainsKey("walk_area"))
			{
				foreach (Vec2 item in Mission.Boundaries["walk_area"])
				{
					min.x = TaleWorlds.Library.MathF.Min(min.x, item.x);
					min.y = TaleWorlds.Library.MathF.Min(min.y, item.y);
					max.x = TaleWorlds.Library.MathF.Max(max.x, item.x);
					max.y = TaleWorlds.Library.MathF.Max(max.y, item.y);
				}
			}
			else
			{
				Mission.Scene.GetBoundingBox(out min, out max);
			}
			Vec3 position = (frame.origin = (min + max) * 0.5f);
			frame.origin.z += 10000f;
			frame.origin.z = Mission.Scene.GetGroundHeightAtPosition(position) + 10f;
		}
		CombatCamera.Frame = frame;
	}

	private bool HandleUserInputDebug()
	{
		bool result = false;
		if (base.DebugInput.IsHotKeyPressed("MissionScreenHotkeyResetDebugVariables"))
		{
			GameNetwork.ResetDebugVariables();
		}
		if (base.DebugInput.IsHotKeyPressed("FixSkeletons"))
		{
			MBCommon.FixSkeletons();
			MessageManager.DisplayMessage("Skeleton models are reloaded...", 4294901760u);
			result = true;
		}
		return result;
	}

	private void HandleUserInput(float dt)
	{
		bool flag = false;
		bool flag2 = _isGamepadActive && PhotoModeRequiresMouse;
		if (Mission == null || Mission.CurrentState == Mission.State.EndingNextFrame)
		{
			return;
		}
		if (!flag && Game.Current.CheatMode)
		{
			flag = HandleUserInputCheatMode(dt);
		}
		if (flag)
		{
			return;
		}
		float num = SceneLayer.Input.GetMouseSensitivity();
		if (!MouseVisible && Mission.MainAgent != null && Mission.MainAgent.State == AgentState.Active && Mission.MainAgent.IsLookRotationInSlowMotion)
		{
			num *= ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.ReducedMouseSensitivityMultiplier);
		}
		float num2 = dt / 0.0009f;
		float num3 = dt / 0.0009f;
		float num4 = 0f;
		float num5 = 0f;
		if ((!MBCommon.IsPaused || IsPhotoModeEnabled) && !IsRadialMenuActive && _cameraSpecialTargetFOV > 9f && Mission.Mode != MissionMode.Barter)
		{
			if (MouseVisible && !SceneLayer.Input.IsKeyDown(InputKey.RightMouseButton))
			{
				if (Mission.Mode != MissionMode.Conversation)
				{
					if (Mission.Mode == MissionMode.Deployment)
					{
						num4 = num2 * SceneLayer.Input.GetGameKeyAxis("CameraAxisX");
						num5 = (0f - num3) * SceneLayer.Input.GetGameKeyAxis("CameraAxisY");
					}
					else
					{
						if (SceneLayer.Input.GetMousePositionRanged().x <= 0.01f)
						{
							num4 = -400f * dt;
						}
						else if (SceneLayer.Input.GetMousePositionRanged().x >= 0.99f)
						{
							num4 = 400f * dt;
						}
						if (SceneLayer.Input.GetMousePositionRanged().y <= 0.01f)
						{
							num5 = -400f * dt;
						}
						else if (SceneLayer.Input.GetMousePositionRanged().y >= 0.99f)
						{
							num5 = 400f * dt;
						}
					}
				}
			}
			else if (!SceneLayer.Input.GetIsMouseActive())
			{
				float gameKeyAxis = SceneLayer.Input.GetGameKeyAxis("CameraAxisX");
				float gameKeyAxis2 = SceneLayer.Input.GetGameKeyAxis("CameraAxisY");
				if (gameKeyAxis > 0.9f || gameKeyAxis < -0.9f)
				{
					num2 = dt / 0.00045f;
				}
				if (gameKeyAxis2 > 0.9f || gameKeyAxis2 < -0.9f)
				{
					num3 = dt / 0.00045f;
				}
				if (_zoomToggled)
				{
					num2 *= BannerlordConfig.ZoomSensitivityModifier;
					num3 *= BannerlordConfig.ZoomSensitivityModifier;
				}
				num4 = num2 * SceneLayer.Input.GetGameKeyAxis("CameraAxisX") + SceneLayer.Input.GetMouseMoveX();
				num5 = (0f - num3) * SceneLayer.Input.GetGameKeyAxis("CameraAxisY") + SceneLayer.Input.GetMouseMoveY();
				if (_missionMainAgentController.IsPlayerAiming && NativeOptions.GetConfig(NativeOptions.NativeOptionsType.EnableGyroAssistedAim) == 1f)
				{
					float config = NativeOptions.GetConfig(NativeOptions.NativeOptionsType.GyroAimSensitivity);
					float gyroX = Input.GetGyroX();
					Input.GetGyroY();
					float gyroZ = Input.GetGyroZ();
					num4 += config * gyroZ * 12f * -1f;
					num5 += config * gyroX * 12f * -1f;
				}
			}
			else
			{
				num4 = SceneLayer.Input.GetMouseMoveX();
				num5 = SceneLayer.Input.GetMouseMoveY();
				if (_zoomAmount > 0.66f)
				{
					num4 *= BannerlordConfig.ZoomSensitivityModifier * _zoomAmount;
					num5 *= BannerlordConfig.ZoomSensitivityModifier * _zoomAmount;
				}
			}
		}
		if (NativeConfig.EnableEditMode && base.DebugInput.IsHotKeyPressed("MissionScreenHotkeySwitchCameraSmooth"))
		{
			_cameraSmoothMode = !_cameraSmoothMode;
			if (_cameraSmoothMode)
			{
				MessageManager.DisplayMessage("Camera smooth mode Enabled.", uint.MaxValue);
			}
			else
			{
				MessageManager.DisplayMessage("Camera smooth mode Disabled.", uint.MaxValue);
			}
		}
		float num6 = 0.0035f;
		float num8;
		if (_cameraSmoothMode)
		{
			num6 *= 0.02f;
			float num7 = 0.02f + dt - 8f * (dt * dt);
			num8 = TaleWorlds.Library.MathF.Max(0f, 1f - 2f * num7);
		}
		else
		{
			num8 = 0f;
		}
		_cameraBearingDelta *= num8;
		_cameraElevationDelta *= num8;
		bool isSessionActive = GameNetwork.IsSessionActive;
		float num9 = num6 * num;
		float num10 = (0f - num4) * num9;
		float num11 = (NativeConfig.InvertMouse ? num5 : (0f - num5)) * num9;
		if (isSessionActive)
		{
			float num12 = 0.3f + 10f * dt;
			num10 = MBMath.ClampFloat(num10, 0f - num12, num12);
			num11 = MBMath.ClampFloat(num11, 0f - num12, num12);
		}
		_cameraBearingDelta += num10;
		_cameraElevationDelta += num11;
		if (isSessionActive)
		{
			float num13 = 0.3f + 10f * dt;
			_cameraBearingDelta = MBMath.ClampFloat(_cameraBearingDelta, 0f - num13, num13);
			_cameraElevationDelta = MBMath.ClampFloat(_cameraElevationDelta, 0f - num13, num13);
		}
		Agent agentToFollow = GetSpectatingData(CombatCamera.Frame.origin).AgentToFollow;
		if (Mission.CameraIsFirstPerson && agentToFollow != null && agentToFollow.Controller == Agent.ControllerType.Player && agentToFollow.HasMount && ((ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.TurnCameraWithHorseInFirstPerson) == 1f && !agentToFollow.WieldedWeapon.IsEmpty && agentToFollow.WieldedWeapon.CurrentUsageItem.IsRangedWeapon) || (ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.TurnCameraWithHorseInFirstPerson) == 2f && (agentToFollow.WieldedWeapon.IsEmpty || agentToFollow.WieldedWeapon.CurrentUsageItem.IsMeleeWeapon)) || ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.TurnCameraWithHorseInFirstPerson) == 3f))
		{
			_cameraBearingDelta += agentToFollow.MountAgent.GetTurnSpeed() * dt;
		}
		if (InputManager.IsGameKeyDown(28))
		{
			Mission.CameraAddedDistance -= 2.1f * dt;
		}
		if (InputManager.IsGameKeyDown(29))
		{
			Mission.CameraAddedDistance += 2.1f * dt;
		}
		Mission.CameraAddedDistance = MBMath.ClampFloat(Mission.CameraAddedDistance, 0.7f, 2.4f);
		_isGamepadActive = !Input.IsMouseActive && Input.IsControllerConnected;
		if (_isGamepadActive)
		{
			Agent mainAgent = Mission.MainAgent;
			if (mainAgent == null || mainAgent.WieldedWeapon.CurrentUsageItem?.IsRangedWeapon != true)
			{
				goto IL_06f8;
			}
		}
		if (!(CustomCamera == null) || IsRadialMenuActive)
		{
			goto IL_06f8;
		}
		int num14 = 1;
		goto IL_0701;
		IL_0701:
		bool flag3 = (byte)num14 != 0;
		if (flag3)
		{
			float applicationTime = Time.ApplicationTime;
			if (SceneLayer.Input.IsHotKeyPressed("ToggleZoom"))
			{
				_zoomToggleTime = applicationTime;
			}
			if (applicationTime - _zoomToggleTime > 0.01f && SceneLayer.Input.IsHotKeyDown("ToggleZoom"))
			{
				_zoomToggleTime = float.MaxValue;
				_zoomToggled = !_zoomToggled;
			}
		}
		else
		{
			_zoomToggled = false;
		}
		bool photoModeOrbit = Mission.Scene.GetPhotoModeOrbit();
		if (IsPhotoModeEnabled)
		{
			if (photoModeOrbit && !flag2)
			{
				_zoomAmount -= SceneLayer.Input.GetDeltaMouseScroll() * 0.002f;
				_zoomAmount = MBMath.ClampFloat(_zoomAmount, 0f, 50f);
			}
		}
		else
		{
			if (agentToFollow != null && agentToFollow.IsMine && (_zoomToggled || (flag3 && SceneLayer.Input.IsGameKeyDown(24))))
			{
				_zoomAmount += 5f * dt;
			}
			else
			{
				_zoomAmount -= 5f * dt;
			}
			_zoomAmount = MBMath.ClampFloat(_zoomAmount, 0f, 1f);
		}
		if (!IsPhotoModeEnabled)
		{
			if (_zoomAmount.ApproximatelyEqualsTo(1f))
			{
				Mission.Scene.SetDepthOfFieldParameters(_zoomAmount * 160f * 110f, _zoomAmount * 1500f * 0.3f, isVignetteOn: false);
			}
			else
			{
				Mission.Scene.SetDepthOfFieldParameters(0f, 0f, isVignetteOn: false);
			}
		}
		Mission.Scene.RayCastForClosestEntityOrTerrain(CombatCamera.Position + CombatCamera.Direction * _cameraRayCastOffset, CombatCamera.Position + CombatCamera.Direction * 3000f, out var collisionDistance);
		Mission.Scene.SetDepthOfFieldFocus(collisionDistance);
		Agent mainAgent2 = Mission.MainAgent;
		if (mainAgent2 != null && !IsPhotoModeEnabled)
		{
			if (_isPlayerAgentAdded)
			{
				_isPlayerAgentAdded = false;
				if (Mission.Mode != MissionMode.Deployment)
				{
					CameraBearing = (Mission.CameraIsFirstPerson ? mainAgent2.LookDirection.RotationZ : mainAgent2.MovementDirectionAsAngle);
					CameraElevation = (Mission.CameraIsFirstPerson ? mainAgent2.LookDirection.RotationX : 0f);
					_cameraSpecialTargetAddedBearing = 0f;
					_cameraSpecialTargetAddedElevation = 0f;
					_cameraSpecialCurrentAddedBearing = 0f;
					_cameraSpecialCurrentAddedElevation = 0f;
				}
			}
			if (Mission.ClearSceneTimerElapsedTime < 0f)
			{
				return;
			}
			if (IsViewingCharacter() || Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter || mainAgent2.IsLookDirectionLocked || _missionMainAgentController?.LockedAgent != null)
			{
				if (Mission.Mode != MissionMode.Barter)
				{
					if (_missionMainAgentController.LockedAgent != null)
					{
						CameraBearing = mainAgent2.LookDirection.RotationZ;
						CameraElevation = mainAgent2.LookDirection.RotationX;
					}
					else
					{
						_cameraSpecialTargetAddedBearing = MBMath.WrapAngle(_cameraSpecialTargetAddedBearing + _cameraBearingDelta);
						_cameraSpecialTargetAddedElevation = MBMath.WrapAngle(_cameraSpecialTargetAddedElevation + _cameraElevationDelta);
						_cameraSpecialCurrentAddedBearing = MBMath.WrapAngle(_cameraSpecialCurrentAddedBearing + _cameraBearingDelta);
						_cameraSpecialCurrentAddedElevation = MBMath.WrapAngle(_cameraSpecialCurrentAddedElevation + _cameraElevationDelta);
					}
				}
				float value = CameraElevation + _cameraSpecialTargetAddedElevation;
				value = MBMath.ClampFloat(value, -1.3659099f, (float)Math.PI * 5f / 14f);
				_cameraSpecialTargetAddedElevation = value - CameraElevation;
				value = CameraElevation + _cameraSpecialCurrentAddedElevation;
				value = MBMath.ClampFloat(value, -1.3659099f, (float)Math.PI * 5f / 14f);
				_cameraSpecialCurrentAddedElevation = value - CameraElevation;
			}
			else
			{
				_cameraSpecialTargetAddedBearing = 0f;
				_cameraSpecialTargetAddedElevation = 0f;
				if (Mission.CameraIsFirstPerson && agentToFollow != null && agentToFollow == Mission.MainAgent && !IsPhotoModeEnabled && !agentToFollow.GetCurrentAnimationFlag(0).HasAnyFlag(AnimFlags.anf_lock_camera) && !agentToFollow.GetCurrentAnimationFlag(1).HasAnyFlag(AnimFlags.anf_lock_camera) && ((Mission.Mode != MissionMode.Conversation && Mission.Mode != MissionMode.Barter) || _missionMainAgentController?.InteractionComponent.CurrentFocusedObject == null || _missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType != FocusableObjectType.Agent) && (_missionMainAgentController == null || !_missionMainAgentController.CustomLookDir.IsNonZero))
				{
					float num15 = MBMath.WrapAngle(CameraBearing + _cameraBearingDelta);
					float num16 = MBMath.WrapAngle(CameraElevation + _cameraElevationDelta);
					CalculateNewBearingAndElevationForFirstPerson(agentToFollow, num15, num16, out var newBearing, out var newElevation);
					if (newBearing != num15)
					{
						_cameraBearingDelta = (MBMath.IsBetween(MBMath.WrapAngle(_cameraBearingDelta), 0f, (float)Math.PI) ? MBMath.ClampFloat(MBMath.WrapAngle(newBearing - CameraBearing), 0f, _cameraBearingDelta) : MBMath.ClampFloat(MBMath.WrapAngle(newBearing - CameraBearing), _cameraBearingDelta, 0f));
					}
					if (newElevation != num16)
					{
						_cameraElevationDelta = (MBMath.IsBetween(MBMath.WrapAngle(_cameraElevationDelta), 0f, (float)Math.PI) ? MBMath.ClampFloat(MBMath.WrapAngle(newElevation - CameraElevation), 0f, _cameraElevationDelta) : MBMath.ClampFloat(MBMath.WrapAngle(newElevation - CameraElevation), _cameraElevationDelta, 0f));
					}
				}
				CameraBearing += _cameraBearingDelta;
				CameraElevation += _cameraElevationDelta;
				CameraElevation = MBMath.ClampFloat(CameraElevation, -1.3659099f, (float)Math.PI * 5f / 14f);
			}
			if (LockCameraMovement)
			{
				_cameraToggleStartTime = float.MaxValue;
			}
			else if (!Input.IsMouseActive)
			{
				float applicationTime2 = Time.ApplicationTime;
				if (SceneLayer.Input.IsGameKeyPressed(27))
				{
					if (SceneLayer.Input.GetGameKeyAxis("MovementAxisX") <= 0.1f && SceneLayer.Input.GetGameKeyAxis("MovementAxisY") <= 0.1f)
					{
						_cameraToggleStartTime = applicationTime2;
					}
				}
				else if (!SceneLayer.Input.IsGameKeyDown(27))
				{
					_cameraToggleStartTime = float.MaxValue;
				}
				if (GetCameraToggleProgress() >= 1f)
				{
					_cameraToggleStartTime = float.MaxValue;
					Mission.CameraIsFirstPerson = !Mission.CameraIsFirstPerson;
					_cameraApplySpecialMovementsInstantly = true;
				}
			}
			else if (SceneLayer.Input.IsGameKeyPressed(27))
			{
				Mission.CameraIsFirstPerson = !Mission.CameraIsFirstPerson;
				_cameraApplySpecialMovementsInstantly = true;
			}
		}
		else
		{
			if (IsPhotoModeEnabled && Mission.CameraIsFirstPerson)
			{
				Mission.CameraIsFirstPerson = false;
			}
			CameraBearing += _cameraBearingDelta;
			CameraElevation += _cameraElevationDelta;
			CameraElevation = MBMath.ClampFloat(CameraElevation, -1.3659099f, (float)Math.PI * 5f / 14f);
		}
		return;
		IL_06f8:
		num14 = (_forceCanZoom ? 1 : 0);
		goto IL_0701;
	}

	public float GetCameraToggleProgress()
	{
		if (_cameraToggleStartTime != float.MaxValue && SceneLayer.Input.IsGameKeyDown(27))
		{
			return (Time.ApplicationTime - _cameraToggleStartTime) / 0.5f;
		}
		return 0f;
	}

	private bool HandleUserInputCheatMode(float dt)
	{
		bool result = false;
		if (!GameNetwork.IsMultiplayer)
		{
			if (InputManager.IsHotKeyPressed("EnterSlowMotion"))
			{
				if (Mission.GetRequestedTimeSpeed(1121, out var _))
				{
					Mission.RemoveTimeSpeedRequest(1121);
				}
				else
				{
					Mission.AddTimeSpeedRequest(new Mission.TimeSpeedRequest(0.1f, 1121));
				}
				result = true;
			}
			if (Mission.GetRequestedTimeSpeed(1121, out var requestedTime2))
			{
				if (InputManager.IsHotKeyDown("MissionScreenHotkeyIncreaseSlowMotionFactor"))
				{
					Mission.RemoveTimeSpeedRequest(1121);
					requestedTime2 = MBMath.ClampFloat(requestedTime2 + 0.5f * dt, 0f, 1f);
					Mission.AddTimeSpeedRequest(new Mission.TimeSpeedRequest(requestedTime2, 1121));
				}
				if (InputManager.IsHotKeyDown("MissionScreenHotkeyDecreaseSlowMotionFactor"))
				{
					Mission.RemoveTimeSpeedRequest(1121);
					requestedTime2 = MBMath.ClampFloat(requestedTime2 - 0.5f * dt, 0f, 1f);
					Mission.AddTimeSpeedRequest(new Mission.TimeSpeedRequest(requestedTime2, 1121));
				}
			}
			if (InputManager.IsHotKeyPressed("Pause"))
			{
				_missionState.Paused = !_missionState.Paused;
				result = true;
			}
			if (InputManager.IsHotKeyPressed("MissionScreenHotkeyHealYourSelf") && Mission.MainAgent != null)
			{
				Mission.MainAgent.Health = Mission.MainAgent.HealthLimit;
				result = true;
			}
			if (InputManager.IsHotKeyPressed("MissionScreenHotkeyHealYourHorse") && Mission.MainAgent?.MountAgent != null)
			{
				Mission.MainAgent.MountAgent.Health = Mission.MainAgent.MountAgent.HealthLimit;
				result = true;
			}
			if (!InputManager.IsShiftDown())
			{
				if (!InputManager.IsAltDown())
				{
					if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillEnemyAgent"))
					{
						return Mission.Current.KillCheats(killAll: false, killEnemy: true, killHorse: false, killYourself: false);
					}
				}
				else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillAllEnemyAgents"))
				{
					return Mission.Current.KillCheats(killAll: true, killEnemy: true, killHorse: false, killYourself: false);
				}
			}
			else if (!InputManager.IsAltDown())
			{
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillEnemyHorse"))
				{
					return Mission.Current.KillCheats(killAll: false, killEnemy: true, killHorse: true, killYourself: false);
				}
			}
			else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillAllEnemyHorses"))
			{
				return Mission.Current.KillCheats(killAll: true, killEnemy: true, killHorse: true, killYourself: false);
			}
			if (!InputManager.IsShiftDown())
			{
				if (!InputManager.IsAltDown())
				{
					if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillFriendlyAgent"))
					{
						return Mission.Current.KillCheats(killAll: false, killEnemy: false, killHorse: false, killYourself: false);
					}
				}
				else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillAllFriendlyAgents"))
				{
					return Mission.Current.KillCheats(killAll: true, killEnemy: false, killHorse: false, killYourself: false);
				}
			}
			else if (!InputManager.IsAltDown())
			{
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillFriendlyHorse"))
				{
					return Mission.Current.KillCheats(killAll: false, killEnemy: false, killHorse: true, killYourself: false);
				}
			}
			else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillAllFriendlyHorses"))
			{
				return Mission.Current.KillCheats(killAll: true, killEnemy: false, killHorse: true, killYourself: false);
			}
			if (!InputManager.IsShiftDown())
			{
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillYourSelf"))
				{
					return Mission.Current.KillCheats(killAll: false, killEnemy: false, killHorse: false, killYourself: true);
				}
			}
			else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillYourHorse"))
			{
				return Mission.Current.KillCheats(killAll: false, killEnemy: false, killHorse: true, killYourself: true);
			}
			if ((GameNetwork.IsServerOrRecorder || !GameNetwork.IsMultiplayer) && InputManager.IsHotKeyPressed("MissionScreenHotkeyGhostCam"))
			{
				IsCheatGhostMode = !IsCheatGhostMode;
			}
		}
		if (!GameNetwork.IsSessionActive)
		{
			if (InputManager.IsHotKeyPressed("MissionScreenHotkeySwitchAgentToAi"))
			{
				Debug.Print("Cheat: SwitchAgentToAi");
				if (Mission.MainAgent != null && Mission.MainAgent.IsActive())
				{
					Mission.MainAgent.Controller = ((Mission.MainAgent.Controller == Agent.ControllerType.Player) ? Agent.ControllerType.AI : Agent.ControllerType.Player);
					result = true;
				}
			}
			if (InputManager.IsHotKeyPressed("MissionScreenHotkeyControlFollowedAgent"))
			{
				Debug.Print("Cheat: ControlFollowedAgent");
				if (Mission.MainAgent != null)
				{
					if (Mission.MainAgent.Controller == Agent.ControllerType.Player)
					{
						Mission.MainAgent.Controller = Agent.ControllerType.AI;
						if (LastFollowedAgent != null)
						{
							LastFollowedAgent.Controller = Agent.ControllerType.Player;
						}
					}
					else
					{
						foreach (Agent agent in Mission.Agents)
						{
							if (agent.Controller == Agent.ControllerType.Player)
							{
								agent.Controller = Agent.ControllerType.AI;
							}
						}
						Mission.MainAgent.Controller = Agent.ControllerType.Player;
					}
					result = true;
				}
				else
				{
					if (LastFollowedAgent != null)
					{
						LastFollowedAgent.Controller = Agent.ControllerType.Player;
					}
					result = true;
				}
			}
		}
		return result;
	}

	public void AddMissionView(MissionView missionView)
	{
		Mission.AddMissionBehavior(missionView);
		RegisterView(missionView);
		missionView.OnMissionScreenInitialize();
		Debug.ReportMemoryBookmark("MissionView Initialized: " + missionView.GetType().Name);
	}

	public void ScreenPointToWorldRay(Vec2 screenPoint, out Vec3 rayBegin, out Vec3 rayEnd)
	{
		rayBegin = Vec3.Invalid;
		rayEnd = Vec3.Invalid;
		Vec2 viewportPoint = SceneView.ScreenPointToViewportPoint(screenPoint);
		CombatCamera.ViewportPointToWorldRay(ref rayBegin, ref rayEnd, viewportPoint);
		float num = -1f;
		foreach (KeyValuePair<string, ICollection<Vec2>> boundary in Mission.Boundaries)
		{
			float boundaryRadius = Mission.Boundaries.GetBoundaryRadius(boundary.Key);
			if (num < boundaryRadius)
			{
				num = boundaryRadius;
			}
		}
		if (num < 0f)
		{
			num = 30f;
		}
		Vec3 vec = rayEnd - rayBegin;
		float a = vec.Normalize();
		rayEnd = rayBegin + vec * TaleWorlds.Library.MathF.Min(a, num);
	}

	public bool GetProjectedMousePositionOnGround(out Vec3 groundPosition, out Vec3 groundNormal, BodyFlags excludeBodyOwnerFlags, bool checkOccludedSurface)
	{
		return SceneView.ProjectedMousePositionOnGround(out groundPosition, out groundNormal, MouseVisible, excludeBodyOwnerFlags, checkOccludedSurface);
	}

	public void CancelQuickPositionOrder()
	{
		if (OrderFlag != null)
		{
			OrderFlag.IsVisible = false;
		}
	}

	public bool MissionStartedRendering()
	{
		if (SceneView != null)
		{
			return SceneView.ReadyToRender();
		}
		return false;
	}

	public Vec3 GetOrderFlagPosition()
	{
		if (OrderFlag != null)
		{
			return OrderFlag.Position;
		}
		return Vec3.Invalid;
	}

	public MatrixFrame GetOrderFlagFrame()
	{
		return OrderFlag.Frame;
	}

	private void ActivateLoadingScreen()
	{
		if (SceneLayer != null && SceneLayer.SceneView != null)
		{
			Scene scene = SceneLayer.SceneView.GetScene();
			if (scene != null)
			{
				scene.PreloadForRendering();
			}
		}
	}

	public void SetRadialMenuActiveState(bool isActive)
	{
		IsRadialMenuActive = isActive;
	}

	public void SetPhotoModeRequiresMouse(bool isRequired)
	{
		PhotoModeRequiresMouse = isRequired;
	}

	public void SetPhotoModeEnabled(bool isEnabled)
	{
		if (IsPhotoModeEnabled == isEnabled || GameNetwork.IsMultiplayer)
		{
			return;
		}
		IsPhotoModeEnabled = isEnabled;
		if (isEnabled)
		{
			MBCommon.PauseGameEngine();
			foreach (MissionView missionView in _missionViews)
			{
				missionView.OnPhotoModeActivated();
			}
		}
		else
		{
			MBCommon.UnPauseGameEngine();
			foreach (MissionView missionView2 in _missionViews)
			{
				missionView2.OnPhotoModeDeactivated();
			}
		}
		Mission.Scene.SetPhotoModeOn(IsPhotoModeEnabled);
	}

	public void SetConversationActive(bool isActive)
	{
		if (IsConversationActive == isActive || GameNetwork.IsMultiplayer)
		{
			return;
		}
		IsConversationActive = isActive;
		foreach (MissionView missionView in _missionViews)
		{
			if (isActive)
			{
				missionView.OnConversationBegin();
			}
			else
			{
				missionView.OnConversationEnd();
			}
		}
	}

	public void SetCameraLockState(bool isLocked)
	{
		LockCameraMovement = isLocked;
	}

	public void RegisterView(MissionView missionView)
	{
		_missionViews.Add(missionView);
		missionView.MissionScreen = this;
	}

	public void UnregisterView(MissionView missionView)
	{
		_missionViews.Remove(missionView);
		missionView.MissionScreen = null;
	}

	public IAgentVisual GetPlayerAgentVisuals(MissionPeer lobbyPeer)
	{
		return lobbyPeer.GetAgentVisualForPeer(0);
	}

	public void SetAgentToFollow(Agent agent)
	{
		_agentToFollowOverride = agent;
	}

	public Mission.SpectatorData GetSpectatingData(Vec3 currentCameraPosition)
	{
		Agent agentToFollow = null;
		IAgentVisual agentVisualToFollow = null;
		SpectatorCameraTypes spectatorCameraTypes = SpectatorCameraTypes.Invalid;
		bool flag = Mission.MainAgent != null && Mission.MainAgent.IsCameraAttachable() && Mission.Mode != MissionMode.Deployment;
		bool flag2 = flag || (LastFollowedAgent != null && LastFollowedAgent.Controller == Agent.ControllerType.Player && LastFollowedAgent.IsCameraAttachable());
		MissionPeer missionPeer = ((GameNetwork.MyPeer != null) ? GameNetwork.MyPeer.GetComponent<MissionPeer>() : null);
		bool flag3 = missionPeer?.HasSpawnedAgentVisuals ?? false;
		bool flag4 = (_missionLobbyComponent != null && (_missionLobbyComponent.MissionType == MultiplayerGameType.Siege || _missionLobbyComponent.MissionType == MultiplayerGameType.TeamDeathmatch)) || Mission.Mode == MissionMode.Deployment;
		SpectatorCameraTypes spectatorCameraTypes2;
		if (!IsCheatGhostMode && !flag2 && flag4 && _agentToFollowOverride != null && _agentToFollowOverride.IsCameraAttachable() && !flag3)
		{
			agentToFollow = _agentToFollowOverride;
			spectatorCameraTypes2 = SpectatorCameraTypes.LockToAnyAgent;
		}
		else
		{
			if (_missionCameraModeLogic != null)
			{
				spectatorCameraTypes = _missionCameraModeLogic.GetMissionCameraLockMode(flag2);
			}
			if (IsCheatGhostMode)
			{
				spectatorCameraTypes2 = SpectatorCameraTypes.Free;
			}
			else if (spectatorCameraTypes != SpectatorCameraTypes.Invalid)
			{
				spectatorCameraTypes2 = spectatorCameraTypes;
			}
			else if (Mission.Mode == MissionMode.Deployment)
			{
				spectatorCameraTypes2 = SpectatorCameraTypes.Free;
			}
			else if (flag)
			{
				spectatorCameraTypes2 = SpectatorCameraTypes.LockToMainPlayer;
				agentToFollow = Mission.MainAgent;
			}
			else if (flag2)
			{
				spectatorCameraTypes2 = SpectatorCameraTypes.LockToMainPlayer;
				agentToFollow = LastFollowedAgent;
			}
			else if (missionPeer == null || GetPlayerAgentVisuals(missionPeer) == null || spectatorCameraTypes == SpectatorCameraTypes.Free)
			{
				spectatorCameraTypes2 = (GameNetwork.IsMultiplayer ? ((SpectatorCameraTypes)MultiplayerOptions.OptionType.SpectatorCamera.GetIntValue()) : SpectatorCameraTypes.Free);
			}
			else
			{
				spectatorCameraTypes2 = SpectatorCameraTypes.LockToPosition;
				agentVisualToFollow = GetPlayerAgentVisuals(missionPeer);
			}
			if ((spectatorCameraTypes2 != SpectatorCameraTypes.LockToMainPlayer && spectatorCameraTypes2 != SpectatorCameraTypes.LockToPosition && Mission.Mode != MissionMode.Deployment) || (IsCheatGhostMode && !IsOrderMenuOpen && !IsTransferMenuOpen))
			{
				if (LastFollowedAgent != null && LastFollowedAgent.IsCameraAttachable())
				{
					agentToFollow = LastFollowedAgent;
				}
				else if (spectatorCameraTypes2 != 0 || (_gatherCustomAgentListToSpectate != null && LastFollowedAgent != null))
				{
					agentToFollow = FindNextCameraAttachableAgent(LastFollowedAgent, spectatorCameraTypes2, 1, currentCameraPosition);
				}
				bool flag5 = Game.Current.CheatMode && InputManager.IsControlDown();
				if (InputManager.IsGameKeyReleased(10) || InputManager.IsGameKeyReleased(11))
				{
					if (!flag5)
					{
						agentToFollow = FindNextCameraAttachableAgent(LastFollowedAgent, spectatorCameraTypes2, -1, currentCameraPosition);
					}
				}
				else if ((InputManager.IsGameKeyReleased(9) || InputManager.IsGameKeyReleased(12)) && !_rightButtonDraggingMode)
				{
					if (!flag5)
					{
						agentToFollow = FindNextCameraAttachableAgent(LastFollowedAgent, spectatorCameraTypes2, 1, currentCameraPosition);
					}
				}
				else if ((InputManager.IsGameKeyDown(0) || InputManager.IsGameKeyDown(1) || InputManager.IsGameKeyDown(2) || InputManager.IsGameKeyDown(3) || (InputManager.GetIsControllerConnected() && (Input.GetKeyState(InputKey.ControllerLStick).y != 0f || Input.GetKeyState(InputKey.ControllerLStick).x != 0f))) && spectatorCameraTypes2 == SpectatorCameraTypes.Free)
				{
					agentToFollow = null;
					agentVisualToFollow = null;
				}
			}
		}
		return new Mission.SpectatorData(agentToFollow, agentVisualToFollow, spectatorCameraTypes2);
	}

	private Agent FindNextCameraAttachableAgent(Agent currentAgent, SpectatorCameraTypes cameraLockMode, int iterationDirection, Vec3 currentCameraPosition)
	{
		if (Mission.AllAgents == null || Mission.AllAgents.Count == 0)
		{
			return null;
		}
		if (MBDebug.IsErrorReportModeActive())
		{
			return null;
		}
		MissionPeer missionPeer = (GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>() : null);
		List<Agent> list;
		if (_gatherCustomAgentListToSpectate != null)
		{
			list = _gatherCustomAgentListToSpectate(currentAgent);
		}
		else
		{
			switch (cameraLockMode)
			{
			case SpectatorCameraTypes.LockToTeamMembers:
			case SpectatorCameraTypes.LockToTeamMembersView:
				list = Mission.AllAgents.Where((Agent x) => (x.Team == Mission.PlayerTeam && x.MissionPeer != null && x.IsCameraAttachable()) || x == currentAgent).ToList();
				break;
			case SpectatorCameraTypes.LockToPlayerFormation:
				list = Mission.AllAgents.Where((Agent x) => (x.Formation != null && x.Formation == missionPeer?.ControlledFormation && x.IsCameraAttachable()) || x == currentAgent).ToList();
				break;
			case SpectatorCameraTypes.LockToAnyPlayer:
				list = Mission.AllAgents.Where((Agent x) => (x.MissionPeer != null && x.IsCameraAttachable()) || x == currentAgent).ToList();
				break;
			case SpectatorCameraTypes.LockToAnyAgent:
				list = Mission.AllAgents.Where((Agent x) => x.IsCameraAttachable() || x == currentAgent).ToList();
				break;
			default:
				list = Mission.AllAgents.Where((Agent x) => x.IsCameraAttachable() || x == currentAgent).ToList();
				break;
			}
		}
		if (list.Count - ((currentAgent != null && !currentAgent.IsCameraAttachable()) ? 1 : 0) == 0)
		{
			return null;
		}
		if (currentAgent == null)
		{
			Agent result = null;
			float num = float.MaxValue;
			foreach (Agent item in list)
			{
				float lengthSquared = (currentCameraPosition - item.Position).LengthSquared;
				if (num > lengthSquared)
				{
					num = lengthSquared;
					result = item;
				}
			}
			return result;
		}
		int num2 = list.IndexOf(currentAgent);
		if (iterationDirection == 1)
		{
			return list[(num2 + 1) % list.Count];
		}
		return (num2 < 0) ? list[list.Count - 1] : list[(num2 + list.Count - 1) % list.Count];
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}

	void IGameStateListener.OnActivate()
	{
		if (_isDeactivated)
		{
			ActivateMissionView();
		}
		_isDeactivated = false;
	}

	void IGameStateListener.OnDeactivate()
	{
		_isDeactivated = true;
		if (Mission?.MissionBehaviors != null)
		{
			foreach (MissionView missionView in _missionViews)
			{
				missionView.OnMissionScreenDeactivate();
			}
		}
		OnDeactivate();
	}

	void IMissionSystemHandler.OnMissionAfterStarting(Mission mission)
	{
		Mission = mission;
		Mission.AddListener(this);
		foreach (MissionBehavior missionBehavior in Mission.MissionBehaviors)
		{
			if (missionBehavior is MissionView missionView)
			{
				RegisterView(missionView);
			}
		}
	}

	void IMissionSystemHandler.OnMissionLoadingFinished(Mission mission)
	{
		Mission = mission;
		InitializeMissionView();
		ActivateMissionView();
	}

	void IMissionSystemHandler.BeforeMissionTick(Mission mission, float realDt)
	{
		if (MBEditor.EditModeEnabled)
		{
			if (base.DebugInput.IsHotKeyReleased("EnterEditMode") && mission == null)
			{
				if (MBEditor.IsEditModeOn)
				{
					MBEditor.LeaveEditMode();
					_tickEditor = false;
				}
				else
				{
					MBEditor.EnterEditMode(SceneView, CombatCamera.Frame, CameraElevation, CameraBearing);
					_tickEditor = true;
				}
			}
			if (_tickEditor && MBEditor.IsEditModeOn)
			{
				MBEditor.TickEditMode(realDt);
				return;
			}
		}
		if (mission == null || mission.Scene == null)
		{
			return;
		}
		mission.Scene.SetOwnerThread();
		mission.Scene.SetDynamicShadowmapCascadesRadiusMultiplier(1f);
		if (MBEditor.EditModeEnabled)
		{
			MBCommon.CheckResourceModifications();
		}
		HandleUserInput(realDt);
		if (!_isRenderingStarted && MissionStartedRendering())
		{
			Mission.Current.OnRenderingStarted();
			_isRenderingStarted = true;
		}
		if (_isRenderingStarted && _loadingScreenFramesLeft >= 0 && !_onSceneRenderingStartedCalled)
		{
			if (_loadingScreenFramesLeft > 0)
			{
				_loadingScreenFramesLeft--;
				Mission current = Mission.Current;
				Utilities.SetLoadingScreenPercentage((current == null || !current.HasMissionBehavior<DeploymentMissionController>()) ? (1f - (float)_loadingScreenFramesLeft * 0.02f) : ((_loadingScreenFramesLeft == 0) ? 1f : (0.92f - (float)_loadingScreenFramesLeft * 0.005f)));
			}
			bool flag = AreViewsReady();
			if (_loadingScreenFramesLeft <= 0 && flag && !MBAnimation.IsAnyAnimationLoadingFromDisk())
			{
				OnSceneRenderingStarted();
				_onSceneRenderingStartedCalled = true;
			}
		}
	}

	private bool AreViewsReady()
	{
		bool flag = true;
		foreach (MissionView missionView in _missionViews)
		{
			bool flag2 = missionView.IsReady();
			flag = flag && flag2;
		}
		return flag;
	}

	private void CameraTick(Mission mission, float realDt)
	{
		if (mission.CurrentState == Mission.State.Continuing)
		{
			CheckForUpdateCamera(realDt);
		}
	}

	void IMissionSystemHandler.UpdateCamera(Mission mission, float realDt)
	{
		CameraTick(mission, realDt);
		if (mission.CurrentState == Mission.State.Continuing && !mission.MissionEnded)
		{
			MBWindowManager.PreDisplay();
		}
	}

	void IMissionSystemHandler.AfterMissionTick(Mission mission, float realDt)
	{
		if ((mission.CurrentState == Mission.State.Continuing || (mission.MissionEnded && mission.CurrentState != Mission.State.Over)) && Game.Current.CheatMode && IsCheatGhostMode && Agent.Main != null && InputManager.IsHotKeyPressed("MissionScreenHotkeyTeleportMainAgent"))
		{
			MatrixFrame lastFinalRenderCameraFrame = Mission.Scene.LastFinalRenderCameraFrame;
			if (Mission.Scene.RayCastForClosestEntityOrTerrain(lastFinalRenderCameraFrame.origin, lastFinalRenderCameraFrame.origin + -lastFinalRenderCameraFrame.rotation.u * 100f, out var collisionDistance, 0.01f, BodyFlags.CommonCollisionExcludeFlags))
			{
				Vec3 origin = lastFinalRenderCameraFrame.origin + -lastFinalRenderCameraFrame.rotation.u * collisionDistance;
				Vec2 vec = -lastFinalRenderCameraFrame.rotation.u.AsVec2;
				vec.Normalize();
				MatrixFrame matrixFrame = default(MatrixFrame);
				matrixFrame.origin = origin;
				matrixFrame.rotation.f = new Vec3(vec.x, vec.y);
				matrixFrame.rotation.u = new Vec3(0f, 0f, 1f);
				matrixFrame.rotation.Orthonormalize();
				Agent.Main.TeleportToPosition(matrixFrame.origin);
			}
		}
		if (SceneLayer.Input.IsGameKeyPressed(4) && !base.DebugInput.IsAltDown() && MBEditor.EditModeEnabled && MBEditor.IsEditModeOn)
		{
			MBEditor.LeaveEditMissionMode();
		}
		if (mission.Scene == null)
		{
			MBDebug.Print("Mission is null on MissionScreen::OnFrameTick second phase");
		}
	}

	IEnumerable<MissionBehavior> IMissionSystemHandler.OnAddBehaviors(IEnumerable<MissionBehavior> behaviors, Mission mission, string missionName, bool addDefaultMissionBehaviors)
	{
		if (addDefaultMissionBehaviors)
		{
			behaviors = AddDefaultMissionBehaviorsTo(mission, behaviors);
		}
		behaviors = ViewCreatorManager.CollectMissionBehaviors(missionName, mission, behaviors);
		return behaviors;
	}

	private void HandleInputs()
	{
		if (!MBEditor.IsEditorMissionOn() && MissionStartedRendering() && SceneLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") && !LoadingWindow.IsLoadingWindowActive)
		{
			OnEscape();
		}
	}

	public void OnEscape()
	{
		if (!IsMissionTickable)
		{
			return;
		}
		foreach (MissionBehavior item in (from v in Mission.MissionBehaviors
			where v is MissionView
			orderby ((MissionView)v).ViewOrderPriority
			select v).ToList())
		{
			MissionView missionView = item as MissionView;
			if (!IsMissionTickable || missionView.OnEscape())
			{
				break;
			}
		}
	}

	bool IMissionSystemHandler.RenderIsReady()
	{
		return MissionStartedRendering();
	}

	void IMissionListener.OnEndMission()
	{
		_agentToFollowOverride = null;
		LastFollowedAgent = null;
		LastFollowedAgentVisuals = null;
		MissionView[] array = _missionViews.ToArray();
		foreach (MissionView missionView in array)
		{
			missionView.OnMissionScreenFinalize();
			UnregisterView(missionView);
		}
		CraftedDataViewManager.Clear();
		Mission.RemoveListener(this);
	}

	void IMissionListener.OnEquipItemsFromSpawnEquipmentBegin(Agent agent, Agent.CreationType creationType)
	{
		agent.ClearEquipment();
		agent.AgentVisuals.ClearVisualComponents(removeSkeleton: false);
	}

	void IMissionListener.OnEquipItemsFromSpawnEquipment(Agent agent, Agent.CreationType creationType)
	{
		switch (creationType)
		{
		case Agent.CreationType.FromRoster:
		case Agent.CreationType.FromCharacterObj:
		{
			bool useTeamColor = false;
			Random randomGenerator = null;
			bool randomizeColors = agent.RandomizeColors;
			uint color3;
			uint color4;
			if (randomizeColors)
			{
				int bodyPropertiesSeed = agent.BodyPropertiesSeed;
				randomGenerator = new Random(bodyPropertiesSeed);
				AgentVisuals.GetRandomClothingColors(bodyPropertiesSeed, Color.FromUint(agent.ClothingColor1), Color.FromUint(agent.ClothingColor2), out var color, out var color2);
				color3 = color.ToUnsignedInteger();
				color4 = color2.ToUnsignedInteger();
			}
			else
			{
				color3 = agent.ClothingColor1;
				color4 = agent.ClothingColor2;
			}
			for (EquipmentIndex equipmentIndex = EquipmentIndex.NumAllWeaponSlots; equipmentIndex < EquipmentIndex.ArmorItemEndSlot; equipmentIndex++)
			{
				if (agent.SpawnEquipment[equipmentIndex].IsVisualEmpty)
				{
					continue;
				}
				ItemObject itemObject = agent.SpawnEquipment[equipmentIndex].CosmeticItem ?? agent.SpawnEquipment[equipmentIndex].Item;
				bool hasGloves = equipmentIndex == EquipmentIndex.Body && agent.SpawnEquipment[EquipmentIndex.Gloves].Item != null;
				bool isFemale = agent.Age >= 14f && agent.IsFemale;
				MetaMesh multiMesh = agent.SpawnEquipment[equipmentIndex].GetMultiMesh(isFemale, hasGloves, needBatchedVersion: true);
				if (multiMesh != null)
				{
					if (randomizeColors)
					{
						multiMesh.SetGlossMultiplier(AgentVisuals.GetRandomGlossFactor(randomGenerator));
					}
					if (itemObject.IsUsingTableau && agent?.Origin?.Banner != null)
					{
						for (int i = 0; i < multiMesh.MeshCount; i++)
						{
							Mesh currentMesh = multiMesh.GetMeshAtIndex(i);
							Mesh mesh = currentMesh;
							if ((object)mesh != null && !mesh.HasTag("dont_use_tableau"))
							{
								Mesh mesh2 = currentMesh;
								if ((object)mesh2 != null && mesh2.HasTag("banner_replacement_mesh"))
								{
									((BannerVisual)agent.Origin.Banner.BannerVisual).GetTableauTextureLarge(delegate(Texture t)
									{
										ApplyBannerTextureToMesh(currentMesh, t);
									});
									currentMesh.ManualInvalidate();
									break;
								}
							}
							currentMesh.ManualInvalidate();
						}
					}
					else if (itemObject.IsUsingTeamColor)
					{
						for (int j = 0; j < multiMesh.MeshCount; j++)
						{
							Mesh meshAtIndex = multiMesh.GetMeshAtIndex(j);
							if (!meshAtIndex.HasTag("no_team_color"))
							{
								meshAtIndex.Color = color3;
								meshAtIndex.Color2 = color4;
								Material material = meshAtIndex.GetMaterial().CreateCopy();
								material.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", showErrors: false);
								meshAtIndex.SetMaterial(material);
								useTeamColor = true;
							}
							meshAtIndex.ManualInvalidate();
						}
					}
					if (itemObject.UsingFacegenScaling)
					{
						multiMesh.UseHeadBoneFaceGenScaling(agent.AgentVisuals.GetSkeleton(), agent.Monster.HeadLookDirectionBoneIndex, agent.AgentVisuals.GetFacegenScalingMatrix());
					}
					Skeleton skeleton = agent.AgentVisuals.GetSkeleton();
					int num = skeleton?.GetComponentCount(GameEntity.ComponentType.ClothSimulator) ?? (-1);
					agent.AgentVisuals.AddMultiMesh(multiMesh, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
					multiMesh.ManualInvalidate();
					int num2 = skeleton?.GetComponentCount(GameEntity.ComponentType.ClothSimulator) ?? (-1);
					if (skeleton != null && equipmentIndex == EquipmentIndex.Cape && num2 > num)
					{
						GameEntityComponent componentAtIndex = skeleton.GetComponentAtIndex(GameEntity.ComponentType.ClothSimulator, num2 - 1);
						agent.SetCapeClothSimulator(componentAtIndex);
					}
				}
				if (equipmentIndex != EquipmentIndex.Body || string.IsNullOrEmpty(itemObject.ArmBandMeshName))
				{
					continue;
				}
				MetaMesh copy = MetaMesh.GetCopy(itemObject.ArmBandMeshName, showErrors: true, mayReturnNull: true);
				if (!(copy != null))
				{
					continue;
				}
				if (randomizeColors)
				{
					copy.SetGlossMultiplier(AgentVisuals.GetRandomGlossFactor(randomGenerator));
				}
				if (itemObject.IsUsingTeamColor)
				{
					for (int k = 0; k < copy.MeshCount; k++)
					{
						Mesh meshAtIndex2 = copy.GetMeshAtIndex(k);
						if (!meshAtIndex2.HasTag("no_team_color"))
						{
							meshAtIndex2.Color = color3;
							meshAtIndex2.Color2 = color4;
							Material material2 = meshAtIndex2.GetMaterial().CreateCopy();
							material2.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", showErrors: false);
							meshAtIndex2.SetMaterial(material2);
							useTeamColor = true;
						}
						meshAtIndex2.ManualInvalidate();
					}
				}
				agent.AgentVisuals.AddMultiMesh(copy, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
				copy.ManualInvalidate();
			}
			ItemObject item = agent.SpawnEquipment[EquipmentIndex.Body].Item;
			if (item != null)
			{
				int lodAtlasIndex = item.LodAtlasIndex;
				if (lodAtlasIndex != -1)
				{
					agent.AgentVisuals.SetLodAtlasShadingIndex(lodAtlasIndex, useTeamColor, agent.ClothingColor1, agent.ClothingColor2);
				}
			}
			break;
		}
		case Agent.CreationType.FromHorseObj:
			MountVisualCreator.AddMountMeshToAgentVisual(agent.AgentVisuals, agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot].Item, agent.SpawnEquipment[EquipmentIndex.HorseHarness].Item, agent.HorseCreationKey, agent);
			break;
		}
		ArmorComponent.ArmorMaterialTypes bodyArmorMaterialType = ArmorComponent.ArmorMaterialTypes.None;
		ItemObject item2 = agent.SpawnEquipment[EquipmentIndex.Body].Item;
		if (item2 != null)
		{
			bodyArmorMaterialType = item2.ArmorComponent.MaterialType;
		}
		agent.SetBodyArmorMaterialType(bodyArmorMaterialType);
	}

	void IMissionListener.OnConversationCharacterChanged()
	{
		_cameraAddSpecialMovement = true;
	}

	void IMissionListener.OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		if (Mission.Mode == MissionMode.Conversation && oldMissionMode != MissionMode.Conversation)
		{
			_cameraAddSpecialMovement = true;
			_cameraApplySpecialMovementsInstantly = atStart;
		}
		else if (Mission.Mode == MissionMode.Battle && oldMissionMode == MissionMode.Deployment && CombatCamera != null)
		{
			_cameraAddSpecialMovement = true;
			_cameraApplySpecialMovementsInstantly = atStart || _playerDeploymentCancelled;
			Agent agentToFollow = GetSpectatingData(CombatCamera.Position).AgentToFollow;
			if (!atStart)
			{
				LastFollowedAgent = agentToFollow;
			}
			_cameraSpecialCurrentAddedElevation = CameraElevation;
			if (agentToFollow != null)
			{
				_cameraSpecialCurrentAddedBearing = MBMath.WrapAngle(CameraBearing - agentToFollow.LookDirectionAsAngle);
				_cameraSpecialCurrentPositionToAdd = CombatCamera.Position - agentToFollow.VisualPosition;
				CameraBearing = agentToFollow.LookDirectionAsAngle;
			}
			else
			{
				_cameraSpecialCurrentAddedBearing = 0f;
				_cameraSpecialCurrentPositionToAdd = Vec3.Zero;
				CameraBearing = 0f;
			}
			CameraElevation = 0f;
		}
		if (((Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter) && oldMissionMode != MissionMode.Conversation && oldMissionMode != MissionMode.Barter) || ((oldMissionMode == MissionMode.Conversation || oldMissionMode == MissionMode.Barter) && Mission.Mode != MissionMode.Conversation && Mission.Mode != MissionMode.Barter))
		{
			_cameraAddSpecialMovement = true;
			_cameraAddSpecialPositionalMovement = true;
			_cameraApplySpecialMovementsInstantly = atStart;
		}
		_cameraHeightLimit = 0f;
		if (Mission.Mode == MissionMode.Deployment)
		{
			GameEntity gameEntity = ((Mission.PlayerTeam.Side != BattleSideEnum.Attacker) ? (Mission.Scene.FindEntityWithTag("strategyCameraDefender") ?? Mission.Scene.FindEntityWithTag("strategyCameraAttacker")) : (Mission.Scene.FindEntityWithTag("strategyCameraAttacker") ?? Mission.Scene.FindEntityWithTag("strategyCameraDefender")));
			if (gameEntity != null)
			{
				_cameraHeightLimit = gameEntity.GetGlobalFrame().origin.z;
			}
		}
		else
		{
			GameEntity gameEntity2 = Mission.Scene.FindEntityWithTag("camera_height_limiter");
			if (gameEntity2 != null)
			{
				_cameraHeightLimit = gameEntity2.GetGlobalFrame().origin.z;
			}
		}
	}

	void IMissionListener.OnResetMission()
	{
		_agentToFollowOverride = null;
		LastFollowedAgent = null;
		LastFollowedAgentVisuals = null;
	}

	void IMissionListener.OnInitialDeploymentPlanMade(BattleSideEnum battleSide, bool isFirstPlan)
	{
		if (!GameNetwork.IsMultiplayer && Mission.Mode == MissionMode.Deployment && isFirstPlan)
		{
			BattleSideEnum side = Mission.PlayerTeam.Side;
			if (side == battleSide)
			{
				DeploymentMissionController missionBehavior = Mission.GetMissionBehavior<DeploymentMissionController>();
				bool flag = missionBehavior != null && MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle();
				GameEntity gameEntity = ((side != BattleSideEnum.Attacker) ? (Mission.Scene.FindEntityWithTag("strategyCameraDefender") ?? Mission.Scene.FindEntityWithTag("strategyCameraAttacker")) : (Mission.Scene.FindEntityWithTag("strategyCameraAttacker") ?? Mission.Scene.FindEntityWithTag("strategyCameraDefender")));
				if (gameEntity == null && flag)
				{
					MatrixFrame battleSideDeploymentFrame = Mission.DeploymentPlan.GetBattleSideDeploymentFrame(side);
					MatrixFrame frame = battleSideDeploymentFrame;
					float num = Math.Max(0.2f * (float)Mission.DeploymentPlan.GetTroopCountForSide(side, DeploymentPlanType.Initial), 32f);
					frame.rotation.RotateAboutSide(-(float)Math.PI / 6f);
					frame.origin -= num * frame.rotation.f;
					bool flag2 = false;
					if (Mission.IsPositionInsideBoundaries(frame.origin.AsVec2))
					{
						flag2 = true;
					}
					else
					{
						IEnumerable<KeyValuePair<string, ICollection<Vec2>>> source = Mission.Boundaries.Where((KeyValuePair<string, ICollection<Vec2>> boundary) => boundary.Key == "walk_area");
						if (!source.IsEmpty())
						{
							List<Vec2> list = source.First().Value as List<Vec2>;
							list = list ?? list.ToList();
							Vec2 rayDir = frame.rotation.f.AsVec2.Normalized();
							Vec2 asVec = frame.origin.AsVec2;
							if (MBMath.IntersectRayWithBoundaryList(asVec, rayDir, list, out var intersectionPoint))
							{
								Vec2 asVec2 = battleSideDeploymentFrame.origin.AsVec2;
								float num2 = intersectionPoint.Distance(asVec2);
								float val = asVec.Distance(asVec2);
								float z = num2 / Math.Max(val, 0.1f) * frame.origin.z;
								Vec3 origin = new Vec3(intersectionPoint, z);
								frame.origin = origin;
								flag2 = true;
							}
						}
					}
					if (!flag2)
					{
						frame = battleSideDeploymentFrame;
						frame.origin.z += 20f;
					}
					CombatCamera.Frame = frame;
					CameraBearing = frame.rotation.f.RotationZ;
					CameraElevation = frame.rotation.f.RotationX;
				}
				_playerDeploymentCancelled = missionBehavior != null && !flag;
			}
		}
		foreach (MissionView missionView in _missionViews)
		{
			missionView.OnInitialDeploymentPlanMadeForSide(battleSide, isFirstPlan);
		}
	}

	private void CalculateNewBearingAndElevationForFirstPerson(Agent agentToFollow, float cameraBearing, float cameraElevation, out float newBearing, out float newElevation)
	{
		newBearing = cameraBearing;
		newElevation = cameraElevation;
		AnimFlags currentAnimationFlag = agentToFollow.GetCurrentAnimationFlag(0);
		AnimFlags currentAnimationFlag2 = agentToFollow.GetCurrentAnimationFlag(1);
		if (currentAnimationFlag.HasAnyFlag(AnimFlags.anf_lock_movement | AnimFlags.anf_synch_with_ladder_movement) || currentAnimationFlag2.HasAnyFlag(AnimFlags.anf_lock_movement | AnimFlags.anf_synch_with_ladder_movement) || agentToFollow.MovementLockedState == AgentMovementLockedState.FrameLocked)
		{
			MatrixFrame boneEntitialFrame = agentToFollow.AgentVisuals.GetBoneEntitialFrame(agentToFollow.Monster.ThoraxLookDirectionBoneIndex, useBoneMapping: true);
			MatrixFrame frame = agentToFollow.AgentVisuals.GetFrame();
			float rotationZ = boneEntitialFrame.rotation.f.RotationZ;
			float num = rotationZ + frame.rotation.f.RotationZ;
			float num2 = 66f.ToRadians();
			if (Math.Abs(rotationZ) > num2 * 0.5f - 0.0001f)
			{
				float num3 = Math.Abs(rotationZ) - (num2 * 0.5f - 0.0001f);
				num2 += num3;
				num += num3 * ((rotationZ < 0f) ? 0.5f : (-0.5f));
			}
			if (Math.Abs(rotationZ) > num2 * 0.5f - 0.0001f)
			{
				float num4 = Math.Abs(rotationZ) - (num2 * 0.5f - 0.0001f);
				num2 += num4;
				num += num4 * ((rotationZ < 0f) ? 0.5f : (-0.5f));
			}
			if (num <= -(float)Math.PI)
			{
				num += (float)Math.PI * 2f;
			}
			else if (num > (float)Math.PI)
			{
				num -= (float)Math.PI * 2f;
			}
			newBearing = MBMath.ClampAngle(MBMath.WrapAngle(cameraBearing), num, num2);
			float restrictionRange = 50f.ToRadians();
			newElevation = MBMath.ClampAngle(MBMath.WrapAngle(cameraElevation), frame.rotation.f.RotationX, restrictionRange);
		}
	}

	private static void ApplyBannerTextureToMesh(Mesh armorMesh, Texture bannerTexture)
	{
		if (armorMesh != null)
		{
			Material material = armorMesh.GetMaterial().CreateCopy();
			material.SetTexture(Material.MBTextureType.DiffuseMap2, bannerTexture);
			uint num = (uint)material.GetShader().GetMaterialShaderFlagMask("use_tableau_blending");
			ulong shaderFlags = material.GetShaderFlags();
			material.SetShaderFlags(shaderFlags | num);
			armorMesh.SetMaterial(material);
		}
	}
}
