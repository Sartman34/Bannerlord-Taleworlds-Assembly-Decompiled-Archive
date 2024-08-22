using System.Collections.Generic;
using Helpers;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.LogEntries;

public class PregnancyLogEntry : LogEntry, IEncyclopediaLog, IChatNotification
{
	[SaveableField(300)]
	public readonly Hero Mother;

	public bool IsVisibleNotification => Mother.Clan.Equals(Hero.MainHero.Clan);

	public override ChatNotificationType NotificationType => CivilianNotification(Mother.Clan);

	public override CampaignTime KeepInHistoryTime => CampaignTime.Days(Campaign.Current.Models.PregnancyModel.PregnancyDurationInDays + 7f);

	internal static void AutoGeneratedStaticCollectObjectsPregnancyLogEntry(object o, List<object> collectedObjects)
	{
		((PregnancyLogEntry)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(Mother);
	}

	internal static object AutoGeneratedGetMemberValueMother(object o)
	{
		return ((PregnancyLogEntry)o).Mother;
	}

	public PregnancyLogEntry(Hero mother)
	{
		Mother = mother;
	}

	public override string ToString()
	{
		return GetEncyclopediaText().ToString();
	}

	public TextObject GetNotificationText()
	{
		return GetEncyclopediaText();
	}

	public bool IsVisibleInEncyclopediaPageOf<T>(T obj) where T : MBObjectBase
	{
		return obj == Mother;
	}

	public TextObject GetEncyclopediaText()
	{
		TextObject textObject = GameTexts.FindText("str_notification_pregnant");
		StringHelpers.SetCharacterProperties("MOTHER", Mother.CharacterObject, textObject);
		return textObject;
	}
}
