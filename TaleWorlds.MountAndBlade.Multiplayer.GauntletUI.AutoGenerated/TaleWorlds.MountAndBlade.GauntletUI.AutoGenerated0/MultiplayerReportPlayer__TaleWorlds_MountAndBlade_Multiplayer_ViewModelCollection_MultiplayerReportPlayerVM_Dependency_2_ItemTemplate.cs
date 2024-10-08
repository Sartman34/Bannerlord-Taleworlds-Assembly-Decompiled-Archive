using System.ComponentModel;
using System.Numerics;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;

namespace TaleWorlds.MountAndBlade.GauntletUI.AutoGenerated0;

public class MultiplayerReportPlayer__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MultiplayerReportPlayerVM_Dependency_2_ItemTemplate : ButtonWidget
{
	private ButtonWidget _widget;

	private ImageWidget _widget_0;

	private ScrollingRichTextWidget _widget_1;

	private HintWidget _widget_2;

	private SelectorItemVM _datasource_Root;

	private HintViewModel _datasource_Root_Hint;

	public MultiplayerReportPlayer__TaleWorlds_MountAndBlade_Multiplayer_ViewModelCollection_MultiplayerReportPlayerVM_Dependency_2_ItemTemplate(UIContext context)
		: base(context)
	{
	}

	public void CreateWidgets()
	{
		_widget = this;
		_widget_0 = new ImageWidget(base.Context);
		_widget.AddChild(_widget_0);
		_widget_1 = new ScrollingRichTextWidget(base.Context);
		_widget.AddChild(_widget_1);
		_widget_2 = new HintWidget(base.Context);
		_widget.AddChild(_widget_2);
	}

	public void SetIds()
	{
		base.Id = "DropdownItemButton";
	}

	public void SetAttributes()
	{
		base.DoNotUseCustomScale = true;
		base.DoNotPassEventsToChildren = true;
		base.WidthSizePolicy = SizePolicy.StretchToParent;
		base.HeightSizePolicy = SizePolicy.Fixed;
		base.SuggestedHeight = 29f;
		base.MarginLeft = 10f;
		base.MarginRight = 10f;
		base.HorizontalAlignment = HorizontalAlignment.Center;
		base.VerticalAlignment = VerticalAlignment.Bottom;
		base.ButtonType = ButtonType.Radio;
		base.UpdateChildrenStates = true;
		base.Brush = base.Context.GetBrush("Standard.DropdownItem.SoundBrush");
		_widget_0.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_0.HeightSizePolicy = SizePolicy.StretchToParent;
		_widget_0.MarginLeft = 5f;
		_widget_0.MarginRight = 5f;
		_widget_0.Brush = base.Context.GetBrush("Standard.DropdownItem.Flat");
		_widget_1.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_1.HeightSizePolicy = SizePolicy.StretchToParent;
		_widget_1.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_1.MarginLeft = 7f;
		_widget_1.MarginRight = 7f;
		_widget_1.VerticalAlignment = VerticalAlignment.Center;
		_widget_1.Brush = base.Context.GetBrush("Standard.DropdownItem.Text");
		_widget_1.IsAutoScrolling = false;
		_widget_1.ScrollOnHoverWidget = _widget.FindChild(new BindingPath("..\\DropdownItemButton"));
		_widget_2.DoNotAcceptEvents = true;
		_widget_2.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_2.HeightSizePolicy = SizePolicy.StretchToParent;
	}

	public void DestroyDataSource()
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
			_widget_1.PropertyChanged -= PropertyChangedListenerOf_widget_1;
			_widget_1.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_1;
			_widget_1.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_1;
			_widget_1.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_1;
			_widget_1.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_1;
			_widget_1.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_1;
			_widget_1.intPropertyChanged -= intPropertyChangedListenerOf_widget_1;
			_widget_1.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_1;
			_widget_1.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_1;
			if (_datasource_Root_Hint != null)
			{
				_datasource_Root_Hint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_Hint;
				_widget_2.EventFire -= EventListenerOf_widget_2;
				_datasource_Root_Hint = null;
			}
			_datasource_Root = null;
		}
	}

	public void SetDataSource(SelectorItemVM dataSource)
	{
		RefreshDataSource_datasource_Root(dataSource);
	}

	private void EventListenerOf_widget_2(Widget widget, string commandName, object[] args)
	{
		if (commandName == "HoverBegin")
		{
			_datasource_Root_Hint.ExecuteBeginHint();
		}
		if (commandName == "HoverEnd")
		{
			_datasource_Root_Hint.ExecuteEndHint();
		}
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
		if (propertyName == "IsEnabled")
		{
			_datasource_Root.CanBeSelected = _widget.IsEnabled;
		}
	}

	private void PropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, object e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void boolPropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, bool e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void floatPropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, float e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void Vec2PropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Vec2 e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void Vector2PropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Vector2 e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void doublePropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, double e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void intPropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, int e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void uintPropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, uint e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void ColorPropertyChangedListenerOf_widget_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Color e)
	{
		HandleWidgetPropertyChangeOf_widget_1(propertyName);
	}

	private void HandleWidgetPropertyChangeOf_widget_1(string propertyName)
	{
		if (propertyName == "Text")
		{
			_datasource_Root.StringItem = _widget_1.Text;
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
		case "Hint":
			RefreshDataSource_datasource_Root_Hint(_datasource_Root.Hint);
			break;
		case "CanBeSelected":
			_widget.IsEnabled = _datasource_Root.CanBeSelected;
			break;
		case "StringItem":
			_widget_1.Text = _datasource_Root.StringItem;
			break;
		}
	}

	private void ViewModelPropertyChangedListenerOf_datasource_Root_Hint(object sender, PropertyChangedEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithValueListenerOf_datasource_Root_Hint(object sender, PropertyChangedWithValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_Hint(object sender, PropertyChangedWithBoolValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_Hint(object sender, PropertyChangedWithIntValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_Hint(object sender, PropertyChangedWithFloatValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_Hint(object sender, PropertyChangedWithUIntValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_Hint(object sender, PropertyChangedWithColorValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_Hint(object sender, PropertyChangedWithDoubleValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_Hint(object sender, PropertyChangedWithVec2ValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_Hint(e.PropertyName);
	}

	private void HandleViewModelPropertyChangeOf_datasource_Root_Hint(string propertyName)
	{
	}

	private void RefreshDataSource_datasource_Root(SelectorItemVM newDataSource)
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
			_widget_1.PropertyChanged -= PropertyChangedListenerOf_widget_1;
			_widget_1.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_1;
			_widget_1.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_1;
			_widget_1.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_1;
			_widget_1.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_1;
			_widget_1.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_1;
			_widget_1.intPropertyChanged -= intPropertyChangedListenerOf_widget_1;
			_widget_1.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_1;
			_widget_1.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_1;
			if (_datasource_Root_Hint != null)
			{
				_datasource_Root_Hint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_Hint;
				_widget_2.EventFire -= EventListenerOf_widget_2;
				_datasource_Root_Hint = null;
			}
			_datasource_Root = null;
		}
		_datasource_Root = newDataSource;
		if (_datasource_Root != null)
		{
			_datasource_Root.PropertyChanged += ViewModelPropertyChangedListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithValue += ViewModelPropertyChangedWithValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithBoolValue += ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithIntValue += ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithFloatValue += ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithUIntValue += ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithColorValue += ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithDoubleValue += ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root;
			_datasource_Root.PropertyChangedWithVec2Value += ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root;
			_widget.IsEnabled = _datasource_Root.CanBeSelected;
			_widget.PropertyChanged += PropertyChangedListenerOf_widget;
			_widget.boolPropertyChanged += boolPropertyChangedListenerOf_widget;
			_widget.floatPropertyChanged += floatPropertyChangedListenerOf_widget;
			_widget.Vec2PropertyChanged += Vec2PropertyChangedListenerOf_widget;
			_widget.Vector2PropertyChanged += Vector2PropertyChangedListenerOf_widget;
			_widget.doublePropertyChanged += doublePropertyChangedListenerOf_widget;
			_widget.intPropertyChanged += intPropertyChangedListenerOf_widget;
			_widget.uintPropertyChanged += uintPropertyChangedListenerOf_widget;
			_widget.ColorPropertyChanged += ColorPropertyChangedListenerOf_widget;
			_widget_1.Text = _datasource_Root.StringItem;
			_widget_1.PropertyChanged += PropertyChangedListenerOf_widget_1;
			_widget_1.boolPropertyChanged += boolPropertyChangedListenerOf_widget_1;
			_widget_1.floatPropertyChanged += floatPropertyChangedListenerOf_widget_1;
			_widget_1.Vec2PropertyChanged += Vec2PropertyChangedListenerOf_widget_1;
			_widget_1.Vector2PropertyChanged += Vector2PropertyChangedListenerOf_widget_1;
			_widget_1.doublePropertyChanged += doublePropertyChangedListenerOf_widget_1;
			_widget_1.intPropertyChanged += intPropertyChangedListenerOf_widget_1;
			_widget_1.uintPropertyChanged += uintPropertyChangedListenerOf_widget_1;
			_widget_1.ColorPropertyChanged += ColorPropertyChangedListenerOf_widget_1;
			_datasource_Root_Hint = _datasource_Root.Hint;
			if (_datasource_Root_Hint != null)
			{
				_datasource_Root_Hint.PropertyChanged += ViewModelPropertyChangedListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithValue += ViewModelPropertyChangedWithValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithBoolValue += ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithIntValue += ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithFloatValue += ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithUIntValue += ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithColorValue += ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithDoubleValue += ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_Hint;
				_datasource_Root_Hint.PropertyChangedWithVec2Value += ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_Hint;
				_widget_2.EventFire += EventListenerOf_widget_2;
			}
		}
	}

	private void RefreshDataSource_datasource_Root_Hint(HintViewModel newDataSource)
	{
		if (_datasource_Root_Hint != null)
		{
			_datasource_Root_Hint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_Hint;
			_widget_2.EventFire -= EventListenerOf_widget_2;
			_datasource_Root_Hint = null;
		}
		_datasource_Root_Hint = newDataSource;
		_datasource_Root_Hint = _datasource_Root.Hint;
		if (_datasource_Root_Hint != null)
		{
			_datasource_Root_Hint.PropertyChanged += ViewModelPropertyChangedListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithValue += ViewModelPropertyChangedWithValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithBoolValue += ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithIntValue += ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithFloatValue += ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithUIntValue += ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithColorValue += ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithDoubleValue += ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_Hint;
			_datasource_Root_Hint.PropertyChangedWithVec2Value += ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_Hint;
			_widget_2.EventFire += EventListenerOf_widget_2;
		}
	}
}
