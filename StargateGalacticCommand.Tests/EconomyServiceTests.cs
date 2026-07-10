using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class EconomyServiceTests
    {
        [Fact]
        public void CreateStartingResources_ReturnsVersion001StartValues()
        {
            var stock = new EconomyService().CreateStartingResources();
            Assert.Equal(500, stock.Naquadah); Assert.Equal(500, stock.Trinium); Assert.Equal(750, stock.Supplies);
            Assert.Equal(100, stock.Energy); Assert.Equal(50, stock.Personnel); Assert.Equal(0, stock.Intel);
        }

        [Fact]
        public void ApplyOfflineProduction_AddsResourcesForElapsedTime()
        {
            var service = new EconomyService();
            var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = new PlayerBase { Resources = service.CreateStartingResources(), BuildingLevels = new BuildingLevels { CommandCenter = 1, NaquadahRefinery = 2, TriniumMine = 1, SupplyDepot = 1, EnergyGenerator = 1 }, LastResourceUpdateUtc = start };
            service.ApplyOfflineProduction(playerBase, start.AddHours(2));
            Assert.Equal(620, playerBase.Resources.Naquadah); Assert.Equal(550, playerBase.Resources.Trinium); Assert.Equal(820, playerBase.Resources.Supplies);
            Assert.Equal(140, playerBase.Resources.Energy); Assert.Equal(54, playerBase.Resources.Personnel);
        }

        [Fact]
        public void ApplyOfflineProduction_AppliesAscensionBonusWhenUserIsLoaded()
        {
            var service = new EconomyService();
            var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var user = new User { AscensionCount = 1 };
            var playerBase = new PlayerBase { User = user, Resources = service.CreateStartingResources(), BuildingLevels = new BuildingLevels { CommandCenter = 1, NaquadahRefinery = 2 }, LastResourceUpdateUtc = start };
            service.ApplyOfflineProduction(playerBase, start.AddHours(2));
            // Ohne Bonus wären es 620 (siehe ApplyOfflineProduction_AddsResourcesForElapsedTime); der Ascension-Bonus (+3 %) erhöht den stündlichen Ertrag von 60 auf 61.
            Assert.Equal(622, playerBase.Resources.Naquadah);
        }

        [Fact]
        public void ApplyOfflineProduction_DoesNotChangeResourcesWhenNoTimeElapsed()
        {
            var service = new EconomyService();
            var now = DateTime.UtcNow;
            var playerBase = new PlayerBase { Resources = service.CreateStartingResources(), BuildingLevels = new BuildingLevels { NaquadahRefinery = 10 }, LastResourceUpdateUtc = now };
            service.ApplyOfflineProduction(playerBase, now.AddMinutes(-1));
            Assert.Equal(500, playerBase.Resources.Naquadah);
        }
        [Fact]
        public void CalculateHourlyProduction_AppliesSectorBonus()
        {
            var production = new EconomyService().CalculateHourlyProduction(new BuildingLevels { CommandCenter = 20, NaquadahRefinery = 10 }, null, null, new SectorBonus { NaquadahMultiplier = 0.10, PersonnelMultiplier = 0.05 });
            Assert.Equal(330, production.Naquadah);
            Assert.Equal(42, production.Personnel);
        }

        [Fact]
        public void CalculateHourlyProduction_ResearchBonusStacksWithSectorBonus()
        {
            // Regressionstest: Sektorbonus muss den Forschungsbonus multiplizieren, nicht überschreiben.
            var levels = new BuildingLevels { CommandCenter = 1, NaquadahRefinery = 10 };
            var research = new ResearchLevels { AdvancedNaquadahRefining = 10 };
            var withoutSector = new EconomyService().CalculateHourlyProduction(levels, research, null, null);
            var withSector = new EconomyService().CalculateHourlyProduction(levels, research, null, new SectorBonus { NaquadahMultiplier = 0.10 });
            Assert.True(withoutSector.Naquadah > 300, "Forschungsbonus allein sollte bereits über der Basisproduktion liegen.");
            Assert.True(withSector.Naquadah > withoutSector.Naquadah, "Sektorbonus muss zusätzlich zum Forschungsbonus wirken, nicht ihn ersetzen.");
        }

        [Theory]
        [InlineData(nameof(ResearchLevels.AdvancedNaquadahRefining))]
        [InlineData(nameof(ResearchLevels.HiddenCaches))]
        public void CalculateHourlyProduction_NaquadahResearchIncreasesOutput(string field)
        {
            var levels = new BuildingLevels { CommandCenter = 1, NaquadahRefinery = 10 };
            var research = new ResearchLevels();
            typeof(ResearchLevels).GetProperty(field).SetValue(research, 10);
            var boosted = new EconomyService().CalculateHourlyProduction(levels, research, null, null);
            var baseline = new EconomyService().CalculateHourlyProduction(levels, null, null, null);
            Assert.True(boosted.Naquadah > baseline.Naquadah);
        }

        [Fact]
        public void CalculateHourlyProduction_SupplyResearchIncreasesOutput()
        {
            var levels = new BuildingLevels { CommandCenter = 1, SupplyDepot = 10 };
            var research = new ResearchLevels { Logistics = 5, FreeJaffaNationLogistics = 5, ExtortionNetworks = 5 };
            var boosted = new EconomyService().CalculateHourlyProduction(levels, research, null, null);
            var baseline = new EconomyService().CalculateHourlyProduction(levels, null, null, null);
            Assert.True(boosted.Supplies > baseline.Supplies);
        }

        [Fact]
        public void CalculateHourlyProduction_PersonnelResearchIncreasesOutput()
        {
            var levels = new BuildingLevels { CommandCenter = 20 };
            var research = new ResearchLevels { Medicine = 10, SymbioteEfficiency = 10, HostBondingTechnology = 10 };
            var boosted = new EconomyService().CalculateHourlyProduction(levels, research, null, null);
            var baseline = new EconomyService().CalculateHourlyProduction(levels, null, null, null);
            Assert.True(boosted.Personnel > baseline.Personnel);
        }

        [Fact]
        public void CalculateHourlyProduction_EnergyResearchIncreasesOutput()
        {
            var levels = new BuildingLevels { CommandCenter = 1, EnergyGenerator = 10 };
            var research = new ResearchLevels { ZeroPointModuleTheory = 10 };
            var boosted = new EconomyService().CalculateHourlyProduction(levels, research, null, null);
            var baseline = new EconomyService().CalculateHourlyProduction(levels, null, null, null);
            Assert.True(boosted.Energy > baseline.Energy);
        }

        [Fact]
        public void CalculateHourlyProduction_IntelResearchIncreasesOutput()
        {
            var levels = new BuildingLevels { CommandCenter = 1, SensorStation = 10 };
            var research = new ResearchLevels { IntelligenceNetworkExpansion = 10 };
            var boosted = new EconomyService().CalculateHourlyProduction(levels, research, null, null);
            var baseline = new EconomyService().CalculateHourlyProduction(levels, null, null, null);
            Assert.True(boosted.Intel > baseline.Intel);
        }
    }
}
