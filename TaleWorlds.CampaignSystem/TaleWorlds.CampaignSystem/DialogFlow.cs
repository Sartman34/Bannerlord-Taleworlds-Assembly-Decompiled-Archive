using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem;

public class DialogFlow
{
	internal readonly List<DialogFlowLine> Lines = new List<DialogFlowLine>();

	internal readonly int Priority;

	private string _currentToken;

	private DialogFlowLine _lastLine;

	private DialogFlowContext _curDialogFlowContext;

	private DialogFlow(string startingToken, int priority = 100)
	{
		_currentToken = startingToken;
		Priority = priority;
	}

	private DialogFlow Line(TextObject text, bool byPlayer, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null, bool isRepeatable = false)
	{
		string text2 = Campaign.Current.ConversationManager.CreateToken();
		AddLine(text, _currentToken, text2, byPlayer, speakerDelegate, listenerDelegate, isRepeatable);
		_currentToken = text2;
		return this;
	}

	public DialogFlow Variation(string text, params object[] propertiesAndWeights)
	{
		return Variation(new TextObject(text), propertiesAndWeights);
	}

	public DialogFlow Variation(TextObject text, params object[] propertiesAndWeights)
	{
		for (int i = 0; i < propertiesAndWeights.Length; i += 2)
		{
			string tagName = (string)propertiesAndWeights[i];
			int weight = Convert.ToInt32(propertiesAndWeights[i + 1]);
			List<GameTextManager.ChoiceTag> list = new List<GameTextManager.ChoiceTag>();
			list.Add(new GameTextManager.ChoiceTag(tagName, weight));
			Lines[Lines.Count - 1].AddVariation(text, list);
		}
		return this;
	}

	public DialogFlow NpcLine(string npcText, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		return NpcLine(new TextObject(npcText), speakerDelegate, listenerDelegate);
	}

	public DialogFlow NpcLine(TextObject npcText, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		return Line(npcText, byPlayer: false, speakerDelegate, listenerDelegate);
	}

	public DialogFlow NpcLineWithVariation(string npcText, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		DialogFlow result = Line(TextObject.Empty, byPlayer: false, speakerDelegate, listenerDelegate);
		List<GameTextManager.ChoiceTag> list = new List<GameTextManager.ChoiceTag>
		{
			new GameTextManager.ChoiceTag("DefaultTag", 1)
		};
		Lines[Lines.Count - 1].AddVariation(new TextObject(npcText), list);
		return result;
	}

	public DialogFlow NpcLineWithVariation(TextObject npcText, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		DialogFlow result = Line(TextObject.Empty, byPlayer: false, speakerDelegate, listenerDelegate);
		List<GameTextManager.ChoiceTag> list = new List<GameTextManager.ChoiceTag>
		{
			new GameTextManager.ChoiceTag("DefaultTag", 1)
		};
		Lines[Lines.Count - 1].AddVariation(npcText, list);
		return result;
	}

	public DialogFlow PlayerLine(string playerText, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		return Line(new TextObject(playerText), byPlayer: true, null, listenerDelegate);
	}

	public DialogFlow PlayerLine(TextObject playerText, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		return Line(playerText, byPlayer: true, null, listenerDelegate);
	}

	private DialogFlow BeginOptions(bool byPlayer)
	{
		_curDialogFlowContext = new DialogFlowContext(_currentToken, byPlayer, _curDialogFlowContext);
		return this;
	}

	public DialogFlow BeginPlayerOptions()
	{
		return BeginOptions(byPlayer: true);
	}

	public DialogFlow BeginNpcOptions()
	{
		return BeginOptions(byPlayer: false);
	}

	private DialogFlow Option(TextObject text, bool byPlayer, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null, bool isRepeatable = false, bool isSpecialOption = false)
	{
		string text2 = Campaign.Current.ConversationManager.CreateToken();
		AddLine(text, _curDialogFlowContext.Token, text2, byPlayer, speakerDelegate, listenerDelegate, isRepeatable, isSpecialOption);
		_currentToken = text2;
		return this;
	}

	public DialogFlow PlayerOption(string text, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		return PlayerOption(new TextObject(text), listenerDelegate);
	}

	public DialogFlow PlayerOption(TextObject text, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		Option(text, byPlayer: true, null, listenerDelegate);
		return this;
	}

	public DialogFlow PlayerSpecialOption(TextObject text, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		Option(text, byPlayer: true, null, listenerDelegate, isRepeatable: false, isSpecialOption: true);
		return this;
	}

	public DialogFlow PlayerRepeatableOption(TextObject text, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		Option(text, byPlayer: true, null, listenerDelegate, isRepeatable: true);
		return this;
	}

	public DialogFlow NpcOption(string text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		Option(new TextObject(text), byPlayer: false, speakerDelegate, listenerDelegate);
		_lastLine.ConditionDelegate = conditionDelegate;
		return this;
	}

	public DialogFlow NpcOption(TextObject text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		Option(text, byPlayer: false, speakerDelegate, listenerDelegate);
		_lastLine.ConditionDelegate = conditionDelegate;
		return this;
	}

	public DialogFlow NpcOptionWithVariation(string text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		NpcOptionWithVariation(new TextObject(text), conditionDelegate, speakerDelegate, listenerDelegate);
		return this;
	}

	public DialogFlow NpcOptionWithVariation(TextObject text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		Option(TextObject.Empty, byPlayer: false, speakerDelegate, listenerDelegate);
		List<GameTextManager.ChoiceTag> list = new List<GameTextManager.ChoiceTag>();
		list.Add(new GameTextManager.ChoiceTag("DefaultTag", 1));
		_lastLine.AddVariation(text, list);
		_lastLine.ConditionDelegate = conditionDelegate;
		return this;
	}

	private DialogFlow EndOptions(bool byPlayer)
	{
		_curDialogFlowContext = _curDialogFlowContext.Parent;
		return this;
	}

	public DialogFlow EndPlayerOptions()
	{
		return EndOptions(byPlayer: true);
	}

	public DialogFlow EndNpcOptions()
	{
		return EndOptions(byPlayer: false);
	}

	public DialogFlow Condition(ConversationSentence.OnConditionDelegate conditionDelegate)
	{
		_lastLine.ConditionDelegate = conditionDelegate;
		return this;
	}

	public DialogFlow ClickableCondition(ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate)
	{
		_lastLine.ClickableConditionDelegate = clickableConditionDelegate;
		return this;
	}

	public DialogFlow Consequence(ConversationSentence.OnConsequenceDelegate consequenceDelegate)
	{
		_lastLine.ConsequenceDelegate = consequenceDelegate;
		return this;
	}

	public static DialogFlow CreateDialogFlow(string inputToken = null, int priority = 100)
	{
		return new DialogFlow(inputToken ?? Campaign.Current.ConversationManager.CreateToken(), priority);
	}

	private DialogFlowLine AddLine(TextObject text, string inputToken, string outputToken, bool byPlayer, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate, bool isRepeatable, bool isSpecialOption = false)
	{
		DialogFlowLine dialogFlowLine = new DialogFlowLine();
		dialogFlowLine.Text = text;
		dialogFlowLine.InputToken = inputToken;
		dialogFlowLine.OutputToken = outputToken;
		dialogFlowLine.ByPlayer = byPlayer;
		dialogFlowLine.SpeakerDelegate = speakerDelegate;
		dialogFlowLine.ListenerDelegate = listenerDelegate;
		dialogFlowLine.IsRepeatable = isRepeatable;
		dialogFlowLine.IsSpecialOption = isSpecialOption;
		Lines.Add(dialogFlowLine);
		_lastLine = dialogFlowLine;
		return dialogFlowLine;
	}

	public DialogFlow NpcDefaultOption(string text)
	{
		return NpcOption(text, null);
	}

	public DialogFlow GotoDialogState(string input)
	{
		_lastLine.OutputToken = input;
		_currentToken = input;
		return this;
	}

	public DialogFlow GetOutputToken(out string oState)
	{
		oState = _lastLine.OutputToken;
		return this;
	}

	public DialogFlow GoBackToDialogState(string iState)
	{
		_currentToken = iState;
		return this;
	}

	public DialogFlow CloseDialog()
	{
		GotoDialogState("close_window");
		return this;
	}

	private ConversationSentence AddDialogLine(ConversationSentence dialogLine)
	{
		Campaign.Current.ConversationManager.AddDialogLine(dialogLine);
		return dialogLine;
	}

	public ConversationSentence AddPlayerLine(string id, string inputToken, string outputToken, string text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, object relatedObject, int priority = 100, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = null, ConversationSentence.OnPersuasionOptionDelegate persuasionOptionDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		return AddDialogLine(new ConversationSentence(id, new TextObject(text), inputToken, outputToken, conditionDelegate, clickableConditionDelegate, consequenceDelegate, 1u, priority, 0, 0, relatedObject, withVariation: false, speakerDelegate, listenerDelegate, persuasionOptionDelegate));
	}

	public ConversationSentence AddDialogLine(string id, string inputToken, string outputToken, string text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, object relatedObject, int priority = 100, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null)
	{
		return AddDialogLine(new ConversationSentence(id, new TextObject(text), inputToken, outputToken, conditionDelegate, clickableConditionDelegate, consequenceDelegate, 0u, priority, 0, 0, relatedObject, withVariation: false, speakerDelegate, listenerDelegate));
	}
}
