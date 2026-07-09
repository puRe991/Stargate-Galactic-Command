using System;

namespace StargateGalacticCommand.Core.Models
{
    public class AllianceWarGoal
    {
        public int Id { get; set; }
        public int AllianceId { get; set; }
        public Alliance Alliance { get; set; }
        public int PlanetId { get; set; }
        public Planet Planet { get; set; }
        public int RequiredSectors { get; set; }
        public int RequiredHours { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime? HoldStreakStartedAtUtc { get; set; }
        public AllianceWarGoalStatus Status { get; set; }
        public DateTime? AchievedAtUtc { get; set; }
        public DateTime? EndedAtUtc { get; set; }
    }
}
