namespace StargateGalacticCommand.Core.Models
{
    public class PlanetSector
    {
        public int Id { get; set; }
        public int PlanetId { get; set; }
        public Planet Planet { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public bool IsSettlementSector { get; set; }
        public SectorType SectorType { get; set; }
        public PlayerBase PlayerBase { get; set; }
        public SectorControl SectorControl { get; set; }
    }
}
