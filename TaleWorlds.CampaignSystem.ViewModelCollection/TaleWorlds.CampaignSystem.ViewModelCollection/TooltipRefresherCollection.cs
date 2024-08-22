using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Information.RundownTooltip;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection;

public static class TooltipRefresherCollection
{
	private static readonly IEqualityComparer<(ItemCategory, int)> itemCategoryDistinctComparer = new CampaignUIHelper.ProductInputOutputEqualityComparer();

	private static string ExtendKeyId = "ExtendModifier";

	private static string FollowModifierKeyId = "FollowModifier";

	private static string MapClickKeyId = "MapClick";

	public static void RefreshExplainedNumberTooltip(RundownTooltipVM explainedNumberTooltip, object[] args)
	{
		explainedNumberTooltip.IsActive = explainedNumberTooltip.IsInitializedProperly;
		if (!explainedNumberTooltip.IsActive)
		{
			return;
		}
		Func<ExplainedNumber> func = args[0] as Func<ExplainedNumber>;
		Func<ExplainedNumber> func2 = args[1] as Func<ExplainedNumber>;
		explainedNumberTooltip.Lines.Clear();
		Func<ExplainedNumber> func3 = ((explainedNumberTooltip.IsExtended && func2 != null) ? func2 : func);
		if (func3 == null)
		{
			return;
		}
		ExplainedNumber explainedNumber = func3();
		explainedNumberTooltip.CurrentExpectedChange = explainedNumber.ResultNumber;
		foreach (var line in explainedNumber.GetLines())
		{
			explainedNumberTooltip.Lines.Add(new RundownLineVM(line.name, line.number));
		}
	}

	public static void RefreshTrackTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		Track track = args[0] as Track;
		propertyBasedTooltipVM.Mode = 1;
		MapTrackModel mapTrackModel = Campaign.Current.Models.MapTrackModel;
		if (mapTrackModel == null)
		{
			return;
		}
		TextObject textObject = mapTrackModel.TrackTitle(track);
		propertyBasedTooltipVM.AddProperty("", textObject.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		foreach (var item in mapTrackModel.GetTrackDescription(track))
		{
			propertyBasedTooltipVM.AddProperty(item.Item1?.ToString(), item.Item2);
		}
	}

	public static void RefreshHeroTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		Hero hero = args[0] as Hero;
		bool flag = (bool)args[1];
		StringHelpers.SetCharacterProperties("NPC", hero.CharacterObject);
		TextObject disableReason;
		bool num = CampaignUIHelper.IsHeroInformationHidden(hero, out disableReason);
		if (hero.IsEnemy(Hero.MainHero))
		{
			propertyBasedTooltipVM.Mode = 3;
		}
		else if (hero == Hero.MainHero || hero.IsFriend(Hero.MainHero))
		{
			propertyBasedTooltipVM.Mode = 2;
		}
		else
		{
			propertyBasedTooltipVM.Mode = 1;
		}
		propertyBasedTooltipVM.AddProperty("", hero.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		if (!hero.IsNotable && !hero.IsWanderer)
		{
			if (hero.Clan?.Kingdom != null)
			{
				propertyBasedTooltipVM.AddProperty("", CampaignUIHelper.GetHeroKingdomRank(hero));
			}
			if (Game.Current.IsDevelopmentMode && hero.Clan?.Leader == hero)
			{
				propertyBasedTooltipVM.AddProperty("DEBUG Clan Leader", "");
			}
			if (Game.Current.IsDevelopmentMode && hero.Clan?.Kingdom != null)
			{
				if (hero == hero.MapFaction?.Leader && hero.Clan?.Kingdom != null)
				{
					propertyBasedTooltipVM.AddProperty("DEBUG Kingdom Gold", hero.Clan.Kingdom.KingdomBudgetWallet.ToString());
				}
				propertyBasedTooltipVM.AddProperty("DEBUG Gold", hero.Gold.ToString());
				if (Game.Current.IsDevelopmentMode && hero.Clan != null && hero.Clan.IsUnderMercenaryService)
				{
					propertyBasedTooltipVM.AddProperty("DEBUG Mercenary Award", hero.Clan.MercenaryAwardMultiplier.ToString());
				}
				if (Game.Current.IsDevelopmentMode && hero.Clan?.Leader == hero)
				{
					propertyBasedTooltipVM.AddProperty("DEBUG Debt To Kingdom", hero.Clan.DebtToKingdom.ToString());
				}
			}
			if (Game.Current.IsDevelopmentMode && hero.PartyBelongedTo != null && !hero.IsSpecial)
			{
				propertyBasedTooltipVM.AddProperty("DEBUG Party Size", hero.PartyBelongedTo.MemberRoster.TotalManCount + "/" + hero.PartyBelongedTo.Party.PartySizeLimit);
				propertyBasedTooltipVM.AddProperty("DEBUG Party Position", (int)hero.PartyBelongedTo.Position2D.X + "," + (int)hero.PartyBelongedTo.Position2D.Y);
				propertyBasedTooltipVM.AddProperty("DEBUG Party Wage", hero.PartyBelongedTo.TotalWage.ToString());
			}
			if (Game.Current.IsDevelopmentMode && hero.PartyBelongedTo != null)
			{
				propertyBasedTooltipVM.AddProperty("DEBUG Party Morale", hero.PartyBelongedTo.Morale.ToString());
			}
			if (Game.Current.IsDevelopmentMode && hero.PartyBelongedTo != null)
			{
				propertyBasedTooltipVM.AddProperty("DEBUG Starving", hero.PartyBelongedTo.Party.IsStarving.ToString());
			}
			if (Game.Current.IsDevelopmentMode && hero.MapFaction?.Leader != null && hero != hero.MapFaction.Leader)
			{
				propertyBasedTooltipVM.AddProperty("DEBUG King Relation", hero.GetRelation(hero.MapFaction.Leader).ToString());
			}
			if (Game.Current.IsDevelopmentMode && hero.PartyBelongedToAsPrisoner != null)
			{
				propertyBasedTooltipVM.AddProperty("DEBUG Prisoner at", hero.PartyBelongedToAsPrisoner.Name.ToString());
			}
		}
		if (hero.Clan != null)
		{
			propertyBasedTooltipVM.AddProperty("", hero.Clan.Name.ToString());
		}
		propertyBasedTooltipVM.AddProperty("", "", -1);
		if (!num)
		{
			int num2 = 0;
			foreach (Settlement item in Settlement.All)
			{
				if (item.IsTown)
				{
					Town town = item.Town;
					for (int i = 0; i < town.Workshops.Length; i++)
					{
						if (town.Workshops[i].Owner == hero && !town.Workshops[i].WorkshopType.IsHidden)
						{
							if (num2 == 0)
							{
								MBTextManager.SetTextVariable("STR1", new TextObject("{=VZjxs5Dt}Owner of "));
								MBTextManager.SetTextVariable("STR2", town.Workshops[i].WorkshopType.Name);
								string text = GameTexts.FindText("str_STR1_STR2").ToString();
								MBTextManager.SetTextVariable("LEFT", text);
								MBTextManager.SetTextVariable("PROPERTIES", text);
							}
							else
							{
								MBTextManager.SetTextVariable("RIGHT", town.Workshops[i].WorkshopType.Name);
								string text2 = GameTexts.FindText("str_LEFT_comma_RIGHT").ToString();
								MBTextManager.SetTextVariable("LEFT", text2);
								MBTextManager.SetTextVariable("PROPERTIES", text2);
							}
							num2++;
						}
					}
				}
				if (!item.IsTown && !item.IsVillage)
				{
					continue;
				}
				foreach (Alley alley in item.Alleys)
				{
					if (alley.Owner == hero)
					{
						if (num2 == 0)
						{
							MBTextManager.SetTextVariable("STR1", new TextObject("{=VZjxs5Dt}Owner of "));
							MBTextManager.SetTextVariable("STR2", alley.Name);
							string text3 = GameTexts.FindText("str_STR1_STR2").ToString();
							MBTextManager.SetTextVariable("STR1", text3);
							MBTextManager.SetTextVariable("NUMBER_OF_MEN", Campaign.Current.Models.AlleyModel.GetTroopsOfAIOwnedAlley(alley).TotalManCount);
							MBTextManager.SetTextVariable("STR2", GameTexts.FindText("str_men_count_in_paranthesis_wo_wounded"));
							text3 = GameTexts.FindText("str_STR1_STR2").ToString();
							MBTextManager.SetTextVariable("LEFT", text3);
							MBTextManager.SetTextVariable("PROPERTIES", text3);
						}
						else
						{
							MBTextManager.SetTextVariable("STR1", alley.Name);
							MBTextManager.SetTextVariable("NUMBER_OF_MEN", Campaign.Current.Models.AlleyModel.GetTroopsOfAIOwnedAlley(alley).TotalManCount);
							MBTextManager.SetTextVariable("STR2", GameTexts.FindText("str_men_count_in_paranthesis_wo_wounded"));
							MBTextManager.SetTextVariable("RIGHT", GameTexts.FindText("str_STR1_STR2").ToString());
							string text4 = GameTexts.FindText("str_LEFT_comma_RIGHT").ToString();
							MBTextManager.SetTextVariable("LEFT", text4);
							MBTextManager.SetTextVariable("PROPERTIES", text4);
						}
						num2++;
					}
				}
			}
			string value = new TextObject("{=j8uZBakZ}{PROPERTIES}").ToString();
			if (num2 > 0)
			{
				propertyBasedTooltipVM.AddProperty("", value, 0, TooltipProperty.TooltipPropertyFlags.MultiLine);
			}
			int num3 = 0;
			TextObject textObject = new TextObject("{=C2qpwFq5}Owner of {SETTLEMENTS}");
			foreach (Settlement item2 in Settlement.All)
			{
				if (item2.IsFortification && item2.OwnerClan != null && item2.OwnerClan.Leader == hero)
				{
					if (num3 == 0)
					{
						MBTextManager.SetTextVariable("SETTLEMENTS", item2.Name);
					}
					else
					{
						MBTextManager.SetTextVariable("RIGHT", item2.Name.ToString());
						MBTextManager.SetTextVariable("LEFT", new TextObject("{=!}{SETTLEMENTS}").ToString());
						MBTextManager.SetTextVariable("SETTLEMENTS", GameTexts.FindText("str_LEFT_comma_RIGHT").ToString());
					}
					num3++;
				}
			}
			if (num3 > 0)
			{
				propertyBasedTooltipVM.AddProperty("", textObject.ToString(), 0, TooltipProperty.TooltipPropertyFlags.MultiLine);
			}
			if (hero.OwnedCaravans.Count > 0)
			{
				TextObject textObject2 = new TextObject("{=TEkWkzbH}Owned Caravans: {CARAVAN_COUNT}");
				textObject2.SetTextVariable("CARAVAN_COUNT", hero.OwnedCaravans.Count);
				propertyBasedTooltipVM.AddProperty("", textObject2.ToString());
			}
			if (hero.GovernorOf != null)
			{
				MBTextManager.SetTextVariable("STR1", new TextObject("{=jQdBl4hf}Governor of "));
				MBTextManager.SetTextVariable("STR2", hero.GovernorOf.Name);
				TextObject textObject3 = GameTexts.FindText("str_STR1_STR2");
				propertyBasedTooltipVM.AddProperty("", textObject3.ToString);
			}
			if (hero != Hero.MainHero)
			{
				MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_relation"));
				string definition = GameTexts.FindText("str_LEFT_ONLY").ToString();
				propertyBasedTooltipVM.AddProperty(definition, ((int)hero.GetRelationWithPlayer()).ToString());
			}
		}
		if (hero.HomeSettlement != null)
		{
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_home"));
			string definition2 = GameTexts.FindText("str_LEFT_ONLY").ToString();
			propertyBasedTooltipVM.AddProperty(definition2, hero.HomeSettlement.Name.ToString());
		}
		if (hero.IsNotable)
		{
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_notable_power"));
			string definition3 = GameTexts.FindText("str_LEFT_colon").ToString();
			MBTextManager.SetTextVariable("RANK", Campaign.Current.Models.NotablePowerModel.GetPowerRankName(hero).ToString());
			MBTextManager.SetTextVariable("NUMBER", ((int)hero.Power).ToString());
			string value2 = GameTexts.FindText("str_RANK_with_NUM_between_parenthesis").ToString();
			propertyBasedTooltipVM.AddProperty(definition3, value2);
			if (Game.Current.IsDevelopmentMode)
			{
				propertyBasedTooltipVM.AddProperty("", "");
				ExplainedNumber explainedNumber = Campaign.Current.Models.NotablePowerModel.CalculateDailyPowerChangeForHero(hero, includeDescriptions: true);
				propertyBasedTooltipVM.AddProperty("[DEV] Daily Power Change", explainedNumber.ResultNumber.ToString("+0.##;-0.##;0"), 0, TooltipProperty.TooltipPropertyFlags.RundownResult);
				propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
				foreach (var (text5, num4) in explainedNumber.GetLines())
				{
					propertyBasedTooltipVM.AddProperty("[DEV] " + text5, num4.ToString("+0.##;-0.##;0"));
				}
				propertyBasedTooltipVM.AddProperty("", "");
			}
		}
		MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type"));
		string definition4 = GameTexts.FindText("str_LEFT_ONLY").ToString();
		propertyBasedTooltipVM.AddProperty(definition4, HeroHelper.GetCharacterTypeName(hero).ToString());
		if (hero.CurrentSettlement != null && LocationComplex.Current != null && hero.CurrentSettlement == Hero.MainHero.CurrentSettlement && LocationComplex.Current.GetLocationOfCharacter(hero) != null)
		{
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_location"));
			string definition5 = GameTexts.FindText("str_LEFT_ONLY").ToString();
			propertyBasedTooltipVM.AddProperty(definition5, LocationComplex.Current.GetLocationOfCharacter(hero).DoorName.ToString());
		}
		if (hero.CurrentSettlement != null && hero.IsNotable && hero.SupporterOf != null)
		{
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_supporter_of"));
			string definition6 = GameTexts.FindText("str_LEFT_ONLY").ToString();
			propertyBasedTooltipVM.AddProperty(definition6, hero.SupporterOf.Name.ToString());
		}
		if (flag)
		{
			List<(CampaignUIHelper.IssueQuestFlags, TextObject, TextObject)> questStateOfHero = CampaignUIHelper.GetQuestStateOfHero(hero);
			for (int j = 0; j < questStateOfHero.Count; j++)
			{
				string questExplanationOfHero = CampaignUIHelper.GetQuestExplanationOfHero(questStateOfHero[j].Item1);
				if (!string.IsNullOrEmpty(questExplanationOfHero))
				{
					propertyBasedTooltipVM.AddProperty("", "", -1);
					propertyBasedTooltipVM.AddProperty(questExplanationOfHero, questStateOfHero[j].Item2.ToString());
				}
			}
		}
		if (!hero.IsAlive)
		{
			propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_dead").ToString());
		}
	}

	public static void RefreshInventoryTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		InventoryLogic inventoryLogic = args[0] as InventoryLogic;
		propertyBasedTooltipVM.Mode = 0;
		List<(ItemRosterElement, int)> soldItems = inventoryLogic.GetSoldItems();
		List<(ItemRosterElement, int)> boughtItems = inventoryLogic.GetBoughtItems();
		TextObject textObject = new TextObject("{=bPFjmYCI}{SHOP_NAME} x {SHOP_DIFFERENCE_COUNT}");
		TextObject textObject2 = new TextObject("{=lxwGbRwu}x {SHOP_DIFFERENCE_COUNT}");
		TextObject textObject3 = (inventoryLogic.IsTrading ? textObject : textObject2);
		int num = 0;
		int num2 = 40;
		foreach (var item2 in soldItems)
		{
			if (num == num2)
			{
				break;
			}
			ItemRosterElement itemRosterElement;
			(itemRosterElement, _) = item2;
			textObject3.SetTextVariable("SHOP_NAME", itemRosterElement.EquipmentElement.GetModifiedItemName());
			(itemRosterElement, _) = item2;
			textObject3.SetTextVariable("SHOP_DIFFERENCE_COUNT", itemRosterElement.Amount);
			if (inventoryLogic.IsTrading)
			{
				string definition = textObject3.ToString();
				int item = item2.Item2;
				propertyBasedTooltipVM.AddColoredProperty(definition, "+" + item, UIColors.PositiveIndicator);
			}
			else
			{
				(itemRosterElement, _) = item2;
				propertyBasedTooltipVM.AddColoredProperty(itemRosterElement.EquipmentElement.GetModifiedItemName().ToString(), textObject3.ToString(), UIColors.NegativeIndicator);
			}
			num++;
		}
		foreach (var item3 in boughtItems)
		{
			if (num == num2)
			{
				break;
			}
			ItemRosterElement itemRosterElement;
			(itemRosterElement, _) = item3;
			textObject3.SetTextVariable("SHOP_NAME", itemRosterElement.EquipmentElement.GetModifiedItemName());
			(itemRosterElement, _) = item3;
			textObject3.SetTextVariable("SHOP_DIFFERENCE_COUNT", itemRosterElement.Amount);
			if (inventoryLogic.IsTrading)
			{
				propertyBasedTooltipVM.AddColoredProperty(textObject3.ToString(), (-item3.Item2).ToString(), UIColors.NegativeIndicator);
			}
			else
			{
				(itemRosterElement, _) = item3;
				propertyBasedTooltipVM.AddColoredProperty(itemRosterElement.EquipmentElement.GetModifiedItemName().ToString(), textObject3.ToString(), UIColors.PositiveIndicator);
			}
			num++;
		}
		if (num == num2)
		{
			int num3 = soldItems.Count + boughtItems.Count - num;
			if (num3 > 0)
			{
				TextObject textObject4 = new TextObject("{=OpsiBFCu}... and {COUNT} more items.");
				textObject4.SetTextVariable("COUNT", num3);
				propertyBasedTooltipVM.AddProperty("", textObject4.ToString());
			}
		}
	}

	public static void RefreshCraftingPartTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		WeaponDesignElement weaponDesignElement = args[0] as WeaponDesignElement;
		propertyBasedTooltipVM.Mode = 0;
		propertyBasedTooltipVM.AddProperty("", weaponDesignElement.CraftingPiece.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		TextObject textObject = GameTexts.FindText("str_crafting_piece_type", weaponDesignElement.CraftingPiece.PieceType.ToString());
		propertyBasedTooltipVM.AddProperty("", textObject.ToString());
		propertyBasedTooltipVM.AddProperty(new TextObject("{=Oo3fkeab}Difficulty: ").ToString(), Campaign.Current.Models.SmithingModel.GetCraftingPartDifficulty(weaponDesignElement.CraftingPiece).ToString());
		propertyBasedTooltipVM.AddProperty(new TextObject("{=XUtiwiYP}Length: ").ToString(), TaleWorlds.Library.MathF.Round(weaponDesignElement.CraftingPiece.Length * 100f, 2).ToString());
		propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_weight_text").ToString(), TaleWorlds.Library.MathF.Round(weaponDesignElement.CraftingPiece.Weight, 2).ToString());
		if (weaponDesignElement.CraftingPiece.PieceType == CraftingPiece.PieceTypes.Blade)
		{
			if (weaponDesignElement.CraftingPiece.BladeData.SwingDamageType != DamageTypes.Invalid)
			{
				DamageTypes swingDamageType = weaponDesignElement.CraftingPiece.BladeData.SwingDamageType;
				MBTextManager.SetTextVariable("SWING_DAMAGE_FACTOR", TaleWorlds.Library.MathF.Round(weaponDesignElement.CraftingPiece.BladeData.SwingDamageFactor, 2) + " " + GameTexts.FindText("str_damage_types", swingDamageType.ToString()).ToString()[0].ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=nYYUQQm0}Swing Damage Factor ").ToString(), new TextObject("{=aTdrjrEh}{SWING_DAMAGE_FACTOR}").ToString());
			}
			if (weaponDesignElement.CraftingPiece.BladeData.ThrustDamageType != DamageTypes.Invalid)
			{
				DamageTypes thrustDamageType = weaponDesignElement.CraftingPiece.BladeData.ThrustDamageType;
				MBTextManager.SetTextVariable("THRUST_DAMAGE_FACTOR", TaleWorlds.Library.MathF.Round(weaponDesignElement.CraftingPiece.BladeData.ThrustDamageFactor, 2) + " " + GameTexts.FindText("str_damage_types", thrustDamageType.ToString()).ToString()[0].ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=KTKBKmvp}Thrust Damage Factor ").ToString(), new TextObject("{=DNq9bdvV}{THRUST_DAMAGE_FACTOR}").ToString());
			}
		}
		if (weaponDesignElement.CraftingPiece.ArmorBonus != 0)
		{
			propertyBasedTooltipVM.AddModifierProperty(new TextObject("{=7Xynf4IA}Hand Armor").ToString(), weaponDesignElement.CraftingPiece.ArmorBonus);
		}
		if (weaponDesignElement.CraftingPiece.SwingDamageBonus != 0)
		{
			propertyBasedTooltipVM.AddModifierProperty(new TextObject("{=QeToaiLt}Swing Damage").ToString(), weaponDesignElement.CraftingPiece.SwingDamageBonus);
		}
		if (weaponDesignElement.CraftingPiece.SwingSpeedBonus != 0)
		{
			propertyBasedTooltipVM.AddModifierProperty(new TextObject("{=sVZaIPoQ}Swing Speed").ToString(), weaponDesignElement.CraftingPiece.SwingSpeedBonus);
		}
		if (weaponDesignElement.CraftingPiece.ThrustDamageBonus != 0)
		{
			propertyBasedTooltipVM.AddModifierProperty(new TextObject("{=dO95yR9b}Thrust Damage").ToString(), weaponDesignElement.CraftingPiece.ThrustDamageBonus);
		}
		if (weaponDesignElement.CraftingPiece.ThrustSpeedBonus != 0)
		{
			propertyBasedTooltipVM.AddModifierProperty(new TextObject("{=4uMWNDoi}Thrust Speed").ToString(), weaponDesignElement.CraftingPiece.ThrustSpeedBonus);
		}
		if (weaponDesignElement.CraftingPiece.HandlingBonus != 0)
		{
			propertyBasedTooltipVM.AddModifierProperty(new TextObject("{=oibdTnXP}Handling").ToString(), weaponDesignElement.CraftingPiece.HandlingBonus);
		}
		if (weaponDesignElement.CraftingPiece.AccuracyBonus != 0)
		{
			propertyBasedTooltipVM.AddModifierProperty(new TextObject("{=TAnabTdy}Accuracy").ToString(), weaponDesignElement.CraftingPiece.AccuracyBonus);
		}
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		propertyBasedTooltipVM.AddProperty(new TextObject("{=hr4MuPnt}Required Materials").ToString(), " ");
		propertyBasedTooltipVM.AddProperty("", string.Empty, -1, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
		foreach (var item2 in weaponDesignElement.CraftingPiece.MaterialsUsed)
		{
			ItemObject craftingMaterialItem = Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem(item2.Item1);
			if (craftingMaterialItem != null)
			{
				string definition = craftingMaterialItem.Name.ToString();
				int item = item2.Item2;
				propertyBasedTooltipVM.AddProperty(definition, item.ToString());
			}
		}
	}

	public static void RefreshCharacterTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		CharacterObject characterObject = args[0] as CharacterObject;
		propertyBasedTooltipVM.Mode = 1;
		propertyBasedTooltipVM.AddProperty("", characterObject.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		TextObject textObject = GameTexts.FindText("str_party_troop_tier");
		textObject.SetTextVariable("TIER_LEVEL", characterObject.Tier);
		propertyBasedTooltipVM.AddProperty("", textObject.ToString());
		if (characterObject.UpgradeTargets.Length != 0)
		{
			GameTexts.SetVariable("XP_AMOUNT", characterObject.GetUpgradeXpCost(PartyBase.MainParty, 0));
			propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_required_xp_to_upgrade").ToString());
		}
		if (characterObject.TroopWage > 0)
		{
			GameTexts.SetVariable("LEFT", GameTexts.FindText("str_wage"));
			GameTexts.SetVariable("STR1", characterObject.TroopWage);
			GameTexts.SetVariable("STR2", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
			GameTexts.SetVariable("RIGHT", GameTexts.FindText("str_STR1_space_STR2"));
			propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_LEFT_colon_RIGHT_wSpaceAfterColon").ToString());
		}
		propertyBasedTooltipVM.AddProperty("", "");
		propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_skills").ToString());
		propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
		foreach (SkillObject item in Skills.All)
		{
			if (characterObject.GetSkillValue(item) > 0)
			{
				propertyBasedTooltipVM.AddProperty(item.Name.ToString(), characterObject.GetSkillValue(item).ToString());
			}
		}
	}

	public static void RefreshItemTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		EquipmentElement? equipmentElement = args[0] as EquipmentElement?;
		ItemObject item = equipmentElement.Value.Item;
		propertyBasedTooltipVM.Mode = 1;
		propertyBasedTooltipVM.AddProperty("", item.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		propertyBasedTooltipVM.AddProperty(new TextObject("{=zMMqgxb1}Type").ToString(), GameTexts.FindText("str_inventory_type_" + (int)item.Type).ToString());
		propertyBasedTooltipVM.AddProperty(" ", " ");
		if (Game.Current.IsDevelopmentMode)
		{
			if (item.Culture != null)
			{
				propertyBasedTooltipVM.AddProperty("Culture: ", item.Culture.StringId);
			}
			else
			{
				propertyBasedTooltipVM.AddProperty("Culture: ", "No Culture");
			}
			propertyBasedTooltipVM.AddProperty("ID: ", item.StringId);
		}
		if (item.RelevantSkill != null && item.Difficulty > 0)
		{
			propertyBasedTooltipVM.AddProperty(new TextObject("{=dWYm9GsC}Requires").ToString(), " ");
			propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
			propertyBasedTooltipVM.AddProperty(item.RelevantSkill.Name.ToString(), item.Difficulty.ToString());
			propertyBasedTooltipVM.AddProperty(" ", " ");
		}
		propertyBasedTooltipVM.AddProperty(new TextObject("{=4Dd2xgPm}Weight").ToString(), item.Weight.ToString());
		string text = "";
		if (item.IsUniqueItem)
		{
			text = text + GameTexts.FindText("str_inventory_flag_unique").ToString() + " ";
		}
		if (item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByFemale))
		{
			text = text + GameTexts.FindText("str_inventory_flag_male_only").ToString() + " ";
		}
		if (item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByMale))
		{
			text = text + GameTexts.FindText("str_inventory_flag_female_only").ToString() + " ";
		}
		if (!string.IsNullOrEmpty(text))
		{
			propertyBasedTooltipVM.AddProperty(new TextObject("{=eHVq6yDa}Item Properties").ToString(), text);
		}
		if (item.HasArmorComponent)
		{
			if (Campaign.Current != null)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=US7UmBbt}Armor Tier").ToString(), ((int)(item.Tier + 1)).ToString());
			}
			if (item.ArmorComponent.HeadArmor != 0)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=O3dhjtOS}Head Armor").ToString(), equipmentElement.Value.GetModifiedHeadArmor().ToString());
			}
			if (item.ArmorComponent.BodyArmor != 0)
			{
				if (item.Type == ItemObject.ItemTypeEnum.HorseHarness)
				{
					propertyBasedTooltipVM.AddProperty(new TextObject("{=kftE5nvv}Horse Armor").ToString(), equipmentElement.Value.GetModifiedMountBodyArmor().ToString());
				}
				else
				{
					propertyBasedTooltipVM.AddProperty(new TextObject("{=HkfY3Ds5}Body Armor").ToString(), equipmentElement.Value.GetModifiedBodyArmor().ToString());
				}
			}
			if (item.ArmorComponent.ArmArmor != 0)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=kx7q8ybD}Arm Armor").ToString(), equipmentElement.Value.GetModifiedArmArmor().ToString());
			}
			if (item.ArmorComponent.LegArmor != 0)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=eIws123Z}Leg Armor").ToString(), equipmentElement.Value.GetModifiedLegArmor().ToString());
			}
		}
		else if (item.WeaponComponent != null && item.Weapons.Count > 0)
		{
			int num = ((item.Weapons.Count > 1 && propertyBasedTooltipVM.IsExtended) ? 1 : 0);
			WeaponComponentData weaponComponentData = item.Weapons[num];
			propertyBasedTooltipVM.AddProperty(new TextObject("{=sqdzHOPe}Class").ToString(), GameTexts.FindText("str_inventory_weapon", ((int)weaponComponentData.WeaponClass).ToString()).ToString());
			if (Campaign.Current != null)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=hn9TPqhK}Weapon Tier").ToString(), ((int)(item.Tier + 1)).ToString());
			}
			ItemObject.ItemTypeEnum itemTypeFromWeaponClass = WeaponComponentData.GetItemTypeFromWeaponClass(weaponComponentData.WeaponClass);
			if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.OneHandedWeapon || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.TwoHandedWeapon || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Polearm)
			{
				if (weaponComponentData.SwingDamageType != DamageTypes.Invalid)
				{
					propertyBasedTooltipVM.AddProperty(new TextObject("{=sVZaIPoQ}Swing Speed").ToString(), equipmentElement.Value.GetModifiedSwingSpeedForUsage(num).ToString());
					propertyBasedTooltipVM.AddProperty(new TextObject("{=QeToaiLt}Swing Damage").ToString(), equipmentElement.Value.GetModifiedSwingDamageForUsage(num).ToString());
				}
				if (weaponComponentData.ThrustDamageType != DamageTypes.Invalid)
				{
					propertyBasedTooltipVM.AddProperty(new TextObject("{=4uMWNDoi}Thrust Speed").ToString(), equipmentElement.Value.GetModifiedThrustSpeedForUsage(num).ToString());
					propertyBasedTooltipVM.AddProperty(new TextObject("{=dO95yR9b}Thrust Damage").ToString(), equipmentElement.Value.GetModifiedThrustDamageForUsage(num).ToString());
				}
				propertyBasedTooltipVM.AddProperty(new TextObject("{=ZcybPatO}Weapon Length").ToString(), weaponComponentData.WeaponLength.ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=oibdTnXP}Handling").ToString(), weaponComponentData.Handling.ToString());
			}
			if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Thrown)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=ZcybPatO}Weapon Length").ToString(), weaponComponentData.WeaponLength.ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=s31DnnAf}Damage").ToString(), ItemHelper.GetMissileDamageText(weaponComponentData, equipmentElement.Value.ItemModifier).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=bAqDnkaT}Missile Speed").ToString(), equipmentElement.Value.GetModifiedMissileSpeedForUsage(num).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=TAnabTdy}Accuracy").ToString(), weaponComponentData.Accuracy.ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=twtbH1zv}Stack Amount").ToString(), equipmentElement.Value.GetModifiedStackCountForUsage(num).ToString());
			}
			if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Shield)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=6GSXsdeX}Speed").ToString(), equipmentElement.Value.GetModifiedSwingSpeedForUsage(num).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=oBbiVeKE}Hit Points").ToString(), equipmentElement.Value.GetModifiedMaximumHitPointsForUsage(num).ToString());
			}
			if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Bow || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Crossbow)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=6GSXsdeX}Speed").ToString(), equipmentElement.Value.GetModifiedSwingSpeedForUsage(num).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=s31DnnAf}Damage").ToString(), ItemHelper.GetThrustDamageText(weaponComponentData, equipmentElement.Value.ItemModifier).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=TAnabTdy}Accuracy").ToString(), weaponComponentData.Accuracy.ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=bAqDnkaT}Missile Speed").ToString(), equipmentElement.Value.GetModifiedMissileSpeedForUsage(num).ToString());
				if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Crossbow)
				{
					propertyBasedTooltipVM.AddProperty(new TextObject("{=cnmRwV4s}Ammo Limit").ToString(), weaponComponentData.MaxDataValue.ToString());
				}
			}
			if (item != null && item.HasBannerComponent)
			{
				TextObject textObject2;
				if (item?.BannerComponent?.BannerEffect != null)
				{
					GameTexts.SetVariable("RANK", item.BannerComponent.BannerEffect.Name);
					string content = string.Empty;
					if (item.BannerComponent.BannerEffect.IncrementType == BannerEffect.EffectIncrementType.AddFactor)
					{
						TextObject textObject = GameTexts.FindText("str_NUMBER_percent");
						textObject.SetTextVariable("NUMBER", ((int)Math.Abs(item.BannerComponent.GetBannerEffectBonus() * 100f)).ToString());
						content = textObject.ToString();
					}
					else if (item.BannerComponent.BannerEffect.IncrementType == BannerEffect.EffectIncrementType.Add)
					{
						content = item.BannerComponent.GetBannerEffectBonus().ToString();
					}
					GameTexts.SetVariable("NUMBER", content);
					textObject2 = GameTexts.FindText("str_RANK_with_NUM_between_parenthesis");
				}
				else
				{
					textObject2 = new TextObject("{=koX9okuG}None");
				}
				propertyBasedTooltipVM.AddProperty(new TextObject("{=DbXZjPdf}Banner Effect: ").ToString(), textObject2.ToString());
			}
			if (weaponComponentData.IsAmmo)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=TAnabTdy}Accuracy").ToString(), weaponComponentData.Accuracy.ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=s31DnnAf}Damage").ToString(), ItemHelper.GetThrustDamageText(weaponComponentData, equipmentElement.Value.ItemModifier).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=twtbH1zv}Stack Amount").ToString(), equipmentElement.Value.GetModifiedStackCountForUsage(num).ToString());
			}
			if (item.Weapons.Any(delegate(WeaponComponentData x)
			{
				string weaponDescriptionId2 = x.WeaponDescriptionId;
				return weaponDescriptionId2 != null && weaponDescriptionId2.IndexOf("couch", StringComparison.OrdinalIgnoreCase) >= 0;
			}))
			{
				propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_inventory_flag_couchable").ToString());
			}
			if (item.Weapons.Any(delegate(WeaponComponentData x)
			{
				string weaponDescriptionId = x.WeaponDescriptionId;
				return weaponDescriptionId != null && weaponDescriptionId.IndexOf("bracing", StringComparison.OrdinalIgnoreCase) >= 0;
			}))
			{
				propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_inventory_flag_braceable").ToString());
			}
		}
		else if (item.HasHorseComponent)
		{
			if (item.HorseComponent.IsMount)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=8BlMRMiR}Horse Tier").ToString(), ((int)(item.Tier + 1)).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=Mfbc4rQR}Charge Damage").ToString(), equipmentElement.Value.GetModifiedMountCharge(in EquipmentElement.Invalid).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=6GSXsdeX}Speed").ToString(), equipmentElement.Value.GetModifiedMountSpeed(in EquipmentElement.Invalid).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=rg7OuWS2}Maneuver").ToString(), equipmentElement.Value.GetModifiedMountManeuver(in EquipmentElement.Invalid).ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=oBbiVeKE}Hit Points").ToString(), equipmentElement.Value.GetModifiedMountHitPoints().ToString());
				propertyBasedTooltipVM.AddProperty(new TextObject("{=ZUgoQ1Ws}Horse Type").ToString(), item.ItemCategory.GetName().ToString());
			}
		}
		else if (item.HasFoodComponent)
		{
			if (item.FoodComponent.MoraleBonus > 0)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=myMbtwXi}Morale Bonus").ToString(), item.FoodComponent.MoraleBonus.ToString());
			}
			if (item.IsFood)
			{
				propertyBasedTooltipVM.AddProperty(new TextObject("{=qSi4DlT4}Food").ToString(), " ");
			}
		}
		if (item.HasBannerComponent && item.BannerComponent?.BannerEffect != null)
		{
			GameTexts.SetVariable("RANK", item.BannerComponent.BannerEffect.Name);
			string content2 = string.Empty;
			if (item.BannerComponent.BannerEffect.IncrementType == BannerEffect.EffectIncrementType.AddFactor)
			{
				TextObject textObject3 = GameTexts.FindText("str_NUMBER_percent");
				textObject3.SetTextVariable("NUMBER", ((int)Math.Abs(item.BannerComponent.GetBannerEffectBonus() * 100f)).ToString());
				content2 = textObject3.ToString();
			}
			else if (item.BannerComponent.BannerEffect.IncrementType == BannerEffect.EffectIncrementType.Add)
			{
				content2 = item.BannerComponent.GetBannerEffectBonus().ToString();
			}
			GameTexts.SetVariable("NUMBER", content2);
			propertyBasedTooltipVM.AddProperty(new TextObject("{=DbXZjPdf}Banner Effect: ").ToString(), GameTexts.FindText("str_RANK_with_NUM_between_parenthesis").ToString());
		}
	}

	public static void RefreshBuildingTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		Building building = args[0] as Building;
		propertyBasedTooltipVM.Mode = 1;
		propertyBasedTooltipVM.AddProperty("", building.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		if (building.BuildingType.IsDefaultProject)
		{
			propertyBasedTooltipVM.AddProperty("", new TextObject("{=bd7oAQq6}Daily").ToString());
		}
		else
		{
			propertyBasedTooltipVM.AddProperty(new TextObject("{=IJdjwXvn}Current Level: ").ToString(), building.CurrentLevel.ToString());
		}
		propertyBasedTooltipVM.AddProperty("", building.Explanation.ToString(), 0, TooltipProperty.TooltipPropertyFlags.MultiLine);
		propertyBasedTooltipVM.AddProperty("", building.GetBonusExplanation().ToString());
	}

	public static void RefreshWorkshopTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		Workshop workshop = args[0] as Workshop;
		propertyBasedTooltipVM.Mode = 1;
		propertyBasedTooltipVM.AddProperty("", workshop.WorkshopType.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		propertyBasedTooltipVM.AddProperty(new TextObject("{=qRqnrtdX}Owner").ToString(), workshop.Owner.Name.ToString());
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty);
		propertyBasedTooltipVM.AddProperty(new TextObject("{=xtt9Oxer}Productions").ToString(), " ");
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		IEnumerable<(ItemCategory, int)> enumerable = workshop.WorkshopType.Productions.SelectMany((WorkshopType.Production p) => p.Inputs).Distinct(itemCategoryDistinctComparer);
		IEnumerable<(ItemCategory, int)> enumerable2 = workshop.WorkshopType.Productions.SelectMany((WorkshopType.Production p) => p.Outputs).Distinct(itemCategoryDistinctComparer);
		if (enumerable.Any())
		{
			propertyBasedTooltipVM.AddProperty(new TextObject("{=XCz81XYm}Inputs").ToString(), " ");
			propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.DefaultSeperator);
			foreach (var item in enumerable)
			{
				propertyBasedTooltipVM.AddProperty(" ", item.Item1.GetName().ToString());
			}
			propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		}
		if (!enumerable2.Any())
		{
			return;
		}
		propertyBasedTooltipVM.AddProperty(new TextObject("{=ErnykQEH}Outputs").ToString(), " ");
		propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.DefaultSeperator);
		foreach (var item2 in enumerable2)
		{
			propertyBasedTooltipVM.AddProperty(" ", item2.Item1.GetName().ToString());
		}
	}

	public static void RefreshEncounterTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		int num = (int)args[0];
		List<MobileParty> partiesToJoinPlayerSide = new List<MobileParty> { MobileParty.MainParty };
		List<MobileParty> partiesToJoinEnemySide = new List<MobileParty> { Campaign.Current.ConversationManager.ConversationParty };
		PlayerEncounter.Current.FindAllNpcPartiesWhoWillJoinEvent(ref partiesToJoinPlayerSide, ref partiesToJoinEnemySide);
		List<MobileParty> parties = null;
		if (num == 0)
		{
			parties = partiesToJoinPlayerSide;
			propertyBasedTooltipVM.Mode = 2;
		}
		else
		{
			parties = partiesToJoinEnemySide;
			propertyBasedTooltipVM.Mode = 3;
		}
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		TroopRoster troopRoster2 = TroopRoster.CreateDummyTroopRoster();
		foreach (MobileParty item in parties)
		{
			for (int i = 0; i < item.MemberRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = item.MemberRoster.GetElementCopyAtIndex(i);
				troopRoster.AddToCounts(elementCopyAtIndex.Character, elementCopyAtIndex.Number, insertAtFront: false, elementCopyAtIndex.WoundedNumber);
			}
			for (int j = 0; j < item.PrisonRoster.Count; j++)
			{
				TroopRosterElement elementCopyAtIndex2 = item.PrisonRoster.GetElementCopyAtIndex(j);
				troopRoster2.AddToCounts(elementCopyAtIndex2.Character, elementCopyAtIndex2.Number, insertAtFront: false, elementCopyAtIndex2.WoundedNumber);
			}
		}
		Func<TroopRoster> funcToDoBeforeLambda = delegate
		{
			TroopRoster troopRoster4 = TroopRoster.CreateDummyTroopRoster();
			foreach (MobileParty item2 in parties)
			{
				for (int l = 0; l < item2.MemberRoster.Count; l++)
				{
					TroopRosterElement elementCopyAtIndex4 = item2.MemberRoster.GetElementCopyAtIndex(l);
					troopRoster4.AddToCounts(elementCopyAtIndex4.Character, elementCopyAtIndex4.Number, insertAtFront: false, elementCopyAtIndex4.WoundedNumber);
				}
			}
			return troopRoster4;
		};
		Func<TroopRoster> funcToDoBeforeLambda2 = delegate
		{
			TroopRoster troopRoster3 = TroopRoster.CreateDummyTroopRoster();
			foreach (MobileParty item3 in parties)
			{
				for (int k = 0; k < item3.PrisonRoster.Count; k++)
				{
					TroopRosterElement elementCopyAtIndex3 = item3.PrisonRoster.GetElementCopyAtIndex(k);
					troopRoster3.AddToCounts(elementCopyAtIndex3.Character, elementCopyAtIndex3.Number, insertAtFront: false, elementCopyAtIndex3.WoundedNumber);
				}
			}
			return troopRoster3;
		};
		bool flag = false;
		foreach (MobileParty item4 in parties)
		{
			flag = flag || item4.IsInspected;
			propertyBasedTooltipVM.AddProperty("", item4.Name.ToString(), 1);
			if (item4.Name.ToString() != item4.MapFaction?.Name.ToString())
			{
				propertyBasedTooltipVM.AddProperty("", item4.MapFaction?.Name.ToString() ?? "");
			}
		}
		if (troopRoster.Count > 0)
		{
			AddPartyTroopProperties(propertyBasedTooltipVM, troopRoster, GameTexts.FindText("str_map_tooltip_troops"), flag, funcToDoBeforeLambda);
		}
		if (troopRoster2.Count > 0)
		{
			AddPartyTroopProperties(propertyBasedTooltipVM, troopRoster2, GameTexts.FindText("str_map_tooltip_prisoners"), flag, funcToDoBeforeLambda2);
		}
		if (!Campaign.Current.IsMapTooltipLongForm && !propertyBasedTooltipVM.IsExtended)
		{
			propertyBasedTooltipVM.AddProperty("", "", -1);
			GameTexts.SetVariable("EXTEND_KEY", propertyBasedTooltipVM.GetKeyText(ExtendKeyId));
			propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_map_tooltip_info").ToString());
		}
	}

	public static void RefreshMapEventTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		MapEvent mapEvent = args[0] as MapEvent;
		propertyBasedTooltipVM.Mode = 4;
		TooltipProperty.TooltipPropertyFlags tooltipPropertyFlags = TooltipProperty.TooltipPropertyFlags.None;
		TooltipProperty.TooltipPropertyFlags tooltipPropertyFlags2 = TooltipProperty.TooltipPropertyFlags.None;
		tooltipPropertyFlags = (FactionManager.IsAtWarAgainstFaction(mapEvent.AttackerSide.LeaderParty.MapFaction, PartyBase.MainParty.MapFaction) ? TooltipProperty.TooltipPropertyFlags.WarFirstEnemy : ((mapEvent.AttackerSide.LeaderParty.MapFaction != PartyBase.MainParty.MapFaction && !FactionManager.IsAlliedWithFaction(mapEvent.AttackerSide.LeaderParty.MapFaction, PartyBase.MainParty.MapFaction)) ? TooltipProperty.TooltipPropertyFlags.WarFirstNeutral : TooltipProperty.TooltipPropertyFlags.WarFirstAlly));
		tooltipPropertyFlags2 = (FactionManager.IsAtWarAgainstFaction(mapEvent.DefenderSide.LeaderParty.MapFaction, PartyBase.MainParty.MapFaction) ? TooltipProperty.TooltipPropertyFlags.WarSecondEnemy : ((mapEvent.DefenderSide.LeaderParty.MapFaction != PartyBase.MainParty.MapFaction && !FactionManager.IsAlliedWithFaction(mapEvent.DefenderSide.LeaderParty.MapFaction, PartyBase.MainParty.MapFaction)) ? TooltipProperty.TooltipPropertyFlags.WarSecondNeutral : TooltipProperty.TooltipPropertyFlags.WarSecondAlly));
		propertyBasedTooltipVM.AddProperty("", "", 1, tooltipPropertyFlags | tooltipPropertyFlags2);
		if (mapEvent.IsSiegeAssault)
		{
			TextObject textObject = new TextObject("{=43HYUImy}{SETTLEMENT}'s Siege");
			textObject.SetTextVariable("SETTLEMENT", mapEvent.MapEventSettlement.Name);
			propertyBasedTooltipVM.AddProperty("", textObject.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		}
		else if (mapEvent.IsRaid)
		{
			TextObject textObject2 = new TextObject("{=T9bndUYP}{SETTLEMENT}'s Raid");
			textObject2.SetTextVariable("SETTLEMENT", mapEvent.MapEventSettlement.Name);
			propertyBasedTooltipVM.AddProperty("", textObject2.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		}
		else
		{
			TextObject textObject3 = new TextObject("{=CnsIzaWo}Field Battle");
			propertyBasedTooltipVM.AddProperty("", textObject3.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		}
		propertyBasedTooltipVM.AddProperty("", "", -1);
		AddEncounterParties(propertyBasedTooltipVM, mapEvent.AttackerSide.Parties, mapEvent.DefenderSide.Parties, propertyBasedTooltipVM.IsExtended);
		if (!propertyBasedTooltipVM.IsExtended)
		{
			propertyBasedTooltipVM.AddProperty("", "", -1);
			GameTexts.SetVariable("EXTEND_KEY", propertyBasedTooltipVM.GetKeyText(ExtendKeyId));
			propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_map_tooltip_info").ToString());
		}
	}

	public static void RefreshSettlementTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		Settlement settlement = args[0] as Settlement;
		bool flag = (bool)args[1];
		PartyBase settlementAsParty = settlement.Party;
		if (settlementAsParty == null)
		{
			return;
		}
		if (FactionManager.IsAtWarAgainstFaction(settlementAsParty.MapFaction, PartyBase.MainParty.MapFaction))
		{
			propertyBasedTooltipVM.Mode = 3;
		}
		else if (settlementAsParty.MapFaction == PartyBase.MainParty.MapFaction || FactionManager.IsAlliedWithFaction(settlementAsParty.MapFaction, PartyBase.MainParty.MapFaction))
		{
			propertyBasedTooltipVM.Mode = 2;
		}
		else
		{
			propertyBasedTooltipVM.Mode = 1;
		}
		if (Game.Current.IsDevelopmentMode && settlement.IsHideout)
		{
			propertyBasedTooltipVM.AddProperty("", string.Concat(settlement.Name, " (", settlementAsParty.Id, ")"), 1);
		}
		else
		{
			propertyBasedTooltipVM.AddProperty("", settlement.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		}
		TextObject disableReason;
		bool flag2 = !CampaignUIHelper.IsSettlementInformationHidden(settlement, out disableReason);
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_owner").ToString(), " ");
		propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
		TextObject textObject = new TextObject("{=HAaElX8X}{PARTY_OWNERS_FACTION}");
		TextObject variable = ((settlement.OwnerClan == null) ? new TextObject("{=3PzgpFGq}Neutral") : settlement.OwnerClan.Name);
		textObject.SetTextVariable("PARTY_OWNERS_FACTION", variable);
		propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_clan").ToString(), textObject.ToString());
		if (settlementAsParty.MapFaction != null)
		{
			TextObject textObject2 = new TextObject("{=s6koeapc}{MAP_FACTION}");
			textObject2.SetTextVariable("MAP_FACTION", settlementAsParty.MapFaction?.Name ?? new TextObject("{=!}ERROR"));
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_faction").ToString(), textObject2.ToString());
		}
		if (settlement.Culture != null && !TextObject.IsNullOrEmpty(settlement.Culture.Name))
		{
			TextObject textObject3 = new TextObject("{=!}{CULTURE}");
			textObject3.SetTextVariable("CULTURE", settlement.Culture.Name);
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_culture").ToString(), textObject3.ToString());
		}
		if (flag2)
		{
			if (settlementAsParty.IsSettlement && (settlementAsParty.Settlement.IsVillage || settlementAsParty.Settlement.IsTown || settlementAsParty.Settlement.IsCastle))
			{
				propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
				propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_information").ToString(), " ");
				propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
			}
			if (settlement.IsVillage || settlement.IsFortification)
			{
				propertyBasedTooltipVM.AddProperty(settlementAsParty.Settlement.IsFortification ? GameTexts.FindText("str_map_tooltip_prosperity").ToString() : GameTexts.FindText("str_map_tooltip_hearths").ToString(), getProsperity);
			}
			if (settlement.IsFortification)
			{
				propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_loyalty").ToString(), getLoyalty);
				propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_security").ToString(), getSecurity());
			}
			if (settlement.IsVillage || settlement.IsFortification)
			{
				propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_map_tooltip_militia").ToString(), getMilitia);
			}
			if (settlement.IsFortification)
			{
				propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_map_tooltip_garrison").ToString(), getGarrison);
				propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_map_tooltip_food_stocks").ToString(), getFood);
				int wallLevel = settlementAsParty.Settlement.Town.GetWallLevel();
				propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_map_tooltip_wall_level").ToString(), wallLevel.ToString());
			}
		}
		if (settlement.IsVillage)
		{
			string definition = GameTexts.FindText("str_bound_settlement").ToString();
			string value = settlementAsParty.Settlement.Village.Bound.Name.ToString();
			propertyBasedTooltipVM.AddProperty(definition, value);
			if (settlementAsParty.Settlement.Village.TradeBound != null)
			{
				string definition2 = GameTexts.FindText("str_trade_bound_settlement").ToString();
				string value2 = settlementAsParty.Settlement.Village.TradeBound.Name.ToString();
				propertyBasedTooltipVM.AddProperty(definition2, value2);
			}
			ItemObject primaryProduction = settlementAsParty.Settlement.Village.VillageType.PrimaryProduction;
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_primary_production").ToString(), primaryProduction.Name.ToString());
		}
		if (settlement.BoundVillages.Count > 0)
		{
			string text = "";
			string definition3 = GameTexts.FindText("str_bound_village").ToString();
			if (settlementAsParty.Settlement.BoundVillages.Count == 1)
			{
				text = settlementAsParty.Settlement.BoundVillages[0].Name.ToString();
			}
			else
			{
				for (int i = 0; i < settlementAsParty.Settlement.BoundVillages.Count; i++)
				{
					text = ((i + 1 == settlementAsParty.Settlement.BoundVillages.Count) ? (text + settlementAsParty.Settlement.BoundVillages[i].Name.ToString()) : (text + settlementAsParty.Settlement.BoundVillages[i].Name.ToString() + ",\n"));
				}
			}
			propertyBasedTooltipVM.AddProperty(definition3, text);
			if (propertyBasedTooltipVM.IsExtended && settlement.IsTown && settlement.Town.TradeBoundVillages.Count > 0)
			{
				string text2 = "";
				string definition4 = GameTexts.FindText("str_trade_bound_village").ToString();
				for (int j = 0; j < settlement.Town.TradeBoundVillages.Count; j++)
				{
					text2 = ((j + 1 == settlement.Town.TradeBoundVillages.Count) ? (text2 + settlement.Town.TradeBoundVillages[j].Name.ToString()) : (text2 + settlement.Town.TradeBoundVillages[j].Name.ToString() + ",\n"));
				}
				propertyBasedTooltipVM.AddProperty(definition4, text2);
			}
		}
		if (Game.Current.IsDevelopmentMode && settlement.IsTown)
		{
			propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
			propertyBasedTooltipVM.AddProperty("[DEV] " + GameTexts.FindText("str_shops").ToString(), " ");
			propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
			int num = 1;
			Workshop[] workshops = settlementAsParty.Settlement.Town.Workshops;
			foreach (Workshop workshop in workshops)
			{
				if (workshop.WorkshopType != null)
				{
					propertyBasedTooltipVM.AddProperty("[DEV] Shop " + num, workshop.WorkshopType.Name.ToString());
					num++;
				}
			}
		}
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		TroopRoster troopRoster2 = TroopRoster.CreateDummyTroopRoster();
		TroopRoster.CreateDummyTroopRoster();
		Func<TroopRoster> func = delegate
		{
			TroopRoster troopRoster4 = TroopRoster.CreateDummyTroopRoster();
			foreach (MobileParty party in settlement.Parties)
			{
				if (!FactionManager.IsAtWarAgainstFaction(party.MapFaction, settlementAsParty.MapFaction) && (!(party.Aggressiveness < 0.01f) || party.IsGarrison || party.IsMilitia) && !party.IsMainParty)
				{
					for (int num7 = 0; num7 < party.MemberRoster.Count; num7++)
					{
						TroopRosterElement elementCopyAtIndex3 = party.MemberRoster.GetElementCopyAtIndex(num7);
						troopRoster4.AddToCounts(elementCopyAtIndex3.Character, elementCopyAtIndex3.Number, insertAtFront: false, elementCopyAtIndex3.WoundedNumber);
					}
				}
			}
			return troopRoster4;
		};
		Func<TroopRoster> func2 = delegate
		{
			TroopRoster troopRoster3 = TroopRoster.CreateDummyTroopRoster();
			foreach (MobileParty party2 in settlement.Parties)
			{
				if (!party2.IsMainParty && !FactionManager.IsAtWarAgainstFaction(party2.MapFaction, settlementAsParty.MapFaction))
				{
					for (int m = 0; m < party2.PrisonRoster.Count; m++)
					{
						TroopRosterElement elementCopyAtIndex = party2.PrisonRoster.GetElementCopyAtIndex(m);
						troopRoster3.AddToCounts(elementCopyAtIndex.Character, elementCopyAtIndex.Number, insertAtFront: false, elementCopyAtIndex.WoundedNumber);
					}
				}
			}
			for (int n = 0; n < settlementAsParty.PrisonRoster.Count; n++)
			{
				TroopRosterElement elementCopyAtIndex2 = settlementAsParty.PrisonRoster.GetElementCopyAtIndex(n);
				troopRoster3.AddToCounts(elementCopyAtIndex2.Character, elementCopyAtIndex2.Number, insertAtFront: false, elementCopyAtIndex2.WoundedNumber);
			}
			return troopRoster3;
		};
		troopRoster2 = func2();
		if (propertyBasedTooltipVM.IsExtended)
		{
			troopRoster = func();
			if (troopRoster.Count > 0)
			{
				AddPartyTroopProperties(propertyBasedTooltipVM, troopRoster, GameTexts.FindText("str_map_tooltip_troops"), flag || flag2, func);
			}
		}
		else
		{
			propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
			if (!settlement.IsHideout && (flag2 || flag))
			{
				List<MobileParty> list = new List<MobileParty>();
				Town town = settlement.Town;
				bool flag3 = town == null || !town.InRebelliousState;
				for (int l = 0; l < settlement.Parties.Count; l++)
				{
					MobileParty mobileParty = settlement.Parties[l];
					bool flag4 = flag3 && mobileParty.IsMilitia;
					if (FactionManager.IsAlliedWithFaction(settlementAsParty.MapFaction, mobileParty.MapFaction) && (mobileParty.IsLordParty || flag4 || mobileParty.IsGarrison))
					{
						list.Add(mobileParty);
					}
				}
				list.Sort(CampaignUIHelper.MobilePartyPrecedenceComparerInstance);
				List<MobileParty> list2 = settlement.Parties.Where((MobileParty p) => !p.IsLordParty && !p.IsMilitia && !p.IsGarrison).ToList();
				list2.Sort(CampaignUIHelper.MobilePartyPrecedenceComparerInstance);
				if (list.Count > 0)
				{
					int num2 = list.Sum((MobileParty p) => p.Party.NumberOfHealthyMembers);
					int num3 = list.Sum((MobileParty p) => p.Party.NumberOfWoundedTotalMembers);
					string value3 = num2 + ((num3 > 0) ? ("+" + num3 + GameTexts.FindText("str_party_nameplate_wounded_abbr").ToString()) : "");
					propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_map_tooltip_defenders").ToString(), value3);
					propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
					foreach (MobileParty item in list)
					{
						propertyBasedTooltipVM.AddProperty(item.Name.ToString(), CampaignUIHelper.GetPartyNameplateText(item, includeAttachedParties: false));
					}
					propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
				}
				if (list2.Count > 0)
				{
					propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.DefaultSeperator);
					foreach (MobileParty item2 in list2)
					{
						propertyBasedTooltipVM.AddProperty(item2.Name.ToString(), CampaignUIHelper.GetPartyNameplateText(item2, includeAttachedParties: false));
					}
				}
			}
			else
			{
				string value4 = GameTexts.FindText("str_missing_info_indicator").ToString();
				propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_map_tooltip_parties").ToString(), value4);
			}
		}
		if (!settlement.IsHideout && troopRoster2.Count > 0 && (flag2 || flag))
		{
			AddPartyTroopProperties(propertyBasedTooltipVM, troopRoster2, GameTexts.FindText("str_map_tooltip_prisoners"), flag2, func2);
		}
		if (settlement.IsFortification && settlement.Town.InRebelliousState)
		{
			propertyBasedTooltipVM.AddProperty(string.Empty, GameTexts.FindText("str_settlement_rebellious_state").ToString(), -1);
		}
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		if (!settlement.IsHideout && !propertyBasedTooltipVM.IsExtended && (flag2 || flag))
		{
			GameTexts.SetVariable("EXTEND_KEY", propertyBasedTooltipVM.GetKeyText(ExtendKeyId));
			propertyBasedTooltipVM.AddProperty(string.Empty, GameTexts.FindText("str_map_tooltip_info").ToString(), -1);
		}
		string getFood()
		{
			int num4 = (int)settlementAsParty.Settlement.Town.FoodChange;
			int variable2 = (int)settlementAsParty.Settlement.Town.FoodStocks;
			TextObject textObject4 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})");
			textObject4.SetTextVariable("VALUE", variable2);
			textObject4.SetTextVariable("POSITIVE", (num4 > 0) ? 1 : 0);
			textObject4.SetTextVariable("DELTA_VALUE", num4);
			return textObject4.ToString();
		}
		string getGarrison()
		{
			int garrisonChange = settlementAsParty.Settlement.Town.GarrisonChange;
			int variable3 = settlementAsParty.Settlement.Town.GarrisonParty?.MemberRoster.TotalManCount ?? 0;
			TextObject textObject5 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})");
			textObject5.SetTextVariable("VALUE", variable3);
			textObject5.SetTextVariable("POSITIVE", (garrisonChange > 0) ? 1 : 0);
			textObject5.SetTextVariable("DELTA_VALUE", garrisonChange);
			return textObject5.ToString();
		}
		string getLoyalty()
		{
			TextObject textObject8 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})");
			textObject8.SetTextVariable("VALUE", float.Parse($"{settlement.Town.Loyalty:0.00}"));
			textObject8.SetTextVariable("POSITIVE", (settlement.Town.LoyaltyChange > 0f) ? 1 : 0);
			textObject8.SetTextVariable("DELTA_VALUE", float.Parse($"{settlement.Town.LoyaltyChange:0.00}"));
			return textObject8.ToString();
		}
		string getMilitia()
		{
			int num5 = (int)(settlementAsParty.Settlement.IsFortification ? settlementAsParty.Settlement.Town.MilitiaChange : settlementAsParty.Settlement.Village.MilitiaChange);
			int variable4 = (int)settlementAsParty.Settlement.Militia;
			TextObject textObject6 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})");
			textObject6.SetTextVariable("VALUE", variable4);
			textObject6.SetTextVariable("POSITIVE", (num5 > 0) ? 1 : 0);
			textObject6.SetTextVariable("DELTA_VALUE", num5);
			return textObject6.ToString();
		}
		string getProsperity()
		{
			float num6 = float.Parse($"{(settlementAsParty.Settlement.IsFortification ? settlementAsParty.Settlement.Town.ProsperityChange : settlementAsParty.Settlement.Village.HearthChange):0.00}");
			int variable5 = (int)(settlementAsParty.Settlement.IsFortification ? settlementAsParty.Settlement.Town.Prosperity : settlementAsParty.Settlement.Village.Hearth);
			TextObject textObject9 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})");
			textObject9.SetTextVariable("VALUE", variable5);
			textObject9.SetTextVariable("POSITIVE", (num6 > 0f) ? 1 : 0);
			textObject9.SetTextVariable("DELTA_VALUE", num6);
			return textObject9.ToString();
		}
		string getSecurity()
		{
			TextObject textObject7 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})");
			textObject7.SetTextVariable("VALUE", float.Parse($"{settlement.Town.Security:0.00}"));
			textObject7.SetTextVariable("POSITIVE", (settlement.Town.SecurityChange > 0f) ? 1 : 0);
			textObject7.SetTextVariable("DELTA_VALUE", float.Parse($"{settlement.Town.SecurityChange:0.00}"));
			return textObject7.ToString();
		}
	}

	public static void RefreshMobilePartyTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		MobileParty mobileParty = args[0] as MobileParty;
		bool flag = (bool)args[1];
		bool flag2 = (bool)args[2];
		if (mobileParty == null)
		{
			return;
		}
		if (FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, PartyBase.MainParty.MapFaction))
		{
			propertyBasedTooltipVM.Mode = 3;
		}
		else if (mobileParty.MapFaction == PartyBase.MainParty.MapFaction || FactionManager.IsAlliedWithFaction(mobileParty.MapFaction, PartyBase.MainParty.MapFaction))
		{
			propertyBasedTooltipVM.Mode = 2;
		}
		else
		{
			propertyBasedTooltipVM.Mode = 1;
		}
		if (Game.Current.IsDevelopmentMode)
		{
			propertyBasedTooltipVM.AddProperty("", string.Concat(mobileParty.Name, " (", mobileParty.Id, ")"), 1, TooltipProperty.TooltipPropertyFlags.Title);
		}
		else
		{
			propertyBasedTooltipVM.AddProperty("", mobileParty.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		}
		bool isInspected = mobileParty.IsInspected;
		if (mobileParty.IsDisorganized)
		{
			TextObject hoursAndDaysTextFromHourValue = CampaignUIHelper.GetHoursAndDaysTextFromHourValue(TaleWorlds.Library.MathF.Ceiling(mobileParty.DisorganizedUntilTime.RemainingHoursFromNow));
			TextObject textObject = new TextObject("{=BbLTwhsA}Disorganized for {REMAINING_TIME}");
			textObject.SetTextVariable("REMAINING_TIME", hoursAndDaysTextFromHourValue.ToString());
			propertyBasedTooltipVM.AddProperty("", textObject.ToString());
			propertyBasedTooltipVM.AddProperty("", "", -1);
		}
		if (isInspected)
		{
			propertyBasedTooltipVM.AddProperty("", CampaignUIHelper.GetMobilePartyBehaviorText(mobileParty));
		}
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_owner").ToString(), " ");
		propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
		if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.Clan != mobileParty.MapFaction)
		{
			TextObject textObject2 = new TextObject("{=oUhd9YhP}{PARTY_LEADERS_FACTION}");
			textObject2.SetTextVariable("PARTY_LEADERS_FACTION", mobileParty.LeaderHero.Clan.Name);
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_clan").ToString(), textObject2.ToString());
		}
		if (mobileParty.MapFaction != null)
		{
			TextObject textObject3 = new TextObject("{=s6koeapc}{MAP_FACTION}");
			textObject3.SetTextVariable("MAP_FACTION", mobileParty.MapFaction?.Name ?? new TextObject("{=!}ERROR"));
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_faction").ToString(), textObject3.ToString());
		}
		if (isInspected)
		{
			propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_information").ToString(), " ");
			propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_map_tooltip_speed").ToString(), CampaignUIHelper.FloatToString(mobileParty.Speed));
			if (propertyBasedTooltipVM.IsExtended)
			{
				TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(mobileParty.CurrentNavigationFace);
				string definition = GameTexts.FindText("str_terrain").ToString();
				int num = (int)faceTerrainType;
				propertyBasedTooltipVM.AddProperty(definition, GameTexts.FindText("str_terrain_types", num.ToString()).ToString());
			}
		}
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		TroopRoster troopRoster2 = TroopRoster.CreateDummyTroopRoster();
		TroopRoster.CreateDummyTroopRoster();
		Func<TroopRoster> func = delegate
		{
			TroopRoster troopRoster4 = TroopRoster.CreateDummyTroopRoster();
			for (int j = 0; j < mobileParty.MemberRoster.Count; j++)
			{
				TroopRosterElement elementCopyAtIndex2 = mobileParty.MemberRoster.GetElementCopyAtIndex(j);
				troopRoster4.AddToCounts(elementCopyAtIndex2.Character, elementCopyAtIndex2.Number, insertAtFront: false, elementCopyAtIndex2.WoundedNumber);
			}
			return troopRoster4;
		};
		Func<TroopRoster> func2 = delegate
		{
			TroopRoster troopRoster3 = TroopRoster.CreateDummyTroopRoster();
			for (int i = 0; i < mobileParty.PrisonRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = mobileParty.PrisonRoster.GetElementCopyAtIndex(i);
				troopRoster3.AddToCounts(elementCopyAtIndex.Character, elementCopyAtIndex.Number, insertAtFront: false, elementCopyAtIndex.WoundedNumber);
			}
			return troopRoster3;
		};
		troopRoster = func();
		troopRoster2 = func2();
		if (troopRoster.Count > 0)
		{
			AddPartyTroopProperties(propertyBasedTooltipVM, troopRoster, GameTexts.FindText("str_map_tooltip_troops"), flag || isInspected || !flag2, func);
		}
		if (troopRoster2.Count > 0 && (isInspected || flag))
		{
			AddPartyTroopProperties(propertyBasedTooltipVM, troopRoster2, GameTexts.FindText("str_map_tooltip_prisoners"), isInspected || !flag2, func2);
		}
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		if (!propertyBasedTooltipVM.IsExtended && (isInspected || flag))
		{
			GameTexts.SetVariable("EXTEND_KEY", propertyBasedTooltipVM.GetKeyText(ExtendKeyId));
			propertyBasedTooltipVM.AddProperty(string.Empty, GameTexts.FindText("str_map_tooltip_info").ToString(), -1);
		}
		if (mobileParty != MobileParty.MainParty && !flag)
		{
			GameTexts.SetVariable("MODIFIER_KEY", propertyBasedTooltipVM.GetKeyText(FollowModifierKeyId));
			GameTexts.SetVariable("CLICK_KEY", propertyBasedTooltipVM.GetKeyText(MapClickKeyId));
			propertyBasedTooltipVM.AddProperty(string.Empty, GameTexts.FindText("str_map_follow_party_info").ToString(), -1);
		}
	}

	public static void RefreshArmyTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		Army army = args[0] as Army;
		bool flag = (bool)args[1];
		bool flag2 = (bool)args[2];
		MobileParty leaderParty = army.LeaderParty;
		if (leaderParty == null)
		{
			return;
		}
		if (FactionManager.IsAtWarAgainstFaction(leaderParty.MapFaction, PartyBase.MainParty.MapFaction))
		{
			propertyBasedTooltipVM.Mode = 3;
		}
		else if (army.Kingdom == PartyBase.MainParty.MapFaction || FactionManager.IsAlliedWithFaction(leaderParty.MapFaction, PartyBase.MainParty.MapFaction))
		{
			propertyBasedTooltipVM.Mode = 2;
		}
		else
		{
			propertyBasedTooltipVM.Mode = 1;
		}
		propertyBasedTooltipVM.AddProperty("", army.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
		if (leaderParty.IsInspected || !flag2)
		{
			propertyBasedTooltipVM.AddProperty("", CampaignUIHelper.GetMobilePartyBehaviorText(leaderParty));
		}
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_owner").ToString(), " ");
		propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
		if (army.Kingdom != null)
		{
			TextObject textObject = new TextObject("{=s6koeapc}{MAP_FACTION}");
			textObject.SetTextVariable("MAP_FACTION", army.Kingdom?.Name ?? new TextObject("{=!}ERROR"));
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_faction").ToString(), textObject.ToString());
		}
		if (leaderParty.IsInspected || !flag2)
		{
			propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_information").ToString(), " ");
			propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
			propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_map_tooltip_speed").ToString(), CampaignUIHelper.FloatToString(leaderParty.Speed));
			if (propertyBasedTooltipVM.IsExtended)
			{
				TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(leaderParty.CurrentNavigationFace);
				string definition = GameTexts.FindText("str_terrain").ToString();
				int num = (int)faceTerrainType;
				propertyBasedTooltipVM.AddProperty(definition, GameTexts.FindText("str_terrain_types", num.ToString()).ToString());
			}
		}
		TroopRoster troopRoster = GetTempRoster();
		TroopRoster troopRoster2 = GetTempPrisonerRoster();
		if (troopRoster.Count > 0)
		{
			AddPartyTroopProperties(propertyBasedTooltipVM, troopRoster, GameTexts.FindText("str_map_tooltip_troops"), flag || leaderParty.IsInspected, GetTempRoster);
		}
		if (troopRoster2.Count > 0 && (leaderParty.IsInspected || flag || !flag2))
		{
			AddPartyTroopProperties(propertyBasedTooltipVM, troopRoster2, GameTexts.FindText("str_map_tooltip_prisoners"), leaderParty.IsInspected || !flag2, GetTempPrisonerRoster);
		}
		TroopRoster GetTempPrisonerRoster()
		{
			TroopRoster troopRoster3 = TroopRoster.CreateDummyTroopRoster();
			for (int i = 0; i < army.LeaderParty.PrisonRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = army.LeaderParty.PrisonRoster.GetElementCopyAtIndex(i);
				troopRoster3.AddToCounts(elementCopyAtIndex.Character, elementCopyAtIndex.Number, insertAtFront: false, elementCopyAtIndex.WoundedNumber);
			}
			foreach (MobileParty attachedParty in army.LeaderParty.AttachedParties)
			{
				for (int j = 0; j < attachedParty.PrisonRoster.Count; j++)
				{
					TroopRosterElement elementCopyAtIndex2 = attachedParty.PrisonRoster.GetElementCopyAtIndex(j);
					troopRoster3.AddToCounts(elementCopyAtIndex2.Character, elementCopyAtIndex2.Number, insertAtFront: false, elementCopyAtIndex2.WoundedNumber);
				}
			}
			return troopRoster3;
		}
		TroopRoster GetTempRoster()
		{
			TroopRoster troopRoster4 = TroopRoster.CreateDummyTroopRoster();
			for (int k = 0; k < army.LeaderParty.MemberRoster.Count; k++)
			{
				TroopRosterElement elementCopyAtIndex3 = army.LeaderParty.MemberRoster.GetElementCopyAtIndex(k);
				troopRoster4.AddToCounts(elementCopyAtIndex3.Character, elementCopyAtIndex3.Number, insertAtFront: false, elementCopyAtIndex3.WoundedNumber);
			}
			foreach (MobileParty attachedParty2 in army.LeaderParty.AttachedParties)
			{
				for (int l = 0; l < attachedParty2.MemberRoster.Count; l++)
				{
					TroopRosterElement elementCopyAtIndex4 = attachedParty2.MemberRoster.GetElementCopyAtIndex(l);
					troopRoster4.AddToCounts(elementCopyAtIndex4.Character, elementCopyAtIndex4.Number, insertAtFront: false, elementCopyAtIndex4.WoundedNumber);
				}
			}
			return troopRoster4;
		}
	}

	private static void AddEncounterParties(PropertyBasedTooltipVM propertyBasedTooltipVM, MBReadOnlyList<MapEventParty> parties1, MBReadOnlyList<MapEventParty> parties2, bool isExtended)
	{
		propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.BattleMode);
		for (int i = 0; i < parties1.Count || i < parties2.Count; i++)
		{
			MBTextManager.SetTextVariable("PARTY_1S_MEMBERS", "");
			MBTextManager.SetTextVariable("PARTY_2S_MEMBERS", "");
			if (i < parties1.Count)
			{
				MBTextManager.SetTextVariable("PARTY_1S_MEMBERS", parties1[i].Party.Name);
			}
			if (i < parties2.Count)
			{
				MBTextManager.SetTextVariable("PARTY_2S_MEMBERS", parties2[i].Party.Name);
			}
			propertyBasedTooltipVM.AddProperty(new TextObject("{=CExQ40Ux}{PARTY_1S_MEMBERS}   ").ToString(), new TextObject("{=OTaPfaJl}{PARTY_2S_MEMBERS}   ").ToString());
		}
		if (parties1.Count > 0 && parties2.Count > 0 && parties1[0].Party?.MapFaction != null && parties2[0].Party?.MapFaction != null)
		{
			propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.DefaultSeperator);
			MBTextManager.SetTextVariable("PARTY_1S_MEMBERS", parties1[0].Party.MapFaction.Name);
			MBTextManager.SetTextVariable("PARTY_2S_MEMBERS", parties2[0].Party.MapFaction.Name);
			propertyBasedTooltipVM.AddProperty(new TextObject("{=CExQ40Ux}{PARTY_1S_MEMBERS}   ").ToString(), new TextObject("{=OTaPfaJl}{PARTY_2S_MEMBERS}   ").ToString());
		}
		int lastHeroIndex = 0;
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		foreach (MapEventParty item in parties1)
		{
			for (int j = 0; j < item.Party.MemberRoster.Count; j++)
			{
				TroopRosterElement elementCopyAtIndex = item.Party.MemberRoster.GetElementCopyAtIndex(j);
				if (elementCopyAtIndex.Character.IsHero)
				{
					troopRoster.AddToCounts(elementCopyAtIndex.Character, elementCopyAtIndex.Number, insertAtFront: false, elementCopyAtIndex.WoundedNumber, 0, removeDepleted: true, lastHeroIndex);
					lastHeroIndex++;
				}
				else
				{
					troopRoster.AddToCounts(elementCopyAtIndex.Character, elementCopyAtIndex.Number, insertAtFront: false, elementCopyAtIndex.WoundedNumber);
				}
			}
		}
		lastHeroIndex = 0;
		TroopRoster troopRoster2 = TroopRoster.CreateDummyTroopRoster();
		foreach (MapEventParty item2 in parties2)
		{
			for (int k = 0; k < item2.Party.MemberRoster.Count; k++)
			{
				TroopRosterElement elementCopyAtIndex2 = item2.Party.MemberRoster.GetElementCopyAtIndex(k);
				if (elementCopyAtIndex2.Character.IsHero)
				{
					troopRoster2.AddToCounts(elementCopyAtIndex2.Character, elementCopyAtIndex2.Number, insertAtFront: false, elementCopyAtIndex2.WoundedNumber, 0, removeDepleted: true, lastHeroIndex);
					lastHeroIndex++;
				}
				else
				{
					troopRoster2.AddToCounts(elementCopyAtIndex2.Character, elementCopyAtIndex2.Number, insertAtFront: false, elementCopyAtIndex2.WoundedNumber);
				}
			}
		}
		Func<string> func = () => "";
		Func<string> func2 = () => "";
		if (troopRoster.Count > 0)
		{
			func = delegate
			{
				TroopRoster troopRoster4 = TroopRoster.CreateDummyTroopRoster();
				lastHeroIndex = 0;
				foreach (MapEventParty item3 in parties1)
				{
					for (int num2 = 0; num2 < item3.Party.MemberRoster.Count; num2++)
					{
						TroopRosterElement elementCopyAtIndex6 = item3.Party.MemberRoster.GetElementCopyAtIndex(num2);
						if (elementCopyAtIndex6.Character.IsHero)
						{
							troopRoster4.AddToCounts(elementCopyAtIndex6.Character, elementCopyAtIndex6.Number, insertAtFront: false, elementCopyAtIndex6.WoundedNumber, 0, removeDepleted: true, lastHeroIndex);
							lastHeroIndex++;
						}
						else
						{
							troopRoster4.AddToCounts(elementCopyAtIndex6.Character, elementCopyAtIndex6.Number, insertAtFront: false, elementCopyAtIndex6.WoundedNumber);
						}
					}
				}
				TextObject textObject4 = new TextObject("{=QlbkxoSp} {TOOLTIP_TROOPS} ({PARTY_SIZE})");
				textObject4.SetTextVariable("TOOLTIP_TROOPS", GameTexts.FindText("str_map_tooltip_troops"));
				textObject4.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(troopRoster4.TotalManCount - troopRoster4.TotalWounded, troopRoster4.TotalWounded, isInspected: true));
				return textObject4.ToString();
			};
		}
		if (troopRoster2.Count > 0)
		{
			func2 = delegate
			{
				TroopRoster troopRoster3 = TroopRoster.CreateDummyTroopRoster();
				lastHeroIndex = 0;
				foreach (MapEventParty item4 in parties2)
				{
					for (int num = 0; num < item4.Party.MemberRoster.Count; num++)
					{
						TroopRosterElement elementCopyAtIndex5 = item4.Party.MemberRoster.GetElementCopyAtIndex(num);
						if (elementCopyAtIndex5.Character.IsHero)
						{
							troopRoster3.AddToCounts(elementCopyAtIndex5.Character, elementCopyAtIndex5.Number, insertAtFront: false, elementCopyAtIndex5.WoundedNumber, 0, removeDepleted: true, lastHeroIndex);
							lastHeroIndex++;
						}
						else
						{
							troopRoster3.AddToCounts(elementCopyAtIndex5.Character, elementCopyAtIndex5.Number, insertAtFront: false, elementCopyAtIndex5.WoundedNumber);
						}
					}
				}
				TextObject textObject3 = new TextObject("{=QlbkxoSp} {TOOLTIP_TROOPS} ({PARTY_SIZE})");
				textObject3.SetTextVariable("TOOLTIP_TROOPS", GameTexts.FindText("str_map_tooltip_troops"));
				textObject3.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(troopRoster3.TotalManCount - troopRoster3.TotalWounded, troopRoster3.TotalWounded, isInspected: true));
				return textObject3.ToString();
			};
		}
		if (func().Length != 0 && func2().Length != 0)
		{
			propertyBasedTooltipVM.AddProperty("", "", -1);
			propertyBasedTooltipVM.AddProperty(func, func2);
		}
		if (isExtended)
		{
			propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.DefaultSeperator);
			for (int l = 0; l < troopRoster.Count || l < troopRoster2.Count; l++)
			{
				string blankString = new TextObject("{=!} ").ToString();
				Func<string> definition = () => blankString;
				Func<string> value = () => blankString;
				if (l < troopRoster.Count)
				{
					CharacterObject character2 = troopRoster.GetElementCopyAtIndex(l).Character;
					definition = delegate
					{
						lastHeroIndex = 0;
						foreach (MapEventParty item5 in parties1)
						{
							for (int n = 0; n < item5.Party.MemberRoster.Count; n++)
							{
								TroopRosterElement elementCopyAtIndex4 = item5.Party.MemberRoster.GetElementCopyAtIndex(n);
								if (elementCopyAtIndex4.Character == character2)
								{
									TextObject textObject2;
									if (elementCopyAtIndex4.Character.IsHero)
									{
										textObject2 = new TextObject("{=W1tsTWZv} {PARTY_MEMBER.LINK} ({MEMBER_HEALTH}%)");
										textObject2.SetTextVariable("MEMBER_HEALTH", elementCopyAtIndex4.Character.HeroObject.HitPoints * 100 / elementCopyAtIndex4.Character.MaxHitPoints());
									}
									else
									{
										textObject2 = new TextObject("{=vLaBJFGy} {PARTY_MEMBER.LINK} ({PARTY_SIZE})");
										textObject2.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(elementCopyAtIndex4.Number - elementCopyAtIndex4.WoundedNumber, elementCopyAtIndex4.WoundedNumber, isInspected: true));
									}
									StringHelpers.SetCharacterProperties("PARTY_MEMBER", elementCopyAtIndex4.Character, textObject2);
									return textObject2.ToString();
								}
							}
						}
						return blankString;
					};
				}
				if (l < troopRoster2.Count)
				{
					CharacterObject character = troopRoster2.GetElementCopyAtIndex(l).Character;
					value = delegate
					{
						lastHeroIndex = 0;
						foreach (MapEventParty item6 in parties2)
						{
							for (int m = 0; m < item6.Party.MemberRoster.Count; m++)
							{
								TroopRosterElement elementCopyAtIndex3 = item6.Party.MemberRoster.GetElementCopyAtIndex(m);
								if (character == elementCopyAtIndex3.Character)
								{
									TextObject textObject;
									if (character.IsHero)
									{
										textObject = new TextObject("{=PS02CqPu} {PARTY_MEMBER.LINK} (Health: {MEMBER_HEALTH}%)");
										textObject.SetTextVariable("MEMBER_HEALTH", elementCopyAtIndex3.Character.HeroObject.HitPoints * 100 / elementCopyAtIndex3.Character.MaxHitPoints());
									}
									else
									{
										textObject = new TextObject("{=vLaBJFGy} {PARTY_MEMBER.LINK} ({PARTY_SIZE})");
										textObject.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(elementCopyAtIndex3.Number - elementCopyAtIndex3.WoundedNumber, elementCopyAtIndex3.WoundedNumber, isInspected: true));
									}
									StringHelpers.SetCharacterProperties("PARTY_MEMBER", elementCopyAtIndex3.Character, textObject);
									return textObject.ToString();
								}
							}
						}
						return blankString;
					};
				}
				propertyBasedTooltipVM.AddProperty(definition, value);
			}
		}
		propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.BattleModeOver);
	}

	private static void AddPartyTroopProperties(PropertyBasedTooltipVM propertyBasedTooltipVM, TroopRoster troopRoster, TextObject title, bool isInspected, Func<TroopRoster> funcToDoBeforeLambda = null)
	{
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		propertyBasedTooltipVM.AddProperty(title.ToString(), delegate
		{
			TroopRoster troopRoster4 = ((funcToDoBeforeLambda != null) ? funcToDoBeforeLambda() : troopRoster);
			int num3 = 0;
			int num4 = 0;
			for (int l = 0; l < troopRoster4.Count; l++)
			{
				TroopRosterElement elementCopyAtIndex5 = troopRoster4.GetElementCopyAtIndex(l);
				num3 += elementCopyAtIndex5.Number - elementCopyAtIndex5.WoundedNumber;
				num4 += elementCopyAtIndex5.WoundedNumber;
			}
			TextObject textObject5 = new TextObject("{=iXXTONWb} ({PARTY_SIZE})");
			textObject5.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(num3, num4, isInspected));
			return textObject5.ToString();
		});
		if (isInspected)
		{
			propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
		}
		if (isInspected)
		{
			Dictionary<FormationClass, Tuple<int, int>> dictionary = new Dictionary<FormationClass, Tuple<int, int>>();
			for (int i = 0; i < troopRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = troopRoster.GetElementCopyAtIndex(i);
				if (dictionary.ContainsKey(elementCopyAtIndex.Character.DefaultFormationClass))
				{
					Tuple<int, int> tuple = dictionary[elementCopyAtIndex.Character.DefaultFormationClass];
					dictionary[elementCopyAtIndex.Character.DefaultFormationClass] = new Tuple<int, int>(tuple.Item1 + elementCopyAtIndex.Number - elementCopyAtIndex.WoundedNumber, tuple.Item2 + elementCopyAtIndex.WoundedNumber);
				}
				else
				{
					dictionary.Add(elementCopyAtIndex.Character.DefaultFormationClass, new Tuple<int, int>(elementCopyAtIndex.Number - elementCopyAtIndex.WoundedNumber, elementCopyAtIndex.WoundedNumber));
				}
			}
			foreach (KeyValuePair<FormationClass, Tuple<int, int>> item in dictionary.OrderBy((KeyValuePair<FormationClass, Tuple<int, int>> x) => x.Key))
			{
				TextObject textObject = new TextObject("{=Dqydb21E} {PARTY_SIZE}");
				textObject.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(item.Value.Item1, item.Value.Item2, isInspected: true));
				TextObject textObject2 = GameTexts.FindText("str_troop_type_name", item.Key.GetName());
				propertyBasedTooltipVM.AddProperty(textObject2.ToString(), textObject.ToString());
			}
		}
		if (!(propertyBasedTooltipVM.IsExtended && isInspected))
		{
			return;
		}
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1);
		propertyBasedTooltipVM.AddProperty(GameTexts.FindText("str_troop_types").ToString(), " ");
		propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.DefaultSeperator);
		for (int j = 0; j < troopRoster.Count; j++)
		{
			TroopRosterElement elementCopyAtIndex2 = troopRoster.GetElementCopyAtIndex(j);
			if (!elementCopyAtIndex2.Character.IsHero)
			{
				continue;
			}
			CharacterObject hero = elementCopyAtIndex2.Character;
			propertyBasedTooltipVM.AddProperty(elementCopyAtIndex2.Character.Name.ToString(), delegate
			{
				TroopRoster troopRoster3 = ((funcToDoBeforeLambda != null) ? funcToDoBeforeLambda() : troopRoster);
				int num2 = troopRoster3.FindIndexOfTroop(hero);
				if (num2 == -1)
				{
					return string.Empty;
				}
				TroopRosterElement elementCopyAtIndex4 = troopRoster3.GetElementCopyAtIndex(num2);
				TextObject textObject4 = new TextObject("{=aE4ZRbB6} {HEALTH}%");
				textObject4.SetTextVariable("HEALTH", elementCopyAtIndex4.Character.HeroObject.HitPoints * 100 / elementCopyAtIndex4.Character.MaxHitPoints());
				return textObject4.ToString();
			});
		}
		for (int k = 0; k < troopRoster.Count; k++)
		{
			int index = k;
			CharacterObject character = troopRoster.GetElementCopyAtIndex(index).Character;
			if (character.IsHero)
			{
				continue;
			}
			propertyBasedTooltipVM.AddProperty(character.Name.ToString(), delegate
			{
				TroopRoster troopRoster2 = ((funcToDoBeforeLambda != null) ? funcToDoBeforeLambda() : troopRoster);
				int num = troopRoster2.FindIndexOfTroop(character);
				if (num != -1)
				{
					if (num > troopRoster2.Count)
					{
						return string.Empty;
					}
					TroopRosterElement elementCopyAtIndex3 = troopRoster2.GetElementCopyAtIndex(num);
					if (elementCopyAtIndex3.Character == null)
					{
						return string.Empty;
					}
					CharacterObject character2 = elementCopyAtIndex3.Character;
					if (character2 != null && !character2.IsHero)
					{
						TextObject textObject3 = new TextObject("{=QyVbwGLp}{PARTY_SIZE}");
						textObject3.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(elementCopyAtIndex3.Number - elementCopyAtIndex3.WoundedNumber, elementCopyAtIndex3.WoundedNumber, isInspected: true));
						return textObject3.ToString();
					}
				}
				return string.Empty;
			});
		}
	}
}
