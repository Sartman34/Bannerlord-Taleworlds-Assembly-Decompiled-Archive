using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem;

public struct ExplainedNumber
{
	private class StatExplainer
	{
		public enum OperationType
		{
			Base,
			Add,
			Multiply,
			LimitMin,
			LimitMax
		}

		public readonly struct ExplanationLine
		{
			public readonly float Number;

			public readonly string Name;

			public readonly OperationType OperationType;

			public ExplanationLine(string name, float number, OperationType operationType)
			{
				Name = name;
				Number = number;
				OperationType = operationType;
			}
		}

		public List<ExplanationLine> Lines { get; private set; } = new List<ExplanationLine>();


		public ExplanationLine? BaseLine { get; private set; }

		public ExplanationLine? LimitMinLine { get; private set; }

		public ExplanationLine? LimitMaxLine { get; private set; }

		public List<(string name, float number)> GetLines(float baseNumber, float resultNumber)
		{
			List<(string, float)> list = new List<(string, float)>();
			if (BaseLine.HasValue)
			{
				list.Add((BaseLine.Value.Name, BaseLine.Value.Number));
			}
			foreach (ExplanationLine line in Lines)
			{
				float num = line.Number;
				if (line.OperationType == OperationType.Multiply)
				{
					num = baseNumber * num * 0.01f;
				}
				list.Add((line.Name, num));
			}
			if (LimitMinLine.HasValue && LimitMinLine.Value.Number > resultNumber)
			{
				list.Add((LimitMinLine.Value.Name, LimitMinLine.Value.Number));
			}
			if (LimitMaxLine.HasValue && LimitMaxLine.Value.Number < resultNumber)
			{
				list.Add((LimitMaxLine.Value.Name, LimitMaxLine.Value.Number));
			}
			return list;
		}

		public void AddLine(string name, float number, OperationType opType)
		{
			ExplanationLine explanationLine = new ExplanationLine(name, number, opType);
			switch (opType)
			{
			case OperationType.Add:
			case OperationType.Multiply:
			{
				int num = -1;
				for (int i = 0; i < Lines.Count; i++)
				{
					if (Lines[i].Name.Equals(name) && Lines[i].OperationType == opType)
					{
						num = i;
						break;
					}
				}
				if (num < 0)
				{
					Lines.Add(explanationLine);
					break;
				}
				explanationLine = new ExplanationLine(name, number + Lines[num].Number, opType);
				Lines[num] = explanationLine;
				break;
			}
			case OperationType.Base:
				BaseLine = explanationLine;
				break;
			case OperationType.LimitMin:
				LimitMinLine = explanationLine;
				break;
			case OperationType.LimitMax:
				LimitMaxLine = explanationLine;
				break;
			}
		}
	}

	private static readonly TextObject LimitMinText = new TextObject("{=GNalaRaN}Minimum");

	private static readonly TextObject LimitMaxText = new TextObject("{=cfjTtxWv}Maximum");

	private static readonly TextObject BaseText = new TextObject("{=basevalue}Base");

	private float? _limitMinValue;

	private float? _limitMaxValue;

	private StatExplainer _explainer;

	private float _sumOfFactors;

	public float ResultNumber => MathF.Clamp(BaseNumber + BaseNumber * _sumOfFactors, LimitMinValue, LimitMaxValue);

	public float BaseNumber { get; private set; }

	public bool IncludeDescriptions => _explainer != null;

	public float LimitMinValue
	{
		get
		{
			if (!_limitMinValue.HasValue)
			{
				return float.MinValue;
			}
			return _limitMinValue.Value;
		}
	}

	public float LimitMaxValue
	{
		get
		{
			if (!_limitMaxValue.HasValue)
			{
				return float.MaxValue;
			}
			return _limitMaxValue.Value;
		}
	}

	public ExplainedNumber(float baseNumber = 0f, bool includeDescriptions = false, TextObject baseText = null)
	{
		BaseNumber = baseNumber;
		_explainer = (includeDescriptions ? new StatExplainer() : null);
		_sumOfFactors = 0f;
		_limitMinValue = float.MinValue;
		_limitMaxValue = float.MaxValue;
		if (_explainer != null && !BaseNumber.ApproximatelyEqualsTo(0f))
		{
			_explainer.AddLine((baseText ?? BaseText).ToString(), BaseNumber, StatExplainer.OperationType.Base);
		}
	}

	public string GetExplanations()
	{
		if (_explainer == null)
		{
			return "";
		}
		MBStringBuilder mBStringBuilder = default(MBStringBuilder);
		mBStringBuilder.Initialize(16, "GetExplanations");
		foreach (var line in _explainer.GetLines(BaseNumber, ResultNumber))
		{
			string value = string.Format("{0} : {1}{2:0.##}\n", line.name, (line.number > 0.001f) ? "+" : "", line.number);
			mBStringBuilder.Append(value);
		}
		return mBStringBuilder.ToStringAndRelease();
	}

	public List<(string name, float number)> GetLines()
	{
		if (_explainer == null)
		{
			return new List<(string, float)>();
		}
		return _explainer.GetLines(BaseNumber, ResultNumber);
	}

	public void AddFromExplainedNumber(ExplainedNumber explainedNumber, TextObject baseText)
	{
		if (explainedNumber._explainer != null && _explainer != null)
		{
			if (explainedNumber._explainer.BaseLine.HasValue && explainedNumber._explainer.BaseLine.HasValue && !explainedNumber.BaseNumber.ApproximatelyEqualsTo(0f))
			{
				float number = explainedNumber._explainer.BaseLine.Value.Number + explainedNumber._explainer.BaseLine.Value.Number * explainedNumber._sumOfFactors;
				_explainer.AddLine(baseText?.ToString() ?? BaseText.ToString(), number, StatExplainer.OperationType.Add);
			}
			foreach (StatExplainer.ExplanationLine line in explainedNumber._explainer.Lines)
			{
				if (line.OperationType == StatExplainer.OperationType.Add)
				{
					float number2 = line.Number + line.Number * explainedNumber._sumOfFactors;
					_explainer.AddLine(line.Name, number2, line.OperationType);
				}
			}
		}
		BaseNumber += explainedNumber.ResultNumber;
	}

	public void Add(float value, TextObject description = null, TextObject variable = null)
	{
		if (value.ApproximatelyEqualsTo(0f))
		{
			return;
		}
		BaseNumber += value;
		if (description != null && _explainer != null && !value.ApproximatelyEqualsTo(0f))
		{
			if (variable != null)
			{
				description.SetTextVariable("A0", variable);
			}
			_explainer.AddLine(description.ToString(), value, StatExplainer.OperationType.Add);
		}
	}

	public void AddFactor(float value, TextObject description = null)
	{
		if (!value.ApproximatelyEqualsTo(0f))
		{
			_sumOfFactors += value;
			if (description != null && _explainer != null && !value.ApproximatelyEqualsTo(0f))
			{
				_explainer.AddLine(description.ToString(), MathF.Round(value, 3) * 100f, StatExplainer.OperationType.Multiply);
			}
		}
	}

	public void LimitMin(float minValue)
	{
		_limitMinValue = minValue;
		if (_explainer != null)
		{
			_explainer.AddLine(LimitMinText.ToString(), minValue, StatExplainer.OperationType.LimitMin);
		}
	}

	public void LimitMax(float maxValue)
	{
		_limitMaxValue = maxValue;
		if (_explainer != null)
		{
			_explainer.AddLine(LimitMaxText.ToString(), maxValue, StatExplainer.OperationType.LimitMax);
		}
	}

	public void Clamp(float minValue, float maxValue)
	{
		LimitMin(minValue);
		LimitMax(maxValue);
	}
}
