using System.ComponentModel;
using System.Numerics;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.MountAndBlade.ViewModelCollection.InitialMenu;

namespace TaleWorlds.MountAndBlade.GauntletUI.AutoGenerated1;

public class InitialScreen__TaleWorlds_MountAndBlade_ViewModelCollection_InitialMenu_InitialMenuVM_Dependency_1_ItemTemplate : Widget
{
	private Widget _widget;

	private ButtonWidget _widget_0;

	private ListPanel _widget_0_0;

	private ImageWidget _widget_0_0_0;

	private RichTextWidget _widget_0_0_1;

	private ImageWidget _widget_0_0_2;

	private HintWidget _widget_0_1;

	private HintWidget _widget_1;

	private InitialMenuOptionVM _datasource_Root;

	private HintViewModel _datasource_Root_EnabledHint;

	private HintViewModel _datasource_Root_DisabledHint;

	public InitialScreen__TaleWorlds_MountAndBlade_ViewModelCollection_InitialMenu_InitialMenuVM_Dependency_1_ItemTemplate(UIContext context)
		: base(context)
	{
	}

	private VisualDefinition CreateVisualDefinitionMainMenu()
	{
		VisualDefinition visualDefinition = new VisualDefinition("MainMenu", 0.4f, 0f, easeIn: true);
		visualDefinition.AddVisualState(new VisualState("Default")
		{
			PositionYOffset = 0f
		});
		visualDefinition.AddVisualState(new VisualState("Activated")
		{
			PositionYOffset = 0f
		});
		return visualDefinition;
	}

	private VisualDefinition CreateVisualDefinitionOptionFadeIn()
	{
		VisualDefinition visualDefinition = new VisualDefinition("OptionFadeIn", 0.3f, 0f, easeIn: true);
		visualDefinition.AddVisualState(new VisualState("Default")
		{
			PositionXOffset = -20f
		});
		visualDefinition.AddVisualState(new VisualState("Activated")
		{
			PositionXOffset = 0f
		});
		return visualDefinition;
	}

	public void CreateWidgets()
	{
		_widget = this;
		_widget_0 = new ButtonWidget(base.Context);
		_widget.AddChild(_widget_0);
		_widget_0_0 = new ListPanel(base.Context);
		_widget_0.AddChild(_widget_0_0);
		_widget_0_0_0 = new ImageWidget(base.Context);
		_widget_0_0.AddChild(_widget_0_0_0);
		_widget_0_0_1 = new RichTextWidget(base.Context);
		_widget_0_0.AddChild(_widget_0_0_1);
		_widget_0_0_2 = new ImageWidget(base.Context);
		_widget_0_0.AddChild(_widget_0_0_2);
		_widget_0_1 = new HintWidget(base.Context);
		_widget_0.AddChild(_widget_0_1);
		_widget_1 = new HintWidget(base.Context);
		_widget.AddChild(_widget_1);
	}

	public void SetIds()
	{
	}

	public void SetAttributes()
	{
		base.WidthSizePolicy = SizePolicy.StretchToParent;
		base.HeightSizePolicy = SizePolicy.CoverChildren;
		base.HorizontalAlignment = HorizontalAlignment.Center;
		base.MarginTop = 16f;
		base.MarginBottom = 16f;
		_widget_0.DoNotPassEventsToChildren = true;
		_widget_0.UpdateChildrenStates = true;
		_widget_0.WidthSizePolicy = SizePolicy.StretchToParent;
		_widget_0.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_0.VerticalAlignment = VerticalAlignment.Center;
		_widget_0_0.UpdateChildrenStates = true;
		_widget_0_0.WidthSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0.StackLayout.LayoutMethod = LayoutMethod.HorizontalCentered;
		_widget_0_0.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_0_0.VerticalAlignment = VerticalAlignment.Center;
		_widget_0_0_0.WidthSizePolicy = SizePolicy.Fixed;
		_widget_0_0_0.HeightSizePolicy = SizePolicy.Fixed;
		_widget_0_0_0.SuggestedWidth = 46f;
		_widget_0_0_0.SuggestedHeight = 20f;
		_widget_0_0_0.HorizontalAlignment = HorizontalAlignment.Left;
		_widget_0_0_0.VerticalAlignment = VerticalAlignment.Center;
		_widget_0_0_0.PositionYOffset = -2f;
		_widget_0_0_0.Brush = base.Context.GetBrush("HoverIndicatorBrush");
		_widget_0_0_1.WidthSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0_1.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0_0_1.MaxWidth = 320f;
		_widget_0_0_1.HorizontalAlignment = HorizontalAlignment.Center;
		_widget_0_0_1.MarginLeft = 8f;
		_widget_0_0_1.MarginRight = 8f;
		_widget_0_0_1.Brush = base.Context.GetBrush("InitialMenuButtonBrush");
		_widget_0_0_1.ClipContents = false;
		_widget_0_0_1.CanBreakWords = false;
		_widget_0_0_2.WidthSizePolicy = SizePolicy.Fixed;
		_widget_0_0_2.HeightSizePolicy = SizePolicy.Fixed;
		_widget_0_0_2.SuggestedWidth = 46f;
		_widget_0_0_2.SuggestedHeight = 20f;
		_widget_0_0_2.HorizontalAlignment = HorizontalAlignment.Right;
		_widget_0_0_2.VerticalAlignment = VerticalAlignment.Center;
		_widget_0_0_2.PositionYOffset = -2f;
		_widget_0_0_2.Brush = base.Context.GetBrush("HoverIndicatorBrushFlipped");
		_widget_0_1.WidthSizePolicy = SizePolicy.CoverChildren;
		_widget_0_1.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_0_1.IsDisabled = true;
		_widget_1.WidthSizePolicy = SizePolicy.CoverChildren;
		_widget_1.HeightSizePolicy = SizePolicy.CoverChildren;
		_widget_1.IsDisabled = true;
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
			_widget_0.EventFire -= EventListenerOf_widget_0;
			_widget_0.PropertyChanged -= PropertyChangedListenerOf_widget_0;
			_widget_0.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_0;
			_widget_0.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_0;
			_widget_0.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_0;
			_widget_0.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_0;
			_widget_0.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_0;
			_widget_0.intPropertyChanged -= intPropertyChangedListenerOf_widget_0;
			_widget_0.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_0;
			_widget_0.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_0;
			_widget_0_0_1.PropertyChanged -= PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.intPropertyChanged -= intPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_0_0_1;
			if (_datasource_Root_EnabledHint != null)
			{
				_datasource_Root_EnabledHint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_EnabledHint;
				_widget_0_1.EventFire -= EventListenerOf_widget_0_1;
				_datasource_Root_EnabledHint = null;
			}
			if (_datasource_Root_DisabledHint != null)
			{
				_datasource_Root_DisabledHint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_DisabledHint;
				_widget_1.EventFire -= EventListenerOf_widget_1;
				_datasource_Root_DisabledHint = null;
			}
			_datasource_Root = null;
		}
	}

	public void SetDataSource(InitialMenuOptionVM dataSource)
	{
		RefreshDataSource_datasource_Root(dataSource);
	}

	private void EventListenerOf_widget_0(Widget widget, string commandName, object[] args)
	{
		if (commandName == "Click")
		{
			_datasource_Root.ExecuteAction();
		}
	}

	private void EventListenerOf_widget_0_1(Widget widget, string commandName, object[] args)
	{
		if (commandName == "HoverBegin")
		{
			_datasource_Root_EnabledHint.ExecuteBeginHint();
		}
		if (commandName == "HoverEnd")
		{
			_datasource_Root_EnabledHint.ExecuteEndHint();
		}
	}

	private void EventListenerOf_widget_1(Widget widget, string commandName, object[] args)
	{
		if (commandName == "HoverBegin")
		{
			_datasource_Root_DisabledHint.ExecuteBeginHint();
		}
		if (commandName == "HoverEnd")
		{
			_datasource_Root_DisabledHint.ExecuteEndHint();
		}
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
		_ = propertyName == "IsDisabled";
	}

	private void PropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, object e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void boolPropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, bool e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void floatPropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, float e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void Vec2PropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Vec2 e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void Vector2PropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Vector2 e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void doublePropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, double e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void intPropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, int e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void uintPropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, uint e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void ColorPropertyChangedListenerOf_widget_0_0_1(PropertyOwnerObject propertyOwnerObject, string propertyName, Color e)
	{
		HandleWidgetPropertyChangeOf_widget_0_0_1(propertyName);
	}

	private void HandleWidgetPropertyChangeOf_widget_0_0_1(string propertyName)
	{
		_ = propertyName == "Text";
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
		case "EnabledHint":
			RefreshDataSource_datasource_Root_EnabledHint(_datasource_Root.EnabledHint);
			break;
		case "DisabledHint":
			RefreshDataSource_datasource_Root_DisabledHint(_datasource_Root.DisabledHint);
			break;
		case "IsDisabled":
			_widget_0.IsDisabled = _datasource_Root.IsDisabled;
			break;
		case "NameText":
			_widget_0_0_1.Text = _datasource_Root.NameText;
			break;
		}
	}

	private void ViewModelPropertyChangedListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithValueListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedWithValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedWithBoolValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedWithIntValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedWithFloatValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedWithUIntValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedWithColorValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedWithDoubleValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_EnabledHint(object sender, PropertyChangedWithVec2ValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(e.PropertyName);
	}

	private void HandleViewModelPropertyChangeOf_datasource_Root_EnabledHint(string propertyName)
	{
	}

	private void ViewModelPropertyChangedListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithValueListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedWithValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedWithBoolValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedWithIntValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedWithFloatValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedWithUIntValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedWithColorValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedWithDoubleValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_DisabledHint(object sender, PropertyChangedWithVec2ValueEventArgs e)
	{
		HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(e.PropertyName);
	}

	private void HandleViewModelPropertyChangeOf_datasource_Root_DisabledHint(string propertyName)
	{
	}

	private void RefreshDataSource_datasource_Root(InitialMenuOptionVM newDataSource)
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
			_widget_0.EventFire -= EventListenerOf_widget_0;
			_widget_0.PropertyChanged -= PropertyChangedListenerOf_widget_0;
			_widget_0.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_0;
			_widget_0.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_0;
			_widget_0.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_0;
			_widget_0.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_0;
			_widget_0.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_0;
			_widget_0.intPropertyChanged -= intPropertyChangedListenerOf_widget_0;
			_widget_0.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_0;
			_widget_0.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_0;
			_widget_0_0_1.PropertyChanged -= PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.boolPropertyChanged -= boolPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.floatPropertyChanged -= floatPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.Vec2PropertyChanged -= Vec2PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.Vector2PropertyChanged -= Vector2PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.doublePropertyChanged -= doublePropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.intPropertyChanged -= intPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.uintPropertyChanged -= uintPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.ColorPropertyChanged -= ColorPropertyChangedListenerOf_widget_0_0_1;
			if (_datasource_Root_EnabledHint != null)
			{
				_datasource_Root_EnabledHint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_EnabledHint;
				_widget_0_1.EventFire -= EventListenerOf_widget_0_1;
				_datasource_Root_EnabledHint = null;
			}
			if (_datasource_Root_DisabledHint != null)
			{
				_datasource_Root_DisabledHint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_DisabledHint;
				_widget_1.EventFire -= EventListenerOf_widget_1;
				_datasource_Root_DisabledHint = null;
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
			_widget_0.IsDisabled = _datasource_Root.IsDisabled;
			_widget_0.EventFire += EventListenerOf_widget_0;
			_widget_0.PropertyChanged += PropertyChangedListenerOf_widget_0;
			_widget_0.boolPropertyChanged += boolPropertyChangedListenerOf_widget_0;
			_widget_0.floatPropertyChanged += floatPropertyChangedListenerOf_widget_0;
			_widget_0.Vec2PropertyChanged += Vec2PropertyChangedListenerOf_widget_0;
			_widget_0.Vector2PropertyChanged += Vector2PropertyChangedListenerOf_widget_0;
			_widget_0.doublePropertyChanged += doublePropertyChangedListenerOf_widget_0;
			_widget_0.intPropertyChanged += intPropertyChangedListenerOf_widget_0;
			_widget_0.uintPropertyChanged += uintPropertyChangedListenerOf_widget_0;
			_widget_0.ColorPropertyChanged += ColorPropertyChangedListenerOf_widget_0;
			_widget_0_0_1.Text = _datasource_Root.NameText;
			_widget_0_0_1.PropertyChanged += PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.boolPropertyChanged += boolPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.floatPropertyChanged += floatPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.Vec2PropertyChanged += Vec2PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.Vector2PropertyChanged += Vector2PropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.doublePropertyChanged += doublePropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.intPropertyChanged += intPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.uintPropertyChanged += uintPropertyChangedListenerOf_widget_0_0_1;
			_widget_0_0_1.ColorPropertyChanged += ColorPropertyChangedListenerOf_widget_0_0_1;
			_datasource_Root_EnabledHint = _datasource_Root.EnabledHint;
			if (_datasource_Root_EnabledHint != null)
			{
				_datasource_Root_EnabledHint.PropertyChanged += ViewModelPropertyChangedListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithValue += ViewModelPropertyChangedWithValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithBoolValue += ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithIntValue += ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithFloatValue += ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithUIntValue += ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithColorValue += ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithDoubleValue += ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_EnabledHint;
				_datasource_Root_EnabledHint.PropertyChangedWithVec2Value += ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_EnabledHint;
				_widget_0_1.EventFire += EventListenerOf_widget_0_1;
			}
			_datasource_Root_DisabledHint = _datasource_Root.DisabledHint;
			if (_datasource_Root_DisabledHint != null)
			{
				_datasource_Root_DisabledHint.PropertyChanged += ViewModelPropertyChangedListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithValue += ViewModelPropertyChangedWithValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithBoolValue += ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithIntValue += ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithFloatValue += ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithUIntValue += ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithColorValue += ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithDoubleValue += ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_DisabledHint;
				_datasource_Root_DisabledHint.PropertyChangedWithVec2Value += ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_DisabledHint;
				_widget_1.EventFire += EventListenerOf_widget_1;
			}
		}
	}

	private void RefreshDataSource_datasource_Root_EnabledHint(HintViewModel newDataSource)
	{
		if (_datasource_Root_EnabledHint != null)
		{
			_datasource_Root_EnabledHint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_EnabledHint;
			_widget_0_1.EventFire -= EventListenerOf_widget_0_1;
			_datasource_Root_EnabledHint = null;
		}
		_datasource_Root_EnabledHint = newDataSource;
		_datasource_Root_EnabledHint = _datasource_Root.EnabledHint;
		if (_datasource_Root_EnabledHint != null)
		{
			_datasource_Root_EnabledHint.PropertyChanged += ViewModelPropertyChangedListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithValue += ViewModelPropertyChangedWithValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithBoolValue += ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithIntValue += ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithFloatValue += ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithUIntValue += ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithColorValue += ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithDoubleValue += ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_EnabledHint;
			_datasource_Root_EnabledHint.PropertyChangedWithVec2Value += ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_EnabledHint;
			_widget_0_1.EventFire += EventListenerOf_widget_0_1;
		}
	}

	private void RefreshDataSource_datasource_Root_DisabledHint(HintViewModel newDataSource)
	{
		if (_datasource_Root_DisabledHint != null)
		{
			_datasource_Root_DisabledHint.PropertyChanged -= ViewModelPropertyChangedListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithValue -= ViewModelPropertyChangedWithValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithBoolValue -= ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithIntValue -= ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithFloatValue -= ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithUIntValue -= ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithColorValue -= ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithDoubleValue -= ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithVec2Value -= ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_DisabledHint;
			_widget_1.EventFire -= EventListenerOf_widget_1;
			_datasource_Root_DisabledHint = null;
		}
		_datasource_Root_DisabledHint = newDataSource;
		_datasource_Root_DisabledHint = _datasource_Root.DisabledHint;
		if (_datasource_Root_DisabledHint != null)
		{
			_datasource_Root_DisabledHint.PropertyChanged += ViewModelPropertyChangedListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithValue += ViewModelPropertyChangedWithValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithBoolValue += ViewModelPropertyChangedWithBoolValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithIntValue += ViewModelPropertyChangedWithIntValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithFloatValue += ViewModelPropertyChangedWithFloatValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithUIntValue += ViewModelPropertyChangedWithUIntValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithColorValue += ViewModelPropertyChangedWithColorValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithDoubleValue += ViewModelPropertyChangedWithDoubleValueListenerOf_datasource_Root_DisabledHint;
			_datasource_Root_DisabledHint.PropertyChangedWithVec2Value += ViewModelPropertyChangedWithVec2ValueListenerOf_datasource_Root_DisabledHint;
			_widget_1.EventFire += EventListenerOf_widget_1;
		}
	}
}
