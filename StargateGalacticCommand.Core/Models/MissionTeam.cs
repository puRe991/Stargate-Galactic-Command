namespace StargateGalacticCommand.Core.Models
{
    public class MissionTeam
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public MissionTeamType Type { get; set; }
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Science { get; set; }
        public int Diplomacy { get; set; }
        public int Stealth { get; set; }
        public int CarryCapacity { get; set; }
        public int Risk { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
