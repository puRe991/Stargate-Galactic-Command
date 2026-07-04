namespace StargateGalacticCommand.Core.Models
{
    public class SpyDefenseResult
    {
        public CounterIntelligenceLevel Level { get; set; }
        public int DetectionRiskPercent { get; set; }
        public bool WasDetected { get; set; }
        public string Summary { get; set; }
    }
}
