using System;

namespace TaleWorlds.Diamond.ChatSystem.Library;

public class Base64Helper
{
	public static string Base64UrlEncode(byte[] data)
	{
		return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_')
			.Trim(new char[1] { '=' });
	}

	public static byte[] Base64UrlDecode(string input)
	{
		return Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/').PadRight(4 * ((input.Length + 3) / 4), '='));
	}
}
