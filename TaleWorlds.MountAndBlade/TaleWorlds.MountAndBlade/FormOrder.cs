using System;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public struct FormOrder
{
	public enum FormOrderEnum
	{
		Deep,
		Wide,
		Wider,
		Custom
	}

	private float _customFlankWidth;

	public readonly FormOrderEnum OrderEnum;

	public static readonly FormOrder FormOrderDeep = new FormOrder(FormOrderEnum.Deep);

	public static readonly FormOrder FormOrderWide = new FormOrder(FormOrderEnum.Wide);

	public static readonly FormOrder FormOrderWider = new FormOrder(FormOrderEnum.Wider);

	public float CustomFlankWidth
	{
		get
		{
			return _customFlankWidth;
		}
		set
		{
			_customFlankWidth = value;
		}
	}

	public OrderType OrderType => OrderEnum switch
	{
		FormOrderEnum.Wide => OrderType.FormWide, 
		FormOrderEnum.Wider => OrderType.FormWider, 
		FormOrderEnum.Custom => OrderType.FormCustom, 
		_ => OrderType.FormDeep, 
	};

	private FormOrder(FormOrderEnum orderEnum, float customFlankWidth = -1f)
	{
		OrderEnum = orderEnum;
		_customFlankWidth = customFlankWidth;
	}

	public static FormOrder FormOrderCustom(float customWidth)
	{
		return new FormOrder(FormOrderEnum.Custom, customWidth);
	}

	public void OnApply(Formation formation)
	{
		OnApplyToArrangement(formation, formation.Arrangement);
	}

	public static int GetUnitCountOf(Formation formation)
	{
		if (!formation.OverridenUnitCount.HasValue)
		{
			return formation.CountOfUnitsWithoutDetachedOnes;
		}
		return formation.OverridenUnitCount.Value;
	}

	public bool OnApplyToCustomArrangement(Formation formation, IFormationArrangement arrangement)
	{
		return false;
	}

	private void OnApplyToArrangement(Formation formation, IFormationArrangement arrangement)
	{
		if (OnApplyToCustomArrangement(formation, arrangement))
		{
			return;
		}
		if (arrangement is ColumnFormation)
		{
			ColumnFormation columnFormation = arrangement as ColumnFormation;
			if (GetUnitCountOf(formation) > 0)
			{
				columnFormation.FormFromWidth(GetRankVerticalFormFileCount(formation));
			}
		}
		else if (arrangement is RectilinearSchiltronFormation)
		{
			(arrangement as RectilinearSchiltronFormation).Form();
		}
		else if (arrangement is CircularSchiltronFormation)
		{
			(arrangement as CircularSchiltronFormation).Form();
		}
		else if (arrangement is CircularFormation)
		{
			CircularFormation circularFormation = arrangement as CircularFormation;
			int unitCountOf = GetUnitCountOf(formation);
			int? fileCount = GetFileCount(unitCountOf);
			float num = 0f;
			if (fileCount.HasValue)
			{
				int rankCount = TaleWorlds.Library.MathF.Max(1, TaleWorlds.Library.MathF.Ceiling((float)unitCountOf * 1f / (float)fileCount.Value));
				num = circularFormation.GetCircumferenceFromRankCount(rankCount);
			}
			else
			{
				num = System.MathF.PI * CustomFlankWidth;
			}
			circularFormation.FormFromCircumference(num);
		}
		else if (arrangement is SquareFormation)
		{
			SquareFormation squareFormation = arrangement as SquareFormation;
			int unitCountOf2 = GetUnitCountOf(formation);
			int? fileCount2 = GetFileCount(unitCountOf2);
			if (fileCount2.HasValue)
			{
				int rankCount2 = TaleWorlds.Library.MathF.Max(1, TaleWorlds.Library.MathF.Ceiling((float)unitCountOf2 * 1f / (float)fileCount2.Value));
				squareFormation.FormFromRankCount(rankCount2);
			}
			else
			{
				squareFormation.FormFromBorderSideWidth(CustomFlankWidth);
			}
		}
		else if (arrangement is SkeinFormation)
		{
			SkeinFormation skeinFormation = arrangement as SkeinFormation;
			int unitCountOf3 = GetUnitCountOf(formation);
			int? fileCount3 = GetFileCount(unitCountOf3);
			if (fileCount3.HasValue)
			{
				skeinFormation.FormFromFlankWidth(fileCount3.Value);
			}
			else
			{
				skeinFormation.FlankWidth = CustomFlankWidth;
			}
		}
		else if (arrangement is WedgeFormation)
		{
			WedgeFormation wedgeFormation = arrangement as WedgeFormation;
			int unitCountOf4 = GetUnitCountOf(formation);
			int? fileCount4 = GetFileCount(unitCountOf4);
			if (fileCount4.HasValue)
			{
				wedgeFormation.FormFromFlankWidth(fileCount4.Value);
			}
			else
			{
				wedgeFormation.FlankWidth = CustomFlankWidth;
			}
		}
		else if (arrangement is TransposedLineFormation)
		{
			TransposedLineFormation transposedLineFormation = arrangement as TransposedLineFormation;
			int unitCountOf5 = GetUnitCountOf(formation);
			if (unitCountOf5 > 0)
			{
				int? num2 = GetFileCount(unitCountOf5);
				if (!num2.HasValue)
				{
					num2 = transposedLineFormation.GetFileCountFromWidth(CustomFlankWidth);
				}
				TaleWorlds.Library.MathF.Ceiling((float)unitCountOf5 * 1f / (float)num2.Value);
				transposedLineFormation.FormFromFlankWidth(GetRankVerticalFormFileCount(formation));
			}
		}
		else if (arrangement is LineFormation)
		{
			LineFormation lineFormation = arrangement as LineFormation;
			int unitCountOf6 = GetUnitCountOf(formation);
			int? fileCount5 = GetFileCount(unitCountOf6);
			if (fileCount5.HasValue)
			{
				lineFormation.FormFromFlankWidth(fileCount5.Value, unitCountOf6 > 40);
			}
			else
			{
				lineFormation.FlankWidth = CustomFlankWidth;
			}
		}
		else
		{
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\Orders\\FormOrder.cs", "OnApplyToArrangement", 224);
		}
	}

	private int? GetFileCount(int unitCount)
	{
		return GetFileCountStatic(OrderEnum, unitCount);
	}

	public static int? GetFileCountStatic(FormOrderEnum order, int unitCount)
	{
		return GetFileCountAux(order, unitCount);
	}

	private int GetRankVerticalFormFileCount(IFormation formation)
	{
		switch (OrderEnum)
		{
		case FormOrderEnum.Wide:
			return 3;
		case FormOrderEnum.Wider:
			return 5;
		case FormOrderEnum.Deep:
			return 1;
		case FormOrderEnum.Custom:
			return TaleWorlds.Library.MathF.Floor((_customFlankWidth + formation.Interval) / (formation.UnitDiameter + formation.Interval));
		default:
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\Orders\\FormOrder.cs", "GetRankVerticalFormFileCount", 265);
			return 1;
		}
	}

	private static int? GetFileCountAux(FormOrderEnum order, int unitCount)
	{
		switch (order)
		{
		case FormOrderEnum.Wide:
			return TaleWorlds.Library.MathF.Max(TaleWorlds.Library.MathF.Round(TaleWorlds.Library.MathF.Sqrt((float)unitCount / 16f)), 1) * 16;
		case FormOrderEnum.Wider:
			return TaleWorlds.Library.MathF.Max(TaleWorlds.Library.MathF.Round(TaleWorlds.Library.MathF.Sqrt((float)unitCount / 64f)), 1) * 64;
		case FormOrderEnum.Deep:
			return TaleWorlds.Library.MathF.Max(TaleWorlds.Library.MathF.Round(TaleWorlds.Library.MathF.Sqrt((float)unitCount / 4f)), 1) * 4;
		case FormOrderEnum.Custom:
			return null;
		default:
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\Orders\\FormOrder.cs", "GetFileCountAux", 285);
			return null;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is FormOrder formOrder)
		{
			return formOrder == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)OrderEnum;
	}

	public static bool operator !=(FormOrder f1, FormOrder f2)
	{
		return f1.OrderEnum != f2.OrderEnum;
	}

	public static bool operator ==(FormOrder f1, FormOrder f2)
	{
		return f1.OrderEnum == f2.OrderEnum;
	}
}
