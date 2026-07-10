using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class ResearchCatalogService
    {
        // Fünf Zweige (allgemein + 4 Startfraktionen), jede Forschung hat eine echte, im Code verdrahtete Wirkung
        // (siehe EconomyService/GateMissionService/SpaceCombatService/LocalCombatService/EspionageService/PlanetMarketService/ShipyardService/BuildQueueService).
        private static readonly IList<ResearchDefinition> Definitions = new List<ResearchDefinition>
        {
            // Allgemein (14): Produktion, Bau-/Werfttempo, Anomalien, Diplomatie-Missionen
            new ResearchDefinition(ResearchType.GateAddressing, "Gate-Adressierung", null, new BuildCost { Naquadah = 120, Trinium = 80, Supplies = 80, Intel = 5 }, 60),
            new ResearchDefinition(ResearchType.NaquadahEnergyTechnology, "Naquadah-Energietechnik", null, new BuildCost { Naquadah = 140, Trinium = 100, Supplies = 60 }, 75),
            new ResearchDefinition(ResearchType.ShieldTechnology, "Schildtechnologie", null, new BuildCost { Naquadah = 180, Trinium = 160, Supplies = 90, Energy = 20 }, 90, ResearchType.NaquadahEnergyTechnology),
            new ResearchDefinition(ResearchType.Hyperdrive, "Hyperraumantrieb", null, new BuildCost { Naquadah = 240, Trinium = 220, Supplies = 130, Energy = 30 }, 120, ResearchType.NaquadahEnergyTechnology),
            new ResearchDefinition(ResearchType.Sensorics, "Sensorik", null, new BuildCost { Naquadah = 90, Trinium = 120, Supplies = 60, Intel = 5 }, 60),
            new ResearchDefinition(ResearchType.Medicine, "Medizin", null, new BuildCost { Naquadah = 70, Trinium = 70, Supplies = 160, Personnel = 5 }, 75),
            new ResearchDefinition(ResearchType.StealthTechnology, "Tarntechnologie", null, new BuildCost { Naquadah = 160, Trinium = 180, Supplies = 90, Intel = 15 }, 100, ResearchType.Sensorics),
            new ResearchDefinition(ResearchType.Diplomacy, "Diplomatie", null, new BuildCost { Naquadah = 60, Trinium = 60, Supplies = 180, Intel = 10 }, 80),
            new ResearchDefinition(ResearchType.Logistics, "Logistik", null, new BuildCost { Naquadah = 100, Trinium = 90, Supplies = 220 }, 80),
            new ResearchDefinition(ResearchType.AdvancedNaquadahRefining, "Fortgeschrittene Naquadah-Raffinierung", null, new BuildCost { Naquadah = 170, Trinium = 130, Supplies = 80, Intel = 5 }, 95, ResearchType.NaquadahEnergyTechnology),
            new ResearchDefinition(ResearchType.AutomatedTriniumExtraction, "Automatisierter Trinium-Abbau", null, new BuildCost { Naquadah = 150, Trinium = 160, Supplies = 80, Intel = 5 }, 95, ResearchType.NaquadahEnergyTechnology),
            new ResearchDefinition(ResearchType.StructuralEngineering, "Strukturtechnik", null, new BuildCost { Naquadah = 130, Trinium = 120, Supplies = 200 }, 90, ResearchType.Logistics),
            new ResearchDefinition(ResearchType.AdvancedShipEngineering, "Fortgeschrittener Schiffbau", null, new BuildCost { Naquadah = 220, Trinium = 240, Supplies = 150, Energy = 20 }, 130, ResearchType.Hyperdrive),
            new ResearchDefinition(ResearchType.XenoArchaeology, "Xenoarchäologie", null, new BuildCost { Naquadah = 140, Trinium = 150, Supplies = 90, Intel = 20 }, 100, ResearchType.Sensorics),

            // Tau'ri/SGC (9): Asgard/Antiker-Technologie, BC-304-Flotte, Iris-Verteidigung
            new ResearchDefinition(ResearchType.AsgardDataAnalysis, "Asgard-Datenanalyse", "SGC", new BuildCost { Naquadah = 150, Trinium = 110, Supplies = 90, Intel = 10 }, 90, ResearchType.Sensorics),
            new ResearchDefinition(ResearchType.Bc304Tactics, "BC-304-Taktik", "SGC", new BuildCost { Naquadah = 170, Trinium = 160, Supplies = 120, Intel = 10 }, 100),
            new ResearchDefinition(ResearchType.IrisSecurityProtocols, "Iris-Sicherheitsprotokolle", "SGC", new BuildCost { Naquadah = 120, Trinium = 100, Supplies = 100, Intel = 10 }, 80, ResearchType.GateAddressing),
            new ResearchDefinition(ResearchType.NaquadahReactorMiniaturization, "Naquadah-Reaktor-Miniaturisierung", "SGC", new BuildCost { Naquadah = 200, Trinium = 190, Supplies = 130, Intel = 10 }, 110, ResearchType.Bc304Tactics),
            new ResearchDefinition(ResearchType.AncientOutpostTechnology, "Antiker-Außenposten-Technologie", "SGC", new BuildCost { Naquadah = 210, Trinium = 180, Supplies = 130, Intel = 15 }, 115, ResearchType.AsgardDataAnalysis),
            new ResearchDefinition(ResearchType.PrometheusEngineering, "Prometheus-Ingenieurwesen", "SGC", new BuildCost { Naquadah = 280, Trinium = 260, Supplies = 170, Energy = 20, Intel = 10 }, 140, ResearchType.NaquadahReactorMiniaturization),
            new ResearchDefinition(ResearchType.StargateNetworkMapping, "Stargate-Netzwerkkartierung", "SGC", new BuildCost { Naquadah = 160, Trinium = 130, Supplies = 110, Intel = 10 }, 95, ResearchType.GateAddressing),
            new ResearchDefinition(ResearchType.AsgardBeamingTechnology, "Asgard-Beamtechnologie", "SGC", new BuildCost { Naquadah = 220, Trinium = 190, Supplies = 140, Intel = 15 }, 115, ResearchType.IrisSecurityProtocols),
            new ResearchDefinition(ResearchType.ZeroPointModuleTheory, "ZPM-Theorie", "SGC", new BuildCost { Naquadah = 350, Trinium = 320, Supplies = 200, Energy = 40, Intel = 25 }, 170, ResearchType.AsgardBeamingTechnology),

            // Freie Jaffa (9): Kriegerkultur, Ha'tak-Verteidigung, Bodentruppen
            new ResearchDefinition(ResearchType.StaffWeaponDiscipline, "Stabwaffen-Disziplin", "Jaffa", new BuildCost { Naquadah = 120, Trinium = 120, Supplies = 140, Personnel = 5 }, 80),
            new ResearchDefinition(ResearchType.HatakCommandStructure, "Ha’tak-Kommandostruktur", "Jaffa", new BuildCost { Naquadah = 180, Trinium = 180, Supplies = 110, Intel = 10 }, 100),
            new ResearchDefinition(ResearchType.JaffaWarriorCode, "Jaffa-Kriegerkodex", "Jaffa", new BuildCost { Naquadah = 90, Trinium = 90, Supplies = 160, Personnel = 10 }, 75),
            new ResearchDefinition(ResearchType.SymbioteEfficiency, "Symbiont-Effizienz", "Jaffa", new BuildCost { Naquadah = 150, Trinium = 140, Supplies = 180, Personnel = 10 }, 95, ResearchType.JaffaWarriorCode),
            new ResearchDefinition(ResearchType.GroundAssaultTactics, "Bodenangriffstaktik", "Jaffa", new BuildCost { Naquadah = 190, Trinium = 170, Supplies = 150, Personnel = 10 }, 105, ResearchType.StaffWeaponDiscipline),
            new ResearchDefinition(ResearchType.FortifiedGarrisons, "Befestigte Garnisonen", "Jaffa", new BuildCost { Naquadah = 210, Trinium = 200, Supplies = 130, Intel = 10 }, 110, ResearchType.HatakCommandStructure),
            new ResearchDefinition(ResearchType.KelNoReemTraining, "Kel-no-reem-Training", "Jaffa", new BuildCost { Naquadah = 160, Trinium = 150, Supplies = 170, Personnel = 15 }, 100, ResearchType.JaffaWarriorCode),
            new ResearchDefinition(ResearchType.HonorGuardProtocols, "Ehrengarde-Protokolle", "Jaffa", new BuildCost { Naquadah = 260, Trinium = 240, Supplies = 160, Intel = 15 }, 130, ResearchType.FortifiedGarrisons),
            new ResearchDefinition(ResearchType.FreeJaffaNationLogistics, "Logistik der Freien Jaffa-Nation", "Jaffa", new BuildCost { Naquadah = 220, Trinium = 200, Supplies = 240, Personnel = 15 }, 125, ResearchType.SymbioteEfficiency),

            // Tok'ra (9): Infiltration, Tarnung, Gegenspionage
            new ResearchDefinition(ResearchType.CovertInfiltration, "Verdeckte Infiltration", "Tok’ra", new BuildCost { Naquadah = 100, Trinium = 130, Supplies = 100, Intel = 15 }, 80, ResearchType.Sensorics),
            new ResearchDefinition(ResearchType.GoauldSabotage, "Goa’uld-Sabotage", "Tok’ra", new BuildCost { Naquadah = 130, Trinium = 140, Supplies = 100, Intel = 20 }, 90),
            new ResearchDefinition(ResearchType.CloakFieldCoordination, "Tarnfeldkoordination", "Tok’ra", new BuildCost { Naquadah = 160, Trinium = 180, Supplies = 100, Intel = 20 }, 100, ResearchType.StealthTechnology),
            new ResearchDefinition(ResearchType.SymbioteHealing, "Symbiontische Heilung", "Tok’ra", new BuildCost { Naquadah = 180, Trinium = 170, Supplies = 150, Intel = 20 }, 110, ResearchType.CovertInfiltration),
            new ResearchDefinition(ResearchType.DeepCoverNetworks, "Tiefentarnungs-Netzwerke", "Tok’ra", new BuildCost { Naquadah = 200, Trinium = 190, Supplies = 130, Intel = 25 }, 115, ResearchType.CovertInfiltration),
            new ResearchDefinition(ResearchType.HostBondingTechnology, "Wirtsbindungs-Technologie", "Tok’ra", new BuildCost { Naquadah = 240, Trinium = 220, Supplies = 180, Personnel = 15, Intel = 20 }, 130, ResearchType.SymbioteHealing),
            new ResearchDefinition(ResearchType.IntelligenceNetworkExpansion, "Ausbau des Geheimdienstnetzwerks", "Tok’ra", new BuildCost { Naquadah = 170, Trinium = 160, Supplies = 110, Intel = 25 }, 100, ResearchType.GoauldSabotage),
            new ResearchDefinition(ResearchType.SystemLordDossiers, "Systemherren-Dossiers", "Tok’ra", new BuildCost { Naquadah = 260, Trinium = 230, Supplies = 150, Intel = 30 }, 135, ResearchType.DeepCoverNetworks),
            new ResearchDefinition(ResearchType.ShadowCouncilInfluence, "Einfluss im Schattenrat", "Tok’ra", new BuildCost { Naquadah = 310, Trinium = 280, Supplies = 180, Intel = 35 }, 155, ResearchType.SystemLordDossiers),

            // Lucian-Allianz (9): Schmuggel, Schwarzmarkt, Söldnerwesen
            new ResearchDefinition(ResearchType.SmugglingRoutes, "Schmuggelrouten", "Lucian", new BuildCost { Naquadah = 90, Trinium = 80, Supplies = 180, Intel = 10 }, 75),
            new ResearchDefinition(ResearchType.BlackMarketLogistics, "Schwarzmarktlogistik", "Lucian", new BuildCost { Naquadah = 120, Trinium = 100, Supplies = 240 }, 90, ResearchType.Logistics),
            new ResearchDefinition(ResearchType.MercenaryContracts, "Söldnerverträge", "Lucian", new BuildCost { Naquadah = 110, Trinium = 100, Supplies = 200, Personnel = 5 }, 85),
            new ResearchDefinition(ResearchType.PirateNetworkConnections, "Piratennetzwerk-Kontakte", "Lucian", new BuildCost { Naquadah = 160, Trinium = 130, Supplies = 220, Intel = 10 }, 95, ResearchType.SmugglingRoutes),
            new ResearchDefinition(ResearchType.RuthlessNegotiation, "Rücksichtslose Verhandlungsführung", "Lucian", new BuildCost { Naquadah = 150, Trinium = 130, Supplies = 210, Personnel = 10 }, 95, ResearchType.MercenaryContracts),
            new ResearchDefinition(ResearchType.StolenTechnologyIntegration, "Integration gestohlener Technologie", "Lucian", new BuildCost { Naquadah = 200, Trinium = 170, Supplies = 260 }, 115, ResearchType.BlackMarketLogistics),
            new ResearchDefinition(ResearchType.HiddenCaches, "Versteckte Lagerstätten", "Lucian", new BuildCost { Naquadah = 220, Trinium = 190, Supplies = 230, Intel = 15 }, 120, ResearchType.PirateNetworkConnections),
            new ResearchDefinition(ResearchType.ExtortionNetworks, "Erpressungsnetzwerke", "Lucian", new BuildCost { Naquadah = 200, Trinium = 170, Supplies = 270, Personnel = 15 }, 120, ResearchType.RuthlessNegotiation),
            new ResearchDefinition(ResearchType.WarlordAmbitions, "Kriegsherren-Ambitionen", "Lucian", new BuildCost { Naquadah = 280, Trinium = 250, Supplies = 280, Personnel = 20 }, 150, ResearchType.StolenTechnologyIntegration)
        };

        public IEnumerable<ResearchDefinition> GetAvailableForFaction(Faction faction)
        {
            string shortName = faction == null ? null : faction.ShortName;
            return Definitions.Where(d => d.FactionShortName == null || string.Equals(d.FactionShortName, shortName, StringComparison.OrdinalIgnoreCase));
        }

        public ResearchDefinition Get(ResearchType type)
        {
            var definition = Definitions.FirstOrDefault(d => d.Type == type);
            if (definition == null) throw new ArgumentOutOfRangeException("type", "Unknown research type.");
            return definition;
        }

        public BuildCost CalculateCost(ResearchType type, int currentLevel)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException("currentLevel", "Research level must not be negative.");
            var baseCost = Get(type).BaseCost;
            double factor = Math.Pow(1.7, currentLevel);
            return new BuildCost { Naquadah = Scale(baseCost.Naquadah, factor), Trinium = Scale(baseCost.Trinium, factor), Supplies = Scale(baseCost.Supplies, factor), Energy = Scale(baseCost.Energy, factor), Personnel = Scale(baseCost.Personnel, factor), Intel = Scale(baseCost.Intel, factor) };
        }

        public int CalculateResearchSeconds(ResearchType type, int currentLevel, int researchLabLevel, double speedMultiplier)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException("currentLevel");
            if (researchLabLevel < 1) throw new InvalidOperationException("Forschung benötigt Forschungslabor Level 1.");
            double safeMultiplier = Math.Max(0.01, speedMultiplier);
            double seconds = Get(type).BaseSeconds * Math.Pow(1.55, currentLevel) / (1 + researchLabLevel * 0.05) / safeMultiplier;
            return Math.Max(1, (int)Math.Ceiling(seconds));
        }

        private static int Scale(int value, double factor)
        {
            if (value <= 0) return 0;
            double scaled = Math.Ceiling(value * factor);
            return scaled > int.MaxValue ? int.MaxValue : (int)scaled;
        }
    }
}
