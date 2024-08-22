using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Party.PartyComponents;

public class BanditPartyComponent : WarPartyComponent
{
	[CachedData]
	private TextObject _cachedName;

	[SaveableField(3)]
	private readonly Settlement _relatedSettlement;

	[SaveableProperty(1)]
	public Hideout Hideout { get; private set; }

	[SaveableProperty(2)]
	public bool IsBossParty { get; private set; }

	public override Settlement HomeSettlement
	{
		get
		{
			if (Hideout == null)
			{
				return _relatedSettlement;
			}
			return Hideout.Settlement;
		}
	}

	public override Hero PartyOwner => base.MobileParty.ActualClan?.Leader;

	public override TextObject Name
	{
		get
		{
			TextObject obj = (Game.Current.IsDevelopmentMode ? new TextObject(base.MobileParty.StringId) : (_cachedName ?? (_cachedName = ((Hideout != null) ? Hideout.MapFaction.Name : base.MobileParty.MapFaction.Name))));
			obj.SetTextVariable("IS_BANDIT", 1);
			return obj;
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsBanditPartyComponent(object o, List<object> collectedObjects)
	{
		((BanditPartyComponent)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_relatedSettlement);
		collectedObjects.Add(Hideout);
	}

	internal static object AutoGeneratedGetMemberValueHideout(object o)
	{
		return ((BanditPartyComponent)o).Hideout;
	}

	internal static object AutoGeneratedGetMemberValueIsBossParty(object o)
	{
		return ((BanditPartyComponent)o).IsBossParty;
	}

	internal static object AutoGeneratedGetMemberValue_relatedSettlement(object o)
	{
		return ((BanditPartyComponent)o)._relatedSettlement;
	}

	public static MobileParty CreateBanditParty(string stringId, Clan clan, Hideout hideout, bool isBossParty)
	{
		return MobileParty.CreateParty(stringId, new BanditPartyComponent(hideout, isBossParty), delegate(MobileParty mobileParty)
		{
			mobileParty.ActualClan = clan;
		});
	}

	public static MobileParty CreateLooterParty(string stringId, Clan clan, Settlement relatedSettlement, bool isBossParty)
	{
		return MobileParty.CreateParty(stringId, new BanditPartyComponent(relatedSettlement), delegate(MobileParty mobileParty)
		{
			mobileParty.ActualClan = clan;
		});
	}

	protected internal BanditPartyComponent(Hideout hideout, bool isBossParty)
	{
		Hideout = hideout;
		IsBossParty = isBossParty;
	}

	protected internal BanditPartyComponent(Settlement relatedSettlement)
	{
		_relatedSettlement = relatedSettlement;
	}

	public void SetHomeHideout(Hideout hideout)
	{
		Hideout = hideout;
	}

	public override void ClearCachedName()
	{
		_cachedName = null;
	}
}
