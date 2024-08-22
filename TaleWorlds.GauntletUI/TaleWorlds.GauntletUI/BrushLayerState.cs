using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI;

public struct BrushLayerState : IBrushAnimationState, IDataSource
{
	public Color Color;

	public float ColorFactor;

	public float AlphaFactor;

	public float HueFactor;

	public float SaturationFactor;

	public float ValueFactor;

	public float OverlayXOffset;

	public float OverlayYOffset;

	public float XOffset;

	public float YOffset;

	public Sprite Sprite;

	public void FillFrom(IBrushLayerData styleLayer)
	{
		ColorFactor = styleLayer.ColorFactor;
		AlphaFactor = styleLayer.AlphaFactor;
		HueFactor = styleLayer.HueFactor;
		SaturationFactor = styleLayer.SaturationFactor;
		ValueFactor = styleLayer.ValueFactor;
		Color = styleLayer.Color;
		OverlayXOffset = styleLayer.OverlayXOffset;
		OverlayYOffset = styleLayer.OverlayYOffset;
		XOffset = styleLayer.XOffset;
		YOffset = styleLayer.YOffset;
		Sprite = styleLayer.Sprite;
	}

	void IBrushAnimationState.FillFrom(IDataSource source)
	{
		StyleLayer styleLayer = (StyleLayer)source;
		FillFrom(styleLayer);
	}

	void IBrushAnimationState.LerpFrom(IBrushAnimationState start, IDataSource end, float ratio)
	{
		BrushLayerState start2 = (BrushLayerState)(object)start;
		IBrushLayerData end2 = (IBrushLayerData)end;
		LerpFrom(start2, end2, ratio);
	}

	public void LerpFrom(BrushLayerState start, IBrushLayerData end, float ratio)
	{
		ColorFactor = Mathf.Lerp(start.ColorFactor, end.ColorFactor, ratio);
		AlphaFactor = Mathf.Lerp(start.AlphaFactor, end.AlphaFactor, ratio);
		HueFactor = Mathf.Lerp(start.HueFactor, end.HueFactor, ratio);
		SaturationFactor = Mathf.Lerp(start.SaturationFactor, end.SaturationFactor, ratio);
		ValueFactor = Mathf.Lerp(start.ValueFactor, end.ValueFactor, ratio);
		Color = Color.Lerp(start.Color, end.Color, ratio);
		OverlayXOffset = Mathf.Lerp(start.OverlayXOffset, end.OverlayXOffset, ratio);
		OverlayYOffset = Mathf.Lerp(start.OverlayYOffset, end.OverlayYOffset, ratio);
		XOffset = Mathf.Lerp(start.XOffset, end.XOffset, ratio);
		YOffset = Mathf.Lerp(start.YOffset, end.YOffset, ratio);
		Sprite = ((ratio > 0.9f) ? end.Sprite : start.Sprite);
	}

	public void SetValueAsFloat(BrushAnimationProperty.BrushAnimationPropertyType propertyType, float value)
	{
		switch (propertyType)
		{
		case BrushAnimationProperty.BrushAnimationPropertyType.ColorFactor:
			ColorFactor = value;
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.AlphaFactor:
			AlphaFactor = value;
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.HueFactor:
			HueFactor = value;
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.SaturationFactor:
			SaturationFactor = value;
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.ValueFactor:
			ValueFactor = value;
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.OverlayXOffset:
			OverlayXOffset = value;
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.OverlayYOffset:
			OverlayYOffset = value;
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.XOffset:
			XOffset = value;
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.YOffset:
			YOffset = value;
			break;
		default:
			Debug.FailedAssert("Invalid value type or property name for data source.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\Brush\\BrushLayerState.cs", "SetValueAsFloat", 109);
			break;
		}
	}

	public void SetValueAsColor(BrushAnimationProperty.BrushAnimationPropertyType propertyType, in Color value)
	{
		if (propertyType == BrushAnimationProperty.BrushAnimationPropertyType.Color)
		{
			Color = value;
		}
		else
		{
			Debug.FailedAssert("Invalid value type or property name for data source.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\Brush\\BrushLayerState.cs", "SetValueAsColor", 122);
		}
	}

	public void SetValueAsSprite(BrushAnimationProperty.BrushAnimationPropertyType propertyType, Sprite value)
	{
		if (propertyType == BrushAnimationProperty.BrushAnimationPropertyType.Sprite)
		{
			Sprite = value;
		}
		else
		{
			Debug.FailedAssert("Invalid value type or property name for data source.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\Brush\\BrushLayerState.cs", "SetValueAsSprite", 135);
		}
	}

	public float GetValueAsFloat(BrushAnimationProperty.BrushAnimationPropertyType propertyType)
	{
		switch (propertyType)
		{
		case BrushAnimationProperty.BrushAnimationPropertyType.ColorFactor:
			return ColorFactor;
		case BrushAnimationProperty.BrushAnimationPropertyType.AlphaFactor:
			return AlphaFactor;
		case BrushAnimationProperty.BrushAnimationPropertyType.HueFactor:
			return HueFactor;
		case BrushAnimationProperty.BrushAnimationPropertyType.SaturationFactor:
			return SaturationFactor;
		case BrushAnimationProperty.BrushAnimationPropertyType.ValueFactor:
			return ValueFactor;
		case BrushAnimationProperty.BrushAnimationPropertyType.OverlayXOffset:
			return OverlayXOffset;
		case BrushAnimationProperty.BrushAnimationPropertyType.OverlayYOffset:
			return OverlayYOffset;
		case BrushAnimationProperty.BrushAnimationPropertyType.XOffset:
			return XOffset;
		case BrushAnimationProperty.BrushAnimationPropertyType.YOffset:
			return YOffset;
		default:
			Debug.FailedAssert("Invalid value type or property name for data source.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\Brush\\BrushLayerState.cs", "GetValueAsFloat", 163);
			return 0f;
		}
	}

	public Color GetValueAsColor(BrushAnimationProperty.BrushAnimationPropertyType propertyType)
	{
		if (propertyType == BrushAnimationProperty.BrushAnimationPropertyType.Color)
		{
			return Color;
		}
		Debug.FailedAssert("Invalid value type or property name for data source.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\Brush\\BrushLayerState.cs", "GetValueAsColor", 175);
		return Color.Black;
	}

	public Sprite GetValueAsSprite(BrushAnimationProperty.BrushAnimationPropertyType propertyType)
	{
		if (propertyType == BrushAnimationProperty.BrushAnimationPropertyType.Sprite)
		{
			return Sprite;
		}
		Debug.FailedAssert("Invalid value type or property name for data source.", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\Brush\\BrushLayerState.cs", "GetValueAsSprite", 187);
		return null;
	}

	public static void SetValueAsLerpOfValues(ref BrushLayerState currentState, in BrushAnimationKeyFrame startValue, in BrushAnimationKeyFrame endValue, BrushAnimationProperty.BrushAnimationPropertyType propertyType, float ratio)
	{
		switch (propertyType)
		{
		case BrushAnimationProperty.BrushAnimationPropertyType.ColorFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.AlphaFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.HueFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.SaturationFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.ValueFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.OverlayXOffset:
		case BrushAnimationProperty.BrushAnimationPropertyType.OverlayYOffset:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextOutlineAmount:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextGlowRadius:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextBlur:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextShadowOffset:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextShadowAngle:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextColorFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextAlphaFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextHueFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextSaturationFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextValueFactor:
		case BrushAnimationProperty.BrushAnimationPropertyType.XOffset:
		case BrushAnimationProperty.BrushAnimationPropertyType.YOffset:
			currentState.SetValueAsFloat(propertyType, MathF.Lerp(startValue.GetValueAsFloat(), endValue.GetValueAsFloat(), ratio));
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.Color:
		case BrushAnimationProperty.BrushAnimationPropertyType.FontColor:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextGlowColor:
		case BrushAnimationProperty.BrushAnimationPropertyType.TextOutlineColor:
		{
			Color value = Color.Lerp(startValue.GetValueAsColor(), endValue.GetValueAsColor(), ratio);
			currentState.SetValueAsColor(propertyType, in value);
			break;
		}
		case BrushAnimationProperty.BrushAnimationPropertyType.Sprite:
			currentState.SetValueAsSprite(propertyType, ((double)ratio > 0.9) ? endValue.GetValueAsSprite() : startValue.GetValueAsSprite());
			break;
		case BrushAnimationProperty.BrushAnimationPropertyType.IsHidden:
			break;
		}
	}

	void IBrushAnimationState.SetValueAsColor(BrushAnimationProperty.BrushAnimationPropertyType propertyType, in Color value)
	{
		SetValueAsColor(propertyType, in value);
	}
}
