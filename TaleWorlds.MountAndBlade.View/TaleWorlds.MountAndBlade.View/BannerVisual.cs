using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace TaleWorlds.MountAndBlade.View;

public class BannerVisual : IBannerVisual
{
	public Banner Banner { get; private set; }

	public BannerVisual(Banner banner)
	{
		Banner = banner;
	}

	public void ValidateCreateTableauTextures()
	{
	}

	public Texture GetTableauTextureSmall(Action<Texture> setAction, bool isTableauOrNineGrid = true)
	{
		return TableauCacheManager.Current.BeginCreateBannerTexture(BannerCode.CreateFrom(Banner), setAction, isTableauOrNineGrid);
	}

	public Texture GetTableauTextureLarge(Action<Texture> setAction, bool isTableauOrNineGrid = true)
	{
		return TableauCacheManager.Current.BeginCreateBannerTexture(BannerCode.CreateFrom(Banner), setAction, isTableauOrNineGrid, isLarge: true);
	}

	public static MatrixFrame GetMeshMatrix(ref Mesh mesh, float marginLeft, float marginTop, float width, float height, bool mirrored, float rotation, float deltaZ)
	{
		MatrixFrame identity = MatrixFrame.Identity;
		float num = width / 1528f;
		float num2 = height / 1528f;
		float x = num / mesh.GetBoundingBoxWidth();
		float y = num2 / mesh.GetBoundingBoxHeight();
		identity.rotation.RotateAboutUp(rotation);
		if (mirrored)
		{
			identity.rotation.RotateAboutForward(System.MathF.PI);
		}
		identity.rotation.ApplyScaleLocal(new Vec3(x, y, 1f));
		identity.origin.x = 0f;
		identity.origin.y = 0f;
		identity.origin.x += marginLeft / 1528f;
		identity.origin.y -= marginTop / 1528f;
		identity.origin.z += deltaZ;
		return identity;
	}

	public MetaMesh ConvertToMultiMesh()
	{
		BannerData bannerData = Banner.BannerDataList[0];
		MetaMesh metaMesh = MetaMesh.CreateMetaMesh();
		Mesh fromResource = Mesh.GetFromResource(BannerManager.Instance.GetBackgroundMeshName(bannerData.MeshId));
		Mesh mesh = fromResource.CreateCopy();
		fromResource.ManualInvalidate();
		mesh.Color = BannerManager.GetColor(bannerData.ColorId2);
		mesh.Color2 = BannerManager.GetColor(bannerData.ColorId);
		MatrixFrame meshMatrix = GetMeshMatrix(ref mesh, bannerData.Position.x, bannerData.Position.y, bannerData.Size.x, bannerData.Size.y, bannerData.Mirror, bannerData.RotationValue * 2f * System.MathF.PI, 0.5f);
		mesh.SetLocalFrame(meshMatrix);
		metaMesh.AddMesh(mesh);
		mesh.ManualInvalidate();
		for (int i = 1; i < Banner.BannerDataList.Count; i++)
		{
			BannerData bannerData2 = Banner.BannerDataList[i];
			BannerIconData iconDataFromIconId = BannerManager.Instance.GetIconDataFromIconId(bannerData2.MeshId);
			Material fromResource2 = Material.GetFromResource(iconDataFromIconId.MaterialName);
			if (fromResource2 != null)
			{
				Mesh mesh2 = Mesh.CreateMeshWithMaterial(fromResource2);
				float num = (float)(iconDataFromIconId.TextureIndex % 4) * 0.25f;
				float num2 = 1f - (float)(iconDataFromIconId.TextureIndex / 4) * 0.25f;
				Vec2 vec = new Vec2(num, num2);
				Vec2 uvCoord = new Vec2(num + 0.25f, num2 - 0.25f);
				UIntPtr uIntPtr = mesh2.LockEditDataWrite();
				int num3 = mesh2.AddFaceCorner(new Vec3(-0.5f, -0.5f), new Vec3(0f, 0f, 1f), vec + new Vec2(0f, -0.25f), uint.MaxValue, uIntPtr);
				int patchNode = mesh2.AddFaceCorner(new Vec3(0.5f, -0.5f), new Vec3(0f, 0f, 1f), uvCoord, uint.MaxValue, uIntPtr);
				int num4 = mesh2.AddFaceCorner(new Vec3(0.5f, 0.5f), new Vec3(0f, 0f, 1f), vec + new Vec2(0.25f, 0f), uint.MaxValue, uIntPtr);
				int patchNode2 = mesh2.AddFaceCorner(new Vec3(-0.5f, 0.5f), new Vec3(0f, 0f, 1f), vec, uint.MaxValue, uIntPtr);
				mesh2.AddFace(num3, patchNode, num4, uIntPtr);
				mesh2.AddFace(num4, patchNode2, num3, uIntPtr);
				mesh2.UnlockEditDataWrite(uIntPtr);
				mesh2.SetColorAndStroke(BannerManager.GetColor(bannerData2.ColorId), BannerManager.GetColor(bannerData2.ColorId2), bannerData2.DrawStroke);
				meshMatrix = GetMeshMatrix(ref mesh2, bannerData2.Position.x, bannerData2.Position.y, bannerData2.Size.x, bannerData2.Size.y, bannerData2.Mirror, bannerData2.RotationValue * 2f * System.MathF.PI, i);
				mesh2.SetLocalFrame(meshMatrix);
				metaMesh.AddMesh(mesh2);
			}
		}
		return metaMesh;
	}
}
