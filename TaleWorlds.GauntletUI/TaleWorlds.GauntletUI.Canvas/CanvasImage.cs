using System.Numerics;
using System.Xml;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public class CanvasImage : CanvasElement
{
	public Sprite Sprite { get; set; }

	public CanvasImage(CanvasObject parent, FontFactory fontFactory, SpriteData spriteData)
		: base(parent, fontFactory, spriteData)
	{
	}

	public override void LoadFrom(XmlNode canvasImageNode)
	{
		base.LoadFrom(canvasImageNode);
		foreach (XmlAttribute attribute in canvasImageNode.Attributes)
		{
			string name = attribute.Name;
			string value = attribute.Value;
			if (name == "Sprite")
			{
				Sprite = base.SpriteData.GetSprite(value);
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
