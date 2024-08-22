using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.AgentOrigins;

public class SimpleAgentOrigin : IAgentOriginBase
{
	private CharacterObject _troop;

	private Banner _banner;

	private UniqueTroopDescriptor _descriptor;

	public BasicCharacterObject Troop => _troop;

	public bool IsUnderPlayersCommand
	{
		get
		{
			PartyBase party = Party;
			if (party != null)
			{
				if (party != PartyBase.MainParty && party.Owner != Hero.MainHero)
				{
					return party.MapFaction.Leader == Hero.MainHero;
				}
				return true;
			}
			return false;
		}
	}

	public uint FactionColor
	{
		get
		{
			if (Party != null)
			{
				return Party.MapFaction.Color;
			}
			if (_troop.IsHero)
			{
				return _troop.HeroObject.MapFaction.Color;
			}
			return 0u;
		}
	}

	public uint FactionColor2
	{
		get
		{
			if (Party != null)
			{
				return Party.MapFaction.Color2;
			}
			if (_troop.IsHero)
			{
				return _troop.HeroObject.MapFaction.Color2;
			}
			return 0u;
		}
	}

	public int Seed
	{
		get
		{
			if (Party != null)
			{
				return CharacterHelper.GetPartyMemberFaceSeed(Party, _troop, Rank);
			}
			return CharacterHelper.GetDefaultFaceSeed(_troop, Rank);
		}
	}

	public PartyBase Party
	{
		get
		{
			if (!_troop.IsHero || _troop.HeroObject.PartyBelongedTo == null)
			{
				return null;
			}
			return _troop.HeroObject.PartyBelongedTo.Party;
		}
	}

	public IBattleCombatant BattleCombatant => Party;

	public Banner Banner => _banner;

	public int Rank { get; private set; }

	public int UniqueSeed => _descriptor.UniqueSeed;

	public SimpleAgentOrigin(BasicCharacterObject troop, int rank = -1, Banner banner = null, UniqueTroopDescriptor descriptor = default(UniqueTroopDescriptor))
	{
		_troop = (CharacterObject)troop;
		_descriptor = descriptor;
		Rank = ((rank == -1) ? MBRandom.RandomInt(10000) : rank);
		_banner = banner;
	}

	public void SetWounded()
	{
	}

	public void SetKilled()
	{
		if (_troop.IsHero)
		{
			KillCharacterAction.ApplyByBattle(_troop.HeroObject, null);
		}
	}

	public void SetRouted()
	{
	}

	public void OnAgentRemoved(float agentHealth)
	{
	}

	void IAgentOriginBase.OnScoreHit(BasicCharacterObject victim, BasicCharacterObject captain, int damage, bool isFatal, bool isTeamKill, WeaponComponentData attackerWeapon)
	{
		if (isTeamKill)
		{
			CharacterObject troop = _troop;
			Campaign.Current.Models.CombatXpModel.GetXpFromHit(troop, (CharacterObject)captain, (CharacterObject)victim, Party, damage, isFatal, CombatXpModel.MissionTypeEnum.Battle, out var xpAmount);
			if (troop.IsHero && attackerWeapon != null)
			{
				SkillObject skillForWeapon = Campaign.Current.Models.CombatXpModel.GetSkillForWeapon(attackerWeapon, isSiegeEngineHit: false);
				troop.HeroObject.AddSkillXp(skillForWeapon, xpAmount);
			}
		}
	}

	public void SetBanner(Banner banner)
	{
		_banner = banner;
	}
}
