using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem;

public class PartyThinkParams
{
	public MobileParty MobilePartyOf;

	private readonly MBList<(AIBehaviorTuple, float)> _aiBehaviorScores;

	public float CurrentObjectiveValue;

	public bool WillGatherAnArmy;

	public bool DoNotChangeBehavior;

	public float StrengthOfLordsWithoutArmy;

	public float StrengthOfLordsWithArmy;

	public float StrengthOfLordsAtSameClanWithoutArmy;

	public MBReadOnlyList<(AIBehaviorTuple, float)> AIBehaviorScores => _aiBehaviorScores;

	public PartyThinkParams(MobileParty mobileParty)
	{
		_aiBehaviorScores = new MBList<(AIBehaviorTuple, float)>(16);
		MobilePartyOf = mobileParty;
		WillGatherAnArmy = false;
		DoNotChangeBehavior = false;
		CurrentObjectiveValue = 0f;
	}

	public void Reset(MobileParty mobileParty)
	{
		_aiBehaviorScores.Clear();
		MobilePartyOf = mobileParty;
		WillGatherAnArmy = false;
		DoNotChangeBehavior = false;
		CurrentObjectiveValue = 0f;
		StrengthOfLordsWithoutArmy = 0f;
		StrengthOfLordsWithArmy = 0f;
		StrengthOfLordsAtSameClanWithoutArmy = 0f;
	}

	public void Initialization()
	{
		StrengthOfLordsWithoutArmy = 0f;
		StrengthOfLordsWithArmy = 0f;
		StrengthOfLordsAtSameClanWithoutArmy = 0f;
		foreach (Hero hero in MobilePartyOf.MapFaction.Heroes)
		{
			if (hero.PartyBelongedTo == null)
			{
				continue;
			}
			MobileParty partyBelongedTo = hero.PartyBelongedTo;
			if (partyBelongedTo.Army != null)
			{
				StrengthOfLordsWithArmy += partyBelongedTo.Party.TotalStrength;
				continue;
			}
			StrengthOfLordsWithoutArmy += partyBelongedTo.Party.TotalStrength;
			if (hero.Clan == MobilePartyOf.LeaderHero?.Clan)
			{
				StrengthOfLordsAtSameClanWithoutArmy += partyBelongedTo.Party.TotalStrength;
			}
		}
	}

	public bool TryGetBehaviorScore(in AIBehaviorTuple aiBehaviorTuple, out float score)
	{
		foreach (var aiBehaviorScore in _aiBehaviorScores)
		{
			var (aIBehaviorTuple, _) = aiBehaviorScore;
			if (aIBehaviorTuple.Equals(aiBehaviorTuple))
			{
				score = aiBehaviorScore.Item2;
				return true;
			}
		}
		score = 0f;
		return false;
	}

	public void SetBehaviorScore(in AIBehaviorTuple aiBehaviorTuple, float score)
	{
		for (int i = 0; i < _aiBehaviorScores.Count; i++)
		{
			if (_aiBehaviorScores[i].Item1.Equals(aiBehaviorTuple))
			{
				_aiBehaviorScores[i] = (_aiBehaviorScores[i].Item1, score);
				return;
			}
		}
		Debug.FailedAssert("AIBehaviorScore not found.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\ICampaignBehaviorManager.cs", "SetBehaviorScore", 152);
	}

	public void AddBehaviorScore(in (AIBehaviorTuple, float) value)
	{
		_aiBehaviorScores.Add(value);
	}
}
