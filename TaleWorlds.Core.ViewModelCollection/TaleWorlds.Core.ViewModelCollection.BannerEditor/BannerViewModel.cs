using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;

namespace TaleWorlds.Core.ViewModelCollection.BannerEditor;

public class BannerViewModel : ViewModel
{
	private string _bannerCode = "";

	private const int _backgroundIndex = 0;

	private const int _bannerIconIndex = 1;

	public Banner Banner { get; }

	[DataSourceProperty]
	public string BannerCode
	{
		get
		{
			return _bannerCode;
		}
		set
		{
			if (value != _bannerCode)
			{
				_bannerCode = value;
				OnPropertyChangedWithValue(value, "BannerCode");
				Banner.Deserialize(value);
			}
		}
	}

	public BannerViewModel(Banner banner)
	{
		Banner = banner;
	}

	public void SetCode(string code)
	{
		BannerCode = code;
	}

	public void SetIconMeshID(int meshID)
	{
		Banner.BannerDataList[1].MeshId = meshID;
	}

	public void SetPrimaryColorId(int colorID)
	{
		Banner.BannerDataList[0].ColorId = colorID;
	}

	public void SetSecondaryColorId(int colorID)
	{
		Banner.BannerDataList[0].ColorId2 = colorID;
	}

	public void SetSigilColorId(int colorID)
	{
		Banner.BannerDataList[1].ColorId = colorID;
	}

	public void SetIconSize(int newSize)
	{
		Banner.BannerDataList[1].Size = new Vec2(newSize, newSize);
	}

	public int GetPrimaryColorId()
	{
		return Banner.BannerDataList[0].ColorId;
	}

	public uint GetPrimaryColor()
	{
		return BannerManager.Instance.ReadOnlyColorPalette.First((KeyValuePair<int, BannerColor> w) => w.Key == GetPrimaryColorId()).Value.Color;
	}

	public int GetSecondaryColorId()
	{
		return Banner.BannerDataList[0].ColorId2;
	}

	public int GetSigilColorId()
	{
		return Banner.BannerDataList[1].ColorId;
	}

	public uint GetSigilColor()
	{
		return BannerManager.Instance.ReadOnlyColorPalette.First((KeyValuePair<int, BannerColor> w) => w.Key == GetSigilColorId()).Value.Color;
	}
}
