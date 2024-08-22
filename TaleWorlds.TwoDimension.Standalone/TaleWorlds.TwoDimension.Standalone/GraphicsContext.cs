using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension.Standalone.Native;
using TaleWorlds.TwoDimension.Standalone.Native.OpenGL;
using TaleWorlds.TwoDimension.Standalone.Native.Windows;

namespace TaleWorlds.TwoDimension.Standalone;

public class GraphicsContext
{
	public const int MaxFrameRate = 60;

	public readonly int MaxTimeToRenderOneFrame;

	private IntPtr _handleDeviceContext;

	private IntPtr _handleRenderContext;

	private int[] _scissorParameters = new int[4];

	private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

	private Matrix4x4 _modelMatrix = Matrix4x4.Identity;

	private Matrix4x4 _viewMatrix = Matrix4x4.Identity;

	private Matrix4x4 _modelViewMatrix = Matrix4x4.Identity;

	private Stopwatch _stopwatch;

	private Dictionary<string, Shader> _loadedShaders;

	private VertexArrayObject _simpleVAO;

	private VertexArrayObject _textureVAO;

	private int _screenWidth;

	private int _screenHeight;

	private ResourceDepot _resourceDepot;

	private bool _blendingMode;

	private bool _vertexArrayClientState;

	private bool _textureCoordArrayClientState;

	internal WindowsForm Control { get; set; }

	public static GraphicsContext Active { get; private set; }

	internal Dictionary<string, OpenGLTexture> LoadedTextures { get; private set; }

	public Matrix4x4 ProjectionMatrix
	{
		get
		{
			return _projectionMatrix;
		}
		set
		{
			_projectionMatrix = value;
		}
	}

	public Matrix4x4 ViewMatrix
	{
		get
		{
			return _viewMatrix;
		}
		set
		{
			_viewMatrix = value;
			_modelViewMatrix = _viewMatrix * _modelMatrix;
		}
	}

	public Matrix4x4 ModelMatrix
	{
		get
		{
			return _modelMatrix;
		}
		set
		{
			_modelMatrix = value;
			_modelViewMatrix = _viewMatrix * _modelMatrix;
		}
	}

	public bool IsActive => Active == this;

	public GraphicsContext()
	{
		LoadedTextures = new Dictionary<string, OpenGLTexture>();
		_loadedShaders = new Dictionary<string, Shader>();
		_stopwatch = new Stopwatch();
		MaxTimeToRenderOneFrame = 16;
	}

	public void CreateContext(ResourceDepot resourceDepot)
	{
		_resourceDepot = resourceDepot;
		_handleDeviceContext = User32.GetDC(Control.Handle);
		if (_handleDeviceContext == IntPtr.Zero)
		{
			TaleWorlds.Library.Debug.Print("Can't get device context");
		}
		if (!Opengl32.wglMakeCurrent(_handleDeviceContext, IntPtr.Zero))
		{
			TaleWorlds.Library.Debug.Print("Can't reset context");
		}
		PixelFormatDescriptor ppfd = default(PixelFormatDescriptor);
		Marshal.SizeOf(typeof(PixelFormatDescriptor));
		ppfd.nSize = (ushort)Marshal.SizeOf(typeof(PixelFormatDescriptor));
		ppfd.nVersion = 1;
		ppfd.dwFlags = 32805u;
		ppfd.iPixelType = 0;
		ppfd.cColorBits = 32;
		ppfd.cRedBits = 0;
		ppfd.cRedShift = 0;
		ppfd.cGreenBits = 0;
		ppfd.cGreenShift = 0;
		ppfd.cBlueBits = 0;
		ppfd.cBlueShift = 0;
		ppfd.cAlphaBits = 8;
		ppfd.cAlphaShift = 0;
		ppfd.cAccumBits = 0;
		ppfd.cAccumRedBits = 0;
		ppfd.cAccumGreenBits = 0;
		ppfd.cAccumBlueBits = 0;
		ppfd.cAccumAlphaBits = 0;
		ppfd.cDepthBits = 24;
		ppfd.cStencilBits = 0;
		ppfd.cAuxBuffers = 0;
		ppfd.iLayerType = 0;
		ppfd.bReserved = 0;
		ppfd.dwLayerMask = 0u;
		ppfd.dwVisibleMask = 0u;
		ppfd.dwDamageMask = 0u;
		int iPixelFormat = Gdi32.ChoosePixelFormat(_handleDeviceContext, ref ppfd);
		if (!Gdi32.SetPixelFormat(_handleDeviceContext, iPixelFormat, ref ppfd))
		{
			TaleWorlds.Library.Debug.Print("can't set pixel format");
		}
		_handleRenderContext = Opengl32.wglCreateContext(_handleDeviceContext);
		SetActive();
		Opengl32ARB.LoadExtensions();
		IntPtr handleRenderContext = _handleRenderContext;
		_handleRenderContext = IntPtr.Zero;
		Active = null;
		int[] array = new int[10];
		int num = 0;
		array[num++] = 8337;
		array[num++] = 3;
		array[num++] = 8338;
		array[num++] = 3;
		array[num++] = 37158;
		array[num++] = 1;
		array[num++] = 0;
		_handleRenderContext = Opengl32ARB.wglCreateContextAttribs(_handleDeviceContext, IntPtr.Zero, array);
		SetActive();
		Opengl32.wglDeleteContext(handleRenderContext);
		Opengl32.ShadeModel(ShadingModel.Smooth);
		Opengl32.ClearColor(0f, 0f, 0f, 0f);
		Opengl32.ClearDepth(1.0);
		Opengl32.Disable(Target.DepthTest);
		Opengl32.Hint(3152u, 4354u);
		ProjectionMatrix = Matrix4x4.Identity;
		ModelMatrix = Matrix4x4.Identity;
		ViewMatrix = Matrix4x4.Identity;
		_simpleVAO = VertexArrayObject.Create();
		_textureVAO = VertexArrayObject.CreateWithUVBuffer();
	}

	public void SetActive()
	{
		if (Active != this)
		{
			if (Opengl32.wglMakeCurrent(_handleDeviceContext, _handleRenderContext))
			{
				Active = this;
			}
			else
			{
				TaleWorlds.Library.Debug.Print("Can't activate context");
			}
		}
	}

	public void BeginFrame(int width, int height)
	{
		_stopwatch.Start();
		Resize(width, height);
		Opengl32.Clear(AttribueMask.ColorBufferBit);
		Opengl32.ClearDepth(1.0);
		Opengl32.Disable(Target.DepthTest);
		Opengl32.Disable(Target.SCISSOR_TEST);
		Opengl32.Disable(Target.STENCIL_TEST);
		Opengl32.Disable(Target.Blend);
	}

	public void SwapBuffers()
	{
		int num = (int)_stopwatch.ElapsedMilliseconds;
		int num2 = 0;
		if (MaxTimeToRenderOneFrame > num)
		{
			num2 = MaxTimeToRenderOneFrame - num;
		}
		if (num2 > 0)
		{
			Thread.Sleep(num2);
		}
		Gdi32.SwapBuffers(_handleDeviceContext);
		_stopwatch.Restart();
	}

	public void DestroyContext()
	{
		Opengl32.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
		Opengl32.wglDeleteContext(_handleRenderContext);
		User32.ReleaseDC(Control.Handle, _handleDeviceContext);
	}

	public void SetScissor(ScissorTestInfo scissorTestInfo)
	{
		Opengl32.GetInteger(Target.VIEWPORT, _scissorParameters);
		Opengl32.Scissor(scissorTestInfo.X, _scissorParameters[3] - scissorTestInfo.Height - scissorTestInfo.Y, scissorTestInfo.Width, scissorTestInfo.Height);
		Opengl32.Enable(Target.SCISSOR_TEST);
	}

	public void ResetScissor()
	{
		Opengl32.Disable(Target.SCISSOR_TEST);
	}

	public void DrawElements(float x, float y, Material material, DrawObject2D drawObject2D)
	{
		ModelMatrix = Matrix4x4.CreateTranslation(x, y, 0f);
		DrawElements(material, drawObject2D);
	}

	public Shader GetOrLoadShader(string shaderName)
	{
		if (!_loadedShaders.ContainsKey(shaderName))
		{
			string filePath = _resourceDepot.GetFilePath(shaderName + ".vert");
			string filePath2 = _resourceDepot.GetFilePath(shaderName + ".frag");
			string vertexShaderCode = File.ReadAllText(filePath);
			string fragmentShaderCode = File.ReadAllText(filePath2);
			Shader shader = Shader.CreateShader(this, vertexShaderCode, fragmentShaderCode);
			_loadedShaders.Add(shaderName, shader);
			return shader;
		}
		return _loadedShaders[shaderName];
	}

	public void DrawElements(Material material, DrawObject2D drawObject2D)
	{
		bool blending = material.Blending;
		Shader orLoadShader = GetOrLoadShader(material.GetType().Name);
		orLoadShader.Use();
		Matrix4x4 matrix = _modelMatrix * _viewMatrix * _projectionMatrix;
		orLoadShader.SetMatrix("MVP", matrix);
		MeshTopology topology = drawObject2D.Topology;
		if (material is SimpleMaterial)
		{
			SimpleMaterial simpleMaterial = (SimpleMaterial)material;
			if (simpleMaterial.Texture != null)
			{
				OpenGLTexture texture = simpleMaterial.Texture.PlatformTexture as OpenGLTexture;
				orLoadShader.SetTexture("Texture", texture);
			}
			orLoadShader.SetBoolean("OverlayEnabled", simpleMaterial.OverlayEnabled);
			if (simpleMaterial.OverlayEnabled)
			{
				OpenGLTexture texture2 = simpleMaterial.OverlayTexture.PlatformTexture as OpenGLTexture;
				orLoadShader.SetVector2("StartCoord", simpleMaterial.StartCoordinate);
				orLoadShader.SetVector2("Size", simpleMaterial.Size);
				orLoadShader.SetTexture("OverlayTexture", texture2);
				orLoadShader.SetVector2("OverlayOffset", new Vector2(simpleMaterial.OverlayXOffset, simpleMaterial.OverlayYOffset));
			}
			float value = TaleWorlds.Library.MathF.Clamp(simpleMaterial.HueFactor / 360f, -0.5f, 0.5f);
			float value2 = TaleWorlds.Library.MathF.Clamp(simpleMaterial.SaturationFactor / 360f, -0.5f, 0.5f);
			float value3 = TaleWorlds.Library.MathF.Clamp(simpleMaterial.ValueFactor / 360f, -0.5f, 0.5f);
			orLoadShader.SetColor("InputColor", simpleMaterial.Color);
			orLoadShader.SetFloat("ColorFactor", simpleMaterial.ColorFactor);
			orLoadShader.SetFloat("AlphaFactor", simpleMaterial.AlphaFactor);
			orLoadShader.SetFloat("HueFactor", value);
			orLoadShader.SetFloat("SaturationFactor", value2);
			orLoadShader.SetFloat("ValueFactor", value3);
			_textureVAO.Bind();
			if (simpleMaterial.CircularMaskingEnabled)
			{
				orLoadShader.SetBoolean("CircularMaskingEnabled", value: true);
				orLoadShader.SetVector2("MaskingCenter", simpleMaterial.CircularMaskingCenter);
				orLoadShader.SetFloat("MaskingRadius", simpleMaterial.CircularMaskingRadius);
				orLoadShader.SetFloat("MaskingSmoothingRadius", simpleMaterial.CircularMaskingSmoothingRadius);
			}
			else
			{
				orLoadShader.SetBoolean("CircularMaskingEnabled", value: false);
			}
			_textureVAO.LoadVertexData(drawObject2D.Vertices);
			_textureVAO.LoadUVData(drawObject2D.TextureCoordinates);
			_textureVAO.LoadIndexData(drawObject2D.Indices);
		}
		else if (material is TextMaterial)
		{
			TextMaterial textMaterial = (TextMaterial)material;
			if (textMaterial.Texture != null)
			{
				OpenGLTexture texture3 = textMaterial.Texture.PlatformTexture as OpenGLTexture;
				orLoadShader.SetTexture("Texture", texture3);
			}
			orLoadShader.SetColor("InputColor", textMaterial.Color);
			orLoadShader.SetColor("GlowColor", textMaterial.GlowColor);
			orLoadShader.SetColor("OutlineColor", textMaterial.OutlineColor);
			orLoadShader.SetFloat("OutlineAmount", textMaterial.OutlineAmount);
			orLoadShader.SetFloat("ScaleFactor", 1.5f / textMaterial.ScaleFactor);
			orLoadShader.SetFloat("SmoothingConstant", textMaterial.SmoothingConstant);
			orLoadShader.SetFloat("GlowRadius", textMaterial.GlowRadius);
			orLoadShader.SetFloat("Blur", textMaterial.Blur);
			orLoadShader.SetFloat("ShadowOffset", textMaterial.ShadowOffset);
			orLoadShader.SetFloat("ShadowAngle", textMaterial.ShadowAngle);
			orLoadShader.SetFloat("ColorFactor", textMaterial.ColorFactor);
			orLoadShader.SetFloat("AlphaFactor", textMaterial.AlphaFactor);
			_textureVAO.Bind();
			_textureVAO.LoadVertexData(drawObject2D.Vertices);
			_textureVAO.LoadUVData(drawObject2D.TextureCoordinates);
			_textureVAO.LoadIndexData(drawObject2D.Indices);
		}
		else if (material is PrimitivePolygonMaterial)
		{
			Color color = ((PrimitivePolygonMaterial)material).Color;
			orLoadShader.SetColor("Color", color);
			_simpleVAO.Bind();
			_simpleVAO.LoadVertexData(drawObject2D.Vertices);
		}
		DrawElements(drawObject2D.Indices, topology, blending);
		VertexArrayObject.UnBind();
		orLoadShader.StopUsing();
	}

	private void DrawElements(uint[] indices, MeshTopology meshTopology, bool blending)
	{
		SetBlending(blending);
		using (new AutoPinner(indices))
		{
			BeginMode mode = BeginMode.Quads;
			switch (meshTopology)
			{
			case MeshTopology.Lines:
				mode = BeginMode.Lines;
				break;
			case MeshTopology.Triangles:
				mode = BeginMode.Triangles;
				break;
			}
			Opengl32.DrawElements(mode, indices.Length, DataType.UnsignedInt, null);
		}
	}

	internal void Resize(int width, int height)
	{
		if (!IsActive)
		{
			SetActive();
		}
		_screenWidth = width;
		_screenHeight = height;
		Opengl32.Viewport(0, 0, width, height);
	}

	public void LoadTextureUsing(OpenGLTexture texture, ResourceDepot resourceDepot, string name)
	{
		if (!LoadedTextures.ContainsKey(name))
		{
			texture.LoadFromFile(resourceDepot, name);
			LoadedTextures.Add(name, texture);
		}
		else
		{
			texture.CopyFrom(LoadedTextures[name]);
		}
	}

	public OpenGLTexture LoadTexture(ResourceDepot resourceDepot, string name)
	{
		OpenGLTexture openGLTexture = null;
		if (LoadedTextures.ContainsKey(name))
		{
			openGLTexture = LoadedTextures[name];
		}
		else
		{
			openGLTexture = OpenGLTexture.FromFile(resourceDepot, name);
			LoadedTextures.Add(name, openGLTexture);
		}
		return openGLTexture;
	}

	public OpenGLTexture GetTexture(string textureName)
	{
		OpenGLTexture result = null;
		if (LoadedTextures.ContainsKey(textureName))
		{
			result = LoadedTextures[textureName];
		}
		return result;
	}

	public void SetBlending(bool enable)
	{
		_blendingMode = enable;
		if (_blendingMode)
		{
			Opengl32.Enable(Target.Blend);
			Opengl32ARB.BlendFuncSeparate(BlendingSourceFactor.SourceAlpha, BlendingDestinationFactor.OneMinusSourceAlpha, BlendingSourceFactor.One, BlendingDestinationFactor.One);
		}
		else
		{
			Opengl32.Disable(Target.Blend);
		}
	}

	public void SetVertexArrayClientState(bool enable)
	{
		if (_vertexArrayClientState != enable)
		{
			_vertexArrayClientState = enable;
			if (_vertexArrayClientState)
			{
				Opengl32.EnableClientState(32884u);
			}
			else
			{
				Opengl32.DisableClientState(32884u);
			}
		}
	}

	public void SetTextureCoordArrayClientState(bool enable)
	{
		if (_textureCoordArrayClientState != enable)
		{
			_textureCoordArrayClientState = enable;
			if (_textureCoordArrayClientState)
			{
				Opengl32.EnableClientState(32888u);
			}
			else
			{
				Opengl32.DisableClientState(32888u);
			}
		}
	}
}
