namespace TaleWorlds.Diamond.ChatSystem.Library;

public enum ChatClientState
{
	Created,
	Connecting,
	Connected,
	Disconnected,
	WaitingForReconnect,
	Stopped
}
