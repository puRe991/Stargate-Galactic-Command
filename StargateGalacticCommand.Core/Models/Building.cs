namespace StargateGalacticCommand.Core.Models
{
    public class Building
    {
        public int Id { get; set; }
        public BuildingType Type { get; set; }
        public int Level { get; set; }
        public int BaseSectorId { get; set; }
        public BaseSector BaseSector { get; set; }
    }
}
