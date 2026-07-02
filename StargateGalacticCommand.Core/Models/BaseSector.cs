using System.Collections.Generic;

namespace StargateGalacticCommand.Core.Models
{
    public class BaseSector
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Faction Faction { get; set; }
        public int PlanetId { get; set; }
        public Planet Planet { get; set; }
        public ResourceStock Resources { get; set; }
        public ICollection<Building> Buildings { get; set; }

        public BaseSector()
        {
            Resources = new ResourceStock();
            Buildings = new List<Building>();
        }
    }
}
