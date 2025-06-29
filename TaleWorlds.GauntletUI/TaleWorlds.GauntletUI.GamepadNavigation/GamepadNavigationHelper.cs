using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.GamepadNavigation;

internal static class GamepadNavigationHelper
{
	internal static void GetRelatedLineOfScope(GamepadNavigationScope scope, Vector2 fromPosition, GamepadNavigationTypes movement, out Vector2 lineBegin, out Vector2 lineEnd, out bool isFromWidget)
	{
		Rectangle discoveryRectangle = scope.GetDiscoveryRectangle();
		if (discoveryRectangle.IsPointInside(fromPosition))
		{
			Widget approximatelyClosestWidgetToPosition = scope.GetApproximatelyClosestWidgetToPosition(fromPosition, movement);
			if (approximatelyClosestWidgetToPosition != null)
			{
				isFromWidget = true;
				GetRelatedLineOfWidget(approximatelyClosestWidgetToPosition, movement, out lineBegin, out lineEnd);
				return;
			}
		}
		isFromWidget = false;
		_ = scope.ParentWidget.EventManager.Context.Scale;
		Vector2 vector = new Vector2(discoveryRectangle.X, discoveryRectangle.Y);
		Vector2 vector2 = new Vector2(discoveryRectangle.X2, discoveryRectangle.Y);
		Vector2 vector3 = new Vector2(discoveryRectangle.X2, discoveryRectangle.Y2);
		Vector2 vector4 = new Vector2(discoveryRectangle.X, discoveryRectangle.Y2);
		switch (movement)
		{
		case GamepadNavigationTypes.Up:
			lineBegin = vector4;
			lineEnd = vector3;
			break;
		case GamepadNavigationTypes.Right:
			lineBegin = vector;
			lineEnd = vector4;
			break;
		case GamepadNavigationTypes.Down:
			lineBegin = vector;
			lineEnd = vector2;
			break;
		case GamepadNavigationTypes.Left:
			lineBegin = vector2;
			lineEnd = vector3;
			break;
		default:
			lineBegin = Vector2.Zero;
			lineEnd = Vector2.Zero;
			break;
		}
	}

	internal static void GetRelatedLineOfWidget(Widget widget, GamepadNavigationTypes movement, out Vector2 lineBegin, out Vector2 lineEnd)
	{
		Vector2 globalPosition = widget.GlobalPosition;
		Vector2 vector = new Vector2(widget.GlobalPosition.X + widget.Size.X, widget.GlobalPosition.Y);
		Vector2 vector2 = widget.GlobalPosition + widget.Size;
		Vector2 vector3 = new Vector2(widget.GlobalPosition.X, widget.GlobalPosition.Y + widget.Size.Y);
		switch (movement)
		{
		case GamepadNavigationTypes.Up:
			lineBegin = vector3;
			lineEnd = vector2;
			break;
		case GamepadNavigationTypes.Right:
			lineBegin = globalPosition;
			lineEnd = vector3;
			break;
		case GamepadNavigationTypes.Down:
			lineBegin = globalPosition;
			lineEnd = vector;
			break;
		case GamepadNavigationTypes.Left:
			lineBegin = vector;
			lineEnd = vector2;
			break;
		default:
			lineBegin = Vector2.Zero;
			lineEnd = Vector2.Zero;
			break;
		}
	}

	internal static float GetDistanceToClosestWidgetEdge(Widget widget, Vector2 point, GamepadNavigationTypes movement, out Vector2 closestPointOnEdge)
	{
		Vector2 globalPosition = widget.GlobalPosition;
		Vector2 size = widget.Size;
		Rectangle rectangle = new Rectangle(globalPosition.X, globalPosition.Y, size.X, size.Y);
		switch (movement)
		{
		case GamepadNavigationTypes.Up:
		{
			Vector2 lineBegin4 = new Vector2(rectangle.X, rectangle.Y2);
			Vector2 lineEnd4 = new Vector2(rectangle.X2, rectangle.Y2);
			closestPointOnEdge = GetClosestPointOnLineSegment(lineBegin4, lineEnd4, point);
			return Vector2.Distance(closestPointOnEdge, point);
		}
		case GamepadNavigationTypes.Right:
		{
			Vector2 lineBegin3 = new Vector2(rectangle.X, rectangle.Y);
			Vector2 lineEnd3 = new Vector2(rectangle.X, rectangle.Y2);
			closestPointOnEdge = GetClosestPointOnLineSegment(lineBegin3, lineEnd3, point);
			return Vector2.Distance(closestPointOnEdge, point);
		}
		case GamepadNavigationTypes.Down:
		{
			Vector2 lineBegin2 = new Vector2(rectangle.X, rectangle.Y);
			Vector2 lineEnd2 = new Vector2(rectangle.X2, rectangle.Y);
			closestPointOnEdge = GetClosestPointOnLineSegment(lineBegin2, lineEnd2, point);
			return Vector2.Distance(closestPointOnEdge, point);
		}
		case GamepadNavigationTypes.Left:
		{
			Vector2 lineBegin = new Vector2(rectangle.X2, rectangle.Y);
			Vector2 lineEnd = new Vector2(rectangle.X2, rectangle.Y2);
			closestPointOnEdge = GetClosestPointOnLineSegment(lineBegin, lineEnd, point);
			return Vector2.Distance(closestPointOnEdge, point);
		}
		default:
			closestPointOnEdge = globalPosition + size / 2f;
			return Vector2.Distance(closestPointOnEdge, point);
		}
	}

	internal static float GetDistanceToClosestWidgetEdge(Widget widget, Vector2 point, GamepadNavigationTypes movement)
	{
		Vector2 closestPointOnEdge;
		return GetDistanceToClosestWidgetEdge(widget, point, movement, out closestPointOnEdge);
	}

	internal static Vector2 GetClosestPointOnLineSegment(Vector2 lineBegin, Vector2 lineEnd, Vector2 point)
	{
		Vector2 value = point - lineBegin;
		Vector2 vector = lineEnd - lineBegin;
		float num = vector.LengthSquared();
		float num2 = Vector2.Dot(value, vector) / num;
		if (num2 < 0f)
		{
			return lineBegin;
		}
		if (num2 > 1f)
		{
			return lineEnd;
		}
		return lineBegin + vector * num2;
	}

	internal static GamepadNavigationTypes GetMovementsToReachRectangle(Vector2 fromPosition, Rectangle rect)
	{
		GamepadNavigationTypes gamepadNavigationTypes = GamepadNavigationTypes.None;
		if (fromPosition.X > rect.X + rect.Width)
		{
			gamepadNavigationTypes |= GamepadNavigationTypes.Left;
		}
		else if (fromPosition.X < rect.X)
		{
			gamepadNavigationTypes |= GamepadNavigationTypes.Right;
		}
		if (fromPosition.Y > rect.Y + rect.Height)
		{
			gamepadNavigationTypes |= GamepadNavigationTypes.Up;
		}
		else if (fromPosition.Y < rect.Y)
		{
			gamepadNavigationTypes |= GamepadNavigationTypes.Down;
		}
		return gamepadNavigationTypes;
	}

	internal static Vector2 GetMovementVectorForNavigation(GamepadNavigationTypes navigationMovement)
	{
		Vector2 value = default(Vector2);
		value.X = navigationMovement switch
		{
			GamepadNavigationTypes.Left => -1, 
			GamepadNavigationTypes.Right => 1, 
			_ => 0, 
		};
		value.Y = navigationMovement switch
		{
			GamepadNavigationTypes.Down => 1, 
			GamepadNavigationTypes.Up => -1, 
			_ => 0, 
		};
		return Vector2.Normalize(value);
	}

	internal static GamepadNavigationScope GetClosestChildScopeAtDirection(GamepadNavigationScope parentScope, Vector2 fromPosition, GamepadNavigationTypes movement, bool checkForAutoGain, out float distanceToScope)
	{
		return GetClosestScopeAtDirectionFromList(parentScope.ChildScopes.ToList(), fromPosition, movement, checkForAutoGain, true, out distanceToScope);
	}

	internal static GamepadNavigationScope GetClosestScopeAtDirectionFromList(List<GamepadNavigationScope> scopesList, GamepadNavigationScope fromScope, Vector2 fromPosition, GamepadNavigationTypes movement, bool checkForAutoGain, out float distanceToScope)
	{
		distanceToScope = -1f;
		if (fromScope != null)
		{
			Widget lastNavigatedWidget = fromScope.LastNavigatedWidget;
			Rectangle rectangle = (fromScope.UseDiscoveryAreaAsScopeEdges ? fromScope.GetDiscoveryRectangle() : fromScope.GetRectangle());
			if (fromScope.NavigateFromScopeEdges || !rectangle.IsPointInside(fromPosition))
			{
				if (lastNavigatedWidget != null)
				{
					fromPosition = lastNavigatedWidget.GlobalPosition + lastNavigatedWidget.Size / 2f;
				}
				switch (movement)
				{
				case GamepadNavigationTypes.Up:
					fromPosition.Y = rectangle.Y;
					break;
				case GamepadNavigationTypes.Right:
					fromPosition.X = rectangle.X2;
					break;
				case GamepadNavigationTypes.Down:
					fromPosition.Y = rectangle.Y2;
					break;
				case GamepadNavigationTypes.Left:
					fromPosition.X = rectangle.X;
					break;
				}
			}
		}
		return GetClosestScopeAtDirectionFromList(scopesList, fromPosition, movement, checkForAutoGain, false, out distanceToScope, fromScope);
	}

	internal static GamepadNavigationScope GetClosestScopeFromList(List<GamepadNavigationScope> scopeList, Vector2 fromPosition, bool checkForAutoGain)
	{
		float num = float.MaxValue;
		int num2 = -1;
		if (scopeList.Count > 0)
		{
			GamepadNavigationTypes[] array = new GamepadNavigationTypes[4]
			{
				GamepadNavigationTypes.Up,
				GamepadNavigationTypes.Right,
				GamepadNavigationTypes.Down,
				GamepadNavigationTypes.Left
			};
			for (int i = 0; i < scopeList.Count; i++)
			{
				if ((checkForAutoGain && scopeList[i].DoNotAutoGainNavigationOnInit) || !scopeList[i].IsAvailable())
				{
					continue;
				}
				if (scopeList[i].GetRectangle().IsPointInside(fromPosition))
				{
					num2 = i;
					break;
				}
				GamepadNavigationTypes movementsToReachMyPosition = scopeList[i].GetMovementsToReachMyPosition(fromPosition);
				foreach (GamepadNavigationTypes gamepadNavigationTypes in array)
				{
					if (movementsToReachMyPosition.HasAnyFlag(gamepadNavigationTypes))
					{
						Vector2 movementVectorForNavigation = GetMovementVectorForNavigation(gamepadNavigationTypes);
						GetRelatedLineOfScope(scopeList[i], fromPosition, gamepadNavigationTypes, out var lineBegin, out var lineEnd, out var isFromWidget);
						Vector2 closestPointOnLineSegment = GetClosestPointOnLineSegment(lineBegin, lineEnd, fromPosition);
						Vector2 value = Vector2.Normalize(closestPointOnLineSegment - fromPosition);
						float num3 = (isFromWidget ? 1f : Vector2.Dot(movementVectorForNavigation, value));
						float num4 = Vector2.Distance(closestPointOnLineSegment, fromPosition) / num3;
						if (num3 > 0.2f && num4 < num)
						{
							num = num4;
							num2 = i;
						}
					}
				}
			}
			if (num2 != -1)
			{
				return scopeList[num2];
			}
		}
		return null;
	}

	internal static GamepadNavigationScope GetClosestScopeAtDirectionFromList(List<GamepadNavigationScope> scopesList, Vector2 fromPosition, GamepadNavigationTypes movement, bool checkForAutoGain, bool checkOnlyOneDirection, out float distanceToScope, params GamepadNavigationScope[] scopesToIgnore)
	{
		distanceToScope = -1f;
		if (scopesList == null || scopesList.Count == 0)
		{
			return null;
		}
		scopesList = scopesList.ToList();
		for (int i = 0; i < scopesToIgnore.Length; i++)
		{
			scopesList.Remove(scopesToIgnore[i]);
			if (scopesToIgnore[i].ParentScope != null)
			{
				scopesList.Remove(scopesToIgnore[i].ParentScope);
			}
		}
		Vector2 movementVectorForNavigation = GetMovementVectorForNavigation(movement);
		Vec2 resolution = Input.Resolution;
		float num = (((movement & GamepadNavigationTypes.Vertical) != 0) ? (resolution.Y * 0.85f) : (((movement & GamepadNavigationTypes.Horizontal) != 0) ? (resolution.X * 0.85f) : 0f));
		float num2 = float.MaxValue;
		int num3 = -1;
		if (scopesList != null && scopesList.Count > 0)
		{
			for (int j = 0; j < scopesList.Count; j++)
			{
				if ((checkForAutoGain && scopesList[j].DoNotAutoGainNavigationOnInit) || !scopesList[j].IsAvailable())
				{
					continue;
				}
				GetRelatedLineOfScope(scopesList[j], fromPosition, movement, out var lineBegin, out var lineEnd, out var isFromWidget);
				Vector2 closestPointOnLineSegment = GetClosestPointOnLineSegment(lineBegin, lineEnd, fromPosition);
				Vector2 value = Vector2.Normalize(closestPointOnLineSegment - fromPosition);
				float num4 = (isFromWidget ? 1f : Vector2.Dot(movementVectorForNavigation, value));
				if (num4 > 0.2f)
				{
					float num5 = ((!checkOnlyOneDirection) ? (Vector2.Distance(closestPointOnLineSegment, fromPosition) / num4) : GetDirectionalDistanceBetweenTwoPoints(movement, fromPosition, closestPointOnLineSegment));
					if (num5 < num2 && num5 < num)
					{
						num2 = num5;
						distanceToScope = num5;
						num3 = j;
					}
				}
			}
			if (num3 != -1)
			{
				return scopesList[num3];
			}
		}
		return null;
	}

	internal static float GetDirectionalDistanceBetweenTwoPoints(GamepadNavigationTypes movement, Vector2 p1, Vector2 p2)
	{
		switch (movement)
		{
		case GamepadNavigationTypes.Left:
		case GamepadNavigationTypes.Right:
			return MathF.Abs(p1.X - p2.X);
		case GamepadNavigationTypes.Up:
		case GamepadNavigationTypes.Down:
			return MathF.Abs(p1.Y - p2.Y);
		default:
			Debug.FailedAssert("Invalid gamepad movement type:" + movement, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI\\GamepadNavigation\\GamepadNavigationHelper.cs", "GetDirectionalDistanceBetweenTwoPoints", 411);
			return 0f;
		}
	}
}
