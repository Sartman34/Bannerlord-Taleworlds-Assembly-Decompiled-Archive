using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public class CaptainScoreboardData : IScoreboardData
{
	public MissionScoreboardComponent.ScoreboardHeader[] GetScoreboardHeaders()
	{
		GameNetwork.MyPeer.GetComponent<MissionRepresentativeBase>();
		return new MissionScoreboardComponent.ScoreboardHeader[9]
		{
			new MissionScoreboardComponent.ScoreboardHeader("ping", (MissionPeer missionPeer) => MathF.Round(missionPeer.GetNetworkPeer().AveragePingInMilliseconds).ToString(), (BotData bot) => ""),
			new MissionScoreboardComponent.ScoreboardHeader("avatar", (MissionPeer missionPeer) => "", (BotData bot) => ""),
			new MissionScoreboardComponent.ScoreboardHeader("badge", (MissionPeer missionPeer) => BadgeManager.GetByIndex(missionPeer.GetPeer().ChosenBadgeIndex)?.StringId, (BotData bot) => ""),
			new MissionScoreboardComponent.ScoreboardHeader("name", (MissionPeer missionPeer) => missionPeer.GetComponent<MissionPeer>().DisplayedName, (BotData bot) => new TextObject("{=hvQSOi79}Bot").ToString()),
			new MissionScoreboardComponent.ScoreboardHeader("kill", (MissionPeer missionPeer) => missionPeer.KillCount.ToString(), (BotData bot) => bot.KillCount.ToString()),
			new MissionScoreboardComponent.ScoreboardHeader("death", (MissionPeer missionPeer) => missionPeer.DeathCount.ToString(), (BotData bot) => bot.DeathCount.ToString()),
			new MissionScoreboardComponent.ScoreboardHeader("assist", (MissionPeer missionPeer) => missionPeer.AssistCount.ToString(), (BotData bot) => bot.AssistCount.ToString()),
			new MissionScoreboardComponent.ScoreboardHeader("soldiers", delegate(MissionPeer missionPeer)
			{
				TextObject textObject2 = new TextObject("{=4FVIuqsl}{ALIVE}/{TOTAL}");
				textObject2.SetTextVariable("ALIVE", missionPeer.BotsUnderControlAlive + (missionPeer.IsControlledAgentActive ? 1 : 0));
				textObject2.SetTextVariable("TOTAL", missionPeer.BotsUnderControlTotal + 1);
				return textObject2.ToString();
			}, delegate(BotData bot)
			{
				TextObject textObject = new TextObject("{=4FVIuqsl}{ALIVE}/{TOTAL}");
				textObject.SetTextVariable("ALIVE", bot.AliveCount);
				textObject.SetTextVariable("TOTAL", 0);
				return textObject.ToString();
			}),
			new MissionScoreboardComponent.ScoreboardHeader("score", (MissionPeer missionPeer) => missionPeer.Score.ToString(), (BotData bot) => bot.Score.ToString())
		};
	}
}
