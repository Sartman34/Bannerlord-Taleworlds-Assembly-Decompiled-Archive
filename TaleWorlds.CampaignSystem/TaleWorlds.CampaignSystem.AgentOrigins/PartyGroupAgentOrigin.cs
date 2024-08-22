using System;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TroopSuppliers;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.AgentOrigins;

public class PartyGroupAgentOrigin : IAgentOriginBase
{
	private readonly PartyGroupTroopSupplier _supplier;

	private readonly UniqueTroopDescriptor _descriptor;

	private readonly int _rank;

	private bool _isRemoved;

	public PartyBase Party => _supplier.GetParty(_descriptor);

	public IBattleCombatant BattleCombatant => Party;

	public Banner Banner
	{
		get
		{
			if (Party.LeaderHero == null)
			{
				return Party.MapFaction.Banner;
			}
			return Party.LeaderHero.ClanBanner;
		}
	}

	public int UniqueSeed => _descriptor.UniqueSeed;

	public CharacterObject Troop => _supplier.GetTroop(_descriptor);

	BasicCharacterObject IAgentOriginBase.Troop => Troop;

	public UniqueTroopDescriptor TroopDesc => _descriptor;

	public int Rank => _rank;

	public bool IsUnderPlayersCommand
	{
		get
		{
			if (Troop == Hero.MainHero.CharacterObject)
			{
				return true;
			}
			return IsPartyUnderPlayerCommand(Party);
		}
	}

	public uint FactionColor => Party.MapFaction.Color;

	public uint FactionColor2 => Party.MapFaction.Color2;

	public int Seed => CharacterHelper.GetPartyMemberFaceSeed(Party, Troop, Rank);

	internal PartyGroupAgentOrigin(PartyGroupTroopSupplier supplier, UniqueTroopDescriptor descriptor, int rank)
	{
		_supplier = supplier;
		_descriptor = descriptor;
		_rank = rank;
	}

	public void SetWounded()
	{
		if (!_isRemoved)
		{
			_supplier.OnTroopWounded(_descriptor);
			_isRemoved = true;
		}
	}

	public void SetKilled()
	{
		if (!_isRemoved)
		{
			_supplier.OnTroopKilled(_descriptor);
			if (Troop.IsHero)
			{
				KillCharacterAction.ApplyByBattle(Troop.HeroObject, null);
			}
			_isRemoved = true;
		}
	}

	public void SetRouted()
	{
		if (!_isRemoved)
		{
			_supplier.OnTroopRouted(_descriptor);
			_isRemoved = true;
		}
	}

	public void OnAgentRemoved(float agentHealth)
	{
		if (Troop.IsHero)
		{
			Troop.HeroObject.HitPoints = TaleWorlds.Library.MathF.Max(1, TaleWorlds.Library.MathF.Round(agentHealth));
		}
	}

	void IAgentOriginBase.OnScoreHit(BasicCharacterObject victim, BasicCharacterObject captain, int damage, bool isFatal, bool isTeamKill, WeaponComponentData attackerWeapon)
	{
		_supplier.OnTroopScoreHit(_descriptor, victim, damage, isFatal, isTeamKill, attackerWeapon);
	}

	public void SetBanner(Banner banner)
	{
		throw new NotImplementedException();
	}

	public static bool IsPartyUnderPlayerCommand(PartyBase party)
	{
		if (party == PartyBase.MainParty)
		{
			return true;
		}
		if (party.Side != PartyBase.MainParty.Side)
		{
			return false;
		}
		bool num = party.Owner == Hero.MainHero;
		bool flag = party.MapFaction?.Leader == Hero.MainHero;
		bool flag2 = party.MobileParty != null && party.MobileParty.DefaultBehavior == AiBehavior.EscortParty && party.MobileParty.TargetParty == MobileParty.MainParty;
		bool flag3 = party.MobileParty != null && party.MobileParty.Army != null && party.MobileParty.Army.LeaderParty == MobileParty.MainParty;
		Settlement mapEventSettlement = party.MapEvent.MapEventSettlement;
		bool flag4 = mapEventSettlement != null && mapEventSettlement.OwnerClan.Leader == Hero.MainHero;
		return num || flag || flag2 || flag3 || flag4;
	}
}
