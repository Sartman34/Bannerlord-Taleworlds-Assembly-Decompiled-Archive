using System.ComponentModel;
using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer.Scoreboard;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

namespace TaleWorlds.MountAndBlade.GauntletUI.AutoGenerated0;

public class MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_8_MultiplayerScoreboardSideInner__InheritedPrefab : MultiplayerScoreboardSideWidget
{
	private MultiplayerScoreboardSideWidget _widget;

	private Widget _widget_0;

	private DimensionSyncWidget _widget_0_0;

	private MultiplayerScoreboardStripedBackgroundWidget _widget_0_0_0;

	private ListPanel _widget_0_1;

	private MissionScoreboardSideVM _datasource_Root;

	private MBBindingList<MissionScoreboardHeaderItemVM> _datasource_Root_EntryProperties;

	private MBBindingList<MissionScoreboardPlayerVM> _datasource_Root_Players;

	public MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_8_MultiplayerScoreboardSideInner__InheritedPrefab(UIContext context)
		: base(context)
	{
	}

	public virtual void CreateWidgets()
	{
		_widget = this;
		_widget_0 = new Widget(base.Context);
		_widget.AddChild(_widget_0);
		_widget_0_0 = new DimensionSyncWidget(base.Context);
		_widget_0.AddChild(_widget_0_0);
		_widget_0_0_0 = new MultiplayerScoreboardStripedBackgroundWidget(base.Context);
		_widget_0_0.AddChild(_widget_0_0_0);
		_widget_0_1 = new ListPanel(base.Context);
		_widget_0.AddChild(_widget_0_1);
	}

	public virtual void SetIds()
	{
		base.Id = "SideParentWidget";
		_widget_0.Id = "Container";
		_widget_0_0.Id = "ScoreboardBackgroundWidget";
		_widget_0_1.Id = "PlayersList";
	}

	public virtual void SetAttributes()
	{
		base.WidthSizePolicy = SizePolicy.StretchToParent;
		base.HeightSizePolicy = SizePolicy.CoverChildren;
		base.HorizontalAlignment = HorizontalAlignment.Center;
		base.Sprite = base.Context.SpriteData.GetSprite("BlankWhiteSquare_9");
		base.AlphaFactor = 0.2f;
		base.NameColumnWidthRatio = 3f;
		base.TitlesListPanel = _widget_0.FindChild(new BindingPath("TitlesContainer\\Titles")) as ListPanel;
		_widget_0.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_0.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_0_0.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_0_0.HeightSizePolicy = SizePolicy.Fixed;
		_widget_0_0.PositionYOffset = 5f;
		_widget_0_0.DimensionToSync = DimensionSyncWidget.Dimensions.Vertical;
		_widget_0_0.MinHeight = 860f;
		_widget_0_0.WidgetToCopyHeightFrom = _widget.FindChild(new BindingPath("..\\..\\SidesList"));
		_widget_0_0_0.DoNotAcceptEvents = true;
		_widget_0_0_0.DoNotPassEventsToChildren = true;
		_widget_0_0_0.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_0_0_0.HeightSizePolicy = SizePolicy.StretchToParent;
		_widget_0_0_0.NameColumnWidthRatio = 3f;
		_widget_0_0_0.ScoreColumnWidthRatio = 2f;
		_widget_0_0_0.SoldiersColumnWidthRatio = 2f;
		_widget_0_1.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_0_1.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0_1.VerticalAlignment = VerticalAlignment.Top;
		_widget_0_1.MarginTop = 10f;
		_widget_0_1.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;
	}

	public virtual void DestroyDataSource()
	{
		if (_datasource_Root == null)
		{
			return;
		}
		_datasource_Root.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root;
		_widget.PropertyChanged -= PropertyChangedListenerOf_widget;
		_widget.boolPropertyChanged -= boolPropertyChangedListenerOf_widget;
		_widget.floatPropertyChanged -= floatPropertyChangedListenerOf_widget;
		_widget.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget;
		_widget.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget;
		_widget.doublePropertyChanged -= doublePropertyChangedListenerOf_widget;
		_widget.intPropertyChanged -= intPropertyChangedListenerOf_widget;
		_widget.uintPropertyChanged -= uintPropertyChangedListenerOf_widget;
		_widget.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget;
		if (_datasource_Root_EntryProperties != null)
		{
			_datasource_Root_EntryProperties.ListChanged -= OnList_datasource_Root_EntryPropertiesChanged;
			for (int num = _widget_0_0_0.ChildCount - 1; num >= 0; num--)
			{
				Widget child = _widget_0_0_0.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child).OnBeforeRemovedChild(child);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)_widget_0_0_0.GetChild(num)).DestroyDataSource();
			}
			_datasource_Root_EntryProperties = null;
		}
		if (_datasource_Root_Players != null)
		{
			_datasource_Root_Players.ListChanged -= OnList_datasource_Root_PlayersChanged;
			for (int num2 = _widget_0_1.ChildCount - 1; num2 >= 0; num2--)
			{
				Widget child2 = _widget_0_1.GetChild(num2);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child2).OnBeforeRemovedChild(child2);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)_widget_0_1.GetChild(num2)).DestroyDataSource();
			}
			_datasource_Root_Players = null;
		}
		_datasource_Root = null;
	}

	public virtual void SetDataSource(MissionScoreboardSideVM dataSource)
	{
		RefreshDataSource_datasource_Root(dataSource);
	}

	private void PropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, object e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void boolPropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, bool e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void floatPropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, float e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void Vec2PropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, Vec2 e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void Vector2PropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, Vector2 e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void doublePropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, double e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void intPropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, int e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void uintPropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, uint e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void ColorPropertyChangedListenerOf_widget(PropertyOwnerObject propertyOwnerObject, string propertyName, Color e)
	{
		HandleWidgetPropertyChangeOf_widget(propertyName);
	}

	private void HandleWidgetPropertyChangeOf_widget(string propertyName)
	{
		if (propertyName == "CultureId")
		{
			_datasource_Root.CultureId = _widget.CultureId;
		}
		else if (propertyName == "CultureColor")
		{
			_datasource_Root.CultureColor1 = _widget.CultureColor;
		}
	}

	private void ViewModelPropertyChangedListenerOf_datasource_Root(object sender, PropertyChangedEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithValueListenerOf_datasource_Root(object sender, PropertyChangedWithValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root(object sender, PropertyChangedWithBoolValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root(object sender, PropertyChangedWithIntValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root(object sender, PropertyChangedWithFloatValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root(object sender, PropertyChangedWithUIntValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root(object sender, PropertyChangedWithColorValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root(object sender, PropertyChangedWithDoubleValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root(object sender, PropertyChangedWithVec2ValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root(e.PropertyName);
	}

	private void HandleViewModelPropertyChangeOf_datasource_Root(string propertyName)
	{
		switch (propertyName)
		{
		case "EntryProperties":
			RefreshDataSource_datasource_Root_EntryProperties(_datasource_Root.EntryProperties);
			break;
		case "Players":
			RefreshDataSource_datasource_Root_Players(_datasource_Root.Players);
			break;
		case "CultureId":
			_widget.CultureId = _datasource_Root.CultureId;
			break;
		case "CultureColor1":
			_widget.CultureColor = _datasource_Root.CultureColor1;
			break;
		}
	}

	public void OnList_datasource_Root_EntryPropertiesChanged(object sender, TaleWorlds.Library.ListChangedEventArgs e)
	{
		switch (e.ListChangedType)
		{
		case TaleWorlds.Library.ListChangedType.Reset:
		{
			for (int num = _widget_0_0_0.ChildCount - 1; num >= 0; num--)
			{
				Widget child3 = _widget_0_0_0.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child3).OnBeforeRemovedChild(child3);
				Widget child4 = _widget_0_0_0.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child4).SetDataSource(null);
				_widget_0_0_0.RemoveChild(child4);
			}
			break;
		}
		case TaleWorlds.Library.ListChangedType.Sorted:
		{
			for (int i = 0; i < _datasource_Root_EntryProperties.Count; i++)
			{
				MissionScoreboardHeaderItemVM bindingObject = _datasource_Root_EntryProperties[i];
				_widget_0_0_0.FindChild((Widget widget) => widget.GetComponent<GeneratedWidgetData>().Data == bindingObject).SetSiblingIndex(i);
			}
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemAdded:
		{
			MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate = new MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate(base.Context);
			GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate);
			MissionScoreboardHeaderItemVM dataSource = (MissionScoreboardHeaderItemVM)(generatedWidgetData.Data = _datasource_Root_EntryProperties[e.NewIndex]);
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.AddComponent(generatedWidgetData);
			_widget_0_0_0.AddChildAtIndex(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate, e.NewIndex);
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.CreateWidgets();
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetIds();
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetAttributes();
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetDataSource(dataSource);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemBeforeDeleted:
		{
			Widget child2 = _widget_0_0_0.GetChild(e.NewIndex);
			((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child2).OnBeforeRemovedChild(child2);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemDeleted:
		{
			Widget child = _widget_0_0_0.GetChild(e.NewIndex);
			((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child).SetDataSource(null);
			_widget_0_0_0.RemoveChild(child);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemChanged:
			break;
		}
	}

	public void OnList_datasource_Root_PlayersChanged(object sender, TaleWorlds.Library.ListChangedEventArgs e)
	{
		switch (e.ListChangedType)
		{
		case TaleWorlds.Library.ListChangedType.Reset:
		{
			for (int num = _widget_0_1.ChildCount - 1; num >= 0; num--)
			{
				Widget child3 = _widget_0_1.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child3).OnBeforeRemovedChild(child3);
				Widget child4 = _widget_0_1.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child4).SetDataSource(null);
				_widget_0_1.RemoveChild(child4);
			}
			break;
		}
		case TaleWorlds.Library.ListChangedType.Sorted:
		{
			for (int i = 0; i < _datasource_Root_Players.Count; i++)
			{
				MissionScoreboardPlayerVM bindingObject = _datasource_Root_Players[i];
				_widget_0_1.FindChild((Widget widget) => widget.GetComponent<GeneratedWidgetData>().Data == bindingObject).SetSiblingIndex(i);
			}
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemAdded:
		{
			MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate = new MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate(base.Context);
			GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate);
			MissionScoreboardPlayerVM dataSource = (MissionScoreboardPlayerVM)(generatedWidgetData.Data = _datasource_Root_Players[e.NewIndex]);
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.AddComponent(generatedWidgetData);
			_widget_0_1.AddChildAtIndex(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate, e.NewIndex);
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.CreateWidgets();
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetIds();
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetAttributes();
			multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetDataSource(dataSource);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemBeforeDeleted:
		{
			Widget child2 = _widget_0_1.GetChild(e.NewIndex);
			((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child2).OnBeforeRemovedChild(child2);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemDeleted:
		{
			Widget child = _widget_0_1.GetChild(e.NewIndex);
			((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child).SetDataSource(null);
			_widget_0_1.RemoveChild(child);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemChanged:
			break;
		}
	}

	private void RefreshDataSource_datasource_Root(MissionScoreboardSideVM newDataSource)
	{
		if (_datasource_Root != null)
		{
			_datasource_Root.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root;
			_widget.PropertyChanged -= PropertyChangedListenerOf_widget;
			_widget.boolPropertyChanged -= boolPropertyChangedListenerOf_widget;
			_widget.floatPropertyChanged -= floatPropertyChangedListenerOf_widget;
			_widget.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget;
			_widget.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget;
			_widget.doublePropertyChanged -= doublePropertyChangedListenerOf_widget;
			_widget.intPropertyChanged -= intPropertyChangedListenerOf_widget;
			_widget.uintPropertyChanged -= uintPropertyChangedListenerOf_widget;
			_widget.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget;
			if (_datasource_Root_EntryProperties != null)
			{
				_datasource_Root_EntryProperties.ListChanged -= OnList_datasource_Root_EntryPropertiesChanged;
				for (int num = _widget_0_0_0.ChildCount - 1; num >= 0; num--)
				{
					Widget child = _widget_0_0_0.GetChild(num);
					((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child).OnBeforeRemovedChild(child);
					Widget child2 = _widget_0_0_0.GetChild(num);
					((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child2).SetDataSource(null);
					_widget_0_0_0.RemoveChild(child2);
				}
				_datasource_Root_EntryProperties = null;
			}
			if (_datasource_Root_Players != null)
			{
				_datasource_Root_Players.ListChanged -= OnList_datasource_Root_PlayersChanged;
				for (int num2 = _widget_0_1.ChildCount - 1; num2 >= 0; num2--)
				{
					Widget child3 = _widget_0_1.GetChild(num2);
					((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child3).OnBeforeRemovedChild(child3);
					Widget child4 = _widget_0_1.GetChild(num2);
					((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child4).SetDataSource(null);
					_widget_0_1.RemoveChild(child4);
				}
				_datasource_Root_Players = null;
			}
			_datasource_Root = null;
		}
		_datasource_Root = newDataSource;
		if (_datasource_Root == null)
		{
			return;
		}
		_datasource_Root.PropertyChanged += ViewModelPropertyChangedListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithValue += ViewModelPropertyChangedWithValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithBoolValue += ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithIntValue += ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithFloatValue += ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithUIntValue += ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithColorValue += ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithDoubleValue += ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root;
		_datasource_Root.PropertyChangedWithVec2Value += ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root;
		_widget.CultureId = _datasource_Root.CultureId;
		_widget.CultureColor = _datasource_Root.CultureColor1;
		_widget.PropertyChanged += PropertyChangedListenerOf_widget;
		_widget.boolPropertyChanged += boolPropertyChangedListenerOf_widget;
		_widget.floatPropertyChanged += floatPropertyChangedListenerOf_widget;
		_widget.Vec2PropertyChanged += Vec2PropertyChangedListenerOf_widget;
		_widget.Vector2PropertyChanged += Vector2PropertyChangedListenerOf_widget;
		_widget.doublePropertyChanged += doublePropertyChangedListenerOf_widget;
		_widget.intPropertyChanged += intPropertyChangedListenerOf_widget;
		_widget.uintPropertyChanged += uintPropertyChangedListenerOf_widget;
		_widget.ColorPropertyChanged += ColorPropertyChangedListenerOf_widget;
		_datasource_Root_EntryProperties = _datasource_Root.EntryProperties;
		if (_datasource_Root_EntryProperties != null)
		{
			_datasource_Root_EntryProperties.ListChanged += OnList_datasource_Root_EntryPropertiesChanged;
			for (int i = 0; i < _datasource_Root_EntryProperties.Count; i++)
			{
				MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate = new MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate(base.Context);
				GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate);
				MissionScoreboardHeaderItemVM dataSource = (MissionScoreboardHeaderItemVM)(generatedWidgetData.Data = _datasource_Root_EntryProperties[i]);
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.AddComponent(generatedWidgetData);
				_widget_0_0_0.AddChildAtIndex(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate, i);
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.CreateWidgets();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetIds();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetAttributes();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetDataSource(dataSource);
			}
		}
		_datasource_Root_Players = _datasource_Root.Players;
		if (_datasource_Root_Players != null)
		{
			_datasource_Root_Players.ListChanged += OnList_datasource_Root_PlayersChanged;
			for (int j = 0; j < _datasource_Root_Players.Count; j++)
			{
				MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate = new MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate(base.Context);
				GeneratedWidgetData generatedWidgetData2 = new GeneratedWidgetData(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate);
				MissionScoreboardPlayerVM dataSource2 = (MissionScoreboardPlayerVM)(generatedWidgetData2.Data = _datasource_Root_Players[j]);
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.AddComponent(generatedWidgetData2);
				_widget_0_1.AddChildAtIndex(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate, j);
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.CreateWidgets();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetIds();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetAttributes();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetDataSource(dataSource2);
			}
		}
	}

	private void RefreshDataSource_datasource_Root_EntryProperties(MBBindingList<MissionScoreboardHeaderItemVM> newDataSource)
	{
		if (_datasource_Root_EntryProperties != null)
		{
			_datasource_Root_EntryProperties.ListChanged -= OnList_datasource_Root_EntryPropertiesChanged;
			for (int num = _widget_0_0_0.ChildCount - 1; num >= 0; num--)
			{
				Widget child = _widget_0_0_0.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child).OnBeforeRemovedChild(child);
				Widget child2 = _widget_0_0_0.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate)child2).SetDataSource(null);
				_widget_0_0_0.RemoveChild(child2);
			}
			_datasource_Root_EntryProperties = null;
		}
		_datasource_Root_EntryProperties = newDataSource;
		_datasource_Root_EntryProperties = _datasource_Root.EntryProperties;
		if (_datasource_Root_EntryProperties != null)
		{
			_datasource_Root_EntryProperties.ListChanged += OnList_datasource_Root_EntryPropertiesChanged;
			for (int i = 0; i < _datasource_Root_EntryProperties.Count; i++)
			{
				MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate = new MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate(base.Context);
				GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate);
				MissionScoreboardHeaderItemVM dataSource = (MissionScoreboardHeaderItemVM)(generatedWidgetData.Data = _datasource_Root_EntryProperties[i]);
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.AddComponent(generatedWidgetData);
				_widget_0_0_0.AddChildAtIndex(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate, i);
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.CreateWidgets();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetIds();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetAttributes();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_12_ItemTemplate.SetDataSource(dataSource);
			}
		}
	}

	private void RefreshDataSource_datasource_Root_Players(MBBindingList<MissionScoreboardPlayerVM> newDataSource)
	{
		if (_datasource_Root_Players != null)
		{
			_datasource_Root_Players.ListChanged -= OnList_datasource_Root_PlayersChanged;
			for (int num = _widget_0_1.ChildCount - 1; num >= 0; num--)
			{
				Widget child = _widget_0_1.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child).OnBeforeRemovedChild(child);
				Widget child2 = _widget_0_1.GetChild(num);
				((MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate)child2).SetDataSource(null);
				_widget_0_1.RemoveChild(child2);
			}
			_datasource_Root_Players = null;
		}
		_datasource_Root_Players = newDataSource;
		_datasource_Root_Players = _datasource_Root.Players;
		if (_datasource_Root_Players != null)
		{
			_datasource_Root_Players.ListChanged += OnList_datasource_Root_PlayersChanged;
			for (int i = 0; i < _datasource_Root_Players.Count; i++)
			{
				MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate = new MultiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate(base.Context);
				GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate);
				MissionScoreboardPlayerVM dataSource = (MissionScoreboardPlayerVM)(generatedWidgetData.Data = _datasource_Root_Players[i]);
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.AddComponent(generatedWidgetData);
				_widget_0_1.AddChildAtIndex(multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate, i);
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.CreateWidgets();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetIds();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetAttributes();
				multiplayerScoreboard__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_Scoreboard_MissionScoreboardVM_Dependency_13_ItemTemplate.SetDataSource(dataSource);
			}
		}
	}
}
