using System;
using System.Collections.Generic;
namespace StargateGalacticCommand.Core.Models
{
    public class LocalCombatMission
    {
        public int Id { get; set; }
        public int AttackerUserId { get; set; }
        public User AttackerUser { get; set; }
        public int? DefenderUserId { get; set; }
        public User DefenderUser { get; set; }
        public int PlanetSectorId { get; set; }
        public PlanetSector PlanetSector { get; set; }
        public LocalCombatObjective Objective { get; set; }
        public GroundUnits AttackingUnits { get; set; }
        public DefenseUnits DefendingUnits { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime ResolvesAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public ICollection<LocalCombatRound> Rounds { get; set; }
        public bool AttackerWon { get; set; }
        public int AttackerLosses { get; set; }
        public int DefenderLosses { get; set; }
        public LocalCombatMission() { Rounds = new List<LocalCombatRound>(); }
    }
}
