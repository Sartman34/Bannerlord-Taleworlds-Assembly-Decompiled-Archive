using System;
using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.GameKeys;

public class GameKeyOptionVM : KeyOptionVM
{
	private InputKey _initalKey;

	private readonly Action<GameKeyOptionVM, InputKey> _onKeySet;

	public GameKey CurrentGameKey { get; private set; }

	public GameKeyOptionVM(GameKey gameKey, Action<KeyOptionVM> onKeybindRequest, Action<GameKeyOptionVM, InputKey> onKeySet)
		: base(gameKey.GroupId, ((GameKeyDefinition)gameKey.Id).ToString(), onKeybindRequest)
	{
		_onKeySet = onKeySet;
		CurrentGameKey = gameKey;
		base.Key = (TaleWorlds.InputSystem.Input.IsGamepadActive ? CurrentGameKey.ControllerKey : CurrentGameKey.KeyboardKey);
		if (base.Key == null)
		{
			base.Key = new Key(InputKey.Invalid);
		}
		base.CurrentKey = new Key(base.Key.InputKey);
		_initalKey = base.Key.InputKey;
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		base.Name = Module.CurrentModule.GlobalTextManager.FindText("str_key_name", _groupId + "_" + _id).ToString();
		base.Description = Module.CurrentModule.GlobalTextManager.FindText("str_key_description", _groupId + "_" + _id).ToString();
		base.OptionValueText = Module.CurrentModule.GlobalTextManager.FindText("str_game_key_text", base.CurrentKey.ToString().ToLower()).ToString();
	}

	private void ExecuteKeybindRequest()
	{
		_onKeybindRequest(this);
	}

	public override void Set(InputKey newKey)
	{
		_onKeySet(this, newKey);
	}

	public override void Update()
	{
		base.Key = (TaleWorlds.InputSystem.Input.IsGamepadActive ? CurrentGameKey.ControllerKey : CurrentGameKey.KeyboardKey);
		if (base.Key == null)
		{
			base.Key = new Key(InputKey.Invalid);
		}
		base.CurrentKey = new Key(base.Key.InputKey);
		base.OptionValueText = Module.CurrentModule.GlobalTextManager.FindText("str_game_key_text", base.CurrentKey.ToString().ToLower()).ToString();
	}

	public override void OnDone()
	{
		base.Key?.ChangeKey(base.CurrentKey.InputKey);
		_initalKey = base.CurrentKey.InputKey;
	}

	internal override bool IsChanged()
	{
		return base.CurrentKey.InputKey != _initalKey;
	}

	public void Revert()
	{
		Set(_initalKey);
		Update();
	}

	public void Apply()
	{
		OnDone();
		base.CurrentKey = base.Key;
	}
}
