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
}
