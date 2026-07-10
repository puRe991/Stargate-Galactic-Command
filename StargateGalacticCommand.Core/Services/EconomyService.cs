using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class EconomyService
    {
        private readonly FactionModifierService _factionModifiers;
        private readonly AscensionService _ascension;

        public EconomyService() : this(new FactionModifierService(), new AscensionService()) { }

        public EconomyService(FactionModifierService factionModifiers) : this(factionModifiers, new AscensionService()) { }

        public EconomyService(FactionModifierService factionModifiers, AscensionService ascension)
        {
            _factionModifiers = factionModifiers ?? throw new ArgumentNullException(nameof(factionModifiers));
            _ascension = ascension ?? throw new ArgumentNullException(nameof(ascension));
        }

        public const int StartNaquadah = 500, StartTrinium = 500, StartSupplies = 750, StartEnergy = 100, StartPersonnel = 50, StartIntel = 0;

        public ResourceStock CreateStartingResources()
        {
            return new ResourceStock { Naquadah = StartNaquadah, Trinium = StartTrinium, Supplies = StartSupplies, Energy = StartEnergy, Personnel = StartPersonnel, Intel = StartIntel };
        }

        public BuildingLevels CreateStartingBuildings()
        {
            return new BuildingLevels { CommandCenter = 1 };
        }

        public ResourceProduction CalculateHourlyProduction(BuildingLevels levels)
        {
            return CalculateHourlyProduction(levels, null, null);
        }

        public ResourceProduction CalculateHourlyProduction(BuildingLevels levels, ResearchLevels researchLevels, Faction faction)
        {
            return CalculateHourlyProduction(levels, researchLevels, faction, null);
        }

        public ResourceProduction CalculateHourlyProduction(BuildingLevels levels, ResearchLevels researchLevels, Faction faction, SectorBonus sectorBonus)
        {
            return CalculateHourlyProduction(levels, researchLevels, faction, sectorBonus, 0);
        }

        public ResourceProduction CalculateHourlyProduction(BuildingLevels levels, ResearchLevels researchLevels, Faction faction, SectorBonus sectorBonus, int ascensionCount)
        {
            if (levels == null) throw new ArgumentNullException("levels");
            double naquadahMultiplier = 1;
            double triniumMultiplier = 1;
            double personnelMultiplier = 1;
            double energyMultiplier = 1 + (Math.Max(0, researchLevels == null ? 0 : researchLevels.NaquadahEnergyTechnology) * 0.02);
            double intelMultiplier = (1 + (Math.Max(0, researchLevels == null ? 0 : researchLevels.Sensorics) * 0.02)) * _factionModifiers.GetIntelProductionMultiplier(faction);
            double suppliesMultiplier = _factionModifiers.GetSuppliesProductionMultiplier(faction);
            if (sectorBonus != null)
            {
                naquadahMultiplier = 1 + Math.Max(0, sectorBonus.NaquadahMultiplier);
                triniumMultiplier = 1 + Math.Max(0, sectorBonus.TriniumMultiplier);
                suppliesMultiplier *= 1 + Math.Max(0, sectorBonus.SuppliesMultiplier);
                personnelMultiplier = 1 + Math.Max(0, sectorBonus.PersonnelMultiplier);
                intelMultiplier *= 1 + Math.Max(0, sectorBonus.IntelMultiplier);
            }
            double ascensionMultiplier = 1 + _ascension.CalculateProductionBonus(ascensionCount);
            return new ResourceProduction
            {
                Naquadah = ScaleProduction(30 * Math.Max(0, levels.NaquadahRefinery), naquadahMultiplier * ascensionMultiplier),
                Trinium = ScaleProduction(25 * Math.Max(0, levels.TriniumMine), triniumMultiplier * ascensionMultiplier),
                Supplies = ScaleProduction(35 * Math.Max(0, levels.SupplyDepot), suppliesMultiplier * ascensionMultiplier),
                Energy = ScaleProduction(20 * Math.Max(0, levels.EnergyGenerator), energyMultiplier * ascensionMultiplier),
                Personnel = ScaleProduction(2 * Math.Max(0, levels.CommandCenter), personnelMultiplier * ascensionMultiplier),
                Intel = ScaleProduction(1 * Math.Max(0, levels.SensorStation), intelMultiplier * ascensionMultiplier)
            };
        }

        public void ApplyOfflineProduction(PlayerBase playerBase, DateTime nowUtc)
        {
            ApplyOfflineProduction(playerBase, nowUtc, null);
        }

        public void ApplyOfflineProduction(PlayerBase playerBase, DateTime nowUtc, SectorBonus sectorBonus)
        {
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (playerBase.Resources == null) throw new ArgumentException("Base has no resource stock.", "playerBase");
            if (playerBase.BuildingLevels == null) throw new ArgumentException("Base has no building levels.", "playerBase");
            if (nowUtc <= playerBase.LastResourceUpdateUtc) return;

            double hours = (nowUtc - playerBase.LastResourceUpdateUtc).TotalHours;
            ResourceProduction hourly = CalculateHourlyProduction(playerBase.BuildingLevels, playerBase.User == null ? null : playerBase.User.ResearchLevels, playerBase.Faction, sectorBonus, playerBase.User == null ? 0 : playerBase.User.AscensionCount);
            playerBase.Resources.Naquadah = AddCapped(playerBase.Resources.Naquadah, hourly.Naquadah, hours);
            playerBase.Resources.Trinium = AddCapped(playerBase.Resources.Trinium, hourly.Trinium, hours);
            playerBase.Resources.Supplies = AddCapped(playerBase.Resources.Supplies, hourly.Supplies, hours);
            playerBase.Resources.Energy = AddCapped(playerBase.Resources.Energy, hourly.Energy, hours);
            playerBase.Resources.Personnel = AddCapped(playerBase.Resources.Personnel, hourly.Personnel, hours);
            playerBase.Resources.Intel = AddCapped(playerBase.Resources.Intel, hourly.Intel, hours);
            playerBase.LastResourceUpdateUtc = nowUtc;
        }

        private static int ScaleProduction(int value, double multiplier)
        {
            if (value <= 0) return 0;
            double scaled = Math.Floor(value * Math.Max(0, multiplier));
            return scaled > int.MaxValue ? int.MaxValue : (int)scaled;
        }

        private static int AddCapped(int current, int perHour, double hours)
        {
            if (perHour <= 0 || hours <= 0) return current;
            double value = current + Math.Floor(perHour * hours);
            return value > int.MaxValue ? int.MaxValue : (int)value;
        }
    }

    public class ResourceProduction
    {
        public int Naquadah { get; set; }
        public int Trinium { get; set; }
        public int Supplies { get; set; }
        public int Energy { get; set; }
        public int Personnel { get; set; }
        public int Intel { get; set; }
    }
}
