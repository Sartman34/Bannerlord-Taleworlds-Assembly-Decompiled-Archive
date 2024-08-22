namespace TaleWorlds.Core;

public class BannerCode
{
	public string Code { get; private set; }

	public Banner CalculateBanner()
	{
		return new Banner(Code);
	}

	public static BannerCode CreateFrom(Banner banner)
	{
		BannerCode bannerCode = new BannerCode();
		if (banner != null)
		{
			bannerCode.Code = banner.Serialize();
		}
		return bannerCode;
	}

	public static BannerCode CreateFrom(string bannerCodeCode)
	{
		return new BannerCode
		{
			Code = bannerCodeCode
		};
	}

	public override int GetHashCode()
	{
		return Code.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is BannerCode))
		{
			return false;
		}
		if (((BannerCode)obj).Code != Code)
		{
			return false;
		}
		return true;
	}

	public static bool operator ==(BannerCode a, BannerCode b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(BannerCode a, BannerCode b)
	{
		return !(a == b);
	}
}
