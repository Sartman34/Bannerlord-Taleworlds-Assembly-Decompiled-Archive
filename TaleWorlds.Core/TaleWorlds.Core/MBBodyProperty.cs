using System.Xml;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.Core;

public class MBBodyProperty : MBObjectBase
{
	private BodyProperties _bodyPropertyMin;

	private BodyProperties _bodyPropertyMax;

	public BodyProperties BodyPropertyMin => _bodyPropertyMin;

	public BodyProperties BodyPropertyMax => _bodyPropertyMax;

	public MBBodyProperty(string stringId)
		: base(stringId)
	{
	}

	public MBBodyProperty()
	{
	}

	public void Init(BodyProperties bodyPropertyMin, BodyProperties bodyPropertyMax)
	{
		base.Initialize();
		_bodyPropertyMin = bodyPropertyMin;
		_bodyPropertyMax = bodyPropertyMax;
		if (_bodyPropertyMax.Age <= 0f)
		{
			_bodyPropertyMax = _bodyPropertyMin;
		}
		if (_bodyPropertyMin.Age <= 0f)
		{
			_bodyPropertyMin = _bodyPropertyMax;
		}
		AfterInitialized();
	}

	public override void Deserialize(MBObjectManager objectManager, XmlNode node)
	{
		base.Deserialize(objectManager, node);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "BodyPropertiesMin")
			{
				BodyProperties.FromXmlNode(childNode, out _bodyPropertyMin);
			}
			else if (childNode.Name == "BodyPropertiesMax")
			{
				BodyProperties.FromXmlNode(childNode, out _bodyPropertyMax);
			}
		}
		if (_bodyPropertyMax.Age <= 0f)
		{
			_bodyPropertyMax = _bodyPropertyMin;
		}
		if (_bodyPropertyMin.Age <= 0f)
		{
			_bodyPropertyMin = _bodyPropertyMax;
		}
	}
}
