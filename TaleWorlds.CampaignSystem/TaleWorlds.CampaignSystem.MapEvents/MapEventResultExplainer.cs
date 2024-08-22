namespace TaleWorlds.CampaignSystem.MapEvents;

public class MapEventResultExplainer
{
	public ExplainedNumber InfluenceExplainedNumber = new ExplainedNumber(0f, includeDescriptions: true);

	public ExplainedNumber RenownExplainedNumber = new ExplainedNumber(0f, includeDescriptions: true);

	public ExplainedNumber GoldExplainedNumber = new ExplainedNumber(0f, includeDescriptions: true);

	public ExplainedNumber MoraleExplainedNumber = new ExplainedNumber(0f, includeDescriptions: true);

	public ExplainedNumber PlunderedGoldExplainedNumber = new ExplainedNumber(0f, includeDescriptions: true);
}
