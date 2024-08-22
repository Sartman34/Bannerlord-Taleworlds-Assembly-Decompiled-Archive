using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection;

public class HeroVM : ViewModel
{
	private ImageIdentifierVM _imageIdentifier;

	private ImageIdentifierVM _clanBanner;

	private ImageIdentifierVM _clanBanner_9;

	private string _nameText;

	private int _relation = -102;

	private bool _isDead = true;

	private bool _isChild;

	public Hero Hero { get; }

	[DataSourceProperty]
	public bool IsDead
	{
		get
		{
			return _isDead;
		}
		set
		{
			if (value != _isDead)
			{
				_isDead = value;
				OnPropertyChangedWithValue(value, "IsDead");
			}
		}
	}

	[DataSourceProperty]
	public bool IsChild
	{
		get
		{
			return _isChild;
		}
		set
		{
			if (value != _isChild)
			{
				_isChild = value;
				OnPropertyChangedWithValue(value, "IsChild");
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
	public ImageIdentifierVM ImageIdentifier
	{
		get
		{
			return _imageIdentifier;
		}
		set
		{
			if (value != _imageIdentifier)
			{
				_imageIdentifier = value;
				OnPropertyChangedWithValue(value, "ImageIdentifier");
			}
		}
	}

	[DataSourceProperty]
	public string NameText
	{
		get
		{
			return _nameText;
		}
		set
		{
			if (value != _nameText)
			{
				_nameText = value;
				OnPropertyChangedWithValue(value, "NameText");
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
	public ImageIdentifierVM ClanBanner_9
	{
		get
		{
			return _clanBanner_9;
		}
		set
		{
			if (value != _clanBanner_9)
			{
				_clanBanner_9 = value;
				OnPropertyChangedWithValue(value, "ClanBanner_9");
			}
		}
	}

	public HeroVM(Hero hero, bool useCivilian = false)
	{
		if (hero != null)
		{
			CharacterCode characterCode = CampaignUIHelper.GetCharacterCode(hero.CharacterObject, useCivilian);
			ImageIdentifier = new ImageIdentifierVM(characterCode);
			ClanBanner = new ImageIdentifierVM(hero.ClanBanner);
			ClanBanner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(hero.ClanBanner), nineGrid: true);
			Relation = GetRelation(hero);
			IsDead = !hero.IsAlive;
			IsChild = !CampaignUIHelper.IsHeroInformationHidden(hero, out var _) && FaceGen.GetMaturityTypeWithAge(hero.Age) <= BodyMeshMaturityType.Child;
		}
		else
		{
			ImageIdentifier = new ImageIdentifierVM();
			ClanBanner = new ImageIdentifierVM();
			ClanBanner_9 = new ImageIdentifierVM();
			Relation = 0;
		}
		Hero = hero;
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		if (Hero != null)
		{
			NameText = Hero.Name.ToString();
		}
		else
		{
			NameText = "";
		}
	}

	public void ExecuteLink()
	{
		if (Hero != null)
		{
			Campaign.Current.EncyclopediaManager.GoToLink(Hero.EncyclopediaLink);
		}
	}

	public virtual void ExecuteBeginHint()
	{
		if (Hero != null)
		{
			InformationManager.ShowTooltip(typeof(Hero), Hero, false);
		}
	}

	public virtual void ExecuteEndHint()
	{
		if (Hero != null)
		{
			MBInformationManager.HideInformations();
		}
	}

	public static int GetRelation(Hero hero)
	{
		if (hero == null)
		{
			return -101;
		}
		if (hero == Hero.MainHero)
		{
			return 101;
		}
		if (ViewModel.UIDebugMode)
		{
			return MBRandom.RandomInt(-100, 100);
		}
		return Hero.MainHero.GetRelation(hero);
	}
}
