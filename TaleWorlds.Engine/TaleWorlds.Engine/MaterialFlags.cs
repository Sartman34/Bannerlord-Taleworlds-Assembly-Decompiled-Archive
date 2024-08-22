using System;

namespace TaleWorlds.Engine;

[Flags]
public enum MaterialFlags : uint
{
	RenderFrontToBack = 1u,
	NoDepthTest = 2u,
	DontDrawToDepthRenderTarget = 4u,
	NoModifyDepthBuffer = 8u,
	CullFrontFaces = 0x10u,
	TwoSided = 0x20u,
	AlphaBlendSort = 0x40u,
	DontOptimizeMesh = 0x80u,
	AlphaBlendNone = 0u,
	AlphaBlendModulate = 0x100u,
	AlphaBlendAdd = 0x200u,
	AlphaBlendMultiply = 0x300u,
	AlphaBlendFactor = 0x700u,
	AlphaBlendMask = 0x700u,
	AlphaBlendBits = 8u,
	BillboardNone = 0u,
	Billboard2d = 0x1000u,
	Billboard3d = 0x2000u,
	BillboardMask = 0x3000u,
	Skybox = 0x20000u,
	MultiPassAlpha = 0x40000u,
	GbufferAlphaBlend = 0x80000u,
	RequiresForwardRendering = 0x100000u,
	AvoidRecomputationOfNormals = 0x200000u,
	RenderOrderPlus1 = 0x9000000u,
	RenderOrderPlus2 = 0xA000000u,
	RenderOrderPlus3 = 0xB000000u,
	RenderOrderPlus4 = 0xC000000u,
	RenderOrderPlus5 = 0xD000000u,
	RenderOrderPlus6 = 0xE000000u,
	RenderOrderPlus7 = 0xF000000u,
	GreaterDepthNoWrite = 0x10000000u,
	AlwaysDepthTest = 0x20000000u,
	RenderToAmbientOcclusionBuffer = 0x40000000u
}
