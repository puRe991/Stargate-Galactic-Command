using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class AchievementServiceTests
    {
        [Fact]
        public void Get_ThrowsForUnknownKey()
        {
            var service = new AchievementService();
            Assert.Throws<ArgumentException>(() => service.Get("DoesNotExist"));
        }

        [Fact]
        public void GetAll_ReturnsAllTwelveDefinitions()
        {
            var service = new AchievementService();
            Assert.Equal(12, service.GetAll().Count);
        }

        [Fact]
        public void TryUnlock_ReturnsFalseWhenGoalNotReached()
        {
            var service = new AchievementService();
            var definition = service.Get("Explorer1");
            var progress = new AchievementProgress { UserId = 1, AchievementKey = definition.Key };

            bool unlocked = service.TryUnlock(definition, progress, definition.GoalAmount - 1, DateTime.UtcNow);

            Assert.False(unlocked);
            Assert.Null(progress.UnlockedAtUtc);
        }

        [Fact]
        public void TryUnlock_UnlocksOnceGoalIsReached()
        {
            var service = new AchievementService();
            var definition = service.Get("Explorer1");
            var progress = new AchievementProgress { UserId = 1, AchievementKey = definition.Key };
            var now = DateTime.UtcNow;

            bool unlocked = service.TryUnlock(definition, progress, definition.GoalAmount, now);

            Assert.True(unlocked);
            Assert.Equal(now, progress.UnlockedAtUtc);
        }

        [Fact]
        public void TryUnlock_ReturnsFalseWhenAlreadyUnlocked()
        {
            var service = new AchievementService();
            var definition = service.Get("Explorer1");
            var progress = new AchievementProgress { UserId = 1, AchievementKey = definition.Key, UnlockedAtUtc = DateTime.UtcNow.AddDays(-1) };

            bool unlocked = service.TryUnlock(definition, progress, definition.GoalAmount * 10, DateTime.UtcNow);

            Assert.False(unlocked);
        }

        [Fact]
        public void VersatileAgentGoal_MatchesNumberOfGateMissionTypes()
        {
            var service = new AchievementService();
            var definition = service.Get("Versatile1");
            int missionTypeCount = Enum.GetValues(typeof(GateMissionType)).Length;

            Assert.Equal(missionTypeCount, definition.GoalAmount);
        }
    }
}
