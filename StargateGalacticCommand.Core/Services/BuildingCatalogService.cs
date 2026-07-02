using System;
using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class BuildingCatalogService
    {
        private static readonly IDictionary<BuildingType, BuildingDefinition> Definitions = new Dictionary<BuildingType, BuildingDefinition>
        {
            { BuildingType.CommandCenter, new BuildingDefinition(BuildingType.CommandCenter, "Kommandozentrum", new BuildCost { Naquadah = 200, Trinium = 200, Supplies = 150 }, 60) },
            { BuildingType.NaquadahRefinery, new BuildingDefinition(BuildingType.NaquadahRefinery, "Naquadah-Raffinerie", new BuildCost { Naquadah = 60, Trinium = 30, Supplies = 20 }, 30) },
            { BuildingType.TriniumMine, new BuildingDefinition(BuildingType.TriniumMine, "Trinium-Mine", new BuildCost { Naquadah = 48, Trinium = 60, Supplies = 20 }, 30) },
            { BuildingType.SupplyDepot, new BuildingDefinition(BuildingType.SupplyDepot, "Versorgungslager", new BuildCost { Naquadah = 40, Trinium = 40, Supplies = 60 }, 30) },
            { BuildingType.EnergyGenerator, new BuildingDefinition(BuildingType.EnergyGenerator, "Energiegenerator", new BuildCost { Naquadah = 75, Trinium = 30, Supplies = 30 }, 45) },
            { BuildingType.ResearchLab, new BuildingDefinition(BuildingType.ResearchLab, "Forschungslabor", new BuildCost { Naquadah = 150, Trinium = 200, Supplies = 100 }, 90) },
            { BuildingType.GateControlRoom, new BuildingDefinition(BuildingType.GateControlRoom, "Gate-Kontrollraum", new BuildCost { Naquadah = 300, Trinium = 250, Supplies = 150, Intel = 10 }, 120) },
            { BuildingType.SensorStation, new BuildingDefinition(BuildingType.SensorStation, "Sensorstation", new BuildCost { Naquadah = 180, Trinium = 120, Supplies = 80 }, 75) },
            { BuildingType.DefenseRing, new BuildingDefinition(BuildingType.DefenseRing, "Verteidigungsring", new BuildCost { Naquadah = 200, Trinium = 250, Supplies = 150 }, 100) },
            { BuildingType.HangarLandingZone, new BuildingDefinition(BuildingType.HangarLandingZone, "Hangar / Landezone", new BuildCost { Naquadah = 250, Trinium = 300, Supplies = 200 }, 110) }
        };

        public IEnumerable<BuildingDefinition> GetAll() { return Definitions.Values; }
        public BuildingDefinition Get(BuildingType type) { return Definitions[type]; }

        public BuildCost CalculateCost(BuildingType type, int currentLevel)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException("currentLevel", "Building level must not be negative.");
            var baseCost = Get(type).BaseCost;
            var factor = Math.Pow(1.6, currentLevel);
            return new BuildCost
            {
                Naquadah = Scale(baseCost.Naquadah, factor),
                Trinium = Scale(baseCost.Trinium, factor),
                Supplies = Scale(baseCost.Supplies, factor),
                Energy = Scale(baseCost.Energy, factor),
                Personnel = Scale(baseCost.Personnel, factor),
                Intel = Scale(baseCost.Intel, factor)
            };
        }

        public int CalculateBuildSeconds(BuildingType type, int currentLevel, int commandCenterLevel)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException("currentLevel", "Building level must not be negative.");
            int safeCommandLevel = Math.Max(0, commandCenterLevel);
            double seconds = Get(type).BaseSeconds * Math.Pow(1.5, currentLevel) / (1 + safeCommandLevel * 0.05);
            return Math.Max(1, (int)Math.Ceiling(seconds));
        }

        private static int Scale(int value, double factor)
        {
            if (value <= 0) return 0;
            double scaled = Math.Ceiling(value * factor);
            return scaled > int.MaxValue ? int.MaxValue : (int)scaled;
        }
    }

    public class BuildingDefinition
    {
        public BuildingDefinition(BuildingType type, string name, BuildCost baseCost, int baseSeconds)
        {
            Type = type; Name = name; BaseCost = baseCost; BaseSeconds = baseSeconds;
        }
        public BuildingType Type { get; private set; }
        public string Name { get; private set; }
        public BuildCost BaseCost { get; private set; }
        public int BaseSeconds { get; private set; }
    }
}
