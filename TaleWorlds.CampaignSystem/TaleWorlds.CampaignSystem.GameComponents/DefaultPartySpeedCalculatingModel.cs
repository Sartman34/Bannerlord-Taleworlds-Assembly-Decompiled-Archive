using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultPartySpeedCalculatingModel : PartySpeedModel
{
	private static readonly TextObject _textCargo = new TextObject("{=fSGY71wd}Cargo within capacity");

	private static readonly TextObject _textOverburdened = new TextObject("{=xgO3cCgR}Overburdened");

	private static readonly TextObject _textOverPartySize = new TextObject("{=bO5gL3FI}Men within party size");

	private static readonly TextObject _textOverPrisonerSize = new TextObject("{=Ix8YjLPD}Men within prisoner size");

	private static readonly TextObject _textCavalry = new TextObject("{=YVGtcLHF}Cavalry");

	private static readonly TextObject _textCavalryWeatherPenalty = new TextObject("{=Cb0k9KM8}Cavalry weather penalty");

	private static readonly TextObject _textKhuzaitCavalryBonus = new TextObject("{=yi07dBks}Khuzait cavalry bonus");

	private static readonly TextObject _textMountedFootmen = new TextObject("{=5bSWSaPl}Footmen on horses");

	private static readonly TextObject _textMountedFootmenWeatherPenalty = new TextObject("{=JAKoFNgt}Footmen on horses weather penalty");

	private static readonly TextObject _textWounded = new TextObject("{=aLsVKIRy}Wounded members");

	private static readonly TextObject _textPrisoners = new TextObject("{=N6QTvjMf}Prisoners");

	private static readonly TextObject _textHerd = new TextObject("{=NhAMSaWU}Herding");

	private static readonly TextObject _textHighMorale = new TextObject("{=aDQcIGfH}High morale");

	private static readonly TextObject _textLowMorale = new TextObject("{=ydspCDIy}Low morale");

	private static readonly TextObject _textCaravan = new TextObject("{=vvabqi2w}Caravan");

	private static readonly TextObject _textDisorganized = new TextObject("{=JuwBb2Yg}Disorganized");

	private static readonly TextObject _movingInForest = new TextObject("{=rTFaZCdY}Forest");

	private static readonly TextObject _fordEffect = new TextObject("{=NT5fwUuJ}Fording");

	private static readonly TextObject _night = new TextObject("{=fAxjyMt5}Night");

	private static readonly TextObject _snow = new TextObject("{=vLjgcdgB}Snow");

	private static readonly TextObject _desert = new TextObject("{=ecUwABe2}Desert");

	private static readonly TextObject _sturgiaSnowBonus = new TextObject("{=0VfEGekD}Sturgia snow bonus");

	private static readonly TextObject _culture = GameTexts.FindText("str_culture");

	private const float MovingAtForestEffect = -0.3f;

	private const float MovingAtWaterEffect = -0.3f;

	private const float MovingAtNightEffect = -0.25f;

	private const float MovingOnSnowEffect = -0.1f;

	private const float MovingInDesertEffect = -0.1f;

	private const float CavalryEffect = 0.4f;

	private const float MountedFootMenEffect = 0.2f;

	private const float HerdEffect = -0.4f;

	private const float WoundedEffect = -0.05f;

	private const float CargoEffect = -0.02f;

	private const float OverburdenedEffect = -0.4f;

	private const float HighMoraleThresold = 70f;

	private const float LowMoraleThresold = 30f;

	private const float HighMoraleEffect = 0.05f;

	private const float LowMoraleEffect = -0.1f;

	private const float DisorganizedEffect = -0.4f;

	public override float BaseSpeed => 5f;

	public override float MinimumSpeed => 1f;

	public override ExplainedNumber CalculateBaseSpeed(MobileParty mobileParty, bool includeDescriptions = false, int additionalTroopOnFootCount = 0, int additionalTroopOnHorseCount = 0)
	{
		PartyBase party = mobileParty.Party;
		int numberOfAvailableMounts = 0;
		float totalWeightCarried = 0f;
		int herdSize = 0;
		int num = mobileParty.MemberRoster.TotalManCount + additionalTroopOnFootCount + additionalTroopOnHorseCount;
		AddCargoStats(mobileParty, ref numberOfAvailableMounts, ref totalWeightCarried, ref herdSize);
		float num2 = mobileParty.ItemRoster.TotalWeight;
		int num3 = (int)Campaign.Current.Models.InventoryCapacityModel.CalculateInventoryCapacity(mobileParty, includeDescriptions: false, additionalTroopOnFootCount, additionalTroopOnHorseCount).ResultNumber;
		int num4 = party.NumberOfMenWithHorse + additionalTroopOnHorseCount;
		int num5 = party.NumberOfMenWithoutHorse + additionalTroopOnFootCount;
		int num6 = party.MemberRoster.TotalWounded;
		int num7 = party.PrisonRoster.TotalManCount;
		float morale = mobileParty.Morale;
		if (mobileParty.AttachedParties.Count != 0)
		{
			foreach (MobileParty attachedParty in mobileParty.AttachedParties)
			{
				AddCargoStats(attachedParty, ref numberOfAvailableMounts, ref totalWeightCarried, ref herdSize);
				num += attachedParty.MemberRoster.TotalManCount;
				num2 += attachedParty.ItemRoster.TotalWeight;
				num3 += attachedParty.InventoryCapacity;
				num4 += attachedParty.Party.NumberOfMenWithHorse;
				num5 += attachedParty.Party.NumberOfMenWithoutHorse;
				num6 += attachedParty.MemberRoster.TotalWounded;
				num7 += attachedParty.PrisonRoster.TotalManCount;
			}
		}
		float baseNumber = CalculateBaseSpeedForParty(num);
		ExplainedNumber result = new ExplainedNumber(baseNumber, includeDescriptions);
		bool num8 = Campaign.Current.Models.MapWeatherModel.GetWeatherEffectOnTerrainForPosition(mobileParty.Position2D) == MapWeatherModel.WeatherEventEffectOnTerrain.Wet;
		GetFootmenPerkBonus(mobileParty, num, num5, ref result);
		float cavalryRatioModifier = GetCavalryRatioModifier(num, num4);
		int num9 = MathF.Min(num5, numberOfAvailableMounts);
		float mountedFootmenRatioModifier = GetMountedFootmenRatioModifier(num, num9);
		result.AddFactor(cavalryRatioModifier, _textCavalry);
		result.AddFactor(mountedFootmenRatioModifier, _textMountedFootmen);
		if (num8)
		{
			float num10 = cavalryRatioModifier * 0.3f;
			float num11 = mountedFootmenRatioModifier * 0.3f;
			result.AddFactor(0f - num10, _textCavalryWeatherPenalty);
			result.AddFactor(0f - num11, _textMountedFootmenWeatherPenalty);
		}
		if (mountedFootmenRatioModifier > 0f && mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Riding.NomadicTraditions))
		{
			result.AddFactor(mountedFootmenRatioModifier * DefaultPerks.Riding.NomadicTraditions.PrimaryBonus, DefaultPerks.Riding.NomadicTraditions.Name);
		}
		float num12 = MathF.Min(num2, (float)num3);
		if (num12 > 0f)
		{
			float cargoEffect = GetCargoEffect(num12, num3);
			result.AddFactor(cargoEffect, _textCargo);
		}
		if (totalWeightCarried > (float)num3)
		{
			float overBurdenedEffect = GetOverBurdenedEffect(totalWeightCarried - (float)num3, num3);
			result.AddFactor(overBurdenedEffect, _textOverburdened);
			if (mobileParty.HasPerk(DefaultPerks.Athletics.Energetic))
			{
				result.AddFactor(overBurdenedEffect * DefaultPerks.Athletics.Energetic.PrimaryBonus, DefaultPerks.Athletics.Energetic.Name);
			}
			if (mobileParty.HasPerk(DefaultPerks.Scouting.Unburdened))
			{
				result.AddFactor(overBurdenedEffect * DefaultPerks.Scouting.Unburdened.PrimaryBonus, DefaultPerks.Scouting.Unburdened.Name);
			}
		}
		if (mobileParty.HasPerk(DefaultPerks.Riding.SweepingWind, checkSecondaryRole: true))
		{
			result.AddFactor(DefaultPerks.Riding.SweepingWind.SecondaryBonus, DefaultPerks.Riding.SweepingWind.Name);
		}
		if (mobileParty.Party.NumberOfAllMembers > mobileParty.Party.PartySizeLimit)
		{
			float overPartySizeEffect = GetOverPartySizeEffect(mobileParty);
			result.AddFactor(overPartySizeEffect, _textOverPartySize);
		}
		herdSize += MathF.Max(0, numberOfAvailableMounts - num9);
		if (!mobileParty.IsVillager)
		{
			float herdingModifier = GetHerdingModifier(num, herdSize);
			result.AddFactor(herdingModifier, _textHerd);
			if (mobileParty.HasPerk(DefaultPerks.Riding.Shepherd))
			{
				result.AddFactor(herdingModifier * DefaultPerks.Riding.Shepherd.PrimaryBonus, DefaultPerks.Riding.Shepherd.Name);
			}
		}
		float woundedModifier = GetWoundedModifier(num, num6, mobileParty);
		result.AddFactor(woundedModifier, _textWounded);
		if (!mobileParty.IsCaravan)
		{
			if (mobileParty.Party.NumberOfPrisoners > mobileParty.Party.PrisonerSizeLimit)
			{
				float overPrisonerSizeEffect = GetOverPrisonerSizeEffect(mobileParty);
				result.AddFactor(overPrisonerSizeEffect, _textOverPrisonerSize);
			}
			float sizeModifierPrisoner = GetSizeModifierPrisoner(num, num7);
			result.AddFactor(1f / sizeModifierPrisoner - 1f, _textPrisoners);
		}
		if (morale > 70f)
		{
			result.AddFactor(0.05f * ((morale - 70f) / 30f), _textHighMorale);
		}
		if (morale < 30f)
		{
			result.AddFactor(-0.1f * (1f - mobileParty.Morale / 30f), _textLowMorale);
		}
		if (mobileParty == MobileParty.MainParty)
		{
			float playerMapMovementSpeedBonusMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerMapMovementSpeedBonusMultiplier();
			if (playerMapMovementSpeedBonusMultiplier > 0f)
			{
				result.AddFactor(playerMapMovementSpeedBonusMultiplier, GameTexts.FindText("str_game_difficulty"));
			}
		}
		if (mobileParty.IsCaravan)
		{
			result.AddFactor(0.1f, _textCaravan);
		}
		if (mobileParty.IsDisorganized)
		{
			result.AddFactor(-0.4f, _textDisorganized);
		}
		result.LimitMin(MinimumSpeed);
		return result;
	}

	private static void AddCargoStats(MobileParty mobileParty, ref int numberOfAvailableMounts, ref float totalWeightCarried, ref int herdSize)
	{
		ItemRoster itemRoster = mobileParty.ItemRoster;
		int numberOfPackAnimals = itemRoster.NumberOfPackAnimals;
		int numberOfLivestockAnimals = itemRoster.NumberOfLivestockAnimals;
		herdSize += numberOfPackAnimals + numberOfLivestockAnimals;
		numberOfAvailableMounts += itemRoster.NumberOfMounts;
		totalWeightCarried += itemRoster.TotalWeight;
	}

	private float CalculateBaseSpeedForParty(int menCount)
	{
		return BaseSpeed * MathF.Pow(200f / (200f + (float)menCount), 0.4f);
	}

	public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
	{
		if (mobileParty.IsCustomParty && !((CustomPartyComponent)mobileParty.PartyComponent).BaseSpeed.ApproximatelyEqualsTo(0f))
		{
			finalSpeed = new ExplainedNumber(((CustomPartyComponent)mobileParty.PartyComponent).BaseSpeed);
		}
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(mobileParty.CurrentNavigationFace);
		Hero effectiveScout = mobileParty.EffectiveScout;
		switch (faceTerrainType)
		{
		case TerrainType.Forest:
		{
			float num = 0f;
			if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.ForestKin))
			{
				for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
				{
					if (!mobileParty.MemberRoster.GetCharacterAtIndex(i).IsMounted)
					{
						num += (float)mobileParty.MemberRoster.GetElementNumber(i);
					}
				}
			}
			float value = ((num / (float)mobileParty.MemberRoster.TotalManCount >= 0.75f) ? (-0.3f * (0f - DefaultPerks.Scouting.ForestKin.PrimaryBonus)) : (-0.3f));
			finalSpeed.AddFactor(value, _movingInForest);
			if (PartyBaseHelper.HasFeat(mobileParty.Party, DefaultCulturalFeats.BattanianForestSpeedFeat))
			{
				float value2 = DefaultCulturalFeats.BattanianForestSpeedFeat.EffectBonus * 0.3f;
				finalSpeed.AddFactor(value2, _culture);
			}
			break;
		}
		case TerrainType.Water:
		case TerrainType.Bridge:
		case TerrainType.River:
		case TerrainType.Fording:
			finalSpeed.AddFactor(-0.3f, _fordEffect);
			break;
		case TerrainType.Desert:
		case TerrainType.Dune:
			if (!PartyBaseHelper.HasFeat(mobileParty.Party, DefaultCulturalFeats.AseraiDesertFeat))
			{
				finalSpeed.AddFactor(-0.1f, _desert);
			}
			if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.DesertBorn))
			{
				finalSpeed.AddFactor(DefaultPerks.Scouting.DesertBorn.PrimaryBonus, DefaultPerks.Scouting.DesertBorn.Name);
			}
			break;
		case TerrainType.Steppe:
		case TerrainType.Plain:
			if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.Pathfinder))
			{
				finalSpeed.AddFactor(DefaultPerks.Scouting.Pathfinder.PrimaryBonus, DefaultPerks.Scouting.Pathfinder.Name);
			}
			break;
		}
		MapWeatherModel.WeatherEvent weatherEventInPosition = Campaign.Current.Models.MapWeatherModel.GetWeatherEventInPosition(mobileParty.Position2D);
		if (weatherEventInPosition == MapWeatherModel.WeatherEvent.Snowy || weatherEventInPosition == MapWeatherModel.WeatherEvent.Blizzard)
		{
			faceTerrainType = TerrainType.Snow;
			finalSpeed.AddFactor(-0.1f, _snow);
		}
		if (Campaign.Current.IsNight)
		{
			finalSpeed.AddFactor(-0.25f, _night);
			if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.NightRunner))
			{
				finalSpeed.AddFactor(DefaultPerks.Scouting.NightRunner.PrimaryBonus, DefaultPerks.Scouting.NightRunner.Name);
			}
		}
		else if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.DayTraveler))
		{
			finalSpeed.AddFactor(DefaultPerks.Scouting.DayTraveler.PrimaryBonus, DefaultPerks.Scouting.DayTraveler.Name);
		}
		if (effectiveScout != null)
		{
			PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Scouting.UncannyInsight, effectiveScout.CharacterObject, DefaultSkills.Scouting, applyPrimaryBonus: true, ref finalSpeed, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
			if (effectiveScout.GetPerkValue(DefaultPerks.Scouting.ForcedMarch) && mobileParty.Morale > 75f)
			{
				finalSpeed.AddFactor(DefaultPerks.Scouting.ForcedMarch.PrimaryBonus, DefaultPerks.Scouting.ForcedMarch.Name);
			}
			if (mobileParty.DefaultBehavior == AiBehavior.EngageParty)
			{
				MobileParty targetParty = mobileParty.TargetParty;
				if (targetParty != null && targetParty.MapFaction.IsAtWarWith(mobileParty.MapFaction) && effectiveScout.GetPerkValue(DefaultPerks.Scouting.Tracker))
				{
					finalSpeed.AddFactor(DefaultPerks.Scouting.Tracker.SecondaryBonus, DefaultPerks.Scouting.Tracker.Name);
				}
			}
		}
		if (mobileParty.Army?.LeaderParty != null && mobileParty.Army.LeaderParty != mobileParty && mobileParty.AttachedTo != mobileParty.Army.LeaderParty && mobileParty.Army.LeaderParty.HasPerk(DefaultPerks.Tactics.CallToArms))
		{
			finalSpeed.AddFactor(DefaultPerks.Tactics.CallToArms.PrimaryBonus, DefaultPerks.Tactics.CallToArms.Name);
		}
		finalSpeed.LimitMin(MinimumSpeed);
		return finalSpeed;
	}

	private float GetCargoEffect(float weightCarried, int partyCapacity)
	{
		return -0.02f * weightCarried / (float)partyCapacity;
	}

	private float GetOverBurdenedEffect(float totalWeightCarried, int partyCapacity)
	{
		return -0.4f * (totalWeightCarried / (float)partyCapacity);
	}

	private float GetOverPartySizeEffect(MobileParty mobileParty)
	{
		int partySizeLimit = mobileParty.Party.PartySizeLimit;
		int numberOfAllMembers = mobileParty.Party.NumberOfAllMembers;
		return 1f / ((float)numberOfAllMembers / (float)partySizeLimit) - 1f;
	}

	private float GetOverPrisonerSizeEffect(MobileParty mobileParty)
	{
		int prisonerSizeLimit = mobileParty.Party.PrisonerSizeLimit;
		int numberOfPrisoners = mobileParty.Party.NumberOfPrisoners;
		return 1f / ((float)numberOfPrisoners / (float)prisonerSizeLimit) - 1f;
	}

	private float GetHerdingModifier(int totalMenCount, int herdSize)
	{
		herdSize -= totalMenCount;
		if (herdSize <= 0)
		{
			return 0f;
		}
		if (totalMenCount == 0)
		{
			return -0.8f;
		}
		return MathF.Max(-0.8f, -0.3f * ((float)herdSize / (float)totalMenCount));
	}

	private float GetWoundedModifier(int totalMenCount, int numWounded, MobileParty party)
	{
		if (numWounded <= totalMenCount / 4)
		{
			return 0f;
		}
		if (totalMenCount == 0)
		{
			return -0.5f;
		}
		float baseNumber = MathF.Max(-0.8f, -0.05f * (float)numWounded / (float)totalMenCount);
		ExplainedNumber stat = new ExplainedNumber(baseNumber);
		PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.Sledges, party, isPrimaryBonus: true, ref stat);
		return stat.ResultNumber;
	}

	private float GetCavalryRatioModifier(int totalMenCount, int totalCavalryCount)
	{
		if (totalMenCount == 0 || totalCavalryCount == 0)
		{
			return 0f;
		}
		return 0.4f * (float)totalCavalryCount / (float)totalMenCount;
	}

	private float GetMountedFootmenRatioModifier(int totalMenCount, int totalMountedFootmenCount)
	{
		if (totalMenCount == 0 || totalMountedFootmenCount == 0)
		{
			return 0f;
		}
		return 0.2f * (float)totalMountedFootmenCount / (float)totalMenCount;
	}

	private void GetFootmenPerkBonus(MobileParty party, int totalMenCount, int totalFootmenCount, ref ExplainedNumber result)
	{
		if (totalMenCount != 0)
		{
			float num = (float)totalFootmenCount / (float)totalMenCount;
			if (party.HasPerk(DefaultPerks.Athletics.Strong, checkSecondaryRole: true) && !num.ApproximatelyEqualsTo(0f))
			{
				result.AddFactor(num * DefaultPerks.Athletics.Strong.SecondaryBonus, DefaultPerks.Athletics.Strong.Name);
			}
		}
	}

	private static float GetSizeModifierWounded(int totalMenCount, int totalWoundedMenCount)
	{
		return MathF.Pow((10f + (float)totalMenCount) / (10f + (float)totalMenCount - (float)totalWoundedMenCount), 0.33f);
	}

	private static float GetSizeModifierPrisoner(int totalMenCount, int totalPrisonerCount)
	{
		return MathF.Pow((10f + (float)totalMenCount + (float)totalPrisonerCount) / (10f + (float)totalMenCount), 0.33f);
	}
}
