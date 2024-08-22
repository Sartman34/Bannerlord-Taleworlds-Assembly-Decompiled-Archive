using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class BattleCaptainModel : GameModel
{
	public abstract float GetCaptainRatingForTroopUsages(Hero hero, TroopUsageFlags flag, out List<PerkObject> compatiblePerks);
}
