using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BattleSideSpawnPathSelector
{
	public const float MaxNeighborCount = 2f;

	private readonly Mission _mission;

	private readonly SpawnPathData _initialSpawnPath;

	private readonly MBList<SpawnPathData> _reinforcementSpawnPaths;

	public SpawnPathData InitialSpawnPath => _initialSpawnPath;

	public MBReadOnlyList<SpawnPathData> ReinforcementPaths => _reinforcementSpawnPaths;

	public BattleSideSpawnPathSelector(Mission mission, Path initialPath, float initialPathCenterRatio, bool initialPathIsInverted)
	{
		_mission = mission;
		_initialSpawnPath = new SpawnPathData(initialPath, SpawnPathOrientation.PathCenter, initialPathCenterRatio, initialPathIsInverted);
		_reinforcementSpawnPaths = new MBList<SpawnPathData>();
		FindReinforcementPaths();
	}

	public bool HasReinforcementPath(Path path)
	{
		if (path != null)
		{
			return _reinforcementSpawnPaths.Exists((SpawnPathData pathData) => pathData.Path.Pointer == path.Pointer);
		}
		return false;
	}

	private void FindReinforcementPaths()
	{
		_reinforcementSpawnPaths.Clear();
		SpawnPathData item = new SpawnPathData(_initialSpawnPath.Path, SpawnPathOrientation.Local, 0f, _initialSpawnPath.IsInverted);
		_reinforcementSpawnPaths.Add(item);
		MBList<Path> allSpawnPaths = MBSceneUtilities.GetAllSpawnPaths(_mission.Scene);
		if (allSpawnPaths.Count == 0)
		{
			return;
		}
		bool flag = false;
		if (allSpawnPaths.Count > 1)
		{
			MatrixFrame[] array = new MatrixFrame[100];
			item.Path.GetPoints(array);
			MatrixFrame matrixFrame = (item.IsInverted ? array[item.Path.NumberOfPoints - 1] : array[0]);
			SortedList<float, SpawnPathData> sortedList = new SortedList<float, SpawnPathData>();
			foreach (Path item2 in allSpawnPaths)
			{
				if (item2.NumberOfPoints > 1 && item2.Pointer != item.Path.Pointer)
				{
					item2.GetPoints(array);
					MatrixFrame matrixFrame2 = array[0];
					MatrixFrame matrixFrame3 = array[item2.NumberOfPoints - 1];
					float key = matrixFrame2.origin.DistanceSquared(matrixFrame.origin);
					float key2 = matrixFrame3.origin.DistanceSquared(matrixFrame.origin);
					sortedList.Add(key, new SpawnPathData(item2, SpawnPathOrientation.Local));
					sortedList.Add(key2, new SpawnPathData(item2, SpawnPathOrientation.Local, 0f, isInverted: true));
				}
				else
				{
					flag = flag || item.Path.Pointer == item2.Pointer;
				}
			}
			int num = 0;
			{
				foreach (KeyValuePair<float, SpawnPathData> item3 in sortedList)
				{
					_reinforcementSpawnPaths.Add(item3.Value);
					num++;
					if ((float)num >= 2f)
					{
						break;
					}
				}
				return;
			}
		}
		flag = item.Path.Pointer == allSpawnPaths[0].Pointer;
	}
}
