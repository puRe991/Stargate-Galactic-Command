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
        public void StartBuild_QueuesSecondOrderInsteadOfBlocking()
        {
            var service = CreateService();
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = CreateBase(new EconomyService().CreateStartingResources());

            service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);
            service.StartBuild(playerBase, BuildingType.TriniumMine, now);

            Assert.Equal(2, playerBase.BuildQueue.Count);
        }

        [Fact]
        public void StartBuild_ChainsSecondOrderAfterFirstCompletion()
        {
            var service = CreateService();
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = CreateBase(new EconomyService().CreateStartingResources());

            var first = service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);
            var second = service.StartBuild(playerBase, BuildingType.TriniumMine, now);

            Assert.Equal(first.CompletesAtUtc, second.StartedAtUtc);
            Assert.True(second.CompletesAtUtc > first.CompletesAtUtc);
        }

        [Fact]
        public void StartBuild_SecondOrderOfSameBuildingUsesNextLevelCost()
        {
            var service = CreateService();
            var catalog = new BuildingCatalogService();
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = CreateBase(new EconomyService().CreateStartingResources());
            int startingNaquadah = playerBase.Resources.Naquadah;

            var first = service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);
            var second = service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);

            Assert.Equal(1, first.TargetLevel);
            Assert.Equal(2, second.TargetLevel);
            int spent = startingNaquadah - playerBase.Resources.Naquadah;
            int expectedSpent = catalog.CalculateCost(BuildingType.NaquadahRefinery, 0).Naquadah
                + catalog.CalculateCost(BuildingType.NaquadahRefinery, 1).Naquadah;
            Assert.Equal(expectedSpent, spent);
        }

        [Fact]
        public void StartBuild_ThrowsWhenQueueIsFull()
        {
            var service = CreateService();
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = CreateBase(new ResourceStock { Naquadah = 1_000_000, Trinium = 1_000_000, Supplies = 1_000_000, Energy = 1_000_000, Personnel = 1_000_000, Intel = 1_000_000 });

            for (int i = 0; i < BuildQueueService.MaxQueueLength; i++)
            {
                service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);
            }

            Assert.Throws<InvalidOperationException>(() => service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now));
            Assert.Equal(BuildQueueService.MaxQueueLength, playerBase.BuildQueue.Count);
        }

        [Fact]
        public void CompleteFinishedBuilds_AppliesQueuedOrdersInSequence()
        {
            var service = CreateService();
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = CreateBase(new EconomyService().CreateStartingResources());

            service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);
            var second = service.StartBuild(playerBase, BuildingType.NaquadahRefinery, now);

            service.CompleteFinishedBuilds(playerBase, second.CompletesAtUtc);

            Assert.Equal(2, playerBase.BuildingLevels.NaquadahRefinery);
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
