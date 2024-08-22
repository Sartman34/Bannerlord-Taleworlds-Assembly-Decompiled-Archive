using System;
using System.Numerics;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension.Standalone.Native.Windows;

namespace TaleWorlds.TwoDimension.Standalone;

public class GraphicsForm : IMessageCommunicator
{
	public const int WM_NCLBUTTONDOWN = 161;

	public const int HT_CAPTION = 2;

	private WindowsForm _windowsForm;

	private InputData _currentInputData;

	private InputData _oldInputData;

	private InputData _messageLoopInputData;

	private object _inputDataLocker = new object();

	private bool _mouseOverDragArea = true;

	private bool _isDragging;

	private LayeredWindowController _layeredWindowController;

	public GraphicsContext GraphicsContext { get; private set; }

	public int Width => _windowsForm.Width;

	public int Height => _windowsForm.Height;

	public GraphicsForm(int width, int height, ResourceDepot resourceDepot, bool borderlessWindow = false, bool enableWindowBlur = false, bool layeredWindow = false, string name = null)
	{
		User32.GetClientRect(User32.GetDesktopWindow(), out var lpRect);
		int x = (lpRect.Width - width) / 2;
		int y = (lpRect.Height - height) / 2;
		_windowsForm = new WindowsForm(x, y, width, height, resourceDepot, borderlessWindow, enableWindowBlur, name);
		Initalize(layeredWindow);
	}

	public GraphicsForm(int x, int y, int width, int height, ResourceDepot resourceDepot, bool borderlessWindow = false, bool enableWindowBlur = false, bool layeredWindow = false, string name = null)
	{
		_windowsForm = new WindowsForm(x, y, width, height, resourceDepot, borderlessWindow, enableWindowBlur, name);
		Initalize(layeredWindow);
	}

	public GraphicsForm(WindowsForm windowsForm)
	{
		_windowsForm = windowsForm;
		Initalize(layeredWindow: false);
	}

	private void Initalize(bool layeredWindow)
	{
		_currentInputData = new InputData();
		_oldInputData = new InputData();
		_messageLoopInputData = new InputData();
		_windowsForm.AddMessageHandler(MessageHandler);
		_windowsForm.Show();
		GraphicsContext = new GraphicsContext();
		if (layeredWindow)
		{
			_layeredWindowController = new LayeredWindowController(_windowsForm.Handle, _windowsForm.Width, _windowsForm.Height);
		}
	}

	public void Destroy()
	{
		GraphicsContext.DestroyContext();
		_windowsForm.Destroy();
		_layeredWindowController?.OnFinalize();
	}

	public void MinimizeWindow()
	{
		User32.ShowWindow(_windowsForm.Handle, WindowShowStyle.Minimize);
	}

	public void InitializeGraphicsContext(ResourceDepot resourceDepot)
	{
		GraphicsContext.Control = _windowsForm;
		GraphicsContext.CreateContext(resourceDepot);
		GraphicsContext.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0f, _windowsForm.Width, _windowsForm.Height, 0f, 0f, 1f);
	}

	public void BeginFrame()
	{
		if (GraphicsContext != null)
		{
			GraphicsContext.BeginFrame(_windowsForm.Width, _windowsForm.Height);
			GraphicsContext.Resize(_windowsForm.Width, _windowsForm.Height);
		}
		GraphicsContext.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0f, _windowsForm.Width, _windowsForm.Height, 0f, 0f, 1f);
	}

	public void Update()
	{
		if (!_isDragging && _mouseOverDragArea && _currentInputData.LeftMouse && !_oldInputData.LeftMouse)
		{
			_isDragging = true;
			MessageHandler(WindowMessage.LeftButtonUp, 0L, 0L);
		}
	}

	public void MessageLoop()
	{
		if (_isDragging)
		{
			User32.ReleaseCapture();
			User32.SendMessage(_windowsForm.Handle, 161u, new IntPtr(2), IntPtr.Zero);
			_isDragging = false;
			User32.SetCapture(_windowsForm.Handle);
		}
	}

	public void UpdateInput(bool mouseOverDragArea = false)
	{
		_mouseOverDragArea = mouseOverDragArea;
		InputData oldInputData = _oldInputData;
		_oldInputData = _currentInputData;
		_currentInputData = oldInputData;
		lock (_inputDataLocker)
		{
			_currentInputData.FillFrom(_messageLoopInputData);
			_messageLoopInputData.Reset();
		}
	}

	public void PostRender()
	{
		if (_layeredWindowController != null)
		{
			_layeredWindowController.PostRender();
		}
	}

	public bool GetKeyDown(InputKey keyCode)
	{
		switch (keyCode)
		{
		case InputKey.LeftMouseButton:
			return LeftMouseDown();
		case InputKey.RightMouseButton:
			return RightMouseDown();
		default:
			if (_currentInputData.KeyData[(int)keyCode])
			{
				return !_oldInputData.KeyData[(int)keyCode];
			}
			return false;
		}
	}

	public bool GetKey(InputKey keyCode)
	{
		return keyCode switch
		{
			InputKey.LeftMouseButton => LeftMouse(), 
			InputKey.RightMouseButton => RightMouse(), 
			_ => _currentInputData.KeyData[(int)keyCode], 
		};
	}

	public bool GetKeyUp(InputKey keyCode)
	{
		switch (keyCode)
		{
		case InputKey.LeftMouseButton:
			return LeftMouseUp();
		case InputKey.RightMouseButton:
			return RightMouseUp();
		default:
			if (!_currentInputData.KeyData[(int)keyCode])
			{
				return _oldInputData.KeyData[(int)keyCode];
			}
			return false;
		}
	}

	public float GetMouseDeltaZ()
	{
		return _currentInputData.MouseScrollDelta;
	}

	public bool LeftMouse()
	{
		return _currentInputData.LeftMouse;
	}

	public bool LeftMouseDown()
	{
		if (_currentInputData.LeftMouse)
		{
			return !_oldInputData.LeftMouse;
		}
		return false;
	}

	public bool LeftMouseUp()
	{
		if (!_currentInputData.LeftMouse)
		{
			return _oldInputData.LeftMouse;
		}
		return false;
	}

	public bool RightMouse()
	{
		return _currentInputData.RightMouse;
	}

	public bool RightMouseDown()
	{
		if (_currentInputData.RightMouse)
		{
			return !_oldInputData.RightMouse;
		}
		return false;
	}

	public bool RightMouseUp()
	{
		if (!_currentInputData.RightMouse)
		{
			return _oldInputData.RightMouse;
		}
		return false;
	}

	public Vector2 MousePosition()
	{
		return new Vector2(_currentInputData.CursorX, _currentInputData.CursorY);
	}

	public bool MouseMove()
	{
		return _currentInputData.MouseMove;
	}

	public void FillInputDataFromCurrent(InputData inputData)
	{
		inputData.FillFrom(_currentInputData);
	}

	private void MessageHandler(WindowMessage message, long wParam, long lParam)
	{
		switch (message)
		{
		case WindowMessage.Close:
			Destroy();
			Environment.Exit(0);
			break;
		case WindowMessage.KeyDown:
			lock (_inputDataLocker)
			{
				_messageLoopInputData.KeyData[wParam] = true;
				break;
			}
		case WindowMessage.KeyUp:
			lock (_inputDataLocker)
			{
				_messageLoopInputData.KeyData[wParam] = false;
				break;
			}
		case WindowMessage.RightButtonUp:
			lock (_inputDataLocker)
			{
				_messageLoopInputData.RightMouse = false;
				int cursorX5 = (int)lParam % 65536;
				int cursorY5 = (int)(lParam / 65536);
				_messageLoopInputData.CursorX = cursorX5;
				_messageLoopInputData.CursorY = cursorY5;
				break;
			}
		case WindowMessage.RightButtonDown:
			lock (_inputDataLocker)
			{
				_messageLoopInputData.RightMouse = true;
				int cursorX4 = (int)lParam % 65536;
				int cursorY4 = (int)(lParam / 65536);
				_messageLoopInputData.CursorX = cursorX4;
				_messageLoopInputData.CursorY = cursorY4;
				break;
			}
		case WindowMessage.LeftButtonUp:
			lock (_inputDataLocker)
			{
				_messageLoopInputData.LeftMouse = false;
				int cursorX3 = (int)lParam % 65536;
				int cursorY3 = (int)(lParam / 65536);
				_messageLoopInputData.CursorX = cursorX3;
				_messageLoopInputData.CursorY = cursorY3;
				break;
			}
		case WindowMessage.LeftButtonDown:
			lock (_inputDataLocker)
			{
				_messageLoopInputData.LeftMouse = true;
				int cursorX2 = (int)lParam % 65536;
				int cursorY2 = (int)(lParam / 65536);
				_messageLoopInputData.CursorX = cursorX2;
				_messageLoopInputData.CursorY = cursorY2;
				break;
			}
		case WindowMessage.MouseMove:
			lock (_inputDataLocker)
			{
				_messageLoopInputData.MouseMove = true;
				int cursorX = (int)lParam % 65536;
				int cursorY = (int)(lParam / 65536);
				_messageLoopInputData.CursorX = cursorX;
				_messageLoopInputData.CursorY = cursorY;
				break;
			}
		case WindowMessage.MouseWheel:
			lock (_inputDataLocker)
			{
				short num = (short)(wParam >> 16);
				_messageLoopInputData.MouseScrollDelta = num;
				break;
			}
		case WindowMessage.KillFocus:
			lock (_inputDataLocker)
			{
				for (int i = 0; i < 256; i++)
				{
					_messageLoopInputData.KeyData[i] = false;
					_messageLoopInputData.RightMouse = false;
					_messageLoopInputData.LeftMouse = false;
				}
				break;
			}
		case WindowMessage.SetFocus:
			lock (_inputDataLocker)
			{
				break;
			}
		}
	}
}
