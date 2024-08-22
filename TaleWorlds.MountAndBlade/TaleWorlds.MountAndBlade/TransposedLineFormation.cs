using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class TransposedLineFormation : LineFormation
{
	public TransposedLineFormation(IFormation owner)
		: base(owner)
	{
		base.IsStaggered = false;
		IsTransforming = true;
	}

	public override IFormationArrangement Clone(IFormation formation)
	{
		return new TransposedLineFormation(formation);
	}

	public override void RearrangeFrom(IFormationArrangement arrangement)
	{
		if (arrangement is ColumnFormation)
		{
			int unitCount = arrangement.UnitCount;
			if (unitCount > 0)
			{
				int? fileCountStatic = FormOrder.GetFileCountStatic(((Formation)owner).FormOrder.OrderEnum, unitCount);
				if (fileCountStatic.HasValue)
				{
					int unitCountOnLine = MathF.Ceiling((float)unitCount * 1f / (float)fileCountStatic.Value);
					FormFromFlankWidth(unitCountOnLine);
				}
			}
		}
		base.RearrangeFrom(arrangement);
	}
}
