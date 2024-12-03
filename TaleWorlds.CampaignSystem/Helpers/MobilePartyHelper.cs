using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Helpers;

public static class MobilePartyHelper
{
	public delegate void ResumePartyEscortBehaviorDelegate();

	public static MobileParty SpawnLordParty(Hero hero, Settlement spawnSettlement)
	{
		return SpawnLordPartyAux(hero, spawnSettlement.GatePosition, 0f, spawnSettlement);
	}

	public static MobileParty SpawnLordParty(Hero hero, Vec2 position, float spawnRadius)
	{
		return SpawnLordPartyAux(hero, position, spawnRadius, null);
	}

	private static MobileParty SpawnLordPartyAux(Hero hero, Vec2 position, float spawnRadius, Settlement spawnSettlement)
	{
		return LordPartyComponent.CreateLordParty(hero.CharacterObject.StringId, hero, position, spawnRadius, spawnSettlement, hero);
	}

	public static void CreateNewClanMobileParty(Hero partyLeader, Clan clan, out bool leaderCameFromMainParty)
	{
		leaderCameFromMainParty = PartyBase.MainParty.MemberRoster.Contains(partyLeader.CharacterObject);
		GiveGoldAction.ApplyBetweenCharacters(null, partyLeader, 3000, disableNotification: true);
		clan.CreateNewMobileParty(partyLeader).Ai.SetMoveModeHold();
	}

	public static void DesertTroopsFromParty(MobileParty party, int stackNo, int numberOfDeserters, int numberOfWoundedDeserters, ref TroopRoster desertedTroopList)
	{
		TroopRosterElement elementCopyAtIndex = party.MemberRoster.GetElementCopyAtIndex(stackNo);
		party.MemberRoster.AddToCounts(elementCopyAtIndex.Character, -(numberOfDeserters + numberOfWoundedDeserters), insertAtFront: false, -numberOfWoundedDeserters);
		if (desertedTroopList == null)
		{
			desertedTroopList = TroopRoster.CreateDummyTroopRoster();
		}
		desertedTroopList.AddToCounts(elementCopyAtIndex.Character, numberOfDeserters + numberOfWoundedDeserters, insertAtFront: false, numberOfWoundedDeserters);
	}

	public static bool IsHeroAssignableForScoutInParty(Hero hero, MobileParty party)
	{
		if (hero.PartyBelongedTo == party && hero != party.GetRoleHolder(SkillEffect.PerkRole.Scout))
		{
			return hero.GetSkillValue(DefaultSkills.Scouting) >= 0;
		}
		return false;
	}

	public static bool IsHeroAssignableForEngineerInParty(Hero hero, MobileParty party)
	{
		if (hero.PartyBelongedTo == party && hero != party.GetRoleHolder(SkillEffect.PerkRole.Engineer))
		{
			return hero.GetSkillValue(DefaultSkills.Engineering) >= 0;
		}
		return false;
	}

	public static bool IsHeroAssignableForSurgeonInParty(Hero hero, MobileParty party)
	{
		if (hero.PartyBelongedTo == party && hero != party.GetRoleHolder(SkillEffect.PerkRole.Surgeon))
		{
			return hero.GetSkillValue(DefaultSkills.Medicine) >= 0;
		}
		return false;
	}

	public static bool IsHeroAssignableForQuartermasterInParty(Hero hero, MobileParty party)
	{
		if (hero.PartyBelongedTo == party && hero != party.GetRoleHolder(SkillEffect.PerkRole.Quartermaster))
		{
			return hero.GetSkillValue(DefaultSkills.Trade) >= 0;
		}
		return false;
	}

	public static Hero GetHeroWithHighestSkill(MobileParty party, SkillObject skill)
	{
		Hero result = null;
		int num = -1;
		for (int i = 0; i < party.MemberRoster.Count; i++)
		{
			CharacterObject characterAtIndex = party.MemberRoster.GetCharacterAtIndex(i);
			if (characterAtIndex.HeroObject != null && characterAtIndex.HeroObject.GetSkillValue(skill) > num)
			{
				num = characterAtIndex.HeroObject.GetSkillValue(skill);
				result = characterAtIndex.HeroObject;
			}
		}
		return result;
	}

	public static TroopRoster GetStrongestAndPriorTroops(MobileParty mobileParty, int maxTroopCount, bool includePlayer)
	{
		FlattenedTroopRoster flattenedTroopRoster = mobileParty.MemberRoster.ToFlattenedRoster();
		flattenedTroopRoster.RemoveIf((FlattenedTroopRosterElement x) => x.IsWounded);
		return GetStrongestAndPriorTroops(flattenedTroopRoster, maxTroopCount, includePlayer);
	}

	public static TroopRoster GetStrongestAndPriorTroops(FlattenedTroopRoster roster, int maxTroopCount, bool includePlayer)
	{
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		List<CharacterObject> list = (from x in roster
			select x.Troop into x
			orderby x.Level descending
			select x).ToList();
		if (list.Any((CharacterObject x) => x.IsPlayerCharacter))
		{
			list.Remove(CharacterObject.PlayerCharacter);
			if (includePlayer)
			{
				troopRoster.AddToCounts(CharacterObject.PlayerCharacter, 1);
				maxTroopCount--;
			}
		}
		List<CharacterObject> list2 = list.Where((CharacterObject x) => x.IsNotTransferableInPartyScreen && x.IsHero).ToList();
		int num = TaleWorlds.Library.MathF.Min(list2.Count, maxTroopCount);
		for (int i = 0; i < num; i++)
		{
			troopRoster.AddToCounts(list2[i], 1);
			list.Remove(list2[i]);
		}
		int count = list.Count;
		for (int j = num; j < maxTroopCount && j < count; j++)
		{
			troopRoster.AddToCounts(list[j], 1);
		}
		return troopRoster;
	}

	public static int GetMaximumXpAmountPartyCanGet(MobileParty party)
	{
		TroopRoster memberRoster = party.MemberRoster;
		int num = 0;
		for (int i = 0; i < memberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = memberRoster.GetElementCopyAtIndex(i);
			if (CanTroopGainXp(party.Party, elementCopyAtIndex.Character, out var gainableMaxXp))
			{
				num += gainableMaxXp;
			}
		}
		return num;
	}

	public static void PartyAddSharedXp(MobileParty party, float xpToDistribute)
	{
		if (!(xpToDistribute > 0f))
		{
			return;
		}
		TroopRoster memberRoster = party.MemberRoster;
		int num = 0;
		for (int i = 0; i < memberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = memberRoster.GetElementCopyAtIndex(i);
			if (CanTroopGainXp(party.Party, elementCopyAtIndex.Character, out var gainableMaxXp))
			{
				num += gainableMaxXp;
			}
		}
		for (int j = 0; j < memberRoster.Count; j++)
		{
			if (!(xpToDistribute >= 1f))
			{
				break;
			}
			if (num <= 0)
			{
				break;
			}
			TroopRosterElement elementCopyAtIndex2 = memberRoster.GetElementCopyAtIndex(j);
			if (CanTroopGainXp(party.Party, elementCopyAtIndex2.Character, out var gainableMaxXp2))
			{
				int num2 = TaleWorlds.Library.MathF.Floor(TaleWorlds.Library.MathF.Max(1f, xpToDistribute * (float)gainableMaxXp2 / (float)num));
				memberRoster.AddXpToTroopAtIndex(num2, j);
				xpToDistribute -= (float)num2;
				num -= gainableMaxXp2;
			}
		}
	}

	public static bool CanTroopGainXp(PartyBase owner, CharacterObject character, out int gainableMaxXp)
	{
		bool result = false;
		gainableMaxXp = 0;
		int index = owner.MemberRoster.FindIndexOfTroop(character);
		int elementNumber = owner.MemberRoster.GetElementNumber(index);
		int elementXp = owner.MemberRoster.GetElementXp(index);
		for (int i = 0; i < character.UpgradeTargets.Length; i++)
		{
			int upgradeXpCost = character.GetUpgradeXpCost(owner, i);
			if (elementXp < upgradeXpCost * elementNumber)
			{
				result = true;
				int num = upgradeXpCost * elementNumber - elementXp;
				if (num > gainableMaxXp)
				{
					gainableMaxXp = num;
				}
			}
		}
		return result;
	}

	public static Vec2 FindReachablePointAroundPosition(Vec2 centerPosition, float maxDistance, float minDistance = 0f)
	{
		Vec2 vec = new Vec2(centerPosition.x, centerPosition.y);
		PathFaceRecord faceIndex = Campaign.Current.MapSceneWrapper.GetFaceIndex(centerPosition);
		Vec2 vec2 = centerPosition;
		if (maxDistance > 0f)
		{
			int num = 0;
			do
			{
				num++;
				Vec2 vec3 = Vec2.One.Normalized();
				vec3.RotateCCW(MBRandom.RandomFloatRanged(0f, System.MathF.PI * 2f));
				vec3 *= MBRandom.RandomFloatRanged(minDistance, maxDistance);
				vec = centerPosition + vec3;
				PathFaceRecord faceIndex2 = Campaign.Current.MapSceneWrapper.GetFaceIndex(vec);
				if (faceIndex2.IsValid() && Campaign.Current.MapSceneWrapper.AreFacesOnSameIsland(faceIndex2, faceIndex, ignoreDisabled: false))
				{
					vec2 = vec;
				}
			}
			while (vec2 == centerPosition && num < 250);
		}
		return vec2;
	}

	public static void TryMatchPartySpeedWithItemWeight(MobileParty party, float targetPartySpeed, ItemObject itemToUse = null)
	{
		targetPartySpeed = TaleWorlds.Library.MathF.Max(1f, targetPartySpeed);
		ItemObject item = itemToUse ?? DefaultItems.HardWood;
		float speed = party.Speed;
		int num = TaleWorlds.Library.MathF.Sign(speed - targetPartySpeed);
		for (int i = 0; i < 200; i++)
		{
			if (TaleWorlds.Library.MathF.Abs(speed - targetPartySpeed) < 0.1f)
			{
				break;
			}
			if (TaleWorlds.Library.MathF.Sign(speed - targetPartySpeed) != num)
			{
				break;
			}
			if (speed >= targetPartySpeed)
			{
				party.ItemRoster.AddToCounts(item, 1);
			}
			else
			{
				if (party.ItemRoster.GetItemNumber(item) <= 0)
				{
					break;
				}
				party.ItemRoster.AddToCounts(item, -1);
			}
			speed = party.Speed;
		}
	}

	public static void UtilizePartyEscortBehavior(MobileParty escortedParty, MobileParty escortParty, ref bool isWaitingForEscortParty, float innerRadius, float outerRadius, ResumePartyEscortBehaviorDelegate onPartyEscortBehaviorResumed, bool showDebugSpheres = false)
	{
		if (!isWaitingForEscortParty)
		{
			if (escortParty.Position2D.DistanceSquared(escortedParty.Position2D) >= outerRadius * outerRadius)
			{
				escortedParty.Ai.SetMoveGoToPoint(escortedParty.Position2D);
				escortedParty.Ai.CheckPartyNeedsUpdate();
				isWaitingForEscortParty = true;
			}
		}
		else if (escortParty.Position2D.DistanceSquared(escortedParty.Position2D) <= innerRadius * innerRadius)
		{
			onPartyEscortBehaviorResumed();
			escortedParty.Ai.CheckPartyNeedsUpdate();
			isWaitingForEscortParty = false;
		}
	}

	public static Hero GetMainPartySkillCounsellor(SkillObject skill)
	{
		PartyBase mainParty = PartyBase.MainParty;
		Hero hero = null;
		int num = 0;
		for (int i = 0; i < mainParty.MemberRoster.Count; i++)
		{
			CharacterObject characterAtIndex = mainParty.MemberRoster.GetCharacterAtIndex(i);
			if (characterAtIndex.IsHero && !characterAtIndex.HeroObject.IsWounded)
			{
				int skillValue = characterAtIndex.GetSkillValue(skill);
				if (skillValue >= num)
				{
					num = skillValue;
					hero = characterAtIndex.HeroObject;
				}
			}
		}
		return hero ?? mainParty.LeaderHero;
	}

	public static Settlement GetCurrentSettlementOfMobilePartyForAICalculation(MobileParty mobileParty)
	{
		Settlement settlement = mobileParty.CurrentSettlement;
		if (settlement == null)
		{
			if (mobileParty.LastVisitedSettlement == null || !(mobileParty.LastVisitedSettlement.Position2D.DistanceSquared(mobileParty.Position2D) < 1f))
			{
				return null;
			}
			settlement = mobileParty.LastVisitedSettlement;
		}
		return settlement;
	}

	public static TroopRoster GetPlayerPrisonersPlayerCanSell()
	{
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		List<string> list = Campaign.Current.GetCampaignBehavior<IViewDataTracker>().GetPartyPrisonerLocks().ToList();
		foreach (TroopRosterElement item in MobileParty.MainParty.PrisonRoster.GetTroopRoster())
		{
			if (!list.Contains(item.Character.StringId))
			{
				troopRoster.Add(item);
			}
		}
		return troopRoster;
	}
}
