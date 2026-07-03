using System;

namespace StargateGalacticCommand.Core.Models
{
    public class KnownGateAddress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int GateAddressId { get; set; }
        public GateAddress GateAddress { get; set; }
        public DateTime DiscoveredAtUtc { get; set; }
        public string DiscoveryMethod { get; set; }
    }
}
