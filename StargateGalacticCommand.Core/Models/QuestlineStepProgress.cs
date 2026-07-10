using System;

namespace StargateGalacticCommand.Core.Models
{
    public class QuestlineStepProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string StepKey { get; set; }
        public DateTime CompletedAtUtc { get; set; }
    }
}
