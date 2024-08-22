namespace TaleWorlds.CampaignSystem.Actions;

public class ChangeRulingClanAction
{
	private static void ApplyInternal(Kingdom kingdom, Clan clan)
	{
		kingdom.RulingClan = clan;
		CampaignEventDispatcher.Instance.OnRulingClanChanged(kingdom, clan);
	}

	public static void Apply(Kingdom kingdom, Clan clan)
	{
		ApplyInternal(kingdom, clan);
	}
}
