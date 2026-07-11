using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class ChatServiceTests
    {
        private static User User(int id, int serverId, bool isNpc = false) => new User { Id = id, ServerId = serverId, UserName = "u" + id, IsNpc = isNpc };

        [Fact]
        public void SendRejectsNpcSenders()
        {
            var service = new ChatService();
            var npc = User(1, 1, isNpc: true);
            Assert.Throws<InvalidOperationException>(() => service.Send(npc, "Hallo", null, DateTime.UtcNow));
        }

        [Fact]
        public void SendRequiresNonEmptyBodyWithinLimit()
        {
            var service = new ChatService();
            var user = User(1, 1);
            Assert.Throws<ArgumentException>(() => service.Send(user, " ", null, DateTime.UtcNow));
            Assert.Throws<ArgumentException>(() => service.Send(user, new string('x', ChatService.MaxBodyLength + 1), null, DateTime.UtcNow));
            var message = service.Send(user, " Hallo Galaxis ", null, DateTime.UtcNow);
            Assert.Equal("Hallo Galaxis", message.Body);
            Assert.Equal(user.ServerId, message.ServerId);
            Assert.Equal(user.Id, message.UserId);
        }

        [Fact]
        public void SendEnforcesMinIntervalBetweenMessages()
        {
            var service = new ChatService();
            var user = User(1, 1);
            var now = DateTime.UtcNow;
            Assert.Throws<InvalidOperationException>(() => service.Send(user, "Zu schnell", now, now.Add(ChatService.MinInterval).AddMilliseconds(-1)));
            var message = service.Send(user, "Rechtzeitig", now, now.Add(ChatService.MinInterval));
            Assert.Equal("Rechtzeitig", message.Body);
        }
    }
}
