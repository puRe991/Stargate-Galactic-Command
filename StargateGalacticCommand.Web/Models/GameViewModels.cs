using System;
using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;

namespace StargateGalacticCommand.Web.Models
{
    public class OverviewViewModel
    {
        public User User { get; set; }
        public PlayerBase Base { get; set; }
        public Planet Planet { get; set; }
        public ResourceProduction Hourly { get; set; }
        public IList<PlanetSector> Sectors { get; set; }
        public IList<Report> Reports { get; set; }
        public IList<BuildingUpgradeViewModel> Buildings { get; set; }
        public BuildQueueItem ActiveBuild { get; set; }
        public IList<BuildQueueItem> BuildQueue { get; set; }
        public IList<ResearchViewModel> Researches { get; set; }
        public ResearchQueueItem ActiveResearch { get; set; }
        public double DefenseModifier { get; set; }
        public DateTime NowUtc { get; set; }
        public IList<KnownGateAddress> KnownGateAddresses { get; set; }
        public IList<MissionTeam> MissionTeams { get; set; }
        public IList<GateMission> ActiveGateMissions { get; set; }
        public IList<GateMissionReport> GateMissionReports { get; set; }
        public IList<SectorClaim> ActiveSectorClaims { get; set; }
        public IList<PlanetInfluence> PlanetInfluences { get; set; }
        public IList<PlanetSector> ControlledSectors { get; set; }
        public SectorBonus SectorBonus { get; set; }
        public int OwnInfluence { get; set; }
        public IList<PlanetMarketOrder> ActiveMarketOrders { get; set; }
        public IList<PlanetMarketOrder> OwnMarketOrders { get; set; }
        public IList<TradeReport> TradeReports { get; set; }
        public IList<ShipViewModel> ShipDefinitions { get; set; }
        public ShipyardQueueItem ActiveShipBuild { get; set; }
        public IList<PlayerBase> FleetTargets { get; set; }
        public IList<FleetMovement> ActiveFleets { get; set; }
        public IList<FleetReport> FleetReports { get; set; }
        public IList<OrbitPresence> OrbitPresences { get; set; }
        public IList<PlayerBase> EspionageTargets { get; set; }
        public IList<IntelligenceReport> IntelligenceReports { get; set; }
        public IList<IntelligenceReport> SpyWarnings { get; set; }
        public IList<LocalCombatMission> ActiveLocalCombats { get; set; }
        public IList<SectorBattleReport> SectorBattleReports { get; set; }
        public Alliance OwnAlliance { get; set; }
        public IList<Alliance> Alliances { get; set; }
        public IList<AllianceApplication> AllianceApplications { get; set; }
        public IList<SpaceCombatMission> ActiveSpaceCombats { get; set; }
        public IList<SpaceCombatReport> SpaceCombatReports { get; set; }
        public IList<DebrisField> DebrisFields { get; set; }
        public PlayerProtectionStatus ProtectionStatus { get; set; }
        public IList<PlayerRankingEntry> PlayerRankings { get; set; }
        public IList<AllianceRankingEntry> AllianceRankings { get; set; }
        public IList<GalaxyEntry> GalaxyEntries { get; set; }
        public IList<PlayerMessage> InboxMessages { get; set; }
        public IList<PlayerMessage> SentMessages { get; set; }
        public int UnreadMessageCount { get; set; }
        public int UnreadReportCount { get; set; }
        public IList<User> MessageablePlayers { get; set; }
        public IList<ContractStatusViewModel> ContractStatuses { get; set; }
        public IList<AchievementStatusViewModel> AchievementStatuses { get; set; }
        public AllianceWarGoal ActiveWarGoal { get; set; }
        public int WarGoalCurrentSectors { get; set; }
        public bool CanManageWarGoal { get; set; }
        public IList<Planet> WarGoalPlanetOptions { get; set; }
        public int AscensionCount { get; set; }
        public double AscensionBonusPercent { get; set; }
        public int CurrentBaseScore { get; set; }
        public bool CanAscendNow { get; set; }
        public string AscensionBlockedReason { get; set; }
        public WorldEvent ActiveWorldEvent { get; set; }
        public string ActiveWorldEventName { get; set; }
        public string ActiveWorldEventDescription { get; set; }
        public int MyWorldEventContribution { get; set; }
        public bool CanContributeToWorldEvent { get; set; }
        public string WorldEventBlockedReason { get; set; }
        public DecoyProfile OwnDecoyProfile { get; set; }
        public IList<TradeRoute> TradeRoutes { get; set; }
        public string SeasonLabel { get; set; }
        public IList<int> SeasonFocusAddressIds { get; set; }
        public CharacterSkills CharacterSkills { get; set; }
    }

    public class AchievementStatusViewModel
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Progress { get; set; }
        public int Goal { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAtUtc { get; set; }
    }

    public class ContractStatusViewModel
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsWeekly { get; set; }
        public int Progress { get; set; }
        public int Goal { get; set; }
        public bool IsComplete { get; set; }
        public bool IsClaimed { get; set; }
        public BuildCost Reward { get; set; }
    }

    public class BuildingUpgradeViewModel
    {
        public BuildingType Type { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public BuildCost Cost { get; set; }
        public int BuildSeconds { get; set; }
        public bool CanAfford { get; set; }
        public bool QueueBusy { get; set; }
    }

    public class ResearchViewModel
    {
        public ResearchType Type { get; set; }
        public string Name { get; set; }
        public bool IsFactionResearch { get; set; }
        public int Level { get; set; }
        public BuildCost Cost { get; set; }
        public int ResearchSeconds { get; set; }
        public string PrerequisiteName { get; set; }
        public bool PrerequisiteMet { get; set; }
        public bool CanAfford { get; set; }
        public bool QueueBusy { get; set; }
        public bool HasResearchLab { get; set; }
    }

    public class ShipViewModel { public ShipType Type { get; set; } public string Name { get; set; } public BuildCost Cost { get; set; } public int CargoCapacity { get; set; } public int Speed { get; set; } public bool IsActive { get; set; } public bool CanBuild { get; set; } public int Available { get; set; } }

}
