using System.Collections.Generic;

namespace TaleWorlds.TwoDimension;

internal class TwoDimensionDrawLayer
{
	private List<TwoDimensionDrawData> _drawData;

	public TwoDimensionDrawLayer()
	{
		_drawData = new List<TwoDimensionDrawData>(2);
	}

	public void Reset()
	{
		_drawData.Clear();
	}

	public void AddDrawData(TwoDimensionDrawData drawData)
	{
		_drawData.Add(drawData);
	}

	public void DrawTo(TwoDimensionContext twoDimensionContext, int layer)
	{
		for (int i = 0; i < _drawData.Count; i++)
		{
			_drawData[i].DrawTo(twoDimensionContext, layer);
		}
	}

	public bool IsIntersects(Rectangle rectangle)
	{
		for (int i = 0; i < _drawData.Count; i++)
		{
			if (_drawData[i].IsIntersects(rectangle))
			{
				return true;
			}
		}
		return false;
	}
}
