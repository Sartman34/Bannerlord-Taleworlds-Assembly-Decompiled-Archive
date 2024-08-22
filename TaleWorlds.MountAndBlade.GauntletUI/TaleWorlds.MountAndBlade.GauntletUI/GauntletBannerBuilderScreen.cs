using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.ViewModelCollection.BannerBuilder;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[GameStateScreen(typeof(BannerBuilderState))]
public class GauntletBannerBuilderScreen : ScreenBase, IGameStateListener
{
	private BannerBuilderVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private IGauntletMovie _movie;

	private SpriteCategory _bannerIconsCategory;

	private SpriteCategory _bannerBuilderCategory;

	private BannerBuilderState _state;

	private bool _isFinalized;

	private Camera _camera;

	private AgentVisuals[] _agentVisuals;

	private readonly ActionIndexCache _idleAction = ActionIndexCache.Create("act_walk_idle_1h_with_shield_left_stance");

	private Scene _scene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private MatrixFrame _characterFrame;

	private Equipment _weaponEquipment;

	private BannerCode _currentBannerCode;

	private float _cameraCurrentRotation;

	private float _cameraTargetRotation;

	private float _cameraCurrentDistanceAdder;

	private float _cameraTargetDistanceAdder;

	private float _cameraCurrentElevationAdder;

	private float _cameraTargetElevationAdder;

	private int _agentVisualToShowIndex;

	private bool _refreshCharacterAndShieldNextFrame;

	private bool _refreshBannersNextFrame;

	private bool _checkWhetherAgentVisualIsReady;

	private bool _firstCharacterRender = true;

	private BannerCode _previousBannerCode;

	private BasicCharacterObject _character;

	private const string DefaultBannerKey = "11.163.166.1528.1528.764.764.1.0.0.133.171.171.483.483.764.764.0.0.0";

	public SceneLayer SceneLayer { get; private set; }

	public GauntletBannerBuilderScreen(BannerBuilderState state)
	{
		_state = state;
		_character = MBObjectManager.Instance.GetObject<BasicCharacterObject>("main_hero");
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_bannerIconsCategory = spriteData.SpriteCategories["ui_bannericons"];
		_bannerIconsCategory.Load(resourceContext, uIResourceDepot);
		_bannerBuilderCategory = spriteData.SpriteCategories["ui_bannerbuilder"];
		_bannerBuilderCategory.Load(resourceContext, uIResourceDepot);
		_agentVisuals = new AgentVisuals[2];
		string initialKey = (string.IsNullOrWhiteSpace(_state.DefaultBannerKey) ? "11.163.166.1528.1528.764.764.1.0.0.133.171.171.483.483.764.764.0.0.0" : _state.DefaultBannerKey);
		_dataSource = new BannerBuilderVM(_character, initialKey, Exit, Refresh, CopyBannerCode);
		_gauntletLayer = new GauntletLayer(100);
		_gauntletLayer.IsFocusLayer = true;
		AddLayer(_gauntletLayer);
		_gauntletLayer.InputRestrictions.SetInputRestrictions();
		ScreenManager.TrySetFocus(_gauntletLayer);
		_movie = _gauntletLayer.LoadMovie("BannerBuilderScreen", _dataSource);
		_gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		_gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		CreateScene();
		AddLayer(SceneLayer);
		_checkWhetherAgentVisualIsReady = true;
		_firstCharacterRender = true;
		RefreshShieldAndCharacter();
	}

	private void Refresh()
	{
		RefreshShieldAndCharacter();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (_isFinalized)
		{
			return;
		}
		HandleUserInput();
		if (_isFinalized)
		{
			return;
		}
		UpdateCamera(dt);
		SceneLayer sceneLayer = SceneLayer;
		if (sceneLayer != null && sceneLayer.ReadyToRender())
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		_scene?.Tick(dt);
		if (_refreshBannersNextFrame)
		{
			UpdateBanners();
			_refreshBannersNextFrame = false;
		}
		if (_refreshCharacterAndShieldNextFrame)
		{
			RefreshShieldAndCharacterAux();
			_refreshCharacterAndShieldNextFrame = false;
		}
		if (!_checkWhetherAgentVisualIsReady)
		{
			return;
		}
		int num = (_agentVisualToShowIndex + 1) % 2;
		if (_agentVisuals[_agentVisualToShowIndex].GetEntity().CheckResources(_firstCharacterRender, checkFaceResources: true))
		{
			_agentVisuals[num].SetVisible(value: false);
			_agentVisuals[_agentVisualToShowIndex].SetVisible(value: true);
			_checkWhetherAgentVisualIsReady = false;
			_firstCharacterRender = false;
		}
		else
		{
			if (!_firstCharacterRender)
			{
				_agentVisuals[num].SetVisible(value: true);
			}
			_agentVisuals[_agentVisualToShowIndex].SetVisible(value: false);
		}
	}

	private void CreateScene()
	{
		_scene = Scene.CreateNewScene(initialize_physics: true, enable_decals: true, DecalAtlasGroup.Battle);
		_scene.SetName("BannerBuilderScreen");
		SceneInitializationData initData = default(SceneInitializationData);
		initData.InitPhysicsWorld = false;
		_scene.Read("banner_editor_scene", ref initData);
		_scene.SetShadow(shadowEnabled: true);
		_scene.DisableStaticShadows(value: true);
		_scene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_scene, 32);
		float aspectRatio = Screen.AspectRatio;
		GameEntity gameEntity = _scene.FindEntityWithTag("spawnpoint_player");
		_characterFrame = gameEntity.GetFrame();
		_characterFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		_cameraTargetDistanceAdder = 3.5f;
		_cameraCurrentDistanceAdder = _cameraTargetDistanceAdder;
		_cameraTargetElevationAdder = 1.15f;
		_cameraCurrentElevationAdder = _cameraTargetElevationAdder;
		_camera = Camera.CreateCamera();
		_camera.SetFovVertical(0.6981317f, aspectRatio, 0.2f, 200f);
		SceneLayer = new SceneLayer();
		SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		SceneLayer.SetScene(_scene);
		UpdateCamera(0f);
		SceneLayer.SetSceneUsesShadows(value: true);
		SceneLayer.SceneView.SetResolutionScaling(value: true);
		int num = -1;
		num &= -5;
		SceneLayer.SetPostfxConfigParams(num);
		AddCharacterEntities(_idleAction);
	}

	private void AddCharacterEntities(ActionIndexCache action)
	{
		_weaponEquipment = new Equipment();
		for (int i = 0; i < 12; i++)
		{
			EquipmentElement equipmentFromSlot = _character.Equipment.GetEquipmentFromSlot((EquipmentIndex)i);
			if (equipmentFromSlot.Item?.PrimaryWeapon == null || (!equipmentFromSlot.Item.PrimaryWeapon.IsShield && !equipmentFromSlot.Item.ItemFlags.HasAllFlags(ItemFlags.DropOnWeaponChange)))
			{
				_weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, equipmentFromSlot);
			}
		}
		_weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)_dataSource.ShieldSlotIndex, _dataSource.ShieldRosterElement.EquipmentElement);
		Monster baseMonsterFromRace = TaleWorlds.Core.FaceGen.GetBaseMonsterFromRace(_character.Race);
		_agentVisuals[0] = AgentVisuals.Create(new AgentVisualsData().Equipment(_weaponEquipment).BodyProperties(_character.GetBodyProperties(_weaponEquipment)).Frame(_characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, _character.IsFemale, "_facegen"))
			.ActionCode(action)
			.Scene(_scene)
			.Monster(baseMonsterFromRace)
			.SkeletonType(_character.IsFemale ? SkeletonType.Female : SkeletonType.Male)
			.Race(_character.Race)
			.PrepareImmediately(prepareImmediately: true)
			.RightWieldedItemIndex(-1)
			.LeftWieldedItemIndex(_dataSource.ShieldSlotIndex)
			.ClothColor1(_dataSource.CurrentBanner.GetPrimaryColor())
			.ClothColor2(_dataSource.CurrentBanner.GetFirstIconColor())
			.Banner(_dataSource.CurrentBanner)
			.UseMorphAnims(useMorphAnims: true), "BannerEditorChar", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: true);
		_agentVisuals[0].SetAgentLodZeroOrMaxExternal(makeZero: true);
		_agentVisuals[0].Refresh(needBatchedVersionForWeaponMeshes: false, _agentVisuals[0].GetCopyAgentVisualsData(), forceUseFaceCache: true);
		MissionWeapon shieldWeapon = new MissionWeapon(_dataSource.ShieldRosterElement.EquipmentElement.Item, _dataSource.ShieldRosterElement.EquipmentElement.ItemModifier, _dataSource.CurrentBanner);
		Action<TaleWorlds.Engine.Texture> setAction = delegate(TaleWorlds.Engine.Texture tex)
		{
			shieldWeapon.GetWeaponData(needBatchedVersionForMeshes: false).TableauMaterial.SetTexture(TaleWorlds.Engine.Material.MBTextureType.DiffuseMap2, tex);
		};
		_dataSource.CurrentBanner.GetTableauTextureLarge(setAction);
		_agentVisuals[0].SetVisible(value: false);
		_agentVisuals[0].GetEntity().CheckResources(addToQueue: true, checkFaceResources: true);
		_agentVisuals[1] = AgentVisuals.Create(new AgentVisualsData().Equipment(_weaponEquipment).BodyProperties(_character.GetBodyProperties(_weaponEquipment)).Frame(_characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, _character.IsFemale, "_facegen"))
			.ActionCode(action)
			.Scene(_scene)
			.Race(_character.Race)
			.Monster(baseMonsterFromRace)
			.SkeletonType(_character.IsFemale ? SkeletonType.Female : SkeletonType.Male)
			.PrepareImmediately(prepareImmediately: true)
			.RightWieldedItemIndex(-1)
			.LeftWieldedItemIndex(_dataSource.ShieldSlotIndex)
			.Banner(_dataSource.CurrentBanner)
			.ClothColor1(_dataSource.CurrentBanner.GetPrimaryColor())
			.ClothColor2(_dataSource.CurrentBanner.GetFirstIconColor())
			.UseMorphAnims(useMorphAnims: true), "BannerEditorChar", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: true);
		_agentVisuals[1].SetAgentLodZeroOrMaxExternal(makeZero: true);
		_agentVisuals[1].Refresh(needBatchedVersionForWeaponMeshes: false, _agentVisuals[1].GetCopyAgentVisualsData(), forceUseFaceCache: true);
		_agentVisuals[1].SetVisible(value: false);
		_agentVisuals[1].GetEntity().CheckResources(addToQueue: true, checkFaceResources: true);
		UpdateBanners();
	}

	private void UpdateBanners()
	{
		BannerCode currentBannerCode = BannerCode.CreateFrom(_dataSource.CurrentBanner);
		_dataSource.CurrentBanner.GetTableauTextureLarge(delegate(TaleWorlds.Engine.Texture resultTexture)
		{
			OnNewBannerReadyForBanners(currentBannerCode, resultTexture);
		});
		if (_previousBannerCode != null)
		{
			TableauCacheManager.Current?.ForceReleaseBanner(_previousBannerCode, isTableau: true, isLarge: true);
			TableauCacheManager.Current?.ForceReleaseBanner(_previousBannerCode, isTableau: true);
		}
		_previousBannerCode = BannerCode.CreateFrom(_dataSource.CurrentBanner);
	}

	private void OnNewBannerReadyForBanners(BannerCode bannerCodeOfTexture, TaleWorlds.Engine.Texture newTexture)
	{
		if (_isFinalized || !(_scene != null) || !(_currentBannerCode == bannerCodeOfTexture))
		{
			return;
		}
		GameEntity gameEntity = _scene.FindEntityWithTag("banner");
		if (gameEntity != null)
		{
			Mesh firstMesh = gameEntity.GetFirstMesh();
			if (firstMesh != null && _dataSource.CurrentBanner != null)
			{
				firstMesh.GetMaterial().SetTexture(TaleWorlds.Engine.Material.MBTextureType.DiffuseMap2, newTexture);
			}
		}
		else
		{
			gameEntity = _scene.FindEntityWithTag("banner_2");
			Mesh firstMesh2 = gameEntity.GetFirstMesh();
			if (firstMesh2 != null && _dataSource.CurrentBanner != null)
			{
				firstMesh2.GetMaterial().SetTexture(TaleWorlds.Engine.Material.MBTextureType.DiffuseMap2, newTexture);
			}
		}
		_refreshCharacterAndShieldNextFrame = true;
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		_bannerIconsCategory.Unload();
		_bannerBuilderCategory.Unload();
		_dataSource.OnFinalize();
		_isFinalized = true;
	}

	private void RefreshShieldAndCharacter()
	{
		_currentBannerCode = BannerCode.CreateFrom(_dataSource.CurrentBanner);
		_dataSource.BannerCodeAsString = _currentBannerCode.Code;
		_refreshBannersNextFrame = true;
	}

	private void RefreshShieldAndCharacterAux()
	{
		_ = _agentVisualToShowIndex;
		_agentVisualToShowIndex = (_agentVisualToShowIndex + 1) % 2;
		AgentVisualsData copyAgentVisualsData = _agentVisuals[_agentVisualToShowIndex].GetCopyAgentVisualsData();
		copyAgentVisualsData.Equipment(_weaponEquipment).RightWieldedItemIndex(-1).LeftWieldedItemIndex(_dataSource.ShieldSlotIndex)
			.Banner(_dataSource.CurrentBanner)
			.Frame(_characterFrame)
			.BodyProperties(_character.GetBodyProperties(_weaponEquipment))
			.ClothColor1(_dataSource.CurrentBanner.GetPrimaryColor())
			.ClothColor2(_dataSource.CurrentBanner.GetFirstIconColor());
		_agentVisuals[_agentVisualToShowIndex].Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData, forceUseFaceCache: true);
		_agentVisuals[_agentVisualToShowIndex].GetEntity().CheckResources(addToQueue: true, checkFaceResources: true);
		_agentVisuals[_agentVisualToShowIndex].GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.001f, _characterFrame, tickAnimsForChildren: true);
		_agentVisuals[_agentVisualToShowIndex].SetVisible(value: false);
		_agentVisuals[_agentVisualToShowIndex].SetVisible(value: true);
		_checkWhetherAgentVisualIsReady = true;
	}

	private void HandleUserInput()
	{
		if (_gauntletLayer.IsFocusedOnInput())
		{
			return;
		}
		if (_gauntletLayer.Input.IsHotKeyReleased("Confirm"))
		{
			_dataSource.ExecuteDone();
			return;
		}
		if (_gauntletLayer.Input.IsHotKeyReleased("Exit"))
		{
			_dataSource.ExecuteCancel();
			return;
		}
		if (SceneLayer.Input.IsHotKeyReleased("Ascend") || SceneLayer.Input.IsHotKeyReleased("Rotate") || SceneLayer.Input.IsHotKeyReleased("Zoom"))
		{
			_gauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: true);
		}
		Vec2 vec = new Vec2(0f - SceneLayer.Input.GetMouseMoveX(), 0f - SceneLayer.Input.GetMouseMoveY());
		if (SceneLayer.Input.IsHotKeyDown("Zoom"))
		{
			_cameraTargetDistanceAdder = MBMath.ClampFloat(_cameraTargetDistanceAdder + vec.y * 0.002f, 1.5f, 5f);
			MBWindowManager.DontChangeCursorPos();
			_gauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: false);
		}
		if (SceneLayer.Input.IsHotKeyDown("Rotate"))
		{
			_cameraTargetRotation = MBMath.WrapAngle(_cameraTargetRotation - vec.x * 0.004f);
			MBWindowManager.DontChangeCursorPos();
			_gauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: false);
		}
		if (SceneLayer.Input.IsHotKeyDown("Ascend"))
		{
			_cameraTargetElevationAdder = MBMath.ClampFloat(_cameraTargetElevationAdder - vec.y * 0.002f, 0.5f, 1.9f * _agentVisuals[0].GetScale());
			MBWindowManager.DontChangeCursorPos();
			_gauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: false);
		}
		if (SceneLayer.Input.GetDeltaMouseScroll() != 0f)
		{
			_cameraTargetDistanceAdder = MBMath.ClampFloat(_cameraTargetDistanceAdder - SceneLayer.Input.GetDeltaMouseScroll() * 0.001f, 1.5f, 5f);
		}
		if (Input.DebugInput.IsHotKeyPressed("Copy"))
		{
			CopyBannerCode();
		}
		if (Input.DebugInput.IsHotKeyPressed("Duplicate"))
		{
			_dataSource.ExecuteDuplicateCurrentLayer();
		}
		if (Input.DebugInput.IsHotKeyPressed("Paste"))
		{
			_dataSource.SetBannerCode(Input.GetClipboardText());
			RefreshShieldAndCharacter();
		}
		if (Input.DebugInput.IsKeyPressed(InputKey.Delete))
		{
			_dataSource.DeleteCurrentLayer();
		}
		Vec2 moveDirection = new Vec2(0f, 0f);
		if (Input.DebugInput.IsKeyReleased(InputKey.Left))
		{
			moveDirection.x = -1f;
		}
		else if (Input.DebugInput.IsKeyReleased(InputKey.Right))
		{
			moveDirection.x = 1f;
		}
		if (Input.DebugInput.IsKeyReleased(InputKey.Down))
		{
			moveDirection.y = 1f;
		}
		else if (Input.DebugInput.IsKeyReleased(InputKey.Up))
		{
			moveDirection.y = -1f;
		}
		if (moveDirection.x != 0f || moveDirection.y != 0f)
		{
			_dataSource.TranslateCurrentLayerWith(moveDirection);
		}
	}

	private void UpdateCamera(float dt)
	{
		_cameraCurrentRotation += MBMath.WrapAngle(_cameraTargetRotation - _cameraCurrentRotation) * TaleWorlds.Library.MathF.Min(1f, 10f * dt);
		_cameraCurrentElevationAdder += MBMath.WrapAngle(_cameraTargetElevationAdder - _cameraCurrentElevationAdder) * TaleWorlds.Library.MathF.Min(1f, 10f * dt);
		_cameraCurrentDistanceAdder += MBMath.WrapAngle(_cameraTargetDistanceAdder - _cameraCurrentDistanceAdder) * TaleWorlds.Library.MathF.Min(1f, 10f * dt);
		MatrixFrame characterFrame = _characterFrame;
		characterFrame.rotation.RotateAboutUp(_cameraCurrentRotation);
		characterFrame.origin += _cameraCurrentElevationAdder * characterFrame.rotation.u + _cameraCurrentDistanceAdder * characterFrame.rotation.f;
		characterFrame.rotation.RotateAboutSide(-(float)Math.PI / 2f);
		characterFrame.rotation.RotateAboutUp((float)Math.PI);
		characterFrame.rotation.RotateAboutForward((float)Math.PI * 3f / 50f);
		_camera.Frame = characterFrame;
		SceneLayer.SetCamera(_camera);
		SoundManager.SetListenerFrame(characterFrame);
	}

	private void CopyBannerCode()
	{
		Input.SetClipboardText(_dataSource.GetBannerCode());
		InformationManager.DisplayMessage(new InformationMessage("Banner code copied to the clipboard."));
	}

	public void Exit(bool isCancel)
	{
		MouseManager.ActivateMouseCursor(CursorType.Default);
		GameStateManager.Current.PopState();
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
		_agentVisuals[0].Reset();
		_agentVisuals[1].Reset();
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_scene, _agentRendererSceneController, deleteThisFrame: false);
		_agentRendererSceneController = null;
		_scene = null;
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}
}
