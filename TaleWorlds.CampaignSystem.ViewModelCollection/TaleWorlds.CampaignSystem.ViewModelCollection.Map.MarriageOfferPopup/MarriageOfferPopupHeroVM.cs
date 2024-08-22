using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MarriageOfferPopup;

public class MarriageOfferPopupHeroVM : ViewModel
{
	private bool _modelCreated;

	private string _encyclopediaLinkWithName;

	private string _ageString;

	private string _occupationString;

	private int _relation;

	private string _clanName;

	private ImageIdentifierVM _clanBanner;

	private HeroViewModel _model;

	private MBBindingList<EncyclopediaTraitItemVM> _traits;

	private MBBindingList<MarriageOfferPopupHeroAttributeVM> _skills;

	public Hero Hero { get; }

	[DataSourceProperty]
	public string EncyclopediaLinkWithName
	{
		get
		{
			return _encyclopediaLinkWithName;
		}
		set
		{
			if (value != _encyclopediaLinkWithName)
			{
				_encyclopediaLinkWithName = value;
				OnPropertyChangedWithValue(value, "EncyclopediaLinkWithName");
			}
		}
	}

	[DataSourceProperty]
	public string AgeString
	{
		get
		{
			return _ageString;
		}
		set
		{
			if (value != _ageString)
			{
				_ageString = value;
				OnPropertyChangedWithValue(value, "AgeString");
			}
		}
	}

	[DataSourceProperty]
	public string OccupationString
	{
		get
		{
			return _occupationString;
		}
		set
		{
			if (value != _occupationString)
			{
				_occupationString = value;
				OnPropertyChangedWithValue(value, "OccupationString");
			}
		}
	}

	[DataSourceProperty]
	public int Relation
	{
		get
		{
			return _relation;
		}
		set
		{
			if (value != _relation)
			{
				_relation = value;
				OnPropertyChangedWithValue(value, "Relation");
			}
		}
	}

	[DataSourceProperty]
	public string ClanName
	{
		get
		{
			return _clanName;
		}
		set
		{
			if (value != _clanName)
			{
				_clanName = value;
				OnPropertyChangedWithValue(value, "ClanName");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM ClanBanner
	{
		get
		{
			return _clanBanner;
		}
		set
		{
			if (value != _clanBanner)
			{
				_clanBanner = value;
				OnPropertyChangedWithValue(value, "ClanBanner");
			}
		}
	}

	[DataSourceProperty]
	public HeroViewModel Model
	{
		get
		{
			return _model;
		}
		set
		{
			if (value != _model)
			{
				_model = value;
				OnPropertyChangedWithValue(value, "Model");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<EncyclopediaTraitItemVM> Traits
	{
		get
		{
			return _traits;
		}
		set
		{
			if (value != _traits)
			{
				_traits = value;
				OnPropertyChangedWithValue(value, "Traits");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MarriageOfferPopupHeroAttributeVM> Skills
	{
		get
		{
			return _skills;
		}
		set
		{
			if (value != _skills)
			{
				_skills = value;
				OnPropertyChangedWithValue(value, "Skills");
			}
		}
	}

	public MarriageOfferPopupHeroVM(Hero hero)
	{
		Hero = hero;
		Model = new HeroViewModel();
		FillHeroInformation();
		CreateClanBanner();
		RefreshValues();
	}

	public void Update()
	{
		if (!_modelCreated && !CampaignUIHelper.IsHeroInformationHidden(Hero, out var _))
		{
			_modelCreated = true;
			CreateHeroModel();
		}
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		EncyclopediaLinkWithName = Hero.EncyclopediaLinkWithName.ToString();
		AgeString = ((int)Hero.Age).ToString();
		OccupationString = CampaignUIHelper.GetHeroOccupationName(Hero);
		Relation = (int)Hero.GetRelationWithPlayer();
	}

	public override void OnFinalize()
	{
		Model?.OnFinalize();
		Traits?.Clear();
		Skills?.Clear();
		base.OnFinalize();
	}

	public void ExecuteHeroLink()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(Hero.EncyclopediaLink);
	}

	public void ExecuteClanLink()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(Hero.Clan.EncyclopediaLink);
	}

	private void CreateClanBanner()
	{
		ClanName = Hero.Clan.Name.ToString();
		ClanBanner = new ImageIdentifierVM(BannerCode.CreateFrom(Hero.ClanBanner), nineGrid: true);
	}

	private void CreateHeroModel()
	{
		Model.FillFrom(Hero, -1, useCivilian: true, useCharacteristicIdleAction: true);
		Model.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default(EquipmentElement));
	}

	private void FillHeroInformation()
	{
		Traits = new MBBindingList<EncyclopediaTraitItemVM>();
		Skills = new MBBindingList<MarriageOfferPopupHeroAttributeVM>();
		foreach (CharacterAttribute item in Attributes.All)
		{
			Skills.Add(new MarriageOfferPopupHeroAttributeVM(Hero, item));
		}
		foreach (TraitObject heroTrait in CampaignUIHelper.GetHeroTraits())
		{
			if (Hero.GetTraitLevel(heroTrait) != 0)
			{
				Traits.Add(new EncyclopediaTraitItemVM(heroTrait, Hero));
			}
		}
	}
}
