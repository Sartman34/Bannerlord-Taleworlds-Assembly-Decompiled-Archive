using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class RomanceModel : GameModel
{
	public abstract int GetAttractionValuePercentage(Hero potentiallyInterestedCharacter, Hero heroOfInterest);
}
