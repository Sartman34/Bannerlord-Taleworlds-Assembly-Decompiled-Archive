using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public static class MountVisualCreator
{
	public static void SetMaterialProperties(ItemObject mountItem, MetaMesh mountMesh, MountCreationKey key, ref uint maneMeshMultiplier)
	{
		HorseComponent horseComponent = mountItem.HorseComponent;
		int index = MathF.Min(key.MaterialIndex, horseComponent.HorseMaterialNames.Count - 1);
		HorseComponent.MaterialProperty materialProperty = horseComponent.HorseMaterialNames[index];
		Material fromResource = Material.GetFromResource(materialProperty.Name);
		if (mountItem.ItemType == ItemObject.ItemTypeEnum.Horse)
		{
			int num = MathF.Min(key.MeshMultiplierIndex, materialProperty.MeshMultiplier.Count - 1);
			if (num != -1)
			{
				maneMeshMultiplier = materialProperty.MeshMultiplier[num].Item1;
			}
			mountMesh.SetMaterialToSubMeshesWithTag(fromResource, "horse_body");
			mountMesh.SetFactorColorToSubMeshesWithTag(maneMeshMultiplier, "horse_tail");
		}
		else
		{
			mountMesh.SetMaterial(fromResource);
		}
	}

	public static List<MetaMesh> AddMountMesh(MBAgentVisuals agentVisual, ItemObject mountItem, ItemObject harnessItem, string mountCreationKeyStr, Agent agent = null)
	{
		List<MetaMesh> list = new List<MetaMesh>();
		HorseComponent horseComponent = mountItem.HorseComponent;
		uint maneMeshMultiplier = uint.MaxValue;
		MetaMesh multiMesh = mountItem.GetMultiMesh(isFemale: false, hasGloves: false, needBatchedVersion: true);
		MountCreationKey mountCreationKey = null;
		if (string.IsNullOrEmpty(mountCreationKeyStr))
		{
			mountCreationKeyStr = MountCreationKey.GetRandomMountKeyString(mountItem, MBRandom.RandomInt());
		}
		mountCreationKey = MountCreationKey.FromString(mountCreationKeyStr);
		if (mountItem.ItemType == ItemObject.ItemTypeEnum.Horse)
		{
			SetHorseColors(multiMesh, mountCreationKey);
		}
		if (horseComponent.HorseMaterialNames != null && horseComponent.HorseMaterialNames.Count > 0)
		{
			SetMaterialProperties(mountItem, multiMesh, mountCreationKey, ref maneMeshMultiplier);
		}
		int nondeterministicRandomInt = MBRandom.NondeterministicRandomInt;
		SetVoiceDefinition(agent, nondeterministicRandomInt);
		MetaMesh metaMesh = null;
		if (harnessItem != null)
		{
			metaMesh = harnessItem.GetMultiMesh(isFemale: false, hasGloves: false, needBatchedVersion: true);
		}
		foreach (KeyValuePair<string, bool> additionalMeshesName in horseComponent.AdditionalMeshesNameList)
		{
			if (additionalMeshesName.Key.Length <= 0)
			{
				continue;
			}
			string text = additionalMeshesName.Key;
			if (harnessItem == null || !additionalMeshesName.Value)
			{
				MetaMesh copy = MetaMesh.GetCopy(text);
				if (maneMeshMultiplier != uint.MaxValue)
				{
					copy.SetFactor1Linear(maneMeshMultiplier);
				}
				list.Add(copy);
				continue;
			}
			ArmorComponent armorComponent = harnessItem.ArmorComponent;
			if (armorComponent == null || armorComponent.ManeCoverType != ArmorComponent.HorseHarnessCoverTypes.All)
			{
				ArmorComponent armorComponent2 = harnessItem.ArmorComponent;
				if (armorComponent2 != null && armorComponent2.ManeCoverType > ArmorComponent.HorseHarnessCoverTypes.None)
				{
					text = text + "_" + harnessItem?.ArmorComponent?.ManeCoverType;
				}
				MetaMesh copy2 = MetaMesh.GetCopy(text);
				if (maneMeshMultiplier != uint.MaxValue)
				{
					copy2.SetFactor1Linear(maneMeshMultiplier);
				}
				list.Add(copy2);
			}
		}
		if (multiMesh != null)
		{
			if (harnessItem != null && harnessItem.ArmorComponent?.TailCoverType == ArmorComponent.HorseTailCoverTypes.All)
			{
				multiMesh.RemoveMeshesWithTag("horse_tail");
			}
			list.Add(multiMesh);
		}
		if (metaMesh != null)
		{
			if (agentVisual != null)
			{
				MetaMesh metaMesh2 = null;
				if (NativeConfig.CharacterDetail > 2 && harnessItem.ArmorComponent != null)
				{
					metaMesh2 = MetaMesh.GetCopy(harnessItem.ArmorComponent.ReinsRopeMesh, showErrors: false, mayReturnNull: true);
				}
				MetaMesh copy3 = MetaMesh.GetCopy(harnessItem.ArmorComponent?.ReinsMesh, showErrors: false, mayReturnNull: true);
				if (metaMesh2 != null && copy3 != null)
				{
					agentVisual.AddHorseReinsClothMesh(copy3, metaMesh2);
					metaMesh2.ManualInvalidate();
				}
				if (copy3 != null)
				{
					list.Add(copy3);
				}
			}
			else if (harnessItem.ArmorComponent != null)
			{
				MetaMesh copy4 = MetaMesh.GetCopy(harnessItem.ArmorComponent.ReinsMesh, showErrors: true, mayReturnNull: true);
				if (copy4 != null)
				{
					list.Add(copy4);
				}
			}
			list.Add(metaMesh);
		}
		return list;
	}

	public static void SetHorseColors(MetaMesh horseMesh, MountCreationKey mountCreationKey)
	{
		horseMesh.SetVectorArgument((int)mountCreationKey._leftFrontLegColorIndex, (int)mountCreationKey._rightFrontLegColorIndex, (int)mountCreationKey._leftBackLegColorIndex, (int)mountCreationKey._rightBackLegColorIndex);
	}

	public static void ClearMountMesh(GameEntity gameEntity)
	{
		gameEntity.RemoveAllChildren();
		gameEntity.Remove(106);
	}

	private static void SetVoiceDefinition(Agent agent, int seedForRandomVoiceTypeAndPitch)
	{
		MBAgentVisuals mBAgentVisuals = agent?.AgentVisuals;
		if (mBAgentVisuals != null)
		{
			string soundAndCollisionInfoClassName = agent.GetSoundAndCollisionInfoClassName();
			int num = ((!string.IsNullOrEmpty(soundAndCollisionInfoClassName)) ? SkinVoiceManager.GetVoiceDefinitionCountWithMonsterSoundAndCollisionInfoClassName(soundAndCollisionInfoClassName) : 0);
			if (num == 0)
			{
				mBAgentVisuals.SetVoiceDefinitionIndex(-1, 0f);
				return;
			}
			int num2 = MathF.Abs(seedForRandomVoiceTypeAndPitch);
			float voicePitch = (float)num2 * 4.656613E-10f;
			int[] array = new int[num];
			SkinVoiceManager.GetVoiceDefinitionListWithMonsterSoundAndCollisionInfoClassName(soundAndCollisionInfoClassName, array);
			int voiceDefinitionIndex = array[num2 % num];
			mBAgentVisuals.SetVoiceDefinitionIndex(voiceDefinitionIndex, voicePitch);
		}
	}

	public static void AddMountMeshToEntity(GameEntity gameEntity, ItemObject mountItem, ItemObject harnessItem, string mountCreationKeyStr, Agent agent = null)
	{
		foreach (MetaMesh item in AddMountMesh(null, mountItem, harnessItem, mountCreationKeyStr, agent))
		{
			gameEntity.AddMultiMeshToSkeleton(item);
			item.ManualInvalidate();
		}
	}

	public static void AddMountMeshToAgentVisual(MBAgentVisuals agentVisual, ItemObject mountItem, ItemObject harnessItem, string mountCreationKeyStr, Agent agent = null)
	{
		foreach (MetaMesh item in AddMountMesh(agentVisual, mountItem, harnessItem, mountCreationKeyStr, agent))
		{
			agentVisual.AddMultiMesh(item, BodyMeshTypes.Invalid);
			item.ManualInvalidate();
		}
		if (mountItem.HorseComponent?.SkeletonScale != null)
		{
			agentVisual.ApplySkeletonScale(mountItem.HorseComponent.SkeletonScale.MountSitBoneScale, mountItem.HorseComponent.SkeletonScale.MountRadiusAdder, mountItem.HorseComponent.SkeletonScale.BoneIndices, mountItem.HorseComponent.SkeletonScale.Scales);
		}
	}
}
