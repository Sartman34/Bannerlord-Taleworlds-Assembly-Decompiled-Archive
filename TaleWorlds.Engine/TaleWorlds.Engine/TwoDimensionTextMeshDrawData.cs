using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[EngineStruct("rglTwo_dimension_text_mesh_draw_data", false)]
public struct TwoDimensionTextMeshDrawData
{
	public float DrawX;

	public float DrawY;

	public float ScreenWidth;

	public float ScreenHeight;

	public uint Color;

	public float ScaleFactor;

	public float SmoothingConstant;

	public float ColorFactor;

	public float AlphaFactor;

	public float HueFactor;

	public float SaturationFactor;

	public float ValueFactor;

	public Vec2 ClipRectPosition;

	public Vec2 ClipRectSize;

	public uint GlowColor;

	public Vec3 OutlineColor;

	public float OutlineAmount;

	public float GlowRadius;

	public float Blur;

	public float ShadowOffset;

	public float ShadowAngle;

	public int Layer;

	public ulong HashCode1;

	public ulong HashCode2;
}
