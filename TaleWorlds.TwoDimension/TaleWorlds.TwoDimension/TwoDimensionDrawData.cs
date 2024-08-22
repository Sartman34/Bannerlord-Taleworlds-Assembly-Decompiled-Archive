namespace TaleWorlds.TwoDimension;

internal struct TwoDimensionDrawData
{
	private bool _scissorTestEnabled;

	private ScissorTestInfo _scissorTestInfo;

	private float _x;

	private float _y;

	private Material _material;

	private DrawObject2D _drawObject2D;

	private Rectangle _rectangle;

	public Rectangle Rectangle => _rectangle;

	public TwoDimensionDrawData(bool scissorTestEnabled, ScissorTestInfo scissorTestInfo, float x, float y, Material material, DrawObject2D drawObject2D, float width, float height)
	{
		_scissorTestEnabled = scissorTestEnabled;
		_scissorTestInfo = scissorTestInfo;
		_x = x;
		_y = y;
		_material = material;
		_drawObject2D = drawObject2D;
		_rectangle = new Rectangle(_x, _y, width, height);
	}

	public bool IsIntersects(Rectangle rectangle)
	{
		return _rectangle.IsCollide(rectangle);
	}

	public void DrawTo(TwoDimensionContext twoDimensionContext, int layer)
	{
		if (_scissorTestEnabled)
		{
			twoDimensionContext.SetScissor(_scissorTestInfo);
		}
		twoDimensionContext.Draw(_x, _y, _material, _drawObject2D, layer);
		if (_scissorTestEnabled)
		{
			twoDimensionContext.ResetScissor();
		}
	}
}
