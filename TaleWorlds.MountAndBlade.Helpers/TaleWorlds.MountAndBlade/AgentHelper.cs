using System;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

internal static class AgentHelper
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static Vec3 GetAgentPosition(UIntPtr agentPositionPointer)
	{
		Vec3* ptr = (Vec3*)agentPositionPointer.ToPointer();
		return *ptr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static void SetAgentPosition(UIntPtr agentPositionPointer, ref Vec3 newPos)
	{
		Debug.FailedAssert("Do not use this!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Helpers\\Helper.cs", "SetAgentPosition", 20);
		Vec3* ptr = (Vec3*)agentPositionPointer.ToPointer();
		*ptr = newPos;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static int GetAgentIndex(UIntPtr indexPtr)
	{
		int* ptr = (int*)indexPtr.ToPointer();
		return *ptr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static AgentFlag GetAgentFlags(UIntPtr flagsPtr)
	{
		AgentFlag* ptr = (AgentFlag*)flagsPtr.ToPointer();
		return *ptr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static AgentState GetAgentState(UIntPtr statePtr)
	{
		AgentState* ptr = (AgentState*)statePtr.ToPointer();
		return *ptr;
	}
}
