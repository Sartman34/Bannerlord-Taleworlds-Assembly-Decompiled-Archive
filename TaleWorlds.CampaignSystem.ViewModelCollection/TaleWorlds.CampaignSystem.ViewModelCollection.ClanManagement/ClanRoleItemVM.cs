using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;

public class ClanRoleItemVM : ViewModel
{
	private class ClanRoleMemberComparer : IComparer<ClanRoleMemberItemVM>
	{
		public int Compare(ClanRoleMemberItemVM x, ClanRoleMemberItemVM y)
		{
			int num = y.RelevantSkillValue.CompareTo(x.RelevantSkillValue);
			if (num == 0)
			{
				return x.Member.HeroObject.Name.ToString().CompareTo(y.Member.HeroObject.Name.ToString());
			}
			return num;
		}
	}

	private Action<ClanRoleItemVM> _onRoleSelectionToggled;

	private Action _onRoleAssigned;

	private MBBindingList<ClanPartyMemberItemVM> _heroMembers;

	private MobileParty _party;

	private ClanRoleMemberComparer _comparer;

	private bool _isEnabled;

	private MBBindingList<ClanRoleMemberItemVM> _members;

	private ClanRoleMemberItemVM _effectiveOwner;

	private HintViewModel _notAssignedHint;

	private HintViewModel _disabledHint;

	private bool _isNotAssigned;

	private bool _hasEffects;

	private string _name;

	private string _assignedMemberEffects;

	private string _noEffectText;

	public SkillEffect.PerkRole Role { get; private set; }

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ClanRoleMemberItemVM> Members
	{
		get
		{
			return _members;
		}
		set
		{
			if (value != _members)
			{
				_members = value;
				OnPropertyChangedWithValue(value, "Members");
			}
		}
	}

	[DataSourceProperty]
	public ClanRoleMemberItemVM EffectiveOwner
	{
		get
		{
			return _effectiveOwner;
		}
		set
		{
			if (value != _effectiveOwner)
			{
				_effectiveOwner = value;
				OnPropertyChangedWithValue(value, "EffectiveOwner");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel NotAssignedHint
	{
		get
		{
			return _notAssignedHint;
		}
		set
		{
			if (value != _notAssignedHint)
			{
				_notAssignedHint = value;
				OnPropertyChangedWithValue(value, "NotAssignedHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DisabledHint
	{
		get
		{
			return _disabledHint;
		}
		set
		{
			if (value != _disabledHint)
			{
				_disabledHint = value;
				OnPropertyChangedWithValue(value, "DisabledHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNotAssigned
	{
		get
		{
			return _isNotAssigned;
		}
		set
		{
			if (value != _isNotAssigned)
			{
				_isNotAssigned = value;
				OnPropertyChangedWithValue(value, "IsNotAssigned");
			}
		}
	}

	[DataSourceProperty]
	public bool HasEffects
	{
		get
		{
			return _hasEffects;
		}
		set
		{
			if (value != _hasEffects)
			{
				_hasEffects = value;
				OnPropertyChangedWithValue(value, "HasEffects");
			}
		}
	}

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				OnPropertyChangedWithValue(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public string AssignedMemberEffects
	{
		get
		{
			return _assignedMemberEffects;
		}
		set
		{
			if (value != _assignedMemberEffects)
			{
				_assignedMemberEffects = value;
				OnPropertyChangedWithValue(value, "AssignedMemberEffects");
			}
		}
	}

	[DataSourceProperty]
	public string NoEffectText
	{
		get
		{
			return _noEffectText;
		}
		set
		{
			if (value != _noEffectText)
			{
				_noEffectText = value;
				OnPropertyChangedWithValue(value, "NoEffectText");
			}
		}
	}

	public ClanRoleItemVM(MobileParty party, SkillEffect.PerkRole role, MBBindingList<ClanPartyMemberItemVM> heroMembers, Action<ClanRoleItemVM> onRoleSelectionToggled, Action onRoleAssigned)
	{
		Role = role;
		_comparer = new ClanRoleMemberComparer();
		_party = party;
		_onRoleSelectionToggled = onRoleSelectionToggled;
		_onRoleAssigned = onRoleAssigned;
		_heroMembers = heroMembers;
		Members = new MBBindingList<ClanRoleMemberItemVM>();
		NotAssignedHint = new HintViewModel(new TextObject("{=S1iS3OYj}Party leader is default for unassigned roles"));
		DisabledHint = new HintViewModel();
		IsEnabled = true;
		Refresh();
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		Name = GameTexts.FindText("role", Role.ToString()).ToString();
		NoEffectText = GameTexts.FindText("str_clan_role_no_effect").ToString();
		AssignedMemberEffects = EffectiveOwner?.GetEffectsList(Role) ?? "";
		HasEffects = !string.IsNullOrEmpty(AssignedMemberEffects);
		Members.ApplyActionOnAllItems(delegate(ClanRoleMemberItemVM x)
		{
			x.RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		Members.ApplyActionOnAllItems(delegate(ClanRoleMemberItemVM x)
		{
			x.OnFinalize();
		});
	}

	public void Refresh()
	{
		Members.ApplyActionOnAllItems(delegate(ClanRoleMemberItemVM x)
		{
			x.OnFinalize();
		});
		Members.Clear();
		foreach (ClanPartyMemberItemVM heroMember in _heroMembers)
		{
			if (ClanRoleMemberItemVM.IsHeroAssignableForRole(heroMember.HeroObject, Role, _party))
			{
				Members.Add(new ClanRoleMemberItemVM(_party, Role, heroMember, OnRoleAssigned));
			}
		}
		Members.Add(new ClanRoleMemberItemVM(_party, Role, null, OnRoleAssigned));
		Members.Sort(_comparer);
		GetMemberAssignedToRole(_party, Role, out var roleOwner, out var effectiveRoleOwner);
		EffectiveOwner = Members.FirstOrDefault((ClanRoleMemberItemVM x) => x.Member?.HeroObject == effectiveRoleOwner);
		IsNotAssigned = roleOwner == null;
	}

	public void ExecuteToggleRoleSelection()
	{
		_onRoleSelectionToggled?.Invoke(this);
	}

	private void GetMemberAssignedToRole(MobileParty party, SkillEffect.PerkRole role, out Hero roleOwner, out Hero effectiveRoleOwner)
	{
		roleOwner = party.GetRoleHolder(role);
		switch (role)
		{
		case SkillEffect.PerkRole.Quartermaster:
			effectiveRoleOwner = party.EffectiveQuartermaster;
			return;
		case SkillEffect.PerkRole.Scout:
			effectiveRoleOwner = party.EffectiveScout;
			return;
		case SkillEffect.PerkRole.Surgeon:
			effectiveRoleOwner = party.EffectiveSurgeon;
			return;
		case SkillEffect.PerkRole.Engineer:
			effectiveRoleOwner = party.EffectiveEngineer;
			return;
		}
		effectiveRoleOwner = party.LeaderHero;
		roleOwner = party.LeaderHero;
		Debug.FailedAssert("Given party role is not valid.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\ClanManagement\\ClanRoleItemVM.cs", "GetMemberAssignedToRole", 107);
	}

	private void OnRoleAssigned()
	{
		MBInformationManager.HideInformations();
		_onRoleAssigned?.Invoke();
	}

	public void SetEnabled(bool enabled, TextObject disabledHint)
	{
		IsEnabled = enabled;
		DisabledHint.HintText = disabledHint;
	}
}
