using System.Collections.Generic;
using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public class BasicGameStarter : IGameStarter
{
	private List<GameModel> _models;

	IEnumerable<GameModel> IGameStarter.Models => _models;

	public BasicGameStarter()
	{
		_models = new List<GameModel>();
	}

	void IGameStarter.AddModel(GameModel gameModel)
	{
		_models.Add(gameModel);
	}
}
