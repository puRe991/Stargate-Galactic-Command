namespace StargateGalacticCommand.Core.Models
{
    public class AchievementDefinition
    {
        public AchievementDefinition(string key, string name, string description, AchievementGoalType goalType, int goalAmount)
        {
            Key = key;
            Name = name;
            Description = description;
            GoalType = goalType;
            GoalAmount = goalAmount;
        }

        public string Key { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public AchievementGoalType GoalType { get; private set; }
        public int GoalAmount { get; private set; }
    }
}
