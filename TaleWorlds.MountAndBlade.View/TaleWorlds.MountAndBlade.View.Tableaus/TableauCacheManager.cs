using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Scripts;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices.Avatar;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class TableauCacheManager
{
	private struct RenderDetails
	{
		public List<Action<Texture>> Actions { get; private set; }

		public RenderDetails(List<Action<Texture>> setActionList)
		{
			Actions = setActionList;
		}
	}

	private struct CustomPoseParameters
	{
		public enum Alignment
		{
			Center,
			Top,
			Bottom
		}

		public string CameraTag;

		public string FrameTag;

		public float DistanceModifier;

		public Alignment FocusAlignment;
	}

	private ThumbnailCreatorView _thumbnailCreatorView;

	private Scene _bannerScene;

	private Scene _inventoryScene;

	private bool _inventorySceneBeingUsed;

	private MBAgentRendererSceneController _inventorySceneAgentRenderer;

	private Scene _mapConversationScene;

	private bool _mapConversationSceneBeingUsed;

	private MBAgentRendererSceneController _mapConversationSceneAgentRenderer;

	private Camera _bannerCamera;

	private Camera _nineGridBannerCamera;

	private readonly ActionIndexCache act_tableau_hand_armor_pose = ActionIndexCache.Create("act_tableau_hand_armor_pose");

	private int _characterCount;

	private int _bannerCount;

	private Dictionary<string, RenderDetails> _renderCallbacks;

	private ThumbnailCache _avatarVisuals;

	private ThumbnailCache _itemVisuals;

	private ThumbnailCache _craftingPieceVisuals;

	private ThumbnailCache _characterVisuals;

	private ThumbnailCache _bannerVisuals;

	private int bannerTableauGPUAllocationIndex;

	private int itemTableauGPUAllocationIndex;

	private int characterTableauGPUAllocationIndex;

	private Texture _heroSilhouetteTexture;

	public static TableauCacheManager Current { get; private set; }

	public MatrixFrame InventorySceneCameraFrame { get; private set; }

	private void InitializeThumbnailCreator()
	{
		_thumbnailCreatorView = ThumbnailCreatorView.CreateThumbnailCreatorView();
		ThumbnailCreatorView.renderCallback = (ThumbnailCreatorView.OnThumbnailRenderCompleteDelegate)Delegate.Combine(ThumbnailCreatorView.renderCallback, new ThumbnailCreatorView.OnThumbnailRenderCompleteDelegate(OnThumbnailRenderComplete));
		Scene[] tableauCharacterScenes = BannerlordTableauManager.TableauCharacterScenes;
		foreach (Scene scene in tableauCharacterScenes)
		{
			_thumbnailCreatorView.RegisterScene(scene);
		}
		_bannerScene = Scene.CreateNewScene(initialize_physics: true, enable_decals: false);
		_bannerScene.DisableStaticShadows(value: true);
		_bannerScene.SetName("TableauCacheManager.BannerScene");
		_bannerScene.SetDefaultLighting();
		SceneInitializationData initData = new SceneInitializationData(initializeWithDefaults: true);
		initData.InitPhysicsWorld = false;
		initData.DoNotUseLoadingScreen = true;
		_inventoryScene = Scene.CreateNewScene(initialize_physics: true, enable_decals: false, DecalAtlasGroup.Battle);
		_inventoryScene.Read("inventory_character_scene", ref initData);
		_inventoryScene.SetShadow(shadowEnabled: true);
		_inventoryScene.DisableStaticShadows(value: true);
		InventorySceneCameraFrame = _inventoryScene.FindEntityWithTag("camera_instance").GetGlobalFrame();
		_inventorySceneAgentRenderer = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_inventoryScene, 32);
		_thumbnailCreatorView.RegisterScene(_bannerScene, usePostFx: false);
		bannerTableauGPUAllocationIndex = Utilities.RegisterGPUAllocationGroup("BannerTableauCache");
		itemTableauGPUAllocationIndex = Utilities.RegisterGPUAllocationGroup("ItemTableauCache");
		characterTableauGPUAllocationIndex = Utilities.RegisterGPUAllocationGroup("CharacterTableauCache");
	}

	public bool IsCachedInventoryTableauSceneUsed()
	{
		return _inventorySceneBeingUsed;
	}

	public Scene GetCachedInventoryTableauScene()
	{
		_inventorySceneBeingUsed = true;
		return _inventoryScene;
	}

	public void ReturnCachedInventoryTableauScene()
	{
		_inventorySceneBeingUsed = false;
	}

	public bool IsCachedMapConversationTableauSceneUsed()
	{
		return _mapConversationSceneBeingUsed;
	}

	public Scene GetCachedMapConversationTableauScene()
	{
		_mapConversationSceneBeingUsed = true;
		return _mapConversationScene;
	}

	public void ReturnCachedMapConversationTableauScene()
	{
		_mapConversationSceneBeingUsed = false;
	}

	public static int GetNumberOfPendingRequests()
	{
		if (Current != null)
		{
			return Current._thumbnailCreatorView.GetNumberOfPendingRequests();
		}
		return 0;
	}

	public static bool IsNativeMemoryCleared()
	{
		if (Current != null)
		{
			return Current._thumbnailCreatorView.IsMemoryCleared();
		}
		return false;
	}

	public static void InitializeManager()
	{
		Current = new TableauCacheManager();
		Current._renderCallbacks = new Dictionary<string, RenderDetails>();
		Current.InitializeThumbnailCreator();
		Current._avatarVisuals = new ThumbnailCache(100, Current._thumbnailCreatorView);
		Current._itemVisuals = new ThumbnailCache(100, Current._thumbnailCreatorView);
		Current._craftingPieceVisuals = new ThumbnailCache(100, Current._thumbnailCreatorView);
		Current._characterVisuals = new ThumbnailCache(100, Current._thumbnailCreatorView);
		Current._bannerVisuals = new ThumbnailCache(100, Current._thumbnailCreatorView);
		Current._bannerCamera = CreateDefaultBannerCamera();
		Current._nineGridBannerCamera = CreateNineGridBannerCamera();
		Current._heroSilhouetteTexture = Texture.GetFromResource("hero_silhouette");
	}

	public static void InitializeSandboxValues()
	{
		SceneInitializationData initData = new SceneInitializationData(initializeWithDefaults: true);
		initData.InitPhysicsWorld = false;
		Current._mapConversationScene = Scene.CreateNewScene(initialize_physics: true, enable_decals: false);
		Current._mapConversationScene.SetName("MapConversationTableau");
		Current._mapConversationScene.DisableStaticShadows(value: true);
		Current._mapConversationScene.Read("scn_conversation_tableau", ref initData);
		Current._mapConversationScene.SetShadow(shadowEnabled: true);
		Current._mapConversationSceneAgentRenderer = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(Current._mapConversationScene, 32);
		Utilities.LoadVirtualTextureTileset("WorldMap");
	}

	public static void ReleaseSandboxValues()
	{
		MBAgentRendererSceneController.DestructAgentRendererSceneController(Current._mapConversationScene, Current._mapConversationSceneAgentRenderer, deleteThisFrame: false);
		Current._mapConversationSceneAgentRenderer = null;
		Current._mapConversationScene.ClearAll();
		Current._mapConversationScene.ManualInvalidate();
		Current._mapConversationScene = null;
	}

	public static void ClearManager()
	{
		Debug.Print("TableauCacheManager::ClearManager");
		if (Current != null)
		{
			Current._renderCallbacks = null;
			Current._avatarVisuals = null;
			Current._itemVisuals = null;
			Current._craftingPieceVisuals = null;
			Current._characterVisuals = null;
			Current._bannerVisuals = null;
			Current._bannerCamera?.ReleaseCamera();
			Current._bannerCamera = null;
			Current._nineGridBannerCamera?.ReleaseCamera();
			Current._nineGridBannerCamera = null;
			MBAgentRendererSceneController.DestructAgentRendererSceneController(Current._inventoryScene, Current._inventorySceneAgentRenderer, deleteThisFrame: true);
			Current._inventoryScene?.ManualInvalidate();
			Current._inventoryScene = null;
			Current._bannerScene?.ClearDecals();
			Current._bannerScene?.ClearAll();
			Current._bannerScene?.ManualInvalidate();
			Current._bannerScene = null;
			ThumbnailCreatorView.renderCallback = (ThumbnailCreatorView.OnThumbnailRenderCompleteDelegate)Delegate.Remove(ThumbnailCreatorView.renderCallback, new ThumbnailCreatorView.OnThumbnailRenderCompleteDelegate(Current.OnThumbnailRenderComplete));
			Current._thumbnailCreatorView.ClearRequests();
			Current._thumbnailCreatorView = null;
			Current = null;
		}
	}

	private string ByteWidthToString(int bytes)
	{
		double num = Math.Log(bytes);
		if (bytes == 0)
		{
			num = 0.0;
		}
		int num2 = (int)(num / Math.Log(1024.0));
		char c = " KMGTPE"[num2];
		return ((double)bytes / Math.Pow(1024.0, num2)).ToString("0.00") + " " + c + "      ";
	}

	private void PrintTextureToImgui(string name, ThumbnailCache cache)
	{
		int totalMemorySize = cache.GetTotalMemorySize();
		Imgui.Text(name);
		Imgui.NextColumn();
		Imgui.Text(cache.Count.ToString());
		Imgui.NextColumn();
		Imgui.Text(ByteWidthToString(totalMemorySize));
		Imgui.NextColumn();
	}

	public void OnImguiProfilerTick()
	{
		Imgui.BeginMainThreadScope();
		Imgui.Begin("Tableau Cache Manager");
		Imgui.Columns(3);
		Imgui.Text("Name");
		Imgui.NextColumn();
		Imgui.Text("Count");
		Imgui.NextColumn();
		Imgui.Text("Memory");
		Imgui.NextColumn();
		Imgui.Separator();
		PrintTextureToImgui("Items", _itemVisuals);
		PrintTextureToImgui("Banners", _bannerVisuals);
		PrintTextureToImgui("Characters", _characterVisuals);
		PrintTextureToImgui("Avatars", _avatarVisuals);
		PrintTextureToImgui("Crafting Pieces", _craftingPieceVisuals);
		Imgui.Text("Render Callbacks");
		Imgui.NextColumn();
		Imgui.Text(_renderCallbacks.Count().ToString());
		Imgui.NextColumn();
		Imgui.Text("-");
		Imgui.NextColumn();
		Imgui.End();
		Imgui.EndMainThreadScope();
	}

	private void OnThumbnailRenderComplete(string renderId, Texture renderTarget)
	{
		Texture texture = null;
		if (_itemVisuals.GetValue(renderId, out texture))
		{
			_itemVisuals.Add(renderId, renderTarget);
		}
		else if (_craftingPieceVisuals.GetValue(renderId, out texture))
		{
			_craftingPieceVisuals.Add(renderId, renderTarget);
		}
		else if (_characterVisuals.GetValue(renderId, out texture))
		{
			_characterVisuals.Add(renderId, renderTarget);
		}
		else if (!_avatarVisuals.GetValue(renderId, out texture) && !_bannerVisuals.GetValue(renderId, out texture))
		{
			renderTarget.Release();
		}
		if (!_renderCallbacks.ContainsKey(renderId))
		{
			return;
		}
		foreach (Action<Texture> action in _renderCallbacks[renderId].Actions)
		{
			action?.Invoke(renderTarget);
		}
		_renderCallbacks.Remove(renderId);
	}

	public Texture CreateAvatarTexture(string avatarID, byte[] avatarBytes, uint width, uint height, AvatarData.ImageType imageType)
	{
		_avatarVisuals.GetValue(avatarID, out var texture);
		if (texture == null)
		{
			switch (imageType)
			{
			case AvatarData.ImageType.Image:
				texture = Texture.CreateFromMemory(avatarBytes);
				break;
			case AvatarData.ImageType.Raw:
				texture = Texture.CreateFromByteArray(avatarBytes, (int)width, (int)height);
				break;
			}
			_avatarVisuals.Add(avatarID, texture);
		}
		_avatarVisuals.AddReference(avatarID);
		return texture;
	}

	public void BeginCreateItemTexture(ItemObject itemObject, string additionalArgs, Action<Texture> setAction)
	{
		string text = itemObject.StringId;
		if (itemObject.Type == ItemObject.ItemTypeEnum.Shield)
		{
			text = text + "_" + additionalArgs;
		}
		if (_itemVisuals.GetValue(text, out var texture))
		{
			if (_renderCallbacks.ContainsKey(text))
			{
				_renderCallbacks[text].Actions.Add(setAction);
			}
			else
			{
				setAction?.Invoke(texture);
			}
			_itemVisuals.AddReference(text);
			return;
		}
		Camera camera = null;
		int num = 2;
		int width = 256;
		int height = 120;
		GameEntity gameEntity = CreateItemBaseEntity(itemObject, BannerlordTableauManager.TableauCharacterScenes[num], ref camera);
		_thumbnailCreatorView.RegisterEntityWithoutTexture(BannerlordTableauManager.TableauCharacterScenes[num], camera, gameEntity, width, height, itemTableauGPUAllocationIndex, text, "item_tableau_" + text);
		gameEntity.ManualInvalidate();
		_itemVisuals.Add(text, null);
		_itemVisuals.AddReference(text);
		if (!_renderCallbacks.ContainsKey(text))
		{
			_renderCallbacks.Add(text, new RenderDetails(new List<Action<Texture>>()));
		}
		_renderCallbacks[text].Actions.Add(setAction);
	}

	public void BeginCreateCraftingPieceTexture(CraftingPiece craftingPiece, string type, Action<Texture> setAction)
	{
		string text = craftingPiece.StringId + "$" + type;
		if (_craftingPieceVisuals.GetValue(text, out var texture))
		{
			if (_renderCallbacks.ContainsKey(text))
			{
				_renderCallbacks[text].Actions.Add(setAction);
			}
			else
			{
				setAction?.Invoke(texture);
			}
			_craftingPieceVisuals.AddReference(text);
			return;
		}
		Camera camera = null;
		int num = 2;
		int width = 256;
		int height = 180;
		GameEntity gameEntity = CreateCraftingPieceBaseEntity(craftingPiece, type, BannerlordTableauManager.TableauCharacterScenes[num], ref camera);
		_thumbnailCreatorView.RegisterEntityWithoutTexture(BannerlordTableauManager.TableauCharacterScenes[num], camera, gameEntity, width, height, itemTableauGPUAllocationIndex, text, "craft_tableau");
		gameEntity.ManualInvalidate();
		_craftingPieceVisuals.Add(text, null);
		_craftingPieceVisuals.AddReference(text);
		if (!_renderCallbacks.ContainsKey(text))
		{
			_renderCallbacks.Add(text, new RenderDetails(new List<Action<Texture>>()));
		}
		_renderCallbacks[text].Actions.Add(setAction);
	}

	public void BeginCreateCharacterTexture(CharacterCode characterCode, Action<Texture> setAction, bool isBig)
	{
		if (MBObjectManager.Instance == null)
		{
			return;
		}
		characterCode.BodyProperties = new BodyProperties(new DynamicBodyProperties((int)characterCode.BodyProperties.Age, (int)characterCode.BodyProperties.Weight, (int)characterCode.BodyProperties.Build), characterCode.BodyProperties.StaticProperties);
		string text = characterCode.CreateNewCodeString();
		text += (isBig ? "1" : "0");
		if (_characterVisuals.GetValue(text, out var texture))
		{
			if (_renderCallbacks.ContainsKey(text))
			{
				_renderCallbacks[text].Actions.Add(setAction);
			}
			else
			{
				setAction?.Invoke(texture);
			}
			_characterVisuals.AddReference(text);
			return;
		}
		Camera camera = null;
		int num = ((!isBig) ? 4 : 0);
		GameEntity poseEntity = CreateCharacterBaseEntity(characterCode, BannerlordTableauManager.TableauCharacterScenes[num], ref camera, isBig);
		poseEntity = FillEntityWithPose(characterCode, poseEntity, BannerlordTableauManager.TableauCharacterScenes[num]);
		int width = 256;
		int height = (isBig ? 120 : 184);
		_thumbnailCreatorView.RegisterEntityWithoutTexture(BannerlordTableauManager.TableauCharacterScenes[num], camera, poseEntity, width, height, characterTableauGPUAllocationIndex, text, "character_tableau_" + _characterCount);
		poseEntity.ManualInvalidate();
		_characterCount++;
		_characterVisuals.Add(text, null);
		_characterVisuals.AddReference(text);
		if (!_renderCallbacks.ContainsKey(text))
		{
			_renderCallbacks.Add(text, new RenderDetails(new List<Action<Texture>>()));
		}
		_renderCallbacks[text].Actions.Add(setAction);
	}

	public Texture GetCachedHeroSilhouetteTexture()
	{
		return _heroSilhouetteTexture;
	}

	public Texture BeginCreateBannerTexture(BannerCode bannerCode, Action<Texture> setAction, bool isTableauOrNineGrid = false, bool isLarge = false)
	{
		int width = 512;
		int height = 512;
		Camera cam = _bannerCamera;
		string text = "BannerThumbnail";
		if (isTableauOrNineGrid)
		{
			cam = _nineGridBannerCamera;
			if (isLarge)
			{
				width = 1024;
				height = 1024;
				text = "BannerTableauLarge";
			}
			else
			{
				text = "BannerTableauSmall";
			}
		}
		string text2 = text + ":" + bannerCode.Code;
		if (_bannerVisuals.GetValue(text2, out var texture))
		{
			if (_renderCallbacks.ContainsKey(text2))
			{
				_renderCallbacks[text2].Actions.Add(setAction);
			}
			else
			{
				setAction?.Invoke(texture);
			}
			_bannerVisuals.AddReference(text2);
			return texture;
		}
		MatrixFrame placementFrame = MatrixFrame.Identity;
		Banner banner = bannerCode.CalculateBanner();
		if (Game.Current == null)
		{
			banner.SetBannerVisual(((IBannerVisualCreator)new BannerVisualCreator()).CreateBannerVisual(banner));
		}
		MetaMesh metaMesh = banner.ConvertToMultiMesh();
		GameEntity gameEntity = _bannerScene.AddItemEntity(ref placementFrame, metaMesh);
		metaMesh.ManualInvalidate();
		gameEntity.SetVisibilityExcludeParents(visible: false);
		Texture texture2 = Texture.CreateRenderTarget(text + _bannerCount, width, height, autoMipmaps: true, isTableau: false, createUninitialized: true, always_valid: true);
		_thumbnailCreatorView.RegisterEntity(_bannerScene, cam, texture2, gameEntity, bannerTableauGPUAllocationIndex, text2);
		_bannerVisuals.Add(text2, texture2);
		_bannerVisuals.AddReference(text2);
		_bannerCount++;
		if (!_renderCallbacks.ContainsKey(text2))
		{
			_renderCallbacks.Add(text2, new RenderDetails(new List<Action<Texture>>()));
		}
		_renderCallbacks[text2].Actions.Add(setAction);
		return texture2;
	}

	public void Tick()
	{
		_avatarVisuals?.Tick();
		_itemVisuals?.Tick();
		_craftingPieceVisuals?.Tick();
		_characterVisuals?.Tick();
		_bannerVisuals?.Tick();
	}

	public void ReleaseTextureWithId(CraftingPiece craftingPiece, string type)
	{
		string key = craftingPiece.StringId + "$" + type;
		_craftingPieceVisuals.MarkForDeletion(key);
	}

	public void ReleaseTextureWithId(CharacterCode characterCode, bool isBig)
	{
		characterCode.BodyProperties = new BodyProperties(new DynamicBodyProperties((int)characterCode.BodyProperties.Age, (int)characterCode.BodyProperties.Weight, (int)characterCode.BodyProperties.Build), characterCode.BodyProperties.StaticProperties);
		string text = characterCode.CreateNewCodeString();
		text += (isBig ? "1" : "0");
		_characterVisuals.MarkForDeletion(text);
	}

	public void ReleaseTextureWithId(ItemObject itemObject)
	{
		string stringId = itemObject.StringId;
		_itemVisuals.MarkForDeletion(stringId);
	}

	public void ReleaseTextureWithId(BannerCode bannerCode, bool isTableau = false, bool isLarge = false)
	{
		string text = "BannerThumbnail";
		if (isTableau)
		{
			text = ((!isLarge) ? "BannerTableauSmall" : "BannerTableauLarge");
		}
		string key = text + ":" + bannerCode.Code;
		_bannerVisuals.MarkForDeletion(key);
	}

	public void ForceReleaseBanner(BannerCode bannerCode, bool isTableau = false, bool isLarge = false)
	{
		string text = "BannerThumbnail";
		if (isTableau)
		{
			text = ((!isLarge) ? "BannerTableauSmall" : "BannerTableauLarge");
		}
		string key = text + ":" + bannerCode.Code;
		_bannerVisuals.ForceDelete(key);
	}

	private void GetItemPoseAndCameraForCraftedItem(ItemObject item, Scene scene, ref Camera camera, ref MatrixFrame itemFrame, ref MatrixFrame itemFrame1, ref MatrixFrame itemFrame2)
	{
		if (camera == null)
		{
			camera = Camera.CreateCamera();
		}
		itemFrame = MatrixFrame.Identity;
		WeaponClass weaponClass = item.WeaponDesign.Template.WeaponDescriptions[0].WeaponClass;
		Vec3 u = itemFrame.rotation.u;
		Vec3 vec = itemFrame.origin - u * (item.WeaponDesign.CraftedWeaponLength * 0.5f);
		Vec3 v = vec + u * item.WeaponDesign.CraftedWeaponLength;
		Vec3 v2 = vec - u * item.WeaponDesign.BottomPivotOffset;
		int num = 0;
		Vec3 v3 = default(Vec3);
		foreach (float topPivotOffset in item.WeaponDesign.TopPivotOffsets)
		{
			if (!(topPivotOffset <= TaleWorlds.Library.MathF.Abs(1E-05f)))
			{
				Vec3 vec2 = vec + u * topPivotOffset;
				if (num == 1)
				{
					v3 = vec2;
				}
				_ = 2;
				num++;
			}
		}
		if (weaponClass == WeaponClass.OneHandedSword || weaponClass == WeaponClass.TwoHandedSword)
		{
			GameEntity gameEntity = scene.FindEntityWithTag("sword_camera");
			Vec3 dofParams = default(Vec3);
			gameEntity.GetCameraParamsFromCameraScript(camera, ref dofParams);
			gameEntity.SetVisibilityExcludeParents(visible: false);
			Vec3 vec3 = itemFrame.TransformToLocal(v2);
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin = -vec3;
			GameEntity gameEntity2 = scene.FindEntityWithTag("sword");
			gameEntity2.SetVisibilityExcludeParents(visible: false);
			itemFrame = gameEntity2.GetGlobalFrame();
			itemFrame = itemFrame.TransformToParent(identity);
		}
		if (weaponClass == WeaponClass.OneHandedAxe || weaponClass == WeaponClass.TwoHandedAxe)
		{
			GameEntity gameEntity3 = scene.FindEntityWithTag("axe_camera");
			Vec3 dofParams2 = default(Vec3);
			gameEntity3.GetCameraParamsFromCameraScript(camera, ref dofParams2);
			gameEntity3.SetVisibilityExcludeParents(visible: false);
			Vec3 vec4 = itemFrame.TransformToLocal(v3);
			MatrixFrame identity2 = MatrixFrame.Identity;
			identity2.origin = -vec4;
			GameEntity gameEntity4 = scene.FindEntityWithTag("axe");
			gameEntity4.SetVisibilityExcludeParents(visible: false);
			itemFrame = gameEntity4.GetGlobalFrame();
			itemFrame = itemFrame.TransformToParent(identity2);
		}
		if (weaponClass == WeaponClass.Dagger)
		{
			GameEntity gameEntity5 = scene.FindEntityWithTag("sword_camera");
			Vec3 dofParams3 = default(Vec3);
			gameEntity5.GetCameraParamsFromCameraScript(camera, ref dofParams3);
			gameEntity5.SetVisibilityExcludeParents(visible: false);
			Vec3 vec5 = itemFrame.TransformToLocal(v2);
			MatrixFrame identity3 = MatrixFrame.Identity;
			identity3.origin = -vec5;
			GameEntity gameEntity6 = scene.FindEntityWithTag("sword");
			gameEntity6.SetVisibilityExcludeParents(visible: false);
			itemFrame = gameEntity6.GetGlobalFrame();
			itemFrame = itemFrame.TransformToParent(identity3);
		}
		if (weaponClass == WeaponClass.ThrowingAxe)
		{
			GameEntity gameEntity7 = scene.FindEntityWithTag("throwing_axe_camera");
			Vec3 dofParams4 = default(Vec3);
			gameEntity7.GetCameraParamsFromCameraScript(camera, ref dofParams4);
			gameEntity7.SetVisibilityExcludeParents(visible: false);
			Vec3 v4 = vec + u * item.PrimaryWeapon.CenterOfMass;
			Vec3 vec6 = itemFrame.TransformToLocal(v4);
			MatrixFrame identity4 = MatrixFrame.Identity;
			identity4.origin = -vec6 * 2.5f;
			GameEntity gameEntity8 = scene.FindEntityWithTag("throwing_axe");
			gameEntity8.SetVisibilityExcludeParents(visible: false);
			itemFrame = gameEntity8.GetGlobalFrame();
			itemFrame = itemFrame.TransformToParent(identity4);
			gameEntity8 = scene.FindEntityWithTag("throwing_axe_1");
			gameEntity8.SetVisibilityExcludeParents(visible: false);
			itemFrame1 = gameEntity8.GetGlobalFrame();
			itemFrame1 = itemFrame1.TransformToParent(identity4);
			gameEntity8 = scene.FindEntityWithTag("throwing_axe_2");
			gameEntity8.SetVisibilityExcludeParents(visible: false);
			itemFrame2 = gameEntity8.GetGlobalFrame();
			itemFrame2 = itemFrame2.TransformToParent(identity4);
		}
		if (weaponClass == WeaponClass.Javelin)
		{
			GameEntity gameEntity9 = scene.FindEntityWithTag("javelin_camera");
			Vec3 dofParams5 = default(Vec3);
			gameEntity9.GetCameraParamsFromCameraScript(camera, ref dofParams5);
			gameEntity9.SetVisibilityExcludeParents(visible: false);
			Vec3 vec7 = itemFrame.TransformToLocal(v3);
			MatrixFrame identity5 = MatrixFrame.Identity;
			identity5.origin = -vec7 * 2.2f;
			GameEntity gameEntity10 = scene.FindEntityWithTag("javelin");
			gameEntity10.SetVisibilityExcludeParents(visible: false);
			itemFrame = gameEntity10.GetGlobalFrame();
			itemFrame = itemFrame.TransformToParent(identity5);
			gameEntity10 = scene.FindEntityWithTag("javelin_1");
			gameEntity10.SetVisibilityExcludeParents(visible: false);
			itemFrame1 = gameEntity10.GetGlobalFrame();
			itemFrame1 = itemFrame1.TransformToParent(identity5);
			gameEntity10 = scene.FindEntityWithTag("javelin_2");
			gameEntity10.SetVisibilityExcludeParents(visible: false);
			itemFrame2 = gameEntity10.GetGlobalFrame();
			itemFrame2 = itemFrame2.TransformToParent(identity5);
		}
		if (weaponClass == WeaponClass.ThrowingKnife)
		{
			GameEntity gameEntity11 = scene.FindEntityWithTag("javelin_camera");
			Vec3 dofParams6 = default(Vec3);
			gameEntity11.GetCameraParamsFromCameraScript(camera, ref dofParams6);
			gameEntity11.SetVisibilityExcludeParents(visible: false);
			Vec3 vec8 = itemFrame.TransformToLocal(v);
			MatrixFrame identity6 = MatrixFrame.Identity;
			identity6.origin = -vec8 * 1.4f;
			GameEntity gameEntity12 = scene.FindEntityWithTag("javelin");
			gameEntity12.SetVisibilityExcludeParents(visible: false);
			itemFrame = gameEntity12.GetGlobalFrame();
			itemFrame = itemFrame.TransformToParent(identity6);
			gameEntity12 = scene.FindEntityWithTag("javelin_1");
			gameEntity12.SetVisibilityExcludeParents(visible: false);
			itemFrame1 = gameEntity12.GetGlobalFrame();
			itemFrame1 = itemFrame1.TransformToParent(identity6);
			gameEntity12 = scene.FindEntityWithTag("javelin_2");
			gameEntity12.SetVisibilityExcludeParents(visible: false);
			itemFrame2 = gameEntity12.GetGlobalFrame();
			itemFrame2 = itemFrame2.TransformToParent(identity6);
		}
		if (weaponClass == WeaponClass.TwoHandedPolearm || weaponClass == WeaponClass.OneHandedPolearm || weaponClass == WeaponClass.LowGripPolearm || weaponClass == WeaponClass.Mace || weaponClass == WeaponClass.TwoHandedMace)
		{
			GameEntity gameEntity13 = scene.FindEntityWithTag("spear_camera");
			Vec3 dofParams7 = default(Vec3);
			gameEntity13.GetCameraParamsFromCameraScript(camera, ref dofParams7);
			gameEntity13.SetVisibilityExcludeParents(visible: false);
			Vec3 vec9 = itemFrame.TransformToLocal(v3);
			MatrixFrame identity7 = MatrixFrame.Identity;
			identity7.origin = -vec9;
			GameEntity gameEntity14 = scene.FindEntityWithTag("spear");
			gameEntity14.SetVisibilityExcludeParents(visible: false);
			itemFrame = gameEntity14.GetGlobalFrame();
			itemFrame = itemFrame.TransformToParent(identity7);
		}
	}

	private void GetItemPoseAndCamera(ItemObject item, Scene scene, ref Camera camera, ref MatrixFrame itemFrame, ref MatrixFrame itemFrame1, ref MatrixFrame itemFrame2)
	{
		if (item.IsCraftedWeapon)
		{
			GetItemPoseAndCameraForCraftedItem(item, scene, ref camera, ref itemFrame, ref itemFrame1, ref itemFrame2);
			return;
		}
		string text = "";
		CustomPoseParameters customPoseParameters = default(CustomPoseParameters);
		customPoseParameters.CameraTag = "goods_cam";
		customPoseParameters.DistanceModifier = 6f;
		customPoseParameters.FrameTag = "goods_frame";
		CustomPoseParameters customPoseParameters2 = customPoseParameters;
		if (item.WeaponComponent != null)
		{
			WeaponClass weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
			if ((uint)(weaponClass - 2) <= 1u)
			{
				text = "sword";
			}
		}
		else
		{
			switch (item.Type)
			{
			case ItemObject.ItemTypeEnum.HeadArmor:
				text = "helmet";
				break;
			case ItemObject.ItemTypeEnum.BodyArmor:
				text = "armor";
				break;
			}
		}
		if (item.Type == ItemObject.ItemTypeEnum.Shield)
		{
			text = "shield";
		}
		if (item.Type == ItemObject.ItemTypeEnum.Crossbow)
		{
			text = "crossbow";
		}
		if (item.Type == ItemObject.ItemTypeEnum.Bow)
		{
			text = "bow";
		}
		if (item.Type == ItemObject.ItemTypeEnum.LegArmor)
		{
			text = "boot";
		}
		if (item.Type == ItemObject.ItemTypeEnum.Horse)
		{
			text = ((HorseComponent)item.ItemComponent).Monster.MonsterUsage;
		}
		if (item.Type == ItemObject.ItemTypeEnum.HorseHarness)
		{
			text = "horse";
		}
		if (item.Type == ItemObject.ItemTypeEnum.Cape)
		{
			text = "cape";
		}
		if (item.Type == ItemObject.ItemTypeEnum.HandArmor)
		{
			text = "glove";
		}
		if (item.Type == ItemObject.ItemTypeEnum.Arrows)
		{
			text = "arrow";
		}
		if (item.Type == ItemObject.ItemTypeEnum.Bolts)
		{
			text = "bolt";
		}
		if (item.Type == ItemObject.ItemTypeEnum.Banner)
		{
			customPoseParameters = default(CustomPoseParameters);
			customPoseParameters.CameraTag = "banner_cam";
			customPoseParameters.DistanceModifier = 1.5f;
			customPoseParameters.FrameTag = "banner_frame";
			customPoseParameters.FocusAlignment = CustomPoseParameters.Alignment.Top;
			customPoseParameters2 = customPoseParameters;
		}
		if (item.Type == ItemObject.ItemTypeEnum.Animal)
		{
			customPoseParameters = default(CustomPoseParameters);
			customPoseParameters.CameraTag = customPoseParameters2.CameraTag;
			customPoseParameters.DistanceModifier = 3f;
			customPoseParameters.FrameTag = customPoseParameters2.FrameTag;
			customPoseParameters2 = customPoseParameters;
		}
		if (item.StringId == "iron" || item.StringId == "hardwood" || item.StringId == "charcoal" || item.StringId == "ironIngot1" || item.StringId == "ironIngot2" || item.StringId == "ironIngot3" || item.StringId == "ironIngot4" || item.StringId == "ironIngot5" || item.StringId == "ironIngot6" || item.ItemCategory == DefaultItemCategories.Silver)
		{
			text = "craftmat";
		}
		if (!string.IsNullOrEmpty(text))
		{
			string tag = text + "_cam";
			string tag2 = text + "_frame";
			GameEntity gameEntity = scene.FindEntityWithTag(tag);
			if (gameEntity != null)
			{
				camera = Camera.CreateCamera();
				Vec3 dofParams = default(Vec3);
				gameEntity.GetCameraParamsFromCameraScript(camera, ref dofParams);
			}
			GameEntity gameEntity2 = scene.FindEntityWithTag(tag2);
			if (gameEntity2 != null)
			{
				itemFrame = gameEntity2.GetGlobalFrame();
				gameEntity2.SetVisibilityExcludeParents(visible: false);
			}
		}
		else
		{
			GameEntity gameEntity3 = scene.FindEntityWithTag(customPoseParameters2.CameraTag);
			if (gameEntity3 != null)
			{
				camera = Camera.CreateCamera();
				Vec3 dofParams2 = default(Vec3);
				gameEntity3.GetCameraParamsFromCameraScript(camera, ref dofParams2);
			}
			GameEntity gameEntity4 = scene.FindEntityWithTag(customPoseParameters2.FrameTag);
			if (gameEntity4 != null)
			{
				itemFrame = gameEntity4.GetGlobalFrame();
				gameEntity4.SetVisibilityExcludeParents(visible: false);
				gameEntity4.UpdateGlobalBounds();
				MatrixFrame globalFrame = gameEntity4.GetGlobalFrame();
				MetaMesh itemMeshForInventory = new ItemRosterElement(item).GetItemMeshForInventory();
				Vec3 vec = new Vec3(1000000f, 1000000f, 1000000f);
				Vec3 vec2 = new Vec3(-1000000f, -1000000f, -1000000f);
				if (itemMeshForInventory != null)
				{
					_ = MatrixFrame.Identity;
					for (int i = 0; i != itemMeshForInventory.MeshCount; i++)
					{
						Vec3 boundingBoxMin = itemMeshForInventory.GetMeshAtIndex(i).GetBoundingBoxMin();
						Vec3 boundingBoxMax = itemMeshForInventory.GetMeshAtIndex(i).GetBoundingBoxMax();
						Vec3[] array = new Vec3[8]
						{
							globalFrame.TransformToParent(new Vec3(boundingBoxMin.x, boundingBoxMin.y, boundingBoxMin.z)),
							globalFrame.TransformToParent(new Vec3(boundingBoxMin.x, boundingBoxMin.y, boundingBoxMax.z)),
							globalFrame.TransformToParent(new Vec3(boundingBoxMin.x, boundingBoxMax.y, boundingBoxMin.z)),
							globalFrame.TransformToParent(new Vec3(boundingBoxMin.x, boundingBoxMax.y, boundingBoxMax.z)),
							globalFrame.TransformToParent(new Vec3(boundingBoxMax.x, boundingBoxMin.y, boundingBoxMin.z)),
							globalFrame.TransformToParent(new Vec3(boundingBoxMax.x, boundingBoxMin.y, boundingBoxMax.z)),
							globalFrame.TransformToParent(new Vec3(boundingBoxMax.x, boundingBoxMax.y, boundingBoxMin.z)),
							globalFrame.TransformToParent(new Vec3(boundingBoxMax.x, boundingBoxMax.y, boundingBoxMax.z))
						};
						for (int j = 0; j < 8; j++)
						{
							vec = Vec3.Vec3Min(vec, array[j]);
							vec2 = Vec3.Vec3Max(vec2, array[j]);
						}
					}
				}
				Vec3 v = (vec + vec2) * 0.5f;
				Vec3 vec3 = gameEntity4.GetGlobalFrame().TransformToLocal(v);
				MatrixFrame globalFrame2 = gameEntity4.GetGlobalFrame();
				globalFrame2.origin -= vec3;
				itemFrame = globalFrame2;
				MatrixFrame frame = camera.Frame;
				float num = (vec2 - vec).Length * customPoseParameters2.DistanceModifier;
				frame.origin += frame.rotation.u * num;
				if (customPoseParameters2.FocusAlignment == CustomPoseParameters.Alignment.Top)
				{
					frame.origin += new Vec3(0f, 0f, (vec2 - vec).Z * 0.3f);
				}
				else if (customPoseParameters2.FocusAlignment == CustomPoseParameters.Alignment.Bottom)
				{
					frame.origin -= new Vec3(0f, 0f, (vec2 - vec).Z * 0.3f);
				}
				camera.Frame = frame;
			}
		}
		if (camera == null)
		{
			camera = Camera.CreateCamera();
			camera.SetViewVolume(perspective: false, -1f, 1f, -0.5f, 0.5f, 0.01f, 100f);
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin -= identity.rotation.u * 7f;
			identity.rotation.u *= -1f;
			camera.Frame = identity;
		}
		if (item.Type == ItemObject.ItemTypeEnum.Shield)
		{
			GameEntity gameEntity5 = scene.FindEntityWithTag("shield_cam");
			itemFrame.rotation = MBItem.GetHolsterFrameByIndex(MBItem.GetItemHolsterIndex(item.ItemHolsters[0])).rotation;
			MatrixFrame frame2 = itemFrame.TransformToParent(gameEntity5.GetFrame());
			camera.Frame = frame2;
		}
	}

	private GameEntity AddItem(Scene scene, ItemObject item, MatrixFrame itemFrame, MatrixFrame itemFrame1, MatrixFrame itemFrame2)
	{
		ItemRosterElement rosterElement = new ItemRosterElement(item);
		MetaMesh itemMeshForInventory = rosterElement.GetItemMeshForInventory();
		if (item.IsCraftedWeapon)
		{
			MatrixFrame frame = itemMeshForInventory.Frame;
			frame.Elevate((0f - item.WeaponDesign.CraftedWeaponLength) / 2f);
			itemMeshForInventory.Frame = frame;
		}
		GameEntity gameEntity = null;
		if (itemMeshForInventory != null && rosterElement.EquipmentElement.Item.ItemType == ItemObject.ItemTypeEnum.HandArmor)
		{
			gameEntity = GameEntity.CreateEmpty(scene);
			AnimationSystemData animationSystemData = Game.Current.DefaultMonster.FillAnimationSystemData(MBActionSet.GetActionSet(Game.Current.DefaultMonster.ActionSetCode), 1f, hasClippingPlane: false);
			gameEntity.CreateSkeletonWithActionSet(ref animationSystemData);
			gameEntity.SetFrame(ref itemFrame);
			gameEntity.Skeleton.SetAgentActionChannel(0, act_tableau_hand_armor_pose);
			gameEntity.AddMultiMeshToSkeleton(itemMeshForInventory);
			gameEntity.Skeleton.TickAnimationsAndForceUpdate(0.01f, itemFrame, tickAnimsForChildren: true);
		}
		else if (itemMeshForInventory != null)
		{
			if (item.WeaponComponent != null)
			{
				WeaponClass weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
				if (weaponClass == WeaponClass.ThrowingAxe || weaponClass == WeaponClass.ThrowingKnife || weaponClass == WeaponClass.Javelin || weaponClass == WeaponClass.Bolt)
				{
					gameEntity = GameEntity.CreateEmpty(scene);
					MetaMesh metaMesh = itemMeshForInventory.CreateCopy();
					metaMesh.Frame = itemFrame;
					gameEntity.AddMultiMesh(metaMesh);
					MetaMesh metaMesh2 = itemMeshForInventory.CreateCopy();
					metaMesh2.Frame = itemFrame1;
					gameEntity.AddMultiMesh(metaMesh2);
					MetaMesh metaMesh3 = itemMeshForInventory.CreateCopy();
					metaMesh3.Frame = itemFrame2;
					gameEntity.AddMultiMesh(metaMesh3);
				}
				else
				{
					gameEntity = scene.AddItemEntity(ref itemFrame, itemMeshForInventory);
				}
			}
			else
			{
				gameEntity = scene.AddItemEntity(ref itemFrame, itemMeshForInventory);
				if (item.Type == ItemObject.ItemTypeEnum.HorseHarness && item.ArmorComponent != null)
				{
					MetaMesh copy = MetaMesh.GetCopy(item.ArmorComponent.ReinsMesh, showErrors: true, mayReturnNull: true);
					if (copy != null)
					{
						gameEntity.AddMultiMesh(copy);
					}
				}
			}
		}
		else
		{
			MBDebug.ShowWarning("[DEBUG]Item with " + rosterElement.EquipmentElement.Item.StringId + "[DEBUG] string id cannot be found");
		}
		gameEntity.SetVisibilityExcludeParents(visible: false);
		return gameEntity;
	}

	private void GetPoseParamsFromCharacterCode(CharacterCode characterCode, out string poseName, out bool hasHorse)
	{
		hasHorse = false;
		if (characterCode.IsHero)
		{
			int num = MBRandom.NondeterministicRandomInt % 8;
			poseName = "lord_" + num;
			return;
		}
		poseName = "troop_villager";
		int num2 = -1;
		int num3 = -1;
		Equipment equipment = characterCode.CalculateEquipment();
		switch (characterCode.FormationClass)
		{
		case FormationClass.Infantry:
		case FormationClass.Cavalry:
		case FormationClass.NumberOfDefaultFormations:
		case FormationClass.HeavyInfantry:
		case FormationClass.LightCavalry:
		case FormationClass.HeavyCavalry:
		case FormationClass.NumberOfRegularFormations:
		case FormationClass.Bodyguard:
		{
			for (int j = 0; j < 4; j++)
			{
				if (equipment[j].Item?.PrimaryWeapon != null)
				{
					if (num3 == -1 && equipment[j].Item.ItemFlags.HasAnyFlag(ItemFlags.HeldInOffHand))
					{
						num3 = j;
					}
					if (num2 == -1 && equipment[j].Item.PrimaryWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.MeleeWeapon))
					{
						num2 = j;
					}
				}
			}
			break;
		}
		case FormationClass.Ranged:
		case FormationClass.HorseArcher:
		{
			for (int i = 0; i < 4; i++)
			{
				if (equipment[i].Item?.PrimaryWeapon != null)
				{
					if (num3 == -1 && equipment[i].Item.ItemFlags.HasAnyFlag(ItemFlags.HeldInOffHand))
					{
						num3 = i;
					}
					if (num2 == -1 && equipment[i].Item.PrimaryWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.RangedWeapon))
					{
						num2 = i;
					}
				}
			}
			break;
		}
		}
		if (num2 != -1)
		{
			switch (equipment[num2].Item.PrimaryWeapon.WeaponClass)
			{
			case WeaponClass.OneHandedSword:
			case WeaponClass.OneHandedAxe:
				if (num3 == -1)
				{
					poseName = "troop_infantry_sword1h";
				}
				else if (equipment[num3].Item.PrimaryWeapon.IsShield)
				{
					poseName = "troop_infantry_sword1h";
				}
				break;
			case WeaponClass.TwoHandedSword:
			case WeaponClass.TwoHandedAxe:
			case WeaponClass.TwoHandedMace:
				poseName = "troop_infantry_sword2h";
				break;
			case WeaponClass.Crossbow:
				poseName = "troop_crossbow";
				break;
			case WeaponClass.Bow:
				poseName = "troop_bow";
				break;
			case WeaponClass.LowGripPolearm:
			case WeaponClass.Javelin:
				poseName = "troop_spear";
				break;
			case WeaponClass.OneHandedPolearm:
			case WeaponClass.TwoHandedPolearm:
				poseName = "troop_spear";
				break;
			}
		}
		if (equipment[EquipmentIndex.ArmorItemEndSlot].IsEmpty)
		{
			return;
		}
		if (num2 != -1)
		{
			HorseComponent horseComponent = equipment[EquipmentIndex.ArmorItemEndSlot].Item.HorseComponent;
			bool flag = horseComponent != null && horseComponent.Monster?.FamilyType == 2;
			switch (equipment[num2].Item.Type)
			{
			case ItemObject.ItemTypeEnum.Bow:
				poseName = "troop_cavalry_archer";
				break;
			case ItemObject.ItemTypeEnum.OneHandedWeapon:
				if (num3 == -1)
				{
					poseName = "troop_cavalry_sword";
				}
				else if (equipment[num3].Item.PrimaryWeapon.IsShield)
				{
					poseName = "troop_cavalry_sword";
				}
				break;
			default:
				poseName = "troop_cavalry_lance";
				break;
			}
			if (flag)
			{
				poseName = "camel_" + poseName;
			}
		}
		hasHorse = true;
	}

	private GameEntity CreateCraftingPieceBaseEntity(CraftingPiece craftingPiece, string ItemType, Scene scene, ref Camera camera)
	{
		MatrixFrame placementFrame = MatrixFrame.Identity;
		bool flag = false;
		string tag = "craftingPiece_cam";
		string tag2 = "craftingPiece_frame";
		if (craftingPiece.PieceType == CraftingPiece.PieceTypes.Blade)
		{
			switch (ItemType)
			{
			case "OneHandedAxe":
			case "ThrowingAxe":
				tag = "craft_axe_camera";
				tag2 = "craft_axe";
				break;
			case "TwoHandedAxe":
				tag = "craft_big_axe_camera";
				tag2 = "craft_big_axe";
				break;
			case "Dagger":
			case "ThrowingKnife":
			case "TwoHandedPolearm":
			case "Pike":
			case "Javelin":
				tag = "craft_spear_blade_camera";
				tag2 = "craft_spear_blade";
				break;
			case "Mace":
			case "TwoHandedMace":
				tag = "craft_mace_camera";
				tag2 = "craft_mace";
				break;
			default:
				tag = "craft_blade_camera";
				tag2 = "craft_blade";
				break;
			}
			flag = true;
		}
		else if (craftingPiece.PieceType == CraftingPiece.PieceTypes.Pommel)
		{
			tag = "craft_pommel_camera";
			tag2 = "craft_pommel";
			flag = true;
		}
		else if (craftingPiece.PieceType == CraftingPiece.PieceTypes.Guard)
		{
			tag = "craft_guard_camera";
			tag2 = "craft_guard";
			flag = true;
		}
		else if (craftingPiece.PieceType == CraftingPiece.PieceTypes.Handle)
		{
			tag = "craft_handle_camera";
			tag2 = "craft_handle";
			flag = true;
		}
		bool flag2 = false;
		if (flag)
		{
			GameEntity gameEntity = scene.FindEntityWithTag(tag);
			if (gameEntity != null)
			{
				camera = Camera.CreateCamera();
				Vec3 dofParams = default(Vec3);
				gameEntity.GetCameraParamsFromCameraScript(camera, ref dofParams);
			}
			GameEntity gameEntity2 = scene.FindEntityWithTag(tag2);
			if (gameEntity2 != null)
			{
				placementFrame = gameEntity2.GetGlobalFrame();
				gameEntity2.SetVisibilityExcludeParents(visible: false);
				flag2 = true;
			}
		}
		else
		{
			GameEntity gameEntity3 = scene.FindEntityWithTag("old_system_item_frame");
			if (gameEntity3 != null)
			{
				placementFrame = gameEntity3.GetGlobalFrame();
				gameEntity3.SetVisibilityExcludeParents(visible: false);
			}
		}
		if (camera == null)
		{
			camera = Camera.CreateCamera();
			camera.SetViewVolume(perspective: false, -1f, 1f, -0.5f, 0.5f, 0.01f, 100f);
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin -= identity.rotation.u * 7f;
			identity.rotation.u *= -1f;
			camera.Frame = identity;
		}
		if (!flag2)
		{
			placementFrame = craftingPiece.GetCraftingPieceFrameForInventory();
		}
		MetaMesh copy = MetaMesh.GetCopy(craftingPiece.MeshName);
		GameEntity gameEntity4 = null;
		if (copy != null)
		{
			gameEntity4 = scene.AddItemEntity(ref placementFrame, copy);
		}
		else
		{
			MBDebug.ShowWarning("[DEBUG]craftingPiece with " + craftingPiece.StringId + "[DEBUG] string id cannot be found");
		}
		gameEntity4.SetVisibilityExcludeParents(visible: false);
		return gameEntity4;
	}

	private GameEntity CreateItemBaseEntity(ItemObject item, Scene scene, ref Camera camera)
	{
		MatrixFrame itemFrame = MatrixFrame.Identity;
		MatrixFrame itemFrame2 = MatrixFrame.Identity;
		MatrixFrame itemFrame3 = MatrixFrame.Identity;
		GetItemPoseAndCamera(item, scene, ref camera, ref itemFrame, ref itemFrame2, ref itemFrame3);
		return AddItem(scene, item, itemFrame, itemFrame2, itemFrame3);
	}

	private GameEntity CreateCharacterBaseEntity(CharacterCode characterCode, Scene scene, ref Camera camera, bool isBig)
	{
		GetPoseParamsFromCharacterCode(characterCode, out var poseName, out var _);
		string tag = poseName + "_pose";
		string tag2 = (isBig ? (poseName + "_cam") : (poseName + "_cam_small"));
		GameEntity gameEntity = scene.FindEntityWithTag(tag);
		if (gameEntity == null)
		{
			return null;
		}
		gameEntity.SetVisibilityExcludeParents(visible: true);
		GameEntity gameEntity2 = GameEntity.CopyFromPrefab(gameEntity);
		gameEntity2.Name = gameEntity.Name + "Instance";
		gameEntity2.RemoveTag(tag);
		scene.AttachEntity(gameEntity2);
		gameEntity2.SetVisibilityExcludeParents(visible: true);
		gameEntity.SetVisibilityExcludeParents(visible: false);
		GameEntity gameEntity3 = scene.FindEntityWithTag(tag2);
		Vec3 dofParams = default(Vec3);
		camera = Camera.CreateCamera();
		if (gameEntity3 != null)
		{
			gameEntity3.GetCameraParamsFromCameraScript(camera, ref dofParams);
			camera.Frame = gameEntity3.GetGlobalFrame();
		}
		return gameEntity2;
	}

	private GameEntity FillEntityWithPose(CharacterCode characterCode, GameEntity poseEntity, Scene scene)
	{
		if (characterCode.IsEmpty)
		{
			Debug.FailedAssert("Trying to fill entity with empty character code", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\TableauCacheManager.cs", "FillEntityWithPose", 1536);
			return poseEntity;
		}
		if (string.IsNullOrEmpty(characterCode.EquipmentCode))
		{
			Debug.FailedAssert("Trying to fill entity with invalid equipment code", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\TableauCacheManager.cs", "FillEntityWithPose", 1542);
			return poseEntity;
		}
		if (TaleWorlds.Core.FaceGen.GetBaseMonsterFromRace(characterCode.Race) == null)
		{
			Debug.FailedAssert("There are no monster data for the race: " + characterCode.Race, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\TableauCacheManager.cs", "FillEntityWithPose", 1549);
			return poseEntity;
		}
		if (Current != null && poseEntity != null)
		{
			GetPoseParamsFromCharacterCode(characterCode, out var _, out var _);
			CharacterSpawner characterSpawner = poseEntity.GetScriptComponents<CharacterSpawner>().First();
			characterSpawner.SetCreateFaceImmediately(value: false);
			characterSpawner.InitWithCharacter(characterCode);
		}
		return poseEntity;
	}

	public static Camera CreateDefaultBannerCamera()
	{
		return CreateCamera(1f / 3f, 2f / 3f, -2f / 3f, -1f / 3f, 0.001f, 510f);
	}

	public static Camera CreateNineGridBannerCamera()
	{
		return CreateCamera(0f, 1f, -1f, 0f, 0.001f, 510f);
	}

	private static Camera CreateCamera(float left, float right, float bottom, float top, float near, float far)
	{
		Camera camera = Camera.CreateCamera();
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin.z = 400f;
		camera.Frame = identity;
		camera.LookAt(new Vec3(0f, 0f, 400f), new Vec3(0f, 0f, 0f, -1f), new Vec3(0f, 1f));
		camera.SetViewVolume(perspective: false, left, right, bottom, top, near, far);
		return camera;
	}
}
