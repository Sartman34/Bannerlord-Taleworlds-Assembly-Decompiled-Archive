using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.Settlements.Buildings;

public class DefaultBuildingTypes
{
	public const int MaxBuildingLevel = 3;

	private BuildingType _buildingFortifications;

	private BuildingType _buildingSettlementGarrisonBarracks;

	private BuildingType _buildingSettlementTrainingFields;

	private BuildingType _buildingSettlementFairgrounds;

	private BuildingType _buildingSettlementMarketplace;

	private BuildingType _buildingSettlementAquaducts;

	private BuildingType _buildingSettlementForum;

	private BuildingType _buildingSettlementGranary;

	private BuildingType _buildingSettlementOrchard;

	private BuildingType _buildingSettlementMilitiaBarracks;

	private BuildingType _buildingSettlementSiegeWorkshop;

	private BuildingType _buildingSettlementLimeKilns;

	private BuildingType _buildingWall;

	private BuildingType _buildingCastleBarracks;

	private BuildingType _buildingCastleTrainingFields;

	private BuildingType _buildingCastleGranary;

	private BuildingType _buildingCastleGardens;

	private BuildingType _buildingCastleCastallansOffice;

	private BuildingType _buildingCastleWorkshop;

	private BuildingType _buildingCastleFairgrounds;

	private BuildingType _buildingCastleSiegeWorkshop;

	private BuildingType _buildingCastleMilitiaBarracks;

	private BuildingType _buildingCastleTollCollector;

	private BuildingType _buildingDailyBuildHouse;

	private BuildingType _buildingDailyTrainMilitia;

	private BuildingType _buildingDailyFestivalsAndGames;

	private BuildingType _buildingDailyIrrigation;

	public static IEnumerable<BuildingType> MilitaryBuildings
	{
		get
		{
			yield return Fortifications;
			yield return SettlementGarrisonBarracks;
			yield return SettlementTrainingFields;
			yield return SettlementWorkshop;
			yield return SettlementMilitiaBarracks;
			yield return SettlementSiegeWorkshop;
			yield return Wall;
			yield return CastleBarracks;
			yield return CastleTrainingFields;
			yield return CastleCastallansOffice;
			yield return CastleWorkshop;
			yield return CastleSiegeWorkshop;
			yield return CastleMilitiaBarracks;
			yield return TrainMilitiaDaily;
		}
	}

	private static DefaultBuildingTypes Instance => Campaign.Current.DefaultBuildingTypes;

	public static BuildingType Fortifications => Instance._buildingFortifications;

	public static BuildingType SettlementGarrisonBarracks => Instance._buildingSettlementGarrisonBarracks;

	public static BuildingType SettlementTrainingFields => Instance._buildingSettlementTrainingFields;

	public static BuildingType SettlementFairgrounds => Instance._buildingSettlementFairgrounds;

	public static BuildingType SettlementMarketplace => Instance._buildingSettlementMarketplace;

	public static BuildingType SettlementAquaducts => Instance._buildingSettlementAquaducts;

	public static BuildingType SettlementForum => Instance._buildingSettlementForum;

	public static BuildingType SettlementGranary => Instance._buildingSettlementGranary;

	public static BuildingType SettlementWorkshop => Instance._buildingSettlementOrchard;

	public static BuildingType SettlementMilitiaBarracks => Instance._buildingSettlementMilitiaBarracks;

	public static BuildingType SettlementSiegeWorkshop => Instance._buildingSettlementSiegeWorkshop;

	public static BuildingType SettlementLimeKilns => Instance._buildingSettlementLimeKilns;

	public static BuildingType Wall => Instance._buildingWall;

	public static BuildingType CastleBarracks => Instance._buildingCastleBarracks;

	public static BuildingType CastleTrainingFields => Instance._buildingCastleTrainingFields;

	public static BuildingType CastleGranary => Instance._buildingCastleGranary;

	public static BuildingType CastleGardens => Instance._buildingCastleGardens;

	public static BuildingType CastleCastallansOffice => Instance._buildingCastleCastallansOffice;

	public static BuildingType CastleWorkshop => Instance._buildingCastleWorkshop;

	public static BuildingType CastleFairgrounds => Instance._buildingCastleFairgrounds;

	public static BuildingType CastleSiegeWorkshop => Instance._buildingCastleSiegeWorkshop;

	public static BuildingType CastleMilitiaBarracks => Instance._buildingCastleMilitiaBarracks;

	public static BuildingType CastleTollCollector => Instance._buildingCastleTollCollector;

	public static BuildingType BuildHouseDaily => Instance._buildingDailyBuildHouse;

	public static BuildingType TrainMilitiaDaily => Instance._buildingDailyTrainMilitia;

	public static BuildingType FestivalsAndGamesDaily => Instance._buildingDailyFestivalsAndGames;

	public static BuildingType IrrigationDaily => Instance._buildingDailyIrrigation;

	public DefaultBuildingTypes()
	{
		RegisterAll();
	}

	private void RegisterAll()
	{
		_buildingFortifications = Create("building_fortifications");
		_buildingSettlementGarrisonBarracks = Create("building_settlement_garrison_barracks");
		_buildingSettlementTrainingFields = Create("building_settlement_training_fields");
		_buildingSettlementFairgrounds = Create("building_settlement_fairgrounds");
		_buildingSettlementMarketplace = Create("building_settlement_marketplace");
		_buildingSettlementAquaducts = Create("building_settlement_aquaducts");
		_buildingSettlementForum = Create("building_settlement_forum");
		_buildingSettlementGranary = Create("building_settlement_granary");
		_buildingSettlementOrchard = Create("building_settlement_lime_kilns");
		_buildingSettlementMilitiaBarracks = Create("building_settlement_militia_barracks");
		_buildingSettlementSiegeWorkshop = Create("building_siege_workshop");
		_buildingSettlementLimeKilns = Create("building_settlement_workshop");
		_buildingWall = Create("building_wall");
		_buildingCastleBarracks = Create("building_castle_barracks");
		_buildingCastleTrainingFields = Create("building_castle_training_fields");
		_buildingCastleGranary = Create("building_castle_granary");
		_buildingCastleGardens = Create("building_castle_gardens");
		_buildingCastleCastallansOffice = Create("building_castle_castallans_office");
		_buildingCastleWorkshop = Create("building_castle_workshops");
		_buildingCastleFairgrounds = Create("building_castle_fairgrounds");
		_buildingCastleSiegeWorkshop = Create("building_castle_siege_workshop");
		_buildingCastleMilitiaBarracks = Create("building_castle_militia_barracks");
		_buildingCastleTollCollector = Create("building_castle_lime_kilns");
		_buildingDailyBuildHouse = Create("building_daily_build_house");
		_buildingDailyTrainMilitia = Create("building_daily_train_militia");
		_buildingDailyFestivalsAndGames = Create("building_festivals_and_games");
		_buildingDailyIrrigation = Create("building_irrigation");
		InitializeAll();
	}

	private BuildingType Create(string stringId)
	{
		return Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType(stringId));
	}

	private void InitializeAll()
	{
		_buildingFortifications.Initialize(new TextObject("{=CVdK1ax1}Fortifications"), new TextObject("{=dIM6xa2O}Better fortifications and higher walls around town, also increases the max garrison limit since it provides more space for the resident troops."), new int[3] { 0, 8000, 16000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.GarrisonCapacity, 25f, 50f, 100f)
		}, 1);
		_buildingSettlementGarrisonBarracks.Initialize(new TextObject("{=54vkRuHo}Garrison Barracks"), new TextObject("{=DHm1MBsj}Lodging for the garrisoned troops. Each level increases the garrison capacity of the stronghold."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.GarrisonCapacity, 30f, 60f, 100f)
		});
		_buildingSettlementTrainingFields.Initialize(new TextObject("{=BkTiRPT4}Training Fields"), new TextObject("{=otWlERkc}A field for military drills that increases the daily experience gain of all garrisoned units."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Experience, 1f, 2f, 3f)
		});
		_buildingSettlementFairgrounds.Initialize(new TextObject("{=ixHqTrX5}Fairgrounds"), new TextObject("{=0B91pZ2R}A permanent space that hosts fairs. Citizens can gather, drink dance and socialize,  increasing the daily morale of the settlement."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Loyalty, 0.5f, 1f, 1.5f)
		});
		_buildingSettlementMarketplace.Initialize(new TextObject("{=zLdXCpne}Marketplace"), new TextObject("{=Z9LWA6A3}Scheduled market days lure folks from surrounding villages to the settlement and of course the local ruler takes a handsome cut of any sales. Increases the wealth and tax yield of the settlement."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Tax, 5f, 10f, 15f)
		});
		_buildingSettlementAquaducts.Initialize(new TextObject("{=f5jHMbOq}Aqueducts"), new TextObject("{=UojHRjdG}Access to clean water provides room for growth with healthy citizens and a clean infrastructure. Increases daily Prosperity change."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Prosperity, 0.3f, 0.6f, 1f)
		});
		_buildingSettlementForum.Initialize(new TextObject("{=paelEWj1}Forum"), new TextObject("{=wTBtu1t5}An open square in the settlement where people can meet, spend time, and share their ideas. Increases influence of the settlement owner."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Influence, 0.5f, 1f, 1.5f)
		});
		_buildingSettlementGranary.Initialize(new TextObject("{=PstO2f5I}Granary"), new TextObject("{=aK23T43P}Keeps stockpiles of food so that the settlement has more food supply. Each level increases the local food supply."), new int[3] { 1000, 1500, 2000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Foodstock, 200f, 400f, 600f)
		});
		_buildingSettlementLimeKilns.Initialize(new TextObject("{=NbgeKwVr}Workshops"), new TextObject("{=qR9bEE6g}A building which provides the means required for the manufacture or repair of buildings. Improves project development speed. Also stonemasons reinforce the walls."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Construction, 3f, 6f, 9f)
		});
		_buildingSettlementMilitiaBarracks.Initialize(new TextObject("{=l91xAgmU}Militia Grounds"), new TextObject("{=RliyRJKl}Provides weapons training for citizens. Increases daily militia recruitment."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Militia, 0.5f, 1f, 1.5f)
		});
		_buildingSettlementSiegeWorkshop.Initialize(new TextObject("{=9Bnwttn6}Siege Workshop"), new TextObject("{=MharAceZ}A workshop dedicated to sieges. Contains tools and materials to repair walls, build and repair siege engines."), new int[3] { 1000, 1500, 2000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[2]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.WallRepairSpeed, 50f, 50f, 50f),
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.SiegeEngineSpeed, 30f, 60f, 100f)
		});
		_buildingSettlementOrchard.Initialize(new TextObject("{=AkbiPIij}Orchards"), new TextObject("{=ZCLVOXgM}Fruit trees and vegetable gardens outside the walls provide food as long as there is no siege."), new int[3] { 2000, 3000, 4000 }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.FoodProduction, 6f, 12f, 18f)
		});
		_buildingWall.Initialize(new TextObject("{=6pNrNj93}Wall"), new TextObject("{=oS5Nesmi}Better fortifications and higher walls around the keep, also increases the max garrison limit since it provides more space for the resident troops."), new int[3] { 0, 2500, 5000 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.GarrisonCapacity, 25f, 50f, 100f)
		}, 1);
		_buildingCastleBarracks.Initialize(new TextObject("{=x2B0OjhI}Barracks"), new TextObject("{=HJ1is924}Lodgings for the garrisoned troops. Increases the garrison capacity of the stronghold."), new int[3] { 500, 1000, 1500 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.GarrisonCapacity, 30f, 60f, 100f)
		});
		_buildingCastleTrainingFields.Initialize(new TextObject("{=BkTiRPT4}Training Fields"), new TextObject("{=otWlERkc}A field for military drills that increases the daily experience gain of all garrisoned units."), new int[3] { 500, 1000, 1500 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Experience, 1f, 2f, 3f)
		});
		_buildingCastleGranary.Initialize(new TextObject("{=PstO2f5I}Granary"), new TextObject("{=iazij7fO}Keeps stockpiles of food so that the settlement has more food supply. Increases the local food supply."), new int[3] { 500, 1000, 1500 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Foodstock, 100f, 150f, 200f)
		});
		_buildingCastleGardens.Initialize(new TextObject("{=yT6XN4Mr}Gardens"), new TextObject("{=ZCLVOXgM}Fruit trees and vegetable gardens outside the walls provide food as long as there is no siege."), new int[3] { 500, 750, 1000 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.FoodProduction, 3f, 6f, 9f)
		});
		_buildingCastleCastallansOffice.Initialize(new TextObject("{=kLNnFMR9}Castellan's Office"), new TextObject("{=GDsI6daq}Provides a warden for the castle who maintains discipline and upholds the law."), new int[3] { 500, 750, 1000 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.GarrisonWageReduce, 10f, 20f, 30f)
		});
		_buildingCastleWorkshop.Initialize(new TextObject("{=NbgeKwVr}Workshops"), new TextObject("{=qR9bEE6g}A building which provides the means required for the manufacture or repair of buildings. Improves project development speed. Also stonemasons reinforce the walls."), new int[3] { 500, 750, 1000 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Construction, 1f, 2f, 3f)
		});
		_buildingCastleFairgrounds.Initialize(new TextObject("{=ixHqTrX5}Fairgrounds"), new TextObject("{=QHZeCDJy}A permanent space that hosts fairs. Citizens can gather, drink dance and socialize, increasing the daily morale of the settlement."), new int[3] { 500, 750, 1000 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Loyalty, 0.5f, 1f, 1.5f)
		});
		_buildingCastleSiegeWorkshop.Initialize(new TextObject("{=9Bnwttn6}Siege Workshop"), new TextObject("{=MharAceZ}A workshop dedicated to sieges. Contains tools and materials to repair walls, build and repair siege engines."), new int[3] { 500, 750, 1000 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[2]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.WallRepairSpeed, 50f, 50f, 50f),
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.SiegeEngineSpeed, 30f, 60f, 100f)
		});
		_buildingCastleMilitiaBarracks.Initialize(new TextObject("{=l91xAgmU}Militia Grounds"), new TextObject("{=YRrx8bAK}Provides weapons training for citizens. Each level increases daily militia recruitment."), new int[3] { 500, 750, 1000 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Militia, 1f, 2f, 3f)
		});
		_buildingCastleTollCollector.Initialize(new TextObject("{=VawDQKLl}Toll Collector"), new TextObject("{=ac8PkfhG}Increases tax income from the region"), new int[3] { 500, 750, 1000 }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Tax, 10f, 20f, 30f)
		});
		_buildingDailyBuildHouse.Initialize(new TextObject("{=F4V7oaVx}Housing"), new TextObject("{=yWXtcxqb}Construct housing so that more folks can settle, increasing population."), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.ProsperityDaily, 1f, 1f, 1f)
		});
		_buildingDailyTrainMilitia.Initialize(new TextObject("{=p1Y3EU5O}Train Militia"), new TextObject("{=61J1wa6k}Schedule drills for commoners, increasing militia recruitment."), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.MilitiaDaily, 3f, 3f, 3f)
		});
		_buildingDailyFestivalsAndGames.Initialize(new TextObject("{=aEmYZadz}Festival and Games"), new TextObject("{=ovDbQIo9}Organize festivals and games in the settlement, increasing morale."), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.LoyaltyDaily, 3f, 3f, 3f)
		});
		_buildingDailyIrrigation.Initialize(new TextObject("{=O4cknzhW}Irrigation"), new TextObject("{=CU9g49fo}Provide irrigation, increasing growth in bound villages."), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[1]
		{
			new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.VillageDevelopmentDaily, 1f, 1f, 1f)
		});
	}
}
