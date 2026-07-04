namespace StargateGalacticCommand.Core.Models
{
    public class ShipTypeDefinition
    {
        public ShipType Type { get; set; }
        public string Name { get; set; }
        public string FactionShortName { get; set; }
        public bool IsActive { get; set; }
        public BuildCost Cost { get; set; }
        public int CargoCapacity { get; set; }
        public int Speed { get; set; }
        public int FuelPerDistance { get; set; }
    }
}
