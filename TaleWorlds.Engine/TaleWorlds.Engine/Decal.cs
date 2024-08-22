using System;
using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[EngineClass("rglDecal")]
public sealed class Decal : GameEntityComponent
{
	public bool IsValid => base.Pointer != UIntPtr.Zero;

	public MatrixFrame Frame
	{
		get
		{
			MatrixFrame outFrame = default(MatrixFrame);
			EngineApplicationInterface.IDecal.GetFrame(base.Pointer, ref outFrame);
			return outFrame;
		}
		set
		{
			EngineApplicationInterface.IDecal.SetFrame(base.Pointer, ref value);
		}
	}

	internal Decal(UIntPtr pointer)
		: base(pointer)
	{
	}

	public static Decal CreateDecal(string name = null)
	{
		return EngineApplicationInterface.IDecal.CreateDecal(name);
	}

	public Decal CreateCopy()
	{
		return EngineApplicationInterface.IDecal.CreateCopy(base.Pointer);
	}

	public uint GetFactor1()
	{
		return EngineApplicationInterface.IDecal.GetFactor1(base.Pointer);
	}

	public void SetFactor1Linear(uint linearFactorColor1)
	{
		EngineApplicationInterface.IDecal.SetFactor1Linear(base.Pointer, linearFactorColor1);
	}

	public void SetFactor1(uint factorColor1)
	{
		EngineApplicationInterface.IDecal.SetFactor1(base.Pointer, factorColor1);
	}

	public void SetVectorArgument(float vectorArgument0, float vectorArgument1, float vectorArgument2, float vectorArgument3)
	{
		EngineApplicationInterface.IDecal.SetVectorArgument(base.Pointer, vectorArgument0, vectorArgument1, vectorArgument2, vectorArgument3);
	}

	public void SetVectorArgument2(float vectorArgument0, float vectorArgument1, float vectorArgument2, float vectorArgument3)
	{
		EngineApplicationInterface.IDecal.SetVectorArgument2(base.Pointer, vectorArgument0, vectorArgument1, vectorArgument2, vectorArgument3);
	}

	public Material GetMaterial()
	{
		return EngineApplicationInterface.IDecal.GetMaterial(base.Pointer);
	}

	public void SetMaterial(Material material)
	{
		EngineApplicationInterface.IDecal.SetMaterial(base.Pointer, material.Pointer);
	}

	public void SetFrame(MatrixFrame Frame)
	{
		this.Frame = Frame;
	}
}
