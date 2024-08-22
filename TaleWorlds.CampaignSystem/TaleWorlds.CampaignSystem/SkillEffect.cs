using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem;

public sealed class SkillEffect : PropertyObject
{
	public enum EffectIncrementType
	{
		Invalid = -1,
		Add,
		AddFactor
	}

	public enum PerkRole
	{
		None,
		Ruler,
		ClanLeader,
		Governor,
		ArmyCommander,
		PartyLeader,
		PartyOwner,
		Surgeon,
		Engineer,
		Scout,
		Quartermaster,
		PartyMember,
		Personal,
		Captain,
		NumberOfPerkRoles
	}

	public static MBReadOnlyList<SkillEffect> All => Campaign.Current.AllSkillEffects;

	public PerkRole PrimaryRole { get; private set; }

	public PerkRole SecondaryRole { get; private set; }

	public float PrimaryBonus { get; private set; }

	public float SecondaryBonus { get; private set; }

	public EffectIncrementType IncrementType { get; private set; }

	public SkillObject[] EffectedSkills { get; private set; }

	public float PrimaryBaseValue { get; private set; }

	public float SecondaryBaseValue { get; private set; }

	public SkillEffect(string stringId)
		: base(stringId)
	{
	}

	public void Initialize(TextObject description, SkillObject[] effectedSkills, PerkRole primaryRole = PerkRole.None, float primaryBonus = 0f, PerkRole secondaryRole = PerkRole.None, float secondaryBonus = 0f, EffectIncrementType incrementType = EffectIncrementType.AddFactor, float primaryBaseValue = 0f, float secondaryBaseValue = 0f)
	{
		Initialize(TextObject.Empty, description);
		PrimaryRole = primaryRole;
		SecondaryRole = secondaryRole;
		PrimaryBonus = primaryBonus;
		SecondaryBonus = secondaryBonus;
		IncrementType = incrementType;
		EffectedSkills = effectedSkills;
		PrimaryBaseValue = primaryBaseValue;
		SecondaryBaseValue = secondaryBaseValue;
		AfterInitialized();
	}

	public float GetPrimaryValue(int skillLevel)
	{
		return MathF.Max(0f, PrimaryBaseValue + PrimaryBonus * (float)skillLevel);
	}

	public float GetSecondaryValue(int skillLevel)
	{
		return MathF.Max(0f, SecondaryBaseValue + SecondaryBonus * (float)skillLevel);
	}
}
