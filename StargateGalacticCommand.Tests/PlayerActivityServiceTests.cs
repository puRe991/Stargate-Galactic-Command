using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class PlayerActivityServiceTests
    {
        [Fact]
        public void CountActiveHumanPlayersCountsRecentlySeenHumans()
        {
            var now = new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc);
            var users = new[]
            {
                new User { Id = 1, UserName = "active", LastSeenAtUtc = now.AddMinutes(-4), IsNpc = false },
                new User { Id = 2, UserName = "inactive", LastSeenAtUtc = now.AddMinutes(-6), IsNpc = false },
                new User { Id = 3, UserName = "npc", LastSeenAtUtc = now, IsNpc = true }
            }.AsQueryable();

            var count = new PlayerActivityService().CountActiveHumanPlayers(users, now);

            Assert.Equal(1, count);
        }

        [Fact]
        public void CountActiveHumanPlayersIncludesBoundaryAtFiveMinutes()
        {
            var now = new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc);
            var users = new[]
            {
                new User { Id = 1, UserName = "boundary", LastSeenAtUtc = now.AddMinutes(-5), IsNpc = false }
            }.AsQueryable();

            var count = new PlayerActivityService().CountActiveHumanPlayers(users, now);

            Assert.Equal(1, count);
        }

        [Fact]
        public void MarkSeenUpdatesLastSeenTimestamp()
        {
            var now = new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc);
            var user = new User { Id = 1, UserName = "player", LastSeenAtUtc = now.AddHours(-1) };

            new PlayerActivityService().MarkSeen(user, now);

            Assert.Equal(now, user.LastSeenAtUtc);
        }
    }
}
