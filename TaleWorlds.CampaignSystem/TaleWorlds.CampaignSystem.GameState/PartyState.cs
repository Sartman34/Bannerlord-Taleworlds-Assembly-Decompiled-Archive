using System;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.GameState;

public class PartyState : PlayerGameState
{
	private IPartyScreenLogicHandler _handler;

	public override bool IsMenuState => true;

	public PartyScreenLogic PartyScreenLogic { get; private set; }

	public IPartyScreenLogicHandler Handler
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

	public void InitializeLogic(PartyScreenLogic partyScreenLogic)
	{
		PartyScreenLogic = partyScreenLogic;
	}

	public void RequestUserInput(string text, Action accept, Action cancel)
	{
		if (Handler != null)
		{
			Handler.RequestUserInput(text, accept, cancel);
		}
	}
}
