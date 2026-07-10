using System;

namespace StargateGalacticCommand.Core.Models
{
    public class WorldEventContribution
    {
        public int Id { get; set; }
        public int WorldEventId { get; set; }
        public WorldEvent WorldEvent { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TotalAmount { get; set; }
        public DateTime? LastContributedAtUtc { get; set; }
        public DateTime? RewardGrantedAtUtc { get; set; }
    }
}
