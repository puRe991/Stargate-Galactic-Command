using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class AllianceWarServiceTests
    {
        [Theory]
        [InlineData(1, 2)]
        [InlineData(4, 2)]
        [InlineData(6, 3)]
        [InlineData(10, 5)]
        public void CalculateRequiredSectors_ScalesWithMemberCountButHasAMinimum(int memberCount, int expected)
        {
            var service = new AllianceWarService();
            Assert.Equal(expected, service.CalculateRequiredSectors(memberCount));
        }

        [Fact]
        public void Declare_ThrowsWhenAllianceAlreadyHasActiveGoal()
        {
            var service = new AllianceWarService();
            var alliance = new Alliance { Id = 1 };
            var planet = new Planet { Id = 2 };
            Assert.Throws<InvalidOperationException>(() => service.Declare(alliance, planet, 6, hasActiveGoal: true, DateTime.UtcNow));
        }

        [Fact]
        public void Declare_CreatesActiveGoalWithScaledThreshold()
        {
            var service = new AllianceWarService();
            var alliance = new Alliance { Id = 1 };
            var planet = new Planet { Id = 2 };
            var now = DateTime.UtcNow;

            var goal = service.Declare(alliance, planet, 6, hasActiveGoal: false, now);

            Assert.Equal(AllianceWarGoalStatus.Active, goal.Status);
            Assert.Equal(3, goal.RequiredSectors);
            Assert.Equal(AllianceWarService.RequiredHours, goal.RequiredHours);
            Assert.Equal(now, goal.StartedAtUtc);
            Assert.Null(goal.HoldStreakStartedAtUtc);
        }

        [Fact]
        public void EvaluateProgress_ResetsStreakWhenBelowThreshold()
        {
            var service = new AllianceWarService();
            var now = DateTime.UtcNow;
            var goal = new AllianceWarGoal { RequiredSectors = 3, RequiredHours = 24, Status = AllianceWarGoalStatus.Active, HoldStreakStartedAtUtc = now.AddHours(-10) };

            bool achieved = service.EvaluateProgress(goal, currentControlledSectors: 2, now);

            Assert.False(achieved);
            Assert.Null(goal.HoldStreakStartedAtUtc);
            Assert.Equal(AllianceWarGoalStatus.Active, goal.Status);
        }

        [Fact]
        public void EvaluateProgress_StartsStreakWhenThresholdFirstMet()
        {
            var service = new AllianceWarService();
            var now = DateTime.UtcNow;
            var goal = new AllianceWarGoal { RequiredSectors = 3, RequiredHours = 24, Status = AllianceWarGoalStatus.Active };

            bool achieved = service.EvaluateProgress(goal, currentControlledSectors: 3, now);

            Assert.False(achieved);
            Assert.Equal(now, goal.HoldStreakStartedAtUtc);
            Assert.Equal(AllianceWarGoalStatus.Active, goal.Status);
        }

        [Fact]
        public void EvaluateProgress_AchievesGoalOnceHoldDurationElapses()
        {
            var service = new AllianceWarService();
            var now = DateTime.UtcNow;
            var goal = new AllianceWarGoal { RequiredSectors = 3, RequiredHours = 24, Status = AllianceWarGoalStatus.Active, HoldStreakStartedAtUtc = now.AddHours(-24) };

            bool achieved = service.EvaluateProgress(goal, currentControlledSectors: 3, now);

            Assert.True(achieved);
            Assert.Equal(AllianceWarGoalStatus.Achieved, goal.Status);
            Assert.Equal(now, goal.AchievedAtUtc);
        }

        [Fact]
        public void EvaluateProgress_ReturnsFalseOnSubsequentCallsAfterAchieved()
        {
            var service = new AllianceWarService();
            var now = DateTime.UtcNow;
            var goal = new AllianceWarGoal { RequiredSectors = 3, RequiredHours = 24, Status = AllianceWarGoalStatus.Achieved, AchievedAtUtc = now };

            Assert.False(service.EvaluateProgress(goal, currentControlledSectors: 99, now));
        }

        [Fact]
        public void Abandon_ThrowsWhenGoalIsNotActive()
        {
            var service = new AllianceWarService();
            var goal = new AllianceWarGoal { Status = AllianceWarGoalStatus.Achieved };
            Assert.Throws<InvalidOperationException>(() => service.Abandon(goal, DateTime.UtcNow));
        }

        [Fact]
        public void Abandon_MarksGoalAbandoned()
        {
            var service = new AllianceWarService();
            var goal = new AllianceWarGoal { Status = AllianceWarGoalStatus.Active };
            var now = DateTime.UtcNow;

            service.Abandon(goal, now);

            Assert.Equal(AllianceWarGoalStatus.Abandoned, goal.Status);
            Assert.Equal(now, goal.EndedAtUtc);
        }

        [Fact]
        public void ApplyVictoryReward_CreditsAllResourceTypes()
        {
            var service = new AllianceWarService();
            var resources = new ResourceStock { Naquadah = 10, Trinium = 10, Supplies = 10, Energy = 10, Personnel = 10, Intel = 10 };

            service.ApplyVictoryReward(resources);

            Assert.Equal(10 + AllianceWarService.VictoryRewardPerMember.Naquadah, resources.Naquadah);
            Assert.Equal(10 + AllianceWarService.VictoryRewardPerMember.Intel, resources.Intel);
        }
    }
}
