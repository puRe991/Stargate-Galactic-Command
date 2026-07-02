using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class BuildQueueServiceTests
    {
        [Fact]
        public void CalculateCost_UsesVersion002Formula()
        {
            var catalog = new BuildingCatalogService();
            var cost = catalog.CalculateCost(BuildingType.NaquadahRefinery, 2);
            Assert.Equal(154, cost.Naquadah);
            Assert.Equal(77, cost.Trinium);
            Assert.Equal(52, cost.Supplies);
        }

        [Fact]
        public void CalculateBuildSeconds_UsesCommandCenterSpeedBonus()
        {
            var catalog = new BuildingCatalogService();
            int seconds = catalog.CalculateBuildSeconds(BuildingType.CommandCenter, 1, 1);
            Assert.Equal(86, seconds);
        }

        [Fact]
        public void StartBuild_RequiresEnoughResources()
        {
            var service = CreateService();
            var playerBase = CreateBase(new ResourceStock { Naquadah = 1, Trinium = 1, Supplies = 1 });
            Assert.Throws<InvalidOperationException>(() => service.StartBuild(playerBase, BuildingType.CommandCenter, DateTime.UtcNow));
            Assert.Empty(playerBase.BuildQueue);
        }

        [Fact]
        public void CompleteFinishedBuilds_IncreasesLevelExactlyOnce()
        {
            var service = CreateService();
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = CreateBase(new EconomyService().CreateStartingResources());
            service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);

            service.CompleteFinishedBuilds(playerBase, now.AddHours(1));
            service.CompleteFinishedBuilds(playerBase, now.AddHours(2));

            Assert.Equal(1, playerBase.BuildingLevels.NaquadahRefinery);
            Assert.Empty(playerBase.BuildQueue);
        }

        [Fact]
        public void ProductionChangesAfterBuildingUpgrade()
        {
            var economy = new EconomyService();
            var service = CreateService();
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = CreateBase(new EconomyService().CreateStartingResources());

            Assert.Equal(0, economy.CalculateHourlyProduction(playerBase.BuildingLevels).Naquadah);
            service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);
            service.CompleteFinishedBuilds(playerBase, now.AddHours(1));

            Assert.Equal(30, economy.CalculateHourlyProduction(playerBase.BuildingLevels).Naquadah);
        }

        private static BuildQueueService CreateService()
        {
            return new BuildQueueService(new BuildingCatalogService(), new ResourceService());
        }

        private static PlayerBase CreateBase(ResourceStock resources)
        {
            return new PlayerBase
            {
                Id = 1,
                Resources = resources,
                BuildingLevels = new BuildingLevels { CommandCenter = 1 },
                LastResourceUpdateUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }
    }
}
