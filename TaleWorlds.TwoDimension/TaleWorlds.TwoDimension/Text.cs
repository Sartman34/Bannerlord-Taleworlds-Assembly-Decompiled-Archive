using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TaleWorlds.TwoDimension.BitmapFont;

namespace TaleWorlds.TwoDimension;

public class Text : IText
{
	private TextHorizontalAlignment _horizontalAlignment;

	private TextVerticalAlignment _verticalAlignment;

	private DrawObject2D _drawObject2D;

	private bool _meshNeedsUpdate;

	private bool _preferredSizeNeedsUpdate;

	private bool _fixedHeight;

	private bool _fixedWidth;

	private float _desiredHeight;

	private float _desiredWidth;

	private Vector2 _preferredSize;

	private string _text;

	private List<TextToken> _tokens;

	private int _fontSize;

	private int _width;

	private int _height;

	private Font _font;

	private float _scaleValue;

	private readonly TextMeshGenerator _textMeshGenerator;

	private readonly Func<int, Font> _getUsableFontForCharacter;

	private bool _skipLineOnContainerExceeded = true;

	public ILanguage CurrentLanguage { get; set; }

	public float ScaleToFitTextInLayout { get; private set; } = 1f;


	public int LineCount { get; private set; }

	public DrawObject2D DrawObject2D
	{
		get
		{
			if (_meshNeedsUpdate)
			{
				RecalculateTextMesh();
				if (ScaleToFitTextInLayout != 1f)
				{
					RecalculateTextMesh(ScaleToFitTextInLayout);
				}
			}
			return _drawObject2D;
		}
	}

	public Font Font
	{
		get
		{
			return _font;
		}
		set
		{
			if (_font != value)
			{
				_meshNeedsUpdate = true;
				_preferredSizeNeedsUpdate = true;
				_font = value;
			}
		}
	}

	private float ExtraPaddingHorizontal => 0.5f;

	private float ExtraPaddingVertical => 5f;

	public TextHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return _horizontalAlignment;
		}
		set
		{
			if (_horizontalAlignment != value)
			{
				_horizontalAlignment = value;
				_meshNeedsUpdate = true;
				_preferredSizeNeedsUpdate = true;
			}
		}
	}

	public TextVerticalAlignment VerticalAlignment
	{
		get
		{
			return _verticalAlignment;
		}
		set
		{
			if (_verticalAlignment != value)
			{
				_verticalAlignment = value;
				_meshNeedsUpdate = true;
				_preferredSizeNeedsUpdate = true;
			}
		}
	}

	public float FontSize
	{
		get
		{
			return _fontSize;
		}
		set
		{
			if (_fontSize != (int)value)
			{
				_fontSize = (int)value;
				_meshNeedsUpdate = true;
				_preferredSizeNeedsUpdate = true;
			}
		}
	}

	public string Value
	{
		get
		{
			return _text;
		}
		set
		{
			string text = value;
			if (text == null)
			{
				text = "";
			}
			if (_text != text)
			{
				_text = text;
				_tokens = TextParser.Parse(text, CurrentLanguage);
				_meshNeedsUpdate = true;
				_preferredSizeNeedsUpdate = true;
			}
		}
	}

	private float EmptyCharacterWidth => ((float)Font.Characters[32].XAdvance + ExtraPaddingHorizontal) * _scaleValue;

	private float LineHeight => ((float)Font.Base + ExtraPaddingVertical) * _scaleValue;

	public bool SkipLineOnContainerExceeded
	{
		get
		{
			return _skipLineOnContainerExceeded;
		}
		set
		{
			if (value != _skipLineOnContainerExceeded)
			{
				_skipLineOnContainerExceeded = value;
				_meshNeedsUpdate = true;
				_preferredSizeNeedsUpdate = true;
			}
		}
	}

	public Text(int width, int height, Font bitmapFont, Func<int, Font> getUsableFontForCharacter)
	{
		Font = bitmapFont;
		_width = width;
		_height = height;
		_getUsableFontForCharacter = getUsableFontForCharacter;
		_meshNeedsUpdate = true;
		_preferredSizeNeedsUpdate = true;
		_text = "";
		_fontSize = 32;
		_tokens = null;
		_textMeshGenerator = new TextMeshGenerator();
	}

	public Vector2 GetPreferredSize(bool fixedWidth, float widthSize, bool fixedHeight, float heightSize, SpriteData spriteData, float renderScale)
	{
		_fixedWidth = fixedWidth;
		_fixedHeight = fixedHeight;
		_desiredHeight = heightSize;
		_desiredWidth = widthSize;
		if (_preferredSizeNeedsUpdate)
		{
			_preferredSize = new Vector2(0f, 0f);
			if (_fontSize != 0 && !string.IsNullOrEmpty(_text))
			{
				_scaleValue = (float)_fontSize / (float)Font.Size;
				float num = 0f;
				LineCount = 1;
				float lineHeight = LineHeight;
				float emptyCharacterWidth = EmptyCharacterWidth;
				for (int i = 0; i < _tokens.Count; i++)
				{
					TextToken textToken = _tokens[i];
					if (textToken.Type == TextToken.TokenType.NewLine)
					{
						LineCount++;
						if (num > _preferredSize.X)
						{
							_preferredSize.X = num;
						}
						num = 0f;
					}
					else if (textToken.Type == TextToken.TokenType.EmptyCharacter || textToken.Type == TextToken.TokenType.NonBreakingSpace)
					{
						num += emptyCharacterWidth;
					}
					else
					{
						if (textToken.Type != TextToken.TokenType.Character)
						{
							continue;
						}
						char token = textToken.Token;
						float num2 = Font.GetCharacterWidth(token, ExtraPaddingHorizontal) * _scaleValue;
						if (fixedWidth && _skipLineOnContainerExceeded)
						{
							if (num + num2 > widthSize && num > 0f)
							{
								int indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex = TextHelper.GetIndexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex(_tokens, i, CurrentLanguage);
								if (indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex == -1)
								{
									int startIndex = Math.Max(0, _tokens.Count - 2);
									int endIndex = Math.Max(0, _tokens.Count - 1);
									float totalWordWidthBetweenIndices = TextHelper.GetTotalWordWidthBetweenIndices(startIndex, endIndex, _tokens, GetFontForTextToken, ExtraPaddingHorizontal, _scaleValue);
									LineCount++;
									num = totalWordWidthBetweenIndices + num2;
									continue;
								}
								float totalWordWidthBetweenIndices2 = TextHelper.GetTotalWordWidthBetweenIndices(indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex, i, _tokens, GetFontForTextToken, ExtraPaddingHorizontal, _scaleValue);
								if (num - totalWordWidthBetweenIndices2 > _preferredSize.X)
								{
									_preferredSize.X = num - totalWordWidthBetweenIndices2;
								}
								num = totalWordWidthBetweenIndices2 + num2;
								LineCount++;
							}
							else
							{
								num += num2;
							}
						}
						else
						{
							num += num2;
						}
					}
				}
				if (num > _preferredSize.X)
				{
					_preferredSize.X = num;
				}
				_preferredSize.Y = (float)LineCount * lineHeight;
			}
			_preferredSize = new Vector2((float)Math.Ceiling(_preferredSize.X) + 1f, (float)Math.Ceiling(_preferredSize.Y) + 1f);
			_preferredSizeNeedsUpdate = false;
		}
		return _preferredSize;
	}

	public void UpdateSize(int width, int height)
	{
		if (_width != width || _height != height)
		{
			_width = width;
			_height = height;
			_meshNeedsUpdate = true;
			_preferredSizeNeedsUpdate = true;
			ScaleToFitTextInLayout = 1f;
		}
	}

	private Font GetFontForTextToken(TextToken token)
	{
		return _getUsableFontForCharacter(token.Token);
	}

	private void RecalculateTextMesh(float customScaleToFitText = 1f)
	{
		if (_fontSize == 0 || string.IsNullOrEmpty(_text))
		{
			_drawObject2D = null;
			return;
		}
		int num = _text.Length;
		_scaleValue = (float)_fontSize / (float)Font.Size * customScaleToFitText;
		float num2 = 0f;
		float num3 = 0f;
		BitmapFontCharacter bitmapFontCharacter = Font.Characters[32];
		float num4 = ((float)Font.Base + ExtraPaddingVertical) * _scaleValue;
		float num5 = ((float)bitmapFontCharacter.XAdvance + ExtraPaddingHorizontal) * _scaleValue;
		TextOutput textOutput = new TextOutput(num4);
		for (int i = 0; i < _tokens.Count; i++)
		{
			TextToken textToken = _tokens[i];
			if (textToken.Type == TextToken.TokenType.NewLine)
			{
				textOutput.AddNewLine(currentLineEnded: true);
				num2 = 0f;
				num3 += num4;
			}
			else if (textToken.Type == TextToken.TokenType.EmptyCharacter || textToken.Type == TextToken.TokenType.NonBreakingSpace)
			{
				textOutput.AddToken(textToken, num5, _scaleValue);
				num2 += num5;
			}
			else
			{
				if (textToken.Type == TextToken.TokenType.ZeroWidthSpace)
				{
					continue;
				}
				if (textToken.Type == TextToken.TokenType.WordJoiner)
				{
					textOutput.AddToken(textToken, 0f, _scaleValue);
				}
				else
				{
					if (textToken.Type != TextToken.TokenType.Character)
					{
						continue;
					}
					char token = textToken.Token;
					float num6 = Font.GetCharacterWidth(token, ExtraPaddingHorizontal) * _scaleValue;
					if (num6 == 0f)
					{
						num6 = (_getUsableFontForCharacter(token)?.GetCharacterWidth(token, ExtraPaddingHorizontal) * _scaleValue) ?? 0f;
					}
					if (num2 + num6 > (float)_width && num2 > 0f && _skipLineOnContainerExceeded)
					{
						_ = num3 + num4;
						_ = _height;
						int indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex = TextHelper.GetIndexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex(_tokens, i, CurrentLanguage);
						int num7 = TextHelper.GetIndexOfFirstAppropriateCharacterToMoveToNextLineForwardsFromIndex(_tokens, i, CurrentLanguage);
						float num8 = 0f;
						if (indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex != -1)
						{
							if (num7 == -1)
							{
								num7 = _tokens.Count;
							}
							num8 = TextHelper.GetTotalWordWidthBetweenIndices(indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex, num7, _tokens, GetFontForTextToken, ExtraPaddingHorizontal, _scaleValue);
						}
						if (((num8 != 0f && (indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex == -1 || !(num8 <= (float)_width))) || indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex == -1) && textOutput.Tokens.Any())
						{
							List<TextTokenOutput> list = textOutput.RemoveTokensFromEnd(1);
							float tokenWidth = Font.GetCharacterWidth(CurrentLanguage.GetLineSeperatorChar(), ExtraPaddingHorizontal) * _scaleValue;
							textOutput.AddToken(TextToken.CreateCharacter(CurrentLanguage.GetLineSeperatorChar()), tokenWidth, _scaleValue);
							textOutput.AddNewLine(currentLineEnded: false);
							num3 += num4;
							textOutput.AddToken(list[0].Token, list[0].Width, _scaleValue);
							textOutput.AddToken(textToken, num6, _scaleValue);
							num++;
							num2 = num6 + list[0].Width;
							continue;
						}
						num2 = TextHelper.GetTotalWordWidthBetweenIndices(indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex, i, _tokens, GetFontForTextToken, ExtraPaddingHorizontal, _scaleValue);
						List<TextTokenOutput> list2 = textOutput.RemoveTokensFromEnd(i - indexOfFirstAppropriateCharacterToMoveToNextLineBackwardsFromIndex);
						textOutput.AddNewLine(currentLineEnded: false);
						num3 += num4;
						for (int num9 = list2.Count - 1; num9 >= 0; num9--)
						{
							TextTokenOutput textTokenOutput = list2[num9];
							if (textTokenOutput.Token.Type != 0 && textTokenOutput.Token.Type != TextToken.TokenType.ZeroWidthSpace)
							{
								textOutput.AddToken(textTokenOutput.Token, textTokenOutput.Width, _scaleValue);
							}
						}
						textOutput.AddToken(textToken, num6, _scaleValue);
						num2 += num6;
					}
					else
					{
						textOutput.AddToken(textToken, num6, _scaleValue);
						num2 += num6;
					}
				}
			}
		}
		_textMeshGenerator.Refresh(Font, num, _scaleValue);
		num2 = 0f;
		num3 = 0f;
		for (int j = 0; j < textOutput.LineCount; j++)
		{
			TextLineOutput line = textOutput.GetLine(j);
			float num10 = num5;
			switch (_horizontalAlignment)
			{
			case TextHorizontalAlignment.Center:
			{
				float num13 = 0f;
				if (!line.LineEnded)
				{
					for (int l = 1; l < line.TokenCount && line.GetToken(line.TokenCount - l).Type == TextToken.TokenType.EmptyCharacter; l++)
					{
						num13 += num5;
					}
					for (int l = 0; l < line.TokenCount && line.GetToken(l).Type == TextToken.TokenType.EmptyCharacter; l++)
					{
						num13 += num5;
					}
				}
				num2 = ((float)_width - (line.Width - num13)) * 0.5f;
				break;
			}
			case TextHorizontalAlignment.Right:
				num2 = (float)_width - line.Width;
				break;
			case TextHorizontalAlignment.Justify:
			{
				float num11 = (float)_width - line.TextWidth;
				if (!line.LineEnded)
				{
					int num12 = line.EmptyCharacterCount;
					for (int k = 1; line.GetToken(line.TokenCount - k).Type == TextToken.TokenType.EmptyCharacter; k++)
					{
						num12--;
					}
					for (int k = 0; line.GetToken(k).Type == TextToken.TokenType.EmptyCharacter; k++)
					{
						num12--;
					}
					num10 = num11 / (float)num12;
				}
				break;
			}
			}
			for (int m = 0; m < line.TokenCount; m++)
			{
				Font font = Font;
				TextToken token2 = line.GetToken(m);
				switch (token2.Type)
				{
				case TextToken.TokenType.EmptyCharacter:
				case TextToken.TokenType.NonBreakingSpace:
					num2 += num10;
					break;
				case TextToken.TokenType.Character:
				{
					int key = token2.Token;
					if (!Font.Characters.ContainsKey(key))
					{
						key = 0;
					}
					BitmapFontCharacter fontCharacter = font.Characters[key];
					float x = num2 + (float)fontCharacter.XOffset * _scaleValue;
					float y = num3 + (float)fontCharacter.YOffset * _scaleValue;
					_textMeshGenerator.AddCharacterToMesh(x, y, fontCharacter);
					num2 += ((float)fontCharacter.XAdvance + ExtraPaddingHorizontal) * _scaleValue;
					break;
				}
				}
			}
			num2 = 0f;
			num3 += num4;
		}
		if (_verticalAlignment == TextVerticalAlignment.Center || _verticalAlignment == TextVerticalAlignment.Bottom)
		{
			float num14;
			if (_verticalAlignment == TextVerticalAlignment.Center)
			{
				num14 = (float)_height - num3;
				num14 *= 0.5f;
			}
			else
			{
				num14 = (float)_height - num3;
			}
			_textMeshGenerator.AddValueToY(num14);
		}
		_drawObject2D = _textMeshGenerator.GenerateMesh();
		_meshNeedsUpdate = false;
		if (_fixedHeight && num3 > _desiredHeight && _desiredHeight > 1f)
		{
			ScaleToFitTextInLayout = _desiredHeight / num3;
		}
		if (_fixedWidth && num2 > _desiredWidth && _desiredWidth > 1f)
		{
			ScaleToFitTextInLayout = Math.Min(ScaleToFitTextInLayout, _desiredWidth / num2);
		}
	}
}
