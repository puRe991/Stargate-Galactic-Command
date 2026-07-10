namespace StargateGalacticCommand.Core.Models
{
    public class CharacterSkills
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int MilitaryLevel { get; set; }
        public int ScienceLevel { get; set; }
        public int DiplomacyLevel { get; set; }
        public int UnspentPoints { get; set; }
    }
}
