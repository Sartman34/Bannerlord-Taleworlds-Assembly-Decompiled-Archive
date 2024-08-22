using System;
using System.Collections.Specialized;
using System.Net;
using TaleWorlds.Library;
using TaleWorlds.Library.Http;

namespace TaleWorlds.Diamond.Rest;

internal class SessionlessClientRestSessionTask
{
	private string _requestAddress;

	private ushort _requestPort;

	private string _postData;

	private bool _isSecure;

	private int _maxIterationCount = 5;

	private int _currentIterationCount;

	private IHttpDriver _networkClient;

	public SessionlessRestRequestMessage RestRequestMessage { get; private set; }

	public bool Finished { get; private set; }

	public bool Successful { get; private set; }

	public IHttpRequestTask Request { get; private set; }

	public SessionlessRestResponse RestResponse { get; private set; }

	public SessionlessClientRestSessionTask(SessionlessRestRequestMessage restRequestMessage)
	{
		RestRequestMessage = restRequestMessage;
	}

	public void SetRequestData(string address, ushort port, bool isSecure, IHttpDriver networkClient)
	{
		_requestAddress = address;
		_requestPort = port;
		_isSecure = isSecure;
		_postData = RestRequestMessage.SerializeAsJson();
		_networkClient = networkClient;
		CreateAndSetRequest();
	}

	public void Tick()
	{
		switch (Request.State)
		{
		case HttpRequestTaskState.NotStarted:
			Request.Start();
			break;
		case HttpRequestTaskState.Finished:
			if (Request.Successful || _currentIterationCount >= _maxIterationCount)
			{
				break;
			}
			if (Request.Exception != null && Request.Exception is WebException)
			{
				WebException ex = (WebException)Request.Exception;
				if (ex.Status == WebExceptionStatus.ConnectionClosed || ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.KeepAliveFailure || ex.Status == WebExceptionStatus.ReceiveFailure || ex.Status == WebExceptionStatus.RequestCanceled || ex.Status == WebExceptionStatus.Timeout || ex.Status == WebExceptionStatus.ProtocolError || ex.Status == WebExceptionStatus.UnknownError)
				{
					Debug.Print(string.Concat("Http Post Request with message failed. Retrying. (", ex.Status, ")"));
					CreateAndSetRequest();
				}
				else
				{
					Debug.Print("Http Post Request with message failed. Exception status: " + ex.Status);
				}
			}
			else if (Request.Exception != null && Request.Exception is InvalidOperationException)
			{
				Debug.Print(string.Concat("Http Post Request with message failed. Retrying: (", Request.Exception?.GetType(), ") ", Request.Exception));
				CreateAndSetRequest();
			}
			else
			{
				Debug.Print(string.Concat("Http Post Request with message failed. Exception: (", Request.Exception?.GetType(), ") ", Request.Exception));
			}
			_currentIterationCount++;
			break;
		case HttpRequestTaskState.Working:
			break;
		}
	}

	public void SetFinishedAsSuccessful(SessionlessRestResponse restResponse)
	{
		Debug.Print("SessionlessClientRestSessionTask::SetFinishedAsSuccessful");
		RestResponse = restResponse;
		Successful = true;
		Finished = true;
		Debug.Print("SessionlessClientRestSessionTask::SetFinishedAsSuccessful done");
	}

	public void SetFinishedAsFailed()
	{
		SetFinishedAsFailed(null);
	}

	public void SetFinishedAsFailed(SessionlessRestResponse restResponse)
	{
		Debug.Print("SessionlessClientRestSessionTask::SetFinishedAsFailed");
		RestResponse = restResponse;
		Successful = false;
		Finished = true;
		Debug.Print("SessionlessClientRestSessionTask::SetFinishedAsFailed done");
	}

	private void CreateAndSetRequest()
	{
		string text = "http://";
		if (_isSecure)
		{
			text = "https://";
		}
		string text2 = text + _requestAddress + ":" + _requestPort + "/SessionlessData/ProcessMessage";
		new NameValueCollection
		{
			{ "url", text2 },
			{ "body", _postData },
			{ "verb", "POST" }
		};
		Request = _networkClient.CreateHttpPostRequestTask(text2, _postData, withUserToken: true);
	}
}
