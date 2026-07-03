namespace StargateGalacticCommand.Core.Models
{
    public class PlanetInfluence
    {
        public int PlanetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }
    }
}
