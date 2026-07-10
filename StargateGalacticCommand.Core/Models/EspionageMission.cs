using System;

namespace StargateGalacticCommand.Core.Models
{
    public class EspionageMission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int SourceBaseId { get; set; }
        public PlayerBase SourceBase { get; set; }
        public int TargetBaseId { get; set; }
        public PlayerBase TargetBase { get; set; }
        public EspionageMissionType MissionType { get; set; }
        public int IntelSpent { get; set; }
        public int ReportDepth { get; set; }
        public int DetectionRiskPercent { get; set; }
        public bool WasDetected { get; set; }
        public CounterIntelligenceLevel TargetCounterIntelligenceLevel { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
