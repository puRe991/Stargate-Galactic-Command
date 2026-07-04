namespace StargateGalacticCommand.Core.Models
{
    public class LocalCombatRound
    {
        public int Id { get; set; }
        public int LocalCombatMissionId { get; set; }
        public LocalCombatMission LocalCombatMission { get; set; }
        public int RoundNumber { get; set; }
        public int AttackerPower { get; set; }
        public int DefenderPower { get; set; }
        public int AttackerLosses { get; set; }
        public int DefenderLosses { get; set; }
        public string Summary { get; set; }
    }
}
