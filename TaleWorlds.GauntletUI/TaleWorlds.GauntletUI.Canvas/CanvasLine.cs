using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public class CanvasLine : CanvasObject
{
	private List<CanvasLineElement> _elements;

	private CanvasTextBox _textBox;

	private int _lineIndex;

	public CanvasLineAlignment Alignment { get; set; }

	public CanvasLine(CanvasTextBox textBox, int lineIndex, FontFactory fontFactory, SpriteData spriteData)
		: base(textBox, fontFactory, spriteData)
	{
		_elements = new List<CanvasLineElement>();
		_lineIndex = lineIndex;
		_textBox = textBox;
	}

	protected override Vector2 Measure()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (CanvasLineElement element in _elements)
		{
			num += element.Width;
			num2 = Mathf.Max(num2, element.Height);
		}
		return new Vector2(num, num2);
	}

	protected override Vector2 Layout()
	{
		Vector2 zero = Vector2.Zero;
		if (Alignment == CanvasLineAlignment.Left)
		{
			zero.X = 0f;
		}
		else if (Alignment == CanvasLineAlignment.Center)
		{
			zero.X = (base.Parent.Width - base.Width) * 0.5f;
		}
		else if (Alignment == CanvasLineAlignment.Right)
		{
			zero.X = base.Parent.Width - base.Width;
		}
		zero.Y = _textBox.GetVerticalPositionOf(_lineIndex);
		return zero;
	}

	public void LoadFrom(XmlNode lineNode)
	{
		foreach (XmlAttribute attribute in lineNode.Attributes)
		{
			string name = attribute.Name;
			string value = attribute.Value;
			if (name == "Alignment")
			{
				Alignment = (CanvasLineAlignment)Enum.Parse(typeof(CanvasLineAlignment), value);
			}
		}
		int num = 0;
		foreach (XmlNode item in lineNode)
		{
			CanvasLineElement canvasLineElement = null;
			if (item.Name == "LineImage")
			{
				CanvasLineImage canvasLineImage = new CanvasLineImage(this, num, base.FontFactory, base.SpriteData);
				canvasLineImage.LoadFrom(item);
				canvasLineElement = canvasLineImage;
			}
			else if (item.Name == "Text")
			{
				CanvasLineText canvasLineText = new CanvasLineText(this, num, base.FontFactory, base.SpriteData);
				canvasLineText.LoadFrom(item);
				canvasLineElement = canvasLineText;
			}
			if (canvasLineElement != null)
			{
				_elements.Add(canvasLineElement);
				base.Children.Add(canvasLineElement);
				num++;
			}
		}
	}

	public float GetHorizontalPositionOf(int index)
	{
		float num = 0f;
		int num2 = 0;
		foreach (CanvasLineElement element in _elements)
		{
			if (num2 == index)
			{
				break;
			}
			num += element.Width;
			num2++;
		}
		return num;
	}
}
