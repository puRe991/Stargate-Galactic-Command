using System;
using System.Collections.Generic;

namespace StargateGalacticCommand.Core.Models
{
    public class PlayerBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int FactionId { get; set; }
        public Faction Faction { get; set; }
        public int PlanetSectorId { get; set; }
        public PlanetSector PlanetSector { get; set; }
        public ResourceStock Resources { get; set; }
        public BuildingLevels BuildingLevels { get; set; }
        public DateTime LastResourceUpdateUtc { get; set; }
        public ICollection<BuildQueueItem> BuildQueue { get; set; }
        public BaseShips Ships { get; set; }
        public ICollection<ShipyardQueueItem> ShipyardQueue { get; set; }
        public PlayerBase() { BuildQueue = new List<BuildQueueItem>(); ShipyardQueue = new List<ShipyardQueueItem>(); }
    }
}
