using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class Bird : MissionObject
{
	private enum State
	{
		TakingOff,
		Airborne,
		Landing,
		Perched
	}

	private const float Speed = 14400f;

	private const string IdleAnimation = "anim_bird_idle";

	private const string LandingAnimation = "anim_bird_landing";

	private const string TakingOffAnimation = "anim_bird_flying";

	private const string FlyCycleAnimation = "anim_bird_cycle";

	public bool CanFly;

	private float _kmPerHour;

	private State _state;

	private BasicMissionTimer _timer;

	private bool _canLand = true;

	private State GetState()
	{
		if (CanFly && !_canLand)
		{
			return State.Airborne;
		}
		return State.Perched;
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		base.GameEntity.SetAnimationSoundActivation(activate: true);
		base.GameEntity.Skeleton.SetAnimationAtChannel("anim_bird_idle", 0);
		base.GameEntity.Skeleton.SetAnimationParameterAtChannel(0, MBRandom.RandomFloat * 0.5f);
		_kmPerHour = 4f;
		_state = GetState();
		if (_timer == null)
		{
			_timer = new BasicMissionTimer();
		}
		SetScriptComponentToTick(GetTickRequirement());
	}

	protected internal override void OnEditorInit()
	{
		base.OnEditorInit();
		base.GameEntity.Skeleton.SetAnimationAtChannel("anim_bird_idle", 0);
		base.GameEntity.Skeleton.SetAnimationParameterAtChannel(0, 0f);
	}

	public override TickRequirement GetTickRequirement()
	{
		return TickRequirement.Tick | base.GetTickRequirement();
	}

	protected internal override void OnTick(float dt)
	{
		switch (_state)
		{
		case State.TakingOff:
			ApplyDisplacement(dt);
			if (base.GameEntity.Skeleton.GetAnimationParameterAtChannel(0) > 0.99f)
			{
				base.GameEntity.Skeleton.SetAnimationAtChannel("anim_bird_cycle", 0);
				_timer.Reset();
				SetDisplacement();
				_state = State.Airborne;
			}
			break;
		case State.Airborne:
			if (_timer.ElapsedTime > 5f)
			{
				if (_canLand)
				{
					base.GameEntity.Skeleton.SetAnimationAtChannel("anim_bird_landing", 0);
					_timer.Reset();
					_state = State.Landing;
					SetDisplacement();
				}
				else
				{
					base.GameEntity.SetVisibilityExcludeParents(visible: false);
				}
			}
			else
			{
				ApplyDisplacement(dt);
			}
			break;
		case State.Landing:
			ApplyDisplacement(dt);
			if (base.GameEntity.Skeleton.GetAnimationParameterAtChannel(0) > 0.99f)
			{
				base.GameEntity.Skeleton.SetAnimationAtChannel("anim_bird_idle", 0);
				_timer.Reset();
				_state = State.Perched;
			}
			break;
		case State.Perched:
			if (CanFly && _timer.ElapsedTime > 5f + MBRandom.RandomFloat * 13f && base.GameEntity.Skeleton.GetAnimationParameterAtChannel(0) > 0.99f)
			{
				base.GameEntity.Skeleton.SetAnimationAtChannel("anim_bird_flying", 0);
				_timer.Reset();
				_state = State.TakingOff;
			}
			break;
		}
	}

	private void ApplyDisplacement(float dt)
	{
		float num = _kmPerHour * dt;
		MatrixFrame frame = base.GameEntity.GetGlobalFrame();
		MatrixFrame matrixFrame = frame;
		Vec3 f = frame.rotation.f;
		Vec3 u = frame.rotation.u;
		f.Normalize();
		u.Normalize();
		if (_state == State.TakingOff)
		{
			matrixFrame.origin = matrixFrame.origin - f * 0.30769232f + u * 0.1f;
		}
		else if (_state == State.Airborne)
		{
			frame.origin -= f * num;
			matrixFrame.origin -= f * num;
		}
		else if (_state == State.Landing)
		{
			matrixFrame.origin = matrixFrame.origin - f * 0.30769232f - u * 0.1f;
		}
		base.GameEntity.SetGlobalFrame(in frame);
	}

	private void SetDisplacement()
	{
		MatrixFrame frame = base.GameEntity.GetGlobalFrame();
		Vec3 f = frame.rotation.f;
		Vec3 u = frame.rotation.u;
		f.Normalize();
		u.Normalize();
		if (_state == State.TakingOff)
		{
			frame.origin -= f * 20f - u * 6.5f;
		}
		else if (_state == State.Landing)
		{
			frame.origin -= f * 20f + u * 6.5f;
		}
		base.GameEntity.SetGlobalFrame(in frame);
	}
}
