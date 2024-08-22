using System.Numerics;
using System.Xml;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public class CanvasLineImage : CanvasLineElement
{
	private FontFactory _fontFactory;

	private SpriteData _spriteData;

	public Sprite Sprite { get; set; }

	public CanvasLineImage(CanvasLine line, int segmentIndex, FontFactory fontFactory, SpriteData spriteData)
		: base(line, segmentIndex, fontFactory, spriteData)
	{
		_fontFactory = fontFactory;
		_spriteData = spriteData;
	}

	public void LoadFrom(XmlNode imageNode)
	{
		foreach (XmlAttribute attribute in imageNode.Attributes)
		{
			string name = attribute.Name;
			string value = attribute.Value;
			if (name == "Sprite")
			{
				Sprite = _spriteData.GetSprite(value);
			}
		}
	}

	protected override Vector2 Measure()
	{
		Vector2 zero = Vector2.Zero;
		if (Sprite != null)
		{
			zero.X = (float)Sprite.Width * base.Scale;
			zero.Y = (float)Sprite.Height * base.Scale;
		}
		return zero;
	}

	protected override void Render(Vector2 globalPosition, TwoDimensionDrawContext drawContext)
	{
		if (Sprite != null)
		{
			Texture texture = Sprite.Texture;
			if (texture != null)
			{
				SimpleMaterial simpleMaterial = new SimpleMaterial();
				simpleMaterial.Texture = texture;
				Vector2 vector = globalPosition + base.LocalPosition;
				drawContext.DrawSprite(Sprite, simpleMaterial, vector.X, vector.Y, base.Scale, base.Width, base.Height, horizontalFlip: false, verticalFlip: false);
			}
		}
	}
}
