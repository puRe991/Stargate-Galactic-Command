using System;

namespace StargateGalacticCommand.Core.Models
{
    public class PlayerMessage
    {
        public int Id { get; set; }
        public int SenderUserId { get; set; }
        public User SenderUser { get; set; }
        public int RecipientUserId { get; set; }
        public User RecipientUser { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ReadAtUtc { get; set; }
        public bool IsDeletedBySender { get; set; }
        public bool IsDeletedByRecipient { get; set; }
    }
}
