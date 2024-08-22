namespace TaleWorlds.Core;

public abstract class ItemCategorySelector : GameModel
{
	public abstract ItemCategory GetItemCategoryForItem(ItemObject itemObject);
}
