using System.Numerics;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public abstract class CanvasLineElement : CanvasObject
{
	public CanvasLine Line { get; set; }

	public int SegmentIndex { get; set; }

	protected CanvasLineElement(CanvasLine line, int segmentIndex, FontFactory fontFactory, SpriteData spriteData)
		: base(line, fontFactory, spriteData)
	{
		Line = line;
		SegmentIndex = segmentIndex;
	}

	protected sealed override Vector2 Layout()
	{
		Vector2 zero = Vector2.Zero;
		zero.X = Line.GetHorizontalPositionOf(SegmentIndex);
		zero.Y = Line.Height - base.Height;
		return zero;
	}
}
