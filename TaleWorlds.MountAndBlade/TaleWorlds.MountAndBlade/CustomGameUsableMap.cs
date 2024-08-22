using System.Collections.Generic;
using System.Linq;

namespace TaleWorlds.MountAndBlade;

public class CustomGameUsableMap
{
	public string map { get; private set; }

	public bool isCompatibleWithAllGameTypes { get; private set; }

	public List<string> compatibleGameTypes { get; private set; }

	public CustomGameUsableMap(string map, bool isCompatibleWithAllGameTypes, List<string> compatibleGameTypes)
	{
		this.map = map;
		this.isCompatibleWithAllGameTypes = isCompatibleWithAllGameTypes;
		this.compatibleGameTypes = compatibleGameTypes;
	}

	public override bool Equals(object obj)
	{
		if (obj is CustomGameUsableMap customGameUsableMap)
		{
			if (customGameUsableMap.map != map || customGameUsableMap.isCompatibleWithAllGameTypes != isCompatibleWithAllGameTypes)
			{
				return false;
			}
			if ((compatibleGameTypes == null || compatibleGameTypes.Count == 0) && (customGameUsableMap.compatibleGameTypes == null || customGameUsableMap.compatibleGameTypes.Count == 0))
			{
				return true;
			}
			if (compatibleGameTypes == null || compatibleGameTypes.Count == 0 || customGameUsableMap.compatibleGameTypes == null || customGameUsableMap.compatibleGameTypes.Count == 0)
			{
				return false;
			}
			return compatibleGameTypes.SequenceEqual(customGameUsableMap.compatibleGameTypes);
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return (((((map != null) ? map.GetHashCode() : 0) * 397) ^ isCompatibleWithAllGameTypes.GetHashCode()) * 397) ^ ((compatibleGameTypes != null) ? compatibleGameTypes.GetHashCode() : 0);
	}
}
