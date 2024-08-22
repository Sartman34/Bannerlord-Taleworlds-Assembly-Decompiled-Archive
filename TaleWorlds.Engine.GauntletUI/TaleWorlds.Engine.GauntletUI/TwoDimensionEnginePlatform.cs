using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.Engine.GauntletUI;

public class TwoDimensionEnginePlatform : ITwoDimensionPlatform
{
	public struct MaterialTuple : IEquatable<MaterialTuple>
	{
		public Texture Texture;

		public Texture OverlayTexture;

		public bool UseCustomMesh;

		public MaterialTuple(Texture texture, Texture overlayTexture, bool customMesh)
		{
			Texture = texture;
			OverlayTexture = overlayTexture;
			UseCustomMesh = customMesh;
		}

		public bool Equals(MaterialTuple other)
		{
			if (other.Texture == Texture && other.OverlayTexture == OverlayTexture)
			{
				return other.UseCustomMesh == UseCustomMesh;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hashCode = Texture.GetHashCode();
			hashCode = ((OverlayTexture != null) ? ((hashCode * 397) ^ OverlayTexture.GetHashCode()) : hashCode);
			return (hashCode * 397) ^ UseCustomMesh.GetHashCode();
		}
	}

	private TwoDimensionView _view;

	private ScissorTestInfo _currentScissorTestInfo;

	private bool _scissorSet;

	private Dictionary<MaterialTuple, Material> _materials;

	private Dictionary<Texture, Material> _textMaterials;

	private Dictionary<string, SoundEvent> _soundEvents;

	float ITwoDimensionPlatform.Width => Screen.RealScreenResolutionWidth * ScreenManager.UsableArea.X;

	float ITwoDimensionPlatform.Height => Screen.RealScreenResolutionHeight * ScreenManager.UsableArea.Y;

	float ITwoDimensionPlatform.ReferenceWidth => 1920f;

	float ITwoDimensionPlatform.ReferenceHeight => 1080f;

	float ITwoDimensionPlatform.ApplicationTime => Time.ApplicationTime;

	public TwoDimensionEnginePlatform(TwoDimensionView view)
	{
		_view = view;
		_scissorSet = false;
		_materials = new Dictionary<MaterialTuple, Material>();
		_textMaterials = new Dictionary<Texture, Material>();
		_soundEvents = new Dictionary<string, SoundEvent>();
	}

	private Material GetOrCreateMaterial(Texture texture, Texture overlayTexture, bool useCustomMesh, bool useOverlayTextureAlphaAsMask)
	{
		MaterialTuple key = new MaterialTuple(texture, overlayTexture, useCustomMesh);
		if (_materials.TryGetValue(key, out var value))
		{
			return value;
		}
		Material material = Material.GetFromResource("two_dimension_simple_material").CreateCopy();
		material.SetTexture(Material.MBTextureType.DiffuseMap, texture);
		if (overlayTexture != null)
		{
			material.AddMaterialShaderFlag("use_overlay_texture", showErrors: true);
			if (useOverlayTextureAlphaAsMask)
			{
				material.AddMaterialShaderFlag("use_overlay_texture_alpha_as_mask", showErrors: true);
			}
			material.SetTexture(Material.MBTextureType.DiffuseMap2, overlayTexture);
		}
		if (useCustomMesh)
		{
			material.AddMaterialShaderFlag("use_custom_mesh", showErrors: true);
		}
		_materials.Add(key, material);
		return material;
	}

	private Material GetOrCreateTextMaterial(Texture texture)
	{
		if (_textMaterials.TryGetValue(texture, out var value))
		{
			return value;
		}
		Material material = Material.GetFromResource("two_dimension_text_material").CreateCopy();
		material.SetTexture(Material.MBTextureType.DiffuseMap, texture);
		_textMaterials.Add(texture, material);
		return material;
	}

	void ITwoDimensionPlatform.Draw(float x, float y, TaleWorlds.TwoDimension.Material material, DrawObject2D mesh, int layer)
	{
		Vec2 clipRectPosition = new Vec2(0f, 0f);
		Vec2 clipRectSize = new Vec2(Screen.RealScreenResolutionWidth, Screen.RealScreenResolutionHeight);
		if (_scissorSet)
		{
			clipRectPosition = new Vec2(_currentScissorTestInfo.X, _currentScissorTestInfo.Y);
			clipRectSize = new Vec2(_currentScissorTestInfo.Width, _currentScissorTestInfo.Height);
		}
		if (material is SimpleMaterial)
		{
			SimpleMaterial simpleMaterial = (SimpleMaterial)material;
			TaleWorlds.TwoDimension.Texture texture = simpleMaterial.Texture;
			if (texture == null)
			{
				return;
			}
			Texture texture2 = ((EngineTexture)texture.PlatformTexture).Texture;
			if (!(texture2 != null))
			{
				return;
			}
			Material material2 = null;
			Vec2 startCoordinate = Vec2.Zero;
			Vec2 size = Vec2.Zero;
			float overlayTextureWidth = 512f;
			float overlayTextureHeight = 512f;
			float overlayXOffset = 0f;
			float overlayYOffset = 0f;
			if (simpleMaterial.OverlayEnabled)
			{
				Texture texture3 = ((EngineTexture)simpleMaterial.OverlayTexture.PlatformTexture).Texture;
				material2 = GetOrCreateMaterial(texture2, texture3, mesh.DrawObjectType == DrawObjectType.Mesh, simpleMaterial.UseOverlayAlphaAsMask);
				startCoordinate = simpleMaterial.StartCoordinate;
				size = simpleMaterial.Size;
				overlayTextureWidth = simpleMaterial.OverlayTextureWidth;
				overlayTextureHeight = simpleMaterial.OverlayTextureHeight;
				overlayXOffset = simpleMaterial.OverlayXOffset;
				overlayYOffset = simpleMaterial.OverlayYOffset;
			}
			if (material2 == null)
			{
				material2 = GetOrCreateMaterial(texture2, null, mesh.DrawObjectType == DrawObjectType.Mesh, useOverlayTextureAlphaAsMask: false);
			}
			uint color = simpleMaterial.Color.ToUnsignedInteger();
			float colorFactor = simpleMaterial.ColorFactor;
			float alphaFactor = simpleMaterial.AlphaFactor;
			float hueFactor = simpleMaterial.HueFactor;
			float saturationFactor = simpleMaterial.SaturationFactor;
			float valueFactor = simpleMaterial.ValueFactor;
			Vec2 clipCircleCenter = Vec2.Zero;
			float clipCircleRadius = 0f;
			float clipCircleSmoothingRadius = 0f;
			if (simpleMaterial.CircularMaskingEnabled)
			{
				clipCircleCenter = simpleMaterial.CircularMaskingCenter;
				clipCircleRadius = simpleMaterial.CircularMaskingRadius;
				clipCircleSmoothingRadius = simpleMaterial.CircularMaskingSmoothingRadius;
			}
			float[] vertices = mesh.Vertices;
			float[] textureCoordinates = mesh.TextureCoordinates;
			uint[] indices = mesh.Indices;
			int vertexCount = mesh.VertexCount;
			TwoDimensionMeshDrawData meshDrawData = default(TwoDimensionMeshDrawData);
			meshDrawData.ScreenWidth = Screen.RealScreenResolutionWidth;
			meshDrawData.ScreenHeight = Screen.RealScreenResolutionHeight;
			meshDrawData.DrawX = x;
			meshDrawData.DrawY = y;
			meshDrawData.ClipRectPosition = clipRectPosition;
			meshDrawData.ClipRectSize = clipRectSize;
			meshDrawData.Layer = layer;
			meshDrawData.Width = mesh.Width;
			meshDrawData.Height = mesh.Height;
			meshDrawData.MinU = mesh.MinU;
			meshDrawData.MinV = mesh.MinV;
			meshDrawData.MaxU = mesh.MaxU;
			meshDrawData.MaxV = mesh.MaxV;
			meshDrawData.ClipCircleCenter = clipCircleCenter;
			meshDrawData.ClipCircleRadius = clipCircleRadius;
			meshDrawData.ClipCircleSmoothingRadius = clipCircleSmoothingRadius;
			meshDrawData.Color = color;
			meshDrawData.ColorFactor = colorFactor;
			meshDrawData.AlphaFactor = alphaFactor;
			meshDrawData.HueFactor = hueFactor;
			meshDrawData.SaturationFactor = saturationFactor;
			meshDrawData.ValueFactor = valueFactor;
			meshDrawData.OverlayTextureWidth = overlayTextureWidth;
			meshDrawData.OverlayTextureHeight = overlayTextureHeight;
			meshDrawData.OverlayXOffset = overlayXOffset;
			meshDrawData.OverlayYOffset = overlayYOffset;
			meshDrawData.StartCoordinate = startCoordinate;
			meshDrawData.Size = size;
			meshDrawData.Type = (uint)mesh.DrawObjectType;
			if (!MBDebug.DisableAllUI)
			{
				if (mesh.DrawObjectType == DrawObjectType.Quad)
				{
					_view.CreateMeshFromDescription(material2, meshDrawData);
				}
				else
				{
					_view.CreateMeshFromDescription(vertices, textureCoordinates, indices, vertexCount, material2, meshDrawData);
				}
			}
		}
		else
		{
			if (!(material is TextMaterial))
			{
				return;
			}
			TextMaterial textMaterial = (TextMaterial)material;
			uint color2 = textMaterial.Color.ToUnsignedInteger();
			TaleWorlds.TwoDimension.Texture texture4 = textMaterial.Texture;
			if (texture4 == null)
			{
				return;
			}
			Texture texture5 = ((EngineTexture)texture4.PlatformTexture).Texture;
			if (texture5 != null)
			{
				Material orCreateTextMaterial = GetOrCreateTextMaterial(texture5);
				TwoDimensionTextMeshDrawData meshDrawData2 = default(TwoDimensionTextMeshDrawData);
				meshDrawData2.DrawX = x;
				meshDrawData2.DrawY = y;
				meshDrawData2.ScreenWidth = Screen.RealScreenResolutionWidth;
				meshDrawData2.ScreenHeight = Screen.RealScreenResolutionHeight;
				meshDrawData2.Color = color2;
				meshDrawData2.ScaleFactor = 1.5f / textMaterial.ScaleFactor;
				meshDrawData2.SmoothingConstant = textMaterial.SmoothingConstant;
				meshDrawData2.GlowColor = textMaterial.GlowColor.ToUnsignedInteger();
				meshDrawData2.OutlineColor = textMaterial.OutlineColor.ToVec3();
				meshDrawData2.OutlineAmount = textMaterial.OutlineAmount;
				meshDrawData2.GlowRadius = textMaterial.GlowRadius;
				meshDrawData2.Blur = textMaterial.Blur;
				meshDrawData2.ShadowOffset = textMaterial.ShadowOffset;
				meshDrawData2.ShadowAngle = textMaterial.ShadowAngle;
				meshDrawData2.ColorFactor = textMaterial.ColorFactor;
				meshDrawData2.AlphaFactor = textMaterial.AlphaFactor;
				meshDrawData2.HueFactor = textMaterial.HueFactor;
				meshDrawData2.SaturationFactor = textMaterial.SaturationFactor;
				meshDrawData2.ValueFactor = textMaterial.ValueFactor;
				meshDrawData2.Layer = layer;
				meshDrawData2.ClipRectPosition = clipRectPosition;
				meshDrawData2.ClipRectSize = clipRectSize;
				meshDrawData2.HashCode1 = mesh.HashCode1;
				meshDrawData2.HashCode2 = mesh.HashCode2;
				if (!MBDebug.DisableAllUI && !_view.CreateTextMeshFromCache(orCreateTextMaterial, meshDrawData2))
				{
					_view.CreateTextMeshFromDescription(mesh.Vertices, mesh.TextureCoordinates, mesh.Indices, mesh.VertexCount, orCreateTextMaterial, meshDrawData2);
				}
			}
		}
	}

	void ITwoDimensionPlatform.SetScissor(ScissorTestInfo scissorTestInfo)
	{
		_currentScissorTestInfo = scissorTestInfo;
		_scissorSet = true;
	}

	void ITwoDimensionPlatform.ResetScissor()
	{
		_scissorSet = false;
	}

	void ITwoDimensionPlatform.PlaySound(string soundName)
	{
		SoundEvent.PlaySound2D("event:/ui/" + soundName);
	}

	void ITwoDimensionPlatform.CreateSoundEvent(string soundName)
	{
		if (!_soundEvents.ContainsKey(soundName))
		{
			SoundEvent value = SoundEvent.CreateEventFromString("event:/ui/" + soundName, null);
			_soundEvents.Add(soundName, value);
		}
	}

	void ITwoDimensionPlatform.PlaySoundEvent(string soundName)
	{
		if (_soundEvents.TryGetValue(soundName, out var value))
		{
			value.Play();
		}
	}

	void ITwoDimensionPlatform.StopAndRemoveSoundEvent(string soundName)
	{
		if (_soundEvents.TryGetValue(soundName, out var value))
		{
			value.Stop();
			_soundEvents.Remove(soundName);
		}
	}

	void ITwoDimensionPlatform.OpenOnScreenKeyboard(string initialText, string descriptionText, int maxLength, int keyboardTypeEnum)
	{
		ScreenManager.OnPlatformScreenKeyboardRequested(initialText, descriptionText, maxLength, keyboardTypeEnum);
	}

	void ITwoDimensionPlatform.BeginDebugPanel(string panelTitle)
	{
		Imgui.BeginMainThreadScope();
		Imgui.Begin(panelTitle);
	}

	void ITwoDimensionPlatform.EndDebugPanel()
	{
		Imgui.End();
		Imgui.EndMainThreadScope();
	}

	void ITwoDimensionPlatform.DrawDebugText(string text)
	{
		Imgui.Text(text);
	}

	bool ITwoDimensionPlatform.DrawDebugTreeNode(string text)
	{
		return Imgui.TreeNode(text);
	}

	void ITwoDimensionPlatform.PopDebugTreeNode()
	{
		Imgui.TreePop();
	}

	void ITwoDimensionPlatform.DrawCheckbox(string label, ref bool isChecked)
	{
		Imgui.Checkbox(label, ref isChecked);
	}

	bool ITwoDimensionPlatform.IsDebugItemHovered()
	{
		return Imgui.IsItemHovered();
	}

	bool ITwoDimensionPlatform.IsDebugModeEnabled()
	{
		return UIConfig.DebugModeEnabled;
	}

	public void Reset()
	{
	}
}
