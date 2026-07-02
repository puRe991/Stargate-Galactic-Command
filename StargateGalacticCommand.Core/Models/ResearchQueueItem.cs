using System;

namespace StargateGalacticCommand.Core.Models
{
    public class ResearchQueueItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public ResearchType ResearchType { get; set; }
        public int TargetLevel { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime CompletesAtUtc { get; set; }
    }
}
