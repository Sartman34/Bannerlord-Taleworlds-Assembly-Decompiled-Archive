namespace TaleWorlds.CampaignSystem;

internal class DialogFlowContext
{
	internal readonly string Token;

	internal readonly bool ByPlayer;

	internal readonly DialogFlowContext Parent;

	public DialogFlowContext(string token, bool byPlayer, DialogFlowContext parent)
	{
		Token = token;
		ByPlayer = byPlayer;
		Parent = parent;
	}
}
