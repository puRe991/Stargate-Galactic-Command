using System;

namespace StargateGalacticCommand.Core.Models
{
    public class ContractProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string ContractKey { get; set; }
        public DateTime PeriodStartUtc { get; set; }
        public DateTime? ClaimedAtUtc { get; set; }
    }
}
