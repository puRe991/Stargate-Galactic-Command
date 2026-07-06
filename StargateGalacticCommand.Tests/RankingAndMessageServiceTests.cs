using System;
using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class RankingAndMessageServiceTests
    {
        private static User User(int id, string name, bool isNpc = false, DateTime? lastSeen = null) => new User { Id = id, UserName = name, IsNpc = isNpc, LastSeenAtUtc = lastSeen, Faction = new Faction { ShortName = "SGC" } };
        private static PlayerBase Base(int id, User user, int naquadah = 0, int f302 = 0, int commandCenter = 0) => new PlayerBase
        {
            Id = id,
            UserId = user.Id,
            User = user,
            Resources = new ResourceStock { Naquadah = naquadah },
            Ships = new BaseShips { F302 = f302 },
            BuildingLevels = new BuildingLevels { CommandCenter = commandCenter }
        };

        [Fact]
        public void CalculateBaseScoreWeighsResourcesShipsAndBuildings()
        {
            var service = new RankingService();
            var user = User(1, "u");
            var b = Base(1, user, naquadah: 100, f302: 2, commandCenter: 1);
            Assert.Equal(100 + 2 * 100 + 1 * 150, service.CalculateBaseScore(b));
        }

        [Fact]
        public void BuildPlayerRankingsOrdersByScoreAndFlagsOnlinePlayers()
        {
            var service = new RankingService();
            var now = DateTime.UtcNow;
            var strong = User(1, "strong", lastSeen: now);
            var weak = User(2, "weak", lastSeen: now.AddHours(-2));
            var bases = new List<PlayerBase> { Base(1, strong, naquadah: 1000), Base(2, weak, naquadah: 10) };
            var rankings = service.BuildPlayerRankings(bases, new Dictionary<int, string>(), TimeSpan.FromMinutes(15), now);
            Assert.Equal("strong", rankings[0].UserName);
            Assert.True(rankings[0].IsOnline);
            Assert.False(rankings[1].IsOnline);
        }

        [Fact]
        public void SendRejectsSelfMessagesAndNpcRecipients()
        {
            var service = new MessageService();
            var sender = User(1, "sender");
            var npc = User(2, "npc", isNpc: true);
            Assert.Throws<InvalidOperationException>(() => service.Send(sender, sender, "Hi", "Text", DateTime.UtcNow));
            Assert.Throws<InvalidOperationException>(() => service.Send(sender, npc, "Hi", "Text", DateTime.UtcNow));
        }

        [Fact]
        public void SendRequiresSubjectAndBody()
        {
            var service = new MessageService();
            var sender = User(1, "sender");
            var recipient = User(2, "recipient");
            Assert.Throws<ArgumentException>(() => service.Send(sender, recipient, " ", "Text", DateTime.UtcNow));
            Assert.Throws<ArgumentException>(() => service.Send(sender, recipient, "Subject", " ", DateTime.UtcNow));
            var message = service.Send(sender, recipient, "Subject", "Text", DateTime.UtcNow);
            Assert.Equal("Subject", message.Subject);
        }

        [Fact]
        public void MarkReadOnlyAllowsRecipient()
        {
            var service = new MessageService();
            var message = new PlayerMessage { SenderUserId = 1, RecipientUserId = 2 };
            var now = DateTime.UtcNow;
            Assert.Throws<InvalidOperationException>(() => service.MarkRead(message, 1, now));
            service.MarkRead(message, 2, now);
            Assert.Equal(now, message.ReadAtUtc);
        }
    }
}
