using System.Numerics;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.BaseTypes;

public class TextWidget : ImageWidget
{
	protected readonly Text _text;

	protected bool IsTextValueDirty = true;

	public bool AutoHideIfEmpty { get; set; }

	[Editor(false)]
	public string Text
	{
		get
		{
			return _text.Value;
		}
		set
		{
			if (_text.Value != value)
			{
				SetText(value);
			}
		}
	}

	[Editor(false)]
	public int IntText
	{
		get
		{
			if (int.TryParse(_text.Value, out var result))
			{
				return result;
			}
			return -1;
		}
		set
		{
			if (_text.Value != value.ToString())
			{
				SetText(value.ToString());
			}
		}
	}

	[Editor(false)]
	public float FloatText
	{
		get
		{
			if (float.TryParse(_text.Value, out var result))
			{
				return result;
			}
			return -1f;
		}
		set
		{
			if (_text.Value != value.ToString())
			{
				SetText(value.ToString());
			}
		}
	}

	public TextWidget(UIContext context)
		: base(context)
	{
		FontFactory fontFactory = context.FontFactory;
		_text = new Text((int)base.Size.X, (int)base.Size.Y, fontFactory.DefaultFont, fontFactory.GetUsableFontForCharacter);
		base.LayoutImp = new TextLayout(_text);
	}

	protected virtual void SetText(string value)
	{
		SetMeasureAndLayoutDirty();
		_text.CurrentLanguage = base.Context.FontFactory.GetCurrentLanguage();
		_text.Value = value;
		OnPropertyChanged(FloatText, "FloatText");
		OnPropertyChanged(IntText, "IntText");
		OnPropertyChanged(Text, "Text");
		RefreshTextParameters();
		if (AutoHideIfEmpty)
		{
			base.IsVisible = !string.IsNullOrEmpty(Text);
		}
		IsTextValueDirty = true;
	}

	protected void RefreshTextParameters()
	{
		float fontSize = (float)base.ReadOnlyBrush.FontSize * base._scaleToUse;
		_text.HorizontalAlignment = base.ReadOnlyBrush.TextHorizontalAlignment;
		_text.VerticalAlignment = base.ReadOnlyBrush.TextVerticalAlignment;
		_text.FontSize = fontSize;
		_text.CurrentLanguage = base.Context.FontFactory.GetCurrentLanguage();
		if (base.ReadOnlyBrush.Font != null)
		{
			_text.Font = base.Context.FontFactory.GetMappedFontForLocalization(base.ReadOnlyBrush.Font.Name);
		}
		else
		{
			_text.Font = base.Context.FontFactory.DefaultFont;
		}
		if (!IsTextValueDirty)
		{
			return;
		}
		for (int i = 0; i < _text.Value.Length; i++)
		{
			if (char.IsLetter(_text.Value[i]) && !_text.Font.Characters.ContainsKey(_text.Value[i]))
			{
				Font usableFontForCharacter = base.Context.FontFactory.GetUsableFontForCharacter(_text.Value[i]);
				if (usableFontForCharacter != null)
				{
					_text.Font = usableFontForCharacter;
				}
				break;
			}
		}
		IsTextValueDirty = false;
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		base.OnRender(twoDimensionContext, drawContext);
		RefreshTextParameters();
		TextMaterial textMaterial = base.BrushRenderer.CreateTextMaterial(drawContext);
		textMaterial.AlphaFactor *= base.Context.ContextAlpha;
		Vector2 cachedGlobalPosition = _cachedGlobalPosition;
		drawContext.Draw(_text, textMaterial, cachedGlobalPosition.X, cachedGlobalPosition.Y, base.Size.X, base.Size.Y);
	}
}
