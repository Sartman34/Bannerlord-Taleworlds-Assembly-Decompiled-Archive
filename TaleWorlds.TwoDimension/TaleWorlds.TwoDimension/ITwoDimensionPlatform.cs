namespace TaleWorlds.TwoDimension;

public interface ITwoDimensionPlatform
{
	float Width { get; }

	float Height { get; }

	float ReferenceWidth { get; }

	float ReferenceHeight { get; }

	float ApplicationTime { get; }

	void Draw(float x, float y, Material material, DrawObject2D drawObject2D, int layer);

	void SetScissor(ScissorTestInfo scissorTestInfo);

	void ResetScissor();

	void PlaySound(string soundName);

	void CreateSoundEvent(string soundName);

	void PlaySoundEvent(string soundName);

	void StopAndRemoveSoundEvent(string soundName);

	void OpenOnScreenKeyboard(string initialText, string descriptionText, int maxLength, int keyboardTypeEnum);

	void BeginDebugPanel(string panelTitle);

	void EndDebugPanel();

	void DrawDebugText(string text);

	bool DrawDebugTreeNode(string text);

	void PopDebugTreeNode();

	void DrawCheckbox(string label, ref bool isChecked);

	bool IsDebugItemHovered();

	bool IsDebugModeEnabled();
}
