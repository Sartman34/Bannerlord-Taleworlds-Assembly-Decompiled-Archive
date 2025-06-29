using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;

namespace TaleWorlds.GauntletUI.Data;

public class GauntletView : WidgetComponent
{
	private BindingPath _viewModelPath;

	private Dictionary<string, ViewBindDataInfo> _bindDataInfos;

	private Dictionary<string, ViewBindCommandInfo> _bindCommandInfos;

	private IViewModel _viewModel;

	private IMBBindingList _bindingList;

	private MBList<GauntletView> _children;

	private List<GauntletView> _items;

	internal bool AddedToChildren { get; private set; }

	public object Tag
	{
		get
		{
			return base.Target.Tag;
		}
		set
		{
			base.Target.Tag = value;
		}
	}

	public GauntletMovie GauntletMovie { get; private set; }

	public ItemTemplateUsageWithData ItemTemplateUsageWithData { get; internal set; }

	public BindingPath ViewModelPath
	{
		get
		{
			if (Parent != null)
			{
				if (_viewModelPath != null)
				{
					return Parent.ViewModelPath.Append(_viewModelPath);
				}
				return Parent.ViewModelPath;
			}
			return _viewModelPath;
		}
	}

	public string ViewModelPathString
	{
		get
		{
			MBStringBuilder sb = default(MBStringBuilder);
			sb.Initialize(16, "ViewModelPathString");
			WriteViewModelPathToStringBuilder(ref sb);
			return sb.ToStringAndRelease();
		}
	}

	public MBReadOnlyList<GauntletView> Children => _children;

	public IEnumerable<GauntletView> AllChildren
	{
		get
		{
			foreach (GauntletView gauntletView in _children)
			{
				yield return gauntletView;
				foreach (GauntletView allChild in gauntletView.AllChildren)
				{
					yield return allChild;
				}
			}
		}
	}

	public GauntletView Parent { get; private set; }

	public string DisplayName
	{
		get
		{
			string text = "";
			if (base.Target != null)
			{
				text = text + base.Target.Id + "!" + base.Target.Tag.ToString();
			}
			if (ViewModelPath != null)
			{
				text = text + "@" + ViewModelPath.Path;
			}
			return text;
		}
	}

	private void WriteViewModelPathToStringBuilder(ref MBStringBuilder sb)
	{
		if (Parent == null)
		{
			if (_viewModelPath != null)
			{
				sb.Append(_viewModelPath.Path);
			}
			return;
		}
		Parent.WriteViewModelPathToStringBuilder(ref sb);
		if (_viewModelPath != null)
		{
			sb.Append("\\");
			sb.Append(_viewModelPath.Path);
		}
	}

	internal void InitializeViewModelPath(BindingPath path)
	{
		_viewModelPath = path;
	}

	internal GauntletView(GauntletMovie gauntletMovie, GauntletView parent, Widget target, int childCount = 64)
		: base(target)
	{
		GauntletMovie = gauntletMovie;
		Parent = parent;
		_children = new MBList<GauntletView>(childCount);
		_bindDataInfos = new Dictionary<string, ViewBindDataInfo>();
		_bindCommandInfos = new Dictionary<string, ViewBindCommandInfo>();
		_items = new List<GauntletView>(childCount);
	}

	public void AddChild(GauntletView child)
	{
		_children.Add(child);
		child.AddedToChildren = true;
	}

	public void RemoveChild(GauntletView child)
	{
		base.Target.OnBeforeRemovedChild(child.Target);
		base.Target.RemoveChild(child.Target);
		_children.Remove(child);
		child.ClearEventHandlersWithChildren();
	}

	public void SwapChildrenAtIndeces(GauntletView child1, GauntletView child2)
	{
		int index = _children.IndexOf(child1);
		int index2 = _children.IndexOf(child2);
		base.Target.SwapChildren(_children[index].Target, _children[index2].Target);
		GauntletView value = _children[index];
		_children[index] = _children[index2];
		_children[index2] = value;
	}

	public void RefreshBinding()
	{
		object viewModelAtPath = GauntletMovie.GetViewModelAtPath(ViewModelPath, ItemTemplateUsageWithData != null && ItemTemplateUsageWithData.ItemTemplateUsage != null);
		ClearEventHandlersWithChildren();
		if (viewModelAtPath is IViewModel)
		{
			_viewModel = viewModelAtPath as IViewModel;
			if (_viewModel != null)
			{
				_viewModel.PropertyChanged += OnViewModelPropertyChanged;
				_viewModel.PropertyChangedWithValue += OnViewModelPropertyChangedWithValue;
				_viewModel.PropertyChangedWithBoolValue += OnViewModelPropertyChangedWithValue;
				_viewModel.PropertyChangedWithColorValue += OnViewModelPropertyChangedWithValue;
				_viewModel.PropertyChangedWithDoubleValue += OnViewModelPropertyChangedWithValue;
				_viewModel.PropertyChangedWithFloatValue += OnViewModelPropertyChangedWithValue;
				_viewModel.PropertyChangedWithIntValue += OnViewModelPropertyChangedWithValue;
				_viewModel.PropertyChangedWithUIntValue += OnViewModelPropertyChangedWithValue;
				_viewModel.PropertyChangedWithVec2Value += OnViewModelPropertyChangedWithValue;
				foreach (ViewBindDataInfo value in _bindDataInfos.Values)
				{
					object propertyValue = _viewModel.GetPropertyValue(value.Path.LastNode);
					SetData(value.Property, propertyValue);
				}
			}
		}
		else if (viewModelAtPath is IMBBindingList)
		{
			_bindingList = viewModelAtPath as IMBBindingList;
			if (_bindingList != null)
			{
				_bindingList.ListChanged += OnViewModelBindingListChanged;
				for (int i = 0; i < _bindingList.Count; i++)
				{
					AddItemToList(i);
				}
			}
		}
		base.Target.EventFire += OnEventFired;
		base.Target.PropertyChanged += OnViewObjectPropertyChanged;
		base.Target.boolPropertyChanged += OnViewObjectPropertyChanged;
		base.Target.ColorPropertyChanged += OnViewObjectPropertyChanged;
		base.Target.doublePropertyChanged += OnViewObjectPropertyChanged;
		base.Target.floatPropertyChanged += OnViewObjectPropertyChanged;
		base.Target.intPropertyChanged += OnViewObjectPropertyChanged;
		base.Target.uintPropertyChanged += OnViewObjectPropertyChanged;
		base.Target.Vec2PropertyChanged += OnViewObjectPropertyChanged;
		base.Target.Vector2PropertyChanged += OnViewObjectPropertyChanged;
	}

	private void OnEventFired(Widget widget, string commandName, object[] args)
	{
		OnCommand(commandName, args);
	}

	public void RefreshBindingWithChildren()
	{
		RefreshBinding();
		for (int i = 0; i < _children.Count; i++)
		{
			_children[i].RefreshBindingWithChildren();
		}
	}

	private void ReleaseBinding()
	{
		if (_viewModel != null)
		{
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
			_viewModel.PropertyChangedWithValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithBoolValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithColorValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithDoubleValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithFloatValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithIntValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithUIntValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithVec2Value -= OnViewModelPropertyChangedWithValue;
		}
		else if (_bindingList != null)
		{
			_bindingList.ListChanged -= OnViewModelBindingListChanged;
		}
	}

	public void ReleaseBindingWithChildren()
	{
		ReleaseBinding();
		for (int i = 0; i < _children.Count; i++)
		{
			_children[i].ReleaseBindingWithChildren();
		}
	}

	internal void ClearEventHandlersWithChildren()
	{
		ClearEventHandlers();
		for (int i = 0; i < _children.Count; i++)
		{
			_children[i].ClearEventHandlersWithChildren();
		}
	}

	private void ClearEventHandlers()
	{
		if (_viewModel != null)
		{
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
			_viewModel.PropertyChangedWithValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithBoolValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithColorValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithDoubleValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithFloatValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithIntValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithUIntValue -= OnViewModelPropertyChangedWithValue;
			_viewModel.PropertyChangedWithVec2Value -= OnViewModelPropertyChangedWithValue;
			_viewModel = null;
		}
		if (_bindingList != null)
		{
			OnListReset();
			_bindingList.ListChanged -= OnViewModelBindingListChanged;
			_bindingList = null;
		}
		base.Target.EventFire -= OnEventFired;
		base.Target.PropertyChanged -= OnViewObjectPropertyChanged;
		base.Target.boolPropertyChanged -= OnViewObjectPropertyChanged;
		base.Target.ColorPropertyChanged -= OnViewObjectPropertyChanged;
		base.Target.doublePropertyChanged -= OnViewObjectPropertyChanged;
		base.Target.floatPropertyChanged -= OnViewObjectPropertyChanged;
		base.Target.intPropertyChanged -= OnViewObjectPropertyChanged;
		base.Target.uintPropertyChanged -= OnViewObjectPropertyChanged;
		base.Target.Vec2PropertyChanged -= OnViewObjectPropertyChanged;
		base.Target.Vector2PropertyChanged -= OnViewObjectPropertyChanged;
	}

	public void BindData(string property, BindingPath path)
	{
		ViewBindDataInfo viewBindDataInfo = new ViewBindDataInfo(this, property, path);
		if (!_bindDataInfos.ContainsKey(path.Path))
		{
			_bindDataInfos.Add(path.Path, viewBindDataInfo);
		}
		else
		{
			_bindDataInfos[path.Path] = viewBindDataInfo;
		}
		if (_viewModel != null)
		{
			object propertyValue = _viewModel.GetPropertyValue(path.LastNode);
			SetData(viewBindDataInfo.Property, propertyValue);
		}
	}

	public void ClearBinding(string propertyName)
	{
		KeyValuePair<string, ViewBindDataInfo>[] array = _bindDataInfos.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, ViewBindDataInfo> keyValuePair = array[i];
			if (keyValuePair.Value.Property == propertyName)
			{
				_bindDataInfos.Remove(keyValuePair.Key);
			}
		}
	}

	internal void BindCommand(string command, BindingPath path, string parameterValue = null)
	{
		ViewBindCommandInfo value = new ViewBindCommandInfo(this, command, path, parameterValue);
		if (!_bindCommandInfos.ContainsKey(command))
		{
			_bindCommandInfos.Add(command, value);
		}
		else
		{
			_bindCommandInfos[command] = value;
		}
	}

	private void OnViewModelBindingListChanged(object sender, TaleWorlds.Library.ListChangedEventArgs e)
	{
		switch (e.ListChangedType)
		{
		case TaleWorlds.Library.ListChangedType.Reset:
			OnListReset();
			break;
		case TaleWorlds.Library.ListChangedType.Sorted:
			OnListSorted();
			break;
		case TaleWorlds.Library.ListChangedType.ItemAdded:
			OnItemAddedToList(e.NewIndex);
			break;
		case TaleWorlds.Library.ListChangedType.ItemDeleted:
			OnItemRemovedFromList(e.NewIndex);
			break;
		case TaleWorlds.Library.ListChangedType.ItemBeforeDeleted:
			OnBeforeItemRemovedFromList(e.NewIndex);
			break;
		case TaleWorlds.Library.ListChangedType.ItemChanged:
			break;
		}
	}

	private void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
	{
		OnPropertyChanged(e.PropertyName, e.Value);
	}

	private void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithBoolValueEventArgs e)
	{
		OnPropertyChanged(e.PropertyName, e.Value);
	}

	private void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithFloatValueEventArgs e)
	{
		OnPropertyChanged(e.PropertyName, e.Value);
	}

	private void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithColorValueEventArgs e)
	{
		OnPropertyChanged(e.PropertyName, e.Value);
	}

	private void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithDoubleValueEventArgs e)
	{
		OnPropertyChanged(e.PropertyName, e.Value);
	}

	private void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithIntValueEventArgs e)
	{
		OnPropertyChanged(e.PropertyName, e.Value);
	}

	private void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithUIntValueEventArgs e)
	{
		OnPropertyChanged(e.PropertyName, e.Value);
	}

	private void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithVec2ValueEventArgs e)
	{
		OnPropertyChanged(e.PropertyName, e.Value);
	}

	private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		object propertyValue = _viewModel.GetPropertyValue(e.PropertyName);
		OnPropertyChanged(e.PropertyName, propertyValue);
	}

	private void OnPropertyChanged(string propertyName, object value)
	{
		if (value is ViewModel || value is IMBBindingList)
		{
			MBStringBuilder sb = default(MBStringBuilder);
			sb.Initialize(16, "OnPropertyChanged");
			WriteViewModelPathToStringBuilder(ref sb);
			sb.Append("\\");
			sb.Append(propertyName);
			string path = sb.ToStringAndRelease();
			{
				foreach (GauntletView child in Children)
				{
					if (BindingPath.IsRelatedWithPathAsString(path, child.ViewModelPathString))
					{
						child.RefreshBindingWithChildren();
					}
				}
				return;
			}
		}
		if (_bindDataInfos.Count > 0 && _bindDataInfos.TryGetValue(propertyName, out var value2))
		{
			SetData(value2.Property, value);
		}
	}

	private void OnCommand(string command, object[] args)
	{
		ViewBindCommandInfo value = null;
		if (_bindCommandInfos.TryGetValue(command, out value))
		{
			object[] array = null;
			if (value.Parameter != null)
			{
				array = new object[args.Length + 1];
				array[args.Length] = value.Parameter;
			}
			else
			{
				array = new object[args.Length];
			}
			for (int i = 0; i < args.Length; i++)
			{
				object parameter = args[i];
				object obj = ConvertCommandParameter(parameter);
				array[i] = obj;
			}
			BindingPath parentPath = value.Path.ParentPath;
			BindingPath bindingPath = ViewModelPath;
			if (parentPath != null)
			{
				bindingPath = bindingPath.Append(parentPath);
			}
			BindingPath path = bindingPath.Simplify();
			IViewModel viewModel = GauntletMovie.ViewModel;
			string lastNode = value.Path.LastNode;
			if (viewModel.GetViewModelAtPath(path, value.Owner.ItemTemplateUsageWithData != null && value.Owner.ItemTemplateUsageWithData.ItemTemplateUsage != null) is ViewModel viewModel2)
			{
				viewModel2.ExecuteCommand(lastNode, array);
			}
		}
	}

	private object ConvertCommandParameter(object parameter)
	{
		object result = parameter;
		if (parameter is Widget)
		{
			Widget widget = (Widget)parameter;
			GauntletView gauntletView = GauntletMovie.FindViewOf(widget);
			result = ((gauntletView == null) ? null : ((gauntletView._viewModel == null) ? ((object)gauntletView._bindingList) : ((object)gauntletView._viewModel)));
		}
		return result;
	}

	private ViewBindDataInfo GetBindDataInfoOfProperty(string propertyName)
	{
		foreach (ViewBindDataInfo value in _bindDataInfos.Values)
		{
			if (value.Property == propertyName)
			{
				return value;
			}
		}
		return null;
	}

	private void OnListSorted()
	{
		List<GauntletView> list = new List<GauntletView>(_items.Capacity);
		for (int i = 0; i < _bindingList.Count; i++)
		{
			object bindingObject = _bindingList[i];
			GauntletView item = _items.Find((GauntletView gauntletView) => gauntletView._viewModel == bindingObject);
			list.Add(item);
		}
		_items = list;
		for (int j = 0; j < _items.Count; j++)
		{
			BindingPath path = new BindingPath(j);
			GauntletView gauntletView2 = _items[j];
			gauntletView2.Target.SetSiblingIndex(j);
			gauntletView2.InitializeViewModelPath(path);
		}
	}

	private void OnListReset()
	{
		GauntletView[] array = _items.ToArray();
		_items.Clear();
		GauntletView[] array2 = array;
		foreach (GauntletView gauntletView in array2)
		{
			base.Target.OnBeforeRemovedChild(gauntletView.Target);
			_children.Remove(gauntletView);
			base.Target.RemoveChild(gauntletView.Target);
			gauntletView.ClearEventHandlersWithChildren();
		}
	}

	private void OnItemAddedToList(int index)
	{
		AddItemToList(index).RefreshBindingWithChildren();
	}

	private GauntletView AddItemToList(int index)
	{
		for (int i = index; i < _items.Count; i++)
		{
			_items[i]._viewModelPath = new BindingPath(i + 1);
		}
		BindingPath path = new BindingPath(index);
		GauntletView gauntletView = null;
		WidgetCreationData widgetCreationData = new WidgetCreationData(GauntletMovie.Context, GauntletMovie.WidgetFactory, base.Target);
		widgetCreationData.AddExtensionData(GauntletMovie);
		widgetCreationData.AddExtensionData(this);
		bool flag = false;
		if (_items.Count == 0 && ItemTemplateUsageWithData.FirstItemTemplate != null)
		{
			gauntletView = ItemTemplateUsageWithData.FirstItemTemplate.Instantiate(widgetCreationData, ItemTemplateUsageWithData.GivenParameters).GetGauntletView();
		}
		else if (_items.Count == index && _items.Count > 0 && ItemTemplateUsageWithData.LastItemTemplate != null)
		{
			BindingPath viewModelPath = _items[_items.Count - 1]._viewModelPath;
			GauntletView gauntletView2 = ItemTemplateUsageWithData.DefaultItemTemplate.Instantiate(widgetCreationData, ItemTemplateUsageWithData.GivenParameters).GetGauntletView();
			_items.Insert(_items.Count, gauntletView2);
			RemoveItemFromList(_items.Count - 2);
			gauntletView2.InitializeViewModelPath(viewModelPath);
			gauntletView2.RefreshBindingWithChildren();
			flag = true;
			gauntletView = ItemTemplateUsageWithData.LastItemTemplate.Instantiate(widgetCreationData, ItemTemplateUsageWithData.GivenParameters).GetGauntletView();
		}
		else
		{
			gauntletView = ItemTemplateUsageWithData.DefaultItemTemplate.Instantiate(widgetCreationData, ItemTemplateUsageWithData.GivenParameters).GetGauntletView();
		}
		gauntletView.InitializeViewModelPath(path);
		_items.Insert(index, gauntletView);
		for (int j = (flag ? (index - 1) : index); j < _items.Count; j++)
		{
			_items[j].Target.SetSiblingIndex(j, flag);
		}
		return gauntletView;
	}

	private void OnItemRemovedFromList(int index)
	{
		RemoveItemFromList(index);
	}

	private void RemoveItemFromList(int index)
	{
		GauntletView gauntletView = _items[index];
		_items.RemoveAt(index);
		_children.Remove(gauntletView);
		base.Target.RemoveChild(gauntletView.Target);
		gauntletView.ClearEventHandlersWithChildren();
		for (int i = index; i < _items.Count; i++)
		{
			_items[i].Target.SetSiblingIndex(i);
		}
		BindingPath viewModelPath = gauntletView._viewModelPath;
		for (int j = index; j < _items.Count; j++)
		{
			GauntletView gauntletView2 = _items[j];
			BindingPath viewModelPath2 = gauntletView2._viewModelPath;
			gauntletView2._viewModelPath = viewModelPath;
			viewModelPath = viewModelPath2;
		}
	}

	private void OnBeforeItemRemovedFromList(int index)
	{
		PreviewRemoveItemFromList(index);
	}

	private void PreviewRemoveItemFromList(int index)
	{
		GauntletView gauntletView = _items[index];
		base.Target.OnBeforeRemovedChild(gauntletView.Target);
	}

	private void SetData(string path, object value)
	{
		WidgetExtensions.SetWidgetAttribute(GauntletMovie.Context, base.Target, path, value);
	}

	private void OnViewPropertyChanged(string propertyName, object value)
	{
		if (_viewModel != null)
		{
			ViewBindDataInfo bindDataInfoOfProperty = GetBindDataInfoOfProperty(propertyName);
			if (bindDataInfoOfProperty != null)
			{
				_viewModel.SetPropertyValue(bindDataInfoOfProperty.Path.LastNode, value);
			}
		}
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, object value)
	{
		OnViewPropertyChanged(propertyName, value);
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, bool value)
	{
		OnViewPropertyChanged(propertyName, value);
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, float value)
	{
		OnViewPropertyChanged(propertyName, value);
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, Color value)
	{
		OnViewPropertyChanged(propertyName, value);
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, double value)
	{
		OnViewPropertyChanged(propertyName, value);
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, int value)
	{
		OnViewPropertyChanged(propertyName, value);
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, uint value)
	{
		OnViewPropertyChanged(propertyName, value);
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, Vec2 value)
	{
		OnViewPropertyChanged(propertyName, value);
	}

	private void OnViewObjectPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, Vector2 value)
	{
		OnViewPropertyChanged(propertyName, value);
	}
}
