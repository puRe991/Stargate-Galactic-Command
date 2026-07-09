using System;

namespace StargateGalacticCommand.Core.Models
{
    public class AchievementProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string AchievementKey { get; set; }
        public DateTime? UnlockedAtUtc { get; set; }
    }
}
