using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.LogEntries;

public class EndCaptivityLogEntry : LogEntry, IEncyclopediaLog, IChatNotification
{
	[SaveableField(730)]
	public readonly IFaction CapturerMapFaction;

	[SaveableField(731)]
	public readonly Hero Prisoner;

	[SaveableProperty(732)]
	public EndCaptivityDetail Detail { get; private set; }

	public bool IsVisibleNotification => true;

	public override ChatNotificationType NotificationType => MilitaryNotification(Prisoner.Clan, null);

	internal static void AutoGeneratedStaticCollectObjectsEndCaptivityLogEntry(object o, List<object> collectedObjects)
	{
		((EndCaptivityLogEntry)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(CapturerMapFaction);
		collectedObjects.Add(Prisoner);
	}

	internal static object AutoGeneratedGetMemberValueDetail(object o)
	{
		return ((EndCaptivityLogEntry)o).Detail;
	}

	internal static object AutoGeneratedGetMemberValueCapturerMapFaction(object o)
	{
		return ((EndCaptivityLogEntry)o).CapturerMapFaction;
	}

	internal static object AutoGeneratedGetMemberValuePrisoner(object o)
	{
		return ((EndCaptivityLogEntry)o).Prisoner;
	}

	public EndCaptivityLogEntry(Hero prisoner, IFaction capturerMapFaction, EndCaptivityDetail detail)
	{
		CapturerMapFaction = capturerMapFaction;
		Prisoner = prisoner;
		Detail = detail;
	}

	public override string ToString()
	{
		return GetEncyclopediaText().ToString();
	}

	public TextObject GetEncyclopediaText()
	{
		return GetNotificationText();
	}

	public bool IsVisibleInEncyclopediaPageOf<T>(T obj) where T : MBObjectBase
	{
		if (obj != Prisoner)
		{
			return obj == Prisoner.Clan;
		}
		return true;
	}

	public TextObject GetNotificationText()
	{
		TextObject textObject = new TextObject("{=6u3t174w}{PRISONER_LORD.LINK}{?PRISONER_LORD_HAS_FACTION_LINK} of the {PRISONER_LORD_FACTION_LINK}{?}{\\?} is now free.");
		switch (Detail)
		{
		case EndCaptivityDetail.Death:
			textObject = new TextObject("{=XbQFAKUz}{PRISONER_LORD.LINK}{?PRISONER_LORD_HAS_FACTION_LINK} of the {PRISONER_LORD_FACTION_LINK}{?}{\\?} has died while being held captive by the {CAPTURER_FACTION}.");
			break;
		case EndCaptivityDetail.Ransom:
			textObject = new TextObject("{=pX0MgdZA}{PRISONER_LORD.LINK}{?PRISONER_LORD_HAS_FACTION_LINK} of the {PRISONER_LORD_FACTION_LINK}{?}{\\?} has been ransomed from the {CAPTURER_FACTION}.");
			break;
		case EndCaptivityDetail.ReleasedAfterBattle:
			textObject = new TextObject("{=hp4jLl3M}{PRISONER_LORD.LINK}{?PRISONER_LORD_HAS_FACTION_LINK} of the {PRISONER_LORD_FACTION_LINK}{?}{\\?} has been released after battle.");
			break;
		case EndCaptivityDetail.ReleasedAfterEscape:
			textObject = new TextObject("{=krTrNonp}{PRISONER_LORD.LINK}{?PRISONER_LORD_HAS_FACTION_LINK} of the {PRISONER_LORD_FACTION_LINK}{?}{\\?} has escaped from captivity.");
			break;
		case EndCaptivityDetail.ReleasedAfterPeace:
			textObject = new TextObject("{=wlhJGG0q}{PRISONER_LORD.LINK}{?PRISONER_LORD_HAS_FACTION_LINK} of the {PRISONER_LORD_FACTION_LINK}{?}{\\?} has been freed because of a peace declaration.");
			break;
		case EndCaptivityDetail.ReleasedByCompensation:
			textObject = new TextObject("{=krTrNonp}{PRISONER_LORD.LINK}{?PRISONER_LORD_HAS_FACTION_LINK} of the {PRISONER_LORD_FACTION_LINK}{?}{\\?} has escaped from captivity.");
			break;
		}
		Clan clan = Prisoner.Clan;
		if (clan != null && !clan.IsMinorFaction)
		{
			textObject.SetTextVariable("PRISONER_LORD_FACTION_LINK", Prisoner.MapFaction.EncyclopediaLinkWithName);
			textObject.SetTextVariable("PRISONER_LORD_HAS_FACTION_LINK", 1);
		}
		StringHelpers.SetCharacterProperties("PRISONER_LORD", Prisoner.CharacterObject, textObject);
		if (CapturerMapFaction != null)
		{
			textObject.SetTextVariable("CAPTURER_FACTION", CapturerMapFaction.InformalName);
		}
		return textObject;
	}

	public override void GetConversationScoreAndComment(Hero talkTroop, bool findString, out string comment, out ImportanceEnum score)
	{
		score = ImportanceEnum.Zero;
		comment = "";
		if (talkTroop == Prisoner && talkTroop.IsPlayerCompanion)
		{
			if (Detail == EndCaptivityDetail.Ransom)
			{
				score = ImportanceEnum.VeryImportant;
				comment = "str_comment_captivity_release_companion_ransom";
			}
			else
			{
				score = ImportanceEnum.VeryImportant;
				comment = "str_comment_captivity_release_companion";
			}
		}
	}
}
