using System.Numerics;
using System.Xml;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.Canvas;

public class CanvasWidget : Widget, ILayout
{
	private ILayout _defaultLayout;

	private bool _requiresUpdate;

	private XmlElement _canvasNode;

	private Canvas _canvas;

	[Editor(false)]
	public string CanvasAsString
	{
		get
		{
			return _canvasNode.ToString();
		}
		set
		{
			if ((_canvasNode == null && value != null) || _canvasNode.ToString() != value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(value);
					_canvasNode = xmlDocument.DocumentElement;
				}
				else
				{
					_canvasNode = null;
				}
				_requiresUpdate = true;
				OnPropertyChanged(value, "CanvasAsString");
				SetMeasureAndLayoutDirty();
			}
		}
	}

	public XmlElement CanvasNode
	{
		get
		{
			return _canvasNode;
		}
		set
		{
			if (_canvasNode != value)
			{
				_canvasNode = value;
				_requiresUpdate = true;
				OnPropertyChanged(value, "CanvasNode");
				SetMeasureAndLayoutDirty();
			}
		}
	}

	public CanvasWidget(UIContext context)
		: base(context)
	{
		_defaultLayout = new DefaultLayout();
		base.LayoutImp = this;
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		DoUpdate();
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		DoUpdate();
	}

	private void DoUpdate()
	{
		if (_requiresUpdate || _canvas == null)
		{
			UpdateCanvas();
		}
		_canvas.Update(base._scaleToUse);
	}

	private void UpdateCanvas()
	{
		_canvas = new Canvas(base.EventManager.Context.SpriteData, base.EventManager.Context.FontFactory);
		_canvas.LoadFrom(CanvasNode);
		_requiresUpdate = false;
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		base.OnRender(twoDimensionContext, drawContext);
		if (_canvas != null)
		{
			_canvas.DoRender(base.GlobalPosition, drawContext);
		}
	}

	Vector2 ILayout.MeasureChildren(Widget widget, Vector2 measureSpec, SpriteData spriteData, float renderScale)
	{
		Vector2 result = _defaultLayout.MeasureChildren(widget, measureSpec, spriteData, renderScale);
		if (_canvas != null)
		{
			_canvas.DoMeasure(base.WidthSizePolicy != SizePolicy.CoverChildren || base.MaxWidth != 0f, base.HeightSizePolicy != SizePolicy.CoverChildren || base.MaxHeight != 0f, measureSpec.X, measureSpec.Y);
			result.X = Mathf.Max(_canvas.Root.Width, result.X);
			result.Y = Mathf.Max(_canvas.Root.Height, result.Y);
		}
		return result;
	}

	void ILayout.OnLayout(Widget widget, float left, float bottom, float right, float top)
	{
		if (_canvas != null)
		{
			_canvas.DoLayout();
		}
		_defaultLayout.OnLayout(widget, left, bottom, right, top);
	}
}
