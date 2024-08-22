using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Library.NewsManager;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Home;

public class MPNewsVM : ViewModel
{
	private NewsManager _newsManager;

	private const int _numOfNewsItemsToShow = 4;

	private bool _hasValidNews;

	private MPNewsItemVM _mainNews;

	private MBBindingList<MPNewsItemVM> _importantNews;

	[DataSourceProperty]
	public bool HasValidNews
	{
		get
		{
			return _hasValidNews;
		}
		set
		{
			if (value != _hasValidNews)
			{
				_hasValidNews = value;
				OnPropertyChangedWithValue(value, "HasValidNews");
			}
		}
	}

	[DataSourceProperty]
	public MPNewsItemVM MainNews
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
	public MBBindingList<MPNewsItemVM> ImportantNews
	{
		get
		{
			return _importantNews;
		}
		set
		{
			if (value != _importantNews)
			{
				_importantNews = value;
				OnPropertyChangedWithValue(value, "ImportantNews");
			}
		}
	}

	public MPNewsVM(NewsManager newsManager)
	{
		_newsManager = newsManager;
		ImportantNews = new MBBindingList<MPNewsItemVM>();
		GetNewsItems();
		if (MainNews != null)
		{
			HasValidNews = true;
		}
	}

	private async void GetNewsItems()
	{
		await _newsManager.GetNewsItems(forceRefresh: false);
		RefreshNews();
	}

	private void RefreshNews()
	{
		ImportantNews.Clear();
		List<IGrouping<int, NewsItem>> list = (from i in (from i in _newsManager.NewsItems.Where((NewsItem n) => n.Feeds.Any((NewsType t) => t.Type == NewsItem.NewsTypes.MultiplayerLobby) && !string.IsNullOrEmpty(n.Title) && !string.IsNullOrEmpty(n.NewsLink) && !string.IsNullOrEmpty(n.ImageSourcePath)).ToList()
				group i by i.Feeds.First((NewsType t) => t.Type == NewsItem.NewsTypes.MultiplayerLobby).Index).ToList()
			orderby i.Key
			select i).ToList();
		for (int j = 0; j < list.Count; j++)
		{
			if (ImportantNews.Count + 1 >= 4)
			{
				break;
			}
			NewsItem newsItem = list[j].First();
			NewsItem item = (newsItem.Equals(default(NewsItem)) ? default(NewsItem) : newsItem);
			if (j == 0)
			{
				MainNews = new MPNewsItemVM(item);
			}
			else
			{
				ImportantNews.Add(new MPNewsItemVM(item));
			}
		}
		if (MainNews != null)
		{
			HasValidNews = true;
		}
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		_newsManager = null;
	}
}
