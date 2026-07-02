using System;

namespace StargateGalacticCommand.Core.Models
{
    public class BuildQueueItem
    {
        public int Id { get; set; }
        public int PlayerBaseId { get; set; }
        public PlayerBase PlayerBase { get; set; }
        public BuildingType BuildingType { get; set; }
        public int TargetLevel { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime CompletesAtUtc { get; set; }
    }
}
