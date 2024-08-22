using System.Collections.Generic;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class NodeComparer : IComparer<ThumbnailCacheNode>
{
	public int Compare(ThumbnailCacheNode x, ThumbnailCacheNode y)
	{
		return x.FrameNo.CompareTo(y.FrameNo);
	}
}
