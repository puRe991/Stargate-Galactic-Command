namespace StargateGalacticCommand.Core.Models
{
    public class AllianceRankingEntry
    {
        public int AllianceId { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public int MemberCount { get; set; }
        public int TotalScore { get; set; }
    }
}
