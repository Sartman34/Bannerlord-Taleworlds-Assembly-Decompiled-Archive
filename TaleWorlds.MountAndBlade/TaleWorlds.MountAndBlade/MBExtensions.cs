using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public static class MBExtensions
{
	private static Vec2 GetGlobalOrganicDirectionAux(ColumnFormation columnFormation, int depthCount = -1)
	{
		IEnumerable<Agent> unitsAtVanguardFile = columnFormation.GetUnitsAtVanguardFile<Agent>();
		Vec2 zero = Vec2.Zero;
		int num = 0;
		Agent agent = null;
		foreach (Agent item in unitsAtVanguardFile)
		{
			if (agent != null)
			{
				Vec2 vec = (agent.Position - item.Position).AsVec2.Normalized();
				zero += vec;
				num++;
			}
			agent = item;
			if (depthCount > 0 && num >= depthCount)
			{
				break;
			}
		}
		if (num == 0)
		{
			return Vec2.Invalid;
		}
		return zero * (1f / (float)num);
	}

	public static Vec2 GetGlobalOrganicDirection(this ColumnFormation columnFormation)
	{
		return GetGlobalOrganicDirectionAux(columnFormation);
	}

	public static Vec2 GetGlobalHeadDirection(this ColumnFormation columnFormation)
	{
		return GetGlobalOrganicDirectionAux(columnFormation, 3);
	}

	public static IEnumerable<T> FindAllWithType<T>(this IEnumerable<GameEntity> entities) where T : ScriptComponentBehavior
	{
		return entities.SelectMany((GameEntity e) => e.GetScriptComponents<T>());
	}

	public static IEnumerable<T> FindAllWithType<T>(this IEnumerable<MissionObject> missionObjects) where T : MissionObject
	{
		return from e in missionObjects
			where e != null && e is T
			select e as T;
	}

	public static List<GameEntity> FindAllWithCompatibleType(this IEnumerable<GameEntity> sceneProps, params Type[] types)
	{
		List<GameEntity> list = new List<GameEntity>();
		foreach (GameEntity sceneProp in sceneProps)
		{
			foreach (ScriptComponentBehavior scriptComponent in sceneProp.GetScriptComponents())
			{
				Type type = scriptComponent.GetType();
				for (int i = 0; i < types.Length; i++)
				{
					if (types[i].IsAssignableFrom(type))
					{
						list.Add(sceneProp);
					}
				}
			}
		}
		return list;
	}

	public static List<MissionObject> FindAllWithCompatibleType(this IEnumerable<MissionObject> missionObjects, params Type[] types)
	{
		List<MissionObject> list = new List<MissionObject>();
		foreach (MissionObject missionObject in missionObjects)
		{
			if (missionObject == null)
			{
				continue;
			}
			Type type = missionObject.GetType();
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i].IsAssignableFrom(type))
				{
					list.Add(missionObject);
				}
			}
		}
		return list;
	}

	private static void CollectObjectsAux<T>(GameEntity entity, MBList<T> list) where T : ScriptComponentBehavior
	{
		IEnumerable<T> scriptComponents = entity.GetScriptComponents<T>();
		list.AddRange(scriptComponents);
		foreach (GameEntity child in entity.GetChildren())
		{
			CollectObjectsAux(child, list);
		}
	}

	public static MBList<T> CollectObjects<T>(this GameEntity entity) where T : ScriptComponentBehavior
	{
		MBList<T> mBList = new MBList<T>();
		CollectObjectsAux(entity, mBList);
		return mBList;
	}

	public static List<T> CollectObjectsWithTag<T>(this GameEntity entity, string tag) where T : ScriptComponentBehavior
	{
		List<T> list = new List<T>();
		foreach (GameEntity child in entity.GetChildren())
		{
			if (child.HasTag(tag))
			{
				IEnumerable<T> scriptComponents = child.GetScriptComponents<T>();
				list.AddRange(scriptComponents);
			}
			if (child.ChildCount > 0)
			{
				list.AddRange(child.CollectObjectsWithTag<T>(tag));
			}
		}
		return list;
	}

	public static List<GameEntity> CollectChildrenEntitiesWithTag(this GameEntity entity, string tag)
	{
		List<GameEntity> list = new List<GameEntity>();
		foreach (GameEntity child in entity.GetChildren())
		{
			if (child.HasTag(tag))
			{
				list.Add(child);
			}
			if (child.ChildCount > 0)
			{
				list.AddRange(child.CollectChildrenEntitiesWithTag(tag));
			}
		}
		return list;
	}

	public static GameEntity GetFirstChildEntityWithTag(this GameEntity entity, string tag)
	{
		foreach (GameEntity child in entity.GetChildren())
		{
			if (child.HasTag(tag))
			{
				return child;
			}
		}
		return null;
	}

	public static T GetFirstScriptInFamilyDescending<T>(this GameEntity entity) where T : ScriptComponentBehavior
	{
		T firstScriptOfType = entity.GetFirstScriptOfType<T>();
		if (firstScriptOfType != null)
		{
			return firstScriptOfType;
		}
		foreach (GameEntity child in entity.GetChildren())
		{
			firstScriptOfType = child.GetFirstScriptInFamilyDescending<T>();
			if (firstScriptOfType != null)
			{
				return firstScriptOfType;
			}
		}
		return null;
	}

	public static bool HasParentOfType(this GameEntity e, Type t)
	{
		do
		{
			e = e.Parent;
			if (e.GetScriptComponents().Any((ScriptComponentBehavior sc) => sc.GetType() == t))
			{
				return true;
			}
		}
		while (e != null);
		return false;
	}

	public static TSource ElementAtOrValue<TSource>(this IEnumerable<TSource> source, int index, TSource value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (index >= 0)
		{
			if (source is IList<TSource> list)
			{
				if (index < list.Count)
				{
					return list[index];
				}
			}
			else
			{
				using IEnumerator<TSource> enumerator = source.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (index == 0)
					{
						return enumerator.Current;
					}
					index--;
				}
			}
		}
		return value;
	}

	public static bool IsOpponentOf(this BattleSideEnum s, BattleSideEnum side)
	{
		if (s != BattleSideEnum.Attacker || side != 0)
		{
			if (s == BattleSideEnum.Defender)
			{
				return side == BattleSideEnum.Attacker;
			}
			return false;
		}
		return true;
	}
}
