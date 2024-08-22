namespace TaleWorlds.CampaignSystem.Conversation.Tags;

public class OldTag : ConversationTag
{
	public const string Id = "OldTag";

	public override string StringId => "OldTag";

	public override bool IsApplicableTo(CharacterObject character)
	{
		return character.Age > 49f;
	}
}
