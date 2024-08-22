namespace TaleWorlds.Diamond;

public class HandlerResult
{
	public bool IsSuccessful { get; }

	public string Error { get; }

	protected HandlerResult(bool isSuccessful, string error = null)
	{
		IsSuccessful = isSuccessful;
		Error = error;
	}

	public static HandlerResult CreateSuccessful()
	{
		return new HandlerResult(isSuccessful: true);
	}

	public static HandlerResult CreateFailed(string error)
	{
		return new HandlerResult(isSuccessful: false, error);
	}
}
