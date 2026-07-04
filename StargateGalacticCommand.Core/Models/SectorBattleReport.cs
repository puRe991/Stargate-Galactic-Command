using System;
namespace StargateGalacticCommand.Core.Models
{
    public class SectorBattleReport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int LocalCombatMissionId { get; set; }
        public LocalCombatMission LocalCombatMission { get; set; }
        public int PlanetSectorId { get; set; }
        public PlanetSector PlanetSector { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool AttackerWon { get; set; }
    }
}
