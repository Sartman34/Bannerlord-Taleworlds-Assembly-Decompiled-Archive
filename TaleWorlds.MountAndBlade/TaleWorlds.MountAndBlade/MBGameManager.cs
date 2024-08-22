using System.Collections.Generic;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade;

public abstract class MBGameManager : GameManagerBase
{
	private readonly object _lockObject = new object();

	public bool IsEnding { get; private set; }

	public new static MBGameManager Current => (MBGameManager)GameManagerBase.Current;

	public bool IsLoaded { get; protected set; }

	public override float ApplicationTime => MBCommon.GetApplicationTime();

	public override bool CheatMode => NativeConfig.CheatMode;

	public override bool IsDevelopmentMode => NativeConfig.IsDevelopmentMode;

	public override bool IsEditModeOn => MBEditor.IsEditModeOn;

	public override UnitSpawnPrioritizations UnitSpawnPrioritization => (UnitSpawnPrioritizations)BannerlordConfig.UnitSpawnPrioritization;

	protected MBGameManager()
	{
		IsEnding = false;
		NativeConfig.OnConfigChanged();
	}

	protected static void StartNewGame()
	{
		MBAPI.IMBGame.StartNew();
	}

	protected static void LoadModuleData(bool isLoadGame)
	{
		MBAPI.IMBGame.LoadModuleData(isLoadGame);
	}

	public static void StartNewGame(MBGameManager gameLoader)
	{
		GameLoadingState gameLoadingState = GameStateManager.Current.CreateState<GameLoadingState>();
		gameLoadingState.SetLoadingParameters(gameLoader);
		GameStateManager.Current.CleanAndPushState(gameLoadingState);
	}

	public override void BeginGameStart(Game game)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.BeginGameStart(game);
		}
	}

	public override void OnNewCampaignStart(Game game, object starterObject)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.OnCampaignStart(game, starterObject);
		}
	}

	public override void RegisterSubModuleObjects(bool isSavedCampaign)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.RegisterSubModuleObjects(isSavedCampaign);
		}
	}

	public override void AfterRegisterSubModuleObjects(bool isSavedCampaign)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.AfterRegisterSubModuleObjects(isSavedCampaign);
		}
	}

	public override void InitializeGameStarter(Game game, IGameStarter starterObject)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.InitializeGameStarter(game, starterObject);
		}
	}

	public override void OnGameInitializationFinished(Game game)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.OnGameInitializationFinished(game);
		}
		foreach (SkeletonScale objectType in Game.Current.ObjectManager.GetObjectTypeList<SkeletonScale>())
		{
			sbyte[] array = new sbyte[objectType.BoneNames.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Skeleton.GetBoneIndexFromName(objectType.SkeletonModel, objectType.BoneNames[i]);
			}
			objectType.SetBoneIndices(array);
		}
	}

	public override void OnAfterGameInitializationFinished(Game game, object initializerObject)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.OnAfterGameInitializationFinished(game, initializerObject);
		}
	}

	public override void OnGameLoaded(Game game, object initializerObject)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.OnGameLoaded(game, initializerObject);
		}
	}

	public override void OnNewGameCreated(Game game, object initializerObject)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.OnNewGameCreated(game, initializerObject);
		}
	}

	public override void OnGameStart(Game game, IGameStarter gameStarter)
	{
		Game.Current.MonsterMissionDataCreator = new MonsterMissionDataCreator();
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.OnGameStart(game, gameStarter);
		}
		Game.Current.AddGameModelsManager<MissionGameModels>(gameStarter.Models);
		Monster.GetBoneIndexWithId = MBActionSet.GetBoneIndexWithId;
		Monster.GetBoneHasParentBone = MBActionSet.GetBoneHasParentBone;
	}

	public override void OnGameEnd(Game game)
	{
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.OnGameEnd(game);
		}
		MissionGameModels.Clear();
		base.OnGameEnd(game);
	}

	public static async void EndGame()
	{
		while (true)
		{
			MBGameManager current = Current;
			if (current == null || current.IsLoaded)
			{
				break;
			}
			await Task.Delay(100);
		}
		MBGameManager current2 = Current;
		if ((current2 != null && !current2.CheckAndSetEnding()) || Game.Current.GameStateManager == null)
		{
			return;
		}
		while (Mission.Current != null && !(Game.Current.GameStateManager.ActiveState is MissionState))
		{
			Game.Current.GameStateManager.PopState();
		}
		if (Game.Current.GameStateManager.ActiveState is MissionState)
		{
			((MissionState)Game.Current.GameStateManager.ActiveState).CurrentMission.EndMission();
			while (Mission.Current != null)
			{
				await Task.Delay(1);
			}
		}
		else
		{
			Game.Current.GameStateManager.CleanStates();
		}
	}

	public override void OnLoadFinished()
	{
		IsLoaded = true;
	}

	public bool CheckAndSetEnding()
	{
		lock (_lockObject)
		{
			if (IsEnding)
			{
				return false;
			}
			IsEnding = true;
			return true;
		}
	}

	public virtual void OnSessionInvitationAccepted(SessionInvitationType targetGameType)
	{
		if (targetGameType != 0)
		{
			EndGame();
		}
	}

	public virtual void OnPlatformRequestedMultiplayer()
	{
		EndGame();
	}

	protected List<MbObjectXmlInformation> GetXmlInformationFromModule()
	{
		return XmlResource.XmlInformationList;
	}
}
