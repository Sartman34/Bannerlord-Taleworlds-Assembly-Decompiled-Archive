using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer;

public class MPChatVM : ViewModel, IChatHandler
{
	private readonly TextObject _hideText = new TextObject("{=ou5KJERr}Press '{KEY}' to hide");

	private readonly TextObject _cycleChannelsText = new TextObject("{=Dhb2N5JD}Press '{KEY}' to cycle through channels");

	private readonly TextObject _sendMessageTextObject = new TextObject("{=f64QfbTO}'{KEY}' to send");

	private readonly TextObject _cancelSendingTextObject = new TextObject("{=U1rHNqOk}'{KEY}' to cancel");

	public const string DefaultCategory = "Default";

	public const string CombatCategory = "Combat";

	public const string SocialCategory = "Social";

	public const string BarkCategory = "Bark";

	private int _maxHistoryCount = 100;

	private const int _spamDetectionInterval = 15;

	private const int _maxMessagesAllowedPerInterval = 5;

	private List<float> _recentlySentMessagesTimes;

	private readonly List<MPChatLineVM> _allMessages;

	private readonly Queue<MPChatLineVM> _requestedMessages;

	private Func<TextObject> _getToggleChatKeyText;

	private Func<TextObject> _getCycleChannelsKeyText;

	private Func<TextObject> _getSendMessageKeyText;

	private Func<TextObject> _getCancelSendingKeyText;

	private ChatBox _chatBox;

	private Game _game;

	private Mission _mission;

	private ChatRoomInformationForClient _currentCustomChatChannel;

	private ChatChannelType _activeChannelType = ChatChannelType.NaN;

	private float _chatBoxSizeX;

	private float _chatBoxSizeY;

	private int _maxMessageLength;

	private string _writtenText = "";

	private string _activeChannelNameText;

	private string _hideShowText;

	private string _toggleCombatLogText;

	private string _toggleBarkText;

	private string _cycleThroughChannelsText;

	private string _sendMessageText;

	private string _cancelSendingText;

	private MBBindingList<MPChatLineVM> _messageHistory;

	private bool _includeCombatLog;

	private bool _includeBark;

	private bool _isTypingText;

	private bool _isInspectingMessages;

	private bool _isChatDisabled;

	private bool _showHideShowHint;

	private bool _isOptionsAvailable;

	private HintViewModel _combatLogHint;

	private Color _activeChannelColor;

	public ChatChannelType ActiveChannelType
	{
		get
		{
			return _activeChannelType;
		}
		set
		{
			if ((value == ChatChannelType.All || value == ChatChannelType.Team) && !GameNetwork.IsClient)
			{
				_activeChannelType = ChatChannelType.NaN;
				IsChatDisabled = true;
			}
			else if (value != _activeChannelType)
			{
				_activeChannelType = value;
				RefreshActiveChannelNameData();
				IsChatDisabled = false;
			}
		}
	}

	private string _playerName
	{
		get
		{
			string text = ((NetworkMain.GameClient.PlayerData != null) ? NetworkMain.GameClient.Name : new TextObject("{=!}ERROR: MISSING PLAYERDATA").ToString());
			MissionPeer obj = GameNetwork.MyPeer?.GetComponent<MissionPeer>();
			if (obj != null && !obj.IsAgentAliveForChatting)
			{
				GameTexts.SetVariable("PLAYER_NAME", "{=!}" + text);
				text = GameTexts.FindText("str_chat_message_dead_player").ToString();
			}
			return text;
		}
	}

	[DataSourceProperty]
	public float ChatBoxSizeX
	{
		get
		{
			return _chatBoxSizeX;
		}
		set
		{
			if (value != _chatBoxSizeX)
			{
				_chatBoxSizeX = value;
				OnPropertyChangedWithValue(value, "ChatBoxSizeX");
			}
		}
	}

	[DataSourceProperty]
	public float ChatBoxSizeY
	{
		get
		{
			return _chatBoxSizeY;
		}
		set
		{
			if (value != _chatBoxSizeY)
			{
				_chatBoxSizeY = value;
				OnPropertyChangedWithValue(value, "ChatBoxSizeY");
			}
		}
	}

	[DataSourceProperty]
	public int MaxMessageLength
	{
		get
		{
			return _maxMessageLength;
		}
		set
		{
			if (value != _maxMessageLength)
			{
				_maxMessageLength = value;
				OnPropertyChangedWithValue(value, "MaxMessageLength");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTypingText
	{
		get
		{
			return _isTypingText;
		}
		set
		{
			if (value != _isTypingText)
			{
				_isTypingText = value;
				OnPropertyChangedWithValue(value, "IsTypingText");
				RefreshVisibility();
			}
		}
	}

	[DataSourceProperty]
	public bool IsInspectingMessages
	{
		get
		{
			return _isInspectingMessages;
		}
		set
		{
			if (value != _isInspectingMessages)
			{
				_isInspectingMessages = value;
				UpdateHideShowText(_isInspectingMessages);
				UpdateShortcutTexts();
				OnPropertyChangedWithValue(value, "IsInspectingMessages");
				RefreshVisibility();
			}
		}
	}

	[DataSourceProperty]
	public bool IsChatDisabled
	{
		get
		{
			return _isChatDisabled;
		}
		set
		{
			if (value != _isChatDisabled)
			{
				_isChatDisabled = value;
				OnPropertyChangedWithValue(value, "IsChatDisabled");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowHideShowHint
	{
		get
		{
			return _showHideShowHint;
		}
		set
		{
			if (value != _showHideShowHint)
			{
				_showHideShowHint = value;
				OnPropertyChangedWithValue(value, "ShowHideShowHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOptionsAvailable
	{
		get
		{
			return _isOptionsAvailable;
		}
		set
		{
			if (value != _isOptionsAvailable)
			{
				_isOptionsAvailable = value;
				OnPropertyChangedWithValue(value, "IsOptionsAvailable");
			}
		}
	}

	[DataSourceProperty]
	public string WrittenText
	{
		get
		{
			return _writtenText;
		}
		set
		{
			if (value != _writtenText)
			{
				_writtenText = value;
				OnPropertyChangedWithValue(value, "WrittenText");
			}
		}
	}

	[DataSourceProperty]
	public Color ActiveChannelColor
	{
		get
		{
			return _activeChannelColor;
		}
		set
		{
			if (value != _activeChannelColor)
			{
				_activeChannelColor = value;
				OnPropertyChangedWithValue(value, "ActiveChannelColor");
			}
		}
	}

	[DataSourceProperty]
	public string ActiveChannelNameText
	{
		get
		{
			return _activeChannelNameText;
		}
		set
		{
			if (value != _activeChannelNameText)
			{
				_activeChannelNameText = value;
				OnPropertyChangedWithValue(value, "ActiveChannelNameText");
			}
		}
	}

	[DataSourceProperty]
	public string HideShowText
	{
		get
		{
			return _hideShowText;
		}
		set
		{
			if (value != _hideShowText)
			{
				_hideShowText = value;
				OnPropertyChangedWithValue(value, "HideShowText");
			}
		}
	}

	[DataSourceProperty]
	public string ToggleCombatLogText
	{
		get
		{
			return _toggleCombatLogText;
		}
		set
		{
			if (value != _toggleCombatLogText)
			{
				_toggleCombatLogText = value;
				OnPropertyChangedWithValue(value, "ToggleCombatLogText");
			}
		}
	}

	[DataSourceProperty]
	public string ToggleBarkText
	{
		get
		{
			return _toggleBarkText;
		}
		set
		{
			if (value != _toggleBarkText)
			{
				_toggleBarkText = value;
				OnPropertyChangedWithValue(value, "ToggleBarkText");
			}
		}
	}

	[DataSourceProperty]
	public string CycleThroughChannelsText
	{
		get
		{
			return _cycleThroughChannelsText;
		}
		set
		{
			if (value != _cycleThroughChannelsText)
			{
				_cycleThroughChannelsText = value;
				OnPropertyChangedWithValue(value, "CycleThroughChannelsText");
			}
		}
	}

	[DataSourceProperty]
	public string SendMessageText
	{
		get
		{
			return _sendMessageText;
		}
		set
		{
			if (value != _sendMessageText)
			{
				_sendMessageText = value;
				OnPropertyChangedWithValue(value, "SendMessageText");
			}
		}
	}

	[DataSourceProperty]
	public string CancelSendingText
	{
		get
		{
			return _cancelSendingText;
		}
		set
		{
			if (value != _cancelSendingText)
			{
				_cancelSendingText = value;
				OnPropertyChangedWithValue(value, "CancelSendingText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPChatLineVM> MessageHistory
	{
		get
		{
			return _messageHistory;
		}
		set
		{
			if (value != _messageHistory)
			{
				_messageHistory = value;
				OnPropertyChangedWithValue(value, "MessageHistory");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CombatLogHint
	{
		get
		{
			return _combatLogHint;
		}
		set
		{
			if (value != _combatLogHint)
			{
				_combatLogHint = value;
				OnPropertyChangedWithValue(value, "CombatLogHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IncludeCombatLog
	{
		get
		{
			return _includeCombatLog;
		}
		set
		{
			if (value != _includeCombatLog)
			{
				_includeCombatLog = value;
				OnPropertyChangedWithValue(value, "IncludeCombatLog");
				ChatHistoryFilterToggled();
				BannerlordConfig.ReportDamage = value;
			}
		}
	}

	[DataSourceProperty]
	public bool IncludeBark
	{
		get
		{
			return _includeBark;
		}
		set
		{
			if (value != _includeBark)
			{
				_includeBark = value;
				OnPropertyChangedWithValue(value, "IncludeBark");
				ChatHistoryFilterToggled();
				BannerlordConfig.ReportBark = value;
			}
		}
	}

	public MPChatVM()
	{
		_allMessages = new List<MPChatLineVM>();
		_requestedMessages = new Queue<MPChatLineVM>();
		MessageHistory = new MBBindingList<MPChatLineVM>();
		CombatLogHint = new HintViewModel();
		IncludeCombatLog = BannerlordConfig.ReportDamage;
		IncludeBark = BannerlordConfig.ReportBark;
		InformationManager.DisplayMessageInternal += OnDisplayMessageReceived;
		InformationManager.ClearAllMessagesInternal += ClearAllMessages;
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnOptionChange));
		MaxMessageLength = 100;
		_recentlySentMessagesTimes = new List<float>();
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		CombatLogHint.HintText = new TextObject("{=FRSGOfUJ}Toggle include Combat Log");
		ToggleCombatLogText = new TextObject("{=rx18kyZb}Combat Log").ToString();
		ToggleBarkText = new TextObject("{=NuMQvQxg}Shouts").ToString();
		UpdateHideShowText(_isInspectingMessages);
		UpdateShortcutTexts();
		RefreshActiveChannelNameData();
	}

	private void RefreshActiveChannelNameData()
	{
		if (ActiveChannelType == ChatChannelType.NaN)
		{
			ActiveChannelNameText = string.Empty;
			ActiveChannelColor = Color.White;
			return;
		}
		if (ActiveChannelType == ChatChannelType.Custom)
		{
			ActiveChannelNameText = "(" + _currentCustomChatChannel.Name + ")";
			ActiveChannelColor = Color.ConvertStringToColor(_currentCustomChatChannel.RoomColor);
			return;
		}
		string content = GameTexts.FindText("str_multiplayer_chat_channel", ActiveChannelType.ToString()).ToString();
		GameTexts.SetVariable("STR", content);
		ActiveChannelNameText = GameTexts.FindText("str_STR_in_parentheses").ToString();
		ActiveChannelColor = GetChannelColor(ActiveChannelType);
	}

	private void OnOptionChange(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
	{
		switch (changedManagedOptionsType)
		{
		case ManagedOptions.ManagedOptionsType.ReportDamage:
			IncludeCombatLog = BannerlordConfig.ReportDamage;
			break;
		case ManagedOptions.ManagedOptionsType.ReportBark:
			IncludeBark = BannerlordConfig.ReportBark;
			break;
		}
	}

	public void ToggleIncludeCombatLog()
	{
		IncludeCombatLog = !IncludeCombatLog;
	}

	public void ExecuteToggleIncludeShouts()
	{
		IncludeBark = !IncludeBark;
	}

	private void UpdateHideShowText(bool isInspecting)
	{
		TextObject textObject = TextObject.Empty;
		if (_game != null && isInspecting)
		{
			textObject = _hideText;
			textObject.SetTextVariable("KEY", _getToggleChatKeyText() ?? TextObject.Empty);
		}
		HideShowText = textObject.ToString();
	}

	private void UpdateShortcutTexts()
	{
		_cycleChannelsText.SetTextVariable("KEY", _getCycleChannelsKeyText?.Invoke() ?? TextObject.Empty);
		CycleThroughChannelsText = _cycleChannelsText.ToString();
		if (TaleWorlds.InputSystem.Input.IsGamepadActive)
		{
			_sendMessageTextObject.SetTextVariable("KEY", _getSendMessageKeyText?.Invoke() ?? TextObject.Empty);
			SendMessageText = _sendMessageTextObject.ToString();
			_cancelSendingTextObject.SetTextVariable("KEY", _getCancelSendingKeyText?.Invoke() ?? TextObject.Empty);
			CancelSendingText = _cancelSendingTextObject.ToString();
		}
		else
		{
			SendMessageText = string.Empty;
			CancelSendingText = string.Empty;
		}
	}

	public void Tick(float dt)
	{
		while (_requestedMessages.Count > 0)
		{
			AddChatLine(_requestedMessages.Dequeue());
		}
		float applicationTime = Time.ApplicationTime;
		for (int i = 0; i < _recentlySentMessagesTimes.Count; i++)
		{
			if (applicationTime - _recentlySentMessagesTimes[i] >= 15f)
			{
				_recentlySentMessagesTimes.RemoveAt(i);
			}
		}
		CheckChatFading(dt);
	}

	public void Clear()
	{
		_allMessages.ForEach(delegate(MPChatLineVM l)
		{
			l.ForceInvisible();
		});
		MessageHistory.ToList().ForEach(delegate(MPChatLineVM l)
		{
			l.ForceInvisible();
		});
		_allMessages.Clear();
		MessageHistory.Clear();
	}

	private void OnDisplayMessageReceived(InformationMessage informationMessage)
	{
		if (IsChatAllowedByOptions())
		{
			HandleAddChatLineRequest(informationMessage);
		}
	}

	private void ClearAllMessages()
	{
		Clear();
	}

	public void UpdateObjects(Game game, Mission mission)
	{
		if (_game != game)
		{
			if (_game != null)
			{
				ClearGame();
			}
			_game = game;
			if (_game != null)
			{
				SetGame();
			}
		}
		if (_mission != mission)
		{
			if (_mission != null)
			{
				ClearMission();
			}
			_mission = mission;
			if (_mission != null)
			{
				SetMission();
			}
		}
		if (_game != null)
		{
			ChatBox gameHandler = _game.GetGameHandler<ChatBox>();
			if (_chatBox != gameHandler)
			{
				if (_chatBox != null)
				{
					ClearChatBox();
				}
				_chatBox = gameHandler;
				if (_chatBox != null)
				{
					SetChatBox();
				}
			}
		}
		IsOptionsAvailable = IsInspectingMessages && IsTypingText;
	}

	private void ClearGame()
	{
		_game = null;
	}

	private void ClearChatBox()
	{
		if (_chatBox != null)
		{
			_chatBox.PlayerMessageReceived -= OnPlayerMessageReceived;
			_chatBox.WhisperMessageSent -= OnWhisperMessageSent;
			_chatBox.WhisperMessageReceived -= OnWhisperMessageReceived;
			_chatBox.ErrorWhisperMessageReceived -= OnErrorWhisperMessageReceived;
			_chatBox.ServerMessage -= OnServerMessage;
			_chatBox.ServerAdminMessage -= OnServerAdminMessage;
			_chatBox = null;
		}
	}

	private void SetGame()
	{
		UpdateHideShowText(IsInspectingMessages);
	}

	private void SetChatBox()
	{
		_chatBox.PlayerMessageReceived += OnPlayerMessageReceived;
		_chatBox.WhisperMessageSent += OnWhisperMessageSent;
		_chatBox.WhisperMessageReceived += OnWhisperMessageReceived;
		_chatBox.ErrorWhisperMessageReceived += OnErrorWhisperMessageReceived;
		_chatBox.ServerMessage += OnServerMessage;
		_chatBox.ServerAdminMessage += OnServerAdminMessage;
	}

	private void SetMission()
	{
		Game current = Game.Current;
		IsChatDisabled = current != null && current.GetGameHandler<ChatBox>()?.IsContentRestricted == true;
	}

	private void ClearMission()
	{
		_mission = null;
		ActiveChannelType = ChatChannelType.NaN;
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		if (_game != null)
		{
			ClearGame();
		}
		if (_mission != null)
		{
			ClearMission();
		}
	}

	private void ExecuteSendMessage()
	{
		string text = WrittenText;
		if (string.IsNullOrEmpty(text))
		{
			WrittenText = string.Empty;
			return;
		}
		if (text.Length > MaxMessageLength)
		{
			text = WrittenText.Substring(0, MaxMessageLength);
		}
		text = Regex.Replace(text.Trim(), "\\s+", " ");
		if (text.StartsWith("/"))
		{
			string[] array = text.Split(new char[1] { ' ' });
			ChatChannelType activeChannelType = ChatChannelType.NaN;
			LobbyClient gameClient = NetworkMain.GameClient;
			if (gameClient != null && gameClient.Connected)
			{
				ChatManager.GetChatRoomResult getChatRoomResult = NetworkMain.GameClient?.ChatManager.TryGetChatRoom(array[0]);
				if (getChatRoomResult.Successful)
				{
					activeChannelType = ChatChannelType.Custom;
					_currentCustomChatChannel = getChatRoomResult.Room;
				}
				else
				{
					switch (array[0].ToLower())
					{
					case "/all":
					case "/a":
						activeChannelType = ChatChannelType.All;
						break;
					case "/team":
					case "/t":
						activeChannelType = ChatChannelType.Team;
						break;
					case "/ab":
						if (Mission.Current != null)
						{
							Mission.Current.GetMissionBehavior<MissionLobbyComponent>()?.RequestAdminMessage(string.Join(" ", array.Skip(1)), isBroadcast: true);
						}
						break;
					case "/ac":
						if (Mission.Current != null)
						{
							Mission.Current.GetMissionBehavior<MissionLobbyComponent>()?.RequestAdminMessage(string.Join(" ", array.Skip(1)), isBroadcast: false);
						}
						break;
					default:
					{
						MPChatLineVM chatLine = new MPChatLineVM(getChatRoomResult.ErrorMessage.ToString(), Color.White, "Social");
						AddChatLine(chatLine);
						break;
					}
					}
				}
			}
			ActiveChannelType = activeChannelType;
		}
		else
		{
			switch (ActiveChannelType)
			{
			case ChatChannelType.All:
			case ChatChannelType.Team:
			case ChatChannelType.Party:
				CheckSpamAndSendMessage(ActiveChannelType, text);
				break;
			case ChatChannelType.Private:
			case ChatChannelType.Custom:
				SendMessageToLobbyChannel(text);
				break;
			default:
				TaleWorlds.Library.Debug.FailedAssert("Player in invalid channel", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Multiplayer\\MPChatVM.cs", "ExecuteSendMessage", 475);
				break;
			}
		}
		WrittenText = "";
	}

	private void CheckSpamAndSendMessage(ChatChannelType channelType, string textToSend)
	{
		if (_recentlySentMessagesTimes.Count >= 5)
		{
			GameTexts.SetVariable("SECONDS", (15f - (Time.ApplicationTime - _recentlySentMessagesTimes[0])).ToString("0.0"));
			AddChatLine(new MPChatLineVM(new TextObject("{=76VR5o8h}You must wait {SECONDS} seconds before sending another message.").ToString(), GetChannelColor(ChatChannelType.System), "Default"));
		}
		else
		{
			_recentlySentMessagesTimes.Add(Time.ApplicationTime);
			SendMessageToChannel(ActiveChannelType, textToSend);
		}
	}

	private void HandleAddChatLineRequest(InformationMessage informationMessage)
	{
		string information = informationMessage.Information;
		string category = (string.IsNullOrEmpty(informationMessage.Category) ? "Default" : informationMessage.Category);
		Color color = informationMessage.Color;
		MPChatLineVM item = new MPChatLineVM(information, color, category);
		_requestedMessages.Enqueue(item);
	}

	public void SendMessageToChannel(ChatChannelType channel, string message)
	{
		LobbyClient gameClient = NetworkMain.GameClient;
		if (gameClient != null && gameClient.Connected)
		{
			switch (channel)
			{
			case ChatChannelType.All:
				_chatBox.SendMessageToAll(message);
				break;
			case ChatChannelType.Team:
				_chatBox.SendMessageToTeam(message);
				break;
			default:
				throw new NotImplementedException();
			}
		}
	}

	public void SendMessageToLobbyChannel(string message)
	{
		LobbyClient gameClient = NetworkMain.GameClient;
		if (gameClient != null && gameClient.Connected)
		{
			NetworkMain.GameClient.SendChannelMessage(_currentCustomChatChannel.RoomId, message);
		}
	}

	void IChatHandler.ReceiveChatMessage(ChatChannelType channel, string sender, string message)
	{
		TextObject textObject = TextObject.Empty;
		if (channel == ChatChannelType.Private)
		{
			textObject = new TextObject("{=6syoutpV}From {WHISPER_TARGET}");
			textObject.SetTextVariable("WHISPER_TARGET", sender);
		}
		AddMessage(message, sender, channel, textObject);
	}

	private void AddMessage(string msg, string author, ChatChannelType type, TextObject customChannelName = null)
	{
		Color channelColor = GetChannelColor(type);
		string text = ((!TextObject.IsNullOrEmpty(customChannelName)) ? customChannelName.ToString() : type.ToString());
		MPChatLineVM chatLine = new MPChatLineVM("(" + text + ") " + author + ": " + msg, channelColor, "Social");
		AddChatLine(chatLine);
	}

	private void AddChatLine(MPChatLineVM chatLine)
	{
		if (NativeConfig.DisableGuiMessages || chatLine == null)
		{
			return;
		}
		_allMessages.Add(chatLine);
		if (_allMessages.Count > _maxHistoryCount * 5)
		{
			_allMessages.RemoveAt(0);
		}
		if (IsMessageIncluded(chatLine))
		{
			MessageHistory.Add(chatLine);
			if (MessageHistory.Count > _maxHistoryCount)
			{
				MessageHistory.RemoveAt(0);
			}
		}
		RefreshVisibility();
	}

	public void CheckChatFading(float dt)
	{
		foreach (MPChatLineVM allMessage in _allMessages)
		{
			allMessage.HandleFading(dt);
		}
	}

	[Conditional("DEBUG")]
	private void CheckFadingOutOrder()
	{
		for (int i = 0; i < _allMessages.Count - 1; i++)
		{
			_ = _allMessages[i];
			_ = _allMessages[i + 1];
		}
	}

	private void ChatHistoryFilterToggled()
	{
		MessageHistory.Clear();
		for (int i = 0; i < _allMessages.Count; i++)
		{
			if (MessageHistory.Count >= _maxHistoryCount)
			{
				break;
			}
			MPChatLineVM mPChatLineVM = _allMessages[i];
			if (IsMessageIncluded(mPChatLineVM))
			{
				MessageHistory.Add(mPChatLineVM);
			}
		}
		RefreshVisibility();
	}

	private bool IsMessageIncluded(MPChatLineVM chatLine)
	{
		if (chatLine.Category == "Combat")
		{
			return IncludeCombatLog;
		}
		if (chatLine.Category == "Bark")
		{
			return IncludeBark;
		}
		return true;
	}

	public void SetGetKeyTextFromKeyIDFunc(Func<TextObject> getToggleChatKeyText)
	{
		_getToggleChatKeyText = getToggleChatKeyText;
	}

	public void SetGetCycleChannelKeyTextFunc(Func<TextObject> getCycleChannelsKeyText)
	{
		_getCycleChannelsKeyText = getCycleChannelsKeyText;
	}

	public void SetGetSendMessageKeyTextFunc(Func<TextObject> getSendMessageKeyText)
	{
		_getSendMessageKeyText = getSendMessageKeyText;
	}

	public void SetGetCancelSendingKeyTextFunc(Func<TextObject> getCancelSendingKeyText)
	{
		_getCancelSendingKeyText = getCancelSendingKeyText;
	}

	private void OnPlayerMessageReceived(NetworkCommunicator player, string message, bool toTeamOnly)
	{
		MissionPeer component = player.GetComponent<MissionPeer>();
		string text = component?.DisplayedName ?? player.UserName;
		if (component != null && !component.IsAgentAliveForChatting)
		{
			GameTexts.SetVariable("PLAYER_NAME", text);
			text = GameTexts.FindText("str_chat_message_dead_player").ToString();
		}
		AddMessage(message, text, (!toTeamOnly) ? ChatChannelType.All : ChatChannelType.Team);
	}

	private void OnWhisperMessageReceived(string fromUserName, string message)
	{
		AddMessage(message, fromUserName, ChatChannelType.Private);
	}

	private void OnErrorWhisperMessageReceived(string toUserName)
	{
		TextObject textObject = new TextObject("{=61isYVW0}Player {USER_NAME} is not found");
		textObject.SetTextVariable("USER_NAME", toUserName);
		MPChatLineVM chatLine = new MPChatLineVM(textObject.ToString(), Color.White, "Social");
		AddChatLine(chatLine);
	}

	private void OnWhisperMessageSent(string message, string whisperTarget)
	{
		AddMessage(message, whisperTarget, ChatChannelType.Private);
	}

	private void OnServerMessage(string message)
	{
		MPChatLineVM chatLine = new MPChatLineVM(message, Color.White, "Social");
		AddChatLine(chatLine);
	}

	private void OnServerAdminMessage(string message)
	{
		MPChatLineVM chatLine = new MPChatLineVM("[Admin]: " + message, Color.ConvertStringToColor("#CC0099FF"), "Social");
		AddChatLine(chatLine);
	}

	private Color GetChannelColor(ChatChannelType type)
	{
		return Color.ConvertStringToColor(type switch
		{
			ChatChannelType.Private => "#8C1ABDFF", 
			ChatChannelType.All => "#EC943EFF", 
			ChatChannelType.Team => "#05C5F7FF", 
			ChatChannelType.Party => "#05C587FF", 
			ChatChannelType.System => "#FF0000FF", 
			ChatChannelType.Custom => "#FF0000FF", 
			_ => "#FFFFFFFF", 
		});
	}

	public bool IsChatAllowedByOptions()
	{
		if (GameNetwork.IsMultiplayer)
		{
			return BannerlordConfig.EnableMultiplayerChatBox;
		}
		if (BannerlordConfig.EnableSingleplayerChatBox)
		{
			if (Mission.Current != null)
			{
				return !BannerlordConfig.HideBattleUI;
			}
			return true;
		}
		return false;
	}

	public void TypeToChannelAll(bool startTyping = false)
	{
		ActiveChannelType = ChatChannelType.All;
		if (startTyping)
		{
			StartTyping();
		}
	}

	public void TypeToChannelTeam(bool startTyping = false)
	{
		ActiveChannelType = ChatChannelType.Team;
		if (startTyping)
		{
			StartTyping();
		}
	}

	public void StartInspectingMessages()
	{
		IsInspectingMessages = true;
		IsTypingText = false;
		WrittenText = "";
	}

	public void StopInspectingMessages()
	{
		IsInspectingMessages = false;
		IsTypingText = false;
		WrittenText = "";
	}

	public void StartTyping()
	{
		IsTypingText = true;
		IsInspectingMessages = true;
	}

	public void StopTyping(bool resetWrittenText = false)
	{
		IsTypingText = false;
		IsInspectingMessages = false;
		if (resetWrittenText)
		{
			WrittenText = "";
		}
	}

	public void SendCurrentlyTypedMessage()
	{
		ExecuteSendMessage();
	}

	private void RefreshVisibility()
	{
		foreach (MPChatLineVM allMessage in _allMessages)
		{
			allMessage.ToggleForceVisible(IsTypingText || IsInspectingMessages);
		}
	}

	public void ExecuteSaveSizes()
	{
		BannerlordConfig.ChatBoxSizeX = ChatBoxSizeX;
		BannerlordConfig.ChatBoxSizeY = ChatBoxSizeY;
		BannerlordConfig.Save();
	}

	public void SetMessageHistoryCapacity(int capacity)
	{
		_maxHistoryCount = capacity;
		MessageHistory?.Clear();
	}
}
