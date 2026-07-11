namespace StargateGalacticCommand.Core.Models
{
    public class PlayerBaseSpecialResource
    {
        public int Id { get; set; }
        public int PlayerBaseId { get; set; }
        public PlayerBase PlayerBase { get; set; }
        public SpecialResourceType Type { get; set; }
        public int Quantity { get; set; }
    }
}
