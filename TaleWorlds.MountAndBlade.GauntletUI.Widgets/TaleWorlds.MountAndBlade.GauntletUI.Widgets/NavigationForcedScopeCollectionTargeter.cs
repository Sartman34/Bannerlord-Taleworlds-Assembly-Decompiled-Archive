using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.GamepadNavigation;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class NavigationForcedScopeCollectionTargeter : Widget
{
	private GamepadNavigationForcedScopeCollection _collection;

	public bool IsCollectionEnabled
	{
		get
		{
			return _collection.IsEnabled;
		}
		set
		{
			if (value != _collection.IsEnabled)
			{
				_collection.IsEnabled = value;
			}
		}
	}

	public bool IsCollectionDisabled
	{
		get
		{
			return _collection.IsDisabled;
		}
		set
		{
			if (value != _collection.IsDisabled)
			{
				_collection.IsDisabled = value;
			}
		}
	}

	public string CollectionID
	{
		get
		{
			return _collection.CollectionID;
		}
		set
		{
			if (value != _collection.CollectionID)
			{
				_collection.CollectionID = value;
			}
		}
	}

	public int CollectionOrder
	{
		get
		{
			return _collection.CollectionOrder;
		}
		set
		{
			if (value != _collection.CollectionOrder)
			{
				_collection.CollectionOrder = value;
			}
		}
	}

	public Widget CollectionParent
	{
		get
		{
			return _collection.ParentWidget;
		}
		set
		{
			if (_collection.ParentWidget != value)
			{
				if (_collection.ParentWidget != null)
				{
					base.GamepadNavigationContext.RemoveForcedScopeCollection(_collection);
				}
				_collection.ParentWidget = value;
				if (value != null)
				{
					base.GamepadNavigationContext.AddForcedScopeCollection(_collection);
				}
			}
		}
	}

	public NavigationForcedScopeCollectionTargeter(UIContext context)
		: base(context)
	{
		_collection = new GamepadNavigationForcedScopeCollection();
		base.WidthSizePolicy = SizePolicy.Fixed;
		base.HeightSizePolicy = SizePolicy.Fixed;
		base.SuggestedHeight = 0f;
		base.SuggestedWidth = 0f;
		base.IsVisible = false;
	}
}
