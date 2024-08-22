using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public struct SpawnPathData
{
	public static readonly SpawnPathData Invalid = new SpawnPathData(null, SpawnPathOrientation.PathCenter, 0f, isInverted: false);

	public readonly Path Path;

	public readonly bool IsInverted;

	public readonly float CenterRatio;

	public readonly SpawnPathOrientation Orientation;

	public bool IsValid
	{
		get
		{
			if (Path != null)
			{
				return Path.NumberOfPoints > 1;
			}
			return false;
		}
	}

	public SpawnPathData(Path path = null, SpawnPathOrientation orientation = SpawnPathOrientation.PathCenter, float centerRatio = 0f, bool isInverted = false)
	{
		Path = path;
		Orientation = orientation;
		CenterRatio = TaleWorlds.Library.MathF.Clamp(centerRatio, 0f, 1f);
		IsInverted = isInverted;
	}

	public SpawnPathData Invert()
	{
		return new SpawnPathData(Path, Orientation, TaleWorlds.Library.MathF.Max(1f - CenterRatio, 0f), !IsInverted);
	}

	public MatrixFrame GetSpawnFrame(float offset = 0f)
	{
		MatrixFrame result = MatrixFrame.Identity;
		if (IsValid)
		{
			float num = TaleWorlds.Library.MathF.Clamp(CenterRatio + offset, 0f, 1f);
			num = (IsInverted ? (1f - num) : num);
			float distance = Path.GetTotalLength() * num;
			bool searchForward = (IsInverted ? true : false);
			result = Path.GetNearestFrameWithValidAlphaForDistance(distance, searchForward);
			result.rotation.f = (IsInverted ? (-result.rotation.f) : result.rotation.f);
			result.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		}
		return result;
	}

	public void GetOrientedSpawnPathPosition(out Vec2 spawnPathPosition, out Vec2 spawnPathDirection, float pathOffset = 0f)
	{
		if (IsValid)
		{
			spawnPathPosition = GetSpawnFrame(pathOffset).origin.AsVec2;
			if (Orientation == SpawnPathOrientation.PathCenter)
			{
				Vec2 asVec = Invert().GetSpawnFrame(pathOffset).origin.AsVec2;
				spawnPathDirection = (asVec - spawnPathPosition).Normalized();
			}
			else
			{
				float offset = ((pathOffset >= 0f) ? 1f : 0f) * TaleWorlds.Library.MathF.Max(0.01f, Math.Abs(pathOffset));
				Vec2 asVec2 = GetSpawnFrame(offset).origin.AsVec2;
				spawnPathDirection = (asVec2 - spawnPathPosition).Normalized();
			}
		}
		else
		{
			spawnPathPosition = Vec2.Invalid;
			spawnPathDirection = Vec2.Invalid;
		}
	}
}
