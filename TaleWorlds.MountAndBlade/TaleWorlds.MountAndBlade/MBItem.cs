using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public static class MBItem
{
	public static int GetItemUsageIndex(string itemUsageName)
	{
		return MBAPI.IMBItem.GetItemUsageIndex(itemUsageName);
	}

	public static int GetItemHolsterIndex(string itemHolsterName)
	{
		return MBAPI.IMBItem.GetItemHolsterIndex(itemHolsterName);
	}

	public static bool GetItemIsPassiveUsage(string itemUsageName)
	{
		return MBAPI.IMBItem.GetItemIsPassiveUsage(itemUsageName);
	}

	public static MatrixFrame GetHolsterFrameByIndex(int index)
	{
		MatrixFrame outFrame = default(MatrixFrame);
		MBAPI.IMBItem.GetHolsterFrameByIndex(index, ref outFrame);
		return outFrame;
	}

	public static ItemObject.ItemUsageSetFlags GetItemUsageSetFlags(string ItemUsageName)
	{
		return (ItemObject.ItemUsageSetFlags)MBAPI.IMBItem.GetItemUsageSetFlags(ItemUsageName);
	}

	public static ActionIndexValueCache GetItemUsageReloadActionCode(string itemUsageName, int usageDirection, bool isMounted, int leftHandUsageSetIndex, bool isLeftStance)
	{
		return new ActionIndexValueCache(MBAPI.IMBItem.GetItemUsageReloadActionCode(itemUsageName, usageDirection, isMounted, leftHandUsageSetIndex, isLeftStance));
	}

	public static int GetItemUsageStrikeType(string itemUsageName, int usageDirection, bool isMounted, int leftHandUsageSetIndex, bool isLeftStance)
	{
		return MBAPI.IMBItem.GetItemUsageStrikeType(itemUsageName, usageDirection, isMounted, leftHandUsageSetIndex, isLeftStance);
	}

	public static float GetMissileRange(float shotSpeed, float zDiff)
	{
		return MBAPI.IMBItem.GetMissileRange(shotSpeed, zDiff);
	}
}
