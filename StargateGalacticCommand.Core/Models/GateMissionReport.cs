using System;

namespace StargateGalacticCommand.Core.Models
{
    public class GateMissionReport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int GateMissionId { get; set; }
        public GateMission GateMission { get; set; }
        public GateMissionOutcome Outcome { get; set; }
        public string Summary { get; set; }
        public int NaquadahFound { get; set; }
        public int TriniumFound { get; set; }
        public int SuppliesFound { get; set; }
        public int IntelFound { get; set; }
        public bool ArtifactLeadFound { get; set; }
        public int PersonnelLost { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
