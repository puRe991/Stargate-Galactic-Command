namespace StargateGalacticCommand.Core.Models
{
    public class PlayerRankingEntry
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FactionShortName { get; set; }
        public string AllianceTag { get; set; }
        public int BaseCount { get; set; }
        public int Score { get; set; }
        public bool IsOnline { get; set; }
    }
}
