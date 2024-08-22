using System.Threading.Tasks;

namespace TaleWorlds.Diamond.InnerProcess;

internal class InnerProcessMessageTask
{
	private TaskCompletionSource<bool> _taskCompletionSource;

	public SessionCredentials SessionCredentials { get; private set; }

	public Message Message { get; private set; }

	public InnerProcessMessageTaskType Type { get; private set; }

	public bool Finished { get; private set; }

	public bool Successful { get; private set; }

	public FunctionResult FunctionResult { get; private set; }

	public InnerProcessMessageTask(SessionCredentials sessionCredentials, Message message, InnerProcessMessageTaskType type)
	{
		SessionCredentials = sessionCredentials;
		Message = message;
		Type = type;
		_taskCompletionSource = new TaskCompletionSource<bool>();
	}

	public async Task WaitUntilFinished()
	{
		await _taskCompletionSource.Task;
	}

	public void SetFinishedAsSuccessful(FunctionResult functionResult)
	{
		FunctionResult = functionResult;
		Successful = true;
		Finished = true;
		_taskCompletionSource.SetResult(result: true);
	}

	public void SetFinishedAsFailed()
	{
		Successful = false;
		Finished = true;
		_taskCompletionSource.SetResult(result: true);
	}
}
