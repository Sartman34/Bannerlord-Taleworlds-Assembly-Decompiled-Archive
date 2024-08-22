using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface ISoundManager
{
	[EngineMethod("set_listener_frame", false)]
	void SetListenerFrame(ref MatrixFrame frame, ref Vec3 attenuationPosition);

	[EngineMethod("get_listener_frame", false)]
	void GetListenerFrame(out MatrixFrame result);

	[EngineMethod("get_attenuation_position", false)]
	void GetAttenuationPosition(out Vec3 result);

	[EngineMethod("reset", false)]
	void Reset();

	[EngineMethod("start_one_shot_event_with_param", false)]
	bool StartOneShotEventWithParam(string eventFullName, Vec3 position, string paramName, float paramValue);

	[EngineMethod("start_one_shot_event", false)]
	bool StartOneShotEvent(string eventFullName, Vec3 position);

	[EngineMethod("set_state", false)]
	void SetState(string stateGroup, string state);

	[EngineMethod("load_event_file_aux", false)]
	void LoadEventFileAux(string soundBankName, bool decompressSamples);

	[EngineMethod("set_global_parameter", false)]
	void SetGlobalParameter(string parameterName, float value);

	[EngineMethod("add_sound_client_with_id", false)]
	void AddSoundClientWithId(ulong client_id);

	[EngineMethod("delete_sound_client_with_id", false)]
	void DeleteSoundClientWithId(ulong client_id);

	[EngineMethod("get_global_index_of_event", false)]
	int GetGlobalIndexOfEvent(string eventFullName);

	[EngineMethod("create_voice_event", false)]
	void CreateVoiceEvent();

	[EngineMethod("destroy_voice_event", false)]
	void DestroyVoiceEvent(int id);

	[EngineMethod("init_voice_play_event", false)]
	void InitializeVoicePlayEvent();

	[EngineMethod("finalize_voice_play_event", false)]
	void FinalizeVoicePlayEvent();

	[EngineMethod("start_voice_record", false)]
	void StartVoiceRecord();

	[EngineMethod("stop_voice_record", false)]
	void StopVoiceRecord();

	[EngineMethod("get_voice_data", false)]
	void GetVoiceData(byte[] voiceBuffer, int chunkSize, ref int readBytesLength);

	[EngineMethod("update_voice_to_play", false)]
	void UpdateVoiceToPlay(byte[] voiceBuffer, int length, int index);

	[EngineMethod("compress_voice_data", false)]
	void CompressData(ulong clientID, byte[] buffer, int length, byte[] compressedBuffer, ref int compressedBufferLength);

	[EngineMethod("decompress_voice_data", false)]
	void DecompressData(ulong clientID, byte[] compressedBuffer, int compressedBufferLength, byte[] decompressedBuffer, ref int decompressedBufferLength);

	[EngineMethod("remove_xbox_remote_user", false)]
	void RemoveXBOXRemoteUser(ulong XUID);

	[EngineMethod("add_xbox_remote_user", false)]
	void AddXBOXRemoteUser(ulong XUID, ulong deviceID, bool canSendMicSound, bool canSendTextSound, bool canSendText, bool canReceiveSound, bool canReceiveText);

	[EngineMethod("initialize_xbox_sound_manager", false)]
	void InitializeXBOXSoundManager();

	[EngineMethod("apply_push_to_talk", false)]
	void ApplyPushToTalk(bool pushed);

	[EngineMethod("clear_xbox_sound_manager", false)]
	void ClearXBOXSoundManager();

	[EngineMethod("update_xbox_local_user", false)]
	void UpdateXBOXLocalUser();

	[EngineMethod("update_xbox_chat_communication_flags", false)]
	void UpdateXBOXChatCommunicationFlags(ulong XUID, bool canSendMicSound, bool canSendTextSound, bool canSendText, bool canReceiveSound, bool canReceiveText);

	[EngineMethod("process_data_to_be_received", false)]
	void ProcessDataToBeReceived(ulong senderDeviceID, byte[] data, uint dataSize);

	[EngineMethod("process_data_to_be_sent", false)]
	void ProcessDataToBeSent(ref int numData);

	[EngineMethod("handle_state_changes", false)]
	void HandleStateChanges();

	[EngineMethod("get_size_of_data_to_be_sent_at", false)]
	void GetSizeOfDataToBeSentAt(int index, ref uint byte_count, ref uint numReceivers);

	[EngineMethod("get_data_to_be_sent_at", false)]
	bool GetDataToBeSentAt(int index, byte[] buffer, ulong[] receivers, ref bool transportGuaranteed);

	[EngineMethod("clear_data_to_be_sent", false)]
	void ClearDataToBeSent();
}
