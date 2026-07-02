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
        public void ApplyOfflineProduction_DoesNotChangeResourcesWhenNoTimeElapsed()
        {
            var service = new EconomyService();
            var now = DateTime.UtcNow;
            var playerBase = new PlayerBase { Resources = service.CreateStartingResources(), BuildingLevels = new BuildingLevels { NaquadahRefinery = 10 }, LastResourceUpdateUtc = now };
            service.ApplyOfflineProduction(playerBase, now.AddMinutes(-1));
            Assert.Equal(500, playerBase.Resources.Naquadah);
        }
    }
}
