using System;

namespace StargateGalacticCommand.Core.Models
{
    public class ServerChatMessage
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public GameServer GameServer { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
