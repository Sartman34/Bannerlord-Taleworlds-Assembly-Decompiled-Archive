using System.Numerics;
using System.Xml;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public class Canvas
{
	private SpriteData _spriteData;

	private FontFactory _fontFactory;

	private CanvasObject _root;

	public CanvasObject Root => _root;

	public Canvas(SpriteData spriteData, FontFactory fontFactory)
	{
		_spriteData = spriteData;
		_fontFactory = fontFactory;
	}

	public void LoadFrom(XmlNode canvasNode)
	{
		_root = null;
		if (canvasNode == null)
		{
			return;
		}
		_root = new CanvasObject(null, _fontFactory, _spriteData);
		foreach (XmlNode item in canvasNode)
		{
			CanvasElement canvasElement = null;
			if (item.Name == "Image")
			{
				CanvasImage canvasImage = new CanvasImage(_root, _fontFactory, _spriteData);
				canvasImage.LoadFrom(item);
				canvasElement = canvasImage;
			}
			else if (item.Name == "TextBox")
			{
				CanvasTextBox canvasTextBox = new CanvasTextBox(_root, _fontFactory, _spriteData);
				canvasTextBox.LoadFrom(item);
				canvasElement = canvasTextBox;
			}
			if (canvasElement != null)
			{
				_root.Children.Add(canvasElement);
			}
		}
	}

	public void Update(float scale)
	{
		_root.Update(scale);
	}

	public void DoMeasure(bool fixedWidth, bool fixedHeight, float width, float height)
	{
		_root.BeginMeasure(fixedWidth, fixedHeight, width, height);
	}

	public void DoLayout()
	{
		_root.DoLayout();
	}

	public void DoRender(Vector2 globalPosition, TwoDimensionDrawContext drawContext)
	{
		_root.DoRender(globalPosition, drawContext);
	}
}
