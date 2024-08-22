using System;
using System.Globalization;
using System.Numerics;
using System.Xml;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public class CanvasLineText : CanvasLineElement
{
	private readonly Text _text;

	public string Value
	{
		get
		{
			return _text.Value;
		}
		set
		{
			_text.CurrentLanguage = base.FontFactory.GetCurrentLanguage();
			_text.Value = value;
		}
	}

	public float FontSize { get; set; }

	public Color FontColor { get; set; }

	public CanvasLineText(CanvasLine line, int segmentIndex, FontFactory fontFactory, SpriteData spriteData)
		: base(line, segmentIndex, fontFactory, spriteData)
	{
		_text = new Text(400, 400, fontFactory.DefaultFont, fontFactory.GetUsableFontForCharacter);
		FontColor = Color.White;
	}

	public void LoadFrom(XmlNode textNode)
	{
		foreach (XmlAttribute attribute in textNode.Attributes)
		{
			string name = attribute.Name;
			string value = attribute.Value;
			switch (name)
			{
			case "Value":
				Value = value;
				break;
			case "FontSize":
				FontSize = Convert.ToSingle(value, CultureInfo.InvariantCulture);
				break;
			case "FontColor":
				FontColor = Color.ConvertStringToColor(value);
				break;
			}
		}
	}

	public override void Update(float scale)
	{
		base.Update(scale);
		_text.FontSize = FontSize * scale;
	}

	protected override Vector2 Measure()
	{
		return _text.GetPreferredSize(fixedWidth: false, 0f, fixedHeight: false, 0f, null, 1f);
	}

	protected override void Render(Vector2 globalPosition, TwoDimensionDrawContext drawContext)
	{
		TextMaterial textMaterial = new TextMaterial();
		textMaterial.Color = FontColor;
		Vector2 vector = globalPosition + base.LocalPosition;
		drawContext.Draw(_text, textMaterial, vector.X, vector.Y, base.Width, base.Height);
	}
}
