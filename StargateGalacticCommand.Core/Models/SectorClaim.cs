using System;

namespace StargateGalacticCommand.Core.Models
{
    public class SectorClaim
    {
        public int Id { get; set; }
        public int PlanetSectorId { get; set; }
        public PlanetSector PlanetSector { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime CompletesAtUtc { get; set; }
        public bool IsCompleted { get; set; }
    }
}
