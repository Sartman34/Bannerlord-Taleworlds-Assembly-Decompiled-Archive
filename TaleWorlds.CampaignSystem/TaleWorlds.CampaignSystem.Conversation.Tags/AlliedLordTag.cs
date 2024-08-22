namespace TaleWorlds.CampaignSystem.Conversation.Tags;

public class AlliedLordTag : ConversationTag
{
	public const string Id = "PlayerIsAlliedTag";

	public override string StringId => "PlayerIsAlliedTag";

	public override bool IsApplicableTo(CharacterObject character)
	{
		if (character.IsHero)
		{
			return FactionManager.IsAlliedWithFaction(character.HeroObject.MapFaction, Hero.MainHero.MapFaction);
		}
		return false;
	}
}
