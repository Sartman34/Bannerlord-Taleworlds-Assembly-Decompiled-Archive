namespace TaleWorlds.Core;

public static class BannerExtensions
{
	public static bool IsContentsSameWith(this Banner banner, Banner otherBanner)
	{
		if (banner == null && otherBanner == null)
		{
			return true;
		}
		if (banner == null || otherBanner == null)
		{
			return false;
		}
		if (banner.BannerDataList.Count != otherBanner.BannerDataList.Count)
		{
			return false;
		}
		for (int i = 0; i < banner.BannerDataList.Count; i++)
		{
			BannerData bannerData = banner.BannerDataList[i];
			BannerData obj = otherBanner.BannerDataList[i];
			if (!bannerData.Equals(obj))
			{
				return false;
			}
		}
		return true;
	}
}
