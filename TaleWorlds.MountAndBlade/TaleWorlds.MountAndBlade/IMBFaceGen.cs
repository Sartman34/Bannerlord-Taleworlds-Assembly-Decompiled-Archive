using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBFaceGen
{
	[EngineMethod("get_num_editable_deform_keys", false)]
	int GetNumEditableDeformKeys(int race, bool initialGender, float age);

	[EngineMethod("get_params_from_key", false)]
	void GetParamsFromKey(ref FaceGenerationParams faceGenerationParams, ref BodyProperties bodyProperties, bool earsAreHidden, bool mouthHidden);

	[EngineMethod("get_params_max", false)]
	void GetParamsMax(int race, int curGender, float curAge, ref int hairNum, ref int beardNum, ref int faceTextureNum, ref int mouthTextureNum, ref int faceTattooNum, ref int soundNum, ref int eyebrowNum, ref float scale);

	[EngineMethod("get_zero_probabilities", false)]
	void GetZeroProbabilities(int race, int curGender, float curAge, ref float tattooZeroProbability);

	[EngineMethod("produce_numeric_key_with_params", false)]
	void ProduceNumericKeyWithParams(ref FaceGenerationParams faceGenerationParams, bool earsAreHidden, bool mouthIsHidden, ref BodyProperties bodyProperties);

	[EngineMethod("produce_numeric_key_with_default_values", false)]
	void ProduceNumericKeyWithDefaultValues(ref BodyProperties initialBodyProperties, bool earsAreHidden, bool mouthIsHidden, int race, int gender, float age);

	[EngineMethod("transform_face_keys_to_default_face", false)]
	void TransformFaceKeysToDefaultFace(ref FaceGenerationParams faceGenerationParams);

	[EngineMethod("get_random_body_properties", false)]
	void GetRandomBodyProperties(int race, int gender, ref BodyProperties bodyPropertiesMin, ref BodyProperties bodyPropertiesMax, int hairCoverType, int seed, string hairTags, string beardTags, string tatooTags, ref BodyProperties outBodyProperties);

	[EngineMethod("enforce_constraints", false)]
	bool EnforceConstraints(ref FaceGenerationParams faceGenerationParams);

	[EngineMethod("get_deform_key_data", false)]
	void GetDeformKeyData(int keyNo, ref DeformKeyData deformKeyData, int race, int gender, float age);

	[EngineMethod("get_face_gen_instances_length", false)]
	int GetFaceGenInstancesLength(int race, int gender, float age);

	[EngineMethod("get_scale", false)]
	float GetScaleFromKey(int race, int gender, ref BodyProperties initialBodyProperties);

	[EngineMethod("get_voice_records_count", false)]
	int GetVoiceRecordsCount(int race, int curGender, float age);

	[EngineMethod("get_hair_color_count", false)]
	int GetHairColorCount(int race, int curGender, float age);

	[EngineMethod("get_hair_color_gradient_points", false)]
	void GetHairColorGradientPoints(int race, int curGender, float age, Vec3[] colors);

	[EngineMethod("get_tatoo_color_count", false)]
	int GetTatooColorCount(int race, int curGender, float age);

	[EngineMethod("get_tatoo_color_gradient_points", false)]
	void GetTatooColorGradientPoints(int race, int curGender, float age, Vec3[] colors);

	[EngineMethod("get_skin_color_count", false)]
	int GetSkinColorCount(int race, int curGender, float age);

	[EngineMethod("get_maturity_type", false)]
	int GetMaturityType(float age);

	[EngineMethod("get_voice_type_usable_for_player_data", false)]
	void GetVoiceTypeUsableForPlayerData(int race, int curGender, float age, bool[] aiArray);

	[EngineMethod("get_skin_color_gradient_points", false)]
	void GetSkinColorGradientPoints(int race, int curGender, float age, Vec3[] colors);

	[EngineMethod("get_race_ids", false)]
	string GetRaceIds();
}
