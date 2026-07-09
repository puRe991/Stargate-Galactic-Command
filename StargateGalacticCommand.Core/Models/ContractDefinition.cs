using System.Collections.Generic;

namespace StargateGalacticCommand.Core.Models
{
    public class ContractDefinition
    {
        public ContractDefinition(string key, string name, string description, ContractGoalType goalType, int goalAmount, bool isWeekly, BuildCost reward, IDictionary<string, string> factionNames = null)
        {
            Key = key;
            Name = name;
            Description = description;
            GoalType = goalType;
            GoalAmount = goalAmount;
            IsWeekly = isWeekly;
            Reward = reward;
            FactionNames = factionNames;
        }

        public string Key { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ContractGoalType GoalType { get; private set; }
        public int GoalAmount { get; private set; }
        public bool IsWeekly { get; private set; }
        public BuildCost Reward { get; private set; }
        public IDictionary<string, string> FactionNames { get; private set; }

        public string GetDisplayName(Faction faction)
        {
            if (FactionNames != null && faction != null && FactionNames.TryGetValue(faction.ShortName, out var flavored)) return flavored;
            return Name;
        }
    }
}
