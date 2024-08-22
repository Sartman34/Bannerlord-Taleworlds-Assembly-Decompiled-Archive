namespace TaleWorlds.Core;

public class Timer
{
	private float _startTime;

	private float _latestGameTime;

	private bool _autoReset;

	public float Duration { get; protected set; }

	public float PreviousDeltaTime { get; private set; }

	public Timer(float gameTime, float duration, bool autoReset = true)
	{
		_startTime = gameTime;
		_latestGameTime = gameTime;
		_autoReset = autoReset;
		Duration = duration;
	}

	public virtual bool Check(float gameTime)
	{
		_latestGameTime = gameTime;
		if (Duration <= 0f)
		{
			PreviousDeltaTime = ElapsedTime();
			_startTime = gameTime;
			return true;
		}
		bool result = false;
		if (ElapsedTime() >= Duration)
		{
			PreviousDeltaTime = ElapsedTime();
			if (_autoReset)
			{
				while (ElapsedTime() >= Duration)
				{
					_startTime += Duration;
				}
			}
			result = true;
		}
		return result;
	}

	public float ElapsedTime()
	{
		return _latestGameTime - _startTime;
	}

	public void Reset(float gameTime)
	{
		Reset(gameTime, Duration);
	}

	public void Reset(float gameTime, float newDuration)
	{
		_startTime = gameTime;
		_latestGameTime = gameTime;
		Duration = newDuration;
	}

	public void AdjustStartTime(float deltaTime)
	{
		_startTime += deltaTime;
	}
}
