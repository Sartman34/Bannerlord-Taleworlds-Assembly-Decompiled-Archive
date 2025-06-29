using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public struct CombatLogData
{
	private const string DetailTagStart = "<Detail>";

	private const string DetailTagEnd = "</Detail>";

	private const uint DamageReceivedColor = 4292917946u;

	private const uint DamageDealedColor = 4210351871u;

	private static List<(string, uint)> _logStringCache = new List<(string, uint)>();

	public readonly bool IsVictimAgentSameAsAttackerAgent;

	public readonly bool IsVictimRiderAgentSameAsAttackerAgent;

	public readonly bool IsAttackerAgentHuman;

	public readonly bool IsAttackerAgentMine;

	public readonly bool DoesAttackerAgentHaveRiderAgent;

	public readonly bool IsAttackerAgentRiderAgentMine;

	public readonly bool IsAttackerAgentMount;

	public readonly bool IsVictimAgentHuman;

	public readonly bool IsVictimAgentMine;

	public readonly bool DoesVictimAgentHaveRiderAgent;

	public readonly bool IsVictimAgentRiderAgentMine;

	public readonly bool IsVictimAgentMount;

	public bool IsVictimEntity;

	public DamageTypes DamageType;

	public bool CrushedThrough;

	public bool Chamber;

	public bool IsRangedAttack;

	public bool IsFriendlyFire;

	public bool IsFatalDamage;

	public BoneBodyPartType BodyPartHit;

	public string VictimAgentName;

	public float HitSpeed;

	public int InflictedDamage;

	public int AbsorbedDamage;

	public int ModifiedDamage;

	public float Distance;

	private bool IsValidForPlayer
	{
		get
		{
			if (IsImportant)
			{
				if (!IsAttackerPlayer)
				{
					return IsVictimPlayer;
				}
				return true;
			}
			return false;
		}
	}

	private bool IsImportant
	{
		get
		{
			if (TotalDamage <= 0 && !CrushedThrough)
			{
				return Chamber;
			}
			return true;
		}
	}

	private bool IsAttackerPlayer
	{
		get
		{
			if (!IsAttackerAgentHuman)
			{
				if (DoesAttackerAgentHaveRiderAgent)
				{
					return IsAttackerAgentRiderAgentMine;
				}
				return false;
			}
			return IsAttackerAgentMine;
		}
	}

	private bool IsVictimPlayer
	{
		get
		{
			if (!IsVictimAgentHuman)
			{
				if (DoesVictimAgentHaveRiderAgent)
				{
					return IsVictimAgentRiderAgentMine;
				}
				return false;
			}
			return IsVictimAgentMine;
		}
	}

	private bool IsAttackerMount => IsAttackerAgentMount;

	private bool IsVictimMount => IsVictimAgentMount;

	public int TotalDamage => InflictedDamage + ModifiedDamage;

	public float AttackProgress { get; internal set; }

	public List<(string, uint)> GetLogString()
	{
		_logStringCache.Clear();
		if (IsValidForPlayer && ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.ReportDamage) > 0f)
		{
			if (IsRangedAttack && IsAttackerPlayer && BodyPartHit == BoneBodyPartType.Head)
			{
				_logStringCache.Add(ValueTuple.Create(GameTexts.FindText("ui_head_shot").ToString(), 4289612505u));
			}
			if (IsFriendlyFire)
			{
				_logStringCache.Add(ValueTuple.Create(GameTexts.FindText("combat_log_friendly_fire").ToString(), 4289612505u));
			}
			if (CrushedThrough && !IsFriendlyFire)
			{
				if (IsAttackerPlayer)
				{
					_logStringCache.Add(ValueTuple.Create(GameTexts.FindText("combat_log_crushed_through_attacker").ToString(), 4289612505u));
				}
				else
				{
					_logStringCache.Add(ValueTuple.Create(GameTexts.FindText("combat_log_crushed_through_victim").ToString(), 4289612505u));
				}
			}
			if (Chamber)
			{
				_logStringCache.Add(ValueTuple.Create(GameTexts.FindText("combat_log_chamber_blocked").ToString(), 4289612505u));
			}
			uint item = 4290563554u;
			GameTexts.SetVariable("DAMAGE", TotalDamage);
			int damageType = (int)DamageType;
			GameTexts.SetVariable("DAMAGE_TYPE", GameTexts.FindText("combat_log_damage_type", damageType.ToString()));
			MBStringBuilder mBStringBuilder = default(MBStringBuilder);
			mBStringBuilder.Initialize(16, "GetLogString");
			if (IsVictimAgentSameAsAttackerAgent)
			{
				mBStringBuilder.Append(GameTexts.FindText("ui_received_number_damage_fall"));
				item = 4292917946u;
			}
			else if (IsVictimMount)
			{
				if (IsVictimRiderAgentSameAsAttackerAgent)
				{
					mBStringBuilder.Append(GameTexts.FindText("ui_received_number_damage_fall_to_horse"));
					item = 4292917946u;
				}
				else
				{
					mBStringBuilder.Append(GameTexts.FindText(IsAttackerPlayer ? "ui_delivered_number_damage_to_horse" : "ui_horse_received_number_damage"));
					item = (IsAttackerPlayer ? 4210351871u : 4292917946u);
				}
			}
			else if (IsVictimEntity)
			{
				mBStringBuilder.Append(GameTexts.FindText("ui_delivered_number_damage_to_entity"));
			}
			else if (IsAttackerMount)
			{
				mBStringBuilder.Append(GameTexts.FindText(IsAttackerPlayer ? "ui_horse_charged_for_number_damage" : "ui_received_number_damage"));
				item = (IsAttackerPlayer ? 4210351871u : 4292917946u);
			}
			else if (TotalDamage > 0)
			{
				mBStringBuilder.Append(GameTexts.FindText(IsAttackerPlayer ? "ui_delivered_number_damage" : "ui_received_number_damage"));
				item = (IsAttackerPlayer ? 4210351871u : 4292917946u);
			}
			if (BodyPartHit != BoneBodyPartType.None)
			{
				damageType = (int)BodyPartHit;
				GameTexts.SetVariable("BODY_PART", GameTexts.FindText("body_part_type", damageType.ToString()));
				mBStringBuilder.Append("<Detail>");
				mBStringBuilder.Append(GameTexts.FindText("combat_log_detail_body_part"));
				mBStringBuilder.Append("</Detail>");
			}
			if (HitSpeed > 1E-05f)
			{
				GameTexts.SetVariable("SPEED", TaleWorlds.Library.MathF.Round(HitSpeed, 2));
				mBStringBuilder.Append("<Detail>");
				mBStringBuilder.Append(IsRangedAttack ? GameTexts.FindText("combat_log_detail_missile_speed") : GameTexts.FindText("combat_log_detail_move_speed"));
				mBStringBuilder.Append("</Detail>");
			}
			if (IsRangedAttack)
			{
				GameTexts.SetVariable("DISTANCE", TaleWorlds.Library.MathF.Round(Distance, 1));
				mBStringBuilder.Append("<Detail>");
				mBStringBuilder.Append(GameTexts.FindText("combat_log_detail_distance"));
				mBStringBuilder.Append("</Detail>");
			}
			if (AbsorbedDamage > 0)
			{
				GameTexts.SetVariable("ABSORBED_DAMAGE", AbsorbedDamage);
				mBStringBuilder.Append("<Detail>");
				mBStringBuilder.Append(GameTexts.FindText("combat_log_detail_absorbed_damage"));
				mBStringBuilder.Append("</Detail>");
			}
			if (ModifiedDamage != 0)
			{
				GameTexts.SetVariable("MODIFIED_DAMAGE", TaleWorlds.Library.MathF.Abs(ModifiedDamage));
				mBStringBuilder.Append("<Detail>");
				if (ModifiedDamage > 0)
				{
					mBStringBuilder.Append(GameTexts.FindText("combat_log_detail_extra_damage"));
				}
				else if (ModifiedDamage < 0)
				{
					mBStringBuilder.Append(GameTexts.FindText("combat_log_detail_reduced_damage"));
				}
				mBStringBuilder.Append("</Detail>");
			}
			_logStringCache.Add(ValueTuple.Create(mBStringBuilder.ToStringAndRelease(), item));
		}
		return _logStringCache;
	}

	public CombatLogData(bool isVictimAgentSameAsAttackerAgent, bool isAttackerAgentHuman, bool isAttackerAgentMine, bool doesAttackerAgentHaveRiderAgent, bool isAttackerAgentRiderAgentMine, bool isAttackerAgentMount, bool isVictimAgentHuman, bool isVictimAgentMine, bool isVictimAgentDead, bool doesVictimAgentHaveRiderAgent, bool isVictimAgentRiderAgentIsMine, bool isVictimAgentMount, bool isVictimEntity, bool isVictimRiderAgentSameAsAttackerAgent, bool crushedThrough, bool chamber, float distance)
	{
		IsVictimAgentSameAsAttackerAgent = isVictimAgentSameAsAttackerAgent;
		IsAttackerAgentHuman = isAttackerAgentHuman;
		IsAttackerAgentMine = isAttackerAgentMine;
		DoesAttackerAgentHaveRiderAgent = doesAttackerAgentHaveRiderAgent;
		IsAttackerAgentRiderAgentMine = isAttackerAgentRiderAgentMine;
		IsAttackerAgentMount = isAttackerAgentMount;
		IsVictimAgentHuman = isVictimAgentHuman;
		IsVictimAgentMine = isVictimAgentMine;
		DoesVictimAgentHaveRiderAgent = doesVictimAgentHaveRiderAgent;
		IsVictimAgentRiderAgentMine = isVictimAgentRiderAgentIsMine;
		IsVictimAgentMount = isVictimAgentMount;
		IsVictimEntity = isVictimEntity;
		IsVictimRiderAgentSameAsAttackerAgent = isVictimRiderAgentSameAsAttackerAgent;
		IsFatalDamage = isVictimAgentDead;
		DamageType = DamageTypes.Blunt;
		CrushedThrough = crushedThrough;
		Chamber = chamber;
		IsRangedAttack = false;
		IsFriendlyFire = false;
		VictimAgentName = null;
		HitSpeed = 0f;
		InflictedDamage = 0;
		AbsorbedDamage = 0;
		ModifiedDamage = 0;
		AttackProgress = 0f;
		BodyPartHit = BoneBodyPartType.None;
		Distance = distance;
	}

	public void SetVictimAgent(Agent victimAgent)
	{
		if (victimAgent?.MissionPeer != null)
		{
			VictimAgentName = victimAgent.MissionPeer.DisplayedName;
		}
		else
		{
			VictimAgentName = victimAgent?.Name;
		}
	}
}
