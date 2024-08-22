using System.Collections.Generic;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;

public class HeroRelationComparer : IComparer<HeroVM>
{
	private readonly Hero _pageHero;

	private readonly bool _isAscending;

	public HeroRelationComparer(Hero pageHero, bool isAscending)
	{
		_pageHero = pageHero;
		_isAscending = isAscending;
	}

	int IComparer<HeroVM>.Compare(HeroVM x, HeroVM y)
	{
		int heroRelation = CharacterRelationManager.GetHeroRelation(_pageHero, x.Hero);
		int heroRelation2 = CharacterRelationManager.GetHeroRelation(_pageHero, y.Hero);
		return heroRelation.CompareTo(heroRelation2) * (_isAscending ? 1 : (-1));
	}
}
