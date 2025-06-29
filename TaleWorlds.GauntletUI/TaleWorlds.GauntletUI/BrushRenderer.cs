using System;
using System.Collections.Generic;
using System.Numerics;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI;

public class BrushRenderer
{
	public enum BrushRendererAnimationState
	{
		None,
		PlayingAnimation,
		PlayingBasicTranisition,
		Ended
	}

	private BrushState _startBrushState;

	private BrushState _currentBrushState;

	private Dictionary<string, BrushLayerState> _startBrushLayerState;

	private Dictionary<string, BrushLayerState> _currentBrushLayerState;

	public bool UseLocalTimer;

	private float _brushLocalTimer;

	private float _globalTime;

	private int _offsetSeed;

	private float _randomXOffset;

	private float _randomYOffset;

	private BrushRendererAnimationState _brushRendererAnimationState;

	private Brush _brush;

	private long _latestStyleVersion;

	private string _currentState;

	private Style _styleOfCurrentState;

	private float _brushTimer
	{
		get
		{
			if (!UseLocalTimer)
			{
				return _globalTime;
			}
			return _brushLocalTimer;
		}
	}

	public bool ForcePixelPerfectPlacement { get; set; }

	public Style CurrentStyle => _styleOfCurrentState;

	public Brush Brush
	{
		get
		{
			return _brush;
		}
		set
		{
			if (_brush == value)
			{
				return;
			}
			_brush = value;
			_brushLocalTimer = 0f;
			int capacity = ((_brush != null) ? _brush.Layers.Count : 0);
			if (_startBrushLayerState == null)
			{
				_startBrushLayerState = new Dictionary<string, BrushLayerState>(capacity);
				_currentBrushLayerState = new Dictionary<string, BrushLayerState>(capacity);
			}
			else
			{
				_startBrushLayerState.Clear();
				_currentBrushLayerState.Clear();
			}
			if (_brush == null)
			{
				return;
			}
			Style defaultStyle = _brush.DefaultStyle;
			BrushState brushState = default(BrushState);
			brushState.FillFrom(defaultStyle);
			_startBrushState = brushState;
			_currentBrushState = brushState;
			foreach (BrushLayer layer in _brush.Layers)
			{
				BrushLayerState value2 = default(BrushLayerState);
				value2.FillFrom(layer);
				_startBrushLayerState[layer.Name] = value2;
				_currentBrushLayerState[layer.Name] = value2;
			}
			if (!string.IsNullOrEmpty(CurrentState))
			{
				_styleOfCurrentState = Brush.GetStyleOrDefault(CurrentState);
			}
		}
	}

	public string CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			if (!(_currentState != value))
			{
				return;
			}
			string currentState = _currentState;
			_brushLocalTimer = 0f;
			_currentState = value;
			_startBrushState = _currentBrushState;
			foreach (KeyValuePair<string, BrushLayerState> item in _currentBrushLayerState)
			{
				_startBrushLayerState[item.Key] = item.Value;
			}
			if (Brush == null)
			{
				return;
			}
			Style style = (_styleOfCurrentState = Brush.GetStyleOrDefault(CurrentState));
			_brushRendererAnimationState = BrushRendererAnimationState.None;
			if (style.AnimationMode == StyleAnimationMode.BasicTransition)
			{
				if (!string.IsNullOrEmpty(currentState))
				{
					_brushRendererAnimationState = BrushRendererAnimationState.PlayingBasicTranisition;
				}
			}
			else if (style.AnimationMode == StyleAnimationMode.Animation && (!string.IsNullOrEmpty(currentState) || !string.IsNullOrEmpty(style.AnimationToPlayOnBegin)))
			{
				_brushRendererAnimationState = BrushRendererAnimationState.PlayingAnimation;
			}
		}
	}

	public BrushRenderer()
	{
		_startBrushState = default(BrushState);
		_currentBrushState = default(BrushState);
		_startBrushLayerState = new Dictionary<string, BrushLayerState>();
		_currentBrushLayerState = new Dictionary<string, BrushLayerState>();
		_brushLocalTimer = 0f;
		_brushRendererAnimationState = BrushRendererAnimationState.None;
		_randomXOffset = -1f;
		_randomYOffset = -1f;
	}

	private float GetRandomXOffset()
	{
		if (_randomXOffset < 0f)
		{
			Random random = new Random(_offsetSeed);
			_randomXOffset = random.Next(0, 2048);
			_randomYOffset = random.Next(0, 2048);
		}
		return _randomXOffset;
	}

	private float GetRandomYOffset()
	{
		if (_randomYOffset < 0f)
		{
			Random random = new Random(_offsetSeed);
			_randomXOffset = random.Next(0, 2048);
			_randomYOffset = random.Next(0, 2048);
		}
		return _randomYOffset;
	}

	public void Update(float globalAnimTime, float dt)
	{
		_globalTime = globalAnimTime;
		_brushLocalTimer += dt;
		if (Brush == null)
		{
			return;
		}
		Style styleOfCurrentState = _styleOfCurrentState;
		if ((_brushRendererAnimationState == BrushRendererAnimationState.None || _brushRendererAnimationState == BrushRendererAnimationState.Ended) && (!string.IsNullOrEmpty(styleOfCurrentState.AnimationToPlayOnBegin) || _styleOfCurrentState.Version != _latestStyleVersion))
		{
			_latestStyleVersion = styleOfCurrentState.Version;
			BrushState brushState = default(BrushState);
			brushState.FillFrom(styleOfCurrentState);
			_startBrushState = brushState;
			_currentBrushState = brushState;
			StyleLayer[] layers = styleOfCurrentState.GetLayers();
			foreach (StyleLayer styleLayer in layers)
			{
				BrushLayerState value = default(BrushLayerState);
				value.FillFrom(styleLayer);
				_currentBrushLayerState[styleLayer.Name] = value;
				_startBrushLayerState[styleLayer.Name] = value;
			}
		}
		else if (_brushRendererAnimationState == BrushRendererAnimationState.PlayingBasicTranisition)
		{
			float num = (UseLocalTimer ? _brushLocalTimer : globalAnimTime);
			if (num >= Brush.TransitionDuration)
			{
				EndAnimation();
				return;
			}
			float num2 = num / Brush.TransitionDuration;
			if (num2 > 1f)
			{
				num2 = 1f;
			}
			BrushState startBrushState = _startBrushState;
			BrushState currentBrushState = default(BrushState);
			currentBrushState.LerpFrom(startBrushState, styleOfCurrentState, num2);
			_currentBrushState = currentBrushState;
			StyleLayer[] layers = styleOfCurrentState.GetLayers();
			foreach (StyleLayer styleLayer2 in layers)
			{
				BrushLayerState start = _startBrushLayerState[styleLayer2.Name];
				BrushLayerState value2 = default(BrushLayerState);
				value2.LerpFrom(start, styleLayer2, num2);
				_currentBrushLayerState[styleLayer2.Name] = value2;
			}
		}
		else
		{
			if (_brushRendererAnimationState != BrushRendererAnimationState.PlayingAnimation)
			{
				return;
			}
			string animationToPlayOnBegin = styleOfCurrentState.AnimationToPlayOnBegin;
			BrushAnimation animation = Brush.GetAnimation(animationToPlayOnBegin);
			if (animation == null || (!animation.Loop && _brushTimer >= animation.Duration))
			{
				EndAnimation();
				return;
			}
			float brushStateTimer = _brushTimer % animation.Duration;
			bool isFirstCycle = _brushTimer < animation.Duration;
			BrushState startBrushState2 = _startBrushState;
			BrushLayerAnimation styleAnimation = animation.StyleAnimation;
			BrushState currentBrushState2 = AnimateBrushState(animation, styleAnimation, brushStateTimer, isFirstCycle, startBrushState2, styleOfCurrentState);
			_currentBrushState = currentBrushState2;
			StyleLayer[] layers = styleOfCurrentState.GetLayers();
			foreach (StyleLayer styleLayer3 in layers)
			{
				BrushLayerState startState = _startBrushLayerState[styleLayer3.Name];
				BrushLayerAnimation layerAnimation = animation.GetLayerAnimation(styleLayer3.Name);
				BrushLayerState value3 = AnimateBrushLayerState(animation, layerAnimation, brushStateTimer, isFirstCycle, startState, styleLayer3);
				_currentBrushLayerState[styleLayer3.Name] = value3;
			}
		}
	}

	private BrushLayerState AnimateBrushLayerState(BrushAnimation animation, BrushLayerAnimation layerAnimation, float brushStateTimer, bool isFirstCycle, BrushLayerState startState, IBrushLayerData source)
	{
		BrushLayerState result = default(BrushLayerState);
		result.FillFrom(source);
		if (layerAnimation != null)
		{
			foreach (BrushAnimationProperty collection in layerAnimation.Collections)
			{
				BrushAnimationProperty.BrushAnimationPropertyType propertyType = collection.PropertyType;
				BrushAnimationKeyFrame brushAnimationKeyFrame = null;
				BrushAnimationKeyFrame brushAnimationKeyFrame2 = null;
				if (animation.Loop)
				{
					BrushAnimationKeyFrame frameAt = collection.GetFrameAt(0);
					if (isFirstCycle && _brushTimer < frameAt.Time)
					{
						brushAnimationKeyFrame = frameAt;
					}
					else
					{
						brushAnimationKeyFrame = collection.GetFrameAfter(brushStateTimer);
						if (brushAnimationKeyFrame != null)
						{
							brushAnimationKeyFrame2 = ((brushAnimationKeyFrame != frameAt) ? collection.GetFrameAt(brushAnimationKeyFrame.Index - 1) : collection.GetFrameAt(collection.Count - 1));
						}
						else
						{
							brushAnimationKeyFrame = frameAt;
							brushAnimationKeyFrame2 = collection.GetFrameAt(collection.Count - 1);
						}
					}
				}
				else
				{
					brushAnimationKeyFrame = collection.GetFrameAfter(brushStateTimer);
					brushAnimationKeyFrame2 = ((brushAnimationKeyFrame == null) ? collection.GetFrameAt(collection.Count - 1) : collection.GetFrameAt(brushAnimationKeyFrame.Index - 1));
				}
				float num = 0f;
				BrushAnimationKeyFrame brushAnimationKeyFrame3 = null;
				BrushLayerState brushLayerState = default(BrushLayerState);
				IBrushLayerData brushLayerData = null;
				BrushAnimationKeyFrame brushAnimationKeyFrame4 = null;
				if (brushAnimationKeyFrame != null)
				{
					if (brushAnimationKeyFrame2 != null)
					{
						float num2;
						float num3;
						if (animation.Loop)
						{
							if (brushAnimationKeyFrame.Index == 0)
							{
								num2 = brushAnimationKeyFrame.Time + (animation.Duration - brushAnimationKeyFrame2.Time);
								num3 = ((!(brushStateTimer >= brushAnimationKeyFrame2.Time)) ? (animation.Duration - brushAnimationKeyFrame2.Time + brushStateTimer) : (brushStateTimer - brushAnimationKeyFrame2.Time));
							}
							else
							{
								num2 = brushAnimationKeyFrame.Time - brushAnimationKeyFrame2.Time;
								num3 = brushStateTimer - brushAnimationKeyFrame2.Time;
							}
						}
						else
						{
							num2 = brushAnimationKeyFrame.Time - brushAnimationKeyFrame2.Time;
							num3 = brushStateTimer - brushAnimationKeyFrame2.Time;
						}
						num = num3 * (1f / num2);
						brushAnimationKeyFrame3 = brushAnimationKeyFrame2;
						brushAnimationKeyFrame4 = brushAnimationKeyFrame;
					}
					else
					{
						num = brushStateTimer * (1f / brushAnimationKeyFrame.Time);
						brushLayerState = startState;
						brushAnimationKeyFrame4 = brushAnimationKeyFrame;
					}
				}
				else
				{
					num = (brushStateTimer - brushAnimationKeyFrame2.Time) * (1f / (animation.Duration - brushAnimationKeyFrame2.Time));
					brushAnimationKeyFrame3 = brushAnimationKeyFrame2;
					brushLayerData = source;
				}
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
				case BrushAnimationProperty.BrushAnimationPropertyType.OverridenWidth:
				case BrushAnimationProperty.BrushAnimationPropertyType.OverridenHeight:
				case BrushAnimationProperty.BrushAnimationPropertyType.ExtendLeft:
				case BrushAnimationProperty.BrushAnimationPropertyType.ExtendRight:
				case BrushAnimationProperty.BrushAnimationPropertyType.ExtendTop:
				case BrushAnimationProperty.BrushAnimationPropertyType.ExtendBottom:
				{
					float valueFrom = brushAnimationKeyFrame3?.GetValueAsFloat() ?? brushLayerState.GetValueAsFloat(propertyType);
					float valueTo = brushLayerData?.GetValueAsFloat(propertyType) ?? brushAnimationKeyFrame4.GetValueAsFloat();
					result.SetValueAsFloat(propertyType, TaleWorlds.Library.MathF.Lerp(valueFrom, valueTo, num));
					break;
				}
				case BrushAnimationProperty.BrushAnimationPropertyType.Color:
				case BrushAnimationProperty.BrushAnimationPropertyType.FontColor:
				case BrushAnimationProperty.BrushAnimationPropertyType.TextGlowColor:
				case BrushAnimationProperty.BrushAnimationPropertyType.TextOutlineColor:
				{
					Color start = brushAnimationKeyFrame3?.GetValueAsColor() ?? brushLayerState.GetValueAsColor(propertyType);
					Color end = brushLayerData?.GetValueAsColor(propertyType) ?? brushAnimationKeyFrame4.GetValueAsColor();
					Color value = Color.Lerp(start, end, num);
					result.SetValueAsColor(propertyType, in value);
					break;
				}
				case BrushAnimationProperty.BrushAnimationPropertyType.Sprite:
				case BrushAnimationProperty.BrushAnimationPropertyType.OverlaySprite:
				{
					Sprite sprite = brushAnimationKeyFrame3?.GetValueAsSprite() ?? brushLayerState.GetValueAsSprite(propertyType);
					Sprite sprite2 = brushLayerData?.GetValueAsSprite(propertyType) ?? brushAnimationKeyFrame4.GetValueAsSprite();
					result.SetValueAsSprite(propertyType, ((double)num <= 0.9) ? sprite : sprite2);
					break;
				}
				}
			}
		}
		return result;
	}

	public bool IsUpdateNeeded()
	{
		if (_brushRendererAnimationState == BrushRendererAnimationState.PlayingBasicTranisition || _brushRendererAnimationState == BrushRendererAnimationState.PlayingAnimation)
		{
			return true;
		}
		if (_styleOfCurrentState != null)
		{
			return _styleOfCurrentState.Version != _latestStyleVersion;
		}
		return false;
	}

	private BrushState AnimateBrushState(BrushAnimation animation, BrushLayerAnimation layerAnimation, float brushStateTimer, bool isFirstCycle, BrushState startState, Style source)
	{
		BrushState result = default(BrushState);
		result.FillFrom(source);
		if (layerAnimation != null)
		{
			foreach (BrushAnimationProperty collection in layerAnimation.Collections)
			{
				BrushAnimationProperty.BrushAnimationPropertyType propertyType = collection.PropertyType;
				BrushAnimationKeyFrame brushAnimationKeyFrame = null;
				BrushAnimationKeyFrame brushAnimationKeyFrame2;
				if (animation.Loop)
				{
					BrushAnimationKeyFrame frameAt = collection.GetFrameAt(0);
					if (isFirstCycle && _brushTimer < frameAt.Time)
					{
						brushAnimationKeyFrame2 = frameAt;
					}
					else
					{
						brushAnimationKeyFrame2 = collection.GetFrameAfter(brushStateTimer);
						if (brushAnimationKeyFrame2 != null)
						{
							brushAnimationKeyFrame = ((brushAnimationKeyFrame2 != frameAt) ? collection.GetFrameAt(brushAnimationKeyFrame2.Index - 1) : collection.GetFrameAt(collection.Count - 1));
						}
						else
						{
							brushAnimationKeyFrame2 = frameAt;
							brushAnimationKeyFrame = collection.GetFrameAt(collection.Count - 1);
						}
					}
				}
				else
				{
					brushAnimationKeyFrame2 = collection.GetFrameAfter(brushStateTimer);
					brushAnimationKeyFrame = ((brushAnimationKeyFrame2 != null) ? collection.GetFrameAt(brushAnimationKeyFrame2.Index - 1) : collection.GetFrameAt(collection.Count - 1));
				}
				BrushAnimationKeyFrame brushAnimationKeyFrame3 = null;
				BrushState brushState = default(BrushState);
				Style style = null;
				BrushAnimationKeyFrame brushAnimationKeyFrame4 = null;
				float value;
				if (brushAnimationKeyFrame2 != null)
				{
					if (brushAnimationKeyFrame != null)
					{
						float num;
						float num2;
						if (animation.Loop)
						{
							if (brushAnimationKeyFrame2.Index == 0)
							{
								num = brushAnimationKeyFrame2.Time + (animation.Duration - brushAnimationKeyFrame.Time);
								num2 = ((!(brushStateTimer >= brushAnimationKeyFrame.Time)) ? (animation.Duration - brushAnimationKeyFrame.Time + brushStateTimer) : (brushStateTimer - brushAnimationKeyFrame.Time));
							}
							else
							{
								num = brushAnimationKeyFrame2.Time - brushAnimationKeyFrame.Time;
								num2 = brushStateTimer - brushAnimationKeyFrame.Time;
							}
						}
						else
						{
							num = brushAnimationKeyFrame2.Time - brushAnimationKeyFrame.Time;
							num2 = brushStateTimer - brushAnimationKeyFrame.Time;
						}
						value = num2 * (1f / num);
						brushAnimationKeyFrame3 = brushAnimationKeyFrame;
						brushAnimationKeyFrame4 = brushAnimationKeyFrame2;
					}
					else
					{
						value = brushStateTimer * (1f / brushAnimationKeyFrame2.Time);
						brushState = startState;
						brushAnimationKeyFrame4 = brushAnimationKeyFrame2;
					}
				}
				else
				{
					value = (brushStateTimer - brushAnimationKeyFrame.Time) * (1f / (animation.Duration - brushAnimationKeyFrame.Time));
					brushAnimationKeyFrame3 = brushAnimationKeyFrame;
					style = source;
				}
				value = TaleWorlds.Library.MathF.Clamp(value, 0f, 1f);
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
				case BrushAnimationProperty.BrushAnimationPropertyType.OverridenWidth:
				case BrushAnimationProperty.BrushAnimationPropertyType.OverridenHeight:
				case BrushAnimationProperty.BrushAnimationPropertyType.ExtendLeft:
				case BrushAnimationProperty.BrushAnimationPropertyType.ExtendRight:
				case BrushAnimationProperty.BrushAnimationPropertyType.ExtendTop:
				case BrushAnimationProperty.BrushAnimationPropertyType.ExtendBottom:
				{
					float valueFrom = brushAnimationKeyFrame3?.GetValueAsFloat() ?? brushState.GetValueAsFloat(propertyType);
					float valueTo = style?.GetValueAsFloat(propertyType) ?? brushAnimationKeyFrame4.GetValueAsFloat();
					result.SetValueAsFloat(propertyType, TaleWorlds.Library.MathF.Lerp(valueFrom, valueTo, value));
					break;
				}
				case BrushAnimationProperty.BrushAnimationPropertyType.Color:
				case BrushAnimationProperty.BrushAnimationPropertyType.FontColor:
				case BrushAnimationProperty.BrushAnimationPropertyType.TextGlowColor:
				case BrushAnimationProperty.BrushAnimationPropertyType.TextOutlineColor:
				{
					Color start = brushAnimationKeyFrame3?.GetValueAsColor() ?? brushState.GetValueAsColor(propertyType);
					Color end = style?.GetValueAsColor(propertyType) ?? brushAnimationKeyFrame4.GetValueAsColor();
					Color value2 = Color.Lerp(start, end, value);
					result.SetValueAsColor(propertyType, in value2);
					break;
				}
				case BrushAnimationProperty.BrushAnimationPropertyType.Sprite:
				case BrushAnimationProperty.BrushAnimationPropertyType.OverlaySprite:
				{
					Sprite sprite = brushAnimationKeyFrame3?.GetValueAsSprite() ?? brushState.GetValueAsSprite(propertyType);
					Sprite sprite2 = style?.GetValueAsSprite(propertyType) ?? brushAnimationKeyFrame4.GetValueAsSprite();
					result.SetValueAsSprite(propertyType, ((double)value <= 0.9) ? sprite : sprite2);
					break;
				}
				}
			}
		}
		return result;
	}

	private void EndAnimation()
	{
		if (Brush != null)
		{
			Style styleOfCurrentState = _styleOfCurrentState;
			BrushState brushState = default(BrushState);
			brushState.FillFrom(styleOfCurrentState);
			_startBrushState = brushState;
			_currentBrushState = brushState;
			if (Brush.TransitionDuration == 0f)
			{
				_brushRendererAnimationState = BrushRendererAnimationState.None;
			}
			StyleLayer[] layers = styleOfCurrentState.GetLayers();
			foreach (StyleLayer styleLayer in layers)
			{
				BrushLayerState value = default(BrushLayerState);
				value.FillFrom(styleLayer);
				_startBrushLayerState[styleLayer.Name] = value;
				_currentBrushLayerState[styleLayer.Name] = value;
			}
			_brushRendererAnimationState = BrushRendererAnimationState.Ended;
		}
	}

	public void Render(TwoDimensionDrawContext drawContext, Vector2 targetPosition, Vector2 targetSize, float scale, float contextAlpha, Vector2 overlayOffset = default(Vector2))
	{
		if (Brush == null)
		{
			return;
		}
		if (ForcePixelPerfectPlacement)
		{
			targetPosition.X = TaleWorlds.Library.MathF.Round(targetPosition.X);
			targetPosition.Y = TaleWorlds.Library.MathF.Round(targetPosition.Y);
		}
		Style styleOfCurrentState = _styleOfCurrentState;
		for (int i = 0; i < styleOfCurrentState.LayerCount; i++)
		{
			StyleLayer layer = styleOfCurrentState.GetLayer(i);
			if (layer.IsHidden)
			{
				continue;
			}
			BrushLayerState brushLayerState;
			if (_currentBrushLayerState.Count == 1)
			{
				Dictionary<string, BrushLayerState>.ValueCollection.Enumerator enumerator = _currentBrushLayerState.Values.GetEnumerator();
				enumerator.MoveNext();
				brushLayerState = enumerator.Current;
			}
			else
			{
				brushLayerState = _currentBrushLayerState[layer.Name];
			}
			Sprite sprite = brushLayerState.Sprite;
			if (sprite == null)
			{
				continue;
			}
			Texture texture = sprite.Texture;
			if (texture == null)
			{
				continue;
			}
			float num = targetPosition.X + brushLayerState.XOffset * scale;
			float num2 = targetPosition.Y + brushLayerState.YOffset * scale;
			SimpleMaterial simpleMaterial = drawContext.CreateSimpleMaterial();
			simpleMaterial.OverlayEnabled = false;
			simpleMaterial.CircularMaskingEnabled = false;
			if (layer.OverlayMethod == BrushOverlayMethod.CoverWithTexture && layer.OverlaySprite != null)
			{
				Sprite overlaySprite = layer.OverlaySprite;
				Texture texture2 = overlaySprite.Texture;
				if (texture2 != null)
				{
					simpleMaterial.OverlayEnabled = true;
					simpleMaterial.StartCoordinate = new Vector2(num, num2);
					simpleMaterial.Size = targetSize;
					simpleMaterial.OverlayTexture = texture2;
					simpleMaterial.UseOverlayAlphaAsMask = layer.UseOverlayAlphaAsMask;
					float num3;
					float num4;
					if (layer.UseOverlayAlphaAsMask)
					{
						num3 = brushLayerState.XOffset;
						num4 = brushLayerState.YOffset;
					}
					else if (overlayOffset == default(Vector2))
					{
						num3 = brushLayerState.OverlayXOffset;
						num4 = brushLayerState.OverlayYOffset;
					}
					else
					{
						num3 = overlayOffset.X;
						num4 = overlayOffset.Y;
					}
					if (layer.UseRandomBaseOverlayXOffset)
					{
						num3 += GetRandomXOffset();
					}
					if (layer.UseRandomBaseOverlayYOffset)
					{
						num4 += GetRandomYOffset();
					}
					simpleMaterial.OverlayXOffset = num3 * scale;
					simpleMaterial.OverlayYOffset = num4 * scale;
					simpleMaterial.Scale = scale;
					simpleMaterial.OverlayTextureWidth = (layer.UseOverlayAlphaAsMask ? targetSize.X : ((float)overlaySprite.Width));
					simpleMaterial.OverlayTextureHeight = (layer.UseOverlayAlphaAsMask ? targetSize.Y : ((float)overlaySprite.Height));
				}
			}
			simpleMaterial.Texture = texture;
			simpleMaterial.Color = brushLayerState.Color * Brush.GlobalColor;
			simpleMaterial.ColorFactor = brushLayerState.ColorFactor * Brush.GlobalColorFactor;
			simpleMaterial.AlphaFactor = brushLayerState.AlphaFactor * Brush.GlobalAlphaFactor * contextAlpha;
			simpleMaterial.HueFactor = brushLayerState.HueFactor;
			simpleMaterial.SaturationFactor = brushLayerState.SaturationFactor;
			simpleMaterial.ValueFactor = brushLayerState.ValueFactor;
			float width = 0f;
			float height = 0f;
			if (layer.WidthPolicy == BrushLayerSizePolicy.StretchToTarget)
			{
				float num5 = layer.ExtendLeft;
				if (layer.HorizontalFlip)
				{
					num5 = layer.ExtendRight;
				}
				width = targetSize.X;
				width += (layer.ExtendRight + layer.ExtendLeft) * scale;
				num -= num5 * scale;
			}
			else if (layer.WidthPolicy == BrushLayerSizePolicy.Original)
			{
				width = (float)sprite.Width * scale;
			}
			else if (layer.WidthPolicy == BrushLayerSizePolicy.Overriden)
			{
				width = layer.OverridenWidth * scale;
			}
			if (layer.HeightPolicy == BrushLayerSizePolicy.StretchToTarget)
			{
				float num6 = layer.ExtendTop;
				if (layer.HorizontalFlip)
				{
					num6 = layer.ExtendBottom;
				}
				height = targetSize.Y;
				height += (layer.ExtendTop + layer.ExtendBottom) * scale;
				num2 -= num6 * scale;
			}
			else if (layer.HeightPolicy == BrushLayerSizePolicy.Original)
			{
				height = (float)sprite.Height * scale;
			}
			else if (layer.HeightPolicy == BrushLayerSizePolicy.Overriden)
			{
				height = layer.OverridenHeight * scale;
			}
			bool horizontalFlip = layer.HorizontalFlip;
			bool verticalFlip = layer.VerticalFlip;
			drawContext.DrawSprite(sprite, simpleMaterial, num, num2, scale, width, height, horizontalFlip, verticalFlip);
		}
	}

	public TextMaterial CreateTextMaterial(TwoDimensionDrawContext drawContext)
	{
		TextMaterial textMaterial = _currentBrushState.CreateTextMaterial(drawContext);
		if (Brush != null)
		{
			textMaterial.ColorFactor *= Brush.GlobalColorFactor;
			textMaterial.AlphaFactor *= Brush.GlobalAlphaFactor;
			textMaterial.Color *= Brush.GlobalColor;
		}
		return textMaterial;
	}

	public void RestartAnimation()
	{
		if (Brush == null)
		{
			return;
		}
		_brushLocalTimer = 0f;
		Style styleOfCurrentState = _styleOfCurrentState;
		_brushRendererAnimationState = BrushRendererAnimationState.None;
		if (styleOfCurrentState != null)
		{
			if (styleOfCurrentState.AnimationMode == StyleAnimationMode.BasicTransition)
			{
				_brushRendererAnimationState = BrushRendererAnimationState.PlayingBasicTranisition;
			}
			else if (styleOfCurrentState.AnimationMode == StyleAnimationMode.Animation)
			{
				_brushRendererAnimationState = BrushRendererAnimationState.PlayingAnimation;
			}
		}
	}

	public void SetSeed(int seed)
	{
		_offsetSeed = seed;
	}
}
