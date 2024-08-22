using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

public class OrderOfBattleFormationClassVM : ViewModel
{
	private FormationClass _class;

	private bool _isLockedOfWeightAdjustments;

	public readonly OrderOfBattleFormationItemVM BelongedFormationItem;

	public static Action<OrderOfBattleFormationClassVM> OnWeightAdjustedCallback;

	public static Action<OrderOfBattleFormationClassVM, FormationClass> OnClassChanged;

	public static Func<OrderOfBattleFormationClassVM, bool> CanAdjustWeight;

	public static Func<FormationClass, int> GetTotalCountOfTroopType;

	private readonly TextObject _weightWithTroopCountText = new TextObject("{=s6qslcQY}{PERCENTAGE} ({TROOP_COUNT}/{TOTAL_TROOP_COUNT})");

	private bool _isFormationClassPreset;

	private bool _isAdjustable;

	private bool _isLocked;

	private bool _isUnset;

	private int _weight;

	private int _shownFormationClass;

	private string _weightText;

	private HintViewModel _lockWeightHint;

	private bool _isWeightHighlightActive;

	public FormationClass Class
	{
		get
		{
			return _class;
		}
		set
		{
			if (value != _class)
			{
				if (!_isFormationClassPreset)
				{
					OnClassChanged?.Invoke(this, value);
				}
				_class = value;
				IsUnset = _class == FormationClass.NumberOfAllFormations;
				ShownFormationClass = (int)((!IsUnset) ? (_class + 1) : FormationClass.Infantry);
				UpdateWeightText();
				_isFormationClassPreset = false;
			}
		}
	}

	public int PreviousWeight { get; private set; }

	[DataSourceProperty]
	public bool IsAdjustable
	{
		get
		{
			return _isAdjustable;
		}
		set
		{
			if (value != _isAdjustable)
			{
				_isAdjustable = value && Mission.Current.PlayerTeam.IsPlayerGeneral;
				OnPropertyChangedWithValue(_isAdjustable, "IsAdjustable");
			}
		}
	}

	[DataSourceProperty]
	public bool IsLocked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			if (value != _isLocked)
			{
				_isLocked = value;
				OnPropertyChangedWithValue(value, "IsLocked");
			}
		}
	}

	[DataSourceProperty]
	public bool IsUnset
	{
		get
		{
			return _isUnset;
		}
		set
		{
			if (value != _isUnset)
			{
				_isUnset = value;
				OnPropertyChangedWithValue(value, "IsUnset");
			}
		}
	}

	[DataSourceProperty]
	public int Weight
	{
		get
		{
			return _weight;
		}
		set
		{
			if (value != _weight)
			{
				PreviousWeight = _weight;
				_weight = value;
				OnPropertyChangedWithValue(value, "Weight");
				OnWeightAdjusted();
			}
		}
	}

	[DataSourceProperty]
	public int ShownFormationClass
	{
		get
		{
			return _shownFormationClass;
		}
		set
		{
			if (value != _shownFormationClass)
			{
				_shownFormationClass = value;
				OnPropertyChangedWithValue(value, "ShownFormationClass");
			}
		}
	}

	[DataSourceProperty]
	public string WeightText
	{
		get
		{
			return _weightText;
		}
		set
		{
			if (value != _weightText)
			{
				_weightText = value;
				OnPropertyChangedWithValue(value, "WeightText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel LockWeightHint
	{
		get
		{
			return _lockWeightHint;
		}
		set
		{
			if (value != _lockWeightHint)
			{
				_lockWeightHint = value;
				OnPropertyChangedWithValue(value, "LockWeightHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsWeightHighlightActive
	{
		get
		{
			return _isWeightHighlightActive;
		}
		set
		{
			if (value != _isWeightHighlightActive)
			{
				_isWeightHighlightActive = value;
				OnPropertyChangedWithValue(value, "IsWeightHighlightActive");
			}
		}
	}

	public OrderOfBattleFormationClassVM(OrderOfBattleFormationItemVM formationItem, FormationClass formationClass = FormationClass.NumberOfAllFormations)
	{
		BelongedFormationItem = formationItem;
		_isFormationClassPreset = formationClass != FormationClass.NumberOfAllFormations;
		Class = formationClass;
		PreviousWeight = 0;
		OnWeightAdjusted();
		RefreshValues();
	}

	public override void RefreshValues()
	{
		LockWeightHint = new HintViewModel(new TextObject("{=mPCrz4rs}Lock troop percentage from relative changes."));
	}

	private void OnWeightAdjusted()
	{
		if (!_isLockedOfWeightAdjustments)
		{
			OnWeightAdjustedCallback?.Invoke(this);
		}
		UpdateWeightText();
	}

	public void UpdateWeightText()
	{
		if (Class != FormationClass.NumberOfAllFormations && GetTotalCountOfTroopType != null)
		{
			GameTexts.SetVariable("NUMBER", Weight);
			GameTexts.SetVariable("PERCENTAGE", GameTexts.FindText("str_NUMBER_percent"));
			GameTexts.SetVariable("TROOP_COUNT", OrderOfBattleUIHelper.GetVisibleCountOfUnitsInClass(this));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalCountOfTroopType(Class));
			WeightText = _weightWithTroopCountText.ToString();
		}
		else
		{
			WeightText = string.Empty;
		}
	}

	public void SetWeightAdjustmentLock(bool isLocked)
	{
		_isLockedOfWeightAdjustments = isLocked;
	}

	public void UpdateWeightAdjustable()
	{
		IsAdjustable = Class != FormationClass.NumberOfAllFormations && (CanAdjustWeight?.Invoke(this) ?? false);
	}
}
