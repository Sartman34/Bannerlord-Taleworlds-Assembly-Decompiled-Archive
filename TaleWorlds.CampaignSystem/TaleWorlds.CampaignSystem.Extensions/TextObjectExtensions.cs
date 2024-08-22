using Helpers;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.Extensions;

public static class TextObjectExtensions
{
	public static void SetCharacterProperties(this TextObject to, string tag, CharacterObject character, bool includeDetails = false)
	{
		StringHelpers.SetCharacterProperties(tag, character, to, includeDetails);
	}
}
