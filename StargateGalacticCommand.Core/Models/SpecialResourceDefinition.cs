namespace StargateGalacticCommand.Core.Models
{
    public class SpecialResourceDefinition
    {
        public SpecialResourceDefinition(SpecialResourceType type, SpecialResourceCategory category, string name, string description)
        {
            Type = type;
            Category = category;
            Name = name;
            Description = description;
        }

        public SpecialResourceType Type { get; private set; }
        public SpecialResourceCategory Category { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
    }
}
