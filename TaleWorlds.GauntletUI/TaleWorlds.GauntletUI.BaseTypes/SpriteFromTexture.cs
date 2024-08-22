using System;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.BaseTypes;

internal class SpriteFromTexture : Sprite
{
	private Texture _texture;

	public override Texture Texture => _texture;

	public SpriteFromTexture(Texture texture, int width, int height)
		: base("Sprite", width, height)
	{
		_texture = texture;
	}

	public override float GetScaleToUse(float width, float height, float scale)
	{
		throw new NotImplementedException();
	}

	protected override DrawObject2D GetArrays(SpriteDrawData spriteDrawData)
	{
		throw new NotImplementedException();
	}
}
