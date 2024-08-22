using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Party.PartyComponents;

public class MilitiaPartyComponent : PartyComponent
{
	[CachedData]
	private TextObject _cachedName;

	[SaveableProperty(1)]
	public Settlement Settlement { get; private set; }

	public override Hero PartyOwner => Settlement.OwnerClan.Leader;

	public override TextObject Name
	{
		get
		{
			if (_cachedName == null)
			{
				_cachedName = GameTexts.FindText("str_militia_name");
				_cachedName.SetTextVariable("SETTLEMENT_NAME", Settlement.Name);
			}
			return _cachedName;
		}
	}

	public override Settlement HomeSettlement => Settlement;

	internal static void AutoGeneratedStaticCollectObjectsMilitiaPartyComponent(object o, List<object> collectedObjects)
	{
		((MilitiaPartyComponent)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(Settlement);
	}

	internal static object AutoGeneratedGetMemberValueSettlement(object o)
	{
		return ((MilitiaPartyComponent)o).Settlement;
	}

	public static MobileParty CreateMilitiaParty(string stringId, Settlement settlement)
	{
		MobileParty mobileParty2 = MobileParty.CreateParty("militias_of_" + stringId + "_aaa1", new MilitiaPartyComponent(settlement), delegate(MobileParty mobileParty)
		{
			(mobileParty.PartyComponent as MilitiaPartyComponent).InitializeMilitiaPartyProperties(mobileParty, settlement);
		});
		EnterSettlementAction.ApplyForParty(mobileParty2, settlement);
		return mobileParty2;
	}

	protected override void OnInitialize()
	{
		Settlement.MilitiaPartyComponent = this;
	}

	protected override void OnFinalize()
	{
		Settlement.MilitiaPartyComponent = null;
	}

	public override void ClearCachedName()
	{
		_cachedName = null;
	}

	private void InitializeMilitiaPartyProperties(MobileParty mobileParty, Settlement settlement)
	{
		PartyTemplateObject militiaPartyTemplate = settlement.Culture.MilitiaPartyTemplate;
		mobileParty.InitializeMobilePartyAtPosition(militiaPartyTemplate, settlement.GatePosition);
		mobileParty.Party.SetVisualAsDirty();
		mobileParty.Ai.DisableAi();
		mobileParty.Aggressiveness = 0f;
	}

	protected internal MilitiaPartyComponent(Settlement settlement)
	{
		Settlement = settlement;
	}
}
