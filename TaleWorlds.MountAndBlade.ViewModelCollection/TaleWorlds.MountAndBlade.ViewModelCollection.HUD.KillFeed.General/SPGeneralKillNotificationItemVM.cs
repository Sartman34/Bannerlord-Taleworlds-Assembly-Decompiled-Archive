using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.HUD.KillFeed.General;

public class SPGeneralKillNotificationItemVM : ViewModel
{
	private const char _seperator = '\0';

	private readonly Agent _affectedAgent;

	private readonly Agent _affectorAgent;

	private readonly Agent _assistedAgent;

	private readonly Action<SPGeneralKillNotificationItemVM> _onRemove;

	private bool _showNames;

	private string _message;

	private Color friendlyColor => new Color(0.54296875f, 0.77734375f, 27f / 64f);

	private Color enemyColor => new Color(61f / 64f, 0.48828125f, 0.42578125f);

	[DataSourceProperty]
	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (value != _message)
			{
				_message = value;
				OnPropertyChangedWithValue(value, "Message");
			}
		}
	}

	public SPGeneralKillNotificationItemVM(Agent affectedAgent, Agent affectorAgent, Agent assistedAgent, bool isHeadshot, Action<SPGeneralKillNotificationItemVM> onRemove)
	{
		_affectedAgent = affectedAgent;
		_affectorAgent = affectorAgent;
		_assistedAgent = assistedAgent;
		_onRemove = onRemove;
		_showNames = BannerlordConfig.ReportCasualtiesType == 0;
		InitProperties(_affectedAgent, _affectorAgent, _assistedAgent, isHeadshot);
	}

	private void InitProperties(Agent affectedAgent, Agent affectorAgent, Agent assistedAgent, bool isHeadshot)
	{
		MBStringBuilder mBStringBuilder = default(MBStringBuilder);
		mBStringBuilder.Initialize(64, "InitProperties");
		if (_showNames || (affectorAgent != null && affectorAgent.Character?.IsHero == true))
		{
			mBStringBuilder.Append(affectorAgent.Name);
		}
		mBStringBuilder.Append('\0');
		mBStringBuilder.Append(GetAgentType(affectorAgent));
		mBStringBuilder.Append('\0');
		if (!_showNames)
		{
			BasicCharacterObject character = affectedAgent.Character;
			if (character == null || !character.IsHero)
			{
				goto IL_00b0;
			}
		}
		mBStringBuilder.Append(affectedAgent.Name);
		goto IL_00b0;
		IL_00b0:
		mBStringBuilder.Append('\0');
		mBStringBuilder.Append(GetAgentType(affectedAgent));
		mBStringBuilder.Append('\0');
		mBStringBuilder.Append((affectedAgent.State == AgentState.Unconscious) ? 1 : 0);
		mBStringBuilder.Append('\0');
		mBStringBuilder.Append(isHeadshot ? 1 : 0);
		mBStringBuilder.Append('\0');
		Team team = affectedAgent.Team;
		mBStringBuilder.Append(((team == null || !team.IsValid) ? Color.FromUint(4284111450u) : ((!affectedAgent.Team.IsPlayerAlly) ? friendlyColor : enemyColor)).ToUnsignedInteger());
		Message = mBStringBuilder.ToStringAndRelease();
	}

	private static string GetAgentType(Agent agent)
	{
		if (agent?.Character != null)
		{
			switch ((FormationClass)agent.Character.DefaultFormationGroup)
			{
			case FormationClass.Infantry:
				return "Infantry_Light";
			case FormationClass.Ranged:
				return "Archer_Light";
			case FormationClass.Cavalry:
				return "Cavalry_Light";
			case FormationClass.HorseArcher:
				return "HorseArcher_Light";
			case FormationClass.LightCavalry:
				return "Cavalry_Light";
			case FormationClass.HeavyCavalry:
				return "Cavalry_Heavy";
			case FormationClass.NumberOfDefaultFormations:
			case FormationClass.HeavyInfantry:
				return "Infantry_Heavy";
			case FormationClass.NumberOfRegularFormations:
			case FormationClass.Bodyguard:
			case FormationClass.NumberOfAllFormations:
				return "Infantry_Heavy";
			default:
				return "None";
			}
		}
		return "None";
	}

	public void ExecuteRemove()
	{
		_onRemove(this);
	}
}
