using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class FaceGen : IFaceGen
{
	private readonly Dictionary<string, int> _raceNamesDictionary;

	private readonly string[] _raceNamesArray;

	private readonly Dictionary<string, Monster> _monstersDictionary;

	private readonly Monster[] _monstersArray;

	private FaceGen()
	{
		_raceNamesDictionary = new Dictionary<string, int>();
		_raceNamesArray = MBAPI.IMBFaceGen.GetRaceIds().Split(new char[1] { ';' });
		for (int i = 0; i < _raceNamesArray.Length; i++)
		{
			_raceNamesDictionary[_raceNamesArray[i]] = i;
		}
		_monstersDictionary = new Dictionary<string, Monster>();
		_monstersArray = new Monster[_raceNamesArray.Length];
	}

	public static void CreateInstance()
	{
		TaleWorlds.Core.FaceGen.SetInstance(new FaceGen());
	}

	public Monster GetMonster(string monsterID)
	{
		if (!_monstersDictionary.TryGetValue(monsterID, out var value))
		{
			value = Game.Current.ObjectManager.GetObject<Monster>(monsterID);
			_monstersDictionary[monsterID] = value;
		}
		return value;
	}

	public Monster GetMonsterWithSuffix(int race, string suffix)
	{
		return GetMonster(_raceNamesArray[race] + suffix);
	}

	public Monster GetBaseMonsterFromRace(int race)
	{
		if (race >= 0 && race < _monstersArray.Length)
		{
			Monster monster = _monstersArray[race];
			if (monster == null)
			{
				monster = Game.Current.ObjectManager.GetObject<Monster>(_raceNamesArray[race]);
				_monstersArray[race] = monster;
			}
			return monster;
		}
		Debug.FailedAssert("Monster race index is out of bounds: " + race, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\FaceGen.cs", "GetBaseMonsterFromRace", 64);
		return null;
	}

	public BodyProperties GetRandomBodyProperties(int race, bool isFemale, BodyProperties bodyPropertiesMin, BodyProperties bodyPropertiesMax, int hairCoverType, int seed, string hairTags, string beardTags, string tattooTags)
	{
		return MBBodyProperties.GetRandomBodyProperties(race, isFemale, bodyPropertiesMin, bodyPropertiesMax, hairCoverType, seed, hairTags, beardTags, tattooTags);
	}

	void IFaceGen.GenerateParentBody(BodyProperties childBodyProperties, int race, ref BodyProperties motherBodyProperties, ref BodyProperties fatherBodyProperties)
	{
		MBBodyProperties.GenerateParentKey(childBodyProperties, race, ref motherBodyProperties, ref fatherBodyProperties);
	}

	void IFaceGen.SetHair(ref BodyProperties bodyProperties, int hair, int beard, int tattoo)
	{
		MBBodyProperties.SetHair(ref bodyProperties, hair, beard, tattoo);
	}

	void IFaceGen.SetBody(ref BodyProperties bodyProperties, int build, int weight)
	{
		MBBodyProperties.SetBody(ref bodyProperties, build, weight);
	}

	void IFaceGen.SetPigmentation(ref BodyProperties bodyProperties, int skinColor, int hairColor, int eyeColor)
	{
		MBBodyProperties.SetPigmentation(ref bodyProperties, skinColor, hairColor, eyeColor);
	}

	public BodyProperties GetBodyPropertiesWithAge(ref BodyProperties bodyProperties, float age)
	{
		return MBBodyProperties.GetBodyPropertiesWithAge(ref bodyProperties, age);
	}

	public void GetParamsFromBody(ref FaceGenerationParams faceGenerationParams, BodyProperties bodyProperties, bool earsAreHidden, bool mouthIsHidden)
	{
		MBBodyProperties.GetParamsFromKey(ref faceGenerationParams, bodyProperties, earsAreHidden, mouthIsHidden);
	}

	public BodyMeshMaturityType GetMaturityTypeWithAge(float age)
	{
		return MBBodyProperties.GetMaturityType(age);
	}

	public int GetRaceCount()
	{
		return _raceNamesArray.Length;
	}

	public int GetRaceOrDefault(string raceId)
	{
		return _raceNamesDictionary[raceId];
	}

	public string GetBaseMonsterNameFromRace(int race)
	{
		return _raceNamesArray[race];
	}

	public string[] GetRaceNames()
	{
		return (string[])_raceNamesArray.Clone();
	}
}
