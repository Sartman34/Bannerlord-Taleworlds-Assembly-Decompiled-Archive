using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.LogEntries;

public class SiegeAftermathLogEntry : LogEntry
{
	[SaveableField(10)]
	private readonly SiegeAftermathAction.SiegeAftermath _siegeAftermath;

	[SaveableField(20)]
	private readonly Settlement _capturedSettlement;

	[SaveableField(30)]
	private readonly Hero _decisionMaker;

	[SaveableField(40)]
	private readonly bool _playerWasInvolved;

	internal static void AutoGeneratedStaticCollectObjectsSiegeAftermathLogEntry(object o, List<object> collectedObjects)
	{
		((SiegeAftermathLogEntry)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_capturedSettlement);
		collectedObjects.Add(_decisionMaker);
	}

	internal static object AutoGeneratedGetMemberValue_siegeAftermath(object o)
	{
		return ((SiegeAftermathLogEntry)o)._siegeAftermath;
	}

	internal static object AutoGeneratedGetMemberValue_capturedSettlement(object o)
	{
		return ((SiegeAftermathLogEntry)o)._capturedSettlement;
	}

	internal static object AutoGeneratedGetMemberValue_decisionMaker(object o)
	{
		return ((SiegeAftermathLogEntry)o)._decisionMaker;
	}

	internal static object AutoGeneratedGetMemberValue_playerWasInvolved(object o)
	{
		return ((SiegeAftermathLogEntry)o)._playerWasInvolved;
	}

	public SiegeAftermathLogEntry(MobileParty leaderParty, IEnumerable<MobileParty> attackers, Settlement settlement, SiegeAftermathAction.SiegeAftermath siegeAftermath)
	{
		_siegeAftermath = siegeAftermath;
		_decisionMaker = leaderParty.LeaderHero;
		_capturedSettlement = settlement;
		_playerWasInvolved = false;
		foreach (MobileParty attacker in attackers)
		{
			if (attacker == MobileParty.MainParty)
			{
				_playerWasInvolved = true;
				break;
			}
		}
	}

	public override ImportanceEnum GetImportanceForClan(Clan clan)
	{
		return ImportanceEnum.Zero;
	}

	public override void GetConversationScoreAndComment(Hero talkTroop, bool findString, out string comment, out ImportanceEnum score)
	{
		score = ImportanceEnum.Zero;
		comment = "";
		if (_playerWasInvolved && Hero.MainHero.CurrentSettlement == _capturedSettlement && Hero.OneToOneConversationHero.IsNotable)
		{
			score = ImportanceEnum.VeryImportant;
			if (_siegeAftermath == SiegeAftermathAction.SiegeAftermath.ShowMercy)
			{
				comment = "str_comment_endplayerbattle_you_stormed_this_city_showed_mercy";
			}
			if (_siegeAftermath == SiegeAftermathAction.SiegeAftermath.Devastate)
			{
				comment = "str_comment_endplayerbattle_you_stormed_this_city_devastated";
			}
			if (_siegeAftermath == SiegeAftermathAction.SiegeAftermath.Pillage)
			{
				comment = "str_comment_endplayerbattle_you_stormed_this_city";
			}
		}
	}

	public override string ToString()
	{
		return GetNotificationText().ToString();
	}

	public TextObject GetNotificationText()
	{
		TextObject textObject = null;
		if (_siegeAftermath == SiegeAftermathAction.SiegeAftermath.ShowMercy)
		{
			textObject = new TextObject("{=wTh00qoj}{HERO.NAME} has showed mercy to {SETTLEMENT}.");
		}
		if (_siegeAftermath == SiegeAftermathAction.SiegeAftermath.Devastate)
		{
			textObject = new TextObject("{=NeTp63aU}{HERO.NAME} has devastated {SETTLEMENT}.");
		}
		if (_siegeAftermath == SiegeAftermathAction.SiegeAftermath.Pillage)
		{
			textObject = new TextObject("{=VzAqZucZ}{HERO.NAME} has pillaged {SETTLEMENT}.");
		}
		if (_decisionMaker != null)
		{
			textObject.SetCharacterProperties("HERO", _decisionMaker.CharacterObject);
		}
		textObject.SetTextVariable("SETTLEMENT", _capturedSettlement.Name);
		return textObject;
	}
}
