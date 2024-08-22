using System;

namespace TaleWorlds.TwoDimension;

[Serializable]
internal struct MeshData
{
	public MeshTopology Topology;

	public float[] Vertices;

	public float[] TextureCoordinates;

	public uint[] Indices;

	public int VertexCount;
}
