using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class AscensionServiceTests
    {
        [Theory]
        [InlineData(0, 0.0)]
        [InlineData(1, 0.03)]
        [InlineData(3, 0.09)]
        [InlineData(10, 0.30)]
        [InlineData(20, 0.30)]
        public void CalculateProductionBonus_ScalesLinearlyThenCaps(int ascensionCount, double expected)
        {
            var service = new AscensionService();
            Assert.Equal(expected, service.CalculateProductionBonus(ascensionCount), 3);
        }

        [Fact]
        public void ValidateCanAscend_ThrowsWhenScoreBelowThreshold()
        {
            var service = new AscensionService();
            Assert.Throws<InvalidOperationException>(() => service.ValidateCanAscend(AscensionService.MinScoreToAscend - 1, null, DateTime.UtcNow));
        }

        [Fact]
        public void ValidateCanAscend_ThrowsDuringCooldown()
        {
            var service = new AscensionService();
            var now = DateTime.UtcNow;
            var lastAscended = now.AddHours(-(AscensionService.MinHoursBetweenAscensions - 1));
            Assert.Throws<InvalidOperationException>(() => service.ValidateCanAscend(AscensionService.MinScoreToAscend, lastAscended, now));
        }

        [Fact]
        public void ValidateCanAscend_SucceedsWhenScoreMetAndNoPriorAscension()
        {
            var service = new AscensionService();
            service.ValidateCanAscend(AscensionService.MinScoreToAscend, null, DateTime.UtcNow);
        }

        [Fact]
        public void ValidateCanAscend_SucceedsAfterCooldownElapsed()
        {
            var service = new AscensionService();
            var now = DateTime.UtcNow;
            var lastAscended = now.AddHours(-AscensionService.MinHoursBetweenAscensions);
            service.ValidateCanAscend(AscensionService.MinScoreToAscend, lastAscended, now);
        }

        [Fact]
        public void Ascend_ResetsPowerButIncrementsCountAndTimestamp()
        {
            var service = new AscensionService();
            var economy = new EconomyService();
            var user = new User { AscensionCount = 1, ResearchLevels = new ResearchLevels { GateAddressing = 5, Sensorics = 3 } };
            var playerBase = new PlayerBase
            {
                Resources = new ResourceStock { Naquadah = 99999, Trinium = 99999, Supplies = 99999, Energy = 99999, Personnel = 99999, Intel = 99999 },
                BuildingLevels = new BuildingLevels { CommandCenter = 8, NaquadahRefinery = 6 },
                Ships = new BaseShips { F302 = 10, SmallTransporter = 5 }
            };
            var now = DateTime.UtcNow;

            service.Ascend(user, playerBase, economy.CreateStartingResources(), economy.CreateStartingBuildings(), now);

            Assert.Equal(2, user.AscensionCount);
            Assert.Equal(now, user.LastAscendedAtUtc);
            Assert.Equal(0, user.ResearchLevels.GateAddressing);
            Assert.Equal(0, user.ResearchLevels.Sensorics);
            Assert.Equal(1, playerBase.BuildingLevels.CommandCenter);
            Assert.Equal(0, playerBase.BuildingLevels.NaquadahRefinery);
            Assert.Equal(0, playerBase.Ships.F302);
            Assert.Equal(0, playerBase.Ships.SmallTransporter);
            Assert.Equal(EconomyService.StartNaquadah, playerBase.Resources.Naquadah);
            Assert.Equal(now, playerBase.LastResourceUpdateUtc);
        }
    }
}
