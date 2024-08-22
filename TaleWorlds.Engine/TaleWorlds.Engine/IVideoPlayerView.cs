using System;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IVideoPlayerView
{
	[EngineMethod("create_video_player_view", false)]
	VideoPlayerView CreateVideoPlayerView();

	[EngineMethod("play_video", false)]
	void PlayVideo(UIntPtr pointer, string videoFileName, string soundFileName, float framerate);

	[EngineMethod("stop_video", false)]
	void StopVideo(UIntPtr pointer);

	[EngineMethod("is_video_finished", false)]
	bool IsVideoFinished(UIntPtr pointer);

	[EngineMethod("finalize", false)]
	void Finalize(UIntPtr pointer);
}
