using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[EngineStruct("rglTwo_dimension_mesh_draw_data", false)]
public struct TwoDimensionMeshDrawData
{
	public float DrawX;

	public float DrawY;

	public float ScreenWidth;

	public float ScreenHeight;

	public Vec2 ClipCircleCenter;

	public float ClipCircleRadius;

	public float ClipCircleSmoothingRadius;

	public uint Color;

	public float ColorFactor;

	public float AlphaFactor;

	public float HueFactor;

	public float SaturationFactor;

	public float ValueFactor;

	public float OverlayTextureWidth;

	public float OverlayTextureHeight;

	public Vec2 ClipRectPosition;

	public Vec2 ClipRectSize;

	public Vec2 StartCoordinate;

	public Vec2 Size;

	public int Layer;

	public float OverlayXOffset;

	public float OverlayYOffset;

	public float Width;

	public float Height;

	public float MinU;

	public float MinV;

	public float MaxU;

	public float MaxV;

	public uint Type;
}
