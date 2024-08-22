using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace TaleWorlds.TwoDimension;

public class EditableText : RichText
{
	private bool _cursorVisible;

	private int _visibleStart;

	private int _selectionAnchor;

	private string _realVisibleText;

	private Regex _nextWordRegex;

	public int CursorPosition { get; private set; }

	public bool HighlightStart { get; set; }

	public bool HighlightEnd { get; set; }

	public int SelectedTextBegin { get; private set; }

	public int SelectedTextEnd { get; private set; }

	public float BlinkTimer { get; set; }

	public string VisibleText { get; set; }

	public EditableText(int width, int height, Font font, Func<int, Font> getUsableFontForCharacter)
		: base(width, height, font, getUsableFontForCharacter)
	{
		_cursorVisible = false;
		CursorPosition = 0;
		_visibleStart = 0;
		VisibleText = "";
		BlinkTimer = 0f;
		HighlightStart = false;
		HighlightEnd = true;
		_selectionAnchor = 0;
		string pattern = "\\w+";
		_nextWordRegex = new Regex(pattern);
	}

	public void SetCursorPosition(int position, bool visible)
	{
		if (CursorPosition != position || _cursorVisible != visible)
		{
			CursorPosition = position;
			if (_visibleStart > CursorPosition)
			{
				_visibleStart = CursorPosition;
			}
			_cursorVisible = visible;
			SetAllDirty();
		}
	}

	public void BlinkCursor()
	{
		_cursorVisible = !_cursorVisible;
	}

	public bool IsCursorVisible()
	{
		return _cursorVisible;
	}

	public void ResetSelected()
	{
		_visibleStart = 0;
		_selectionAnchor = 0;
		SelectedTextBegin = 0;
		SelectedTextEnd = 0;
	}

	public void BeginSelection()
	{
		_selectionAnchor = CursorPosition;
	}

	public bool IsAnySelected()
	{
		return SelectedTextEnd != SelectedTextBegin;
	}

	public Vector2 GetCursorPosition(Font font, float fontSize, float scale)
	{
		float num = fontSize / (float)font.Size;
		float num2 = (float)font.LineHeight * num * scale;
		string word = VisibleText.Substring(_visibleStart, CursorPosition - _visibleStart);
		float num3 = font.GetWordWidth(word, 0.5f) * num * scale;
		float num4 = font.GetWordWidth(_realVisibleText, 0.5f) * num * scale;
		float num5 = 0f;
		if (base.HorizontalAlignment == TextHorizontalAlignment.Center)
		{
			num5 = ((float)base.Width - num4) * 0.5f;
		}
		else if (base.HorizontalAlignment == TextHorizontalAlignment.Right)
		{
			num5 = (float)base.Width - num4;
		}
		float y = 0f;
		if (base.VerticalAlignment == TextVerticalAlignment.Center)
		{
			y = ((float)base.Height - num2) * 0.5f;
		}
		else if (base.VerticalAlignment == TextVerticalAlignment.Bottom)
		{
			y = (float)base.Height - num2;
		}
		return new Vector2(num5 + num3, y);
	}

	private void UpdateSelectedText(Vector2 mousePosition)
	{
		string text = VisibleText;
		_visibleStart = Math.Min(_visibleStart, CursorPosition);
		StyleFontContainer.FontData fontData = base.StyleFontContainer.GetFontData("Default");
		float num = fontData.FontSize / (float)fontData.Font.Size;
		int num2 = 10;
		int i;
		for (i = 0; i < _visibleStart; i++)
		{
			if (!(text != ""))
			{
				break;
			}
			if (!(fontData.Font.GetWordWidth(text, 0f) * num > (float)(base.Width - num2 - num2)))
			{
				break;
			}
			text = text.Substring(1);
		}
		while (text.Length > CursorPosition - _visibleStart && text != "" && fontData.Font.GetWordWidth(text, 0f) * num > (float)(base.Width - num2 - num2))
		{
			text = text.Substring(0, text.Length - 1);
		}
		while (text != "" && fontData.Font.GetWordWidth(text, 0f) * num > (float)(base.Width - num2 - num2))
		{
			text = text.Substring(1);
			i++;
			_visibleStart = Math.Min(_visibleStart + 1, CursorPosition);
		}
		Vector2 mousePosition2 = mousePosition;
		if (base.TextOutput != null && base.HorizontalAlignment != 0)
		{
			if (base.HorizontalAlignment == TextHorizontalAlignment.Center)
			{
				mousePosition2.X -= ((float)base.Width - base.TextOutput.GetLine(0).Width) * 0.5f;
			}
			else if (base.HorizontalAlignment == TextHorizontalAlignment.Right)
			{
				mousePosition2.X -= (float)base.Width - base.TextOutput.GetLine(0).Width;
			}
		}
		if (HighlightStart)
		{
			SelectedTextBegin = FindCharacterPosition(VisibleText, text, num, mousePosition2, i);
			HighlightStart = false;
			SetCursor(SelectedTextBegin);
		}
		if (!HighlightEnd)
		{
			SelectedTextEnd = FindCharacterPosition(VisibleText, text, num, mousePosition2, i);
			SetCursor(SelectedTextEnd);
		}
		else if (SelectedTextBegin > SelectedTextEnd)
		{
			int selectedTextBegin = SelectedTextBegin;
			SelectedTextBegin = SelectedTextEnd;
			SelectedTextEnd = selectedTextBegin;
		}
		int num3 = Math.Min(Math.Max(SelectedTextBegin - i, 0), text.Length);
		int num4 = Math.Min(Math.Max(SelectedTextEnd - i, 0), text.Length);
		if (num3 > num4)
		{
			int num5 = num3;
			num3 = num4;
			num4 = num5;
		}
		if (num3 > num4)
		{
			int num6 = num3;
			num3 = num4;
			num4 = num6;
		}
		string value = text.Substring(0, num3) + "<span style=\"Highlight\">" + text.Substring(num3, num4 - num3) + "</span>" + text.Substring(num4, text.Length - num4);
		_realVisibleText = text.Substring(0, num3) + text.Substring(num3, num4 - num3) + text.Substring(num4, text.Length - num4);
		base.Value = value;
	}

	public override void Update(SpriteData spriteData, Vector2 focusPosition, bool focus, bool isFixedWidth, bool isFixedHeight, float renderScale)
	{
		base.Update(spriteData, focusPosition, focus, isFixedWidth, isFixedHeight, renderScale);
		UpdateSelectedText(focusPosition);
	}

	public void SelectAll()
	{
		SelectedTextBegin = 0;
		SetCursor(VisibleText.Length, visible: true, withSelection: true);
	}

	public int FindNextWordPosition(int direction)
	{
		MatchCollection matchCollection = _nextWordRegex.Matches(VisibleText);
		int result = 0;
		int result2 = VisibleText.Length;
		foreach (Match item in matchCollection)
		{
			int index = item.Index;
			if (index < CursorPosition)
			{
				result = index;
			}
			else if (index > CursorPosition)
			{
				result2 = index;
				break;
			}
		}
		if (direction <= 0)
		{
			return result;
		}
		return result2;
	}

	public void SetCursor(int position, bool visible = true, bool withSelection = false)
	{
		BlinkTimer = 0f;
		int num = Mathf.Clamp(position, 0, VisibleText.Length);
		SetCursorPosition(num, visible);
		if (withSelection)
		{
			SelectedTextBegin = Math.Min(num, _selectionAnchor);
			SelectedTextEnd = Math.Max(num, _selectionAnchor);
		}
	}

	private int FindCharacterPosition(string fullText, string text, float scale, Vector2 mousePosition, int omitCount)
	{
		int length = text.Length;
		int i = 0;
		int num = 0;
		float num2 = 0f;
		if (mousePosition.X > (float)base.Width + 15f * scale)
		{
			return Math.Min(omitCount + length + 1, fullText.Length);
		}
		if (mousePosition.X < -15f * scale)
		{
			return Math.Max(omitCount - 1, 0);
		}
		StyleFontContainer.FontData fontData = base.StyleFontContainer.GetFontData("Default");
		for (; i < length; i++)
		{
			num = i + omitCount;
			num2 = fontData.Font.GetWordWidth(text.Substring(0, i + 1), 0f) * scale;
			if (num2 > mousePosition.X)
			{
				return num;
			}
		}
		return i + omitCount;
	}
}
