using System;
using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IAsyncTask
{
	[EngineMethod("create_with_function", false)]
	AsyncTask CreateWithDelegate(ManagedDelegate function, bool isBackground);

	[EngineMethod("invoke", false)]
	void Invoke(UIntPtr Pointer);

	[EngineMethod("wait", false)]
	void Wait(UIntPtr Pointer);
}
