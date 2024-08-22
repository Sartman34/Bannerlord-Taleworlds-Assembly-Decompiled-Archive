namespace TaleWorlds.Library;

public struct PathFaceRecord
{
	public struct StackArray6PathFaceRecord
	{
		private PathFaceRecord _element0;

		private PathFaceRecord _element1;

		private PathFaceRecord _element2;

		private PathFaceRecord _element3;

		private PathFaceRecord _element4;

		private PathFaceRecord _element5;

		public const int Length = 6;

		public PathFaceRecord this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return _element0;
				case 1:
					return _element1;
				case 2:
					return _element2;
				case 3:
					return _element3;
				case 4:
					return _element4;
				case 5:
					return _element5;
				default:
					Debug.FailedAssert("Index out of range.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\PathFaceRecord.cs", "Item", 34);
					return NullFaceRecord;
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					_element0 = value;
					break;
				case 1:
					_element1 = value;
					break;
				case 2:
					_element2 = value;
					break;
				case 3:
					_element3 = value;
					break;
				case 4:
					_element4 = value;
					break;
				case 5:
					_element5 = value;
					break;
				default:
					Debug.FailedAssert("Index out of range.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\PathFaceRecord.cs", "Item", 50);
					break;
				}
			}
		}
	}

	public int FaceIndex;

	public int FaceGroupIndex;

	public int FaceIslandIndex;

	public static readonly PathFaceRecord NullFaceRecord = new PathFaceRecord(-1, -1, -1);

	public PathFaceRecord(int index, int groupIndex, int islandIndex)
	{
		FaceIndex = index;
		FaceGroupIndex = groupIndex;
		FaceIslandIndex = islandIndex;
	}

	public bool IsValid()
	{
		return FaceIndex != -1;
	}
}
