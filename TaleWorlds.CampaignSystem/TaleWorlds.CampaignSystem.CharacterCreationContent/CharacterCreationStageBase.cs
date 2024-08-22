namespace TaleWorlds.CampaignSystem.CharacterCreationContent;

public abstract class CharacterCreationStageBase
{
	public CharacterCreationState CharacterCreationState { get; }

	public ICharacterCreationStageListener Listener { get; set; }

	protected CharacterCreationStageBase(CharacterCreationState state)
	{
		CharacterCreationState = state;
	}

	protected internal virtual void OnFinalize()
	{
		Listener?.OnStageFinalize();
	}
}
