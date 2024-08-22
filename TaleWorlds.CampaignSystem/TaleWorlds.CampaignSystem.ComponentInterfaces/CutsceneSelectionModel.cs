using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class CutsceneSelectionModel : GameModel
{
	public abstract SceneNotificationData GetKingdomDestroyedSceneNotification(Kingdom kingdom);
}
