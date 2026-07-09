using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class ContractServiceTests
    {
        private static PlayerBase Base() => new PlayerBase { Id = 1, Resources = new ResourceStock { Naquadah = 100, Trinium = 100, Supplies = 100, Energy = 100, Personnel = 100, Intel = 10 } };

        [Fact]
        public void Get_ThrowsForUnknownKey()
        {
            var service = new ContractService();
            Assert.Throws<ArgumentException>(() => service.Get("DoesNotExist"));
        }

        [Fact]
        public void GetPeriodStart_DailyContract_ReturnsCurrentDate()
        {
            var service = new ContractService();
            var definition = service.Get("DailyGateMissions");
            var now = new DateTime(2026, 1, 7, 15, 30, 0, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2026, 1, 7), service.GetPeriodStart(definition, now));
        }

        [Theory]
        [InlineData(2026, 1, 5, 2026, 1, 5)] // Monday -> same day
        [InlineData(2026, 1, 7, 2026, 1, 5)] // Wednesday -> Monday of that week
        [InlineData(2026, 1, 11, 2026, 1, 5)] // Sunday -> Monday of that week
        public void GetPeriodStart_WeeklyContract_ReturnsMondayOfTheWeek(int y, int m, int d, int ey, int em, int ed)
        {
            var service = new ContractService();
            var definition = service.Get("WeeklyGateMissions");
            var now = new DateTime(y, m, d, 12, 0, 0, DateTimeKind.Utc);

            Assert.Equal(new DateTime(ey, em, ed), service.GetPeriodStart(definition, now));
        }

        [Fact]
        public void Claim_ThrowsWhenGoalNotReached()
        {
            var service = new ContractService();
            var definition = service.Get("DailyGateMissions");
            var progress = new ContractProgress { UserId = 1, ContractKey = definition.Key };

            Assert.Throws<InvalidOperationException>(() => service.Claim(definition, progress, Base(), definition.GoalAmount - 1, DateTime.UtcNow));
        }

        [Fact]
        public void Claim_ThrowsWhenAlreadyClaimed()
        {
            var service = new ContractService();
            var definition = service.Get("DailyGateMissions");
            var progress = new ContractProgress { UserId = 1, ContractKey = definition.Key, ClaimedAtUtc = DateTime.UtcNow };

            Assert.Throws<InvalidOperationException>(() => service.Claim(definition, progress, Base(), definition.GoalAmount, DateTime.UtcNow));
        }

        [Fact]
        public void Claim_CreditsRewardAndMarksProgressClaimed()
        {
            var service = new ContractService();
            var definition = service.Get("DailyGateMissions");
            var playerBase = Base();
            var progress = new ContractProgress { UserId = 1, ContractKey = definition.Key };
            var now = DateTime.UtcNow;

            service.Claim(definition, progress, playerBase, definition.GoalAmount, now);

            Assert.Equal(100 + definition.Reward.Naquadah, playerBase.Resources.Naquadah);
            Assert.Equal(100 + definition.Reward.Trinium, playerBase.Resources.Trinium);
            Assert.Equal(now, progress.ClaimedAtUtc);
        }

        [Fact]
        public void GetDisplayName_UsesFactionFlavorWhenAvailable()
        {
            var service = new ContractService();
            var definition = service.Get("DailyGateMissions");

            Assert.Equal("SGC-Erkundungsauftrag", definition.GetDisplayName(new Faction { ShortName = "SGC" }));
            Assert.Equal(definition.Name, definition.GetDisplayName(new Faction { ShortName = "Unbekannt" }));
            Assert.Equal(definition.Name, definition.GetDisplayName(null));
        }
    }
}
