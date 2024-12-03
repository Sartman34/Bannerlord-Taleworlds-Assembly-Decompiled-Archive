using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TaleWorlds.Library;

namespace TaleWorlds.Diamond.ChatSystem.Library;

public class ChatClient
{
	public delegate void MessageReceivedDelegate(ChatClient client, ChatMessage message);

	private ConcurrentQueue<ChatMessage> _receivedMessages;

	private string _endpoint;

	private ClientWebSocket _socket;

	private string _userName;

	private string _displayName;

	private long? reconnectTimer;

	private const long ReconnectInterval = 100000000L;

	public ChatClientState State { get; private set; }

	public Guid RoomId { get; private set; }

	public bool IsConnected => State == ChatClientState.Connected;

	public event MessageReceivedDelegate OnMessageReceived;

	public ChatClient(string endpoint, string userName, string displayName, Guid roomId)
	{
		_endpoint = endpoint;
		_receivedMessages = new ConcurrentQueue<ChatMessage>();
		_socket = new ClientWebSocket();
		State = ChatClientState.Created;
		RoomId = roomId;
		_userName = userName;
		_displayName = displayName;
	}

	public async void Connect()
	{
		_ = 1;
		try
		{
			State = ChatClientState.Connecting;
			string text = Base64Helper.Base64UrlEncode(AesHelper.Encrypt(Common.SerializeObject(new UserInfo(_userName, _displayName)), Parameters.Key, Parameters.InitializationVector));
			await _socket.ConnectAsync(new Uri(_endpoint + "?user=" + text), CancellationToken.None);
			State = ChatClientState.Connected;
			await Receive();
		}
		catch (Exception ex)
		{
			Console.Write(ex.Message);
			State = ChatClientState.Disconnected;
		}
	}

	public void OnTick()
	{
		if (State == ChatClientState.Disconnected)
		{
			if (!reconnectTimer.HasValue)
			{
				reconnectTimer = DateTime.Now.Ticks;
			}
			if (reconnectTimer.HasValue && DateTime.Now.Ticks - reconnectTimer.Value > 100000000)
			{
				Connect();
				reconnectTimer = null;
			}
		}
		if (_receivedMessages.TryDequeue(out var result))
		{
			this.OnMessageReceived?.Invoke(this, result);
		}
	}

	public async Task Stop()
	{
		State = ChatClientState.Stopped;
		if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.Aborted)
		{
			await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
		}
	}

	public async Task Send(ChatMessage message)
	{
		if (IsConnected)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)message));
			ArraySegment<byte> buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
			await _socket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	private async Task Receive()
	{
		ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4096]);
		while (true)
		{
			using MemoryStream ms = new MemoryStream();
			WebSocketReceiveResult webSocketReceiveResult;
			do
			{
				webSocketReceiveResult = await _socket.ReceiveAsync(buffer, CancellationToken.None);
				ms.Write(buffer.Array, buffer.Offset, webSocketReceiveResult.Count);
			}
			while (!webSocketReceiveResult.EndOfMessage);
			if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
			{
				await Stop();
				break;
			}
			ms.Seek(0L, SeekOrigin.Begin);
			using StreamReader reader = new StreamReader(ms, Encoding.UTF8);
			ChatMessage[] array = JsonConvert.DeserializeObject<ChatMessage[]>(await reader.ReadToEndAsync());
			foreach (ChatMessage item in array)
			{
				_receivedMessages.Enqueue(item);
			}
		}
	}
}
