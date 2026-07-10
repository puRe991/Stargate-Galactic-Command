using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class WorldEventServiceTests
    {
        private static PlayerBase Base() => new PlayerBase { Resources = new ResourceStock { Naquadah = 100, Trinium = 100, Supplies = 1000, Energy = 100, Personnel = 1000, Intel = 10 } };

        [Fact]
        public void TryStartEvent_ReturnsNullWhenAnEventIsAlreadyActive()
        {
            var service = new WorldEventService();
            var active = new WorldEvent { Status = WorldEventStatus.Active };
            Assert.Null(service.TryStartEvent(active, null, DateTime.UtcNow));
        }

        [Fact]
        public void TryStartEvent_ReturnsNullDuringCooldown()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var lastResolved = new WorldEvent { ResolvedAtUtc = now.AddHours(-(WorldEventService.CooldownHoursBetweenEvents - 1)) };
            Assert.Null(service.TryStartEvent(null, lastResolved, now));
        }

        [Fact]
        public void TryStartEvent_StartsNewEventAfterCooldownAndAlternatesType()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var lastResolved = new WorldEvent { Type = WorldEventType.ReplicatorInvasion, ResolvedAtUtc = now.AddHours(-WorldEventService.CooldownHoursBetweenEvents) };

            var created = service.TryStartEvent(null, lastResolved, now);

            Assert.NotNull(created);
            Assert.Equal(WorldEventType.OriIncursion, created.Type);
            Assert.Equal(WorldEventStatus.Active, created.Status);
            Assert.Equal(now.AddHours(WorldEventService.EventDurationHours), created.EndsAtUtc);
            Assert.Equal(WorldEventService.GoalProgress, created.GoalProgress);
        }

        [Fact]
        public void TryStartEvent_StartsFirstEventImmediatelyWhenNoneEverHappened()
        {
            var service = new WorldEventService();
            var created = service.TryStartEvent(null, null, DateTime.UtcNow);
            Assert.NotNull(created);
            Assert.Equal(WorldEventType.ReplicatorInvasion, created.Type);
        }

        [Fact]
        public void Contribute_ThrowsWhenNotActive()
        {
            var service = new WorldEventService();
            var evt = new WorldEvent { Status = WorldEventStatus.Succeeded };
            Assert.Throws<InvalidOperationException>(() => service.Contribute(evt, new WorldEventContribution(), Base(), DateTime.UtcNow));
        }

        [Fact]
        public void Contribute_ThrowsDuringPersonalCooldown()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var evt = new WorldEvent { Status = WorldEventStatus.Active, EndsAtUtc = now.AddHours(10) };
            var contribution = new WorldEventContribution { LastContributedAtUtc = now.AddHours(-1) };
            Assert.Throws<InvalidOperationException>(() => service.Contribute(evt, contribution, Base(), now));
        }

        [Fact]
        public void Contribute_ThrowsWhenNotEnoughResources()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var evt = new WorldEvent { Status = WorldEventStatus.Active, EndsAtUtc = now.AddHours(10) };
            var playerBase = new PlayerBase { Resources = new ResourceStock { Supplies = 1, Personnel = 1 } };
            Assert.Throws<InvalidOperationException>(() => service.Contribute(evt, new WorldEventContribution(), playerBase, now));
        }

        [Fact]
        public void Contribute_DeductsCostAndAddsProgress()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var evt = new WorldEvent { Status = WorldEventStatus.Active, EndsAtUtc = now.AddHours(10), GoalProgress = WorldEventService.GoalProgress };
            var contribution = new WorldEventContribution();
            var playerBase = Base();

            service.Contribute(evt, contribution, playerBase, now);

            Assert.Equal(1000 - WorldEventService.ContributionCost.Supplies, playerBase.Resources.Supplies);
            Assert.Equal(1000 - WorldEventService.ContributionCost.Personnel, playerBase.Resources.Personnel);
            Assert.Equal(WorldEventService.ContributionAmount, evt.CurrentProgress);
            Assert.Equal(WorldEventService.ContributionAmount, contribution.TotalAmount);
            Assert.Equal(now, contribution.LastContributedAtUtc);
            Assert.Equal(WorldEventStatus.Active, evt.Status);
        }

        [Fact]
        public void Contribute_MarksEventSucceededOnceGoalReached()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var evt = new WorldEvent { Status = WorldEventStatus.Active, EndsAtUtc = now.AddHours(10), GoalProgress = WorldEventService.ContributionAmount, CurrentProgress = 0 };

            service.Contribute(evt, new WorldEventContribution(), Base(), now);

            Assert.Equal(WorldEventStatus.Succeeded, evt.Status);
            Assert.Equal(now, evt.ResolvedAtUtc);
        }

        [Fact]
        public void ResolveIfExpired_MarksFailedWhenGoalNotReached()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var evt = new WorldEvent { Status = WorldEventStatus.Active, EndsAtUtc = now.AddSeconds(-1), GoalProgress = 100, CurrentProgress = 50 };

            bool resolved = service.ResolveIfExpired(evt, now);

            Assert.True(resolved);
            Assert.Equal(WorldEventStatus.Failed, evt.Status);
        }

        [Fact]
        public void ResolveIfExpired_MarksSucceededWhenGoalWasReached()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var evt = new WorldEvent { Status = WorldEventStatus.Active, EndsAtUtc = now.AddSeconds(-1), GoalProgress = 100, CurrentProgress = 100 };

            Assert.True(service.ResolveIfExpired(evt, now));
            Assert.Equal(WorldEventStatus.Succeeded, evt.Status);
        }

        [Fact]
        public void ResolveIfExpired_ReturnsFalseBeforeEndTime()
        {
            var service = new WorldEventService();
            var now = DateTime.UtcNow;
            var evt = new WorldEvent { Status = WorldEventStatus.Active, EndsAtUtc = now.AddHours(1) };
            Assert.False(service.ResolveIfExpired(evt, now));
        }

        [Fact]
        public void TryGrantParticipationReward_RequiresContributionAndSucceededEvent()
        {
            var service = new WorldEventService();
            var resources = new ResourceStock();
            var succeeded = new WorldEvent { Status = WorldEventStatus.Succeeded };

            Assert.False(service.TryGrantParticipationReward(succeeded, new WorldEventContribution { TotalAmount = 0 }, resources, DateTime.UtcNow));
            Assert.False(service.TryGrantParticipationReward(new WorldEvent { Status = WorldEventStatus.Active }, new WorldEventContribution { TotalAmount = 10 }, resources, DateTime.UtcNow));
        }

        [Fact]
        public void TryGrantParticipationReward_GrantsOnceAndBlocksSecondCall()
        {
            var service = new WorldEventService();
            var resources = new ResourceStock();
            var evt = new WorldEvent { Status = WorldEventStatus.Succeeded };
            var contribution = new WorldEventContribution { TotalAmount = 10 };
            var now = DateTime.UtcNow;

            bool first = service.TryGrantParticipationReward(evt, contribution, resources, now);
            bool second = service.TryGrantParticipationReward(evt, contribution, resources, now);

            Assert.True(first);
            Assert.False(second);
            Assert.Equal(WorldEventService.ParticipationReward.Naquadah, resources.Naquadah);
        }
    }
}
