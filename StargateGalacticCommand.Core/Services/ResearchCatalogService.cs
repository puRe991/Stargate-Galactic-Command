using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class ResearchCatalogService
    {
        private static readonly IList<ResearchDefinition> Definitions = new List<ResearchDefinition>
        {
            new ResearchDefinition(ResearchType.GateAddressing, "Gate-Adressierung", null, new BuildCost { Naquadah = 120, Trinium = 80, Supplies = 80, Intel = 5 }, 60),
            new ResearchDefinition(ResearchType.NaquadahEnergyTechnology, "Naquadah-Energietechnik", null, new BuildCost { Naquadah = 140, Trinium = 100, Supplies = 60 }, 75),
            new ResearchDefinition(ResearchType.ShieldTechnology, "Schildtechnologie", null, new BuildCost { Naquadah = 180, Trinium = 160, Supplies = 90, Energy = 20 }, 90, ResearchType.NaquadahEnergyTechnology),
            new ResearchDefinition(ResearchType.Hyperdrive, "Hyperraumantrieb", null, new BuildCost { Naquadah = 240, Trinium = 220, Supplies = 130, Energy = 30 }, 120, ResearchType.NaquadahEnergyTechnology),
            new ResearchDefinition(ResearchType.Sensorics, "Sensorik", null, new BuildCost { Naquadah = 90, Trinium = 120, Supplies = 60, Intel = 5 }, 60),
            new ResearchDefinition(ResearchType.Medicine, "Medizin", null, new BuildCost { Naquadah = 70, Trinium = 70, Supplies = 160, Personnel = 5 }, 75),
            new ResearchDefinition(ResearchType.StealthTechnology, "Tarntechnologie", null, new BuildCost { Naquadah = 160, Trinium = 180, Supplies = 90, Intel = 15 }, 100, ResearchType.Sensorics),
            new ResearchDefinition(ResearchType.Diplomacy, "Diplomatie", null, new BuildCost { Naquadah = 60, Trinium = 60, Supplies = 180, Intel = 10 }, 80),
            new ResearchDefinition(ResearchType.Logistics, "Logistik", null, new BuildCost { Naquadah = 100, Trinium = 90, Supplies = 220 }, 80),
            new ResearchDefinition(ResearchType.AsgardDataAnalysis, "Asgard-Datenanalyse", "SGC", new BuildCost { Naquadah = 150, Trinium = 110, Supplies = 90, Intel = 10 }, 90, ResearchType.Sensorics),
            new ResearchDefinition(ResearchType.Bc304Tactics, "BC-304-Taktik", "SGC", new BuildCost { Naquadah = 170, Trinium = 160, Supplies = 120, Intel = 10 }, 100),
            new ResearchDefinition(ResearchType.IrisSecurityProtocols, "Iris-Sicherheitsprotokolle", "SGC", new BuildCost { Naquadah = 120, Trinium = 100, Supplies = 100, Intel = 10 }, 80, ResearchType.GateAddressing),
            new ResearchDefinition(ResearchType.StaffWeaponDiscipline, "Stabwaffen-Disziplin", "Jaffa", new BuildCost { Naquadah = 120, Trinium = 120, Supplies = 140, Personnel = 5 }, 80),
            new ResearchDefinition(ResearchType.HatakCommandStructure, "Ha’tak-Kommandostruktur", "Jaffa", new BuildCost { Naquadah = 180, Trinium = 180, Supplies = 110, Intel = 10 }, 100),
            new ResearchDefinition(ResearchType.JaffaWarriorCode, "Jaffa-Kriegerkodex", "Jaffa", new BuildCost { Naquadah = 90, Trinium = 90, Supplies = 160, Personnel = 10 }, 75),
            new ResearchDefinition(ResearchType.CovertInfiltration, "Verdeckte Infiltration", "Tok’ra", new BuildCost { Naquadah = 100, Trinium = 130, Supplies = 100, Intel = 15 }, 80, ResearchType.Sensorics),
            new ResearchDefinition(ResearchType.GoauldSabotage, "Goa’uld-Sabotage", "Tok’ra", new BuildCost { Naquadah = 130, Trinium = 140, Supplies = 100, Intel = 20 }, 90),
            new ResearchDefinition(ResearchType.CloakFieldCoordination, "Tarnfeldkoordination", "Tok’ra", new BuildCost { Naquadah = 160, Trinium = 180, Supplies = 100, Intel = 20 }, 100, ResearchType.StealthTechnology),
            new ResearchDefinition(ResearchType.SmugglingRoutes, "Schmuggelrouten", "Lucian", new BuildCost { Naquadah = 90, Trinium = 80, Supplies = 180, Intel = 10 }, 75),
            new ResearchDefinition(ResearchType.BlackMarketLogistics, "Schwarzmarktlogistik", "Lucian", new BuildCost { Naquadah = 120, Trinium = 100, Supplies = 240 }, 90, ResearchType.Logistics),
            new ResearchDefinition(ResearchType.MercenaryContracts, "Söldnerverträge", "Lucian", new BuildCost { Naquadah = 110, Trinium = 100, Supplies = 200, Personnel = 5 }, 85)
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
