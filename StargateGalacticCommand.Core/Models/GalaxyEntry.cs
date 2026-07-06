namespace StargateGalacticCommand.Core.Models
{
    public class GalaxyEntry
    {
        public int PlayerBaseId { get; set; }
        public string BaseName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FactionShortName { get; set; }
        public string AllianceTag { get; set; }
        public int SectorNumber { get; set; }
        public string SectorName { get; set; }
        public int InfluenceScore { get; set; }
        public bool IsOnline { get; set; }
        public bool IsOwnBase { get; set; }
    }
}
