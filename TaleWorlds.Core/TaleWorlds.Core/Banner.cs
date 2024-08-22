using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.Core;

public class Banner
{
	private enum BannerIconOrientation
	{
		None = -1,
		CentralPositionedOneIcon,
		CenteredTwoMirroredIcons,
		DiagonalIcons,
		HorizontalIcons,
		VerticalIcons,
		SquarePositionedFourIcons,
		NumberOfOrientation
	}

	public const int MaxSize = 8000;

	public const int BannerFullSize = 1528;

	public const int BannerEditableAreaSize = 512;

	public const int MaxIconCount = 32;

	private const char Splitter = '.';

	public const int BackgroundDataIndex = 0;

	public const int BannerIconDataIndex = 1;

	[SaveableField(1)]
	private readonly MBList<BannerData> _bannerDataList;

	[CachedData]
	private IBannerVisual _bannerVisual;

	public MBReadOnlyList<BannerData> BannerDataList => _bannerDataList;

	public IBannerVisual BannerVisual => _bannerVisual ?? (_bannerVisual = Game.Current.CreateBannerVisual(this));

	public Banner()
	{
		_bannerDataList = new MBList<BannerData>();
	}

	public Banner(Banner banner)
	{
		_bannerDataList = new MBList<BannerData>();
		foreach (BannerData bannerData in banner.BannerDataList)
		{
			_bannerDataList.Add(new BannerData(bannerData));
		}
	}

	public Banner(string bannerKey)
	{
		_bannerDataList = new MBList<BannerData>();
		Deserialize(bannerKey);
	}

	public Banner(string bannerKey, uint color1, uint color2)
	{
		_bannerDataList = new MBList<BannerData>();
		Deserialize(bannerKey);
		ChangePrimaryColor(color1);
		ChangeIconColors(color2);
	}

	public void SetBannerVisual(IBannerVisual visual)
	{
		_bannerVisual = visual;
	}

	public void ChangePrimaryColor(uint mainColor)
	{
		int colorId = BannerManager.GetColorId(mainColor);
		if (colorId >= 0)
		{
			BannerDataList[0].ColorId = colorId;
			BannerDataList[0].ColorId2 = colorId;
		}
	}

	public void ChangeBackgroundColor(uint primaryColor, uint secondaryColor)
	{
		int colorId = BannerManager.GetColorId(primaryColor);
		int colorId2 = BannerManager.GetColorId(secondaryColor);
		if (colorId >= 0 && colorId2 >= 0)
		{
			BannerDataList[0].ColorId = colorId;
			BannerDataList[0].ColorId2 = colorId2;
		}
	}

	public void ChangeIconColors(uint color)
	{
		int colorId = BannerManager.GetColorId(color);
		if (colorId >= 0)
		{
			for (int i = 1; i < BannerDataList.Count; i++)
			{
				BannerDataList[i].ColorId = colorId;
				BannerDataList[i].ColorId2 = colorId;
			}
		}
	}

	public void RotateBackgroundToRight()
	{
		BannerDataList[0].RotationValue -= 0.00278f;
		BannerDataList[0].RotationValue = ((BannerDataList[0].RotationValue < 0f) ? (BannerDataList[0].RotationValue + 1f) : BannerDataList[0].RotationValue);
	}

	public void RotateBackgroundToLeft()
	{
		BannerDataList[0].RotationValue += 0.00278f;
		BannerDataList[0].RotationValue = ((BannerDataList[0].RotationValue > 0f) ? (BannerDataList[0].RotationValue - 1f) : BannerDataList[0].RotationValue);
	}

	public void SetBackgroundMeshId(int meshId)
	{
		BannerDataList[0].MeshId = meshId;
	}

	public string Serialize()
	{
		return GetBannerCodeFromBannerDataList(_bannerDataList);
	}

	public void Deserialize(string message)
	{
		_bannerVisual = null;
		_bannerDataList.Clear();
		_bannerDataList.AddRange(GetBannerDataFromBannerCode(message));
	}

	public void ClearAllIcons()
	{
		BannerData item = _bannerDataList[0];
		_bannerDataList.Clear();
		_bannerDataList.Add(item);
	}

	public void AddIconData(BannerData iconData)
	{
		if (_bannerDataList.Count < 33)
		{
			_bannerDataList.Add(iconData);
		}
	}

	public void AddIconData(BannerData iconData, int index)
	{
		if (_bannerDataList.Count < 33 && index > 0 && index <= _bannerDataList.Count)
		{
			_bannerDataList.Insert(index, iconData);
		}
	}

	public void RemoveIconDataAtIndex(int index)
	{
		if (index > 0 && index < _bannerDataList.Count)
		{
			_bannerDataList.RemoveAt(index);
		}
	}

	public static Banner CreateRandomClanBanner(int seed = -1)
	{
		return CreateRandomBannerInternal(seed, BannerIconOrientation.CentralPositionedOneIcon);
	}

	public static Banner CreateRandomBanner()
	{
		return CreateRandomBannerInternal();
	}

	private static Banner CreateRandomBannerInternal(int seed = -1, BannerIconOrientation orientation = BannerIconOrientation.None)
	{
		_ = Game.Current;
		MBFastRandom mBFastRandom = ((seed == -1) ? new MBFastRandom() : new MBFastRandom((uint)seed));
		Banner banner = new Banner();
		BannerData iconData = new BannerData(BannerManager.Instance.GetRandomBackgroundId(mBFastRandom), mBFastRandom.Next(BannerManager.ColorPalette.Count), mBFastRandom.Next(BannerManager.ColorPalette.Count), new Vec2(1528f, 1528f), new Vec2(764f, 764f), drawStroke: false, mirror: false, 0f);
		banner.AddIconData(iconData);
		switch ((BannerIconOrientation)((orientation == BannerIconOrientation.None) ? mBFastRandom.Next(6) : ((int)orientation)))
		{
		case BannerIconOrientation.CentralPositionedOneIcon:
			banner.CentralPositionedOneIcon(mBFastRandom);
			break;
		case BannerIconOrientation.CenteredTwoMirroredIcons:
			banner.CenteredTwoMirroredIcons(mBFastRandom);
			break;
		case BannerIconOrientation.DiagonalIcons:
			banner.DiagonalIcons(mBFastRandom);
			break;
		case BannerIconOrientation.HorizontalIcons:
			banner.HorizontalIcons(mBFastRandom);
			break;
		case BannerIconOrientation.VerticalIcons:
			banner.VerticalIcons(mBFastRandom);
			break;
		case BannerIconOrientation.SquarePositionedFourIcons:
			banner.SquarePositionedFourIcons(mBFastRandom);
			break;
		}
		return banner;
	}

	public static Banner CreateOneColoredEmptyBanner(int colorIndex)
	{
		Banner banner = new Banner();
		BannerData iconData = new BannerData(BannerManager.Instance.GetRandomBackgroundId(new MBFastRandom()), colorIndex, colorIndex, new Vec2(1528f, 1528f), new Vec2(764f, 764f), drawStroke: false, mirror: false, 0f);
		banner.AddIconData(iconData);
		return banner;
	}

	public static Banner CreateOneColoredBannerWithOneIcon(uint backgroundColor, uint iconColor, int iconMeshId)
	{
		Banner banner = CreateOneColoredEmptyBanner(BannerManager.GetColorId(backgroundColor));
		if (iconMeshId == -1)
		{
			iconMeshId = BannerManager.Instance.GetRandomBannerIconId(new MBFastRandom());
		}
		banner.AddIconData(new BannerData(iconMeshId, BannerManager.GetColorId(iconColor), BannerManager.GetColorId(iconColor), new Vec2(512f, 512f), new Vec2(764f, 764f), drawStroke: false, mirror: false, 0f));
		return banner;
	}

	private void CentralPositionedOneIcon(MBFastRandom random)
	{
		int randomBannerIconId = BannerManager.Instance.GetRandomBannerIconId(random);
		int colorId = random.Next(BannerManager.ColorPalette.Count);
		bool flag = random.NextFloat() < 0.5f;
		int randomColorIdForStroke = GetRandomColorIdForStroke(flag, random);
		bool mirror = random.Next(2) == 0;
		float num = random.NextFloat();
		float rotationValue = 0f;
		if (num > 0.9f)
		{
			rotationValue = 0.25f;
		}
		else if (num > 0.8f)
		{
			rotationValue = 0.5f;
		}
		else if (num > 0.7f)
		{
			rotationValue = 0.75f;
		}
		BannerData iconData = new BannerData(randomBannerIconId, colorId, randomColorIdForStroke, new Vec2(512f, 512f), new Vec2(764f, 764f), flag, mirror, rotationValue);
		AddIconData(iconData);
	}

	private void DiagonalIcons(MBFastRandom random)
	{
		int num = ((random.NextFloat() < 0.5f) ? 2 : 3);
		bool flag = random.NextFloat() < 0.5f;
		int num2 = (512 - 20 * (num + 1)) / num;
		int num3 = BannerManager.Instance.GetRandomBannerIconId(random);
		int num4 = random.Next(BannerManager.ColorPalette.Count);
		bool flag2 = random.NextFloat() < 0.5f;
		int randomColorIdForStroke = GetRandomColorIdForStroke(flag2, random);
		int num5 = (512 - num * num2) / (num + 1);
		bool flag3 = random.NextFloat() < 0.3f;
		bool flag4 = flag3 || random.NextFloat() < 0.3f;
		for (int i = 0; i < num; i++)
		{
			num3 = (flag3 ? BannerManager.Instance.GetRandomBannerIconId(random) : num3);
			num4 = (flag4 ? random.Next(BannerManager.ColorPalette.Count) : num4);
			int num6 = i * (num2 + num5) + num5 + num2 / 2;
			int num7 = i * (num2 + num5) + num5 + num2 / 2;
			if (flag)
			{
				num7 = 512 - num7;
			}
			BannerData iconData = new BannerData(num3, num4, randomColorIdForStroke, new Vec2(num2, num2), new Vec2(num6 + 508, num7 + 508), flag2, mirror: false, 0f);
			AddIconData(iconData);
		}
	}

	private void HorizontalIcons(MBFastRandom random)
	{
		int num = ((random.NextFloat() < 0.5f) ? 2 : 3);
		int num2 = (512 - 20 * (num + 1)) / num;
		int num3 = BannerManager.Instance.GetRandomBannerIconId(random);
		int num4 = random.Next(BannerManager.ColorPalette.Count);
		bool flag = random.NextFloat() < 0.5f;
		int randomColorIdForStroke = GetRandomColorIdForStroke(flag, random);
		int num5 = (512 - num * num2) / (num + 1);
		bool flag2 = random.NextFloat() < 0.3f;
		bool flag3 = flag2 || random.NextFloat() < 0.3f;
		for (int i = 0; i < num; i++)
		{
			num3 = (flag2 ? BannerManager.Instance.GetRandomBannerIconId(random) : num3);
			num4 = (flag3 ? random.Next(BannerManager.ColorPalette.Count) : num4);
			int num6 = i * (num2 + num5) + num5 + num2 / 2;
			BannerData iconData = new BannerData(num3, num4, randomColorIdForStroke, new Vec2(num2, num2), new Vec2(num6 + 508, 764f), flag, mirror: false, 0f);
			AddIconData(iconData);
		}
	}

	private void VerticalIcons(MBFastRandom random)
	{
		int num = ((random.NextFloat() < 0.5f) ? 2 : 3);
		int num2 = (512 - 20 * (num + 1)) / num;
		int num3 = BannerManager.Instance.GetRandomBannerIconId(random);
		int num4 = random.Next(BannerManager.ColorPalette.Count);
		bool flag = random.NextFloat() < 0.5f;
		int randomColorIdForStroke = GetRandomColorIdForStroke(flag, random);
		int num5 = (512 - num * num2) / (num + 1);
		bool flag2 = random.NextFloat() < 0.3f;
		bool flag3 = flag2 || random.NextFloat() < 0.3f;
		for (int i = 0; i < num; i++)
		{
			num3 = (flag2 ? BannerManager.Instance.GetRandomBannerIconId(random) : num3);
			num4 = (flag3 ? random.Next(BannerManager.ColorPalette.Count) : num4);
			int num6 = i * (num2 + num5) + num5 + num2 / 2;
			BannerData iconData = new BannerData(num3, num4, randomColorIdForStroke, new Vec2(num2, num2), new Vec2(764f, num6 + 508), flag, mirror: false, 0f);
			AddIconData(iconData);
		}
	}

	private void SquarePositionedFourIcons(MBFastRandom random)
	{
		bool flag = random.NextFloat() < 0.5f;
		int num;
		int num2;
		if (!flag)
		{
			num = ((random.NextFloat() < 0.5f) ? 1 : 0);
			if (num != 0)
			{
				num2 = 1;
				goto IL_0034;
			}
		}
		else
		{
			num = 0;
		}
		num2 = ((random.NextFloat() < 0.5f) ? 1 : 0);
		goto IL_0034;
		IL_0034:
		bool flag2 = (byte)num2 != 0;
		bool flag3 = random.NextFloat() < 0.5f;
		int randomBannerIconId = BannerManager.Instance.GetRandomBannerIconId(random);
		int randomColorIdForStroke = GetRandomColorIdForStroke(flag3, random);
		int num3 = random.Next(BannerManager.ColorPalette.Count);
		BannerData iconData = new BannerData(randomBannerIconId, num3, randomColorIdForStroke, new Vec2(220f, 220f), new Vec2(654f, 654f), flag3, mirror: false, 0f);
		AddIconData(iconData);
		randomBannerIconId = ((num != 0) ? BannerManager.Instance.GetRandomBannerIconId(random) : randomBannerIconId);
		num3 = (flag2 ? random.Next(BannerManager.ColorPalette.Count) : num3);
		iconData = new BannerData(randomBannerIconId, num3, randomColorIdForStroke, new Vec2(220f, 220f), new Vec2(874f, 654f), flag3, flag, 0f);
		AddIconData(iconData);
		randomBannerIconId = ((num != 0) ? BannerManager.Instance.GetRandomBannerIconId(random) : randomBannerIconId);
		num3 = (flag2 ? random.Next(BannerManager.ColorPalette.Count) : num3);
		iconData = new BannerData(randomBannerIconId, num3, randomColorIdForStroke, new Vec2(220f, 220f), new Vec2(654f, 874f), flag3, flag, flag ? 0.5f : 0f);
		AddIconData(iconData);
		randomBannerIconId = ((num != 0) ? BannerManager.Instance.GetRandomBannerIconId(random) : randomBannerIconId);
		num3 = (flag2 ? random.Next(BannerManager.ColorPalette.Count) : num3);
		iconData = new BannerData(randomBannerIconId, num3, randomColorIdForStroke, new Vec2(220f, 220f), new Vec2(874f, 874f), flag3, mirror: false, flag ? 0.5f : 0f);
		AddIconData(iconData);
	}

	private void CenteredTwoMirroredIcons(MBFastRandom random)
	{
		bool flag = random.NextFloat() < 0.5f;
		bool flag2 = random.NextFloat() < 0.5f;
		int randomBannerIconId = BannerManager.Instance.GetRandomBannerIconId(random);
		int randomColorIdForStroke = GetRandomColorIdForStroke(flag2, random);
		int num = random.Next(BannerManager.ColorPalette.Count);
		BannerData iconData = new BannerData(randomBannerIconId, num, randomColorIdForStroke, new Vec2(200f, 200f), new Vec2(664f, 764f), flag2, mirror: false, 0f);
		AddIconData(iconData);
		num = (flag ? random.Next(BannerManager.ColorPalette.Count) : num);
		iconData = new BannerData(randomBannerIconId, num, randomColorIdForStroke, new Vec2(200f, 200f), new Vec2(864f, 764f), flag2, mirror: true, 0f);
		AddIconData(iconData);
	}

	private int GetRandomColorIdForStroke(bool hasStroke, MBFastRandom random)
	{
		if (!hasStroke)
		{
			return BannerManager.ColorPalette.Count - 1;
		}
		return random.Next(BannerManager.ColorPalette.Count);
	}

	public uint GetPrimaryColor()
	{
		if (BannerDataList.Count <= 0)
		{
			return uint.MaxValue;
		}
		return BannerManager.GetColor(BannerDataList[0].ColorId);
	}

	public uint GetSecondaryColor()
	{
		if (BannerDataList.Count <= 0)
		{
			return uint.MaxValue;
		}
		return BannerManager.GetColor(BannerDataList[0].ColorId2);
	}

	public uint GetFirstIconColor()
	{
		if (BannerDataList.Count <= 1)
		{
			return uint.MaxValue;
		}
		return BannerManager.GetColor(BannerDataList[1].ColorId);
	}

	public int GetVersionNo()
	{
		int num = 0;
		for (int i = 0; i < _bannerDataList.Count; i++)
		{
			num += _bannerDataList[i].LocalVersion;
		}
		return num;
	}

	public static string GetBannerCodeFromBannerDataList(MBList<BannerData> bannerDataList)
	{
		MBStringBuilder mBStringBuilder = default(MBStringBuilder);
		mBStringBuilder.Initialize(16, "GetBannerCodeFromBannerDataList");
		bool flag = true;
		foreach (BannerData bannerData in bannerDataList)
		{
			if (!flag)
			{
				mBStringBuilder.Append('.');
			}
			flag = false;
			mBStringBuilder.Append(bannerData.MeshId);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append(bannerData.ColorId);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append(bannerData.ColorId2);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append((int)bannerData.Size.x);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append((int)bannerData.Size.y);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append((int)bannerData.Position.x);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append((int)bannerData.Position.y);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append(bannerData.DrawStroke ? 1 : 0);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append(bannerData.Mirror ? 1 : 0);
			mBStringBuilder.Append('.');
			mBStringBuilder.Append((int)(bannerData.RotationValue / 0.00278f));
		}
		return mBStringBuilder.ToStringAndRelease();
	}

	public static List<BannerData> GetBannerDataFromBannerCode(string bannerCode)
	{
		List<BannerData> list = new List<BannerData>();
		string[] array = bannerCode.Split(new char[1] { '.' });
		for (int i = 0; i + 10 <= array.Length; i += 10)
		{
			BannerData item = new BannerData(int.Parse(array[i]), int.Parse(array[i + 1]), int.Parse(array[i + 2]), new Vec2(int.Parse(array[i + 3]), int.Parse(array[i + 4])), new Vec2(int.Parse(array[i + 5]), int.Parse(array[i + 6])), int.Parse(array[i + 7]) == 1, int.Parse(array[i + 8]) == 1, (float)int.Parse(array[i + 9]) * 0.00278f);
			list.Add(item);
		}
		return list;
	}

	internal static void AutoGeneratedStaticCollectObjectsBanner(object o, List<object> collectedObjects)
	{
		((Banner)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected virtual void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		collectedObjects.Add(_bannerDataList);
	}

	internal static object AutoGeneratedGetMemberValue_bannerDataList(object o)
	{
		return ((Banner)o)._bannerDataList;
	}
}
