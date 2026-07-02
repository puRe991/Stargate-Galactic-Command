using System.Collections.Generic;

namespace StargateGalacticCommand.Core.Models
{
    public class Planet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Galaxy { get; set; }
        public string Type { get; set; }
        public bool StargateActive { get; set; }
        public string Status { get; set; }
        public ICollection<PlanetSector> Sectors { get; set; }
        public Planet() { Sectors = new List<PlanetSector>(); }
    }
}
