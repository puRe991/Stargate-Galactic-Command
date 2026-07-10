using System;

namespace StargateGalacticCommand.Core.Models
{
    public class WorldEvent
    {
        public int Id { get; set; }
        public WorldEventType Type { get; set; }
        public WorldEventStatus Status { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime EndsAtUtc { get; set; }
        public int GoalProgress { get; set; }
        public int CurrentProgress { get; set; }
        public DateTime? ResolvedAtUtc { get; set; }
    }
}
