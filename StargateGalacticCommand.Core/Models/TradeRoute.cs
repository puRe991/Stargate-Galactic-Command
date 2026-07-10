using System;

namespace StargateGalacticCommand.Core.Models
{
    public class TradeRoute
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int OriginBaseId { get; set; }
        public PlayerBase OriginBase { get; set; }
        public int TargetBaseId { get; set; }
        public PlayerBase TargetBase { get; set; }
        public ShipType ShipType { get; set; }
        public int ShipCount { get; set; }
        public int Naquadah { get; set; }
        public int Trinium { get; set; }
        public int Supplies { get; set; }
        public int Energy { get; set; }
        public int Personnel { get; set; }
        public int IntervalHours { get; set; }
        public bool IsActive { get; set; }
        public DateTime NextDueAtUtc { get; set; }
        public DateTime? LastExecutedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
