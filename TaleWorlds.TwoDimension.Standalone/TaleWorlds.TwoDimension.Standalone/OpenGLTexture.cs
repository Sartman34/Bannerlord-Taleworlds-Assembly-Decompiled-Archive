using System.IO;
using StbSharp;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension.Standalone.Native.OpenGL;

namespace TaleWorlds.TwoDimension.Standalone;

public class OpenGLTexture : ITexture
{
	private int _width;

	private int _height;

	private GraphicsContext _context;

	private int _id;

	private bool _clampToEdge;

	public int Width => _width;

	public int Height => _height;

	internal static OpenGLTexture ActiveTexture { get; private set; }

	internal int Id => _id;

	public string Name { get; private set; }

	public bool ClampToEdge
	{
		get
		{
			return _clampToEdge;
		}
		set
		{
			_clampToEdge = value;
			if (ActiveTexture != this)
			{
				MakeActive();
			}
			else
			{
				SetTextureParameters();
			}
		}
	}

	public void Initialize(string name, int width, int height)
	{
		_context = GraphicsContext.Active;
		Name = name;
		_id = 0;
		Opengl32.GenTextures(1, ref _id);
		_width = width;
		_height = height;
	}

	public void CopyFrom(OpenGLTexture texture)
	{
		_width = texture._width;
		_height = texture._height;
		Name = texture.Name;
		_id = texture._id;
		_context = texture._context;
	}

	public void Delete()
	{
		Opengl32.DeleteTextures(1, new int[1] { _id });
		Debug.Print("texture deleted! : " + Name);
	}

	internal void MakeActive()
	{
		if (ActiveTexture != this)
		{
			Opengl32.BindTexture(Target.Texture2D, _id);
			ActiveTexture = this;
			SetTextureParameters();
		}
	}

	private void SetTextureParameters()
	{
		Opengl32.TexParameteri(Target.Texture2D, TextureParameterName.TextureMinFilter, 9729);
		Opengl32.TexParameteri(Target.Texture2D, TextureParameterName.TextureMagFilter, 9729);
		if (ClampToEdge)
		{
			Opengl32.TexParameteri(Target.Texture2D, TextureParameterName.TextureWrapS, 33071);
			Opengl32.TexParameteri(Target.Texture2D, TextureParameterName.TextureWrapT, 33071);
		}
	}

	public static OpenGLTexture FromFile(ResourceDepot resourceDepot, string name)
	{
		OpenGLTexture openGLTexture = new OpenGLTexture();
		openGLTexture.LoadFromFile(resourceDepot, name);
		return openGLTexture;
	}

	public static OpenGLTexture FromFile(string fullFilePath)
	{
		OpenGLTexture openGLTexture = new OpenGLTexture();
		openGLTexture.LoadFromFile(fullFilePath);
		return openGLTexture;
	}

	public void Release()
	{
		Delete();
	}

	public void LoadFromFile(ResourceDepot resourceDepot, string name)
	{
		string filePath = resourceDepot.GetFilePath(name + ".png");
		LoadFromFile(filePath);
	}

	public void LoadFromFile(string fullPathName)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!File.Exists(fullPathName))
		{
			Debug.Print("File not found: " + fullPathName);
			return;
		}
		Image val = null;
		using (Stream stream = new MemoryStream(File.ReadAllBytes(fullPathName)))
		{
			val = new ImageReader().Read(stream, 0);
		}
		if (val == null)
		{
			Debug.Print("Error while reading file: " + fullPathName);
			return;
		}
		int width = val.Width;
		int height = val.Height;
		Initialize(Path.GetFileName(fullPathName), width, height);
		MakeActive();
		PixelFormat format = PixelFormat.Red;
		uint internalformat = 0u;
		bool flag = true;
		switch (val.Comp)
		{
		case 1:
			format = PixelFormat.Red;
			internalformat = 33321u;
			break;
		case 3:
			format = PixelFormat.RGB;
			internalformat = 32849u;
			break;
		case 4:
			format = PixelFormat.RGBA;
			internalformat = 32856u;
			break;
		default:
			flag = false;
			Debug.Print("Unknown image format at file: " + fullPathName + ". Supported formats are: Single-Channel, RGB and RGBA.");
			break;
		}
		if (flag)
		{
			Opengl32.TexImage2D(Target.Texture2D, 0, internalformat, width, height, 0, format, DataType.UnsignedByte, val.Data);
		}
	}

	public bool IsLoaded()
	{
		return true;
	}
}
