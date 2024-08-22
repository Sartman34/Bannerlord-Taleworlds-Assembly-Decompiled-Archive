using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultPartyDesertionModel : PartyDesertionModel
{
	public override int GetMoraleThresholdForTroopDesertion(MobileParty party)
	{
		return 10;
	}

	public override int GetNumberOfDeserters(MobileParty mobileParty)
	{
		bool num = mobileParty.IsWageLimitExceeded();
		bool flag = mobileParty.Party.NumberOfAllMembers > mobileParty.LimitedPartySize;
		int result = 0;
		if (num)
		{
			int num2 = mobileParty.TotalWage - mobileParty.PaymentLimit;
			result = MathF.Min(20, MathF.Max(1, (int)((float)num2 / Campaign.Current.AverageWage * 0.25f)));
		}
		else if (flag)
		{
			result = ((!mobileParty.IsGarrison) ? ((mobileParty.Party.NumberOfAllMembers > mobileParty.LimitedPartySize) ? MathF.Max(1, (int)((float)(mobileParty.Party.NumberOfAllMembers - mobileParty.LimitedPartySize) * 0.25f)) : 0) : MathF.Ceiling((float)(mobileParty.Party.NumberOfAllMembers - mobileParty.LimitedPartySize) * 0.25f));
		}
		return result;
	}
}
