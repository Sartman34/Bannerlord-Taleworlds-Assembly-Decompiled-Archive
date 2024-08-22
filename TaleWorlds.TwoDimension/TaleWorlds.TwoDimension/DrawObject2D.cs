using System;
using System.Collections.Generic;
using System.Numerics;
using TaleWorlds.Library;

namespace TaleWorlds.TwoDimension;

public sealed class DrawObject2D
{
	private static byte[] floatTemporaryHolder;

	private static uint[] uintTemporaryHolder;

	private static List<Vector2> _referenceCirclePoints;

	private static List<Vector2> _circlePolygonPoints;

	public MeshTopology Topology { get; private set; }

	public float[] Vertices { get; private set; }

	public float[] TextureCoordinates { get; private set; }

	public uint[] Indices { get; private set; }

	public int VertexCount { get; set; }

	public ulong HashCode1 { get; private set; }

	public ulong HashCode2 { get; private set; }

	public Rectangle BoundingRectangle { get; private set; }

	public DrawObjectType DrawObjectType { get; set; }

	public float Width { get; set; }

	public float Height { get; set; }

	public float MinU { get; set; }

	public float MinV { get; set; }

	public float MaxU { get; set; }

	public float MaxV { get; set; }

	static DrawObject2D()
	{
		floatTemporaryHolder = new byte[4];
		uintTemporaryHolder = new uint[1];
		_referenceCirclePoints = new List<Vector2>(64);
		_circlePolygonPoints = new List<Vector2>(64);
		for (int i = 0; i < 16; i++)
		{
			Vector2 item = default(Vector2);
			float num = i;
			num *= 22.5f;
			item.X = Mathf.Cos(num * ((float)Math.PI / 180f));
			item.Y = Mathf.Sin(num * ((float)Math.PI / 180f));
			_referenceCirclePoints.Add(item);
		}
	}

	public DrawObject2D(MeshTopology topology, float[] vertices, float[] uvs, uint[] indices, int vertexCount)
	{
		Topology = topology;
		Vertices = vertices;
		TextureCoordinates = uvs;
		Indices = indices;
		VertexCount = vertexCount;
	}

	public DrawObject2D(MeshTopology topology, int vertexCount)
	{
		Topology = topology;
		Vertices = new float[vertexCount * 2];
		TextureCoordinates = new float[vertexCount * 2];
		Indices = new uint[vertexCount];
		VertexCount = vertexCount;
	}

	public void SetVertexAt(int index, Vector2 vertex)
	{
		Vertices[2 * index] = vertex.X;
		Vertices[2 * index + 1] = vertex.Y;
	}

	public static DrawObject2D CreateTriangleTopologyMeshWithPolygonCoordinates(List<Vector2> vertices)
	{
		int num = 3 * (vertices.Count - 2);
		float[] array = new float[num * 2];
		float[] uvs = new float[num * 2];
		uint[] array2 = new uint[num];
		for (int i = 0; i < num / 3; i++)
		{
			array[6 * i] = vertices[0].X;
			array[6 * i + 1] = vertices[0].Y;
			array[6 * i + 2] = vertices[i + 1].X;
			array[6 * i + 3] = vertices[i + 1].Y;
			array[6 * i + 4] = vertices[i + 2].X;
			array[6 * i + 5] = vertices[i + 2].Y;
		}
		for (uint num2 = 0u; num2 < num; num2++)
		{
			array2[num2] = num2;
		}
		return new DrawObject2D(MeshTopology.Triangles, array, uvs, array2, num);
	}

	public static DrawObject2D CreateLineTopologyMeshWithPolygonCoordinates(List<Vector2> vertices)
	{
		int num = 2 * vertices.Count;
		float[] array = new float[num * 2];
		float[] uvs = new float[num * 2];
		uint[] indices = new uint[num];
		FillLineTopologyMeshWithPolygonCoordinates(array, indices, vertices);
		return new DrawObject2D(MeshTopology.Lines, array, uvs, indices, num);
	}

	private static void FillLineTopologyMeshWithPolygonCoordinates(float[] lineTopologyVertices, uint[] indices, List<Vector2> vertices)
	{
		for (int i = 0; i < vertices.Count; i++)
		{
			int index = i;
			int index2 = ((i + 1 != vertices.Count) ? (i + 1) : 0);
			lineTopologyVertices[i * 4] = vertices[index].X;
			lineTopologyVertices[i * 4 + 1] = vertices[index].Y;
			lineTopologyVertices[i * 4 + 2] = vertices[index2].X;
			lineTopologyVertices[i * 4 + 3] = vertices[index2].Y;
			indices[i] = (uint)i;
		}
	}

	public static DrawObject2D CreateLineTopologyMeshWithQuadVertices(float[] quadVertices, uint[] indices, int vertexCount)
	{
		float[] array = new float[vertexCount * 2 * 2];
		float[] uvs = new float[vertexCount * 2 * 2];
		QuadVerticesToLineVertices(quadVertices, vertexCount, array);
		return new DrawObject2D(MeshTopology.Lines, array, uvs, indices, vertexCount);
	}

	public static void QuadVerticesToLineVertices(float[] quadVertices, int vertexCount, float[] lineVertices)
	{
		for (int i = 0; i < vertexCount; i++)
		{
			int num = 2 * i;
			int num2 = ((i + 1 != vertexCount) ? (2 * (i + 1)) : 0);
			lineVertices[i * 4] = quadVertices[num];
			lineVertices[i * 4 + 1] = quadVertices[num + 1];
			lineVertices[i * 4 + 2] = quadVertices[num2];
			lineVertices[i * 4 + 3] = quadVertices[num2 + 1];
		}
	}

	public static DrawObject2D CreateTriangleTopologyMeshWithCircleRadius(float radius)
	{
		_circlePolygonPoints.Clear();
		for (int i = 0; i < _referenceCirclePoints.Count; i++)
		{
			Vector2 item = _referenceCirclePoints[i];
			item.X *= radius;
			item.Y *= radius;
			_circlePolygonPoints.Add(item);
		}
		return CreateTriangleTopologyMeshWithPolygonCoordinates(_circlePolygonPoints);
	}

	public static DrawObject2D CreateLineTopologyMeshWithCircleRadius(float radius)
	{
		_circlePolygonPoints.Clear();
		for (int i = 0; i < _referenceCirclePoints.Count; i++)
		{
			Vector2 item = _referenceCirclePoints[i];
			item.X *= radius;
			item.Y *= radius;
			_circlePolygonPoints.Add(item);
		}
		int num = 2 * _circlePolygonPoints.Count + 2;
		float[] array = new float[num * 2];
		float[] uvs = new float[num * 2];
		uint[] indices = new uint[num];
		FillLineTopologyMeshWithPolygonCoordinates(array, indices, _circlePolygonPoints);
		Vector2 vector = new Vector2(1f, 0f);
		vector.X *= radius;
		vector.Y *= radius;
		array[array.Length - 4] = 0f;
		array[array.Length - 3] = 0f;
		array[array.Length - 2] = vector.X;
		array[array.Length - 1] = vector.Y;
		return new DrawObject2D(MeshTopology.Lines, array, uvs, indices, num);
	}

	public void RecalculateProperties()
	{
		ConvertToHashInPlace(out var hash, out var hash2);
		HashCode1 = hash;
		HashCode2 = hash2;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		for (int i = 0; i < VertexCount; i++)
		{
			float num5 = Vertices[2 * i];
			float num6 = Vertices[2 * i + 1];
			if (num5 < num)
			{
				num = num5;
			}
			if (num6 < num2)
			{
				num2 = num6;
			}
			if (num5 > num3)
			{
				num3 = num5;
			}
			if (num6 > num4)
			{
				num4 = num6;
			}
		}
		BoundingRectangle = new Rectangle(num, num2, num3 - num, num4 - num2);
	}

	public byte[] AsByteArray()
	{
		MeshData meshData = default(MeshData);
		meshData.Topology = Topology;
		meshData.Vertices = Vertices;
		meshData.TextureCoordinates = TextureCoordinates;
		meshData.Indices = Indices;
		meshData.VertexCount = VertexCount;
		return Common.SerializeObject(meshData);
	}

	public void ConvertToHashInPlace(out ulong hash1, out ulong hash2)
	{
		ulong num = 5381uL;
		ulong num2 = 5381uL;
		int num3 = Vertices.Length / 2;
		int num4 = TextureCoordinates.Length / 2;
		int num5 = Indices.Length / 2;
		for (int i = 0; i < num3; i++)
		{
			Buffer.BlockCopy(Vertices, i * 4, floatTemporaryHolder, 0, 4);
			num = (num << 5) + num + floatTemporaryHolder[0];
			num = (num << 5) + num + floatTemporaryHolder[1];
			num = (num << 5) + num + floatTemporaryHolder[2];
			num = (num << 5) + num + floatTemporaryHolder[3];
		}
		for (int j = num3; j < Vertices.Length; j++)
		{
			Buffer.BlockCopy(Vertices, j * 4, floatTemporaryHolder, 0, 4);
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[0];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[1];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[2];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[3];
		}
		for (int k = 0; k < num4; k++)
		{
			Buffer.BlockCopy(TextureCoordinates, k * 4, floatTemporaryHolder, 0, 4);
			num = (num << 5) + num + floatTemporaryHolder[0];
			num = (num << 5) + num + floatTemporaryHolder[1];
			num = (num << 5) + num + floatTemporaryHolder[2];
			num = (num << 5) + num + floatTemporaryHolder[3];
		}
		for (int l = num4; l < TextureCoordinates.Length; l++)
		{
			Buffer.BlockCopy(TextureCoordinates, l * 4, floatTemporaryHolder, 0, 4);
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[0];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[1];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[2];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[3];
		}
		for (int m = 0; m < num5; m++)
		{
			Buffer.BlockCopy(Indices, m * 4, floatTemporaryHolder, 0, 4);
			num = (num << 5) + num + floatTemporaryHolder[0];
			num = (num << 5) + num + floatTemporaryHolder[1];
			num = (num << 5) + num + floatTemporaryHolder[2];
			num = (num << 5) + num + floatTemporaryHolder[3];
		}
		for (int n = num5; n < Indices.Length; n++)
		{
			Buffer.BlockCopy(Indices, n * 4, floatTemporaryHolder, 0, 4);
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[0];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[1];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[2];
			num2 = (num2 << 5) + num2 + floatTemporaryHolder[3];
		}
		num = (num << 5) + num + (byte)Topology;
		uintTemporaryHolder[0] = (uint)VertexCount;
		Buffer.BlockCopy(uintTemporaryHolder, 0, floatTemporaryHolder, 0, 4);
		num2 = (num2 << 5) + num2 + floatTemporaryHolder[0];
		num2 = (num2 << 5) + num2 + floatTemporaryHolder[1];
		num2 = (num2 << 5) + num2 + floatTemporaryHolder[2];
		num2 = (num2 << 5) + num2 + floatTemporaryHolder[3];
		hash1 = num;
		hash2 = num2;
	}

	public static DrawObject2D CreateQuad(Vector2 size)
	{
		DrawObject2D drawObject2D = CreateTriangleTopologyMeshWithPolygonCoordinates(new List<Vector2>
		{
			new Vector2(0f, 0f),
			new Vector2(0f, size.Y),
			new Vector2(size.X, size.Y),
			new Vector2(size.X, 0f)
		});
		drawObject2D.DrawObjectType = DrawObjectType.Quad;
		drawObject2D.TextureCoordinates[0] = 0f;
		drawObject2D.TextureCoordinates[1] = 0f;
		drawObject2D.TextureCoordinates[2] = 0f;
		drawObject2D.TextureCoordinates[3] = 1f;
		drawObject2D.TextureCoordinates[4] = 1f;
		drawObject2D.TextureCoordinates[5] = 1f;
		drawObject2D.TextureCoordinates[6] = 0f;
		drawObject2D.TextureCoordinates[7] = 0f;
		drawObject2D.TextureCoordinates[8] = 1f;
		drawObject2D.TextureCoordinates[9] = 1f;
		drawObject2D.TextureCoordinates[10] = 1f;
		drawObject2D.TextureCoordinates[11] = 0f;
		drawObject2D.Width = size.X;
		drawObject2D.Height = size.Y;
		drawObject2D.MinU = 0f;
		drawObject2D.MaxU = 1f;
		drawObject2D.MinV = 0f;
		drawObject2D.MaxV = 1f;
		return drawObject2D;
	}
}
