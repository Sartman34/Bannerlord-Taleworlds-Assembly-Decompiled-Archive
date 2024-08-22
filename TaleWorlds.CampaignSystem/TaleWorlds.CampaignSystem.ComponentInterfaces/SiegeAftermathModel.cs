using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class SiegeAftermathModel : GameModel
{
	public abstract int GetSiegeAftermathTraitXpChangeForPlayer(TraitObject trait, Settlement devastatedSettlement, SiegeAftermathAction.SiegeAftermath aftermathType);
}
