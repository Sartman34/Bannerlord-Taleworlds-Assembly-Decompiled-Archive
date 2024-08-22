using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.MapNotificationTypes;

public class TraitChangedMapNotification : InformationData
{
	public override TextObject TitleText
	{
		get
		{
			if (!Gained)
			{
				return new TextObject("{=UT211jbX}Trait Lost");
			}
			return new TextObject("{=kbaA7aC8}Trait Gained");
		}
	}

	public override string SoundEventPath => "event:/ui/notification/trait_change";

	[SaveableProperty(1)]
	public TraitObject Trait { get; private set; }

	[SaveableProperty(2)]
	public bool Gained { get; private set; }

	[SaveableProperty(3)]
	public int PreviousTraitLevel { get; private set; }

	[SaveableProperty(4)]
	public int CurrentTraitLevel { get; private set; }

	internal static void AutoGeneratedStaticCollectObjectsTraitChangedMapNotification(object o, List<object> collectedObjects)
	{
		((TraitChangedMapNotification)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(Trait);
	}

	internal static object AutoGeneratedGetMemberValueTrait(object o)
	{
		return ((TraitChangedMapNotification)o).Trait;
	}

	internal static object AutoGeneratedGetMemberValueGained(object o)
	{
		return ((TraitChangedMapNotification)o).Gained;
	}

	internal static object AutoGeneratedGetMemberValuePreviousTraitLevel(object o)
	{
		return ((TraitChangedMapNotification)o).PreviousTraitLevel;
	}

	internal static object AutoGeneratedGetMemberValueCurrentTraitLevel(object o)
	{
		return ((TraitChangedMapNotification)o).CurrentTraitLevel;
	}

	public TraitChangedMapNotification(TraitObject trait, bool gained, int previousLevel, TextObject descriptionText)
		: base(descriptionText)
	{
		CurrentTraitLevel = Hero.MainHero.GetTraitLevel(trait);
		Gained = gained;
		Trait = trait;
		PreviousTraitLevel = previousLevel;
	}
}