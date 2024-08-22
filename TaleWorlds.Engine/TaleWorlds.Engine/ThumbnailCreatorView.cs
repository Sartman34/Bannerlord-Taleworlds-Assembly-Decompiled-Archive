using System;
using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineClass("rglThumbnail_creator_view")]
public sealed class ThumbnailCreatorView : View
{
	public delegate void OnThumbnailRenderCompleteDelegate(string renderId, Texture renderTarget);

	public static OnThumbnailRenderCompleteDelegate renderCallback;

	internal ThumbnailCreatorView(UIntPtr pointer)
		: base(pointer)
	{
	}

	[EngineCallback]
	internal static void OnThumbnailRenderComplete(string renderId, Texture renderTarget)
	{
		renderCallback(renderId, renderTarget);
	}

	public static ThumbnailCreatorView CreateThumbnailCreatorView()
	{
		return EngineApplicationInterface.IThumbnailCreatorView.CreateThumbnailCreatorView();
	}

	public void RegisterScene(Scene scene, bool usePostFx = true)
	{
		EngineApplicationInterface.IThumbnailCreatorView.RegisterScene(base.Pointer, scene.Pointer, usePostFx);
	}

	public void RegisterEntity(Scene scene, Camera cam, Texture texture, GameEntity itemEntity, int allocationGroupIndex, string renderId = "")
	{
		EngineApplicationInterface.IThumbnailCreatorView.RegisterEntity(base.Pointer, scene.Pointer, cam.Pointer, texture.Pointer, itemEntity.Pointer, renderId, allocationGroupIndex);
	}

	public void ClearRequests()
	{
		EngineApplicationInterface.IThumbnailCreatorView.ClearRequests(base.Pointer);
	}

	public void CancelRequest(string renderID)
	{
		EngineApplicationInterface.IThumbnailCreatorView.CancelRequest(base.Pointer, renderID);
	}

	public void RegisterEntityWithoutTexture(Scene scene, Camera camera, GameEntity entity, int width, int height, int allocationGroupIndex, string renderId = "", string debugName = "")
	{
		EngineApplicationInterface.IThumbnailCreatorView.RegisterEntityWithoutTexture(base.Pointer, scene.Pointer, camera.Pointer, entity.Pointer, width, height, renderId, debugName, allocationGroupIndex);
	}

	public int GetNumberOfPendingRequests()
	{
		return EngineApplicationInterface.IThumbnailCreatorView.GetNumberOfPendingRequests(base.Pointer);
	}

	public bool IsMemoryCleared()
	{
		return EngineApplicationInterface.IThumbnailCreatorView.IsMemoryCleared(base.Pointer);
	}
}
