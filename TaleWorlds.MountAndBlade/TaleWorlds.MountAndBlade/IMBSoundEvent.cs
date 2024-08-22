using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBSoundEvent
{
	[EngineMethod("create_event_from_external_file", false)]
	int CreateEventFromExternalFile(string programmerSoundEventName, string filePath, UIntPtr scene);

	[EngineMethod("create_event_from_sound_buffer", false)]
	int CreateEventFromSoundBuffer(string programmerSoundEventName, byte[] soundBuffer, UIntPtr scene);

	[EngineMethod("play_sound", false)]
	bool PlaySound(int fmodEventIndex, ref Vec3 position);

	[EngineMethod("play_sound_with_int_param", false)]
	bool PlaySoundWithIntParam(int fmodEventIndex, int paramIndex, float paramVal, ref Vec3 position);

	[EngineMethod("play_sound_with_str_param", false)]
	bool PlaySoundWithStrParam(int fmodEventIndex, string paramName, float paramVal, ref Vec3 position);

	[EngineMethod("play_sound_with_param", false)]
	bool PlaySoundWithParam(int soundCodeId, SoundEventParameter parameter, ref Vec3 position);
}
