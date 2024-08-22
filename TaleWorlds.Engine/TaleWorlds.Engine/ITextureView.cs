using System;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface ITextureView
{
	[EngineMethod("create_texture_view", false)]
	TextureView CreateTextureView();

	[EngineMethod("set_texture", false)]
	void SetTexture(UIntPtr pointer, UIntPtr texture_ptr);
}
