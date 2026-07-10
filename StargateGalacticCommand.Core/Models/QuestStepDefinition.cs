namespace StargateGalacticCommand.Core.Models
{
    public class QuestStepDefinition
    {
        public string Key { get; }
        public string FactionShortName { get; }
        public int Order { get; }
        public string Title { get; }
        public string Narrative { get; }
        public GateMissionType RequiredMissionType { get; }
        public int RequiredSuccessfulCount { get; }
        public int IntelReward { get; }

        public QuestStepDefinition(string key, string factionShortName, int order, string title, string narrative, GateMissionType requiredMissionType, int requiredSuccessfulCount, int intelReward)
        {
            Key = key;
            FactionShortName = factionShortName;
            Order = order;
            Title = title;
            Narrative = narrative;
            RequiredMissionType = requiredMissionType;
            RequiredSuccessfulCount = requiredSuccessfulCount;
            IntelReward = intelReward;
        }
    }
}
