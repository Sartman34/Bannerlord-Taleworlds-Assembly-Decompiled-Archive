using System.ComponentModel;
using System.Numerics;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Information;

namespace TaleWorlds.MountAndBlade.GauntletUI.AutoGenerated0;

public class PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM : Widget, IGeneratedGauntletMovieRoot
{
	private Widget _widget;

	private PropertyBasedTooltipWidget _widget_0;

	private ListPanel _widget_0_0;

	private DimensionSyncWidget _widget_0_0_0;

	private Widget _widget_0_0_1;

	private ListPanel _widget_0_0_1_0;

	private DimensionSyncWidget _widget_0_0_2;

	private PropertyBasedTooltipVM _datasource_Root;

	private MBBindingList<TooltipProperty> _datasource_Root_TooltipPropertyList;

	public PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM(UIContext context)
		: base(context)
	{
	}

	public void CreateWidgets()
	{
		_widget = this;
		_widget_0 = new PropertyBasedTooltipWidget(base.Context);
		_widget.AddChild(_widget_0);
		_widget_0_0 = new ListPanel(base.Context);
		_widget_0.AddChild(_widget_0_0);
		_widget_0_0_0 = new DimensionSyncWidget(base.Context);
		_widget_0_0.AddChild(_widget_0_0_0);
		_widget_0_0_1 = new Widget(base.Context);
		_widget_0_0.AddChild(_widget_0_0_1);
		_widget_0_0_1_0 = new ListPanel(base.Context);
		_widget_0_0_1.AddChild(_widget_0_0_1_0);
		_widget_0_0_2 = new DimensionSyncWidget(base.Context);
		_widget_0_0.AddChild(_widget_0_0_2);
	}

	public void SetIds()
	{
		_widget_0.Id = "TooltipWidget";
		_widget_0_0.Id = "Body";
		_widget_0_0_1.Id = "PropertyListBackground";
		_widget_0_0_1_0.Id = "PropertyList";
	}

	public void SetAttributes()
	{
		base.WidthSizePolicy = SizePolicy.StretchToParent;
		base.HeightSizePolicy = SizePolicy.StretchToParent;
		base.IsDisabled = true;
		_widget_0.WidthSizePolicy = SizePolicy.CoverChildren;
		_widget_0.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0.NeutralColor = new Color(0f, 0f, 0f, 0.8509805f);
		_widget_0.AllyColor = new Color(0.02352941f, 0.09019608f, 0.01568628f, 0.8509805f);
		_widget_0.EnemyColor = new Color(0.08627451f, 0.02745098f, 0.01960784f, 0.8509805f);
		_widget_0.PropertyListBackground = _widget_0_0_1;
		_widget_0.PropertyList = _widget_0_0_1_0;
		_widget_0.EnemyTroopsTextBrush = base.Context.GetBrush("Tooltip.EnemyTroop.Text");
		_widget_0.AllyTroopsTextBrush = base.Context.GetBrush("Tooltip.AllyTroop.Text");
		_widget_0.NeutralTroopsTextBrush = base.Context.GetBrush("Tooltip.NeutralTroop.Text");
		_widget_0_0.WidthSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;
		_widget_0_0_0.WidthSizePolicy = SizePolicy.Fixed;
		_widget_0_0_0.HeightSizePolicy = SizePolicy.Fixed;
		_widget_0_0_0.SuggestedWidth = 220f;
		_widget_0_0_0.SuggestedHeight = 12f;
		_widget_0_0_0.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_0_0_0.PositionYOffset = 5f;
		_widget_0_0_0.DimensionToSync = DimensionSyncWidget.Dimensions.Horizontal;
		_widget_0_0_0.IsEnabled = false;
		_widget_0_0_0.PaddingAmount = 20;
		_widget_0_0_0.WidgetToCopyHeightFrom = _widget_0_0_1_0;
		_widget_0_0_0.Sprite = base.Context.SpriteData.GetSprite("General\\TooltipHint\\tooltip_frame");
		_widget_0_0_0.ExtendLeft = 4f;
		_widget_0_0_0.ExtendRight = 4f;
		_widget_0_0_1.WidthSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0_1.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0_1.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_0_0_1.VerticalAlignment = VerticalAlignment.Top;
		_widget_0_0_1.Sprite = base.Context.SpriteData.GetSprite("BlankWhiteSquare_9");
		_widget_0_0_1_0.WidthSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0_1_0.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0_1_0.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_0_0_1_0.VerticalAlignment = VerticalAlignment.Top;
		_widget_0_0_1_0.MarginTop = 5f;
		_widget_0_0_1_0.MarginBottom = 5f;
		_widget_0_0_1_0.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;
		_widget_0_0_2.WidthSizePolicy = SizePolicy.Fixed;
		_widget_0_0_2.HeightSizePolicy = SizePolicy.Fixed;
		_widget_0_0_2.SuggestedHeight = 12f;
		_widget_0_0_2.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_0_0_2.PositionYOffset = -5f;
		_widget_0_0_2.Sprite = base.Context.SpriteData.GetSprite("General\\TooltipHint\\tooltip_frame");
		_widget_0_0_2.ExtendLeft = 4f;
		_widget_0_0_2.ExtendRight = 4f;
		_widget_0_0_2.DimensionToSync = DimensionSyncWidget.Dimensions.Horizontal;
		_widget_0_0_2.IsEnabled = false;
		_widget_0_0_2.PaddingAmount = 20;
		_widget_0_0_2.WidgetToCopyHeightFrom = _widget_0_0_1_0;
	}

	public void RefreshBindingWithChildren()
	{
		PropertyBasedTooltipVM datasource_Root = _datasource_Root;
		SetDataSource(null);
		SetDataSource(datasource_Root);
	}

	public void DestroyDataSource()
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
		_widget_0.PropertyChanged -= PropertyChangedListenerOf_widget_0;
		_widget_0.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_0;
		_widget_0.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_0;
		_widget_0.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_0;
		_widget_0.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_0;
		_widget_0.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_0;
		_widget_0.intPropertyChanged -= intPropertyChangedListenerOf_widget_0;
		_widget_0.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_0;
		_widget_0.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_0;
		if (_datasource_Root_TooltipPropertyList != null)
		{
			_datasource_Root_TooltipPropertyList.ListChanged -= OnList_datasource_Root_TooltipPropertyListChanged;
			for (int num = _widget_0_0_1_0.ChildCount - 1; num >= 0; num--)
			{
				Widget child = _widget_0_0_1_0.GetChild(num);
				((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child).OnBeforeRemovedChild(child);
				((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)_widget_0_0_1_0.GetChild(num)).DestroyDataSource();
			}
			_datasource_Root_TooltipPropertyList = null;
		}
		_datasource_Root = null;
	}

	public void SetDataSource(PropertyBasedTooltipVM dataSource)
	{
		RefreshDataSource_datasource_Root(dataSource);
	}

	private void PropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, object e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void boolPropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, bool e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void floatPropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, float e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void Vec2PropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, Vec2 e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void Vector2PropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, Vector2 e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void doublePropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, double e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void intPropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, int e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void uintPropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, uint e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void ColorPropertyChangedListenerOf_widget_0(PropertyOwnerObject propertyOwnerObject, string propertyName, Color e)
	{
		HandleWidgetPropertyChangeOf_widget_0(propertyName);
	}

	private void HandleWidgetPropertyChangeOf_widget_0(string propertyName)
	{
		if (propertyName == "IsVisible")
		{
			_datasource_Root.IsActive = _widget_0.IsVisible;
		}
		else if (propertyName == "Mode")
		{
			_datasource_Root.Mode = _widget_0.Mode;
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
		case "TooltipPropertyList":
			RefreshDataSource_datasource_Root_TooltipPropertyList(_datasource_Root.TooltipPropertyList);
			break;
		case "IsActive":
			_widget_0.IsVisible = _datasource_Root.IsActive;
			break;
		case "Mode":
			_widget_0.Mode = _datasource_Root.Mode;
			break;
		}
	}

	public void OnList_datasource_Root_TooltipPropertyListChanged(object sender, TaleWorlds.Library.ListChangedEventArgs e)
	{
		switch (e.ListChangedType)
		{
		case TaleWorlds.Library.ListChangedType.Reset:
		{
			for (int num = _widget_0_0_1_0.ChildCount - 1; num >= 0; num--)
			{
				Widget child3 = _widget_0_0_1_0.GetChild(num);
				((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child3).OnBeforeRemovedChild(child3);
				Widget child4 = _widget_0_0_1_0.GetChild(num);
				((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child4).SetDataSource(null);
				_widget_0_0_1_0.RemoveChild(child4);
			}
			break;
		}
		case TaleWorlds.Library.ListChangedType.Sorted:
		{
			for (int i = 0; i < _datasource_Root_TooltipPropertyList.Count; i++)
			{
				TooltipProperty bindingObject = _datasource_Root_TooltipPropertyList[i];
				_widget_0_0_1_0.FindChild((Widget widget) => widget.GetComponent<GeneratedWidgetData>().Data == bindingObject).SetSiblingIndex(i);
			}
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemAdded:
		{
			PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate = new PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate(base.Context);
			GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate);
			TooltipProperty dataSource = (TooltipProperty)(generatedWidgetData.Data = _datasource_Root_TooltipPropertyList[e.NewIndex]);
			propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.AddComponent(generatedWidgetData);
			_widget_0_0_1_0.AddChildAtIndex(propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate, e.NewIndex);
			propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.CreateWidgets();
			propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetIds();
			propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetAttributes();
			propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetDataSource(dataSource);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemBeforeDeleted:
		{
			Widget child2 = _widget_0_0_1_0.GetChild(e.NewIndex);
			((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child2).OnBeforeRemovedChild(child2);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemDeleted:
		{
			Widget child = _widget_0_0_1_0.GetChild(e.NewIndex);
			((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child).SetDataSource(null);
			_widget_0_0_1_0.RemoveChild(child);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemChanged:
			break;
		}
	}

	private void RefreshDataSource_datasource_Root(PropertyBasedTooltipVM newDataSource)
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
			_widget_0.PropertyChanged -= PropertyChangedListenerOf_widget_0;
			_widget_0.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_0;
			_widget_0.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_0;
			_widget_0.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_0;
			_widget_0.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_0;
			_widget_0.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_0;
			_widget_0.intPropertyChanged -= intPropertyChangedListenerOf_widget_0;
			_widget_0.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_0;
			_widget_0.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_0;
			if (_datasource_Root_TooltipPropertyList != null)
			{
				_datasource_Root_TooltipPropertyList.ListChanged -= OnList_datasource_Root_TooltipPropertyListChanged;
				for (int num = _widget_0_0_1_0.ChildCount - 1; num >= 0; num--)
				{
					Widget child = _widget_0_0_1_0.GetChild(num);
					((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child).OnBeforeRemovedChild(child);
					Widget child2 = _widget_0_0_1_0.GetChild(num);
					((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child2).SetDataSource(null);
					_widget_0_0_1_0.RemoveChild(child2);
				}
				_datasource_Root_TooltipPropertyList = null;
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
		_widget_0.IsVisible = _datasource_Root.IsActive;
		_widget_0.Mode = _datasource_Root.Mode;
		_widget_0.PropertyChanged += PropertyChangedListenerOf_widget_0;
		_widget_0.boolPropertyChanged += boolPropertyChangedListenerOf_widget_0;
		_widget_0.floatPropertyChanged += floatPropertyChangedListenerOf_widget_0;
		_widget_0.Vec2PropertyChanged += Vec2PropertyChangedListenerOf_widget_0;
		_widget_0.Vector2PropertyChanged += Vector2PropertyChangedListenerOf_widget_0;
		_widget_0.doublePropertyChanged += doublePropertyChangedListenerOf_widget_0;
		_widget_0.intPropertyChanged += intPropertyChangedListenerOf_widget_0;
		_widget_0.uintPropertyChanged += uintPropertyChangedListenerOf_widget_0;
		_widget_0.ColorPropertyChanged += ColorPropertyChangedListenerOf_widget_0;
		_datasource_Root_TooltipPropertyList = _datasource_Root.TooltipPropertyList;
		if (_datasource_Root_TooltipPropertyList != null)
		{
			_datasource_Root_TooltipPropertyList.ListChanged += OnList_datasource_Root_TooltipPropertyListChanged;
			for (int i = 0; i < _datasource_Root_TooltipPropertyList.Count; i++)
			{
				PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate = new PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate(base.Context);
				GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate);
				TooltipProperty dataSource = (TooltipProperty)(generatedWidgetData.Data = _datasource_Root_TooltipPropertyList[i]);
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.AddComponent(generatedWidgetData);
				_widget_0_0_1_0.AddChildAtIndex(propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate, i);
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.CreateWidgets();
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetIds();
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetAttributes();
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetDataSource(dataSource);
			}
		}
	}

	private void RefreshDataSource_datasource_Root_TooltipPropertyList(MBBindingList<TooltipProperty> newDataSource)
	{
		if (_datasource_Root_TooltipPropertyList != null)
		{
			_datasource_Root_TooltipPropertyList.ListChanged -= OnList_datasource_Root_TooltipPropertyListChanged;
			for (int num = _widget_0_0_1_0.ChildCount - 1; num >= 0; num--)
			{
				Widget child = _widget_0_0_1_0.GetChild(num);
				((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child).OnBeforeRemovedChild(child);
				Widget child2 = _widget_0_0_1_0.GetChild(num);
				((PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate)child2).SetDataSource(null);
				_widget_0_0_1_0.RemoveChild(child2);
			}
			_datasource_Root_TooltipPropertyList = null;
		}
		_datasource_Root_TooltipPropertyList = newDataSource;
		_datasource_Root_TooltipPropertyList = _datasource_Root.TooltipPropertyList;
		if (_datasource_Root_TooltipPropertyList != null)
		{
			_datasource_Root_TooltipPropertyList.ListChanged += OnList_datasource_Root_TooltipPropertyListChanged;
			for (int i = 0; i < _datasource_Root_TooltipPropertyList.Count; i++)
			{
				PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate = new PropertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate(base.Context);
				GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate);
				TooltipProperty dataSource = (TooltipProperty)(generatedWidgetData.Data = _datasource_Root_TooltipPropertyList[i]);
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.AddComponent(generatedWidgetData);
				_widget_0_0_1_0.AddChildAtIndex(propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate, i);
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.CreateWidgets();
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetIds();
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetAttributes();
				propertyBasedTooltip__TaleWorlds_Core_ViewModelCollection_Information_PropertyBasedTooltipVM_Dependency_1_ItemTemplate.SetDataSource(dataSource);
			}
		}
	}
}
