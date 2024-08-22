using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public class CanvasTextBox : CanvasElement
{
	private List<CanvasLine> _lines;

	public CanvasTextBox(CanvasObject parent, FontFactory fontFactory, SpriteData spriteData)
		: base(parent, fontFactory, spriteData)
	{
		_lines = new List<CanvasLine>();
	}

	public override void LoadFrom(XmlNode canvasTextNode)
	{
		base.LoadFrom(canvasTextNode);
		int num = 0;
		foreach (XmlNode item in canvasTextNode)
		{
			CanvasLine canvasLine = new CanvasLine(this, num, base.FontFactory, base.SpriteData);
			canvasLine.LoadFrom(item);
			_lines.Add(canvasLine);
			base.Children.Add(canvasLine);
			num++;
		}
	}

	protected override Vector2 Measure()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (CanvasLine line in _lines)
		{
			num = Mathf.Max(num, line.Width);
			num2 += line.Height;
		}
		return new Vector2(num, num2);
	}

	public float GetVerticalPositionOf(int index)
	{
		float num = 0f;
		int num2 = 0;
		foreach (CanvasLine line in _lines)
		{
			if (num2 == index)
			{
				break;
			}
			num += line.Height;
			num2++;
		}
		return num;
	}
}
