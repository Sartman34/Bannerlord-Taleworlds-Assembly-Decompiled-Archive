using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.InputSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.BaseTypes;

public class EditableTextWidget : BrushWidget
{
	protected enum MouseState
	{
		None,
		Down,
		Up
	}

	protected enum CursorMovementDirection
	{
		None = 0,
		Left = -1,
		Right = 1
	}

	protected enum KeyboardAction
	{
		None,
		BackSpace,
		Delete
	}

	protected EditableText _editableText;

	protected readonly char _obfuscationChar = '*';

	protected float _lastScale = -1f;

	protected bool _isObfuscationEnabled;

	protected string _lastLanguageCode;

	protected Brush _lastFontBrush;

	protected MouseState _mouseState;

	protected Vector2 _mouseDownPosition;

	protected bool _cursorVisible;

	protected int _textHeight;

	protected CursorMovementDirection _cursorDirection;

	protected KeyboardAction _keyboardAction;

	protected int _nextRepeatTime;

	protected bool _isSelection;

	private string _realText = "";

	private string _keyboardInfoText = "";

	public int MaxLength { get; set; } = -1;


	public bool IsObfuscationEnabled
	{
		get
		{
			return _isObfuscationEnabled;
		}
		set
		{
			if (value != _isObfuscationEnabled)
			{
				_isObfuscationEnabled = value;
				OnObfuscationToggled(value);
			}
		}
	}

	private Vector2 LocalMousePosition
	{
		get
		{
			Vector2 mousePosition = base.EventManager.MousePosition;
			Vector2 globalPosition = base.GlobalPosition;
			float x = mousePosition.X - globalPosition.X;
			float y = mousePosition.Y - globalPosition.Y;
			return new Vector2(x, y);
		}
	}

	public string DefaultSearchText { get; set; }

	[Editor(false)]
	public string RealText
	{
		get
		{
			return _realText;
		}
		set
		{
			if (_realText != value)
			{
				_editableText.CurrentLanguage = base.Context.FontFactory.GetCurrentLanguage();
				if (string.IsNullOrEmpty(value))
				{
					value = "";
				}
				Text = (IsObfuscationEnabled ? ObfuscateText(value) : value);
				_realText = value;
				OnPropertyChanged(value, "RealText");
			}
		}
	}

	[Editor(false)]
	public string KeyboardInfoText
	{
		get
		{
			return _keyboardInfoText;
		}
		set
		{
			if (_keyboardInfoText != value)
			{
				_keyboardInfoText = value;
				OnPropertyChanged(value, "KeyboardInfoText");
			}
		}
	}

	[Editor(false)]
	public string Text
	{
		get
		{
			return _editableText.VisibleText;
		}
		set
		{
			if (_editableText.VisibleText != value)
			{
				_editableText.CurrentLanguage = base.Context.FontFactory.GetCurrentLanguage();
				_editableText.VisibleText = value;
				if (!string.IsNullOrEmpty(_editableText.VisibleText))
				{
					_editableText.SetCursor(_editableText.VisibleText.Length);
				}
				if (string.IsNullOrEmpty(value))
				{
					_editableText.VisibleText = "";
					_editableText.SetCursor(0, base.IsFocused);
				}
				RealText = value;
				OnPropertyChanged(value, "Text");
				SetMeasureAndLayoutDirty();
			}
		}
	}

	public EditableTextWidget(UIContext context)
		: base(context)
	{
		FontFactory fontFactory = context.FontFactory;
		_editableText = new EditableText((int)base.Size.X, (int)base.Size.Y, fontFactory.DefaultFont, fontFactory.GetUsableFontForCharacter);
		base.LayoutImp = new TextLayout(_editableText);
		_realText = "";
		_textHeight = -1;
		_cursorVisible = false;
		_lastFontBrush = null;
		_cursorDirection = CursorMovementDirection.None;
		_keyboardAction = KeyboardAction.None;
		_nextRepeatTime = int.MinValue;
		_isSelection = false;
		base.IsFocusable = true;
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		UpdateText();
		if (base.IsFocused && base.IsEnabled)
		{
			_editableText.BlinkTimer += dt;
			if (_editableText.BlinkTimer > 0.5f)
			{
				_editableText.BlinkCursor();
				_editableText.BlinkTimer = 0f;
			}
			if (ContainsState("Selected"))
			{
				SetState("Selected");
			}
		}
		else if (_editableText.IsCursorVisible())
		{
			_editableText.BlinkCursor();
		}
		SetEditTextParameters();
	}

	private void SetEditTextParameters()
	{
		bool flag = false;
		_editableText.CurrentLanguage = base.Context.FontFactory.GetCurrentLanguage();
		UpdateFontData();
		if (_editableText.HorizontalAlignment != base.ReadOnlyBrush.TextHorizontalAlignment)
		{
			_editableText.HorizontalAlignment = base.ReadOnlyBrush.TextHorizontalAlignment;
			flag = true;
		}
		if (_editableText.VerticalAlignment != base.ReadOnlyBrush.TextVerticalAlignment)
		{
			_editableText.VerticalAlignment = base.ReadOnlyBrush.TextVerticalAlignment;
			flag = true;
		}
		if (_editableText.TextHeight != _textHeight)
		{
			_textHeight = _editableText.TextHeight;
			flag = true;
		}
		if (flag)
		{
			SetMeasureAndLayoutDirty();
		}
	}

	protected void BlinkCursor()
	{
		_cursorVisible = !_cursorVisible;
	}

	protected void ResetSelected()
	{
		_editableText.ResetSelected();
	}

	protected void DeleteChar(bool nextChar = false)
	{
		int num = _editableText.CursorPosition;
		if (nextChar)
		{
			num++;
		}
		if (num != 0 && num <= Text.Length)
		{
			if (IsObfuscationEnabled)
			{
				RealText = RealText.Substring(0, num - 1) + RealText.Substring(num, RealText.Length - num);
				Text = ObfuscateText(RealText);
			}
			else
			{
				Text = Text.Substring(0, num - 1) + Text.Substring(num, Text.Length - num);
				RealText = Text;
			}
			_editableText.SetCursor(num - 1);
			ResetSelected();
		}
	}

	protected int FindNextWordPosition(int direction)
	{
		return _editableText.FindNextWordPosition(direction);
	}

	protected void MoveCursor(int direction, bool withSelection = false)
	{
		if (!withSelection)
		{
			ResetSelected();
		}
		_editableText.SetCursor(_editableText.CursorPosition + direction, visible: true, withSelection);
	}

	protected string GetAppendCharacterResult(int charCode)
	{
		if (MaxLength > -1 && Text.Length >= MaxLength)
		{
			return RealText;
		}
		int cursorPosition = _editableText.CursorPosition;
		char c = Convert.ToChar(charCode);
		return RealText.Substring(0, cursorPosition) + c + RealText.Substring(cursorPosition, RealText.Length - cursorPosition);
	}

	protected void AppendCharacter(int charCode)
	{
		if (MaxLength <= -1 || Text.Length < MaxLength)
		{
			int cursorPosition = _editableText.CursorPosition;
			RealText = GetAppendCharacterResult(charCode);
			Text = (IsObfuscationEnabled ? ObfuscateText(RealText) : RealText);
			_editableText.SetCursor(cursorPosition + 1);
			ResetSelected();
		}
	}

	protected void AppendText(string text)
	{
		if (MaxLength <= -1 || Text.Length < MaxLength)
		{
			if (MaxLength > -1 && Text.Length + text.Length >= MaxLength)
			{
				text = text.Substring(0, MaxLength - Text.Length);
			}
			int cursorPosition = _editableText.CursorPosition;
			RealText = RealText.Substring(0, cursorPosition) + text + RealText.Substring(cursorPosition, RealText.Length - cursorPosition);
			Text = (IsObfuscationEnabled ? ObfuscateText(RealText) : RealText);
			_editableText.SetCursor(cursorPosition + text.Length);
			ResetSelected();
		}
	}

	protected void DeleteText(int beginIndex, int endIndex)
	{
		if (beginIndex != endIndex)
		{
			RealText = RealText.Substring(0, beginIndex) + RealText.Substring(endIndex, RealText.Length - endIndex);
			Text = (IsObfuscationEnabled ? ObfuscateText(RealText) : RealText);
			_editableText.SetCursor(beginIndex);
			ResetSelected();
		}
	}

	protected void CopyText(int beginIndex, int endIndex)
	{
		if (beginIndex != endIndex)
		{
			int num = Math.Min(beginIndex, endIndex);
			int num2 = Math.Max(beginIndex, endIndex);
			if (num < 0)
			{
				num = 0;
			}
			if (num2 > RealText.Length)
			{
				num2 = RealText.Length;
			}
			Input.SetClipboardText(RealText.Substring(num, num2 - num));
		}
	}

	protected void PasteText()
	{
		string text = Regex.Replace(Input.GetClipboardText(), "[<>]+", " ");
		AppendText(text);
	}

	public override void HandleInput(IReadOnlyList<int> lastKeysPressed)
	{
		if (base.IsDisabled)
		{
			return;
		}
		int count = lastKeysPressed.Count;
		for (int i = 0; i < count; i++)
		{
			int num = lastKeysPressed[i];
			if (num >= 32 && (num < 127 || num >= 160))
			{
				if (num != 60 && num != 62)
				{
					DeleteText(_editableText.SelectedTextBegin, _editableText.SelectedTextEnd);
					AppendCharacter(num);
				}
				_cursorDirection = CursorMovementDirection.None;
				_isSelection = false;
			}
		}
		int tickCount = Environment.TickCount;
		bool flag = false;
		bool flag2 = false;
		if (Input.IsKeyPressed(InputKey.Left))
		{
			_cursorDirection = CursorMovementDirection.Left;
			flag = true;
		}
		else if (Input.IsKeyPressed(InputKey.Right))
		{
			_cursorDirection = CursorMovementDirection.Right;
			flag = true;
		}
		else if ((_cursorDirection == CursorMovementDirection.Left && !Input.IsKeyDown(InputKey.Left)) || (_cursorDirection == CursorMovementDirection.Right && !Input.IsKeyDown(InputKey.Right)))
		{
			_cursorDirection = CursorMovementDirection.None;
			if (!Input.IsKeyDown(InputKey.LeftShift))
			{
				_isSelection = false;
			}
		}
		else if (Input.IsKeyReleased(InputKey.LeftShift))
		{
			_isSelection = false;
		}
		else if (Input.IsKeyDown(InputKey.Home))
		{
			_cursorDirection = CursorMovementDirection.Left;
			flag2 = true;
		}
		else if (Input.IsKeyDown(InputKey.End))
		{
			_cursorDirection = CursorMovementDirection.Right;
			flag2 = true;
		}
		if (flag || flag2)
		{
			if (flag)
			{
				_nextRepeatTime = tickCount + 500;
			}
			if (Input.IsKeyDown(InputKey.LeftShift))
			{
				if (!_editableText.IsAnySelected())
				{
					_editableText.BeginSelection();
				}
				_isSelection = true;
			}
		}
		if (_cursorDirection != 0)
		{
			if (flag || tickCount >= _nextRepeatTime)
			{
				int direction = (int)_cursorDirection;
				if (Input.IsKeyDown(InputKey.LeftControl))
				{
					direction = FindNextWordPosition(direction) - _editableText.CursorPosition;
				}
				MoveCursor(direction, _isSelection);
				if (tickCount >= _nextRepeatTime)
				{
					_nextRepeatTime = tickCount + 30;
				}
			}
			else if (flag2)
			{
				int direction2 = ((_cursorDirection == CursorMovementDirection.Left) ? (-_editableText.CursorPosition) : (_editableText.VisibleText.Length - _editableText.CursorPosition));
				MoveCursor(direction2, _isSelection);
			}
		}
		bool flag3 = false;
		if (Input.IsKeyPressed(InputKey.BackSpace))
		{
			flag3 = true;
			_keyboardAction = KeyboardAction.BackSpace;
			_nextRepeatTime = tickCount + 500;
		}
		else if (Input.IsKeyPressed(InputKey.Delete))
		{
			flag3 = true;
			_keyboardAction = KeyboardAction.Delete;
			_nextRepeatTime = tickCount + 500;
		}
		if ((_keyboardAction == KeyboardAction.BackSpace && !Input.IsKeyDown(InputKey.BackSpace)) || (_keyboardAction == KeyboardAction.Delete && !Input.IsKeyDown(InputKey.Delete)))
		{
			_keyboardAction = KeyboardAction.None;
		}
		if (Input.IsKeyReleased(InputKey.Enter) || Input.IsKeyReleased(InputKey.NumpadEnter))
		{
			EventFired("TextEntered");
		}
		else if (_keyboardAction == KeyboardAction.BackSpace || _keyboardAction == KeyboardAction.Delete)
		{
			if (flag3 || tickCount >= _nextRepeatTime)
			{
				if (_editableText.IsAnySelected())
				{
					DeleteText(_editableText.SelectedTextBegin, _editableText.SelectedTextEnd);
				}
				else if (Input.IsKeyDown(InputKey.LeftControl))
				{
					int num2 = FindNextWordPosition(-1) - _editableText.CursorPosition;
					DeleteText(_editableText.CursorPosition + num2, _editableText.CursorPosition);
				}
				else
				{
					DeleteChar(_keyboardAction == KeyboardAction.Delete);
				}
				if (tickCount >= _nextRepeatTime)
				{
					_nextRepeatTime = tickCount + 30;
				}
			}
		}
		else if (Input.IsKeyDown(InputKey.LeftControl))
		{
			if (Input.IsKeyPressed(InputKey.A))
			{
				_editableText.SelectAll();
			}
			else if (Input.IsKeyPressed(InputKey.C))
			{
				CopyText(_editableText.SelectedTextBegin, _editableText.SelectedTextEnd);
			}
			else if (Input.IsKeyPressed(InputKey.X))
			{
				CopyText(_editableText.SelectedTextBegin, _editableText.SelectedTextEnd);
				DeleteText(_editableText.SelectedTextBegin, _editableText.SelectedTextEnd);
			}
			else if (Input.IsKeyPressed(InputKey.V))
			{
				DeleteText(_editableText.SelectedTextBegin, _editableText.SelectedTextEnd);
				PasteText();
			}
		}
	}

	protected internal override void OnGainFocus()
	{
		base.OnGainFocus();
		_editableText.SetCursor(RealText.Length);
		if (string.IsNullOrEmpty(RealText) && !string.IsNullOrEmpty(DefaultSearchText))
		{
			_editableText.VisibleText = "";
		}
	}

	protected internal override void OnLoseFocus()
	{
		base.OnLoseFocus();
		_editableText.ResetSelected();
		_isSelection = false;
		_editableText.SetCursor(0, visible: false);
		if (string.IsNullOrEmpty(RealText) && !string.IsNullOrEmpty(DefaultSearchText))
		{
			_editableText.VisibleText = DefaultSearchText;
		}
	}

	private void UpdateText()
	{
		if (base.IsDisabled)
		{
			SetState("Disabled");
		}
		else if (base.IsPressed)
		{
			SetState("Pressed");
		}
		else if (base.IsHovered)
		{
			SetState("Hovered");
		}
		else
		{
			SetState("Default");
		}
		if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(DefaultSearchText) && _mouseState == MouseState.None && base.EventManager.FocusedWidget != this)
		{
			_editableText.VisibleText = DefaultSearchText;
		}
	}

	private void UpdateFontData()
	{
		if (_lastFontBrush == base.ReadOnlyBrush && _lastScale == base._scaleToUse && _lastLanguageCode == base.Context.FontFactory.CurrentLangageID)
		{
			return;
		}
		_editableText.StyleFontContainer.ClearFonts();
		foreach (Style style in base.ReadOnlyBrush.Styles)
		{
			Font font = null;
			font = ((style.Font == null) ? ((base.ReadOnlyBrush.Font == null) ? base.Context.FontFactory.DefaultFont : base.ReadOnlyBrush.Font) : style.Font);
			Font mappedFontForLocalization = base.Context.FontFactory.GetMappedFontForLocalization(font.Name);
			_editableText.StyleFontContainer.Add(style.Name, mappedFontForLocalization, (float)base.ReadOnlyBrush.FontSize * base._scaleToUse);
		}
		_lastFontBrush = base.ReadOnlyBrush;
		_lastScale = base._scaleToUse;
		_lastLanguageCode = base.Context.FontFactory.CurrentLangageID;
		_editableText.CurrentLanguage = base.Context.FontFactory.GetCurrentLanguage();
	}

	private Font GetFont(Style style = null)
	{
		if (style?.Font != null)
		{
			return base.Context.FontFactory.GetMappedFontForLocalization(style.Font.Name);
		}
		if (base.ReadOnlyBrush.Font != null)
		{
			return base.Context.FontFactory.GetMappedFontForLocalization(base.ReadOnlyBrush.Font.Name);
		}
		return base.Context.FontFactory.DefaultFont;
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (base.Size.X > 0f && base.Size.Y > 0f)
		{
			Vector2 localMousePosition = LocalMousePosition;
			bool focus = _mouseState == MouseState.Down;
			_editableText.UpdateSize((int)base.Size.X, (int)base.Size.Y);
			SetEditTextParameters();
			UpdateFontData();
			bool isFixedWidth = base.WidthSizePolicy != SizePolicy.CoverChildren || base.MaxWidth != 0f;
			bool isFixedHeight = base.HeightSizePolicy != SizePolicy.CoverChildren || base.MaxHeight != 0f;
			_editableText.Update(base.Context.SpriteData, localMousePosition, focus, isFixedWidth, isFixedHeight, base._scaleToUse);
		}
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		base.OnRender(twoDimensionContext, drawContext);
		if (string.IsNullOrEmpty(_editableText.Value))
		{
			return;
		}
		Vector2 globalPosition = base.GlobalPosition;
		foreach (RichTextPart part in _editableText.GetParts())
		{
			DrawObject2D drawObject2D = part.DrawObject2D;
			Material material = null;
			Style styleOrDefault = base.ReadOnlyBrush.GetStyleOrDefault(part.Style);
			Font font = GetFont(styleOrDefault);
			int fontSize = styleOrDefault.FontSize;
			float scaleFactor = (float)fontSize * base._scaleToUse;
			float num = (float)fontSize / (float)font.Size;
			float height = (float)font.LineHeight * num * base._scaleToUse;
			TextMaterial textMaterial = styleOrDefault.CreateTextMaterial(drawContext);
			textMaterial.ColorFactor *= base.ReadOnlyBrush.GlobalColorFactor;
			textMaterial.AlphaFactor *= base.ReadOnlyBrush.GlobalAlphaFactor;
			textMaterial.Color *= base.ReadOnlyBrush.GlobalColor;
			textMaterial.Texture = font.FontSprite.Texture;
			textMaterial.ScaleFactor = scaleFactor;
			textMaterial.Smooth = font.Smooth;
			textMaterial.SmoothingConstant = font.SmoothingConstant;
			if (textMaterial.GlowRadius > 0f || textMaterial.Blur > 0f || textMaterial.OutlineAmount > 0f)
			{
				TextMaterial textMaterial2 = styleOrDefault.CreateTextMaterial(drawContext);
				textMaterial2.CopyFrom(textMaterial);
				drawContext.Draw(globalPosition.X, globalPosition.Y, textMaterial2, drawObject2D, base.Size.X, base.Size.Y);
			}
			textMaterial.GlowRadius = 0f;
			textMaterial.Blur = 0f;
			textMaterial.OutlineAmount = 0f;
			material = textMaterial;
			if (part.Style == "Highlight")
			{
				SpriteData spriteData = base.Context.SpriteData;
				string name = "warm_overlay";
				Sprite sprite = spriteData.GetSprite(name);
				SimpleMaterial simpleMaterial = drawContext.CreateSimpleMaterial();
				simpleMaterial.Reset(sprite?.Texture);
				drawContext.DrawSprite(sprite, simpleMaterial, globalPosition.X + part.PartPosition.X, globalPosition.Y + part.PartPosition.Y, 1f, part.WordWidth, height, horizontalFlip: false, verticalFlip: false);
			}
			drawContext.Draw(globalPosition.X, globalPosition.Y, material, drawObject2D, base.Size.X, base.Size.Y);
		}
		if (_editableText.IsCursorVisible())
		{
			Style styleOrDefault2 = base.ReadOnlyBrush.GetStyleOrDefault("Default");
			Font font2 = GetFont(styleOrDefault2);
			int fontSize2 = styleOrDefault2.FontSize;
			float num2 = (float)fontSize2 / (float)font2.Size;
			float height2 = (float)font2.LineHeight * num2 * base._scaleToUse;
			SpriteData spriteData2 = base.Context.SpriteData;
			string name2 = "BlankWhiteSquare_9";
			Sprite sprite2 = spriteData2.GetSprite(name2);
			SimpleMaterial simpleMaterial2 = drawContext.CreateSimpleMaterial();
			simpleMaterial2.Reset(sprite2?.Texture);
			Vector2 cursorPosition = _editableText.GetCursorPosition(font2, fontSize2, base._scaleToUse);
			drawContext.DrawSprite(sprite2, simpleMaterial2, (int)(globalPosition.X + cursorPosition.X), globalPosition.Y + cursorPosition.Y, 1f, 1f, height2, horizontalFlip: false, verticalFlip: false);
		}
	}

	protected internal override void OnMousePressed()
	{
		base.OnMousePressed();
		_mouseDownPosition = LocalMousePosition;
		_mouseState = MouseState.Down;
		_editableText.HighlightStart = true;
		_editableText.HighlightEnd = false;
	}

	protected internal override void OnMouseReleased()
	{
		base.OnMouseReleased();
		_mouseState = MouseState.Up;
		_editableText.HighlightEnd = true;
	}

	private void OnObfuscationToggled(bool isEnabled)
	{
		if (isEnabled)
		{
			Text = ObfuscateText(RealText);
		}
		else
		{
			Text = RealText;
		}
	}

	private string ObfuscateText(string stringToObfuscate)
	{
		return new string(_obfuscationChar, stringToObfuscate.Length);
	}

	public virtual void SetAllText(string text)
	{
		DeleteText(0, RealText.Length);
		string text2 = Regex.Replace(text, "[<>]+", " ");
		AppendText(text2);
	}
}
