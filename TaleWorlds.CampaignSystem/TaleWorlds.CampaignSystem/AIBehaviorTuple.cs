using System;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;

namespace TaleWorlds.CampaignSystem;

public struct AIBehaviorTuple : IEquatable<AIBehaviorTuple>
{
	public IMapPoint Party;

	public AiBehavior AiBehavior;

	public bool WillGatherArmy;

	public AIBehaviorTuple(IMapPoint party, AiBehavior aiBehavior, bool willGatherArmy = false)
	{
		Party = party;
		AiBehavior = aiBehavior;
		WillGatherArmy = willGatherArmy;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AIBehaviorTuple aIBehaviorTuple))
		{
			return false;
		}
		if (aIBehaviorTuple.Party == Party && aIBehaviorTuple.AiBehavior == AiBehavior)
		{
			return aIBehaviorTuple.WillGatherArmy == WillGatherArmy;
		}
		return false;
	}

	public bool Equals(AIBehaviorTuple other)
	{
		if (other.Party == Party && other.AiBehavior == AiBehavior)
		{
			return other.WillGatherArmy == WillGatherArmy;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int aiBehavior = (int)AiBehavior;
		int hashCode = aiBehavior.GetHashCode();
		hashCode = ((Party != null) ? ((hashCode * 397) ^ Party.GetHashCode()) : hashCode);
		return (hashCode * 397) ^ WillGatherArmy.GetHashCode();
	}

	public static bool operator ==(AIBehaviorTuple a, AIBehaviorTuple b)
	{
		if (a.Party == b.Party && a.AiBehavior == b.AiBehavior)
		{
			return a.WillGatherArmy == b.WillGatherArmy;
		}
		return false;
	}

	public static bool operator !=(AIBehaviorTuple a, AIBehaviorTuple b)
	{
		return !(a == b);
	}
}
