namespace TaleWorlds.MountAndBlade.Diamond;

internal class BannerlordConfig
{
	public int AdmittancePercentage { get; private set; }

	public BannerlordConfig(int admittancePercentage)
	{
		AdmittancePercentage = admittancePercentage;
	}
}
