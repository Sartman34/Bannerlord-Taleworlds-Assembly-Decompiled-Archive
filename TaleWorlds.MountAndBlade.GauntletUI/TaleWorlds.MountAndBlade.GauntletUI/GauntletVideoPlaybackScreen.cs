using System.Collections.Generic;
using System.IO;
using System.Text;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.VideoPlayback;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[GameStateScreen(typeof(VideoPlaybackState))]
public class GauntletVideoPlaybackScreen : VideoPlaybackScreen
{
	private GauntletLayer _layer;

	private VideoPlaybackVM _dataSource;

	public GauntletVideoPlaybackScreen(VideoPlaybackState videoPlaybackState)
		: base(videoPlaybackState)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		string subtitleExtensionOfLanguage = LocalizedTextManager.GetSubtitleExtensionOfLanguage(BannerlordConfig.Language);
		string text = _videoPlaybackState.SubtitleFileBasePath + "_" + subtitleExtensionOfLanguage + ".srt";
		List<SRTHelper.SubtitleItem> subtitles = null;
		if (!string.IsNullOrEmpty(_videoPlaybackState.SubtitleFileBasePath))
		{
			if (File.Exists(text))
			{
				subtitles = SRTHelper.SrtParser.ParseStream(new FileStream(text, FileMode.Open, FileAccess.Read), Encoding.UTF8);
			}
			else
			{
				Debug.FailedAssert("No Subtitle file exists in path: " + text, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletVideoPlaybackScreen.cs", "OnInitialize", 41);
			}
		}
		_layer = new GauntletLayer(100002);
		_dataSource = new VideoPlaybackVM();
		_layer.LoadMovie("VideoPlayer", _dataSource);
		_dataSource.SetSubtitles(subtitles);
		_layer.InputRestrictions.SetInputRestrictions(isMouseVisible: false);
		AddLayer(_layer);
	}

	protected override void OnVideoPlaybackTick(float dt)
	{
		base.OnVideoPlaybackTick(dt);
		_dataSource.Tick(_totalElapsedTimeSinceVideoStart);
	}
}
