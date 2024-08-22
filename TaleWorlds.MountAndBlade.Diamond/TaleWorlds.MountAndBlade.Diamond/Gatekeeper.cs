using System;
using System.Threading.Tasks;
using TaleWorlds.Diamond.Rest;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Diamond;

public static class Gatekeeper
{
	private static Random _random;

	private static int _roll;

	private static readonly bool _isDeployBuild;

	public static async Task<bool> IsGenerous()
	{
		if (_isDeployBuild)
		{
			if (_random == null)
			{
				_random = new Random(MachineId.AsInteger());
				_roll = _random.Next() % 101;
			}
			int num = await GetAdmittancePercentage();
			return _roll <= num;
		}
		return true;
	}

	private static async Task<int> GetAdmittancePercentage()
	{
		try
		{
			string json = await HttpHelper.DownloadStringTaskAsync("https://taleworldswebsiteassets.blob.core.windows.net/upload/blconfig.json").ConfigureAwait(continueOnCapturedContext: false);
			return new RestDataJsonConverter().ReadJson<BannerlordConfig>(json).AdmittancePercentage;
		}
		catch (Exception)
		{
			return 100;
		}
	}
}
