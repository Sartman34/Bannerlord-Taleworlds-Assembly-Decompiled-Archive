using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Diamond;

public class InventoryData
{
	public List<ItemData> Items { get; private set; }

	public InventoryData()
	{
		Items = new List<ItemData>();
	}

	public ItemData GetItemWithIndex(int itemIndex)
	{
		return Items.SingleOrDefault((ItemData q) => q.Index == itemIndex);
	}

	public void DebugPrint()
	{
		string text = "";
		foreach (ItemData item in Items)
		{
			text = text + item.Index + " " + item.TypeId + "\n";
		}
		Debug.Print(text);
	}
}
