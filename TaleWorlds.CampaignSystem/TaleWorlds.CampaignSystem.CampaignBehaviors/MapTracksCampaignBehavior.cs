using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class MapTracksCampaignBehavior : CampaignBehaviorBase, IMapTracksCampaignBehavior, ICampaignBehavior
{
	private class TrackPool
	{
		private Stack<Track> _stack;

		private int MaxSize { get; }

		public int Size => _stack?.Count ?? 0;

		public TrackPool(int size)
		{
			MaxSize = size;
			_stack = new Stack<Track>();
			for (int i = 0; i < size; i++)
			{
				_stack.Push(new Track());
			}
		}

		public Track RequestTrack(MobileParty party, Vec2 trackPosition, Vec2 trackDirection)
		{
			Track track = ((_stack.Count > 0) ? _stack.Pop() : new Track());
			int num = party.Party.NumberOfAllMembers;
			int num2 = party.Party.NumberOfHealthyMembers;
			int num3 = party.Party.NumberOfMenWithHorse;
			int num4 = party.Party.NumberOfMenWithoutHorse;
			int num5 = party.Party.NumberOfPackAnimals;
			int num6 = party.Party.NumberOfPrisoners;
			TextObject partyName = party.Name;
			if (party.Army != null && party.Army.LeaderParty == party)
			{
				partyName = party.ArmyName;
				foreach (MobileParty attachedParty in party.Army.LeaderParty.AttachedParties)
				{
					num += attachedParty.Party.NumberOfAllMembers;
					num2 += attachedParty.Party.NumberOfHealthyMembers;
					num3 += attachedParty.Party.NumberOfMenWithHorse;
					num4 += attachedParty.Party.NumberOfMenWithoutHorse;
					num5 += attachedParty.Party.NumberOfPackAnimals;
					num6 += attachedParty.Party.NumberOfPrisoners;
				}
			}
			track.Position = trackPosition;
			track.Direction = trackDirection.RotationInRadians;
			track.PartyType = Track.GetPartyTypeEnum(party);
			track.PartyName = partyName;
			track.Culture = party.Party.Culture;
			if (track.Culture == null)
			{
				string message = $"Track culture is null for {party.StringId}: {party.Name}";
				Debug.Print(message);
				Debug.FailedAssert(message, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\MapTracksCampaignBehavior.cs", "RequestTrack", 62);
			}
			track.Speed = party.Speed;
			track.Life = Campaign.Current.Models.MapTrackModel.GetTrackLife(party);
			track.IsEnemy = FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, party.MapFaction);
			track.NumberOfAllMembers = num;
			track.NumberOfHealthyMembers = num2;
			track.NumberOfMenWithHorse = num3;
			track.NumberOfMenWithoutHorse = num4;
			track.NumberOfPackAnimals = num5;
			track.NumberOfPrisoners = num6;
			track.IsPointer = false;
			track.IsDetected = false;
			track.CreationTime = CampaignTime.Now;
			return track;
		}

		public Track RequestMapArrow(TextObject pointerName, Vec2 trackPosition, Vec2 trackDirection, float life)
		{
			Track obj = ((_stack.Count > 0) ? _stack.Pop() : new Track());
			obj.Position = trackPosition;
			obj.Direction = trackDirection.RotationInRadians;
			obj.PartyName = pointerName;
			obj.Life = life;
			obj.IsPointer = true;
			obj.IsDetected = true;
			obj.CreationTime = CampaignTime.Now;
			return obj;
		}

		public void ReleaseTrack(Track track)
		{
			track.Reset();
			if (_stack.Count < MaxSize)
			{
				_stack.Push(track);
			}
		}

		public override string ToString()
		{
			return $"TrackPool: {Size}";
		}
	}

	private const float PartyTrackPositionDelta = 5f;

	private List<Track> _allTracks = new List<Track>();

	private MBList<Track> _detectedTracksCache = new MBList<Track>();

	private Dictionary<MobileParty, Vec2> _trackDataDictionary = new Dictionary<MobileParty, Vec2>();

	private MBCampaignEvent _quarterHourlyTick;

	private LocatorGrid<Track> _trackLocator = new LocatorGrid<Track>();

	private TrackPool _trackPool;

	public MBReadOnlyList<Track> DetectedTracks => _detectedTracksCache;

	public MapTracksCampaignBehavior()
	{
		_trackPool = new TrackPool(2048);
	}

	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, GameLoadFinished);
		CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, OnHourlyTickParty);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		if (_trackDataDictionary.ContainsKey(mobileParty))
		{
			_trackDataDictionary.Remove(mobileParty);
		}
	}

	private void OnNewGameCreated(CampaignGameStarter gameStarted)
	{
		_trackDataDictionary = new Dictionary<MobileParty, Vec2>();
		AddEventHandler();
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_allTracks", ref _allTracks);
		dataStore.SyncData("_trackDataDictionary", ref _trackDataDictionary);
	}

	private void OnHourlyTickParty(MobileParty mobileParty)
	{
		if (Campaign.Current.Models.MapTrackModel.CanPartyLeaveTrack(mobileParty))
		{
			Vec2 vec = Vec2.Zero;
			if (_trackDataDictionary.ContainsKey(mobileParty))
			{
				vec = _trackDataDictionary[mobileParty];
			}
			if (vec.DistanceSquared(mobileParty.Position2D) > 5f && IsTrackDropped(mobileParty))
			{
				Vec2 position2D = mobileParty.Position2D;
				Vec2 trackDirection = mobileParty.Position2D - vec;
				trackDirection.Normalize();
				AddTrack(mobileParty, position2D, trackDirection);
				_trackDataDictionary[mobileParty] = position2D;
			}
		}
	}

	private void OnHourlyTick()
	{
		RemoveExpiredTracks();
	}

	private void GameLoadFinished()
	{
		_allTracks.RemoveAll((Track x) => x.IsExpired);
		_detectedTracksCache = _allTracks.Where((Track x) => x.IsDetected).ToMBList();
		AddEventHandler();
		foreach (Track allTrack in _allTracks)
		{
			_trackLocator.UpdateLocator(allTrack);
		}
		foreach (MobileParty item in _trackDataDictionary.Keys.ToList())
		{
			if (!item.IsActive)
			{
				_trackDataDictionary.Remove(item);
			}
		}
	}

	private void AddEventHandler()
	{
		_quarterHourlyTick = CampaignPeriodicEventManager.CreatePeriodicEvent(CampaignTime.Hours(0.25f), CampaignTime.Hours(0.1f));
		_quarterHourlyTick.AddHandler(QuarterHourlyTick);
	}

	private void QuarterHourlyTick(MBCampaignEvent campaignEvent, object[] delegateParams)
	{
		if (!PartyBase.MainParty.IsValid)
		{
			return;
		}
		int num = ((MobileParty.MainParty.EffectiveScout != null) ? MobileParty.MainParty.EffectiveScout.GetSkillValue(DefaultSkills.Scouting) : 0);
		if (num == 0)
		{
			return;
		}
		float maxTrackSpottingDistanceForMainParty = Campaign.Current.Models.MapTrackModel.GetMaxTrackSpottingDistanceForMainParty();
		LocatableSearchData<Track> data = _trackLocator.StartFindingLocatablesAroundPosition(MobileParty.MainParty.Position2D, maxTrackSpottingDistanceForMainParty);
		for (Track track = _trackLocator.FindNextLocatable(ref data); track != null; track = _trackLocator.FindNextLocatable(ref data))
		{
			if (!track.IsDetected && _allTracks.Contains(track) && Campaign.Current.Models.MapTrackModel.GetTrackDetectionDifficultyForMainParty(track, maxTrackSpottingDistanceForMainParty) < (float)num)
			{
				TrackDetected(track);
			}
		}
	}

	private void RemoveExpiredTracks()
	{
		for (int num = _allTracks.Count - 1; num >= 0; num--)
		{
			Track track = _allTracks[num];
			if (track.IsExpired)
			{
				_allTracks.Remove(track);
				if (_detectedTracksCache.Contains(track))
				{
					_detectedTracksCache.Remove(track);
					CampaignEventDispatcher.Instance.TrackLost(track);
				}
				_trackLocator.RemoveLocatable(track);
				_trackPool.ReleaseTrack(track);
			}
		}
	}

	private void TrackDetected(Track track)
	{
		track.IsDetected = true;
		_detectedTracksCache.Add(track);
		CampaignEventDispatcher.Instance.TrackDetected(track);
		SkillLevelingManager.OnTrackDetected(track);
	}

	public bool IsTrackDropped(MobileParty mobileParty)
	{
		float skipTrackChance = Campaign.Current.Models.MapTrackModel.GetSkipTrackChance(mobileParty);
		if (MBRandom.RandomFloat < skipTrackChance)
		{
			return false;
		}
		float num = mobileParty.Position2D.DistanceSquared(MobileParty.MainParty.Position2D);
		float num2 = MobileParty.MainParty.Speed * Campaign.Current.Models.MapTrackModel.MaxTrackLife;
		if (num2 * num2 > num)
		{
			return true;
		}
		return false;
	}

	public void AddTrack(MobileParty party, Vec2 trackPosition, Vec2 trackDirection)
	{
		Track track = _trackPool.RequestTrack(party, trackPosition, trackDirection);
		_allTracks.Add(track);
		_trackLocator.UpdateLocator(track);
	}

	public void AddMapArrow(TextObject pointerName, Vec2 trackPosition, Vec2 trackDirection, float life)
	{
		Track track = _trackPool.RequestMapArrow(pointerName, trackPosition, trackDirection, life);
		_allTracks.Add(track);
		_trackLocator.UpdateLocator(track);
		TrackDetected(track);
	}
}
