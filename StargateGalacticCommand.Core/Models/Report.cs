using System;

namespace StargateGalacticCommand.Core.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool IsRead { get; set; }
    }
}
