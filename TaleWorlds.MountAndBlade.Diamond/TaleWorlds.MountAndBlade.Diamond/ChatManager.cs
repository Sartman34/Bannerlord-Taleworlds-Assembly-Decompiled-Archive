using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Diamond.ChatSystem.Library;
using TaleWorlds.Localization;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Diamond;

public class ChatManager
{
	public class GetChatRoomResult
	{
		public bool Successful { get; private set; }

		public ChatRoomInformationForClient Room { get; private set; }

		public TextObject ErrorMessage { get; private set; }

		public GetChatRoomResult(bool successful, ChatRoomInformationForClient room, TextObject error)
		{
			Successful = successful;
			Room = room;
			ErrorMessage = error;
		}

		public static GetChatRoomResult CreateSuccessful(ChatRoomInformationForClient room)
		{
			return new GetChatRoomResult(successful: true, room, new TextObject());
		}

		public static GetChatRoomResult CreateFailed(TextObject error)
		{
			return new GetChatRoomResult(successful: false, null, error);
		}
	}

	private readonly Dictionary<string, ChatClient> _clients;

	private readonly Dictionary<Guid, ChatRoomInformationForClient> _rooms;

	private readonly IChatClientHandler _chatClientHandler;

	private bool _isCleaningUp;

	public List<ChatRoomInformationForClient> Rooms => _rooms.Values.ToList();

	public ChatManager(IChatClientHandler chatClientHandler)
	{
		_clients = new Dictionary<string, ChatClient>();
		_rooms = new Dictionary<Guid, ChatRoomInformationForClient>();
		_chatClientHandler = chatClientHandler;
	}

	public void OnJoinChatRoom(ChatRoomInformationForClient info, PlayerId playerId, string playerName)
	{
		if (!_rooms.ContainsKey(info.RoomId))
		{
			if (!_clients.ContainsKey(info.Endpoint))
			{
				ChatClient chatClient = new ChatClient(info.Endpoint, playerId.ToString(), playerName, info.RoomId);
				_clients.Add(info.Endpoint, chatClient);
				chatClient.OnMessageReceived += ClientOnMessageReceived;
				chatClient.Connect();
			}
			_rooms.Add(info.RoomId, info);
		}
	}

	public void OnTick()
	{
		if (_isCleaningUp)
		{
			return;
		}
		List<string> list = null;
		foreach (string key in _clients.Keys)
		{
			ChatClient chatClient = _clients[key];
			chatClient.OnTick();
			if (chatClient.State == ChatClientState.Stopped)
			{
				if (list == null)
				{
					list = new List<string>(1);
				}
				list.Add(key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (string item in list)
		{
			ChatClient chatClient2 = _clients[item];
			_rooms.Remove(chatClient2.RoomId);
			_clients.Remove(item);
		}
	}

	public void OnChatRoomClosed(Guid roomId)
	{
		if (_rooms.TryGetValue(roomId, out var room))
		{
			room = _rooms[roomId];
			_rooms.Remove(roomId);
			if (!_rooms.Any((KeyValuePair<Guid, ChatRoomInformationForClient> item) => room.Endpoint == item.Value.Endpoint))
			{
				_clients[room.Endpoint].Stop();
				_clients.Remove(room.Endpoint);
			}
		}
	}

	private void ClientOnMessageReceived(ChatClient client, ChatMessage message)
	{
		if (_rooms.TryGetValue(message.RoomId, out var value))
		{
			_chatClientHandler.OnChatMessageReceived(message.RoomId, value.Name, message.Name, message.Text, value.RoomColor, message.Type);
		}
	}

	public async void SendMessage(Guid roomId, string message)
	{
		if (_rooms.TryGetValue(roomId, out var value))
		{
			string endpoint = value.Endpoint;
			if (_clients.TryGetValue(endpoint, out var value2))
			{
				await value2.Send(new ChatMessage
				{
					RoomId = roomId,
					Text = message
				});
			}
		}
	}

	public async void Cleanup()
	{
		_isCleaningUp = true;
		foreach (KeyValuePair<string, ChatClient> client in _clients)
		{
			await client.Value.Stop();
		}
		_clients.Clear();
		_rooms.Clear();
		_isCleaningUp = false;
	}

	public GetChatRoomResult TryGetChatRoom(string command)
	{
		if (command.StartsWith("/"))
		{
			string value = command.ToLower().Split(new char[1] { '/' }).Last();
			List<ChatRoomInformationForClient> list = new List<ChatRoomInformationForClient>();
			foreach (ChatRoomInformationForClient value2 in _rooms.Values)
			{
				if (value2.Name.ToLower().StartsWith(value))
				{
					list.Add(value2);
				}
			}
			if (list.Count == 1)
			{
				return GetChatRoomResult.CreateSuccessful(list[0]);
			}
			if (list.Count == 0)
			{
				TextObject textObject = new TextObject("{=YOYvBVu1}No chat room found matching {COMMAND}");
				textObject.SetTextVariable("COMMAND", command);
				return GetChatRoomResult.CreateFailed(textObject);
			}
			TextObject textObject2 = new TextObject("{=6doiovtH}Disambiguation: {CHATROOMS}");
			string text = "";
			for (int i = 0; i < list.Count; i++)
			{
				text += list[i];
				if (i != list.Count - 1)
				{
					text += ", ";
				}
			}
			textObject2.SetTextVariable("CHATROOMS", text);
			return GetChatRoomResult.CreateFailed(textObject2);
		}
		return GetChatRoomResult.CreateFailed(new TextObject("{=taPBAd4c}Given command does not start with /"));
	}
}
