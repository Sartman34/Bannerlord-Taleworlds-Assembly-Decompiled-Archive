using System;
using TaleWorlds.Diamond;

namespace TaleWorlds.MountAndBlade.Diamond;

[Serializable]
public struct PlayerSessionId
{
	private Guid _guid;

	public Guid Guid => _guid;

	public SessionKey SessionKey => new SessionKey(_guid);

	public PlayerSessionId(Guid guid)
	{
		_guid = guid;
	}

	public PlayerSessionId(SessionKey sessionKey)
	{
		_guid = sessionKey.Guid;
	}

	public PlayerSessionId(byte[] b)
	{
		_guid = new Guid(b);
	}

	public PlayerSessionId(string g)
	{
		_guid = new Guid(g);
	}

	public static PlayerSessionId NewGuid()
	{
		return new PlayerSessionId(Guid.NewGuid());
	}

	public override string ToString()
	{
		return _guid.ToString();
	}

	public byte[] ToByteArray()
	{
		return _guid.ToByteArray();
	}

	public static bool operator ==(PlayerSessionId a, PlayerSessionId b)
	{
		return a._guid == b._guid;
	}

	public static bool operator !=(PlayerSessionId a, PlayerSessionId b)
	{
		return a._guid != b._guid;
	}

	public override bool Equals(object o)
	{
		if (o != null && o is PlayerSessionId playerSessionId)
		{
			return _guid.Equals(playerSessionId.Guid);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _guid.GetHashCode();
	}
}
