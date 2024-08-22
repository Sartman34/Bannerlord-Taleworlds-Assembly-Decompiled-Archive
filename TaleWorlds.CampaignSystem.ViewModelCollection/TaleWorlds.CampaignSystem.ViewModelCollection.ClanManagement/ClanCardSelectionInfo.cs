using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;

public readonly struct ClanCardSelectionInfo
{
	public readonly TextObject Title;

	public readonly IEnumerable<ClanCardSelectionItemInfo> Items;

	public readonly Action<List<object>, Action> OnClosedAction;

	public readonly bool IsMultiSelection;

	public ClanCardSelectionInfo(TextObject title, IEnumerable<ClanCardSelectionItemInfo> items, Action<List<object>, Action> onClosedAction, bool isMultiSelection)
	{
		Title = title;
		Items = items;
		OnClosedAction = onClosedAction;
		IsMultiSelection = isMultiSelection;
	}
}
