using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade.View;

public class BorderFlagEntityFactory : IEntityFactory
{
	private readonly string _prefabName;

	private GameEntity _cachedFlagEntity;

	public BorderFlagEntityFactory(string prefabName)
	{
		_prefabName = prefabName;
	}

	public GameEntity MakeEntity(params object[] paramObjects)
	{
		Scene scene = Mission.Current.Scene;
		if (_cachedFlagEntity == null)
		{
			_cachedFlagEntity = GameEntity.Instantiate(null, _prefabName, callScriptCallbacks: false);
		}
		GameEntity gameEntity = GameEntity.CopyFrom(scene, _cachedFlagEntity);
		gameEntity.SetMobility(GameEntity.Mobility.dynamic);
		Banner banner = (Banner)(paramObjects.FirstOrDefault((object o) => o is Banner) ?? Banner.CreateRandomBanner());
		Mesh firstMesh = gameEntity.GetFirstMesh();
		Material material = firstMesh.GetMaterial();
		Material tableauMaterial = material.CreateCopy();
		Action<Texture> setAction = delegate(Texture tex)
		{
			tableauMaterial.SetTexture(Material.MBTextureType.DiffuseMap2, tex);
		};
		banner.GetTableauTextureSmall(setAction);
		firstMesh.SetMaterial(tableauMaterial);
		return gameEntity;
	}
}
