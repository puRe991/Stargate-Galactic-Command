using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class EconomyService
    {
        public BuildCost CalculateBuildingCost(BuildingType type, int targetLevel)
        {
            if (targetLevel < 1 || targetLevel > 100)
            {
                throw new ArgumentOutOfRangeException("targetLevel", "Target level must be between 1 and 100.");
            }

            BuildCost baseCost = GetBaseCost(type);
            double factor = Math.Pow(1.55, targetLevel - 1);

            return new BuildCost
            {
                Naquadah = Scale(baseCost.Naquadah, factor),
                Trinium = Scale(baseCost.Trinium, factor),
                Deuterium = Scale(baseCost.Deuterium, factor),
                Supplies = Scale(baseCost.Supplies, factor),
                Seconds = Math.Max(30, Scale(baseCost.Seconds, Math.Pow(1.35, targetLevel - 1)))
            };
        }

        public int CalculateHourlyProduction(BuildingType type, int level)
        {
            if (level < 0 || level > 100)
            {
                throw new ArgumentOutOfRangeException("level", "Level must be between 0 and 100.");
            }

            if (level == 0)
            {
                return 0;
            }

            if (type != BuildingType.NaquadahMine && type != BuildingType.TriniumExtractor && type != BuildingType.DeuteriumPlant && type != BuildingType.SupplyDepot)
            {
                return 0;
            }

            return (int)Math.Floor(20 * level * Math.Pow(1.12, level));
        }

        private BuildCost GetBaseCost(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.CommandBunker:
                    return new BuildCost { Naquadah = 80, Trinium = 60, Supplies = 40, Seconds = 45 };
                case BuildingType.NaquadahMine:
                    return new BuildCost { Naquadah = 60, Trinium = 20, Supplies = 20, Seconds = 35 };
                case BuildingType.TriniumExtractor:
                    return new BuildCost { Naquadah = 50, Trinium = 45, Supplies = 25, Seconds = 40 };
                case BuildingType.DeuteriumPlant:
                    return new BuildCost { Naquadah = 40, Deuterium = 25, Supplies = 30, Seconds = 40 };
                case BuildingType.SupplyDepot:
                    return new BuildCost { Naquadah = 35, Trinium = 35, Supplies = 15, Seconds = 30 };
                case BuildingType.ResearchLab:
                    return new BuildCost { Naquadah = 120, Trinium = 90, Deuterium = 40, Supplies = 80, Seconds = 90 };
                case BuildingType.Shipyard:
                    return new BuildCost { Naquadah = 160, Trinium = 140, Deuterium = 80, Supplies = 120, Seconds = 120 };
                case BuildingType.GateRoom:
                    return new BuildCost { Naquadah = 200, Trinium = 160, Deuterium = 120, Supplies = 160, Seconds = 150 };
                default:
                    throw new ArgumentOutOfRangeException("type", "Unknown building type.");
            }
        }

        private int Scale(int value, double factor)
        {
            if (value <= 0)
            {
                return 0;
            }

            double scaled = Math.Ceiling(value * factor);
            if (scaled > int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)scaled;
        }
    }
}
