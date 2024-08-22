using System.Collections.Generic;
using Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CharacterCreationContent;

public class CharacterCreation
{
	private readonly MBList<FaceGenChar> _faceGenChars;

	public readonly List<CharacterCreationMenu> CharacterCreationMenus;

	public bool IsPlayerAlone { get; set; }

	public bool HasSecondaryCharacter { get; set; }

	public string PrefabId { get; private set; }

	public sbyte PrefabBoneUsage { get; private set; }

	public MBReadOnlyList<FaceGenChar> FaceGenChars => _faceGenChars;

	public FaceGenMount FaceGenMount { get; private set; }

	public bool CharsEquipmentNeedsRefresh { get; private set; }

	public bool CharsNeedsRefresh { get; set; }

	public bool MountsNeedsRefresh { get; set; }

	public string Name { get; set; }

	public int CharacterCreationMenuCount => CharacterCreationMenus.Count;

	public void ChangeFaceGenChars(List<FaceGenChar> newChars)
	{
		_faceGenChars.Clear();
		foreach (FaceGenChar newChar in newChars)
		{
			_faceGenChars.Add(newChar);
		}
		CharsNeedsRefresh = true;
	}

	public void SetFaceGenMount(FaceGenMount newMount)
	{
		FaceGenMount = null;
		if (newMount != null)
		{
			FaceGenMount = newMount;
		}
		MountsNeedsRefresh = true;
	}

	public void ClearFaceGenMounts()
	{
		FaceGenMount = null;
		MountsNeedsRefresh = true;
	}

	public void ClearFaceGenChars()
	{
		_faceGenChars.Clear();
		CharsNeedsRefresh = true;
	}

	public void ClearFaceGenPrefab()
	{
		PrefabId = "";
		PrefabBoneUsage = 0;
	}

	public void ChangeCharactersEquipment(List<Equipment> equipmentList)
	{
		for (int i = 0; i < equipmentList.Count; i++)
		{
			_faceGenChars[i].Equipment.FillFrom(equipmentList[i]);
		}
		CharsEquipmentNeedsRefresh = true;
	}

	public void ClearCharactersEquipment()
	{
		for (int i = 0; i < _faceGenChars.Count; i++)
		{
			_faceGenChars[i].Equipment.FillFrom(new Equipment());
		}
		CharsEquipmentNeedsRefresh = true;
	}

	public void ChangeCharacterPrefab(string id, sbyte boneUsage)
	{
		PrefabId = id;
		PrefabBoneUsage = boneUsage;
	}

	public void ChangeCharsAnimation(List<string> actionList)
	{
		for (int i = 0; i < actionList.Count; i++)
		{
			_faceGenChars[i].ActionName = actionList[i];
		}
		CharsEquipmentNeedsRefresh = true;
	}

	public void ChangeMountsAnimation(string action)
	{
		FaceGenMount.ActionName = action;
		MountsNeedsRefresh = true;
	}

	public CharacterCreation()
	{
		_faceGenChars = new MBList<FaceGenChar>();
		CharacterCreationMenus = new List<CharacterCreationMenu>();
		CharsEquipmentNeedsRefresh = false;
	}

	public void AddNewMenu(CharacterCreationMenu menu)
	{
		CharacterCreationMenus.Add(menu);
	}

	public CharacterCreationMenu GetCurrentMenu(int index)
	{
		if (index >= 0 && index < CharacterCreationMenus.Count)
		{
			return CharacterCreationMenus[index];
		}
		return null;
	}

	public IEnumerable<CharacterCreationOption> GetCurrentMenuOptions(int index)
	{
		List<CharacterCreationOption> list = new List<CharacterCreationOption>();
		CharacterCreationMenu currentMenu = GetCurrentMenu(index);
		if (currentMenu != null)
		{
			foreach (CharacterCreationCategory characterCreationCategory in currentMenu.CharacterCreationCategories)
			{
				CharacterCreationOnCondition categoryCondition = characterCreationCategory.CategoryCondition;
				if (categoryCondition != null && !categoryCondition())
				{
					continue;
				}
				foreach (CharacterCreationOption characterCreationOption in characterCreationCategory.CharacterCreationOptions)
				{
					if (characterCreationOption.OnCondition == null || characterCreationOption.OnCondition())
					{
						list.Add(characterCreationOption);
					}
				}
			}
		}
		return list;
	}

	public void ResetMenuOptions()
	{
		for (int i = 0; i < CharacterCreationMenus.Count; i++)
		{
			CharacterCreationMenus[i].SelectedOptions.Clear();
		}
	}

	public void OnInit(int stage)
	{
		if (CharacterCreationMenus[stage].OnInit != null)
		{
			CharacterCreationMenus[stage].OnInit(this);
		}
	}

	public TextObject GetCurrentMenuText(int stage)
	{
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter);
		return CharacterCreationMenus[stage].Text;
	}

	public TextObject GetCurrentMenuTitle(int stage)
	{
		return CharacterCreationMenus[stage].Title;
	}

	public void RunConsequence(CharacterCreationOption option, int stage, bool fromInit)
	{
		if (CharacterCreationMenus[stage].MenuType == CharacterCreationMenu.MenuTypes.MultipleChoice)
		{
			CharacterCreationMenus[stage].SelectedOptions.Clear();
		}
		if (!fromInit && CharacterCreationMenus[stage].SelectedOptions.Contains(option.Id))
		{
			CharacterCreationMenus[stage].SelectedOptions.Remove(option.Id);
			return;
		}
		CharacterCreationMenus[stage].SelectedOptions.Add(option.Id);
		if (option.OnSelect != null)
		{
			option.OnSelect(this);
		}
	}

	public IEnumerable<int> GetSelectedOptions(int stage)
	{
		return CharacterCreationMenus[stage].SelectedOptions;
	}

	public void ApplyFinalEffects()
	{
		Clan.PlayerClan.Renown = 0f;
		CharacterCreationContentBase.Instance.ApplyCulture(this);
		foreach (CharacterCreationMenu characterCreationMenu in CharacterCreationMenus)
		{
			characterCreationMenu.ApplyFinalEffect(this);
		}
		Campaign.Current.PlayerTraitDeveloper.UpdateTraitXPAccordingToTraitLevels();
	}
}
