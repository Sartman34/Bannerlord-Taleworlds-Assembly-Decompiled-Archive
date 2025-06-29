namespace TaleWorlds.DotNet;

public class ManagedDelegate : DotNetObject
{
	public delegate void DelegateDefinition();

	private DelegateDefinition _instance;

	public DelegateDefinition Instance
	{
		get
		{
			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	[LibraryCallback]
	public void InvokeAux()
	{
		Instance();
	}
}
