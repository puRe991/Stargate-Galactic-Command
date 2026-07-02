namespace StargateGalacticCommand.Core.Models
{
    public class ResourceStock
    {
        public int Id { get; set; }
        public int PlayerBaseId { get; set; }
        public int Naquadah { get; set; }
        public int Trinium { get; set; }
        public int Supplies { get; set; }
        public int Energy { get; set; }
        public int Personnel { get; set; }
        public int Intel { get; set; }
    }
}
