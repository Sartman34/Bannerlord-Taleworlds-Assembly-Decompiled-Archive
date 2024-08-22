using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Conversation;

public class ConversationAnswersContainerWidget : Widget
{
	private Widget _answerContainerWidget;

	[Editor(false)]
	public Widget AnswerContainerWidget
	{
		get
		{
			return _answerContainerWidget;
		}
		set
		{
			if (value != _answerContainerWidget)
			{
				_answerContainerWidget = value;
				OnPropertyChanged(value, "AnswerContainerWidget");
			}
		}
	}

	public ConversationAnswersContainerWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		UpdateHeight();
		base.OnLateUpdate(dt);
		UpdateHeight();
	}

	protected override void OnUpdate(float dt)
	{
		UpdateHeight();
		base.OnUpdate(dt);
		UpdateHeight();
	}

	private void UpdateHeight()
	{
		if (AnswerContainerWidget.Size.Y >= base.Size.Y)
		{
			base.HeightSizePolicy = SizePolicy.Fixed;
			base.ScaledSuggestedHeight = AnswerContainerWidget.Size.Y;
		}
		else
		{
			base.HeightSizePolicy = SizePolicy.CoverChildren;
		}
	}
}
