using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class EspionageService
    {
        private readonly ResourceService _resources;
        public EspionageService(ResourceService resources) { _resources = resources ?? throw new ArgumentNullException("resources"); }

        public const int DecoyChargeIntelCost = 40;
        public const int MaxDecoyCharges = 3;
        public const double BaseDeceptionChance = 0.6;
        public const double DeceptionChancePerAttackerSkillPoint = 0.03;
        public const double MinDeceptionChance = 0.1;

        public EspionageMission StartMission(User user, PlayerBase source, PlayerBase target, EspionageMissionType type, int intelSpent, DateTime now)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            if (source.Resources == null || source.BuildingLevels == null || user.ResearchLevels == null) throw new InvalidOperationException("Spionage benötigt vollständige Basis-, Ressourcen- und Forschungsdaten.");
            if (target.BuildingLevels == null || target.Resources == null) throw new InvalidOperationException("Zielbasis ist unvollständig.");
            if (source.Id == target.Id) throw new InvalidOperationException("Eigene Basis kann nicht ausspioniert werden.");
            if (source.BuildingLevels.SensorStation < 1) throw new InvalidOperationException("Sensorstation Level 1 ist für einfache Spionage erforderlich.");
            if (intelSpent < 5) throw new ArgumentOutOfRangeException("intelSpent", "Mindestens 5 Intel müssen eingesetzt werden.");
            if (intelSpent > 500) throw new ArgumentOutOfRangeException("intelSpent", "Maximal 500 Intel pro Mission.");
            if (RequiresStealth(type) && user.ResearchLevels.StealthTechnology < 1) throw new InvalidOperationException("Tarntechnologie ist für verdeckte Missionen erforderlich.");

            _resources.Spend(source.Resources, new BuildCost { Intel = intelSpent });
            var risk = CalculateDetectionRisk(user, source, target, type, intelSpent);
            var depth = CalculateReportDepth(user, source, target, type, intelSpent);
            return new EspionageMission { UserId = user.Id, User = user, SourceBaseId = source.Id, SourceBase = source, TargetBaseId = target.Id, TargetBase = target, MissionType = type, IntelSpent = intelSpent, ReportDepth = depth, DetectionRiskPercent = risk.DetectionRiskPercent, WasDetected = risk.WasDetected, TargetCounterIntelligenceLevel = risk.Level, CreatedAtUtc = now };
        }

        // Arming a decoy is a consumable countermeasure (see GAMEPLAY_IDEAS.md 1.2), not a passive stat: each charge is spent from Intel and used up on a single successful deception.
        public DecoyProfile ArmDecoy(PlayerBase playerBase, DecoyProfile existing, ResourceStock fakeValues, int fakeShipTotal, DateTime now)
        {
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (fakeValues == null) throw new ArgumentNullException("fakeValues");
            _resources.Spend(playerBase.Resources, new BuildCost { Intel = DecoyChargeIntelCost });
            var profile = existing ?? new DecoyProfile { PlayerBaseId = playerBase.Id, PlayerBase = playerBase };
            profile.IsActive = true;
            profile.FakeNaquadah = Math.Max(0, fakeValues.Naquadah);
            profile.FakeTrinium = Math.Max(0, fakeValues.Trinium);
            profile.FakeSupplies = Math.Max(0, fakeValues.Supplies);
            profile.FakeEnergy = Math.Max(0, fakeValues.Energy);
            profile.FakePersonnel = Math.Max(0, fakeValues.Personnel);
            profile.FakeIntel = Math.Max(0, fakeValues.Intel);
            profile.FakeShipTotal = Math.Max(0, fakeShipTotal);
            profile.Charges = Math.Min(MaxDecoyCharges, profile.Charges + 1);
            profile.LastArmedAtUtc = now;
            return profile;
        }

        // Higher attacker sensor/stealth skill sees through the bluff more often, so decoys can't fully neutralize espionage (balancing note in GAMEPLAY_IDEAS.md 1.2).
        public double CalculateDeceptionChance(User attacker)
        {
            int skill = attacker == null || attacker.ResearchLevels == null ? 0 : attacker.ResearchLevels.Sensorics + attacker.ResearchLevels.StealthTechnology;
            double chance = BaseDeceptionChance - skill * DeceptionChancePerAttackerSkillPoint;
            return Math.Max(MinDeceptionChance, Math.Min(BaseDeceptionChance, chance));
        }

        public IntelligenceReport CreateReport(EspionageMission mission, IEnumerable<PlanetSector> targetControlledSectors, IEnumerable<PlanetMarketOrder> targetMarketOrders, IEnumerable<FleetMovement> targetFleets, DateTime now, DecoyProfile decoy = null, Random random = null)
        {
            if (mission == null) throw new ArgumentNullException("mission");
            var t = mission.TargetBase ?? throw new InvalidOperationException("Mission target missing.");
            var displayedResources = t.Resources;
            int displayedShipTotal = TotalShipCount(t.Ships);
            bool decoyTriggered = false;
            if (decoy != null && decoy.IsActive && decoy.Charges > 0 && mission.TargetCounterIntelligenceLevel >= CounterIntelligenceLevel.Hardened)
            {
                var rnd = random ?? Random.Shared;
                if (rnd.NextDouble() < CalculateDeceptionChance(mission.User))
                {
                    decoyTriggered = true;
                    decoy.Charges--;
                    displayedResources = new ResourceStock { Naquadah = decoy.FakeNaquadah, Trinium = decoy.FakeTrinium, Supplies = decoy.FakeSupplies, Energy = decoy.FakeEnergy, Personnel = decoy.FakePersonnel, Intel = decoy.FakeIntel };
                    displayedShipTotal = decoy.FakeShipTotal;
                }
            }
            var parts = new List<string> { "Detailtiefe " + mission.ReportDepth + ": Basis " + t.Name + ", Fraktion " + (t.Faction != null ? t.Faction.Name : "unbekannt") + "." };
            if (mission.ReportDepth == 1) parts.Add("Ressourcen grob: " + CoarseResources(displayedResources) + ".");
            if (mission.ReportDepth >= 2) parts.Add("Gebäude grob: Sensorstation " + Coarse(t.BuildingLevels.SensorStation) + ", Verteidigung " + Coarse(t.BuildingLevels.DefenseRing) + ". Sektoren: " + (targetControlledSectors == null ? 0 : targetControlledSectors.Count()) + ".");
            if (mission.ReportDepth >= 3) parts.Add("Ressourcen genau: NQ " + displayedResources.Naquadah + ", TR " + displayedResources.Trinium + ", Vorräte " + displayedResources.Supplies + ", Energie " + displayedResources.Energy + ", Personal " + displayedResources.Personnel + ", Intel " + displayedResources.Intel + ". Gebäudelevel: Sensor " + t.BuildingLevels.SensorStation + ", Verteidigung " + t.BuildingLevels.DefenseRing + ", Gate " + t.BuildingLevels.GateControlRoom + ". Laufende Aktivitäten wurden erfasst, falls sichtbar.");
            if (mission.ReportDepth >= 4) parts.Add("Flotten/Schiffe: " + displayedShipTotal + " erfasst. Lokale Marktaktivitäten: " + (targetMarketOrders == null ? 0 : targetMarketOrders.Count()) + ". Gate-Aktivität: Kontrollraum Level " + t.BuildingLevels.GateControlRoom + ". Aktive Flottenbewegungen: " + (targetFleets == null ? 0 : targetFleets.Count()) + ".");
            if (decoyTriggered) parts.Add("Anzeichen für gezielte Falschinformationen wurden im Datenstrom erkannt – die Zuverlässigkeit dieses Berichts ist fraglich.");
            return new IntelligenceReport { UserId = mission.UserId, EspionageMission = mission, CreatedAtUtc = now, DetailDepth = mission.ReportDepth, WasDetected = mission.WasDetected, IsSuspectedDecoy = decoyTriggered, Title = "Geheimdienstbericht: " + mission.MissionType, Body = string.Join(" ", parts) };
        }

        public SpyDefenseResult CalculateDetectionRisk(User user, PlayerBase source, PlayerBase target, EspionageMissionType type, int intelSpent)
        {
            int attack = source.BuildingLevels.SensorStation + user.ResearchLevels.Sensorics + user.ResearchLevels.StealthTechnology + user.ResearchLevels.CovertInfiltration + user.ResearchLevels.DeepCoverNetworks + user.ResearchLevels.ShadowCouncilInfluence + IntelBonus(intelSpent) + FactionSpyBonus(user.Faction, type);
            int defense = target.BuildingLevels.SensorStation * 2 + (target.User != null && target.User.ResearchLevels != null ? target.User.ResearchLevels.Sensorics + target.User.ResearchLevels.CloakFieldCoordination : 0) + FactionDefenseBonus(target.Faction, type);
            int risk = Math.Max(5, Math.Min(95, 35 + defense * 8 - attack * 6));
            bool detected = risk >= 50;
            return new SpyDefenseResult { DetectionRiskPercent = risk, WasDetected = detected, Level = defense >= 8 ? CounterIntelligenceLevel.Lockdown : defense >= 5 ? CounterIntelligenceLevel.Hardened : defense >= 3 ? CounterIntelligenceLevel.Guarded : CounterIntelligenceLevel.Low, Summary = detected ? "Spionageaktivität entdeckt." : "Keine sichere Identifikation." };
        }

        public int CalculateReportDepth(User user, PlayerBase source, PlayerBase target, EspionageMissionType type, int intelSpent)
        {
            int attack = source.BuildingLevels.SensorStation + user.ResearchLevels.Sensorics + IntelBonus(intelSpent) + FactionSpyBonus(user.Faction, type);
            int defense = target.BuildingLevels.SensorStation + FactionDefenseBonus(target.Faction, type);
            int raw = 1 + (attack - defense + 2) / 3;
            if (user.ResearchLevels.Sensorics >= 1) raw++;
            return Math.Max(1, Math.Min(4, raw));
        }

        private static bool RequiresStealth(EspionageMissionType type) { return type == EspionageMissionType.AgentInfiltration || type == EspionageMissionType.GateObservation; }
        private static int IntelBonus(int intel) { return Math.Min(5, intel / 50); }
        private static int FactionSpyBonus(Faction f, EspionageMissionType t) { var s = f == null ? "" : f.ShortName; if (s == "Tok’ra") return t == EspionageMissionType.AgentInfiltration ? 4 : 2; if (s == "SGC") return t == EspionageMissionType.SensorRecon ? 2 : 0; if (s == "Lucian") return t == EspionageMissionType.MarketObservation ? 3 : t == EspionageMissionType.SensorRecon ? 1 : 0; if (s == "Jaffa") return -1; return 0; }
        private static int FactionDefenseBonus(Faction f, EspionageMissionType t) { var s = f == null ? "" : f.ShortName; if (s == "SGC") return 2; if (s == "Jaffa" && (t == EspionageMissionType.SensorRecon || t == EspionageMissionType.SectorRecon)) return 3; return 0; }
        private static string CoarseResources(ResourceStock r) { return "NQ " + Coarse(r.Naquadah) + ", TR " + Coarse(r.Trinium) + ", Intel " + Coarse(r.Intel); }
        private static string Coarse(int v) { return v < 100 ? "niedrig" : v < 1000 ? "mittel" : "hoch"; }
        private static int TotalShipCount(BaseShips s) { return s == null ? 0 : s.F302 + s.SmallTransporter + s.SupplyShuttle + s.Teltak + s.JaffaTransporter + s.CloakedTeltak + s.AgentTransporter + s.SmugglerTransporter + s.PirateFighter; }
    }
}
