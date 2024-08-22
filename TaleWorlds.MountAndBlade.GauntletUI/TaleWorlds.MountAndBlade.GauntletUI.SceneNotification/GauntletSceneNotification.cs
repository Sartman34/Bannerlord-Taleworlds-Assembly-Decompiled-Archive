using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Scripts;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.SceneNotification;

public class GauntletSceneNotification : GlobalLayer
{
	private readonly GauntletLayer _gauntletLayer;

	private readonly Queue<(SceneNotificationData, bool)> _notificationQueue;

	private readonly List<ISceneNotificationContextProvider> _contextProviders;

	private SceneNotificationVM _dataSource;

	private SceneNotificationData _activeData;

	private bool _isActive;

	private bool _isLastActiveGameStatePaused;

	private Scene _scene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private List<PopupSceneSpawnPoint> _sceneCharacterScripts;

	private PopupSceneCameraPath _cameraPathScript;

	private Dictionary<string, GameEntity> _customPrefabBannerEntities;

	public static GauntletSceneNotification Current { get; private set; }

	public bool IsActive => _isActive;

	private GauntletSceneNotification()
	{
		_dataSource = new SceneNotificationVM(OnPositiveAction, CloseNotification, GetContinueKeyText);
		_notificationQueue = new Queue<(SceneNotificationData, bool)>();
		_contextProviders = new List<ISceneNotificationContextProvider>();
		_gauntletLayer = new GauntletLayer(4600);
		_gauntletLayer.LoadMovie("SceneNotification", _dataSource);
		base.Layer = _gauntletLayer;
		MBInformationManager.OnShowSceneNotification += OnShowSceneNotification;
		MBInformationManager.OnHideSceneNotification += OnHideSceneNotification;
		MBInformationManager.IsAnySceneNotificationActive += IsAnySceneNotifiationActive;
		_gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, null);
	}

	private bool IsAnySceneNotifiationActive()
	{
		return _isActive;
	}

	public static void Initialize()
	{
		if (Current == null)
		{
			Current = new GauntletSceneNotification();
			ScreenManager.AddGlobalLayer(Current, isFocusable: false);
			ScreenManager.SetSuspendLayer(Current.Layer, isSuspended: true);
		}
	}

	private void OnHideSceneNotification()
	{
		CloseNotification();
	}

	private void OnShowSceneNotification(SceneNotificationData campaignNotification)
	{
		_notificationQueue.Enqueue((campaignNotification, campaignNotification.PauseActiveState));
	}

	protected override void OnTick(float dt)
	{
		base.OnTick(dt);
		if (_dataSource != null)
		{
			_dataSource.EndProgress = _cameraPathScript?.GetCameraFade() ?? 0f;
			_cameraPathScript?.SetIsReady(_dataSource.IsReady);
		}
		_scene?.Tick(dt);
	}

	protected override void OnLateTick(float dt)
	{
		base.OnLateTick(dt);
		QueueTick();
	}

	private void QueueTick()
	{
		if (!_isActive && _notificationQueue.Count > 0)
		{
			SceneNotificationData.RelevantContextType relevantContext = _notificationQueue.Peek().Item1.RelevantContext;
			if (IsGivenContextApplicableToCurrentContext(relevantContext))
			{
				(SceneNotificationData, bool) tuple = _notificationQueue.Dequeue();
				CreateSceneNotification(tuple.Item1, tuple.Item2);
			}
		}
	}

	private void OnPositiveAction()
	{
		_cameraPathScript?.SetPositiveState();
		foreach (PopupSceneSpawnPoint sceneCharacterScript in _sceneCharacterScripts)
		{
			sceneCharacterScript.SetPositiveState();
		}
	}

	private void OpenScene()
	{
		_scene = Scene.CreateNewScene(initialize_physics: true, enable_decals: true, DecalAtlasGroup.Battle);
		SceneInitializationData initData = new SceneInitializationData(initializeWithDefaults: true);
		_scene.Read(_activeData.SceneID, ref initData);
		_scene.SetClothSimulationState(state: true);
		_scene.SetShadow(shadowEnabled: true);
		_scene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_scene, 32);
		_agentRendererSceneController.SetEnforcedVisibilityForAllAgents(_scene);
		_sceneCharacterScripts = new List<PopupSceneSpawnPoint>();
		_customPrefabBannerEntities = new Dictionary<string, GameEntity>();
		GameEntity firstEntityWithScriptComponent = _scene.GetFirstEntityWithScriptComponent<PopupSceneCameraPath>();
		_cameraPathScript = firstEntityWithScriptComponent.GetFirstScriptOfType<PopupSceneCameraPath>();
		_cameraPathScript?.Initialize();
		_cameraPathScript?.SetInitialState();
		List<SceneNotificationData.SceneNotificationCharacter> list = _activeData.GetSceneNotificationCharacters().ToList();
		List<Banner> list2 = _activeData.GetBanners().ToList();
		if (list != null)
		{
			int num = 1;
			for (int i = 0; i < list.Count; i++)
			{
				SceneNotificationData.SceneNotificationCharacter sceneNotificationCharacter = list[i];
				BasicCharacterObject character = sceneNotificationCharacter.Character;
				if (character == null)
				{
					num++;
					continue;
				}
				string tag = "spawnpoint_player_" + num;
				GameEntity gameEntity = _scene.FindEntitiesWithTag(tag).ToList().FirstOrDefault();
				if (gameEntity == null)
				{
					num++;
					continue;
				}
				PopupSceneSpawnPoint firstScriptOfType = gameEntity.GetFirstScriptOfType<PopupSceneSpawnPoint>();
				MatrixFrame frame = gameEntity.GetFrame();
				Equipment equipment = character.GetFirstEquipment(civilianSet: false);
				if (sceneNotificationCharacter.OverriddenEquipment != null)
				{
					equipment = sceneNotificationCharacter.OverriddenEquipment;
				}
				else if (sceneNotificationCharacter.UseCivilianEquipment)
				{
					equipment = character.GetFirstEquipment(civilianSet: true);
				}
				BodyProperties bodyProperties = character.GetBodyProperties(character.Equipment);
				if (sceneNotificationCharacter.OverriddenBodyProperties != default(BodyProperties))
				{
					bodyProperties = sceneNotificationCharacter.OverriddenBodyProperties;
				}
				uint clothColor = character.Culture.Color;
				uint clothColor2 = character.Culture.Color2;
				if (sceneNotificationCharacter.CustomColor1 != uint.MaxValue)
				{
					clothColor = sceneNotificationCharacter.CustomColor1;
				}
				if (sceneNotificationCharacter.CustomColor2 != uint.MaxValue)
				{
					clothColor2 = sceneNotificationCharacter.CustomColor2;
				}
				Monster baseMonsterFromRace = TaleWorlds.Core.FaceGen.GetBaseMonsterFromRace(character.Race);
				AgentVisuals agentVisuals = AgentVisuals.Create(new AgentVisualsData().UseMorphAnims(useMorphAnims: true).Equipment(equipment).Race(character.Race)
					.BodyProperties(bodyProperties)
					.SkeletonType(character.IsFemale ? SkeletonType.Female : SkeletonType.Male)
					.Frame(frame)
					.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, character.IsFemale, "_facegen"))
					.Scene(_scene)
					.Monster(baseMonsterFromRace)
					.PrepareImmediately(prepareImmediately: true)
					.UseTranslucency(useTranslucency: true)
					.UseTesselation(useTesselation: true)
					.ClothColor1(clothColor)
					.ClothColor2(clothColor2), "notification_agent_visuals_" + num, isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
				AgentVisuals agentVisuals2 = null;
				if (sceneNotificationCharacter.UseHorse)
				{
					ItemObject item = equipment[EquipmentIndex.ArmorItemEndSlot].Item;
					string randomMountKeyString = MountCreationKey.GetRandomMountKeyString(item, character.GetMountKeySeed());
					MBActionSet actionSet = MBGlobals.GetActionSet(item.HorseComponent.Monster.ActionSetCode);
					agentVisuals2 = AgentVisuals.Create(new AgentVisualsData().Equipment(equipment).Frame(frame).ActionSet(actionSet)
						.Scene(_scene)
						.Monster(item.HorseComponent.Monster)
						.Scale(item.ScaleFactor)
						.PrepareImmediately(prepareImmediately: true)
						.UseTranslucency(useTranslucency: true)
						.UseTesselation(useTesselation: true)
						.MountCreationKey(randomMountKeyString), "notification_mount_visuals_" + num, isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
				}
				firstScriptOfType.InitializeWithAgentVisuals(agentVisuals, agentVisuals2);
				agentVisuals.SetAgentLodZeroOrMaxExternal(makeZero: true);
				agentVisuals2?.SetAgentLodZeroOrMaxExternal(makeZero: true);
				firstScriptOfType.SetInitialState();
				_sceneCharacterScripts.Add(firstScriptOfType);
				if (!string.IsNullOrEmpty(firstScriptOfType.BannerTagToUseForAddedPrefab) && firstScriptOfType.AddedPrefabComponent != null)
				{
					_customPrefabBannerEntities.Add(firstScriptOfType.BannerTagToUseForAddedPrefab, firstScriptOfType.AddedPrefabComponent.GetEntity());
				}
				num++;
			}
		}
		if (list2 != null)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				Banner banner = list2[j];
				string text = "banner_" + (j + 1);
				GameEntity bannerEntity = _scene.FindEntityWithTag(text);
				GameEntity entity;
				if (bannerEntity != null)
				{
					((BannerVisual)banner.BannerVisual).GetTableauTextureLarge(delegate(Texture t)
					{
						OnBannerTableauRenderDone(bannerEntity, t);
					});
				}
				else if (_customPrefabBannerEntities.TryGetValue(text, out entity))
				{
					((BannerVisual)banner.BannerVisual).GetTableauTextureLarge(delegate(Texture t)
					{
						OnBannerTableauRenderDone(entity, t);
					});
				}
			}
		}
		_dataSource.Scene = _scene;
	}

	private void OnBannerTableauRenderDone(GameEntity bannerEntity, Texture bannerTexture)
	{
		if (!(bannerEntity != null))
		{
			return;
		}
		foreach (Mesh item in bannerEntity.GetAllMeshesWithTag("banner_replacement_mesh"))
		{
			ApplyBannerTextureToMesh(item, bannerTexture);
		}
		if (bannerEntity.Skeleton?.GetAllMeshes() == null)
		{
			return;
		}
		foreach (Mesh item2 in bannerEntity.Skeleton?.GetAllMeshes())
		{
			if (item2.HasTag("banner_replacement_mesh"))
			{
				ApplyBannerTextureToMesh(item2, bannerTexture);
			}
		}
	}

	private void ApplyBannerTextureToMesh(Mesh bannerMesh, Texture bannerTexture)
	{
		if (bannerMesh != null)
		{
			Material material = bannerMesh.GetMaterial().CreateCopy();
			material.SetTexture(Material.MBTextureType.DiffuseMap2, bannerTexture);
			uint num = (uint)material.GetShader().GetMaterialShaderFlagMask("use_tableau_blending");
			ulong shaderFlags = material.GetShaderFlags();
			material.SetShaderFlags(shaderFlags | num);
			bannerMesh.SetMaterial(material);
		}
	}

	private void CreateSceneNotification(SceneNotificationData data, bool pauseGameActiveState)
	{
		if (!_isActive)
		{
			_isActive = true;
			_dataSource.CreateNotification(data);
			ScreenManager.SetSuspendLayer(base.Layer, isSuspended: false);
			base.Layer.IsFocusLayer = true;
			ScreenManager.TrySetFocus(base.Layer);
			base.Layer.InputRestrictions.SetInputRestrictions();
			_isLastActiveGameStatePaused = pauseGameActiveState;
			if (_isLastActiveGameStatePaused)
			{
				GameStateManager.Current.RegisterActiveStateDisableRequest(this);
				MBCommon.PauseGameEngine();
			}
			_activeData = data;
			_dataSource.EndProgress = 0f;
			OpenScene();
		}
	}

	private void CloseNotification()
	{
		if (!_isActive)
		{
			return;
		}
		_dataSource.ForceClose();
		_isActive = false;
		base.Layer.InputRestrictions.ResetInputRestrictions();
		ScreenManager.SetSuspendLayer(base.Layer, isSuspended: true);
		base.Layer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(base.Layer);
		if (_isLastActiveGameStatePaused)
		{
			GameStateManager.Current.UnregisterActiveStateDisableRequest(this);
			MBCommon.UnPauseGameEngine();
		}
		_cameraPathScript?.Destroy();
		if (_sceneCharacterScripts != null)
		{
			foreach (PopupSceneSpawnPoint sceneCharacterScript in _sceneCharacterScripts)
			{
				sceneCharacterScript.Destroy();
			}
			_sceneCharacterScripts = null;
		}
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_scene, _agentRendererSceneController, deleteThisFrame: false);
		_activeData = null;
		_scene.ClearAll();
		_scene = null;
	}

	private string GetContinueKeyText()
	{
		if (Input.IsGamepadActive)
		{
			TextObject textObject = Module.CurrentModule.GlobalTextManager.FindText("str_click_to_continue_console");
			textObject.SetTextVariable("CONSOLE_KEY_NAME", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("ConversationHotKeyCategory", "ContinueKey")));
			return textObject.ToString();
		}
		return Module.CurrentModule.GlobalTextManager.FindText("str_click_to_continue").ToString();
	}

	public void OnFinalize()
	{
		_dataSource.OnFinalize();
		_dataSource = null;
	}

	public void RegisterContextProvider(ISceneNotificationContextProvider provider)
	{
		_contextProviders.Add(provider);
	}

	public bool RemoveContextProvider(ISceneNotificationContextProvider provider)
	{
		return _contextProviders.Remove(provider);
	}

	private bool IsGivenContextApplicableToCurrentContext(SceneNotificationData.RelevantContextType givenContextType)
	{
		if (LoadingWindow.IsLoadingWindowActive)
		{
			return false;
		}
		if (givenContextType == SceneNotificationData.RelevantContextType.Any)
		{
			return true;
		}
		for (int i = 0; i < _contextProviders.Count; i++)
		{
			ISceneNotificationContextProvider sceneNotificationContextProvider = _contextProviders[i];
			if (sceneNotificationContextProvider != null && !sceneNotificationContextProvider.IsContextAllowed(givenContextType))
			{
				return false;
			}
		}
		return true;
	}
}
