namespace TaleWorlds.Engine;

public class SceneProblemChecker
{
	[EngineCallback]
	internal static bool OnCheckForSceneProblems(Scene scene)
	{
		return false;
	}
}
