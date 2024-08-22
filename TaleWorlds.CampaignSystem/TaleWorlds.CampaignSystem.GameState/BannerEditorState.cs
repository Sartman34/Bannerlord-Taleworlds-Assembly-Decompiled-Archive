using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.GameState;

public class BannerEditorState : TaleWorlds.Core.GameState
{
	private IBannerEditorStateHandler _handler;

	public override bool IsMenuState => true;

	public IBannerEditorStateHandler Handler
	{
		get
		{
			return _handler;
		}
		set
		{
			_handler = value;
		}
	}

	public Clan GetClan()
	{
		return Clan.PlayerClan;
	}

	public CharacterObject GetCharacter()
	{
		return CharacterObject.PlayerCharacter;
	}
}
