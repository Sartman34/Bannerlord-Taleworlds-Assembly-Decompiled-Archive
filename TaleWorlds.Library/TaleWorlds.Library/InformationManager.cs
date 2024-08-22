using System;
using System.Collections.Generic;

namespace TaleWorlds.Library;

public static class InformationManager
{
	public static Func<bool> IsAnyInquiryActive;

	private static Dictionary<Type, (Type tooltipType, object onRefreshData, string movieName)> _registeredTypes = new Dictionary<Type, (Type, object, string)>();

	private static List<Func<bool>> _isAnyTooltipActiveCallbacks = new List<Func<bool>>();

	private static List<Func<bool>> _isAnyTooltipExtendedCallbacks = new List<Func<bool>>();

	public static IReadOnlyDictionary<Type, (Type tooltipType, object onRefreshData, string movieName)> RegisteredTypes => _registeredTypes;

	public static event Action<InformationMessage> DisplayMessageInternal;

	public static event Action ClearAllMessagesInternal;

	public static event Action<string> OnAddSystemNotification;

	public static event Action<Type, object[]> OnShowTooltip;

	public static event Action OnHideTooltip;

	public static event Action<InquiryData, bool, bool> OnShowInquiry;

	public static event Action<TextInquiryData, bool, bool> OnShowTextInquiry;

	public static event Action OnHideInquiry;

	public static void DisplayMessage(InformationMessage message)
	{
		InformationManager.DisplayMessageInternal?.Invoke(message);
	}

	public static void ClearAllMessages()
	{
		InformationManager.ClearAllMessagesInternal?.Invoke();
	}

	public static void AddSystemNotification(string message)
	{
		InformationManager.OnAddSystemNotification?.Invoke(message);
	}

	public static void ShowTooltip(Type type, params object[] args)
	{
		InformationManager.OnShowTooltip?.Invoke(type, args);
	}

	public static void HideTooltip()
	{
		InformationManager.OnHideTooltip?.Invoke();
	}

	public static void ShowInquiry(InquiryData data, bool pauseGameActiveState = false, bool prioritize = false)
	{
		InformationManager.OnShowInquiry?.Invoke(data, pauseGameActiveState, prioritize);
	}

	public static void ShowTextInquiry(TextInquiryData textData, bool pauseGameActiveState = false, bool prioritize = false)
	{
		InformationManager.OnShowTextInquiry?.Invoke(textData, pauseGameActiveState, prioritize);
	}

	public static void HideInquiry()
	{
		InformationManager.OnHideInquiry?.Invoke();
	}

	public static void RegisterIsAnyTooltipActiveCallback(Func<bool> callback)
	{
		_isAnyTooltipActiveCallbacks.Add(callback);
	}

	public static void UnregisterIsAnyTooltipActiveCallback(Func<bool> callback)
	{
		_isAnyTooltipActiveCallbacks.Remove(callback);
	}

	public static void RegisterIsAnyTooltipExtendedCallback(Func<bool> callback)
	{
		_isAnyTooltipExtendedCallbacks.Add(callback);
	}

	public static void UnregisterIsAnyTooltipExtendedCallback(Func<bool> callback)
	{
		_isAnyTooltipExtendedCallbacks.Remove(callback);
	}

	public static bool GetIsAnyTooltipActive()
	{
		for (int i = 0; i < _isAnyTooltipActiveCallbacks.Count; i++)
		{
			if (_isAnyTooltipActiveCallbacks[i]())
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetIsAnyTooltipExtended()
	{
		for (int i = 0; i < _isAnyTooltipExtendedCallbacks.Count; i++)
		{
			if (_isAnyTooltipExtendedCallbacks[i]())
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetIsAnyTooltipActiveAndExtended()
	{
		if (GetIsAnyTooltipActive())
		{
			return GetIsAnyTooltipExtended();
		}
		return false;
	}

	public static void RegisterTooltip<TRegistered, TTooltip>(Action<TTooltip, object[]> onRefreshData, string movieName) where TTooltip : TooltipBaseVM
	{
		Type typeFromHandle = typeof(TRegistered);
		Type typeFromHandle2 = typeof(TTooltip);
		_registeredTypes[typeFromHandle] = (typeFromHandle2, onRefreshData, movieName);
	}

	public static void UnregisterTooltip<TRegistered>()
	{
		Type typeFromHandle = typeof(TRegistered);
		if (_registeredTypes.ContainsKey(typeFromHandle))
		{
			_registeredTypes.Remove(typeFromHandle);
		}
	}

	public static void Clear()
	{
		InformationManager.DisplayMessageInternal = null;
		InformationManager.OnShowInquiry = null;
		InformationManager.OnShowTextInquiry = null;
		InformationManager.OnHideInquiry = null;
		IsAnyInquiryActive = null;
		InformationManager.OnShowTooltip = null;
		InformationManager.OnHideTooltip = null;
	}
}
