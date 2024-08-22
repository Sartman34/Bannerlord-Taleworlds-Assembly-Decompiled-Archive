using System;
using System.Globalization;
using System.Numerics;
using System.Xml;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public abstract class CanvasElement : CanvasObject
{
	public bool _usingRelativeX;

	public bool _usingRelativeY;

	private float _relativePositionX;

	private float _relativePositionY;

	public float PositionX { get; set; }

	public float PositionY { get; set; }

	public float RelativePositionX
	{
		get
		{
			return _relativePositionX;
		}
		set
		{
			_relativePositionX = value;
			_usingRelativeX = true;
		}
	}

	public float RelativePositionY
	{
		get
		{
			return _relativePositionY;
		}
		set
		{
			_relativePositionY = value;
			_usingRelativeY = true;
		}
	}

	public float PivotX { get; set; }

	public float PivotY { get; set; }

	protected CanvasElement(CanvasObject parent, FontFactory fontFactory, SpriteData spriteData)
		: base(parent, fontFactory, spriteData)
	{
	}

	public virtual void LoadFrom(XmlNode canvasImageNode)
	{
		foreach (XmlAttribute attribute in canvasImageNode.Attributes)
		{
			string name = attribute.Name;
			string value = attribute.Value;
			switch (name)
			{
			case "PositionX":
				PositionX = Convert.ToSingle(value, CultureInfo.InvariantCulture);
				break;
			case "PositionY":
				PositionY = Convert.ToSingle(value, CultureInfo.InvariantCulture);
				break;
			case "RelativePositionX":
				RelativePositionX = Convert.ToSingle(value, CultureInfo.InvariantCulture);
				break;
			case "RelativePositionY":
				RelativePositionY = Convert.ToSingle(value, CultureInfo.InvariantCulture);
				break;
			case "PivotX":
				PivotX = Convert.ToSingle(value, CultureInfo.InvariantCulture);
				break;
			case "PivotY":
				PivotY = Convert.ToSingle(value, CultureInfo.InvariantCulture);
				break;
			}
		}
	}

	protected sealed override Vector2 Layout()
	{
		Vector2 result = new Vector2(PositionX, PositionY);
		if (_usingRelativeX)
		{
			result.X = base.Parent.Width * RelativePositionX;
		}
		else
		{
			result.X *= base.Scale;
		}
		if (_usingRelativeY)
		{
			result.Y = base.Parent.Height * RelativePositionY;
		}
		else
		{
			result.Y *= base.Scale;
		}
		result.X -= PivotX * base.Width;
		result.Y -= PivotY * base.Height;
		return result;
	}

	public override Vector2 GetMarginSize()
	{
		return new Vector2(PositionX * base.Scale, PositionY * base.Scale);
	}
}
