using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.LogEntries;

public class KingdomDestroyedLogEntry : LogEntry, IEncyclopediaLog, IChatNotification
{
	[SaveableField(10)]
	private Kingdom _kingdom;

	public bool IsVisibleNotification => true;

	internal static void AutoGeneratedStaticCollectObjectsKingdomDestroyedLogEntry(object o, List<object> collectedObjects)
	{
		((KingdomDestroyedLogEntry)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_kingdom);
	}

	internal static object AutoGeneratedGetMemberValue_kingdom(object o)
	{
		return ((KingdomDestroyedLogEntry)o)._kingdom;
	}

	public KingdomDestroyedLogEntry(Kingdom kingdom)
	{
		_kingdom = kingdom;
	}

	public bool IsVisibleInEncyclopediaPageOf<T>(T obj) where T : MBObjectBase
	{
		return obj == _kingdom;
	}

	public TextObject GetEncyclopediaText()
	{
		TextObject textObject = GameTexts.FindText("str_kingdom_destroyed");
		textObject.SetTextVariable("KINGDOM", _kingdom.EncyclopediaLinkWithName);
		return textObject;
	}

	public TextObject GetNotificationText()
	{
		return GetEncyclopediaText();
	}
}