using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace TaleWorlds.CampaignSystem;

public sealed class CharacterObject : BasicCharacterObject, ICharacterData
{
	private CharacterRestrictionFlags _characterRestrictionFlags;

	[SaveableField(101)]
	private Hero _heroObject;

	[SaveableField(103)]
	private CharacterObject _originCharacter;

	private TraitObject _persona;

	private CharacterTraits _characterTraits;

	private CharacterObject _civilianEquipmentTemplate;

	private CharacterObject _battleEquipmentTemplate;

	private Occupation _occupation;

	public override TextObject Name
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.Name;
			}
			return base.Name;
		}
	}

	public string EncyclopediaLink
	{
		get
		{
			if (!IsHero)
			{
				return Campaign.Current.EncyclopediaManager.GetIdentifier(typeof(CharacterObject)) + "-" + base.StringId;
			}
			return _heroObject.EncyclopediaLink;
		}
	}

	public TextObject EncyclopediaLinkWithName
	{
		get
		{
			if (IsHero)
			{
				return _heroObject.EncyclopediaLinkWithName;
			}
			if (Campaign.Current.EncyclopediaManager.GetPageOf(typeof(CharacterObject)).IsValidEncyclopediaItem(this))
			{
				return HyperlinkTexts.GetUnitHyperlinkText(EncyclopediaLink, Name);
			}
			return Name;
		}
	}

	public bool HiddenInEncylopedia { get; set; }

	public bool IsNotTransferableInPartyScreen => (_characterRestrictionFlags & CharacterRestrictionFlags.NotTransferableInPartyScreen) == CharacterRestrictionFlags.NotTransferableInPartyScreen;

	public bool IsNotTransferableInHideouts => (_characterRestrictionFlags & CharacterRestrictionFlags.CanNotGoInHideout) == CharacterRestrictionFlags.CanNotGoInHideout;

	public CharacterObject OriginalCharacter => _originCharacter;

	public bool IsOriginalCharacter => _originCharacter == null;

	public Hero HeroObject
	{
		get
		{
			return _heroObject;
		}
		internal set
		{
			_heroObject = value;
		}
	}

	public override MBReadOnlyList<Equipment> AllEquipments
	{
		get
		{
			if (!IsHero)
			{
				return base.AllEquipments;
			}
			return new MBList<Equipment> { HeroObject.BattleEquipment, HeroObject.CivilianEquipment };
		}
	}

	public override Equipment Equipment
	{
		get
		{
			if (!IsHero)
			{
				return base.Equipment;
			}
			return HeroObject.BattleEquipment;
		}
	}

	public IEnumerable<Equipment> BattleEquipments
	{
		get
		{
			if (!IsHero)
			{
				return AllEquipments.WhereQ((Equipment e) => !e.IsCivilian);
			}
			return new List<Equipment> { HeroObject.BattleEquipment }.AsEnumerable();
		}
	}

	public IEnumerable<Equipment> CivilianEquipments
	{
		get
		{
			if (IsHero)
			{
				return new List<Equipment> { HeroObject.CivilianEquipment }.AsEnumerable();
			}
			return AllEquipments.WhereQ((Equipment e) => e.IsCivilian);
		}
	}

	public Equipment FirstBattleEquipment
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.BattleEquipment;
			}
			return AllEquipments.FirstOrDefaultQ((Equipment e) => !e.IsCivilian);
		}
	}

	public Equipment FirstCivilianEquipment
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.CivilianEquipment;
			}
			return AllEquipments.FirstOrDefaultQ((Equipment e) => e.IsCivilian);
		}
	}

	public Equipment RandomBattleEquipment
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.BattleEquipment;
			}
			return AllEquipments.GetRandomElementWithPredicate((Equipment e) => !e.IsCivilian);
		}
	}

	public Equipment RandomCivilianEquipment
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.CivilianEquipment;
			}
			return AllEquipments.GetRandomElementWithPredicate((Equipment e) => e.IsCivilian);
		}
	}

	public override int HitPoints
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.HitPoints;
			}
			return MaxHitPoints();
		}
	}

	public override string HairTags
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.HairTags;
			}
			return base.HairTags;
		}
	}

	public override string BeardTags
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.BeardTags;
			}
			return base.BeardTags;
		}
	}

	public override string TattooTags
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.TattooTags;
			}
			return base.TattooTags;
		}
	}

	public ExplainedNumber MaxHitPointsExplanation => Campaign.Current.Models.CharacterStatsModel.MaxHitpoints(this, includeDescriptions: true);

	public override int Level
	{
		get
		{
			if (!IsHero)
			{
				return base.Level;
			}
			return HeroObject.Level;
		}
	}

	public static CharacterObject PlayerCharacter => Game.Current.PlayerTroop as CharacterObject;

	public static CharacterObject OneToOneConversationCharacter => Campaign.Current.ConversationManager.OneToOneConversationCharacter;

	public static IEnumerable<CharacterObject> ConversationCharacters => Campaign.Current.ConversationManager.ConversationCharacters;

	public new CultureObject Culture
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.Culture;
			}
			return (CultureObject)base.Culture;
		}
		set
		{
			if (IsHero)
			{
				HeroObject.Culture = value;
			}
			else
			{
				base.Culture = value;
			}
		}
	}

	public override bool IsFemale
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.IsFemale;
			}
			return base.IsFemale;
		}
	}

	public bool IsBasicTroop { get; set; }

	public bool IsTemplate { get; private set; }

	public bool IsChildTemplate { get; private set; }

	public override bool IsPlayerCharacter => PlayerCharacter == this;

	public override bool IsHero => _heroObject != null;

	public bool IsRegular => _heroObject == null;

	public Occupation Occupation
	{
		get
		{
			if (IsHero)
			{
				return HeroObject.Occupation;
			}
			return _occupation;
		}
	}

	public override float Age => HeroObject?.Age ?? base.Age;

	public int ConformityNeededToRecruitPrisoner => Campaign.Current.Models.PrisonerRecruitmentCalculationModel.GetConformityNeededToRecruitPrisoner(this);

	public CharacterObject[] UpgradeTargets { get; private set; } = new CharacterObject[0];


	public ItemCategory UpgradeRequiresItemFromCategory { get; private set; }

	public override bool IsMounted
	{
		get
		{
			if (IsHero)
			{
				return Equipment[10].Item != null;
			}
			return base.IsMounted;
		}
	}

	public override bool IsRanged
	{
		get
		{
			if (IsHero)
			{
				for (int i = 0; i < 4; i++)
				{
					ItemObject item = Equipment[i].Item;
					if (item != null && (item.ItemType == ItemObject.ItemTypeEnum.Bow || item.ItemType == ItemObject.ItemTypeEnum.Crossbow))
					{
						return true;
					}
				}
			}
			return base.IsRanged;
		}
	}

	public int TroopWage
	{
		get
		{
			if (IsHero)
			{
				return 2 + Level * 2;
			}
			return Campaign.Current.Models.PartyWageModel.GetCharacterWage(this);
		}
	}

	public int Tier => Campaign.Current.Models.CharacterStatsModel.GetTier(this);

	public static MBReadOnlyList<CharacterObject> All => Campaign.Current.Characters;

	internal static void AutoGeneratedStaticCollectObjectsCharacterObject(object o, List<object> collectedObjects)
	{
		((CharacterObject)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_heroObject);
		collectedObjects.Add(_originCharacter);
	}

	internal static object AutoGeneratedGetMemberValue_heroObject(object o)
	{
		return ((CharacterObject)o)._heroObject;
	}

	internal static object AutoGeneratedGetMemberValue_originCharacter(object o)
	{
		return ((CharacterObject)o)._originCharacter;
	}

	public override string ToString()
	{
		return Name.ToString();
	}

	public override int MaxHitPoints()
	{
		return TaleWorlds.Library.MathF.Round(Campaign.Current.Models.CharacterStatsModel.MaxHitpoints(this).ResultNumber);
	}

	public CharacterObject()
	{
		Init();
	}

	[LoadInitializationCallback]
	private void OnLoad(MetaData metaData, ObjectLoadData objectLoadData)
	{
		Init();
	}

	private void Init()
	{
		_occupation = Occupation.NotAssigned;
		_characterTraits = new CharacterTraits();
		Level = 1;
		_characterRestrictionFlags = CharacterRestrictionFlags.None;
	}

	public static CharacterObject CreateFrom(CharacterObject character)
	{
		CharacterObject characterObject = MBObjectManager.Instance.CreateObject<CharacterObject>();
		characterObject._originCharacter = character._originCharacter ?? character;
		if (characterObject.IsHero)
		{
			characterObject.HeroObject.StaticBodyProperties = (character.IsHero ? character.HeroObject.StaticBodyProperties : character.GetBodyPropertiesMin().StaticProperties);
		}
		characterObject._occupation = character._occupation;
		characterObject._persona = character._persona;
		characterObject._characterTraits = new CharacterTraits(character._characterTraits);
		characterObject._civilianEquipmentTemplate = character._civilianEquipmentTemplate;
		characterObject._battleEquipmentTemplate = character._battleEquipmentTemplate;
		characterObject.FillFrom(character);
		return characterObject;
	}

	public override void AfterRegister()
	{
		base.AfterRegister();
		if (Equipment != null)
		{
			Equipment.SyncEquipments = true;
		}
		if (FirstCivilianEquipment != null)
		{
			FirstCivilianEquipment.SyncEquipments = true;
		}
	}

	public override BodyProperties GetBodyPropertiesMin(bool returnBaseValue = false)
	{
		if (IsHero && !returnBaseValue)
		{
			return HeroObject.BodyProperties;
		}
		return base.GetBodyPropertiesMin();
	}

	public override BodyProperties GetBodyPropertiesMax()
	{
		if (IsHero)
		{
			return HeroObject.BodyProperties;
		}
		return base.GetBodyPropertiesMax();
	}

	public override void UpdatePlayerCharacterBodyProperties(BodyProperties properties, int race, bool isFemale)
	{
		if (IsPlayerCharacter && IsHero)
		{
			HeroObject.StaticBodyProperties = properties.StaticProperties;
			HeroObject.Weight = properties.Weight;
			HeroObject.Build = properties.Build;
			base.Race = race;
			HeroObject.UpdatePlayerGender(isFemale);
			CampaignEventDispatcher.Instance.OnPlayerBodyPropertiesChanged();
		}
	}

	public Occupation GetDefaultOccupation()
	{
		return _occupation;
	}

	public bool HasThrowingWeapon()
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			ItemObject item = Equipment[equipmentIndex].Item;
			if (item != null && item.Type == ItemObject.ItemTypeEnum.Thrown)
			{
				return true;
			}
		}
		return false;
	}

	public int GetUpgradeXpCost(PartyBase party, int index)
	{
		CharacterObject upgradeTarget = null;
		if (index >= 0 && index < UpgradeTargets.Length)
		{
			upgradeTarget = UpgradeTargets[index];
		}
		return Campaign.Current.Models.PartyTroopUpgradeModel.GetXpCostForUpgrade(party, this, upgradeTarget);
	}

	public int GetUpgradeGoldCost(PartyBase party, int index)
	{
		return Campaign.Current.Models.PartyTroopUpgradeModel.GetGoldCostForUpgrade(party, this, UpgradeTargets[index]);
	}

	public void InitializeHeroCharacterOnAfterLoad()
	{
		InitializeHeroBasicCharacterOnAfterLoad(_originCharacter);
		_occupation = _originCharacter._occupation;
		IsChildTemplate = _originCharacter.IsChildTemplate;
		_basicName = _originCharacter._basicName;
		UpgradeTargets = _originCharacter.UpgradeTargets;
		IsBasicTroop = _originCharacter.IsBasicTroop;
		UpgradeRequiresItemFromCategory = _originCharacter.UpgradeRequiresItemFromCategory;
		_civilianEquipmentTemplate = _originCharacter._civilianEquipmentTemplate;
		_battleEquipmentTemplate = _originCharacter._battleEquipmentTemplate;
		_persona = _originCharacter._persona;
		_characterTraits = _originCharacter._characterTraits;
		DefaultCharacterSkills = _originCharacter.DefaultCharacterSkills;
		base.IsReady = true;
	}

	public override void Deserialize(MBObjectManager objectManager, XmlNode node)
	{
		base.Deserialize(objectManager, node);
		XmlNode xmlNode = node.Attributes["occupation"];
		if (xmlNode != null)
		{
			_occupation = (Occupation)Enum.Parse(typeof(Occupation), xmlNode.InnerText);
		}
		XmlNode xmlNode2 = node.Attributes["is_template"];
		IsTemplate = xmlNode2 != null && Convert.ToBoolean(xmlNode2.InnerText);
		XmlNode xmlNode3 = node.Attributes["is_child_template"];
		IsChildTemplate = xmlNode3 != null && Convert.ToBoolean(xmlNode3.InnerText);
		XmlNode xmlNode4 = node.Attributes["is_hidden_encyclopedia"];
		HiddenInEncylopedia = xmlNode4 != null && Convert.ToBoolean(xmlNode4.InnerText);
		List<CharacterObject> list = new List<CharacterObject>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "Traits")
			{
				_characterTraits.Deserialize(objectManager, childNode);
			}
			else
			{
				if (!(childNode.Name == "upgrade_targets"))
				{
					continue;
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "upgrade_target")
					{
						CharacterObject item = objectManager.ReadObjectReferenceFromXml("id", typeof(CharacterObject), childNode2) as CharacterObject;
						list.Add(item);
					}
				}
			}
		}
		UpgradeTargets = list.ToArray();
		XmlNode xmlNode7 = node.Attributes["voice"];
		if (xmlNode7 != null)
		{
			_persona = MBObjectManager.Instance.GetObject<TraitObject>(xmlNode7.Value);
		}
		XmlNode xmlNode8 = node.Attributes["is_basic_troop"];
		if (xmlNode8 != null)
		{
			IsBasicTroop = Convert.ToBoolean(xmlNode8.InnerText);
		}
		else
		{
			IsBasicTroop = false;
		}
		UpgradeRequiresItemFromCategory = objectManager.ReadObjectReferenceFromXml<ItemCategory>("upgrade_requires", node);
		XmlNode xmlNode9 = node.Attributes["level"];
		Level = ((xmlNode9 == null) ? 1 : Convert.ToInt32(xmlNode9.InnerText));
		if (node.Attributes["civilianTemplate"] != null)
		{
			_civilianEquipmentTemplate = objectManager.ReadObjectReferenceFromXml("civilianTemplate", typeof(CharacterObject), node) as CharacterObject;
		}
		if (node.Attributes["battleTemplate"] != null)
		{
			_battleEquipmentTemplate = objectManager.ReadObjectReferenceFromXml("battleTemplate", typeof(CharacterObject), node) as CharacterObject;
		}
		_originCharacter = null;
	}

	public override float GetPower()
	{
		return GetPowerImp(IsHero ? (HeroObject.Level / 4 + 1) : Tier, IsHero, IsMounted);
	}

	public override float GetBattlePower()
	{
		return TaleWorlds.Library.MathF.Max(1f + 0.5f * (GetPower() - GetPowerImp(0)), 1f);
	}

	public override float GetMoraleResistance()
	{
		int num = (IsHero ? (HeroObject.Level / 4 + 1) : Tier);
		return (IsHero ? 1.5f : 1f) * (0.5f * (float)num + 1f);
	}

	public void GetSimulationAttackPower(out float attackPoints, out float defencePoints, Equipment equipment = null)
	{
		if (equipment == null)
		{
			equipment = Equipment;
		}
		attackPoints = 0f;
		defencePoints = 0f;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		num2 = equipment.GetArmArmorSum() + equipment.GetHeadArmorSum() + equipment.GetHumanBodyArmorSum() + equipment.GetLegArmorSum();
		num2 = num2 * num2 / equipment.GetTotalWeightOfArmor(forHuman: true);
		defencePoints += num2 * 10f + 4000f;
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			EquipmentElement equipmentElement = equipment[equipmentIndex];
			if (equipmentElement.IsEmpty)
			{
				continue;
			}
			float num4 = ((equipmentElement.Item.RelevantSkill == null) ? 1f : (0.3f + (float)GetSkillValue(equipmentElement.Item.RelevantSkill) / 300f * 0.7f));
			float num5 = num4 * equipmentElement.Item.Effectiveness;
			if (equipmentElement.Item.PrimaryWeapon.IsRangedWeapon)
			{
				for (EquipmentIndex equipmentIndex2 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex2 < EquipmentIndex.NumAllWeaponSlots; equipmentIndex2++)
				{
					EquipmentElement equipmentElement2 = equipment[equipmentIndex];
					if (equipmentIndex != equipmentIndex2 && !equipmentElement2.IsEmpty && equipmentElement2.Item.PrimaryWeapon.IsAmmo)
					{
						num5 += num4 * equipmentElement2.Item.Effectiveness;
						break;
					}
				}
			}
			if (equipmentElement.Item.PrimaryWeapon.IsShield)
			{
				defencePoints += num5 * 10f;
			}
			else
			{
				num = TaleWorlds.Library.MathF.Max(num, num5);
			}
		}
		attackPoints += num;
		for (EquipmentIndex equipmentIndex3 = EquipmentIndex.ArmorItemEndSlot; equipmentIndex3 <= EquipmentIndex.HorseHarness; equipmentIndex3++)
		{
			EquipmentElement equipmentElement3 = equipment[equipmentIndex3];
			if (!equipmentElement3.IsEmpty)
			{
				num3 += equipmentElement3.Item.Effectiveness;
			}
		}
		float num6 = ((equipment.Horse.Item == null || equipment.Horse.Item.RelevantSkill == null) ? 1f : (0.3f + (float)GetSkillValue(equipment.Horse.Item.RelevantSkill) / 300f * 0.7f));
		num3 *= num6;
		attackPoints += num3 * 2.5f;
		defencePoints += num3 * 5f;
	}

	public float GetHeadArmorSum(bool civilianEquipment = false)
	{
		if (!civilianEquipment)
		{
			return FirstBattleEquipment.GetHeadArmorSum();
		}
		return FirstCivilianEquipment.GetHeadArmorSum();
	}

	public float GetBodyArmorSum(bool civilianEquipment = false)
	{
		if (!civilianEquipment)
		{
			return FirstBattleEquipment.GetHumanBodyArmorSum();
		}
		return FirstCivilianEquipment.GetHumanBodyArmorSum();
	}

	public float GetLegArmorSum(bool civilianEquipment = false)
	{
		if (!civilianEquipment)
		{
			return FirstBattleEquipment.GetLegArmorSum();
		}
		return FirstCivilianEquipment.GetLegArmorSum();
	}

	public float GetArmArmorSum(bool civilianEquipment = false)
	{
		if (!civilianEquipment)
		{
			return FirstBattleEquipment.GetArmArmorSum();
		}
		return FirstCivilianEquipment.GetArmArmorSum();
	}

	public float GetHorseArmorSum(bool civilianEquipment = false)
	{
		if (!civilianEquipment)
		{
			return FirstBattleEquipment.GetHorseArmorSum();
		}
		return FirstCivilianEquipment.GetHorseArmorSum();
	}

	public float GetTotalArmorSum(bool civilianEquipment = false)
	{
		return GetHeadArmorSum(civilianEquipment) + GetBodyArmorSum(civilianEquipment) + GetLegArmorSum(civilianEquipment) + GetArmArmorSum(civilianEquipment);
	}

	public override BodyProperties GetBodyProperties(Equipment equipment, int seed = -1)
	{
		if (IsHero)
		{
			return HeroObject.BodyProperties;
		}
		switch (seed)
		{
		case -2:
			return GetBodyPropertiesMin();
		case -1:
			seed = base.StringId.GetDeterministicHashCode();
			break;
		}
		return FaceGen.GetRandomBodyProperties(base.Race, IsFemale, GetBodyPropertiesMin(), GetBodyPropertiesMax(), (int)(equipment?.HairCoverType ?? ArmorComponent.HairCoverTypes.None), seed, HairTags, BeardTags, TattooTags);
	}

	public void SetTransferableInPartyScreen(bool isTransferable)
	{
		if (isTransferable)
		{
			_characterRestrictionFlags &= ~CharacterRestrictionFlags.NotTransferableInPartyScreen;
		}
		else
		{
			_characterRestrictionFlags |= CharacterRestrictionFlags.NotTransferableInPartyScreen;
		}
	}

	public void SetTransferableInHideouts(bool isTransferable)
	{
		if (isTransferable)
		{
			_characterRestrictionFlags &= ~CharacterRestrictionFlags.CanNotGoInHideout;
		}
		else
		{
			_characterRestrictionFlags |= CharacterRestrictionFlags.CanNotGoInHideout;
		}
	}

	public void ClearAttributes()
	{
		if (IsHero)
		{
			HeroObject.ClearAttributes();
		}
	}

	public int GetTraitLevel(TraitObject trait)
	{
		if (IsHero)
		{
			return HeroObject.GetTraitLevel(trait);
		}
		return _characterTraits.GetPropertyValue(trait);
	}

	public bool GetPerkValue(PerkObject perk)
	{
		if (IsHero)
		{
			return HeroObject.GetPerkValue(perk);
		}
		return false;
	}

	public override int GetSkillValue(SkillObject skill)
	{
		if (IsHero)
		{
			return HeroObject.GetSkillValue(skill);
		}
		return base.GetSkillValue(skill);
	}

	public TraitObject GetPersona()
	{
		if (_persona == null)
		{
			return DefaultTraits.PersonaSoftspoken;
		}
		return _persona;
	}

	public override int GetMountKeySeed()
	{
		if (!IsHero)
		{
			return MBRandom.NondeterministicRandomInt;
		}
		return HeroObject.RandomValue;
	}

	public override FormationClass GetFormationClass()
	{
		if (IsHero && Equipment != null)
		{
			bool num = Equipment[EquipmentIndex.ArmorItemEndSlot].Item?.HasHorseComponent ?? false;
			bool flag = Equipment.HasWeaponOfClass(WeaponClass.Bow) || Equipment.HasWeaponOfClass(WeaponClass.Crossbow);
			if (!num)
			{
				if (!flag)
				{
					return FormationClass.Infantry;
				}
				return FormationClass.Ranged;
			}
			if (!flag)
			{
				return FormationClass.Cavalry;
			}
			return FormationClass.HorseArcher;
		}
		return base.GetFormationClass();
	}

	public static CharacterObject Find(string idString)
	{
		return MBObjectManager.Instance.GetObject<CharacterObject>(idString);
	}

	public static CharacterObject FindFirst(Predicate<CharacterObject> predicate)
	{
		return All.FirstOrDefault((CharacterObject x) => predicate(x));
	}

	public static IEnumerable<CharacterObject> FindAll(Predicate<CharacterObject> predicate)
	{
		return All.Where((CharacterObject x) => predicate(x));
	}

	private static float GetPowerImp(int tier, bool isHero = false, bool isMounted = false)
	{
		return (float)((2 + tier) * (8 + tier)) * 0.02f * (isHero ? 1.5f : (isMounted ? 1.2f : 1f));
	}
}
