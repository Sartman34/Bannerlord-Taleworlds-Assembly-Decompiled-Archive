using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace TaleWorlds.CampaignSystem;

public interface INavigationHandler
{
	bool PartyEnabled { get; }

	bool InventoryEnabled { get; }

	bool QuestsEnabled { get; }

	bool CharacterDeveloperEnabled { get; }

	NavigationPermissionItem ClanPermission { get; }

	NavigationPermissionItem KingdomPermission { get; }

	bool EscapeMenuEnabled { get; }

	bool PartyActive { get; }

	bool InventoryActive { get; }

	bool QuestsActive { get; }

	bool CharacterDeveloperActive { get; }

	bool ClanActive { get; }

	bool KingdomActive { get; }

	bool EscapeMenuActive { get; }

	void OpenQuests();

	void OpenQuests(QuestBase quest);

	void OpenQuests(IssueBase issue);

	void OpenQuests(JournalLogEntry log);

	void OpenInventory();

	void OpenParty();

	void OpenCharacterDeveloper();

	void OpenCharacterDeveloper(Hero hero);

	void OpenKingdom();

	void OpenKingdom(Army army);

	void OpenKingdom(Settlement settlement);

	void OpenKingdom(Clan clan);

	void OpenKingdom(PolicyObject policy);

	void OpenKingdom(IFaction faction);

	void OpenKingdom(KingdomDecision decision);

	void OpenClan();

	void OpenClan(Hero hero);

	void OpenClan(PartyBase party);

	void OpenClan(Settlement settlement);

	void OpenClan(Workshop workshop);

	void OpenClan(Alley alley);

	void OpenEscapeMenu();
}
