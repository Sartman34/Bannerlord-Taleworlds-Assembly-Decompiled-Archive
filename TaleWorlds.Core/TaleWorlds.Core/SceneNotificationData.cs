using System.Collections.Generic;
using TaleWorlds.Localization;

namespace TaleWorlds.Core;

public class SceneNotificationData
{
	public readonly struct SceneNotificationCharacter
	{
		public readonly BasicCharacterObject Character;

		public readonly Equipment OverriddenEquipment;

		public readonly BodyProperties OverriddenBodyProperties;

		public readonly bool UseCivilianEquipment;

		public readonly bool UseHorse;

		public readonly uint CustomColor1;

		public readonly uint CustomColor2;

		public SceneNotificationCharacter(BasicCharacterObject character, Equipment overriddenEquipment = null, BodyProperties overriddenBodyProperties = default(BodyProperties), bool useCivilianEquipment = false, uint customColor1 = uint.MaxValue, uint customColor2 = uint.MaxValue, bool useHorse = false)
		{
			Character = character;
			OverriddenEquipment = overriddenEquipment;
			OverriddenBodyProperties = overriddenBodyProperties;
			UseCivilianEquipment = useCivilianEquipment;
			CustomColor1 = customColor1;
			CustomColor2 = customColor2;
			UseHorse = useHorse;
		}
	}

	public enum RelevantContextType
	{
		Any,
		MPLobby,
		CustomBattle,
		Mission,
		Map
	}

	public virtual string SceneID { get; }

	public virtual string SoundEventPath { get; }

	public virtual TextObject TitleText { get; }

	public virtual TextObject AffirmativeDescriptionText { get; }

	public virtual TextObject NegativeDescriptionText { get; }

	public virtual TextObject AffirmativeHintText { get; }

	public virtual TextObject AffirmativeHintTextExtended { get; }

	public virtual TextObject AffirmativeTitleText { get; }

	public virtual TextObject NegativeTitleText { get; }

	public virtual TextObject AffirmativeText { get; }

	public virtual TextObject NegativeText { get; }

	public virtual bool IsAffirmativeOptionShown { get; }

	public virtual bool IsNegativeOptionShown { get; }

	public virtual bool PauseActiveState { get; } = true;


	public virtual RelevantContextType RelevantContext { get; }

	public virtual void OnAffirmativeAction()
	{
	}

	public virtual void OnNegativeAction()
	{
	}

	public virtual void OnCloseAction()
	{
	}

	public virtual IEnumerable<Banner> GetBanners()
	{
		return new List<Banner>();
	}

	public virtual IEnumerable<SceneNotificationCharacter> GetSceneNotificationCharacters()
	{
		return new List<SceneNotificationCharacter>();
	}
}
