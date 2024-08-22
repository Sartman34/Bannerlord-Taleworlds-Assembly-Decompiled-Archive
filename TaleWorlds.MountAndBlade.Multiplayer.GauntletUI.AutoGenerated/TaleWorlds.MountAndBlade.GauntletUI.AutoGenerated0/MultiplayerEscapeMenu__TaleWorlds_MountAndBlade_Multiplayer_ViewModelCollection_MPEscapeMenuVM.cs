using System.ComponentModel;
using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace TaleWorlds.MountAndBlade.GauntletUI.AutoGenerated0;

public class MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM : Widget, IGeneratedGauntletMovieRoot
{
	private Widget _widget;

	private Widget _widget_0;

	private Widget _widget_1;

	private Widget _widget_2;

	private Widget _widget_2_0;

	private Widget _widget_2_0_0;

	private TextWidget _widget_2_1;

	private Widget _widget_2_2;

	private Widget _widget_2_2_0;

	private NavigationScopeTargeter _widget_2_3;

	private NavigatableListPanel _widget_2_4;

	private MPEscapeMenuVM _datasource_Root;

	private MBBindingList<EscapeMenuItemVM> _datasource_Root_MenuItems;

	public MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM(UIContext context)
		: base(context)
	{
	}

	public void CreateWidgets()
	{
		_widget = this;
		_widget_0 = new Widget(base.Context);
		_widget.AddChild(_widget_0);
		_widget_1 = new Widget(base.Context);
		_widget.AddChild(_widget_1);
		_widget_2 = new Widget(base.Context);
		_widget.AddChild(_widget_2);
		_widget_2_0 = new Widget(base.Context);
		_widget_2.AddChild(_widget_2_0);
		_widget_2_0_0 = new Widget(base.Context);
		_widget_2_0.AddChild(_widget_2_0_0);
		_widget_2_1 = new TextWidget(base.Context);
		_widget_2.AddChild(_widget_2_1);
		_widget_2_2 = new Widget(base.Context);
		_widget_2.AddChild(_widget_2_2);
		_widget_2_2_0 = new Widget(base.Context);
		_widget_2_2.AddChild(_widget_2_2_0);
		_widget_2_3 = new NavigationScopeTargeter(base.Context);
		_widget_2.AddChild(_widget_2_3);
		_widget_2_4 = new NavigatableListPanel(base.Context);
		_widget_2.AddChild(_widget_2_4);
	}

	public void SetIds()
	{
		_widget_2.Id = "MainPanel";
		_widget_2_4.Id = "ButtonsContainer";
	}

	public void SetAttributes()
	{
		base.WidthSizePolicy = SizePolicy.StretchToParent;
		base.HeightSizePolicy = SizePolicy.StretchToParent;
		base.Sprite = base.Context.SpriteData.GetSprite("BlankWhiteSquare_9");
		base.Color = new Color(0f, 0f, 0f);
		base.AlphaFactor = 0.4f;
		_widget_0.WidthSizePolicy = SizePolicy.Fixed;
		_widget_0.HeightSizePolicy = SizePolicy.StretchToParent;
		_widget_0.SuggestedWidth = 382f;
		_widget_0.Sprite = base.Context.SpriteData.GetSprite("escape_menu_gradient_9");
		_widget_1.WidthSizePolicy = SizePolicy.Fixed;
		_widget_1.HeightSizePolicy = SizePolicy.StretchToParent;
		_widget_1.SuggestedWidth = 382f;
		_widget_1.HorizontalAlignment = HorizontalAlignment.Right;
		_widget_1.Sprite = base.Context.SpriteData.GetSprite("escape_menu_gradient_9");
		_widget_1.HorizontalFlip = true;
		_widget_2.WidthSizePolicy = SizePolicy.Fixed;
		_widget_2.HeightSizePolicy = SizePolicy.StretchToParent;
		_widget_2.SuggestedWidth = 632f;
		_widget_2.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_2.VerticalAlignment = VerticalAlignment.Center;
		_widget_2.Sprite = base.Context.SpriteData.GetSprite("flat_panel_9");
		_widget_2_0.ClipContents = true;
		_widget_2_0.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_2_0.HeightSizePolicy = SizePolicy.Fixed;
		_widget_2_0.SuggestedHeight = 533f;
		_widget_2_0.MarginLeft = 60f;
		_widget_2_0.MarginRight = 60f;
		_widget_2_0.VerticalAlignment = VerticalAlignment.Bottom;
		_widget_2_0_0.WidthSizePolicy = SizePolicy.Fixed;
		_widget_2_0_0.HeightSizePolicy = SizePolicy.StretchToParent;
		_widget_2_0_0.SuggestedWidth = 1377f;
		_widget_2_0_0.Sprite = base.Context.SpriteData.GetSprite("StdAssets\\flat_panel_texture");
		_widget_2_1.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_2_1.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_2_1.MarginTop = 60f;
		_widget_2_1.Brush = base.Context.GetBrush("MPEscapeMenu.Title");
		_widget_2_2.ClipContents = true;
		_widget_2_2.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_2_2.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_2_2.MarginTop = 120f;
		_widget_2_2.MarginLeft = 60f;
		_widget_2_2.MarginRight = 60f;
		_widget_2_2.VerticalAlignment = VerticalAlignment.Top;
		_widget_2_2_0.WidthSizePolicy = SizePolicy.Fixed;
		_widget_2_2_0.HeightSizePolicy = SizePolicy.Fixed;
		_widget_2_2_0.SuggestedWidth = 664f;
		_widget_2_2_0.SuggestedHeight = 20f;
		_widget_2_2_0.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_2_2_0.Sprite = base.Context.SpriteData.GetSprite("MPTeamSelection\\divider_notched");
		_widget_2_3.ScopeID = "EscapeMenuScope";
		_widget_2_3.ScopeParent = _widget_2_4;
		_widget_2_3.ScopeMovements = GamepadNavigationTypes.Vertical;
		_widget_2_4.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;
		_widget_2_4.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_2_4.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_2_4.MarginTop = 200f;
		_widget_2_4.MarginBottom = 115f;
		_widget_2_4.MinIndex = 0;
		_widget_2_4.MaxIndex = 100;
	}

	public void RefreshBindingWithChildren()
	{
		MPEscapeMenuVM datasource_Root = _datasource_Root;
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
		_widget_2_1.PropertyChanged -= PropertyChangedListenerOf_widget_2_1;
		_widget_2_1.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_2_1;
		_widget_2_1.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_2_1;
		_widget_2_1.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_2_1;
		_widget_2_1.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_2_1;
		_widget_2_1.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_2_1;
		_widget_2_1.intPropertyChanged -= intPropertyChangedListenerOf_widget_2_1;
		_widget_2_1.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_2_1;
		_widget_2_1.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_2_1;
		if (_datasource_Root_MenuItems != null)
		{
			_datasource_Root_MenuItems.ListChanged -= OnList_datasource_Root_MenuItemsChanged;
			for (int num = _widget_2_4.ChildCount - 1; num >= 0; num--)
			{
				Widget child = _widget_2_4.GetChild(num);
				((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child).OnBeforeRemovedChild(child);
				((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)_widget_2_4.GetChild(num)).DestroyDataSource();
			}
			_datasource_Root_MenuItems = null;
		}
		_datasource_Root = null;
	}

	public void SetDataSource(MPEscapeMenuVM dataSource)
	{
		RefreshDataSource_datasource_Root(dataSource);
	}

	private void PropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, object e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void boolPropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, bool e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void floatPropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, float e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void Vec2PropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Vec2 e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void Vector2PropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Vector2 e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void doublePropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, double e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void intPropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, int e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void uintPropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, uint e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void ColorPropertyChangedListenerOf_widget_2_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Color e)
	{
		HandleWidgetPropertyChangeOf_widget_2_1(propertyName);
	}

	private void HandleWidgetPropertyChangeOf_widget_2_1(string propertyName)
	{
		if (propertyName == "Text")
		{
			_datasource_Root.Title = _widget_2_1.Text;
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
		if (propertyName == "MenuItems")
		{
			RefreshDataSource_datasource_Root_MenuItems(_datasource_Root.MenuItems);
		}
		else if (propertyName == "Title")
		{
			_widget_2_1.Text = _datasource_Root.Title;
		}
	}

	public void OnList_datasource_Root_MenuItemsChanged(object sender, TaleWorlds.Library.ListChangedEventArgs e)
	{
		switch (e.ListChangedType)
		{
		case TaleWorlds.Library.ListChangedType.Reset:
		{
			for (int num = _widget_2_4.ChildCount - 1; num >= 0; num--)
			{
				Widget child3 = _widget_2_4.GetChild(num);
				((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child3).OnBeforeRemovedChild(child3);
				Widget child4 = _widget_2_4.GetChild(num);
				((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child4).SetDataSource(null);
				_widget_2_4.RemoveChild(child4);
			}
			break;
		}
		case TaleWorlds.Library.ListChangedType.Sorted:
		{
			for (int i = 0; i < _datasource_Root_MenuItems.Count; i++)
			{
				EscapeMenuItemVM bindingObject = _datasource_Root_MenuItems[i];
				_widget_2_4.FindChild((Widget widget) => widget.GetComponent<GeneratedWidgetData>().Data == bindingObject).SetSiblingIndex(i);
			}
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemAdded:
		{
			MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate = new MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate(base.Context);
			GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate);
			EscapeMenuItemVM dataSource = (EscapeMenuItemVM)(generatedWidgetData.Data = _datasource_Root_MenuItems[e.NewIndex]);
			multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.AddComponent(generatedWidgetData);
			_widget_2_4.AddChildAtIndex(multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate, e.NewIndex);
			multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.CreateWidgets();
			multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetIds();
			multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetAttributes();
			multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetDataSource(dataSource);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemBeforeDeleted:
		{
			Widget child2 = _widget_2_4.GetChild(e.NewIndex);
			((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child2).OnBeforeRemovedChild(child2);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemDeleted:
		{
			Widget child = _widget_2_4.GetChild(e.NewIndex);
			((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child).SetDataSource(null);
			_widget_2_4.RemoveChild(child);
			break;
		}
		case TaleWorlds.Library.ListChangedType.ItemChanged:
			break;
		}
	}

	private void RefreshDataSource_datasource_Root(MPEscapeMenuVM newDataSource)
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
			_widget_2_1.PropertyChanged -= PropertyChangedListenerOf_widget_2_1;
			_widget_2_1.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_2_1;
			_widget_2_1.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_2_1;
			_widget_2_1.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_2_1;
			_widget_2_1.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_2_1;
			_widget_2_1.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_2_1;
			_widget_2_1.intPropertyChanged -= intPropertyChangedListenerOf_widget_2_1;
			_widget_2_1.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_2_1;
			_widget_2_1.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_2_1;
			if (_datasource_Root_MenuItems != null)
			{
				_datasource_Root_MenuItems.ListChanged -= OnList_datasource_Root_MenuItemsChanged;
				for (int num = _widget_2_4.ChildCount - 1; num >= 0; num--)
				{
					Widget child = _widget_2_4.GetChild(num);
					((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child).OnBeforeRemovedChild(child);
					Widget child2 = _widget_2_4.GetChild(num);
					((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child2).SetDataSource(null);
					_widget_2_4.RemoveChild(child2);
				}
				_datasource_Root_MenuItems = null;
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
		_widget_2_1.Text = _datasource_Root.Title;
		_widget_2_1.PropertyChanged += PropertyChangedListenerOf_widget_2_1;
		_widget_2_1.boolPropertyChanged += boolPropertyChangedListenerOf_widget_2_1;
		_widget_2_1.floatPropertyChanged += floatPropertyChangedListenerOf_widget_2_1;
		_widget_2_1.Vec2PropertyChanged += Vec2PropertyChangedListenerOf_widget_2_1;
		_widget_2_1.Vector2PropertyChanged += Vector2PropertyChangedListenerOf_widget_2_1;
		_widget_2_1.doublePropertyChanged += doublePropertyChangedListenerOf_widget_2_1;
		_widget_2_1.intPropertyChanged += intPropertyChangedListenerOf_widget_2_1;
		_widget_2_1.uintPropertyChanged += uintPropertyChangedListenerOf_widget_2_1;
		_widget_2_1.ColorPropertyChanged += ColorPropertyChangedListenerOf_widget_2_1;
		_datasource_Root_MenuItems = _datasource_Root.MenuItems;
		if (_datasource_Root_MenuItems != null)
		{
			_datasource_Root_MenuItems.ListChanged += OnList_datasource_Root_MenuItemsChanged;
			for (int i = 0; i < _datasource_Root_MenuItems.Count; i++)
			{
				MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate = new MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate(base.Context);
				GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate);
				EscapeMenuItemVM dataSource = (EscapeMenuItemVM)(generatedWidgetData.Data = _datasource_Root_MenuItems[i]);
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.AddComponent(generatedWidgetData);
				_widget_2_4.AddChildAtIndex(multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate, i);
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.CreateWidgets();
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetIds();
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetAttributes();
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetDataSource(dataSource);
			}
		}
	}

	private void RefreshDataSource_datasource_Root_MenuItems(MBBindingList<EscapeMenuItemVM> newDataSource)
	{
		if (_datasource_Root_MenuItems != null)
		{
			_datasource_Root_MenuItems.ListChanged -= OnList_datasource_Root_MenuItemsChanged;
			for (int num = _widget_2_4.ChildCount - 1; num >= 0; num--)
			{
				Widget child = _widget_2_4.GetChild(num);
				((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child).OnBeforeRemovedChild(child);
				Widget child2 = _widget_2_4.GetChild(num);
				((MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate)child2).SetDataSource(null);
				_widget_2_4.RemoveChild(child2);
			}
			_datasource_Root_MenuItems = null;
		}
		_datasource_Root_MenuItems = newDataSource;
		_datasource_Root_MenuItems = _datasource_Root.MenuItems;
		if (_datasource_Root_MenuItems != null)
		{
			_datasource_Root_MenuItems.ListChanged += OnList_datasource_Root_MenuItemsChanged;
			for (int i = 0; i < _datasource_Root_MenuItems.Count; i++)
			{
				MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate = new MultiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate(base.Context);
				GeneratedWidgetData generatedWidgetData = new GeneratedWidgetData(multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate);
				EscapeMenuItemVM dataSource = (EscapeMenuItemVM)(generatedWidgetData.Data = _datasource_Root_MenuItems[i]);
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.AddComponent(generatedWidgetData);
				_widget_2_4.AddChildAtIndex(multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate, i);
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.CreateWidgets();
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetIds();
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetAttributes();
				multiplayerEscapeMenu__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MPEscapeMenuVM_Dependency_1_ItemTemplate.SetDataSource(dataSource);
			}
		}
	}
}
