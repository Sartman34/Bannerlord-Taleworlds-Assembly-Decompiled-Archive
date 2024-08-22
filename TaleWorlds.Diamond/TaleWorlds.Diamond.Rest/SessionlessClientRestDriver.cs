using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TaleWorlds.Library.Http;

namespace TaleWorlds.Diamond.Rest;

public class SessionlessClientRestDriver : ISessionlessClientDriver
{
	private readonly string _address;

	private readonly ushort _port;

	private RestDataJsonConverter _restDataJsonConverter;

	private bool _isSecure;

	private IHttpDriver _platformNetworkClient;

	public SessionlessClientRestDriver(string address, ushort port, bool isSecure, IHttpDriver platformNetworkClient)
	{
		_isSecure = isSecure;
		_platformNetworkClient = platformNetworkClient;
		_address = address;
		_port = port;
		_restDataJsonConverter = new RestDataJsonConverter();
	}

	private void AssignRequestJob(SessionlessClientRestSessionTask requestMessageTask)
	{
		requestMessageTask.SetRequestData(_address, _port, _isSecure, _platformNetworkClient);
	}

	private void TickTask(SessionlessClientRestSessionTask messageTask)
	{
		messageTask.Tick();
		IHttpRequestTask request = messageTask.Request;
		if (request.State != HttpRequestTaskState.Finished)
		{
			return;
		}
		if (request.Successful)
		{
			string responseData = messageTask.Request.ResponseData;
			if (!string.IsNullOrEmpty(responseData))
			{
				SessionlessRestResponse sessionlessRestResponse = JsonConvert.DeserializeObject<SessionlessRestResponse>(responseData, new JsonConverter[1] { _restDataJsonConverter });
				if (sessionlessRestResponse.Successful)
				{
					messageTask.SetFinishedAsSuccessful(sessionlessRestResponse);
				}
				else
				{
					messageTask.SetFinishedAsFailed(sessionlessRestResponse);
				}
			}
			else
			{
				messageTask.SetFinishedAsFailed();
			}
		}
		else
		{
			messageTask.SetFinishedAsFailed();
		}
	}

	private void SendMessage(SessionlessRestRequestMessage message)
	{
		SessionlessClientRestSessionTask clientRestSessionTask = new SessionlessClientRestSessionTask(message);
		AssignRequestJob(clientRestSessionTask);
		Task.Run(async delegate
		{
			while (!clientRestSessionTask.Finished)
			{
				TickTask(clientRestSessionTask);
				await Task.Delay(1);
			}
		});
	}

	void ISessionlessClientDriver.SendMessage(Message message)
	{
		SendMessage(new SessionlessRestDataRequestMessage(message, MessageType.Message));
	}

	async Task<TResult> ISessionlessClientDriver.CallFunction<TResult>(Message message)
	{
		SessionlessClientRestSessionTask clientRestSessionTask = new SessionlessClientRestSessionTask(new SessionlessRestDataRequestMessage(message, MessageType.Function));
		AssignRequestJob(clientRestSessionTask);
		await Task.Run(async delegate
		{
			while (!clientRestSessionTask.Finished)
			{
				TickTask(clientRestSessionTask);
				await Task.Delay(1);
			}
		});
		if (clientRestSessionTask.Successful)
		{
			return (TResult)clientRestSessionTask.RestResponse.FunctionResult.GetFunctionResult();
		}
		throw new Exception("Could not call function with " + message.GetType().Name);
	}

	async Task<bool> ISessionlessClientDriver.CheckConnection()
	{
		try
		{
			string text = "http://";
			if (_isSecure)
			{
				text = "https://";
			}
			string url = text + _address + ":" + _port + "/Data/Ping";
			await _platformNetworkClient.HttpGetString(url, withUserToken: false);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
