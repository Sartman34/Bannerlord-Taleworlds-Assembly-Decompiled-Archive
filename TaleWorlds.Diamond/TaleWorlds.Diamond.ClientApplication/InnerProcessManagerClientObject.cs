using TaleWorlds.Diamond.InnerProcess;

namespace TaleWorlds.Diamond.ClientApplication;

public class InnerProcessManagerClientObject : DiamondClientApplicationObject
{
	public InnerProcessManager InnerProcessManager { get; private set; }

	public InnerProcessManagerClientObject(DiamondClientApplication application, InnerProcessManager innerProcessManager)
		: base(application)
	{
		InnerProcessManager = innerProcessManager;
	}
}
