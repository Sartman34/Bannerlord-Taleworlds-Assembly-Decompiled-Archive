using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.MapNotificationTypes;

public class KingdomDecisionMapNotification : InformationData
{
	public override TextObject TitleText => Decision.GetGeneralTitle();

	public override string SoundEventPath => "event:/ui/notification/kingdom_decision";

	[SaveableProperty(1)]
	public Kingdom KingdomOfDecision { get; private set; }

	[SaveableProperty(2)]
	public KingdomDecision Decision { get; private set; }

	internal static void AutoGeneratedStaticCollectObjectsKingdomDecisionMapNotification(object o, List<object> collectedObjects)
	{
		((KingdomDecisionMapNotification)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(KingdomOfDecision);
		collectedObjects.Add(Decision);
	}

	internal static object AutoGeneratedGetMemberValueKingdomOfDecision(object o)
	{
		return ((KingdomDecisionMapNotification)o).KingdomOfDecision;
	}

	internal static object AutoGeneratedGetMemberValueDecision(object o)
	{
		return ((KingdomDecisionMapNotification)o).Decision;
	}

	public KingdomDecisionMapNotification(Kingdom kingdom, KingdomDecision decision, TextObject descriptionText)
		: base(descriptionText)
	{
		KingdomOfDecision = kingdom;
		Decision = decision;
	}

	public override bool IsValid()
	{
		if (Decision is KingdomPolicyDecision kingdomPolicyDecision)
		{
			return kingdomPolicyDecision.Policy.IsReady;
		}
		return true;
	}
}
