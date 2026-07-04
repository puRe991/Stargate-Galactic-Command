namespace StargateGalacticCommand.Core.Models
{
    public class DefenseUnits
    {
        public int Id { get; set; }
        public int LocalCombatMissionId { get; set; }
        public LocalCombatMission LocalCombatMission { get; set; }
        public int BaseGuards { get; set; }
        public int DefenseRings { get; set; }
        public int SensorAlarms { get; set; }
        public int LocalMilitia { get; set; }
        public int Total => BaseGuards + DefenseRings + SensorAlarms + LocalMilitia;
    }
}
