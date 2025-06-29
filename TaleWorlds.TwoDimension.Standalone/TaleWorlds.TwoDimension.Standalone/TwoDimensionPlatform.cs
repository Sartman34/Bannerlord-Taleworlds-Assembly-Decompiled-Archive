using System;
using TaleWorlds.Library;

namespace TaleWorlds.TwoDimension.Standalone;

public class TwoDimensionPlatform : ITwoDimensionPlatform, ITwoDimensionResourceContext
{
	private GraphicsContext _graphicsContext;

	private GraphicsForm _form;

	private bool _isAssetsUnderDefaultFolders;

	float ITwoDimensionPlatform.Width => _form.Width;

	float ITwoDimensionPlatform.Height => _form.Height;

	float ITwoDimensionPlatform.ReferenceWidth => _form.Width;

	float ITwoDimensionPlatform.ReferenceHeight => _form.Height;

	float ITwoDimensionPlatform.ApplicationTime => Environment.TickCount;

	public TwoDimensionPlatform(GraphicsForm form, bool isAssetsUnderDefaultFolders)
	{
		_form = form;
		_isAssetsUnderDefaultFolders = isAssetsUnderDefaultFolders;
		_graphicsContext = _form.GraphicsContext;
	}

	void ITwoDimensionPlatform.Draw(float x, float y, Material material, DrawObject2D drawObject2D, int layer)
	{
		_graphicsContext.DrawElements(x, y, material, drawObject2D);
	}

	Texture ITwoDimensionResourceContext.LoadTexture(ResourceDepot resourceDepot, string name)
	{
		OpenGLTexture openGLTexture = new OpenGLTexture();
		string name2 = name;
		if (!_isAssetsUnderDefaultFolders)
		{
			string[] array = name.Split(new char[1] { '\\' });
			name2 = array[array.Length - 1];
		}
		openGLTexture.LoadFromFile(resourceDepot, name2);
		return new Texture(openGLTexture);
	}

	void ITwoDimensionPlatform.PlaySound(string soundName)
	{
		Debug.Print("Playing sound: " + soundName);
	}

	void ITwoDimensionPlatform.SetScissor(ScissorTestInfo scissorTestInfo)
	{
		_graphicsContext.SetScissor(scissorTestInfo);
	}

	void ITwoDimensionPlatform.ResetScissor()
	{
		_graphicsContext.ResetScissor();
	}

	void ITwoDimensionPlatform.CreateSoundEvent(string soundName)
	{
		Debug.Print("Created sound event: " + soundName);
	}

	void ITwoDimensionPlatform.StopAndRemoveSoundEvent(string soundName)
	{
		Debug.Print("Stopped sound event: " + soundName);
	}

	void ITwoDimensionPlatform.PlaySoundEvent(string soundName)
	{
		Debug.Print("Played sound event: " + soundName);
	}

	void ITwoDimensionPlatform.OpenOnScreenKeyboard(string initialText, string descriptionText, int maxLength, int keyboardTypeEnum)
	{
		Debug.Print("Opened on-screen keyboard");
	}

	void ITwoDimensionPlatform.BeginDebugPanel(string panelTitle)
	{
	}

	void ITwoDimensionPlatform.EndDebugPanel()
	{
	}

	void ITwoDimensionPlatform.DrawDebugText(string text)
	{
		Debug.Print(text);
	}

	bool ITwoDimensionPlatform.IsDebugModeEnabled()
	{
		return false;
	}

	bool ITwoDimensionPlatform.DrawDebugTreeNode(string text)
	{
		return false;
	}

	void ITwoDimensionPlatform.DrawCheckbox(string label, ref bool isChecked)
	{
	}

	bool ITwoDimensionPlatform.IsDebugItemHovered()
	{
		return false;
	}

	void ITwoDimensionPlatform.PopDebugTreeNode()
	{
	}
}
