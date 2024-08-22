using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;

public class ClanRoleMemberItemVM : ViewModel
{
	private Action _onRoleAssigned;

	private MobileParty _party;

	private readonly IEnumerable<SkillEffect> _skillEffects;

	private readonly IEnumerable<PerkObject> _perks;

	private ClanPartyMemberItemVM _member;

	private HintViewModel _hint;

	private bool _isRemoveAssigneeOption;

	public SkillEffect.PerkRole Role { get; private set; }

	public SkillObject RelevantSkill { get; private set; }

	public int RelevantSkillValue { get; private set; }

	[DataSourceProperty]
	public ClanPartyMemberItemVM Member
	{
		get
		{
			return _member;
		}
		set
		{
			if (value != _member)
			{
				_member = value;
				OnPropertyChangedWithValue(value, "Member");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel Hint
	{
		get
		{
			return _hint;
		}
		set
		{
			if (value != _hint)
			{
				_hint = value;
				OnPropertyChangedWithValue(value, "Hint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRemoveAssigneeOption
	{
		get
		{
			return _isRemoveAssigneeOption;
		}
		set
		{
			if (value != _isRemoveAssigneeOption)
			{
				_isRemoveAssigneeOption = value;
				OnPropertyChangedWithValue(value, "IsRemoveAssigneeOption");
			}
		}
	}

	public ClanRoleMemberItemVM(MobileParty party, SkillEffect.PerkRole role, ClanPartyMemberItemVM member, Action onRoleAssigned)
	{
		Role = role;
		Member = member;
		_party = party;
		_onRoleAssigned = onRoleAssigned;
		RelevantSkill = GetRelevantSkillForRole(role);
		RelevantSkillValue = Member?.HeroObject?.GetSkillValue(RelevantSkill) ?? (-1);
		_skillEffects = SkillEffect.All.Where((SkillEffect x) => x.PrimaryRole != SkillEffect.PerkRole.Personal || x.SecondaryRole != SkillEffect.PerkRole.Personal);
		_perks = PerkObject.All.Where((PerkObject x) => Member.HeroObject.GetPerkValue(x));
		IsRemoveAssigneeOption = Member == null;
		Hint = new HintViewModel(IsRemoveAssigneeOption ? new TextObject("{=bfWlTVjs}Remove assignee") : GetRoleHint(Role));
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
	}

	public void ExecuteAssignHeroToRole()
	{
		if (Member == null)
		{
			switch (Role)
			{
			case SkillEffect.PerkRole.Quartermaster:
				_party.SetPartyQuartermaster(null);
				break;
			case SkillEffect.PerkRole.Scout:
				_party.SetPartyScout(null);
				break;
			case SkillEffect.PerkRole.Surgeon:
				_party.SetPartySurgeon(null);
				break;
			case SkillEffect.PerkRole.Engineer:
				_party.SetPartyEngineer(null);
				break;
			}
		}
		else
		{
			OnSetMemberAsRole(Role);
		}
		_onRoleAssigned?.Invoke();
	}

	private void OnSetMemberAsRole(SkillEffect.PerkRole role)
	{
		if (role != 0)
		{
			if (_party.GetHeroPerkRole(Member.HeroObject) != role)
			{
				_party.RemoveHeroPerkRole(Member.HeroObject);
				switch (role)
				{
				case SkillEffect.PerkRole.Engineer:
					_party.SetPartyEngineer(Member.HeroObject);
					break;
				case SkillEffect.PerkRole.Quartermaster:
					_party.SetPartyQuartermaster(Member.HeroObject);
					break;
				case SkillEffect.PerkRole.Scout:
					_party.SetPartyScout(Member.HeroObject);
					break;
				case SkillEffect.PerkRole.Surgeon:
					_party.SetPartySurgeon(Member.HeroObject);
					break;
				}
				Game.Current?.EventManager.TriggerEvent(new ClanRoleAssignedThroughClanScreenEvent(role, Member.HeroObject));
			}
		}
		else if (role == SkillEffect.PerkRole.None)
		{
			_party.RemoveHeroPerkRole(Member.HeroObject);
		}
		_onRoleAssigned?.Invoke();
	}

	private TextObject GetRoleHint(SkillEffect.PerkRole role)
	{
		string text = "";
		if (RelevantSkillValue <= 0)
		{
			GameTexts.SetVariable("LEFT", RelevantSkill.Name.ToString());
			GameTexts.SetVariable("RIGHT", RelevantSkillValue.ToString());
			GameTexts.SetVariable("STR2", GameTexts.FindText("str_LEFT_colon_RIGHT").ToString());
			GameTexts.SetVariable("STR1", Member.Name.ToString());
			text = GameTexts.FindText("str_string_newline_string").ToString();
		}
		else if (!DoesHeroHaveEnoughSkillForRole(Member.HeroObject, role, _party))
		{
			GameTexts.SetVariable("SKILL_NAME", RelevantSkill.Name.ToString());
			GameTexts.SetVariable("MIN_SKILL_AMOUNT", 0);
			text = GameTexts.FindText("str_character_role_disabled_tooltip").ToString();
		}
		else if (!role.Equals(SkillEffect.PerkRole.None))
		{
			IEnumerable<SkillEffect> enumerable = _skillEffects.Where((SkillEffect x) => x.PrimaryRole == role || x.SecondaryRole == role);
			IEnumerable<PerkObject> enumerable2 = _perks.Where((PerkObject x) => x.PrimaryRole == role || x.SecondaryRole == role);
			GameTexts.SetVariable("LEFT", RelevantSkill.Name.ToString());
			GameTexts.SetVariable("RIGHT", RelevantSkillValue.ToString());
			GameTexts.SetVariable("STR2", GameTexts.FindText("str_LEFT_colon_RIGHT").ToString());
			GameTexts.SetVariable("STR1", Member.Name.ToString());
			text = GameTexts.FindText("str_string_newline_string").ToString();
			int num = 0;
			TextObject textObject = GameTexts.FindText("str_LEFT_colon_RIGHT").CopyTextObject();
			textObject.SetTextVariable("LEFT", new TextObject("{=Avy8Gua1}Perks"));
			textObject.SetTextVariable("RIGHT", new TextObject("{=Gp2vmZGZ}{PERKS}"));
			foreach (PerkObject item in enumerable2)
			{
				if (num == 0)
				{
					GameTexts.SetVariable("PERKS", item.Name.ToString());
				}
				else
				{
					GameTexts.SetVariable("RIGHT", item.Name.ToString());
					GameTexts.SetVariable("LEFT", new TextObject("{=Gp2vmZGZ}{PERKS}").ToString());
					GameTexts.SetVariable("PERKS", GameTexts.FindText("str_LEFT_comma_RIGHT").ToString());
				}
				num++;
			}
			GameTexts.SetVariable("newline", "\n \n");
			if (num > 0)
			{
				GameTexts.SetVariable("STR1", text);
				GameTexts.SetVariable("STR2", textObject.ToString());
				text = GameTexts.FindText("str_string_newline_string").ToString();
			}
			GameTexts.SetVariable("LEFT", new TextObject("{=DKJIp6xG}Effects").ToString());
			string content = GameTexts.FindText("str_LEFT_colon").ToString();
			GameTexts.SetVariable("STR1", text);
			GameTexts.SetVariable("STR2", content);
			text = GameTexts.FindText("str_string_newline_string").ToString();
			GameTexts.SetVariable("newline", "\n");
			foreach (SkillEffect item2 in enumerable)
			{
				GameTexts.SetVariable("STR1", text);
				GameTexts.SetVariable("STR2", SkillHelper.GetEffectDescriptionForSkillLevel(item2, RelevantSkillValue));
				text = GameTexts.FindText("str_string_newline_string").ToString();
			}
		}
		else
		{
			text = null;
		}
		if (!string.IsNullOrEmpty(text))
		{
			return new TextObject("{=!}" + text);
		}
		return TextObject.Empty;
	}

	public string GetEffectsList(SkillEffect.PerkRole role)
	{
		string text = "";
		IEnumerable<SkillEffect> enumerable = _skillEffects.Where((SkillEffect x) => x.PrimaryRole == role || x.SecondaryRole == role);
		int num = 0;
		if (RelevantSkillValue > 0)
		{
			foreach (SkillEffect item in enumerable)
			{
				if (num == 0)
				{
					text = SkillHelper.GetEffectDescriptionForSkillLevel(item, RelevantSkillValue);
				}
				else
				{
					GameTexts.SetVariable("STR1", text);
					GameTexts.SetVariable("STR2", SkillHelper.GetEffectDescriptionForSkillLevel(item, RelevantSkillValue));
					text = GameTexts.FindText("str_string_newline_string").ToString();
				}
				num++;
			}
		}
		return text;
	}

	private static SkillObject GetRelevantSkillForRole(SkillEffect.PerkRole role)
	{
		switch (role)
		{
		case SkillEffect.PerkRole.Engineer:
			return DefaultSkills.Engineering;
		case SkillEffect.PerkRole.Quartermaster:
			return DefaultSkills.Steward;
		case SkillEffect.PerkRole.Scout:
			return DefaultSkills.Scouting;
		case SkillEffect.PerkRole.Surgeon:
			return DefaultSkills.Medicine;
		default:
			Debug.FailedAssert($"Undefined clan role relevant skill {role}", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\ClanManagement\\ClanRoleMemberItemVM.cs", "GetRelevantSkillForRole", 246);
			return null;
		}
	}

	public static bool IsHeroAssignableForRole(Hero hero, SkillEffect.PerkRole role, MobileParty party)
	{
		if (DoesHeroHaveEnoughSkillForRole(hero, role, party))
		{
			return hero.CanBeGovernorOrHavePartyRole();
		}
		return false;
	}

	private static bool DoesHeroHaveEnoughSkillForRole(Hero hero, SkillEffect.PerkRole role, MobileParty party)
	{
		if (party.GetHeroPerkRole(hero) == role)
		{
			return true;
		}
		switch (role)
		{
		case SkillEffect.PerkRole.Engineer:
			return MobilePartyHelper.IsHeroAssignableForEngineerInParty(hero, party);
		case SkillEffect.PerkRole.Quartermaster:
			return MobilePartyHelper.IsHeroAssignableForQuartermasterInParty(hero, party);
		case SkillEffect.PerkRole.Scout:
			return MobilePartyHelper.IsHeroAssignableForScoutInParty(hero, party);
		case SkillEffect.PerkRole.Surgeon:
			return MobilePartyHelper.IsHeroAssignableForSurgeonInParty(hero, party);
		case SkillEffect.PerkRole.None:
			return true;
		default:
			Debug.FailedAssert($"Undefined clan role is asked if assignable {role}", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\ClanManagement\\ClanRoleMemberItemVM.cs", "DoesHeroHaveEnoughSkillForRole", 284);
			return false;
		}
	}
}
