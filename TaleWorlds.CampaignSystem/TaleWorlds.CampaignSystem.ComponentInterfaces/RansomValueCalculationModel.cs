using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class RansomValueCalculationModel : GameModel
{
	public abstract int PrisonerRansomValue(CharacterObject prisoner, Hero sellerHero = null);
}
