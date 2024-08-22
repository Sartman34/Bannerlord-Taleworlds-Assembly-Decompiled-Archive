using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;

public class KingdomTruceItemVM : KingdomDiplomacyItemVM
{
	private readonly Action<KingdomTruceItemVM> _onAction;

	private readonly Action<KingdomDiplomacyItemVM> _onSelection;

	private int _tributePaid;

	public int TributePaid
	{
		get
		{
			return _tributePaid;
		}
		set
		{
			if (value != _tributePaid)
			{
				_tributePaid = value;
				OnPropertyChangedWithValue(value, "TributePaid");
			}
		}
	}

	public KingdomTruceItemVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction)
		: base(faction1, faction2)
	{
		_onAction = onAction;
		_onSelection = onSelection;
		UpdateDiplomacyProperties();
	}

	protected override void OnSelect()
	{
		UpdateDiplomacyProperties();
		_onSelection(this);
	}

	protected override void UpdateDiplomacyProperties()
	{
		base.UpdateDiplomacyProperties();
		base.Stats.Add(new KingdomWarComparableStatVM((int)Faction1.TotalStrength, (int)Faction2.TotalStrength, GameTexts.FindText("str_total_strength"), _faction1Color, _faction2Color, 10000));
		base.Stats.Add(new KingdomWarComparableStatVM(_faction1Towns.Count, _faction2Towns.Count, GameTexts.FindText("str_towns"), _faction1Color, _faction2Color, 25, new BasicTooltipViewModel(() => CampaignUIHelper.GetTruceOwnedSettlementsTooltip(_faction1Towns, Faction1.Name, isTown: true)), new BasicTooltipViewModel(() => CampaignUIHelper.GetTruceOwnedSettlementsTooltip(_faction2Towns, Faction2.Name, isTown: true))));
		base.Stats.Add(new KingdomWarComparableStatVM(_faction1Castles.Count, _faction2Castles.Count, GameTexts.FindText("str_castles"), _faction1Color, _faction2Color, 25, new BasicTooltipViewModel(() => CampaignUIHelper.GetTruceOwnedSettlementsTooltip(_faction1Castles, Faction1.Name, isTown: false)), new BasicTooltipViewModel(() => CampaignUIHelper.GetTruceOwnedSettlementsTooltip(_faction2Castles, Faction2.Name, isTown: false))));
		StanceLink stanceWith = _playerKingdom.GetStanceWith(Faction2);
		TributePaid = stanceWith.GetDailyTributePaid(_playerKingdom);
		if (stanceWith.IsNeutral && TributePaid != 0)
		{
			base.Stats.Add(new KingdomWarComparableStatVM(stanceWith.GetTotalTributePaid(Faction2), stanceWith.GetTotalTributePaid(Faction1), GameTexts.FindText("str_comparison_tribute_received"), _faction1Color, _faction2Color, 10000));
		}
	}

	protected override void ExecuteAction()
	{
		_onAction(this);
	}
}
