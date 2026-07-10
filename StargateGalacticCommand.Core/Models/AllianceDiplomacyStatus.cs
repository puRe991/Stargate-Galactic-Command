using System;

namespace StargateGalacticCommand.Core.Models
{
    public class AllianceDiplomacyStatus
    {
        public int Id { get; set; }
        public int AllianceAId { get; set; }
        public Alliance AllianceA { get; set; }
        public int AllianceBId { get; set; }
        public Alliance AllianceB { get; set; }
        public AllianceDiplomacyStatusType Status { get; set; }
        public int ProposedByAllianceId { get; set; }
        public DateTime SinceUtc { get; set; }
        public int? BrokenByAllianceId { get; set; }
        public DateTime? LastBrokenAtUtc { get; set; }
    }
}
