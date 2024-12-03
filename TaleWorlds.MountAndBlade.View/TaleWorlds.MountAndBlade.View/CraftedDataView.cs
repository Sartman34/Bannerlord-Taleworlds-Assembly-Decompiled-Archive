using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public class CraftedDataView
{
	private MetaMesh _weaponMesh;

	private MetaMesh _holsterMesh;

	private MetaMesh _holsterMeshWithWeapon;

	private MetaMesh _nonBatchedWeaponMesh;

	private MetaMesh _nonBatchedHolsterMesh;

	private MetaMesh _nonBatchedHolsterMeshWithWeapon;

	public WeaponDesign CraftedData { get; private set; }

	public MetaMesh WeaponMesh
	{
		get
		{
			if (!(_weaponMesh != null) || !_weaponMesh.HasVertexBufferOrEditDataOrPackageItem())
			{
				return _weaponMesh = GenerateWeaponMesh(batchMeshes: true);
			}
			return _weaponMesh;
		}
	}

	public MetaMesh HolsterMesh => _holsterMesh ?? (_holsterMesh = GenerateHolsterMesh());

	public MetaMesh HolsterMeshWithWeapon
	{
		get
		{
			if (!(_holsterMeshWithWeapon != null) || !_holsterMeshWithWeapon.HasVertexBufferOrEditDataOrPackageItem())
			{
				return _holsterMeshWithWeapon = GenerateHolsterMeshWithWeapon(batchMeshes: true);
			}
			return _holsterMeshWithWeapon;
		}
	}

	public MetaMesh NonBatchedWeaponMesh => _nonBatchedWeaponMesh ?? (_nonBatchedWeaponMesh = GenerateWeaponMesh(batchMeshes: false));

	public MetaMesh NonBatchedHolsterMesh => _nonBatchedHolsterMesh ?? (_nonBatchedHolsterMesh = GenerateHolsterMesh());

	public MetaMesh NonBatchedHolsterMeshWithWeapon => _nonBatchedHolsterMeshWithWeapon ?? (_nonBatchedHolsterMeshWithWeapon = GenerateHolsterMeshWithWeapon(batchMeshes: false));

	public CraftedDataView(WeaponDesign craftedData)
	{
		CraftedData = craftedData;
	}

	public void Clear()
	{
		_weaponMesh = null;
		_holsterMesh = null;
		_holsterMeshWithWeapon = null;
		_nonBatchedWeaponMesh = null;
		_nonBatchedHolsterMesh = null;
		_nonBatchedHolsterMeshWithWeapon = null;
	}

	private MetaMesh GenerateWeaponMesh(bool batchMeshes)
	{
		if (CraftedData.UsedPieces != null)
		{
			return BuildWeaponMesh(CraftedData, 0f, pieceTypeHidingEnabledForHolster: false, batchMeshes);
		}
		return null;
	}

	private MetaMesh GenerateHolsterMesh()
	{
		if (CraftedData.UsedPieces != null)
		{
			return BuildHolsterMesh(CraftedData);
		}
		return null;
	}

	private MetaMesh GenerateHolsterMeshWithWeapon(bool batchMeshes)
	{
		if (CraftedData.UsedPieces != null)
		{
			return BuildHolsterMeshWithWeapon(CraftedData, 0f, batchMeshes);
		}
		return null;
	}

	public static MetaMesh BuildWeaponMesh(WeaponDesign craftedData, float pivotDiff, bool pieceTypeHidingEnabledForHolster, bool batchAllMeshes)
	{
		CraftingTemplate template = craftedData.Template;
		MetaMesh metaMesh = MetaMesh.CreateMetaMesh();
		List<MetaMesh> list = new List<MetaMesh>();
		List<MetaMesh> list2 = new List<MetaMesh>();
		List<MetaMesh> list3 = new List<MetaMesh>();
		PieceData[] buildOrders = template.BuildOrders;
		for (int i = 0; i < buildOrders.Length; i++)
		{
			PieceData pieceData = buildOrders[i];
			if (pieceTypeHidingEnabledForHolster && template.IsPieceTypeHiddenOnHolster(pieceData.PieceType))
			{
				continue;
			}
			WeaponDesignElement weaponDesignElement = craftedData.UsedPieces[(int)pieceData.PieceType];
			float num = craftedData.PiecePivotDistances[(int)pieceData.PieceType];
			if (weaponDesignElement != null && weaponDesignElement.IsValid && !float.IsNaN(num))
			{
				MetaMesh copy = MetaMesh.GetCopy(weaponDesignElement.CraftingPiece.MeshName);
				if (!batchAllMeshes)
				{
					copy.ClearMeshesForOtherLods(0);
				}
				MatrixFrame frame = new MatrixFrame(Mat3.Identity, num * Vec3.Up);
				if (weaponDesignElement.IsPieceScaled)
				{
					Vec3 scalingVector = (weaponDesignElement.CraftingPiece.FullScale ? (Vec3.One * weaponDesignElement.ScaleFactor) : new Vec3(1f, 1f, weaponDesignElement.ScaleFactor));
					frame.Scale(scalingVector);
				}
				copy.Frame = frame;
				if (copy.HasClothData())
				{
					list3.Add(copy);
				}
				else
				{
					list2.Add(copy);
				}
			}
		}
		foreach (MetaMesh item in list2)
		{
			if (batchAllMeshes)
			{
				list.Add(item);
			}
			else
			{
				metaMesh.MergeMultiMeshes(item);
			}
		}
		if (batchAllMeshes)
		{
			metaMesh.BatchMultiMeshesMultiple(list);
		}
		foreach (MetaMesh item2 in list3)
		{
			metaMesh.MergeMultiMeshes(item2);
			metaMesh.AssignClothBodyFrom(item2);
		}
		metaMesh.SetEditDataPolicy(EditDataPolicy.Keep_until_first_render);
		if (batchAllMeshes)
		{
			metaMesh.SetLodBias(1);
		}
		MatrixFrame frame2 = metaMesh.Frame;
		frame2.Elevate(pivotDiff);
		metaMesh.Frame = frame2;
		return metaMesh;
	}

	public static MetaMesh BuildHolsterMesh(WeaponDesign craftedData)
	{
		if (craftedData.Template.UseWeaponAsHolsterMesh)
		{
			return null;
		}
		BladeData bladeData = craftedData.UsedPieces[0].CraftingPiece.BladeData;
		if (craftedData.Template.AlwaysShowHolsterWithWeapon || string.IsNullOrEmpty(bladeData.HolsterMeshName))
		{
			return null;
		}
		float z = craftedData.PiecePivotDistances[0];
		MetaMesh copy = MetaMesh.GetCopy(bladeData.HolsterMeshName, showErrors: false);
		MatrixFrame frame = copy.Frame;
		frame.origin += new Vec3(0f, 0f, z);
		WeaponDesignElement weaponDesignElement = craftedData.UsedPieces[0];
		if (TaleWorlds.Library.MathF.Abs(weaponDesignElement.ScaledLength - weaponDesignElement.CraftingPiece.Length) > 1E-05f)
		{
			Vec3 scalingVector = (weaponDesignElement.CraftingPiece.FullScale ? (Vec3.One * weaponDesignElement.ScaleFactor) : new Vec3(1f, 1f, weaponDesignElement.ScaleFactor));
			frame.Scale(scalingVector);
		}
		copy.Frame = frame;
		MetaMesh metaMesh = MetaMesh.CreateMetaMesh(bladeData.HolsterMeshName);
		metaMesh.MergeMultiMeshes(copy);
		return metaMesh;
	}

	public static MetaMesh BuildHolsterMeshWithWeapon(WeaponDesign craftedData, float pivotDiff, bool batchAllMeshes)
	{
		if (craftedData.Template.UseWeaponAsHolsterMesh)
		{
			return null;
		}
		WeaponDesignElement weaponDesignElement = craftedData.UsedPieces[0];
		BladeData bladeData = weaponDesignElement.CraftingPiece.BladeData;
		if (string.IsNullOrEmpty(bladeData.HolsterMeshName))
		{
			return null;
		}
		MetaMesh metaMesh = MetaMesh.CreateMetaMesh();
		MetaMesh copy = MetaMesh.GetCopy(bladeData.HolsterMeshName, showErrors: false, mayReturnNull: true);
		string text = bladeData.HolsterMeshName + "_skeleton";
		if (Skeleton.SkeletonModelExist(text))
		{
			MetaMesh metaMesh2 = BuildWeaponMesh(craftedData, 0f, pieceTypeHidingEnabledForHolster: true, batchAllMeshes);
			float num = craftedData.PiecePivotDistances[0];
			float scaledDistanceToPreviousPiece = craftedData.UsedPieces[0].ScaledDistanceToPreviousPiece;
			float num2 = num - scaledDistanceToPreviousPiece;
			List<MetaMesh> list = new List<MetaMesh>();
			Skeleton skeleton = Skeleton.CreateFromModel(text);
			for (sbyte b = 1; b < skeleton.GetBoneCount(); b++)
			{
				MatrixFrame boneEntitialRestFrame = skeleton.GetBoneEntitialRestFrame(b, useBoneMapping: false);
				if (craftedData.Template.RotateWeaponInHolster)
				{
					boneEntitialRestFrame.rotation.RotateAboutForward(System.MathF.PI);
				}
				MetaMesh metaMesh3 = metaMesh2.CreateCopy();
				MatrixFrame frame = new MatrixFrame(boneEntitialRestFrame.rotation, boneEntitialRestFrame.origin);
				frame.Elevate(0f - num2);
				metaMesh3.Frame = frame;
				if (batchAllMeshes)
				{
					int num3 = 8 - (b - 1);
					metaMesh3.SetMaterial(Material.GetFromResource("weapon_crafting_quiver_deformer"));
					metaMesh3.SetFactor1Linear((uint)(419430400uL * (ulong)num3));
					list.Add(metaMesh3);
				}
				else
				{
					metaMesh.MergeMultiMeshes(metaMesh3);
				}
			}
			if (list.Count > 0)
			{
				metaMesh.BatchMultiMeshesMultiple(list);
			}
			if (craftedData.Template.PieceTypeToScaleHolsterWith != CraftingPiece.PieceTypes.Invalid)
			{
				WeaponDesignElement weaponDesignElement2 = craftedData.UsedPieces[(int)craftedData.Template.PieceTypeToScaleHolsterWith];
				MatrixFrame frame2 = copy.Frame;
				int num4 = -TaleWorlds.Library.MathF.Sign(skeleton.GetBoneEntitialRestFrame(0, useBoneMapping: false).rotation.u.z);
				float num5 = weaponDesignElement.CraftingPiece.BladeData.HolsterMeshLength * (weaponDesignElement2.ScaleFactor - 1f) * 0.5f * (float)num4;
				WeaponDesignElement weaponDesignElement3 = craftedData.UsedPieces[(int)craftedData.Template.PieceTypeToScaleHolsterWith];
				if (weaponDesignElement3.IsPieceScaled)
				{
					Vec3 scalingVector = (weaponDesignElement3.CraftingPiece.FullScale ? (Vec3.One * weaponDesignElement3.ScaleFactor) : new Vec3(1f, 1f, weaponDesignElement3.ScaleFactor));
					frame2.Scale(scalingVector);
				}
				frame2.origin += new Vec3(0f, 0f, 0f - num5);
				copy.Frame = frame2;
			}
		}
		else
		{
			if (craftedData.Template.PieceTypeToScaleHolsterWith != CraftingPiece.PieceTypes.Invalid)
			{
				MatrixFrame frame3 = copy.Frame;
				frame3.origin += new Vec3(0f, 0f, craftedData.PiecePivotDistances[(int)craftedData.Template.PieceTypeToScaleHolsterWith]);
				WeaponDesignElement weaponDesignElement4 = craftedData.UsedPieces[(int)craftedData.Template.PieceTypeToScaleHolsterWith];
				if (weaponDesignElement4.IsPieceScaled)
				{
					Vec3 scalingVector2 = (weaponDesignElement4.CraftingPiece.FullScale ? (Vec3.One * weaponDesignElement4.ScaleFactor) : new Vec3(1f, 1f, weaponDesignElement4.ScaleFactor));
					frame3.Scale(scalingVector2);
				}
				copy.Frame = frame3;
			}
			metaMesh.MergeMultiMeshes(BuildWeaponMesh(craftedData, 0f, pieceTypeHidingEnabledForHolster: true, batchAllMeshes));
		}
		metaMesh.MergeMultiMeshes(copy);
		MatrixFrame frame4 = metaMesh.Frame;
		frame4.origin += new Vec3(0f, 0f, pivotDiff);
		metaMesh.Frame = frame4;
		return metaMesh;
	}
}
