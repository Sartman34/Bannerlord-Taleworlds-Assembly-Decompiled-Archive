using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public class MultiplayerItemTestMissionController : MissionLogic
{
	private Agent mainAgent;

	private BasicCultureObject _culture;

	private List<BasicCharacterObject> _troops = new List<BasicCharacterObject>();

	private const float HorizontalGap = 3f;

	private const float VerticalGap = 3f;

	private Vec3 _coordinate = new Vec3(200f, 200f);

	private int _mapHorizontalEndCoordinate = 800;

	private static bool _initializeFlag;

	public MultiplayerItemTestMissionController(BasicCultureObject culture)
	{
		_culture = culture;
		if (!_initializeFlag)
		{
			Game.Current.ObjectManager.LoadXML("MPCharacters");
			_initializeFlag = true;
		}
	}

	public override void AfterStart()
	{
		GetAllTroops();
		SpawnMainAgent();
		SpawnMultiplayerTroops();
	}

	private void SpawnMultiplayerTroops()
	{
		foreach (BasicCharacterObject troop in _troops)
		{
			GetNextSpawnFrame(out var position, out var direction);
			foreach (Equipment allEquipment in troop.AllEquipments)
			{
				base.Mission.SpawnAgent(new AgentBuildData(new BasicBattleAgentOrigin(troop)).Equipment(allEquipment).InitialPosition(in position).InitialDirection(in direction));
				position += new Vec3(0f, 2f);
			}
		}
	}

	private void GetNextSpawnFrame(out Vec3 position, out Vec2 direction)
	{
		_coordinate += new Vec3(3f);
		if (_coordinate.x > (float)_mapHorizontalEndCoordinate)
		{
			_coordinate.x = 3f;
			_coordinate.y += 3f;
		}
		position = _coordinate;
		direction = new Vec2(0f, -1f);
	}

	private XmlDocument LoadXmlFile(string path)
	{
		Debug.Print("opening " + path);
		XmlDocument xmlDocument = new XmlDocument();
		string xml = new StreamReader(path).ReadToEnd();
		xmlDocument.LoadXml(xml);
		return xmlDocument;
	}

	private void SpawnMainAgent()
	{
		if (mainAgent == null || mainAgent.State != AgentState.Active)
		{
			BasicCharacterObject @object = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("main_hero");
			Mission mission = base.Mission;
			AgentBuildData agentBuildData = new AgentBuildData(new BasicBattleAgentOrigin(@object)).Team(base.Mission.DefenderTeam);
			Vec3 position = new Vec3(200f + (float)MBRandom.RandomInt(15), 200f + (float)MBRandom.RandomInt(15), 1f);
			mainAgent = mission.SpawnAgent(agentBuildData.InitialPosition(in position).InitialDirection(in Vec2.Forward).Controller(Agent.ControllerType.Player));
		}
	}

	private void GetAllTroops()
	{
		foreach (XmlNode item in LoadXmlFile(BasePath.Name + "/Modules/Native/ModuleData/mpcharacters.xml").DocumentElement.SelectNodes("NPCCharacter"))
		{
			if (item.Attributes?["occupation"] != null && item.Attributes["occupation"].InnerText == "Soldier")
			{
				string innerText = item.Attributes["id"].InnerText;
				BasicCharacterObject @object = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(innerText);
				if (@object != null && @object.Culture == _culture)
				{
					_troops.Add(@object);
				}
			}
		}
	}
}
