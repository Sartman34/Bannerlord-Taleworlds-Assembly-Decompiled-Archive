namespace TaleWorlds.TwoDimension;

internal class TextMeshGenerator
{
	private Font _font;

	private float[] _vertices;

	private float[] _uvs;

	private uint[] _indices;

	private int _textMeshCharacterCount;

	private float _scaleValue;

	internal TextMeshGenerator()
	{
		_scaleValue = 1f;
	}

	internal void Refresh(Font font, int possibleMaxCharacterLength, float scaleValue)
	{
		_font = font;
		_textMeshCharacterCount = 0;
		int num = possibleMaxCharacterLength * 8 * 2;
		int num2 = possibleMaxCharacterLength * 8 * 2;
		if (_vertices == null || _vertices.Length < num)
		{
			_vertices = new float[num];
		}
		if (_uvs == null || _uvs.Length < num2)
		{
			_uvs = new float[num2];
		}
		_scaleValue = scaleValue;
	}

	internal DrawObject2D GenerateMesh()
	{
		int num = _textMeshCharacterCount * 6;
		if ((_indices == null || _indices.Length != num) && (_indices == null || _indices.Length < num))
		{
			_indices = new uint[num];
			for (uint num2 = 0u; num2 < _textMeshCharacterCount; num2++)
			{
				int num3 = (int)(6 * num2);
				uint num4 = 4 * num2;
				_indices[num3] = num4;
				_indices[num3 + 1] = 1 + num4;
				_indices[num3 + 2] = 2 + num4;
				_indices[num3 + 3] = num4;
				_indices[num3 + 4] = 2 + num4;
				_indices[num3 + 5] = 3 + num4;
			}
		}
		DrawObject2D drawObject2D = new DrawObject2D(MeshTopology.Triangles, _vertices, _uvs, _indices, num);
		drawObject2D.RecalculateProperties();
		return drawObject2D;
	}

	internal void AddCharacterToMesh(float x, float y, BitmapFontCharacter fontCharacter)
	{
		float minU = _font.FontSprite.MinU;
		float minV = _font.FontSprite.MinV;
		float num = 1f / (float)_font.FontSprite.Texture.Width;
		float num2 = 1f / (float)_font.FontSprite.Texture.Height;
		float num3 = minU + (float)fontCharacter.X * num;
		float num4 = minV + (float)fontCharacter.Y * num2;
		float num5 = num3 + (float)fontCharacter.Width * num;
		float num6 = num4 + (float)fontCharacter.Height * num2;
		float num7 = (float)fontCharacter.Width * _scaleValue;
		float num8 = (float)fontCharacter.Height * _scaleValue;
		_uvs[8 * _textMeshCharacterCount] = num3;
		_uvs[8 * _textMeshCharacterCount + 1] = num4;
		_uvs[8 * _textMeshCharacterCount + 2] = num5;
		_uvs[8 * _textMeshCharacterCount + 3] = num4;
		_uvs[8 * _textMeshCharacterCount + 4] = num5;
		_uvs[8 * _textMeshCharacterCount + 5] = num6;
		_uvs[8 * _textMeshCharacterCount + 6] = num3;
		_uvs[8 * _textMeshCharacterCount + 7] = num6;
		_vertices[8 * _textMeshCharacterCount] = x;
		_vertices[8 * _textMeshCharacterCount + 1] = y;
		_vertices[8 * _textMeshCharacterCount + 2] = x + num7;
		_vertices[8 * _textMeshCharacterCount + 3] = y;
		_vertices[8 * _textMeshCharacterCount + 4] = x + num7;
		_vertices[8 * _textMeshCharacterCount + 5] = y + num8;
		_vertices[8 * _textMeshCharacterCount + 6] = x;
		_vertices[8 * _textMeshCharacterCount + 7] = y + num8;
		_textMeshCharacterCount++;
	}

	internal void AddValueToX(float value)
	{
		for (int i = 0; i < _vertices.Length; i += 2)
		{
			_vertices[i] += value;
		}
	}

	internal void AddValueToY(float value)
	{
		for (int i = 0; i < _vertices.Length; i += 2)
		{
			_vertices[i + 1] += value;
		}
	}
}
