namespace StargateGalacticCommand.Core.Models
{
    public class ResearchDefinition
    {
        public ResearchDefinition(ResearchType type, string name, string factionShortName, BuildCost baseCost, int baseSeconds, ResearchType? prerequisite = null)
        {
            Type = type;
            Name = name;
            FactionShortName = factionShortName;
            BaseCost = baseCost;
            BaseSeconds = baseSeconds;
            Prerequisite = prerequisite;
        }

        public ResearchType Type { get; private set; }
        public string Name { get; private set; }
        public string FactionShortName { get; private set; }
        public BuildCost BaseCost { get; private set; }
        public int BaseSeconds { get; private set; }
        public ResearchType? Prerequisite { get; private set; }
    }
}
