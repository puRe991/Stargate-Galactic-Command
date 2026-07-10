using System.Collections.Generic;

namespace StargateGalacticCommand.Core.Models
{
    public class GateAddress
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public GameServer GameServer { get; set; }
        public int? PlanetId { get; set; }
        public Planet Planet { get; set; }
        public string Code { get; set; }
        public string WorldName { get; set; }
        public string Description { get; set; }
        public bool IsNeutralPve { get; set; }
        public int RiskLevel { get; set; }
        public bool AnomalyFound { get; set; }
        public ICollection<KnownGateAddress> KnownByUsers { get; set; }
        public GateAddress() { KnownByUsers = new List<KnownGateAddress>(); }
    }
}
