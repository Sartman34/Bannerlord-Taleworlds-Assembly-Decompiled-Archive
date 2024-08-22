using System.Collections.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.ScreenSystem;

public abstract class ScreenBase
{
	public delegate void OnLayerAddedEvent(ScreenLayer addedLayer);

	public delegate void OnLayerRemovedEvent(ScreenLayer removedLayer);

	private readonly List<ScreenComponent> _components;

	private readonly MBList<ScreenLayer> _layers;

	private readonly List<ScreenLayer> _layersCopy;

	protected bool _shouldTickLayersThisFrame = true;

	private bool _isInitialized;

	public IInputContext DebugInput => Input.DebugInput;

	public MBReadOnlyList<ScreenLayer> Layers => _layers;

	public bool IsActive { get; private set; }

	public bool IsPaused { get; private set; }

	public virtual bool MouseVisible { get; set; }

	public event OnLayerAddedEvent OnAddLayer;

	public event OnLayerRemovedEvent OnRemoveLayer;

	internal void HandleInitialize()
	{
		Debug.Print(string.Concat(this, "::HandleInitialize"));
		if (!_isInitialized)
		{
			_isInitialized = true;
			OnInitialize();
			Debug.ReportMemoryBookmark("ScreenBase Initialized: " + GetType().Name);
		}
	}

	internal void HandleFinalize()
	{
		Debug.Print(string.Concat(this, "::HandleFinalize"));
		if (_isInitialized)
		{
			_isInitialized = false;
			OnFinalize();
			for (int num = _layers.Count - 1; num >= 0; num--)
			{
				_layers[num].HandleFinalize();
			}
		}
		IsActive = false;
		this.OnAddLayer = null;
		this.OnRemoveLayer = null;
	}

	internal void HandleActivate()
	{
		Debug.Print(string.Concat(this, "::HandleActivate"));
		if (IsActive)
		{
			return;
		}
		IsActive = true;
		for (int num = _layers.Count - 1; num >= 0; num--)
		{
			ScreenLayer screenLayer = _layers[num];
			if (!screenLayer.IsActive)
			{
				screenLayer.HandleActivate();
			}
		}
		OnActivate();
	}

	internal void HandleDeactivate()
	{
		Debug.Print(string.Concat(this, "::HandleDeactivate"));
		if (!IsActive)
		{
			return;
		}
		IsActive = false;
		for (int num = _layers.Count - 1; num >= 0; num--)
		{
			ScreenLayer screenLayer = _layers[num];
			if (screenLayer.IsActive)
			{
				screenLayer.HandleDeactivate();
			}
		}
		OnDeactivate();
	}

	internal void HandleResume()
	{
		Debug.Print(string.Concat(this, "::HandleResume"));
		if (!IsPaused)
		{
			return;
		}
		for (int num = _layers.Count - 1; num >= 0; num--)
		{
			ScreenLayer screenLayer = _layers[num];
			if (!screenLayer.IsActive)
			{
				screenLayer.HandleActivate();
			}
		}
		IsPaused = false;
		OnResume();
	}

	internal void HandlePause()
	{
		Debug.Print(string.Concat(this, "::HandlePause"));
		if (IsPaused)
		{
			return;
		}
		for (int num = _layers.Count - 1; num >= 0; num--)
		{
			ScreenLayer screenLayer = _layers[num];
			if (screenLayer.IsActive)
			{
				screenLayer.HandleDeactivate();
			}
		}
		IsPaused = true;
		OnPause();
	}

	internal void FrameTick(float dt)
	{
		_shouldTickLayersThisFrame = true;
		if (IsActive)
		{
			OnFrameTick(dt);
		}
		if (!IsActive)
		{
			return;
		}
		if (!_shouldTickLayersThisFrame)
		{
			dt = 0f;
		}
		for (int i = 0; i < _layers.Count; i++)
		{
			if (_layers[i].IsActive)
			{
				_layersCopy.Add(_layers[i]);
			}
		}
		for (int j = 0; j < _layersCopy.Count; j++)
		{
			if (!_layersCopy[j].Finalized)
			{
				_layersCopy[j].Tick(dt);
			}
		}
		ScreenManager.UpdateLateTickLayers(_layersCopy);
	}

	public void ActivateAllLayers()
	{
		foreach (ScreenLayer layer in _layers)
		{
			if (!layer.IsActive)
			{
				layer.HandleActivate();
			}
		}
	}

	public void DeactivateAllLayers()
	{
		foreach (ScreenLayer layer in _layers)
		{
			if (layer.IsActive)
			{
				layer.HandleDeactivate();
			}
		}
	}

	public void Deactivate()
	{
		if (IsActive)
		{
			HandleDeactivate();
			IsActive = false;
		}
	}

	public void Activate()
	{
		if (!IsActive)
		{
			HandleActivate();
			IsActive = true;
		}
	}

	public virtual void UpdateLayout()
	{
		for (int i = 0; i < _layers.Count; i++)
		{
			if (!_layers[i].Finalized)
			{
				_layers[i].UpdateLayout();
			}
		}
	}

	internal void IdleTick(float dt)
	{
		OnIdleTick(dt);
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnFinalize()
	{
	}

	protected virtual void OnPause()
	{
	}

	protected virtual void OnResume()
	{
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnDeactivate()
	{
	}

	protected virtual void OnFrameTick(float dt)
	{
	}

	protected virtual void OnIdleTick(float dt)
	{
	}

	public virtual void OnFocusChangeOnGameWindow(bool focusGained)
	{
	}

	public void AddComponent(ScreenComponent component)
	{
		_components.Add(component);
	}

	public T FindComponent<T>() where T : ScreenComponent
	{
		foreach (ScreenComponent component in _components)
		{
			if (component is T)
			{
				return (T)component;
			}
		}
		return null;
	}

	public void AddLayer(ScreenLayer layer)
	{
		if (layer == null || layer.Finalized)
		{
			Debug.FailedAssert("Trying to add a null or finalized layer", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.ScreenSystem\\ScreenBase.cs", "AddLayer", 334);
		}
		else if (!_layers.Contains(layer))
		{
			_layers.Add(layer);
			_layers.Sort();
			if (IsActive)
			{
				layer.LastActiveState = true;
				layer.HandleActivate();
			}
			this.OnAddLayer?.Invoke(layer);
		}
		else
		{
			Debug.FailedAssert("Layer is already added to the screen!", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.ScreenSystem\\ScreenBase.cs", "AddLayer", 353);
		}
	}

	public void RemoveLayer(ScreenLayer layer)
	{
		if (IsActive)
		{
			layer.LastActiveState = false;
			layer.HandleDeactivate();
		}
		layer.HandleFinalize();
		_layers.Remove(layer);
		this.OnRemoveLayer?.Invoke(layer);
		ScreenManager.RefreshGlobalOrder();
	}

	public bool HasLayer(ScreenLayer layer)
	{
		return _layers.Contains(layer);
	}

	public T FindLayer<T>() where T : ScreenLayer
	{
		foreach (ScreenLayer layer in _layers)
		{
			if (layer is T)
			{
				return (T)layer;
			}
		}
		return null;
	}

	public T FindLayer<T>(string name) where T : ScreenLayer
	{
		foreach (ScreenLayer layer in _layers)
		{
			if (layer is T val && val.Name == name)
			{
				return val;
			}
		}
		return null;
	}

	public void SetLayerCategoriesState(string[] categoryIds, bool isActive)
	{
		foreach (ScreenLayer layer in _layers)
		{
			if (categoryIds.IndexOf(layer._categoryId) >= 0)
			{
				if (isActive && !layer.IsActive)
				{
					layer.HandleActivate();
				}
				else if (!isActive && layer.IsActive)
				{
					layer.HandleDeactivate();
				}
			}
		}
	}

	public void SetLayerCategoriesStateAndToggleOthers(string[] categoryIds, bool isActive)
	{
		foreach (ScreenLayer layer in _layers)
		{
			if (categoryIds.IndexOf(layer._categoryId) >= 0)
			{
				if (isActive && !layer.IsActive)
				{
					layer.HandleActivate();
				}
				else if (!isActive && layer.IsActive)
				{
					layer.HandleDeactivate();
				}
			}
			else if (layer.IsActive)
			{
				layer.HandleDeactivate();
			}
			else
			{
				layer.HandleActivate();
			}
		}
	}

	public void SetLayerCategoriesStateAndDeactivateOthers(string[] categoryIds, bool isActive)
	{
		foreach (ScreenLayer layer in _layers)
		{
			if (categoryIds.IndexOf(layer._categoryId) >= 0)
			{
				if (isActive && !layer.IsActive)
				{
					layer.HandleActivate();
				}
				else if (!isActive && layer.IsActive)
				{
					layer.HandleDeactivate();
				}
			}
			else if (layer.IsActive)
			{
				layer.HandleDeactivate();
			}
		}
	}

	protected ScreenBase()
	{
		IsPaused = true;
		IsActive = false;
		_components = new List<ScreenComponent>();
		_layers = new MBList<ScreenLayer>();
		_layersCopy = new List<ScreenLayer>();
	}

	internal void Update(IReadOnlyList<int> lastKeysPressed)
	{
		if (!IsActive)
		{
			return;
		}
		foreach (ScreenLayer layer in _layers)
		{
			if (layer.IsActive)
			{
				layer.Update(lastKeysPressed);
			}
		}
	}
}
