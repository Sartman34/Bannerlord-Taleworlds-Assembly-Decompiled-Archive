using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.HUD.KillFeed.General;

public class SPGeneralKillNotificationVM : ViewModel
{
	private MBBindingList<SPGeneralKillNotificationItemVM> _notificationList;

	[DataSourceProperty]
	public MBBindingList<SPGeneralKillNotificationItemVM> NotificationList
	{
		get
		{
			return _notificationList;
		}
		set
		{
			if (value != _notificationList)
			{
				_notificationList = value;
				OnPropertyChangedWithValue(value, "NotificationList");
			}
		}
	}

	public SPGeneralKillNotificationVM()
	{
		NotificationList = new MBBindingList<SPGeneralKillNotificationItemVM>();
	}

	public void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, Agent assistedAgent, bool isHeadshot)
	{
		NotificationList.Add(new SPGeneralKillNotificationItemVM(affectedAgent, affectorAgent, assistedAgent, isHeadshot, RemoveItem));
	}

	private void RemoveItem(SPGeneralKillNotificationItemVM item)
	{
		NotificationList.Remove(item);
	}
}
