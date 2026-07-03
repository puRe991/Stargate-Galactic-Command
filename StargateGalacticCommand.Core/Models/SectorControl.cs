using System;

namespace StargateGalacticCommand.Core.Models
{
    public class SectorControl
    {
        public int Id { get; set; }
        public int PlanetSectorId { get; set; }
        public PlanetSector PlanetSector { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime ControlledAtUtc { get; set; }
    }
}
