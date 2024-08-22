using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CharacterCreationContent;

public class SandboxCharacterCreationContent : CharacterCreationContentBase
{
	protected enum SandboxAgeOptions
	{
		YoungAdult = 20,
		Adult = 30,
		MiddleAged = 40,
		Elder = 50
	}

	protected enum OccupationTypes
	{
		Artisan,
		Bard,
		Retainer,
		Merchant,
		Farmer,
		Hunter,
		Vagabond,
		Mercenary,
		Herder,
		Healer,
		NumberOfTypes
	}

	protected const int FocusToAddYouthStart = 2;

	protected const int FocusToAddAdultStart = 4;

	protected const int FocusToAddMiddleAgedStart = 6;

	protected const int FocusToAddElderlyStart = 8;

	protected const int AttributeToAddYouthStart = 1;

	protected const int AttributeToAddAdultStart = 2;

	protected const int AttributeToAddMiddleAgedStart = 3;

	protected const int AttributeToAddElderlyStart = 4;

	protected readonly Dictionary<string, Vec2> _startingPoints = new Dictionary<string, Vec2>
	{
		{
			"empire",
			new Vec2(657.95f, 279.08f)
		},
		{
			"sturgia",
			new Vec2(356.75f, 551.52f)
		},
		{
			"aserai",
			new Vec2(300.78f, 259.99f)
		},
		{
			"battania",
			new Vec2(293.64f, 446.39f)
		},
		{
			"khuzait",
			new Vec2(680.73f, 480.8f)
		},
		{
			"vlandia",
			new Vec2(207.04f, 389.04f)
		}
	};

	protected SandboxAgeOptions _startingAge = SandboxAgeOptions.YoungAdult;

	protected OccupationTypes _familyOccupationType;

	protected TextObject _educationIntroductoryText = new TextObject("{=!}{EDUCATION_INTRO}");

	protected TextObject _youthIntroductoryText = new TextObject("{=!}{YOUTH_INTRO}");

	public override TextObject ReviewPageDescription => new TextObject("{=W6pKpEoT}You prepare to set off for a grand adventure in Calradia! Here is your character. Continue if you are ready, or go back to make changes.");

	public override IEnumerable<Type> CharacterCreationStages
	{
		get
		{
			yield return typeof(CharacterCreationCultureStage);
			yield return typeof(CharacterCreationFaceGeneratorStage);
			yield return typeof(CharacterCreationGenericStage);
			yield return typeof(CharacterCreationBannerEditorStage);
			yield return typeof(CharacterCreationClanNamingStage);
			yield return typeof(CharacterCreationReviewStage);
			yield return typeof(CharacterCreationOptionsStage);
		}
	}

	protected override void OnCultureSelected()
	{
		base.SelectedTitleType = 1;
		base.SelectedParentType = 0;
		TextObject textObject = FactionHelper.GenerateClanNameforPlayer();
		Clan.PlayerClan.ChangeClanName(textObject, textObject);
	}

	public override int GetSelectedParentType()
	{
		return base.SelectedParentType;
	}

	public override void OnCharacterCreationFinalized()
	{
		CultureObject culture = CharacterObject.PlayerCharacter.Culture;
		if (_startingPoints.TryGetValue(culture.StringId, out var value))
		{
			MobileParty.MainParty.Position2D = value;
		}
		else
		{
			MobileParty.MainParty.Position2D = Campaign.Current.DefaultStartingPosition;
			Debug.FailedAssert("Selected culture is not in the dictionary!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CharacterCreationContent\\SandboxCharacterCreationContent.cs", "OnCharacterCreationFinalized", 108);
		}
		if (GameStateManager.Current.ActiveState is MapState mapState)
		{
			mapState.Handler.ResetCamera(resetDistance: true, teleportToMainParty: true);
			mapState.Handler.TeleportCameraToMainParty();
		}
		SetHeroAge((float)_startingAge);
	}

	protected override void OnInitialized(CharacterCreation characterCreation)
	{
		AddParentsMenu(characterCreation);
		AddChildhoodMenu(characterCreation);
		AddEducationMenu(characterCreation);
		AddYouthMenu(characterCreation);
		AddAdulthoodMenu(characterCreation);
		AddAgeSelectionMenu(characterCreation);
	}

	protected override void OnApplyCulture()
	{
	}

	protected void AddParentsMenu(CharacterCreation characterCreation)
	{
		CharacterCreationMenu characterCreationMenu = new CharacterCreationMenu(new TextObject("{=b4lDDcli}Family"), new TextObject("{=XgFU1pCx}You were born into a family of..."), ParentsOnInit);
		CharacterCreationCategory characterCreationCategory = characterCreationMenu.AddMenuCategory(EmpireParentsOnCondition);
		characterCreationCategory.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Polearm
		}, effectedAttribute: DefaultCharacterAttributes.Vigor, text: new TextObject("{=InN5ZZt3}A landlord's retainers"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: EmpireLandlordsRetainerOnConsequence, onApply: EmpireLandlordsRetainerOnApply, descriptionText: new TextObject("{=ivKl4mV2}Your father was a trusted lieutenant of the local landowning aristocrat. He rode with the lord's cavalry, fighting as an armored lancer."));
		characterCreationCategory.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Charm
		}, effectedAttribute: DefaultCharacterAttributes.Social, text: new TextObject("{=651FhzdR}Urban merchants"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: EmpireMerchantOnConsequence, onApply: EmpireMerchantOnApply, descriptionText: new TextObject("{=FQntPChs}Your family were merchants in one of the main cities of the Empire. They sometimes organized caravans to nearby towns, and discussed issues in the town council."));
		characterCreationCategory.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.Polearm
		}, effectedAttribute: DefaultCharacterAttributes.Endurance, text: new TextObject("{=sb4gg8Ak}Freeholders"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: EmpireFreeholderOnConsequence, onApply: EmpireFreeholderOnApply, descriptionText: new TextObject("{=09z8Q08f}Your family were small farmers with just enough land to feed themselves and make a small profit. People like them were the pillars of the imperial rural economy, as well as the backbone of the levy."));
		characterCreationCategory.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Crafting,
			DefaultSkills.Crossbow
		}, effectedAttribute: DefaultCharacterAttributes.Intelligence, text: new TextObject("{=v48N6h1t}Urban artisans"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: EmpireArtisanOnConsequence, onApply: EmpireArtisanOnApply, descriptionText: new TextObject("{=ueCm5y1C}Your family owned their own workshop in a city, making goods from raw materials brought in from the countryside. Your father played an active if minor role in the town council, and also served in the militia."));
		characterCreationCategory.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Scouting,
			DefaultSkills.Bow
		}, effectedAttribute: DefaultCharacterAttributes.Control, text: new TextObject("{=7eWmU2mF}Foresters"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: EmpireWoodsmanOnConsequence, onApply: EmpireWoodsmanOnApply, descriptionText: new TextObject("{=yRFSzSDZ}Your family lived in a village, but did not own their own land. Instead, your father supplemented paid jobs with long trips in the woods, hunting and trapping, always keeping a wary eye for the lord's game wardens."));
		characterCreationCategory.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Roguery,
			DefaultSkills.Throwing
		}, effectedAttribute: DefaultCharacterAttributes.Cunning, text: new TextObject("{=aEke8dSb}Urban vagabonds"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: EmpireVagabondOnConsequence, onApply: EmpireVagabondOnApply, descriptionText: new TextObject("{=Jvf6K7TZ}Your family numbered among the many poor migrants living in the slums that grow up outside the walls of imperial cities, making whatever money they could from a variety of odd jobs. Sometimes they did service for one of the Empire's many criminal gangs, and you had an early look at the dark side of life."));
		CharacterCreationCategory characterCreationCategory2 = characterCreationMenu.AddMenuCategory(VlandianParentsOnCondition);
		characterCreationCategory2.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Polearm
		}, effectedAttribute: DefaultCharacterAttributes.Social, text: new TextObject("{=2TptWc4m}A baron's retainers"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: VlandiaBaronsRetainerOnConsequence, onApply: VlandiaBaronsRetainerOnApply, descriptionText: new TextObject("{=0Suu1Q9q}Your father was a bailiff for a local feudal magnate. He looked after his liege's estates, resolved disputes in the village, and helped train the village levy. He rode with the lord's cavalry, fighting as an armored knight."));
		characterCreationCategory2.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Charm
		}, effectedAttribute: DefaultCharacterAttributes.Intelligence, text: new TextObject("{=651FhzdR}Urban merchants"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: VlandiaMerchantOnConsequence, onApply: VlandiaMerchantOnApply, descriptionText: new TextObject("{=qNZFkxJb}Your family were merchants in one of the main cities of the kingdom. They organized caravans to nearby towns and were active in the local merchant's guild."));
		characterCreationCategory2.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Polearm,
			DefaultSkills.Crossbow
		}, effectedAttribute: DefaultCharacterAttributes.Endurance, text: new TextObject("{=RDfXuVxT}Yeomen"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: VlandiaYeomanOnConsequence, onApply: VlandiaYeomanOnApply, descriptionText: new TextObject("{=BLZ4mdhb}Your family were small farmers with just enough land to feed themselves and make a small profit. People like them were the pillars of the kingdom's economy, as well as the backbone of the levy."));
		characterCreationCategory2.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Crafting,
			DefaultSkills.TwoHanded
		}, effectedAttribute: DefaultCharacterAttributes.Vigor, text: new TextObject("{=p2KIhGbE}Urban blacksmith"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: VlandiaBlacksmithOnConsequence, onApply: VlandiaBlacksmithOnApply, descriptionText: new TextObject("{=btsMpRcA}Your family owned a smithy in a city. Your father played an active if minor role in the town council, and also served in the militia."));
		characterCreationCategory2.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Scouting,
			DefaultSkills.Crossbow
		}, effectedAttribute: DefaultCharacterAttributes.Control, text: new TextObject("{=YcnK0Thk}Hunters"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: VlandiaHunterOnConsequence, onApply: VlandiaHunterOnApply, descriptionText: new TextObject("{=yRFSzSDZ}Your family lived in a village, but did not own their own land. Instead, your father supplemented paid jobs with long trips in the woods, hunting and trapping, always keeping a wary eye for the lord's game wardens."));
		characterCreationCategory2.AddCategoryOption(effectedSkills: new MBList<SkillObject>
		{
			DefaultSkills.Roguery,
			DefaultSkills.Crossbow
		}, effectedAttribute: DefaultCharacterAttributes.Cunning, text: new TextObject("{=ipQP6aVi}Mercenaries"), focusToAdd: FocusToAdd, skillLevelToAdd: SkillLevelToAdd, attributeLevelToAdd: AttributeLevelToAdd, optionCondition: null, onSelect: VlandiaMercenaryOnConsequence, onApply: VlandiaMercenaryOnApply, descriptionText: new TextObject("{=yYhX6JQC}Your father joined one of Vlandia's many mercenary companies, composed of men who got such a taste for war in their lord's service that they never took well to peace. Their crossbowmen were much valued across Calradia. Your mother was a camp follower, taking you along in the wake of bloody campaigns."));
		CharacterCreationCategory characterCreationCategory3 = characterCreationMenu.AddMenuCategory(SturgianParentsOnCondition);
		characterCreationCategory3.AddCategoryOption(new TextObject("{=mc78FEbA}A boyar's companions"), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.TwoHanded
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaBoyarsCompanionOnConsequence, SturgiaBoyarsCompanionOnApply, new TextObject("{=hob3WVkU}Your father was a member of a boyar's druzhina, the 'companions' that make up his retinue. He sat at his lord's table in the great hall, oversaw the boyar's estates, and stood by his side in the center of the shield wall in battle."));
		characterCreationCategory3.AddCategoryOption(new TextObject("{=HqzVBfpl}Urban traders"), new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Tactics
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaTraderOnConsequence, SturgiaTraderOnApply, new TextObject("{=bjVMtW3W}Your family were merchants who lived in one of Sturgia's great river ports, organizing the shipment of the north's bounty of furs, honey and other goods to faraway lands."));
		characterCreationCategory3.AddCategoryOption(new TextObject("{=zrpqSWSh}Free farmers"), new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.Polearm
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaFreemanOnConsequence, SturgiaFreemanOnApply, new TextObject("{=Mcd3ZyKq}Your family had just enough land to feed themselves and make a small profit. People like them were the pillars of the kingdom's economy, as well as the backbone of the levy."));
		characterCreationCategory3.AddCategoryOption(new TextObject("{=v48N6h1t}Urban artisans"), new MBList<SkillObject>
		{
			DefaultSkills.Crafting,
			DefaultSkills.OneHanded
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaArtisanOnConsequence, SturgiaArtisanOnApply, new TextObject("{=ueCm5y1C}Your family owned their own workshop in a city, making goods from raw materials brought in from the countryside. Your father played an active if minor role in the town council, and also served in the militia."));
		characterCreationCategory3.AddCategoryOption(new TextObject("{=YcnK0Thk}Hunters"), new MBList<SkillObject>
		{
			DefaultSkills.Scouting,
			DefaultSkills.Bow
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaHunterOnConsequence, SturgiaHunterOnApply, new TextObject("{=WyZ2UtFF}Your family had no taste for the authority of the boyars. They made their living deep in the woods, slashing and burning fields which they tended for a year or two before moving on. They hunted and trapped fox, hare, ermine, and other fur-bearing animals."));
		characterCreationCategory3.AddCategoryOption(new TextObject("{=TPoK3GSj}Vagabonds"), new MBList<SkillObject>
		{
			DefaultSkills.Roguery,
			DefaultSkills.Throwing
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaVagabondOnConsequence, SturgiaVagabondOnApply, new TextObject("{=2SDWhGmQ}Your family numbered among the poor migrants living in the slums that grow up outside the walls of the river cities, making whatever money they could from a variety of odd jobs. Sometimes they did services for one of the region's many criminal gangs."));
		CharacterCreationCategory characterCreationCategory4 = characterCreationMenu.AddMenuCategory(AseraiParentsOnCondition);
		characterCreationCategory4.AddCategoryOption(new TextObject("{=Sw8OxnNr}Kinsfolk of an emir"), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Throwing
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiTribesmanOnConsequence, AseraiTribesmanOnApply, new TextObject("{=MFrIHJZM}Your family was from a smaller offshoot of an emir's tribe. Your father's land gave him enough income to afford a horse but he was not quite wealthy enough to buy the armor needed to join the heavier cavalry. He fought as one of the light horsemen for which the desert is famous."));
		characterCreationCategory4.AddCategoryOption(new TextObject("{=ngFVgwDD}Warrior-slaves"), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Polearm
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiWariorSlaveOnConsequence, AseraiWariorSlaveOnApply, new TextObject("{=GsPC2MgU}Your father was part of one of the slave-bodyguards maintained by the Aserai emirs. He fought by his master's side with tribe's armored cavalry, and was freed - perhaps for an act of valor, or perhaps he paid for his freedom with his share of the spoils of battle. He then married your mother."));
		characterCreationCategory4.AddCategoryOption(new TextObject("{=651FhzdR}Urban merchants"), new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Charm
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiMerchantOnConsequence, AseraiMerchantOnApply, new TextObject("{=1zXrlaav}Your family were respected traders in an oasis town. They ran caravans across the desert, and were experts in the finer points of negotiating passage through the desert tribes' territories."));
		characterCreationCategory4.AddCategoryOption(new TextObject("{=g31pXuqi}Oasis farmers"), new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.OneHanded
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiOasisFarmerOnConsequence, AseraiOasisFarmerOnApply, new TextObject("{=5P0KqBAw}Your family tilled the soil in one of the oases of the Nahasa and tended the palm orchards that produced the desert's famous dates. Your father was a member of the main foot levy of his tribe, fighting with his kinsmen under the emir's banner."));
		characterCreationCategory4.AddCategoryOption(new TextObject("{=EEedqolz}Bedouin"), new MBList<SkillObject>
		{
			DefaultSkills.Scouting,
			DefaultSkills.Bow
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiBedouinOnConsequence, AseraiBedouinOnApply, new TextObject("{=PKhcPbBX}Your family were part of a nomadic clan, crisscrossing the wastes between wadi beds and wells to feed their herds of goats and camels on the scraggly scrubs of the Nahasa."));
		characterCreationCategory4.AddCategoryOption(new TextObject("{=tRIrbTvv}Urban back-alley thugs"), new MBList<SkillObject>
		{
			DefaultSkills.Roguery,
			DefaultSkills.Polearm
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiBackAlleyThugOnConsequence, AseraiBackAlleyThugOnApply, new TextObject("{=6bUSbsKC}Your father worked for a fitiwi, one of the strongmen who keep order in the poorer quarters of the oasis towns. He resolved disputes over land, dice and insults, imposing his authority with the fitiwi's traditional staff."));
		CharacterCreationCategory characterCreationCategory5 = characterCreationMenu.AddMenuCategory(BattanianParentsOnCondition);
		characterCreationCategory5.AddCategoryOption(new TextObject("{=GeNKQlHR}Members of the chieftain's hearthguard"), new MBList<SkillObject>
		{
			DefaultSkills.TwoHanded,
			DefaultSkills.Bow
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattaniaChieftainsHearthguardOnConsequence, BattaniaChieftainsHearthguardOnApply, new TextObject("{=LpH8SYFL}Your family were the trusted kinfolk of a Battanian chieftain, and sat at his table in his great hall. Your father assisted his chief in running the affairs of the clan and trained with the traditional weapons of the Battanian elite, the two-handed sword or falx and the bow."));
		characterCreationCategory5.AddCategoryOption(new TextObject("{=AeBzTj6w}Healers"), new MBList<SkillObject>
		{
			DefaultSkills.Medicine,
			DefaultSkills.Charm
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattaniaHealerOnConsequence, BattaniaHealerOnApply, new TextObject("{=j6py5Rv5}Your parents were healers who gathered herbs and treated the sick. As a living reservoir of Battanian tradition, they were also asked to adjudicate many disputes between the clans."));
		characterCreationCategory5.AddCategoryOption(new TextObject("{=tGEStbxb}Tribespeople"), new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.Throwing
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattaniaTribesmanOnConsequence, BattaniaTribesmanOnApply, new TextObject("{=WchH8bS2}Your family were middle-ranking members of a Battanian clan, who tilled their own land. Your father fought with the kern, the main body of his people's warriors, joining in the screaming charges for which the Battanians were famous."));
		characterCreationCategory5.AddCategoryOption(new TextObject("{=BCU6RezA}Smiths"), new MBList<SkillObject>
		{
			DefaultSkills.Crafting,
			DefaultSkills.TwoHanded
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattaniaSmithOnConsequence, BattaniaSmithOnApply, new TextObject("{=kg9YtrOg}Your family were smiths, a revered profession among the Battanians. They crafted everything from fine filigree jewelry in geometric designs to the well-balanced longswords favored by the Battanian aristocracy."));
		characterCreationCategory5.AddCategoryOption(new TextObject("{=7eWmU2mF}Foresters"), new MBList<SkillObject>
		{
			DefaultSkills.Scouting,
			DefaultSkills.Tactics
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattaniaWoodsmanOnConsequence, BattaniaWoodsmanOnApply, new TextObject("{=7jBroUUQ}Your family had little land of their own, so they earned their living from the woods, hunting and trapping. They taught you from an early age that skills like finding game trails and killing an animal with one shot could make the difference between eating and starvation."));
		characterCreationCategory5.AddCategoryOption(new TextObject("{=SpJqhEEh}Bards"), new MBList<SkillObject>
		{
			DefaultSkills.Roguery,
			DefaultSkills.Charm
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattaniaBardOnConsequence, BattaniaBardOnApply, new TextObject("{=aVzcyhhy}Your father was a bard, drifting from chieftain's hall to chieftain's hall making his living singing the praises of one Battanian aristocrat and mocking his enemies, then going to his enemy's hall and doing the reverse. You learned from him that a clever tongue could spare you  from a life toiling in the fields, if you kept your wits about you."));
		CharacterCreationCategory characterCreationCategory6 = characterCreationMenu.AddMenuCategory(KhuzaitParentsOnCondition);
		characterCreationCategory6.AddCategoryOption(new TextObject("{=FVaRDe2a}A noyan's kinsfolk"), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Polearm
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitNoyansKinsmanOnConsequence, KhuzaitNoyansKinsmanOnApply, new TextObject("{=jAs3kDXh}Your family were the trusted kinsfolk of a Khuzait noyan, and shared his meals in the chieftain's yurt. Your father assisted his chief in running the affairs of the clan and fought in the core of armored lancers in the center of the Khuzait battle line."));
		characterCreationCategory6.AddCategoryOption(new TextObject("{=TkgLEDRM}Merchants"), new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Charm
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitMerchantOnConsequence, KhuzaitMerchantOnApply, new TextObject("{=qPg3IDiq}Your family came from one of the merchant clans that dominated the cities in eastern Calradia before the Khuzait conquest. They adjusted quickly to their new masters, keeping the caravan routes running and ensuring that the tariff revenues that once went into imperial coffers now flowed to the khanate."));
		characterCreationCategory6.AddCategoryOption(new TextObject("{=tGEStbxb}Tribespeople"), new MBList<SkillObject>
		{
			DefaultSkills.Bow,
			DefaultSkills.Riding
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitTribesmanOnConsequence, KhuzaitTribesmanOnApply, new TextObject("{=URgZ4ai4}Your family were middle-ranking members of one of the Khuzait clans. He had some  herds of his own, but was not rich. When the Khuzait horde was summoned to battle, he fought with the horse archers, shooting and wheeling and wearing down the enemy before the lancers delivered the final punch."));
		characterCreationCategory6.AddCategoryOption(new TextObject("{=gQ2tAvCz}Farmers"), new MBList<SkillObject>
		{
			DefaultSkills.Polearm,
			DefaultSkills.Throwing
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitFarmerOnConsequence, KhuzaitFarmerOnApply, new TextObject("{=5QSGoRFj}Your family tilled one of the small patches of arable land in the steppes for generations. When the Khuzaits came, they ceased paying taxes to the emperor and providing conscripts for his army, and served the khan instead."));
		characterCreationCategory6.AddCategoryOption(new TextObject("{=vfhVveLW}Shamans"), new MBList<SkillObject>
		{
			DefaultSkills.Medicine,
			DefaultSkills.Charm
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitShamanOnConsequence, KhuzaitShamanOnApply, new TextObject("{=WOKNhaG2}Your family were guardians of the sacred traditions of the Khuzaits, channelling the spirits of the wilderness and of the ancestors. They tended the sick and dispensed wisdom, resolving disputes and providing practical advice."));
		characterCreationCategory6.AddCategoryOption(new TextObject("{=Xqba1Obq}Nomads"), new MBList<SkillObject>
		{
			DefaultSkills.Scouting,
			DefaultSkills.Riding
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitNomadOnConsequence, KhuzaitNomadOnApply, new TextObject("{=9aoQYpZs}Your family's clan never pledged its loyalty to the khan and never settled down, preferring to live out in the deep steppe away from his authority. They remain some of the finest trackers and scouts in the grasslands, as the ability to spot an enemy coming and move quickly is often all that protects their herds from their neighbors' predations."));
		characterCreation.AddNewMenu(characterCreationMenu);
	}

	protected void AddChildhoodMenu(CharacterCreation characterCreation)
	{
		CharacterCreationMenu characterCreationMenu = new CharacterCreationMenu(new TextObject("{=8Yiwt1z6}Early Childhood"), new TextObject("{=character_creation_content_16}As a child you were noted for..."), ChildhoodOnInit);
		CharacterCreationCategory characterCreationCategory = characterCreationMenu.AddMenuCategory();
		characterCreationCategory.AddCategoryOption(new TextObject("{=kmM68Qx4}your leadership skills."), new MBList<SkillObject>
		{
			DefaultSkills.Leadership,
			DefaultSkills.Tactics
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ChildhoodYourLeadershipSkillsOnConsequence, ChildhoodGoodLeadingOnApply, new TextObject("{=FfNwXtii}If the wolf pup gang of your early childhood had an alpha, it was definitely you. All the other kids followed your lead as you decided what to play and where to play, and led them in games and mischief."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=5HXS8HEY}your brawn."), new MBList<SkillObject>
		{
			DefaultSkills.TwoHanded,
			DefaultSkills.Throwing
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ChildhoodYourBrawnOnConsequence, ChildhoodGoodAthleticsOnApply, new TextObject("{=YKzuGc54}You were big, and other children looked to have you around in any scrap with children from a neighboring village. You pushed a plough and threw an axe like an adult."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=QrYjPUEf}your attention to detail."), new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.Bow
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ChildhoodAttentionToDetailOnConsequence, ChildhoodGoodMemoryOnApply, new TextObject("{=JUSHAPnu}You were quick on your feet and attentive to what was going on around you. Usually you could run away from trouble, though you could give a good account of yourself in a fight with other children if cornered."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=Y3UcaX74}your aptitude for numbers."), new MBList<SkillObject>
		{
			DefaultSkills.Engineering,
			DefaultSkills.Trade
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ChildhoodAptitudeForNumbersOnConsequence, ChildhoodGoodMathOnApply, new TextObject("{=DFidSjIf}Most children around you had only the most rudimentary education, but you lingered after class to study letters and mathematics. You were fascinated by the marketplace - weights and measures, tallies and accounts, the chatter about profits and losses."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=GEYzLuwb}your way with people."), new MBList<SkillObject>
		{
			DefaultSkills.Charm,
			DefaultSkills.Leadership
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ChildhoodWayWithPeopleOnConsequence, ChildhoodGoodMannersOnApply, new TextObject("{=w2TEQq26}You were always attentive to other people, good at guessing their motivations. You studied how individuals were swayed, and tried out what you learned from adults on your friends."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=MEgLE2kj}your skill with horses."), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Medicine
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ChildhoodSkillsWithHorsesOnConsequence, ChildhoodAffinityWithAnimalsOnApply, new TextObject("{=ngazFofr}You were always drawn to animals, and spent as much time as possible hanging out in the village stables. You could calm horses, and were sometimes called upon to break in new colts. You learned the basics of veterinary arts, much of which is applicable to humans as well."));
		characterCreation.AddNewMenu(characterCreationMenu);
	}

	protected void AddEducationMenu(CharacterCreation characterCreation)
	{
		CharacterCreationMenu characterCreationMenu = new CharacterCreationMenu(new TextObject("{=rcoueCmk}Adolescence"), _educationIntroductoryText, EducationOnInit);
		CharacterCreationCategory characterCreationCategory = characterCreationMenu.AddMenuCategory();
		characterCreationCategory.AddCategoryOption(new TextObject("{=RKVNvimC}herded the sheep."), new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.Throwing
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, RuralAdolescenceOnCondition, RuralAdolescenceHerderOnConsequence, RuralAdolescenceHerderOnApply, new TextObject("{=KfaqPpbK}You went with other fleet-footed youths to take the villages' sheep, goats or cattle to graze in pastures near the village. You were in charge of chasing down stray beasts, and always kept a big stone on hand to be hurled at lurking predators if necessary."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=bTKiN0hr}worked in the village smithy."), new MBList<SkillObject>
		{
			DefaultSkills.TwoHanded,
			DefaultSkills.Crafting
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, RuralAdolescenceOnCondition, RuralAdolescenceSmithyOnConsequence, RuralAdolescenceSmithyOnApply, new TextObject("{=y6j1bJTH}You were apprenticed to the local smith. You learned how to heat and forge metal, hammering for hours at a time until your muscles ached."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=tI8ZLtoA}repaired projects."), new MBList<SkillObject>
		{
			DefaultSkills.Crafting,
			DefaultSkills.Engineering
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, RuralAdolescenceOnCondition, RuralAdolescenceRepairmanOnConsequence, RuralAdolescenceRepairmanOnApply, new TextObject("{=6LFj919J}You helped dig wells, rethatch houses, and fix broken plows. You learned about the basics of construction, as well as what it takes to keep a farming community prosperous."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=TRwgSLD2}gathered herbs in the wild."), new MBList<SkillObject>
		{
			DefaultSkills.Medicine,
			DefaultSkills.Scouting
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, RuralAdolescenceOnCondition, RuralAdolescenceGathererOnConsequence, RuralAdolescenceGathererOnApply, new TextObject("{=9ks4u5cH}You were sent by the village healer up into the hills to look for useful medicinal plants. You learned which herbs healed wounds or brought down a fever, and how to find them."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=T7m7ReTq}hunted small game."), new MBList<SkillObject>
		{
			DefaultSkills.Bow,
			DefaultSkills.Tactics
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, RuralAdolescenceOnCondition, RuralAdolescenceHunterOnConsequence, RuralAdolescenceHunterOnApply, new TextObject("{=RuvSk3QT}You accompanied a local hunter as he went into the wilderness, helping him set up traps and catch small animals."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=qAbMagWq}sold product at the market."), new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Charm
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, RuralAdolescenceOnCondition, RuralAdolescenceHelperOnConsequence, RuralAdolescenceHelperOnApply, new TextObject("{=DIgsfYfz}You took your family's goods to the nearest town to sell your produce and buy supplies. It was hard work, but you enjoyed the hubbub of the marketplace."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=nOfSqRnI}at the town watch's training ground."), new MBList<SkillObject>
		{
			DefaultSkills.Crossbow,
			DefaultSkills.Tactics
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanAdolescenceOnCondition, UrbanAdolescenceWatcherOnConsequence, UrbanAdolescenceWatcherOnApply, new TextObject("{=qnqdEJOv}You watched the town's watch practice shooting and perfect their plans to defend the walls in case of a siege."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=8a6dnLd2}with the alley gangs."), new MBList<SkillObject>
		{
			DefaultSkills.Roguery,
			DefaultSkills.OneHanded
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanAdolescenceOnCondition, UrbanAdolescenceGangerOnConsequence, UrbanAdolescenceGangerOnApply, new TextObject("{=1SUTcF0J}The gang leaders who kept watch over the slums of Calradian cities were always in need of poor youth to run messages and back them up in turf wars, while thrill-seeking merchants' sons and daughters sometimes slummed it in their company as well."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=7Hv984Sf}at docks and building sites."), new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.Crafting
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanAdolescenceOnCondition, UrbanAdolescenceDockerOnConsequence, UrbanAdolescenceDockerOnApply, new TextObject("{=bhdkegZ4}All towns had their share of projects that were constantly in need of both skilled and unskilled labor. You learned how hoists and scaffolds were constructed, how planks and stones were hewn and fitted, and other skills."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=kbcwb5TH}in the markets and caravanserais."), new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Charm
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanPoorAdolescenceOnCondition, UrbanAdolescenceMarketerOnConsequence, UrbanAdolescenceMarketerOnApply, new TextObject("{=lLJh7WAT}You worked in the marketplace, selling trinkets and drinks to busy shoppers."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=kbcwb5TH}in the markets and caravanserais."), new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Charm
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanRichAdolescenceOnCondition, UrbanAdolescenceMarketerOnConsequence, UrbanAdolescenceMarketerOnApply, new TextObject("{=rmMcwSn8}You helped your family handle their business affairs, going down to the marketplace to make purchases and oversee the arrival of caravans."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=mfRbx5KE}reading and studying."), new MBList<SkillObject>
		{
			DefaultSkills.Engineering,
			DefaultSkills.Leadership
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanPoorAdolescenceOnCondition, UrbanAdolescenceTutorOnConsequence, UrbanAdolescenceDockerOnApply, new TextObject("{=elQnygal}Your family scraped up the money for a rudimentary schooling and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=etG87fB7}with your tutor."), new MBList<SkillObject>
		{
			DefaultSkills.Engineering,
			DefaultSkills.Leadership
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanRichAdolescenceOnCondition, UrbanAdolescenceTutorOnConsequence, UrbanAdolescenceDockerOnApply, new TextObject("{=hXl25avg}Your family arranged for a private tutor and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=FKpLEamz}caring for horses."), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Steward
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanRichAdolescenceOnCondition, UrbanAdolescenceHorserOnConsequence, UrbanAdolescenceDockerOnApply, new TextObject("{=Ghz90npw}Your family owned a few horses at the town stables and you took charge of their care. Many evenings you would take them out beyond the walls and gallup through the fields, racing other youth."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=vH7GtuuK}working at the stables."), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Steward
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, UrbanPoorAdolescenceOnCondition, UrbanAdolescenceHorserOnConsequence, UrbanAdolescenceDockerOnApply, new TextObject("{=csUq1RCC}You were employed as a hired hand at the town's stables. The overseers recognized that you had a knack for horses, and you were allowed to exercise them and sometimes even break in new steeds."));
		characterCreation.AddNewMenu(characterCreationMenu);
	}

	protected void AddYouthMenu(CharacterCreation characterCreation)
	{
		CharacterCreationMenu characterCreationMenu = new CharacterCreationMenu(new TextObject("{=ok8lSW6M}Youth"), _youthIntroductoryText, YouthOnInit);
		CharacterCreationCategory characterCreationCategory = characterCreationMenu.AddMenuCategory();
		characterCreationCategory.AddCategoryOption(new TextObject("{=CITG915d}joined a commander's staff."), new MBList<SkillObject>
		{
			DefaultSkills.Steward,
			DefaultSkills.Tactics
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthCommanderOnCondition, YouthCommanderOnConsequence, YouthCommanderOnApply, new TextObject("{=Ay0G3f7I}Your family arranged for you to be part of the staff of an imperial strategos. You were not given major responsibilities - mostly carrying messages and tending to his horse -- but it did give you a chance to see how campaigns were planned and men were deployed in battle."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=bhE2i6OU}served as a baron's groom."), new MBList<SkillObject>
		{
			DefaultSkills.Steward,
			DefaultSkills.Tactics
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthGroomOnCondition, YouthGroomOnConsequence, YouthGroomOnApply, new TextObject("{=iZKtGI6Y}Your family arranged for you to accompany a minor baron of the Vlandian kingdom. You were not given major responsibilities - mostly carrying messages and tending to his horse -- but it did give you a chance to see how campaigns were planned and men were deployed in battle."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=F2bgujPo}were a chieftain's servant."), new MBList<SkillObject>
		{
			DefaultSkills.Steward,
			DefaultSkills.Tactics
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthChieftainOnCondition, YouthChieftainOnConsequence, YouthChieftainOnApply, new TextObject("{=7AYJ3SjK}Your family arranged for you to accompany a chieftain of your people. You were not given major responsibilities - mostly carrying messages and tending to his horse -- but it did give you a chance to see how campaigns were planned and men were deployed in battle."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=h2KnarLL}trained with the cavalry."), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Polearm
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthCavalryOnCondition, YouthCavalryOnConsequence, YouthCavalryOnApply, new TextObject("{=7cHsIMLP}You could never have bought the equipment on your own, but you were a good enough rider so that the local lord lent you a horse and equipment. You joined the armored cavalry, training with the lance."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=zsC2t5Hb}trained with the hearth guard."), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Polearm
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthHearthGuardOnCondition, YouthHearthGuardOnConsequence, YouthHearthGuardOnApply, new TextObject("{=RmbWW6Bm}You were a big and imposing enough youth that the chief's guard allowed you to train alongside them, in preparation to join them some day."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=aTncHUfL}stood guard with the garrisons."), new MBList<SkillObject>
		{
			DefaultSkills.Crossbow,
			DefaultSkills.Engineering
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthGarrisonOnCondition, YouthGarrisonOnConsequence, YouthGarrisonOnApply, new TextObject("{=63TAYbkx}Urban troops spend much of their time guarding the town walls. Most of their training was in missile weapons, especially useful during sieges."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=aTncHUfL}stood guard with the garrisons."), new MBList<SkillObject>
		{
			DefaultSkills.Bow,
			DefaultSkills.Engineering
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthOtherGarrisonOnCondition, YouthOtherGarrisonOnConsequence, YouthOtherGarrisonOnApply, new TextObject("{=1EkEElZd}Urban troops spend much of their time guarding the town walls. Most of their training was in missile weapons."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=VlXOgIX6}rode with the scouts."), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Bow
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthOutridersOnCondition, YouthOutridersOnConsequence, YouthOutridersOnApply, new TextObject("{=888lmJqs}All of Calradia's kingdoms recognize the value of good light cavalry and horse archers, and are sure to recruit nomads and borderers with the skills to fulfill those duties. You were a good enough rider that your neighbors pitched in to buy you a small pony and a good bow so that you could fulfill their levy obligations."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=VlXOgIX6}rode with the scouts."), new MBList<SkillObject>
		{
			DefaultSkills.Riding,
			DefaultSkills.Bow
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthOtherOutridersOnCondition, YouthOtherOutridersOnConsequence, YouthOtherOutridersOnApply, new TextObject("{=sYuN6hPD}All of Calradia's kingdoms recognize the value of good light cavalry, and are sure to recruit nomads and borderers with the skills to fulfill those duties. You were a good enough rider that your neighbors pitched in to buy you a small pony and a sheaf of javelins so that you could fulfill their levy obligations."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=a8arFSra}trained with the infantry."), new MBList<SkillObject>
		{
			DefaultSkills.Polearm,
			DefaultSkills.OneHanded
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, YouthInfantryOnConsequence, YouthInfantryOnApply, new TextObject("{=afH90aNs}Levy armed with spear and shield, drawn from smallholding farmers, have always been the backbone of most armies of Calradia."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=oMbOIPc9}joined the skirmishers."), new MBList<SkillObject>
		{
			DefaultSkills.Throwing,
			DefaultSkills.OneHanded
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthSkirmisherOnCondition, YouthSkirmisherOnConsequence, YouthSkirmisherOnApply, new TextObject("{=bXAg5w19}Younger recruits, or those of a slighter build, or those too poor to buy shield and armor tend to join the skirmishers. Fighting with bow and javelin, they try to stay out of reach of the main enemy forces."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=cDWbwBwI}joined the kern."), new MBList<SkillObject>
		{
			DefaultSkills.Throwing,
			DefaultSkills.OneHanded
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthKernOnCondition, YouthKernOnConsequence, YouthKernOnApply, new TextObject("{=tTb28jyU}Many Battanians fight as kern, versatile troops who could both harass the enemy line with their javelins or join in the final screaming charge once it weakened."));
		characterCreationCategory.AddCategoryOption(new TextObject("{=GFUggps8}marched with the camp followers."), new MBList<SkillObject>
		{
			DefaultSkills.Roguery,
			DefaultSkills.Throwing
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, YouthCamperOnCondition, YouthCamperOnConsequence, YouthCamperOnApply, new TextObject("{=64rWqBLN}You avoided service with one of the main forces of your realm's armies, but followed instead in the train - the troops' wives, lovers and servants, and those who make their living by caring for, entertaining, or cheating the soldiery."));
		characterCreation.AddNewMenu(characterCreationMenu);
	}

	protected void AddAdulthoodMenu(CharacterCreation characterCreation)
	{
		MBTextManager.SetTextVariable("EXP_VALUE", SkillLevelToAdd);
		CharacterCreationMenu characterCreationMenu = new CharacterCreationMenu(new TextObject("{=MafIe9yI}Young Adulthood"), new TextObject("{=4WYY0X59}Before you set out for a life of adventure, your biggest achievement was..."), AccomplishmentOnInit);
		CharacterCreationCategory characterCreationCategory = characterCreationMenu.AddMenuCategory();
		characterCreationCategory.AddCategoryOption(new TextObject("{=8bwpVpgy}you defeated an enemy in battle."), new MBList<SkillObject>
		{
			DefaultSkills.OneHanded,
			DefaultSkills.TwoHanded
		}, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AccomplishmentDefeatedEnemyOnConsequence, AccomplishmentDefeatedEnemyOnApply, new TextObject("{=1IEroJKs}Not everyone who musters for the levy marches to war, and not everyone who goes on campaign sees action. You did both, and you also took down an enemy warrior in direct one-to-one combat, in the full view of your comrades."), new MBList<TraitObject> { DefaultTraits.Valor }, 1, 20);
		characterCreationCategory.AddCategoryOption(new TextObject("{=mP3uFbcq}you led a successful manhunt."), new MBList<SkillObject>
		{
			DefaultSkills.Tactics,
			DefaultSkills.Leadership
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentPosseOnConditions, AccomplishmentExpeditionOnConsequence, AccomplishmentExpeditionOnApply, new TextObject("{=4f5xwzX0}When your community needed to organize a posse to pursue horse thieves, you were the obvious choice. You hunted down the raiders, surrounded them and forced their surrender, and took back your stolen property."), new MBList<TraitObject> { DefaultTraits.Calculating }, 1, 10);
		characterCreationCategory.AddCategoryOption(new TextObject("{=wfbtS71d}you led a caravan."), new MBList<SkillObject>
		{
			DefaultSkills.Tactics,
			DefaultSkills.Leadership
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentMerchantOnCondition, AccomplishmentMerchantOnConsequence, AccomplishmentExpeditionOnApply, new TextObject("{=joRHKCkm}Your family needed someone trustworthy to take a caravan to a neighboring town. You organized supplies, ensured a constant watch to keep away bandits, and brought it safely to its destination."), new MBList<TraitObject> { DefaultTraits.Calculating }, 1, 10);
		characterCreationCategory.AddCategoryOption(new TextObject("{=x1HTX5hq}you saved your village from a flood."), new MBList<SkillObject>
		{
			DefaultSkills.Tactics,
			DefaultSkills.Leadership
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentSavedVillageOnCondition, AccomplishmentSavedVillageOnConsequence, AccomplishmentExpeditionOnApply, new TextObject("{=bWlmGDf3}When a sudden storm caused the local stream to rise suddenly, your neighbors needed quick-thinking leadership. You provided it, directing them to build levees to save their homes."), new MBList<TraitObject> { DefaultTraits.Calculating }, 1, 10);
		characterCreationCategory.AddCategoryOption(new TextObject("{=s8PNllPN}you saved your city quarter from a fire."), new MBList<SkillObject>
		{
			DefaultSkills.Tactics,
			DefaultSkills.Leadership
		}, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentSavedStreetOnCondition, AccomplishmentSavedStreetOnConsequence, AccomplishmentExpeditionOnApply, new TextObject("{=ZAGR6PYc}When a sudden blaze broke out in a back alley, your neighbors needed quick-thinking leadership and you provided it. You organized a bucket line to the nearest well, putting the fire out before any homes were lost."), new MBList<TraitObject> { DefaultTraits.Calculating }, 1, 10);
		characterCreationCategory.AddCategoryOption(new TextObject("{=xORjDTal}you invested some money in a workshop."), new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Crafting
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentUrbanOnCondition, AccomplishmentWorkshopOnConsequence, AccomplishmentWorkshopOnApply, new TextObject("{=PyVqDLBu}Your parents didn't give you much money, but they did leave just enough for you to secure a loan against a larger amount to build a small workshop. You paid back what you borrowed, and sold your enterprise for a profit."), new MBList<TraitObject> { DefaultTraits.Calculating }, 1, 10);
		characterCreationCategory.AddCategoryOption(new TextObject("{=xKXcqRJI}you invested some money in land."), new MBList<SkillObject>
		{
			DefaultSkills.Trade,
			DefaultSkills.Crafting
		}, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentRuralOnCondition, AccomplishmentWorkshopOnConsequence, AccomplishmentWorkshopOnApply, new TextObject("{=cbF9jdQo}Your parents didn't give you much money, but they did leave just enough for you to purchase a plot of unused land at the edge of the village. You cleared away rocks and dug an irrigation ditch, raised a few seasons of crops, than sold it for a considerable profit."), new MBList<TraitObject> { DefaultTraits.Calculating }, 1, 10);
		characterCreationCategory.AddCategoryOption(new TextObject("{=TbNRtUjb}you hunted a dangerous animal."), new MBList<SkillObject>
		{
			DefaultSkills.Polearm,
			DefaultSkills.Crossbow
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentRuralOnCondition, AccomplishmentSiegeHunterOnConsequence, AccomplishmentSiegeHunterOnApply, new TextObject("{=I3PcdaaL}Wolves, bears are a constant menace to the flocks of northern Calradia, while hyenas and leopards trouble the south. You went with a group of your fellow villagers and fired the missile that brought down the beast."), new MBList<TraitObject> { DefaultTraits.Valor }, 1, 5);
		characterCreationCategory.AddCategoryOption(new TextObject("{=WbHfGCbd}you survived a siege."), new MBList<SkillObject>
		{
			DefaultSkills.Bow,
			DefaultSkills.Crossbow
		}, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentUrbanOnCondition, AccomplishmentSiegeHunterOnConsequence, AccomplishmentSiegeHunterOnApply, new TextObject("{=FhZPjhli}Your hometown was briefly placed under siege, and you were called to defend the walls. Everyone did their part to repulse the enemy assault, and everyone is justly proud of what they endured."), null, 0, 5);
		characterCreationCategory.AddCategoryOption(new TextObject("{=kNXet6Um}you had a famous escapade in town."), new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.Roguery
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentRuralOnCondition, AccomplishmentEscapadeOnConsequence, AccomplishmentEscapadeOnApply, new TextObject("{=DjeAJtix}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, on one of your trips into town you got into the kind of trouble from which only a quick tongue or quick feet get you out alive."), new MBList<TraitObject> { DefaultTraits.Valor }, 1, 5);
		characterCreationCategory.AddCategoryOption(new TextObject("{=qlOuiKXj}you had a famous escapade."), new MBList<SkillObject>
		{
			DefaultSkills.Athletics,
			DefaultSkills.Roguery
		}, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, AccomplishmentUrbanOnCondition, AccomplishmentEscapadeOnConsequence, AccomplishmentEscapadeOnApply, new TextObject("{=lD5Ob3R4}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, you got into the kind of trouble from which only a quick tongue or quick feet get you out alive."), new MBList<TraitObject> { DefaultTraits.Valor }, 1, 5);
		characterCreationCategory.AddCategoryOption(new TextObject("{=Yqm0Dics}you treated people well."), new MBList<SkillObject>
		{
			DefaultSkills.Charm,
			DefaultSkills.Steward
		}, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AccomplishmentTreaterOnConsequence, AccomplishmentTreaterOnApply, new TextObject("{=dDmcqTzb}Yours wasn't the kind of reputation that local legends are made of, but it was the kind that wins you respect among those around you. You were consistently fair and honest in your business dealings and helpful to those in trouble. In doing so, you got a sense of what made people tick."), new MBList<TraitObject>
		{
			DefaultTraits.Mercy,
			DefaultTraits.Generosity,
			DefaultTraits.Honor
		}, 1, 5);
		characterCreation.AddNewMenu(characterCreationMenu);
	}

	protected void AddAgeSelectionMenu(CharacterCreation characterCreation)
	{
		MBTextManager.SetTextVariable("EXP_VALUE", SkillLevelToAdd);
		CharacterCreationMenu characterCreationMenu = new CharacterCreationMenu(new TextObject("{=HDFEAYDk}Starting Age"), new TextObject("{=VlOGrGSn}Your character started off on the adventuring path at the age of..."), StartingAgeOnInit);
		CharacterCreationCategory characterCreationCategory = characterCreationMenu.AddMenuCategory();
		characterCreationCategory.AddCategoryOption(new TextObject("{=!}20"), new MBList<SkillObject>(), null, 0, 0, 0, null, StartingAgeYoungOnConsequence, StartingAgeYoungOnApply, new TextObject("{=2k7adlh7}While lacking experience a bit, you are full with youthful energy, you are fully eager, for the long years of adventuring ahead."), null, 0, 0, 0, 2, 1);
		characterCreationCategory.AddCategoryOption(new TextObject("{=!}30"), new MBList<SkillObject>(), null, 0, 0, 0, null, StartingAgeAdultOnConsequence, StartingAgeAdultOnApply, new TextObject("{=NUlVFRtK}You are at your prime, You still have some youthful energy but also have a substantial amount of experience under your belt. "), null, 0, 0, 0, 4, 2);
		characterCreationCategory.AddCategoryOption(new TextObject("{=!}40"), new MBList<SkillObject>(), null, 0, 0, 0, null, StartingAgeMiddleAgedOnConsequence, StartingAgeMiddleAgedOnApply, new TextObject("{=5MxTYApM}This is the right age for starting off, you have years of experience, and you are old enough for people to respect you and gather under your banner."), null, 0, 0, 0, 6, 3);
		characterCreationCategory.AddCategoryOption(new TextObject("{=!}50"), new MBList<SkillObject>(), null, 0, 0, 0, null, StartingAgeElderlyOnConsequence, StartingAgeElderlyOnApply, new TextObject("{=ePD5Afvy}While you are past your prime, there is still enough time to go on that last big adventure for you. And you have all the experience you need to overcome anything!"), null, 0, 0, 0, 8, 4);
		characterCreation.AddNewMenu(characterCreationMenu);
	}

	protected void ParentsOnInit(CharacterCreation characterCreation)
	{
		characterCreation.IsPlayerAlone = false;
		characterCreation.HasSecondaryCharacter = false;
		ClearMountEntity(characterCreation);
		characterCreation.ClearFaceGenPrefab();
		if (base.PlayerBodyProperties != CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment))
		{
			base.PlayerBodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment);
			BodyProperties motherBodyProperties = base.PlayerBodyProperties;
			BodyProperties fatherBodyProperties = base.PlayerBodyProperties;
			FaceGen.GenerateParentKey(base.PlayerBodyProperties, CharacterObject.PlayerCharacter.Race, ref motherBodyProperties, ref fatherBodyProperties);
			motherBodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.3f, 0.2f), motherBodyProperties.StaticProperties);
			fatherBodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.5f, 0.5f), fatherBodyProperties.StaticProperties);
			base.MotherFacegenCharacter = new FaceGenChar(motherBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), isFemale: true, "anim_mother_1");
			base.FatherFacegenCharacter = new FaceGenChar(fatherBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), isFemale: false, "anim_father_1");
		}
		characterCreation.ChangeFaceGenChars(new List<FaceGenChar> { base.MotherFacegenCharacter, base.FatherFacegenCharacter });
		ChangeParentsOutfit(characterCreation);
		ChangeParentsAnimation(characterCreation);
	}

	protected void ChangeParentsOutfit(CharacterCreation characterCreation, string fatherItemId = "", string motherItemId = "", bool isLeftHandItemForFather = true, bool isLeftHandItemForMother = true)
	{
		characterCreation.ClearFaceGenPrefab();
		List<Equipment> list = new List<Equipment>();
		Equipment equipment = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("mother_char_creation_" + base.SelectedParentType + "_" + GetSelectedCulture().StringId)?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
		Equipment equipment2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("father_char_creation_" + base.SelectedParentType + "_" + GetSelectedCulture().StringId)?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
		if (motherItemId != "")
		{
			ItemObject @object = Game.Current.ObjectManager.GetObject<ItemObject>(motherItemId);
			if (@object != null)
			{
				equipment.AddEquipmentToSlotWithoutAgent((!isLeftHandItemForMother) ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(@object));
			}
			else
			{
				Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCreation.FaceGenChars[0].Race);
				characterCreation.ChangeCharacterPrefab(motherItemId, isLeftHandItemForMother ? baseMonsterFromRace.MainHandItemBoneIndex : baseMonsterFromRace.OffHandItemBoneIndex);
			}
		}
		if (fatherItemId != "")
		{
			ItemObject object2 = Game.Current.ObjectManager.GetObject<ItemObject>(fatherItemId);
			if (object2 != null)
			{
				equipment2.AddEquipmentToSlotWithoutAgent((!isLeftHandItemForFather) ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(object2));
			}
		}
		list.Add(equipment);
		list.Add(equipment2);
		characterCreation.ChangeCharactersEquipment(list);
	}

	protected void ChangeParentsAnimation(CharacterCreation characterCreation)
	{
		List<string> actionList = new List<string>
		{
			"anim_mother_" + base.SelectedParentType,
			"anim_father_" + base.SelectedParentType
		};
		characterCreation.ChangeCharsAnimation(actionList);
	}

	protected void SetParentAndOccupationType(CharacterCreation characterCreation, int parentType, OccupationTypes occupationType, string fatherItemId = "", string motherItemId = "", bool isLeftHandItemForFather = true, bool isLeftHandItemForMother = true)
	{
		base.SelectedParentType = parentType;
		_familyOccupationType = occupationType;
		characterCreation.ChangeFaceGenChars(new List<FaceGenChar> { base.MotherFacegenCharacter, base.FatherFacegenCharacter });
		ChangeParentsAnimation(characterCreation);
		ChangeParentsOutfit(characterCreation, fatherItemId, motherItemId, isLeftHandItemForFather, isLeftHandItemForMother);
	}

	protected void EmpireLandlordsRetainerOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
	}

	protected void EmpireMerchantOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
	}

	protected void EmpireFreeholderOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Farmer);
	}

	protected void EmpireArtisanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Artisan);
	}

	protected void EmpireWoodsmanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Hunter);
	}

	protected void EmpireVagabondOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
	}

	protected void EmpireLandlordsRetainerOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void EmpireMerchantOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void EmpireFreeholderOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void EmpireArtisanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void EmpireWoodsmanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void EmpireVagabondOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void VlandiaBaronsRetainerOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
	}

	protected void VlandiaMerchantOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
	}

	protected void VlandiaYeomanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Farmer);
	}

	protected void VlandiaBlacksmithOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Artisan);
	}

	protected void VlandiaHunterOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Hunter);
	}

	protected void VlandiaMercenaryOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Mercenary);
	}

	protected void VlandiaBaronsRetainerOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void VlandiaMerchantOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void VlandiaYeomanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void VlandiaBlacksmithOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void VlandiaHunterOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void VlandiaMercenaryOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void SturgiaBoyarsCompanionOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
	}

	protected void SturgiaTraderOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
	}

	protected void SturgiaFreemanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Farmer);
	}

	protected void SturgiaArtisanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Artisan);
	}

	protected void SturgiaHunterOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Hunter);
	}

	protected void SturgiaVagabondOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
	}

	protected void SturgiaBoyarsCompanionOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void SturgiaTraderOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void SturgiaFreemanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void SturgiaArtisanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void SturgiaHunterOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void SturgiaVagabondOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void AseraiTribesmanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
	}

	protected void AseraiWariorSlaveOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Mercenary);
	}

	protected void AseraiMerchantOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Merchant);
	}

	protected void AseraiOasisFarmerOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
	}

	protected void AseraiBedouinOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Herder);
	}

	protected void AseraiBackAlleyThugOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Artisan);
	}

	protected void AseraiTribesmanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void AseraiWariorSlaveOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void AseraiMerchantOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void AseraiOasisFarmerOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void AseraiBedouinOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void AseraiBackAlleyThugOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void BattaniaChieftainsHearthguardOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
	}

	protected void BattaniaHealerOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Healer);
	}

	protected void BattaniaTribesmanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Farmer);
	}

	protected void BattaniaSmithOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Artisan);
	}

	protected void BattaniaWoodsmanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Hunter);
	}

	protected void BattaniaBardOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Bard);
	}

	protected void BattaniaChieftainsHearthguardOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void BattaniaHealerOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void BattaniaTribesmanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void BattaniaSmithOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void BattaniaWoodsmanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void BattaniaBardOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void KhuzaitNoyansKinsmanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
	}

	protected void KhuzaitMerchantOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
	}

	protected void KhuzaitTribesmanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Herder);
	}

	protected void KhuzaitFarmerOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
	}

	protected void KhuzaitShamanOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Healer);
	}

	protected void KhuzaitNomadOnConsequence(CharacterCreation characterCreation)
	{
		SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Herder);
	}

	protected void KhuzaitNoyansKinsmanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void KhuzaitMerchantOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void KhuzaitTribesmanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void KhuzaitFarmerOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void KhuzaitShamanOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected void KhuzaitNomadOnApply(CharacterCreation characterCreation)
	{
		FinalizeParents();
	}

	protected bool EmpireParentsOnCondition()
	{
		return GetSelectedCulture().StringId == "empire";
	}

	protected bool VlandianParentsOnCondition()
	{
		return GetSelectedCulture().StringId == "vlandia";
	}

	protected bool SturgianParentsOnCondition()
	{
		return GetSelectedCulture().StringId == "sturgia";
	}

	protected bool AseraiParentsOnCondition()
	{
		return GetSelectedCulture().StringId == "aserai";
	}

	protected bool BattanianParentsOnCondition()
	{
		return GetSelectedCulture().StringId == "battania";
	}

	protected bool KhuzaitParentsOnCondition()
	{
		return GetSelectedCulture().StringId == "khuzait";
	}

	protected void FinalizeParents()
	{
		CharacterObject @object = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_mother");
		CharacterObject object2 = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_father");
		@object.HeroObject.ModifyPlayersFamilyAppearance(base.MotherFacegenCharacter.BodyProperties.StaticProperties);
		object2.HeroObject.ModifyPlayersFamilyAppearance(base.FatherFacegenCharacter.BodyProperties.StaticProperties);
		@object.HeroObject.Weight = base.MotherFacegenCharacter.BodyProperties.Weight;
		@object.HeroObject.Build = base.MotherFacegenCharacter.BodyProperties.Build;
		object2.HeroObject.Weight = base.FatherFacegenCharacter.BodyProperties.Weight;
		object2.HeroObject.Build = base.FatherFacegenCharacter.BodyProperties.Build;
		EquipmentHelper.AssignHeroEquipmentFromEquipment(@object.HeroObject, base.MotherFacegenCharacter.Equipment);
		EquipmentHelper.AssignHeroEquipmentFromEquipment(object2.HeroObject, base.FatherFacegenCharacter.Equipment);
		@object.Culture = Hero.MainHero.Culture;
		object2.Culture = Hero.MainHero.Culture;
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter);
		TextObject textObject = GameTexts.FindText("str_player_father_name", Hero.MainHero.Culture.StringId);
		object2.HeroObject.SetName(textObject, textObject);
		TextObject textObject2 = new TextObject("{=XmvaRfLM}{PLAYER_FATHER.NAME} was the father of {PLAYER.LINK}. He was slain when raiders attacked the inn at which his family was staying.");
		StringHelpers.SetCharacterProperties("PLAYER_FATHER", object2, textObject2);
		object2.HeroObject.EncyclopediaText = textObject2;
		TextObject textObject3 = GameTexts.FindText("str_player_mother_name", Hero.MainHero.Culture.StringId);
		@object.HeroObject.SetName(textObject3, textObject3);
		TextObject textObject4 = new TextObject("{=hrhvEWP8}{PLAYER_MOTHER.NAME} was the mother of {PLAYER.LINK}. She was slain when raiders attacked the inn at which her family was staying.");
		StringHelpers.SetCharacterProperties("PLAYER_MOTHER", @object, textObject4);
		@object.HeroObject.EncyclopediaText = textObject4;
		@object.HeroObject.UpdateHomeSettlement();
		object2.HeroObject.UpdateHomeSettlement();
		@object.HeroObject.SetHasMet();
		object2.HeroObject.SetHasMet();
	}

	protected static List<FaceGenChar> ChangePlayerFaceWithAge(float age, string actionName = "act_childhood_schooled")
	{
		List<FaceGenChar> list = new List<FaceGenChar>();
		BodyProperties originalBodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment);
		originalBodyProperties = FaceGen.GetBodyPropertiesWithAge(ref originalBodyProperties, age);
		list.Add(new FaceGenChar(originalBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), CharacterObject.PlayerCharacter.IsFemale, actionName));
		return list;
	}

	protected Equipment ChangePlayerOutfit(CharacterCreation characterCreation, string outfit)
	{
		List<Equipment> list = new List<Equipment>();
		Equipment equipment = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(outfit)?.DefaultEquipment;
		if (equipment == null)
		{
			Debug.FailedAssert("item shouldn't be null!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CharacterCreationContent\\SandboxCharacterCreationContent.cs", "ChangePlayerOutfit", 1048);
			equipment = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_default").DefaultEquipment;
		}
		list.Add(equipment);
		characterCreation.ChangeCharactersEquipment(list);
		return equipment;
	}

	protected static void ChangePlayerMount(CharacterCreation characterCreation, Hero hero)
	{
		if (hero.CharacterObject.HasMount())
		{
			FaceGenMount faceGenMount = new FaceGenMount(MountCreationKey.GetRandomMountKey(hero.CharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, hero.CharacterObject.GetMountKeySeed()), hero.CharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, hero.CharacterObject.Equipment[EquipmentIndex.HorseHarness].Item, "act_horse_stand_1");
			characterCreation.SetFaceGenMount(faceGenMount);
		}
	}

	protected static void ClearMountEntity(CharacterCreation characterCreation)
	{
		characterCreation.ClearFaceGenMounts();
	}

	protected void ChildhoodOnInit(CharacterCreation characterCreation)
	{
		characterCreation.IsPlayerAlone = true;
		characterCreation.HasSecondaryCharacter = false;
		characterCreation.ClearFaceGenPrefab();
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(ChildhoodAge));
		string text = "player_char_creation_childhood_age_" + GetSelectedCulture().StringId + "_" + base.SelectedParentType;
		text += (Hero.MainHero.IsFemale ? "_f" : "_m");
		ChangePlayerOutfit(characterCreation, text);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
		ClearMountEntity(characterCreation);
	}

	protected static void ChildhoodYourLeadershipSkillsOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_leader" });
	}

	protected static void ChildhoodYourBrawnOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
	}

	protected static void ChildhoodAttentionToDetailOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_memory" });
	}

	protected static void ChildhoodAptitudeForNumbersOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_numbers" });
	}

	protected static void ChildhoodWayWithPeopleOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
	}

	protected static void ChildhoodSkillsWithHorsesOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_animals" });
	}

	protected static void ChildhoodGoodLeadingOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void ChildhoodGoodAthleticsOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void ChildhoodGoodMemoryOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void ChildhoodGoodMathOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void ChildhoodGoodMannersOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void ChildhoodAffinityWithAnimalsOnApply(CharacterCreation characterCreation)
	{
	}

	protected void EducationOnInit(CharacterCreation characterCreation)
	{
		characterCreation.IsPlayerAlone = true;
		characterCreation.HasSecondaryCharacter = false;
		characterCreation.ClearFaceGenPrefab();
		TextObject textObject = new TextObject("{=WYvnWcXQ}Like all village children you helped out in the fields. You also...");
		TextObject textObject2 = new TextObject("{=DsCkf6Pb}Growing up, you spent most of your time...");
		_educationIntroductoryText.SetTextVariable("EDUCATION_INTRO", RuralType() ? textObject : textObject2);
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(EducationAge));
		string text = "player_char_creation_education_age_" + GetSelectedCulture().StringId + "_" + base.SelectedParentType;
		text += (Hero.MainHero.IsFemale ? "_f" : "_m");
		ChangePlayerOutfit(characterCreation, text);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
		ClearMountEntity(characterCreation);
	}

	protected bool RuralType()
	{
		if (_familyOccupationType != OccupationTypes.Retainer && _familyOccupationType != OccupationTypes.Farmer && _familyOccupationType != OccupationTypes.Hunter && _familyOccupationType != OccupationTypes.Bard && _familyOccupationType != OccupationTypes.Herder && _familyOccupationType != OccupationTypes.Vagabond && _familyOccupationType != OccupationTypes.Healer)
		{
			return _familyOccupationType == OccupationTypes.Artisan;
		}
		return true;
	}

	protected bool RichParents()
	{
		if (_familyOccupationType != OccupationTypes.Retainer)
		{
			return _familyOccupationType == OccupationTypes.Merchant;
		}
		return true;
	}

	protected bool RuralAdolescenceOnCondition()
	{
		return RuralType();
	}

	protected bool UrbanAdolescenceOnCondition()
	{
		return !RuralType();
	}

	protected bool UrbanRichAdolescenceOnCondition()
	{
		if (!RuralType())
		{
			return RichParents();
		}
		return false;
	}

	protected bool UrbanPoorAdolescenceOnCondition()
	{
		if (!RuralType())
		{
			return !RichParents();
		}
		return false;
	}

	protected void RefreshPropsAndClothing(CharacterCreation characterCreation, bool isChildhoodStage, string itemId, bool isLeftHand, string secondItemId = "")
	{
		characterCreation.ClearFaceGenPrefab();
		characterCreation.ClearCharactersEquipment();
		string text = (isChildhoodStage ? ("player_char_creation_childhood_age_" + GetSelectedCulture().StringId + "_" + base.SelectedParentType) : ("player_char_creation_education_age_" + GetSelectedCulture().StringId + "_" + base.SelectedParentType));
		text += (Hero.MainHero.IsFemale ? "_f" : "_m");
		Equipment equipment = ChangePlayerOutfit(characterCreation, text).Clone();
		if (Game.Current.ObjectManager.GetObject<ItemObject>(itemId) != null)
		{
			ItemObject @object = Game.Current.ObjectManager.GetObject<ItemObject>(itemId);
			equipment.AddEquipmentToSlotWithoutAgent((!isLeftHand) ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(@object));
			if (secondItemId != "")
			{
				@object = Game.Current.ObjectManager.GetObject<ItemObject>(secondItemId);
				equipment.AddEquipmentToSlotWithoutAgent(isLeftHand ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(@object));
			}
		}
		else
		{
			Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCreation.FaceGenChars[0].Race);
			characterCreation.ChangeCharacterPrefab(itemId, isLeftHand ? baseMonsterFromRace.MainHandItemBoneIndex : baseMonsterFromRace.OffHandItemBoneIndex);
		}
		List<Equipment> list = new List<Equipment>();
		list.Add(equipment);
		characterCreation.ChangeCharactersEquipment(list);
	}

	protected void RuralAdolescenceHerderOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_streets" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "carry_bostaff_rogue1", isLeftHand: true);
	}

	protected void RuralAdolescenceSmithyOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_militia" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "peasant_hammer_1_t1", isLeftHand: true);
	}

	protected void RuralAdolescenceRepairmanOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_grit" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "carry_hammer", isLeftHand: true);
	}

	protected void RuralAdolescenceGathererOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "_to_carry_bd_basket_a", isLeftHand: true);
	}

	protected void RuralAdolescenceHunterOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "composite_bow", isLeftHand: true);
	}

	protected void RuralAdolescenceHelperOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers_2" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "_to_carry_bd_fabric_c", isLeftHand: true);
	}

	protected void UrbanAdolescenceWatcherOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_fox" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "", isLeftHand: true);
	}

	protected void UrbanAdolescenceMarketerOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "", isLeftHand: true);
	}

	protected void UrbanAdolescenceGangerOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "", isLeftHand: true);
	}

	protected void UrbanAdolescenceDockerOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "_to_carry_bd_basket_a", isLeftHand: true);
	}

	protected void UrbanAdolescenceHorserOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers_2" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "_to_carry_bd_fabric_c", isLeftHand: true);
	}

	protected void UrbanAdolescenceTutorOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_book" });
		RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "character_creation_notebook", isLeftHand: false);
	}

	protected static void RuralAdolescenceHerderOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void RuralAdolescenceSmithyOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void RuralAdolescenceRepairmanOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void RuralAdolescenceGathererOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void RuralAdolescenceHunterOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void RuralAdolescenceHelperOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void UrbanAdolescenceWatcherOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void UrbanAdolescenceMarketerOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void UrbanAdolescenceGangerOnApply(CharacterCreation characterCreation)
	{
	}

	protected static void UrbanAdolescenceDockerOnApply(CharacterCreation characterCreation)
	{
	}

	protected void YouthOnInit(CharacterCreation characterCreation)
	{
		characterCreation.IsPlayerAlone = true;
		characterCreation.HasSecondaryCharacter = false;
		characterCreation.ClearFaceGenPrefab();
		TextObject textObject = new TextObject("{=F7OO5SAa}As a youngster growing up in Calradia, war was never too far away. You...");
		TextObject textObject2 = new TextObject("{=5kbeAC7k}In wartorn Calradia, especially in frontier or tribal areas, some women as well as men learn to fight from an early age. You...");
		_youthIntroductoryText.SetTextVariable("YOUTH_INTRO", CharacterObject.PlayerCharacter.IsFemale ? textObject2 : textObject);
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(YouthAge));
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
		if (base.SelectedTitleType < 1 || base.SelectedTitleType > 10)
		{
			base.SelectedTitleType = 1;
		}
		RefreshPlayerAppearance(characterCreation);
	}

	protected void RefreshPlayerAppearance(CharacterCreation characterCreation)
	{
		string text = "player_char_creation_" + GetSelectedCulture().StringId + "_" + base.SelectedTitleType;
		text += (Hero.MainHero.IsFemale ? "_f" : "_m");
		ChangePlayerOutfit(characterCreation, text);
		ApplyEquipments(characterCreation);
	}

	protected bool YouthCommanderOnCondition()
	{
		if (GetSelectedCulture().StringId == "empire")
		{
			return _familyOccupationType == OccupationTypes.Retainer;
		}
		return false;
	}

	protected void YouthCommanderOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthGroomOnCondition()
	{
		if (GetSelectedCulture().StringId == "vlandia")
		{
			return _familyOccupationType == OccupationTypes.Retainer;
		}
		return false;
	}

	protected void YouthCommanderOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 10;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_decisive" });
	}

	protected void YouthGroomOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 10;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
	}

	protected void YouthChieftainOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 10;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_ready" });
	}

	protected void YouthCavalryOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 9;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_apprentice" });
	}

	protected void YouthHearthGuardOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 9;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
	}

	protected void YouthOutridersOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 2;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_gracious" });
	}

	protected void YouthOtherOutridersOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 2;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_gracious" });
	}

	protected void YouthInfantryOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 3;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_fierce" });
	}

	protected void YouthSkirmisherOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 4;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_fox" });
	}

	protected void YouthGarrisonOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 1;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_vibrant" });
	}

	protected void YouthOtherGarrisonOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 1;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
	}

	protected void YouthKernOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 8;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_apprentice" });
	}

	protected void YouthCamperOnConsequence(CharacterCreation characterCreation)
	{
		base.SelectedTitleType = 5;
		RefreshPlayerAppearance(characterCreation);
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_militia" });
	}

	protected void YouthGroomOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthChieftainOnCondition()
	{
		if (GetSelectedCulture().StringId == "battania" || GetSelectedCulture().StringId == "khuzait")
		{
			return _familyOccupationType == OccupationTypes.Retainer;
		}
		return false;
	}

	protected void YouthChieftainOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthCavalryOnCondition()
	{
		if (!(GetSelectedCulture().StringId == "empire") && !(GetSelectedCulture().StringId == "khuzait") && !(GetSelectedCulture().StringId == "aserai"))
		{
			return GetSelectedCulture().StringId == "vlandia";
		}
		return true;
	}

	protected void YouthCavalryOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthHearthGuardOnCondition()
	{
		if (!(GetSelectedCulture().StringId == "sturgia"))
		{
			return GetSelectedCulture().StringId == "battania";
		}
		return true;
	}

	protected void YouthHearthGuardOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthOutridersOnCondition()
	{
		if (!(GetSelectedCulture().StringId == "empire"))
		{
			return GetSelectedCulture().StringId == "khuzait";
		}
		return true;
	}

	protected void YouthOutridersOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthOtherOutridersOnCondition()
	{
		if (GetSelectedCulture().StringId != "empire")
		{
			return GetSelectedCulture().StringId != "khuzait";
		}
		return false;
	}

	protected void YouthOtherOutridersOnApply(CharacterCreation characterCreation)
	{
	}

	protected void YouthInfantryOnApply(CharacterCreation characterCreation)
	{
	}

	protected void YouthSkirmisherOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthGarrisonOnCondition()
	{
		if (!(GetSelectedCulture().StringId == "empire"))
		{
			return GetSelectedCulture().StringId == "vlandia";
		}
		return true;
	}

	protected void YouthGarrisonOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthOtherGarrisonOnCondition()
	{
		if (GetSelectedCulture().StringId != "empire")
		{
			return GetSelectedCulture().StringId != "vlandia";
		}
		return false;
	}

	protected void YouthOtherGarrisonOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthSkirmisherOnCondition()
	{
		return GetSelectedCulture().StringId != "battania";
	}

	protected bool YouthKernOnCondition()
	{
		return GetSelectedCulture().StringId == "battania";
	}

	protected void YouthKernOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool YouthCamperOnCondition()
	{
		return _familyOccupationType != OccupationTypes.Retainer;
	}

	protected void YouthCamperOnApply(CharacterCreation characterCreation)
	{
	}

	protected void AccomplishmentOnInit(CharacterCreation characterCreation)
	{
		characterCreation.IsPlayerAlone = true;
		characterCreation.HasSecondaryCharacter = false;
		characterCreation.ClearFaceGenPrefab();
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(AccomplishmentAge));
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
		RefreshPlayerAppearance(characterCreation);
	}

	protected void AccomplishmentDefeatedEnemyOnApply(CharacterCreation characterCreation)
	{
	}

	protected void AccomplishmentExpeditionOnApply(CharacterCreation characterCreation)
	{
	}

	protected bool AccomplishmentRuralOnCondition()
	{
		return RuralType();
	}

	protected bool AccomplishmentMerchantOnCondition()
	{
		return _familyOccupationType == OccupationTypes.Merchant;
	}

	protected bool AccomplishmentPosseOnConditions()
	{
		if (_familyOccupationType != OccupationTypes.Retainer && _familyOccupationType != OccupationTypes.Herder)
		{
			return _familyOccupationType == OccupationTypes.Mercenary;
		}
		return true;
	}

	protected bool AccomplishmentSavedVillageOnCondition()
	{
		if (RuralType() && _familyOccupationType != OccupationTypes.Retainer)
		{
			return _familyOccupationType != OccupationTypes.Herder;
		}
		return false;
	}

	protected bool AccomplishmentSavedStreetOnCondition()
	{
		if (!RuralType() && _familyOccupationType != OccupationTypes.Merchant)
		{
			return _familyOccupationType != OccupationTypes.Mercenary;
		}
		return false;
	}

	protected bool AccomplishmentUrbanOnCondition()
	{
		return !RuralType();
	}

	protected void AccomplishmentWorkshopOnApply(CharacterCreation characterCreation)
	{
	}

	protected void AccomplishmentSiegeHunterOnApply(CharacterCreation characterCreation)
	{
	}

	protected void AccomplishmentEscapadeOnApply(CharacterCreation characterCreation)
	{
	}

	protected void AccomplishmentTreaterOnApply(CharacterCreation characterCreation)
	{
	}

	protected void AccomplishmentDefeatedEnemyOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
	}

	protected void AccomplishmentExpeditionOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_gracious" });
	}

	protected void AccomplishmentMerchantOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_ready" });
	}

	protected void AccomplishmentSavedVillageOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_vibrant" });
	}

	protected void AccomplishmentSavedStreetOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_vibrant" });
	}

	protected void AccomplishmentWorkshopOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_decisive" });
	}

	protected void AccomplishmentSiegeHunterOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_tough" });
	}

	protected void AccomplishmentEscapadeOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_clever" });
	}

	protected void AccomplishmentTreaterOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
	}

	protected void StartingAgeOnInit(CharacterCreation characterCreation)
	{
		characterCreation.IsPlayerAlone = true;
		characterCreation.HasSecondaryCharacter = false;
		characterCreation.ClearFaceGenPrefab();
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge((float)_startingAge));
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
		RefreshPlayerAppearance(characterCreation);
	}

	protected void StartingAgeYoungOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ClearFaceGenPrefab();
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(20f));
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_focus" });
		RefreshPlayerAppearance(characterCreation);
		_startingAge = SandboxAgeOptions.YoungAdult;
		SetHeroAge(20f);
	}

	protected void StartingAgeAdultOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ClearFaceGenPrefab();
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(30f));
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_ready" });
		RefreshPlayerAppearance(characterCreation);
		_startingAge = SandboxAgeOptions.Adult;
		SetHeroAge(30f);
	}

	protected void StartingAgeMiddleAgedOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ClearFaceGenPrefab();
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(40f));
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
		RefreshPlayerAppearance(characterCreation);
		_startingAge = SandboxAgeOptions.MiddleAged;
		SetHeroAge(40f);
	}

	protected void StartingAgeElderlyOnConsequence(CharacterCreation characterCreation)
	{
		characterCreation.ClearFaceGenPrefab();
		characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(50f));
		characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_tough" });
		RefreshPlayerAppearance(characterCreation);
		_startingAge = SandboxAgeOptions.Elder;
		SetHeroAge(50f);
	}

	protected void StartingAgeYoungOnApply(CharacterCreation characterCreation)
	{
		_startingAge = SandboxAgeOptions.YoungAdult;
	}

	protected void StartingAgeAdultOnApply(CharacterCreation characterCreation)
	{
		_startingAge = SandboxAgeOptions.Adult;
	}

	protected void StartingAgeMiddleAgedOnApply(CharacterCreation characterCreation)
	{
		_startingAge = SandboxAgeOptions.MiddleAged;
	}

	protected void StartingAgeElderlyOnApply(CharacterCreation characterCreation)
	{
		_startingAge = SandboxAgeOptions.Elder;
	}

	protected void ApplyEquipments(CharacterCreation characterCreation)
	{
		ClearMountEntity(characterCreation);
		string text = "player_char_creation_" + GetSelectedCulture().StringId + "_" + base.SelectedTitleType;
		text += (Hero.MainHero.IsFemale ? "_f" : "_m");
		MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(text);
		base.PlayerStartEquipment = @object?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
		base.PlayerCivilianEquipment = @object?.GetCivilianEquipments().FirstOrDefault() ?? MBEquipmentRoster.EmptyEquipment;
		if (base.PlayerStartEquipment != null && base.PlayerCivilianEquipment != null)
		{
			CharacterObject.PlayerCharacter.Equipment.FillFrom(base.PlayerStartEquipment);
			CharacterObject.PlayerCharacter.FirstCivilianEquipment.FillFrom(base.PlayerCivilianEquipment);
		}
		ChangePlayerMount(characterCreation, Hero.MainHero);
	}

	protected void SetHeroAge(float age)
	{
		Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(0f - age));
	}
}
