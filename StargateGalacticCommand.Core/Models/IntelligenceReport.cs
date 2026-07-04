using System;

namespace StargateGalacticCommand.Core.Models
{
    public class IntelligenceReport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? EspionageMissionId { get; set; }
        public EspionageMission EspionageMission { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int DetailDepth { get; set; }
        public bool IsWarning { get; set; }
        public bool WasDetected { get; set; }
    }
}
