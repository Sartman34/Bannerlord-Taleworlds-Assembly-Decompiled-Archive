using System.Linq;
using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class CharacterTableauWidget : TextureWidget
{
	private ButtonWidget _swapPlacesButtonWidget;

	private string _bannerCode;

	private string _bodyProperties;

	private string _charStringId;

	private string _equipmentCode;

	private string _mountCreationKey;

	private string _idleAction;

	private string _idleFaceAnim;

	private string _customAnimation;

	private int _leftHandWieldedEquipmentIndex;

	private int _rightHandWieldedEquipmentIndex;

	private uint _armorColor1;

	private uint _armorColor2;

	private int _stanceIndex;

	private int _race;

	private bool _isEquipmentAnimActive;

	private bool _isFemale;

	private bool _isCharacterMountSwapped;

	private bool _isBannerShownInBackground;

	private bool _isPlayingCustomAnimations;

	private bool _shouldLoopCustomAnimation;

	private float _customAnimationProgressRatio;

	private float _customRenderScale;

	private float _customAnimationWaitDuration;

	[Editor(false)]
	public string BannerCodeText
	{
		get
		{
			return _bannerCode;
		}
		set
		{
			if (value != _bannerCode)
			{
				_bannerCode = value;
				OnPropertyChanged(value, "BannerCodeText");
				SetTextureProviderProperty("BannerCodeText", value);
			}
		}
	}

	[Editor(false)]
	public ButtonWidget SwapPlacesButtonWidget
	{
		get
		{
			return _swapPlacesButtonWidget;
		}
		set
		{
			if (value != _swapPlacesButtonWidget)
			{
				_swapPlacesButtonWidget = value;
				OnPropertyChanged(value, "SwapPlacesButtonWidget");
				if (value != null)
				{
					_swapPlacesButtonWidget.ClickEventHandlers.Add(OnSwapClick);
				}
			}
		}
	}

	[Editor(false)]
	public string BodyProperties
	{
		get
		{
			return _bodyProperties;
		}
		set
		{
			if (value != _bodyProperties)
			{
				_bodyProperties = value;
				OnPropertyChanged(value, "BodyProperties");
				SetTextureProviderProperty("BodyProperties", value);
			}
		}
	}

	[Editor(false)]
	public float CustomAnimationProgressRatio
	{
		get
		{
			return _customAnimationProgressRatio;
		}
		set
		{
			if (value != _customAnimationProgressRatio)
			{
				_customAnimationProgressRatio = value;
				OnPropertyChanged(value, "CustomAnimationProgressRatio");
			}
		}
	}

	[Editor(false)]
	public float CustomRenderScale
	{
		get
		{
			return _customRenderScale;
		}
		set
		{
			if (value != _customRenderScale)
			{
				_customRenderScale = value;
				OnPropertyChanged(value, "CustomRenderScale");
				SetTextureProviderProperty("CustomRenderScale", value);
			}
		}
	}

	[Editor(false)]
	public float CustomAnimationWaitDuration
	{
		get
		{
			return _customAnimationWaitDuration;
		}
		set
		{
			if (value != _customAnimationWaitDuration)
			{
				_customAnimationWaitDuration = value;
				OnPropertyChanged(value, "CustomAnimationWaitDuration");
				SetTextureProviderProperty("CustomAnimationWaitDuration", value);
			}
		}
	}

	[Editor(false)]
	public string CharStringId
	{
		get
		{
			return _charStringId;
		}
		set
		{
			if (value != _charStringId)
			{
				_charStringId = value;
				OnPropertyChanged(value, "CharStringId");
				SetTextureProviderProperty("CharStringId", value);
			}
		}
	}

	[Editor(false)]
	public int StanceIndex
	{
		get
		{
			return _stanceIndex;
		}
		set
		{
			if (value != _stanceIndex)
			{
				_stanceIndex = value;
				OnPropertyChanged(value, "StanceIndex");
				SetTextureProviderProperty("StanceIndex", value);
			}
		}
	}

	[Editor(false)]
	public bool IsEquipmentAnimActive
	{
		get
		{
			return _isEquipmentAnimActive;
		}
		set
		{
			if (value != _isEquipmentAnimActive)
			{
				_isEquipmentAnimActive = value;
				OnPropertyChanged(value, "IsEquipmentAnimActive");
				SetTextureProviderProperty("IsEquipmentAnimActive", value);
			}
		}
	}

	[Editor(false)]
	public bool IsFemale
	{
		get
		{
			return _isFemale;
		}
		set
		{
			if (value != _isFemale)
			{
				_isFemale = value;
				OnPropertyChanged(value, "IsFemale");
				SetTextureProviderProperty("IsFemale", value);
			}
		}
	}

	[Editor(false)]
	public int Race
	{
		get
		{
			return _race;
		}
		set
		{
			if (value != _race)
			{
				_race = value;
				OnPropertyChanged(value, "Race");
				SetTextureProviderProperty("Race", value);
			}
		}
	}

	[Editor(false)]
	public string EquipmentCode
	{
		get
		{
			return _equipmentCode;
		}
		set
		{
			if (value != _equipmentCode)
			{
				_equipmentCode = value;
				OnPropertyChanged(value, "EquipmentCode");
				SetTextureProviderProperty("EquipmentCode", value);
			}
		}
	}

	[Editor(false)]
	public string MountCreationKey
	{
		get
		{
			return _mountCreationKey;
		}
		set
		{
			if (value != _mountCreationKey)
			{
				_mountCreationKey = value;
				OnPropertyChanged(value, "MountCreationKey");
				SetTextureProviderProperty("MountCreationKey", value);
			}
		}
	}

	[Editor(false)]
	public string IdleAction
	{
		get
		{
			return _idleAction;
		}
		set
		{
			if (value != _idleAction)
			{
				_idleAction = value;
				OnPropertyChanged(value, "IdleAction");
				SetTextureProviderProperty("IdleAction", value);
			}
		}
	}

	[Editor(false)]
	public string IdleFaceAnim
	{
		get
		{
			return _idleFaceAnim;
		}
		set
		{
			if (value != _idleFaceAnim)
			{
				_idleFaceAnim = value;
				OnPropertyChanged(value, "IdleFaceAnim");
				SetTextureProviderProperty("IdleFaceAnim", value);
			}
		}
	}

	[Editor(false)]
	public string CustomAnimation
	{
		get
		{
			return _customAnimation;
		}
		set
		{
			if (value != _customAnimation)
			{
				_customAnimation = value;
				OnPropertyChanged(value, "CustomAnimation");
				SetTextureProviderProperty("CustomAnimation", value);
			}
		}
	}

	[Editor(false)]
	public int LeftHandWieldedEquipmentIndex
	{
		get
		{
			return _leftHandWieldedEquipmentIndex;
		}
		set
		{
			if (value != _leftHandWieldedEquipmentIndex)
			{
				_leftHandWieldedEquipmentIndex = value;
				OnPropertyChanged(value, "LeftHandWieldedEquipmentIndex");
				SetTextureProviderProperty("LeftHandWieldedEquipmentIndex", value);
			}
		}
	}

	[Editor(false)]
	public int RightHandWieldedEquipmentIndex
	{
		get
		{
			return _rightHandWieldedEquipmentIndex;
		}
		set
		{
			if (value != _rightHandWieldedEquipmentIndex)
			{
				_rightHandWieldedEquipmentIndex = value;
				OnPropertyChanged(value, "RightHandWieldedEquipmentIndex");
				SetTextureProviderProperty("RightHandWieldedEquipmentIndex", value);
			}
		}
	}

	[Editor(false)]
	public uint ArmorColor1
	{
		get
		{
			return _armorColor1;
		}
		set
		{
			if (value != _armorColor1)
			{
				_armorColor1 = value;
				OnPropertyChanged(value, "ArmorColor1");
				SetTextureProviderProperty("ArmorColor1", value);
			}
		}
	}

	[Editor(false)]
	public uint ArmorColor2
	{
		get
		{
			return _armorColor2;
		}
		set
		{
			if (value != _armorColor2)
			{
				_armorColor2 = value;
				OnPropertyChanged(value, "ArmorColor2");
				SetTextureProviderProperty("ArmorColor2", value);
			}
		}
	}

	[Editor(false)]
	public bool IsBannerShownInBackground
	{
		get
		{
			return _isBannerShownInBackground;
		}
		set
		{
			if (value != _isBannerShownInBackground)
			{
				_isBannerShownInBackground = value;
				OnPropertyChanged(value, "IsBannerShownInBackground");
				SetTextureProviderProperty("IsBannerShownInBackground", value);
			}
		}
	}

	[Editor(false)]
	public bool IsPlayingCustomAnimations
	{
		get
		{
			return _isPlayingCustomAnimations;
		}
		set
		{
			if (value != _isPlayingCustomAnimations)
			{
				_isPlayingCustomAnimations = value;
				OnPropertyChanged(value, "IsPlayingCustomAnimations");
				SetTextureProviderProperty("IsPlayingCustomAnimations", value);
			}
		}
	}

	[Editor(false)]
	public bool ShouldLoopCustomAnimation
	{
		get
		{
			return _shouldLoopCustomAnimation;
		}
		set
		{
			if (value != _shouldLoopCustomAnimation)
			{
				_shouldLoopCustomAnimation = value;
				OnPropertyChanged(value, "ShouldLoopCustomAnimation");
				SetTextureProviderProperty("ShouldLoopCustomAnimation", value);
			}
		}
	}

	public CharacterTableauWidget(UIContext context)
		: base(context)
	{
		base.TextureProviderName = "CharacterTableauTextureProvider";
	}

	protected override void OnMousePressed()
	{
		SetTextureProviderProperty("CurrentlyRotating", true);
	}

	protected override void OnMouseReleased()
	{
		SetTextureProviderProperty("CurrentlyRotating", false);
	}

	private void OnSwapClick(Widget obj)
	{
		_isCharacterMountSwapped = !_isCharacterMountSwapped;
		SetTextureProviderProperty("TriggerCharacterMountPlacesSwap", _isCharacterMountSwapped);
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		if ((LeftHandWieldedEquipmentIndex != -1 || RightHandWieldedEquipmentIndex != -1) && !IsRecursivelyVisible())
		{
			LeftHandWieldedEquipmentIndex = -1;
			RightHandWieldedEquipmentIndex = -1;
		}
		if (IsPlayingCustomAnimations && base.TextureProvider != null && !(bool)GetTextureProviderProperty("IsPlayingCustomAnimations"))
		{
			IsPlayingCustomAnimations = false;
		}
		if (base.TextureProvider != null)
		{
			CustomAnimationProgressRatio = (float)GetTextureProviderProperty("CustomAnimationProgressRatio");
		}
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		_isRenderRequestedPreviousFrame = true;
		if (base.TextureProvider != null)
		{
			base.Texture = base.TextureProvider.GetTexture(twoDimensionContext, string.Empty);
			SimpleMaterial simpleMaterial = drawContext.CreateSimpleMaterial();
			StyleLayer styleLayer = base.ReadOnlyBrush?.GetStyleOrDefault(base.CurrentState).GetLayers()?.FirstOrDefault() ?? null;
			simpleMaterial.OverlayEnabled = false;
			simpleMaterial.CircularMaskingEnabled = false;
			simpleMaterial.Texture = base.Texture;
			simpleMaterial.AlphaFactor = (styleLayer?.AlphaFactor ?? 1f) * base.ReadOnlyBrush.GlobalAlphaFactor * base.Context.ContextAlpha;
			simpleMaterial.ColorFactor = (styleLayer?.ColorFactor ?? 1f) * base.ReadOnlyBrush.GlobalColorFactor;
			simpleMaterial.HueFactor = styleLayer?.HueFactor ?? 0f;
			simpleMaterial.SaturationFactor = styleLayer?.SaturationFactor ?? 0f;
			simpleMaterial.ValueFactor = styleLayer?.ValueFactor ?? 0f;
			simpleMaterial.Color = (styleLayer?.Color ?? Color.White) * base.ReadOnlyBrush.GlobalColor;
			Vector2 globalPosition = base.GlobalPosition;
			float x = globalPosition.X;
			float y = globalPosition.Y;
			_ = base.Size;
			_ = base.Size;
			DrawObject2D drawObject2D = null;
			if (_cachedQuad != null && _cachedQuadSize == base.Size)
			{
				drawObject2D = _cachedQuad;
			}
			if (drawObject2D == null)
			{
				drawObject2D = (_cachedQuad = DrawObject2D.CreateQuad(base.Size));
				_cachedQuadSize = base.Size;
			}
			if (drawContext.CircularMaskEnabled)
			{
				simpleMaterial.CircularMaskingEnabled = true;
				simpleMaterial.CircularMaskingCenter = drawContext.CircularMaskCenter;
				simpleMaterial.CircularMaskingRadius = drawContext.CircularMaskRadius;
				simpleMaterial.CircularMaskingSmoothingRadius = drawContext.CircularMaskSmoothingRadius;
			}
			drawContext.Draw(x, y, simpleMaterial, drawObject2D, base.Size.X, base.Size.Y);
		}
	}
}
