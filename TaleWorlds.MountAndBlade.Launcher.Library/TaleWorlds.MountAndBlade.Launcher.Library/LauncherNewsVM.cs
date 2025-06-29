using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Library.NewsManager;

namespace TaleWorlds.MountAndBlade.Launcher.Library;

public class LauncherNewsVM : ViewModel
{
	private readonly NewsManager _newsManager;

	private const int _numOfNewsItemsToShow = 3;

	private LauncherNewsItemVM _mainNews;

	private MBBindingList<LauncherNewsItemVM> _newsItems;

	private bool _isDisabledOnMultiplayer;

	[DataSourceProperty]
	public bool IsDisabledOnMultiplayer
	{
		get
		{
			return _isDisabledOnMultiplayer;
		}
		set
		{
			if (value != _isDisabledOnMultiplayer)
			{
				_isDisabledOnMultiplayer = value;
				OnPropertyChangedWithValue(value, "IsDisabledOnMultiplayer");
			}
		}
	}

	[DataSourceProperty]
	public LauncherNewsItemVM MainNews
	{
		get
		{
			return _mainNews;
		}
		set
		{
			if (value != _mainNews)
			{
				_mainNews = value;
				OnPropertyChangedWithValue(value, "MainNews");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<LauncherNewsItemVM> NewsItems
	{
		get
		{
			return _newsItems;
		}
		set
		{
			if (value != _newsItems)
			{
				_newsItems = value;
				OnPropertyChangedWithValue(value, "NewsItems");
			}
		}
	}

	public LauncherNewsVM(NewsManager newsManager, bool isDefaultMultiplayer)
	{
		_newsManager = newsManager;
		NewsItems = new MBBindingList<LauncherNewsItemVM>();
		GetNewsItems(isDefaultMultiplayer);
		IsDisabledOnMultiplayer = false;
	}

	private async void GetNewsItems(bool isMultiplayer)
	{
		await _newsManager.GetNewsItems(forceRefresh: false);
		Refresh(isMultiplayer);
	}

	public void Refresh(bool isMultiplayer)
	{
		NewsItems.Clear();
		MainNews = new LauncherNewsItemVM(default(NewsItem), isMultiplayer);
		NewsItem.NewsTypes singleplayerMultiplayerEnum = (isMultiplayer ? NewsItem.NewsTypes.LauncherMultiplayer : NewsItem.NewsTypes.LauncherSingleplayer);
		List<IGrouping<int, NewsItem>> list = (from i in (from i in _newsManager.NewsItems.Where((NewsItem n) => n.Feeds.Any((NewsType t) => t.Type == singleplayerMultiplayerEnum) && !string.IsNullOrEmpty(n.Title) && !string.IsNullOrEmpty(n.NewsLink) && !string.IsNullOrEmpty(n.ImageSourcePath)).ToList()
				group i by i.Feeds.First((NewsType t) => t.Type == singleplayerMultiplayerEnum).Index).ToList()
			orderby i.Key
			select i).ToList();
		for (int j = 0; j < list.Count; j++)
		{
			if (NewsItems.Count >= 3)
			{
				break;
			}
			NewsItem newsItem = list[j].First();
			NewsItem item = (newsItem.Equals(default(NewsItem)) ? default(NewsItem) : newsItem);
			NewsItems.Add(new LauncherNewsItemVM(item, isMultiplayer));
		}
	}
}
