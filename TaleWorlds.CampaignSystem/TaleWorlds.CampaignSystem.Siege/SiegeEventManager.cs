using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Siege;

public class SiegeEventManager
{
	[SaveableField(1)]
	private MBList<SiegeEvent> _siegeEvents;

	public MBReadOnlyList<SiegeEvent> SiegeEvents => _siegeEvents;

	internal static void AutoGeneratedStaticCollectObjectsSiegeEventManager(object o, List<object> collectedObjects)
	{
		((SiegeEventManager)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected virtual void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		collectedObjects.Add(_siegeEvents);
	}

	internal static object AutoGeneratedGetMemberValue_siegeEvents(object o)
	{
		return ((SiegeEventManager)o)._siegeEvents;
	}

	public SiegeEventManager()
	{
		_siegeEvents = new MBList<SiegeEvent>();
	}

	public SiegeEvent StartSiegeEvent(Settlement settlement, MobileParty besiegerParty)
	{
		SiegeEvent siegeEvent = new SiegeEvent(settlement, besiegerParty);
		_siegeEvents.Add(siegeEvent);
		settlement.Party.SetVisualAsDirty();
		return siegeEvent;
	}

	public void Tick(float dt)
	{
		for (int i = 0; i < _siegeEvents.Count; i++)
		{
			if (_siegeEvents[i].ReadyToBeRemoved)
			{
				_siegeEvents[i] = _siegeEvents[_siegeEvents.Count - 1];
				_siegeEvents.RemoveAt(_siegeEvents.Count - 1);
				i--;
			}
			else
			{
				_siegeEvents[i].Tick(dt);
			}
		}
	}
}
