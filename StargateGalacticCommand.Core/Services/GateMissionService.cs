using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class GateMissionService
    {
        private readonly ResourceService _resources;
        private readonly FactionModifierService _factionModifiers;
        private readonly SeasonService _season;
        private readonly SkillTreeService _skillTree;
        public GateMissionService(ResourceService resources, FactionModifierService factionModifiers = null, SeasonService season = null, SkillTreeService skillTree = null) { _resources = resources; _factionModifiers = factionModifiers ?? new FactionModifierService(); _season = season ?? new SeasonService(); _skillTree = skillTree ?? new SkillTreeService(); }

        public const double AnomalyChance = 0.02;

        public BuildCost GetMissionCost(GateMissionType type)
        {
            switch (type)
            {
                case GateMissionType.SecureResources: return new BuildCost { Energy = 35, Supplies = 45, Personnel = 8 };
                case GateMissionType.SearchArtifact: return new BuildCost { Energy = 45, Supplies = 35, Personnel = 6, Intel = 2 };
                case GateMissionType.DiplomaticContact: return new BuildCost { Energy = 25, Supplies = 55, Personnel = 5 };
                case GateMissionType.RiskAnalysis: return new BuildCost { Energy = 30, Supplies = 25, Personnel = 4, Intel = 1 };
                case GateMissionType.AnalyzeAddress: return new BuildCost { Energy = 40, Supplies = 30, Personnel = 4, Intel = 3 };
                case GateMissionType.FoundColony: return new BuildCost { Energy = 80, Supplies = 120, Personnel = 20, Intel = 5 };
                default: return new BuildCost { Energy = 30, Supplies = 30, Personnel = 5 };
            }
        }

        public int GetMissionSeconds(GateMissionType type)
        {
            return type == GateMissionType.AnalyzeAddress ? 180 : type == GateMissionType.FoundColony ? 300 : 120;
        }

        public GateMission StartMission(User user, PlayerBase playerBase, GateAddress address, MissionTeam team, GateMissionType type, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (address == null) throw new ArgumentNullException("address");
            if (team == null) throw new ArgumentNullException("team");
            if (playerBase.BuildingLevels == null || playerBase.BuildingLevels.GateControlRoom < 1) throw new InvalidOperationException("Gate-Missionen benötigen Gate-Kontrollraum Level 1.");
            if (user.ResearchLevels == null || user.ResearchLevels.GateAddressing < 1) throw new InvalidOperationException("Gate-Missionen benötigen Gate-Adressierung Level 1.");
            if (!team.IsAvailable) throw new InvalidOperationException("Missionsteam ist bereits im Einsatz.");
            if (address.Planet != null && !address.Planet.StargateActive) throw new InvalidOperationException("Diese Gate-Adresse ist aktuell instabil oder inaktiv.");

            _resources.Spend(playerBase.Resources, GetMissionCost(type));
            team.IsAvailable = false;
            return new GateMission { UserId = user.Id, User = user, GateAddressId = address.Id, GateAddress = address, MissionTeamId = team.Id, MissionTeam = team, MissionType = type, StartedAtUtc = nowUtc, CompletesAtUtc = nowUtc.AddSeconds(GetMissionSeconds(type)), IsCompleted = false };
        }

        public GateMissionReport CompleteMission(GateMission mission, PlayerBase playerBase, IList<GateAddress> undiscoveredAddresses, DateTime nowUtc, Random random = null, CharacterSkills skills = null)
        {
            if (mission == null) throw new ArgumentNullException("mission");
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (nowUtc < mission.CompletesAtUtc) throw new InvalidOperationException("Gate-Mission läuft noch.");
            if (mission.IsCompleted) throw new InvalidOperationException("Gate-Mission wurde bereits abgeschlossen.");
            var team = mission.MissionTeam ?? throw new InvalidOperationException("Missionsteam fehlt.");
            int score = team.Strength + team.Science + team.Diplomacy + team.Stealth + team.CarryCapacity - team.Risk - (mission.GateAddress == null ? 0 : mission.GateAddress.RiskLevel);
            score += (int)mission.MissionType;
            score += _factionModifiers.GetGateMissionScoreBonus(playerBase.Faction, mission.MissionType);
            score += _skillTree.GetGateMissionScoreBonus(skills, mission.MissionType);
            var outcome = score >= 28 ? GateMissionOutcome.Success : score >= 20 ? GateMissionOutcome.PartialSuccess : GateMissionOutcome.Failure;
            var report = new GateMissionReport { UserId = mission.UserId, GateMission = mission, GateMissionId = mission.Id, Outcome = outcome, CreatedAtUtc = nowUtc };

            if (outcome == GateMissionOutcome.Failure)
            {
                report.PersonnelLost = Math.Max(1, team.Risk / 3);
                playerBase.Resources.Personnel = Math.Max(0, playerBase.Resources.Personnel - report.PersonnelLost);
                report.Outcome = GateMissionOutcome.WoundedOrLosses;
                report.Summary = "Das Team kehrt verwundet zurück. Keine Schiffe oder Großflotten wurden durch das Gate bewegt.";
            }
            else
            {
                int weekIndex = _season.GetWeekIndex(nowUtc);
                double seasonMultiplier = _season.GetRewardMultiplier(mission.GateAddress, weekIndex);
                report.IsSeasonFocusBonus = seasonMultiplier > 1.0;
                ApplyRewards(mission, playerBase, report, (outcome == GateMissionOutcome.Success ? 1.0 : 0.5) * seasonMultiplier);
                TryTriggerAnomaly(mission, playerBase, report, random ?? Random.Shared);
            }
            if (skills != null) _skillTree.AwardMissionPoint(skills);
            mission.IsCompleted = true;
            team.IsAvailable = true;
            return report;
        }

        // Deliberately rare and one-shot per address (see GameplayIdeas balancing note): once an address has yielded its anomaly it's "erschöpft" and never rolls again, so this can't be farmed.
        private static void TryTriggerAnomaly(GateMission mission, PlayerBase playerBase, GateMissionReport report, Random random)
        {
            if (mission.MissionType != GateMissionType.Explore && mission.MissionType != GateMissionType.AnalyzeAddress) return;
            var address = mission.GateAddress;
            if (address == null || address.AnomalyFound) return;
            if (random.NextDouble() >= AnomalyChance) return;

            address.AnomalyFound = true;
            var anomalyType = random.NextDouble() < 0.5 ? GateAnomalyType.AncientRuin : GateAnomalyType.AsgardWreck;
            report.AnomalyType = anomalyType;
            int bonusIntel = anomalyType == GateAnomalyType.AncientRuin ? 40 : 25;
            int bonusNaquadah = anomalyType == GateAnomalyType.AsgardWreck ? 150 : 0;
            report.IntelFound += bonusIntel;
            report.NaquadahFound += bonusNaquadah;
            playerBase.Resources.Intel += bonusIntel;
            playerBase.Resources.Naquadah += bonusNaquadah;
            report.Summary += " Ungewöhnlicher Fund: " + (anomalyType == GateAnomalyType.AncientRuin ? "eine antike Ruine mit Datenfragmenten" : "ein havariertes Asgard-Wrack") + ". Die Adresse gilt damit als vollständig erkundet; ein weiterer Fund an dieser Adresse ist ausgeschlossen.";
        }

        private static void ApplyRewards(GateMission mission, PlayerBase playerBase, GateMissionReport report, double factor)
        {
            int carry = Math.Max(1, mission.MissionTeam.CarryCapacity);
            if (mission.MissionType == GateMissionType.SecureResources || mission.MissionType == GateMissionType.Explore)
            {
                report.TriniumFound = (int)Math.Ceiling(carry * 8 * factor); report.NaquadahFound = (int)Math.Ceiling(carry * 5 * factor); report.Outcome = GateMissionOutcome.ResourceDiscovery;
            }
            else if (mission.MissionType == GateMissionType.DiplomaticContact || mission.MissionType == GateMissionType.RiskAnalysis)
            {
                report.IntelFound = (int)Math.Ceiling((mission.MissionTeam.Diplomacy + mission.MissionTeam.Science) * factor); report.Outcome = GateMissionOutcome.IntelDiscovery;
            }
            else if (mission.MissionType == GateMissionType.SearchArtifact)
            {
                report.ArtifactLeadFound = factor >= 1; report.IntelFound = (int)Math.Ceiling(mission.MissionTeam.Science * factor); report.Outcome = GateMissionOutcome.ArtifactLead;
            }
            else if (mission.MissionType == GateMissionType.AnalyzeAddress)
            {
                report.IntelFound = 3; report.Outcome = GateMissionOutcome.IntelDiscovery;
            }
            else if (mission.MissionType == GateMissionType.FoundColony)
            {
                report.IntelFound = 1; report.Outcome = GateMissionOutcome.IntelDiscovery;
            }
            playerBase.Resources.Naquadah += report.NaquadahFound; playerBase.Resources.Trinium += report.TriniumFound; playerBase.Resources.Supplies += report.SuppliesFound; playerBase.Resources.Intel += report.IntelFound;
            report.Summary = "Gate-Mission abgeschlossen. Keine Schiffe oder Großflotten wurden durch das Gate bewegt; Transport beschränkte sich lorekonform auf Personen, kleine Ausrüstung und Missionsteams.";
            if (report.IsSeasonFocusBonus) report.Summary += " Diese Adresse liegt in der aktuellen Fokuswoche: Belohnungen sind erhöht.";
        }

        public string ApplyFoundColonyResult(User user, GateAddress targetAddress, IList<Planet> planets, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (targetAddress == null) throw new ArgumentNullException("targetAddress");
            if (planets == null) throw new ArgumentNullException("planets");
            bool knowsTarget = user.KnownGateAddresses != null && user.KnownGateAddresses.Any(k => k.GateAddressId == targetAddress.Id);
            if (!knowsTarget) throw new InvalidOperationException("Koloniegründung benötigt eine bereits bekannte Gate-Adresse.");

            if (targetAddress.Planet == null)
            {
                var planet = CreateColonyPlanet(targetAddress.Code, Math.Max(1, planets.Count + 1));
                targetAddress.Planet = planet;
                targetAddress.PlanetId = planet.Id == 0 ? (int?)null : planet.Id;
                targetAddress.IsNeutralPve = false;
                planets.Add(planet);
                return "Neuer Kolonieplanet freigeschaltet: " + planet.Name + ". Stargates transportierten nur Team und Ausrüstung; Großschiffe bleiben auf Hyperraumrouten beschränkt.";
            }

            var expandable = planets.OrderBy(p => p.Id).ThenBy(p => p.Name).FirstOrDefault(IsNearlyFull);
            if (expandable == null) expandable = targetAddress.Planet;
            int next = expandable.Sectors.Any() ? expandable.Sectors.Max(s => s.Number) + 1 : 1;
            expandable.Sectors.Add(new PlanetSector { Number = next, Name = "Koloniesektor " + next, SectorType = SectorType.SettlementSector, IsSettlementSector = true });
            expandable.Sectors.Add(new PlanetSector { Number = next + 1, Name = "lokaler Versorgungsaußenposten", SectorType = SectorType.LocalSettlement, IsSettlementSector = true });
            return "Bestehender Planet erweitert: " + expandable.Name + " erhält zusätzliche Siedlungssektoren. Werte sind vorläufig, noch mit Spieldesign-Zielen abzugleichen.";
        }

        private static bool IsNearlyFull(Planet planet)
        {
            var settlementSectors = planet.Sectors.Where(s => s.IsSettlementSector).ToList();
            if (settlementSectors.Count == 0) return false;
            int occupied = settlementSectors.Count(s => s.PlayerBase != null);
            return occupied >= settlementSectors.Count - 1;
        }

        private static Planet CreateColonyPlanet(string code, int sequence)
        {
            var planet = new Planet { Name = code, Galaxy = "Milchstraße", Type = "Koloniewelt", StargateActive = true, Status = "neu erschlossen" };
            string[] names = { "Stargate-Zone", "Pioniersiedlung", "Koloniesektor 3", "Koloniesektor 4", "Triniumprospektion", "Antike Ruinenstätte", "Naquadah-Ader", "Tauschposten" };
            SectorType[] types = { SectorType.StargateZone, SectorType.LocalSettlement, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.TriniumField, SectorType.GoauldRuin, SectorType.NaquadahDeposit, SectorType.TradingPost };
            for (int i = 0; i < names.Length; i++) planet.Sectors.Add(new PlanetSector { Number = i + 1, Name = names[i], SectorType = types[i], IsSettlementSector = types[i] == SectorType.LocalSettlement || types[i] == SectorType.SettlementSector });
            return planet;
        }

        public KnownGateAddress DiscoverRandomAddress(User user, IList<GateAddress> undiscoveredAddresses, Random random, string discoveryMethod, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (random == null) throw new ArgumentNullException("random");
            if (undiscoveredAddresses == null || undiscoveredAddresses.Count == 0) return null;

            var picked = undiscoveredAddresses[random.Next(undiscoveredAddresses.Count)];
            return new KnownGateAddress { UserId = user.Id, GateAddressId = picked.Id, GateAddress = picked, DiscoveredAtUtc = nowUtc, DiscoveryMethod = discoveryMethod };
        }

        public MissionTeam CreateFactionTeam(User user)
        {
            string s = user?.Faction?.ShortName ?? "SGC";
            if (s == "Jaffa") return new MissionTeam { User = user, UserId = user?.Id ?? 0, Type = MissionTeamType.JaffaSquad, Name = "Jaffa-Trupp", Strength = 10, Science = 3, Diplomacy = 4, Stealth = 5, CarryCapacity = 8, Risk = 5 };
            if (s == "Tok’ra") return new MissionTeam { User = user, UserId = user?.Id ?? 0, Type = MissionTeamType.TokraAgentCell, Name = "Tok’ra-Agentenzelle", Strength = 5, Science = 8, Diplomacy = 7, Stealth = 10, CarryCapacity = 4, Risk = 4 };
            if (s == "Lucian") return new MissionTeam { User = user, UserId = user?.Id ?? 0, Type = MissionTeamType.LucianScoutUnit, Name = "Lucian-Erkundungstrupp", Strength = 7, Science = 4, Diplomacy = 6, Stealth = 7, CarryCapacity = 9, Risk = 6 };
            return new MissionTeam { User = user, UserId = user?.Id ?? 0, Type = MissionTeamType.SgTeam, Name = "SG-Team", Strength = 7, Science = 8, Diplomacy = 7, Stealth = 6, CarryCapacity = 6, Risk = 4 };
        }
    }
}
