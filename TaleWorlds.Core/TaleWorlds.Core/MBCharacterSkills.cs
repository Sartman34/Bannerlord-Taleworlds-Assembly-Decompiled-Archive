using System.Xml;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.Core;

public class MBCharacterSkills : MBObjectBase
{
	public CharacterSkills Skills { get; private set; }

	public MBCharacterSkills()
	{
		Skills = new CharacterSkills();
	}

	public void Init(MBObjectManager objectManager, XmlNode node)
	{
		base.Initialize();
		Skills.Deserialize(objectManager, node);
		AfterInitialized();
	}

	public override void Deserialize(MBObjectManager objectManager, XmlNode node)
	{
		base.Deserialize(objectManager, node);
		Skills.Deserialize(objectManager, node);
	}
}
