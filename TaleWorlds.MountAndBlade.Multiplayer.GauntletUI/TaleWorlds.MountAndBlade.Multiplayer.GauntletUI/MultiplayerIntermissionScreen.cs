using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Intermission;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI;

[GameStateScreen(typeof(LobbyGameStateCustomGameClient))]
[GameStateScreen(typeof(LobbyGameStateCommunityClient))]
public class MultiplayerIntermissionScreen : ScreenBase, IGameStateListener, IGauntletChatLogHandlerScreen
{
	private MPIntermissionVM _dataSource;

	private SpriteCategory _customGameClientCategory;

	public GauntletLayer Layer { get; private set; }

	public MultiplayerIntermissionScreen(LobbyGameStateCustomGameClient gameState)
	{
		Construct();
	}

	public MultiplayerIntermissionScreen(LobbyGameStateCommunityClient gameState)
	{
		Construct();
	}

	private void Construct()
	{
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_customGameClientCategory = spriteData.SpriteCategories["ui_mpintermission"];
		_customGameClientCategory.Load(resourceContext, uIResourceDepot);
		_dataSource = new MPIntermissionVM();
		Layer = new GauntletLayer(100);
		Layer.IsFocusLayer = true;
		AddLayer(Layer);
		Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		Layer.LoadMovie("MultiplayerIntermission", _dataSource);
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		_dataSource.Tick();
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		_customGameClientCategory.Unload();
		Layer.InputRestrictions.ResetInputRestrictions();
		Layer = null;
		_dataSource.OnFinalize();
		_dataSource = null;
	}

	void IGameStateListener.OnActivate()
	{
		Layer.InputRestrictions.SetInputRestrictions();
		ScreenManager.TrySetFocus(Layer);
		LoadingWindow.EnableGlobalLoadingWindow();
	}

	void IGameStateListener.OnDeactivate()
	{
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}

	void IGauntletChatLogHandlerScreen.TryUpdateChatLogLayerParameters(ref bool isTeamChatAvailable, ref bool inputEnabled, ref InputContext inputContext)
	{
		if (Layer != null)
		{
			isTeamChatAvailable = false;
			inputEnabled = true;
			inputContext = Layer.Input;
		}
	}
}
