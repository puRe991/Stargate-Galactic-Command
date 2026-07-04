namespace StargateGalacticCommand.Core.Models
{
    public class GroundUnits
    {
        public int Id { get; set; }
        public int LocalCombatMissionId { get; set; }
        public LocalCombatMission LocalCombatMission { get; set; }
        public int SgSecurityTeams { get; set; }
        public int Marines { get; set; }
        public int JaffaWarriors { get; set; }
        public int EliteJaffa { get; set; }
        public int AgentCells { get; set; }
        public int Saboteurs { get; set; }
        public int Mercenaries { get; set; }
        public int SmugglerSquads { get; set; }
        public int Total => SgSecurityTeams + Marines + JaffaWarriors + EliteJaffa + AgentCells + Saboteurs + Mercenaries + SmugglerSquads;
    }
}
