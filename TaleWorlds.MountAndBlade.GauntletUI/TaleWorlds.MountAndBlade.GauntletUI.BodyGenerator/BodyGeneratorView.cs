using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.FaceGenerator;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.BodyGenerator;

public class BodyGeneratorView : IFaceGeneratorHandler
{
	private const int ViewOrderPriority = 1;

	private Scene _facegenScene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private IGauntletMovie _viewMovie;

	private AgentVisuals _visualToShow;

	private List<KeyValuePair<AgentVisuals, int>> _visualsBeingPrepared;

	private readonly bool _openedFromMultiplayer;

	private AgentVisuals _nextVisualToShow;

	private int _currentAgentVisualIndex;

	private bool _refreshCharacterEntityNextFrame;

	private MatrixFrame _initialCharacterFrame;

	private bool _setMorphAnimNextFrame;

	private string _nextMorphAnimToSet = "";

	private bool _nextMorphAnimLoopValue;

	private readonly ActionIndexCache act_inventory_idle_cached = ActionIndexCache.Create("act_inventory_idle");

	private List<BodyProperties> _templateBodyProperties;

	private readonly ActionIndexCache act_inventory_idle_start_cached = ActionIndexCache.Create("act_inventory_idle_start");

	private readonly ActionIndexCache act_command_leftstance_cached = ActionIndexCache.Create("act_command_leftstance");

	private readonly ControlCharacterCreationStage _affirmativeAction;

	private readonly ControlCharacterCreationStage _negativeAction;

	private readonly ControlCharacterCreationStageReturnInt _getTotalStageCountAction;

	private readonly ControlCharacterCreationStageReturnInt _getCurrentStageIndexAction;

	private readonly ControlCharacterCreationStageReturnInt _getFurthestIndexAction;

	private readonly ControlCharacterCreationStageWithInt _goToIndexAction;

	public bool IsDressed;

	public SkeletonType SkeletonType;

	private Equipment _dressedEquipment;

	private bool _makeSound = true;

	private Camera _camera;

	private int _cameraLookMode;

	private MatrixFrame _targetCameraGlobalFrame;

	private MatrixFrame _defaultCameraGlobalFrame;

	private float _characterCurrentRotation;

	private float _characterTargetRotation;

	private float _cameraCurrentDistanceAdder;

	private float _cameraCurrentElevationAdder;

	private SpriteCategory _facegenCategory;

	private IInputContext DebugInput => Input.DebugInput;

	public FaceGenVM DataSource { get; private set; }

	public GauntletLayer GauntletLayer { get; private set; }

	public SceneLayer SceneLayer { get; private set; }

	public TaleWorlds.MountAndBlade.BodyGenerator BodyGen { get; private set; }

	public BodyGeneratorView(ControlCharacterCreationStage affirmativeAction, TextObject affirmativeActionText, ControlCharacterCreationStage negativeAction, TextObject negativeActionText, BasicCharacterObject character, bool openedFromMultiplayer, IFaceGeneratorCustomFilter filter, Equipment dressedEquipment = null, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction = null, ControlCharacterCreationStageReturnInt getTotalStageCountAction = null, ControlCharacterCreationStageReturnInt getFurthestIndexAction = null, ControlCharacterCreationStageWithInt goToIndexAction = null)
	{
		_affirmativeAction = affirmativeAction;
		_negativeAction = negativeAction;
		_getCurrentStageIndexAction = getCurrentStageIndexAction;
		_getTotalStageCountAction = getTotalStageCountAction;
		_getFurthestIndexAction = getFurthestIndexAction;
		_goToIndexAction = goToIndexAction;
		_openedFromMultiplayer = openedFromMultiplayer;
		BodyGen = new TaleWorlds.MountAndBlade.BodyGenerator(character);
		_dressedEquipment = dressedEquipment ?? BodyGen.Character.Equipment.Clone();
		if (!_dressedEquipment[EquipmentIndex.ExtraWeaponSlot].IsEmpty && _dressedEquipment[EquipmentIndex.ExtraWeaponSlot].Item.IsBannerItem)
		{
			_dressedEquipment[EquipmentIndex.ExtraWeaponSlot] = EquipmentElement.Invalid;
		}
		FaceGenerationParams faceGenerationParams = BodyGen.InitBodyGenerator(isDressed: false);
		faceGenerationParams.UseCache = true;
		faceGenerationParams.UseGpuMorph = true;
		SkeletonType = (BodyGen.IsFemale ? SkeletonType.Female : SkeletonType.Male);
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_facegenCategory = spriteData.SpriteCategories["ui_facegen"];
		_facegenCategory.Load(resourceContext, uIResourceDepot);
		OpenScene();
		AddCharacterEntity();
		bool openedFromMultiplayer2 = _openedFromMultiplayer;
		if (_getCurrentStageIndexAction == null || _getTotalStageCountAction == null || _getFurthestIndexAction == null)
		{
			DataSource = new FaceGenVM(BodyGen, this, OnHeightChanged, OnAgeChanged, affirmativeActionText, negativeActionText, 0, 0, 0, GoToIndex, openedFromMultiplayer2, openedFromMultiplayer, filter);
		}
		else
		{
			DataSource = new FaceGenVM(BodyGen, this, OnHeightChanged, OnAgeChanged, affirmativeActionText, negativeActionText, _getCurrentStageIndexAction(), _getTotalStageCountAction(), _getFurthestIndexAction(), GoToIndex, canChangeGender: true, openedFromMultiplayer, filter);
		}
		DataSource.SetPreviousTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		DataSource.SetNextTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		DataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		DataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		DataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").GetGameKey(55));
		DataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").GetGameKey(56));
		DataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys.FirstOrDefault((GameAxisKey x) => x.Id == "CameraAxisX"));
		DataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys.FirstOrDefault((GameAxisKey x) => x.Id == "CameraAxisY"));
		DataSource.SetFaceGenerationParams(faceGenerationParams);
		DataSource.Refresh(clearProperties: true);
		GauntletLayer = new GauntletLayer(1);
		GauntletLayer.InputRestrictions.SetInputRestrictions();
		GauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		GauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		GauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		GauntletLayer.InputRestrictions.SetCanOverrideFocusOnHit(canOverrideFocusOnHit: true);
		GauntletLayer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(GauntletLayer);
		_viewMovie = GauntletLayer.LoadMovie("FaceGen", DataSource);
		if (!_openedFromMultiplayer)
		{
			_templateBodyProperties = new List<BodyProperties>();
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_0").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_1").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_2").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_3").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_4").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_5").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_6").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_7").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_8").GetBodyProperties(null));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_9").GetBodyProperties(null));
		}
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		DataSource.SelectedGender = (BodyGen.IsFemale ? 1 : 0);
	}

	private void OpenScene()
	{
		_facegenScene = Scene.CreateNewScene(initialize_physics: true, enable_decals: false);
		_facegenScene.DisableStaticShadows(value: true);
		SceneInitializationData initData = default(SceneInitializationData);
		initData.InitPhysicsWorld = false;
		_facegenScene.Read("character_menu_new", ref initData);
		_facegenScene.SetShadow(shadowEnabled: true);
		_facegenScene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		_facegenScene.FindEntityWithName("cradle")?.SetVisibilityExcludeParents(visible: false);
		_facegenScene.DisableStaticShadows(value: true);
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_facegenScene, 32);
		_camera = Camera.CreateCamera();
		_defaultCameraGlobalFrame = InitCamera(_camera, new Vec3(6.45f, 5.15f, 1.75f));
		_targetCameraGlobalFrame = _defaultCameraGlobalFrame;
		SceneLayer = new SceneLayer();
		SceneLayer.IsFocusLayer = true;
		SceneLayer.SetScene(_facegenScene);
		SceneLayer.SetCamera(_camera);
		SceneLayer.SetSceneUsesShadows(value: true);
		SceneLayer.SetRenderWithPostfx(value: true);
		SceneLayer.SetPostfxFromConfig();
		SceneLayer.SceneView.SetResolutionScaling(value: true);
		SceneLayer.InputRestrictions.SetCanOverrideFocusOnHit(canOverrideFocusOnHit: true);
		int num = -1;
		num &= -5;
		SceneLayer.SetPostfxConfigParams(num);
		SceneLayer.SetPostfxFromConfig();
		SceneLayer.SceneView.SetAcceptGlobalDebugRenderObjects(value: true);
	}

	private void AddCharacterEntity()
	{
		GameEntity gameEntity = _facegenScene.FindEntityWithTag("spawnpoint_player_1");
		_initialCharacterFrame = gameEntity.GetFrame();
		_initialCharacterFrame.origin.z = 0f;
		_visualToShow = null;
		_visualsBeingPrepared = new List<KeyValuePair<AgentVisuals, int>>();
		Monster baseMonsterFromRace = TaleWorlds.Core.FaceGen.GetBaseMonsterFromRace(BodyGen.Race);
		AgentVisualsData data = new AgentVisualsData().UseMorphAnims(useMorphAnims: true).Equipment(BodyGen.Character.Equipment).BodyProperties(BodyGen.Character.GetBodyProperties(BodyGen.Character.Equipment))
			.Race(BodyGen.Race)
			.Frame(_initialCharacterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, BodyGen.IsFemale, "_facegen"))
			.Scene(_facegenScene)
			.Monster(baseMonsterFromRace)
			.UseTranslucency(useTranslucency: true)
			.UseTesselation(useTesselation: false)
			.PrepareImmediately(prepareImmediately: true);
		_nextVisualToShow = AgentVisuals.Create(data, "facegenvisual", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		_nextVisualToShow.GetEntity().Skeleton.SetAgentActionChannel(1, act_inventory_idle_start_cached);
		_nextVisualToShow.GetEntity();
		_nextVisualToShow.SetAgentLodZeroOrMaxExternal(makeZero: true);
		_nextVisualToShow.GetEntity().CheckResources(addToQueue: true, checkFaceResources: true);
		_nextVisualToShow.SetVisible(value: false);
		_visualsBeingPrepared.Add(new KeyValuePair<AgentVisuals, int>(_nextVisualToShow, 1));
		SceneLayer.SetFocusedShadowmap(enable: true, ref _initialCharacterFrame.origin, 0.59999996f);
	}

	private void SetNewBodyPropertiesAndBodyGen(BodyProperties bodyProperties)
	{
		BodyGen.CurrentBodyProperties = bodyProperties;
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
	}

	public void ResetFaceToDefault()
	{
		MBBodyProperties.ProduceNumericKeyWithDefaultValues(ref BodyGen.CurrentBodyProperties, BodyGen.Character.Equipment.EarsAreHidden, BodyGen.Character.Equipment.MouthIsHidden, BodyGen.Race, BodyGen.IsFemale ? 1 : 0, (int)BodyGen.Character.Age);
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
	}

	private void OnHeightChanged(float sliderValue)
	{
	}

	private void OnAgeChanged()
	{
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("show_debug", "facegen")]
	public static string FaceGenShowDebug(List<string> strings)
	{
		TaleWorlds.Core.FaceGen.ShowDebugValues = !TaleWorlds.Core.FaceGen.ShowDebugValues;
		return "FaceGen: Show Debug Values are " + (TaleWorlds.Core.FaceGen.ShowDebugValues ? "enabled" : "disabled");
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("toggle_update_deform_keys", "facegen")]
	public static string FaceGenUpdateDeformKeys(List<string> strings)
	{
		TaleWorlds.Core.FaceGen.UpdateDeformKeys = !TaleWorlds.Core.FaceGen.UpdateDeformKeys;
		return "FaceGen: update deform keys is now " + (TaleWorlds.Core.FaceGen.UpdateDeformKeys ? "enabled" : "disabled");
	}

	public bool ReadyToRender()
	{
		if (SceneLayer != null && SceneLayer.SceneView != null && SceneLayer.SceneView.ReadyToRender())
		{
			return true;
		}
		return false;
	}

	public void OnTick(float dt)
	{
		DataSource.CharacterGamepadControlsEnabled = Input.IsGamepadActive && SceneLayer.IsHitThisFrame;
		TickUserInputs(dt);
		if (SceneLayer != null && SceneLayer.ReadyToRender())
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		if (_refreshCharacterEntityNextFrame)
		{
			RefreshCharacterEntityAux();
			_refreshCharacterEntityNextFrame = false;
		}
		if (_visualToShow != null)
		{
			Skeleton skeleton = _visualToShow.GetVisuals().GetSkeleton();
			bool flag = skeleton.GetAnimationParameterAtChannel(1) > 0.6f;
			if (skeleton.GetActionAtChannel(1) == act_command_leftstance_cached && flag)
			{
				_visualToShow.GetEntity().Skeleton.SetAgentActionChannel(1, act_inventory_idle_cached);
			}
		}
		if (!_openedFromMultiplayer)
		{
			if (DebugInput.IsHotKeyReleased("MbFaceGeneratorScreenHotkeySetFaceKeyMin"))
			{
				BodyGen.BodyPropertiesMin = BodyGen.CurrentBodyProperties;
			}
			else if (DebugInput.IsHotKeyReleased("MbFaceGeneratorScreenHotkeySetFaceKeyMax"))
			{
				BodyGen.BodyPropertiesMax = BodyGen.CurrentBodyProperties;
			}
			else if (DebugInput.IsHotKeyPressed("Reset"))
			{
				string hairTags = "";
				string beardTags = "";
				string tatooTags = "";
				BodyGen.CurrentBodyProperties = MBBodyProperties.GetRandomBodyProperties(BodyGen.Race, BodyGen.IsFemale, BodyGen.BodyPropertiesMin, BodyGen.BodyPropertiesMax, 0, MBRandom.RandomInt(), hairTags, beardTags, tatooTags);
				SetNewBodyPropertiesAndBodyGen(BodyGen.CurrentBodyProperties);
				DataSource.SetBodyProperties(BodyGen.CurrentBodyProperties, ignoreDebugValues: false);
				DataSource.UpdateFacegen();
			}
		}
		if (DebugInput.IsHotKeyReleased("MbFaceGeneratorScreenHotkeySetCurFaceKeyToMin"))
		{
			BodyGen.CurrentBodyProperties = BodyGen.BodyPropertiesMin;
			SetNewBodyPropertiesAndBodyGen(BodyGen.BodyPropertiesMin);
			DataSource.SetBodyProperties(BodyGen.CurrentBodyProperties, ignoreDebugValues: false);
			DataSource.UpdateFacegen();
		}
		else if (DebugInput.IsHotKeyReleased("MbFaceGeneratorScreenHotkeySetCurFaceKeyToMax"))
		{
			BodyGen.CurrentBodyProperties = BodyGen.BodyPropertiesMax;
			SetNewBodyPropertiesAndBodyGen(BodyGen.BodyPropertiesMax);
			DataSource.SetBodyProperties(BodyGen.CurrentBodyProperties, ignoreDebugValues: false);
			DataSource.UpdateFacegen();
		}
		if (DebugInput.IsHotKeyDown("FaceGeneratorExtendedDebugKey") && DebugInput.IsHotKeyDown("MbFaceGeneratorScreenHotkeyResetFaceToDefault"))
		{
			ResetFaceToDefault();
			DataSource.SetBodyProperties(BodyGen.CurrentBodyProperties, ignoreDebugValues: false);
			DataSource.UpdateFacegen();
		}
		Utilities.CheckResourceModifications();
		if (DebugInput.IsHotKeyReleased("Refresh"))
		{
			((IFaceGeneratorHandler)this).RefreshCharacterEntity();
		}
		_facegenScene?.Tick(dt);
		if (_visualToShow != null)
		{
			_visualToShow.TickVisuals();
		}
		foreach (KeyValuePair<AgentVisuals, int> item in _visualsBeingPrepared)
		{
			item.Key.TickVisuals();
		}
		for (int i = 0; i < _visualsBeingPrepared.Count; i++)
		{
			AgentVisuals key = _visualsBeingPrepared[i].Key;
			int value = _visualsBeingPrepared[i].Value;
			key.SetVisible(value: false);
			if (!key.GetEntity().CheckResources(addToQueue: false, checkFaceResources: true))
			{
				continue;
			}
			if (value > 0)
			{
				_visualsBeingPrepared[i] = new KeyValuePair<AgentVisuals, int>(key, value - 1);
				continue;
			}
			if (key == _nextVisualToShow)
			{
				if (_visualToShow != null)
				{
					_visualToShow.Reset();
				}
				_visualToShow = key;
				_visualToShow.SetVisible(value: true);
				_nextVisualToShow = null;
				if (_setMorphAnimNextFrame)
				{
					_visualToShow.GetEntity().Skeleton.SetFacialAnimation(Agent.FacialAnimChannel.High, _nextMorphAnimToSet, playSound: true, _nextMorphAnimLoopValue);
					_setMorphAnimNextFrame = false;
				}
			}
			else
			{
				_visualsBeingPrepared[i].Key.Reset();
			}
			_visualsBeingPrepared[i] = _visualsBeingPrepared[_visualsBeingPrepared.Count - 1];
			_visualsBeingPrepared.RemoveAt(_visualsBeingPrepared.Count - 1);
			i--;
		}
		SoundManager.SetListenerFrame(_camera.Frame);
		UpdateCamera(dt);
		TickLayerInputs();
	}

	public void OnFinalize()
	{
		_facegenCategory.Unload();
		ClearAgentVisuals();
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_facegenScene, _agentRendererSceneController, deleteThisFrame: false);
		_agentRendererSceneController = null;
		_facegenScene.ClearAll();
		_facegenScene = null;
		SceneLayer.SceneView.SetEnable(value: false);
		SceneLayer.SceneView.ClearAll(clearScene: true, removeTerrain: true);
		DataSource?.OnFinalize();
		DataSource = null;
	}

	private void TickLayerInputs()
	{
		if (IsHotKeyReleasedOnAnyLayer("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((IFaceGeneratorHandler)this).Cancel();
		}
		else if (IsHotKeyReleasedOnAnyLayer("Confirm"))
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((IFaceGeneratorHandler)this).Done();
		}
	}

	private void TickUserInputs(float dt)
	{
		if (SceneLayer.Input.IsHotKeyReleased("Ascend") || SceneLayer.Input.IsHotKeyReleased("Rotate") || SceneLayer.Input.IsHotKeyReleased("Zoom"))
		{
			GauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: true);
		}
		Vec2 vec = new Vec2(0f - SceneLayer.Input.GetMouseMoveX(), 0f - SceneLayer.Input.GetMouseMoveY());
		bool num = SceneLayer.Input.IsHotKeyDown("Zoom");
		float gameKeyState = SceneLayer.Input.GetGameKeyState(55);
		float num2 = SceneLayer.Input.GetGameKeyState(56) - gameKeyState;
		float num3 = (num ? (vec.y * 0.002f) : ((num2 != 0f) ? (num2 * 0.02f) : (SceneLayer.Input.GetDeltaMouseScroll() * -0.001f)));
		float length = (_targetCameraGlobalFrame.origin.AsVec2 - _initialCharacterFrame.origin.AsVec2).Length;
		_cameraCurrentDistanceAdder = MBMath.ClampFloat(_cameraCurrentDistanceAdder + num3, 0.3f - length, 3f - length);
		if (num)
		{
			MBWindowManager.DontChangeCursorPos();
			GauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: false);
		}
		float gameKeyAxis = SceneLayer.Input.GetGameKeyAxis("CameraAxisX");
		gameKeyAxis = ((!(TaleWorlds.Library.MathF.Abs(gameKeyAxis) < 0.1f)) ? ((gameKeyAxis - (float)TaleWorlds.Library.MathF.Sign(gameKeyAxis) * 0.1f) / 0.9f) : 0f);
		bool num4 = SceneLayer.Input.IsHotKeyDown("Rotate");
		float num5 = (num4 ? (vec.x * -0.004f) : (gameKeyAxis * -0.02f));
		_characterTargetRotation = MBMath.WrapAngle(_characterTargetRotation + num5);
		if (num4)
		{
			MBWindowManager.DontChangeCursorPos();
			GauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: false);
		}
		if (SceneLayer.Input.IsHotKeyDown("Ascend"))
		{
			float num6 = ((_visualToShow != null) ? _visualToShow.GetScale() : 1f);
			float value = _cameraCurrentElevationAdder - vec.y * 0.002f;
			float minValue = 0.15f - _targetCameraGlobalFrame.origin.z;
			float maxValue = 1.9f * num6 - _targetCameraGlobalFrame.origin.z;
			_cameraCurrentElevationAdder = MBMath.ClampFloat(value, minValue, maxValue);
			MBWindowManager.DontChangeCursorPos();
			GauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: false);
		}
		else if (Input.IsGamepadActive)
		{
			float num7 = 0f - SceneLayer.Input.GetGameKeyAxis("CameraAxisY");
			if (TaleWorlds.Library.MathF.Abs(num7) > 0.1f)
			{
				num7 = (num7 - (float)TaleWorlds.Library.MathF.Sign(num7) * 0.1f) / 0.9f;
				float num8 = ((_visualToShow != null) ? _visualToShow.GetScale() : 1f);
				float value2 = _cameraCurrentElevationAdder - num7 * 0.01f;
				float minValue2 = 0.15f - _targetCameraGlobalFrame.origin.z;
				float maxValue2 = 1.9f * num8 - _targetCameraGlobalFrame.origin.z;
				_cameraCurrentElevationAdder = MBMath.ClampFloat(value2, minValue2, maxValue2);
			}
		}
		if (IsHotKeyPressedOnAnyLayer("SwitchToPreviousTab"))
		{
			UISoundsHelper.PlayUISound("event:/ui/tab");
			DataSource.SelectPreviousTab();
		}
		else if (IsHotKeyPressedOnAnyLayer("SwitchToNextTab"))
		{
			UISoundsHelper.PlayUISound("event:/ui/tab");
			DataSource.SelectNextTab();
		}
		if (!SceneLayer.Input.IsControlDown() && !GauntletLayer.Input.IsControlDown())
		{
			return;
		}
		if (IsHotKeyPressedOnAnyLayer("Copy"))
		{
			Input.SetClipboardText(BodyGen.CurrentBodyProperties.ToString());
		}
		else if (IsHotKeyPressedOnAnyLayer("Paste"))
		{
			if (BodyProperties.FromString(Input.GetClipboardText(), out var bodyProperties))
			{
				DataSource.SetBodyProperties(bodyProperties, !TaleWorlds.Core.FaceGen.ShowDebugValues, 0, -1, recordChange: true);
			}
			else
			{
				InformationManager.ShowInquiry(new InquiryData(GameTexts.FindText("str_error").ToString(), GameTexts.FindText("str_facegen_error_on_paste").ToString(), isAffirmativeOptionShown: false, isNegativeOptionShown: true, "", GameTexts.FindText("str_ok").ToString(), null, null));
			}
		}
	}

	private bool IsHotKeyReleasedOnAnyLayer(string hotkeyName)
	{
		if (!GauntletLayer.Input.IsHotKeyReleased(hotkeyName))
		{
			return SceneLayer.Input.IsHotKeyReleased(hotkeyName);
		}
		return true;
	}

	private bool IsHotKeyPressedOnAnyLayer(string hotkeyName)
	{
		if (!GauntletLayer.Input.IsHotKeyPressed(hotkeyName))
		{
			return SceneLayer.Input.IsHotKeyPressed(hotkeyName);
		}
		return true;
	}

	private void RefreshCharacterEntityAux()
	{
		SkeletonType skeletonType = SkeletonType;
		if (skeletonType < SkeletonType.KidsStart)
		{
			skeletonType = (BodyGen.IsFemale ? SkeletonType.Female : SkeletonType.Male);
		}
		_currentAgentVisualIndex = (_currentAgentVisualIndex + 1) % 2;
		Monster baseMonsterFromRace = TaleWorlds.Core.FaceGen.GetBaseMonsterFromRace(BodyGen.Race);
		AgentVisualsData data = new AgentVisualsData().UseMorphAnims(useMorphAnims: true).Scene(_facegenScene).Monster(baseMonsterFromRace)
			.UseTranslucency(useTranslucency: true)
			.UseTesselation(useTesselation: false)
			.SkeletonType(skeletonType)
			.Equipment(IsDressed ? _dressedEquipment : null)
			.BodyProperties(BodyGen.CurrentBodyProperties)
			.Race(BodyGen.Race)
			.PrepareImmediately(prepareImmediately: true);
		AgentVisuals obj = _visualToShow ?? _nextVisualToShow;
		ActionIndexCache actionAtChannel = obj.GetEntity().Skeleton.GetActionAtChannel(1);
		float animationParameterAtChannel = obj.GetVisuals().GetSkeleton().GetAnimationParameterAtChannel(1);
		_nextVisualToShow = AgentVisuals.Create(data, "facegenvisual", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		_nextVisualToShow.SetAgentLodZeroOrMax(value: true);
		_nextVisualToShow.GetEntity().Skeleton.SetAgentActionChannel(1, actionAtChannel, animationParameterAtChannel);
		_nextVisualToShow.GetEntity().SetEnforcedMaximumLodLevel(0);
		_nextVisualToShow.GetEntity().CheckResources(addToQueue: true, checkFaceResources: true);
		_nextVisualToShow.SetVisible(value: false);
		MatrixFrame frame = _initialCharacterFrame;
		frame.rotation.RotateAboutUp(_characterCurrentRotation);
		frame.rotation.ApplyScaleLocal(_nextVisualToShow.GetScale());
		_nextVisualToShow.GetEntity().SetFrame(ref frame);
		_nextVisualToShow.GetVisuals().GetSkeleton().SetAnimationParameterAtChannel(1, animationParameterAtChannel);
		_nextVisualToShow.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.001f, frame, tickAnimsForChildren: true);
		_nextVisualToShow.SetVisible(value: false);
		_visualsBeingPrepared.Add(new KeyValuePair<AgentVisuals, int>(_nextVisualToShow, 1));
	}

	void IFaceGeneratorHandler.MakeVoice(int voiceIndex, float pitch)
	{
		if (_makeSound)
		{
			_visualToShow?.MakeRandomVoiceForFacegen();
		}
	}

	void IFaceGeneratorHandler.RefreshCharacterEntity()
	{
		_refreshCharacterEntityNextFrame = true;
	}

	void IFaceGeneratorHandler.SetFacialAnimation(string faceAnimation, bool loop)
	{
		_setMorphAnimNextFrame = true;
		_nextMorphAnimToSet = faceAnimation;
		_nextMorphAnimLoopValue = loop;
	}

	private void ClearAgentVisuals()
	{
		if (_visualToShow != null)
		{
			_visualToShow.Reset();
			_visualToShow = null;
		}
		foreach (KeyValuePair<AgentVisuals, int> item in _visualsBeingPrepared)
		{
			item.Key.Reset();
		}
		_visualsBeingPrepared.Clear();
	}

	void IFaceGeneratorHandler.Done()
	{
		BodyGen.SaveCurrentCharacter();
		ClearAgentVisuals();
		if (TaleWorlds.MountAndBlade.Mission.Current != null)
		{
			TaleWorlds.MountAndBlade.Mission.Current.MainAgent.UpdateBodyProperties(BodyGen.CurrentBodyProperties);
			TaleWorlds.MountAndBlade.Mission.Current.MainAgent.EquipItemsFromSpawnEquipment(neededBatchedItems: false);
		}
		_affirmativeAction();
	}

	void IFaceGeneratorHandler.Cancel()
	{
		_negativeAction();
		ClearAgentVisuals();
	}

	void IFaceGeneratorHandler.ChangeToFaceCamera()
	{
		_cameraLookMode = 1;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToEyeCamera()
	{
		_cameraLookMode = 2;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToNoseCamera()
	{
		_cameraLookMode = 3;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToMouthCamera()
	{
		_cameraLookMode = 4;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToBodyCamera()
	{
		_cameraLookMode = 0;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToHairCamera()
	{
		_cameraLookMode = 1;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.UndressCharacterEntity()
	{
		IsDressed = false;
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
	}

	void IFaceGeneratorHandler.DressCharacterEntity()
	{
		IsDressed = true;
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
	}

	void IFaceGeneratorHandler.DefaultFace()
	{
		FaceGenerationParams faceGenerationParams = BodyGen.InitBodyGenerator(isDressed: false);
		faceGenerationParams.UseCache = true;
		faceGenerationParams.UseGpuMorph = true;
		MBBodyProperties.TransformFaceKeysToDefaultFace(ref faceGenerationParams);
		DataSource.SetFaceGenerationParams(faceGenerationParams);
		DataSource.Refresh(clearProperties: true);
	}

	private void GoToIndex(int index)
	{
		BodyGen.SaveCurrentCharacter();
		ClearAgentVisuals();
		_goToIndexAction(index);
	}

	public static MatrixFrame InitCamera(Camera camera, Vec3 cameraPosition)
	{
		camera.SetFovVertical((float)Math.PI / 4f, Screen.AspectRatio, 0.02f, 200f);
		return camera.Frame = Camera.ConstructCameraFromPositionElevationBearing(cameraPosition, -0.195f, 163.17f);
	}

	private void UpdateCamera(float dt)
	{
		_characterCurrentRotation += MBMath.WrapAngle(_characterTargetRotation - _characterCurrentRotation) * TaleWorlds.Library.MathF.Min(1f, 20f * dt);
		_targetCameraGlobalFrame.origin = _defaultCameraGlobalFrame.origin;
		if (_visualToShow != null)
		{
			MatrixFrame frame = _initialCharacterFrame;
			frame.rotation.RotateAboutUp(_characterCurrentRotation);
			frame.rotation.ApplyScaleLocal(_visualToShow.GetScale());
			_visualToShow.GetEntity().SetFrame(ref frame);
			float z = _visualToShow.GetGlobalStableEyePoint(isHumanoid: true).z;
			float z2 = _visualToShow.GetGlobalStableNeckPoint(isHumanoid: true).z;
			float scale = _visualToShow.GetScale();
			switch (_cameraLookMode)
			{
			case 1:
			{
				Vec2 vec = new Vec2(6.45f, 6.75f);
				vec += (vec - _initialCharacterFrame.origin.AsVec2) * (scale - 1f);
				_targetCameraGlobalFrame.origin = new Vec3(vec, z + (z - z2) * 0.75f);
				break;
			}
			case 2:
			{
				Vec2 vec = new Vec2(6.45f, 7f);
				vec += (vec - _initialCharacterFrame.origin.AsVec2) * (scale - 1f);
				_targetCameraGlobalFrame.origin = new Vec3(vec, z + (z - z2) * 0.5f);
				break;
			}
			case 3:
			{
				Vec2 vec = new Vec2(6.45f, 7f);
				vec += (vec - _initialCharacterFrame.origin.AsVec2) * (scale - 1f);
				_targetCameraGlobalFrame.origin = new Vec3(vec, z + (z - z2) * 0.25f);
				break;
			}
			case 4:
			{
				Vec2 vec = new Vec2(6.45f, 7f);
				vec += (vec - _initialCharacterFrame.origin.AsVec2) * (scale - 1f);
				_targetCameraGlobalFrame.origin = new Vec3(vec, z - (z - z2) * 0.25f);
				break;
			}
			}
		}
		Vec2 vec2 = (_targetCameraGlobalFrame.origin.AsVec2 - _initialCharacterFrame.origin.AsVec2).Normalized();
		Vec3 origin = _targetCameraGlobalFrame.origin;
		origin.AsVec2 = _targetCameraGlobalFrame.origin.AsVec2 + vec2 * _cameraCurrentDistanceAdder;
		origin.z += _cameraCurrentElevationAdder;
		_camera.Frame = new MatrixFrame(_camera.Frame.rotation, _camera.Frame.origin * (1f - 10f * dt) + origin * 10f * dt);
		SceneLayer.SetCamera(_camera);
	}
}
