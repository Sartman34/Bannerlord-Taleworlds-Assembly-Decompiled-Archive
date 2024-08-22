using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem;

public sealed class Clan : MBObjectBase, IFaction
{
	[SaveableField(54)]
	private PartyTemplateObject _defaultPartyTemplate;

	[SaveableField(97)]
	private bool _isEliminated;

	[SaveableField(99)]
	private MBList<CharacterObject> _minorFactionCharacterTemplates;

	[CachedData]
	private MBList<Hero> _supporterNotablesCache;

	[SaveableField(57)]
	private Kingdom _kingdom;

	[CachedData]
	private MBList<Town> _fiefsCache;

	[CachedData]
	private MBList<Village> _villagesCache;

	[CachedData]
	private MBList<Settlement> _settlementsCache;

	[CachedData]
	private MBList<Hero> _lordsCache;

	[CachedData]
	private MBList<Hero> _heroesCache;

	[CachedData]
	private MBList<Hero> _companionsCache;

	[CachedData]
	private MBList<WarPartyComponent> _warPartyComponentsCache;

	[SaveableField(62)]
	private float _influence;

	[CachedData]
	private Settlement _clanMidSettlement;

	[SaveableField(82)]
	private CharacterObject _basicTroop;

	[SaveableField(83)]
	private Hero _leader;

	[SaveableField(84)]
	private Banner _banner;

	[SaveableField(91)]
	private int _tier;

	[SaveableField(120)]
	private float _aggressiveness;

	[SaveableField(130)]
	private int _tributeWallet;

	[SaveableField(95)]
	private Settlement _home;

	[SaveableField(110)]
	private int _clanDebtToKingdom;

	[CachedData]
	private List<StanceLink> _stances;

	[CachedData]
	private float _distanceToClosestNonAllyFortificationCache;

	[CachedData]
	internal bool _distanceToClosestNonAllyFortificationCacheDirty = true;

	[SaveableProperty(51)]
	public TextObject Name { get; private set; }

	[SaveableProperty(52)]
	public TextObject InformalName { get; private set; }

	[SaveableProperty(53)]
	public CultureObject Culture { get; set; }

	[SaveableProperty(55)]
	public CampaignTime LastFactionChangeTime { get; set; }

	public PartyTemplateObject DefaultPartyTemplate
	{
		get
		{
			if (_defaultPartyTemplate != null)
			{
				return _defaultPartyTemplate;
			}
			return Culture.DefaultPartyTemplate;
		}
	}

	[SaveableProperty(58)]
	public int AutoRecruitmentExpenses { get; set; }

	[SaveableProperty(56)]
	public TextObject EncyclopediaText { get; private set; }

	[SaveableProperty(140)]
	public bool IsNoble { get; set; }

	public bool IsEliminated => _isEliminated;

	public IList<CharacterObject> MinorFactionCharacterTemplates => _minorFactionCharacterTemplates;

	public string EncyclopediaLink => Campaign.Current.EncyclopediaManager.GetIdentifier(typeof(Clan)) + "-" + base.StringId;

	public TextObject EncyclopediaLinkWithName => HyperlinkTexts.GetClanHyperlinkText(EncyclopediaLink, Name);

	public Kingdom Kingdom
	{
		get
		{
			return _kingdom;
		}
		set
		{
			if (_kingdom != value)
			{
				SetKingdomInternal(value);
			}
		}
	}

	public IEnumerable<CharacterObject> DungeonPrisonersOfClan
	{
		get
		{
			foreach (Town fief in Fiefs)
			{
				foreach (CharacterObject prisonerHero in fief.Settlement.Party.PrisonerHeroes)
				{
					yield return prisonerHero;
				}
			}
		}
	}

	public MBReadOnlyList<Town> Fiefs => _fiefsCache;

	public MBReadOnlyList<Village> Villages => _villagesCache;

	public MBReadOnlyList<Settlement> Settlements => _settlementsCache;

	public MBReadOnlyList<Hero> SupporterNotables => _supporterNotablesCache;

	public MBReadOnlyList<Hero> Lords => _lordsCache;

	public MBReadOnlyList<Hero> Heroes => _heroesCache;

	public MBReadOnlyList<Hero> Companions => _companionsCache;

	public MBReadOnlyList<WarPartyComponent> WarPartyComponents => _warPartyComponentsCache;

	public float Influence
	{
		get
		{
			return _influence;
		}
		set
		{
			if (value < _influence && Leader != null)
			{
				SkillLevelingManager.OnInfluenceSpent(Leader, value - _influence);
			}
			_influence = value;
		}
	}

	public ExplainedNumber InfluenceChangeExplained => Campaign.Current.Models.ClanPoliticsModel.CalculateInfluenceChange(this, includeDescriptions: true);

	[CachedData]
	public float TotalStrength { get; private set; }

	[SaveableProperty(65)]
	public int MercenaryAwardMultiplier { get; set; }

	public bool IsMapFaction => _kingdom == null;

	[SaveableProperty(66)]
	public uint LabelColor { get; set; }

	[SaveableProperty(67)]
	public Vec2 InitialPosition { get; set; }

	[SaveableProperty(68)]
	public bool IsRebelClan { get; set; }

	[SaveableProperty(69)]
	public bool IsMinorFaction { get; private set; }

	[SaveableProperty(70)]
	public bool IsOutlaw { get; private set; }

	[SaveableProperty(71)]
	public bool IsNomad { get; private set; }

	[SaveableProperty(72)]
	public bool IsMafia { get; private set; }

	[SaveableProperty(73)]
	public bool IsClanTypeMercenary { get; private set; }

	[SaveableProperty(74)]
	public bool IsSect { get; private set; }

	[SaveableProperty(75)]
	public bool IsUnderMercenaryService { get; private set; }

	[SaveableProperty(76)]
	public uint Color { get; set; }

	[SaveableProperty(77)]
	public uint Color2 { get; set; }

	[SaveableProperty(78)]
	public uint AlternativeColor { get; set; }

	[SaveableProperty(79)]
	public uint AlternativeColor2 { get; set; }

	[SaveableProperty(111)]
	private uint BannerBackgroundColorPrimary { get; set; }

	[SaveableProperty(112)]
	private uint BannerBackgroundColorSecondary { get; set; }

	[SaveableProperty(113)]
	private uint BannerIconColor { get; set; }

	[CachedData]
	private bool _midPointCalculated { get; set; }

	public CharacterObject BasicTroop
	{
		get
		{
			return _basicTroop ?? Culture.BasicTroop;
		}
		set
		{
			_basicTroop = value;
		}
	}

	public static Clan PlayerClan => Campaign.Current.PlayerDefaultFaction;

	public Hero Leader => _leader;

	public int Gold => Leader?.Gold ?? 0;

	public Banner Banner
	{
		get
		{
			if (Kingdom == null || Kingdom.RulingClan != this)
			{
				return _banner;
			}
			return Kingdom.Banner;
		}
		private set
		{
			_banner = value;
		}
	}

	[SaveableProperty(85)]
	public bool IsBanditFaction { get; private set; }

	bool IFaction.IsKingdomFaction => false;

	public bool IsClan => true;

	[SaveableProperty(88)]
	public float Renown { get; set; }

	[SaveableProperty(89)]
	public float MainHeroCrimeRating { get; set; }

	public float DailyCrimeRatingChange => Campaign.Current.Models.CrimeModel.GetDailyCrimeRatingChange(this).ResultNumber;

	public ExplainedNumber DailyCrimeRatingChangeExplained => Campaign.Current.Models.CrimeModel.GetDailyCrimeRatingChange(this, includeDescriptions: true);

	public int Tier
	{
		get
		{
			return _tier;
		}
		private set
		{
			int minClanTier = Campaign.Current.Models.ClanTierModel.MinClanTier;
			int maxClanTier = Campaign.Current.Models.ClanTierModel.MaxClanTier;
			if (value > maxClanTier)
			{
				value = maxClanTier;
			}
			else if (value < minClanTier)
			{
				value = minClanTier;
			}
			_tier = value;
		}
	}

	public IFaction MapFaction
	{
		get
		{
			if (Kingdom != null)
			{
				return Kingdom;
			}
			return this;
		}
	}

	[SaveableProperty(100)]
	public CampaignTime NotAttackableByPlayerUntilTime { get; set; }

	public float Aggressiveness
	{
		get
		{
			return _aggressiveness;
		}
		internal set
		{
			_aggressiveness = TaleWorlds.Library.MathF.Clamp(value, 0f, 100f);
		}
	}

	public int TributeWallet
	{
		get
		{
			return _tributeWallet;
		}
		set
		{
			_tributeWallet = value;
		}
	}

	public Settlement HomeSettlement
	{
		get
		{
			return _home;
		}
		private set
		{
			_home = value;
		}
	}

	public int DebtToKingdom
	{
		get
		{
			return _clanDebtToKingdom;
		}
		set
		{
			_clanDebtToKingdom = value;
		}
	}

	public IEnumerable<StanceLink> Stances => _stances;

	public int RenownRequirementForNextTier => Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(Tier + 1);

	public int CompanionLimit => Campaign.Current.Models.ClanTierModel.GetCompanionLimit(this);

	public float DistanceToClosestNonAllyFortification
	{
		get
		{
			if (_distanceToClosestNonAllyFortificationCacheDirty)
			{
				_distanceToClosestNonAllyFortificationCache = FactionHelper.GetDistanceToClosestNonAllyFortificationOfFaction(this);
				_distanceToClosestNonAllyFortificationCacheDirty = false;
			}
			return _distanceToClosestNonAllyFortificationCache;
		}
	}

	public int CommanderLimit => Campaign.Current.Models.ClanTierModel.GetPartyLimitForTier(this, Tier);

	public Settlement FactionMidSettlement
	{
		get
		{
			if (!_midPointCalculated)
			{
				UpdateFactionMidSettlement();
			}
			return _clanMidSettlement;
		}
	}

	public static MBReadOnlyList<Clan> All => Campaign.Current.Clans;

	public static IEnumerable<Clan> NonBanditFactions
	{
		get
		{
			foreach (Clan clan in Campaign.Current.Clans)
			{
				if (!clan.IsBanditFaction)
				{
					yield return clan;
				}
			}
		}
	}

	public static IEnumerable<Clan> BanditFactions
	{
		get
		{
			foreach (Clan clan in Campaign.Current.Clans)
			{
				if (clan.IsBanditFaction)
				{
					yield return clan;
				}
			}
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsClan(object o, List<object> collectedObjects)
	{
		((Clan)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_defaultPartyTemplate);
		collectedObjects.Add(_minorFactionCharacterTemplates);
		collectedObjects.Add(_kingdom);
		collectedObjects.Add(_basicTroop);
		collectedObjects.Add(_leader);
		collectedObjects.Add(_banner);
		collectedObjects.Add(_home);
		collectedObjects.Add(Name);
		collectedObjects.Add(InformalName);
		collectedObjects.Add(Culture);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(LastFactionChangeTime, collectedObjects);
		collectedObjects.Add(EncyclopediaText);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(NotAttackableByPlayerUntilTime, collectedObjects);
	}

	internal static object AutoGeneratedGetMemberValueName(object o)
	{
		return ((Clan)o).Name;
	}

	internal static object AutoGeneratedGetMemberValueInformalName(object o)
	{
		return ((Clan)o).InformalName;
	}

	internal static object AutoGeneratedGetMemberValueCulture(object o)
	{
		return ((Clan)o).Culture;
	}

	internal static object AutoGeneratedGetMemberValueLastFactionChangeTime(object o)
	{
		return ((Clan)o).LastFactionChangeTime;
	}

	internal static object AutoGeneratedGetMemberValueAutoRecruitmentExpenses(object o)
	{
		return ((Clan)o).AutoRecruitmentExpenses;
	}

	internal static object AutoGeneratedGetMemberValueEncyclopediaText(object o)
	{
		return ((Clan)o).EncyclopediaText;
	}

	internal static object AutoGeneratedGetMemberValueIsNoble(object o)
	{
		return ((Clan)o).IsNoble;
	}

	internal static object AutoGeneratedGetMemberValueMercenaryAwardMultiplier(object o)
	{
		return ((Clan)o).MercenaryAwardMultiplier;
	}

	internal static object AutoGeneratedGetMemberValueLabelColor(object o)
	{
		return ((Clan)o).LabelColor;
	}

	internal static object AutoGeneratedGetMemberValueInitialPosition(object o)
	{
		return ((Clan)o).InitialPosition;
	}

	internal static object AutoGeneratedGetMemberValueIsRebelClan(object o)
	{
		return ((Clan)o).IsRebelClan;
	}

	internal static object AutoGeneratedGetMemberValueIsMinorFaction(object o)
	{
		return ((Clan)o).IsMinorFaction;
	}

	internal static object AutoGeneratedGetMemberValueIsOutlaw(object o)
	{
		return ((Clan)o).IsOutlaw;
	}

	internal static object AutoGeneratedGetMemberValueIsNomad(object o)
	{
		return ((Clan)o).IsNomad;
	}

	internal static object AutoGeneratedGetMemberValueIsMafia(object o)
	{
		return ((Clan)o).IsMafia;
	}

	internal static object AutoGeneratedGetMemberValueIsClanTypeMercenary(object o)
	{
		return ((Clan)o).IsClanTypeMercenary;
	}

	internal static object AutoGeneratedGetMemberValueIsSect(object o)
	{
		return ((Clan)o).IsSect;
	}

	internal static object AutoGeneratedGetMemberValueIsUnderMercenaryService(object o)
	{
		return ((Clan)o).IsUnderMercenaryService;
	}

	internal static object AutoGeneratedGetMemberValueColor(object o)
	{
		return ((Clan)o).Color;
	}

	internal static object AutoGeneratedGetMemberValueColor2(object o)
	{
		return ((Clan)o).Color2;
	}

	internal static object AutoGeneratedGetMemberValueAlternativeColor(object o)
	{
		return ((Clan)o).AlternativeColor;
	}

	internal static object AutoGeneratedGetMemberValueAlternativeColor2(object o)
	{
		return ((Clan)o).AlternativeColor2;
	}

	internal static object AutoGeneratedGetMemberValueBannerBackgroundColorPrimary(object o)
	{
		return ((Clan)o).BannerBackgroundColorPrimary;
	}

	internal static object AutoGeneratedGetMemberValueBannerBackgroundColorSecondary(object o)
	{
		return ((Clan)o).BannerBackgroundColorSecondary;
	}

	internal static object AutoGeneratedGetMemberValueBannerIconColor(object o)
	{
		return ((Clan)o).BannerIconColor;
	}

	internal static object AutoGeneratedGetMemberValueIsBanditFaction(object o)
	{
		return ((Clan)o).IsBanditFaction;
	}

	internal static object AutoGeneratedGetMemberValueRenown(object o)
	{
		return ((Clan)o).Renown;
	}

	internal static object AutoGeneratedGetMemberValueMainHeroCrimeRating(object o)
	{
		return ((Clan)o).MainHeroCrimeRating;
	}

	internal static object AutoGeneratedGetMemberValueNotAttackableByPlayerUntilTime(object o)
	{
		return ((Clan)o).NotAttackableByPlayerUntilTime;
	}

	internal static object AutoGeneratedGetMemberValue_defaultPartyTemplate(object o)
	{
		return ((Clan)o)._defaultPartyTemplate;
	}

	internal static object AutoGeneratedGetMemberValue_isEliminated(object o)
	{
		return ((Clan)o)._isEliminated;
	}

	internal static object AutoGeneratedGetMemberValue_minorFactionCharacterTemplates(object o)
	{
		return ((Clan)o)._minorFactionCharacterTemplates;
	}

	internal static object AutoGeneratedGetMemberValue_kingdom(object o)
	{
		return ((Clan)o)._kingdom;
	}

	internal static object AutoGeneratedGetMemberValue_influence(object o)
	{
		return ((Clan)o)._influence;
	}

	internal static object AutoGeneratedGetMemberValue_basicTroop(object o)
	{
		return ((Clan)o)._basicTroop;
	}

	internal static object AutoGeneratedGetMemberValue_leader(object o)
	{
		return ((Clan)o)._leader;
	}

	internal static object AutoGeneratedGetMemberValue_banner(object o)
	{
		return ((Clan)o)._banner;
	}

	internal static object AutoGeneratedGetMemberValue_tier(object o)
	{
		return ((Clan)o)._tier;
	}

	internal static object AutoGeneratedGetMemberValue_aggressiveness(object o)
	{
		return ((Clan)o)._aggressiveness;
	}

	internal static object AutoGeneratedGetMemberValue_tributeWallet(object o)
	{
		return ((Clan)o)._tributeWallet;
	}

	internal static object AutoGeneratedGetMemberValue_home(object o)
	{
		return ((Clan)o)._home;
	}

	internal static object AutoGeneratedGetMemberValue_clanDebtToKingdom(object o)
	{
		return ((Clan)o)._clanDebtToKingdom;
	}

	public void UpdateStrength()
	{
		TotalStrength = 0f;
		foreach (WarPartyComponent item in _warPartyComponentsCache)
		{
			TotalStrength += item.MobileParty.Party.TotalStrength;
		}
		foreach (Town fief in Fiefs)
		{
			if (fief.GarrisonParty != null)
			{
				TotalStrength += fief.GarrisonParty.Party.TotalStrength;
			}
		}
	}

	public bool IsAtWarWith(IFaction other)
	{
		return FactionManager.IsAtWarAgainstFaction(this, other);
	}

	private void UpdateFactionMidSettlement()
	{
		_clanMidSettlement = FactionHelper.FactionMidSettlement(this);
		_midPointCalculated = _clanMidSettlement != null;
	}

	private void InitMembers()
	{
		_companionsCache = new MBList<Hero>();
		_warPartyComponentsCache = new MBList<WarPartyComponent>();
		_stances = new List<StanceLink>();
		_supporterNotablesCache = new MBList<Hero>();
		_lordsCache = new MBList<Hero>();
		_heroesCache = new MBList<Hero>();
		_villagesCache = new MBList<Village>();
		_fiefsCache = new MBList<Town>();
		_settlementsCache = new MBList<Settlement>();
	}

	public Clan()
	{
		InitMembers();
		_isEliminated = false;
		NotAttackableByPlayerUntilTime = CampaignTime.Zero;
	}

	public static Clan CreateClan(string stringID)
	{
		stringID = Campaign.Current.CampaignObjectManager.FindNextUniqueStringId<Clan>(stringID);
		Clan clan = new Clan();
		clan.StringId = stringID;
		Campaign.Current.CampaignObjectManager.AddClan(clan);
		return clan;
	}

	public void InitializeClan(TextObject name, TextObject informalName, CultureObject culture, Banner banner, Vec2 initialPosition = default(Vec2), bool isDeserialize = false)
	{
		ChangeClanName(name, informalName);
		Culture = culture;
		Banner = banner;
		if (!isDeserialize)
		{
			ValidateInitialPosition(initialPosition);
		}
	}

	protected override void PreAfterLoad()
	{
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.2") && base.StringId == "neutral")
		{
			foreach (Hero aliveHero in Campaign.Current.AliveHeroes)
			{
				if (aliveHero.Clan == this)
				{
					aliveHero.ResetClanForOldSave();
					if (_lordsCache.Contains(aliveHero))
					{
						_lordsCache.Remove(aliveHero);
					}
					if (_heroesCache.Contains(aliveHero))
					{
						_heroesCache.Remove(aliveHero);
					}
				}
			}
			foreach (Hero deadOrDisabledHero in Campaign.Current.DeadOrDisabledHeroes)
			{
				if (deadOrDisabledHero.Clan == this)
				{
					deadOrDisabledHero.ResetClanForOldSave();
					if (_lordsCache.Contains(deadOrDisabledHero))
					{
						_lordsCache.Remove(deadOrDisabledHero);
					}
					if (_heroesCache.Contains(deadOrDisabledHero))
					{
						_heroesCache.Remove(deadOrDisabledHero);
					}
				}
			}
			for (int num = Heroes.Count - 1; num >= 0; num--)
			{
				Hero hero = Heroes[num];
				hero.ResetClanForOldSave();
				if (_lordsCache.Contains(hero))
				{
					_lordsCache.Remove(hero);
				}
				if (_heroesCache.Contains(hero))
				{
					_heroesCache.Remove(hero);
				}
			}
			DestroyClanAction.Apply(this);
			Campaign.Current.CampaignObjectManager.RemoveClan(this);
		}
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("e1.8.0.0"))
		{
			IsNoble = Leader?.IsNobleForOldSaves ?? false;
		}
		_kingdom?.AddClanInternal(this);
		_midPointCalculated = _clanMidSettlement != null;
		if (HomeSettlement == null && !IsBanditFaction)
		{
			UpdateHomeSettlement(null);
		}
		ValidateInitialPosition(InitialPosition);
		UpdateBannerColorsAccordingToKingdom();
	}

	protected override void AfterLoad()
	{
		UpdateStrength();
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("e1.8.0.0") && Kingdom != null)
		{
			FactionHelper.AdjustFactionStancesForClanJoiningKingdom(this, Kingdom);
		}
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.1.3") && Kingdom == null && IsUnderMercenaryService)
		{
			EndMercenaryService(isByLeavingKingdom: true);
		}
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.0") && IsEliminated && Leader != null && Leader.IsAlive)
		{
			DestroyClanAction.Apply(this);
		}
	}

	public override void Deserialize(MBObjectManager objectManager, XmlNode node)
	{
		base.Deserialize(objectManager, node);
		SetLeader(objectManager.ReadObjectReferenceFromXml("owner", typeof(Hero), node) as Hero);
		Kingdom = (Kingdom)objectManager.ReadObjectReferenceFromXml("super_faction", typeof(Kingdom), node);
		Tier = ((node.Attributes["tier"] == null) ? 1 : Convert.ToInt32(node.Attributes["tier"].Value));
		Renown = Campaign.Current.Models.ClanTierModel.CalculateInitialRenown(this);
		InitializeClan(new TextObject(node.Attributes["name"].Value), (node.Attributes["short_name"] != null) ? new TextObject(node.Attributes["short_name"].Value) : new TextObject(node.Attributes["name"].Value), (CultureObject)objectManager.ReadObjectReferenceFromXml("culture", typeof(CultureObject), node), null, default(Vec2), isDeserialize: true);
		XmlNode xmlNode = node.Attributes["is_noble"];
		if (xmlNode != null)
		{
			IsNoble = Convert.ToBoolean(xmlNode.InnerText);
		}
		LabelColor = ((node.Attributes["label_color"] != null) ? Convert.ToUInt32(node.Attributes["label_color"].Value, 16) : 0u);
		Color = ((node.Attributes["color"] == null) ? 4291609515u : Convert.ToUInt32(node.Attributes["color"].Value, 16));
		Color2 = ((node.Attributes["color2"] == null) ? 4291609515u : Convert.ToUInt32(node.Attributes["color2"].Value, 16));
		AlternativeColor = ((node.Attributes["alternative_color"] == null) ? 4291609515u : Convert.ToUInt32(node.Attributes["alternative_color"].Value, 16));
		AlternativeColor2 = ((node.Attributes["alternative_color2"] == null) ? 4291609515u : Convert.ToUInt32(node.Attributes["alternative_color2"].Value, 16));
		if (node.Attributes["initial_posX"] != null && node.Attributes["initial_posY"] != null)
		{
			InitialPosition = new Vec2((float)Convert.ToDouble(node.Attributes["initial_posX"].Value), (float)Convert.ToDouble(node.Attributes["initial_posY"].Value));
		}
		IsBanditFaction = node.Attributes["is_bandit"] != null && Convert.ToBoolean(node.Attributes["is_bandit"].Value);
		IsMinorFaction = node.Attributes["is_minor_faction"] != null && Convert.ToBoolean(node.Attributes["is_minor_faction"].Value);
		IsOutlaw = node.Attributes["is_outlaw"] != null && Convert.ToBoolean(node.Attributes["is_outlaw"].Value);
		IsSect = node.Attributes["is_sect"] != null && Convert.ToBoolean(node.Attributes["is_sect"].Value);
		IsMafia = node.Attributes["is_mafia"] != null && Convert.ToBoolean(node.Attributes["is_mafia"].Value);
		IsClanTypeMercenary = node.Attributes["is_clan_type_mercenary"] != null && Convert.ToBoolean(node.Attributes["is_clan_type_mercenary"].Value);
		IsNomad = node.Attributes["is_nomad"] != null && Convert.ToBoolean(node.Attributes["is_nomad"].Value);
		_defaultPartyTemplate = (PartyTemplateObject)objectManager.ReadObjectReferenceFromXml("default_party_template", typeof(PartyTemplateObject), node);
		EncyclopediaText = ((node.Attributes["text"] != null) ? new TextObject(node.Attributes["text"].Value) : TextObject.Empty);
		if (node.Attributes["banner_key"] != null)
		{
			_banner = new Banner();
			_banner.Deserialize(node.Attributes["banner_key"].Value);
		}
		else
		{
			_banner = Banner.CreateRandomClanBanner(base.StringId.GetDeterministicHashCode());
		}
		BannerBackgroundColorPrimary = _banner.GetPrimaryColor();
		BannerBackgroundColorSecondary = _banner.GetSecondaryColor();
		BannerIconColor = _banner.GetFirstIconColor();
		UpdateBannerColorsAccordingToKingdom();
		_minorFactionCharacterTemplates = new MBList<CharacterObject>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "minor_faction_character_templates")
			{
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					CharacterObject item = objectManager.ReadObjectReferenceFromXml("id", typeof(CharacterObject), childNode2) as CharacterObject;
					_minorFactionCharacterTemplates.Add(item);
				}
			}
			else if (childNode.Name == "relationship")
			{
				IFaction faction = ((childNode.Attributes["clan"] == null) ? ((IFaction)(Kingdom)objectManager.ReadObjectReferenceFromXml("kingdom", typeof(Kingdom), childNode)) : ((IFaction)(Clan)objectManager.ReadObjectReferenceFromXml("clan", typeof(Clan), childNode)));
				int num = Convert.ToInt32(childNode.Attributes["value"].InnerText);
				if (num > 0)
				{
					FactionManager.DeclareAlliance(this, faction);
				}
				else if (num < 0)
				{
					FactionManager.DeclareWar(this, faction);
				}
				else
				{
					FactionManager.SetNeutral(this, faction);
				}
			}
		}
	}

	protected override void OnBeforeLoad()
	{
	}

	[LoadInitializationCallback]
	private void OnLoad(MetaData metaData)
	{
		InitMembers();
	}

	public void OnGameCreated()
	{
		ValidateInitialPosition(InitialPosition);
	}

	public int GetRelationWithClan(Clan other)
	{
		if (Leader != null && other.Leader != null)
		{
			return Leader.GetRelation(other.Leader);
		}
		return 0;
	}

	public void SetLeader(Hero leader)
	{
		_leader = leader;
		if (leader != null)
		{
			leader.Clan = this;
		}
	}

	private float FindSettlementScoreForBeingHomeSettlement(Settlement settlement)
	{
		int num = (int)settlement.Town.Prosperity;
		foreach (Village boundVillage in settlement.BoundVillages)
		{
			num += (int)boundVillage.Hearth;
		}
		float num2 = (settlement.IsTown ? 1f : 0.5f);
		float num3 = TaleWorlds.Library.MathF.Sqrt(1000f + (float)num) / 50f;
		float num4 = ((HomeSettlement == settlement) ? 1f : 0.65f);
		float num5 = ((settlement.Culture == Culture) ? 1f : 0.25f);
		float num6 = ((settlement.OwnerClan.Culture == Culture) ? 1f : 0.85f);
		float num7 = ((settlement.OwnerClan == this) ? 1f : 0.1f);
		float num8 = ((settlement.MapFaction == MapFaction) ? 1f : 0.1f);
		float num9 = 0f;
		num9 = ((MapFaction.FactionMidSettlement == null) ? InitialPosition.Distance(settlement.GatePosition) : Campaign.Current.Models.MapDistanceModel.GetDistance(MapFaction.FactionMidSettlement, settlement));
		float num10 = 1f - num9 / Campaign.MaximumDistanceBetweenTwoSettlements;
		num10 *= num10;
		return num2 * num3 * num5 * num6 * num8 * num7 * num10 * num4;
	}

	public void UpdateHomeSettlement(Settlement updatedSettlement)
	{
		Settlement settlement = HomeSettlement;
		if (HomeSettlement == null || updatedSettlement == null || (HomeSettlement == updatedSettlement && updatedSettlement.OwnerClan != this))
		{
			float num = 0f;
			foreach (Settlement item in Settlement.All)
			{
				if (item.IsFortification)
				{
					float num2 = FindSettlementScoreForBeingHomeSettlement(item);
					if (num2 > num)
					{
						settlement = item;
						num = num2;
					}
				}
			}
		}
		if (settlement == HomeSettlement)
		{
			return;
		}
		HomeSettlement = settlement;
		foreach (Hero hero in Heroes)
		{
			hero.UpdateHomeSettlement();
		}
	}

	private void ValidateInitialPosition(Vec2 initialPosition)
	{
		if (initialPosition.IsValid && InitialPosition.IsNonZero())
		{
			InitialPosition = initialPosition;
			return;
		}
		Vec2 centerPosition = ((Settlements.Count <= 0) ? (Settlement.All.GetRandomElementWithPredicate((Settlement x) => x.Culture == Culture)?.GatePosition ?? Settlement.All.GetRandomElement().GatePosition) : Settlements.GetRandomElement().GatePosition);
		InitialPosition = MobilePartyHelper.FindReachablePointAroundPosition(centerPosition, 150f);
	}

	public override TextObject GetName()
	{
		return Name;
	}

	public void ChangeClanName(TextObject name, TextObject informalName)
	{
		Name = name;
		InformalName = informalName;
	}

	public override string ToString()
	{
		return string.Concat("(", base.Id, ") ", Name);
	}

	public StanceLink GetStanceWith(IFaction other)
	{
		return FactionManager.Instance.GetStanceLinkInternal(this, other);
	}

	internal void AddStanceInternal(StanceLink stanceLink)
	{
		_stances.Add(stanceLink);
	}

	internal void RemoveStanceInternal(StanceLink stanceLink)
	{
		_stances.Remove(stanceLink);
	}

	private void SetKingdomInternal(Kingdom value)
	{
		if (Kingdom != null)
		{
			LeaveKingdomInternal();
		}
		_kingdom = value;
		if (Kingdom != null)
		{
			EnterKingdomInternal();
		}
		UpdateBannerColorsAccordingToKingdom();
		LastFactionChangeTime = CampaignTime.Now;
	}

	private void EnterKingdomInternal()
	{
		_kingdom.AddClanInternal(this);
		foreach (Hero hero in Heroes)
		{
			_kingdom.OnHeroAdded(hero);
		}
		foreach (Town fief in Fiefs)
		{
			_kingdom.OnFortificationAdded(fief);
		}
		foreach (WarPartyComponent warPartyComponent in WarPartyComponents)
		{
			_kingdom.OnWarPartyAdded(warPartyComponent);
		}
	}

	private void LeaveKingdomInternal()
	{
		ChangeClanInfluenceAction.Apply(this, 0f - Influence);
		_kingdom.RemoveClanInternal(this);
		foreach (Hero hero in Heroes)
		{
			_kingdom.OnHeroRemoved(hero);
		}
		foreach (Town fief in Fiefs)
		{
			_kingdom.OnFiefRemoved(fief);
		}
		List<WarPartyComponent> list = WarPartyComponents.ToListQ();
		for (int num = list.Count() - 1; num >= 0; num--)
		{
			if (list[num].MobileParty.Army != null)
			{
				list[num].MobileParty.Army = null;
			}
			_kingdom.OnWarPartyRemoved(list[num]);
		}
	}

	public void ClanLeaveKingdom(bool giveBackFiefs = false)
	{
		ChangeClanInfluenceAction.Apply(this, 0f - Influence);
		if (Kingdom != null)
		{
			foreach (Settlement settlement in Campaign.Current.Settlements)
			{
				if (settlement.IsTown && settlement.OwnerClan == this)
				{
					SettlementHelper.TakeEnemyVillagersOutsideSettlements(settlement);
				}
			}
		}
		LastFactionChangeTime = CampaignTime.Now;
		Kingdom = null;
	}

	public float CalculateTotalSettlementBaseValue()
	{
		float num = 0f;
		foreach (Town fief in Fiefs)
		{
			num += Campaign.Current.Models.SettlementValueModel.CalculateSettlementBaseValue(fief.Owner.Settlement);
		}
		return num;
	}

	public void StartMercenaryService()
	{
		IsUnderMercenaryService = true;
	}

	private int DistanceOfTwoValues(int x, int y)
	{
		int num = ((x < 50) ? x : (100 - x));
		int num2 = ((y < 50) ? y : (100 - y));
		return TaleWorlds.Library.MathF.Min(num + num2, x - y);
	}

	public static Clan FindFirst(Predicate<Clan> predicate)
	{
		return All.FirstOrDefault((Clan x) => predicate(x));
	}

	public void EndMercenaryService(bool isByLeavingKingdom)
	{
		IsUnderMercenaryService = false;
	}

	public static IEnumerable<Clan> FindAll(Predicate<Clan> predicate)
	{
		return All.Where((Clan x) => predicate(x));
	}

	public float CalculateTotalSettlementValueForFaction(Kingdom kingdom)
	{
		float num = 0f;
		foreach (Town fief in Fiefs)
		{
			num += Campaign.Current.Models.SettlementValueModel.CalculateSettlementValueForFaction(fief.Owner.Settlement, kingdom);
		}
		return num;
	}

	internal void OnFortificationAdded(Town settlement)
	{
		_fiefsCache.Add(settlement);
		_settlementsCache.Add(settlement.Settlement);
		foreach (Village boundVillage in settlement.Settlement.BoundVillages)
		{
			OnBoundVillageAddedInternal(boundVillage);
		}
		_midPointCalculated = false;
		_distanceToClosestNonAllyFortificationCacheDirty = true;
		if (_kingdom != null)
		{
			_kingdom._midPointCalculated = false;
			_kingdom._distanceToClosestNonAllyFortificationCacheDirty = true;
			_kingdom.OnFortificationAdded(settlement);
		}
	}

	internal void OnFortificationRemoved(Town settlement)
	{
		_fiefsCache.Remove(settlement);
		_settlementsCache.Remove(settlement.Settlement);
		foreach (Village boundVillage in settlement.Settlement.BoundVillages)
		{
			_villagesCache.Remove(boundVillage);
			_settlementsCache.Remove(boundVillage.Settlement);
		}
		_midPointCalculated = false;
		_distanceToClosestNonAllyFortificationCacheDirty = true;
		if (_kingdom != null)
		{
			_kingdom._midPointCalculated = false;
			_kingdom._distanceToClosestNonAllyFortificationCacheDirty = true;
			_kingdom.OnFiefRemoved(settlement);
		}
	}

	public void OnBoundVillageAdded(Village village)
	{
		OnBoundVillageAddedInternal(village);
		if (_kingdom != null)
		{
			_kingdom.OnBoundVillageAdded(village);
		}
	}

	private void OnBoundVillageAddedInternal(Village village)
	{
		_villagesCache.Add(village);
		_settlementsCache.Add(village.Settlement);
	}

	internal void OnLordAdded(Hero lord)
	{
		_lordsCache.Add(lord);
		OnHeroAdded(lord);
	}

	internal void OnLordRemoved(Hero lord)
	{
		_lordsCache.Remove(lord);
		OnHeroRemoved(lord);
	}

	internal void OnCompanionAdded(Hero companion)
	{
		_companionsCache.Add(companion);
		OnHeroAdded(companion);
	}

	internal void OnCompanionRemoved(Hero companion)
	{
		_companionsCache.Remove(companion);
		OnHeroRemoved(companion);
	}

	private void OnHeroAdded(Hero hero)
	{
		_heroesCache.Add(hero);
		if (_kingdom != null)
		{
			_kingdom.OnHeroAdded(hero);
		}
	}

	private void OnHeroRemoved(Hero hero)
	{
		_heroesCache.Remove(hero);
		if (_kingdom != null)
		{
			_kingdom.OnHeroRemoved(hero);
		}
	}

	internal void OnWarPartyAdded(WarPartyComponent warPartyComponent)
	{
		_warPartyComponentsCache.Add(warPartyComponent);
		if (_kingdom != null)
		{
			_kingdom.OnWarPartyAdded(warPartyComponent);
		}
	}

	internal void OnWarPartyRemoved(WarPartyComponent warPartyComponent)
	{
		_warPartyComponentsCache.Remove(warPartyComponent);
		if (_kingdom != null)
		{
			_kingdom.OnWarPartyRemoved(warPartyComponent);
		}
	}

	internal void OnSupporterNotableAdded(Hero hero)
	{
		_supporterNotablesCache.Add(hero);
	}

	internal void OnSupporterNotableRemoved(Hero hero)
	{
		_supporterNotablesCache.Remove(hero);
	}

	public void AddRenown(float value, bool shouldNotify = true)
	{
		if (value > 0f)
		{
			Renown += value;
			int num = Campaign.Current.Models.ClanTierModel.CalculateTier(this);
			if (num > Tier)
			{
				Tier = num;
				CampaignEventDispatcher.Instance.OnClanTierChanged(this, shouldNotify);
			}
		}
	}

	public void ResetClanRenown()
	{
		Renown = 0f;
		Tier = Campaign.Current.Models.ClanTierModel.CalculateTier(this);
		CampaignEventDispatcher.Instance.OnClanTierChanged(this, shouldNotify: false);
	}

	public void OnSupportedByClan(Clan supporterClan)
	{
		DiplomacyModel diplomacyModel = Campaign.Current.Models.DiplomacyModel;
		int influenceCostOfSupportingClan = diplomacyModel.GetInfluenceCostOfSupportingClan();
		if (supporterClan.Influence >= (float)influenceCostOfSupportingClan)
		{
			int influenceValueOfSupportingClan = diplomacyModel.GetInfluenceValueOfSupportingClan();
			int relationValueOfSupportingClan = diplomacyModel.GetRelationValueOfSupportingClan();
			ChangeClanInfluenceAction.Apply(supporterClan, -influenceCostOfSupportingClan);
			ChangeClanInfluenceAction.Apply(this, influenceValueOfSupportingClan);
			ChangeRelationAction.ApplyRelationChangeBetweenHeroes(supporterClan.Leader, Leader, relationValueOfSupportingClan);
		}
	}

	public static Clan CreateSettlementRebelClan(Settlement settlement, Hero owner, int iconMeshId = -1)
	{
		TextObject textObject = new TextObject("{=2LIV2cy7}{SETTLEMENT}{.o} rebels");
		textObject.SetTextVariable("SETTLEMENT", settlement.Name);
		Clan clan = CreateClan(settlement.StringId + "_rebel_clan", settlement, owner, settlement.MapFaction, settlement.Culture, textObject, Campaign.Current.Models.ClanTierModel.RebelClanStartingTier, iconMeshId);
		clan.IsRebelClan = true;
		return clan;
	}

	public static Clan CreateClan(string stringId, Settlement settlement, Hero owner, IFaction inheritingFaction, CultureObject culture, TextObject clanName, int tier, int iconMeshId = -1)
	{
		Clan clan = CreateClan(stringId);
		clan.InitializeClan(clanName, clanName, culture, Banner.CreateOneColoredBannerWithOneIcon(inheritingFaction.Banner.GetFirstIconColor(), inheritingFaction.Banner.GetPrimaryColor(), iconMeshId), settlement.GatePosition);
		clan.SetLeader(owner);
		clan.LabelColor = inheritingFaction.LabelColor;
		clan.Color = inheritingFaction.Color2;
		clan.Color2 = inheritingFaction.Color;
		clan.Tier = tier;
		clan.BannerBackgroundColorPrimary = settlement.MapFaction.Banner.GetFirstIconColor();
		clan.BannerBackgroundColorSecondary = settlement.MapFaction.Banner.GetFirstIconColor();
		clan.BannerIconColor = settlement.MapFaction.Banner.GetPrimaryColor();
		clan._midPointCalculated = false;
		clan._distanceToClosestNonAllyFortificationCacheDirty = true;
		clan.HomeSettlement = settlement;
		return clan;
	}

	public static Clan CreateCompanionToLordClan(Hero hero, Settlement settlement, TextObject clanName, int newClanIconId)
	{
		Clan clan = CreateClan(Hero.MainHero.MapFaction.StringId + "_companion_clan");
		clan.InitializeClan(clanName, clanName, settlement.Culture, Banner.CreateOneColoredBannerWithOneIcon(settlement.MapFaction.Banner.GetFirstIconColor(), settlement.MapFaction.Banner.GetPrimaryColor(), newClanIconId), settlement.GatePosition);
		clan.Kingdom = Hero.MainHero.Clan.Kingdom;
		clan.Tier = Campaign.Current.Models.ClanTierModel.CompanionToLordClanStartingTier;
		hero.Clan = clan;
		clan.SetLeader(hero);
		clan.IsNoble = true;
		ChangeOwnerOfSettlementAction.ApplyByGift(settlement, hero);
		CampaignEventDispatcher.Instance.OnCompanionClanCreated(clan);
		return clan;
	}

	public MobileParty CreateNewMobileParty(Hero hero)
	{
		if (hero.CurrentSettlement != null)
		{
			Settlement currentSettlement = hero.CurrentSettlement;
			if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.IsMainParty)
			{
				PartyBase.MainParty.MemberRoster.RemoveTroop(hero.CharacterObject);
			}
			return MobilePartyHelper.SpawnLordParty(hero, currentSettlement);
		}
		MobileParty partyBelongedTo = hero.PartyBelongedTo;
		partyBelongedTo?.AddElementToMemberRoster(hero.CharacterObject, -1);
		return MobilePartyHelper.SpawnLordParty(hero, partyBelongedTo?.Position2D ?? SettlementHelper.GetBestSettlementToSpawnAround(hero).GatePosition, 5f);
	}

	public MobileParty CreateNewMobilePartyAtPosition(Hero hero, Vec2 spawnPosition)
	{
		return MobilePartyHelper.SpawnLordParty(hero, spawnPosition, 5f);
	}

	public Dictionary<Hero, int> GetHeirApparents()
	{
		Dictionary<Hero, int> dictionary = new Dictionary<Hero, int>();
		int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
		Hero maxSkillHero = Leader;
		foreach (Hero hero in Heroes)
		{
			if (hero != Leader && hero.IsAlive && hero.DeathMark == KillCharacterAction.KillCharacterActionDetail.None && !hero.IsNotSpawned && !hero.IsDisabled && !hero.IsWanderer && !hero.IsNotable && hero.Age >= (float)heroComesOfAge)
			{
				int value = Campaign.Current.Models.HeirSelectionCalculationModel.CalculateHeirSelectionPoint(hero, Leader, ref maxSkillHero);
				dictionary.Add(hero, value);
			}
		}
		if (maxSkillHero != Leader)
		{
			dictionary[maxSkillHero] += Campaign.Current.Models.HeirSelectionCalculationModel.HighestSkillPoint;
		}
		return dictionary;
	}

	private void UpdateBannerColorsAccordingToKingdom()
	{
		if (Kingdom != null)
		{
			Banner?.ChangePrimaryColor(Kingdom.PrimaryBannerColor);
			Banner?.ChangeIconColors(Kingdom.SecondaryBannerColor);
			if (Kingdom.RulingClan == this)
			{
				_banner?.ChangePrimaryColor(Kingdom.PrimaryBannerColor);
				_banner?.ChangeIconColors(Kingdom.SecondaryBannerColor);
			}
		}
		else if (BannerBackgroundColorPrimary != 0 || BannerBackgroundColorSecondary != 0 || BannerIconColor != 0)
		{
			Banner?.ChangeBackgroundColor(BannerBackgroundColorPrimary, BannerBackgroundColorSecondary);
			Banner?.ChangeIconColors(BannerIconColor);
		}
		else if (IsMinorFaction)
		{
			Banner?.ChangePrimaryColor(Color);
			Banner?.ChangeIconColors((Color != Color2) ? Color2 : uint.MaxValue);
		}
	}

	public void UpdateBannerColor(uint backgroundColor, uint iconColor)
	{
		BannerBackgroundColorPrimary = backgroundColor;
		BannerBackgroundColorSecondary = backgroundColor;
		BannerIconColor = iconColor;
	}

	internal void DeactivateClan()
	{
		_isEliminated = true;
	}

	[SpecialName]
	string IFaction.get_StringId()
	{
		return base.StringId;
	}

	[SpecialName]
	MBGUID IFaction.get_Id()
	{
		return base.Id;
	}
}
