using System;
using System.Collections.Generic;
using System.Linq;

namespace TaleWorlds.Library;

public static class MBMath
{
	public const float TwoPI = (float)Math.PI * 2f;

	public const float PI = (float)Math.PI;

	public const float HalfPI = (float)Math.PI / 2f;

	public const float E = (float)Math.E;

	public const float DegreesToRadians = (float)Math.PI / 180f;

	public const float RadiansToDegrees = 180f / (float)Math.PI;

	public const float Epsilon = 1E-05f;

	public static float ToRadians(this float f)
	{
		return f * ((float)Math.PI / 180f);
	}

	public static float ToDegrees(this float f)
	{
		return f * (180f / (float)Math.PI);
	}

	public static bool ApproximatelyEqualsTo(this float f, float comparedValue, float epsilon = 1E-05f)
	{
		return Math.Abs(f - comparedValue) <= epsilon;
	}

	public static bool ApproximatelyEquals(float first, float second, float epsilon = 1E-05f)
	{
		return Math.Abs(first - second) <= epsilon;
	}

	public static bool IsValidValue(float f)
	{
		if (!float.IsNaN(f))
		{
			return !float.IsInfinity(f);
		}
		return false;
	}

	public static int ClampIndex(int value, int minValue, int maxValue)
	{
		return ClampInt(value, minValue, maxValue - 1);
	}

	public static int ClampInt(int value, int minValue, int maxValue)
	{
		return Math.Max(Math.Min(value, maxValue), minValue);
	}

	public static float ClampFloat(float value, float minValue, float maxValue)
	{
		return Math.Max(Math.Min(value, maxValue), minValue);
	}

	public static void ClampUnit(ref float value)
	{
		value = ClampFloat(value, 0f, 1f);
	}

	public static int GetNumberOfBitsToRepresentNumber(uint value)
	{
		int num = 0;
		for (uint num2 = value; num2 != 0; num2 >>= 1)
		{
			num++;
		}
		return num;
	}

	public static IEnumerable<(T, int)> DistributeShares<T>(int totalAward, IEnumerable<T> stakeHolders, Func<T, int> shareFunction)
	{
		List<(T, int)> sharesList = new List<(T, int)>(20);
		int num = 0;
		foreach (T stakeHolder in stakeHolders)
		{
			int num2 = shareFunction(stakeHolder);
			sharesList.Add((stakeHolder, num2));
			num += num2;
		}
		if (num <= 0)
		{
			yield break;
		}
		int remainingShares = num;
		int remaingAward = totalAward;
		int i = 0;
		while (i < sharesList.Count && remaingAward > 0)
		{
			int item = sharesList[i].Item2;
			int num3 = MathF.Round((float)remaingAward * (float)item / (float)remainingShares);
			if (num3 > remaingAward)
			{
				num3 = remaingAward;
			}
			remaingAward -= num3;
			remainingShares -= item;
			yield return (sharesList[i].Item1, num3);
			int num4 = i + 1;
			i = num4;
		}
	}

	public static int GetNumberOfBitsToRepresentNumber(ulong value)
	{
		int num = 0;
		for (ulong num2 = value; num2 != 0; num2 >>= 1)
		{
			num++;
		}
		return num;
	}

	public static float Lerp(float valueFrom, float valueTo, float amount, float minimumDifference = 1E-05f)
	{
		if (Math.Abs(valueFrom - valueTo) <= minimumDifference)
		{
			return valueTo;
		}
		return valueFrom + (valueTo - valueFrom) * amount;
	}

	public static float LinearExtrapolation(float valueFrom, float valueTo, float amount)
	{
		return valueFrom + (valueTo - valueFrom) * amount;
	}

	public static Vec3 Lerp(Vec3 vecFrom, Vec3 vecTo, float amount, float minimumDifference)
	{
		return new Vec3(Lerp(vecFrom.x, vecTo.x, amount, minimumDifference), Lerp(vecFrom.y, vecTo.y, amount, minimumDifference), Lerp(vecFrom.z, vecTo.z, amount, minimumDifference));
	}

	public static Vec2 Lerp(Vec2 vecFrom, Vec2 vecTo, float amount, float minimumDifference)
	{
		return new Vec2(Lerp(vecFrom.x, vecTo.x, amount, minimumDifference), Lerp(vecFrom.y, vecTo.y, amount, minimumDifference));
	}

	public static float Map(float input, float inputMinimum, float inputMaximum, float outputMinimum, float outputMaximum)
	{
		input = ClampFloat(input, inputMinimum, inputMaximum);
		return (input - inputMinimum) * (outputMaximum - outputMinimum) / (inputMaximum - inputMinimum) + outputMinimum;
	}

	public static Mat3 Lerp(ref Mat3 matFrom, ref Mat3 matTo, float amount, float minimumDifference)
	{
		return new Mat3(Lerp(matFrom.s, matTo.s, amount, minimumDifference), Lerp(matFrom.f, matTo.f, amount, minimumDifference), Lerp(matFrom.u, matTo.u, amount, minimumDifference));
	}

	public static float LerpRadians(float valueFrom, float valueTo, float amount, float minChange, float maxChange)
	{
		float smallestDifferenceBetweenTwoAngles = GetSmallestDifferenceBetweenTwoAngles(valueFrom, valueTo);
		if (Math.Abs(smallestDifferenceBetweenTwoAngles) <= minChange)
		{
			return valueTo;
		}
		float num = (float)Math.Sign(smallestDifferenceBetweenTwoAngles) * ClampFloat(Math.Abs(smallestDifferenceBetweenTwoAngles * amount), minChange, maxChange);
		return WrapAngle(valueFrom + num);
	}

	public static float SplitLerp(float value1, float value2, float value3, float cutOff, float amount, float minimumDifference)
	{
		if (amount <= cutOff)
		{
			float amount2 = amount / cutOff;
			return Lerp(value1, value2, amount2, minimumDifference);
		}
		float num = 1f - cutOff;
		float amount3 = (amount - cutOff) / num;
		return Lerp(value2, value3, amount3, minimumDifference);
	}

	public static float InverseLerp(float valueFrom, float valueTo, float value)
	{
		return (value - valueFrom) / (valueTo - valueFrom);
	}

	public static float SmoothStep(float edge0, float edge1, float value)
	{
		float num = ClampFloat((value - edge0) / (edge1 - edge0), 0f, 1f);
		return num * num * (3f - 2f * num);
	}

	public static float BilinearLerp(float topLeft, float topRight, float botLeft, float botRight, float x, float y)
	{
		float valueFrom = Lerp(topLeft, topRight, x);
		float valueTo = Lerp(botLeft, botRight, x);
		return Lerp(valueFrom, valueTo, y);
	}

	public static float GetSmallestDifferenceBetweenTwoAngles(float fromAngle, float toAngle)
	{
		float num = toAngle - fromAngle;
		if (num > (float)Math.PI)
		{
			num = (float)Math.PI * -2f + num;
		}
		if (num < -(float)Math.PI)
		{
			num = (float)Math.PI * 2f + num;
		}
		return num;
	}

	public static float ClampAngle(float angle, float restrictionCenter, float restrictionRange)
	{
		restrictionRange /= 2f;
		float smallestDifferenceBetweenTwoAngles = GetSmallestDifferenceBetweenTwoAngles(restrictionCenter, angle);
		if (smallestDifferenceBetweenTwoAngles > restrictionRange)
		{
			angle = restrictionCenter + restrictionRange;
		}
		else if (smallestDifferenceBetweenTwoAngles < 0f - restrictionRange)
		{
			angle = restrictionCenter - restrictionRange;
		}
		if (angle > (float)Math.PI)
		{
			angle -= (float)Math.PI * 2f;
		}
		else if (angle < -(float)Math.PI)
		{
			angle += (float)Math.PI * 2f;
		}
		return angle;
	}

	public static float WrapAngle(float angle)
	{
		angle = (float)Math.IEEERemainder(angle, Math.PI * 2.0);
		if (angle <= -(float)Math.PI)
		{
			angle += (float)Math.PI * 2f;
		}
		else if (angle > (float)Math.PI)
		{
			angle -= (float)Math.PI * 2f;
		}
		return angle;
	}

	public static bool IsBetween(float numberToCheck, float bottom, float top)
	{
		if (numberToCheck > bottom)
		{
			return numberToCheck < top;
		}
		return false;
	}

	public static bool IsBetween(int value, int minValue, int maxValue)
	{
		if (value >= minValue)
		{
			return value < maxValue;
		}
		return false;
	}

	public static bool IsBetweenInclusive(float numberToCheck, float bottom, float top)
	{
		if (numberToCheck >= bottom)
		{
			return numberToCheck <= top;
		}
		return false;
	}

	public static uint ColorFromRGBA(float red, float green, float blue, float alpha)
	{
		return ((uint)(alpha * 255f) << 24) + ((uint)(red * 255f) << 16) + ((uint)(green * 255f) << 8) + (uint)(blue * 255f);
	}

	public static Color HSBtoRGB(float hue, float saturation, float brightness, float outputAlpha)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = brightness * saturation;
		float num5 = num4 * (1f - MathF.Abs(hue * (1f / 60f) % 2f - 1f));
		float num6 = brightness - num4;
		switch ((int)(hue * (1f / 60f) % 6f))
		{
		case 0:
			num = num4;
			num2 = num5;
			num3 = 0f;
			break;
		case 1:
			num = num5;
			num2 = num4;
			num3 = 0f;
			break;
		case 2:
			num = 0f;
			num2 = num4;
			num3 = num5;
			break;
		case 3:
			num = 0f;
			num2 = num5;
			num3 = num4;
			break;
		case 4:
			num = num5;
			num2 = 0f;
			num3 = num4;
			break;
		case 5:
			num = num4;
			num2 = 0f;
			num3 = num5;
			break;
		}
		return new Color(num + num6, num2 + num6, num3 + num6, outputAlpha);
	}

	public static Vec3 RGBtoHSB(Color rgb)
	{
		Vec3 result = new Vec3(0f, 0f, 0f, -1f);
		float num = MathF.Min(MathF.Min(rgb.Red, rgb.Green), rgb.Blue);
		float num2 = MathF.Max(MathF.Max(rgb.Red, rgb.Green), rgb.Blue);
		float num3 = num2 - num;
		result.z = num2;
		if (MathF.Abs(num3) < 0.0001f)
		{
			result.x = 0f;
		}
		else if (MathF.Abs(num2 - rgb.Red) < 0.0001f)
		{
			result.x = 60f * ((rgb.Green - rgb.Blue) / num3 % 6f);
		}
		else if (MathF.Abs(num2 - rgb.Green) < 0.0001f)
		{
			result.x = 60f * ((rgb.Blue - rgb.Red) / num3 + 2f);
		}
		else
		{
			result.x = 60f * ((rgb.Red - rgb.Green) / num3 + 4f);
		}
		result.x %= 360f;
		if (result.x < 0f)
		{
			result.x += 360f;
		}
		if (MathF.Abs(num2) < 0.0001f)
		{
			result.y = 0f;
		}
		else
		{
			result.y = num3 / num2;
		}
		return result;
	}

	public static Vec3 GammaCorrectRGB(float gamma, Vec3 rgb)
	{
		float y = 1f / gamma;
		rgb.x = MathF.Pow(rgb.x, y);
		rgb.y = MathF.Pow(rgb.y, y);
		rgb.z = MathF.Pow(rgb.z, y);
		return rgb;
	}

	public static Vec3 GetClosestPointInLineSegmentToPoint(Vec3 point, Vec3 lineSegmentBegin, Vec3 lineSegmentEnd)
	{
		Vec3 vec = lineSegmentEnd - lineSegmentBegin;
		if (!vec.IsNonZero)
		{
			return lineSegmentBegin;
		}
		float num = Vec3.DotProduct(point - lineSegmentBegin, vec) / Vec3.DotProduct(vec, vec);
		if (num < 0f)
		{
			return lineSegmentBegin;
		}
		if (num > 1f)
		{
			return lineSegmentEnd;
		}
		return lineSegmentBegin + vec * num;
	}

	public static bool GetRayPlaneIntersectionPoint(in Vec3 planeNormal, in Vec3 planeCenter, in Vec3 rayOrigin, in Vec3 rayDirection, out float t)
	{
		float num = Vec3.DotProduct(planeNormal, rayDirection);
		if (num > 1E-06f)
		{
			Vec3 v = planeCenter - rayOrigin;
			t = Vec3.DotProduct(v, planeNormal) / num;
			return t >= 0f;
		}
		t = -1f;
		return false;
	}

	public static Vec2 GetClosestPointInLineSegmentToPoint(Vec2 point, Vec2 lineSegmentBegin, Vec2 lineSegmentEnd)
	{
		Vec2 vec = lineSegmentEnd - lineSegmentBegin;
		if (!vec.IsNonZero())
		{
			return lineSegmentBegin;
		}
		float num = Vec2.DotProduct(point - lineSegmentBegin, vec) / Vec2.DotProduct(vec, vec);
		if (num < 0f)
		{
			return lineSegmentBegin;
		}
		if (num > 1f)
		{
			return lineSegmentEnd;
		}
		return lineSegmentBegin + vec * num;
	}

	public static bool CheckLineToLineSegmentIntersection(Vec2 lineOrigin, Vec2 lineDirection, Vec2 segmentA, Vec2 segmentB, out float t, out Vec2 intersect)
	{
		t = float.MaxValue;
		intersect = Vec2.Zero;
		Vec2 vec = lineOrigin - segmentA;
		Vec2 vec2 = segmentB - segmentA;
		Vec2 v = new Vec2(0f - lineDirection.y, lineDirection.x);
		float num = vec2.DotProduct(v);
		if (MathF.Abs(num) < 1E-05f)
		{
			return false;
		}
		float num2 = vec2.x * vec.y - vec2.y * vec.x;
		t = num2 / num;
		intersect = lineOrigin + lineDirection * t;
		float num3 = vec.DotProduct(v) / num;
		if (num3 >= 0f)
		{
			return num3 <= 1f;
		}
		return false;
	}

	public static float GetClosestPointOnLineSegment(Vec2 point, Vec2 segmentA, Vec2 segmentB, out Vec2 closest)
	{
		Vec2 vec = point - segmentA;
		Vec2 vec2 = segmentB - segmentA;
		float num = vec.DotProduct(vec2) / Math.Max(vec2.LengthSquared, 1E-05f);
		if (num < 0f)
		{
			closest = segmentA;
		}
		else if (num > 1f)
		{
			closest = segmentB;
		}
		else
		{
			closest = segmentA + vec2 * num;
		}
		return point.Distance(closest);
	}

	public static bool IntersectRayWithBoundaryList(Vec2 rayOrigin, Vec2 rayDir, List<Vec2> boundaries, out Vec2 intersectionPoint)
	{
		List<(float, Vec2)> list = new List<(float, Vec2)>();
		for (int j = 0; j < boundaries.Count; j++)
		{
			Vec2 segmentA = boundaries[j];
			Vec2 segmentB = boundaries[(j + 1) % boundaries.Count];
			if (CheckLineToLineSegmentIntersection(rayOrigin, rayDir, segmentA, segmentB, out var t, out var intersect) && t > 0f)
			{
				list.Add((t, intersect));
			}
		}
		list = list.OrderBy(((float, Vec2) i) => i.Item1).ToList();
		if (list.Count != 0)
		{
			intersectionPoint = list[0].Item2;
			return true;
		}
		intersectionPoint = rayOrigin;
		return false;
	}

	public static string ToOrdinal(int number)
	{
		if (number < 0)
		{
			return number.ToString();
		}
		long num = number % 100;
		if (num >= 11 && num <= 13)
		{
			return number + "th";
		}
		return (number % 10) switch
		{
			1 => number + "st", 
			2 => number + "nd", 
			3 => number + "rd", 
			_ => number + "th", 
		};
	}

	public static int IndexOfMax<T>(MBReadOnlyList<T> array, Func<T, int> func)
	{
		int num = int.MinValue;
		int result = -1;
		for (int i = 0; i < array.Count; i++)
		{
			int num2 = func(array[i]);
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static T MaxElement<T>(IEnumerable<T> collection, Func<T, float> func)
	{
		float num = float.MinValue;
		T result = default(T);
		foreach (T item in collection)
		{
			float num2 = func(item);
			if (num2 > num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public static (T, T) MaxElements2<T>(IEnumerable<T> collection, Func<T, float> func)
	{
		float num = float.MinValue;
		float num2 = float.MinValue;
		T val = default(T);
		T item = default(T);
		foreach (T item2 in collection)
		{
			float num3 = func(item2);
			if (num3 > num2)
			{
				if (num3 > num)
				{
					num2 = num;
					item = val;
					num = num3;
					val = item2;
				}
				else
				{
					num2 = num3;
					item = item2;
				}
			}
		}
		return (val, item);
	}

	public static (T, T, T) MaxElements3<T>(IEnumerable<T> collection, Func<T, float> func)
	{
		float num = float.MinValue;
		float num2 = float.MinValue;
		float num3 = float.MinValue;
		T val = default(T);
		T val2 = default(T);
		T item = default(T);
		foreach (T item2 in collection)
		{
			float num4 = func(item2);
			if (!(num4 > num3))
			{
				continue;
			}
			if (num4 > num2)
			{
				num3 = num2;
				item = val2;
				if (num4 > num)
				{
					num2 = num;
					val2 = val;
					num = num4;
					val = item2;
				}
				else
				{
					num2 = num4;
					val2 = item2;
				}
			}
			else
			{
				num3 = num4;
				item = item2;
			}
		}
		return (val, val2, item);
	}

	public static (T, T, T, T) MaxElements4<T>(IEnumerable<T> collection, Func<T, float> func)
	{
		float num = float.MinValue;
		float num2 = float.MinValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		T val = default(T);
		T val2 = default(T);
		T val3 = default(T);
		T item = default(T);
		foreach (T item2 in collection)
		{
			float num5 = func(item2);
			if (!(num5 > num4))
			{
				continue;
			}
			if (num5 > num3)
			{
				num4 = num3;
				item = val3;
				if (num5 > num2)
				{
					num3 = num2;
					val3 = val2;
					if (num5 > num)
					{
						num2 = num;
						val2 = val;
						num = num5;
						val = item2;
					}
					else
					{
						num2 = num5;
						val2 = item2;
					}
				}
				else
				{
					num3 = num5;
					val3 = item2;
				}
			}
			else
			{
				num4 = num5;
				item = item2;
			}
		}
		return (val, val2, val3, item);
	}

	public static (T, T, T, T, T) MaxElements5<T>(IEnumerable<T> collection, Func<T, float> func)
	{
		float num = float.MinValue;
		float num2 = float.MinValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		float num5 = float.MinValue;
		T val = default(T);
		T val2 = default(T);
		T val3 = default(T);
		T val4 = default(T);
		T item = default(T);
		foreach (T item2 in collection)
		{
			float num6 = func(item2);
			if (!(num6 > num5))
			{
				continue;
			}
			if (num6 > num4)
			{
				num5 = num4;
				item = val4;
				if (num6 > num3)
				{
					num4 = num3;
					val4 = val3;
					if (num6 > num2)
					{
						num3 = num2;
						val3 = val2;
						if (num6 > num)
						{
							num2 = num;
							val2 = val;
							num = num6;
							val = item2;
						}
						else
						{
							num2 = num6;
							val2 = item2;
						}
					}
					else
					{
						num3 = num6;
						val3 = item2;
					}
				}
				else
				{
					num4 = num6;
					val4 = item2;
				}
			}
			else
			{
				num5 = num6;
				item = item2;
			}
		}
		return (val, val2, val3, val4, item);
	}

	public static IList<T> TopologySort<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies)
	{
		List<T> list = new List<T>();
		Dictionary<T, bool> visited = new Dictionary<T, bool>();
		foreach (T item in source)
		{
			Visit(item, getDependencies, list, visited);
		}
		return list;
	}

	private static void Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, List<T> sorted, Dictionary<T, bool> visited)
	{
		if (visited.TryGetValue(item, out var _))
		{
			return;
		}
		visited[item] = true;
		IEnumerable<T> enumerable = getDependencies(item);
		if (enumerable != null)
		{
			foreach (T item2 in enumerable)
			{
				Visit(item2, getDependencies, sorted, visited);
			}
		}
		visited[item] = false;
		sorted.Add(item);
	}
}
