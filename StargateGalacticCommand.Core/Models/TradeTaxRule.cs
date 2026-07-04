namespace StargateGalacticCommand.Core.Models
{
    public class TradeTaxRule
    {
        public int Id { get; set; }
        public double BaseFeeRate { get; set; }
        public double LucianAllianceReduction { get; set; }
        public double TradingPostReduction { get; set; }
        public int MaxIntelAmount { get; set; }
    }
}
